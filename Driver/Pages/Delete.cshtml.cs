using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace EthernetGlobalData.Models
{
    public class DeleteModel : PageModel
    {
        private readonly EthernetGlobalData.Data.ApplicationDbContext _context;

        public DeleteModel(EthernetGlobalData.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Channel Channel { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var channel = await _context.Channel.FirstOrDefaultAsync(m => m.ChannelID == id);

            if (channel == null)
            {
                return NotFound();
            }
            else
            {
                Channel = channel;
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var channel = await _context.Channel.FindAsync(id);
            if (channel != null)
            {
                Channel = channel;
                _context.Channel.Remove(Channel);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}
