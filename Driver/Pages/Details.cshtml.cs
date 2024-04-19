using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace EthernetGlobalData.Models
{
    public class DetailsModel : PageModel
    {
        private readonly EthernetGlobalData.Data.ApplicationDbContext _context;

        public DetailsModel(EthernetGlobalData.Data.ApplicationDbContext context)
        {
            _context = context;
        }

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
    }
}
