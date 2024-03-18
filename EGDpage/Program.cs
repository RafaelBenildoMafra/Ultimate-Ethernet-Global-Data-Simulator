using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using EthernetGlobalData.Data;
using EthernetGlobalData.Services;
using EthernetGlobalData.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddRazorPages();

builder.Services.AddScoped<IPointService, PointService>();

builder.Services.AddDbContext<ProtocolContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("ProtocolContext") ?? throw new InvalidOperationException("Connection string 'ProtocolContext' not found.")));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

//ENSURE CREATED DATABASE
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var context = services.GetRequiredService<ProtocolContext>();
    context.Database.EnsureCreated();
    // DbInitializer.Initialize(context);
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();
