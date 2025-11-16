using CRM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.Data.Configurations;

public class OrderLineConfiguration : IEntityTypeConfiguration<OrderLine>
{
    public void Configure(EntityTypeBuilder<OrderLine> builder)
    {
        builder.ToTable("OrderLines");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.UnitPrice).HasPrecision(18,2);
        builder.Property(x => x.LineTotal).HasPrecision(18,2);
        builder.HasIndex(x => new { x.OrderId, x.ProductId }).IsUnique();
        builder.HasOne(x => x.Product).WithMany(x => x.OrderLines).HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);
    }
}
