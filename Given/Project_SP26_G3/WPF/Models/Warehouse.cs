using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace WPF.Models;

public partial class Warehouse
{
    public int WarehouseId { get; set; }

    public string WarehouseName { get; set; } = null!;

    public int? Size { get; set; }

    public bool Status { get; set; }
    [JsonIgnore]
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
