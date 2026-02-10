namespace Payment.Infrastructure.EntityConfigurations;

public class PaymentItemConfiguration : IEntityTypeConfiguration<PaymentItem>
{
    public void Configure(EntityTypeBuilder<PaymentItem> builder)
    {
        builder.ToTable("payment_items");

        builder.HasKey(pi => pi.Id);

        builder.Property(pi => pi.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property<PaymentId>("PaymentId")
            .HasColumnName("payment_id")
            .HasConversion(
                id => id.Value,
                value => PaymentId.From(value))
            .IsRequired();

        builder.Property(pi => pi.ProductId)
            .HasColumnName("product_id")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(pi => pi.ProductName)
            .HasColumnName("product_name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(pi => pi.Quantity)
            .HasColumnName("quantity")
            .IsRequired();

        builder.OwnsOne(pi => pi.UnitPrice, money =>
        {
            money.Property(m => m.Value)
                .HasColumnName("unit_price")
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            money.Property(m => m.Currency)
                .HasColumnName("unit_price_currency")
                .HasMaxLength(3)
                .HasDefaultValue("BRL")
                .IsRequired();
        });

        builder.OwnsOne(pi => pi.Amount, money =>
        {
            money.Property(m => m.Value)
                .HasColumnName("amount")
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            money.Property(m => m.Currency)
                .HasColumnName("amount_currency")
                .HasMaxLength(3)
                .HasDefaultValue("BRL")
                .IsRequired();
        });

        // Indexes
        builder.HasIndex("PaymentId")
            .HasDatabaseName("ix_payment_items_payment_id");

        builder.HasIndex(pi => pi.ProductId)
            .HasDatabaseName("ix_payment_items_product_id");
    }
}
