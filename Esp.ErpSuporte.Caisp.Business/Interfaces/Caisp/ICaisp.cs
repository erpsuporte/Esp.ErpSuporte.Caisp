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
        List<ContatosModel> buscarContatos(); //Funcionando
        List<DocModel> buscarDoc(DateTime DataInicio,DateTime DataFim); //Funcionando
        List<EntregasDiaModel> buscarEntregasDia(EntregasDiaBuscarModel request); //Funcionando
        List<Eventos> buscarEventos();
        FinanceiroModel buscarFinanceiro(); //Funcionando
        List<CardModel> buscarCard();
        UserInfoModel buscarUserInfo();
        List<ProgramacaoModel> buscarProgramacao(ProgramacaoBsucarModel request);
        ResponseModel enviarSac(SacModelPost request);
        List<SacModelGet> buscarSac();
        List<EntregasItensModel> buscarEntregasPeriodo(EntregasPeriodoBuscaModel request);
    }
}
