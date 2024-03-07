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
        public int Numero { get; set; }
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
        public string Link { get; set; }
    }

    public class EntregasPeriodoBuscaModel
    {
        public string DataInicio { get; set; }
        public string DataFim { get; set; }
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
        public int Saldo { get; set; }
        public List<FinanceiroItemModel> Itens { get; set; }
    }

    public class FinanceiroItemModel
    {
        public int Handle { get; set; }
        public DateTime DataEmissao { get; set; }
        public DateTime DataVencimento { get; set; }
        public DateTime DataPagamento { get; set; }
        public string DocumentoDigitado { get; set; }
        public int Valor { get; set; }
        public string Operacao { get; set; }
        public string CFOP { get; set; }
        public string Historico { get; set; }
        public string Observacao { get; set; }
        public string EntradaSaida { get; set; }
        public List<FinanceiroProdutosModel> Produtos { get; set; }
    }

    public class FinanceiroProdutosModel
    {
        public int Handle { get; set; }
        public int Quantidade { get; set; }
        public int ValorUnitario { get; set; }
        public int Total { get; set; }
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
    }

    public class ProgramacaoModel
    {
        public int Handle { get; set; }
        public string Produto { get; set; }
        public DateTime Periodo { get; set; }
        public int Programado { get; set; }
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
}
