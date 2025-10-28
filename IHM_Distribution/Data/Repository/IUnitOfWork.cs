using IHM_Distribution.Models;
using Microsoft.EntityFrameworkCore.Storage;

namespace IHM_Distribution.Data.Repository
{
	public interface IUnitOfWork : IDisposable
    {
		// Core Entities
		IRepository<Agent> Agents { get; }
		IRepository<Client> Clients { get; }
		IRepository<Product> Products { get; }
		IRepository<Receipt> Receipts { get; }
		IRepository<ReceiptDetail> ReceiptDetails { get; }

		// New Entities for Daily Workflow & Live Tracking
		IRepository<DailyTrip> DailyTrips { get; }
		IRepository<LoadedItem> LoadedItems { get; }
		IRepository<ReturnedItem> ReturnedItems { get; }
		IRepository<CurrentCarStock> CurrentCarStock { get; }
		IRepository<ProductImage> ProductImage { get; }

		// Save and check changes
		Task<bool> CompleteAsync();
		bool HasChanges();

        Task<IDbContextTransaction> BeginTransactionAsync();

    }
}
