using System;
using System.Collections.Generic;

namespace WPF.Models;

public partial class Person
{
    public int PersonId { get; set; }

    public string PersonName { get; set; } = null!;

    public string Password { get; set; } = null!;

    public DateTime Birthdate { get; set; }

    public string? Address { get; set; }

    public string? Phone { get; set; }

    public bool Status { get; set; }

    public int RoleId { get; set; }

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();

    public virtual Role Role { get; set; } = null!;
}
