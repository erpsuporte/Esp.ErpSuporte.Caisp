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
    public class ProcessarAnaliseFatTask : BusinessComponent<ProcessarAnaliseFatTask>, IProcessarAnaliseFat
    {
        public void Run(ProcessarAnaliseModel request)
        {

            Query qFaturamento = new Query(@"SELECT FN_DOCUMENTOS.DATAEMISSAO,
                                                   FN_DOCUMENTOS.NRNOTAFISCAL,
                                                   GN_PESSOAS.APELIDO,
                                                   GRUPOEMPRESARIAL.HANDLE 'GRUPOEMPRESARIAL',
                                                   PD_PRODUTOS.HANDLE 'CODIGOREFERENCIA',
                                                   PD_PRODUTOS.NOME,
                                                   PD_GRUPOSPRODUTOS.HANDLE AS 'GRUPO',
                                                   QUANTIDADE,
                                                   VALORUNITARIO,
                                                   CM_ORDENSVENDA.K_NUMEROPEDIDO  AS 'NUMEROPEDIDO',
                                                   VALORTOTAL,
                                                   J.HANDLE AS 'UN_NEGOCIO',
                                                   GN_PESSOAS.PERCENTUALDESCONTO,
                                                   VALORUNITARIO - (VALORUNITARIO * (GN_PESSOAS.PERCENTUALDESCONTO/100)) 'VALOR UN DESCONTO',
                                                   ICMS,
                                                   CM_ITENS.REDUCAOICMS,
                                                   ICMS - ((ICMS * CM_ITENS.REDUCAOICMS)/100) 'CALCULO_ICMS',
                                                   PD_PRODUTOS.ALIQUOTAPISSAIDAS 'PIS',
                                                   PD_PRODUTOS.ALIQUOTACOFINSSAIDA 'COFINS'
                                              FROM CM_ITENS
                                                   LEFT OUTER JOIN FN_DOCUMENTOS ON (FN_DOCUMENTOS.HANDLE = CM_ITENS.DOCUMENTO)
                                                   LEFT OUTER JOIN GN_PESSOAS ON (GN_PESSOAS.HANDLE = FN_DOCUMENTOS.PESSOA)
                                                   LEFT OUTER JOIN GN_PESSOAS GRUPOEMPRESARIAL ON (GRUPOEMPRESARIAL.HANDLE = GN_PESSOAS.GRUPOEMPRESARIAL)
                                                   LEFT OUTER JOIN PD_PRODUTOS ON (PD_PRODUTOS.HANDLE = CM_ITENS.PRODUTO)
                                                   LEFT OUTER JOIN CT_CC ON (CT_CC.HANDLE = CM_ITENS.CENTROCUSTO)
                                                   LEFT OUTER JOIN PD_GRUPOSPRODUTOS ON (PD_GRUPOSPRODUTOS.HANDLE = PD_PRODUTOS.GRUPO)
                                                   LEFT OUTER JOIN K_PD_GRUPOSGRUPODEPRODUTO AS J ON(J.HANDLE = PD_GRUPOSPRODUTOS.K_GRUPO)
                                                   LEFT OUTER JOIN CM_ORDENSVENDA ON (CM_ORDENSVENDA.HANDLE = FN_DOCUMENTOS.ORDEMVENDA)
                                             WHERE FN_DOCUMENTOS.OPERACAOFATURAMENTO IN (24, 64, 124, 139)
                                                   AND FN_DOCUMENTOS.DATACONFIRMACAO BETWEEN CONVERT(DATETIME, :DATAINICO, 103) AND CONVERT(DATETIME, :DATAFIM,
                                                   103)
                                                   AND STATUSNFE IN (1, 6)
                                                   AND FN_DOCUMENTOS.STATUS = 2
                                             ORDER BY FN_DOCUMENTOS.DATACONFIRMACAO ASC
                                            ");
            qFaturamento.Parameters.Add("DATAINICO", request.Datainicio); //"01/04/2021 05:00:00"
            qFaturamento.Parameters.Add("DATAFIM", request.Datafim); //"01/05/2021 05:00:00"
            var Faturamentos = qFaturamento.Execute();
            List<AnaliseFaturamentoModel> DadosAnalise = new List<AnaliseFaturamentoModel>();
            foreach (var Faturamento in Faturamentos)
            {
                DadosAnalise.Add(new AnaliseFaturamentoModel()
                {
                    DataEmissao = Convert.ToDateTime(Faturamento.Fields["DATAEMISSAO"]),
                    GrupoEmpresarial = Convert.ToInt32(Faturamento.Fields["GRUPOEMPRESARIAL"]),
                    CodigoReferencia = Convert.ToInt32(Faturamento.Fields["CODIGOREFERENCIA"]),
                    DescricaoItem = Convert.ToString(Faturamento.Fields["NOME"]),
                    GrupoProdutos = Convert.ToInt32(Faturamento.Fields["GRUPO"]),
                    UnidadeNegocio = Convert.ToInt32(Faturamento.Fields["UN_NEGOCIO"]),
                    QuantidadeFaturamento = Convert.ToInt32(Faturamento.Fields["QUANTIDADE"]),
                    FaturamentoBruto = Convert.ToDouble(Faturamento.Fields["VALORTOTAL"]),
                    DF = Convert.ToDouble(Faturamento.Fields["PERCENTUALDESCONTO"]),
                    ICMS = Convert.ToDouble(Faturamento.Fields["ICMS"]),
                    ReducaoICMS = Convert.ToDouble(Faturamento.Fields["REDUCAOICMS"]),
                    PorcentagemICMS = Convert.ToDouble(Faturamento.Fields["CALCULO_ICMS"]),
                    PIS = Convert.ToDouble(Faturamento.Fields["PIS"]),
                    COFINS = Convert.ToDouble(Faturamento.Fields["COFINS"]),
                    NumeroPedido = Convert.ToString(Faturamento.Fields["NUMEROPEDIDO"]),

                });
            }



            Query qBonificacao = new Query(@"SELECT FN_DOCUMENTOS.DATAEMISSAO,
                                           GN_PESSOAS.APELIDO AS 'APELIDO LOJA',
                                           GP2.HANDLE 'GRUPOEMPRESARIAL',
                                           PD_PRODUTOS.HANDLE AS 'CODIGOREFERENCIA',
                                           PD_PRODUTOS.NOME 'NOME',
                                           QUANTIDADE,
                                           VALORUNITARIO,
                                           VALORTOTAL,
                                           J.HANDLE AS 'UN_NEGOCIO',
                                           CM_ORDENSVENDA.K_NUMEROPEDIDO AS 'NUMEROPEDIDO',
                                           PD_GRUPOSPRODUTOS.HANDLE AS 'GRUPO'
                                      FROM CM_ITENS
                                           LEFT OUTER JOIN FN_DOCUMENTOS ON (FN_DOCUMENTOS.HANDLE = CM_ITENS.DOCUMENTO)
                                           LEFT OUTER JOIN GN_PESSOAS ON (GN_PESSOAS.HANDLE = FN_DOCUMENTOS.PESSOA)
                                           LEFT OUTER JOIN PD_PRODUTOS ON (PD_PRODUTOS.HANDLE = CM_ITENS.PRODUTO)
                                           LEFT OUTER JOIN CM_UNIDADESMEDIDA ON (CM_UNIDADESMEDIDA.HANDLE = CM_ITENS.UNIDADE)
                                           LEFT OUTER JOIN CT_CC ON (CT_CC.HANDLE = CM_ITENS.CENTROCUSTO)
                                           LEFT OUTER JOIN GN_PESSOAS AS GP2 ON (GP2.HANDLE = GN_PESSOAS.GRUPOEMPRESARIAL)
                                           LEFT OUTER JOIN PD_GRUPOSPRODUTOS ON (PD_GRUPOSPRODUTOS.HANDLE = PD_PRODUTOS.GRUPO)
                                           LEFT OUTER JOIN K_PD_GRUPOSGRUPODEPRODUTO AS J ON(J.HANDLE = PD_GRUPOSPRODUTOS.K_GRUPO)
                                           LEFT OUTER JOIN CM_OPERACOESFATURAMENTO AS H ON (H.HANDLE = FN_DOCUMENTOS.OPERACAOFATURAMENTO)
                                           LEFT OUTER JOIN CM_ORDENSVENDA ON (CM_ORDENSVENDA.HANDLE = FN_DOCUMENTOS.ORDEMVENDA)
                                     WHERE FN_DOCUMENTOS.OPERACAOFATURAMENTO IN (28, 60, 69, 13)
                                           AND FN_DOCUMENTOS.DATACONFIRMACAO BETWEEN CONVERT(DATETIME, :DATAINICO, 103) AND CONVERT(DATETIME, :DATAFIM,
                                           103)
                                           AND FN_DOCUMENTOS.STATUSNFE IN (6)
                                           AND PD_PRODUTOS.GRUPO IS NOT NULL
                                     GROUP BY CM_UNIDADESMEDIDA.ABREVIATURA,
                                           K_QUANTIDADEPORCAIXA,
                                           FN_DOCUMENTOS.DATAEMISSAO,
                                           GN_PESSOAS.APELIDO,
                                           GP2.NOME,
                                           PD_PRODUTOS.HANDLE,
                                           PD_PRODUTOS.NOME,
                                           QUANTIDADE,
                                           VALORUNITARIO,
                                           VALORTOTAL,
                                           FN_DOCUMENTOS.DOCUMENTODIGITADO,
                                           FN_DOCUMENTOS.VALORNOMINAL,
                                           J.HANDLE,
                                           PD_GRUPOSPRODUTOS.HANDLE,
                                           GP2.HANDLE,
                                           H.NOME,
                                           CM_ORDENSVENDA.K_NUMEROPEDIDO
                                     ORDER BY FN_DOCUMENTOS.DATAEMISSAO,
                                           FN_DOCUMENTOS.DOCUMENTODIGITADO ");
            qBonificacao.Parameters.Add("DATAINICO", request.Datainicio);
            qBonificacao.Parameters.Add("DATAFIM", request.Datafim);
            var Bonificacoes = qBonificacao.Execute();
            foreach (var Bonificacao in Bonificacoes)
            {
                DadosAnalise.Add(new AnaliseFaturamentoModel()
                {
                    DataEmissao = Convert.ToDateTime(Bonificacao.Fields["DATAEMISSAO"]),
                    GrupoEmpresarial = Convert.ToInt32(Bonificacao.Fields["GRUPOEMPRESARIAL"]),
                    CodigoReferencia = Convert.ToInt32(Bonificacao.Fields["CODIGOREFERENCIA"]),
                    DescricaoItem = Convert.ToString(Bonificacao.Fields["NOME"]),
                    GrupoProdutos = Convert.ToInt32(Bonificacao.Fields["GRUPO"]),
                    UnidadeNegocio = Convert.ToInt32(Bonificacao.Fields["UN_NEGOCIO"]),
                    Bonificacao = Convert.ToDouble(Bonificacao.Fields["VALORTOTAL"]),
                    NumeroPedido = Convert.ToString(Bonificacao.Fields["NUMEROPEDIDO"]),


                });
            }

            Query qDevolucao = new Query(@"SELECT FN_DOCUMENTOS.DATAEMISSAO,
                                               FN_DOCUMENTOS.DOCUMENTODIGITADO,
                                               GN_PESSOAS.APELIDO,
                                               GRUPOEMPRESARIAL.HANDLE 'GRUPOEMPRESARIAL',
                                               PD_PRODUTOS.HANDLE 'CODIGOREFERENCIA',
                                               PD_PRODUTOS.NOME,
                                               PD_GRUPOSPRODUTOS.HANDLE AS 'GRUPO',
                                               CM_UNIDADESMEDIDA.ABREVIATURA,
                                               QUANTIDADE,
                                               VALORUNITARIO,
                                               CM_ORDENSVENDA.K_NUMEROPEDIDO AS 'NUMEROPEDIDO',
                                               VALORTOTAL,
                                               J.HANDLE AS 'UN_NEGOCIO',
                                               K_MOTIVODEVOLUCAO.NOME
                                          FROM CM_ITENS
                                               LEFT OUTER JOIN FN_DOCUMENTOS ON (FN_DOCUMENTOS.HANDLE = CM_ITENS.DOCUMENTO)
                                               LEFT OUTER JOIN GN_PESSOAS ON (GN_PESSOAS.HANDLE = FN_DOCUMENTOS.PESSOA)
                                               LEFT OUTER JOIN GN_PESSOAS GRUPOEMPRESARIAL ON (GRUPOEMPRESARIAL.HANDLE = GN_PESSOAS.GRUPOEMPRESARIAL)
                                               LEFT OUTER JOIN PD_PRODUTOS ON (PD_PRODUTOS.HANDLE = CM_ITENS.PRODUTO)
                                               LEFT OUTER JOIN CT_CC ON (CT_CC.HANDLE = CM_ITENS.CENTROCUSTO)
                                               LEFT OUTER JOIN CM_UNIDADESMEDIDA ON (CM_UNIDADESMEDIDA.HANDLE = CM_ITENS.UNIDADE)
                                               LEFT OUTER JOIN K_MOTIVODEVOLUCAO ON (K_MOTIVODEVOLUCAO.HANDLE = FN_DOCUMENTOS.K_KMOTIVODEVOLUCAO)
                                               LEFT OUTER JOIN Z_GRUPOUSUARIOS ON (Z_GRUPOUSUARIOS.HANDLE = FN_DOCUMENTOS.USUARIOINCLUIU)
                                               LEFT OUTER JOIN PD_GRUPOSPRODUTOS H ON (H.HANDLE = PD_PRODUTOS.GRUPO)
                                               LEFT OUTER JOIN K_PD_GRUPOSGRUPODEPRODUTO AS J ON(J.HANDLE = H.K_GRUPO)
                                               LEFT OUTER JOIN PD_GRUPOSPRODUTOS ON (PD_GRUPOSPRODUTOS.HANDLE = PD_PRODUTOS.GRUPO)
                                               LEFT OUTER JOIN CM_ORDENSVENDA ON (CM_ORDENSVENDA.HANDLE = FN_DOCUMENTOS.ORDEMVENDA)
                                         WHERE TIPOMOVIMENTACAO IN (35, 63, 151, 152)
                                               AND FN_DOCUMENTOS.STATUS = 2
                                               AND FN_DOCUMENTOS.DATAENTRADA BETWEEN CONVERT(DATE, :DATAINICO, 103) AND CONVERT(DATE, :DATAFIM, 103) 
                                        ");
            DateTime date = DateTime.ParseExact(request.Datafim, "dd/MM/yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);//new DateTime(2021, 5, 01); // Exemplo de data
            qDevolucao.Parameters.Add("DATAINICO", (request.Datainicio).Substring(0, 10));
            qDevolucao.Parameters.Add("DATAFIM", date.Day == 1 ? date.AddDays(-1) : date);
            var Devolucoes = qDevolucao.Execute();
            foreach (var Devolucao in Devolucoes)
            {
                DadosAnalise.Add(new AnaliseFaturamentoModel()
                {
                    DataEmissao = Convert.ToDateTime(Devolucao.Fields["DATAEMISSAO"]),
                    GrupoEmpresarial = Convert.ToInt32(Devolucao.Fields["GRUPOEMPRESARIAL"]),
                    CodigoReferencia = Convert.ToInt32(Devolucao.Fields["CODIGOREFERENCIA"]),
                    DescricaoItem = Convert.ToString(Devolucao.Fields["NOME"]),
                    GrupoProdutos = Convert.ToInt32(Devolucao.Fields["GRUPO"]),
                    UnidadeNegocio = Convert.ToInt32(Devolucao.Fields["UN_NEGOCIO"]),
                    Devolucao = Convert.ToDouble(Devolucao.Fields["VALORTOTAL"]),
                    NumeroPedido = Convert.ToString(Devolucao.Fields["NUMEROPEDIDO"]),

                });
            }



            var query = from d in DadosAnalise
                        group d by new { d.GrupoEmpresarial, d.CodigoReferencia, d.NumeroPedido } into g
                        select new
                        {
                            Dados = g.Key,
                            DescricaoItem = g.First().DescricaoItem,
                            GrupoProdutos = g.First().GrupoProdutos,
                            UnidadeNegocio = g.First().UnidadeNegocio,
                            QuantidadeFaturamento = g.Sum(d => d.QuantidadeFaturamento),
                            FaturamentoBruto = g.Sum(d => d.FaturamentoBruto),
                            Bonificacao = g.Sum(d => d.Bonificacao),
                            Devolucao = g.Sum(d => d.Devolucao),
                            DF = g.Where(d => d.DF > 0).Select(d => d.DF).DefaultIfEmpty(0).Average(),
                            ICMS = g.Where(d => d.ICMS > 0).Select(d => d.ICMS).DefaultIfEmpty(0).Average(),
                            ReducaoICMS = g.Where(d => d.ReducaoICMS > 0).Select(d => d.ReducaoICMS).DefaultIfEmpty(0).Average(),
                            PorcentagemICMS = g.Where(d => d.PorcentagemICMS > 0).Select(d => d.PorcentagemICMS).DefaultIfEmpty(0).Average(),
                            PIS = g.Where(d => d.PIS > 0).Select(d => d.PIS).DefaultIfEmpty(0).Average(),
                            COFINS = g.Where(d => d.COFINS > 0).Select(d => d.COFINS).DefaultIfEmpty(0).Average(),
                        };
            var teste = from d in DadosAnalise
                        group d by new { d.GrupoEmpresarial, d.CodigoReferencia } into g
                        select new
                        {
                            Dados = g.Key,
                            DescricaoItem = g.First().DescricaoItem,
                            GrupoProdutos = g.First().GrupoProdutos,
                            UnidadeNegocio = g.First().UnidadeNegocio,
                            QuantidadeFaturamento = g.Sum(d => d.QuantidadeFaturamento),
                            FaturamentoBruto = g.Sum(d => d.FaturamentoBruto),
                            Bonificacao = g.Sum(d => d.Bonificacao),
                            Devolucao = g.Sum(d => d.Devolucao),
                            DF = g.Where(d => d.DF > 0).Select(d => d.DF).DefaultIfEmpty(0).Average(),
                            ICMS = g.Where(d => d.ICMS > 0).Select(d => d.ICMS).DefaultIfEmpty(0).Average(),
                            ReducaoICMS = g.Where(d => d.ReducaoICMS > 0).Select(d => d.ReducaoICMS).DefaultIfEmpty(0).Average(),
                            PorcentagemICMS = g.Where(d => d.PorcentagemICMS > 0).Select(d => d.PorcentagemICMS).DefaultIfEmpty(0).Average(),
                            PIS = g.Where(d => d.PIS > 0).Select(d => d.PIS).DefaultIfEmpty(0).Average(),
                            COFINS = g.Where(d => d.COFINS > 0).Select(d => d.COFINS).DefaultIfEmpty(0).Average(),
                        };
            List<AnaliseFaturamentoModel> agrupado = new List<AnaliseFaturamentoModel>();
            foreach (var x in query)
            {
                agrupado.Add(new AnaliseFaturamentoModel
                {
                    GrupoEmpresarial = x.Dados.GrupoEmpresarial,
                    CodigoReferencia = x.Dados.CodigoReferencia,
                    DescricaoItem = x.DescricaoItem,
                    GrupoProdutos = x.GrupoProdutos,
                    UnidadeNegocio = x.UnidadeNegocio,
                    QuantidadeFaturamento = x.QuantidadeFaturamento,
                    FaturamentoBruto = x.FaturamentoBruto,
                    DF = x.DF,
                    ICMS = x.ICMS,
                    ReducaoICMS = x.ReducaoICMS,
                    PorcentagemICMS = x.PorcentagemICMS,
                    PIS = x.PIS,
                    COFINS = x.COFINS,
                    NumeroPedido = x.Dados.NumeroPedido,
                    FaturamentoLiquido = x.FaturamentoBruto - (x.FaturamentoBruto * (x.DF * 100)) * (1 - x.ICMS / 100) * (1 - (x.PIS + x.COFINS) / 100),
                    Bonificacao = x.Bonificacao,
                    Devolucao = x.Devolucao,
                });
                EntityBase AnaliseFaturamento = Entity.Create(EntityDefinition.GetByName("K_CM_ANALISEFATURAMENTO"));
                AnaliseFaturamento.Fields["GRUPOEMPRESARIAL"] = new EntityAssociation(x.Dados.GrupoEmpresarial, EntityDefinition.GetByName("GN_PESSOAS"));
                AnaliseFaturamento.Fields["CODIGOREFERENCIA"] = new EntityAssociation(x.Dados.CodigoReferencia, EntityDefinition.GetByName("PD_PRODUTOS"));
                AnaliseFaturamento.Fields["DESCRICAOITEM"] = x.DescricaoItem;
                AnaliseFaturamento.Fields["GRUPOPRODUTOS"] = new EntityAssociation(x.GrupoProdutos, EntityDefinition.GetByName("PD_GRUPOSPRODUTOS"));
                AnaliseFaturamento.Fields["UNIDADENEGOCIO"] = new EntityAssociation(x.UnidadeNegocio, EntityDefinition.GetByName("K_PD_GRUPOSGRUPODEPRODUTO"));
                AnaliseFaturamento.Fields["QUANTIDADEFATURAMENTO"] = x.QuantidadeFaturamento;
                AnaliseFaturamento.Fields["FATURAMENTOBRUTO"] = x.FaturamentoBruto;
                AnaliseFaturamento.Fields["PEDIDO"] = x.Dados.NumeroPedido;
                AnaliseFaturamento.Fields["DF"] = x.DF;
                AnaliseFaturamento.Fields["ICMS"] = x.ICMS;
                AnaliseFaturamento.Fields["REDUCAOICMS"] = x.ReducaoICMS;
                AnaliseFaturamento.Fields["PORCENTAGEMICMS"] = x.PorcentagemICMS;
                AnaliseFaturamento.Fields["PIS"] = x.PIS;
                AnaliseFaturamento.Fields["COFINS"] = x.COFINS;
                AnaliseFaturamento.Fields["FATURAMENTOLIQUIDO"] = x.FaturamentoBruto - (((x.FaturamentoBruto * x.DF) / 100) * (1 - x.ICMS / 100) * (1 - (x.PIS + x.COFINS) / 100));
                AnaliseFaturamento.Fields["BONIFICACAO"] = x.Bonificacao;
                AnaliseFaturamento.Fields["DEVOLUCAO"] = x.Devolucao;
                AnaliseFaturamento.Fields["PROCESSO"] = new EntityAssociation(request.Processo, EntityDefinition.GetByName("K_CM_PROCESSARANALISEFATURAMEN"));
                AnaliseFaturamento.Save();


            }

        }
    }
    
}
