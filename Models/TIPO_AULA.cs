using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace HorariosIPBejaMVC.Models;

[Table("TIPO_AULA")]
[Index("descricao", Name = "UQ__TIPO_AUL__91D38C28863316F7", IsUnique = true)]
public partial class TIPO_AULA
{
    [Key]
    public int id { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string descricao { get; set; } = null!;

    
    [InverseProperty("tipo_aula")]
    public virtual ICollection<TURMA> TURMAs { get; set; } = new List<TURMA>();

    [InverseProperty("tipo_aula")]
    public virtual ICollection<UC_DOCENTE> UC_DOCENTEs { get; set; } = new List<UC_DOCENTE>();
}
