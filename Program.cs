using ReniPressApp.Data;
using ReniPressApp.Data.Repositories;
using ReniPressApp.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddScoped<Conexion>();
builder.Services.AddScoped<IPoblacionRepository, PoblacionRepository>();
builder.Services.AddScoped<IIraRepository, IraRepository>();
builder.Services.AddScoped<IAnalisisEpidemiologicoService, AnalisisEpidemiologicoService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
