using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using EthernetGlobalData.Data;
using EthernetGlobalData.Models;

namespace EthernetGlobalData.Pages.Node
{
    public class CreateModel : PageModel
    {
        private readonly ProtocolContext _context;

        public CreateModel(ProtocolContext context)
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
