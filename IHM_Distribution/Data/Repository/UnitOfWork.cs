using IHM_Distribution.Models;
using Microsoft.EntityFrameworkCore.Storage;

namespace IHM_Distribution.Data.Repository
{
	public class UnitOfWork : IUnitOfWork
	{
		private readonly AppDbContext _context;
        private bool _disposed = false;


        public UnitOfWork(AppDbContext context)
		{
			_context = context;
		}

		// Lazy-loading of repositories for all entities

		public IRepository<Agent> Agents => _agents ??= new Repository<Agent>(_context);
		private IRepository<Agent>? _agents;

		public IRepository<Client> Clients => _clients ??= new Repository<Client>(_context);
		private IRepository<Client>? _clients;

		public IRepository<Product> Products => _products ??= new Repository<Product>(_context);
		private IRepository<Product>? _products;

        public IRepository<ProductImage> ProductImage=> _productImage ??= new Repository<ProductImage>(_context);
        private IRepository<ProductImage>? _productImage;

        public IRepository<Receipt> Receipts => _receipts ??= new Repository<Receipt>(_context);
		private IRepository<Receipt>? _receipts;

		public IRepository<ReceiptDetail> ReceiptDetails => _receiptDetails ??= new Repository<ReceiptDetail>(_context);
		private IRepository<ReceiptDetail>? _receiptDetails;

		// New Repositories
		public IRepository<DailyTrip> DailyTrips => _dailyTrips ??= new Repository<DailyTrip>(_context);
		private IRepository<DailyTrip>? _dailyTrips;

		public IRepository<LoadedItem> LoadedItems => _loadedItems ??= new Repository<LoadedItem>(_context);
		private IRepository<LoadedItem>? _loadedItems;

		public IRepository<ReturnedItem> ReturnedItems => _returnedItems ??= new Repository<ReturnedItem>(_context);
		private IRepository<ReturnedItem>? _returnedItems;

		public IRepository<CurrentCarStock> CurrentCarStock => _currentCarStock ??= new Repository<CurrentCarStock>(_context);

        private IRepository<CurrentCarStock>? _currentCarStock;

		public async Task<bool> CompleteAsync() => await _context.SaveChangesAsync() > 0;
		public bool HasChanges() => _context.ChangeTracker.HasChanges();

        public async Task<IDbContextTransaction> BeginTransactionAsync()
        {
            return await _context.Database.BeginTransactionAsync();
        }

        // Dispose pattern implementation
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _context?.Dispose();
                }
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
