using ScoreRebanho.Application;
using AutoMapper;
namespace ScoreRebanho.Web.Models;

public sealed class SimulacaoProfile : Profile
{
    public SimulacaoProfile()
    {
        CreateMap<SimulacaoVm, EntradaScoreDto>().ReverseMap();
        CreateMap<ResumoScoreDto, ResumoVm>();
        CreateMap<ItemDto, ItemVm>(); CreateMap<AlertaDto, AlertaVm>();
        CreateMap<ResultadoScoreDto, ResultadoVm>().ForMember(x => x.Entrada, o => o.Ignore());
    }
}
