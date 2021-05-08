namespace Core.TimerMessage.DTOs
{
    public class TimerMessageDto : DtoBase
    {
        public string Name { get; set; }
        public string Content { get; set; }
        public int PeriodInMilliseconds { get; set; }
    }
}
