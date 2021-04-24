using System;

namespace Domain.Entities
{
    public class Viewer : Entity
    {
        public string DisplayName { get; set; }
        public DateTime DateJoined { get; set; }
        public int Points { get; set; }
        public int SecondsViewing { get; set; }
        public int LevelId { get; set; }
        public Level Level { get; set; }
    }
}
