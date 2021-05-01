using Core.Viewer.Queries;
using MediatR;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Commands
{
    public class ArgenPointsCommand : ITwitchCommand
    {
        private readonly IMediator _mediator;

        public ArgenPointsCommand(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<string> Execute(string commandName, string userName, IList<string> arguments, string argumentsAsString)
        {
            int points;

            if (arguments.Count == 0)
            {
                // My points
                points = await _mediator.Send(new GetPointsByUserNameQuery(userName));

                return $"@{userName}: you have {points} points.";
            }
            else
            {
                // Someone else points
                var someoneUserName = arguments[0];
                someoneUserName = someoneUserName.StartsWith('@') 
                    ? someoneUserName.Substring(1, someoneUserName.Length - 1) 
                    : someoneUserName;

                points = await _mediator.Send(new GetPointsByUserNameQuery(someoneUserName));

                return $"@{userName}: @{someoneUserName} has {points} points.";
            }
        }
    }
}
