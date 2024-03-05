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
        List<DocModel> buscarDoc(string DataInicio, string DataFim); //-TABELA -UI -API Funcionando - TESTE //remover incio e fim
        //Atas --CAMPO TIPO  vem como parametro na rota
        //Estatutos
        //Circulares Normativas
        //Desenvolvimento Agricola
        //DATA VALIDADE CAMPO
        //você deseja que esse documento seja publico? //publico aberto todo mundo //privado campo tab com  filtro e preencheria K_documentousuario create

        List<EntregasDiaModel> buscarEntregasDia(EntregasDiaBuscarModel request); // -API Funcionando -TESTE
        List<Eventos> buscarEventos(); //-TABELA -UI -API - Funcionando -TESTE
        FinanceiroModel buscarFinanceiro(); //-API Funcionando - TESTAR // saldo tudo diferença de todos documentos em aberto, 3 rotas, filtro de inicio e fim dentro do app
        //saldo geral de tudo
        //Pagamentos aprovados -- entrada da mega  valor invertido mesmo select 
        //Debitos pendentes -- saida de mega valor invertido mesmo select 
        // 3 rota Consulta notas fiscais filtro no select inicio e fim
        //pagina para baixar xml


        List<CardModel> buscarCard();//Wes Pagina Card campo inativo // Tabela
        UserInfoModel buscarUserInfo();//
        List<ProgramacaoModel> buscarProgramacao(ProgramacaoBuscarModel request);// - TABELA BUILDER -UI WEB -DEV API -TESTE
        ResponseModel enviarSac(SacModelPost request); // TABELA BUILDER SAC  INSERIR REGISTRO API
        List<SacModelGet> buscarSac(); // BUSCAR SAC DO USUARIO LOGADO API -GESTÂO ESTADO WEB  /DATA RECEBIDA /REPOSTA? /QUEM RECEBEU /NOME: OUVIDORIA e EMAIL Configuravel
        List<EntregasItensModel> buscarEntregasPeriodo(EntregasPeriodoBuscaModel request); // - DEV API SUM ENTREGAS SOMAS DOS ITENS DO PEIRODO DE DIAS - ROTA COM DATAINICIO E DATA FIM DA REQUESIÃO STIMULSOFT GERANDO O PDF -TESTE


        //Componente que realiza requisição API e gerencia status, do Sac
    }
}
