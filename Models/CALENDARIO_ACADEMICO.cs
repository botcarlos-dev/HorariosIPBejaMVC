using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace HorariosIPBejaMVC.Models;

[Table("CALENDARIO_ACADEMICO")]
[Index("data", Name = "UQ__CALENDAR__D9DE21E198D0C5EF", IsUnique = true)]
public partial class CALENDARIO_ACADEMICO
{
    [Key]
    public int id { get; set; }

    public DateOnly data { get; set; }

    [StringLength(100)]
    [Unicode(false)]
    public string descricao { get; set; } = null!;

    [StringLength(50)]
    [Unicode(false)]
    public string tipo_evento { get; set; } = null!;

    public int ano_letivo_id { get; set; }

    public int? semana_id { get; set; }

    [ForeignKey("ano_letivo_id")]
    [InverseProperty("CALENDARIO_ACADEMICOs")]
    public virtual ANO_LETIVO ano_letivo { get; set; } = null!;

    [ForeignKey("semana_id")]
    [InverseProperty("CALENDARIO_ACADEMICOs")]
    public virtual SEMANA? semana { get; set; }
}
