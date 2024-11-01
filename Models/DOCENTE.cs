using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace HorariosIPBejaMVC.Models;

[Table("DOCENTE")]
[Index("numero", Name = "UQ__DOCENTE__FC77F211B5C9725B", IsUnique = true)]
public partial class DOCENTE
{
    [Key]
    public int id { get; set; }

    public int numero { get; set; }

    [StringLength(255)]
    public string? Nome { get; set; }

    [InverseProperty("docente")]
    public virtual COORDENADOR_CURSO? COORDENADOR_CURSO { get; set; }

    [InverseProperty("docente")]
    public virtual ICollection<HORARIO_REFERENCIAL> HORARIO_REFERENCIALs { get; set; } = new List<HORARIO_REFERENCIAL>();

    [InverseProperty("docente")]
    public virtual ICollection<HORARIO_SEMANAL> HORARIO_SEMANALs { get; set; } = new List<HORARIO_SEMANAL>();

    [InverseProperty("docente")]
    public virtual ICollection<TURMA> TURMAs { get; set; } = new List<TURMA>();

    [InverseProperty("docente")]
    public virtual ICollection<UC_DOCENTE> UC_DOCENTEs { get; set; } = new List<UC_DOCENTE>();

    [ForeignKey("id")]
    [InverseProperty("DOCENTE")]
    public virtual UTILIZADOR idNavigation { get; set; } = null!;

    [ForeignKey("docente_id")]
    [InverseProperty("docentes")]
    public virtual ICollection<PERIODO_HORARIO> periodo_horarios { get; set; } = new List<PERIODO_HORARIO>();
}
