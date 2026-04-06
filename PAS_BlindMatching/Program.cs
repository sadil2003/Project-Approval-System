using Microsoft.EntityFrameworkCore;
using PAS_BlindMatching.Data;
using PAS_BlindMatching.Data; // This tells the code where to find AppDbContext

var builder = WebApplication.CreateBuilder(args);

// 1️⃣ Register services for controllers with views
builder.Services.AddControllersWithViews();

// 2️⃣ Register the DbContext with SQL Server
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 3️⃣ Enable session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// 4️⃣ Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// ⚡ For local development, temporarily disable HTTPS to avoid ERR_CONNECTION_REFUSED
// You can re-enable in production
// app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseRouting();

// Enable session
app.UseSession();
app.UseAuthorization();

// 5️⃣ Map default controller route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

// 6️⃣ Run the app
app.Run();