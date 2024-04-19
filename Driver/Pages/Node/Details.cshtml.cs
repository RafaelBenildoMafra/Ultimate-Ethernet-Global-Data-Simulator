using EthernetGlobalData.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace EthernetGlobalData.Pages.Node
{
    public class DetailsModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DetailsModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public EthernetGlobalData.Models.Node Node { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var node = await _context.Node
                .Include(p => p.Channel)
                .FirstOrDefaultAsync(m => m.NodeID == id);

            if (node == null)
            {
                return NotFound();
            }
            else
            {
                Node = node;
            }
            return Page();
        }
    }
}
