namespace IHM_Distribution.Models
{
    public class AuditLog
    {
        public Guid Id { get; set; }

        public string UserEmail { get; set; }

        public string? IPAddress { get; set; }

        public string EntityName { get; set; }

        public Guid EntityId { get; set; }

        public string? ColumnName { get; set; }

        public string Action { get; set; }

        public DateTime Timestamp { get; set; }

        public string? OldValue { get; set; }

        public string? NewValue { get; set; }
    }
}
