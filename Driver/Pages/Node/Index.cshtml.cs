using EthernetGlobalData.Data;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace EthernetGlobalData.Pages.Node
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<EthernetGlobalData.Models.Node> Node { get; set; } = default!;

        public async Task OnGetAsync()
        {
            Node = await _context.Node
                .Include(n => n.Channel)
                .Include(n => n.Points)
                .ToListAsync();
        }
    }
}
