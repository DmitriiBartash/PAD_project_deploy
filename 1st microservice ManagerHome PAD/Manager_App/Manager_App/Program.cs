using Manager_App.Services;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Registration HttpClient
builder.Services.AddHttpClient();
// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<MongoDbService>();
builder.Services.AddScoped<ConditionerService>();

// Добавляем сервисы для аутентификации с использованием Cookie Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login/LoginPage"; // Путь к странице входа
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

// Добавляем Middleware аутентификации
app.UseAuthentication();
app.UseAuthorization();
app.UseDeveloperExceptionPage();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Register}/{action=LoginPage}/{id?}");

app.Run();

