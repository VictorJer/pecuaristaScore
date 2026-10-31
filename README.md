# Score do Rebanho

MVP ASP.NET Core MVC (.NET 10), com Identity/EF Core SQLite, Bootstrap, AutoMapper e FluentResults.

## Executar

```powershell
dotnet restore --configfile NuGet.Config
dotnet build ScoreRebanho.sln
dotnet test ScoreRebanho.sln
dotnet run --project src/ScoreRebanho.Web -- --migrate
dotnet run --project src/ScoreRebanho.Web
```

Execute a migration antes do primeiro acesso. O banco `score.db` é criado no diretório de execução da Web. Configure `ConnectionStrings__DefaultConnection` para usar caminho absoluto em produção. Faça backup do banco antes de novas migrations. Não se usa EnsureCreated nem migration automática ao atender requisições.

Cadastro, login, logout e alteração de senha estão disponíveis. Não há confirmação/recuperação de senha por e-mail, papéis de negócio nem histórico de simulações. Use HTTPS em produção e persista as chaves de Data Protection quando hospedar em contêiner.

## Premissas

Valores iniciais ficam em `Premissas` no Domain; podem ser sobrescritos pela seção `Premissas` em appsettings ou variáveis de ambiente. Incremente `Versao` quando alterar valores. O relatório inclui cópia dos parâmetros efetivamente usados. Fórmulas estão isoladas em `Calculadora`. Convenções aprovadas estão na seção 13 da SPEC.

AutoMapper 16.2.0: configure `AutoMapper__LicenseKey` conforme a licença aplicável ao projeto; não inclua chaves no repositório.

## Verificação

Build completo sem avisos e 15 testes MSTest aprovados (Domain, Application, mapeamentos AutoMapper e migration SQLite). A aplicação está disponível em http://localhost:5020 com o perfil `http`.

O visual usa Bootstrap local com tema em `wwwroot/css/site.css`. O formulário tem parciais por seção, e o cálculo separa composição de custos e alertas. As fórmulas e convenções de negócio foram preservadas.

A inspeção visual automatizada não foi concluída porque o host de acesso ao navegador estava indisponível. Login, cadastro e CSS foram verificados por HTTP.

Em ambientes com restrição ao Event Log do Windows, execute com `$env:Logging__EventLog__LogLevel__Default='None'` para manter os logs no console. As chaves de Data Protection podem ser temporárias nesse ambiente; reiniciar o servidor exige novo login.

## Área administrativa

Novos scores são persistidos e consultáveis em `/Administracao` por contas com papel `Administrador`. Aplique a migration antes de executar a nova versão. Para promover uma conta cadastrada, execute no terminal do servidor:

```powershell
$env:AdministradorEmail='email-da-conta-cadastrada'
dotnet run --project src/ScoreRebanho.Web -- --administrador
Remove-Item Env:AdministradorEmail
```

A conta deve sair e entrar novamente após receber o papel. Nunca configure senha padrão ou promoção automática por cadastro. Relatórios históricos preservam os resultados originais e não são recalculados ao consultar.

## Meus rebanhos

A home (`/` ou `/Rebanhos`) permite cadastrar, consultar, editar e excluir os rebanhos da conta autenticada. Nome e propriedade identificam cada cadastro. Salvar recalcula o score e mantém um snapshot administrativo; excluir remove o cadastro após confirmação, preservando esses snapshots. Simulações avulsas anteriores continuam no histórico administrativo e não são importadas automaticamente.

Antes de iniciar esta versão, aplique também a migration `CadastroRebanhos` com o comando `--migrate` acima. Os testes de CRUD usam SQLite em memória e verificam isolamento entre contas, concorrência, paginação e manutenção do histórico.
