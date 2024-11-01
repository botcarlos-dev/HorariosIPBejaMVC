namespace HorariosIPBejaMVC.Models.DTO
{
    /// <summary>
    /// Data Transfer Object (DTO) utilizado para transferir dados referentes a um horário referencial entre diferentes camadas da aplicação.
    /// </summary>
    public class HorarioReferencialDTO
    {
        /// <summary>
        /// Identificador da turma associada ao horário referencial.
        /// </summary>
        public int turma_id { get; set; }

        /// <summary>
        /// Identificador da sala onde a turma terá lugar.
        /// </summary>
        public int sala_id { get; set; }

        /// <summary>
        /// Identificador do período de horário (ex.: manhã, tarde) em que a turma ocorrerá.
        /// </summary>
        public int periodo_horario_id { get; set; }

        /// <summary>
        /// Identificador da Unidade Curricular (UC) associada ao horário referencial.
        /// </summary>
        public int uc_id { get; set; }

        /// <summary>
        /// Identificador do docente responsável pela Unidade Curricular.
        /// </summary>
        public int docente_id { get; set; }
    }
}
