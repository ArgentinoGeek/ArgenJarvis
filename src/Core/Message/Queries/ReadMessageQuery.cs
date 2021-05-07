using MediatR;
using NetCoreAudio;
using System;
using System.IO;
using System.Media;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Core.Message.Queries
{
    public class ReadMessageQuery : IRequest<Unit>
    {
        public ReadMessageQuery(string textToRead, string language)
        {
            TextToRead = textToRead;
            Language = language;
        }

        public string TextToRead { get; set; }
        public string Language { get; set; }
    }

    public class ReadMessageQueryHandler : IRequestHandler<ReadMessageQuery, Unit>
    {
        public async Task<Unit> Handle(ReadMessageQuery request, CancellationToken cancellationToken)
        {
            var language = string.IsNullOrWhiteSpace(request.Language) ? "es" : request.Language;
            var textToSpeech = request.TextToRead.Replace(' ', '+');

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://translate.google.com");
                var response = await client.GetAsync($"/translate_tts?ie=UTF-8&client=tw-ob&tl={language}&q={textToSpeech}");

                var audio = await response.Content.ReadAsStreamAsync();

                if (audio.Length > 0)
                {
                    var fileLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\Audio\\";
                    Directory.CreateDirectory(fileLocation);

                    var fileName = fileLocation + $"{DateTime.Now.Ticks}.mp3";

                    using (var fileStream = File.Create(fileName))
                    {
                        audio.Seek(0, SeekOrigin.Begin);
                        audio.CopyTo(fileStream);
                    }

                    var player = new Player();
                    await player.Play(fileName); 
                }
            }

            return new Unit();
        }
    }
}
