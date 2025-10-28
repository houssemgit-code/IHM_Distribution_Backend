namespace IHM_Distribution.Data.Repository
{
    public interface IContext : IDisposable
    {
        /// <summary>
        /// Sauvegarde les changements courants du context.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns> true. </returns>
        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    }
}