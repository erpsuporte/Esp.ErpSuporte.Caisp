using Benner.Tecnologia.Business;
using Benner.Tecnologia.Business.Tasks;
using Benner.Tecnologia.Common.EnterpriseServiceLibrary;
using Benner.Tecnologia.Common;
using Benner.Tecnologia.Common.Tasks;
using Esp.ErpSuporte.Caisp.Business.Interfaces.Caisp;
using Esp.ErpSuporte.Caisp.Business.Modelos.Caisp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Esp.Erpsuporte.Caisp.Business.Interfaces.Caisp;
using Esp.ErpSuporte.Caisp.Business.Entidades;

namespace Esp.ErpSuporte.Caisp.Components.Caisp
{
    public class ProcessarAnaliseTask : BusinessComponent<ProcessarAnaliseTask>, IProcessarAnalise
    {
        public void Run(ProcessarAnaliseModel request)
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
                catch (Exception ex)
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
            foreach (var registro in RegistrosAnaliseCooperado)
            {
                int Produtomatriz = Convert.ToInt32(registro.Fields["ITEM"]);
                double pedidomercado = Convert.ToDouble(registro.Fields["PEDIDOMERCADO"]);
                double atendimentoNFE = Convert.ToDouble(registro.Fields["ATENDIMENTONFE"]);
                double programado = Convert.ToDouble(registro.Fields["PROGRAMADO"]);
                double cotaatual = Convert.ToDouble(registro.Fields["COTAATUAL"]);
                double nfexprog = 0;
                double porcAtendidoMercado = 0;
                double cotaprojetada = 0;


                if (pedidomercado != 0)
                {
                    porcAtendidoMercado = (atendimentoNFE - pedidomercado) / pedidomercado * 100;
                }



                if (programado == 0 && atendimentoNFE != 0)
                {
                    double Soma = 0;
                    foreach (var registro1 in NFe)
                    {
                        if (Produtomatriz == Convert.ToInt32(registro1.Fields["ITEM MATRIZ"]))
                        {
                            Soma += Convert.ToInt32(registro1.Fields["QUANTIDADE"]);
                        }
                    }
                    nfexprog = atendimentoNFE / Soma; //verificar se Soma é diferente de 0 quando programa
                }
                else
                {
                    
                        //nfexprog = (atendimentoNFE - programado) / programado * 100;
                        nfexprog = 0;
                    
                }
                EntityBase AnaliseCooperado = Entity.Get(EntityDefinition.GetByName("K_GN_ANALISECOOPERADO"), new Criteria($"A.FORNECEDOR = :PRODUTOR AND A.ITEM = :PRODUTOMATRIZ AND A.PROCESSO = {request.Processo}", new Parameter("PRODUTOR", Convert.ToInt32(registro.Fields["FORNECEDOR"])), new Parameter("PRODUTOMATRIZ", Convert.ToInt32(registro.Fields["ITEM"]))), GetMode.Edit);
                AnaliseCooperado.Fields["NFEXPROGRAMADO"] = nfexprog;
                AnaliseCooperado.Fields["DIFERENCAATENDIDONFEMERCADO"] = atendimentoNFE - pedidomercado;
                AnaliseCooperado.Fields["PORCENTAGEMATENDIDOMERCADO"] = porcAtendidoMercado;
                AnaliseCooperado.Fields["DIFERENCAPROGRAMADOATENDIDONFE"] = atendimentoNFE - programado;
                if (programado == 0)
                {
                    AnaliseCooperado.Fields["PORCENTAGEMPROGRAMADOATENDIDO"] = 0;
                }
                else
                {
                    AnaliseCooperado.Fields["PORCENTAGEMPROGRAMADOATENDIDO"] = (atendimentoNFE - programado) / programado * 100;
                }
                if (cotaatual == 0 && nfexprog > 0)
                {
                    cotaprojetada = nfexprog * 100;
                }
                else
                {
                    if (atendimentoNFE < programado && pedidomercado < programado)
                    {
                        cotaprojetada = cotaatual + (cotaatual * (porcAtendidoMercado / 100));
                    }
                    else
                    {
                        cotaprojetada = cotaatual + (cotaatual * (nfexprog / 100));
                    }
                }
                AnaliseCooperado.Fields["COTAPROJETADA"] = cotaprojetada;
                AnaliseCooperado.Save();
            }

            EntityBase ProcessarAnalise = Entity.Get(EntityDefinition.GetByName("K_GN_PROCESSARANALISE"), new Criteria($"A.HANDLE = :HANDLE", new Parameter("HANDLE", request.Processo)), GetMode.Edit);
            ProcessarAnalise.Fields["STATUS"] = new ListItem(3, "");
            ProcessarAnalise.Save();

        }
    }
    
}
