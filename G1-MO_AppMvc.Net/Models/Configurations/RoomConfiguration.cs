using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using App.Data.Entities;
using App.Models;

namespace App.Data.Configurations
{
    public class RoomConfiguration : IEntityTypeConfiguration<Room>
    {
       
        public void Configure(EntityTypeBuilder<Room> builder)
        {
            builder.ToTable("Rooms");

            builder.Property(s => s.Name).IsRequired().HasMaxLength(100);

            builder.HasOne(s => s.Admin)
                .WithMany(u => u.Rooms)
                
                .IsRequired();
            builder.Property(s => s.User2Id).IsRequired().HasMaxLength(450);
        }
    }
}
