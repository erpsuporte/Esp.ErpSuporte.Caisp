using Benner.Tecnologia.Common;
using Benner.Tecnologia.Common.Components;
using System;
using System.Collections.Generic;

namespace Esp.ErpSuporte.Caisp.Business.Modelos.Caisp
{
    public class ContatosModel
    {
        public int Handle { get; set; }
        public string Descricao { get; set; }
        public string Nome { get; set; }
        public string Cargo { get; set; }
        public string Telefone { get; set; }
        public string Ramal { get; set; }
        public string Whatsapp { get; set; }
    }

    public class ProgramacaoBuscarModel
    {
        public string Inicio { get; set; }
        public string Fim { get; set; }
    }

    public class DocModel
    {
        public int Handle { get; set; }
        public DateTime Data { get; set; }
        public string Numero { get; set; }
        public string Nome { get; set; }
        public string Descricao { get; set; }
        public string UrlPDF { get; set; }
    }

    public class EntregasDiaModel
    {
        public int Handle { get; set; }
        public DateTime DataEntrega { get; set; }
        public int Numero { get; set; }
        public string Conferente { get; set; }
        public string Assinatura { get; set; }
        public List<EntregasItensModel> Itens { get; set; }
    }

    public class EntregasItensModel
    {
        public int Handle { get; set; }
        public string CodigoReferencia { get; set; }
        public string Produto { get; set; }
        public string Variacao { get; set; }
        public int QuantidadeRecebida { get; set; }

    }

    public class EntregasPeriodoBuscaModel
    {
        public string DataInicio { get; set; }
        public string DataFim { get; set; }
    }
    public class BuscaDocModel
    {
        public int Tipo { get; set; }
    }

    public class EntregasDiaBuscarModel
    {
        public string Data { get; set; }
    }

    public class Eventos
    {
        public int Handle { get; set; }
        public string Descricao { get; set; }
        public DateTime Data { get; set; }
        public string Link { get; set; }
        public string Nome { get; set; }
        public string Color { get; set; }
    }

    public class FinanceiroModel
    {
        public double Saldo { get; set; }
        public List<FinanceiroItemModel> Itens { get; set; }
    }

    public class FinanceiroItemModel
    {
        public int Handle { get; set; }
        public DateTime DataEmissao { get; set; }
        public DateTime DataVencimento { get; set; }
        public DateTime DataPagamento { get; set; }
        public string DocumentoDigitado { get; set; }
        public Double Valor { get; set; }
        public string Operacao { get; set; }
        public string CFOP { get; set; }
        public string Historico { get; set; }
        public string Observacao { get; set; }
        public string EntradaSaida { get; set; }
        public List<FinanceiroProdutosModel> Produtos { get; set; }
    }

    public class BuscarNotasFiscalModel
    {
        public string DataInicio { get; set; }
        public string DataFim { get; set; }
    }

    public class NotasFiscalModel
    {
        public int Handle { get; set; }
        public DateTime DataEmissao { get; set; }
        public List<NotaFiscalParcelasModel> Parcelas { get; set; }
        public string DocumentoDigitado { get; set; }
        public double Valor { get; set; }
        public string Operacao { get; set; }
        public string CFOP { get; set; }
        public string Historico { get; set; }
        public string Observacao { get; set; }
        public string EntradaSaida { get; set; }
        public List<NotasFiscalProdutosModel> Produtos { get; set; }



    }

    public class NotaFiscalParcelasModel
    {
        public int Handle { get; set; }
        public DateTime DataVencimento { get; set; }
        public string DataBaixa { get; set; }
        public Double Valor { get; set; }
        public Double ValorBaixado { get; set; }

    }
    public class FinanceiroProdutosModel
    {
        public int Handle { get; set; }
        public int Quantidade { get; set; }
        public int ValorUnitario { get; set; }
        public int Total { get; set; }
        public string Nome { get; set; }
    }
    public class NotasFiscalProdutosModel
    {
        public int Handle { get; set; }
        public int Quantidade { get; set; }
        public double ValorUnitario { get; set; }
        public double Total { get; set; }
        public string Nome { get; set; }
    }

    public class CardModel
    {
        public int Handle { get; set; }//hanlde
        public string Valor { get; set; }// resultado SELECT SQL
        public string ConsultaSQL { get; set; }//SELECT SQL
        public string Nome { get; set; }//NOME
        public string Color { get; set; }//COR
        public string Screen { get; set; }//STRING
    }

    public class HomeModel
    {
        public int id { get; set; }
        public object screen { get; set; }
        public string title { get; set; }
        public string icon { get; set; }
        public int type { get; set; }
    }

    public class Login
    {
        public string username { get; set; }
        public string password { get; set; }
        public string pushToken { get; set; }
    }

    public class Response : UserInfoModel
    {
        public int expires_in { get; set; }
        public string statusCode { get; set; }
        public string access_token { get; set; }
        public string message { get; set; }

    }

    public class UserInfoModel
    {
        public string AvatarBase64 { get; set; } //Z_GRUPOUSUARIO//Figura IMAGEM
        public string Nome { get; set; }//Z_GRUPOUSUARIO
        public DateTime DataInicioCooperado { get; set; }//Z_GRUPOUSUARIO DD?MM/YYYY
        public int CapitalSocial { get; set; } //EMPRESAS
        public int RelatorioEntregaDia { get; set; }
        public int RelatorioEntregaPeriodo { get; set; }

        public int RelatorioEntregaProgramacao { get; set; }
    }

    public class ProgramacaoModel
    {
        public int Handle { get; set; }
        public string Produto { get; set; }
        public DateTime Periodo { get; set; }
        public DateTime DataFim { get; set; }
        public int Programado { get; set; }
        public int ProgramadoPeriodo { get; set; }
    }

    public class SacModelPost
    {
        public string Titulo { get; set; }
        public string Mensagem { get; set; }
    }

    public class SacModelGet
    {
        public int Handle { get; set; }
        public int Numero { get; set; }
        public string Titulo { get; set; }
        public string Mensagem { get; set; }
        public string Resposta { get; set; }
        public string Color { get; set; }
        public int Status { get; set; }
    }

    public class ResponseModel
    {
        public int status { get; set; }
        public string descricao { get; set; }
    }

    public class RequestDocumento
    {
        public Int32 Proceso { get; set; } //1 - Relatório, 2-Bdoc
        public Int32 HandleOrigem { get; set; }
        public string Format { get; set; }
        public string Condicao { get; set; }
        public string Tabela { get; set; }
        public string Campo { get; set; }
    }
    public class ResponseDocumento
    {
        public EntityBase Tabela { get; set; }

    }

    public class ProcessarAnaliseModel
    {
        public string Datainicio { get; set; }
        public string Datafim { get; set; }
        public int Processo { get; set; }

    }

    public class QProgramacao
    {
        public int Handle { get; set; }
        public string Nome { get; set; }
        public int Produto { get; set; }
        public int Programado { get; set; }
        public DateTime Periodo { get; set; }

    }

    public class QPedidoMercado
    {
        public DateTime DataPedido { get; set; }
        public string CodigoRef { get; set; }
        public string Produto { get; set; }
        public string ProdutoBase { get; set; }
        public int ProdutoMatriz { get; set; }
        public int Pedido { get; set; }
        public int Caisp { get; set; }

    }
    public class QPedidoCooperado
    {
        public DateTime DataPedido { get; set; }
        public string Produtor { get; set; }
        public string CodigoRef { get; set; }
        public string Produto { get; set; }
        public string ProdutoBase { get; set; }
        public string ProdutoMatriz { get; set; }
        public int Pedido { get; set; }
        public int QuantidadeEntregue { get; set; }

    }
    public class QCotas
    {
        public int Produtor { get; set; }
        public string ProdutoBase { get; set; }
        public int ProdutoMatriz { get; set; }
        public double Cota { get; set; }
    }
    public class QOrdemProducao
    {
        public string Apelido { get; set; }
        public DateTime DataOrdem { get; set; }
        public DateTime DataPedido { get; set; }
        public int Numero { get; set; }
        public int CodigoRef { get; set; }
        public string Produto { get; set; }
        public string GrupoItens { get; set; }
        public string Abreviatura { get; set; }
        public string ProdutoMatriz { get; set; }
        public int QtdPedida { get; set; }
        public int FaltaReceber { get; set; }
        public int QtdRecebida { get; set; }
    }
    public class QQuebras
    {
        public string Produtor { get; set; }
        public string Grupo { get; set; }
        public int Codigo { get; set; }
        public int CodigoRef { get; set; }
        public string Produto { get; set; }
        public string ProdutoBase { get; set; }
        public string ProdutoMatriz { get; set; }
        public string Lote { get; set; }
        public DateTime DataInclusao { get; set; }
        public DateTime DataBaixa { get; set; }
        public int QtdEntrada { get; set; }
        public string UN { get; set; }
        public int QtdBaixada { get; set; }
        public string Motivo { get; set; }
    }
    public class QNFe
    {
        public DateTime DataEmissao { get; set; }
        public string DocumentoDigitado { get; set; }
        public int Apelido { get; set; }
        public int CategoriaFornecedor { get; set; }
        public int Item { get; set; }
        public string ItemBase { get; set; }
        public int ItemMatriz { get; set; }
        public double ValorLiquido { get; set; }
        public double DescontoValor { get; set; }
        public int Quantidade { get; set; }
        public string CentroDeCusto { get; set; }
        public string Unidade { get; set; }
    }

    public class AnaliseCooperado
    {
        public int Fornecedor { get; set; }
        public int Item { get; set; }
        public double PrecoMedio { get; set; }
        public int Programado77Dias { get; set; }
        public double CotaAtual { get; set; }
        public int PedidoMercado { get; set; }
        public int OrdemDeCompra { get; set; }
        public double AtendimentoNfe { get; set; }
        public double DiferencaNFe { get; set; }
        public double porcentagemDiferenca { get; set; }
        public double DiferencaProgramadoAtendido { get; set; }
        public double porcentagemDiferencaProgramadoAtendid { get; set; }
        public double NFeXProgramado { get; set; }
        public double CotaProjetada { get; set; }

    }
    public class RequestCarregarFornecerdor
    {
        public int Produto { get; set;}
        public int Quantidade { get;set; }
        public string DataInicio {  get; set; }
        public string DataFim { get; set; }
        public int Handle { get; set; }

    }
}
