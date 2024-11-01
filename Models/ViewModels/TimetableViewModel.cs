using System;
using System.Collections.Generic;

namespace HorariosIPBejaMVC.Models.ViewModels
{
    /// <summary>
    /// ViewModel utilizado para representar e organizar várias soluções de horários e a solução selecionada para exibição.
    /// </summary>
    public class TimetableViewModel
    {
        /// <summary>
        /// Lista dos dias da semana para organizar os horários.
        /// </summary>
        public List<string> DiasDaSemana { get; set; }

        /// <summary>
        /// Lista de períodos horários (intervalos de tempo de uma hora) disponíveis para cada dia.
        /// </summary>
        public List<string> Periodos { get; set; }

        /// <summary>
        /// Lista de múltiplas soluções de horários, onde cada solução é organizada por dias e períodos.
        /// </summary>
        public List<Dictionary<string, Dictionary<string, List<HORARIO_REFERENCIAL>>>> SolucoesHorarios { get; set; }

        /// <summary>
        /// Solução específica selecionada para exibição final.
        /// </summary>
        public Dictionary<string, Dictionary<string, List<HORARIO_REFERENCIAL>>> HorariosReferenciais { get; set; }

        /// <summary>
        /// Mensagem de resultado para feedback do utilizador.
        /// </summary>
        public string MensagemResultado { get; set; }

        /// <summary>
        /// Mensagem de erro para feedback do utilizador.
        /// </summary>
        public string MensagemErro { get; set; }

        /// <summary>
        /// Construtor da classe <see cref="TimetableViewModel"/>.
        /// Inicializa as listas de dias da semana, períodos e estrutura dos horários.
        /// </summary>
        public TimetableViewModel()
        {
            DiasDaSemana = new List<string> { "Segunda-feira", "Terça-feira", "Quarta-feira", "Quinta-feira", "Sexta-feira" };
            Periodos = GerarPeriodos(new TimeSpan(8, 30, 0), new TimeSpan(23, 30, 0), TimeSpan.FromHours(1));
            SolucoesHorarios = new List<Dictionary<string, Dictionary<string, List<HORARIO_REFERENCIAL>>>>();
            HorariosReferenciais = InicializarHorarios();
        }

        /// <summary>
        /// Gera uma lista de períodos de horário (intervalos de tempo) com base numa hora inicial, numa hora final e num intervalo de tempo especificado.
        /// </summary>
        /// <param name="start">Hora de início do primeiro período.</param>
        /// <param name="end">Hora de fim do último período.</param>
        /// <param name="interval">Duração de cada período de horário.</param>
        /// <returns>Lista de strings representando os intervalos de tempo.</returns>
        private List<string> GerarPeriodos(TimeSpan start, TimeSpan end, TimeSpan interval)
        {
            var periodos = new List<string>();
            var current = start;

            while (current + interval <= end)
            {
                var periodo = $"{current:hh\\:mm} - {(current + interval):hh\\:mm}";
                periodos.Add(periodo);
                current += interval;
            }

            return periodos;
        }

        /// <summary>
        /// Adiciona uma nova solução de horários à lista de soluções.
        /// </summary>
        /// <param name="horarioReferencialList">Lista de objetos <see cref="HORARIO_REFERENCIAL"/> que representa uma solução de horários.</param>
        public void AdicionarSolucao(List<HORARIO_REFERENCIAL> horarioReferencialList)
        {
            var solucao = InicializarHorarios();

            // Supondo 15 períodos por dia como no script Python
            int maxPeriodsPerDay = 15;
            var diasDaSemana = DiasDaSemana;
            var periodos = Periodos;

            foreach (var horario in horarioReferencialList)
            {
                int periodId = horario.periodo_horario_id;

                // Calcula o índice do dia e o índice do horário dentro do dia
                int dayIndex = (periodId - 1) / maxPeriodsPerDay;
                int timeIndex = (periodId - 1) % maxPeriodsPerDay;

                // Certifique-se de que os índices estão dentro do intervalo permitido
                if (dayIndex < diasDaSemana.Count && timeIndex < periodos.Count)
                {
                    string dia = diasDaSemana[dayIndex];
                    string periodo = periodos[timeIndex];

                    // Adiciona o horário à solução como lista
                    if (solucao.ContainsKey(dia) && solucao[dia].ContainsKey(periodo))
                    {
                        solucao[dia][periodo].Add(horario);
                    }
                }
            }

            SolucoesHorarios.Add(solucao);
        }

        /// <summary>
        /// Define uma solução específica como a solução atual para exibição final.
        /// </summary>
        /// <param name="indice">Índice da solução a ser exibida.</param>
        public void SelecionarSolucaoParaExibicao(int indice)
        {
            if (indice >= 0 && indice < SolucoesHorarios.Count)
            {
                HorariosReferenciais = SolucoesHorarios[indice];
            }
        }

        /// <summary>
        /// Inicializa a estrutura dos horários, organizando os dias da semana e períodos horários.
        /// </summary>
        /// <returns>
        /// Dicionário onde cada dia da semana possui um dicionário de períodos com horários inicialmente vazios.
        /// </returns>
        private Dictionary<string, Dictionary<string, List<HORARIO_REFERENCIAL>>> InicializarHorarios()
        {
            var horarios = new Dictionary<string, Dictionary<string, List<HORARIO_REFERENCIAL>>>();

            foreach (var dia in DiasDaSemana)
            {
                horarios[dia] = new Dictionary<string, List<HORARIO_REFERENCIAL>>();
                foreach (var periodo in Periodos)
                {
                    horarios[dia][periodo] = new List<HORARIO_REFERENCIAL>(); // Inicialmente vazio
                }
            }

            return horarios;
        }
    }
}
