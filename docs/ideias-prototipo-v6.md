# Ideias extraídas do protótipo Risco do Rebanho v6

Referência analisada: `C:\Users\victo\Downloads\risco-do-rebanho-dev-v6.html` (31/10/2026). O protótipo é uma demonstração autocontida em HTML/CSS/JavaScript. Suas telas e integrações representam possibilidades de produto; não são fonte validada de regras, cotações ou previsões.

## O que já existe no projeto

- Formulário de terreno, lote e manejo com validação no servidor.
- Relatório de score, custos, alertas, premissas e cenário de preço menor.
- Cadastro/login Identity, histórico no SQLite e consulta administrativa protegida por papel.
- Campo de arrendamento condicional e tema verde responsivo.

## Melhorias recomendadas para o fluxo atual

1. **Análise do terreno no mapa** — localizar a propriedade por cidade ou posição atual, desenhar o contorno e estimar a área. Preencher a área do formulário, mantendo entrada manual se GPS, mapa ou conexão falharem. Pedir geolocalização após ação explícita; explicar finalidade e não guardar coordenadas sem necessidade.
2. **Fluxo guiado com progresso** — transformar o formulário em etapas (localização, terreno, lote, manejo), com indicador de progresso, validação ao avançar e retorno fácil. Preservar dados entre etapas e em erros.
3. **Resumo antes do cálculo** — exibir área, animais, UA e ocupação estimadas antes de gerar o relatório.
4. **Resultado mais visual** — medidor circular de score, barras dos componentes, KPIs resumidos e alertas ordenados por prioridade. Manter detalhes, unidades e explicações para auditoria.
5. **Acompanhamento de rebanhos** — salvar simulação com nome e propriedade, abrir/editar, recalcular como nova versão, comparar resultados e excluir de modo controlado. Hoje os scores são gravados, mas não há conceito de propriedade ou interface de acompanhamento.
6. **Mercado da semana** — catálogo por categoria (gado, suplementação, saúde, pasto e cercas), com unidade, preço atual/anterior, data e direção da variação. Identificar fonte e região; dados demonstrativos não podem parecer cotações reais.
7. **Alertas e recomendações** — destacar “o que fazer agora” e ligar cada recomendação ao fator calculado. Separar recomendações do domínio de avisos climáticos e sugestões de compra.
8. **Acessibilidade e preferências** — modo claro/escuro, preferência do sistema, áreas de toque confortáveis, foco visível, navegação por teclado e informação que não dependa só de cor.

## Funcionalidades novas, dependências e decisões

### Localização e mapa

- Biblioteca de mapa/tiles, busca de endereço e desenho de polígono.
- Definir retenção, precisão, consentimento, fornecedor e comportamento sem rede. Coordenadas são dados sensíveis; não gravá-las no histórico de scores por padrão.
- Definir se a área do polígono substitui a área digitada ou apenas sugere valor e como excluir áreas impróprias.

### Clima

- O protótipo consulta Open-Meteo para previsão/elevação e deriva alertas para chuva, frio/geada, calor, vento e seca.
- Validar região, unidades, horizonte, frequência, limites, licença/atribuição, cache, indisponibilidade e textos com especialista. Exibir previsão separada do score produtivo e informar atualização.
- Não mostrar “sem alertas” em caso de falha do provedor. Distinguir indisponível de condição segura. Mensagens do protótipo são hipóteses, não aconselhamento veterinário definitivo.

### Mercado e administração

- Substituir preços estáticos e armazenamento local por fonte identificada ou cadastro administrativo persistente, com histórico e unidade/região.
- Criar tela protegida para editar preço/data/fonte, validar valores e registrar alterações. Não copiar credenciais do protótipo nem confiar em checagens JavaScript.
- O painel do protótipo inclui gestão de usuários e premissas. O projeto atual autoriza o administrador a consultar scores; gestão de contas, parâmetros e mercado requer definir permissões e trilha de auditoria.

### IA

- O protótipo sugere análise por IA com fallback simulado. Definir provedor, custo, dados enviados, consentimento, proteção da propriedade e tratamento de falha antes de implementar.
- O cálculo determinístico continua sendo a fonte do score. Texto gerado explica os dados sem alterar pontuação ou prometer crédito/lucro. Nunca apresentar exemplo como análise real.

## Prioridade sugerida

- **P1 — UX sem dependências externas:** progresso do formulário, resumo de ocupação, score visual e alertas prioritários.
- **P2 — valor operacional:** salvar/nomear propriedades, reabrir/editar, recalcular e comparar versões, com histórico claro.
- **P3 — integrações:** mapa/localização, seguido de clima com cache e falhas explícitas.
- **P4 — mercado e IA:** depois de escolher fontes, permissões, custos e critérios de confiança.

## Lacunas do protótipo a não copiar

- O mercado usa preços de exemplo e `localStorage`; clima depende de chamadas diretas do navegador. Isso não assegura proveniência, persistência entre usuários, controle administrativo ou confiabilidade.
- Um HTML mistura simulação, clima, mercado, administração e IA. No produto, manter cálculo no Domain/Application, integrações na Infrastructure e autorização no servidor.
- A nomenclatura e as faixas de risco do protótipo podem divergir das do score atual. Manter uma política única até validação de produto.
- Coordenadas, preços e dados de usuários no navegador não substituem histórico persistente com controle de acesso.

## Implementação P1 — 31/10/2026

- Fluxo progressivo: terreno, lote, manejo e resumo; a etapa de localização aguarda a escolha do mapa. Sem JavaScript, todos os campos e ações continuam disponíveis.
- Resumo validado na Application usa o mesmo cálculo do relatório, devolve somente área, animais, UA, capacidade e ocupação, e não persiste simulação. O relatório completo só aparece ao gerar score.
- Medidor circular, barras acessíveis dos componentes e KPIs; detalhes e premissas preservados.
- Ordem de atenção adotada: prejuízo, superlotação, sanidade, indicadores indisponíveis, margem, rotação e subutilização. É uma ordem de apresentação, sem alteração nas fórmulas ou pesos; não é classificação clínica.
- Tema claro/escuro/sistema, com apenas a preferência visual no armazenamento local. Nenhuma localização coletada.
- P2–P4 continuam propostas: propriedade/versionamento/exclusão precisam de regras de acesso e retenção; mapas, clima, mercado e IA dependem das decisões descritas acima. Nenhum dado demonstrativo foi apresentado como integração real.
