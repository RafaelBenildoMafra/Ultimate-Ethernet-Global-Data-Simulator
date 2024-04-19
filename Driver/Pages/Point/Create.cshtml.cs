using EthernetGlobalData.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EthernetGlobalData.Pages.Point
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
            ViewData["NodeID"] = new SelectList(_context.Node, "NodeID", "NodeName");
            return Page();
        }

        [BindProperty]
        public EthernetGlobalData.Models.Point Point { get; set; } = default!;

        // To protect from overposting attacks, see https://aka.ms/RazorPagesCRUD
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Point.Add(Point);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
