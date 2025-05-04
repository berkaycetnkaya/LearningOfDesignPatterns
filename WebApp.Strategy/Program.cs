
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NuGet.Configuration;
using WebApp.Models;
using WebApp.Strategy.Models;
using WebApp.Strategy.Repositories;

var builder = WebApplication.CreateBuilder(args);

// 1. Servisleri ekle
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<AppIdentityDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServer")));

builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<IProductRepository>(sp =>
{
    var httpContextAccessor = sp.GetRequiredService<IHttpContextAccessor>();

    var claim = httpContextAccessor.HttpContext.User.Claims.Where(claim => claim.Type == WebApp.Strategy.Models.Settings.claimDataBaseType).FirstOrDefault();

    var context = sp.GetRequiredService<AppIdentityDbContext>();

    if (claim != null) return new ProductRepositoryFromSqlServer(context);

    var databaseType = (EDatabaseType)int.Parse(claim.Value);

    return databaseType switch
    {
        EDatabaseType.SqlServer => new ProductRepositoryFromSqlServer(context),
        EDatabaseType.MongoDb => new ProductRepositoryFromSqlServer(context),
        _ => throw new NotImplementedException()
    };


});


// ?? Identity servisini doðru User tipiyle tanýt
builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
{
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<AppIdentityDbContext>()
.AddDefaultTokenProviders(); // Token iþlemleri için (þifre reset vs.)

// 2. Uygulamayý oluþtur
var app = builder.Build();



// 3. Migration ve ilk kullanýcýyý oluþtur
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var dbContext = services.GetRequiredService<AppIdentityDbContext>();
    var userManager = services.GetRequiredService<UserManager<AppUser>>();

    dbContext.Database.Migrate();

    if (!userManager.Users.Any())
    {
        var user = new AppUser
        {
            UserName = "berkay",
            Email = "berkayerdo@hotmail.com"
        };

        var result = await userManager.CreateAsync(user, "Password12*");
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                Console.WriteLine(error.Description);
            }
        }
    }
}

// 4. HTTP pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication(); // ?? bu þart!
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
