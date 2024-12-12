using App.Areas.Identity.Controllers;
using App.Model;
using App.Models;
using App.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

public class BannerHub : Hub
{
    private readonly AppDbContext _context;
    private readonly ILogger<RoleController> _logger;

    public BannerHub(AppDbContext context, ILogger<RoleController> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task GetFirstBanner()
    {
        try
        {
            // Lấy dòng đầu tiên từ bảng banner
            App.Models.Banner firstBanner = await _context.Banners.FirstOrDefaultAsync(); // Sử dụng FirstOrDefaultAsync để tránh ngoại lệ nếu không có dòng nào
            if (firstBanner != null)
                
            {
                if (firstBanner.Text != null)
                {
                    await Clients.Caller.SendAsync("ReceiveFirstBanner", firstBanner.Link, firstBanner.Text);
                } else
                {
                    firstBanner.Text = "";
                    await Clients.Caller.SendAsync("ReceiveFirstBanner", firstBanner.Link, firstBanner.Text);
                }
                    // Gửi đường dẫn của banner tới client
                   
            }
            else
            {
                // Trường hợp không có dữ liệu, gửi một thông báo hoặc xử lý phù hợp
                await Clients.Caller.SendAsync("NoBannerAvailable");
            }
        }
        catch (Exception ex)
        {
            // Ghi log lỗi và ném ngoại lệ lại để SignalR có thể gửi thông báo lỗi về client
            _logger.LogError("An unexpected error occurred while retrieving the first banner.", ex);
            throw;
        }

    }
}