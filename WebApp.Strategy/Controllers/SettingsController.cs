using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;
using MongoDB.Driver;
using System.Security.Claims;
using WebApp.Models;
using WebApp.Strategy.Models;

namespace WebApp.Strategy.Controllers
{
    [Authorize]
    public class SettingsController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;

        public SettingsController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public IActionResult Index()
        {
            Settings setting = new();
            if(User.Claims.Where(claim=> claim.Type == Settings.claimDataBaseType).FirstOrDefault()!=null)
            {
                setting.DatabaseType = (EDatabaseType)int.Parse(User.Claims.First(claim => claim.Type == Settings.claimDataBaseType).Value);
            }
            else
            {
                setting.DatabaseType = setting.GetDefaultDatabaseType;
            }

                return View();
        }

        [HttpPost]
        public async Task<IActionResult> ChangeDatabase(int databaseType)
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            var newClaim = new Claim(Settings.claimDataBaseType, databaseType.ToString());

            var claims = await _userManager.GetClaimsAsync(user);

            var hasDatabaseTypeClaim = claims.FirstOrDefault(claim => claim.Type == Settings.claimDataBaseType);

            if (hasDatabaseTypeClaim != null) {
                await _userManager.ReplaceClaimAsync(user, hasDatabaseTypeClaim, newClaim);
            }
            else
            {
                await _userManager.AddClaimAsync(user,  newClaim);  
            }

            await _signInManager.SignOutAsync();

            var authenticateResult =  await HttpContext.AuthenticateAsync();

            await _signInManager.SignInAsync(user, authenticateResult.Properties);

            return RedirectToAction(nameof(Index));    
        }
    }
}
