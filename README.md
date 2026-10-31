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

## Verificação neste ambiente

Domain e Application compilados. Testes dessas duas camadas executados. A restauração completa depende do pacote `Microsoft.EntityFrameworkCore.Sqlite` 10.0.13, ausente do cache local; a rede deste ambiente bloqueou api.nuget.org. Por isso o build Web/Infrastructure, execução da migration, testes integrados e inspeção visual ainda precisam ser concluídos em ambiente com esse pacote disponível.

Se o ambiente restringir a configuração NuGet do perfil do usuário, use temporariamente `$env:APPDATA` apontando para uma pasta local gravável, mantendo `--configfile NuGet.Config`. Isso não é necessário em uma máquina normalmente configurada.
