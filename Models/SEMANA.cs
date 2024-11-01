using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace HorariosIPBejaMVC.Models;

[Table("SEMANAS")]
public partial class SEMANA
{
    [Key]
    public int id { get; set; }

    public int numero_semana { get; set; }

    public DateOnly data_inicio { get; set; }

    public DateOnly data_fim { get; set; }

    public int ano_letivo_id { get; set; }

    [InverseProperty("semana")]
    public virtual ICollection<CALENDARIO_ACADEMICO> CALENDARIO_ACADEMICOs { get; set; } = new List<CALENDARIO_ACADEMICO>();

    [InverseProperty("semana")]
    public virtual ICollection<HORARIO_SEMANAL> HORARIO_SEMANALs { get; set; } = new List<HORARIO_SEMANAL>();

    [ForeignKey("ano_letivo_id")]
    [InverseProperty("SEMANAs")]
    public virtual ANO_LETIVO ano_letivo { get; set; } = null!;
}
