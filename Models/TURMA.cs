using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace HorariosIPBejaMVC.Models;

[Table("TURMA")]
public partial class TURMA
{
    [Key]
    public int id { get; set; }

    public int unidade_curricular_id { get; set; }

    public int docente_id { get; set; }

    public int tipo_aula_id { get; set; }

    public int duracao { get; set; }

    [StringLength(10)]
    [Unicode(false)]
    public string turma_label { get; set; } = null!;

    [InverseProperty("turma")]
    public virtual ICollection<HORARIO_REFERENCIAL> HORARIO_REFERENCIALs { get; set; } = new List<HORARIO_REFERENCIAL>();

    [InverseProperty("turma")]
    public virtual ICollection<HORARIO_SEMANAL> HORARIO_SEMANALs { get; set; } = new List<HORARIO_SEMANAL>();

    [ForeignKey("docente_id")]
    [InverseProperty("TURMAs")]
    public virtual DOCENTE docente { get; set; } = null!;

    [ForeignKey("tipo_aula_id")]
    [InverseProperty("TURMAs")]
    public virtual TIPO_AULA tipo_aula { get; set; } = null!;

    [ForeignKey("unidade_curricular_id")]
    [InverseProperty("TURMAs")]
    public virtual UNIDADE_CURRICULAR unidade_curricular { get; set; } = null!;
}
