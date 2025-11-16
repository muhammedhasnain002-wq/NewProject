using CRM.Domain.Entities;
using CRM.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.Data.Configurations;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Email).IsRequired().HasMaxLength(320);
        builder.HasIndex(x => x.Email).IsUnique();
        builder.OwnsOne(x => x.Address, a =>
        {
            a.Property(p => p.Line1).HasMaxLength(200).IsRequired();
            a.Property(p => p.Line2).HasMaxLength(200);
            a.Property(p => p.City).HasMaxLength(100).IsRequired();
            a.Property(p => p.State).HasMaxLength(100).IsRequired();
            a.Property(p => p.PostalCode).HasMaxLength(20).IsRequired();
            a.Property(p => p.Country).HasMaxLength(100).IsRequired();
        });
        builder.HasMany(x => x.Contacts).WithOne(x => x.Customer).HasForeignKey(x => x.CustomerId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(x => x.Opportunities).WithOne(x => x.Customer).HasForeignKey(x => x.CustomerId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(x => x.Orders).WithOne(x => x.Customer).HasForeignKey(x => x.CustomerId).OnDelete(DeleteBehavior.Restrict);
    }
}
