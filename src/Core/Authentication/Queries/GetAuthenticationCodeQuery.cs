using MediatR;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Core.Authentication.Queries
{
    public class GetAuthenticationCodeQuery : IRequest<Unit>
    {
    }

    public class GetAuthenticationCodeQueryHandler : IRequestHandler<GetAuthenticationCodeQuery, Unit>
    {
        private readonly IRepository<Domain.Entities.Configuration> _configurationRepository;

        public GetAuthenticationCodeQueryHandler(IRepository<Domain.Entities.Configuration> configurationRepository)
        {
            _configurationRepository = configurationRepository;
        }

        public async Task<Unit> Handle(GetAuthenticationCodeQuery request, CancellationToken cancellationToken)
        {
            // Get configuration values from the database

            var clientId = _configurationRepository.Get().FirstOrDefault(x => x.Key.Equals("TwitchClientId") && x.IsEnabled).Value;
            var redirectUri = _configurationRepository.Get().FirstOrDefault(x => x.Key.Equals("TwitchRedirectUri") && x.IsEnabled).Value;

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://id.twitch.tv");

                var response = await client.GetAsync($"/oauth2/authorize?client_id={clientId}&redirect_uri={redirectUri}&response_type=code&scope=chat:read%20channel:moderate%20chat:edit");
            }

            return new Unit();
        }
    }
}
