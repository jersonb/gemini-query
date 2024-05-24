using GeminiQuery.Mvc.Services;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

var services = builder.Services;

services.AddScoped<QuestionService>();

services.AddSerilog(lc =>
{
    lc.MinimumLevel.Debug()
      .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
      .Enrich.FromLogContext()
      .WriteTo.Console(outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss zzz} {Level:u3}] {Message:lj}{NewLine}{Exception}");
});

services.AddControllersWithViews();

var app = builder.Build();

app.UseExceptionHandler("/Home/Error");
// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
app.UseHsts();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Questions}/{action=Create}");

app.Run();