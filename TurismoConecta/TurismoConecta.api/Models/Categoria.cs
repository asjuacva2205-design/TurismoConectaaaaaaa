using System;
using System.Collections.Generic;

namespace TurismoConecta.api.Models;

public partial class Categoria
{
    public int IdCategoria { get; set; }
    public string Nombre { get; set; } = null!;
    public string? Descripcion { get; set; }
    public string? Icono { get; set; }
    public bool AplicaNegocio { get; set; }
    public bool AplicaSitio { get; set; }

    public virtual ICollection<Negocio> Negocios { get; set; } = new List<Negocio>();

    public virtual ICollection<SitioTuristico> SitiosTuristicos { get; set; } = new List<SitioTuristico>();
}
