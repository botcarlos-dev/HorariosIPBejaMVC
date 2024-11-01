using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace HorariosIPBejaMVC.Models;

[Table("UTILIZADOR")]
[Index("email", Name = "UQ__UTILIZAD__AB6E6164A5B09CFF", IsUnique = true)]
public partial class UTILIZADOR
{
    [Key]
    public int id { get; set; }

    [StringLength(100)]
    [Unicode(false)]
    public string nome { get; set; } = null!;

    [StringLength(100)]
    [Unicode(false)]
    public string email { get; set; } = null!;

    [MaxLength(64)]
    public byte[] senha { get; set; } = null!;

    [InverseProperty("idNavigation")]
    public virtual ALUNO? ALUNO { get; set; }

    [InverseProperty("idNavigation")]
    public virtual DOCENTE? DOCENTE { get; set; }

    [InverseProperty("idNavigation")]
    public virtual FUNCIONARIO? FUNCIONARIO { get; set; }

    [ForeignKey("utilizador_id")]
    [InverseProperty("utilizadors")]
    public virtual ICollection<TIPO_UTILIZADOR> tipo_utilizadors { get; set; } = new List<TIPO_UTILIZADOR>();
}
