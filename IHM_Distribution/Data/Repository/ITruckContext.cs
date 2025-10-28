using IHM_Distribution.Models;
using Microsoft.EntityFrameworkCore;

namespace IHM_Distribution.Data.Repository
{
    public interface ITruckContext : IContext
    {
        public DbSet<Agent> Agents
        {
            get; set;
        }

        public DbSet<AuditLog> AuditLog
        {
            get; set;
        }

        public DbSet<Client> Clients
        {
            get; set;
        }

        public DbSet<CurrentCarStock> CurrentCarStocks
        {
            get; set;
        }

        public DbSet<DailyTrip> DailyTrips
        {
            get; set;
        }

        public DbSet<LoadedItem> LoadedItems
        {
            get; set;
        }

        public DbSet<Product> Products
        {
            get; set;
        }

        public DbSet<ProductImage> ProductImages
        {
            get; set;
        }

        public DbSet<Receipt> Receipts
        {
            get; set;
        }

        public DbSet<ReceiptDetail> ReceiptDetails
        {
            get; set;
        }

        public DbSet<ReturnedItem> ReturnedItems
        {
            get; set;
        }
    }
}