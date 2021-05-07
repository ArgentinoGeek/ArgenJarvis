using Core.Message.DTOs;
using MediatR;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Core.Message.Queries
{
    public class GetMessageLanguageCodeQuery : IRequest<string>
    {
        public GetMessageLanguageCodeQuery(string message)
        {
            Message = message;
        }

        public string Message { get; set; }
    }

    public class GetMessageLanguageCodeQueryHandler : IRequestHandler<GetMessageLanguageCodeQuery, string>
    {
        public async Task<string> Handle(GetMessageLanguageCodeQuery request, CancellationToken cancellationToken)
        {
            var languageCode = string.Empty;

            var message = request.Message.Replace(' ', '+');

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://api.dandelion.eu");
                var response = await client.GetAsync($"/datatxt/li/v1/?token=5c90bf7ecca54227b920fc4c6cdacf47&text={message}");

                var languageDetectionDto = JsonConvert.DeserializeObject<LanguageDetectionDto>(await response.Content.ReadAsStringAsync());

                languageCode = languageDetectionDto.DetectedLangs.FirstOrDefault().Lang;
            }

            return languageCode;
        }
    }
}
