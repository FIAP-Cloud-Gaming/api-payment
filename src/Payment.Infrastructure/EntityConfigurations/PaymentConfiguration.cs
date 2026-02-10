namespace Payment.Infrastructure.EntityConfigurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Domain.AggregatesModel.PaymentAggregate.Payment>
{
    public void Configure(EntityTypeBuilder<Domain.AggregatesModel.PaymentAggregate.Payment> builder)
    {
        builder.ToTable("payments");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .HasColumnName("id")
            .HasConversion(
                id => id.Value,
                value => PaymentId.From(value))
            .IsRequired();

        builder.Property(p => p.OrderId)
            .HasColumnName("order_id")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(p => p.UserId)
            .HasColumnName("user_id")
            .HasMaxLength(100)
            .IsRequired();

        builder.OwnsOne(p => p.TotalAmount, money =>
        {
            money.Property(m => m.Value)
                .HasColumnName("total_amount")
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            money.Property(m => m.Currency)
                .HasColumnName("currency")
                .HasMaxLength(3)
                .HasDefaultValue("BRL")
                .IsRequired();
        });

        builder.Property(p => p.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(p => p.Method)
            .HasColumnName("payment_method")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.OwnsOne(p => p.Details, details =>
        {
            details.Property(d => d.TransactionId)
                .HasColumnName("transaction_id")
                .HasMaxLength(100);

            details.Property(d => d.AuthorizationCode)
                .HasColumnName("authorization_code")
                .HasMaxLength(100);

            details.Property(d => d.FailureReason)
                .HasColumnName("failure_reason")
                .HasMaxLength(500);

            details.Property(d => d.ProcessorResponse)
                .HasColumnName("processor_response")
                .HasMaxLength(1000);

            details.Property(d => d.ProcessedAt)
                .HasColumnName("details_processed_at");
        });

        builder.OwnsOne(p => p.CardInfo, card =>
        {
            card.Property(c => c.CardHolderName)
                .HasColumnName("card_holder_name")
                .HasMaxLength(200);

            card.Property(c => c.MaskedCardNumber)
                .HasColumnName("masked_card_number")
                .HasMaxLength(20);

            card.Property(c => c.CardBrand)
                .HasColumnName("card_brand")
                .HasMaxLength(50);

            card.Property(c => c.ExpirationMonth)
                .HasColumnName("card_expiration_month");

            card.Property(c => c.ExpirationYear)
                .HasColumnName("card_expiration_year");
        });

        builder.Property(p => p.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(p => p.ProcessedAt)
            .HasColumnName("processed_at");

        builder.Property(p => p.UpdatedAt)
            .HasColumnName("updated_at");

        builder.HasMany(p => p.Items)
            .WithOne()
            .HasForeignKey("PaymentId")
            .HasPrincipalKey(p => p.Id)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(p => p.OrderId)
            .HasDatabaseName("ix_payments_order_id");

        builder.HasIndex(p => p.UserId)
            .HasDatabaseName("ix_payments_user_id");

        builder.HasIndex(p => p.Status)
            .HasDatabaseName("ix_payments_status");

        builder.HasIndex(p => p.CreatedAt)
            .HasDatabaseName("ix_payments_created_at");

        builder.Ignore(p => p.Notifications);
    }
}
