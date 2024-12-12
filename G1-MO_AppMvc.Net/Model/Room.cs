using System;
using System.Collections.Generic;

#nullable disable

namespace App.Model
{
    public partial class Room
    {
        public Room()
        {
            Messages = new HashSet<Message>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string AdminId { get; set; }
        public string User2Id { get; set; }

        public virtual User Admin { get; set; }
        public virtual User User2 { get; set; }
        public virtual ICollection<Message> Messages { get; set; }
    }
}
