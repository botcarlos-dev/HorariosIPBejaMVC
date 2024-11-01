using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace HorariosIPBejaMVC.Models;

[Table("INSTITUICAO")]
[Index("nome", Name = "UQ__INSTITUI__6F71C0DCA200F591", IsUnique = true)]
public partial class INSTITUICAO
{
    [Key]
    public int id { get; set; }

    [StringLength(100)]
    [Unicode(false)]
    public string nome { get; set; } = null!;

    [InverseProperty("instituicao")]
    public virtual ICollection<ESCOLA> ESCOLAs { get; set; } = new List<ESCOLA>();

    [InverseProperty("instituicao")]
    public virtual ICollection<FUNCIONARIO> FUNCIONARIOs { get; set; } = new List<FUNCIONARIO>();
}
