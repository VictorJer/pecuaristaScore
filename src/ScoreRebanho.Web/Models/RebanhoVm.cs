using System.ComponentModel.DataAnnotations;
using AutoMapper;
using ScoreRebanho.Application;

namespace ScoreRebanho.Web.Models;

public sealed class RebanhoFormularioVm
{
    public Guid? Id { get; set; }
    public Guid Versao { get; set; }
    [Required(ErrorMessage = "Informe o nome do rebanho."), StringLength(100, ErrorMessage = "Use até 100 caracteres.")]
    [Display(Name = "Nome do rebanho")]
    public string Nome { get; set; } = "";
    [Required(ErrorMessage = "Informe a propriedade."), StringLength(150, ErrorMessage = "Use até 150 caracteres.")]
    [Display(Name = "Propriedade")]
    public string Propriedade { get; set; } = "";
    public SimulacaoVm Entrada { get; set; } = new();
}
public sealed record RebanhoVm(Guid Id, Guid Versao, string Nome, string Propriedade,
    DateTime CriadoEmUtc, DateTime AtualizadoEmUtc, SimulacaoVm Entrada, ResultadoVm Resultado);
public sealed record PaginaRebanhosVm(int Pagina, bool TemProxima, IReadOnlyList<RebanhoVm> Itens);
public sealed class ExcluirRebanhoVm
{
    public Guid Id { get; set; }
    public Guid Versao { get; set; }
}
public sealed class RebanhosProfile : Profile
{
    public RebanhosProfile()
    {
        CreateMap<RebanhoFormularioVm, SalvarRebanhoDto>();
        CreateMap<RebanhoDto, RebanhoFormularioVm>();
        CreateMap<RebanhoDto, RebanhoVm>();
        CreateMap<PaginaRebanhosDto, PaginaRebanhosVm>();
    }
}
