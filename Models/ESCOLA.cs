using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace HorariosIPBejaMVC.Models;

[Table("ESCOLA")]
[Index("nome", Name = "UQ__ESCOLA__6F71C0DC3ED81842", IsUnique = true)]
public partial class ESCOLA
{
    [Key]
    public int id { get; set; }

    [StringLength(100)]
    [Unicode(false)]
    public string nome { get; set; } = null!;

    public int instituicao_id { get; set; }

    [InverseProperty("escola")]
    public virtual ICollection<CURSO> CURSOs { get; set; } = new List<CURSO>();

    [InverseProperty("escola")]
    public virtual ICollection<SALA> SALAs { get; set; } = new List<SALA>();

    [ForeignKey("instituicao_id")]
    [InverseProperty("ESCOLAs")]
    public virtual INSTITUICAO instituicao { get; set; } = null!;
}
