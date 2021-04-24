using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Core.Viewer.Commands
{
    public class AddPointsToViewerCommand : IRequest<Unit>
    {
        public AddPointsToViewerCommand(string userName, int pointsToAdd)
        {
            UserName = userName;
            PointsToAdd = pointsToAdd;
        }

        public string UserName { get; set; }
        public int PointsToAdd { get; set; }
    }

    public class AddPointsToViewerCommandHandler : IRequestHandler<AddPointsToViewerCommand, Unit>
    {
        private readonly IRepository<Domain.Entities.Viewer> _viewerRepository;

        public AddPointsToViewerCommandHandler(IRepository<Domain.Entities.Viewer> viewerRepository)
        {
            _viewerRepository = viewerRepository;
        }

        public async Task<Unit> Handle(AddPointsToViewerCommand request, CancellationToken cancellationToken)
        {
            var viewer = _viewerRepository.Get().FirstOrDefault(x => x.DisplayName.Equals(request.UserName) && x.IsEnabled);

            await _viewerRepository.Update(viewer);

            viewer.Points += request.PointsToAdd;
            viewer.LastUpdate = DateTime.Now;

            await _viewerRepository.SaveChangesAsync();

            return new Unit();
        }
    }
}
