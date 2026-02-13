using Payment.Infrastructure.IntegrationEvents;

namespace Payment.Infrastructure.EntityConfigurations;

internal class IntegrationEventLogConfiguration : IEntityTypeConfiguration<IntegrationEventLogEntry>
{
    public void Configure(EntityTypeBuilder<IntegrationEventLogEntry> builder)
    {
        builder.ToTable("integration_event_logs");

        builder.HasKey(e => e.EventId);

        builder.Property(e => e.EventId)
            .HasColumnName("event_id")
            .IsRequired();

        builder.Property(e => e.EventTypeName)
            .HasColumnName("event_type_name")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(e => e.Content)
            .HasColumnName("content")
            .IsRequired();

        builder.Property(e => e.CreationTime)
            .HasColumnName("creation_time")
            .IsRequired();

        builder.Property(e => e.State)
            .HasColumnName("state")
            .IsRequired();

        builder.Property(e => e.TimesSent)
            .HasColumnName("times_sent")
            .IsRequired();

        builder.Property(e => e.TransactionId)
            .HasColumnName("transaction_id")
            .IsRequired();

        builder.HasIndex(e => e.TransactionId);
        builder.HasIndex(e => e.State);
    }
}
