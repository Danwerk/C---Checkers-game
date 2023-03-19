using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using DAL;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using DAL.Db;
using Domain;
using GameBrain;

namespace WebApp.Pages_CheckersGames
{
    public class CreateModel : PageModel
    {
        private readonly DAL.Db.AppDbContext _context;
        private IGamePlayRepository _repo;

        public CreateModel(DAL.Db.AppDbContext context, IGamePlayRepository repo)
        {
            _context = context;
            _repo = repo;
        }

        public IActionResult OnGet()
        {
            OptionsSelectList = new SelectList(_context.CheckersOptions, "Id", "Name");
            return Page();

        }

        [BindProperty]
        public CheckersGame CheckersGame { get; set; } = default!;
        public SelectList OptionsSelectList { get; set; } = default!;

        // To protect from overposting attacks, see https://aka.ms/RazorPagesCRUD
        public async Task<IActionResult> OnPostAsync()
        {
          if (!ModelState.IsValid)
            {
                return Page();
            }

          
          var gameOptionsId = CheckersGame.CheckersOptionId;
          var options = _context.CheckersOptions.First(o => o.Id == gameOptionsId);

          
          var state = new CheckersGameState();
          var brain = new CheckersBrain(options);
          state.NextMoveByBlack = brain.NextMoveByBlack();
          state.SerializedGameState = JsonSerializer.Serialize(brain.GetBoard());
          CheckersGame.CheckersGameStates = new List<CheckersGameState>();
          CheckersGame.CheckersGameStates.Add(state);

          _repo.SaveGame(CheckersGame);

          if (CheckersGame.Player1Type == EPlayerType.Ai)
          {
              return RedirectToPage("./Play", new { id = CheckersGame.Id, playerNo = 1 });

          }if (CheckersGame.Player2Type == EPlayerType.Ai)
          {
              return RedirectToPage("./Play", new { id = CheckersGame.Id, playerNo = 0 });

          }

            return RedirectToPage("./LaunchGame", new {id = CheckersGame.Id});
        }
    }
}
