using ThereforeReportGenerator.Components;
using ThereforeReportGenerator.Controllers;
using ThereforeReportGenerator.Models;
using Newtonsoft.Json;
using ThereforeReportGenerator.Context;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

if (!File.Exists("Data/templates.json")) { File.Copy("./Defaults/templates.json", "./Data/templates.json"); }
List<MailTemplate> tempTemplates = JsonConvert.DeserializeObject<List<MailTemplate>>(File.ReadAllText("./Data/templates.json")) ?? new List<MailTemplate>();
if (tempTemplates.Count == 0) { throw new Exception("Invalid Mail Templates"); }

builder.Services.AddSingleton< List<MailTemplate> >(tempTemplates);
builder.Services.AddSqlite<ReportDbContext>("data source=./Data/data.db");
builder.Services.AddHostedService<BackgroundProcessingController>();
builder.Services.AddScoped<ReportController>();


var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ReportDbContext>();
    if (File.Exists("./Imports/deletedb.txt")) 
    {
        app.Logger.LogWarning("Deleting database");
        db.Database.EnsureDeleted();
    }
    //db.Database.EnsureDeleted();
    db.Database.EnsureCreated();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    //app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Logger.LogWarning($"Current Culture: {Thread.CurrentThread.CurrentCulture.Name}");

app.Run();
