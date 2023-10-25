using System.Net.Security;
using Microsoft.AspNetCore.Mvc;
using foodbyte.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using foodbyte.Models;
using NuGet.Versioning;

namespace foodbyte.Controllers;


public class AdminController : Controller
{
    
    
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _um;
    public AdminController(ApplicationDbContext db, UserManager<ApplicationUser> um)
    {
        _db = db;
        _um = um;
    }
    
    
    // GET
    [Authorize(Roles = "Admin")]
    public IActionResult ManageUsers()
    {
        var user = _um.GetUserAsync(User).Result;
        if (user != null)
        {
            var recipes = _db.Users.ToList();
            return View(recipes);
        }
        
        return RedirectToAction(nameof(ManageUsers));
        
    }

    

    
    [HttpGet]
    public IActionResult ManageUsers(string id)
    {
        var users = _um.GetUserAsync(User).Result;
        if (users != null)
        {

            var user = _um.Users.ToList();

            return View(user);
        }
        
        return RedirectToAction(nameof(ManageUsers));
        
    }
    
    
    
    [HttpPost, ActionName("ManageUsers")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id)
    {
        ApplicationUser user = await _um.FindByIdAsync(id);
        if (user != null)
        {
            _um.DeleteAsync(user);
        }
        
        return View("ManageUsers", _um.Users);
    }
    
    

    
}