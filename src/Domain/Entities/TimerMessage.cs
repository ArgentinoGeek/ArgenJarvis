namespace Domain.Entities
{
    public class TimerMessage : Entity
    {
        public string Name { get; set; }
        public string Content { get; set; }
        public int PeriodInMilliseconds { get; set; }
    }
}
