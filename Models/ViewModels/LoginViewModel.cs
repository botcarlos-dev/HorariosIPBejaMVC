using System.ComponentModel.DataAnnotations;

namespace HorariosIPBejaMVC.Models.ViewModels
{
    /// <summary>
    /// ViewModel utilizado para capturar os dados de login do utilizador.
    /// </summary>
    public class LoginViewModel
    {
        /// <summary>
        /// Endereço de email do utilizador para autenticação.
        /// </summary>
        [Required(ErrorMessage = "Por favor, insira o email.")]
        [EmailAddress(ErrorMessage = "Email inválido.")]
        public string Email { get; set; }

        /// <summary>
        /// Palavra-passe do utilizador para autenticação.
        /// </summary>
        [Required(ErrorMessage = "Por favor, insira a palavra-passe.")]
        [DataType(DataType.Password)]
        public string PalavraPasse { get; set; }
    }
}
