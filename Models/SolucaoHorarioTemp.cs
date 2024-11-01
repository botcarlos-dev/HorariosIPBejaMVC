using System;
using System.ComponentModel.DataAnnotations;

namespace HorariosIPBejaMVC.Models
{
    public class SolucaoHorarioTemp
    {
        [Key]
        public int Id { get; set; }

        public string DadosJson { get; set; }

        public DateTime DataCriacao { get; set; } = DateTime.Now;
    }
}
