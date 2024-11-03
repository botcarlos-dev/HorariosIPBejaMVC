import pickle
import os
import logging
import pandas as pd

# Configuração do logging para informar sobre o progresso e possíveis erros no processamento
logging.basicConfig(level=logging.INFO, format='%(levelname)s: %(message)s')

def get_uc_id_from_var(var_name, turma_uc_map):
    """
    Extrai o UC_id a partir do nome da variável.

    Args:
        var_name (str): Nome da variável no formato 'x{turma_id}_{sala_id}_{period_id}'.
        turma_uc_map (dict): Mapeamento de turma_id para UC_id.

    Returns:
        int or None: UC_id correspondente ou None se não puder ser extraído.
    """
    try:
        parts = var_name.split('_')
        turma_id = int(parts[0][1:])  # Remove o prefixo 'x' e converte para int
        return turma_uc_map.get(turma_id, None)
    except (IndexError, ValueError):
        logging.error(f"Formato de variável inválido: {var_name}")
        return None

def generate_lp_file(pickle_file, output_file):
    """
    Gera um ficheiro LP (Linear Programming) a partir dos dados contidos num ficheiro pickle, 
    incluindo variáveis e restrições necessárias para resolver um problema de alocação de horários.
    
    Args:
        pickle_file (str): Caminho do ficheiro pickle contendo dados de entrada para geração de horários.
        output_file (str): Caminho do ficheiro de saída LP onde será escrito o modelo de programação linear.
    """
    # Carrega os dados do ficheiro pickle para gerar horários
    with open(pickle_file, 'rb') as f:
        loaded_data = pickle.load(f)

    # Extrai as tabelas principais dos dados carregados
    turmas = loaded_data['turmas']
    salas = loaded_data['salas']
    periodos = loaded_data['periodos']
    uc_sala = loaded_data['uc_sala']
    indisponibilidade_docente = loaded_data['indisponibilidade_docente']
    unidades_curriculares = loaded_data.get('unidades_curriculares', None)

    # Define restrições específicas entre UCs
    restricoes_entre_ucs = {
        3: [5],  # UC3 não pode coincidir com UC5
        5: [3],
        11: [13, 14, 15],
        13: [11],
        14: [11],
        15: [11],
        22: [23],
        23: [22]
        
          
        # Adicione mais UCs e suas restrições conforme necessário
    }

    # Verifica se 'unidades_curriculares' está presente para obter o semestre
    if unidades_curriculares is not None:
        # Verifique se a coluna 'semestre' está presente
        if 'semestre' in unidades_curriculares.columns:
            uc_semestre = dict(zip(unidades_curriculares['id'], unidades_curriculares['semestre']))
            logging.info("Mapeamento de 'semestre' para cada UC:")
            for uc_id, semestre in uc_semestre.items():
                logging.info(f"  UC {int(uc_id)}: Semestre {int(semestre)}")
        else:
            logging.error("A coluna 'semestre' não foi encontrada em 'unidades_curriculares'.")
            logging.error("Por favor, adicione uma coluna 'semestre' que identifique o semestre de cada UC.")
            return
    else:
        logging.error("Dados de 'unidades_curriculares' não encontrados no ficheiro pickle.")
        return

    # Dicionários auxiliares para organizar informações por turma
    turma_set = sorted(turmas['id'].unique())
    turma_docente = dict(zip(turmas['id'], turmas['docente_id']))
    turma_duracao = dict(zip(turmas['id'], turmas['duracao']))
    turma_tipo = dict(zip(turmas['id'], turmas['tipo_aula_id']))
    turma_uc = dict(zip(turmas['id'], turmas['unidade_curricular_id']))
    turma_label = dict(zip(turmas['id'], turmas['turma_label_id']))

    # Mapeia cada turma às salas permitidas com base na unidade curricular
    turma_sala = {
        turma_id: list(set(uc_sala[uc_sala['unidade_curricular_id'] == turma_uc[turma_id]]['sala_id']))
        for turma_id in turma_set
    }

    # Mapeia períodos indisponíveis de cada docente por turma
    I_d_turma = {
        turma_id: indisponibilidade_docente[indisponibilidade_docente['docente_id'] == turma_docente[turma_id]]['periodo_horario_id'].tolist()
        for turma_id in turma_set
    }

    # Cria o ficheiro LP e escreve a função objetivo e restrições
    with open(output_file, 'w') as lp_file:
        # Define a função objetivo para minimizar o uso das salas
        lp_file.write("Minimize\n obj: " + " + ".join([f"z_{int(sala_id)}" for sala_id in salas['id']]) + "\n")
        lp_file.write("\nSubject To\n")

        # Estruturas para controlar conflitos e variáveis de uso
        docente_periodo_vars = {}
        sala_periodo_vars = {}
        variables = set()
        z_variables = {f"z_{int(sala_id)}" for sala_id in salas['id']}
        written_constraints = set()

        # Dicionários para rastrear ocupação de períodos para cada sala e docente
        sala_periodos_ocupados = {}
        docente_periodos_ocupados = {}

        # Grupos de turmas organizados por unidade curricular (UC) e tipo de aula
        uc_groups = {}
        for turma_id, uc_id in turma_uc.items():
            tipo_aula = turma_tipo[turma_id]
            uc_groups.setdefault(uc_id, {'early': [], 'late': []})
            if tipo_aula in [1, 2]:  # Tipos de aula 'early'
                uc_groups[uc_id]['early'].append(turma_id)
            else:  # Tipos de aula 'late'
                uc_groups[uc_id]['late'].append(turma_id)

        # Processamento de cada turma para alocação em períodos e salas disponíveis
        for turma_id in turma_set:
            docente_id = turma_docente.get(turma_id)
            duracao = turma_duracao.get(turma_id)
            salas_permitidas = turma_sala.get(turma_id, [])
            label_id = turma_label.get(turma_id, 0)
            uc_id = turma_uc.get(turma_id)
            semestre_atual = uc_semestre.get(uc_id, None)

            # Verifica se o semestre está disponível
            if semestre_atual is None:
                logging.warning(f"Semestre não encontrado para UC_id {int(uc_id)} da turma {int(turma_id)}.")
                continue

            # Log da turma que está a ser processada
            logging.info(f"Processando Turma {int(turma_id)} (UC {int(uc_id)}, Semestre {int(semestre_atual)}) com duração {int(duracao)}, salas permitidas {salas_permitidas}, label_id {int(label_id)}")

            # Verifica se há salas disponíveis
            if not salas_permitidas:
                logging.warning(f"Nenhuma sala permitida encontrada para turma: {int(turma_id)}")
                continue

            turma_agendada = False  # Flag para verificar se a turma foi alocada

            # Tenta alocar a turma em salas e períodos válidos
            for sala_id in salas_permitidas:
                periodos_ordenados = sorted(periodos['id'].unique())

                # Encontra sequências de períodos consecutivos de acordo com a duração
                for start_period_index in range(len(periodos_ordenados) - duracao + 1):
                    sequencia_periodos = periodos_ordenados[start_period_index:start_period_index + duracao]

                    # Verifica se os períodos são consecutivos
                    if not all(sequencia_periodos[i] + 1 == sequencia_periodos[i + 1] for i in range(duracao - 1)):
                        continue

                    # Verifica conflitos de sala e disponibilidade de docente
                    if (any((int(sala_id), int(p)) in sala_periodos_ocupados for p in sequencia_periodos) or \
                       any(int(p) in I_d_turma.get(turma_id, []) for p in sequencia_periodos) or \
                       any((int(docente_id), int(p)) in docente_periodos_ocupados for p in sequencia_periodos)):
                        continue

                    # Implementação da restrição para label_id=1
                    if int(label_id) == 1:
                        conflito_semestre = False
                        for p in sequencia_periodos:
                            # Verifica se há alguma turma já alocada nesse período
                            turmas_no_periodo = [t for (s, per), t in sala_periodos_ocupados.items() if per == int(p)]
                            for turma_alocada in turmas_no_periodo:
                                uc_alocada = turma_uc.get(turma_alocada)
                                semestre_alocada = uc_semestre.get(uc_alocada, None)
                                if semestre_alocada == semestre_atual:
                                    conflito_semestre = True
                                    logging.info(f"Conflito de semestre encontrado: Turma {int(turma_id)} (Semestre {int(semestre_atual)}) não pode ser alocada no período {int(p)} devido à Turma {int(turma_alocada)} (Semestre {int(semestre_atual)})")
                                    break
                            if conflito_semestre:
                                # Se a UC problemática está sendo processada, adicione logs específicos
                                if int(uc_id) in restricoes_entre_ucs:
                                    for uc_restrita in restricoes_entre_ucs[uc_id]:
                                        logging.error(f"Restrição de semestre impedindo a alocação da Turma {int(turma_id)} (UC {int(uc_id)}) no período {[int(p) for p in sequencia_periodos]}.")
                                break
                        if conflito_semestre:
                            continue  # Passa para o próximo período

                    # Adicionar restrições específicas entre UCs (UC3, UC5, UC11, UC13, UC14, UC15, UC22, UC23)
                    if uc_id in restricoes_entre_ucs:
                        ucs_restritas = restricoes_entre_ucs[uc_id]
                        conflito_entre_ucs = False
                        for uc_restrita in ucs_restritas:
                            turmas_restritas = turmas[turmas['unidade_curricular_id'] == uc_restrita]['id'].tolist()
                            for turma_restrita_id in turmas_restritas:
                                # Verifica se a turma restrita está alocada nos períodos desejados
                                for p in sequencia_periodos:
                                    # Encontra turmas restritas alocadas no mesmo período
                                    for sala_restrita_id in salas['id']:
                                        if (sala_restrita_id, int(p)) in sala_periodos_ocupados and sala_periodos_ocupados[(sala_restrita_id, int(p))] == turma_restrita_id:
                                            conflito_entre_ucs = True
                                            logging.info(f"Conflito entre UC{uc_id} e UC{uc_restrita} encontrado: Turma {int(turma_id)} não pode ser alocada no período {int(p)} devido à Turma {int(turma_restrita_id)}.")
                                            break
                            if conflito_entre_ucs:
                                break
                        if conflito_entre_ucs:
                            continue  # Passa para o próximo período

                    # Aloca a turma e atualiza registros de ocupação
                    uc_variables = []
                    for period_id in sequencia_periodos:
                        var_name = f"x{int(turma_id)}_{int(sala_id)}_{int(period_id)}"
                        variables.add(var_name)
                        uc_variables.append(var_name)

                        # Marca períodos como ocupados pela sala e docente
                        sala_periodos_ocupados[(int(sala_id), int(period_id))] = turma_id
                        docente_periodos_ocupados[(int(docente_id), int(period_id))] = turma_id

                        # Regista variáveis para evitar conflitos
                        docente_periodo_vars.setdefault((int(docente_id), int(period_id)), []).append(var_name)
                        sala_periodo_vars.setdefault((int(sala_id), int(period_id)), []).append(var_name)

                    # Gera restrições de continuidade
                    if uc_variables:
                        var_start = uc_variables[0]
                        for var in uc_variables[1:]:
                            cont_constraint = f"cont_{var_start}_{var}: {var_start} - {var} = 0\n"
                            if cont_constraint not in written_constraints:
                                lp_file.write(cont_constraint)
                                written_constraints.add(cont_constraint)

                    # Gera restrição para alocação completa da turma
                    if uc_variables:
                        allocation_constraint = f"turma_{int(turma_id)}_agendada: " + " + ".join(uc_variables) + f" = {int(duracao)}\n"
                        if allocation_constraint not in written_constraints:
                            lp_file.write(allocation_constraint)
                            written_constraints.add(allocation_constraint)

                        # Vincula uso da sala à variável binária `z_sala_id`
                        sala_constraint = f"sala_{int(sala_id)}_uso_para_turma_{int(turma_id)}: " + " + ".join(uc_variables) + f" - {int(duracao)} z_{int(sala_id)} <= 0\n"
                        if sala_constraint not in written_constraints:
                            lp_file.write(sala_constraint)
                            written_constraints.add(sala_constraint)

                    turma_agendada = True  # Marca a turma como alocada
                    logging.info(f"Turma {int(turma_id)} agendada na sala {int(sala_id)} nos períodos {[int(p) for p in sequencia_periodos]}")
                    break  # Sai do loop de períodos

                if turma_agendada:
                    break  # Sai do loop de salas

            # Informa caso a turma não possa ser alocada
            if not turma_agendada:
                # Se a UC problemática está sendo processada, adicione logs específicos
                if int(uc_id) in restricoes_entre_ucs:
                    logging.error(f"*** Falha na alocação para Turma {int(turma_id)} (UC {int(uc_id)}) ***")
                else:
                    logging.error(f"*** Nenhuma sequência válida encontrada para turma: {int(turma_id)} ***")

        # **Adicionar Restrições Específicas Entre UC3, UC5, UC11, UC13, UC14, UC15, UC22, UC23**
        # Para cada período, garantir que no máximo uma turma entre essas UCs seja alocada
        logging.info("Adicionando restrições específicas entre UC3, UC5, UC11, UC13, UC14, UC15, UC22, UC23 para evitar sobreposição.")
        for p in periodos['id'].unique():
            # Coletar todas as variáveis x para as UCs restritivas neste período
            turmas_uc3 = turmas[turmas['unidade_curricular_id'] == 3]['id'].tolist()
            turmas_uc5 = turmas[turmas['unidade_curricular_id'] == 5]['id'].tolist()
            turmas_uc11 = turmas[turmas['unidade_curricular_id'] == 11]['id'].tolist()
            turmas_uc13 = turmas[turmas['unidade_curricular_id'] == 13]['id'].tolist()
            turmas_uc14 = turmas[turmas['unidade_curricular_id'] == 14]['id'].tolist()
            turmas_uc15 = turmas[turmas['unidade_curricular_id'] == 15]['id'].tolist()
            turmas_uc22 = turmas[turmas['unidade_curricular_id'] == 22]['id'].tolist()
            turmas_uc23 = turmas[turmas['unidade_curricular_id'] == 23]['id'].tolist()

            # Variáveis reais alocadas para as UCs restritivas no período p
            vars_uc3_p_real = [f"x{turma_id}_{sala_id}_{int(p)}" for turma_id in turmas_uc3 for sala_id in salas['id']]
            vars_uc5_p_real = [f"x{turma_id}_{sala_id}_{int(p)}" for turma_id in turmas_uc5 for sala_id in salas['id']]
            vars_uc11_p_real = [f"x{turma_id}_{sala_id}_{int(p)}" for turma_id in turmas_uc11 for sala_id in salas['id']]
            vars_uc13_p_real = [f"x{turma_id}_{sala_id}_{int(p)}" for turma_id in turmas_uc13 for sala_id in salas['id']]
            vars_uc14_p_real = [f"x{turma_id}_{sala_id}_{int(p)}" for turma_id in turmas_uc14 for sala_id in salas['id']]
            vars_uc15_p_real = [f"x{turma_id}_{sala_id}_{int(p)}" for turma_id in turmas_uc15 for sala_id in salas['id']]
            vars_uc22_p_real = [f"x{turma_id}_{sala_id}_{int(p)}" for turma_id in turmas_uc22 for sala_id in salas['id']]
            vars_uc23_p_real = [f"x{turma_id}_{sala_id}_{int(p)}" for turma_id in turmas_uc23 for sala_id in salas['id']]

            # Escrever as restrições no LP
            if vars_uc3_p_real or vars_uc5_p_real:
                restricao = " + ".join(vars_uc3_p_real + vars_uc5_p_real) + " <= 1\n"
                constraint_name = f"restricao_UC3_UC5_periodo_{int(p)}"
                lp_file.write(f"{constraint_name}: {restricao}")

            if vars_uc11_p_real or vars_uc13_p_real:
                restricao = " + ".join(vars_uc11_p_real + vars_uc13_p_real) + " <= 1\n"
                constraint_name = f"restricao_UC11_UC13_periodo_{int(p)}"
                lp_file.write(f"{constraint_name}: {restricao}")
            
            if vars_uc11_p_real or vars_uc14_p_real:
                restricao = " + ".join(vars_uc11_p_real + vars_uc14_p_real) + " <= 1\n"
                constraint_name = f"restricao_UC11_UC14_periodo_{int(p)}"
                lp_file.write(f"{constraint_name}: {restricao}")

            if vars_uc11_p_real or vars_uc15_p_real:
                restricao = " + ".join(vars_uc11_p_real + vars_uc15_p_real) + " <= 1\n"
                constraint_name = f"restricao_UC11_UC15_periodo_{int(p)}"
                lp_file.write(f"{constraint_name}: {restricao}")

            if vars_uc22_p_real or vars_uc23_p_real:
                restricao = " + ".join(vars_uc22_p_real + vars_uc23_p_real) + " <= 1\n"
                constraint_name = f"restricao_UC22_UC23_periodo_{int(p)}"
                lp_file.write(f"{constraint_name}: {restricao}")

        # Gera restrições de ordem para tipos de aula da mesma UC
        for uc_id, groups in uc_groups.items():
            for early_turma_id in groups['early']:
                for late_turma_id in groups['late']:
                    lp_file.write(f"ordem_uc_{int(uc_id)}_turma_{int(early_turma_id)}_{int(late_turma_id)}: end_{int(early_turma_id)} - start_{int(late_turma_id)} <= -1\n")

        # Adiciona restrições para evitar conflitos de períodos entre docentes e salas
        for key, vars_in_period in docente_periodo_vars.items():
            if len(vars_in_period) > 1:
                # Filtra as variáveis que não pertencem a UC_id 24 ou 25
                vars_filtered = [var for var in vars_in_period if turma_uc[int(var.split('_')[0][1:])] not in [24, 25]]
                if len(vars_filtered) > 1:
                    lp_file.write(f"docente_{int(key[0])}_periodo_{int(key[1])}_conflito: " + " + ".join(vars_filtered) + " <= 1\n")

        for key, vars_in_period in sala_periodo_vars.items():
            if len(vars_in_period) > 1:
                # Filtra as variáveis que não pertencem a UC_id 24 ou 25
                vars_filtered = [var for var in vars_in_period if turma_uc[int(var.split('_')[0][1:])] not in [24, 25]]
                if len(vars_filtered) > 1:
                    lp_file.write(f"sala_{int(key[0])}_periodo_{int(key[1])}_conflito: " + " + ".join(vars_filtered) + " <= 1\n")

        # **Correção Aqui: Incluir todas as variáveis x como binárias**
        # Declara variáveis binárias e finaliza o ficheiro LP
        lp_file.write("\nBinary\n")
        for var in sorted(variables):
            lp_file.write(f" {var}\n")
        for z_var in sorted(z_variables):
            lp_file.write(f" {z_var}\n")
        lp_file.write("End\n")


if __name__ == "__main__":
    # Diretório para armazenar os arquivos LP
    lp_directory = os.path.join(os.getcwd(), 'LPFiles')

    # Garantir que o diretório LPFiles existe
    os.makedirs(lp_directory, exist_ok=True)

    # Definição dos caminhos para os ficheiros de entrada e saída
    pickle_impares = os.path.join(os.getcwd(), 'coronetMatrices_impares.pkl')
    pickle_pares = os.path.join(os.getcwd(), 'coronetMatrices_pares.pkl')
    lp_impares = os.path.join(lp_directory, 'schedule_impares.lp')
    lp_pares = os.path.join(lp_directory, 'schedule_pares.lp')

    # Geração dos ficheiros LP para todas as UCs
    logging.info("Gerando ficheiro LP para todas as UCs.")
    generate_lp_file(pickle_impares, lp_impares)
    generate_lp_file(pickle_pares, lp_pares)