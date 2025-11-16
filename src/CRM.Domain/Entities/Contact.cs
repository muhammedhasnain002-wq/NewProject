namespace CRM.Domain.Entities;

public class Contact
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid CustomerId { get; private set; }
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public string Email { get; private set; }
    public string Phone { get; private set; }
    public bool IsPrimary { get; private set; }

    public Customer Customer { get; private set; } = null!;

    public Contact(Guid customerId, string firstName, string lastName, string email, string phone, bool isPrimary)
    {
        CustomerId = customerId;
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        Phone = phone;
        IsPrimary = isPrimary;
    }
}
