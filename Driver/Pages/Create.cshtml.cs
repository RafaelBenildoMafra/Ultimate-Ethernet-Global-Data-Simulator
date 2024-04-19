using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EthernetGlobalData.Models
{
    public class CreateModel : PageModel
    {
        private readonly EthernetGlobalData.Data.ApplicationDbContext _context;

        public CreateModel(EthernetGlobalData.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult OnGet()
        {
            return Page();
        }

        [BindProperty]
        public Channel Channel { get; set; } = default!;

        // To protect from overposting attacks, see https://aka.ms/RazorPagesCRUD
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Channel.Add(Channel);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
