using foodbyte.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace foodbyte.Controllers;

public class WheelSpinnController : Controller
{
    private ApplicationDbContext _db; 
 
    public WheelSpinnController(ApplicationDbContext db) 
    { 
        _db = db; 
    }
    
    
    public IActionResult SpinnWheel()
    {
        return View();
    }

    public async Task<IActionResult> ViewRecipe(int? id)
    {
        if (id == null || _db.Recipes == null)
        {
            return NotFound();
        }
        
        Thread.Sleep(6000);
        Random rand = new Random();
        int toSkip = rand.Next(0, _db.Recipes.Count());
        
        _db.Recipes.Skip(toSkip).Take(1).First();

        var recipes = await _db.Recipes 
            .Include(p => p.User)
            .FirstOrDefaultAsync(m => m.Id == id);
        if (recipes == null)
        {
            return NotFound();
        }

        return View(recipes);
    }
}