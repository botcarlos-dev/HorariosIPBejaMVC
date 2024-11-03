import xml.etree.ElementTree as ET
import pandas as pd
import pickle
import os

def parse_solution(file_path, pickle_file):
    try:
        tree = ET.parse(file_path)
        root = tree.getroot()
    except ET.ParseError as e:
        print(f"Erro ao analisar o arquivo XML: {e}")
        return [], set()
    except FileNotFoundError:
        print(f"Arquivo não encontrado: {file_path}")
        return [], set()
    
    # Carregar os dados preparados para mapear turma_id
    try:
        with open(pickle_file, 'rb') as f:
            prepared_data = pickle.load(f)
        turmas_df = prepared_data['turmas']
    except Exception as e:
        print(f"Erro ao carregar o arquivo pickle: {e}")
        return [], set()
    
    scheduled_classes = []
    used_rooms = set()
    total_variables = 0
    x_variables_found = 0
    
    for variable in root.findall('.//variable'):
        total_variables += 1
        name = variable.get('name')
        value = float(variable.get('value'))
        
        # Debug: Imprimir todas as variáveis
        print(f"Variável encontrada: {name} = {value}")
        
        if name.startswith('x') and value == 1:
            x_variables_found += 1
            parts = name.split('_')
            if len(parts) == 3:
                x_turma_id, sala_id, periodo_id = parts
                turma_id = x_turma_id[1:]  # Remove 'x' do início
                
                try:
                    periodo_id = int(periodo_id)
                except ValueError:
                    print(f"Periodo_id inválido: {periodo_id} na variável {name}")
                    continue
                
                # Obter informações da turma a partir do turma_id
                try:
                    turma_id_int = int(turma_id)
                except ValueError:
                    print(f"Turma_id inválido: {turma_id} na variável {name}")
                    continue
                
                turma_info = turmas_df[turmas_df['id'] == turma_id_int]
                if turma_info.empty:
                    print(f"Turma_id não encontrada: {turma_id}")
                    continue
                
                turma_info = turma_info.iloc[0]
                
                scheduled_classes.append({
                    'turma_id': turma_id,  # Mantém internamente, mas não exibirá
                    'unidade_curricular_id': turma_info['unidade_curricular_id'],
                    'tipo_aula_id': turma_info['tipo_aula_id'],
                    'turma_label_id': turma_info['turma_label_id'],
                    'sala_id': sala_id,
                    'periodo_id': periodo_id
                })
            else:
                print(f"Formato inesperado da variável x: {name}")
        
        elif name.startswith('z_') and value == 1:
            parts = name.split('_')
            if len(parts) >= 2:
                sala_id = parts[1]
                used_rooms.add(sala_id)
            else:
                print(f"Formato inesperado da variável z: {name}")
    
    print(f"\nTotal de variáveis processadas: {total_variables}")
    print(f"Total de variáveis 'x' com valor 1 encontradas: {x_variables_found}")
    
    return scheduled_classes, used_rooms

def create_timetable(scheduled_classes, max_periods_per_day=15):
    # Definindo os horários de 8h30 a 23h30 com intervalos de 1 hora
    horarios = [f'{8 + i}:30 - {8 + i + 1}:30' for i in range(max_periods_per_day)]
    
    # Criando o DataFrame do horário
    days = ["Segunda-feira", "Terça-feira", "Quarta-feira", "Quinta-feira", "Sexta-feira"]
    timetable = pd.DataFrame(index=horarios, columns=days)

    # Inicializar todas as células como strings vazias
    timetable.fillna("", inplace=True)

    # Preenchendo a grelha de horário com as aulas agendadas
    for cls in scheduled_classes:
        period_id = cls['periodo_id']
        
        # Calcular o dia e o horário, verificando se está no intervalo permitido
        day_idx = (period_id - 1) // max_periods_per_day
        time_idx = (period_id - 1) % max_periods_per_day
        
        if day_idx < len(days) and time_idx < len(horarios):  # Certificar-se de que o índice é válido
            day = days[day_idx]
            time_slot = horarios[time_idx]

            allocation = (f"UC: {cls['unidade_curricular_id']} | "
                          f"Tipo: {cls['tipo_aula_id']} | "
                          f"Label: {cls['turma_label_id']} | "
                          f"Sala: {cls['sala_id']}")
            
            # Adiciona a aula à célula correta, com quebra de linha se já houver conteúdo
            if timetable.at[time_slot, day] == "":
                timetable.at[time_slot, day] = allocation
            else:
                timetable.at[time_slot, day] += f"<br>{allocation}"
            
            # Debug opcional
            print(f"Aula {cls['turma_id']} alocada em {day}, {time_slot}.")
        else:
            print(f"Atenção: Período {period_id} excede o limite de {len(days) * max_periods_per_day} períodos.")
    
    # Estilizando o HTML com CSS
    timetable_html = timetable.to_html(classes='timetable', border=0, escape=False)
    try:
        with open("timetable.html", "w", encoding='utf-8') as file:
            file.write("""
            <html>
            <head>
            <title>School Timetable</title>
            <style>
                body { font-family: Arial, sans-serif; }
                h1 { text-align: center; }
                .timetable {
                    width: 80%;
                    margin: 0 auto;
                    border-collapse: collapse;
                    font-size: 14px;
                }
                .timetable th, .timetable td {
                    border: 1px solid #ddd;
                    padding: 8px;
                    text-align: center;
                    vertical-align: top;
                }
                .timetable th {
                    background-color: #4CAF50;
                    color: white;
                }
                .timetable tr:nth-child(even) { background-color: #f2f2f2; }
                .timetable tr:hover { background-color: #ddd; }
            </style>
            </head>
            <body>
            <h1>School Timetable</h1>
            """ + timetable_html + """
            </body>
            </html>
            """)
        
        print("\n=== Timetable foi salvo em timetable.html ===")
    except Exception as e:
        print(f"Erro ao salvar o arquivo HTML: {e}")

def main():
    solution_file = 'schedule_impares.sol'
    pickle_file = 'coronetMatrices_impares.pkl'  # Ajuste conforme necessário
    
    # Verifique se o arquivo pickle existe
    if not os.path.exists(pickle_file):
        print(f"Arquivo pickle não encontrado: {pickle_file}")
        return
    
    # Parse the solution
    scheduled, rooms = parse_solution(solution_file, pickle_file)
    
    if not scheduled:
        print("Nenhuma aula agendada encontrada. Verifique o arquivo de solução.")
    else:
        print("Aulas Agendadas:")
        for cls in scheduled:
            print(cls)
        
        print("\nSalas Utilizadas:")
        for room in rooms:
            print(room)
        
        # Criar e salvar o horário como HTML
        create_timetable(scheduled)

if __name__ == "__main__":
    main()
