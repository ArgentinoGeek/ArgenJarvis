using System;

namespace Domain.Entities
{
    public class Message : Entity
    {
        public Viewer Viewer { get; set; }
        public string Content { get; set; }
        public DateTime DateReceived { get; set; }
    }
}
