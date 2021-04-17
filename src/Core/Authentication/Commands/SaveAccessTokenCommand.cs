using Core.Authentication.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            return new TwitchTokenDto
            {

            };
        }
    }
}
