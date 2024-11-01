using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace HorariosIPBejaMVC.Models;

[Table("COORDENADOR_CURSO")]
[Index("docente_id", Name = "UQ__COORDENA__32BA4B112EFE30D1", IsUnique = true)]
public partial class COORDENADOR_CURSO
{
    [Key]
    public int docente_id { get; set; }

    public int curso_id { get; set; }

    [ForeignKey("curso_id")]
    [InverseProperty("COORDENADOR_CURSOs")]
    public virtual CURSO curso { get; set; } = null!;

    [ForeignKey("docente_id")]
    [InverseProperty("COORDENADOR_CURSO")]
    public virtual DOCENTE docente { get; set; } = null!;
}
