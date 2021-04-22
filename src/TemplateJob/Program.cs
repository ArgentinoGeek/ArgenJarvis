using Core;
using Core.Configuration.Queries;
using Core.Country.Queries;
using Infrastructure;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using TwitchLib.Client;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Clients;
using TwitchLib.Communication.Models;

namespace TemplateJob
{
    class Program
    {
        static TwitchClient client;

        private static string TwitchChatbotName { get; set; }
        private static string TwitchChannelName { get; set; }
        private static string TwitchAccessToken { get; set; }

        static async Task Main(string[] args)
        {
            const string TwitchChatbotNameKey = "TwitchChatbotName";
            const string TwitchChannelNameKey = "TwitchChannelName";
            const string TwitchAccessTokenKey = "TwitchAccessToken";

            using IHost host = CreateHostBuilder(args).Build();
            await host.StartAsync();
            await Task.Delay(1000);

            // Application code should start here.
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Starting...");
            Console.WriteLine();

            // Get login url
            string loginUrl = await GetLoginUrl(host.Services);

            // Open the url in a new browser tab
            Process.Start("explorer.exe", loginUrl);

            // Get last access token LastUpdate date if exists
            // Compare the stored LastUpdate date to know if it was updated or not
            // After getting the new AccessToken, Connect to the chat

            TwitchChatbotName = await GetValueForAsync(host.Services, TwitchChatbotNameKey);
            TwitchChannelName = await GetValueForAsync(host.Services, TwitchChannelNameKey);
            TwitchAccessToken = await GetValueForAsync(host.Services, TwitchAccessTokenKey);

            var credentials = new ConnectionCredentials(TwitchChatbotName, TwitchAccessToken);

            var clientOptions = new ClientOptions
            {
                //MessagesAllowedInPeriod = 750,
                //ThrottlingPeriod = TimeSpan.FromSeconds(30)
            };
            WebSocketClient customClient = new WebSocketClient(clientOptions);
            client = new TwitchClient(customClient);
            client.Initialize(credentials, TwitchChannelName);

            client.OnConnected += Client_OnConnected;
            client.OnMessageReceived += Client_OnMessageReceived;

            client.Connect();

            Console.WriteLine($"Connected!");
            Console.ReadKey();

            // After this line, the code will not be executed
            await host.StopAsync();
        }

        private static void Client_OnConnected(object sender, OnConnectedArgs e)
        {
            client.SendMessage(TwitchChannelName, "Hey guys! I am a bot connected via TwitchLib!");
        }

        private static void Client_OnMessageReceived(object sender, OnMessageReceivedArgs e)
        {
            var message = e.ChatMessage.Message;

            Console.WriteLine($"[{DateTime.Now}] {e.ChatMessage.DisplayName}: {message}");
        }

        private static async Task<string> GetValueForAsync(IServiceProvider services, string key)
        {
            var mediator = services.GetRequiredService<IMediator>();

            return await mediator.Send(new GetConfigurationQuery(key));
        }

        private static async Task<string> GetLoginUrl(IServiceProvider services)
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
