using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Core.Viewer.Commands
{
    public class EnsureViewerExistsCommand : IRequest<Unit>
    {
        public EnsureViewerExistsCommand(string userName, bool shouldUpdateDateJoined = false)
        {
            UserName = userName;
            ShouldUpdateDateJoined = shouldUpdateDateJoined;
        }

        public string UserName { get; set; }
        public bool ShouldUpdateDateJoined { get; set; }
    }

    public class EnsureViewerExistsCommandHandler : IRequestHandler<EnsureViewerExistsCommand, Unit>
    {
        private readonly IRepository<Domain.Entities.Viewer> _viewerRepository;

        public EnsureViewerExistsCommandHandler(IRepository<Domain.Entities.Viewer> viewerRepository)
        {
            _viewerRepository = viewerRepository;
        }

        public async Task<Unit> Handle(EnsureViewerExistsCommand request, CancellationToken cancellationToken)
        {
            var viewer = _viewerRepository.Get().FirstOrDefault(x => x.DisplayName.Equals(request.UserName) && x.IsEnabled);

            var currentDate = DateTime.Now;

            if (viewer == null)
            {
                // Add new viewer
                var newViewer = new Domain.Entities.Viewer
                {
                    DateJoined = currentDate,
                    DisplayName = request.UserName,
                    LevelId = 1
                };

                await _viewerRepository.Add(newViewer);
            }
            else
            {
                await _viewerRepository.Update(viewer);

                if (request.ShouldUpdateDateJoined)
                {
                    viewer.DateJoined = currentDate; 
                }

                viewer.LastUpdate = currentDate;
            }

            await _viewerRepository.SaveChangesAsync();

            return new Unit();
        }
    }
}
