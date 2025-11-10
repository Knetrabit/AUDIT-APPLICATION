
using Microsoft.Extensions.FileProviders;
using VrsAuditApplication.Models;

var builder = WebApplication.CreateBuilder(args);

string connectionString = builder.Configuration.GetConnectionString("MyDb");

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddSingleton<DbService>();

// ✅ Add session services
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Optional: session timeout
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.Configure<MediaPathsOptions>(builder.Configuration.GetSection("Paths"));
builder.Services.AddSingleton<MediaHelper>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

//For videosPath from E:\Plaza\Download\Images
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(@"D:\Plaza\Download\Images"),
    RequestPath = "/Plaza/Download/Images"
});

// For imagesPath from E:\Plaza\Download\NPRImages
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(@"D:\Plaza\Download\NPRImages"),
    RequestPath = "/Plaza/Download/NPRImages"
});

// For imagesPath from E:\Plaza\Download\CCHImages
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(@"D:\Plaza\Download\CCHImages"),
    RequestPath = "/Plaza/Download/CCHImages"
});

app.UseRouting();

// ✅ Use session middleware
app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
