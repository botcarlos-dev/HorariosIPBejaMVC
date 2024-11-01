// Models/HorariosReferenciaisViewModel.cs
using System;
using System.Collections.Generic;

namespace HorariosIPBejaMVC.Models
{
    /// <summary>
    /// ViewModel utilizado para representar e organizar os horários referenciais disponíveis para visualização.
    /// </summary>
    public class HorariosReferenciaisViewModel
    {
        /// <summary>
        /// Lista dos dias da semana para organização dos horários.
        /// </summary>
        public List<string> DiasDaSemana { get; set; }

        /// <summary>
        /// Lista de períodos horários (intervalos de tempo) disponíveis para cada dia.
        /// </summary>
        public List<string> Periodos { get; set; }

        /// <summary>
        /// Dicionário que organiza os horários referenciais por dias da semana e períodos.
        /// </summary>
        public Dictionary<string, Dictionary<string, HORARIO_REFERENCIAL>> HorariosReferenciais { get; set; }

        /// <summary>
        /// Construtor da classe <see cref="HorariosReferenciaisViewModel"/>.
        /// Inicializa as listas de dias da semana, períodos e estrutura dos horários referenciais.
        /// </summary>
        public HorariosReferenciaisViewModel()
        {
            DiasDaSemana = new List<string>
            {
                "Segunda-feira",
                "Terça-feira",
                "Quarta-feira",
                "Quinta-feira",
                "Sexta-feira",
                "Sábado"
            };

            Periodos = GerarPeriodos(new TimeSpan(8, 30, 0), new TimeSpan(23, 30, 0), TimeSpan.FromHours(1));
            HorariosReferenciais = InicializarHorarios();
        }

        /// <summary>
        /// Gera uma lista de períodos de horário (intervalos de tempo) com base em uma hora inicial, uma hora final e um intervalo de tempo especificado.
        /// </summary>
        /// <param name="start">Hora de início do primeiro período.</param>
        /// <param name="end">Hora de fim do último período.</param>
        /// <param name="interval">Duração de cada período de horário.</param>
        /// <returns>Lista de strings representando os intervalos de tempo.</returns>
        /// <remarks>
        /// [Referência: Manipulação de Datas e Horas em C#](https://learn.microsoft.com/pt-pt/dotnet/standard/datetime/)
        /// </remarks>
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
        /// Inicializa a estrutura dos horários referenciais, organizando os dias da semana e períodos horários.
        /// </summary>
        /// <returns>
        /// Dicionário onde cada dia da semana possui um dicionário de períodos com horários inicialmente vazios.
        /// </returns>
        private Dictionary<string, Dictionary<string, HORARIO_REFERENCIAL>> InicializarHorarios()
        {
            var horarios = new Dictionary<string, Dictionary<string, HORARIO_REFERENCIAL>>();

            foreach (var dia in DiasDaSemana)
            {
                horarios[dia] = new Dictionary<string, HORARIO_REFERENCIAL>();
                foreach (var periodo in Periodos)
                {
                    horarios[dia][periodo] = null; // Inicialmente vazio
                }
            }

            return horarios;
        }
    }
}
