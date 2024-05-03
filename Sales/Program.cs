using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Sales.Data;
using Sales.Data.Models;
using Sales.Models.Cart;
using Sales.Options;
using Sales.Services;
using System.Globalization;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole<int>>()
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddControllersWithViews();

builder.Services.Configure<MailOptions>(builder.Configuration.GetSection("Mail"));

builder.Services.AddScoped<IEmailSender, EmailSenderService>();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(opt =>
{
    opt.Cookie.HttpOnly = true;
    opt.Cookie.IsEssential = true;
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped(sp => CartService.GetCart(sp));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}


var scopeFactory = app.Services.GetRequiredService<IServiceScopeFactory>();

using var scope = scopeFactory.CreateAsyncScope();

await using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
var userStore = scope.ServiceProvider.GetRequiredService<IUserStore<ApplicationUser>>();
var userEmailStore = (IUserEmailStore<ApplicationUser>)userStore;
await context.Database.MigrateAsync();

async Task CreateNewUserAsync(string email, string password, string name, string? address, string? phoneNumber, string role)
{
    var scopeFactory = app.Services.GetRequiredService<IServiceScopeFactory>();

    using var scope = scopeFactory.CreateAsyncScope();

    await using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var userStore = scope.ServiceProvider.GetRequiredService<IUserStore<ApplicationUser>>();
    var userEmailStore = (IUserEmailStore<ApplicationUser>)userStore;

    var user = new ApplicationUser
    {
        EmailConfirmed = true,
        Name = name,
        PhoneNumber = phoneNumber
    };
    await userStore.SetUserNameAsync(user, email, CancellationToken.None);
    await userEmailStore.SetEmailAsync(user, email, CancellationToken.None);
    var result = await userManager.CreateAsync(user, password);

    

    if (result.Succeeded)
    {
        user = await userManager.FindByNameAsync(email);
        if (user != null)
        {
            await userManager.AddToRoleAsync(user, role);

            if (address != null)
            {
                context.UserAddresses.Add(new UserAddress
                {
                    UserId = user.Id,
                    Address = address
                });

                await context.SaveChangesAsync();
            }
        }
    }
}

async Task CreateNewOrderAsync(int customerId, DateTime date, int employeeId)
{
    var scopeFactory = app.Services.GetRequiredService<IServiceScopeFactory>();

    using var scope = scopeFactory.CreateAsyncScope();

    await using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var userStore = scope.ServiceProvider.GetRequiredService<IUserStore<ApplicationUser>>();
    var userEmailStore = (IUserEmailStore<ApplicationUser>)userStore;

    var customer = await context.Users.Include(u => u.UserAddresses).FirstOrDefaultAsync(o => o.Id == customerId);


    var order = new Order
    {
        CustomerId = customerId,
        CreatedDate = date,
        EmployeeId = employeeId,
        Address = customer!.UserAddresses.FirstOrDefault()!.Address
    };

    await context.Orders.AddAsync(order);
    await context.SaveChangesAsync();
}

async Task CreateNewOrderedProductsAsync(int orderId, int productId, int quntity)
{
    var scopeFactory = app.Services.GetRequiredService<IServiceScopeFactory>();

    using var scope = scopeFactory.CreateAsyncScope();

    await using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var userStore = scope.ServiceProvider.GetRequiredService<IUserStore<ApplicationUser>>();
    var userEmailStore = (IUserEmailStore<ApplicationUser>)userStore;


    var orderedProduct = new OrderedProduct
    {
        OrderId = orderId,
        ProductId = productId,
        Quantity = quntity
    };

    await context.OrderedProducts.AddAsync(orderedProduct);
    await context.SaveChangesAsync();
}

if (!context.Users.Any())
{
    // Создаем пользователей
    await CreateNewUserAsync("ivanov@gmail.com", "Qa_12345", "Иванов Максим Михайлович", "Могилев, Вавилова, 12", "8029270657", "User");
    await CreateNewUserAsync("petrov@gmail.com", "Qa_12345", "Петров Никита Сергеевич", "Могилев, Шмидта, 2", "8029567546", "User");
    await CreateNewUserAsync("kuleshov@gmail.com", "Qa_12345", "Кулешов Николай Иванович", "Минск, Абрикосовая, 4", "8029654786", "User");
    await CreateNewUserAsync("sergeev@gmail.com", "Qa_12345", "Сергеев Антон Михайлович", "Могилев, Первомайская, 21", "8029567524", "User");
    await CreateNewUserAsync("kamenev@gmail.com", "Qa_12345", "Каменев Павел Николаевич", "Минск, Автозаводская, 8", "8029789546", "User");
    await CreateNewUserAsync("listovoi@gmail.com", "Qa_12345", "Листовой Артем Тимофеевич", "Витебск, Березовая, 2", "8029236424", "User");
    await CreateNewUserAsync("peskovoi@gmail.com", "Qa_12345", "Песковой Максим Игоревич", "Витебск, Восточная, 5", "8029875356", "User");
    await CreateNewUserAsync("vavilov@gmail.com", "Qa_12345", "Вавилов Александр Валерьевич", "Брест, Космонавтов, 7", "8029236424", "User");
    await CreateNewUserAsync("krychkova@gmail.com", "Qa_12345", "Крючкова Ольга Георгиевна", "Гродно, Строителей, 4", "8029642568", "User");
    await CreateNewUserAsync("larina@gmail.com", "Qa_12345", "Ларина Арина Ивановна", "Витебск, Народная, 11", "8029754385", "User");
    await CreateNewUserAsync("polyakova@gmail.com", "Qa_12345", "Полякова Кристина Богдановна", "Минск, Боровая, 9", "8029876376", "User");
    await CreateNewUserAsync("petrova@gmail.com", "Qa_12345", "Петрова Вера Алексеевна", "Минск, Быховская, 12", "8029287226", "User");
    await CreateNewUserAsync("bogomolova@gmail.com", "Qa_12345", "Богомолова Полина Артуровна", "Могилев, Шмидта, 3", "8029298576", "User");
    await CreateNewUserAsync("rymyancev@gmail.com", "Qa_12345", "Румянцев Вячеслав Валентирович", "Могилев, Крупской, 31", "8029129816", "User");
    await CreateNewUserAsync("komarov@gmail.com", "Qa_12345", "Комаров Василий Иванович", "Брест, Мирная, 3", "8029186573", "User");

    // Создаем менеджеров
    await CreateNewUserAsync("ivanov22@mail.ru", "Qa_12345", "Иванов Алексей Михайлович", null, null, "Manager");
    await CreateNewUserAsync("petrov11@mail.ru", "Qa_12345", "Петров Илья Александрович", null, null, "Manager");
    await CreateNewUserAsync("alex12@mail.ru", "Qa_12345", "Алексеев Федор Тимофеевич", null, null, "Manager");
    await CreateNewUserAsync("valov23@mail.ru", "Qa_12345", "Валов Вадим Валерьевич", null, null, "Manager");
    await CreateNewUserAsync("kamen143@mail.ru", "Qa_12345", "Камневский Илья Федорович", null, null, "Manager");
    await CreateNewUserAsync("max24@mail.ru", "Qa_12345", "Ветров Максим Артурович", null, null, "Manager");
    await CreateNewUserAsync("artur12@mail.ru", "Qa_12345", "Лужин Артур Семенович", null, null, "Manager");
}

// Добавляем заказы
if (!context.Orders.Any())
{
    await CreateNewOrderAsync(1, new DateTime(2024, 4, 1), 5);
    await CreateNewOrderAsync(2, new DateTime(2024, 4, 2), 3);
    await CreateNewOrderAsync(3, new DateTime(2024, 4, 14), 3);
    await CreateNewOrderAsync(4, new DateTime(2024, 4, 15), 4);
    await CreateNewOrderAsync(5, new DateTime(2024, 4, 16), 4);
    await CreateNewOrderAsync(6, new DateTime(2024, 4, 17), 5);
    await CreateNewOrderAsync(7, new DateTime(2024, 4, 17), 3);
    await CreateNewOrderAsync(8, new DateTime(2024, 4, 18), 5);
    await CreateNewOrderAsync(9, new DateTime(2024, 4, 19), 6);
    await CreateNewOrderAsync(10, new DateTime(2024, 4, 19), 3);
    await CreateNewOrderAsync(11, new DateTime(2024, 4, 19), 6);
    await CreateNewOrderAsync(12, new DateTime(2024, 4, 20), 4);
    await CreateNewOrderAsync(13, new DateTime(2024, 4, 21), 4);
    await CreateNewOrderAsync(14, new DateTime(2024, 4, 21), 6);
    await CreateNewOrderAsync(15, new DateTime(2024, 4, 22), 5);
}

// Добавляем товары к заказу
if (!context.OrderedProducts.Any())
{
    await CreateNewOrderedProductsAsync(1, 1, 2);
    await CreateNewOrderedProductsAsync(2, 2, 3);
    await CreateNewOrderedProductsAsync(3, 5, 2);
    await CreateNewOrderedProductsAsync(4, 3, 1);
    await CreateNewOrderedProductsAsync(5, 6, 1);
    await CreateNewOrderedProductsAsync(6, 7, 1);
    await CreateNewOrderedProductsAsync(7, 8, 1);
    await CreateNewOrderedProductsAsync(8, 2, 1);
    await CreateNewOrderedProductsAsync(9, 9, 2);
    await CreateNewOrderedProductsAsync(10, 4, 2);
    await CreateNewOrderedProductsAsync(11, 10, 3);
    await CreateNewOrderedProductsAsync(12, 6, 2);
    await CreateNewOrderedProductsAsync(13, 11, 3);
    await CreateNewOrderedProductsAsync(14, 12, 3);
    await CreateNewOrderedProductsAsync(15, 3, 3);   
}

// Создаем админа
var user = await userManager.FindByNameAsync("admin@mail.com");

if (user == null)
{
    user = new ApplicationUser();
    user.EmailConfirmed = true;
    await userStore.SetUserNameAsync(user, "admin@mail.com", CancellationToken.None);
    await userEmailStore.SetEmailAsync(user, "admin@mail.com", CancellationToken.None);
    var result = await userManager.CreateAsync(user, "c8XBx%");

    if (result.Succeeded)
    {
        user = await userManager.FindByNameAsync("admin@mail.com");
        if (user != null)
        {
            await userManager.AddToRoleAsync(user, "Administrator");
        }
    }
}
else
{
    if (!await userManager.IsInRoleAsync(user, "Administrator"))
    {
        await userManager.AddToRoleAsync(user, "Administrator");
    }
}


app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseSession();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
