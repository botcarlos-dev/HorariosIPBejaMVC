using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace HorariosIPBejaMVC.Models;

[Table("TIPO_UTILIZADOR")]
[Index("descricao", Name = "UQ__TIPO_UTI__91D38C28D3DEAB80", IsUnique = true)]
public partial class TIPO_UTILIZADOR
{
    [Key]
    public int id { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string descricao { get; set; } = null!;

    [ForeignKey("tipo_utilizador_id")]
    [InverseProperty("tipo_utilizadors")]
    public virtual ICollection<UTILIZADOR> utilizadors { get; set; } = new List<UTILIZADOR>();
}
