using WebApp.Template.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Servisleri ekle
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<AppIdentityDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServer")));

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
            Email = "berkayerdo@hotmail.com",
            PictureUrl="/userpictures/Oracle-Symbol.png",
            Description = "berkay description berkay"
        };

        
        for (global::System.Int32 i = 0; i < 5; i++)
        {
            user.UserName += i;
            var result = await userManager.CreateAsync(user, "Password12*"+i);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    Console.WriteLine(error.Description);
                }
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
