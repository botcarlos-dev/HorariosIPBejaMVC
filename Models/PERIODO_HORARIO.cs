using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace HorariosIPBejaMVC.Models;

[Table("PERIODO_HORARIO")]
public partial class PERIODO_HORARIO
{
    [Key]
    public int id { get; set; }

    [StringLength(10)]
    [Unicode(false)]
    public string dia_semana { get; set; } = null!;

    public TimeOnly hora_inicio { get; set; }

    public TimeOnly hora_fim { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string descricao { get; set; } = null!;

    [InverseProperty("periodo_horario")]
    public virtual ICollection<HORARIO_REFERENCIAL> HORARIO_REFERENCIALs { get; set; } = new List<HORARIO_REFERENCIAL>();

    [InverseProperty("periodo_horario")]
    public virtual ICollection<HORARIO_SEMANAL> HORARIO_SEMANALs { get; set; } = new List<HORARIO_SEMANAL>();

    [ForeignKey("periodo_horario_id")]
    [InverseProperty("periodo_horarios")]
    public virtual ICollection<DOCENTE> docentes { get; set; } = new List<DOCENTE>();

    [ForeignKey("periodo_horario_id")]
    [InverseProperty("periodo_horarios")]
    public virtual ICollection<SALA> salas { get; set; } = new List<SALA>();
}
