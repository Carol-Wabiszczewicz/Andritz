# ANDRITZ Dev Assessment

Na implementação atual, dentro da classe genérica Graph<T> já contida no código, a principal melhoria foi a adição da funcionalidade para encontrar rotas entre dois nós específicos no grafo.

Introduzi um dicionário interno _graph, que mapeia cada nó para uma lista de seus nós adjacentes.
Esse dicionário permite representar eficientemente a estrutura de um grafo direcionado, facilitando a adição e a busca de vizinhos de cada nó.

Dentro da classe Graph<T> foi implementado um aceite para uma coleção de links (IEnumerable<ILink<T>>). 
Durante a inicialização, o construtor preenche o dicionário _graph com base nesses links. 
Isso garante que todos os nós sejam inclusos e que as conexões entre eles sejam certas.

Implementei dentro do método RoutesBetween uma forma de encontrar todas as rotas possíveis entre dois nós. 
Esse método utiliza o Observable.Create para retornar um IObservable<IEnumerable<T>>, o que permite a emissão de rotas de forma assíncrona e reativa.

O método DepthFirstSearch realiza a DFS, adicionando nós ao caminho atual e verificando se o nó final foi alcançado.
Se sim, o caminho é adicionado à lista de caminhos encontrados.
O algoritmo garante que cada nó seja visitado apenas uma vez por caminho e remove o nó do conjunto de visitados ao final da exploração.
