using cursovaia2.Models;
using cursovaia2.ModelsDb;
using Microsoft.EntityFrameworkCore;

namespace cursovaia2.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // Models (старые)
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }

        // Authentication & Users
        public DbSet<RoleDb> Roles { get; set; }
        public DbSet<UserDb> Users { get; set; }
        public DbSet<CustomerDb> Customers { get; set; }

        // Products
        public DbSet<CategoryDb> CategoriesDb { get; set; }
        public DbSet<BrandDb> Brands { get; set; }
        public DbSet<ProductDb> ProductsDb { get; set; }
        public DbSet<ProductImageDb> ProductImages { get; set; }
        public DbSet<ProductAttributeDb> ProductAttributes { get; set; }

        // Orders
        public DbSet<OrderDb> Orders { get; set; }
        public DbSet<OrderItemDb> OrderItems { get; set; }

        // Payments
        public DbSet<PaymentMethodDb> PaymentMethods { get; set; }
        public DbSet<PaymentDb> Payments { get; set; }

        // Deliveries
        public DbSet<DeliveryMethodDb> DeliveryMethods { get; set; }
        public DbSet<DeliveryDb> Deliveries { get; set; }

        // Discounts & Promo
        public DbSet<DiscountDb> Discounts { get; set; }
        public DbSet<PromoCodeDb> PromoCodes { get; set; }
        public DbSet<ProductDiscountDb> ProductDiscounts { get; set; }

        // Cart
        public DbSet<CartDb> Carts { get; set; }
        public DbSet<CartItemDb> CartItems { get; set; }

        // Reviews & Wishlist
        public DbSet<ReviewDb> Reviews { get; set; }
        public DbSet<WishlistDb> Wishlists { get; set; }

        // Order status history
        public DbSet<OrderStatusHistoryDb> OrderStatusHistories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Unique constraints
            modelBuilder.Entity<UserDb>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<ProductDb>()
                .HasIndex(p => p.Sku)
                .IsUnique();

            modelBuilder.Entity<BrandDb>()
                .HasIndex(b => b.Name)
                .IsUnique();

            modelBuilder.Entity<RoleDb>()
                .HasIndex(r => r.Name)
                .IsUnique();

            modelBuilder.Entity<CartItemDb>()
                .HasIndex(ci => new { ci.CartId, ci.ProductId })
                .IsUnique();

            modelBuilder.Entity<ReviewDb>()
                .HasIndex(r => new { r.ProductId, r.CustomerId })
                .IsUnique();

            // Check constraints (seeded through SQL migrations if needed)
            modelBuilder.Entity<OrderItemDb>()
                .HasCheckConstraint("chk_price_positive", "\"price\" >= 0");

            modelBuilder.Entity<PaymentDb>()
                .HasCheckConstraint("chk_payment_amount", "\"amount\" >= 0");

            modelBuilder.Entity<OrderDb>()
                .HasCheckConstraint("chk_order_status", 
                    "\"status\" IN ('pending', 'paid', 'shipped', 'completed', 'cancelled')");

            modelBuilder.Entity<PaymentDb>()
                .HasCheckConstraint("chk_payment_status",
                    "\"status\" IN ('pending', 'paid', 'failed')");

            modelBuilder.Entity<DeliveryDb>()
                .HasCheckConstraint("chk_delivery_status",
                    "\"status\" IN ('pending', 'in_progress', 'delivered')");

            // Configure cascade delete
            modelBuilder.Entity<UserDb>()
                .HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<CustomerDb>()
                .HasOne(c => c.User)
                .WithOne(u => u.Customer)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ProductDb>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<ProductDb>()
                .HasOne(p => p.Brand)
                .WithMany(b => b.Products)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<ProductImageDb>()
                .HasOne(pi => pi.Product)
                .WithMany(p => p.Images)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ProductAttributeDb>()
                .HasOne(pa => pa.Product)
                .WithMany(p => p.Attributes)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OrderDb>()
                .HasOne(o => o.Customer)
                .WithMany(c => c.Orders)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OrderItemDb>()
                .HasOne(oi => oi.Order)
                .WithMany(o => o.Items)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PaymentDb>()
                .HasOne(p => p.Order)
                .WithMany(o => o.Payments)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DeliveryDb>()
                .HasOne(d => d.Order)
                .WithMany(o => o.Deliveries)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CartDb>()
                .HasOne(c => c.Customer)
                .WithMany()
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CartItemDb>()
                .HasOne(ci => ci.Cart)
                .WithMany(c => c.Items)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OrderStatusHistoryDb>()
                .HasOne(h => h.Order)
                .WithMany()
                .HasForeignKey(h => h.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OrderStatusHistoryDb>()
                .HasOne(h => h.ChangedByUser)
                .WithMany()
                .HasForeignKey(h => h.ChangedByUserId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
