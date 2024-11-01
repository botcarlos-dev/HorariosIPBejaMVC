using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace HorariosIPBejaMVC.Models;

[Table("TIPO_CURSO")]
[Index("descricao", Name = "UQ__TIPO_CUR__91D38C28ACBF61FC", IsUnique = true)]
public partial class TIPO_CURSO
{
    [Key]
    public int id { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string descricao { get; set; } = null!;

    [InverseProperty("tipo_curso")]
    public virtual ICollection<CURSO> CURSOs { get; set; } = new List<CURSO>();
}
