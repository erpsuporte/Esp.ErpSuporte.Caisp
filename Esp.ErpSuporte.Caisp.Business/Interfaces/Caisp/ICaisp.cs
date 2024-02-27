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
        List<ContatosModel> buscarContatos(); //-TABELA -UI -API Funcionando - TESTE
        List<DocModel> buscarDoc(string DataInicio, string DataFim); //-TABELA -UI -API Funcionando - TESTE
        List<EntregasDiaModel> buscarEntregasDia(EntregasDiaBuscarModel request); // -API Funcionando -TESTE
        List<Eventos> buscarEventos(); //-TABELA -UI -API - Funcionando -TESTE
        FinanceiroModel buscarFinanceiro(); //-API Funcionando - TESTAR
        List<CardModel> buscarCard();
        UserInfoModel buscarUserInfo();
        List<ProgramacaoModel> buscarProgramacao(ProgramacaoBuscarModel request);// - TABELA BUILDER -UI WEB -DEV API -TESTE
        ResponseModel enviarSac(SacModelPost request); // TABELA BUILDER SAC  INSERIR REGISTRO API
        List<SacModelGet> buscarSac(); // BUSCAR SAC DO USUARIO LOGADO API -GESTÂO ESTADO WEB  /DATA RECEBIDA /REPOSTA? /QUEM RECEBEU
        List<EntregasItensModel> buscarEntregasPeriodo(EntregasPeriodoBuscaModel request); // - DEV API SUM ENTREGAS SOMAS DOS ITENS DO PEIRODO DE DIAS - ROTA COM DATAINICIO E DATA FIM DA REQUESIÃO STIMULSOFT GERANDO O PDF -TESTE
    }
}
