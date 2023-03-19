using DAL.Db;
using Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApp.Pages;

public class IndexModel : PageModel
{
    private readonly DAL.Db.AppDbContext _context;
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(ILogger<IndexModel> logger, AppDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task OnGetAsync()
    {
        if (_context.CheckersOptions.Count() == 0)
        {
            var option = new CheckersOption()
            {
                Name = "Standard game",
                
            };
            _context.CheckersOptions.Add(option);
            await _context.SaveChangesAsync();
        }
    }
}