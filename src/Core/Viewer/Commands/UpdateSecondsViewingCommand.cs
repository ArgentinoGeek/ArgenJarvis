using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Core.Viewer.Commands
{
    public class UpdateSecondsViewingCommand : IRequest<Unit>
    {
        public UpdateSecondsViewingCommand(string userName)
        {
            UserName = userName;
        }

        public string UserName { get; set; }
    }

    public class UpdateSecondsViewingCommandHandler : IRequestHandler<UpdateSecondsViewingCommand, Unit>
    {
        private readonly IRepository<Domain.Entities.Viewer> _viewerRepository;

        public UpdateSecondsViewingCommandHandler(IRepository<Domain.Entities.Viewer> viewerRepository)
        {
            _viewerRepository = viewerRepository;
        }

        public async Task<Unit> Handle(UpdateSecondsViewingCommand request, CancellationToken cancellationToken)
        {
            var viewer = _viewerRepository.Get().FirstOrDefault(x => x.DisplayName.Equals(request.UserName) && x.IsEnabled);

            var joinedDate = viewer.DateJoined;
            var leftDate = DateTime.Now;

            var dateDiffInSeconds = Convert.ToInt32((leftDate - joinedDate).TotalSeconds);

            await _viewerRepository.Update(viewer);

            viewer.SecondsViewing += dateDiffInSeconds;
            viewer.LastUpdate = leftDate;

            await _viewerRepository.SaveChangesAsync();

            return new Unit();
        }
    }
}
