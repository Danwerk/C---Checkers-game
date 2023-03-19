using System.Text.Json;
using DAL;
using Domain;
using GameBrain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;


namespace WebApp.Pages_CheckersGames;

public class Play : PageModel
{
    private readonly IGamePlayRepository _repo;

    public Play(IGamePlayRepository repo)
    {
        _repo = repo;
    }

    public EGamePiece?[][] Board { get; set; } = default!;
    public CheckersGameState GameState { get; set; } = default!;
    public CheckersGame Game { get; set; } = default!;
    public CheckersBrain Brain { get; set; } = default!;
    public bool? WonByBlack { get; set; }
    public int? FromX { get; set; }
    public int? FromY { get; set; }
    public bool StartMove { get; set; } = true;
    public int PlayerNo { get; set; }

    public IActionResult OnGet(int? id, int? fromX, int? fromY, int? toX, int? toY, int? playerNo, bool? checkAi)
    {
        CheckersGameState state;
        FromX = fromX;
        FromY = fromY;

        // Check whether the first click on board cell was done
        if (fromX.HasValue && fromY.HasValue)
        {
            StartMove = false;
        }

        if (id == null)
        {
            return RedirectToPage("/Index", new { error = "No game id was provided" });
        }


        if (playerNo == null || playerNo.Value < 0 || playerNo.Value > 1)
        {
            return RedirectToPage("/Index",
                new { error = "No player number was provided or is not in allowed range." });
        }

        PlayerNo = playerNo.Value;
        // playerNo 0 - first player. first player is always white
        // PlayerNo 1 - second player. second player is always black;


        var game = _repo.GetGameById(id);
        if (game == null || game.CheckersOption == null || game.CheckersGameStates == null)
        {
            return NotFound();
        }

        Game = game;
        Brain = new CheckersBrain(game.CheckersOption);

        
        // get Game's last state
        GameState = game.CheckersGameStates.Last();


        // Unserialize Game's last state
        Board = JsonSerializer.Deserialize<EGamePiece?[][]>(GameState.SerializedGameState)!;
        Brain.SetGameBoard(Board, GameState);

        
        if (Game.GameOverAt != null)
        {
            WonByBlack = Game.GameWonByPlayer == Game.Player2Name;
            return Page();
        }

        // AI MOVEMENT
        if (checkAi.HasValue && (!Brain.NextMoveByBlack() && Game.Player1Type == EPlayerType.Ai || Brain.NextMoveByBlack() && Game.Player2Type == EPlayerType.Ai))
        {
            Brain.MovePieceByAi(playerNo != 1);
            // When move is done, create a new state 
            state = new CheckersGameState
            {
                SerializedGameState = JsonSerializer.Serialize(Brain.GetBoard()),
                NextMoveByBlack = Brain.NextMoveByBlack()
            };
            GameState = state;
            Game.CheckersGameStates.Add(state);
            _repo.UpdateGame(game);
            return Page();
        }


        if (toX == null || toY == null || fromX == null || fromY == null)
        {
            return Page();
        }


        // If player wants to change piece. Player should click once more on selected piece and then choose another piece to move.
        if (toX == fromX && toY == fromY)
        {
            return RedirectToPage("Play", new { id = Game.Id, playerNo = PlayerNo });
        }


        
        // HUMAN movement
        Brain.MovePiece((int)fromX, (int)fromY, (int)toX, (int)toY);
        // When move is done, create a new state 
        state = new CheckersGameState
        {
            SerializedGameState = JsonSerializer.Serialize(Brain.GetBoard()),
            NextMoveByBlack = Brain.NextMoveByBlack()
        };
        

        StartMove = true;
        Game.CheckersGameStates.Add(state);
        GameState = state;
        

        if (Brain.IsGameOver())
        {
            Game.GameWonByPlayer = Brain.GameWonByBlack() ? Game.Player2Name : Game.Player1Name;
            Game.GameOverAt = DateTime.Now;
        }


        _repo.UpdateGame(game);

        if (Game.GameOverAt != null)
        {
            WonByBlack = Game.GameWonByPlayer == Game.Player2Name;
        }

        return Page();
    }
    
    public bool IsPlayerMove()
    {
        if (PlayerNo == 0 && !GameState.NextMoveByBlack)
        {
            return true;
        }

        if (PlayerNo == 1 && GameState.NextMoveByBlack)
        {
            return true;
        }

        return false;
    }
}