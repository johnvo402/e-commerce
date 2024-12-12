using App.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace App.Data.Entities
{
    public class Message
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public DateTime Timestamp { get; set; }
        public int ToRoomId { get; set; }
        public Room ToRoom { get; set; }
        public AppUser FromUser { get; set; }

    }
}
