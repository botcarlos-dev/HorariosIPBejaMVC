using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HorariosIPBejaMVC.Data;
using HorariosIPBejaMVC.Models;
using HorariosIPBejaMVC.Models.ViewModels;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace HorariosIPBejaMVC.Controllers
{
    /// <summary>
    /// Controlador responsável pela gestão e visualização de horários para Alunos e Docentes.
    /// </summary>
    [Authorize(Roles = "Aluno,Docente")]
    public class HorarioController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<HorarioController> _logger;

        /// <summary>
        /// Construtor do <see cref="HorarioController"/>.
        /// </summary>
        /// <param name="context">Contexto da base de dados da aplicação.</param>
        /// <param name="logger">Logger para registar eventos e erros.</param>
        public HorarioController(ApplicationDbContext context, ILogger<HorarioController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Exibe o horário do utilizador logado (Aluno ou Docente).
        /// </summary>
        /// <returns>Vista do horário com base no papel do utilizador.</returns>
        /// <remarks>
        /// [Referência: Authorization in ASP.NET Core](https://learn.microsoft.com/pt-pt/aspnet/core/security/authorization/introduction?view=aspnetcore-6.0)
        /// </remarks>
        // GET: /Horario/Timetable
        public async Task<IActionResult> Timetable()
        {
            var viewModel = await BuildTimetableViewModel();
            return View(viewModel);
        }

        /// <summary>
        /// Exibe os horários pessoais do utilizador logado (Aluno ou Docente).
        /// </summary>
        /// <returns>Vista do horário pessoal.</returns>
        // GET: /Horario/HorariosPessoais
        public async Task<IActionResult> HorariosPessoais()
        {
            var viewModel = await BuildTimetableViewModel();
            return View("Timetable", viewModel); 
        }

        /// <summary>
        /// Exibe os horários referenciais com possibilidade de filtragem por escola, curso e semestre.
        /// </summary>
        /// <param name="escolaId">ID da escola para filtrar os horários.</param>
        /// <param name="cursoId">ID do curso para filtrar os horários.</param>
        /// <param name="semestre">Número do semestre para filtrar os horários.</param>
        /// <returns>Vista dos horários referenciais filtrados.</returns>
        // GET: /Horario/HorariosReferenciais
        public async Task<IActionResult> HorariosReferenciais(int? escolaId, int? cursoId, int? semestre)
        {
            // Carregar as escolas e cursos para o filtro
            var escolas = await _context.ESCOLAs.ToListAsync();
            var cursos = await _context.CURSOs.ToListAsync();

            // Passar os dados para a ViewBag para uso na view
            ViewBag.Escolas = escolas;
            ViewBag.Cursos = cursos;

            var viewModel = new TimetableViewModel
            {
                DiasDaSemana = new List<string> { "Segunda-feira", "Terça-feira", "Quarta-feira", "Quinta-feira", "Sexta-feira" },
                Periodos = new List<string>
                {
                    "08:30 - 09:30",
                    "09:30 - 10:30",
                    "10:30 - 11:30",
                    "11:30 - 12:30",
                    "12:30 - 13:30",
                    "13:30 - 14:30",
                    "14:30 - 15:30",
                    "15:30 - 16:30",
                    "16:30 - 17:30",
                    "17:30 - 18:30"
                },
                HorariosReferenciais = new Dictionary<string, Dictionary<string, List<HORARIO_REFERENCIAL>>>()
            };

            // Verificar se os filtros estão aplicados
            if (escolaId.HasValue && cursoId.HasValue && semestre.HasValue)
            {
                // Obter os dados filtrados
                var horariosReferenciais = await _context.HORARIO_REFERENCIALs
                    .Include(h => h.uc)
                        .ThenInclude(uc => uc.curso)
                    .Include(h => h.turma)
                        .ThenInclude(t => t.tipo_aula)
                    .Include(h => h.docente)
                    .Include(h => h.sala)
                    .Include(h => h.periodo_horario)
                    .Where(h => h.uc.curso_id == cursoId.Value && h.uc.semestre == semestre.Value && h.uc.curso.escola_id == escolaId.Value)
                    .ToListAsync();

                // Organizar os dados no ViewModel
                foreach (var horario in horariosReferenciais)
                {
                    string diaSemana = ObterNomeDiaSemana(horario.periodo_horario.dia_semana);
                    string periodo = ObterDescricaoPeriodo(horario.periodo_horario.hora_inicio, horario.periodo_horario.hora_fim);

                    if (!viewModel.HorariosReferenciais.ContainsKey(diaSemana))
                    {
                        viewModel.HorariosReferenciais[diaSemana] = new Dictionary<string, List<HORARIO_REFERENCIAL>>();
                    }

                    if (!viewModel.HorariosReferenciais[diaSemana].ContainsKey(periodo))
                    {
                        viewModel.HorariosReferenciais[diaSemana][periodo] = new List<HORARIO_REFERENCIAL>();
                    }

                    viewModel.HorariosReferenciais[diaSemana][periodo].Add(horario);
                }
            }

            return View("HorariosReferenciais", viewModel); // Carrega a view específica "HorariosReferenciais"
        }

        /// <summary>
        /// Obtém os horários referenciais filtrados com base nos parâmetros fornecidos e retorna em formato JSON.
        /// </summary>
        /// <param name="escolaId">ID da escola para filtrar os horários.</param>
        /// <param name="cursoId">ID do curso para filtrar os horários.</param>
        /// <param name="semestre">Número do semestre para filtrar os horários.</param>
        /// <returns>Dados dos horários referenciais em formato JSON.</returns>
        // GET: /Horario/GetHorariosReferenciais
        [HttpGet]
        public async Task<IActionResult> GetHorariosReferenciais(int escolaId, int cursoId, int semestre)
        {
            // Obter os horários referenciais filtrados
            var horariosReferenciais = await _context.HORARIO_REFERENCIALs
                .Include(h => h.uc)
                    .ThenInclude(uc => uc.curso)
                .Include(h => h.turma)
                    .ThenInclude(t => t.tipo_aula)
                .Include(h => h.docente)
                .Include(h => h.sala)
                .Include(h => h.periodo_horario)
                .Where(h => h.uc.curso_id == cursoId && h.uc.semestre == semestre && h.uc.curso.escola_id == escolaId)
                .ToListAsync();

            // Preparar os dados para serem enviados em formato JSON
            var horariosData = horariosReferenciais.Select(h => new
            {
                Dia = ObterNomeDiaSemana(h.periodo_horario.dia_semana),
                Periodo = ObterDescricaoPeriodo(h.periodo_horario.hora_inicio, h.periodo_horario.hora_fim),
                UC = h.uc.nome,
                Docente = h.docente.Nome,
                Sala = h.sala.nome,
                TipoAula = h.turma.tipo_aula.descricao,
                Turma = h.turma.turma_label
            }).ToList();

            return Json(horariosData);
        }

        /// <summary>
        /// Obtém os cursos associados a uma escola específica e retorna em formato JSON.
        /// </summary>
        /// <param name="escolaId">ID da escola para filtrar os cursos.</param>
        /// <returns>Lista de cursos em formato JSON.</returns>
        // GET: /Horario/GetCursosByEscola/{escolaId}
        [HttpGet]
        public async Task<IActionResult> GetCursosByEscola(int escolaId)
        {
            var cursos = await _context.CURSOs
                .Where(c => c.escola_id == escolaId)
                .Select(c => new { c.id, c.nome })
                .ToListAsync();

            return Json(cursos);
        }

        /// <summary>
        /// Constrói o <see cref="TimetableViewModel"/> com base no utilizador logado e seus papéis (Aluno ou Docente).
        /// </summary>
        /// <returns>Instância de <see cref="TimetableViewModel"/> preenchida com os horários pessoais.</returns>
        private async Task<TimetableViewModel> BuildTimetableViewModel()
        {
            var viewModel = new TimetableViewModel
            {
                DiasDaSemana = new List<string> { "Segunda-feira", "Terça-feira", "Quarta-feira", "Quinta-feira", "Sexta-feira" },
                Periodos = new List<string>
                {
                    "08:30 - 09:30",
                    "09:30 - 10:30",
                    "10:30 - 11:30",
                    "11:30 - 12:30",
                    "12:30 - 13:30",
                    "13:30 - 14:30",
                    "14:30 - 15:30",
                    "15:30 - 16:30",
                    "16:30 - 17:30",
                    "17:30 - 18:30"
                },
                HorariosReferenciais = new Dictionary<string, Dictionary<string, List<HORARIO_REFERENCIAL>>>()
            };

            // Obter o ID do utilizador logado
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                _logger.LogError("Erro ao obter o ID do utilizador logado.");
                throw new UnauthorizedAccessException("Utilizador não autorizado.");
            }

            // Identificar os papéis do utilizador
            var roles = User.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToList();

            // Carregar Horários Pessoais filtrados com base no papel do utilizador
            List<HORARIO_REFERENCIAL> horariosPessoais;
            if (roles.Contains("Docente"))
            {
                horariosPessoais = await _context.HORARIO_REFERENCIALs
                    .Where(h => h.docente_id == userId)
                    .Include(h => h.uc)
                    .Include(h => h.turma)
                        .ThenInclude(t => t.tipo_aula)
                    .Include(h => h.sala)
                    .Include(h => h.periodo_horario)
                    .Include(h => h.docente)
                    .AsNoTracking()
                    .ToListAsync();
            }
            else if (roles.Contains("Aluno"))
            {
                
                var ucIds = await _context.ALUNO_UCs
                    .Where(au => au.aluno_id == userId)
                    .Select(au => au.uc_id)
                    .ToListAsync();

                horariosPessoais = await _context.HORARIO_REFERENCIALs
                    .Where(h => ucIds.Contains(h.uc_id))
                    .Include(h => h.uc)
                    .Include(h => h.turma)
                        .ThenInclude(t => t.tipo_aula)
                    .Include(h => h.sala)
                    .Include(h => h.periodo_horario)
                    .Include(h => h.docente)
                    .AsNoTracking()
                    .ToListAsync();
            }
            else
            {
                _logger.LogWarning("Utilizador com papéis não reconhecidos tentou aceder aos horários.");
                return viewModel; // Retorna um TimetableViewModel vazio
            }

            // Eliminar possíveis duplicados com base no ID do HORARIO_REFERENCIAL
            var horariosUnicos = horariosPessoais.GroupBy(h => h.id).Select(g => g.First()).ToList();

            // Preencher o ViewModel com os horários pessoais
            foreach (var horario in horariosUnicos)
            {
                string diaSemana = ObterNomeDiaSemana(horario.periodo_horario.dia_semana);
                string periodo = ObterDescricaoPeriodo(horario.periodo_horario.hora_inicio, horario.periodo_horario.hora_fim);

                if (!viewModel.HorariosReferenciais.ContainsKey(diaSemana))
                {
                    viewModel.HorariosReferenciais[diaSemana] = new Dictionary<string, List<HORARIO_REFERENCIAL>>();
                }

                if (!viewModel.HorariosReferenciais[diaSemana].ContainsKey(periodo))
                {
                    viewModel.HorariosReferenciais[diaSemana][periodo] = new List<HORARIO_REFERENCIAL>();
                }

                viewModel.HorariosReferenciais[diaSemana][periodo].Add(horario);
            }

            return viewModel;
        }

        /// <summary>
        /// Obtém o nome completo do dia da semana com base no identificador ou nome abreviado.
        /// </summary>
        /// <param name="diaSemana">Identificador ou nome do dia da semana.</param>
        /// <returns>Nome completo do dia da semana.</returns>
        private string ObterNomeDiaSemana(string diaSemana)
        {
            var dias = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "1", "Segunda-feira" },
                { "2", "Terça-feira" },
                { "3", "Quarta-feira" },
                { "4", "Quinta-feira" },
                { "5", "Sexta-feira" },
                { "Segunda", "Segunda-feira" },
                { "Terça", "Terça-feira" },
                { "Quarta", "Quarta-feira" },
                { "Quinta", "Quinta-feira" },
                { "Sexta", "Sexta-feira" }
            };

            return dias.ContainsKey(diaSemana) ? dias[diaSemana] : "Desconhecido";
        }

        /// <summary>
        /// Obtém a descrição do período de horário com base nas horas de início e fim.
        /// </summary>
        /// <param name="horaInicio">Hora de início do período.</param>
        /// <param name="horaFim">Hora de fim do período.</param>
        /// <returns>Descrição formatada do período de horário.</returns>
        private string ObterDescricaoPeriodo(TimeOnly horaInicio, TimeOnly horaFim)
        {
            return $"{horaInicio:HH\\:mm} - {horaFim:HH\\:mm}";
        }
    }
}
