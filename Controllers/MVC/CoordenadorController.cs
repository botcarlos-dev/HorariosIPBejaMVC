using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using HorariosIPBejaMVC.Models;
using HorariosIPBejaMVC.Models.ViewModels;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;
using System.Threading.Tasks;
using HorariosIPBejaMVC.Data;
using Newtonsoft.Json;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Microsoft.EntityFrameworkCore;
using HorariosIPBejaMVC.Models.DTO;
using Microsoft.AspNetCore.Hosting;

namespace HorariosIPBejaMVC.Controllers
{
    /// <summary>
    /// Controlador responsável pela gestão de funcionalidades exclusivas para o papel de Coordenador.
    /// </summary>
    [Authorize(Roles = "Coordenador")]
    public class CoordenadorController : Controller
    {
        private readonly ILogger<CoordenadorController> _logger;
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Construtor do <see cref="CoordenadorController"/>.
        /// </summary>
        /// <param name="logger">Logger para registar eventos e erros.</param>
        /// <param name="configuration">Configurações da aplicação.</param>
        /// <param name="context">Contexto da base de dados da aplicação.</param>
        public CoordenadorController(ILogger<CoordenadorController> logger, IConfiguration configuration, ApplicationDbContext context)
        {
            _logger = logger;
            _configuration = configuration;
            _context = context;
        }

        /// <summary>
        /// Exibe a página para geração de horários.
        /// </summary>
        /// <returns>Vista da página de geração de horários.</returns>
        [HttpGet]
        public IActionResult GerarHorario()
        {
            return View();
        }

        /// <summary>
        /// Executa o processo de geração de horários, incluindo a execução de scripts Python e otimização com CPLEX.
        /// </summary>
        /// <returns>Redireciona para a vista de seleção de horários com o modelo de vista atualizado.</returns>
        /// <remarks>
        /// [Referência: Executando Processos Externos em ASP.NET Core](https://learn.microsoft.com/pt-pt/dotnet/api/system.diagnostics.process)
        /// </remarks>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExecutarGeracaoHorario()
        {
            var viewModel = new TimetableViewModel();
            try
            {
                // Caminho dos scripts Python a serem executados
                string processDataScriptPath = _configuration["ScriptPaths:ProcessData"];
                string geradorHorarioScriptPath = _configuration["ScriptPaths:GeradorHorario"];

                // Executa os scripts Python para gerar os arquivos .pkl e .lp
                await ExecutarScriptPythonAsync(processDataScriptPath);
                await ExecutarScriptPythonAsync(geradorHorarioScriptPath);

                // Diretório para os arquivos LP e solução
                string lpDirectory = _configuration["ScriptPaths:LpDirectory"];
                if (!Directory.Exists(lpDirectory))
                {
                    Directory.CreateDirectory(lpDirectory);
                }

                // Otimizar arquivos LP e parsear as soluções
                string[] lpFiles = { "schedule_impares.lp", "schedule_pares.lp" };
                foreach (var lpFile in lpFiles)
                {
                    string lpFilePath = Path.Combine(lpDirectory, lpFile);
                    string solutionFilePath = lpFilePath.Replace(".lp", ".sol");

                    var optimizationResult = await RunCplexAsync(lpFilePath, solutionFilePath);
                    if (optimizationResult.Item1 == 0)
                    {
                        var horariosReferenciais = ParseCplexSolution(solutionFilePath);
                        viewModel.AdicionarSolucao(horariosReferenciais);

                        // Log para verificar o conteúdo de SolucoesHorarios
                        _logger.LogInformation($"Soluções adicionadas para o arquivo {lpFile}:");
                        foreach (var solucao in viewModel.SolucoesHorarios)
                        {
                            foreach (var dia in solucao.Keys)
                            {
                                foreach (var periodo in solucao[dia].Keys)
                                {
                                    var horarios = solucao[dia][periodo];
                                    foreach (var horario in horarios)
                                    {
                                        _logger.LogInformation($"Dia: {dia}, Período: {periodo}, UC: {(horario?.uc?.nome ?? "Vazio")}, Sala: {(horario?.sala?.nome ?? "Vazio")}");
                                    }
                                }
                            }
                        }
                    }
                }

                viewModel.MensagemResultado = "Horários gerados com sucesso. Selecione uma solução abaixo para salvar.";

                // Converter as soluções para DTOs
                var solucoesDTO = viewModel.SolucoesHorarios.Select(solucao =>
                    solucao.ToDictionary(
                        dia => dia.Key,
                        dia => dia.Value.ToDictionary(
                            periodo => periodo.Key,
                            periodo => periodo.Value.Select(horario => new HorarioReferencialDTO
                            {
                                turma_id = horario.turma_id,
                                sala_id = horario.sala_id,
                                periodo_horario_id = horario.periodo_horario_id,
                                uc_id = horario.uc_id,
                                docente_id = horario.docente_id

                            }).ToList()
                        )
                    )
                ).ToList();

                // Serializa as soluções DTO em JSON
                var solucoesJson = JsonConvert.SerializeObject(solucoesDTO);

                // Cria uma nova instância de SolucaoHorarioTemp
                var solucaoTemp = new SolucaoHorarioTemp
                {
                    DadosJson = solucoesJson
                };

                // Armazena a solução temporária na base de dados
                _context.SolucaoHorarioTemps.Add(solucaoTemp);
                await _context.SaveChangesAsync();

                // Armazena o Id da solução temporária no TempData
                TempData["SolucaoTempId"] = solucaoTemp.Id;

                // Log para confirmar que as soluções foram armazenadas
                _logger.LogInformation($"Soluções armazenadas na base de dados com Id: {solucaoTemp.Id}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado ao gerar horários.");
                viewModel.MensagemErro = ex.Message;
            }

            return View("SelecionarHorario", viewModel); // Apenas exibe as soluções
        }

        /// <summary>
        /// Salva a solução de horário selecionada pelo coordenador na base de dados.
        /// </summary>
        /// <param name="solucaoIndex">Índice da solução selecionada.</param>
        /// <returns>Redireciona para a página de geração de horários com mensagens de sucesso ou erro.</returns>
        /// <remarks>
        /// [Referência: Trabalhando com TempData em ASP.NET Core](https://learn.microsoft.com/pt-pt/aspnet/core/fundamentals/app-state?view=aspnetcore-6.0#tempdata)
        /// </remarks>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SalvarHorarioSelecionado(int solucaoIndex)
        {
            try
            {
                // Recupera o Id da solução temporária do TempData
                if (!TempData.ContainsKey("SolucaoTempId"))
                {
                    TempData["MensagemErro"] = "Nenhuma solução foi encontrada. Por favor, gere os horários novamente.";
                    return RedirectToAction("GerarHorario");
                }

                int solucaoTempId = (int)TempData["SolucaoTempId"];

                // Recupera a solução temporária da base de dados
                var solucaoTemp = await _context.SolucaoHorarioTemps.FindAsync(solucaoTempId);

                if (solucaoTemp == null)
                {
                    TempData["MensagemErro"] = "A solução não foi encontrada ou expirou.";
                    return RedirectToAction("GerarHorario");
                }

                // Desserializa as soluções DTO
                var solucoesDTO = JsonConvert.DeserializeObject<List<Dictionary<string, Dictionary<string, List<HorarioReferencialDTO>>>>>(solucaoTemp.DadosJson);

                if (solucaoIndex < 0 || solucaoIndex >= solucoesDTO.Count)
                {
                    TempData["MensagemErro"] = "Índice da solução inválido selecionado.";
                    return RedirectToAction("SelecionarHorario");
                }

                // Seleciona a solução específica
                var solucaoSelecionadaDTO = solucoesDTO[solucaoIndex];

                // Cria a nova SolucaoHorario
                var solucaoHorario = new SolucaoHorario
                {
                    DataCriacao = DateTime.Now
                };

                // Adiciona a SolucaoHorario ao contexto para obter o Id
                _context.SolucaoHorarios.Add(solucaoHorario);
                await _context.SaveChangesAsync();

                // **Obter o Ano Letivo Atual**
                var anoLetivoAtual = await _context.ANO_LETIVOs.FirstOrDefaultAsync();
                if (anoLetivoAtual == null)
                {
                    TempData["MensagemErro"] = "Ano letivo não encontrado. Por favor, cadastre um ano letivo.";
                    return RedirectToAction("GerarHorario");
                }

                // Recria os objetos HORARIO_REFERENCIAL
                var horariosReferenciais = new List<HORARIO_REFERENCIAL>();

                foreach (var dia in solucaoSelecionadaDTO.Keys)
                {
                    foreach (var periodo in solucaoSelecionadaDTO[dia].Keys)
                    {
                        var horariosDTO = solucaoSelecionadaDTO[dia][periodo];

                        foreach (var dto in horariosDTO)
                        {
                            var horario = new HORARIO_REFERENCIAL
                            {
                                turma_id = dto.turma_id,
                                sala_id = dto.sala_id,
                                periodo_horario_id = dto.periodo_horario_id,
                                uc_id = dto.uc_id,
                                docente_id = dto.docente_id,
                                solucaoHorarioId = solucaoHorario.Id,
                                ano_letivo_id = anoLetivoAtual.id // **Define o ano_letivo_id corretamente**
                            };

                            // Adicionar o horario à lista
                            horariosReferenciais.Add(horario);
                        }
                    }
                }

                // Adiciona os HORARIO_REFERENCIAL ao contexto
                _context.HORARIO_REFERENCIALs.AddRange(horariosReferenciais);
                await _context.SaveChangesAsync();

                // Remove a solução temporária da base de dados
                _context.SolucaoHorarioTemps.Remove(solucaoTemp);
                await _context.SaveChangesAsync();

                TempData["MensagemSucesso"] = "Solução salva com sucesso!";
                return RedirectToAction("GerarHorario");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao salvar a solução selecionada.");
                TempData["MensagemErro"] = "Ocorreu um erro ao salvar a solução.";
                return RedirectToAction("GerarHorario");
            }
        }

        /// <summary>
        /// Executa o CPLEX pela linha de comando de forma assíncrona para otimização de horários.
        /// </summary>
        /// <param name="lpFilePath">Caminho para o arquivo LP a ser otimizado.</param>
        /// <param name="solutionFilePath">Caminho para o arquivo onde a solução será armazenada.</param>
        /// <returns>Retorna uma tupla contendo o código de saída e a saída do processo.</returns>
        private async Task<(int, string)> RunCplexAsync(string lpFilePath, string solutionFilePath)
        {
            var cplexCommand = _configuration["Cplex:ExecutablePath"];
            var cplexArguments = $"-c \"read '{lpFilePath}'\" \"optimize\" \"write '{solutionFilePath}'\"";

            _logger.LogInformation("Executando CPLEX com comando: " + cplexCommand + " " + cplexArguments);

            var processStartInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/C \"\"{cplexCommand}\" {cplexArguments}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            using (var process = new Process { StartInfo = processStartInfo })
            {
                var outputBuilder = new System.Text.StringBuilder();
                var errorBuilder = new System.Text.StringBuilder();

                process.OutputDataReceived += (sender, args) =>
                {
                    if (args.Data != null)
                        outputBuilder.AppendLine(args.Data);
                };
                process.ErrorDataReceived += (sender, args) =>
                {
                    if (args.Data != null)
                        errorBuilder.AppendLine(args.Data);
                };

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                await process.WaitForExitAsync();

                string output = outputBuilder.ToString();
                string error = errorBuilder.ToString();

                _logger.LogInformation("CPLEX Output: " + output);
                _logger.LogError("CPLEX Error Output: " + error);

                return (process.ExitCode, string.IsNullOrEmpty(error) ? output : error);
            }
        }

        /// <summary>
        /// Executa um script Python de forma assíncrona.
        /// </summary>
        /// <param name="scriptPath">Caminho para o script Python a ser executado.</param>
        /// <returns>Retorna uma tupla contendo o código de saída e a saída do processo.</returns>
        private async Task<(int, string)> ExecutarScriptPythonAsync(string scriptPath)
        {
            var processStartInfo = new ProcessStartInfo
            {
                FileName = "python",
                Arguments = $"\"{scriptPath}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            using (var process = new Process { StartInfo = processStartInfo })
            {
                var outputBuilder = new System.Text.StringBuilder();
                var errorBuilder = new System.Text.StringBuilder();

                process.OutputDataReceived += (sender, args) =>
                {
                    if (args.Data != null)
                        outputBuilder.AppendLine(args.Data);
                };
                process.ErrorDataReceived += (sender, args) =>
                {
                    if (args.Data != null)
                        errorBuilder.AppendLine(args.Data);
                };

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                await process.WaitForExitAsync();

                string output = outputBuilder.ToString();
                string error = errorBuilder.ToString();

                return (process.ExitCode, string.IsNullOrEmpty(error) ? output : error);
            }
        }

        /// <summary>
        /// Analisa o arquivo de solução gerado pelo CPLEX e cria uma lista de objetos <see cref="HORARIO_REFERENCIAL"/>.
        /// </summary>
        /// <param name="solutionFilePath">Caminho para o arquivo de solução XML.</param>
        /// <returns>Lista de objetos <see cref="HORARIO_REFERENCIAL"/>.</returns>
        private List<HORARIO_REFERENCIAL> ParseCplexSolution(string solutionFilePath)
        {
            var horarioReferencialList = new List<HORARIO_REFERENCIAL>();

            try
            {
                // Carregar o documento XML do arquivo de solução
                XDocument doc = XDocument.Load(solutionFilePath);
                var variables = doc.Descendants("variable")
                    .Where(v => v.Attribute("value")?.Value == "1");

                foreach (var variable in variables)
                {
                    string name = variable.Attribute("name")?.Value;
                    if (string.IsNullOrEmpty(name)) continue;

                    // Dividir o nome para obter turma_id, sala_id e periodo_id
                    var parts = name.Substring(1).Split('_');
                    if (parts.Length == 3 &&
                        int.TryParse(parts[0], out int turmaId) &&
                        int.TryParse(parts[1], out int salaId) &&
                        int.TryParse(parts[2], out int periodoId))
                    {
                        // Carregar a turma incluindo o tipo_aula
                        var turma = _context.TURMAs
                                            .Include(t => t.tipo_aula)
                                            .FirstOrDefault(t => t.id == turmaId);
                        if (turma == null)
                        {
                            _logger.LogWarning($"Turma não encontrada com id={turmaId}");
                            continue;
                        }

                        // Criar o objeto HORARIO_REFERENCIAL
                        var horario = new HORARIO_REFERENCIAL
                        {
                            turma_id = turmaId,
                            sala_id = salaId,
                            periodo_horario_id = periodoId,
                            uc_id = turma.unidade_curricular_id,
                            docente_id = turma.docente_id,

                        };

                        // Carregar entidades relacionadas
                        horario.uc = _context.UNIDADE_CURRICULARs.Find(horario.uc_id);
                        horario.sala = _context.SALAs.Find(salaId);
                        horario.docente = _context.DOCENTEs.Find(horario.docente_id);
                        horario.periodo_horario = _context.PERIODO_HORARIOs.Find(periodoId);
                        horario.turma = turma; // Já inclui tipo_aula

                        // Adicionar horário à lista se estiver completo
                        if (horario.uc != null && horario.sala != null && horario.docente != null && horario.periodo_horario != null)
                        {
                            horarioReferencialList.Add(horario);
                        }
                        else
                        {
                            _logger.LogWarning($"Dados incompletos para turmaId={turmaId}, salaId={salaId}, periodoId={periodoId}");
                        }
                    }
                    else
                    {
                        _logger.LogWarning($"Formato inesperado da variável no arquivo de solução: {name}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao analisar o arquivo de solução: {solutionFilePath}");
            }

            return horarioReferencialList;
        }
    }
}

