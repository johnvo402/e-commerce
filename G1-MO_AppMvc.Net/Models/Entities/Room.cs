using App.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace App.Data.Entities
{
    public class Room
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public AppUser Admin { get; set; }
       
        public string User2Id { get; set; }
        public ICollection<Message> Messages { get; set; }

    }
}
