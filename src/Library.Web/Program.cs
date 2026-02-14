using Library.DataAccess;
using Library.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services
    .AddLibraryInfrastructure(builder.Configuration)
    .AddLibraryApplication();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.MapControllerRoute(
    "default",
    "{controller=Home}/{action=Index}/{id?}");

app.Run();