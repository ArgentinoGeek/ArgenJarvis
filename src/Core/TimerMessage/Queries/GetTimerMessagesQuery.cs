using AutoMapper;
using Core.TimerMessage.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Core.TimerMessage.Commands
{
    public class GetTimerMessagesQuery : IRequest<IEnumerable<TimerMessageDto>>
    {
        public GetTimerMessagesQuery() { }
    }

    public class GetTimerMessagesQueryHandler : IRequestHandler<GetTimerMessagesQuery, IEnumerable<TimerMessageDto>>
    {
        private readonly IRepository<Domain.Entities.TimerMessage> _timerMessageRepository;
        private readonly IMapper _mapper;

        public GetTimerMessagesQueryHandler(IRepository<Domain.Entities.TimerMessage> timerMessageRepository, IMapper mapper)
        {
            _timerMessageRepository = timerMessageRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<TimerMessageDto>> Handle(GetTimerMessagesQuery request, CancellationToken cancellationToken)
        {
            return await Task.FromResult(
                _mapper.Map<IEnumerable<TimerMessageDto>>(_timerMessageRepository.Get()
                .Where(x => x.IsEnabled)
                .OrderBy(x => x.Name)
                .ToList()));
        }
    }
}
