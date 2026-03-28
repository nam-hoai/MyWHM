using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace WPF.Models;

public partial class Role
{
    public int RoleId { get; set; }

    public string? RoleName { get; set; }
    [JsonIgnore]
    public virtual ICollection<Person> People { get; set; } = new List<Person>();
}
