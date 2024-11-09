using Manager_App.Services;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Registration HttpClient
builder.Services.AddHttpClient();
// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<MongoDbService>();
builder.Services.AddScoped<ConditionerService>();

// Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login/LoginPage"; 
        options.LogoutPath = "/Home/Logout";
    });


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Middleware
app.UseAuthentication();
app.UseAuthorization();
app.UseDeveloperExceptionPage();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Register}/{action=LoginPage}/{id?}");

app.Run();

