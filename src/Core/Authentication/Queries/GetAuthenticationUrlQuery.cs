using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Core.Authentication.Queries
{
    public class GetAuthenticationUrlQuery : IRequest<Uri>
    {
    }

    public class GetAuthenticationUrlQueryHandler : IRequestHandler<GetAuthenticationUrlQuery, Uri>
    {
        private readonly IRepository<Domain.Entities.Configuration> _configurationRepository;

        public GetAuthenticationUrlQueryHandler(IRepository<Domain.Entities.Configuration> configurationRepository)
        {
            _configurationRepository = configurationRepository;
        }

        public async Task<Uri> Handle(GetAuthenticationUrlQuery request, CancellationToken cancellationToken)
        {
            // Get configuration values from the database

            var clientId = _configurationRepository.Get().FirstOrDefault(x => x.Key.Equals("TwitchClientId") && x.IsEnabled).Value;
            var redirectUri = _configurationRepository.Get().FirstOrDefault(x => x.Key.Equals("TwitchRedirectUri") && x.IsEnabled).Value;

            return await Task.FromResult(new Uri($"https://id.twitch.tv/oauth2/authorize?client_id={clientId}&redirect_uri={redirectUri}&response_type=code&scope=chat:read%20channel:moderate%20chat:edit"));
        }
    }
}
