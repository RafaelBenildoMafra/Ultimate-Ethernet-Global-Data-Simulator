using EthernetGlobalData.Data;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace EthernetGlobalData.Pages.Point
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<EthernetGlobalData.Models.Point> Point { get; set; } = default!;

        public async Task OnGetAsync()
        {
            Point = await _context.Point
                .Include(p => p.Node)
                .ToListAsync();

        }
    }
}
