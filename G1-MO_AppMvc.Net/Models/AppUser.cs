using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;
using App.Data.Entities;

namespace App.Models
{
    public class AppUser : IdentityUser
    {
       
        [Column(TypeName = "nvarchar")]
        [StringLength(400)]
        public string HomeAdress { get; set; }
        // [Required]       
        [DataType(DataType.Date)]
        public DateTime? BirthDate { get; set; }
        [Column(TypeName = "int")]
        public int Gender { get; set; }   
        
        [DataType(DataType.DateTime)]
        public DateTime? DateCreate { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime? LastLogin { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime? BlockDate { get; set; }

        [Column(TypeName = "int")]
        public int Status { get; set; }

        
        [Column(TypeName = "nvarchar")]
        [StringLength(400)]
        public string FullName { get; set; }
        [Column(TypeName = "nvarchar")]
        [StringLength(400)]
        public string Avt {  get; set; }

        public ICollection<Room> Rooms { get; set; }
        public ICollection<Message> Messages { get; set; }
    }
}
