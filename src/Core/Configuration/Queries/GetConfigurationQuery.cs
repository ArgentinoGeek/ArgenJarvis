using MediatR;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Core.Configuration.Queries
{
    public class GetConfigurationQuery : IRequest<string>
    {
        public GetConfigurationQuery(string key)
        {
            Key = key;
        }

        public string Key { get; set; }
    }

    public class GetConfigurationQueryHandler : IRequestHandler<GetConfigurationQuery, string>
    {
        private readonly IRepository<Domain.Entities.Configuration> _configurationRepository;

        public GetConfigurationQueryHandler(IRepository<Domain.Entities.Configuration> configurationRepository)
        {
            _configurationRepository = configurationRepository;
        }

        public async Task<string> Handle(GetConfigurationQuery request, CancellationToken cancellationToken)
        {
            return await Task.FromResult(_configurationRepository.Get().FirstOrDefault(x => x.Key.Equals(request.Key)).Value);
        }
    }
}
