// /Models/IDte.cs
using System.Collections.Generic;

namespace VisorDTE.Models;

public interface IDte
{
    Identificacion Identificacion { get; set; }
    Emisor Emisor { get; set; }
    Receptor Receptor { get; set; }
    List<CuerpoDocumento> CuerpoDocumento { get; set; }
    Resumen Resumen { get; set; }
    Extension Extension { get; set; }
    List<Apendice> Apendice { get; set; }
    List<ItemsAnulados>? ItemsAnulados { get; set; }
}