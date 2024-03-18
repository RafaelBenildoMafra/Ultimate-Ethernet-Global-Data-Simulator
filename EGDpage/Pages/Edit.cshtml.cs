using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EthernetGlobalData.Data;

namespace EthernetGlobalData.Models
{
    public class EditModel : PageModel
    {
        private readonly EthernetGlobalData.Data.ProtocolContext _context;

        public EditModel(EthernetGlobalData.Data.ProtocolContext context)
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

            var channel =  await _context.Channel.FirstOrDefaultAsync(m => m.ChannelID == id);
            if (channel == null)
            {
                return NotFound();
            }
            Channel = channel;
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

            _context.Attach(Channel).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ChannelExists(Channel.ChannelID))
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

        private bool ChannelExists(int id)
        {
            return _context.Channel.Any(e => e.ChannelID == id);
        }
    }
}
