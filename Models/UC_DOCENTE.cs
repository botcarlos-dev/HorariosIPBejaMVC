using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace HorariosIPBejaMVC.Models;

[PrimaryKey("uc_id", "docente_id", "tipo_aula_id")]
[Table("UC_DOCENTE")]
public partial class UC_DOCENTE
{
    [Key]
    public int uc_id { get; set; }

    [Key]
    public int docente_id { get; set; }

    [Key]
    public int tipo_aula_id { get; set; }

    [ForeignKey("docente_id")]
    [InverseProperty("UC_DOCENTEs")]
    public virtual DOCENTE docente { get; set; } = null!;

    [ForeignKey("tipo_aula_id")]
    [InverseProperty("UC_DOCENTEs")]
    public virtual TIPO_AULA tipo_aula { get; set; } = null!;

    [ForeignKey("uc_id")]
    [InverseProperty("UC_DOCENTEs")]
    public virtual UNIDADE_CURRICULAR uc { get; set; } = null!;
}
