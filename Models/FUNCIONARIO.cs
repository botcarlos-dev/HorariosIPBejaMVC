using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace HorariosIPBejaMVC.Models;

[Table("FUNCIONARIO")]
[Index("numero", Name = "UQ__FUNCIONA__FC77F211F97356F7", IsUnique = true)]
public partial class FUNCIONARIO
{
    [Key]
    public int id { get; set; }

    public int numero { get; set; }

    [StringLength(100)]
    [Unicode(false)]
    public string? cargo { get; set; }

    public int instituicao_id { get; set; }

    public bool is_gabinete_gestao { get; set; }

    [ForeignKey("id")]
    [InverseProperty("FUNCIONARIO")]
    public virtual UTILIZADOR idNavigation { get; set; } = null!;

    [ForeignKey("instituicao_id")]
    [InverseProperty("FUNCIONARIOs")]
    public virtual INSTITUICAO instituicao { get; set; } = null!;
}
