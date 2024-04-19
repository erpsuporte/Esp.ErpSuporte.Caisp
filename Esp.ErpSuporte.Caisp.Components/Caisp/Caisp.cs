using Benner.Tecnologia.Business;
using Benner.Tecnologia.Business.Tasks;
using Benner.Tecnologia.Common;
using Benner.Tecnologia.Common.Components;
using Benner.Tecnologia.Common.EnterpriseServiceLibrary;
using Benner.Tecnologia.Common.Services;
using Benner.Tecnologia.Metadata.Entities;
using Benner.Tecnologia.Metadata.TransformData;
using Esp.ErpSuporte.Caisp.Business.Interfaces.Caisp;
using Esp.ErpSuporte.Caisp.Business.Modelos.Caisp;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices.ComTypes;
using System.Security.Policy;
using Benner.Tecnologia.Application.Services.ReportService;
using static System.Net.WebRequestMethods;
using Benner.Corporativo.Definicoes.Integracao.Hospitalar;
using Benner.Corporativo.Definicoes.Producao.OrdensProducao;
using System.Linq;

namespace Esp.ErpSuporte.Caisp.Components.Caisp
{
    public class CaispComponente : BusinessComponent<CaispComponente>, ICaisp
    {
        public Handle EmitReport_(Handle reportHandle, string format, Criteria criteria, bool defaultFilter, bool companyFilter, EntityBase filterEntity, bool emitToTemporaryTable, TransitoryData transitoryData)
        {
            var report = RRelatorios.Get(reportHandle);

            var reportEmitter = ReportEmitterFactory.CreateEmitter(report);
            reportEmitter.Render(filterEntity, format, criteria, defaultFilter, companyFilter, false, true, transitoryData);
            reportEmitter.Emit(emitToTemporaryTable);

            return reportEmitter.GetEmittedReportHandle();
        }

        public ResponseDocumento BuscarDocumento(RequestDocumento request)
        {
            ResponseDocumento retorno = new ResponseDocumento();

            try
            {
                if (request.Proceso == 1)
                {
                    Int32 handleReport;
                    string format;
                    Criteria criteria = null;
                    bool defaultFilter = true;
                    bool companyFilter = true;
                    bool emitToTemporaryTable = true;

                    handleReport = request.HandleOrigem;
                    format = request.Format;
                    if (request.Condicao != "")
                    {
                        criteria = new Criteria(request.Condicao);
                    }

                    var processid = EmitReport_(
                        new Handle(handleReport),
                        format,
                        criteria,
                        defaultFilter,
                        companyFilter,
                        null,
                        emitToTemporaryTable,
                        new TransitoryData());

                   
                    retorno.Tabela = Entity.Get(EntityDefinition.GetByName("Z_ARQUIVOSTEMPORARIOS"), new Criteria("A.HANDLE = :HANDLE", new Parameter("HANDLE", (int)(processid.Value)))); 
                    
                    

                }
                if (request.Proceso == 2)
                {
                    retorno.Tabela= Entity.Get(EntityDefinition.GetByName(request.Tabela), new Criteria("A.HANDLE = :HANDLE", new Parameter("HANDLE", request.HandleOrigem)));
                }
            }
            catch (Exception erro)
            {
                throw erro;
            }
            return retorno;
        }


        public object Destinatarios { get; private set; }

        public List<ContatosModel> buscarContatos()
        {

            List<ContatosModel> retorno = new List<ContatosModel>();


            //retorno.Add(new ContatosModel()
            //{
            //    Handle = 1,
            //    Nome = "Nome 1",
            //    Descricao = "Descricao 1",
            //    Cargo = "Cargo 1",
            //    Telefone = "11991595816",
            //    Ramal = "Ramal 1",
            //    Whatsapp = "11991595816"
            //});

            List<EntityBase> registros = Entity.GetMany(EntityDefinition.GetByName("K_GN_CONTATOS"), new Criteria());


            foreach (EntityBase registro in registros)
            {
                retorno.Add(new ContatosModel()
                {
                    Handle = Convert.ToInt32(registro.Fields["HANDLE"]),
                    Nome = Convert.ToString(registro.Fields["NOME"]),
                    Descricao = registro.Fields["DESCRICAO"] != null ? Convert.ToString(registro.Fields["DESCRICAO"]) : "",
                    Cargo = Convert.ToString(registro.Fields["CARGO"]),
                    Telefone = Convert.ToString(registro.Fields["TELEFONE"]),
                    Ramal = registro.Fields["RAMAL"] != null ? Convert.ToString(registro.Fields["RAMAL"]) : "",
                    Whatsapp = Convert.ToString(registro.Fields["WHATSAPP"])
                });
            }

            return retorno;
        }
        public List<DocModel> buscarDoc(BuscaDocModel request)
        {
            List<DocModel> retorno = new List<DocModel>();

            //retorno.Add(new DocModel()
            //{
            //    Handle = 1,
            //    Data = DateTime.Parse("01 /01/2022"),
            //    Numero = 1,
            //    Nome = "Nome 1",
            //    Descricao = "Descricao 1",
            //    UrlPDF = @"http://erpsuporte.com.br"
            //});
            //retorno.Add(new DocModel()
            //{
            //    Handle = 2,
            //    Data = DateTime.Parse("01/01/2022"),
            //    Numero = 2,
            //    Nome = "Nome 2",
            //    Descricao = "Descricao 2",
            //    UrlPDF = @"http://erpsuporte.com.br"
            //});



            Query query = new Query(@"SELECT DISTINCT C.HANDLE,
                                               C.DATA,
                                               C.NUMERO,
                                               C.NOME,
                                               C.DESCRICAO
                                          FROM K_GN_DOCUMENTOS C
                                               LEFT JOIN K_GN_DOCUMENTOPESSOAS B ON C.HANDLE = B.DOCUMENTO
                                               LEFT JOIN K_GN_PESSOAUSUARIOS A ON B.PESSOA = A.PESSOA
                                         WHERE
                                               (
                                                   (
                                                       @USUARIO = A.USUARIO
                                                       AND B.DOCUMENTO IS NOT NULL
                                                   )
                                                   OR B.DOCUMENTO IS NULL
                                               )
                                               AND C.TIPO = :TIPO;");
            
            query.Parameters.Add(new Parameter("TIPO", request.Tipo));
            //List<EntityBase> registros = Entity.GetMany(EntityDefinition.GetByName("K_GN_DOCUMENTOS"), new Criteria()); ;
            var registros = query.Execute();
            
           


            foreach (EntityBase registro in registros)
            {
                string url = "http://sistemateste.caisp.com.br:1010/CORP_TESTE/Pages/Public/Arquivo.ashx?Processo=2&Tipo=PDF&Campo=ARQUIVOPDF&Tabela=K_GN_DOCUMENTOS";//"https://erpsuporte.com.br/NEW_CORP_CAISP_DEV_20200328/Pages/Public/Arquivo.ashx?Processo=2&Tipo=PDF&Campo=ARQUIVOPDF&Tabela=K_GN_DOCUMENTOS";
                //Gerar um link com dois parâmetros
                var urlLinkDefinition = new UrlLinkDefinition(url);
                

                //Handle 
                urlLinkDefinition.Parameters.Add("HandleDocumento", Convert.ToString(registro.Fields["HANDLE"]));

                retorno.Add(new DocModel()
                {
                    Handle = Convert.ToInt32(registro.Fields["HANDLE"]),
                    Data = Convert.ToDateTime(registro.Fields["DATA"]),
                    Numero = Convert.ToString(registro.Fields["NUMERO"]),
                    Nome = Convert.ToString(registro.Fields["NOME"]),
                    Descricao = registro.Fields["DESCRICAO"] != null ? Convert.ToString(registro.Fields["DESCRICAO"]) : "",
                    UrlPDF = urlLinkDefinition.GetEncodedUrl()
                }); ;
            }
            return retorno;


        }
        public List<EntregasDiaModel> buscarEntregasDia(EntregasDiaBuscarModel request)
        {
            List<EntregasDiaModel> retorno = new List<EntregasDiaModel>();
            //retorno.Add(new EntregasDiaModel()
            //{
            //    Handle = 1,
            //    DataEntrega = DateTime.Parse("01/01/2022"),
            //    Numero = 1,
            //    Conferente = "Conferente 1",
            //    Assinatura = "Assinatura 1",
            //    Itens = new List<EntregasItensModel>()
            //    {
            //        new EntregasItensModel()
            //        {
            //            Handle = 1,
            //            CodigoReferencia = "CodigoReferencia 1",
            //            Produto = "Produto 1",
            //            QuantidadeRecebida = 1
            //        },
            //        new EntregasItensModel()
            //        {
            //            Handle = 2,
            //            CodigoReferencia = "CodigoReferencia 2",
            //            Produto = "Produto 2",
            //            QuantidadeRecebida = 2
            //        }
            //    }
            //});

            Query query = new Query(@"SELECT A.HANDLE,
                                          A.DATAENTRADA,
                                          A.NUMERONOTAFISCAL,
                                          A.K_CONFERENTE,
                                          A.K_ASSINATURA
                                                FROM CP_RECEBIMENTOFISICOPAI A
                                                INNER JOIN GN_PESSOAS B ON A.FORNECEDOR = B.HANDLE
                                                WHERE A.FORNECEDOR IN (SELECT PESSOA FROM K_GN_PESSOAUSUARIOS U WHERE U.USUARIO = @USUARIO) 
                                                   AND CONVERT(DATE, A.DATAENTRADA, 103) = CONVERT(DATE, :DATA, 103)");
            
            query.Parameters.Add(new Parameter("DATA", request.Data)); //System.NullReferenceException: 'Referência de objeto não definida para uma instância de um objeto.'
            //List<DocModel> retorno = 

            var registros = query.Execute();//como converter
            //List<EntityBase> registros2  = query.Execute();

            foreach (EntityBase registro in registros)
            {

                Query query2 = new Query(@"SELECT A.HANDLE,
                                           B.CODIGOREFERENCIA,
                                           B.NOME,
                                           A.QTDEENTREGA
                                             FROM CP_RECEBIMENTOFISICO A
                                               INNER JOIN PD_PRODUTOS B ON A.PRODUTO = B.HANDLE
                                                WHERE A.RECEBIMENTOFISICOPAI = :RECEBIMENTOFISICOPAI");
                query2.Parameters.Add(new Parameter("RECEBIMENTOFISICOPAI", Convert.ToInt32(registro.Fields["HANDLE"])));

                List<EntregasItensModel> _itens = new List<EntregasItensModel>();
                //
                var registros2 = query2.Execute();
                foreach (EntityBase registro2 in registros2)
                {
                    _itens.Add(new EntregasItensModel()
                    {
                        Handle = Convert.ToInt32(registro2.Fields["HANDLE"]),
                        CodigoReferencia = Convert.ToString(registro2.Fields["CODIGOREFERENCIA"]),
                        Produto = Convert.ToString(registro2.Fields["NOME"]),
                        QuantidadeRecebida = Convert.ToInt32(registro2.Fields["QTDEENTREGA"]),

                    });
                }
                byte[] bytes = (byte[])registro.Fields["K_ASSINATURA"];
                retorno.Add(new EntregasDiaModel()
                {
                    Handle = Convert.ToInt32(registro.Fields["HANDLE"]),
                    DataEntrega = Convert.ToDateTime(registro.Fields["DATAENTRADA"]),
                    Numero = Convert.ToInt32(registro.Fields["NUMERONOTAFISCAL"]),
                    Conferente = registro.Fields["K_CONFERENTE"] != null ? Convert.ToString(registro.Fields["K_CONFERENTE"]) : null,
                    Assinatura = registro.Fields["K_ASSINATURA"] != null ? Convert.ToBase64String(bytes, 4, bytes.Length - 4) : null,
                    Itens = _itens
                });
            }

            return retorno;
        }

        public List<EntregasItensModel> buscarEntregasPeriodo(EntregasPeriodoBuscaModel request)
        {
            List<EntregasItensModel> retorno = new List<EntregasItensModel>();
            //retorno.Add(new EntregasItensModel()
            //{
            //    Handle = 1,
            //    CodigoReferencia = "CodigoReferencia 1",
            //    Produto = "Produto 1",
            //    QuantidadeRecebida = 100
            //}
            //        );
            //retorno.Add(new EntregasItensModel()
            //{
            //    Handle = 2,
            //    CodigoReferencia = "CodigoReferencia 2",
            //    Produto = "Produto 2",
            //    QuantidadeRecebida = 200
            //}
            //);
            Query query = new Query(@"SELECT A.HANDLE,
                                          A.DATAENTRADA,
                                          A.NUMERONOTAFISCAL,
                                          A.K_CONFERENTE,
                                          A.K_ASSINATURA
                                                FROM CP_RECEBIMENTOFISICOPAI A
                                                INNER JOIN GN_PESSOAS B ON A.FORNECEDOR = B.HANDLE
                                                WHERE A.FORNECEDOR IN (SELECT PESSOA FROM K_GN_PESSOAUSUARIOS U WHERE U.USUARIO = @USUARIO) 
                                                   AND CONVERT(DATE, A.DATAENTRADA, 103) BETWEEN CONVERT(DATE, :DATAINICIO, 103) AND CONVERT(DATE, :DATAFIM, 103)");
            query.Parameters.Add(new Parameter("DATAINICIO", request.DataInicio));
            query.Parameters.Add(new Parameter("DATAFIM", request.DataFim));
            

            var registros = query.Execute();
            

            

            
            foreach (EntityBase registro in registros)
            {
                Query query2 = new Query(@"SELECT A.HANDLE,
                                           B.CODIGOREFERENCIA,
                                           B.NOME,
                                           A.QTDEENTREGA
                                             FROM CP_RECEBIMENTOFISICO A
                                               INNER JOIN PD_PRODUTOS B ON A.PRODUTO = B.HANDLE
                                                WHERE A.RECEBIMENTOFISICOPAI = :RECEBIMENTOFISICOPAI");
                query2.Parameters.Add(new Parameter("RECEBIMENTOFISICOPAI", Convert.ToInt32(registro.Fields["HANDLE"])));

                

                var registros2 = query2.Execute();
                foreach (EntityBase registro2 in registros2)
                {
                    retorno.Add(new EntregasItensModel()
                    {
                        Handle = Convert.ToInt32(registro2.Fields["HANDLE"]),
                        CodigoReferencia = Convert.ToString(registro2.Fields["CODIGOREFERENCIA"]),
                        Produto = Convert.ToString(registro2.Fields["NOME"]),
                        QuantidadeRecebida = Convert.ToInt32(registro2.Fields["QTDEENTREGA"]),
                        

                    });
                }
            }

            return retorno;
        }

        public List<Eventos> buscarEventos()
        {


            //string cor = ColorField.OleColorToHtmlHex(3150273);
            //List<Eventos> retorno = new List<Eventos>();
            //retorno.Add(new Eventos()
            //{
            //    Handle = 1,
            //    Descricao = "Descricao 1",
            //    Data = DateTime.Parse("01/01/2022"),
            //    Link = @"http://erpsuporte.com.br",
            //    Nome = "Nome 1",
            //    Color = cor
            //});
            //retorno.Add(new Eventos()
            //{
            //    Handle = 2,
            //    Descricao = "Descricao 2",
            //    Data = DateTime.Parse("01/01/2022"),
            //    Link = @"http://erpsuporte.com.br",
            //    Nome = "Nome 2",
            //    Color = cor
            //});

            List<Eventos> retorno = new List<Eventos>();
            List<EntityBase> registros = Entity.GetMany(EntityDefinition.GetByName("K_GN_EVENTOS"), new Criteria());

            foreach (EntityBase registro in registros)
            {
                int? Color = ((ColorField)registro.Fields["COR"]).Value;
                int colorInt = Color.Value;
                retorno.Add(new Eventos()
                {
                    Handle = Convert.ToInt32(registro.Fields["HANDLE"]),
                    Descricao = registro.Fields["DESCRICAO"] != null ? Convert.ToString(registro.Fields["DESCRICAO"]) : "",
                    Data = Convert.ToDateTime(registro.Fields["DATA"]),
                    Link = Convert.ToString(registro.Fields["LINK"]),
                    Nome = Convert.ToString(registro.Fields["NOME"]),
                    Color = ColorField.OleColorToHtmlHex(colorInt)
                });
            }
            return retorno;
        }
        public FinanceiroModel buscarFinanceiro()
        {
            FinanceiroModel retorno = new FinanceiroModel(); //SALDO PESSOA vinculados ao usuario logado
            Double Saldo = 0;
            Query query0 = new Query(@"SELECT
                                        LANCAMENTO_VALOR PARCELAVALOR,

                                        ROUND((LANCAMENTO_VALOR * (1-((B.VALOR - ((COALESCE(VALORPAGO,0) - COALESCE(VALORESTORNO,0)))) / B.VALOR))) * (COALESCE(VALORPAGO,0) - COALESCE(VALORESTORNO,0)),2) VALORESBAIXADOS,

                                        LANCAMENTO_VALOR
                                        -
                                        ROUND((LANCAMENTO_VALOR / B.VALOR ) * (COALESCE(VALORPAGO,0) - COALESCE(VALORESTORNO,0)),2)
                                        -
                                        ROUND((COALESCE(B.ABATIMENTOS,0) * (LANCAMENTO_VALOR / B.VALOR )),2)
                                        +
                                        ROUND(((COALESCE(B.ACRESCIMOS,0)+ COALESCE(B.VALORDESAGIO,0)) * (LANCAMENTO_VALOR / B.VALOR )),2)

                                        PARCELAVALORSALDO,


                                        ROUND(
                                        (
                                        LANCAMENTO_VALOR * (1 + ROUND(((COALESCE(B.ACRESCIMOS,0)+ COALESCE(B.VALORDESAGIO,0) - COALESCE(B.ABATIMENTOS,0)) / B.VALOR),10))
                                        )
                                        -
                                        (
                                        LANCAMENTO_VALOR * (1-((B.VALOR - ((COALESCE(VALORPAGO,0) - COALESCE(VALORESTORNO,0)))) / B.VALOR))
                                        )
                                        ,2) SALDO_COTACAO_EMISSAO,
                                        ROUND(
                                        (
                                        LANCAMENTO_VALOR * (1 + ROUND(((COALESCE(B.ACRESCIMOS,0)+ COALESCE(B.VALORDESAGIO,0) - COALESCE(B.ABATIMENTOS,0)) / B.VALOR),10))
                                        )
                                        -
                                        (
                                        LANCAMENTO_VALOR * (1-((B.VALOR - ((COALESCE(VALORPAGO,0) - COALESCE(VALORESTORNO,0)))) / B.VALOR))
                                        )
                                        ,2) SALDO_COTACAO_ATUALIZADO,
                                        NULL CLIENTE,
                                        E.NOME OPERACAO,
                                        NULL TIPOSAIDA,
                                        C.ENTRADASAIDA,
                                        CASE C.TIPODEMOVIMENTO WHEN 1 THEN 'Normal' WHEN 2 THEN 'Adiantamento' WHEN 3 THEN 'Devolução' ELSE 'Outros' END TIPODEMOVIMENTO,
                                        F.NOME FILIAL,
                                        D.NOME PESSOA,
                                        C.DOCUMENTODIGITADO,
                                        C.DATAENTRADA,
                                        L.ESTRUTURA CENTROCUSTO_ESTURUTURA,
                                        L.NOME CENTROCUSTO_NOME,
                                        M.ESTRUTURA PROJETO_ESTRUTURA,
                                        M.NOME PROJETO_NOME,
                                        G.NOME CONTA_FINANCEIRA,
                                        G.ESTRUTURA CONTA_FINANCEIRAESTRUTURA,
                                        C.VALORNOMINAL DOC_VALORNOMINAL,
                                        C.VALORLIQUIDO DOC_VALORLIQUIDO,
                                        B.AP,
                                        B.PARCELADIGITADA,
                                        B.VENCIMENTOORIGINAL VENCIMENTO,
                                        B.PARCELADIGITADA,
                                        B.VCTOPRORROGADO VCTOPRORROGADO,
                                        B.VALOR PARCELA_VALOR
                                        FROM (
                                        SELECT A.HANDLE PARCELA,
                                        F.CONTA,
                                        FC.CENTROCUSTO,
                                        FC.PROJETO,
                                        ((CASE WHEN F.NATUREZA = 'D' THEN -1 ELSE CASE WHEN F.NATUREZA = 'C' THEN 1 ELSE CASE WHEN FN_DOCUMENTOS.ENTRADASAIDA = 'E' THEN -1 ELSE 1 END END END) * (COALESCE(FC.VALOR,F.VALOR, A.VALOR))) LANCAMENTO_VALOR
                                        FROM FN_PARCELAS A
                                        INNER JOIN FN_DOCUMENTOS FN_DOCUMENTOS ON (FN_DOCUMENTOS.HANDLE = A.DOCUMENTO)
                                        INNER JOIN FN_LANCAMENTOS F ON A.HANDLE = F.PARCELA AND F.TIPO IN (3)
                                        LEFT JOIN FN_LANCAMENTOCC FC ON F.HANDLE = FC.LANCAMENTO
                                        WHERE A.VALOR > 0
                                        AND A.PREVISAO = 'N'
                                        AND EXISTS
                                        (
                                        SELECT *
                                        FROM FN_DOCUMENTOS DOCS
                                        WHERE DOCS.HANDLE = A.DOCUMENTO AND DOCS.PESSOA IN (SELECT PESSOA FROM K_GN_PESSOAUSUARIOS U WHERE U.USUARIO = @USUARIO)
                                        AND
                                        (
                                        (
                                        (
                                        (
                                        DOCS.ENTRADASAIDA IN ('E')
                                        AND
                                        (
                                        DOCS.ABRANGENCIA <> 'R'
                                        )
                                        AND DOCS.TIPODEMOVIMENTO IN(1, 2)
                                        )
                                        OR
                                        (
                                        DOCS.ENTRADASAIDA IN ('S')
                                        AND DOCS.TIPODEMOVIMENTO = 3
                                        )
                                        )
                                        AND
                                        (
                                        (
                                        DOCS.DATACANCELAMENTO IS NULL
                                        )
                                        OR
                                        (
                                        DATACONTABILCANCELAMENTO > CONVERT(DATETIME, FLOOR(CONVERT(FLOAT, GETDATE())))
                                        )
                                        )
                                        AND
                                        (
                                        (
                                        A.DATALIQUIDACAO IS NULL
                                        AND
                                        (
                                        A.EMABERTO = 'S'
                                        )
                                        )
                                        )
                                        )
                                        OR
                                        (
                                        (
                                        (
                                        DOCS.ENTRADASAIDA IN ('S')
                                        AND DOCS.TIPODEMOVIMENTO IN(1, 2)
                                        )
                                        OR
                                        (
                                        DOCS.ENTRADASAIDA IN ('E')
                                        AND DOCS.TIPODEMOVIMENTO = 3
                                        )
                                        )
                                        AND
                                        (
                                        (
                                        DOCS.DATACANCELAMENTO IS NULL
                                        )
                                        OR
                                        (
                                        DOCS.DATACANCELAMENTO > CONVERT(DATETIME, FLOOR(CONVERT(FLOAT, GETDATE())))
                                        )
                                        )
                                        AND
                                        (
                                        (
                                        DOCS.DATACONTABILCANCELAMENTO IS NULL
                                        )
                                        OR
                                        (
                                        DOCS.DATACONTABILCANCELAMENTO > CONVERT(DATETIME, FLOOR(CONVERT(FLOAT, GETDATE())))
                                        )
                                        )
                                        AND
                                        (
                                        A.DATALIQUIDACAO > CONVERT(DATETIME, FLOOR(CONVERT(FLOAT, GETDATE())))
                                        OR A.DATALIQUIDACAO IS NULL
                                        )
                                        AND DOCS.DATAEMISSAO <= CONVERT(DATETIME, FLOOR(CONVERT(FLOAT, GETDATE())))
                                        )
                                        )
                                        )
                                        ) A
                                        INNER JOIN FN_PARCELAS B ON A.PARCELA = B.HANDLE
                                        LEFT JOIN FN_DOCUMENTOS C ON B.DOCUMENTO = C.HANDLE
                                        LEFT JOIN GN_PESSOAS D ON C.PESSOA = D.HANDLE
                                        LEFT JOIN GN_OPERACOES E ON C.OPERACAO = E.HANDLE
                                        LEFT JOIN FILIAIS F ON C.FILIAL = F.HANDLE
                                        LEFT JOIN FN_CONTAS G ON A.CONTA = G.HANDLE
                                        LEFT JOIN GN_MOEDAS H ON C.MOEDA = H.HANDLE
                                        LEFT JOIN GN_PARAMETROS K ON C.EMPRESA = K.EMPRESA
                                        LEFT JOIN CT_CC L ON A.CENTROCUSTO = L.HANDLE
                                        LEFT JOIN GN_PROJETOS M ON A.PROJETO = M.HANDLE
                                        LEFT JOIN
                                        (
                                        SELECT FN_MOVIMENTACOES.PARCELA,
                                        SUM(VALOR) VALORPAGO,
                                        SUM(VALORESBAIXADOSMOEDA) VALORPAGOMOEDA
                                        FROM FN_MOVIMENTACOES
                                        WHERE ((FN_MOVIMENTACOES.TIPOMOVIMENTO IN (1, 2, 5, 8, 14)) OR (FN_MOVIMENTACOES.TIPOMOVIMENTO = 3 AND FN_MOVIMENTACOES.MOVIMENTACAO IS NOT NULL))
                                        AND FN_MOVIMENTACOES.DATA <= CONVERT(DATETIME, FLOOR(CONVERT(FLOAT, GETDATE())))
                                        GROUP BY FN_MOVIMENTACOES.PARCELA
                                        ) BAIXA ON A.PARCELA = BAIXA.PARCELA
                                        LEFT JOIN
                                        (
                                        SELECT A.PARCELA,
                                        SUM(A.VALOR) VALORESTORNO,
                                        SUM(A.VALORESBAIXADOSMOEDA) VALORESTORNOMOEDA
                                        FROM FN_MOVIMENTACOES A
                                        WHERE A.TIPOMOVIMENTO = 9
                                        AND A.DATA <= CONVERT(DATETIME, FLOOR(CONVERT(FLOAT, GETDATE())))
                                        
                                        GROUP BY A.PARCELA
                                        ) ESTORNO ON A.PARCELA = ESTORNO.PARCELA
                                        WHERE B.VALOR > 0
                                        ");
            var registros0 = query0.Execute();
            foreach (EntityBase registro0 in registros0)
            {
                Saldo += Convert.ToDouble(registro0.Fields["PARCELAVALORSALDO"]);
            }
            retorno.Saldo= Math.Round(Saldo, 2);



            Query query1 = new Query(@"SELECT A.HANDLE HANDLEDOCUMENTO,
                                                            B.HANDLE,
                                                            A.DATAEMISSAO,
                                                            B.DATAVENCIMENTO DataVencimento,
                                                            B.VCTOPRORROGADO DataPagamento,
                                                            A.DOCUMENTODIGITADO,
                                                            A.ENTRADASAIDA,
                                                            B.VALOR - B.VALORESBAIXADOS Valor,
                                                            O.NOME Operacao,
                                                            NULL CFOP, --PEGA DE QQ ITEM
                                                            A.HISTORICO,
                                                            NULL Observacao
                                                        FROM FN_DOCUMENTOS A
                                                            JOIN FN_PARCELAS B ON (B.DOCUMENTO = A.HANDLE)
                                                            JOIN GN_OPERACOES O ON (O.HANDLE = A.OPERACAO)
                                                            LEFT OUTER JOIN TR_MODELOSFISCAIS M ON (M.HANDLE = A.MODELO)
                                                        WHERE

                                                                A.OPERACAO = O.HANDLE

                                                            AND

                                                                A.ENTRADASAIDA IN ('E', 'S') 

                                                            AND

                                                                A.TIPODEMOVIMENTO = 1
                                                            AND NOT EXISTS
                                                            (
                                                                SELECT 1
                                                                        FROM (SELECT 'R' ID
                                                                                UNION ALL SELECT 'F' ID
                                                                                        UNION ALL SELECT 'A' ID
                                                                                                    UNION ALL SELECT 'B' ID
                                                                                                                UNION ALL SELECT 'D' ID
                                                                                                                            UNION ALL SELECT 'G' ID
                                                                                                                                    UNION ALL SELECT 'H' ID) AB_LIST WHERE (A.ABRANGENCIA = AB_LIST.ID))
                                                                AND A.EHPREVISAO = 'N'
                                                                AND A.PESSOA IN (SELECT PESSOA FROM K_GN_PESSOAUSUARIOS U WHERE U.USUARIO = @USUARIO)
                                                                AND (((B.VALORESBAIXADOS IS NULL OR B.VALORESBAIXADOS = 0) AND (B.VALOR > 0)))
                                                            ORDER BY B.AP");

            // A.ENTRADASAIDA IN ('E', 'S')  tratar como E sinal positivo ou S negativo  (caso seja um unico select).
            // caso sejam select separados um para E outro para S.

            //System.NullReferenceException: 'Referência de objeto não definida para uma instância de um objeto.'
            //List<DocModel> retorno = 
            var registros1 = query1.Execute();
            retorno.Itens = new List<FinanceiroItemModel>();
            foreach (EntityBase registro1 in registros1)
            {
                
                Query query2 = new Query(@"SELECT A.HANDLE,
                                                   A.QUANTIDADE,
                                                   A.VALORUNITARIO,
                                                   A.VALORTOTAL,
                                                   C.NOME PRODUTO,
                                                   D.ESTRUTURA CFOP
                                              FROM CM_ITENS A
                                                   INNER JOIN FN_DOCUMENTOS B ON A.DOCUMENTO = B.HANDLE
                                                   INNER JOIN PD_PRODUTOS C ON A.PRODUTO = C.HANDLE
                                                   INNER JOIN GN_NATUREZASFISCAIS D ON A.CLASSIFICACAOFISCAL = D.HANDLE
                                             WHERE B.DOCUMENTOORIGEM = :HANDLEDOCUMENTO");
                query2.Parameters.Add(new Parameter("HANDLEDOCUMENTO", Convert.ToInt32(registro1.Fields["HANDLEDOCUMENTO"])));
                var registros2 = query2.Execute();
                List<FinanceiroProdutosModel> _Produtos = new List<FinanceiroProdutosModel>();
                List<string> _CFOP = new List<string>();
                foreach (EntityBase registro2 in registros2)
                {
                    

                    _Produtos.Add(new FinanceiroProdutosModel()
                    {
                        Handle = Convert.ToInt32(registro2.Fields["HANDLE"]),
                        Quantidade = Convert.ToInt32(registro2.Fields["QUANTIDADE"]),
                        ValorUnitario = Convert.ToInt32(registro2.Fields["VALORUNITARIO"]), 
                        Total = Convert.ToInt32(registro2.Fields["VALORTOTAL"]),
                        Nome = Convert.ToString(registro2.Fields["PRODUTO"])
                        
                    });
                    _CFOP.Add(Convert.ToString(registro2.Fields["CFOP"]));

                }
                
                retorno.Itens.Add(new FinanceiroItemModel()
                {
                        Handle = Convert.ToInt32(registro1.Fields["HANDLE"]),
                        DataEmissao = Convert.ToDateTime(registro1.Fields["DATAEMISSAO"]),
                        DataVencimento = Convert.ToDateTime(registro1.Fields["DataVencimento"]),
                        DataPagamento = Convert.ToDateTime(registro1.Fields["DataPagamento"]),
                        DocumentoDigitado = Convert.ToString(registro1.Fields["DOCUMENTODIGITADO"]).StartsWith("NFe-") ? Convert.ToString(registro1.Fields["DOCUMENTODIGITADO"]).Substring(4) : Convert.ToString(registro1.Fields["DOCUMENTODIGITADO"]),
                    Valor = Convert.ToDouble(registro1.Fields["Valor"]),
                        CFOP = _CFOP.Count > 0 ? _CFOP[0] : string.Empty,
                        Operacao = Convert.ToString(registro1.Fields["Operacao"]),
                        Historico = Convert.ToString(registro1.Fields["HISTORICO"]),
                        Observacao = Convert.ToString(registro1.Fields["Observacao"]),
                        EntradaSaida = Convert.ToString(registro1.Fields["ENTRADASAIDA"]),
                        Produtos = _Produtos,
                });


                // Adicionando Produtos ao último item em retorno.Itens

               

            }
            return retorno;
            //FinanceiroModel retorno = new FinanceiroModel()
            //{
            //    Saldo = 1,

            //    Itens = new List<FinanceiroItemModel>()
            //    {
            //        //new FinanceiroItemModel()
            //        //{
            //        //    Handle = 1,
            //        //    DataEmissao = DateTime.Parse("01/01/2022"),
            //        //    DataVencimento = DateTime.Parse("01/01/2022"),
            //        //    DataPagamento = DateTime.Parse("01/01/2022"),
            //        //    DocumentoDigitado = "123456",
            //        //    Valor = 1000,
            //        //    Operacao = "Venda",
            //        //    CFOP = "5102",
            //        //    Historico = "Venda de produtos",
            //        //    Observacao = "Pagamento em dinheiro",
            //        //    Produtos = new List<FinanceiroProdutosModel>()
            //        //    {
            //        //        new FinanceiroProdutosModel()
            //        //        {
            //        //            Handle = 1,
            //        //            Quantidade = 5,
            //        //            ValorUnitario = 200,
            //        //            Total = 1000,
            //        //            Nome = "Produto 1"
            //        //        },
            //        //        new FinanceiroProdutosModel()
            //        //        {
            //        //            Handle = 2,
            //        //            Quantidade = 3,
            //        //            ValorUnitario = 150,
            //        //            Total = 450,
            //        //            Nome = "Produto 2"
            //        //        }
            //        //    }
            //        //}

            //    }
            //};




        }
        public List<NotasFiscalModel> buscarNotasFicais(BuscarNotasFiscalModel request)
        {
            List<NotasFiscalModel> retorno = new List<NotasFiscalModel>();
            
            Query query = new Query(@"SELECT DISTINCT A.HANDLE,
                                           A.DATAEMISSAO,
                                           A.DOCUMENTODIGITADO,
                                           A.VALORNOMINAL,
                                           A.HISTORICO,
                                           CASE WHEN A.ENTRADASAIDA IN ('E','I') THEN 'E' ELSE 'S' END ENTRADASAIDA,
                                           B.NOME OPERACAO,
                                           NULL OBSERVACOES,
                                           A.ABRANGENCIA
                                           --CFOP VEM QDE QQ ITEM, PREENCHE DEPOIS QUANDO FOR BUSCAR OS ITENS
                                      FROM FN_DOCUMENTOS A
                                           INNER JOIN GN_OPERACOES B ON A.OPERACAO = B.HANDLE
                                           INNER JOIN CM_OPERACOESFATURAMENTO C ON A.OPERACAOFATURAMENTO = C.HANDLE AND C.ATUALIZAFINANCEIRO = 'S'
                                     WHERE A.ABRANGENCIA IN ('R','A')
                                           AND A.STATUS = 2
                                           AND EXISTS (SELECT PESSOA FROM K_GN_PESSOAUSUARIOS U WHERE U.USUARIO = @USUARIO AND U.PESSOA =A.PESSOA)
                                           AND A.DATAEMISSAO BETWEEN CONVERT(DATE, :DATAINICIO, 103) AND CONVERT(DATE, :DATAFIM, 103);");

            query.Parameters.Add(new Parameter("DATAINICIO", request.DataInicio));
            query.Parameters.Add(new Parameter("DATAFIM", request.DataFim));
            var registros = query.Execute();
            
            foreach (EntityBase registro in registros)
            {
                var _CFOP = "";
                List<NotaFiscalParcelasModel> parcelas = new List<NotaFiscalParcelasModel>();
                List<NotasFiscalProdutosModel> produtos = new List<NotasFiscalProdutosModel>();
                Query query2 = new Query(@"SELECT DISTINCT A.HANDLE,
                                               VCTOPRORROGADO,
                                               A.VALOR,
                                               A.VALORESBAIXADOS,
                                               A.DATALIQUIDACAO
                                          FROM FN_PARCELAS A
                                               INNER JOIN FN_DOCUMENTOS B ON A.DOCUMENTO = B.DOCUMENTOORIGEM
                                         WHERE B.HANDLE = :HANDLE   ");
                query2.Parameters.Add(new Parameter("HANDLE", Convert.ToInt32(registro.Fields["HANDLE"])));
                var resgitros2 = query2.Execute();
                foreach(EntityBase registro2 in resgitros2)
                {

                    parcelas.Add(new NotaFiscalParcelasModel()
                    {
                        Handle = Convert.ToInt32(registro2.Fields["HANDLE"]),
                        DataVencimento = Convert.ToDateTime(registro2.Fields["VCTOPRORROGADO"]),
                        DataBaixa = Convert.ToString(registro2.Fields["DATALIQUIDACAO"]),//Convert.ToString(registro2.Fields["DATALIQUIDACAO"]),
                        Valor = Convert.ToDouble(registro2.Fields["VALOR"]),
                        ValorBaixado = Convert.ToDouble(registro2.Fields["VALORESBAIXADOS"])
                    }); ;
                    

                }
                Query query3 = new Query(@"SELECT DISTINCT C.HANDLE,
                                               C.ESTRUTURA CFOP,
                                               B.NOME PRODUTO,
                                               A.QUANTIDADE,
                                               A.VALORUNITARIO,
                                               A.VALORTOTAL
                                          FROM CM_ITENS A
                                               INNER JOIN PD_PRODUTOS B ON A.PRODUTO = B.HANDLE
                                               INNER JOIN GN_NATUREZASFISCAIS C ON A.CLASSIFICACAOFISCAL = C.HANDLE
                                             WHERE A.DOCUMENTO = :HANDLE");
                query3.Parameters.Add(new Parameter("HANDLE", Convert.ToInt32(registro.Fields["HANDLE"])));
                var resgitros3 = query3.Execute();
                foreach (EntityBase registro3 in resgitros3)
                {
                    produtos.Add(new NotasFiscalProdutosModel()
                    {
                        Handle = Convert.ToInt32(registro3.Fields["HANDLE"]),
                        Quantidade = Convert.ToInt32(registro3.Fields["QUANTIDADE"]),
                        ValorUnitario = Convert.ToDouble(registro3.Fields["VALORUNITARIO"]),
                        Total = Convert.ToDouble(registro3.Fields["VALORTOTAL"]),
                        Nome = Convert.ToString(registro3.Fields["PRODUTO"]),


                    });
                    _CFOP = Convert.ToString(registro3.Fields["CFOP"]);
                }

                retorno.Add(new NotasFiscalModel()
                {
                    Handle = Convert.ToInt32(registro.Fields["HANDLE"]),
                    DataEmissao = Convert.ToDateTime(registro.Fields["DATAEMISSAO"]),
                    DocumentoDigitado = Convert.ToString(registro.Fields["DOCUMENTODIGITADO"]),
                Valor = Convert.ToDouble(registro.Fields["VALORNOMINAL"]),
                    Operacao = Convert.ToString(registro.Fields["OPERACAO"]),
                    Historico = Convert.ToString(registro.Fields["HISTORICO"]),
                    Observacao = Convert.ToString(registro.Fields["OBSERVACOES"]),
                    EntradaSaida = Convert.ToString(registro.Fields["ENTRADASAIDA"]),
                    CFOP = _CFOP,
                    Parcelas = parcelas,
                    Produtos = produtos

                });
            }

            

            return retorno;
        }
        public List<CardModel> buscarCard()
        {
            //string cor = ColorField.OleColorToHtmlHex(3150273);
            List<CardModel> retorno = new List<CardModel>();
            //retorno.Add(new CardModel()
            //{
            //    Handle = 1,
            //    Nome = "Ulitma entrega",
            //    Valor = "23/01/2024",
            //    Color = cor,
            //    Screen = "EntregasDia"


            //});
            //retorno.Add(new CardModel()
            //{
            //    Handle = 1,
            //    Nome = "Valor em abeto",
            //    Valor = "R$ 1.000,00",
            //    Color = cor,
            //    Screen = "Financeiro"


            //});

            //Query query = new Query(@"SELECT * FROM K_GN_CARDS
                                        //WHERE ATIVO = 'S' ");
            //var registros = query.Execute();
            var Valor = "";
            List<EntityBase> registros = Entity.GetMany(EntityDefinition.GetByName("K_GN_CARDS"), new Criteria("ATIVO = 'S'"));
            //< List> EntityBase = Entity.GetMany(EntityDefinition.GetByName("K_GN_CARDS", new Criteria("ATIVO = 'S'"));

            foreach (EntityBase registro in registros)
            {
                
                //int corInt = Convert.ToInt32(((ColorField)registro.Fields["COR"]).Value);
                Query query2 = new Query(Convert.ToString(registro.Fields["CONSULTASQL"]));
                var registros2 = query2.Execute();
                

                foreach (EntityBase registro2 in registros2)
                {
                    Valor = Convert.ToString(registro2.Fields["VALOR"]);
                    
                }
                
                retorno.Add(new CardModel()
                {
                    Handle = registro.Fields["HANDLE"] != null ? Convert.ToInt32(registro.Fields["HANDLE"]) : 0,
                    Nome = registro.Fields["NOME"] != null ? Convert.ToString(registro.Fields["NOME"]) : "",
                    Color = ColorField.OleColorToHtmlHex(Convert.ToInt32(((ColorField)registro.Fields["COR"]).Value)), //(((ColorField)registro.Fields["COR"]).Color.R).ToString("X2") + (((ColorField)registro.Fields["COR"]).Color.G).ToString("X2") + (((ColorField)registro.Fields["COR"]).Color.B).ToString("X2"),
                    Screen = registro.Fields["TELA"]!= null ? (registro.Fields["TELA"]as ListItem).ToString() : "", //(registro.Fields["TELA"] as ListItem)?.Text?.ToString() ?? "", 
                    Valor = Valor
                });;
            }
            return retorno;
        }
        public UserInfoModel buscarUserInfo()
        {
            UserInfoModel retorno = new UserInfoModel();
            //{
            //    AvatarBase64 = "iVBORw0KGgoAAAANSUhEUgAAAQAAAAEACAYAAABccqhmAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAJ/fSURBVHhe7V0FuFVFF310dykKEjYKKghIt4gioBhYKJjYLRaKgYLS3SXd3d0NoqCAKBZiNyq6/7Xmzj7OPW9eISj8nOFb373cd+85c/Zea82ekwlRi1rUoha1qEUtalGLWtSiFrWoRS1qUYta1KIWtahFLWpRi1rUoha1qEUtalE75lo2IA+Q20Eu4GSgLHAhcEEqwe/yN/wtl+Euk+vguqIWtagdpZYOOAUoDtQGGgDXAI8BDwFPA32BARbDgJnAPGCugznAcuAdYCewI5Xgd/kb/pbLcJfJdXBdXKeun31hn9g39pF9ZZ/Zd24Dt4XbFLWoRQ2NYsgJnAM0Bm4HXgEopkXASmAbsB34FPgc+BYQ4E/7eixC+8a+ss/sO7eB28Jt4rZxG7mt3GZuO2PAWEQGEbX/u5YFyA+cATQCOEKOBJYAewGK5DvgFyAspv93cJu57YwBY8GYMDaMEWPFmDF2WYGoRe24aJwjk7h1gPuAoQBHP5L9N+BYHr2PFTBGjBVjxthxqvE4wOnEWUBeIGpRO2ba6QBHLM59xwB7AB+xI/xzfAxMBB4FLgc4fYha1P71xr3kdwDc+bUeiEb2/wasEJiDNgCPXkQtaketFQNaAxMA7in3ETLCf4d3AVYH9wLMVdSi9o9bRqAWMBzgYbGfAR/5Ihw74P4D5oo5Y+4yAVGLWppaEaAFMB/Qw2//GdKniyFj+nSSySJrxr+RLVM6yZctvRTKnkEKhnBSzoxSMm+mwwJ/G14e18F1cZ1uH7Rf7KP217ct/zKYuxXATQBzGrWoJdu4Q+9B4D3gX5nXpwMypk8wIsqRKb3kypxe8mbNIEVyZpBieTJKqXyZ5eyCmeXcQpnlwpOySoWi2QxqlsghtV2UzCF1QvB9llYktVx33eyL9ot9ZF/ZZ/ad28Bt4TZx27iN3FZuM7fdF5OjAOaSOWVumeOoRS2u8ZRXzh150oqPQEcEJH12jtYQQ+EcGaVY7kxyRv7Mcl7hLBBPVrnkVAj7tOxSv1QOg3rJoG4YEOW/itD6fX1U6PZw27iN3FZuM7edMWAsGBPGhjHyxe4IgjlmrpnzqJ3gLT1wPbAQ8JHlHyELRrr82TCSg+RnFsgsZYtkkYtB/urFsxthXFo6pzQA6pfKGYiFYgqPuP8vcI2C28xtZwz4f8aEsWGMGCvGjLFjDH2xPQJgzlsC5EDUTsB2MfAW8BPgI0iakQmjF0lbCvPn80FklsTVVOyn/y12M2p6BHKiQiuKwBQQK8aMsWMMz0MsS+fLZPZBHOEKgTsMJwHkQtROkMYr2R4G9gE+UqQanMNyhDoJc9wyhbJIZZS3HMlIaI5s9e3oFok97WDMTKWAGNIU+H/GljFmrBlzxv4I7UcgF8iJ6CrH//N2PjAN+B3wESFFpEsX22FXOEcGOQ9E5CjFHWFK1kjwRweuIfCVMWfsmQPmgjn5h0ceyAlygxyJ2v9huxrYBfiSnyJ4iIt7srnjqkqxbLEyHjCCB3ykjXD04MaeYE6474BTMObKl8NUghwhV6L2f9KyA08BvwK+hCeLTBnSycm5MsgFRbKaw1/cm21IFyJkhP8OzAVzwtwwR8wVc8bc+XKaCpAr5Ay5E7XjuBUCeK64L8nJIgNGkaK5Mprj2iRYg9KR6I8HuLli7phD5tKX41SA3CGHonYcthLAZMCX2GRRNGdGuejkrIZQugMqTLQIxzZiRpDTvGcumVNfrlMBcohcitpx1E4DZgG+hCaJPFnSm8N3tUpY4aOkDBMrwvEF5pC5ZE6ZW+bYl/sUQC6RU1E7DhrvQZcm8XOnEU9brX5adqnH8jES/v8dmFPmljlmrg9j/wA5RW5F7RhuBQHesNKXQC9yY0S44KQswc49H3ki/P/AGAHAacFhVAPkFjkWtWOw8fbVQwBf4rzgDiIeS66PkcFHlgj/v2A1wNyTAz5uJANyjFyL2jHWXgJSdQUfL1ktmTezIQJH/jA5IpwYMFUfXkvnz2w44eOKB+QYuRa1Y6jdDKTqZh1M9FkFM6MMjJ1R5iNGhBMH5ADPLiQn0mAC5Bo5F7VjoFUGeLNIX6LikCVDOilTOItJeiT+CAruFyDOBzd4SrGPOx6Qc5cAUfsPG+8bPwPwJSgOvIrs3EJ2Z5+HBBFObJAT5Ea5Ilkkc+qPEPCOUYWBqP1HrR3gS0wcYmV/bOSPxB8hKZAb5Ai5kobpADkYtf+gVQJSvF8fLxc9PX/m6DBfhFSBHCFXziiQ2csnD8jBikDU/sUGXZtLN30JiQMP8/DS0Uj8EVILrRRPTf0hQk5Do3sJ/IuNe2BTvKafl4aa229Fx/kjpBG8O1GN07IbDvm4FcIfwG1A1P6Fxts7bwJ8iQjAu8XwbC+9ICRChLSC3CGHUnlkYCMQ3Xr8X2jPAL4EBOC8n/e1jw71HduodVo2A9/fjhXQBM4okCW1tx4jN6N2FFtp4G3AF/wAvMV0zdOiY/3HMmqXyC6NzslvwPe+7xwLIIfIJXLKx7UQyM1SQNSOUuODHXyBD5AxXYJccFLWaN5/jKPKyRnlvssvkvuuKC9Vi2b0fudYAblETpFbPs6FwKdER+0otJOARYAv6AFOzpnB7MGNjvcf27jkpPTS4e4r5ZU7L5dKhdN5v3OsQPlEbvk4F8IygFyN2hFu1wDJ7vnPkD7BPHUmKv2PbdQollUuP7egTOz6KEygsVQqlOD93rEEcorcIsd83HNAjpKrUTuCLQcwEvAFPEB0zP/4wCUnZZBWtc+Vd+f2lxdua3hcGAA5RW6l8vJhcjW6oegRbGWBHwBfsA0yYH5W/uSs5iwuXwIjHDtgyf/MjXXkjw8WyrM31T0uDIAgt8ixVDydiFy9EIjaEWg86+9+wBfoAHxSTK0S2aPR/xhHzeJZzV7/IS/eIfLxcmMEx4sBkFvkWCqrgKeB6LmDR6Cx/F8D+IJswNGfl3Lyum5f4iIcO6h+Sma5smwR2Ti5uxz6cIk8fRwZAEGOkWupeAoRT1bLCUTtH7bzgGQf6sF7u1Utli0q/48DVD05o7SqU0YO7l0iB3cvOv4MAByrVjyb5E35foLkbAUgav+w8QktvgAH4O29olN+j32w9OcUoMPdTUS+Wi+/7l5w3BkAQa7xzsI+LobwLBC1f9jmAr7gGvD2znwUFJ/G60tWhGMHFH+90rlkZp9nRL5cBwOYf9waADmXiluLk7tR+weNt2D+EPAF1yBv1vRSIzrt97hAjWJZpPF5heWT1WNFvl4vv+w6Pg2AXCPnyD0fJx2Qu9Gjxf5BuwL4DvAF1+DU3BmP6OhfrWgmc5yaJ6v4/h7h8FHj1CzyQJNKmPsvxBRgw79mADWRS+aU66EJ1S31z/lCzpF7Pk46+Aa4FIjaYTbOoQ4BvuCaPbHnFOT835+ktKJm8WzSptGF8shVleXK8wvJxQUTDHGqnZLJlK+8co3z2GP54pVjFYwZxTek/V3y1ycrRQ6sgwEcuX0AmhfmiLlizpg75rDxeYXksasvkdfvaSwta5xh+uFbRlpAzpF7KRwN4FmBTwJRO8w2CfAF1iBrxvRS8ZQjd+ovL1AZ3aGNfLlpvLw9p7fM7f+M9H7yBnkU5Gl24clmJKl+amZDIIUag5qDb7kRYpf+MnbrJnYzo798sVZ+4U7Am9JmACpyFbqbCy6fOWp2YVHkrApyd6PJ4baZveSrbZPk683j5ekWNY0x+JadFpBz5F42cNDHTQejgagdRssErAZ8QTXIkzV24Y8vQYeDKidnkHEd7wM5V4h8u07ky1Xyx5558s2WSfLRihGydXpPmdL9cenYpqmpFJqUKyINzshjdmyRmCQgTaQqphFqFJExxMDy/4YqpeXzDZMw/9+IGKMCSMYAVOgqbMaUseV7/o0xZ+yblDvJ5II5YW6Yo49WjjQ5+2PPfORwNda3RuSbNfhsgjzWvIpZTnh9aQV5xweO8tFyPm46WAxE7TAan8aa7LX/J+XMeESP/dMAxrx2r8i+RSIfA3z9ZInI5zCEL0Ak4uOl8juI9dPOWagUJsrmaT0M8fq1bSlPYXRpWetsubbiadK03Mly2dmxa91ZjrIUrVwknVkH/18dguAIdqKYQxWMup3uu1oOfrDYiD9mAAvlqRtqy0V5E0xMGBvGiLHi/xkbxpCxZEwZW8a4X9tbTMwZ+y83TUIuZpucyCfLMLWA2Jkn5uyTpbEcfrQQ/18uX20ad8QMgGAVUChHilcIksPFgKilsV0G7Ad8QTUocYTv+hNnAEqcj/hKgLj7AJLqUxDtc8xj9wN8/Qxk+5SEW27Obvt07TjZNKWHzO73nPR/rpV0vP8qeQQl6T2NLpJba50j11xcXK7AvLTe6bnMOi+EAC4ukCAVMRJSADxZhp9rFUGjqIPRxtfnYxLoq5bnsaoI25gnQab2ahsb/T+jmcYM4Omb6hqhMyaMDWPEWDFm/Z9vZWLIWDKmjC1jLJ8CjLnJwSqA8cdnNIB9+A5zFeDoGsBpeTKldLegfUBVIGppbLzJ4i+AL6jm9N/zCsXu9e9LzuEg3gBIHscA9LMAMAJUA8YQSDxDTCUlCY6R6EuMdJzvfrsJrxvl+x1z5KNVY2TjtJ6ycNgrMrn7kzKw3e3y5kPXyit3NZanb6wrDzerYkY6ni1nqohz8sulZ+aVihgVOVKWzwezwHuWzTQMvvLaevY9AHdcomSmAI8EuCwu010H1+n2gX1i39hH9pV9Zt+5DbfWPkda1y0jOxYMEfl+K+IE0X6+Rv74aJmsHPuGzBn4AmLSC7EZixjNNbEyMfsasfuK1cJaxBTx/By/M0YbM1sTd+bgY4heofkJxG9BA9h45A3g7AKZU9oR+DVwFRC1NDZeTOELqAGf6c8bNh55A2jzN4k+hAHsI1wDIMksDPkofhdKUAVIS3zGkQokPgBCcxT8dnNMDD9tF/nlXfP5ryiPv357luxaMkI+WD5K1k3uIcvHvCFL3+ook3s8JeM7PyYTuj4hvZ66Rd544Frp8nAL6Yiy+skWteWhZpfIw1dVMXi0eXW5s0E5szOseYVi/whcBpfFZeryuS6uk+tmH9gX9ol9Yx/ZV/aZfec27Fn2luxbPVZ++xDx2g8xswJgPD7H6zcQ+ndbEBO8HlgfEzv/RpMwrzBUHjUIRO+AsTc5AIK82NwZqAEgh5/RAMbDAKoeUQMoWzjFh4jwjsH3AVFLY+sO+AJqwMc3XVIs2xG9+i8wAM7/PwaBTAWgBuASyzGAOPGTmGEDICyJP3NhCW7EAFAYFMCXGyAKCOIbGsTbIj/AIH6kSewAdor8+h6+u07+2LdS/vxktfz+4Qr55p05cmDLjABfbZslH6+diBF3mOxcNPwfgcvgsrhMdx1cJ9fNPrAv7JPpG/vIvrLP7Pv322Lb8jUQiD8MjQWWQ8ErNG5xsVQDsDE3JgAEBqCwBmBM4OgZQAUMQhyMfBy14GHsB4CopbENAHwBNeBtv7kX9ogbwOuYAhgDsMRJVP5T+IQdeUwFoKRUuIQFWLYaUivRXagIUB0kAgXjwRcwigMb8AocQDXxlRWYgv//BtXFdzCQIwEuy7cOrtv0gX1Bn3x99W4X4TMAAnFSI0gkfoVjAIpE4lcDsCZu9gHQAI7cFIDcq1osu3norI+jDroCUUtD42WUyT70k1cAHum7/8QMIFwB0AhIJBW+AqRzETYBI3qFFb8ijvA+A/AJ6R/gc4zOhwPfsg4b7va5CBuAhVYBwZxfEYozxR+YMRA2AJ0CHIWdgLH7A+Qwg5GPow76AlFLQ+M9AJI1gILZM0htJiGUlH+CvysAkMc1ACVUcgagJsC90XHCd0FiEyr6sPh9wkklfAI+mvD1IdUIGwDhip9w42bj6TOAIP5qAjY/NAI1b+IoGkDmyACOeDsVWAH4gmlQOMfRMgC3AqD4CRJKDcAZbeLET7jiV4TFT4Dwnyr5XTEQPsGE4BNkskB5nmb4lpMMfP1MFrq9agCMiSJsAAoren1142/gGABhxG+NPJgCHLl9AGoAuVI+GWgcwBPbopbKdhGwE/AF04DHX00SQkn5J4jtBLQVwCeAET/B9zQBCzUC9xCgi0TEVdGHAQEYI/Dgc4jEJzQDn2D/a/j6aeEVvgONQxAX1yzVDBxTDWLN2KvwXTi50hx+vuLI7wMAOAgVQjXq46iDOUBuIGqpbOWAdwBfMA1K5T1KBuBOAQLxK1T8gJIvgJKSBA2Ln3CF78IRAgVihO9CheQTXRLYv+HIw7eeJOGIPwC2xWcCicTvQsXvINE0IGwAKnyFNYD9qAA2H+EKAKABcDrq46iDWUAuIGqpbOcDyZ4GzGf/aRLCiTlcxBsAyBNMAfh/+5kBiZaU+K0BGBNQ8SdjAip8RSLhK3xCA3xi/bfg608iaP/dbQPUCJIVv4tkDMDAZwBq3NYAjvQUAIgM4Oi0isAuwBdMg9L5/kUD0IogzgCAOPETjviTM4Fg3kvxKygICEURJ34FBOUT4rEEr/hdqPgdE4ib/ytCcTMnVKkBKJIwAAMVP4EcBlOAyACOh8ZTJ38GfME0KFMoyxEVP+GfAqgBhMSvO57ixO8xAEJJHAifcIXvAiIJDMAKab8PVnDHHJw+hk3ANbhgezUOYSOwMYsTvwuNu0WQFzUBR/yRARx3rRnwE+ALpgFvzXwkzwEgAgP4BIRxdwIGBuCSjAZA4ikJ3ZHJIaopXV3xu4R3RW/fB4JRAVkxJYkNSWDj3/jiMOD+3sC3fH3v61cIagZx4nfhmICBGoATSwM3zgRzYM3YQA3AIjKA47L9xwYAwccdBSCRlFQqfkXIBHSuqsI3sCNaWPhx82IVfhgqHgpJBQfwDLxEgCjjsOkIIrxsZ71uv+IE78K3bUAiA7DwTQPMEQAHGnsDFb+COWMObR6jnYDHVTsGDMAlkDUAvQIwzgQc8ccZgJKWROaIFiJ4nPgVrjiscNxRNE5oQCJR+uATc1rhW64L9sUiro92G+K2y4Wz7YEREDZmcSZg4xoYQFj8LpCfIGdqAKgAjAEc+cOAkQEc+fbfGgBHD2MAzuivBhA2ASN+xwBM2a+ktQaQqPRXWPIHUGG4oleoqAiIzkUgRJ94jwacdYb7EtdPp/+JjCC87UC4EvCagMbZNYGw+AnmSw3cNYCoAjge2n9sACBNMH90DYCwBDMnAan41QA4QjkGEMxnFQ65Fa4oeEVdgLDoXbiCgyDD8Ir2CCNunW5/fP11jcBuq4Erfk9sNG6uCSTaD+AagIpfETYA3hIsMoDjoR0bBsDTSA2BQgZAon0CwgXCV4RGf3Nuu4pfYcntkj8QvyMUA1dErsh8CIsyBJ+IUwvf8gx8/SDcfit0m3RbaQAExe8zgFDcjAlYg40zAWvAWpUF4td8OQawKTKA46X9hwZwX0z8xgBAnI8skcxnlmQGlniEOUWV4lcD0Fc1ApD3c4rehRW+AYShSCT8MHyCU/hEqtj8N75IAu53vMtw4Vu/D+H+6zZye60BuEhkAIyfE0uvCfjKf8IaOI08OgpwXLVjaApAEoUNwBIuICANQEHxWwMISGwRJntY/GEkEk9YXFaMwWhthXxU4awvzhAI7Ve43xa+bQxMUGNiDVJjFgjfMYAANvZx+wE0R9YE9NZgxgCiCuB4af+9AQQ7Aa0BaOlvhE84FUCcCVD4LkhkHdVc4YcRFgYFAxjxuKIHXBEG8InVxZY0wrcMH+z64wzAhd2ORNuncGOg8QHUAOKMwCN+RWAAagLWAHQKEFUAx1U7hgxAieSKXxE2ABDTd7svFb8LrxFQEFYw7sjpCipO9AqfMAmfsP8p3OV7+uL21cBuj9cEwttPaIzUNBk/jaeagCN8A8eM1QDMVIDitzD7ACIDOF7aMTIFUALhfVBakmCuAVD4CpLTJ36FkluhpA8JIyz6ABSYis0VYhI4kBQg5GQR+r5v2YmgBqBw+83tARIZgU/4LtQAHBPQCiCR+DUnNj/BvgDmD0YeHQU4rtqxZQBx839rAkq8RHcAIklBWCKRAagJgPCKONG7CIleoUJzERasFz6hJwXf70NI0QAIjwkQgQFYE/CeHuzGzDEBNQCNuU/8mqewAUQVwHHT/kMDsEcBeJw/zgDUBByyBcJ3xe/CGkFwzT+IrXPcJEd+worGPeHGwIrMwCdCCyPSsKi3pgHh3zqIW5fbnzBsf+NMQMFtdLedsSCSEH8QR8JjAnF3CLLi15ypCfy3OwFnA9ENQdLQ/lsD4Pzf3Qmo4jcn/xA0AJIO5NNDUgEpkxB/nPDD4lfhO0hS+IQjQq/YffAJPSX4lqNw+hAHt5/ONgTCt0hyOuCYQKJ7BWhsQyZg8mBNOTABR/zE/pVHxwBK5JD82VI0gKlAFiBqqWz/7RQguBbA7gQMRn8VP6HEU7gm4JA2GMlIaiW5YwDucXFXIDpaJjIAiEzL8ACuMH1CPlJw16Nw+pHIABQh8QewMfCagI1bnAmExB+cD2ArAJMXmydCxW8M4CjsAwD/eE/AbJlSvCkob3EftTS0/8wARnXgTUHVAPCq+wOUVO78P2wAvG7dEJTCd8VPhA0AiDv7j4JQqPBTEr9PkGH4hHw48C1bEepXnBFQ/C50G3W7KXyFjUuwT8DGLjAAwhW/G381ADUBa9zGAI7STkBrAKm4K3BvIGppaP+JAfC58ZO6PAKCkUQkUNgA8LkizgQsIXX0D3ZaqfCBRFMAQkWgoiAoeiIk/DBSbQAKn6hTgm85ycHto9t/IJEBELr9bkyAsAEYqAHAYDXeceK3CKYAhM2fVgCbxsujV/8nzwXoA0QtDe1fNwA+obZykfSyesxrYp4nr8QxVUCoAghMQAlI4Tujv4FLXiCRAXDECxlAMOqHDYBwxRVCnAh9Qj5ScNdDcN2hvnjB/jvbFq4E3NFf46TGGRgBDdUaQKIKwBW/grlyTBwG8DUqgMevqRYZwHHQ/hMDqFQ4QdZPfEPkG5BOdwCaHUlKJtcELPkSHQGwRA2g4uerJbmOeq74gxHSwmsChIoKAjSAMBWJBLvNjy+Tge/7iZYLBOt1++IK3gfdLos4E1ADUDBeFL6F2RegMbYmYIwA8Wcu4oTvQE38s2Xy6/uz5fV7rpTKhdN5OZBWRAZw9Nq/bgC1TstmTGDL1G6xCkCJE2cCBEgWlJw0ATsaqQl4TwKiAVDwCgrfFb+ColBAMMFxfxWWI7YkR3ufgI80wusMG4ALuw1x26bQ7dZ4uEbgGf0JnwGY0d+tAFT8zJvN4adL5a+PFkrPx1qYx5f7OJBWRAZw9Nq/bgB8Dj4fh/3+gv4gNAgWVAA+AyBIPGsASkZjAA5ZSV4taQNiO2Q3UBEQrjjUAEKCSlL4PoSE6xvxfQj/zrtsHzxGoNsRt23uNis0Hj4DcEzANYDwNMA1AJMzxwD4tCdUdhPefFAqFIgM4Fhv/7oBcAfgE9dVl2+2TAT5QKbgmQCWSCp+hRIvmAJYAwimABS/zwAUYQNwBRIWvwqL4vchJESfqP8pwuuIQ6g/iUyASMoENA6A7hdRA1AkMgCNNw2AYDVmERgAoQZO0ADWycpRr0qDM/NIzeJZvTxICyIDOHrtXzeAi1EWDn/pDhAOJCJhzL0ALHmM+F0DsGQLzkRTQroVgCt+hSN+RYriDyEsNgrwS4VHuHF4O5Xw/TaM5MSvcPqt2xW3rdx2JxZBfBQaNxoAoaO/moCNf6IKQE1AKwDmEvhipexbPkxa1z3X7PD18SAtiAzg6LV/1QCqn5pZGp1TQDZM6BSb/8c9FVhHf8cAzBSAJuCIXy9WUfG7SNYAXEFQ+A7cPeyJDvupAAkIkgjE6QrZFbaDr5JAou+6y7VIcmqgfXP6HQe7Xa4ZxJ0NSITFb6GxjDMANQFryMF+AJsvrQKMmQMfL5ZDnyyRrg9fg4qPj5fP7uVDahEZwNFr/5oB8FTOSoXTyUutGsrP788GkSB4GkDcUQDHBALxA8Hor+JXA3BNgOS1CERPgPjh0d/sGbciUdEkJ3yfOOPgE7SFT/yE77sGvuVbuH2K66v2n9tityuoAvTVxiFsAnHzfxeMrWMCOhUzObEGnVQFwLweWCk75vSWay4uJlUw7fNxIrWIDODotabAUTcAjgAVCyXIzdXOkA+XDgERQaJg9FcDAMKHAI34XQOwJhAYgUtYFX945LfEJ4JDY1YkcQYQEn5Q7is8gjTwiVixPRn4vk/41uECffGZQNxUQLeNwlc4cUhkAK4JqLGq+AkbezVkY85W/HEGoCbA3AKfLpHh7W+X2qdlM9WfjxupQWQAR6/dAPwB+IJp8E8NoNZp2c2Ov+YXnSIrR70CUkLMHy342wCMCaRkACp+BUkJkpodga74fQZAgPSB+AlH/IlGfoUVWZLid0XrE3ha4S5PEV6n9sf2LWwEgQGETMAYAWJgxE848Ym7RFhNwBW/awDhCoB5ovjDBmBN4JPFcnDXLOnxyLVmEKh2Suw5k2lFZABHrz0K+AJpkC5dglQ8JZvUO0wDqFk8m1Qtmkla1zlHtk7rChKRIBS+YwCmAiAofjUAO8IYstnSM1EVQANQwrriVzjiN3CF74qfcIWvUHFBeC68QvUJ+nDgWzbXGeqDK/owUjKAuJ2BBGOlBmBNwMQ2JH6FVgGBARAqfkDFT3w4H8tbKr/BBAY9e4tcemZe8CHtZwdGBnD02uOAL5AGNIAqxbIflgHUKJbV4Lkba8kHSweBPBwlIHySQglCBAYA8iihAvEr1ARU/EAw/3fIqzuyjPAVGwAKIDkDIFREKvyw+CFGIhCmT8AWX6UCvt/Fwa5H1xvXF+2fI/y4bQFcEwjvBwhMwDFMFb8xACJsADYHQU58BkDxK6wBEHwG5IfzZF6/tnJNhVPNmaA+ziSFyACOXnsM8AXS4HANgCf78Phvl/ubyXfvTAbBIH4QIAaO/hasBpI0AIUSj6OPGoCK34UaQFj8gI6CBmoAhAoFAiISiZ6wAjQjMV/D8InXwid8he/7Br51AEmagGsEKn53Gy3UBNUAjAmER38FKwCtAqwBxInfGoCagLsT0FR4Fsw1DWAv8r4Pr/uXyKZpneW2WmeZyjC1RwciAzh67YgbAIXPeX/3h66WX3bNiIl/79wYCYwBsAKgAViSBOIndApAkFwgmjkXAOQLRiI7MunOqvBJQOYUYJI8hOTEHycmBQXvQsVohRon6HeOEOzyEhlCuC9A2ATUxILtcrdVzc+NCQ3AMQFjBGqmrgEAar5xo78Vv4FrAIDJrRW/AfJODnwIHFguH6Ei5LSQ04HUmEBkAEevHVEDYDLp7C+0rCO/7Ib4PwMZPpgdMgBADSCoAEge1wCUYNYAAvG7BkC4oz+JTPHb0c1Aya6jn0/8CkdMBlZkgfCBONErfEI+XPiWD7h98InfRVImEMTCRcgAghOBHPEbOAYQPgdADUCnAcE+AI8BGMxBf5bI1ilvSPPyp6bq6EBkAEevHVEDqHZKZlPefb5yCEik4kfC1f3dCsCIP2QAZkegksoagBlx7BRATcCInxWAJayO/sYAXBNQolvxGwNQcQCmZPYZAIUfEr8BxRgSp0FIyF+nAuHfJIJddlwloPCZgN2OcCWg2x2U/4TGx0JNwJT/SRiAMQHkITAANQHmLIkK4CMVv8IawIfgxb55MrFTG4g7p7lAzMcnRRoMILohSBrbI4AvkAZpMQAmkcmc3+9JEAWJ34skf0DxWwQGQGKoAVjCBCZAIqn4gUD8FoaIFL9jAMHo5ZpAWPy+0d8iLH4tseMER1hBeuET8LspwPcbwrN8rwkAbr8NuC2h7SMCA/CYgMaMcA0gOBSo4ncNQE3AGkBg4GoCWgEQzDmFrwAXPpiF78yVX9+dJE9dVy3Fw4NpMIB+QNTS0J4DfIE0SIsBsJR7uOnF8vuOyUguks4kmwrAVgHhCsCMEjQBNQJrAG4FEMw5VfzWAIwJgKRm/u+aAIlMUhNKdEt8txwOxEHBuAag4ncMIO4MPkeUiYTrE3lqEV4W4a7Lrt8Vv4Htq2sAcVMAwm5zYABqAhonxkzjpwZAMMY21ip+AxW/NQBj2oRr5kkZgB0MyI29wMfzZfWo9lKvdK5kqwA1gFxZ0nt56mACkBGIWipaOoBzJl8gDbLBcaulwgA49+ee/xXDnkdSkWgjfptkNQAzDQAZjPgVagIkDcWvBqAEsyYQPgfAjFBaAfgMAAQPDIBwDQCi8Jb+rvgVEFmy4veJ2YcdIfi+E4a7PsLpiyt+Im46oCagRhA2ABsbAzvyB/Gz8dQqIK4CsOJXJDIAd/RXA7BTAAMVvzWAD2bi9/Pk563jpC2qgOTuHqS3BS8UPRfgiDYaAEsmXyANcsNxqxdP2QBqFMssreucLV+vHwFyIOl7kFwm2FsB0ARCVYA5IcgSKTABwD0MGIhfoUTlqEUomWEA4SlAcARAR0UVP+CeXeeKyogMwjOA2BVxo7TCivbrw0Qges+yg3VrX4iwCTjboCYQNxUIGYDCNYEgjo4BxE0DbB4SHQGgCUD87vF/1+Q150EFYAeHPdYEPp0vC/o/hQoyS5JHBNLwXIA5QA4gaqlomYARgC+QBiy5UmMAvMhnaLuW8ucHM2yC8WoMAO9pAHR/1wDMyKAGQPETOgWwBqDHmY341QBcE3ANgAROwgASiR/wVQBxwnfF7yCRQF3xAl9jdD8cxC0nvA7A15ekTCDYLsA1AMYhzgRsnBgvjV+cAWicUzIAGjcNQE1ADYA51gpAxU+AD+QGB4k9PFK0AGnvJy1rnGF2Ivv4lQYD+BZ4AIhaKlob4EfAF0iDnKkwAC3/12AuJ/tBgD3T/zYAM/q7FYCagGsAgBqAEb9WABS/lv+uCZCQYQMAgikAoOKPG/0t4sRPUDQQkAEEZU76cQwgKMHDwlTREx5Rf70zGfi+b5GkEdj+JDIA9ln7b83MbJc1Oq8JWAMI9gH4DCA0BSBM+W+NWQ0gmAJQ/I4BBPN/hTUBVoVmcLAG8OFs+f39qdLx7svNvSJ8HEuDARC/AHcDUUum1QA+B3wBDJA3a3qpkYIB8Lj/7Sj/P17cF8RAomkATKwxAFsBBO6fhAFoFeA1ANcE1ADs/F/h7gOIOxHIMYADKn7CJ34LM5o6QoszgJREf6Rg15HICGxf1ADUsOK2wa0C7Pa6BhgYAKDiZ+zUSDWmarQm5jb+pgKwJqAGoBVAMA1APo34dfS34g+qADUAcIQ8Ib5cIhNev1suLsR7ByTmmBrAyTkzennqAblNjkfN0woAiwFf4OJwCgLOva/cCxtOiqJioXTS4Y6GcvDdCUg+XT0FA9hrS0NTJqoBAIEBKLHsSGMI54rfYwCmArBkTlT+KygEQsWhBuCYQHgKEAhfoeIPwyPibzDSJwXf9w08y/aZgBpA3Ojvwt1GNQAbB9cAAhNg/MIGEK4AmAPmAkg0BbAG7jUA5lzFHzYAcGU38PkCWYcKsknZwmZfQJhjNAC+lsybycvTJECOk+tRC7VkrwB0kStDQooVAG/8OLL9bSATEr5nWrwBBEcDXAOwFYDuJDLitwYQtw+AUBPgyOMaAKBHAQgd/c383yE3T301xKcAHAMITMAagJk/q/ABHWnVAIITeFSYKla+UtBHCkkZgYrfhWMCuh8gMDRAtzPYbmsAJiZOjHQakKj8VwMAgsOAdvQ3FYA1AZMzd/5PqAEAgfg5EFiQG2b0J1fAmX2zZf+KgdKmYVlz6XiYY2oARbP7eZoMyPWoOe0kYDvgC1YczjurlLRsUkManp0fZZn/GC0v9+Xrwv5PgKQgwV4k9UMI/iMkeR+Svg/JNwARPiZADN411jwTEOAVgp+CRJ8RINTnIBnxGYj2GV9BPAMK3iJsADp6BRUAoQTnaMey1zGAYApghRI36lskKX4VqMIn4iMFXYfPCGz/3ErAQE3AGlycAbhVgMbH3Q9g46gm4MbZvBLIxecEc8NcMTd8j9wRzCVzyrs9Mb/Mtcm55h/mz3NEyA2eDUiufEjOAJ/NlVdaN/DfSdgeHWh5aXm595amUubMEl7OekCuk/NRs+1BINkbgBD3tWwiH6wdI7sWDpCrLzolyb2znP/fVKWUvDv9DTn00Sz5Y+dk+eO9KfIbXg++O0kO7iCmxPDeNPl15zT5aNkQeXdWT/l+G/62a7YcfH8WgNddc+TgbmKu/LF3IbBIDn24WP7aZ6cA+0HEL0DQr0Dcr0Bi4gDemxGNJMbfgtOAleA+A7DiiDMAxwSCY/4qNggvgCt+F65w34vhm2Sg3zFwf0v4lq/r1z5ZE4gTP+FWAdxGFT+gcYibAqj4AcaQ8SQ0vow1Y74fhoAc/IVRnzn540PmZ6HJlQHzaIBc4vX7bZOR414m18w5c39wx1QLcAMgR8iVP96bLIfem4T1LJAhz94s1U7NnOikIP6/XumcMrvvU/IbqopdS4fLHS0agaspnhlIrpPzUUPLDCwBfIEKcGW9yvL920jI1yvkz90z5LqKxY3Q3YQoOF+7rmIxef2uy6Rdy9rS9rqq8hTQqvbZ5g5AvPb7bxQzaHrBSXLl+YWleXn8/eLYZwZ43xzfu/bi4vL4NVWl7Q215PmW9aXXkzfLoHZ3yJwB7WTV+Dflg+WjMLBNlW/emS0/714kf/JBoV+CrD9AFN9BBN9Q1CQ8iW7hTgFcAwjEbw0g7oSfsPgJV5Qh4SYS+vvJIPxdIG55znoSnTjkGECcCTjbEhgAkJQBMEaM1ff4LWOHGDKWP+9ebGLLGDPWjPmcgS+YHDAXzAlzwxwxV3zGg5tDk0fkljlmroO/OVzg38kRcqXttVXluZtqSr+218uT11xieBU+H4CVJs8WXDmyPcwKVcUXS+XbbRMNV30cDoGcJ/dP+FYH2A/4gmRwykkFZd3ULijjULZ/MEP+hDu3qn1WkhWAggmraW8AwkOCPC2Y53f7wL/ROLhM398JLiMGXlqcLRgReK1BnVI5QK4i0rruefL8rZdK/+day5ReT8u2OYPkw9UT5Ovt8+SvzzCa/QKh/QRh0RR0JHTFr0hS/ApHfF7xh4ScCCkI30VcRaDrAtw+GGjftL+OAbjbZgzAmgBj8C2+x5j8skv+Qun/9TvzELPxJnaMIWP5/K0NTWwZY8baxByx1zxojglf7mKI5TglHmiONc8up1zQAOqfnls2T3gV0wXuP5gGjs4zXCVnfVx2QM6T+yd8aw/4AmSQPn06ueeGS1HqYV62CxXA7kny185x8myLaiZBSZ2lRfBvLnzfSQvCy6t9WvaAgLzfAEHyGIKhOqlyUnrz/xZVSstj19aQ3m1vlRn9XpANM/rLF9sw1/weovl1D14pJorEmoDZ+WcNwMynHbjij5v/W4EGe/UdASeCK34Xvu868BlBIvFbBH1WE3AMgNvKbT74gXllLBgTxqZ329tMrBgzxo4xZCxVmBpnjTtzEM6LL3dpQXh5SS2T/ahXOrd8uWqAyN6pIu9NiHEUXCVnyV0fpx2Q+yd0ywlMAXzBMciZI6tsmNgB7jpRZOdYBBnYMVq63neFJcE/T/jRgEueGhh1qp6c0dyBmDuTGqMMfaDpJdLtsZtk+oD2smvFuJhIft6FyoCiptBVPI4JxIlfhW8RJ3z7+i1EHQDLTjXc3wHBMh0kVQ0Ewnf6z23hNnHbuI3YVm7z9IEvIQY3m1gwJowNY8RYMWZuDH0x/q9hzL3yafLN6r4YmMYbXsrOMeDqBMNZctfHaQfkPjVwwrbzgHcAX3AMKpU7XX7bNgzCR3DfGYkgA++OkBHP3WCIcawaQHIgcS4pkk4q5E+QuqVzyk3Vz5K2N9aTcV3byt41GEmMWCCwbykuK6pgdKX4FVZ8gTApVIKiTQkQYiL4vqfQZasZ2HUb2P6oCRjzoujRX24Dt+Wb7bJ37VQZ162t2VZuM7edMWAsGBNfrI4V+E4EYgV632Xnyw9r+oi8Pwr8HAFugp/g6m9bhxnu+jjtgNynBk7YdiXAUyR9wTFod09TBBYG8O5wke1D8B54d6jM6nyHmQeyCggnhsaghwKPdbCM5AMqKmHk4w6lFpeUlnatrpCFb3WWH/asjAntx90QEkREA0hR/IqwgB2he0d9IFkjCC8f6wwMgLD9Yh/Z1x+5nB1mG7gt3CZuW73Tc5lt5TZz230xOdagUw73MxoC9ye8eltN+W0DKoB3wdG3wU1ylFwFZ8ldH6cdkPvUwAnbeCjEF5gA8/s/glF/KII7UGQb5lrEOwNl7dCHpT4EU9OTGO6YqQdz4KEb92/HOkg0lr+c7156Rh65ucbZMurNJ+Wj9TPkTwrr1w8xmkKMgehSEr8VdCByGklqYL+fyAzC63FNAOL/Dp+hj+wr+8y+cxu4LdwmbtvxInoFdxw2OCO3Oa38srPymcGFn/O1Ggxg6FPN5K8tMIC3wcut/cFPgFwFZ8ldH6dDOKEPB74J+IJikCtHFvlw9isI5iCRLSizNvfGK/BOP/l45gvSAKNJeKTnocG76p8rw567yRiB7wyuYx2mgkFJbHZ8Ac0uOlX6tbtbti8eA3FBaL99DIFCbGoCcXv8HcEGwnfhE7wPzm/cZep63J2B7Av6ROFvXzRa+j1/t+kz+85t4LaocI4nkDusMse+2lrm9npYrjyvkCn7+TdjAKdkkdlv3grRg5vk5ZZelqP4PzhL7pLDPm47oAZOyJYbGAv4gmJQpVwp+XLR6yirENBN3WPY2A0O20u+mt9eGpcpkMgAuBPp0WYVRD6eIbN6PCTXVyxuPiMR3e8dLzBmgFGTZTPPfuz59O2yY9UUkZ/2Ah/ERBgWftyo78KK+7sk4BO/Im75NABrAuwDwD6xb+wj+8o+H4+iJ8gVcobcmdn9QZEPpsrOmW9I03JF8LfYvgpuW2VUNJuHP4SSH/zc2DWGTeAneQrOfrnoNcNhH7cdUAPUwgnXSgBrAF9QDJrXu0B+WPYaSisEdEPnGNa/CYftKt8tfFFa1ywZOLImhcdyX25VV+STGSL7ZiBxnaXHQ83kqgtPlkqF05uSriYP3+G7/D6nDLqDh1d2BbCf8zv8roE57AdBAjWK8dizD3regQOYFKcq+vu/1x1bv/Y/JVBUlQsnmLl03+fvkc+3L8bI+wnE6wg0TrgU9p7Dh88QdD1c58FPTB/6YsRnn9i3tJT4Gt8gxkF8EUfEzBtLAzfeMZicOvH9O8Z2PVyfxd/rJmLf5e/JDXKEXOnx0FXgThczkMiH02T3nC7mc36Hv+fAc0WZ/LJ70lMQO/i5/o0YNoCf5Ck4S+6Swz5uO6AGqIUTrp0P7AV8QTFoe2sd+XMNKoBNCOq6jhb8fyf5ecmL8mSzslIdhFBCmePCSGbvR69C0qaK7JoII5iN18lI5psyseOd8sRVFTAnzQ3SZDVJN2IuToBIDmriMwP7HSUWv9vwrDzSssop0qp6cWlVrRigrwA/C3CatK5RQm6qfIpZJ0kTIzjx97q4DYT+ncTUbfKh6skZzJ7zuxpeJLOGvSG/f/EORuEPRb53xUrxh+EReZJwfueaAIWPdXGdXDf7wL6wT76+KrhNZtud7Q1ijVhojPl3xooxY+wYwxhsTINY2/f4jLlgTpgbNRLNm+ZR1/U3/v4OucB1khsTO94lO2dBwLtQZRnugEN7JsvPW0bI9ZX+PvuUlUDrmqXls5nPYkAiPzFQkZvKU3CW3CWHfdx2QA1QCydcuwhI9tr/zg9fAVdFYIk1rwKvAC/j/6/K78tfkE63VUEiWGrGSKZ7age2vS5mAO8jee9PQgk3LZbMPRPl+2Wd5KPxd8nKN66QMY9cIv1bni2dbjhHHm5QXB5tWEIeufQ0eaJRKenV+kLpefuFMvrRqrLwlUtlWcdGsntIC/lgxE3y0Vu3yBcTWsuBSXfIgYl34vUu4G45MPkeOTDlXuA+OTD1fuABOTDtQdk/5QGs8175YOy9snlgK5nf6VoZ92xj6XJ7VXnm6nJyfYWT5fKz80rDM/OY/nPnUpWimD+DmCQ0PwuD28qTZDhHbdfqStm3eZ4ZkeVHTA3UAAJA0MR3KNdThP2ugbsMLJPLPvixWVe7Vo3NutkH35EY00f0ndvAbeE28TNu4+Vn5zPbzG3vcns1GfdcY8TkOhMbxoixYswYOxNDxpIxZWwZ48mItYk5cafJxUdv3Wxys3toC5Orha80MLljDplL5pS5ZY6Za+acuR/zSGVw4XLDie+XQbg83+TTOTHOkDvkkD35jMf81QCqYpueaHaBfLPgBZT+4Odq8JLcNBwFV9d3MLwlh33cdkANUAsnXKsPHAJ8QTEY0/5akc0UPwK7+iWgvciqF/H/l+TPFc9L/wfrxhkAnbzhWXllZrf7RPZOjp2ZpSbAVx6eQfUgq58QWXKvyISr5c/RDeSPiVfKr+OBCU2Bq+TgpKvl0LTr5ND060Vm3SgyryVwm8ji24E7gbtiWHJPbDlLsb6l94sse0BkOeaMyx8WWfGoyMrHgMfR5yfR56dF1j6DdQMr8X55Wzm05Ek5uOhJ+XH+47IPpF/S5Xrpf38teazxudKi4slyJUpMjlYVi2SUqqdk8ZpBbO96BmlZu4wsHt9HDlGsP++zok2L8D1wjQDL5LK5Dq6L6/TtV2Ef2Vf2mX3nNnBbuE3cNm7jvvH3YZufwLY/hRighF6BeKyysVkHMFaM2UrkiTFkLJc/EovrMsR3GWLNmDP2S9oAdwPMB3Nzh8h85Io5m3UTctgCuA45bS6/TrwKaIZcNzE5Z+5l/NWxZZATm1DCvzsixhVUja4BHHp3jNxa8/RgCnDJyZmkw63V5Jfl4OY6cHMVjIDcXE2Ap+AouUsO+7jtgBqgFk641hDwBSTAwu5I4jY46cp2wPMWz8VeVz8vU9s3MSMLy34mpQbmnzSAWd1BkDgDAHiGFvfSrkDJthDkmQ/CDKslMramyMR6IpPqi0wGISY3FJnSCLhcZCoqkKmNgSYi05oBmFpMay4y/RqRGagyZrYAyW6ImcTsm4FbRObcKjK3FdAaJIRhzAMpua4FMIsFICzXvQj9WwIiLyWhQe4VIN8KEh5YAUFADN/MfFBWdL1Wet1dRR5pdKYZMatjrluhcEZsc/w8m9vPfR8Nzsgjg15+SH7+fDtG6s8gYgjXlPMhYX+PkdyH8PdMNYBlYFlcJpfd4Iy8Zl0acwX7xL6xj+wr+8y+r+h6jdkWI3Bum9lGAtu8DOJmDJY8BOHCPBchNgsRowUQ5HyIej5iNw+Cnos4MqZzIOzZ4ARjDXGbuDMHM2HU0zFYTEdupkHQJldNkbcrY/ljLplT5nbypcg18sycj0P+h9aO5Yd5ITe4N59cMeIH3qMZTEQFMFba3VTdDDLc9kpFMkj/R7DMtRigVoGPK8BLIuApOAvuksM+bodALZxwrQHgC0aABd2Q5C1wWCZmBQkELOfoydH0eVnV4wYz2rgGcBkMYE53kOkDOjgNAHhvvMj2obE52hIIjkSbBhINrwYSwABIBPOq74GJIMaUOkBdkAhkmQ6DmA7iTAOBpoNI05H8GTCIeSDaQpBuEUxhHgg4G5gDMs6BQcwGMeeAoLNB1DnYljk0CBCYRA5MgiMWDQKEXwiTUINYClFw5FsDsSx+WLYPvFHeeqy2PN30HLn2osIYYTOhtI4/vMYRuTpGqNcfaCH7318j8sf+eEH7RO+DawBYBpfFZVa3F8vo+rhu9oF9YZ/YN/aRfWWfTd+5DdwWbhO3jeKmGXKbaY6uuOfCPE2MaKY0VRosYjgbsZzF+CKui/GesV4AM54FgTMHM5iLy2J5YX6mIVfTkDPmbjIwAbnUvAa5tq/kwDTkhpxYir6SI+QKOcOBw5zfPwEGMCZkABml70NY3yqM9raiM7xUnhozedlw2MftEC4FTrh2BeALRoD5nUGAzSitlmHkWAYyEUsxcixFWb3+OXlnaCuUYpkDEXDHTJPzC8vmsSjD9tC9kUQmkOdo09lZPSzCaMQRpz/E3bWkSL8zRAadLzLkAuBCjAgXoTK4SLZ1O1+6Pn+O9Hy+jDz/+Lly931ny5OPlJEubc+XLs+Ukw5tL5Q32lWSpzGPvOfSU+XOeqdK/zvPlW2dqsmmjtVle9fasrt3Pdnb71L5fEgj+fqtxvLTuCby+ySMTjNgGDNB4pkgNAnOKoIGwZHNGAQwl4KwBrEAYmHJu+ohOTjrLnm7X3MZ90QtefjSUmYnFkXIV06FeMyde+Pb3nyZfLJzZawS+J47CF2R8/8+hL6D33IZXJbZw2+O52MdzjrZB/aFfTo4G4Jejdguo9ApcHf0pri5bRS4Hb257YwBY4GY/DnlajmIMv2HMU3km5GNZT/i9unAhvJRn/qyt1cdGf3gBXJ73VPk7kuLycNXlZLOz1WWbs9fLE89eqHc/UBZeebR86XjU+fJc4+eI+2eKCPd25WRPi+eK9/1Kwehl4/l1uQYuR5cNpZ7cqA/DMJUIeAGOWKqAHBG+YNB5K8dY+T5G/42AN4ncOJLMKONmO8vAx/JSXJTeUrOgrvksI/bIZyQU4D7AF8wDArmzSYbBkAQ6+GqJrgoF5cQcOklmA+uflI+eKuVNCtX2Ow51hGw2YUnya6ZmOdzZw4TuHMcHH0YloPPmJjFHI2Q6NfOFnkqQeSF9CIvZxV5NSc+yyPSsYD81rGQ3NU6v6S7NK9kbFBIEmqdJAnVi+K1mOS+rKSUufoMKXf9OXJW47MlIVtm098cmdPLY41KSJ/bykivW8+TAXdeIMPuLS9vPVhJxj1yiUx8vIrMfLa6zH+htizvUE82dmkoO/tcLgdGNJbfJzSWvyajVGXZOgMjGw3CAOLQqcZMCgbCmQchLYGwMN/9cvwNsqpTQ+lyS1lpel5+lOGxvds0Qpbpd19+sby3dhZG8S9EfnCF/lESsCbA7+I3/C2XEbtMNotZNtfBdXGdXDf7YObfS9En9o19ZF9n4nP23WwDqiKzXcD0ZnJo0hXy4+hG8t2YpvLp0Kayp38TebtnY9nYuZGs7dhAVrxaXxa9WFtmP1NNpj9ZWSY/erFMeri8dL35XDm9cPaAI0UuOkUuuLGcFG96lpza5Awp3bSU5LnsNOSJuTpJMiB3mS7Li6qksMibRUxu5bW8Ih1yIefZRF5E7tuCA+QCOUFuLIOIyZV3UAWQO6YSgAmgAnj11prgWswAKhRMJwu7wdg4RWW1Q04ablqemkHqacNhcln7nAROyLsFPwn4gmFQvEhueXsIRgtbAsdgXdo49aPyydhW0rJKCePKgQFccJK8Nx1l3G4aAK8exHyOp2lyRyJNhGXeFBB0EOb1M7C88RiFRqKEHIVycgxG5/HN5duRzeXMe+pLQsvmkvGW1nJqmwfl7Gc6Scm23eSUJzrLGW27SJ03hkipeg1NXzNlzCjXNq4rrz3RSl56tJW88tCN0uGB5vL6Q9fI6/c2lo73NpIx7ZrLlj43y+4hreS9wbfK4k5Xy1uP15NRT9SVSc81lNb1S8oDjU6VKW3Ly/4Rl8ov41FeTkQ5O4VAeTuFc1n0eSrnthQSRDUXfZ9/o/w+7VqI6HIZdt9FckOFQmaU5k6q8vkT5PEW9eTArrUiv8MEUjQAAn//7Qs58P5a81sug8viMrnsYfeVlz0DrsA6IW6sW+bCnMzcG31i39hHmtlU9hnbMKmhHJwAoxjXWGa2ryoPXVFc6p6fXzq1riRLOjaTWe0by5R2V8iEZ6+QsU9fLqOfukyGPXmFDH76ahnx4s0y/JV7ZHD7O6TXc3dIv1celDeee0BOLxW79Vb2AgWk0iPPSN1OA6Rq++5S9JFOkqHN65Lh7vaS4db7JOHmWyXh1quk1bNXym9jYUTj0M8x6Oco9HHk5bHczwQHyAVygtwgR7gDj6f0cl/AezCB91kJjJUhTzUFz2KHGSsUTC/zuqCi2YQKdSGmN8ZArIkoX8HdtwffYrjsctuDp4ATrj0O+IJhUKxwTtk2CElZjYQs4g4iCwabWPagfDX5Dnn48nPNfgCdAsQM4DUYAF0bBsCrszb3RPncLpZc7nwagnnhKiT4rz9FDv2O0e4gXhW/yc4vvpCcHfpJwnPDJeHpRZLh2YVSuMMKKdtzg1Tpv1lqDt0u1XqskLxnVTR9PeWUU6RNmzbywIMPyYMPPSwPP/KIPProowaPP/aYTJs6RQ7+8hPW8yvm1RDY5xvl0J658t7MrtLjyRbSsc1lcn3dcmZZWbJnlRrXN5DHO7aWcT2vlg+GN5JvR2E+Ox7z1Uk1MJ/F1GU8/j8B89wJ3JkFkXFn5QyQeFoTOTCkgQxvc6HcU7eUIWv5vAny5I0N5Pt9W0V+5XTACv2HffFQ8eM7/C5/w99yGVwWl3lgCOfXWA/XxXVOhMhpUqYvwET0bRL6OaGWfI0+bx14qfTrfJU83qGlNHniVslX/GSzjeeUPFkGPHWNDG13kwxsd5v0e/kB6fPak9Kr04vS+Y2O0rXLmzJ71gzZ9f778tlnn8muXbtkxsxZ8sqrHeTFF1+Um266SbJk4Sm26eS0y2+X2m/tkjO7rpMSb6yRs/F6Tre1Uuz1FZLl+fnI4WA5r88o+fanH0xu/84zgdyTA+QCOUFu0ATIlS3gjLmyzxoAzGDks1f/bQCYAszrDAPYiCkD92lw/4YxAsJyFdzdNvBGw2XldRJ4AjjhWlvAFwyDogVyyNYBGF1WwlG5Z3hhmxjM3nRg8b3y04y7pH2Li8xxZr8BwMF5dRbP0OKOGe515p7ovtWR2IWSVJu262PJ8uogSWg3VjK2WyaZX1giWV5cItnbL5GSnVdJ9RE75IJHe0vGHHlNXytUqCA33HCD3HDjjXLzzTdLy5Yt5bbbWslNeN+hw2vyw48QfxJt3vz58vRjD8qDd9wo2bPHyttTqlwitV58Uio9eZ9c/MR9cvUzreT116+WVQMaym+jIbCJVWEC1UTGYjvGwhTG2p1bNAcCJffP8x+T8R1uk3tQwvP5dp0euUX+otB/+iSx+BX4G7/T6ZGbzW/42/EdbjXLMmW8Lp/r4tETs270gX1Bnw6ib6sHXCaDul0n93e4Syo8fp+UvP8euejZJ+Xsq5pIQobY/fPvaHm9dO34srzesaN0eO11efmVV6X9Sy/Ls889Ly+82F5Wr0HF4mlr166TRx55VO644w457TSU+lhWntMvkKrdFkvd4dthzhvl7G5rJO+ryyTh2UXAUuTwLcnx+iDZ9uV3dime9t4CkX7YDh6dYOlOrmwAZ7gzkIPI+zCBHSNlxNPN4g3gTUx31rdFJcSduJaXylNyFtzdOuB6w2X2NRnwcfgnXLsV8AXDIFvmjLKiG0ryVSiruIdcwWCbgN8lf82/R3q3qS6VTvr77KxmFxSW96a+AgOga49G+T84tmeXe2jNPgCUZkNR/u1dbbOfuPVc97ZkfKm/pHthguR5dbmU7LJKLuyzTi7uu04u6LNWqr31vpxx4xOSkC6dpAMqV64sjRo1MrjiiiukcePGBldffbVMnIipSDJt376P5bnn28mtMIxSpWLnjRcuX0Oqdx4lF7zSVwo80k4Sbn9QElo/IEXuu09atmspI3pcIR+NgBBHVwJgBoERAPz/eGwf90p/PEW+27VUxvV4QW5vcJFMG9TRlPde8RP4G7/D7/I3/K3smxxbFpdp1mXXw3Xy/+gD+8I+sW+F7r1fEu54SPLf/4iUeaa9VH+9j9TvP1OK1W1mti1v3rxGxE8/86w89vjjpkp68KGH5P7774ew75TBgwfLn39iVE6ijRw5Upo3by4XXXSRWV7GHHmk3CM9pRpM+TSYc86XYdRAlvaLARjAC2Ng5gNk+vuobpJqe9fEOKH7AMgVcobc2cFDgjSAEYkNoBOmEOvwfT2SY3kZcHXVA4bD5LLyOglcD5xwrS7gC0aA+a8hKatRWvFwEYMc7FUG5gOL75GRj9UxJ50EBlAOBjClvcguJu2t2FyOx2rp6jxcMwvLGHOzYJJrs5+4dVixSTK8NBCjxxTJiFE/d4dlUrjTCinVbZVc1G+91JnwgZRocqfpY/r06Q0Zq1atalC9enWpUaOGeU9DWLgw6UqD7fvvv5ennnrKGMeZZ55pllmwfF2p2n+9VB32tlTqu0Qu7jZJzn2lnxR8rL0k3Pm4pG9zm9zf5W75cwlGKx7zHn0JtgliHANRjsFozP0Hi1HKsvr5ejPK3K/k/bWzZfnkQfL7dyj1f2QV8DGgrwA+49+WTx5svit/fRn7LZfBZXGZXLZZB4WPdWLd7AP7kr5NK5jUY5JwbzvJ/HgPyfbsW1LktTlSbgCmTEO2SOGKl5ptK1mypNx+++1y5513SqtWreTWW2+VW265Ra6//nrzfvHixTYy/rZ161a5EZVW+fLljflymWfe9LjUGLtLKiBmlQdukPL910lJ5Cp3h+UwgImS8eUB0mv9drsETzuwC9sETpAbK2F2K8GVwABGmfk/dwqOaNvEMYAMMq/jteAWYjOHh3QtL5Wn5Cy4Sw6zjymgHnDCNR779AUjwMLXMcdcibKKh8PMoSR7vNjgVvztPpn98hVIhmsAheS9yc+jAoAB8Kwuzv+5U4cGsArztWn43QS49HcgfxLt9ZXWAF6cKgkwgPQvLZYcHZbKyZ1XSjkYQL1Je6VEs7tNH0nCM844Q84//3yDcuXKyQUXXCBly5Y1lUFKFcD+/fuNGKpVqxaUtYUq1JUa/VdL+cFbpETP9VKw81rJ/doSydZ+lqR/eogkPPiYNHzjaTn0LraRJF2Niod73CnO0ZVRoteDQWIuyqvS+DAM3tbr9wOY3++XP7/FSGgFH8CagPnbr5/Hvsvf8LdcBpc1DnNkLpvr4LpWv4p1D0EfxqMvz0jCA49L5meHyalvzJdze6+RioO3SWX0v+rwbVK9/0qYWm2zbaeffrqpjDiKN23aVK688kpTLTVo0ECuueYaWblypY2Mv3366afSokULOfvsswMDKIlc1EVOLoABFOu6SvK8vkyyvboUeVticshcvrpio12Cp5EL5MQ0zOnJER7D56DBwYP7AcxUcpA1gNg1DcYAOjQ1o7w5OSngJc2AAGfBXXKYfUwBJ+R5ACmeCTi/w6Uox+Ck5hgyYM4CU9yCv90lK99sIrVK8s5AscNfxgAmIYG7Y65tLtHkKZpM6poXkGiUbWOwrO9B9CQayZIepEn30lQpjPK/LIhVcfAGqTRko1wydKPUm/KhlLgqZgDESSedZEY2onTp0obkBAX9OMrc5NqCBQukTp06cuGFF0q+fPnM8gqVryM1Bq6SqiO3ydmYcuR/Y4VkRhWSocNKSffyAlQm/eXKYaPk0GcgNR9ptms6iIrt5ajFsxQnIG5zYZxreS/FaRjJ34XAUeL/+PdoH8OnFn+bQOw7+C5/w99yGVwWl8ll80IXHh7bPcOs+9DnG6TJ8FGmT+xbRvSRfc3TabkU775aLhy0WaoPXG1MjdtWvHhxqVu3rkHt2rWlZs2apmqqVKmS1KtXz8QjubZx40YTr2LFigUGUKLZXdJg6odSftAGKdljteTFurPAsDPCBBLaxwyApp5kIxfICXKDHFlJrryMwaMHOMTb0cEAYAYjnmocMoDG+C7m+7N44pLDTeUruEsOK0+SwQlpALwl8kHAFxCDYY+i1FxIV7WCN6eAKm5CaXobBvhm5tltPBRoDKBsQXlvAsq4XRAEy9eNXTBawQB4gsdaVAI8FDQbCeYe4SRad+4DeLk/yDNB0r9K4S0xyPvmcinZa7XUmLBHzry1bdDPHDlySOHChaVIkSLGDIoWLWqODBQqVMhUAtOnQ6Ce9sUXX5jyl0Zx1llnSaZMsWfMFa7cQKoP2yAVh2+RCjCeijCdsgPWS9FuqyGweRjVBkmTsXPkEPdg//GTmHv080ElH8zFdmPOvgzbNwfGuRaj9G4awE4rcM71wwag72kCdl+AMQD8ZvfU2DK4LC6TV8jxEWofY128EQjWfQjz9SvHzkafhki6V+ZLNoy+ud5YLqd0XyXnD1gnNUa9LTUG/V0BcB8Ad5pefPHFpoyn8bFiOu+888wUqH17TN+SaV27djXiL1iw4N8VQPM2UmfqXhOnCkMwBQC47iKYBiS8PBVmPkBeWZ5MBUAukBPkBjliTjVHPzh4bOdZgaNhAH3jDaBwRpn3EgcolPvmpCbLy+CUcHAW3CWHlSdJgBo4IR8YegHwCeALikGXO8sjkBB6cOYYcUPsJBNi/k2ys18z8xAQc/WcGsD4J0Teh2vz/oG8QQPdnBdrrEOpPA7ztnlI7p+HbPYTtwUffIwSEgbw0hhJwGiWpdMyKQJCn4u5Za23tsilk3dJubb9JEve2L3fScScOXNKnjx5DDiS58+fXwoUKGDMoUyZMtK3b1/ZsmWL7Nu3T3bv3i1z5syR6667zpgER0W+mu3Gskpd00ZqTdkrZ/TD6N8VI2pHmNBrGM0wqiW8MgNiGyB3TF8sf9n+mvbHLyjbMZf9dA22F6Y3ByXtuhQM4CcYAJGcAXAZXNbazrFl89oArsu2Q+hEUxrACwMl42vz5KQea6TMwPUQ4UYphxH5wqGbpcZbW6VIVT4xJ0EyZsxopkznnHOOMT2+Z7XEHaA0TprB5MkwMU9bs2aNqRQY62zZYifXpEuXXs5q/ZzUmLRHSvddK7k6r0CcFkvCqwByl/DyBMn0Sn/pt/EduxRPIxfICXJjLThCrpAzm2gA4BB3Jm/pCQO4IjCAsvnSyazna8EYUeqbE7XAR70uhLC8JYdNXpMGNUAtnHCtLPAR4AuKwbMtzpNDMxBcnhfOV4IXfihmXSs/TrlFOt1W1ZysUrFwBmsAj8G1MXcLDIBJRRWwDuXsRCRpCuZtvyR9WGjHl9/CAPpBaCMkBwhVvM9qOWfgOjlv0Ho5EyZQbhjK2iGrJd+5Fwd95c5AHpvmoTySk68UP8nKzzNnzmz2EXCuy52ENIisWbMakyAoDC4nU+58Uu7pflJ/8vtSf+xWVAEb5bQ+ayQbqo+E10DoVyZLuvb9peuabba3ofbrt9hebPPcu7G9PBwKA+AdfAJx0wDcKkChBmHPE+D9/vhbxmwOlsVlctmh9hf+3TsL/Wo/AKKbjj5i7t1xiUGeLitM3OpO/0hKXXd/ECtuO03v1FNPNaI/+eSTTeVEMF40htdff12WL19udvpxvwBHfu5fYZXEuOron7XAyXJhuyHSAPGqNWqLXIDRv1jv1ZKd8UI1QhOnmS/5EEaXVCMXyAlyg9trrjgFZ8gdYwCjMB3oLiOfutyInzy7okwh2dK9EUYLcNPlpPIUnCV3yWHd7iRADVALJ1w7DVgN+IJicFv9kvLLJJRlvKCGO56mE3BpBc8+m9daDsx+Uia1v9rM/y87O4/sHMMbiWLuxvJtE+ZxnMdyR+AGzJEnozx7C4n+FqRPon1/8Hcp03s0SD1Ysr65SLKDyOk7YU75xlI5BeSqAFHWm7pHSl57L0bs9EF/ScoMGTIYMZOoCoqfcLeNpkAiEyp+omCFWlJ7NIg8fLOcCuMpDcMpM3i9lOy/VnKjCkl4ebRkaN9HVuxLYh8GT27ZNBBxwdx0QyeIGNMP3sFHha1GYEzAQj8LThKiAeA3/O0GzPm5LC6Ty/a0Phu2S7qXYJgdJksC5t9Zu2CqhOql4vBNUhcmVg/ivKDd4KBiYoxYJdH4+MppASun3Llzm1eNG6cErAj4yt9o3FT8RP5yVVBhbJJyiFcprPN0lP5EcVYD3VYih8Mle4f+svub721vPY1cICfIDXKEXCFnyB0awI4ReN9F+j5QF1PNbPJUs3KyZVgb+WMORv/pPCvT4SQ5Sq6Cs+QuOax9TQLUALVwwrUiwFzAFxSDKyoWNeeLm6vreLqpOeWU4CWfwNRmqAJ4MgbKty1vyu5xD8mAh+rL3kmPY/QfFNsJyB053DnGuezGN5BkzM1GYZ72XdIjwh+Y1z40B6XkC30xomFUw5w2R7cVcu6Q9VIN4rxk1GapNOZtqTJoqeQunbTDk6gEqwMFiawmwVeXzBmz55TznugudafulqoYzU7H6JkFYsrwxjLJ2nWFZO6Msrb9UCnVfYR8/tPfZXhc++1HjGAQ7WKYIM9m46OwWbbrmX4qcBW9gf0s+A7A3/C3WxA/LovL5LI9bf1nByTjS32MOeXusUpOg1mx76f2WyNFYJjno2KqM26LMTfdVm67Vkp8JVgRERS5a4qEmmvcZxkyyum3PSV1pu2RqqM3S2msM3MXjPow6iww7UxvLjQmXmHAePnxN795mUYukBPkBk2TXCFnjAGAQ+DRn+s6ybjnm8jM166T7xdjirAF35vXCpUD+Kl8JJSn4Cy5Sw67ffaAGqAWTriWEegP+IJiULZEbvl82OUopzjaM8BwWwXPh+d553RdnuBD50ZSDq16WQ7xsBjvzEoDoAh4cQcdncmdfa/IMBjH/ndt9v1t5q6PJN0LvVByj5V8IPHpg9fJaQPXSs6eK6RAn1VyCQhXf9r7EGxXyZAtxTO9UoVil98ktca/LReO3Cxlh2006zh/2AbJ03OlpOuMkrYj5v/P95YHZi+X3w8lcbLMLyjTFyAey5+NHQblI7V5Wa8KW6GiTyR+eyUgf8Pfchlc1oInYsv2tAM//ypl+qBiemmIZOi8UDJ2RV87L5Ws3ZdLGfS/BqqAWpN3Srl2AyUzpjjuNtMU1QwVapZqoK5Jush/fmWpNnKdlIdZVhi5SarAnMsN3yB5e62U9DCAhNenwcT7yDOL1siff8XtMYlv5AI5QW4YAwBXeLSDh5B5Idm2AfIX/v/rcgw05NnmN2GI7WJz/Sm85wB+63KTXAVnyV1y2Nd3B9QAtXBCttcBX1AMsmfNIB8NbAg3haMyyAF4QQxhg88RivNdOvcGvHLH1SYmjxUADQAjPx19IxK3CETuX1dk3zqbfX/77MefpeLACWaHW7o350n6rign31wqeXqvlEqjN0nVsVuk4pitUnPi21Kqxf2SLmNsD/7honDVS6XasJVSd8oOqY7RMj9MJmO35XJK/zVSCpVHrl4g9KujJcuLvWXWLozaSbXvUc7OuCMWC17PzmfxBdf3W3EnBf7dwN4LgL/llXBcFpfJZXvaoT//kqcXrpGE53piCjBdMqD0Lth3lVwwcqNUGLVJzh66XsqhTK87dYeUaH4Xttcv6LQgc54CciEMp/609+SSMZuNKWftsUJOHbBGSsKscyNPCa+Mlkzte8uivcnM/9nIhQHgBLlBjpAr5MzmXjEObe0b+8xwy4JnR/JKR2MAykfCchScJXfJYV//HVADJ2xrA/iCEkO6BFn/Zi04LUd8XoACMOABGqNsu0LMRRhMEHfccC8u9+DyEA53AvIuQOZuwnB2JpfVQY8KcHX/nma3dV6zTRKe7oqRZKIkdF0m+fuulrJvbZRzR2yQAv1WyRnD1oHU70rtidukdMtHJUt+uyc/LcDodlKtxlJjxAqpAYGUwbIrj90Mc9kkuUHqhC5LJQHTj/Sd50jCs73kqnFz5dtfD9oeetq+1YjXPbHt5YMq+YiuwADUBDwIhG/F/y1e+Vw/3hRzPUZDLnPfKruSxG3xh59KHu44ffUtyd5zmZQYsk6K9F+Nvi+TfDCD6uO3SO3J70j1sRvl1Mtv9McilciYLaecfc8LUnPKu3IhqqRLYMbVsPxC/bA+VB6MV8KbiFe7flJ96GQ58NOvtpdJNHKhJzhBbvCuvowdX8kd7gOgEbAqoBHq9ICX/FLovPLR5aTyFJwld8lh3zY4oAZO2MZzoH8HfIExGHT/BSipGFSK3Qo+AP7Pq9F46qXZ0Qfh0wRWc4cfErh9ENzb3rNdb9fMQzuD4Pbz8b2/kj4UyPbhdz/KBf3Go+zug1FtvhQetE7yWVIXxGvNiVulAkafi8dvk7oz3pMLXxwoBS6u6d0OH3KVOkfOvPNpqTFmvVQDmWtP2ianD1sv2fuslNOHr5fzYDZ5UQEkdAepIawcL/WVKTsh1KQay9xtIxGPB2PbynMC1AA4pw/uERgG/45Xvf+ffscYAJbB2HGZXHYSpfRPv/8u10yYLwnP9MR0ZTb6DBF2XSoFB6yWKuO3ykUwtNIwzOrYzjoTNkuxpreZObwvLskhR7HT5dxHOmLev1PqTHlHzoRh5oUZl0bcyo3aKIUHMl48AoC8PdvdnAL8V3LlPzlALpAT5IYxAIDnj+g0kmdDriW3IH7yiwbAq/0mY3rK242FOUmugrPkrm8bHJD7J+R1ANqqAPsAX3AM2jY/Q/5iYHn5KQMeBi+J5UkX5kaMTBLET6yDi/MxYizfeDiHCSW29RYZ10Jk1E2Y035jWZB0G7Jlp2RBGZnu1RExIYJcufuvkovHbZYzRq6XfANWSaXxm6XG5LelMsrR6iNXyHlPdpGiDa+VnCXOlPSZ458Mk6VgESlYsZacedezUrHPTKkza5fUnb5DzsSyzgWBq0/aapaZ0G0p1rNa8pHQb2Au+0wPuWXKouR3Zv35h8jSFzEcPxUbvfbOss/qo6jdOwWr2MOw3zMPFsF7/pbL4H6UxbxSjudPYB1JNF5wU+C1gZLw8nBJ6LFECgxcK2WwTacMWSuZ+6yQs95aL7Umb5PKEG4tbCfjlK9spbj4JIX0mTObmFbsNlnqzd4t5SdslbKjN5rlFR1C0S+TnP0w9Ri0FmY9D9OR3lJr+DT58ucURn9ygFwYd32MGxthnOQJOUPusAKgmXJQMfwCOAXgxT4TeQ/JRok5Ca6Ss+Sub1sckPvUwAnbTgc2AL7gGFxRobD8MeEyBBWBnYRgB8BnBJPA46+8ISOTYxIFotKlmUBiMxycCSUojLkPifSphhEOJE+h/fL7H3LD5AUQYHdJ6DheMvddIaVGbpAiIF26Xsuk1AiM3iDzaSPWSbmxm6TOzJ1SY84ezONRyg+YKxW6TZQKb46R8sQbY6RS7+lSdeRyqYnv1ZuzS87FyHgxDKQ8kKnvSimB5ZUds1Gyw2QSei6XdN1QyqICObfXaNl+IAXD+up9mCFGasaBI9cny2PnAOjDPIInBqkRhOB7FiCXwWVxmXMQN64jicajJ3fPXGbMKqHTJMk9cLVk43b0WGriRXMrDaM7460NqJh2SC2YX5Xhy6QspjZFL71Gsp9aUjJkzSYZssSQMVceyXNWOSnRoo1c9NpwVEiIFaZJVSH6SyZtkbxYfnFUFRejosg/CJVZT1QdPZfAgIZIrlf7y6QdqGxSauRAn+oxTtDolCfkzDZw521UkdwfEAwwBKoAnuvPeyAoD11ugqvkLLnr47QDcp8aOGFbdoDPSPcFx6Bogazywyje5JEGoMGO3WnG3AyDFcD0q0WWY9QzBoBEmWQhSRQ7qwDuCOQhHZZyfL8W1UHfqkgsiJ2Ktufb76XigAkoKXtK+q4zJCvK2oTeKyUHRuoKE7bIycPWSi6QsdbUt+XM0RukLKqDujN3yAWTUe7O3SO1LKph5Ko1e5dUxYh/1miYBb5fCqNi4aFrpQpIXQTLSei9XIoMxfwZJW26HiipMa3I1WGADEUlkmJ7ZzxK9YcxYoGwvIDl8w1/CzmAituO9AH0c/s987xB/J/L4LIoAi6b60im7fzqWynfn9Om3pgCzML2rJBcg9bI+TDHoti+TBilK8IILoJoz8QIXm/2+1Jtxk6pPgHVD6qnS4YulipAVb4OWyJVR63CXH+bVJ/5vtSCaVwIoyw6fB0MYKucheoioddyyYfll8B0KQuWnfDaGFP6PzBnhdk5mWIjB8gFcsLwhIOF5Yk+7JNHBAynbBXAMwV5+Jk3ZiEHlY/KT3CVnCV3fZx2QO5TAyd06wT4gmOQO3tGWf1aZbgqnJXBNsC8X0EDoOvynmzGAFACM1k8m4uip4PzJo/ckcOkEm/3E3mrWezkD96lJxVt7SdfSNk+Y2PE7jJN0qGkPQ0ELIqRPwFVwal4rTR5q2QduErKwAAqYoTi36pD5GeP3SgXgPAk/Rkg/SVTtsrJw9fKORj9y47fJBlA3FPw3XPxvfQkcd9VkqEXxP/yIHPSz6srkrmQRdvvP2Ne+jyAUp3bzR2AX2yOiZin9SrizCAM53vmu/gtl8FlcZlc9iJUWlxXMm0GpwIdB5udcOm6z5bCw9dLfpbpfZab7ayMOBUYukZKj1ovtae9LafDNKtM2y7VYQRVUBlVn4Wp1Mz3pM6c96Usplb1Zr0r5yGmJe33c2G05+/LT9wseYbAjPuslIwDVkr6zpPNkYhGb82UAz8ncZ6E25h7coBc2AZOKD/IFXLGPJEaVQCPKgUDCzjGC8t4+HkCDcDhovITXCVnyV0fpx2Q+yd84w0Rk9wRmDFDOnnz1rMRVLqrG2wIn2AZRifmThnjzjQAgC7NuRwvl92K5HKHDpNKMNnzYBg9Lhb5eL1lQ8pt3acH5Py+MAGUuOk7T5Ccg0A6VAMkH0VeECNcuv4rpTzFP3KdFOJIBbHnAVnPHrdJzsfolWvwaqkIARSCAWQfvEouAIlzUxwwkUIo/7MPxvtusfP9M0H8zy1eF7voJ6W2e57I7PsRA8xPSdwP8f+vtlsx8ynCCkfgieB+z+Krt2PL4n0VeYks18F1pdD6bHhHcrzSH9vRX9L3wDSmH0wNFdM5iBO3PQNidsHELXIW/n/yiLWI0zYpBSO4EJ+VhVHSFKrBPE9BHBmv8zhFGsjYbpViqJpMvLCc4jDUdH2XS8KbkzDy95AL+o2T979O5u4/bmPue1aMcYGcUH6QK+SMGTxgBhz53Qd+8O6/3ONvDMDy0AC8JEfBVXKW3PVx2oKcPyFvBhpuNYFkLwq6unIRlFV0Vyt4AwTfAO/H8eEOvAEDR34In+AVXSyFmcRtEMQWJhhuTjC5HNEG1IKTI/lpaBs+OyCVBk40ZEvoMBJzzrmSZchaORWETei/QnJC7OeCrNkg9NwcpUBemkJJzOvPQBmcAaZReswGKQJiJ/RbAQJvkIKYFydwWtFniWTojGWjyijcabD0WLc9deL/FYRfhlFq4WPYPoxgvHz101UQ77sW79hXClo/84F/V/D//B3AZXGZHB25Dq6L60yhcQ98ng4DY2dUdpkq2YeskrwwOW53HpjlBTDKLIhTIRhABcQp59DVch7EfzaqolyIXaWpMEr8rSgEzzimhwHkw+/Owt8zogpIh2lX1oEQfyeYMvJRa+gUWYNKLdWNuScHWPKbQcLyg1zZCsMzhwDxN8MpO6hwkOFdpSj2CXUdHlpukqPgKjnr47IDcp7cP+EbH42c7FOCyxTLId8Oh8j59J4g4BbjkQTeo46XYVL0Jln2SULcW8t9AMYE8Mr5nIKHCGfchflftTRVAWwffPuDtJ62WDK+BGK/2E8S3hiLMn4hRLxS8kPYBd+CuCH0bCDxGSBrwoAVchLK16JjQP5BqyTn8DVSHCNfAqYLmVDCZhm0DPNllK8vo2x+rodUGjBB5n3wsV1bKtqehSIz28RG/+2oeLjn/gtMG4yAUQWokNME/s6Cy+IyuWyug+viOlPRJu38QM7q8Ra2q5ckdBwtCb0XmNgUgKiLIiax2KyT0xCb9INhjuM2SolxGyQdPr9wyhbJhVhlhnGcPWGT5MaInwATOGn0enwGw+w+0+zwS3i+pzQZPdvkJdWNOWfup2PgIBcMLyB880rOYNCgAbCKDHhFA0AFwIeIcNAh98J8BEfJVXLWx2UH5PwJ+VhwXxsO+IJkUCh3ZlnQ7iI4KwyAQQ+DyeCcjPdyY5JMwgAmi+UcicuEmjmdBT/b2E2kN0rAmY+CEanYYeS0n3//Q8a9s1sqD5wgmWgEL/AkmOEg5XRJ128RiLoc4l6JEQ+kHYTKAETOPhykxfsMg1dghMNcvydKY4qCV9K92EdO7jxUOq3cLJ8lcxPRRO2nAyhJMSotRFnKuStvg/bxEpEvt8WO4xtAxIQr6hShvyW2xpbJZXMdCx7HOhFbrjsVbesXX0GgsyTji71jcYJhZuq/yAieZlAcos9OcUPoJ6M6KkxjgNBLwTyzIW4JmCrlhanmHUkDWC7peiNuHRBrVErc19BuyTr5JrmTo8KNVdUM5Jy53wAOGAPgAGG5wQGDU0e+8qQgwycMKgSnArzdF2+K6uMiOEqukrM+Ljsg56Nm2wPAQcAXKEkHPNq4OISKsssEuk48WAFMuQKlKYipoz9vAML3ejaXSajCJpiHt3jYrNfFKHFnW3akrfG4/LCtO6Xa4Mly0htDJd1LmPdyR+GLKH1fwxThjXHxeH1UbNR6jt/pK7lfGyTn9R4jbReuMXvQ09ZgWtsnIC488+/NGHl5+u9nKNmNAShcMRMQuBfud/S3ED9fOQ3gsikUrovr5LpTaZy//HFIBm3eYfahZGoPw2zXJzaF6j5Dsg9ajPIeVRDEnW0YK6RVMIblkm/kask0GCX+gKWSvh8qhy6YHrUfhPj2kgKvD5KGI2fI8n2fpdG60d6dKtIdAwoPa5IDASdU+BasCHiCmWsAfM8bfvCxYmEekpvgKLlKzvq4bEGuk/NRs+1CYD/gC5ZBrXPzyc8jIfRJHgMYj885/+KRAFMBQPwE99byLMC3YQAc8WkE25Fw8wrwacGc+w3GMkc2h5r3W4akvfG6+NUf7zej0ZVjZkmF/hOkZPe35JQuw+XULiPwGgPfn9dnrFQbMlnazFwmY7fvlu8PJn13omTbpxtiJF7RPkbWd0agVJ8rsn9j7Dx+I16FCppwhe5C/+787sCW2LK4TC6b62DMuE6um31IQ+OJOZ1WbZG6w6dJ0c7DYASYGjzdHWbI+wlgVH8N0wVWRTRKGugrQyF4mMXT3STTy/3k7J6j5boJ82TOrn3yezJ3D06yMcfM9WCU65zrkwNhblD85izS3rEq0vDJmgCrTN4abVyNxDwEN8lRctXHYQdfAOR81JyW7AlBBXNlkjnPlEPphsSx5A+Dt6o292JHoih8YwC8uyvPCGRZZxPMe7zxCi8Fy9qVr2JEwLLnIrmpPCyYUvvip1+MISz44BNZuDcGvueFKdxLffBQ8qchp9h++FxkMQjJQ3N6xtruySKfrIgduuO8/QBejYCBLwlH2MlBf2PAZXB5AE8K4qnB5hoLTK0WYt3sA/uSxvYbtn8h4vHKso1y06SFUmPoFCnXb5yc2XMUqqKBcgrMgUbJvfqNYah3zVhqjiy8k9LJUMk1PgCGOWaumXPm3uXCOzQDWxHQAHgpuVaTBA2ARwB4ui+nAGEOgpvkKLnq47CDlUDUQq0D8CfgC5jBM1edFjOA8Qw4Rn0XNADejZW3dKbwieU0AJRwHB1NUmkASDIvcVXwZg8kwkyYR/eymBPie8mdO34stN9hUmu6oxR9ABUOXil+Pr1m30KM1BiRKX4vrJiTgjEOIvw7jP6sAPavF/kI6+AVgmYfCvvAQ48QCvv0D9onP/wk73z5jTnKwvMIeAefbV98Le/is59+T/r041Q3nvO/Djxgjmcg1ztGxnLPCkC5YAyAFQAHDIDVo+ESBhQzqMAM+OwAlvruk6QJchLcJEd93HVAjr8ARC3UKgK/AL6gGVxyRm7Z1/sSzPeZACf4BB2Z92Vb1tYmDU5P8NDgZh77t1UAS1gKnuAtn4PbPmMEGIPSrg+Wvzv5O9P+p42j2BZsw4y7MT99IyZEbssHs2PzdJ65Z8RqQfEmQljgCt93AZoKwWVzHTwiYEZPrJt9YF/YJ/btWG2758d2+jHHb2MgYM5N/i0PuD00ADP35w5BmAXvDcBBRAcUGgDPN+FgE+YfOElukqM+7jogxysBUQu1nMBGwBc0gwzp08m4h8tA6JwG0IEdMCkszViimdHfGgDf85i/2cOL5DLJxgBGA6P+Bp/+wmPAwxthflgfglpimXMMtUD892DU7RgTIMm7Z0psL/1nayHSdQBGakKF6+KLZOD7PmGWx+UCXMfHiyEorJPrZh9Wv455Mfp0rJoAczkIOR1xeSzH5kk/Tu4Jbgunh9wRSAPgFIeX/hoeOSYw73ZwrXpi/oGT5CY56uOuA3KcXI+apz0M+IIW4OrKBeXHEdUgdk4DKHyLMTAA7gjkwx2NAaASUPDiDh7f1WO7TDbv9MpRIAD+zzJ6EwgyuA4IA5PZNS9WOh4L7eB3KElBzukYbVfxeD+rGZgZ98zzAR5m9KcBKFwjcOERuA/620D8drk0gE9XinyIdXLduhONc2r2bSPep+IkoX+lMXfMIXPJnDK35kGfzL2bf7znFICjP08A4jUAPHrE6pGDCE2AlQC5xLsA8eEoyjsCXCQnyU0fZ0Mgx6OWRDsf+B7wBc6Ap1fOewbzuKlIqJsELcv4dFYj/Kf+fuXFHOb0TmsCdHre6ZWPfNqJ0cAFSc1bYA/HaNGvKspAfP8fzm//ceMTe1Z1iQlsDct+lLAkLPv/wUyMyMsgzNXAGgvXBHxwxe3C910Vvi6bwLq4Tq6bfTDzZ/RpDUpmVgLsK/v8XzbmbAMMkzlkLplT85z/UL758E9WAOZwIHLNIxwEdwDyClMdRGgCfG4gb0XHwcblHrhITqZw6i9BbpPjUUuiZQUGAr7gBahfNp/8OBSJnQjBU/gmEXRlvOeOwGUQPbH8yZgBmBOCeI43k0sTADFIWib/PZCC4MhggPcsqbfiO5wv9q0EU8Hv/wtC89r7vUtFFmD04U7K9V1BVs5XAT62+oMZIvsWYUReAUCUhDEChWsGCnwW938Xzt/4OxW8u1xdD9fJde+ZHjMB7Rf7yL4ugGDY92TuH3DUGnPFnDF3zCFzySMkzK2bZ+adFYAZ/fEdzvv1EnKeARiIH2AlwOqST0gKDACv4CC5SE76uBoCuU2ORy2ZdhnwM+ALYIABd50pMok7YGwiDGACvDcAbxIaAK7NQzg8j51VgLkWACbAEtpUASABb3vFkd8F57jbUd5y1O1bEYlvCaLgsyRujX3E2zd7ULKClFz//MfRf/SdI5XpM8RP4XGvPw/NsSTnFOATvBLm//azRKaQShiR8/eEu0wHrAI4/TAmwNEUfeP+FZba7DOrAW7DN6m4Lv9INOaGOZqIXDFnjB1zyFyG82tAA0CfyQVT9qPfhiMYLHgGIAcQ5RErAB4BYJWpgw25Bw6Siz6OhkBOk9tRS6HlACYBviAGKF0kq+zoXAHzL60CAM7NeJUWn/FO4RvgPR3c3AyEybUmwJ2CZiqA0ev9SbHj2wHs//lsPBJlyYsiQ+tjLllbZDZGgb1Ljt7I9uPn6BfWOR+Vy7Q7MfK8HCMx+8nRioa1ByM/hccRmK8f4ZVi5DkAATyCDQSdHDy/U2PRZRvhc90wIO0D+2RGVDu35lGV5eg7t2E+4s9t4rYdjWYqJeRk9mOY6yNHQxvEcsbcMYcmnwqbY/7NPPUX/SQXAvETvALwFcsfxwTmw1DcanNKLcNBctHH0RDIaXI7aqlozYAfAV8gA9Q9L6983Adl3hTuEKQBVPt7RyCPBtAIjBkgeXR0Hg4keIYXyzyzP4BzaYxeLBH5KCwDjBgKjm58Qs4WfJel7ZC6mFNeJjIHy30Pc+CfD/yzPd88L53X1x/Ygb5hHjoXJJ7eBmRD/zejJDXTFPSPYKVCofHyXD60412WrxQc/4Z+c++8ESmqgsAIHJipAuGIOw7OdxPBLnMf1sF1cZ1cNx9KyoeTsk/sG/uo/aURcBu4LdymOcjLZoy23FZuc2qudEyqMeaMPXPAXPDoDXPDHDFXzCNzx+cZuvnk5zSA9zANYO513m8MgNNEgEeNeNJPMIiQS8DMG2PCJ9fAuY96VzQc9HEzhJ8AcjpqqWzZgamAL5hxuLJCAXm/GyqB6XUAVAMzaQAgm4pfwcs4zU1BbJLp9Ew8CcATQlgOGvFD7HGwRNoDopFM67tjVLtDZFSTmBGMByl4eeyuuSJf746danrw+xjBw2epk7R8uMYvX4v88CkEtxHrh0gWgWwsV2dQ+OjrevSTAtMylXNWkpY73Xg67k58vpWHrID38TlHYjMi20rAVAMERXskoMvDnN6M+HbU57q3oQ9beVwd/dw7J7ZfwoywOtdGX2liG7BN3DZuI7eV28xtZwy+RywYE8YmkZkihowlY8rYMsaMNWPO2DMHb/GmscgJc8McMVcc+U0+wznF3yl+HgnivN/s9AMXOMUy/AB4c1BWjawejfgth+Zfi/5jsJleG5y7WC4tlyrxExz9yemopaFxvvQt4AtoHC4qmVMG33OWLH6xnCx6vpy8N/gaJJBJw2iq4EkcdPZNNsmBCbASABF4QoiagBE8iBwA/+eIy/cfzIoJccsAMQ/gmNIKlcfVIsMaxog4DQRf0h7r6wxxcH4JIW/naAMhbMA8c9WbIgufxfcgAhKYxJ2LkZEPneBoSXIaAaFc1akJ+6Ti34G/b0Z/N8O4duLv+1D6cuTneQABINQA1hAOC+5y3OUTWCfX/R62azNiwf6wb+wj+8oqgSW3ux3cNm4jt5XbTCM1MUAsFvJUW8SGMWKsTMwQO8aQsVzyUiy2jDFjzZgz9swBc8F1MjfMEXNl8uXmkZ+hT8xxIH4t+8EFUx3ybkAAbx9vhE/u0AQA8GlNx+qyoN354NqZhnM+LnrwJcAnYUftMFpHwBdUL3JlTS85smSQB5uXF1nNBD4CcDoA8KgAnZ0PDCE4z2Pi6f5aCeihNVP6U+xKKrwqzGfA3tkYBefHRr4NWM4yVBjzQBTu+JrUElORFiD3dcD1eH8DAKJPuiU2+s1+SGQxCd8RfcH6OUJy5DSjFtZNsQSlK0mMPrDEfg9C2gSBbKRpDcdnWL+OyB8TNALXDPjeFXFSYnbhfs9dlrN8rTbMutGHbejLRmwH+/Ye+mymA+izESK3x9kmbuMH+JzTA267eYwZzJkxYWwYI8ZqPGOG2DGGjCUf28W/z0Nel2F+zoqCsef6mQvNi4kjXt1cGVPAuvWQpY78ceLnwIABwjxKDtWFGfXt4LHiCfl80t1yVrHcki1Tmh9qQg5H7TBbIWAt4Atskri8ckkkESMDDYBXCC55GO9BHN7Y0tz00SbaOD6Sr5UAzwDj8eAdICfLW44aNAASTGEqAAdqBCyJSb6dGLm2g2Q89MS5qHlVDI79jcvnaKTL09HJBYVi1g8Cm1EV39uK369HvzeizxxNzQ5ABacBFhRmAAj2sOEsx11+ALtubjP7xBKc0wETF9vnwNjC2+fElqU5Y8LYBLFS2Bjyb5wWcXkm3q7wuR4H/JynK/M918Nc8szP7egbcxwnfjsgmOqQ3OiGweMFcAaDhuEOsPJxmfFaM8mRLc1PfyJ3yeGo/YPG+6Yne8uwMMqWKij7p9yFso1JhPiXYHRhQrlnlzcB4UUs5s6v1gSCaoAmgFHMnC2IKQFFtgdkNSQjqTDHJbHNqwv+DfgQ7zn6EUpSjtIf2c8M9PskJ8XhAmQNgP8ruffidyy116Hva9l/9JeiMsvXdblm4GAfAbHGwRF2HNzvhJYTB902u272hX1i39Yhruwr+2wEaUdgA2tqun0KY3IaP8TXLFe3i+ux6+Lf3Hgb8P8hMFdcJisOM99HLplT5taU/YCW/Eb4jvh5/J/H/A1vWEECqx6XN9rUkIwZ/n4SdCpAzp7Q9/w/ko1PT/kO8AU6EQrnzS5zXm8KJ0cZx2O3vIcbTcDsB2CSAZNwNQFrAFoJ8JgwDw2RNDyuzXkjyWvEaEmmhFQYY1CChkFCuqAwFBSAioHCoEjsZwZWRBxZOUfmGW3sL/+m6zR9UahgrHgMfCJOA4JluusBzLoB9oV9Yt/YR1MF0LwIux26bSz/+V4/j4sFYWOUnMCD7Xagf+MyaNrMmbnAB33h9C4QP3Jsyn1Wf87IbwYGTBF5H0nO/40BWBNY84S0uqyMl2tJgFw9oZ/4czRaC+ArwBfwOPCUzC73VkdJikQaAwB4FRfPCjRPfkGijQlYIwgqAVYBagIoEwMjGAYyjYqRypSvIG5ATpIvJIwAJCdelbABqQEVB8VglgkEJbIViREKvsf5K+e8K3l/+g6xPvNvQTVh+xEIwu2DipdGkEq4v4lbll2+io3rZh/YX1ZV7Bv7yKMY7PNebqPdDj2iYsD3NDoaAk0A33NjE8RVt4dw+wEEMbX9YE64bCN87nzlURIVPnNpxW/KfmsARviO+Dn68w5A3GfkDBy/L3tEqp2f4mO+FbzZB7kataPQrgS2A77Ax+H2RueiHKUBPBjDIoA7dHhr6w1ItEk4dwq6JgBicDpgzhOgEZA4BOaN3HHEk0ZYTpqTh1Dm6rkDJLgRg0tKBf+Pv5HkRtQkPgWB3wU7xxyYHX+A+Z4VCMvY1Z1Qmr4KgWF+ym2g6Lh8jnh8VZHo+oNy2n6eZuhv7fJ0W/RvZt0UK96zL+wT+8aLglZhFOVJQWabsQ1G9Ha7wttrtpkxtHEhAkPQ9brA+rheE0NdHnJhdvBxxKfwkStzUY/mj7lkhUcgv3ElvyN+Dg685x9HfsMbmMCKR2THsJvlzFNTdchvC9AQiNpRbLyYojeQbDVQv3xx+YHXqS+j+DH6LyaQUF7hxWTTBDbQAKwZbAYRaAK8rbYBzYAjhhoBqwEagZ0akGh6MxHu0SYJDSEt0Q3pFfg/d0RxdHJBYVMoNBM9HZmHznbzsBmXhd/SALiDbNlLscNnvN/BMsxROY81lQgFCPHwt+zfuxj9PoBQtJQ2CInIFXcc7N8T/d6Cy+Y6jPnRnPAb9oF9YZ/Yt+UQEM8A3AFB8m/mcCBFag8H6rZyPwG3nTEIx4Wx8sXQxISxxHcYc8aee/XNFYncwccRn8LXUR8w+eOITyCvzLExfYoer4YHBKYuNACe8ku+cMDg6+rHZMILjSRvjvhnPIZwCJgJRBf6/IttLOBLhkHJk3LL6p7NMRrBzXmFIMHHiPNSYT7sUZ8WTNAQzCFCjgqsBNQI1AQ4JXCnBdYIONLQDFhuGiKOjIElKPc6k6R6aixFzFfeX58wp/YCxgTUCAgrED1tlUKj8JZidF3KO9LY8nQZqpl1EBqfWb8W5sC7IHPk4isrE606DNw5NhESdoDw9+zvuSz2xV0H17ke62Yf2BdTMqNv7OMS9JVnCbLvgfixTQbYRhW+OWOQMSD43sZIY8XYmfs2EIwpBc8YQ/A0YMaeOTDCD5f7BHPHHFL4Kn6W/HbEN8JXLqBqWW8f+02eGBPA6/rH5YVbU3yA6Q9AAyBq/2J7FPAlw4A3Zhj8RB0kEKRceB+SCfCegTy+yweH8tRg3t12vWsCHBk4HdApQbgSSMoEeHiPoBlYQ+C1+jzVlMagV8qZi3loCISSnEZA8hOOERgTADjyvQPi85FcPF6+EAJceI/8OqOVfD6sufw46SY5NOd2kQV34e9t8DdUPXyevwqfo/A/gVkGlsVlctlcB9Z1aO7t8t24G+TToVfLwZmtTZ9M39hH9pV95lWVWtmEhc9tNtuucQACoVsjZexMHDnCU/AY5U18GWvEnbE383zkQsUfCN8VvzX1oOxX4av4wQViNUyNRkauUPxLYALLH5QWdVJ8yu+nQHEgav9iqwH8AfgSYvDYtRfKX8tZ+lP8wAIYAHfs8LRgJtyYAJzfkAAIKgE1AmsCcacPuybgMwI1AZCVo5RrBJyjchRLZAQUghqBrQhMNUAjwAjKEXHFK+j/U9gOlNkLHpQlT18ib9bPL10bFpThN5SUjR1qy48TbxCZ3wrfgRmwktFRXOfWaYWaCJfFZc5vLd+Pb4HBv7qMuLGUdLm0gHRpUEBWPF8NfYJwTN/QR/aV22VGfrsdiYSv4rdx0BFezZIxM7Fzhe+KnzGn8AGd78eJn/lyRn0z8lP8yK+K3wwAHPm54xJc4FWjplq0fIH4P3zrFrn4rBSf8rsayABE7V9shYGPAV9CDBpWLC5fTLwNJSpMgKO/gocDjQHYxAcmYCsBPUKg+wXM/JGwRmBMADAGEDIB3lXG3IA0KRMgwR0jMFMFnxFYMzACwgi6FcunsFheL3pG9o+4TcbcVkZerZlHXqySQ16vnVcGX1tc3nmjlhyacT2EiFGZZNcRnOV4WmDOpMNvGY+FreXP6S3k7U41zTpeq5VXXrwku3SolUcm3llWvhgJ01mMqdWSdrH5P/saJ3wg0YgfEj7NMRj1kxF/olGfoPhV9ARzRfEDOurryM95Pqs+I3zlACtCvPKisQWsomgA4MmaR2TWq1dI/pzJzv+JvkDU/uWWDRgH+BJiUCR/dtnQm/sBHrSJZfmKUnU5Es3HhnEqwLsFeU2AlYCtBoJKwBqBIZmtBHgTicAECBJUjUCJ6xhBcBdaNQIKwBqBgRVIMC+22GUFxQuAzFl3b8pvc9vK+lcuhShLykvVckn7KjnlzXr5ZfHj5eSX8Y1F5t0MEvNCJZTiFLTuREsJ+l3+Fsv4ccwVsujRsjAZmA2ET9MZdsPpZt0H0Qczkm5Cn3izTdNXGJbbd1f4QZmvSEn41lANEFcVvhE/KzHCij84pdcd+Sl+5JE51ZLfjPyO+MkDniTGk8XIDx0o1j4sb9xVxcutEG4GovYftBTvITj22QaY26ECYGJpAnw10wDM94wJOEZgqgJrBHq+AI8SGBKRTCSVNQMz4tiSM5gW0AxIUFYDTkUQjGAktZqBJbyOfOGqwMAdMWkK1gjMDkKIjL/b2lu+ntBGVjxbU4a1OF1eqgojuCSbTLuztHw/sr7ItCaxeS37wZLejPDcMReCOaSJv/E7/C5/M72JfP9WfZncupS8UCmrMZjhN5wh616qJ99PRUx5aO0d9MHsr2CfOOI7gqeZGej2WMEbcJsBEwOI3szxGRcrfJpnYKY2noyribOtwDQHgejtK3PF6i3Y2WeFb3b0Icfr8Mp8m9x3AJB/Xv7LnX86SCy+V36de7fcdulZXl454I0+zgKi9h807gfwJSXAQ1efLwfn3IGymYm92wLuzj3ZJADv/Mpj2IYQnmqAcPcLqBEYskEAphqgCVgj0NHJGIE1g20eIzAmAJhqQAFBGNAUXCMAXCPQaYLuYOPhyNWvyrdjWsmW1xrK6FvPlVdr5Ja5954uv42pKzIR4NNseFUb+2nEGhr1+Rn/xu/wu/jNr6Nry6y7S0kHLGv87edj2ZfJt2MxteDJMjsgWq6bfTD9cUd5whW9O9Ir0Gcz2ltTNFDh29FeRR+M+mHx2/gH5/I7o35Y/Frya55V/Jp/Xv47/66Y+MmR5ffL7uE3yEVnFPLyysF6IA8Qtf+glQTeA3yJMahwViH5bMxNIDZEzwQrmHCTfJR+gQnYctCUhiQMjYCVAMFKIGQEwajjGgFNgJVA2Ah0NCO51QiARFMD1wwoGjWDsCEQ1gw4NeD/+RSk+Q/Lz5PukH39r5dtHRvJj7wV9oQ6IqMvid0xaXKjmMB5e2tDdoDv+Rn/xu/wu/jNDyMayfZOl8ung1rIr5N5Zx9UTlwH12XWqaLX/gAqevY56L8VvMIIn+Co78RChR+IXoWv4md8KXzALfdd4Wu5b+AInzk1wmeOVfgApzmrUQ2y/DcGAPHzzj+r7peFHa+QLJkyeHnloBuQEYjaf9C4H2AA4EuMQfp06WRtj6YQPA2AJKYB4JUlLk8KMgbgmMBa1wRAHFMJAGoCZmQh0awRGBOwRhCuBsyUQAlMgNiBCVgY8mtFoEagUCNwzCAYURVWeCy9+X4t+jr3QeA+kZn3yV+8PHnKDSLjMB3gHZNGV7ZmUDX2fwO852f8G//P7+I35rezsBwuax6WyWVzHabMp9DDCIs+JHxuk1Y+fLyYEb0rfFf8rvAZSyt8xjhO+D7xc9QHfOI3OWauNe8Aq0HO+XVwMIc775FOd1b2ciqE6LTf/7jdCfgSE+DlWyvIn/MwDeCxcmMCAJ2eRwN4SJBYS6gJsBogaAKsAgiagGsEIJwxApoASKg7Cs2xZ1sRaLlq5q1KaJDb7Cy0ZmAOaykgBpbEgUgoGNcMHEMw4qchOKagozCFymf489n3fIgIMQ2j/KRmIuPrxe5pFzYAfsa/8Tv8rv6Oy+CyuExdvq5P1x8nekfsLrg9OtLr9qrwTanvG/FDwjfip9mq6C1MLhzhG9Om+Jk/FT6hIz+Fr3nHe5795w4OS+6W7ya3lGbVSnj55IDz/3JA1P7DVhU4APgSZFCz7Eny8/RbRRYhwTQCBee7rAK4Q9CAxCBBSBRAdxAG+wWAOBMIVQMcjQITsEbgVgMKNQJjBhQAjcA1g7ARuIYAcZkdZ64ZWCG6ZsB79E/D9vKuObzr0FQQ2/wfcZh4FcR+GUZ6PmEJ4Ht+NqVl7Dv8Ln/D3/L/fBaBMRhH9MHJOo7oTd/YRyt4A26Hnd8Hwrfb7ArfHM4jNE4qfiv84NCeit/G3uSAUOGr+LH9ptzXUT8sfuSbpT+fJs2dwvOxvRQ/n/2/7B55d+A1UiB3iof/FgI8HB21/7AVBOYAvgQZ5MuVWXYNwRx3MZI7H6ObMQCevYaRjXt/18AEAiMgOUCSNSAM54emGiCR7GgSmAGJRtLBCAiSMJgWOBUBy1VzIYodycKGoAIwYFUAmBNd1Ah01KSIwqagYlPxWSGqSPnIrikc+SHsyRAzMQXbTYFPuQ2f8y5FAN+bz/A3/R5/w99yGTrau+sJ1u2KnbD9NH0GdKoTCF7B7Q0L3oqer2a0t6LnCT1a6seJ3o76cXN9Ct+O+pzrax5NTh3h86IfgqeGc85vOIHtpRFgoBj2RE0vl0JoD6QDovYftxcBX4ICdLkL89uFFD8wF0SnAZgq4CmYAIhAGFLQCNQErBG41YAagTl9VE3AwpBSq4GQEbhTAzWD4FwCVgROVaBGYGAFpIJyzcAAogvgiNKMzBAtHyPGkXzizTFRB0AlMBnCJ/heP+Mrv8vf8Lc6yvsEv93tB+H0MRB9aKQPhK/it6IPYhIa7QmNJw2Wog8u4GHcKX4rfC33zajPvKnwVfzMLbCKYM5h/tz5R04YcFC4Q36bcZtcV6Okl0cOfgOiO/0eI+1y4GvAlyiDyucUlr/mgNTzkWQ+PYgmwFee9bUSRGApqKMCTcCMEtYIgmmBmgBgzh6jCWg1YM0gbmqgxKUZ2JEszgjUDHT0oyCsOCgSs6+AoBFYMQXTBEUypqAjtTEBbO843mfvpuTB7/C7/I0K3izPXT7hiD3oiyN4g5DojfDtiK/nULiiN6Dwrfg1dsGor6K3wtdSPxA++qzlvhn1CTvqmyrP5pe5pvj5yG8eBQn4ACy6Q/YOv1by58zs5ZGDTcDpQNSOgVYEWAn4EmWQFwld9gbmuotYATDhBEY/UwU8GSOEMQGFYwTGBBwjCPYN2GrAlJ+OERgTcI3AQquBoCJQM1AhqBE4JhDACikQF81A4RoC4ZiCjta8Ww/L/DHXiYy9PiZ0F/yMf+N3+N1glPcJ3UVY9ITTb92WQPQqfDXA1ApfxW+FH5zKbcXvzvN11Dcm7pT7Kn4aPncA88q/uQ4f5gGoEvs/VFXSpUvx5p99gKgdQ60z4EtUgPuuPBtJR8LnQPgUv8JUAc/FSGGMAAhMwJLIjCRORWBMADCVgJpB2AhAWDUCNQFjBLYiMIYA8ptDiAprCMYICDtiEl5DIFSAKkYLV6gUM2+JPvcRMXfcHd08JnqC7/kZ/8bvGOE7vw2MRpetQrd9MHD65vZZtyNO9I7w1RAZEyN8GyMVftzOPUBHfI17IH6aswofeYoTvjV1k1vkmA/95H3/5rexHHAMANxoUP4UL38c/A60BqJ2DLX6QLI3CTmneB75eATIvpCJxlx3Dua8BJNv7v5KE3CNgKQhgThndI3AmoBWA7wuPjACEjRkBDo1MCcSWTPg6OZedhyAoqA41AhcM7CGYE40IhzRxRkCoSJ1wB2KFPAKbAcP8Y3nrcsBvudn/Jv5Tvi3znIDwVPcXK/THz58NahiCIreCl+3y4Dbye3W0d6KPhA+4Qhf4+mK30zHCC33KX4KX8XP6i0kfJNblP0s/Xm9vxE+OQATIA8Wtpa13S5PzcU/vPNPCSBqx1DjU1eXA76EGfCsro6ty6PkZxVAA3DAnUC8dbgSxGcEBrp/gGagRqBmQHBUYllKkLDWENQI1AziKgMd/QgrDNcM3DMN4+CKjYJ0wZHYwjUKCpqjOpe7Cv0n+J6f8W/6Pff3iZZN2PUm6hPFrtD+O4I3cEQfjPRAUOYDgeAtgj37HO1Z6jvC13wYoyaYJ+aMoneF/0wM3PFH8esAoIPBktZy1+VnSjoPd0KIrv47RttTAG/P5EuawcVnFpCPRlyNEQAEmH3L35gDmGPAT8YMQGFMwGMEZoRhNeBWBEBgAq4RADqKxRkBYQWggogzA2sIen4BxWSMIAwrvsAMCP7fJ1wLjuJa4vO97zsG7jKBOLETTj/c03PN6O5CtyckfI2FgY72FLyK3hG+Eb+NsxnxCQjfiN4VPnMFqPhNLlX8j0P8oQGA+V/cCt1vLCVPSvGJP6wyo6f8HqONe2V5V1Zf4gzSp08nb9yBKmApyz+K/2bHCPCeJ4MYE7CECaoBWxHoEQNjAmoErAbUCAA+WcYYAUkbMgOtCHhJqppBsK+ARsCqwFcZKNQMFDrKEo4YXZibnCpoDMnB+a5vWQY0Il2n05dEYleoudltNHBF7wrfFb0Kn7F0he+W+R7ha7lv5vrIIc/0W45X3g2KO32DfFvMx+g/r6XcednpqRn9lwHRuf/HcOsK+BIXoHihHLKmS0OMBpj7GQNwQULQBDAdMAYA8hgTCBsBCQe4JqBGYI4YECQty1XXCKwZqBEYE/CYgbkM2TUDFyosKzhXhAYqTg9UvEnB95sAofV4Be/2U/sPhEWvMHFISfgUvUUi4RM62lvh6zSOuTPib4tcPxab5mmODfCelR8Gg/FP15A8OVI89Ec8BETtGG7lAT6O2Ze8AA3LF5UvRmEqsAAl4KybYlAT4HszHYAJkEDGBGAGKwk1Aq0GtCKwZmBONrFTA3N9gZqBNYJg5xVIbnYaWjMwh7YUEEZwyrE1BAMKiXBFpnBFCAQ7GF2EBOxF+DcW4eUnWj+h/dP+av8dwZtj9wSF7wpeRc8YMVY2bjrih8/g05jrIT0VfjDXZ84gfN6pmLeD50k+ml8Xi26FhzWSsiVSddvvHUABIGrHcMsMDAd8CYxDy7ol5cfJ12HEBxFm3WiNQF9pAigXWTbqKEJCsZQ0ZmCNYIUaAV6NGZCctiIwJ5/glXfQNYTWikCrAo5wlvxGDHY01OPcagjGDNQQwmYQBoSYyCBUtBRySkhC5LrcAO46tU8q+pDgdVt0Xu/uyQ+Mka+2zDexYvyskeo8n8I3gg8Ln0BOmBvNFc/w5INgeHSHJ/gYgyc018DClvLZyKukxnkp3vNPcS8QteOgVQJS9TxBzvt+oAnMpehv+JscCpKHJApIRROgGdiqwJSaNAKLoCKgEdjRiuVqUBnoqAaiE3GGoCMhzYAiscKhqDZDXJshti0QnwEE6mKrAqO4YgsEbcD3hwv8fiuB9wH4f7s+s37bJ3MPRfSRfTXzffQ7UXlPsavgHdEHI72O9ohXeOdeUOYDJt462lP4Ni8UPcFc8eEe3LsfzikB0/9s1FXSpHKKx/wVG4HoQZ/HSeMFGjwi4EtkIrx44/kQOkhBzIQJKGgIfCWJFj2EaoDEevLv0YUVQWAE1gyCEYlEdcwgzgg4uukI5xhCUCFAIBwluRwY0M+Tb5RPhzSXz4Y1l/0jrrW4TvYPv9bcGnz/0Kvli2FXy5fDrgKulq+HXyXfAt8B3wM/Aj+NuEp+Bn4Bfh3ZTA4Cvzng/38d0cz8nd/j93+wv+dyvgG+Go51AAewHq5vPzH8mr/7xP7g/5+gr1+Nuk7+mon5NY+3GxOg6FXsCo/oDazgDRjDsOhV+Iy9HfGN6JkbgIdzeXuv2ZzeMY8qfJtPiP/zt66SKyulWvzEtUDUjqOWH5gF+JIZh8wZ00uHW8vJ7ySIMYEWFpYwfM/SkaQiuVwTCEYetyJIyggAYwSAMQJCBaBGYEEj4OfzW8u3Q2vLjDtLSM8GuaVLnZzSA689L80jvS7LJ32bFJVBVxaWEVcWkLFN8svkpvll5lX5ZMHV+WTZNXllzbV5ZPN1eWT79XnkvRa5Ze8NueXjm3LJ5zfllAM355QvAb5+hv/vuzGX7MHfd+J72/D9jfjtamBJ87wyD8ub3iyfTMTyR1+ZT4Y1LigDrjxZel9e0PSF6FYvl3Srm1MGNy0o77xyoci0qxELxIZTnUDwCit6M02yoleYnXoEY0fR+4RP87WxZy64v4a54XyfV/eZaZzmz2IW8rjgZvkCI3/TS071ciEJ8KYzWYCoHWeN9wz8APAlNREeuPJM+WlicxCIJnB9TPhxwOckF/co62iznHDNgCOSawRqBpbUwchGMyBIfrcicA2BonlZ/lp0n/wyobmsfq6C9GtSRF6pmk1erJBBXqtTUIY8cq3M6vWSrBzdWzaM7ytbx/eUdyf0kN2TesqHU3rJx1N7y+fTe8uXM3rL1zN6yXfAjzN7yc8ze8ovs3rKr8AveP8T8AM+/xZ//wrfPzC9l3w2rZd8NLWn7JnUQ3aO7y7bxnaXDaO7y8pRPWVO/9dkZLt7pHOzs+WlylnkxYszScdauWT8raXlk/4N5BBHX4o1Tvi6bVb4RvwUvBOTOOGr6Akr/MBwKXqCI/7jMQPgVI37bYzwmS99BSj+RTfLgVHNpFnaxM9rTEoDUTtOGy/ZTPZKQRd3NiwtP02iCWAE4f31icAA7HvuF1j8sK0GLPmMEagZqBFoVQAERkCA4LrTUE9XNdBRkOWwBYXDnWIrsNwlbeSHCTfIinZVZej1p8lrNXNJl6ZnyZQO98mGWaPlw+0b5Ycff5Y/5ci2v4BDwI+/i3yyd49sWz5X5vZ7RfrfXkterpZTul9WUCbfVUbe6Qrhz2qFuGC6RDEb8XMbuC2hUV632S3xg/hYwZudeoQKn7ElEGfGnLGn+AlONThVC3JlMQOg+GHq7/e/Ii1zfuJDIHrG//9Bux1I9ilCLm6qfZp8OOxKlIscQa5zjICv/D/A48hmSoBqwIxAJGLICMzDMtUIHDMwIxpHOECNgAJYTUMgVCQUjVYIGDn53RVYz+rH5dvJd8jGTo1kyj3nS99Lc0rv2lll7C3ny9JXW8o7o1+XfQtGyldbFsov+z+QP378xgg5Le3Qz9/Lwa8/le/eWyefrZws703sImt7PCizHr1Uhlx5kvSoliDDry0qC9pWlT2DMd/nwzRXYftZlps99uhv0H8VvDU8rYa4zYyBij5O8IqQ8NV0WYUx7hz1TcnPqk1zRfHbPDFnC2+SNW/Wk/Kn5/PmOwl8BtQDovZ/0p4HfIn2ouZ5hWRN53ogjxLrWgtLLL5nicnzBUhCYwSOGZgRiiMVjcCagTEEawphM3CrA1MhQCBBaQwBmR2JEBNf+b21eN34svy2+Cn5dMyd8nbXJrLowbIyqWkOGVszQWY1zSnLbztNNj9WWXa2qyd7O14ln/W8Rb7scaP88NbjKPs7ycHpHQx+nfE6pj4vyLcD7pADPW+WT7rfLLtebCDb21aXdW3OkUUtCsmU+gkysWE6mXNbcVn7Qk3ZPfAG+WrKfegH+rcRBmVGcQrcCl733BtYsSvU/LS094k+iJuO9oAZ7RFjip+g6fCcfgo+LjcWswHkb9IzVeXMU3J585wEfgRuAqL2f9R4+uYLgC/hXpxZNKdMfq4qSGbJxFtnB0agwOdzbouVoDQCMzJZ8Km5qTKDkCHETRcAUyUQFJhjCJwebHxDZBsPw3WV35a/LN/PeUoOTLhXPuzdXN5tX1W23FdCNt2cSzZflSDvNEuQ3U0T5OMbEmT/bVnkq9syGXzZKrN8dkt6+ah5gryP72wHNl+XQbbdfYq8/cR5sqtjffl0aEv5dvpj8ssi9IHH89/m4b8usUN7akwGNC4rdAO7DUbsVvAGdruN2F24ordmGpjro4gvsBhx5kNeuKMvTvg2JzTsOdfLXzOvlY6ty0n+XKk6w0/xC3AHELX/w5YeeA74HfAlPxEKgDxv3l5Ofp3YDKUmRxqYAI0gzgzwnjueeFfZJZj/kqSKwAxcI3DNADBGQFAQhDUCrxkQNAPCjrg8zMbTeHkjjneHiOwAtvWXPzf2lD/WdpHfV3YCOsrBhS/Ij5Puk58n3iu/TLwH23SPHLSvv0xsgyrgXvl52iPy29JXzPf/WNNZDq3vJn/xGoPtA2PLfQevvCiIJwPxSIUZ4bU/juANVPRA3ChvtzcQvIreFb6NmxtLGqxexsvS3s3BdAL/h+hlQQv5dNgVcnuDUpIxfYo39nDBu/zyLtNR+z9vaTIBolX9ErJnwGUQOUaXmTSA5jHCBcD/SUReXcZ7zJtqQEEjUDPgiEawpA2ZQTDXDZuBYwiBKTigKXCPOk8a4hV6vMKP9/Hj8wXfHy+ya1wM7/NBHvbW3kmBz+h/n9/ng0n5Gz7cE5/xJiG8apBnCPJYvhnprcgVbh91pI8Tvd3OpESvJX4gfCeGfI4Db+XOIzE0XGPEVvSK2fj//Otl6Wu1pMo5Bbx5TAa8yo/7iqJ2AjSeKNQW+A7wkcGLimfkk7kvVgPRSDaAx7mNEbjAZ9z7bO4uy2rgYUBfQWRjBGFDoBlQBIQVRXBEgbAjJqGlsztt4Gmw5r39jEcNeFouL+fl/fyMsCeI7J4Se/bf3jnAbA/wOR8LzmcF8ok/NBHe3JNXAPJMPjN/D69TYfsU9FX7bhGIHdDtdQWvMQlEj5jxSAtBQ+Wde8xIrzFX4du4z7tOfkc+et9zoRTJm9Wbv2SwD2gKRO0Eay2BbwEfKbw4KV8W6XTb+fLj+CYgHQhJE5h2FQASmvckqCUpjxRwrspTUjmCGZDUJDhel7qGoEJwDMFUCI4pGFBMChUbhUdAiDra8u8UHktxjti8AIcX61DMia75d8ALgmgePHmHO/OMqLHeuPXYdZj1KGyf9CgI+63bYMD37iiv240YMB4qdiN8AjFbyHIfRkpDVbN140zMwP/nXis7+l4qt9dP8U6+PmwH6gJRO0FbY4BXefnI4QXvKdC86inybt/6MAGMQjNJRpqAQgmK99w5xTnrovtjpOaea1YGriFodRDMdSEQ3rgiEIwDYwjJmYLCCjMYfe3vzHLUbHzQdeA3wQgeEnkAtw/u8j0w28Rt43YSdruN8BEHExcCoid4aI8X72gc3ZgSNNnZED9iP6FtJSlXIo83VylgFRA92SdqCRcDyd5V2IdSJ+WQIQ+Vl0OTm2LuDzJOt+SMQ7MYWfWQoXkEtSU5EVcdAO50IYBrDI5QCd1THsAK0QtXsMnB91tFaH2uyNmvQOiKsOCdbTWGyDggJnw1sUG5zxN6OL/X+IVjSsOd21z2D28k911eSvLmyOTNTwoYAhQDohY1004DRgLJ3lIsjKyZ0stNtYrJLlYDs0HOWSDnVJBWYQis7/G3WZwW3GPJTuJb8hvQDKwhGCNQWPHEmQGhgrOIG8l9CAs2Kfh+6yC83rg+OWI3sNtgtssi2F7G4D5H+K1jFVMQLxW9jSHfQ/i/T2kqU5+tLOVLp+oa/jB+ALgTODq3P2qJWk6AJwyl+tRhxekn55DhD10kP0+8EuUr56moCqYqLKH1/9x5xXMHjBFQAAQEYMzAMQRXNAauGSh0hHVFmBr4hEz4vuuDrjfUH3d0VwSlPYWuosf26ivP3+fefQqc8TGx05hZzMLf5jWXnb3qyoONS0uWjOm9eUgBu4Hoqr6opdh4/QBv/ewjUbJoWbuYrHmzJgQOws4Ccac0iZE6AP9vPzMnEbWK7Sg0gtBXmgHEYeCYwmKfKRAqOhcUY0ic/wTedXj6okIPYLdDt8vFXEyJeDIPK6MgPhS8fY+R3pgBDPXnCY1lwL0XyHnFc3vjngpMByoCUYtaqtq5wDjAR6ZkUbJwduncuox8O7pRzAiM4FEZTFHg/wZ4T8JzLzcvMKIRJIIrGgqJgLDM9IGGgFcDNQiFR5yHjfCyCbte9kH7w76p2NlvA90OVDv6nvtCZt0cE76JgcbDAT+bgdggfos7VJdrqhT1xjoV4Mk9LwN5gahFLU2NpOGNRb4EfORKEunSJUjt8wrIjOcrgegg+QyOamoAYTS2RnADjOD2mFjmUzB3W+HoexWSFVecKVjEVQ1HGK7QDWwfArGzb9pHp99mW4B5d9kRH1MkNUAfpiNWs5rKvkH15amrz5BTC6T5uL5iPRA9wDNq/7jxqjCSyUeyZJE/Vya5tc6p8l7v2jABkHsmiQ/BJwXXCDhS8nAYTzE24HuLOHMg7OhqphEKK9B/DGeZiQROaH9s38L91Vtx64g/mduqgne2fQZiM6uJ/Drhcnnr0Yvk/BJpuoDHBZ/cOwgoCUQtakekFQT4/ME07yAkShXJLt1uLyNfDKuPshYjHM1g8hUOrAj4fgoQGEGrmIA4ehpDUHGFRBYAglQkEuo/QLDc8PqcvmgfFeZmnCz1MQ1yt00Fz23mZ1PxOruJ/Ib3S1+pIg0vLCRZMh3WTj7ifeBmIBMQtagd8XYFsA7wkS9FVD83v0x9uoL8MKYhRMNpgRVBHC63oDjwHRrBbD68glUBzcCFFZsrRC/Cwk0NfMuxCIQe7g9gLtaxe/WD7XG3zwFGfJneWLZ1qyEPXlFSsmfJ4I1bKsCr+AYDZwBRi9pRbUWBN4E0XUugSJ8uQVpULyrzX6qEkRDimM1qAK+TVPgecM7MK+B4qy3e456jK4VmXgFjDi48wvxHCC1f1xusH+ChTXPaLqqXuP6r4O3/uZ3T8f+5TWTvgDrywnVnmB2nvlilEmuAW4GoRe1fbY2AhYCPlCmiYK7M8uDlJWRrt+qYEkAQ0yiQRhDIZRZ8r7D/pxGYw4eYU7uPtvYiJNLDgm+5IRjhw5w4bXH7Gga3bQowu7F8M6K+vHbzWVL5zMM6mUfBk3peB6K5ftT+s8Zq4FngG8BH0hRxxsnZ5fGmJeWLIXVhAhAIMUVNIAxMHfhq9pZfAzFhjk0Buo+5Pqrguux6uG6esktTosAnXur00wGFT3Ob3kh+GdNABt1/vlQ7O0235/JhDlAbiFrUjonGR5JNA7gH2kfYFHHOqTmla6tz5MDg2hhNIRwIxghoIkSvMAbA9xQbXllac67Ne+KZ591hisD9BXx1n4D7T2CWqcA6uC7u0edOPO2f9ivon33lNsDQfnirvox6uJxUPTvvP5nnE3sAPp+Pt32PWtSOqcbzy1sD7wA+8qaIDOnTydlFc0jvO86V/TSCmRDQdIoMgo+DFVnwHt8xJ89gROZOQ3NffBqCAqN1muD8lsviMrlsroPrilu3C9s/9hl9/2JoHRn7SEz4fP6Cb5tTCZ6L0R+IbtEdtWO+8Sozzk15owkfmVOFGufmk9EPny/7h9aCEDmSUmANLKzQAujnAMtxHj3g9fMUrd7anLctM0/IoTn4wL/hO/yuubsufstlcFmmxHfWkWj9FlPRR/R1/9DaMuqhslLnvPzebUsDWFGx3G8IRC1qx1Xjg0lGADxE5SN3qnDpBQVk4D3nyqcDakKgGFmnQIATQjCirI/3LurFPjdzcczBzfH3K2OjuJ5vHwCf6Yk5/K4p7blsLMNdJtehJuCufzLEP6uRfDustgxpUwZ9LujdljSC1+uzosoKRC1qx2Xj04qvAxYAPpKnGtXPySev33S67B9UA6MzBEcjMKKnSFNCXZHxqQS/611GCFw3+zD9UvlmWE0Z2OZcaVAuv6Tz9D2N+BTgjtVo737U/m/aSQBvOpmmuw/5cEGJXNIBRrCvb1WMvBDiFAiRr2kR+eGC69B1TqonXw6uYUypfOlc5t4Ivv6mAd8DvHCnLBC1qP1fNhrBkwBHOZ8IUgXuLCxZOKs8dHkx2dKpovwylkcOIMypLNEp1jpHFlwml411/DKmtuzoWkmeaFJcShfJltZbb/vAQ6jDAV6uy1u3Ry1q//ftTKA78BHgE0WqkStbRmlRrYjMaltOPupTRf4cU0tkJsQ6nYYA4U6CgCeEBJ0c+N1JFDx+PwPLwbK4TC571tPlzLpyZ8/o7UsawTMpxwOVgEj4UTsh20UARz8+i84nkjSh3Gk55PEri8mER8rI2lcvkl3dK8nPI6rHjGAu5uuzIGgr6jjwM/6N38F3fx5RA7+tjGWUN8viMrls3zoPA3z01kzgeoBPbIpa1E74xkuOeQnrYZ9RGEbRfFnkYszNW1QtbAT8couS0vfOM2Xcw+fK6IfOiQM/49/4HX6Xv+FvuQzfsg8TvwOTAe4Uje7JF7WoeRpPb+0N/KNDh8khW+b0ZoedC37m++4RxCTgKiAXELWoRS2ZxkOH1YCBwFEzgn8BB4EJQBMgOnU3alFLY2OZXBkYCnwC+ER2LILP2eMJUOx7DiBqUYvaP2h8luFZAI8avAv4RHcs4D2gL1ABYJ+jFrWoHeHGB5g8A6wA/gR8Qvw3wT6wL+2A6G48UYvav9ROBm4DWGofAHziPJrgOrlu9oF9iVrUovYfNN1heA/Ae+LtAnyCPRLgsrkOrovr5LqjFrWoHSMtO3A6cAlwJ9ARmAXwluY8z54PyeCeeV5i6wP/xu/wu7zP3kSAy+CyuEwum+uIWtSidhy0DACfd8gHnPAaBN6ngCM372x8WQj8jH/jd/jdPADFzmVELWpRi1rUoha1qEUtalGLWtSiFrWoRS1qUYta1KIWtahFLWpRi1rUoha1qEUtalGLWtSiFrWoRS1qUYta1I6VlpDwP8Xi11XOCwTXAAAAAElFTkSuQmCC",
            //    Nome = "Nome do usuário",
            //    DataInicioCooperado = DateTime.Parse("01/01/2022"),
            //    CapitalSocial = 1000
            //};
            Query query = new Query(@"SELECT
                                        A.K_AVATAR,
                                        A.NOME,
                                        A.K_DATAINICIOCOOPERADO,
                                        B.K_CAPITALSOCIAL,
                                        D.K_RELATORIOENTREGADIA,
                                        D.K_RELATORIOENTREGAPERIODO,
                                        D.K_RELATORIOPROGRAMACAO
                                    FROM
                                        Z_GRUPOUSUARIOS A
                                        CROSS JOIN EMPRESAS B
                                        CROSS JOIN GN_PARAMETROS D
                                    WHERE
                                        A.HANDLE = @USUARIO");
            var registros = query.Execute();
            byte[] bytes;
            foreach (EntityBase registro in registros)
            {
                if (registro.Fields["K_AVATAR"] != null)
                {
                    bytes = (byte[])registro.Fields["K_AVATAR"];
                    retorno.AvatarBase64 = Convert.ToBase64String(bytes, 4, bytes.Length - 4);
                }
                else
                {
                    retorno.AvatarBase64 = "";
                }
                 //tratar campo nulo
                retorno.Nome = Convert.ToString(registro.Fields["NOME"]);
                retorno.DataInicioCooperado = Convert.ToDateTime(registro.Fields["K_DATAINICIOCOOPERADO"]);
                retorno.CapitalSocial = Convert.ToInt32(registro.Fields["K_CAPITALSOCIAL"]);
                retorno.RelatorioEntregaDia = Convert.ToInt32(registro.Fields["K_RELATORIOENTREGADIA"]);
                retorno.RelatorioEntregaPeriodo = Convert.ToInt32(registro.Fields["K_RELATORIOENTREGAPERIODO"]);
                retorno.RelatorioEntregaProgramacao = Convert.ToInt32(registro.Fields["K_RELATORIOPROGRAMACAO"]);
            }


            return retorno;
        }

        public List<ProgramacaoModel> buscarProgramacao(ProgramacaoBuscarModel request)
        {
            List<ProgramacaoModel> retorno = new List<ProgramacaoModel>();

            //List<EntityBase> registros = Entity.GetMany(EntityDefinition.GetByName("K_GN_PROGRAMACAO"), new Criteria());

            Query query = new Query(@"SELECT C.HANDLE,
                                                C.COTA,
                                                C.QUANTIDADECOTA,
                                                C.QUANTIDADEPROGRAMADA,
                                                D.PRODUTO,
                                                P.DATAINICIO,
                                                E.NOME
                                                FROM K_CM_PROGRAMADOFORNECEDORES C
                                                JOIN K_CM_PROGRAMADOPRODUTOS D ON C.PROGRAMADOPRODUTO = D.HANDLE
                                                JOIN PD_PRODUTOS E ON D.PRODUTO = E.HANDLE
                                                JOIN K_CM_PROGRAMADOS P ON D.PROGRAMADOS = P.HANDLE
                                                WHERE P.DATAINICIO <= :FIM AND P.DATAFIM >= :INICIO AND EXISTS (SELECT PESSOA FROM K_GN_PESSOAUSUARIOS U WHERE U.USUARIO = @USUARIO AND U.PESSOA =C.FORNECEDOR)");
            
            query.Parameters.Add(new Parameter("INICIO", request.Inicio));
            query.Parameters.Add(new Parameter("FIM", request.Fim));
            var registros = query.Execute();

            foreach (EntityBase registro in registros)
            {
                retorno.Add(new ProgramacaoModel() // adicionar filtro de periodo inicio fim
                {
                    Handle = Convert.ToInt32(registro.Fields["HANDLE"]),
                    Produto = Convert.ToString(registro.Fields["NOME"]),
                    Periodo = Convert.ToDateTime(registro.Fields["DATAINICIO"]),
                    Programado = Convert.ToInt32(registro.Fields["QUANTIDADEPROGRAMADA"])
                });
            }

            return retorno;

            //retorno.Add(new ProgramacaoModel()
            //{
            //    Handle = 1,
            //    Produto = "Produto A",
            //    Periodo = "Janeiro 2022",
            //    Programado = 10
            //});

            //retorno.Add(new ProgramacaoModel()
            //{
            //    Handle = 2,
            //    Produto = "Produto B",
            //    Periodo = "Janeiro 2022",
            //    Programado = 20
            //});
        }
        public ResponseModel enviarSac(SacModelPost request)
        {
            

            ResponseModel retorno = new ResponseModel();
            retorno.status = 1;
            retorno.descricao = "Sucesso";

            
            EntityBase Sac = Entity.Create(EntityDefinition.GetByName("K_SAC"));
            Sac.Fields["TITULO"] = request.Titulo;
            Sac.Fields["MENSAGEM"] = request.Mensagem;
            Sac.Fields["COR"] = new ColorField(255);
            Sac.Fields["STATUS"] = new ListItem(1, "");
            (Sac.Fields["USUARIOENVIO"] as EntityAssociation).Handle = BennerContext.Security.GetLoggedUserHandle();
            Sac.Save();

            
            try
            {
                NotificacaoSacRequest requestemail = new NotificacaoSacRequest("", $"Nova mensagem Ouvidoria: {request.Titulo}", request.Mensagem); 
                BusinessTask.Factory.NewComponentTask<INotificacaoSac>()
                        .WithDescription("Notificação de Sac")
                        .WithNotification()
                        .WithRequestValue(requestemail)
                        .Start();
            }
            catch (Exception e)
            {
                throw e;
            }


            return retorno;
        }
        public List<SacModelGet> buscarSac()
        {
            //string cor = ColorField.OleColorToHtmlHex(3150273);
            List<SacModelGet> retorno = new List<SacModelGet>();
            //retorno.Add(new SacModelGet()
            //{
            //    Handle = 1,
            //    Numero = 1,
            //    Titulo = "Titulo 1",
            //    Mensagem = "Mensagem 1",
            //    Resposta = "Resposta 1",
            //    Color = cor,
            //    Status = 1
            //});
            //retorno.Add(new SacModelGet()
            //{
            //    Handle = 2,
            //    Numero = 2,
            //    Titulo = "Titulo 2",
            //    Mensagem = "Mensagem 2",
            //    Resposta = "Resposta 2",
            //    Color = cor,
            //    Status = 2
            //});
            Query query = new Query(@"SELECT * FROM K_SAC
                                        WHERE USUARIOENVIO = @USUARIO");
            var registros = query.Execute();
            foreach (var registro in registros)
            {
                //int? Color = ((ColorField)registro.Fields["COR"]).Value;
                //int colorInt = Color.Value;
                int corInt = Convert.ToInt32(registro.Fields["COR"]);
                //string corHexa = corInt.ToString("X");
                retorno.Add(new SacModelGet()
                {
                    Handle = Convert.ToInt32(registro.Fields["HANDLE"]),
                    Numero = Convert.ToInt32(registro.Fields["NUMERO"]),
                    Titulo = registro.Fields["TITULO"] != null ? Convert.ToString(registro.Fields["TITULO"]) : "",
                    Mensagem = registro.Fields["MENSAGEM"] != null ? Convert.ToString(registro.Fields["MENSAGEM"]) : "",
                    Resposta = registro.Fields["RESPOSTA"] != null ? Convert.ToString(registro.Fields["RESPOSTA"]) : "",
                    Color = ColorField.OleColorToHtmlHex(corInt),//"#" + corInt.ToString("X").PadRight(6, '0'),
                    Status = Convert.ToInt32(registro.Fields["STATUS"])
                });
            }
                


            
            return retorno;
        }

        public string ProcessarAnalise(ProssarAnaliseModel request)
        {
            List<QProgramacao> RetornoProgramacao = new List<QProgramacao>();
            List<QPedidoMercado> RetornoPedidoMercado = new List<QPedidoMercado>();
            List<QPedidoCooperado> RetornoPedidoCooperado = new List<QPedidoCooperado>();
            List<QCotas> RetornoCotas = new List<QCotas>();
            List<QOrdemProducao> RetornoOrdemProducao = new List<QOrdemProducao>();
            List<QQuebras> RetornoQuebras = new List<QQuebras>(); 
            List<QNFe> RetornoNFe = new List<QNFe>();


            Query queryOrdemProducao = new Query(@"SELECT GN_PESSOAS.APELIDO,
                                                               CP_ORDENSCOMPRA.DATADAORDEM,
                                                               CP_ORDENSCOMPRA.K_DATADOPEDIDO,
                                                               CP_ORDENSCOMPRA.NUMERO,
                                                               PD_PRODUTOS.CODIGOREFERENCIA,
                                                               PD_PRODUTOS.NOME AS 'PRODUTO',
                                                               PD_GRUPOSPRODUTOS.NOME AS 'GRUPO DE ITENS',
                                                               CM_UNIDADESMEDIDA.ABREVIATURA,
                                                               PRODUTOMATRIZ.NOME 'PRODUTOMATRIZ',
                                                               QUANTIDADE AS 'QTD PEDIDA',
                                                               SALDO AS 'FALTA RECEBER',
                                                               QUANTIDADE - SALDO AS 'QTD RECEBIDA'
                                                          FROM CP_ORDENSCOMPRAITENS
                                                               LEFT OUTER JOIN CP_ORDENSCOMPRA ON (CP_ORDENSCOMPRA.HANDLE = CP_ORDENSCOMPRAITENS.ORDEMCOMPRA)
                                                               LEFT OUTER JOIN GN_PESSOAS ON (GN_PESSOAS.HANDLE = CP_ORDENSCOMPRA.FORNECEDOR)
                                                               LEFT OUTER JOIN PD_PRODUTOS ON (PD_PRODUTOS.HANDLE = CP_ORDENSCOMPRAITENS.PRODUTO)
                                                               LEFT OUTER JOIN CM_UNIDADESMEDIDA ON (CM_UNIDADESMEDIDA.HANDLE = CP_ORDENSCOMPRAITENS.UNIDADE)
                                                               LEFT OUTER JOIN PD_PRODUTOS PRODUTOMATRIZ ON (PRODUTOMATRIZ.HANDLE = PD_PRODUTOS.K_PRODUTOMATRIZ)
                                                               LEFT OUTER JOIN PD_GRUPOSPRODUTOS ON (PD_GRUPOSPRODUTOS.HANDLE = PD_PRODUTOS.GRUPO)
                                                         WHERE CP_ORDENSCOMPRA.K_DATADOPEDIDO BETWEEN CONVERT(DATETIME, :INICIO, 103) AND CONVERT(DATETIME, :FIM,
                                                               103)
                                                               AND PD_PRODUTOS.NOME LIKE 'MP%'
                                                               AND PD_PRODUTOS.NOME NOT LIKE '%MPH%'
                                                               AND PD_PRODUTOS.NOME NOT LIKE '%MPOH%'
                                                               AND CP_ORDENSCOMPRA.USUARIOINCLUIU <> 62
                                                         ORDER BY GN_PESSOAS.NOME ");
            queryOrdemProducao.Parameters.Add(new Parameter("INICIO", request.Datainicio));
            queryOrdemProducao.Parameters.Add(new Parameter("FIM", request.Datafim));
            var OrdemProducao = queryOrdemProducao.Execute();
            foreach (var registro in OrdemProducao)
            {
                RetornoOrdemProducao.Add(new QOrdemProducao()
                {
                    Apelido = Convert.ToString(registro.Fields["APELIDO"]),
                    DataOrdem = Convert.ToDateTime(registro.Fields["DATADAORDEM"]),
                    DataPedido = Convert.ToDateTime(registro.Fields["K_DATADOPEDIDO"]),
                    Numero = Convert.ToInt32(registro.Fields["NUMERO"]),
                    CodigoRef = Convert.ToInt32(registro.Fields["CODIGOREFERENCIA"]),
                    Produto = Convert.ToString(registro.Fields["PRODUTO"]),
                    GrupoItens = Convert.ToString(registro.Fields["GRUPO DE ITENS"]),
                    Abreviatura = Convert.ToString(registro.Fields["ABREVIATURA"]),
                    ProdutoMatriz = Convert.ToString(registro.Fields["PRODUTOMATRIZ"]),
                    QtdPedida = Convert.ToInt32(registro.Fields["QTD PEDIDA"]),
                    FaltaReceber = Convert.ToInt32(registro.Fields["FALTA RECEBER"]),
                    QtdRecebida = Convert.ToInt32(registro.Fields["QTD RECEBIDA"])
                });
            }

            Query queryQuebras = new Query(@"SELECT D.APELIDO AS 'PRODUTOR',
                                                       F.NOME AS 'GRUPO',
                                                       E.CODIGO,
                                                       E.CODIGOREFERENCIA AS 'CODIGOREFERENCIA',
                                                       E.NOME AS 'PRODUTO',
                                                       PRODUTOBASE.NOME AS 'PRODUTO BASE',
                                                       PRODUTOMATRIZ.NOME AS 'PRODUTO MATRIZ',
                                                       G.DOCUMENTODIGITADO AS 'LOTE',
                                                       G.DATAINCLUSAO,
                                                       A.DATA AS 'DATABAIXA',
                                                       SUM(H.QUANTIDADE) AS 'QUANTIDADEENTRADA',
                                                       J.ABREVIATURA AS 'UN',
                                                       A.QUANTIDADE AS 'QUANTIDADEBAIXADA',
                                                       C.DESCRICAOMOTIVO AS 'MOTIVO'
                                                  FROM PD_BAIXASDIRETAS A
                                                       LEFT OUTER JOIN GN_OPERACOES B ON (B.HANDLE = A.OPERACAO)
                                                       LEFT OUTER JOIN PD_MOTIVOSACERTOESTOQUE C ON (C.HANDLE = A.MOTIVO)
                                                       LEFT OUTER JOIN GN_PESSOAS D ON (D.HANDLE = A.PESSOA)
                                                       LEFT OUTER JOIN PD_PRODUTOS E ON (E.HANDLE = A.PRODUTO)
                                                       LEFT OUTER JOIN PD_GRUPOSPRODUTOS F ON (F.HANDLE = E.GRUPO)
                                                       LEFT OUTER JOIN FN_DOCUMENTOS G ON (G.HANDLE = A.K_DOCUMENTODIGITADO)
                                                       LEFT OUTER JOIN CM_ITENS H ON(H.DOCUMENTO = G.HANDLE AND H.PRODUTO = E.HANDLE)
                                                       LEFT OUTER JOIN K_PD_GRUPOSGRUPODEPRODUTO I ON (I.HANDLE = F.K_GRUPO)
                                                       LEFT OUTER JOIN CM_UNIDADESMEDIDA J ON (J.HANDLE = E.UNIDADEMEDIDAESTOQUE)
                                                       LEFT OUTER JOIN GN_CATEGORIASFORNECEDORES K ON (K.HANDLE = D.CATEGORIAFORNECEDOR)
                                                       LEFT OUTER JOIN PD_PRODUTOS PRODUTOBASE ON (PRODUTOBASE.HANDLE = E.K_PRODUTOBASE)
                                                       LEFT OUTER JOIN PD_PRODUTOS PRODUTOMATRIZ ON (PRODUTOMATRIZ.HANDLE = E.K_PRODUTOMATRIZ)
                                                 WHERE DATA BETWEEN :INICIO AND :FIM
                                                       AND A.OPERACAO IN (470)
                                                       AND C.HANDLE IN (29, 30, 31, 33, 11, 2)
                                                       AND F.HANDLE IN (82, 83, 84, 92, 93, 36, 37, 100, 101, 65, 82)
                                                 GROUP BY D.APELIDO,
                                                       F.NOME,
                                                       E.CODIGO,
                                                       E.CODIGOREFERENCIA,
                                                       E.NOME,
                                                       G.DOCUMENTODIGITADO,
                                                       G.DATAINCLUSAO,
                                                       A.DATA,
                                                       J.ABREVIATURA,
                                                       A.QUANTIDADE,
                                                       C.DESCRICAOMOTIVO,
                                                       PRODUTOBASE.NOME,
                                                       PRODUTOMATRIZ.NOME
                                                 ORDER BY A.DATA ASC ");
            queryQuebras.Parameters.Add(new Parameter("INICIO", request.Datainicio));
            queryQuebras.Parameters.Add(new Parameter("FIM", request.Datafim));
            var Quebras = queryQuebras.Execute();
            foreach (var registro in Quebras)
            {
                RetornoQuebras.Add(new QQuebras()
                {
                    Produtor = Convert.ToString(registro.Fields["PRODUTOR"]),
                    Grupo = Convert.ToString(registro.Fields["GRUPO"]),
                    Codigo = Convert.ToInt32(registro.Fields["CODIGO"]),
                    CodigoRef = Convert.ToInt32(registro.Fields["CODIGOREFERENCIA"]),
                    Produto = Convert.ToString(registro.Fields["PRODUTO"]),
                    ProdutoBase = Convert.ToString(registro.Fields["PRODUTO BASE"]),
                    ProdutoMatriz = Convert.ToString(registro.Fields["PRODUTO MATRIZ"]),
                    Lote = Convert.ToString(registro.Fields["LOTE"]),
                    DataInclusao = Convert.ToDateTime(registro.Fields["DATAINCLUSAO"]),
                    DataBaixa = Convert.ToDateTime(registro.Fields["DATABAIXA"]),
                    QtdEntrada = Convert.ToInt32(registro.Fields["QUANTIDADEENTRADA"]),
                    UN = Convert.ToString(registro.Fields["UN"]),
                    QtdBaixada = Convert.ToInt32(registro.Fields["QUANTIDADEBAIXADA"]),
                    Motivo = Convert.ToString(registro.Fields["MOTIVO"])
                });
            }

            Query queryNFe = new Query(@"SELECT FN_DOCUMENTOS.DATAEMISSAO,
                                               FN_DOCUMENTOS.DOCUMENTODIGITADO,
                                               GN_PESSOAS.HANDLE AS 'APELIDO', --SUBSTITUI O APELIDO PELO HANDLE
                                               GN_PESSOAS.CATEGORIAFORNECEDOR,
                                               PD_PRODUTOS.HANDLE AS 'ITEM', --SUBSTITUI O NOME PELO HANDLE
                                               A.NOME AS 'ITEM BASE',
                                               MATRIZ.HANDLE AS 'ITEM MATRIZ', --SUBSTITUI O NOME PELO HANDLE
                                               CM_ITENS.VALORUNITARIO * CM_ITENS.QUANTIDADE 'VALORLIQUIDO',
                                               0 'DESCONTOVALOR',
                                               CM_ITENS.QUANTIDADE,
                                               CT_CC.NOME AS 'CENTRODECUSTO',
                                               K_PD_GRUPOSGRUPODEPRODUTO.NOME AS 'UNIDADE'
                                          FROM CM_ITENS
                                               LEFT OUTER JOIN FN_DOCUMENTOS ON (FN_DOCUMENTOS.HANDLE = CM_ITENS.DOCUMENTO)
                                               LEFT OUTER JOIN GN_PESSOAS ON (GN_PESSOAS.HANDLE = FN_DOCUMENTOS.PESSOA)
                                               LEFT OUTER JOIN PD_PRODUTOS ON (PD_PRODUTOS.HANDLE = CM_ITENS.PRODUTO)
                                               LEFT OUTER JOIN PD_PRODUTOSPAI ON (PD_PRODUTOSPAI.HANDLE = PD_PRODUTOS.PRODUTOPAI)
                                               LEFT OUTER JOIN FN_CONTAS FINENTRADA ON (FINENTRADA.HANDLE = PD_PRODUTOSPAI.CONTAFINANCEIRAENTRADA)
                                               LEFT OUTER JOIN FN_CONTAS FINSAIDA ON (FINSAIDA.HANDLE = PD_PRODUTOSPAI.CONTAFINANCEIRASAIDA)
                                               LEFT OUTER JOIN FN_CONTAS FINDEVCLI ON (FINDEVCLI.HANDLE = PD_PRODUTOSPAI.CONTAFINANCEIRADEVCLI)
                                               LEFT OUTER JOIN FN_CONTAS FINDEVFOR ON (FINDEVFOR.HANDLE = PD_PRODUTOSPAI.CONTAFINANCEIRADEVFOR)
                                               LEFT OUTER JOIN CT_CONTAS CONTABILCONSUMO ON (CONTABILCONSUMO.HANDLE = PD_PRODUTOSPAI.CONTACONTABIL)
                                               LEFT OUTER JOIN CT_CONTAS CONTABILESTOQUE ON (CONTABILESTOQUE.HANDLE = PD_PRODUTOSPAI.CONTACONTABILESTOQUE)
                                               LEFT OUTER JOIN CT_CC ON (CT_CC.HANDLE = CM_ITENS.CENTROCUSTO)
                                               LEFT OUTER JOIN PD_GRUPOSPRODUTOS ON (PD_GRUPOSPRODUTOS.HANDLE = PD_PRODUTOS.GRUPO)
                                               LEFT OUTER JOIN K_PD_GRUPOSGRUPODEPRODUTO ON (K_PD_GRUPOSGRUPODEPRODUTO.HANDLE = PD_GRUPOSPRODUTOS.K_GRUPO)
                                               LEFT OUTER JOIN PD_PRODUTOS AS A ON (A.HANDLE = PD_PRODUTOS.K_PRODUTOBASE)
                                               LEFT OUTER JOIN PD_PRODUTOS AS MATRIZ ON (MATRIZ.HANDLE = PD_PRODUTOS.K_PRODUTOMATRIZ)
                                         WHERE FN_DOCUMENTOS.OPERACAO IN (54, 506, 326, 368)
                                               AND FN_DOCUMENTOS.DATAEMISSAO BETWEEN CONVERT(DATETIME, :INICIO, 103) AND CONVERT(DATETIME, :FIM, 103)
                                         UNION ALL SELECT FN_DOCUMENTOS.DATAEMISSAO,
                                                          FN_DOCUMENTOS.DOCUMENTODIGITADO,
                                                          GN_PESSOAS.HANDLE, --SUBSTITUI O APELIDO PELO HANDLE
                                                          GN_PESSOAS.CATEGORIAFORNECEDOR AS '1=COOP, 2=TERC',
                                                          PD_PRODUTOS.HANDLE AS 'ITEM', --SUBSTITUI O NOME PELO HANDLE
                                                          A.NOME AS 'ITEM BASE',
                                                          MATRIZ.HANDLE AS 'ITEM MATRIZ', --SUBSTITUI O NOME PELO HANDLE
                                                          CM_ITENS.VALORLIQUIDO,
                                                          CM_ITENS.DESCONTOSVALOR,
                                                          CM_ITENS.QUANTIDADE,
                                                          CT_CC.NOME AS 'CENTRO DE CUSTO',
                                                          K_PD_GRUPOSGRUPODEPRODUTO.NOME AS 'UNIDADE DE NEGÓCIO'
                                                     FROM CM_ITENS
                                                          LEFT OUTER JOIN FN_DOCUMENTOS ON (FN_DOCUMENTOS.HANDLE = CM_ITENS.DOCUMENTO)
                                                          LEFT OUTER JOIN GN_PESSOAS ON (GN_PESSOAS.HANDLE = FN_DOCUMENTOS.PESSOA)
                                                          LEFT OUTER JOIN PD_PRODUTOS ON (PD_PRODUTOS.HANDLE = CM_ITENS.PRODUTO)
                                                          LEFT OUTER JOIN CT_CC ON (CT_CC.HANDLE = CM_ITENS.CENTROCUSTO)
                                                          LEFT OUTER JOIN PD_GRUPOSPRODUTOS ON (PD_GRUPOSPRODUTOS.HANDLE = PD_PRODUTOS.GRUPO)
                                                          LEFT OUTER JOIN K_PD_GRUPOSGRUPODEPRODUTO ON (K_PD_GRUPOSGRUPODEPRODUTO.HANDLE = PD_GRUPOSPRODUTOS.K_GRUPO)
                                                          LEFT OUTER JOIN PD_PRODUTOS AS A ON (A.HANDLE = PD_PRODUTOS.K_PRODUTOBASE)
                                                          LEFT OUTER JOIN PD_PRODUTOS AS MATRIZ ON (MATRIZ.HANDLE = PD_PRODUTOS.K_PRODUTOMATRIZ)
                                                    WHERE FN_DOCUMENTOS.OPERACAO IN (244, 508, 328, 512)
                                                          AND FN_DOCUMENTOS.DATAEMISSAO BETWEEN CONVERT(DATETIME, :INICIO, 103) AND CONVERT(DATETIME, :FIM,103)
                                                          AND FN_DOCUMENTOS.STATUS = '2'
                                                          AND FN_DOCUMENTOS.STATUSNFE = '6'
                                                    ORDER BY FN_DOCUMENTOS.DATAEMISSAO ASC ");
            queryNFe.Parameters.Add(new Parameter("INICIO", request.Datainicio));
            queryNFe.Parameters.Add(new Parameter("FIM", request.Datafim));
            var NFe = queryNFe.Execute();
            foreach (var registro in NFe)
            {
                RetornoNFe.Add(new QNFe()
                {
                    DataEmissao = Convert.ToDateTime(registro.Fields["DATAEMISSAO"]),
                    DocumentoDigitado = Convert.ToString(registro.Fields["DOCUMENTODIGITADO"]),
                    Apelido = Convert.ToInt32(registro.Fields["APELIDO"]),
                    CategoriaFornecedor = Convert.ToInt32(registro.Fields["CATEGORIAFORNECEDOR"]),
                    Item = Convert.ToInt32(registro.Fields["ITEM"]),
                    ItemBase = Convert.ToString(registro.Fields["ITEM BASE"]),
                    ItemMatriz = Convert.ToInt32(registro.Fields["ITEM MATRIZ"]),
                    ValorLiquido = Convert.ToDouble(registro.Fields["VALORLIQUIDO"]),
                    DescontoValor = Convert.ToDouble(registro.Fields["DESCONTOVALOR"]),
                    Quantidade = Convert.ToInt32(registro.Fields["QUANTIDADE"]),
                    CentroDeCusto = Convert.ToString(registro.Fields["CENTRODECUSTO"]),
                    Unidade = Convert.ToString(registro.Fields["UNIDADE"])
                });
                
                
                Query VerificarRegistros = new Query($@"SELECT * FROM K_GN_ANALISECOOPERADO A
                                                        WHERE A.FORNECEDOR = :PRODUTOR AND A.ITEM = :PRODUTOMATRIZ AND A.PROCESSO = {request.Processo}");
                VerificarRegistros.Parameters.Add("PRODUTOR", Convert.ToInt32(registro.Fields["APELIDO"]));
                VerificarRegistros.Parameters.Add("PRODUTOMATRIZ", Convert.ToInt32(registro.Fields["ITEM MATRIZ"]));
                var verificar = VerificarRegistros.Execute();
                if (verificar.Count == 0)
                {
                    double SomaValorLiquido = 0;
                    double SomaQuantidadeNFe = 0;
                    double Fornecedor = Convert.ToInt32(registro.Fields["APELIDO"]);
                    double ProdutoMatriz = Convert.ToInt32(registro.Fields["ITEM MATRIZ"]);

                    foreach (var precomedio in NFe)
                    {
                        if (Fornecedor == Convert.ToInt32(precomedio.Fields["APELIDO"]) && ProdutoMatriz == Convert.ToInt32(precomedio.Fields["ITEM MATRIZ"]))
                        {
                            SomaValorLiquido += Convert.ToDouble(precomedio.Fields["VALORLIQUIDO"]);
                            SomaQuantidadeNFe += Convert.ToDouble(precomedio.Fields["QUANTIDADE"]);
                        }
                        
                    }
                    EntityBase AnaliseCooperado = Entity.Create(EntityDefinition.GetByName("K_GN_ANALISECOOPERADO"));
                    AnaliseCooperado.Fields["FORNECEDOR"] = new EntityAssociation(Convert.ToInt32(registro.Fields["APELIDO"]), EntityDefinition.GetByName("GN_PESSOAS"));
                    AnaliseCooperado.Fields["ITEM"] = new EntityAssociation(Convert.ToInt32(registro.Fields["ITEM MATRIZ"]), EntityDefinition.GetByName("PD_PRODUTOS"));
                    AnaliseCooperado.Fields["PROCESSO"] = new EntityAssociation(request.Processo, EntityDefinition.GetByName("PD_PRODUTOS"));
                    AnaliseCooperado.Fields["PRECOMEDIO"] = SomaValorLiquido / SomaQuantidadeNFe;
                    AnaliseCooperado.Save();

                }
                try
                {
                    EntityBase AnaliseCooperado1 = Entity.Get(EntityDefinition.GetByName("K_GN_ANALISECOOPERADO"), new Criteria($"A.FORNECEDOR = :PRODUTOR AND A.ITEM = :PRODUTOMATRIZ AND A.PROCESSO = {request.Processo}", new Parameter("PRODUTOR", Convert.ToInt32(registro.Fields["APELIDO"])), new Parameter("PRODUTOMATRIZ", Convert.ToInt32(registro.Fields["ITEM MATRIZ"]))), GetMode.Edit);
                    int Fornecedor1 = Convert.ToInt32(registro.Fields["APELIDO"]);
                    int ProdutoMatriz1 = Convert.ToInt32(registro.Fields["ITEM MATRIZ"]);
                    double SomaQuantidadeNFe1 = 0;
                    foreach (var SomaQuantidade in NFe)
                    {
                        if (Fornecedor1 == Convert.ToInt32(SomaQuantidade.Fields["APELIDO"]) && ProdutoMatriz1 == Convert.ToInt32(SomaQuantidade.Fields["ITEM MATRIZ"]))
                        {
                            SomaQuantidadeNFe1 += Convert.ToInt32(SomaQuantidade.Fields["QUANTIDADE"]);
                        }
                    }
                    AnaliseCooperado1.Fields["ATENDIMENTONFE"] = SomaQuantidadeNFe1;
                    AnaliseCooperado1.Save();
                }
                catch(Exception ex)
                {

                }
                        
                    
                    //EntityBase VerificarAnaliseCooperado = Entity.Get(EntityDefinition.GetByName("K_GN_ANALISECOOPERADO"), new Criteria($"A.FORNECEDOR = :PRODUTOR AND A.ITEM = :PRODUTOMATRIZ AND A.PROCESSO = {request.Processo}", new Parameter("PRODUTOR", Convert.ToInt32(registro.Fields["APELIDO"])), new Parameter("PRODUTOMATRIZ", Convert.ToInt32(registro.Fields["ITEM MATRIZ"]))));

                
                
            }

            Query queryCotas = new Query(@"SELECT GN_PESSOAS.HANDLE 'PRODUTOR', -- troquei apelido pelo HANDLE
                                                               PRODUTOBASE.NOME 'PRODUTOBASE',
                                                               PRODUTOMATRIZ.HANDLE 'PRODUTOMATRIZ', -- troquei NOME pelo HANDLE
                                                               MAX(PERCENTUALPREFERENCIA) 'COTA'
                                                          FROM K_PD_PRODUTOCOTAPREFERENCIAS
                                                               LEFT OUTER JOIN K_PD_PRODUTOCOTAS ON (K_PD_PRODUTOCOTAS.HANDLE = K_PD_PRODUTOCOTAPREFERENCIAS.PRODUTOCOTA)
                                                               LEFT OUTER JOIN K_PD_COTAPERIODO ON (K_PD_COTAPERIODO.HANDLE = K_PD_PRODUTOCOTAS.COTAPERIODO)
                                                               LEFT OUTER JOIN PD_PRODUTOS ON (PD_PRODUTOS.HANDLE = K_PD_PRODUTOCOTAPREFERENCIAS.PRODUTODERIVADO)
                                                               LEFT OUTER JOIN PD_PRODUTOS PRODUTOBASE ON (PRODUTOBASE.HANDLE = PD_PRODUTOS.K_PRODUTOBASE)
                                                               LEFT OUTER JOIN GN_PESSOAS ON (GN_PESSOAS.HANDLE = K_PD_PRODUTOCOTAS.PESSOA)
                                                               LEFT OUTER JOIN PD_PRODUTOS PRODUTOMATRIZ ON (PRODUTOMATRIZ.HANDLE = PD_PRODUTOS.K_PRODUTOMATRIZ)
                                                         WHERE
                                                               (
                                                                   (
                                                                       :INICIO BETWEEN K_PD_COTAPERIODO.DATAINICIO AND K_PD_COTAPERIODO.DATAFIM
                                                                   )
                                                                   OR
                                                                   (
                                                                       :FIM BETWEEN K_PD_COTAPERIODO.DATAINICIO AND K_PD_COTAPERIODO.DATAFIM
                                                                   )
                                                               )
                                                         GROUP BY GN_PESSOAS.HANDLE, -- troquei apelido pelo HANDLE
                                                               PRODUTOBASE.NOME,
                                                               PRODUTOMATRIZ.HANDLE -- troquei NOME pelo HANDLE ");
            queryCotas.Parameters.Add(new Parameter("INICIO", request.Datainicio));
            queryCotas.Parameters.Add(new Parameter("FIM", request.Datafim));
            var Cotas = queryCotas.Execute();
            foreach (var registro in Cotas)
            {
                RetornoCotas.Add(new QCotas()
                {
                    Produtor = Convert.ToInt32(registro.Fields["PRODUTOR"]),
                    ProdutoBase = Convert.ToString(registro.Fields["PRODUTOBASE"]),
                    ProdutoMatriz = Convert.ToInt32(registro.Fields["PRODUTOMATRIZ"]),
                    Cota = Convert.ToInt32(registro.Fields["COTA"])
                });
                Query VerificarRegistros = new Query($@"SELECT * FROM K_GN_ANALISECOOPERADO A
                                                        WHERE A.FORNECEDOR = :PRODUTOR AND A.ITEM = :PRODUTOMATRIZ AND A.PROCESSO = {request.Processo}");
                VerificarRegistros.Parameters.Add("PRODUTOR", Convert.ToInt32(registro.Fields["PRODUTOR"]));
                VerificarRegistros.Parameters.Add("PRODUTOMATRIZ", Convert.ToInt32(registro.Fields["PRODUTOMATRIZ"]));
                var verificar = VerificarRegistros.Execute();
                if (verificar.Count != 0)
                {
                    EntityBase AnaliseCooperado = Entity.Get(EntityDefinition.GetByName("K_GN_ANALISECOOPERADO"), new Criteria($"A.FORNECEDOR = :PRODUTOR AND A.ITEM = :PRODUTOMATRIZ AND A.PROCESSO = {request.Processo}", new Parameter("PRODUTOR", Convert.ToInt32(registro.Fields["PRODUTOR"])), new Parameter("PRODUTOMATRIZ", Convert.ToInt32(registro.Fields["PRODUTOMATRIZ"]))), GetMode.Edit);
                    double SomaValorMedia = 0;
                    double Contador = 0;
                    int Fornecedor = Convert.ToInt32(registro.Fields["PRODUTOR"]);
                    int ProdutoMatriz = Convert.ToInt32(registro.Fields["PRODUTOMATRIZ"]);
                    foreach (var Media in Cotas)
                    {
                        if (Fornecedor == Convert.ToInt32(Media.Fields["PRODUTOR"]) && ProdutoMatriz == Convert.ToInt32(Media.Fields["PRODUTOMATRIZ"]))
                        {
                            Contador += 1;
                            SomaValorMedia += Convert.ToDouble(Media.Fields["COTA"]);
                        }
                    }
                    if (SomaValorMedia != 0)
                    {
                        AnaliseCooperado.Fields["COTAATUAL"] = SomaValorMedia / Contador;
                        AnaliseCooperado.Save();
                    }
                }
                
            }

            Query queryPedidoMercado = new Query(@"SELECT K_CM_CORTES.DATADOPEDIDO,
                                                       PD_PRODUTOS.CODIGOREFERENCIA 'CODIGOREFERENCIA',
                                                       PD_PRODUTOS.NOME 'PRODUTO',
                                                       PRODUTOBASE.NOME 'PRODUTOBASE',
                                                       PRODUTOMATRIZ.HANDLE 'PRODUTOMATRIZ',
                                                       SUM(K_CM_CORTEPRODUTOS.QUANTIDADEPEDIDO) 'PEDIDO',
                                                       SUM(K_CM_CORTEPRODUTOS.QUANTIDADEATENDER) 'CAISP'
                                                  FROM K_CM_CORTEPRODUTOS
                                                       LEFT OUTER JOIN K_CM_CORTES ON(K_CM_CORTES.HANDLE = K_CM_CORTEPRODUTOS.CORTE)
                                                       LEFT OUTER JOIN PD_PRODUTOS ON(PD_PRODUTOS.HANDLE = K_CM_CORTEPRODUTOS.PRODUTO)
                                                       LEFT OUTER JOIN PD_PRODUTOS PRODUTOBASE ON (PRODUTOBASE.HANDLE = PD_PRODUTOS.K_PRODUTOBASE)
                                                       LEFT OUTER JOIN PD_PRODUTOS PRODUTOMATRIZ ON (PRODUTOMATRIZ.HANDLE = PD_PRODUTOS.K_PRODUTOMATRIZ)
                                                 WHERE DATADOPEDIDO >= :INICIO
                                                       AND DATADOPEDIDO < = :FIM
                                                       AND K_CM_CORTES.STATUS NOT IN (5)
                                                       AND K_CM_CORTEPRODUTOS.QUANTIDADEPEDIDO > 0
                                                 GROUP BY K_CM_CORTES.DATADOPEDIDO,
                                                       PD_PRODUTOS.CODIGOREFERENCIA,
                                                       PD_PRODUTOS.NOME,
                                                       PRODUTOBASE.NOME,
                                                       PRODUTOMATRIZ.HANDLE");
            queryPedidoMercado.Parameters.Add(new Parameter("INICIO", request.Datainicio));
            queryPedidoMercado.Parameters.Add(new Parameter("FIM", request.Datafim));
            var PedidoMercado = queryPedidoMercado.Execute();
            foreach (var registro in PedidoMercado)
            {
                RetornoPedidoMercado.Add(new QPedidoMercado()
                {
                    DataPedido = Convert.ToDateTime(registro.Fields["DATADOPEDIDO"]),
                    CodigoRef = Convert.ToString(registro.Fields["CODIGOREFERENCIA"]),
                    Produto = Convert.ToString(registro.Fields["PRODUTO"]),
                    ProdutoBase = Convert.ToString(registro.Fields["PRODUTOBASE"]),
                    ProdutoMatriz = Convert.ToInt32(registro.Fields["PRODUTOMATRIZ"]),
                    Pedido = Convert.ToInt32(registro.Fields["PEDIDO"]),
                    Caisp = Convert.ToInt32(registro.Fields["CAISP"]),

                });

                
                int ProdutoMatriz = Convert.ToInt32(registro.Fields["PRODUTOMATRIZ"]);
                double SomaValor = 0;

                Query VerificarRegistros = new Query($@"SELECT * FROM K_GN_ANALISECOOPERADO A
                                                    WHERE A.ITEM = :PRODUTOMATRIZ AND A.PROCESSO = {request.Processo}");
                VerificarRegistros.Parameters.Add("PRODUTOMATRIZ", Convert.ToInt32(registro.Fields["PRODUTOMATRIZ"]));
                var verificar = VerificarRegistros.Execute();
                foreach (var Soma in PedidoMercado)
                {
                    if (ProdutoMatriz == Convert.ToInt32(Soma.Fields["PRODUTOMATRIZ"]))
                    {
                        SomaValor += Convert.ToInt32(Soma.Fields["PEDIDO"]);
                    }
                }
                if (verificar.Count != 0)
                {
                        
                    //List<EntityBase> QAnaliseCooperado = Entity.GetMany(EntityDefinition.GetByName("K_GN_ANALISECOOPERADO"), new Criteria("A.ITEM = :PRODUTOMATRIZ",new Parameter("PRODUTOMATRIZ", Convert.ToInt32(registro.Fields["PRODUTOMATRIZ"]))));

                    foreach (var registro1 in verificar)
                    {
                            
                        EntityBase AnaliseCooperado = Entity.Get(EntityDefinition.GetByName("K_GN_ANALISECOOPERADO"), new Criteria($"A.FORNECEDOR = :PRODUTOR AND A.ITEM = :PRODUTOMATRIZ AND A.PROCESSO = {request.Processo}", new Parameter("PRODUTOR", Convert.ToInt32(registro1.Fields["FORNECEDOR"])), new Parameter("PRODUTOMATRIZ", Convert.ToInt32(registro1.Fields["ITEM"]))), GetMode.Edit);
                        int Fornecedor = Convert.ToInt32(registro1.Fields["FORNECEDOR"]);
                        double SomaCota = 0;
                        int Contador = 0;
                        foreach (var Soma in Cotas)
                        {
                            if (Fornecedor == Convert.ToInt32(Soma.Fields["PRODUTOR"]) && ProdutoMatriz == Convert.ToInt32(Soma.Fields["PRODUTOMATRIZ"]) && Convert.ToInt32(registro1.Fields["FORNECEDOR"]) == Convert.ToInt32(Soma.Fields["PRODUTOR"]))
                            {
                                SomaCota += Convert.ToDouble(Soma.Fields["COTA"]);
                                Contador += 1;
                            }
                        }
                        if (SomaCota != 0 && Contador != 0)
                        {
                            double MediaSomatoria = SomaCota / Contador;
                            AnaliseCooperado.Fields["PEDIDOMERCADO"] = (SomaValor / 100) * MediaSomatoria;
                            AnaliseCooperado.Save();
                        }
                    }
                }



            }

            Query queryPedidoCooperado = new Query(@"SELECT K_CM_CORTES.DATADOPEDIDO,
                                                               GN_PESSOAS.HANDLE 'PRODUTOR',
                                                               PD_PRODUTOS.CODIGOREFERENCIA 'CODIGOREFERENCIA',
                                                               PD_PRODUTOS.NOME 'PRODUTO',
                                                               PRODUTOBASE.NOME 'PRODUTOBASE',
                                                               PRODUTOMATRIZ.HANDLE 'PRODUTOMATRIZ',
                                                               SUM(K_CM_CORTEPRODUTOFORNECEDORES.QUANTIDADEAJUSTADA)'PEDIDO',
                                                               SUM(QTDEENTREGA) QTDEENTREGUE
                                                          FROM CP_ORDENSCOMPRAITENS
                                                               LEFT OUTER JOIN K_CM_CORTEPRODUTOFORNECEDORES ON (K_CM_CORTEPRODUTOFORNECEDORES.HANDLE = CP_ORDENSCOMPRAITENS.K_CORTEPRODUTOFORNECEDOR)
                                                               LEFT OUTER JOIN K_CM_CORTEPRODUTOS ON (K_CM_CORTEPRODUTOS.HANDLE = K_CM_CORTEPRODUTOFORNECEDORES.CORTEPRODUTO)
                                                               LEFT OUTER JOIN K_CM_CORTES ON (K_CM_CORTES.HANDLE = K_CM_CORTEPRODUTOS.CORTE)
                                                               LEFT OUTER JOIN GN_PESSOAS ON (GN_PESSOAS.HANDLE = K_CM_CORTEPRODUTOFORNECEDORES.PESSOA)
                                                               LEFT OUTER JOIN PD_PRODUTOS ON(PD_PRODUTOS.HANDLE = K_CM_CORTEPRODUTOS.PRODUTO)
                                                               LEFT OUTER JOIN PD_PRODUTOS PRODUTOBASE ON (PRODUTOBASE.HANDLE = PD_PRODUTOS.K_PRODUTOBASE)
                                                               LEFT OUTER JOIN PD_PRODUTOS PRODUTOMATRIZ ON (PRODUTOMATRIZ.HANDLE = PD_PRODUTOS.K_PRODUTOMATRIZ)
                                                               LEFT OUTER JOIN CP_RECEBIMENTOFISICO ON (CP_ORDENSCOMPRAITENS.HANDLE = CP_RECEBIMENTOFISICO.ORDEMCOMPRAITEM)
                                                         WHERE DATADOPEDIDO >= :INICIO
                                                               AND DATADOPEDIDO < = :FIM
                                                               AND PD_PRODUTOS.NOME NOT LIKE '%MPH%'
                                                         GROUP BY K_CM_CORTES.DATADOPEDIDO,
                                                               GN_PESSOAS.HANDLE,
                                                               PD_PRODUTOS.CODIGOREFERENCIA,
                                                               PD_PRODUTOS.NOME,
                                                               PRODUTOBASE.NOME,
                                                               PRODUTOMATRIZ.HANDLE
                                                         UNION ALL SELECT K_CM_PREVIAS.DATADOPEDIDO,
                                                                          GN_PESSOAS.HANDLE 'PRODUTOR',
                                                                          PD_PRODUTOS.CODIGOREFERENCIA 'CODIGOREFERENCIA',
                                                                          PD_PRODUTOS.NOME 'PRODUTO',
                                                                          PRODUTOBASE.NOME 'PRODUTOBASE',
                                                                          PRODUTOMATRIZ.HANDLE 'PRODUTOMATRIZ',
                                                                          SUM(K_CM_PREVIAPRODUTOFORNECEDORES.QUANTIDADEAJUSTADA) 'PEDIDO',
                                                                          SUM(QTDEENTREGA) QTDEENTREGUE
                                                                     FROM CP_ORDENSCOMPRAITENS
                                                                          LEFT OUTER JOIN K_CM_PREVIAPRODUTOFORNECEDORES ON (K_CM_PREVIAPRODUTOFORNECEDORES.HANDLE = CP_ORDENSCOMPRAITENS.K_PREVIAPRODUTOFORNECEDOR)
                                                                          LEFT OUTER JOIN K_CM_PREVIAPRODUTOS ON (K_CM_PREVIAPRODUTOS.HANDLE = K_CM_PREVIAPRODUTOFORNECEDORES.PREVIAPRODUTO)
                                                                          LEFT OUTER JOIN K_CM_PREVIAS ON (K_CM_PREVIAS.HANDLE = K_CM_PREVIAPRODUTOS.PREVIA)
                                                                          LEFT OUTER JOIN GN_PESSOAS ON (GN_PESSOAS.HANDLE = K_CM_PREVIAPRODUTOFORNECEDORES.PESSOA)
                                                                          LEFT OUTER JOIN PD_PRODUTOS ON(PD_PRODUTOS.HANDLE = K_CM_PREVIAPRODUTOS.PRODUTO)
                                                                          LEFT OUTER JOIN PD_PRODUTOS PRODUTOBASE ON (PRODUTOBASE.HANDLE = PD_PRODUTOS.K_PRODUTOBASE)
                                                                          LEFT OUTER JOIN PD_PRODUTOS PRODUTOMATRIZ ON (PRODUTOMATRIZ.HANDLE = PD_PRODUTOS.K_PRODUTOMATRIZ)
                                                                          LEFT OUTER JOIN CP_RECEBIMENTOFISICO ON (CP_ORDENSCOMPRAITENS.HANDLE = CP_RECEBIMENTOFISICO.ORDEMCOMPRAITEM)
                                                                    WHERE DATADOPEDIDO >= :INICIO
                                                                          AND DATADOPEDIDO < = :FIM
                                                                          AND PD_PRODUTOS.NOME NOT LIKE '%MPH%'
                                                                    GROUP BY K_CM_PREVIAS.DATADOPEDIDO,
                                                                          GN_PESSOAS.HANDLE,
                                                                          PD_PRODUTOS.CODIGOREFERENCIA,
                                                                          PD_PRODUTOS.NOME,
                                                                          PRODUTOBASE.NOME,
                                                                          PRODUTOMATRIZ.HANDLE ");
            queryPedidoCooperado.Parameters.Add(new Parameter("INICIO", request.Datainicio));
            queryPedidoCooperado.Parameters.Add(new Parameter("FIM", request.Datafim));
            var PedidoCooperado = queryPedidoCooperado.Execute();
            foreach (var registro in PedidoCooperado)
            {
                RetornoPedidoCooperado.Add(new QPedidoCooperado()
                {
                    DataPedido = Convert.ToDateTime(registro.Fields["DATADOPEDIDO"]),
                    Produtor = Convert.ToString(registro.Fields["PRODUTOR"]),
                    CodigoRef = Convert.ToString(registro.Fields["CODIGOREFERENCIA"]),
                    Produto = Convert.ToString(registro.Fields["PRODUTO"]),
                    ProdutoBase = Convert.ToString(registro.Fields["PRODUTOBASE"]),
                    ProdutoMatriz = Convert.ToString(registro.Fields["PRODUTOMATRIZ"]),
                    Pedido = Convert.ToInt32(registro.Fields["PEDIDO"]),
                    QuantidadeEntregue = Convert.ToInt32(registro.Fields["QTDEENTREGUE"]),

                });

                
                Query VerificarRegistros = new Query($@"SELECT * FROM K_GN_ANALISECOOPERADO A
                                                        WHERE A.FORNECEDOR = :PRODUTOR AND A.ITEM = :PRODUTOMATRIZ AND A.PROCESSO = {request.Processo}");
                VerificarRegistros.Parameters.Add("PRODUTOR", Convert.ToInt32(registro.Fields["PRODUTOR"]));
                VerificarRegistros.Parameters.Add("PRODUTOMATRIZ", Convert.ToInt32(registro.Fields["PRODUTOMATRIZ"]));
                var verificar = VerificarRegistros.Execute();
                if (verificar.Count != 0)
                {
                    int Fornecedor = Convert.ToInt32(registro.Fields["PRODUTOR"]);
                    int ProdutoMatriz = Convert.ToInt32(registro.Fields["PRODUTOMATRIZ"]);
                    double SomaPedidoCooperado = 0;
                    foreach (var Soma in PedidoCooperado)
                    {
                        if (Fornecedor == Convert.ToInt32(Soma.Fields["PRODUTOR"]) && ProdutoMatriz == Convert.ToInt32(Soma.Fields["PRODUTOMATRIZ"]))
                        {
                            SomaPedidoCooperado += Convert.ToInt32(Soma.Fields["PEDIDO"]);
                        }
                    }
                    EntityBase AnaliseCooperado = Entity.Get(EntityDefinition.GetByName("K_GN_ANALISECOOPERADO"), new Criteria($"A.FORNECEDOR = :PRODUTOR AND A.ITEM = :PRODUTOMATRIZ AND A.PROCESSO = {request.Processo}", new Parameter("PRODUTOR", Convert.ToInt32(registro.Fields["PRODUTOR"])), new Parameter("PRODUTOMATRIZ", Convert.ToInt32(registro.Fields["PRODUTOMATRIZ"]))), GetMode.Edit);
                    AnaliseCooperado.Fields["ORDEMDECOMPRA"] = SomaPedidoCooperado;
                    AnaliseCooperado.Save();
                }
                
            }

            Query queryProgramacao = new Query($@"SELECT C.FORNECEDOR,
                                                                C.COTA,
                                                                C.QUANTIDADECOTA,
                                                                C.QUANTIDADEPROGRAMADA,
                                                                D.PRODUTO
                                                                FROM K_CM_PROGRAMADOFORNECEDORES C
                                                                JOIN K_CM_PROGRAMADOPRODUTOS D ON C.PROGRAMADOPRODUTO = D.HANDLE
                                                                JOIN K_CM_PROGRAMADOS P ON D.PROGRAMADOS = P.HANDLE
                                                                WHERE P.DATAINICIO <= :FIM AND P.DATAFIM >= :INICIO");
            queryProgramacao.Parameters.Add(new Parameter("INICIO", request.Datainicio));
            queryProgramacao.Parameters.Add(new Parameter("FIM", request.Datafim));
            var Programacao = queryProgramacao.Execute();
            foreach (var registro in Programacao)
            {
                
                int ProdutoMatriz = Convert.ToInt32(registro.Fields["PRODUTO"]);
                int Fornecedor = Convert.ToInt32(registro.Fields["FORNECEDOR"]);
                Query queryListarCooperados = new Query($@"SELECT * FROM K_GN_ANALISECOOPERADO WHERE PROCESSO ={request.Processo}");
                var ListarCooperados = queryListarCooperados.Execute();
                foreach (var registro1 in ListarCooperados)
                {
                    
                    if (ProdutoMatriz == Convert.ToInt32(registro1.Fields["ITEM"]) && Fornecedor == Convert.ToInt32(registro1.Fields["FORNECEDOR"]))
                    {
                        EntityBase AnaliseCooperado = Entity.Get(EntityDefinition.GetByName("K_GN_ANALISECOOPERADO"), new Criteria($"A.FORNECEDOR = :PRODUTOR AND A.ITEM = :PRODUTOMATRIZ AND A.PROCESSO = {request.Processo}", new Parameter("PRODUTOR", Convert.ToInt32(registro1.Fields["FORNECEDOR"])), new Parameter("PRODUTOMATRIZ", Convert.ToInt32(registro1.Fields["ITEM"]))), GetMode.Edit);
                        AnaliseCooperado.Fields["PROGRAMADO"] = Convert.ToInt32(registro.Fields["QUANTIDADEPROGRAMADA"]);
                        AnaliseCooperado.Save();
                        break;
                    }
                        
                    
                }
            }

            Query queryAnaliseCooperado = new Query($@"SELECT * FROM K_GN_ANALISECOOPERADO
                                                        WHERE PROCESSO ={request.Processo}");
            var RegistrosAnaliseCooperado = queryAnaliseCooperado.Execute();
            foreach(var registro in RegistrosAnaliseCooperado)
            {
                int Produtomatriz = Convert.ToInt32(registro.Fields["ITEM"]);
                double pedidomercado = Convert.ToDouble(registro.Fields["PEDIDOMERCADO"]);
                double atendimentoNFE = Convert.ToDouble(registro.Fields["ATENDIMENTONFE"]);
                double programado = Convert.ToDouble(registro.Fields["PROGRAMADO"]);
                double cotaatual = Convert.ToDouble(registro.Fields["COTAATUAL"]);
                double nfexprog = 0;
                double porcAtendidoMercado = 0;
                double cotaprojetada = 0;
                if(programado == 0)
                {
                    programado = 1;//TESTE ENQUANTO NAO TEM PROGRAMADO
                }
                

                if (pedidomercado != 0)
                {
                    porcAtendidoMercado = (atendimentoNFE - pedidomercado) / pedidomercado * 100;
                }

                

                if(programado ==0 && atendimentoNFE!=0)
                {
                    double Soma = 0;
                    foreach(var registro1 in NFe)
                    {
                        if(Produtomatriz == Convert.ToInt32(registro1.Fields["ITEM MATRIZ"]))
                        {
                            Soma += Convert.ToInt32(registro1.Fields["QUANTIDADE"]);
                        }
                    }
                    nfexprog = atendimentoNFE / Soma;
                }
                else
                {
                    if (programado == 1)
                    {
                        nfexprog = 0;//TESTE ENQUANTO NAO TEM PROGRAMADO
                    }
                    else
                    {
                        nfexprog = (atendimentoNFE - programado) / programado * 100;
                    }
                }
                EntityBase AnaliseCooperado = Entity.Get(EntityDefinition.GetByName("K_GN_ANALISECOOPERADO"), new Criteria($"A.FORNECEDOR = :PRODUTOR AND A.ITEM = :PRODUTOMATRIZ AND A.PROCESSO = {request.Processo}", new Parameter("PRODUTOR", Convert.ToInt32(registro.Fields["FORNECEDOR"])), new Parameter("PRODUTOMATRIZ", Convert.ToInt32(registro.Fields["ITEM"]))), GetMode.Edit);
                AnaliseCooperado.Fields["NFEXPROGRAMADO"] = nfexprog;
                AnaliseCooperado.Fields["DIFERENCAATENDIDONFEMERCADO"] = atendimentoNFE - pedidomercado;
                AnaliseCooperado.Fields["PORCENTAGEMATENDIDOMERCADO"] = porcAtendidoMercado;
                AnaliseCooperado.Fields["DIFERENCAPROGRAMADOATENDIDONFE"] = atendimentoNFE - programado;
                AnaliseCooperado.Fields["PORCENTAGEMPROGRAMADOATENDIDO"] = (atendimentoNFE - programado)/ programado * 100;
                if (cotaatual == 0 && nfexprog > 0)
                {
                    cotaprojetada = nfexprog * 100;
                }
                else
                {
                    if (atendimentoNFE < programado && pedidomercado < programado)
                    {
                        cotaprojetada = cotaatual + (cotaatual * (porcAtendidoMercado/100));
                    }
                    else
                    {
                        cotaprojetada = cotaatual + (cotaatual * (nfexprog/100));
                    }
                }
                AnaliseCooperado.Fields["COTAPROJETADA"] = cotaprojetada;
                AnaliseCooperado.Save();
            }

            
            return "Processo Executado";
        }

        public string CarregarFornecedores(RequestCarregarFornecerdor request)
        {

            EntityBase ProgramadoProdutos = Entity.Get(EntityDefinition.GetByName("K_CM_PROGRAMADOPRODUTOS"), new Criteria($"A.HANDLE = :HANDLE", new Parameter("HANDLE", request.Handle)),GetMode.Edit);
            if (((ListItem)ProgramadoProdutos.Fields["STATUS"]).Value >= 2)
            {
                ProgramadoProdutos.Save();
                return "Processamento ja foi inciado anteriormente";
            }
            else
            {
                ProgramadoProdutos.Fields["STATUS"] = new ListItem(2, "");
                ProgramadoProdutos.Save();
                Query query = new Query(@"SELECT GN_PESSOAS.HANDLE 'PRODUTOR', -- troquei apelido pelo HANDLE
                                                                   PRODUTOBASE.NOME 'PRODUTOBASE',
                                                                   PRODUTOMATRIZ.HANDLE 'PRODUTOMATRIZ', -- troquei NOME pelo HANDLE
                                                                   MAX(PERCENTUALPREFERENCIA) 'COTA'
                                                              FROM K_PD_PRODUTOCOTAPREFERENCIAS
                                                                   LEFT OUTER JOIN K_PD_PRODUTOCOTAS ON (K_PD_PRODUTOCOTAS.HANDLE = K_PD_PRODUTOCOTAPREFERENCIAS.PRODUTOCOTA)
                                                                   LEFT OUTER JOIN K_PD_COTAPERIODO ON (K_PD_COTAPERIODO.HANDLE = K_PD_PRODUTOCOTAS.COTAPERIODO)
                                                                   LEFT OUTER JOIN PD_PRODUTOS ON (PD_PRODUTOS.HANDLE = K_PD_PRODUTOCOTAPREFERENCIAS.PRODUTODERIVADO)
                                                                   LEFT OUTER JOIN PD_PRODUTOS PRODUTOBASE ON (PRODUTOBASE.HANDLE = PD_PRODUTOS.K_PRODUTOBASE)
                                                                   LEFT OUTER JOIN GN_PESSOAS ON (GN_PESSOAS.HANDLE = K_PD_PRODUTOCOTAS.PESSOA)
                                                                   LEFT OUTER JOIN PD_PRODUTOS PRODUTOMATRIZ ON (PRODUTOMATRIZ.HANDLE = PD_PRODUTOS.K_PRODUTOMATRIZ)
                                                             WHERE
                                                                   (
                                                                       (
                                                                           :INICIO BETWEEN K_PD_COTAPERIODO.DATAINICIO AND K_PD_COTAPERIODO.DATAFIM
                                                                       )
                                                                       OR
                                                                       (
                                                                           :FIM BETWEEN K_PD_COTAPERIODO.DATAINICIO AND K_PD_COTAPERIODO.DATAFIM
                                                                       )
                                                                   )
                                                                   AND PRODUTOMATRIZ.HANDLE = :PRODUTOMATRIZ
                                                             GROUP BY GN_PESSOAS.HANDLE, -- troquei apelido pelo HANDLE
                                                                   PRODUTOBASE.NOME,
                                                                   PRODUTOMATRIZ.HANDLE -- troquei NOME pelo HANDLE ");
                query.Parameters.Add(new Parameter("INICIO", request.DataInicio));
                query.Parameters.Add(new Parameter("FIM", request.DataFim));
                query.Parameters.Add(new Parameter("PRODUTOMATRIZ", request.Produto));
                var registros = query.Execute();
                foreach (var registro in registros)
                {
                    EntityBase ProgramadoFornecedores = Entity.Create(EntityDefinition.GetByName("K_CM_PROGRAMADOFORNECEDORES"));
                    ProgramadoFornecedores.Fields["PROGRAMADOPRODUTO"] = new EntityAssociation(request.Handle, EntityDefinition.GetByName("K_CM_PROGRAMADOPRODUTOS"));
                    ProgramadoFornecedores.Fields["FORNECEDOR"] = new EntityAssociation(Convert.ToInt32(registro.Fields["PRODUTOR"]), EntityDefinition.GetByName("GN_PESSOAS"));
                    ProgramadoFornecedores.Fields["COTA"] = Convert.ToDouble(registro.Fields["COTA"]);
                    //ProgramadoFornecedores.Fields["NOME"] = "teste";
                    //ProgramadoFornecedores.Fields["DATAINICIO"] = DateTime.Now;
                    //ProgramadoFornecedores.Fields["DATAFIM"] = DateTime.Now;
                    ProgramadoFornecedores.Fields["QUANTIDADECOTA"] = (Convert.ToDouble(registro.Fields["COTA"]) / 100) * request.Quantidade;
                    ProgramadoFornecedores.Fields["USUARIOINCLUIU"] = new EntityAssociation(Convert.ToInt32(BennerContext.Security.GetLoggedUserHandle()), EntityDefinition.GetByName("GN_PESSOAS"));
                    ProgramadoFornecedores.Save();
                }
                return "Processo Concluído";
            }
        }
        public string CarregarFonecedoresSelecionados(BusinessArgs args)
        {

            
            string selecionados = string.Join(",", (args.SelectedEntitiesHandles.ToArray()).Select(h => h.ToString()));
            if (selecionados == "")
            {
                throw new BusinessException("Selecione os Registros");
            }
            else
            {
                RequestCarregarFornecerdor request = new RequestCarregarFornecerdor();
                Query query = new Query($"SELECT * FROM K_CM_PROGRAMADOPRODUTOS WHERE HANDLE IN ({selecionados})");
                var registros = query.Execute();
                foreach (var registro in registros)
                {
                    EntityBase ProgramadoProdutos = Entity.Get(EntityDefinition.GetByName("K_CM_PROGRAMADOPRODUTOS"), new Criteria($"A.HANDLE = :HANDLE", new Parameter("HANDLE", Convert.ToInt32(registro.Fields["HANDLE"]))));
                    request.Handle = Convert.ToInt32(ProgramadoProdutos.Fields["HANDLE"]);
                    request.Produto = Convert.ToInt32(((EntityAssociation)ProgramadoProdutos.Fields["PRODUTO"]).Handle);
                    request.Quantidade = Convert.ToInt32(ProgramadoProdutos.Fields["QUANTIDADE"]);
                    request.DataInicio = Convert.ToString((DateTime)((EntityAssociation)ProgramadoProdutos.Fields["PROGRAMADOS"]).Instance.Fields["DATAINICIO"]);
                    request.DataFim = Convert.ToString((DateTime)((EntityAssociation)ProgramadoProdutos.Fields["PROGRAMADOS"]).Instance.Fields["DATAFIM"]);
                    string Msg = CarregarFornecedores(request);
                }
            }
            return "Processo Concluido";
        }
    }
}
