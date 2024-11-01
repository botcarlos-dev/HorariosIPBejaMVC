using System.ComponentModel.DataAnnotations.Schema;

namespace HorariosIPBejaMVC.Models
{
    [Table("ALUNO_UC")]
    public class ALUNO_UC
    {
        
        public int aluno_id { get; set; }
        public int uc_id { get; set; }

        // Navegações
        [ForeignKey("aluno_id")]
        public virtual ALUNO ALUNO { get; set; } = null!;

        [ForeignKey("uc_id")]
        public virtual UNIDADE_CURRICULAR UNIDADE_CURRICULAR { get; set; } = null!;
    }
}
