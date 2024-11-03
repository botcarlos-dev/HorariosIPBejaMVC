import pickle

# Caminho para o ficheiro pickle
pickle_file = 'D:\\CS\\Projecto Final\\gerarLP\\coronetMatrices_impares.pkl'

# Carregar os dados do ficheiro pickle
with open(pickle_file, 'rb') as f:
    loaded_data = pickle.load(f)

# Exibir o tipo e o conteúdo dos dados carregados
print(f"Tipo dos dados carregados: {type(loaded_data)}")

# Se for uma tupla, mostra os elementos
if isinstance(loaded_data, tuple):
    for idx, element in enumerate(loaded_data):
        print(f"Elemento {idx}: Tipo: {type(element)} - Conteúdo: {element}")

# Se for um dicionário, mostra as chaves
elif isinstance(loaded_data, dict):
    print("Chaves do dicionário:")
    for key in loaded_data.keys():
        print(f" - {key}")

# Mostrar os primeiros elementos para inspecionar
print(f"Conteúdo completo: {loaded_data}")
