using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace HorariosIPBejaMVC.Models;

[Table("SALA")]
[Index("nome", Name = "UQ__SALA__6F71C0DC9E272544", IsUnique = true)]
public partial class SALA
{
    [Key]
    public int id { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string nome { get; set; } = null!;

    public int capacidade { get; set; }

    [StringLength(20)]
    [Unicode(false)]
    public string tipo { get; set; } = null!;

    public int escola_id { get; set; }

    [StringLength(100)]
    [Unicode(false)]
    public string? localizacao { get; set; }

    [StringLength(255)]
    [Unicode(false)]
    public string? recursos_adicionais { get; set; }

    [InverseProperty("sala")]
    public virtual ICollection<HORARIO_REFERENCIAL> HORARIO_REFERENCIALs { get; set; } = new List<HORARIO_REFERENCIAL>();

    [InverseProperty("sala")]
    public virtual ICollection<HORARIO_SEMANAL> HORARIO_SEMANALs { get; set; } = new List<HORARIO_SEMANAL>();

    [ForeignKey("escola_id")]
    [InverseProperty("SALAs")]
    public virtual ESCOLA escola { get; set; } = null!;

    [ForeignKey("sala_id")]
    [InverseProperty("salas")]
    public virtual ICollection<PERIODO_HORARIO> periodo_horarios { get; set; } = new List<PERIODO_HORARIO>();

    [ForeignKey("sala_id")]
    [InverseProperty("salas")]
    public virtual ICollection<UNIDADE_CURRICULAR> unidade_curriculars { get; set; } = new List<UNIDADE_CURRICULAR>();
}
