using Esp.ErpSuporte.Caisp.Business.Modelos.Caisp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Esp.ErpSuporte.Caisp.Business.Interfaces.Caisp
{
    public interface ICaisp
    {
        ResponseDocumento BuscarDocumento(RequestDocumento request);
        List<ContatosModel> buscarContatos();
        List<DocModel> buscarDoc(BuscaDocModel request);
        List<EntregasDiaModel> buscarEntregasDia(EntregasDiaBuscarModel request); 
        List<Eventos> buscarEventos(); 
        FinanceiroModel buscarFinanceiro();
        List<NotasFiscalModel> buscarNotasFicais(BuscarNotasFiscalModel request);
        List<CardModel> buscarCard();
        UserInfoModel buscarUserInfo();
        List<ProgramacaoModel> buscarProgramacao(ProgramacaoBuscarModel request);
        ResponseModel enviarSac(SacModelPost request); 
        List<SacModelGet> buscarSac(); 
        List<EntregasItensModel> buscarEntregasPeriodo(EntregasPeriodoBuscaModel request);
        string ProcessarAnalise(ProssarAnaliseModel request);

    }
}
