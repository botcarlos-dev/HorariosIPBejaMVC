using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace HorariosIPBejaMVC.Models;

[Table("ALUNO")]
[Index("numero", Name = "UQ__ALUNO__FC77F211E4522784", IsUnique = true)]
public partial class ALUNO
{
    [Key]
    public int id { get; set; }

    public int numero { get; set; }

    public int curso_id { get; set; }

    [ForeignKey("curso_id")]
    [InverseProperty("ALUNOs")]
    public virtual CURSO curso { get; set; } = null!;

    [ForeignKey("id")]
    [InverseProperty("ALUNO")]
    public virtual UTILIZADOR idNavigation { get; set; } = null!;

    [ForeignKey("aluno_id")]
    [InverseProperty("alunos")]
    public virtual ICollection<UNIDADE_CURRICULAR> ucs { get; set; } = new List<UNIDADE_CURRICULAR>();

    // Propriedade de navegação para ALUNO_UC
    public virtual ICollection<ALUNO_UC> ALUNO_UCs { get; set; } = new List<ALUNO_UC>();
}
