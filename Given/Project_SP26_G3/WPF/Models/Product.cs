using System;
using System.Collections.Generic;

namespace WPF.Models;

public partial class Product
{
    public int ProductId { get; set; }

    public string ProductCode { get; set; } = null!;

    public string? ProductName { get; set; }

    public int? Quantity { get; set; }

    public string? Sender { get; set; }

    public string? Location { get; set; }

    public int CatId { get; set; }

    public DateTime DateIn { get; set; }

    public DateTime? DateOut { get; set; }

    public bool Status { get; set; }

    public virtual Category Cat { get; set; } = null!;

    public virtual Warehouse? LocationNavigation { get; set; }

    public virtual Person? SenderNavigation { get; set; }
}
