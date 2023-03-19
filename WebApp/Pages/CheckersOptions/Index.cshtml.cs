using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using DAL.Db;
using Domain;

namespace WebApp.Pages_CheckersOptions
{
    public class IndexModel : PageModel
    {
        private readonly DAL.Db.AppDbContext _context;

        public IndexModel(DAL.Db.AppDbContext context)
        {
            _context = context;
        }

        public IList<CheckersOption> CheckersOption { get;set; } = default!;

        public async Task OnGetAsync()
        {
            CheckersOption = await _context.CheckersOptions.ToListAsync();

            
            if (_context.CheckersOptions.Count() == 0 && !_context.CheckersOptions.Any(o=>o.Name == "Standard game"))
            {
                var option = new CheckersOption()
                {
                    Name = "Standard game"
                };
                CheckersOption.Add(option);
                await _context.SaveChangesAsync(); 
                    
            }
        }
        public bool IsGamesUsingGivenOptions(int optionsId)
        {
            var isUsing = false;
            var games = _context.CheckersGames;
            foreach (var game in games)
            {
                if (game.CheckersOption != null && game.CheckersOption.Id == optionsId)
                {
                    isUsing = true;
                    break;
                }
            }

            return isUsing;
        }
    }
}
