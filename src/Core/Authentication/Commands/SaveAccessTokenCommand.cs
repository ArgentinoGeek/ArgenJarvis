using Core.Authentication.DTOs;
using MediatR;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Core.Authentication.Commands
{
    public class SaveAccessTokenCommand : IRequest<TwitchTokenDto>
    {
        public SaveAccessTokenCommand(string code)
        {
            Code = code;
        }

        public string Code { get; set; }
    }

    public class SaveAccessTokenCommandHandler : IRequestHandler<SaveAccessTokenCommand, TwitchTokenDto>
    {
        private readonly IRepository<Domain.Entities.Configuration> _configurationRepository;

        public SaveAccessTokenCommandHandler(IRepository<Domain.Entities.Configuration> configurationRepository)
        {
            _configurationRepository = configurationRepository;
        }

        public async Task<TwitchTokenDto> Handle(SaveAccessTokenCommand request, CancellationToken cancellationToken)
        {
            var twitchTokenDto = new TwitchTokenDto();

            // Get configuration values from the database

            var clientId = _configurationRepository.Get().FirstOrDefault(x => x.Key.Equals("TwitchClientId") && x.IsEnabled).Value;
            var clientSecret = _configurationRepository.Get().FirstOrDefault(x => x.Key.Equals("TwitchClientSecret") && x.IsEnabled).Value;
            var redirectUri = _configurationRepository.Get().FirstOrDefault(x => x.Key.Equals("TwitchRedirectUri") && x.IsEnabled).Value;

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://id.twitch.tv");

                var requestBody = new StringContent(string.Empty);

                var response = await client.PostAsync($"/oauth2/token?client_id={clientId}&client_secret={clientSecret}&code={request.Code}&grant_type=authorization_code&redirect_uri={redirectUri}", requestBody);

                twitchTokenDto = JsonConvert.DeserializeObject<TwitchTokenDto>(await response.Content.ReadAsStringAsync());
            }

            // Save or update
            var twitchAccessToken = _configurationRepository.Get().FirstOrDefault(x => x.Key.Equals("TwitchAccessToken") && x.IsEnabled);

            if (twitchAccessToken == null)
            {
                // Insert
                var configuration = new Domain.Entities.Configuration
                {
                    Key = "TwitchAccessToken",
                    Value = twitchTokenDto.AccessToken
                };

                await _configurationRepository.Add(configuration);
            }
            else
            {
                // Update
                await _configurationRepository.Update(twitchAccessToken);
                
                twitchAccessToken.Value = twitchTokenDto.AccessToken;
                twitchAccessToken.LastUpdate = DateTime.Now;
            }

            // Save
            await _configurationRepository.SaveChangesAsync();

            return twitchTokenDto;
        }
    }
}
