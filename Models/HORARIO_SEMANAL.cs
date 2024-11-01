using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace HorariosIPBejaMVC.Models;

[Table("HORARIO_SEMANAL")]
public partial class HORARIO_SEMANAL 
{
    [Key]
    public int id { get; set; }

    public int semana_id { get; set; }

    public int uc_id { get; set; }

    public int turma_id { get; set; }

    public int docente_id { get; set; }

    public int sala_id { get; set; }

    public int periodo_horario_id { get; set; }

    public int ano_letivo_id { get; set; }

    [ForeignKey("ano_letivo_id")]
    [InverseProperty("HORARIO_SEMANALs")]
    public virtual ANO_LETIVO ano_letivo { get; set; } = null!;

    [ForeignKey("docente_id")]
    [InverseProperty("HORARIO_SEMANALs")]
    public virtual DOCENTE docente { get; set; } = null!;

    [ForeignKey("periodo_horario_id")]
    [InverseProperty("HORARIO_SEMANALs")]
    public virtual PERIODO_HORARIO periodo_horario { get; set; } = null!;

    [ForeignKey("sala_id")]
    [InverseProperty("HORARIO_SEMANALs")]
    public virtual SALA sala { get; set; } = null!;

    [ForeignKey("semana_id")]
    [InverseProperty("HORARIO_SEMANALs")]
    public virtual SEMANA semana { get; set; } = null!;

    [ForeignKey("turma_id")]
    [InverseProperty("HORARIO_SEMANALs")]
    public virtual TURMA turma { get; set; } = null!;

    [ForeignKey("uc_id")]
    [InverseProperty("HORARIO_SEMANALs")]
    public virtual UNIDADE_CURRICULAR uc { get; set; } = null!;

    // Implementação da propriedade DescricaoPeriodo
    public string DescricaoPeriodo => periodo_horario.descricao;


}
