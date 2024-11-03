import pandas as pd
import pickle
import os
import sys
import datetime
import logging

# Configuração do logging para registar informações e erros durante o processamento
logging.basicConfig(level=logging.INFO, format='%(levelname)s: %(message)s')

def importar_dados():
    """
    Importa os dados a partir de ficheiros Excel localizados na pasta 'Tabelas_Excel'.
    
    Returns:
        dict: Um dicionário contendo DataFrames para cada tabela importada, 
        onde cada chave representa o nome da tabela.
        
    Raises:
        FileNotFoundError: Caso algum dos ficheiros esperados não seja encontrado.
    """
    pasta = os.path.join(os.getcwd(), 'Tabelas_Excel')
    
    ficheiros = {
        'unidades_curriculares': 'Unidades_Curriculares.xlsx',
        'uc_sala': 'UC_SALA.xlsx',
        'turmas': 'TURMA.xlsx',
        'docentes': 'Docentes.xlsx',
        'salas': 'Salas.xlsx',
        'periodos': 'Periodos_Horarios.xlsx',
        'tipo_aula': 'Tipos_Aula.xlsx',
        'uc_turma_label': 'UC_TurmaLabel.xlsx',  
        'uc_duracao': 'UC_Duracao.xlsx',  
        'uc_docente': 'UC_Docente.xlsx',
        'indisponibilidade_docente': 'Indisponibilidade_Docentes.xlsx',
        'indisponibilidade_sala': 'Indisponibilidade_Salas.xlsx'
    }
    
    dados = {}
    
    for chave, ficheiro in ficheiros.items():
        caminho = os.path.join(pasta, ficheiro)
        if not os.path.exists(caminho):
            raise FileNotFoundError(f"Ficheiro não encontrado: {caminho}")
        dados[chave] = pd.read_excel(caminho)
        logging.info(f"Ficheiro '{ficheiro}' importado com sucesso.")
    
    return dados

def verificar_colunas(dados):
    """
    Verifica se todas as tabelas importadas contêm as colunas necessárias para o processamento.
    
    Args:
        dados (dict): Dicionário com tabelas importadas.
    
    Raises:
        Exception: Se uma coluna ou tabela obrigatória estiver ausente.
    """
    required_columns = {
        'unidades_curriculares': ['id', 'nome', 'curso_id', 'semestre', 'carga_horaria_teorica', 'carga_horaria_pratica', 'numero_turmas_teoricas', 'numero_turmas_praticas'],
        'uc_sala': ['unidade_curricular_id', 'sala_id'],
        'turmas': ['id', 'unidade_curricular_id', 'docente_id', 'tipo_aula_id', 'duracao', 'turma_label'],
        'docentes': ['id', 'Nome'],
        'salas': ['id', 'nome', 'capacidade'],
        'periodos': ['id', 'descricao', 'dia_semana', 'hora_inicio', 'hora_fim'],
        'tipo_aula': ['id', 'descricao'],
        'uc_turma_label': ['unidade_curricular_id', 'turma_label'],
        'uc_duracao': ['unidade_curricular_id', 'duracao'],
        'uc_docente': ['unidade_curricular_id', 'docente_id'],
        'indisponibilidade_docente': ['docente_id', 'periodo_horario_id'],
        'indisponibilidade_sala': ['sala_id', 'periodo_horario_id']
    }
    
    for tabela, colunas in required_columns.items():
        if tabela not in dados:
            raise Exception(f"Tabela '{tabela}' não encontrada nos dados importados.")
        for coluna in colunas:
            if coluna not in dados[tabela].columns:
                raise Exception(f"Coluna '{coluna}' não encontrada na tabela '{tabela}'.")
    
    logging.info("Todas as tabelas possuem as colunas necessárias.")

def preparar_dados(dados):
    """
    Prepara os dados para o agendamento, incluindo junções, mapeamento de labels e filtragem por semestre.
    
    Args:
        dados (dict): Dicionário com tabelas importadas.
    
    Returns:
        tuple: Dados preparados para semestres ímpares e pares.
    
    Raises:
        Exception: Se forem encontrados 'turma_label' sem mapeamento definido.
    """
    turmas = dados['turmas']
    
    # Junções para associar labels, durações e docentes
    turmas = turmas.merge(dados['uc_turma_label'], on='unidade_curricular_id', how='left', suffixes=('', '_label'))
    turmas = turmas.merge(dados['uc_duracao'], on='unidade_curricular_id', how='left', suffixes=('', '_duracao'))
    turmas = turmas.merge(dados['uc_docente'], on='unidade_curricular_id', how='left', suffixes=('', '_docente'))
    turmas = turmas.merge(dados['unidades_curriculares'][['id', 'semestre']], left_on='unidade_curricular_id', right_on='id', how='left', suffixes=('', '_uc'))

    # Remover duplicatas para garantir unicidade
    turmas = turmas.drop_duplicates(subset=['id', 'unidade_curricular_id', 'docente_id', 'tipo_aula_id', 'turma_label'])

    # Mapeamento de labels
    turma_label_to_id = {
        'TT': 1,
        'A': 2,
        'B': 3,
        'C': 4,
        'D': 5,
    }
    
    labels_unicos = set(turmas['turma_label'].unique())
    labels_nao_mapeados = labels_unicos - set(turma_label_to_id.keys())
    if labels_nao_mapeados:
        raise Exception(f"Os seguintes 'turma_label' não têm mapeamento definido: {labels_nao_mapeados}")
    
    turmas['turma_label_id'] = turmas['turma_label'].map(turma_label_to_id)
    
    # Filtrar períodos válidos
    periodos = dados['periodos']
    dias_semana_to_int = {'Segunda': 1, 'Terça': 2, 'Quarta': 3, 'Quinta': 4, 'Sexta': 5, 'Sábado': 6, 'Domingo': 7}
    periodos['dia_semana_num'] = periodos['dia_semana'].map(dias_semana_to_int)
    periodos = periodos[periodos['dia_semana_num'].isin([1, 2, 3, 4, 5])]
    
    # Converter horários para formato datetime.time
    periodos['hora_inicio'] = pd.to_datetime(periodos['hora_inicio']).dt.time
    periodos['hora_fim'] = pd.to_datetime(periodos['hora_fim']).dt.time
    hora_inicio_min = datetime.time(hour=8, minute=30)
    hora_fim_max = datetime.time(hour=18, minute=30)
    periodos = periodos[(periodos['hora_inicio'] >= hora_inicio_min) & (periodos['hora_fim'] <= hora_fim_max)]
    
    # Dividir turmas por semestre
    turmas_impares = turmas[turmas['semestre'] % 2 == 1].copy()
    turmas_pares = turmas[turmas['semestre'] % 2 == 0].copy()
    
    turma_label_ids_validos = set(turma_label_to_id.values())
    turmas_impares = turmas_impares[turmas_impares['turma_label_id'].isin(turma_label_ids_validos)]
    turmas_pares = turmas_pares[turmas_pares['turma_label_id'].isin(turma_label_ids_validos)]
    
    dados_preparados_impares = {
        'turmas': turmas_impares[['id', 'unidade_curricular_id', 'docente_id', 'tipo_aula_id', 'duracao', 'turma_label_id']],
        'salas': dados['salas'],
        'periodos': periodos,
        'docentes': dados['docentes'],
        'uc_sala': dados['uc_sala'],
        'indisponibilidade_docente': dados['indisponibilidade_docente'],
        'indisponibilidade_sala': dados['indisponibilidade_sala'],
        'turma_label_to_id': turma_label_to_id,
        'unidades_curriculares': dados['unidades_curriculares']
    }
    
    dados_preparados_pares = {
        'turmas': turmas_pares[['id', 'unidade_curricular_id', 'docente_id', 'tipo_aula_id', 'duracao', 'turma_label_id']],
        'salas': dados['salas'],
        'periodos': periodos,
        'docentes': dados['docentes'],
        'uc_sala': dados['uc_sala'],
        'indisponibilidade_docente': dados['indisponibilidade_docente'],
        'indisponibilidade_sala': dados['indisponibilidade_sala'],
        'turma_label_to_id': turma_label_to_id,
        'unidades_curriculares': dados['unidades_curriculares']
    }
    
    return dados_preparados_impares, dados_preparados_pares


def salvar_pickled_data(dados_preparados, output_file):
    """
    Salva os dados preparados num ficheiro em formato pickle.
    
    Args:
        dados_preparados (dict): Dados a serem salvos.
        output_file (str): Caminho para o ficheiro de saída.
    """
    with open(output_file, 'wb') as f:
        pickle.dump(dados_preparados, f)
    logging.info(f"Ficheiro pickle gerado: {output_file}")

if __name__ == "__main__": 
    try:
        dados = importar_dados()
        verificar_colunas(dados)
        dados_preparados_impares, dados_preparados_pares = preparar_dados(dados)
        
        output_pickle_impares = os.path.join(os.getcwd(), 'coronetMatrices_impares.pkl')
        output_pickle_pares = os.path.join(os.getcwd(), 'coronetMatrices_pares.pkl')
        
        salvar_pickled_data(dados_preparados_impares, output_pickle_impares)
        salvar_pickled_data(dados_preparados_pares, output_pickle_pares)
        
    except Exception as e:
        logging.error(f"Erro durante o processamento: {e}")
        sys.exit(1)
