using IHM_Distribution.Data.Repository;
using IHM_Distribution.Models.Common;
using IHM_Distribution.Models;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using IHM_Distribution.Services;

namespace IHM_Distribution.Data
{
    public class DbContextBase : DbContext
    {
        private static readonly List<string> NonChangeTrackableProperties = new() { "Id", "CreatedDate", "CreateBy", "CreatedById", "ModifiedDate", "ModifiedBy", "ModifiedById", "DeletedDate", "DeletedBy", "DeletedById" };
        private readonly IIdentityService identityService;
        private Guid? id;

        public DbContextBase(DbContextOptions options, IIdentityService identityService)
           : base(options)
        {
            // Default settings
            this.ChangeTracker.LazyLoadingEnabled = false;
            this.identityService = identityService;
        }

        /// <summary>
        /// Gets the context identifier.
        /// </summary>
        /// <value>
        /// The context identifier.
        /// </value>
        public Guid? Id
        {
            get
            {
                return this.id ?? (this.id = Guid.NewGuid());
            }
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
        public DbSet<CurrentCarStock> CurrentCarStocks { get; set; }

        public DbSet<ProductImage> ProductImages { get; set; }

        public DbSet<AuditLog> AuditLog { get; set; }

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

            modelBuilder.Entity<DailyTrip>()
                .Property(t => t.Date)
                .HasConversion(
                    v => v.ToUniversalTime(),
                    v => DateTime.SpecifyKind(v, DateTimeKind.Utc)
                );


            // You can add other specific configurations here if needed
            // (e.g., unique constraints, indexes for performance)
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DbContextBase"/> class.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="loggerService">The logger service.</param>
        /// <param name="userService">The user service.</param>
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            int result;
            this.OnValidate();
            this.OnBeforeSaveChanges();
            result = await base.SaveChangesAsync(cancellationToken);
            this.OnAfterSaveChanges();

            return result;
        }

        /// <summary>
        /// Befores the SaveChanges.
        /// </summary>
        protected virtual void OnBeforeSaveChanges()
        {
            this.UseAuditable();
            this.UseSoftDelete();
        }

        /// <summary>
        /// After the SaveChanges.
        /// </summary>
        protected virtual void OnAfterSaveChanges()
        {
        }

        /// <summary>
        /// Validation before the SaveChanges
        /// </summary>
        protected virtual void OnValidate()
        {
            var entities = from e in this.ChangeTracker.Entries()
                           where e.State == EntityState.Added || e.State == EntityState.Modified
                           select e.Entity;

            foreach (var entity in entities)
            {
                var validationContext = new ValidationContext(entity);
                Validator.ValidateObject(entity, validationContext, validateAllProperties: true);
            }
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.EnableSensitiveDataLogging(true); // To show sql queries parameters values
        }

        /// <summary>
        /// Uses the auditable behaviour.
        /// </summary>
        protected virtual void UseAuditable()
        {
            // Change Created date & Modified date
            foreach (var entry in this.ChangeTracker.Entries<IAuditable>())
            {
                if (entry.Entity is IAuditable entity)
                {
                    if (entry.State == EntityState.Added)
                    {
                        entity.CreatedDate = DateTimeOffset.Now;
                        entity.CreatedBy = this.identityService.GetCurrentUserEmail();
                        entity.CreatedById = this.identityService.GetCurrentUserName();
                    }
                    else if (entry.State == EntityState.Modified)
                    {
                        entity.ModifiedBy = this.identityService.GetCurrentUserEmail();
                        entity.ModifiedDate = DateTimeOffset.Now;
                    }
                }
            }

            var auditLogs = new List<AuditLog>();

            foreach (var entry in this.ChangeTracker.Entries<IChangeTrackable>())
            {
                if (entry.Entity is IChangeTrackable)
                {
                    var mainLog = new AuditLog();
                    mainLog.EntityName = entry.Entity.GetType().Name;
                    mainLog.EntityId = Guid.Parse(entry.Property("Id").CurrentValue.ToString());
                    mainLog.UserEmail = this.identityService.GetCurrentUserEmail();
                    mainLog.Action = entry.State.ToString();
                    mainLog.Timestamp = DateTime.UtcNow;
                    mainLog.IPAddress = this.identityService.IPAddress;

                    if (entry.State == EntityState.Added)
                    {
                        auditLogs.Add(mainLog);
                    }
                    else if (entry.State == EntityState.Modified)
                    {
                        foreach (var property in entry.OriginalValues.Properties.Where(x => !NonChangeTrackableProperties.Contains(x.Name)))
                        {
                            var originalValue = entry.OriginalValues[property];
                            var currentValue = entry.CurrentValues[property];

                            if (!Equals(originalValue, currentValue))
                            {
                                var log = new AuditLog();
                                log.EntityName = mainLog.EntityName;
                                log.EntityId = mainLog.EntityId;
                                log.UserEmail = mainLog.UserEmail;
                                log.Action = mainLog.Action;
                                log.Timestamp = mainLog.Timestamp;
                                log.UserEmail = mainLog.UserEmail;
                                log.ColumnName = property.Name;
                                log.OldValue = originalValue?.ToString();
                                log.NewValue = currentValue?.ToString();
                                log.IPAddress = mainLog.IPAddress;

                                auditLogs.Add(log);
                            }
                        }
                    }
                }
            }

            this.Set<AuditLog>().AddRange(auditLogs);
            this.SaveChanges();
        }

        /// <summary>
        /// Uses the soft delete behaviour.
        /// </summary>
        protected virtual void UseSoftDelete()
        {
            foreach (var entry in this.ChangeTracker.Entries<ISoftDeletable>())
            {
                if (entry.Entity is ISoftDeletable softDelete && entry.State == EntityState.Deleted)
                {
                    softDelete.IsDeleted = true;
                    softDelete.DeletedDate = DateTimeOffset.UtcNow;
                    entry.State = EntityState.Modified;
                }
            }
        }
    }
}