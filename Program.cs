using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using NuitInfo.Rubeus.Composants;
using NuitInfo.Rubeus.Composants.Account;
using NuitInfo.Rubeus.Data;
using NuitInfo.Rubeus.Repositories;
using MudBlazor.Services;
using NuitInfo.Rubeus.RadioOccitania.Services.Interfaces;
using NuitInfo.Rubeus.RadioOccitania.Services.Implementations;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddMudServices();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityUserAccessor>();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    })
    .AddIdentityCookies();

var postgresConnectionString = builder.Configuration["AUTH_STRING_POSTGREE"];
var mongoConnectionString = builder.Configuration["AUTH_STRING_MONGO"];
var varEnv3 = builder.Configuration["VAR_ENV_3"];
var varEnv4 = builder.Configuration["VAR_ENV_4"];
var varEnv5 = builder.Configuration["VAR_ENV_5"];
var varEnv6 = builder.Configuration["VAR_ENV_6"];
var varEnv7 = builder.Configuration["VAR_ENV_7"];
var varEnv8 = builder.Configuration["VAR_ENV_8"];
var varEnv9 = builder.Configuration["VAR_ENV_9"];
var varEnv10 = builder.Configuration["VAR_ENV_10"];

Console.WriteLine($"🧩 AUTH_STRING_POSTGREE = {postgresConnectionString}");
Console.WriteLine($"🧩 AUTH_STRING_MONGO = {mongoConnectionString}");
Console.WriteLine($"🧩 VAR_ENV_3 = {varEnv3}");
Console.WriteLine($"🧩 VAR_ENV_4 = {varEnv4}");
Console.WriteLine($"🧩 VAR_ENV_5 = {varEnv5}");
Console.WriteLine($"🧩 VAR_ENV_6 = {varEnv6}");
Console.WriteLine($"🧩 VAR_ENV_7 = {varEnv7}");
Console.WriteLine($"🧩 VAR_ENV_8 = {varEnv8}");
Console.WriteLine($"🧩 VAR_ENV_9 = {varEnv9}");
Console.WriteLine($"🧩 VAR_ENV_10 = {varEnv10}");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(postgresConnectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// 🧩 Config MongoDB
builder.Services.AddSingleton<IMongoClient>(_ =>
{
    var mongoUrl = new MongoUrl(mongoConnectionString);
    return new MongoClient(mongoUrl);
});

// On expose IMongoDatabase, basé sur la db du connection string
builder.Services.AddScoped<IMongoDatabase>(sp =>
{
    var mongoUrl = new MongoUrl(mongoConnectionString);
    var client = sp.GetRequiredService<IMongoClient>();
    return client.GetDatabase(mongoUrl.DatabaseName);
});

builder.Services.AddIdentityCore<ApplicationUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false; // ⬅️ important
        options.User.RequireUniqueEmail = true; 
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();


builder.Services.AddScoped<IChatMessageRepository, ChatMessageRepository>();
builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();

// ==========================
// Services Radio Occitania 
// ==========================
builder.Services.AddSingleton<IEnregistreurAudioService, EnregistreurAudioService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();


app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Add additional endpoints required by the Identity /Account Razor components.
app.MapAdditionalIdentityEndpoints();

app.Run();


// =====================
// Stockage temporaire : 
// =====================

// Config initiale
// var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
// builder.Services.AddDbContext<ApplicationDbContext>(options =>
//     options.UseSqlite(connectionString));

// BDD pour le Entity Framework avec PostgreSQL
// var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
//     ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// ======================

// Config initiale
// builder.Services.AddIdentityCore<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
//     .AddEntityFrameworkStores<ApplicationDbContext>()
//     .AddSignInManager()
//     .AddDefaultTokenProviders();