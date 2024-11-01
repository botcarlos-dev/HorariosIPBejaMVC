using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace HorariosIPBejaMVC.Models;

[Table("ANO_LETIVO")]
[Index("descricao", Name = "UQ__ANO_LETI__91D38C2826011623", IsUnique = true)]
public partial class ANO_LETIVO
{
    [Key]
    public int id { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string descricao { get; set; } = null!;

    public DateOnly data_inicio { get; set; }

    public DateOnly data_fim { get; set; }

    public bool ativo { get; set; }

    [InverseProperty("ano_letivo")]
    public virtual ICollection<CALENDARIO_ACADEMICO> CALENDARIO_ACADEMICOs { get; set; } = new List<CALENDARIO_ACADEMICO>();

    [InverseProperty("ano_letivo")]
    public virtual ICollection<HORARIO_REFERENCIAL> HORARIO_REFERENCIALs { get; set; } = new List<HORARIO_REFERENCIAL>();

    [InverseProperty("ano_letivo")]
    public virtual ICollection<HORARIO_SEMANAL> HORARIO_SEMANALs { get; set; } = new List<HORARIO_SEMANAL>();

    [InverseProperty("ano_letivo")]
    public virtual ICollection<SEMANA> SEMANAs { get; set; } = new List<SEMANA>();
}
