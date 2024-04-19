using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace EthernetGlobalData.Models
{
    public class IndexModel : PageModel
    {
        private readonly EthernetGlobalData.Data.ApplicationDbContext _context;

        public IndexModel(EthernetGlobalData.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<Channel> Channel { get; set; } = default!;

        public async Task OnGetAsync()
        {
            Channel = await _context.Channel.ToListAsync();
        }
    }
}
