using App.Models;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using App.Data;
using App.Data.Entities;
using App.Hubs;
using App.Models;
using IdentityServer4.Models;

namespace App.Controllers
{
    [Route("api/Rooms")]
    [ApiController]
    public class RoomsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly IHubContext<ChatHub> _hubContext;
        public RoomsController(AppDbContext context,
          IMapper mapper, IHubContext<ChatHub> hubContext)
        {
            _context = context;
            _mapper = mapper;
            _hubContext = hubContext;
        }


        [HttpGet]
        public async Task<ActionResult<IEnumerable<RoomViewModel>>> Get()
        {
            var user = _context.Users.FirstOrDefault(u => u.UserName == User.Identity.Name);


            var rooms = await _context.Rooms.Include(i => i.Admin).Where(r => (r.Admin.Id == user.Id) || (r.User2Id == user.Id))
                 .ToListAsync();
            var roomsViewModel = rooms.Select(room =>
            {
                var roomViewModel = _mapper.Map<Room, RoomViewModel>(room);
                if (room.User2Id == user.Id)
                {
                    var otherUser = _context.Users.FirstOrDefault(u => u.Id == room.Admin.Id);
                   
                        roomViewModel.FullName = otherUser.FullName;
                        roomViewModel.Avt = otherUser.Avt;
                    
                   
                }
                else
                {
                    var otherUser = _context.Users.FirstOrDefault(u => u.Id == room.User2Id);
                    roomViewModel.FullName = otherUser.FullName;
                    roomViewModel.Avt = otherUser.Avt;

                }
                return roomViewModel;

            });

               

            return Ok(roomsViewModel);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Room>> Get(int id)
        {
            var room = await _context.Rooms.FindAsync(id);
            if (room == null)
                return NotFound();

            var roomViewModel = _mapper.Map<Room, RoomViewModel>(room);
            return Ok(roomViewModel);
        }



        [HttpPost]
        public async Task<ActionResult<Room>> Create(RoomViewModel roomViewModel)
        {
            
                


            var user = _context.Users.FirstOrDefault(u => u.UserName == User.Identity.Name);
            var user2 = _context.Users.FirstOrDefault(u => u.UserName == roomViewModel.Name);
            if (_context.Rooms.Any(r => r.Name == (user.UserName + user2.UserName)))
            {
                await _hubContext.Clients.All.SendAsync("onError", "You are texting with this person");
                return BadRequest("Invalid room name or room already exists");
            }
            var room = new Room()
            {
                Name = user.UserName + user2.UserName,
                Admin = user,
                User2Id = user2.Id
            };

            _context.Rooms.Add(room);
            await _context.SaveChangesAsync();

            await _hubContext.Clients.All.SendAsync("addChatRoom", new { id = room.Id, name = room.Name });

            return CreatedAtAction(nameof(Get), new { id = room.Id }, new { id = room.Id, name = room.Name });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Edit(int id, RoomViewModel roomViewModel)
        {
            if (_context.Rooms.Any(r => r.Name == roomViewModel.Name))
                return BadRequest("Invalid room name or room already exists");
           
            var room = await _context.Rooms
                .Include(r => r.Admin)
                .Where(r => r.Id == id && r.Admin.UserName == User.Identity.Name)
                .FirstOrDefaultAsync();

            if (room == null)
                return NotFound();

            room.Name = roomViewModel.Name;
            await _context.SaveChangesAsync();

            await _hubContext.Clients.All.SendAsync("updateChatRoom", new { id = room.Id, room.Name });

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            
            var room = await _context.Rooms
                .Include(r => r.Admin)
                .Where(r => r.Id == id && r.Admin.UserName == User.Identity.Name)
                .FirstOrDefaultAsync();

            if (room == null)
                return NotFound();

            _context.Rooms.Remove(room);
            await _context.SaveChangesAsync();

            await _hubContext.Clients.All.SendAsync("removeChatRoom", room.Id);
            await _hubContext.Clients.Group(room.Name).SendAsync("onRoomDeleted", string.Format("Room {0} has been deleted.\nYou are moved to the first available room!", room.Name));

            return NoContent();
        }

    }
}
