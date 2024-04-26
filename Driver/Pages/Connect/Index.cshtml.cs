using EthernetGlobalData.Data;
using EthernetGlobalData.Protocol;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace EthernetGlobalData.Pages.Connect
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IServiceScopeFactory _serviceProvider;
        public const string SessionKey = "_Running";        

        public IndexModel(ApplicationDbContext context, IServiceScopeFactory serviceProvider)
        {
            _context = context;
            _serviceProvider = serviceProvider;
        }

        public IList<EthernetGlobalData.Models.Point> Point { get; set; } = default!;
        public IList<EthernetGlobalData.Models.Node> Node { get; set; } = default!;
        public IList<EthernetGlobalData.Models.Channel> Channel { get; set; } = default!;

        public async Task OnGetAsync()
        {
            Node = await _context.Node
                .Include(n => n.Channel)
                .Include(n => n.Points)
                .ToListAsync();

            Point = await _context.Point
                .Include(p => p.Node)
                .ToListAsync();

            Channel = await _context.Channel.ToListAsync();
        }

        public async Task<IActionResult> OnPostAsync(string? start, string? stop)
        {   
            if (!ModelState.IsValid)
            {
                return Page();
            }

            Node = await _context.Node
                .Include(n => n.Channel)
                .Include(n => n.Points)
                .ToListAsync();

            Point = await _context.Point
                .Include(p => p.Node)
                .ToListAsync();

            Channel = await _context.Channel.ToListAsync();

            CancellationTokenSource token = new CancellationTokenSource();

            EthernetGlobalData.Protocol.Protocol protocol = new EthernetGlobalData.Protocol.Protocol(_serviceProvider, token);

            if (!string.IsNullOrEmpty(start))
            {
                HttpContext.Session.SetBoolean("_Running", true);

                await protocol.Start(Node, Channel);                             
            }
            else if (!string.IsNullOrEmpty(stop))
            {
                HttpContext.Session.SetBoolean("_Running", false);

                protocol.Stop();
            }

            return RedirectToPage("./Index");
        }
    }

    public static class SessionExtensions
    {
        public static bool? GetBoolean(this ISession session, string key)
        {
            var data = session.Get(key);
            if (data == null)
            {
                return null;
            }
            return BitConverter.ToBoolean(data, 0);
        }
        public static void SetBoolean(this ISession session, string key, bool value)
        {
            session.Set(key, BitConverter.GetBytes(value));
        }
    }
}
