using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace HorariosIPBejaMVC.Models;

[Table("CURSO")]
[Index("nome", Name = "UQ__CURSO__6F71C0DCB838515A", IsUnique = true)]
public partial class CURSO
{
    [Key]
    public int id { get; set; }

    [StringLength(100)]
    [Unicode(false)]
    public string nome { get; set; } = null!;

    public int tipo_curso_id { get; set; }

    public int escola_id { get; set; }

    [InverseProperty("curso")]
    public virtual ICollection<ALUNO> ALUNOs { get; set; } = new List<ALUNO>();

    [InverseProperty("curso")]
    public virtual ICollection<COORDENADOR_CURSO> COORDENADOR_CURSOs { get; set; } = new List<COORDENADOR_CURSO>();

    [InverseProperty("curso")]
    public virtual ICollection<UNIDADE_CURRICULAR> UNIDADE_CURRICULARs { get; set; } = new List<UNIDADE_CURRICULAR>();

    [ForeignKey("escola_id")]
    [InverseProperty("CURSOs")]
    public virtual ESCOLA escola { get; set; } = null!;

    [ForeignKey("tipo_curso_id")]
    [InverseProperty("CURSOs")]
    public virtual TIPO_CURSO tipo_curso { get; set; } = null!;
}
