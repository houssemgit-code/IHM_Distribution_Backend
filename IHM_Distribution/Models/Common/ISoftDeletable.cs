namespace IHM_Distribution.Models.Common
{
    using System;

    /// <summary>
    /// Interface for entities that support soft deletion.
    /// </summary>
    public interface ISoftDeletable
    {
        /// <summary>
        /// Gets or sets a value indicating whether this entity is deleted.
        /// </summary>
        /// <value>
        ///   <c>true</c> if is deleted; otherwise, <c>false</c>.
        /// </value>
        bool IsDeleted { get; set; }

        /// <summary>
        /// Gets or sets the deleted date.
        /// </summary>
        /// <value>
        /// The deleted date.
        /// </value>
        DateTimeOffset? DeletedDate { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the user who deleted the entity.
        /// </summary>
        public Guid? DeletedById { get; set; }

        /// <summary>
        /// Gets or sets the name of the user who deleted the entity.
        /// </summary>
        public string? DeletedBy { get; set; }
    }
}
