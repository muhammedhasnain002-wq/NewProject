using CRM.Domain.Enums;
using CRM.Domain.ValueObjects;

namespace CRM.Domain.Entities;

public class Customer
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Name { get; private set; }
    public string Email { get; private set; }
    public string Phone { get; private set; }
    public CustomerStatus Status { get; private set; }
    public Address Address { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    public ICollection<Contact> Contacts { get; private set; } = new List<Contact>();
    public ICollection<Opportunity> Opportunities { get; private set; } = new List<Opportunity>();
    public ICollection<Order> Orders { get; private set; } = new List<Order>();

    public Customer(string name, string email, string phone, CustomerStatus status, Address address)
    {
        Name = name;
        Email = email;
        Phone = phone;
        Status = status;
        Address = address;
    }

    public void Update(string name, string phone, CustomerStatus status, Address address)
    {
        Name = name;
        Phone = phone;
        Status = status;
        Address = address;
    }
}
