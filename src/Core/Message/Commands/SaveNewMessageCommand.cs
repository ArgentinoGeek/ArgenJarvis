using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Core.Message.Commands
{
    public class SaveNewMessageCommand : IRequest<Unit>
    {
        public SaveNewMessageCommand(string userName, string content)
        {
            UserName = userName;
            Content = content;
        }

        public string UserName { get; set; }
        public string Content { get; set; }
    }

    public class SaveNewMessageCommandHandler : IRequestHandler<SaveNewMessageCommand, Unit>
    {
        private readonly IRepository<Domain.Entities.Message> _messageRepository;
        private readonly IRepository<Domain.Entities.Viewer> _viewerRepository;

        public SaveNewMessageCommandHandler(IRepository<Domain.Entities.Message> messageRepository,
            IRepository<Domain.Entities.Viewer> viewerRepository)
        {
            _messageRepository = messageRepository;
            _viewerRepository = viewerRepository;
        }

        public async Task<Unit> Handle(SaveNewMessageCommand request, CancellationToken cancellationToken)
        {
            // Get User
            var viewer = _viewerRepository.Get().FirstOrDefault(x => x.DisplayName.Equals(request.UserName) && x.IsEnabled);

            // Save message
            var message = new Domain.Entities.Message
            {
                ViewerId = viewer.Id,
                DateReceived = DateTime.Now,
                Content = request.Content
            };

            await _messageRepository.Add(message);
            await _messageRepository.SaveChangesAsync();

            return new Unit();
        }
    }
}
