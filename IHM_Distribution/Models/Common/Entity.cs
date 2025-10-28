namespace IHM_Distribution.Models.Common
{
    /// <summary>
    /// Base class for domain entities that provides auditing information.
    /// </summary>
    public abstract class Entity : IAuditable, ISoftDeletable
    {
        /// <summary>
        /// Gets or sets the unique identifier for the entity.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the entity was created.
        /// </summary>
        public DateTimeOffset CreatedDate { get; set; }

        /// <summary>
        /// Gets or sets the name of the user who created the entity.
        /// </summary>
        public string? CreatedBy { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the user who created the entity.
        /// </summary>
        public string? CreatedById { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the entity was last modified.
        /// </summary>
        public DateTimeOffset? ModifiedDate { get; set; }

        /// <summary>
        /// Gets or sets the name of the user who last modified the entity.
        /// </summary>
        public string? ModifiedBy { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the user who last modified the entity.
        /// </summary>
        public string? ModifiedById { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the entity is marked as deleted.
        /// </summary>
        public bool IsDeleted { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the entity was deleted, if applicable.
        /// </summary>
        public DateTimeOffset? DeletedDate { get; set; }

        public Guid? DeletedById { get; set; }

        public string? DeletedBy { get; set; }
    }
}
