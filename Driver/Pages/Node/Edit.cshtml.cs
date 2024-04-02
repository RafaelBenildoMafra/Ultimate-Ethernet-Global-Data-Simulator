using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EthernetGlobalData.Data;
using EthernetGlobalData.Models;

namespace EthernetGlobalData.Pages.Node
{
    public class EditModel : PageModel
    {
        private readonly ProtocolContext _context;

        public EditModel(ProtocolContext context)
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

            var node = await _context.Node.FirstOrDefaultAsync(m => m.NodeID == id);
            if (node == null)
            {
                return NotFound();
            }
            Node = node;
            ViewData["ChannelID"] = new SelectList(_context.Channel, "ChannelID", "IP");
            return Page();
        }

        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Attach(Node).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!NodeExists(Node.NodeID))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToPage("./Index");
        }

        private bool NodeExists(int id)
        {
            return _context.Node.Any(e => e.NodeID == id);
        }
    }
}
