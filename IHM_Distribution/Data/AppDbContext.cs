using IHM_Distribution.Models;
using Microsoft.EntityFrameworkCore;

namespace IHM_Distribution.Data
{
	public class AppDbContext : DbContext
	{
		public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
		{
		}

		// DbSets for all your entities
		public DbSet<Agent> Agents { get; set; }
		public DbSet<Client> Clients { get; set; }
		public DbSet<Product> Products { get; set; }
		public DbSet<Receipt> Receipts { get; set; }
		public DbSet<ReceiptDetail> ReceiptDetails { get; set; }

		// New DbSets for the daily workflow and live tracking
		public DbSet<DailyTrip> DailyTrips { get; set; }
		public DbSet<LoadedItem> LoadedItems { get; set; }
		public DbSet<ReturnedItem> ReturnedItems { get; set; }
		public DbSet<CurrentCarStock> CurrentCarStock { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			// Configure precision for decimal properties globally (if needed)
			foreach (var property in modelBuilder.Model.GetEntityTypes()
				.SelectMany(t => t.GetProperties())
				.Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?)))
			{
				property.SetPrecision(18);
				property.SetScale(2);
			}

			// Configure the one-to-one relationship for CurrentCarStock
			// Product has one CurrentCarStock, CurrentCarStock has one Product
			modelBuilder.Entity<CurrentCarStock>()
				.HasOne(cs => cs.Product)
				.WithOne() // Assuming you don't add a navigation property back from Product to CurrentCarStock
				.HasForeignKey<CurrentCarStock>(cs => cs.ProductId)
				.OnDelete(DeleteBehavior.Cascade); // If a product is deleted, delete its car stock

			// Configure the relationship for Receipt -> DailyTrip
			modelBuilder.Entity<Receipt>()
				.HasOne(r => r.DailyTrip)
				.WithMany(dt => dt.Receipts)
				.HasForeignKey(r => r.DailyTripId)
				.OnDelete(DeleteBehavior.Restrict); // Prevent deleting a trip if receipts exist

			// Configure the relationship for LoadedItem -> DailyTrip
			modelBuilder.Entity<LoadedItem>()
				.HasOne(li => li.DailyTrip)
				.WithMany(dt => dt.LoadedItems)
				.HasForeignKey(li => li.DailyTripId)
				.OnDelete(DeleteBehavior.Cascade); // If a trip is deleted, delete its loaded items

			// Configure the relationship for ReturnedItem -> DailyTrip
			modelBuilder.Entity<ReturnedItem>()
				.HasOne(ri => ri.DailyTrip)
				.WithMany(dt => dt.ReturnedItems)
				.HasForeignKey(ri => ri.DailyTripId)
				.OnDelete(DeleteBehavior.Cascade); // If a trip is deleted, delete its returned items

			// You can add other specific configurations here if needed
			// (e.g., unique constraints, indexes for performance)
		}
	}
}
