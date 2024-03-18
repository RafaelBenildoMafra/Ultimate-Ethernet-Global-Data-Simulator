using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using EthernetGlobalData.Data;

namespace EthernetGlobalData.Models
{
    public class CreateModel : PageModel
    {
        private readonly EthernetGlobalData.Data.ProtocolContext _context;

        public CreateModel(EthernetGlobalData.Data.ProtocolContext context)
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
