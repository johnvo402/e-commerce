using System;
using System.Collections.Generic;

#nullable disable

namespace App.Model
{
    public partial class Message
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public DateTime Timestamp { get; set; }
        public int ToRoomId { get; set; }
        public string FromUserId { get; set; }

        public virtual User FromUser { get; set; }
        public virtual Room ToRoom { get; set; }
    }
}
