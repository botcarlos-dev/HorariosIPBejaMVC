using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HorariosIPBejaMVC.Models
{
    [Table("SOLUCAO_HORARIO")]
    public class SolucaoHorario
    {
        [Key]
        public int Id { get; set; }

        public DateTime DataCriacao { get; set; }

        // Relação com HORARIO_REFERENCIAL
        public virtual ICollection<HORARIO_REFERENCIAL> HorariosReferenciais { get; set; } = new List<HORARIO_REFERENCIAL>();
    }
}
