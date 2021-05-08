using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Core.TimerMessage.Commands
{
    public class AddNewTimerMessageCommand : IRequest<Unit>
    {
        public AddNewTimerMessageCommand(string name, string content, int periodInMilliseconds)
        {
            Name = name;
            Content = content;
            PeriodInMilliseconds = periodInMilliseconds;
        }

        public string Name { get; set; }
        public string Content { get; set; }
        public int PeriodInMilliseconds { get; set; }
    }

    public class AddNewTimerMessageCommandHandler : IRequestHandler<AddNewTimerMessageCommand, Unit>
    {
        private readonly IRepository<Domain.Entities.TimerMessage> _timerMessageRepository;

        public AddNewTimerMessageCommandHandler(IRepository<Domain.Entities.TimerMessage> timerMessageRepository)
        {
            _timerMessageRepository = timerMessageRepository;
        }

        public async Task<Unit> Handle(AddNewTimerMessageCommand request, CancellationToken cancellationToken)
        {
            var newTimerMessage = new Domain.Entities.TimerMessage
            {
                Name = request.Name,
                Content = request.Content,
                PeriodInMilliseconds = request.PeriodInMilliseconds
            };

            await _timerMessageRepository.Add(newTimerMessage);
            await _timerMessageRepository.SaveChangesAsync();

            return new Unit();
        }
    }
}
