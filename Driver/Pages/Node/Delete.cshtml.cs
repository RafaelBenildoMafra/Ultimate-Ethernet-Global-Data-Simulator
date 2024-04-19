using EthernetGlobalData.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace EthernetGlobalData.Pages.Node
{
    public class DeleteModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DeleteModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
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

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var node = await _context.Node.FindAsync(id);
            if (node != null)
            {
                Node = node;
                _context.Node.Remove(Node);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}
