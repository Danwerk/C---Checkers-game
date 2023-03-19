using DAL;
using Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApp.Pages_CheckersGames;

public class LaunchGame : PageModel
{
    
    private readonly IGamePlayRepository _repo;


    public LaunchGame(IGamePlayRepository repo)
    {
        _repo = repo;
    }

    public int GameId { get; set; }

    public IActionResult OnGet(int? id)
    {
        if (id == null)
        {
            return RedirectToPage("/Index", new {error = "No id!"});
        }

        GameId = (int) id;
        
        var game = _repo.GetGameById(id);

        
        if (game == null)
        {
            return RedirectToPage("/Index ", "No game found!");
        }


        if (game.Player1Type == EPlayerType.Human && game.Player2Type == EPlayerType.Human)
        {
            return Page();
        }
        
        
        return RedirectToPage("./Play", new {id = game.Id, playerNo = 0});
    }
}