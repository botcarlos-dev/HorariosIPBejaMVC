using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace HorariosIPBejaMVC.Models;

[Table("UNIDADE_CURRICULAR")]
[Index("nome", Name = "UQ__UNIDADE___6F71C0DCEDA3E211", IsUnique = true)]
public partial class UNIDADE_CURRICULAR
{
    [Key]
    public int id { get; set; }

    [StringLength(100)]
    [Unicode(false)]
    public string nome { get; set; } = null!;

    public int curso_id { get; set; }

    public int semestre { get; set; }

    public int carga_horaria_teorica { get; set; }

    public int carga_horaria_pratica { get; set; }

    public int numero_turmas_teoricas { get; set; }

    public int numero_turmas_praticas { get; set; }

    [InverseProperty("uc")]
    public virtual ICollection<HORARIO_REFERENCIAL> HORARIO_REFERENCIALs { get; set; } = new List<HORARIO_REFERENCIAL>();

    [InverseProperty("uc")]
    public virtual ICollection<HORARIO_SEMANAL> HORARIO_SEMANALs { get; set; } = new List<HORARIO_SEMANAL>();

    [InverseProperty("unidade_curricular")]
    public virtual ICollection<TURMA> TURMAs { get; set; } = new List<TURMA>();

    [InverseProperty("uc")]
    public virtual ICollection<UC_DOCENTE> UC_DOCENTEs { get; set; } = new List<UC_DOCENTE>();

    [ForeignKey("curso_id")]
    [InverseProperty("UNIDADE_CURRICULARs")]
    public virtual CURSO curso { get; set; } = null!;

    [ForeignKey("uc_id")]
    [InverseProperty("ucs")]
    public virtual ICollection<ALUNO> alunos { get; set; } = new List<ALUNO>();

    [ForeignKey("unidade_curricular_id")]
    [InverseProperty("unidade_curriculars")]
    public virtual ICollection<SALA> salas { get; set; } = new List<SALA>();

    public virtual ICollection<ALUNO_UC> ALUNO_UCs { get; set; } = new List<ALUNO_UC>();

}
