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
