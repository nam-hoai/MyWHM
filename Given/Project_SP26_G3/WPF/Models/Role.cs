using System;
using System.Collections.Generic;

namespace WPF.Models;

public partial class Role
{
    public int RoleId { get; set; }

    public string? RoleName { get; set; }

    public virtual ICollection<Person> People { get; set; } = new List<Person>();
}
