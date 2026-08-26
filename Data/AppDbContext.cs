using EndustriB2C.Entities;
using Microsoft.EntityFrameworkCore;

namespace EndustriB2C.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<UserAddress> UserAddresses => Set<UserAddress>();
    public DbSet<Cart> Carts => Set<Cart>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<ProductCategory> ProductCategories => Set<ProductCategory>();
    public DbSet<ProductTranslation> ProductTranslations => Set<ProductTranslation>();
    public DbSet<ProductPrice> ProductPrices => Set<ProductPrice>();
    public DbSet<ProductImage> ProductImages => Set<ProductImage>();
    public DbSet<ProductFeaturesHeader> ProductFeaturesHeaders => Set<ProductFeaturesHeader>();
    public DbSet<ProductFeaturesDetail> ProductFeaturesDetails => Set<ProductFeaturesDetail>();
    public DbSet<Campaign> Campaigns => Set<Campaign>();
    public DbSet<OrderHeader> OrderHeaders => Set<OrderHeader>();
    public DbSet<OrderDetail> OrderDetails => Set<OrderDetail>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(e =>
        {
            e.ToTable("Users");
            e.HasIndex(x => x.Email).IsUnique();
            e.HasIndex(x => x.Phone);
            e.Property(x => x.Email).HasMaxLength(256).IsRequired();
            e.Property(x => x.Phone).HasMaxLength(20).IsRequired();
            e.Property(x => x.PasswordHash).HasMaxLength(500).IsRequired();
            e.Property(x => x.FirstName).HasMaxLength(80).IsRequired();
            e.Property(x => x.LastName).HasMaxLength(80).IsRequired();
            e.Property(x => x.RefreshTokenHash).HasMaxLength(500);
        });

        modelBuilder.Entity<UserAddress>(e =>
        {
            e.ToTable("UserAddresses");
            e.HasOne(x => x.User).WithMany(x => x.Addresses).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
            e.Property(x => x.Title).HasMaxLength(60).IsRequired();
            e.Property(x => x.FullName).HasMaxLength(160).IsRequired();
            e.Property(x => x.Phone).HasMaxLength(20).IsRequired();
            e.Property(x => x.City).HasMaxLength(80).IsRequired();
            e.Property(x => x.District).HasMaxLength(80).IsRequired();
            e.Property(x => x.AddressLine).HasMaxLength(500).IsRequired();
            e.Property(x => x.PostalCode).HasMaxLength(16);
        });

        modelBuilder.Entity<Cart>(e =>
        {
            e.ToTable("Carts");
            e.HasIndex(x => x.GuestToken).IsUnique().HasFilter("[GuestToken] IS NOT NULL");
            e.HasIndex(x => x.UserId);
            e.HasOne(x => x.User).WithMany(x => x.Carts).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<CartItem>(e =>
        {
            e.ToTable("CartItems");
            e.HasIndex(x => new { x.CartId, x.ProductId }).IsUnique();
            e.HasOne(x => x.Cart).WithMany(x => x.Items).HasForeignKey(x => x.CartId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Product).WithMany(x => x.CartItems).HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Product>(e =>
        {
            e.ToTable("Products");
            e.HasIndex(x => x.Sku).IsUnique();
            e.Property(x => x.Sku).HasMaxLength(50).IsRequired();
            e.Property(x => x.Category).HasMaxLength(120).IsRequired();
            e.Property(x => x.Manufacturer).HasMaxLength(160);
        });

        modelBuilder.Entity<Category>(e =>
        {
            e.ToTable("Categories");
            e.HasIndex(x => x.Slug).IsUnique();
            e.Property(x => x.Name).HasMaxLength(160).IsRequired();
            e.Property(x => x.Slug).HasMaxLength(160).IsRequired();
            e.Property(x => x.Description).HasMaxLength(1000);
            e.Property(x => x.ImageUrl).HasMaxLength(500);
        });

        modelBuilder.Entity<ProductCategory>(e =>
        {
            e.ToTable("ProductCategories");
            e.HasIndex(x => new { x.ProductId, x.CategoryId }).IsUnique();
            e.HasOne(x => x.Product).WithMany(x => x.ProductCategories).HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Category).WithMany(x => x.ProductCategories).HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ProductTranslation>(e =>
        {
            e.ToTable("ProductTranslations");
            e.HasIndex(x => new { x.ProductId, x.LanguageCode }).IsUnique();
            e.HasIndex(x => x.Slug);
            e.Property(x => x.LanguageCode).HasMaxLength(5).IsRequired();
            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
            e.Property(x => x.ShortDescription).HasMaxLength(500);
            e.Property(x => x.Slug).HasMaxLength(250).IsRequired();
            e.HasOne(x => x.Product).WithMany(x => x.Translations).HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ProductPrice>(e =>
        {
            e.ToTable("ProductPrices");
            e.Property(x => x.Amount).HasColumnType("decimal(18,2)");
            e.Property(x => x.Currency).HasMaxLength(3).IsRequired();
            e.HasOne(x => x.Product).WithMany(x => x.Prices).HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Campaign).WithMany(x => x.ProductPrices).HasForeignKey(x => x.CampaignId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<ProductImage>(e =>
        {
            e.ToTable("ProductImages");
            e.Property(x => x.FilePath).HasMaxLength(500).IsRequired();
            e.Property(x => x.AltText).HasMaxLength(200);
            e.HasOne(x => x.Product).WithMany(x => x.Images).HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ProductFeaturesHeader>(e =>
        {
            e.ToTable("ProductFeaturesHeaders");
            e.Property(x => x.Name).HasMaxLength(120).IsRequired();
            e.Property(x => x.Unit).HasMaxLength(30);
        });

        modelBuilder.Entity<ProductFeaturesDetail>(e =>
        {
            e.ToTable("ProductFeaturesDetails");
            e.HasIndex(x => new { x.ProductId, x.ProductFeaturesHeaderId }).IsUnique();
            e.Property(x => x.Value).HasMaxLength(300).IsRequired();
            e.HasOne(x => x.Product).WithMany(x => x.Features).HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Header).WithMany(x => x.Details).HasForeignKey(x => x.ProductFeaturesHeaderId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Campaign>(e =>
        {
            e.ToTable("Campaigns");
            e.Property(x => x.Name).HasMaxLength(160).IsRequired();
            e.Property(x => x.Description).HasMaxLength(1000);
            e.Property(x => x.DiscountValue).HasColumnType("decimal(18,2)");
            e.Property(x => x.MinOrderAmount).HasColumnType("decimal(18,2)");
            e.Property(x => x.CouponCode).HasMaxLength(40);
            e.HasIndex(x => x.CouponCode).IsUnique().HasFilter("[CouponCode] IS NOT NULL");
            e.Property(x => x.ImageUrl).HasMaxLength(500);
        });

        modelBuilder.Entity<OrderHeader>(e =>
        {
            e.ToTable("OrderHeaders");
            e.HasIndex(x => x.OrderNumber).IsUnique();
            e.Property(x => x.OrderNumber).HasMaxLength(30).IsRequired();
            e.Property(x => x.CustomerName).HasMaxLength(160).IsRequired();
            e.Property(x => x.CustomerEmail).HasMaxLength(256).IsRequired();
            e.Property(x => x.CustomerPhone).HasMaxLength(20).IsRequired();
            e.Property(x => x.ShippingFullName).HasMaxLength(160).IsRequired();
            e.Property(x => x.ShippingPhone).HasMaxLength(20).IsRequired();
            e.Property(x => x.ShippingCity).HasMaxLength(80).IsRequired();
            e.Property(x => x.ShippingDistrict).HasMaxLength(80).IsRequired();
            e.Property(x => x.ShippingAddressLine).HasMaxLength(500).IsRequired();
            e.Property(x => x.ShippingPostalCode).HasMaxLength(16);
            e.Property(x => x.SubTotal).HasColumnType("decimal(18,2)");
            e.Property(x => x.DiscountAmount).HasColumnType("decimal(18,2)");
            e.Property(x => x.Total).HasColumnType("decimal(18,2)");
            e.Property(x => x.Notes).HasMaxLength(1000);
            e.HasOne(x => x.User).WithMany(x => x.Orders).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.SetNull);
            e.HasOne(x => x.Campaign).WithMany(x => x.Orders).HasForeignKey(x => x.CampaignId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<OrderDetail>(e =>
        {
            e.ToTable("OrderDetails");
            e.Property(x => x.ProductName).HasMaxLength(200).IsRequired();
            e.Property(x => x.ProductSku).HasMaxLength(50).IsRequired();
            e.Property(x => x.UnitPrice).HasColumnType("decimal(18,2)");
            e.Property(x => x.LineTotal).HasColumnType("decimal(18,2)");
            e.HasOne(x => x.OrderHeader).WithMany(x => x.Details).HasForeignKey(x => x.OrderHeaderId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Product).WithMany(x => x.OrderDetails).HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);
        });
    }
}
