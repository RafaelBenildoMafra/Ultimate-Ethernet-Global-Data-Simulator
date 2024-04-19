using EthernetGlobalData.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EthernetGlobalData.Pages.Node
{
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public CreateModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult OnGet()
        {
            ViewData["ChannelID"] = new SelectList(_context.Channel, "ChannelID", "ChannelName");
            return Page();
        }

        [BindProperty]
        public EthernetGlobalData.Models.Node Node { get; set; } = default!;

        // To protect from overposting attacks, see https://aka.ms/RazorPagesCRUD
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Node.Add(Node);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
