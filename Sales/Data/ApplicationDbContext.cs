using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Sales.Data.Models;

namespace Sales.Data;

/// <summary>
/// Контекст базы данных
/// </summary>
/// <param name="options"></param>
public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser, IdentityRole<int>, int>(options)
{
    /// <summary>
    /// Адреса пользователей
    /// </summary>
    public DbSet<UserAddress> UserAddresses { get; set; }

    /// <summary>
    /// Категории
    /// </summary>
    public DbSet<Category> Categories { get; set; }

    /// <summary>
    /// Товары
    /// </summary>
    public DbSet<Product> Products { get; set; }

    /// <summary>
    /// Товары в заказе
    /// </summary>
    public DbSet<OrderedProduct> OrderedProducts { get; set; }

    /// <summary>
    /// Заказы
    /// </summary>
    public DbSet<Order> Orders { get; set; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="builder"></param>
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<UserAddress>()
            .HasOne(ua => ua.User)
            .WithMany(u => u.UserAddresses)
            .HasForeignKey(ua => ua.UserId)
            .IsRequired(true);

        builder.Entity<Category>()
            .Property(c => c.Name)
            .HasMaxLength(255)
            .IsRequired(true);

        builder.Entity<Product>()
            .Property(c => c.Name)
            .HasMaxLength(255)
            .IsRequired(true);

        builder.Entity<Product>()
            .HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId)
            .IsRequired(true);


        builder.Entity<Product>()
            .Property(p => p.Price)
            .HasPrecision(18, 2)
            .IsRequired(true);

        builder.Entity<Product>()
            .Property(p => p.Length)
            .HasPrecision(18, 2)
            .IsRequired(true);

        builder.Entity<Product>()
            .Property(p => p.Width)
            .HasPrecision(18, 2)
            .IsRequired(true);

        builder.Entity<Product>()
            .Property(p => p.Thickness)
            .HasPrecision(18, 2)
            .IsRequired(true);

        builder.Entity<OrderedProduct>()
            .HasOne(op => op.Product)
            .WithMany(p => p.OrderedProducts)
            .HasForeignKey(op => op.ProductId)
            .IsRequired(true);

        builder.Entity<OrderedProduct>()
            .HasOne(op => op.Order)
            .WithMany(o => o.OrderedProducts)
            .HasForeignKey(op => op.OrderId)
            .IsRequired(true);

        builder.Entity<Order>()
            .HasOne(o => o.Customer)
            .WithMany(c => c.Orders)
            .HasForeignKey(o => o.CustomerId)
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired(true);

        builder.Entity<Order>()
            .HasOne(o => o.Employee)
            .WithMany(c => c.AcceptedOrders)
            .HasForeignKey(o => o.EmployeeId)
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired(false);


        builder.Entity<IdentityRole<int>>()
            .HasData(new List<IdentityRole<int>>
            {
                new() {
                    Id = 1,
                    Name = "Administrator",
                    NormalizedName = "ADMINISTRATOR",
                    ConcurrencyStamp = Guid.NewGuid().ToString()
                },
                new() {
                    Id = 2,
                    Name = "Manager",
                    NormalizedName = "MANAGER",
                    ConcurrencyStamp = Guid.NewGuid().ToString()
                },
                new() {
                    Id = 3,
                    Name = "User",
                    NormalizedName = "USER",
                    ConcurrencyStamp = Guid.NewGuid().ToString()
                }
            });


        builder.Entity<Category>().HasData(new List<Category>
        {
            new()
            {
                Id = 1,
                Name = "Трубы",
                Image = "categories\\tube.jpeg"
            },
            new()
            {
                Id = 2,
                Name = "Сетка",
                Image = "categories\\setka.jpg"
            },
            new()
            {
                Id = 3,
                Name = "Проволока",
                Image = "categories\\provoloka.jpg"
            },
            new()
            {
                Id = 4,
                Name = "Листовой металл",
                Image = "categories\\list.jpg"
            }
        });

        builder.Entity<Product>().HasData(new List<Product>
        {
            new()
            {
                Id = 1,
                CategoryId = 1,
                Name = "Стальная труба",
                Length = 5.5f,
                Width = 2.8f,
                Thickness = 0.2f,
                Price = 2000.00m,
                Image = "products\\steel_tube.jpg"
            },
            new()
            {
                Id = 2,
                CategoryId = 1,
                Name = "Медная труба",
                Length = 4.4f,
                Width = 2.5f,
                Thickness = 0.3f,
                Price = 1200.00m,
                Image = "products\\cooper_tube.png"
            },
            new()
            {
                Id = 3,
                CategoryId = 1,
                Name = "Алюминиевая труба",
                Length = 5.4f,
                Width = 2.3f,
                Thickness = 0.5f,
                Price = 1000.00m,
                Image = "products\\al_tube.jpeg"
            },

            new()
            {
                Id = 4,
                CategoryId = 2,
                Name = "Медная сетка",
                Length = 2.1f,
                Width = 3.0f,
                Thickness = 0.03f,
                Price = 500.00m,
                Image = "products\\cooper_setka.jpg"
            },
            new()
            {
                Id = 5,
                CategoryId = 2,
                Name = "Алюминиевая сетка",
                Length = 1.9f,
                Width = 2.5f,
                Thickness = 0.05f,
                Price = 300.00m,
                Image = "products\\al_setka.jpg"
            },
            new()
            {
                Id = 6,
                CategoryId = 2,
                Name = "Стальная сетка",
                Length = 1.7f,
                Width = 5.0f,
                Thickness = 0.02f,
                Price = 80.00m,
                Image = "products\\steel_setka.jpg"
            },

            new()
            {
                Id = 7,
                CategoryId = 3,
                Name = "Алюминиевая проволока",
                Length = 1.1f,
                Width = 2.0f,
                Thickness = 0.1f,
                Price = 50.00m,
                Image = "products\\al_prov.jpg"
            },
            new()
            {
                Id = 8,
                CategoryId = 3,
                Name = "Медная проволока",
                Length = 1.4f,
                Width = 3.0f,
                Thickness = 0.2f,
                Price = 70.00m,
                Image = "products\\cooper_prov.jpg"
            },
            new()
            {
                Id = 9,
                CategoryId = 3,
                Name = "Стальная проволока",
                Length = 1.5f,
                Width = 4.0f,
                Thickness = 0.4f,
                Price = 190.00m,
                Image = "products\\steel_prov.jpg"
            },

            new()
            {
                Id = 10,
                CategoryId = 4,
                Name = "Алюминиевый лист",
                Length = 80.0f,
                Width = 150.0f,
                Thickness = 2.0f,
                Price = 5500.00m,
                Image = "products\\al_list.jpg"
            },
            new()
            {
                Id = 11,
                CategoryId = 4,
                Name = "Медный лист",
                Length = 90.0f,
                Width = 150.0f,
                Thickness = 3.0f,
                Price = 5000.00m,
                Image = "products\\cooper_list.png"
            },
            new()
            {
                Id = 12,
                CategoryId = 4,
                Name = "Стальной лист",
                Length = 70.0f,
                Width = 250.0f,
                Thickness = 4.0f,
                Price = 6000.00m,
                Image = "products\\steel_list.jpg"
            },
        });
    }
}