using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace WPF.Models;

public partial class Category
{
    public int CatId { get; set; }

    public string? CatName { get; set; }
    [JsonIgnore]
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
