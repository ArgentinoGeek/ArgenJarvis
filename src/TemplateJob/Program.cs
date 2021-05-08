using Core;
using Core.Commands;
using Core.Configuration.Queries;
using Core.Message.Commands;
using Core.Message.Queries;
using Core.TimerMessage.Commands;
using Core.TimerMessage.DTOs;
using Core.Viewer.Commands;
using Infrastructure;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using TwitchLib.Client;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Clients;
using TwitchLib.Communication.Events;
using TwitchLib.Communication.Models;

namespace TemplateJob
{
    class Program
    {
        private static IServiceProvider _serviceProvider;

        static TwitchClient client;

        private static string TwitchChatbotName { get; set; }
        private static string TwitchChannelName { get; set; }
        private static string TwitchAccessToken { get; set; }

        static async Task Main(string[] args)
        {
            const string TwitchChatbotNameKey = "TwitchChatbotName";
            const string TwitchChannelNameKey = "TwitchChannelName";
            const string TwitchAccessTokenKey = "TwitchAccessToken";

            #region Dependency Injection initialization
            using IHost host = CreateHostBuilder(args).Build();
            await host.StartAsync();
            await Task.Delay(1000);
            _serviceProvider = host.Services; 
            #endregion

            // Application code should start here.
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Title = "ArgenJarvis";
            Console.WriteLine("Starting...");
            Console.WriteLine();

            #region Twitch Authentication
            // Get login url
            string loginUrl = await GetLoginUrl();

            // Open the url in a new browser tab
            Process.Start("explorer.exe", loginUrl);

            await Task.Delay(3000);

            TwitchChatbotName = await GetValueForAsync(TwitchChatbotNameKey);
            TwitchChannelName = await GetValueForAsync(TwitchChannelNameKey);
            TwitchAccessToken = await GetValueForAsync(TwitchAccessTokenKey); 
            #endregion

            var credentials = new ConnectionCredentials(TwitchChatbotName, TwitchAccessToken);

            var clientOptions = new ClientOptions();
            WebSocketClient customClient = new WebSocketClient(clientOptions);
            client = new TwitchClient(customClient);
            client.Initialize(credentials, TwitchChannelName);

            client.OnConnected += Client_OnConnected;
            client.OnDisconnected += Client_OnDisconected;
            client.OnUserJoined += Client_OnUserJoined;
            client.OnUserLeft += Client_OnUserLeft;
            client.OnMessageReceived += Client_OnMessageReceived;
            client.OnChatCommandReceived += Client_OnChatCommandReceived;
            client.OnNewSubscriber += Client_OnNewSubscriber;

            client.Connect();

            Console.WriteLine($"Connected!");

            await InitializeTimerMessages();

            Console.ReadKey();

            // After this line, the code will not be executed
            await host.StopAsync();
        }

        #region Event Handlers
        private static void Client_OnConnected(object sender, OnConnectedArgs e)
        {
            client.SendMessage(TwitchChannelName, "Hey guys! I am a bot connected via TwitchLib!");
        }

        private static void Client_OnDisconected(object sender, OnDisconnectedEventArgs e)
        {
            client.SendMessage(TwitchChannelName, "Chatbot disconnected!");
        }

        private static void Client_OnUserJoined(object sender, OnUserJoinedArgs e)
        {
            EnsureViewerExists(e.Username, shouldUpdateDateJoined: true).Wait();
            Console.WriteLine($"[{DateTime.Now}] {e.Username} has Joined");
        }

        private static void Client_OnUserLeft(object sender, OnUserLeftArgs e)
        {
            UpdateSecondsViewing(e.Username).Wait();
            Console.WriteLine($"[{DateTime.Now}] {e.Username} has Left");
        }

        private static void Client_OnMessageReceived(object sender, OnMessageReceivedArgs e)
        {
            var message = e.ChatMessage.Message;

            EnsureViewerExists(e.ChatMessage.Username).Wait();

            Console.WriteLine($"[{DateTime.Now}] {e.ChatMessage.DisplayName}: {message}");
            SaveNewMessage(e.ChatMessage.Username, message).Wait();
            AddPointsToViewer(e.ChatMessage.Username, 1).Wait();
        }

        private static void Client_OnChatCommandReceived(object sender, OnChatCommandReceivedArgs e)
        {
            var commandName = e.Command.CommandText;
            var arguments = e.Command.ArgumentsAsList;
            var argumentsAsString = e.Command.ArgumentsAsString;
            var userName = e.Command.ChatMessage.DisplayName;

            ExecuteCommand(commandName, userName, arguments, argumentsAsString).Wait();
        }

        private static void Client_OnNewSubscriber(object sender, OnNewSubscriberArgs e)
        {
            var message = e.Subscriber.SystemMessageParsed;
            var messageLanguage = GetMessageLanguage(message).Result;

            AddPointsToViewer(e.Subscriber.DisplayName, 100).Wait();
            ReadMessage(message, messageLanguage).Wait();
        } 
        #endregion

        #region Authentication
        private static async Task<string> GetValueForAsync(string key)
        {
            var mediator = _serviceProvider.GetRequiredService<IMediator>();

            return await mediator.Send(new GetConfigurationQuery(key));
        }

        private static async Task<string> GetLoginUrl()
        {
            var loginUrl = string.Empty;

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://localhost:44306");

                var response = await client.GetAsync("/api/Authentication/init");

                loginUrl = await response.Content.ReadAsStringAsync();
            }

            return loginUrl;
        }
        #endregion

        #region Private methods
        private static async Task EnsureViewerExists(string username, bool shouldUpdateDateJoined = false)
        {
            var mediator = _serviceProvider.GetRequiredService<IMediator>();

            await mediator.Send(new EnsureViewerExistsCommand(username, shouldUpdateDateJoined));
        }

        private static async Task UpdateSecondsViewing(string username)
        {
            var mediator = _serviceProvider.GetRequiredService<IMediator>();

            await mediator.Send(new UpdateSecondsViewingCommand(username));
        }

        private static async Task SaveNewMessage(string username, string message)
        {
            var mediator = _serviceProvider.GetRequiredService<IMediator>();

            await mediator.Send(new SaveNewMessageCommand(username, message));
        }

        private static async Task AddPointsToViewer(string username, int pointsToAdd)
        {
            var mediator = _serviceProvider.GetRequiredService<IMediator>();

            await mediator.Send(new AddPointsToViewerCommand(username, pointsToAdd));
        }

        private static async Task ReadMessage(string message, string language)
        {
            var mediator = _serviceProvider.GetRequiredService<IMediator>();

            await mediator.Send(new ReadMessageQuery(message, language));
        }

        private static async Task<string> GetMessageLanguage(string message)
        {
            var mediator = _serviceProvider.GetRequiredService<IMediator>();

            return await mediator.Send(new GetMessageLanguageCodeQuery(message));
        }

        private static async Task ExecuteCommand(string commandName, string userName, List<string> arguments, string argumentsAsString)
        {
            var mediator = _serviceProvider.GetRequiredService<IMediator>();

            var response = await new CommandHandler(mediator).Handle(commandName, userName, arguments, argumentsAsString);

            if (!string.IsNullOrWhiteSpace(response))
            {
                client.SendMessage(TwitchChannelName, response);
            }
        }

        private static async Task InitializeTimerMessages()
        {
            var mediator = _serviceProvider.GetRequiredService<IMediator>();

            var timerMessages = await mediator.Send(new GetTimerMessagesQuery());

            foreach (var timerMessage in timerMessages)
            {
                var random = new Random((int)DateTime.Now.Ticks);
                var timer = new Timer(On_TimerCallback, timerMessage, random.Next(1000, 3000), timerMessage.PeriodInMilliseconds);
            }
        }

        private static void On_TimerCallback(object state)
        {
            var timerMessage = (TimerMessageDto)state;
            Console.WriteLine($"[{DateTime.Now}] {timerMessage.Content}");
            SendMessageToTheChat(timerMessage.Content);

            Console.ReadKey();
        }

        private static void SendMessageToTheChat(string message)
        {
            client.SendMessage(TwitchChannelName, message);
        }
        #endregion

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            var host = Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostingContext, configuration) =>
                {
                    configuration.Sources.Clear();

                    var env = hostingContext.HostingEnvironment;

                    configuration
                        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                        .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true);

                    var configurationRoot = configuration.Build();
                    ServiceCollectionExtension.Configuration = configurationRoot;
                })
                .ConfigureLogging((context, logging) =>
                {
                    logging.ClearProviders();
                    logging.AddConfiguration(context.Configuration.GetSection("Logging"));
                    logging.AddDebug();
                    logging.AddConsole();
                })
                .ConfigureServices((_, services) =>
                    services.AddMediatR(typeof(Program), typeof(CoreAssemblies))
                    .RegisterInfrastructureDependencies());

            return host;
        }
    }
}
