using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Core.Viewer.Queries
{
    public class GetPointsByUserNameQuery : IRequest<int>
    {
        public GetPointsByUserNameQuery(string userName)
        {
            UserName = userName;
        }

        public string UserName { get; set; }
    }

    public class GGetPointsByUserNameQueryHandler : IRequestHandler<GetPointsByUserNameQuery, int>
    {
        private readonly IRepository<Domain.Entities.Viewer> _viewerRepository;

        public GGetPointsByUserNameQueryHandler(IRepository<Domain.Entities.Viewer> viewerRepository)
        {
            _viewerRepository = viewerRepository;
        }

        public Task<int> Handle(GetPointsByUserNameQuery request, CancellationToken cancellationToken)
        {
            var viewer = _viewerRepository.Get().FirstOrDefault(x => x.DisplayName.Equals(request.UserName) && x.IsEnabled);

            var points = viewer != null ? viewer.Points : 0;

            return Task.FromResult(points);
        }
    }
}
