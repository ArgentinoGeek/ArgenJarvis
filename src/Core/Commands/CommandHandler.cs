using MediatR;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Commands
{
    public class CommandHandler
    {
        private readonly IMediator _mediator;

        public CommandHandler(IMediator mediator)
        {
            _mediator = mediator;
        }

        public Task<string> Handle(string commandName, string userName, IList<string> arguments, string argumentsAsString)
        {
            switch (commandName.ToString())
            {
                case "argenpoints":
                    return new ArgenPointsCommand(_mediator).Execute(commandName, userName, arguments, argumentsAsString);
                case "t":
                    return new TranslateTextCommand(_mediator).Execute(commandName, userName, arguments, argumentsAsString);
                default:
                    return Task.FromResult(string.Empty);
            }
        }
    }
}
