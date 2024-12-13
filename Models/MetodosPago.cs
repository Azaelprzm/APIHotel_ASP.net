﻿using System;
using System.Collections.Generic;

namespace HotelAPI.Models;

public partial class MetodosPago
{
    public int Id { get; set; }

    public string Nombre { get; set; } = null!;

    public virtual ICollection<Pago> Pagos { get; set; } = new List<Pago>();
}
