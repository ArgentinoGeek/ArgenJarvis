using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Core.Configuration.Commands
{
    public class UpdateConfigurationValueCommand : IRequest<Unit>
    {
        public UpdateConfigurationValueCommand(string key, string newValue)
        {
            Key = key;
            Value = newValue;
        }

        public string Key { get; set; }
        public string Value { get; set; }
    }

    public class UpdateConfigurationValueCommandHandler : IRequestHandler<UpdateConfigurationValueCommand, Unit>
    {
        private readonly IRepository<Domain.Entities.Configuration> _configurationRepository;

        public UpdateConfigurationValueCommandHandler(IRepository<Domain.Entities.Configuration> configurationRepository)
        {
            _configurationRepository = configurationRepository;
        }

        public async Task<Unit> Handle(UpdateConfigurationValueCommand request, CancellationToken cancellationToken)
        {
            var configuration = _configurationRepository.Get().FirstOrDefault(x => x.Key.Equals(request.Key) && x.IsEnabled);

            if(configuration == null)
            {
                return new Unit();
            }

            await _configurationRepository.Update(configuration);

            configuration.Value = request.Value;
            configuration.LastUpdate = DateTime.Now;

            await _configurationRepository.SaveChangesAsync();

            return new Unit();
        }
    }
}
