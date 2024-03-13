using Benner.Tecnologia.Common.IoC;
using Esp.ErpSuporte.Caisp.Business.Interfaces.Caisp;
using Esp.ErpSuporte.Caisp.Business.Modelos.Caisp;
using Ninject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace Esp.ErpSuporte.Caisp.API.Controller.Api.EspCaisp
{
    /// <summary>
    /// APIs especificas da Caisp
    /// </summary>
    [Authorize]
    [RoutePrefix("api/EspCaisp")]
    public class EspCaispController : ApiController
    {
        // ok deu certo
        ICaisp componente = DependencyContainer.Get<ICaisp>();

        // ok deu certo
        //[Inject]
        //public ICaisp componente { get; set; }

        // ok deu certo
        //ICaisp componente = BusinessComponent.CreateProxyInstance<ICaisp>();


        /// <summary>
        /// Busca os contatos do Administrativo da Caisp
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("buscarContatos")] 
        public IHttpActionResult buscarContatos()
        {
            
            try
            {
                List<ContatosModel> retorno = componente.buscarContatos();
                return Ok(retorno);
            }
            catch (Exception ex)
            {
                return Content(
                    System.Net.HttpStatusCode.InternalServerError,
                    new { ex.Message });
            }
        }
        /// <summary>
        /// Busca os documentos do Administrativo da Caisp
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("buscarDoc")]
        public IHttpActionResult buscarDoc([FromUri] BuscaDocModel request)
        {

            
            try
            {
                List<DocModel> retorno = componente.buscarDoc(request);
                return Ok(retorno);
            }
            catch (Exception ex)
            {
                return Content(
                    System.Net.HttpStatusCode.InternalServerError,
                    new { ex.Message });
            }
        }

        /// <summary>
        /// Busca as entregas do dia
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("buscarEntregasDia")]
        public IHttpActionResult buscarEntregasDia()//([FromUri] EntregasDiaBuscarModel request)
        {

            
            try
            {
                List<EntregasDiaModel> retorno = componente.buscarEntregasDia();
                return Ok(retorno);
            }
            catch (Exception ex)
            {
                return Content(
                    System.Net.HttpStatusCode.InternalServerError,
                    new { ex.Message });
            }
        }
        /// <summary>
        /// Busca as entregas do periodo informado
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("buscarEntregasPeriodo")]
        public IHttpActionResult buscarEntregasPeriodo([FromUri] EntregasPeriodoBuscaModel request)
        {

            
            try
            {
                List<EntregasItensModel> retorno = componente.buscarEntregasPeriodo(request);
                return Ok(retorno);
            }
            catch (Exception ex)
            {
                return Content(
                    System.Net.HttpStatusCode.InternalServerError,
                    new { ex.Message });
            }
        }
        /// <summary>
        /// Busca os proximos enventos da Caisp
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("buscarEventos")]
        public IHttpActionResult buscarEventos()
        {

            
            try
            {
                List<Eventos> retorno = componente.buscarEventos();
                return Ok(retorno);
            }
            catch (Exception ex)
            {
                return Content(
                    System.Net.HttpStatusCode.InternalServerError,
                    new { ex.Message });
            }
        }
        /// <summary>
        /// Busca os documentod financeiros em Aberto do Cooperado
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("buscarFinanceiro")] 
        public IHttpActionResult buscarFinanceiro()//
        {

            
            try
            {
                FinanceiroModel retorno = componente.buscarFinanceiro();
                return Ok(retorno);
            }
            catch (Exception ex)
            {
                return Content(
                    System.Net.HttpStatusCode.InternalServerError,
                    new { ex.Message });
            }
        }
        /// <summary>
        /// Busca as notas fiscais
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("buscarNotasFiscais")]
        public IHttpActionResult buscarNotasFicais(BuscarNotasFiscalModel request)//
        {

            
            try
            {
                List<NotasFiscalModel> retorno = componente.buscarNotasFicais(request);
                return Ok(retorno);
            }
            catch (Exception ex)
            {
                return Content(
                    System.Net.HttpStatusCode.InternalServerError,
                    new { ex.Message });
            }
        }
        /// <summary>
        /// Busca os cards confirgurados no sistema 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("buscarCard")]
        public IHttpActionResult buscarCard()
        {
            try
            {
                List<CardModel> retorno = componente.buscarCard();
                return Ok(retorno);
            }
            catch(Exception ex)
            {
                return Content(
                    System.Net.HttpStatusCode.InternalServerError,
                    new { ex.Message });
            }
            
        }

        /// <summary>
        /// Busca as informações do usuario logado
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("buscarUserInfo")]
        public IHttpActionResult buscarUserInfo()
        {

            
            try
            {
                UserInfoModel retorno = componente.buscarUserInfo();
                return Ok(retorno);
            }
            catch (Exception ex)
            {
                return Content(
                    System.Net.HttpStatusCode.InternalServerError,
                    new { ex.Message });
            }
        }
        /// <summary>
        /// Busca a programação do periodo
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("buscarProgramacao")]
        public IHttpActionResult buscarProgramacao([FromUri] ProgramacaoBuscarModel request)
        {

            
            try
            {
                List<ProgramacaoModel> retorno = componente.buscarProgramacao(request);
                return Ok(retorno);
            }
            catch (Exception ex)
            {
                return Content(
                    System.Net.HttpStatusCode.InternalServerError,
                    new { ex.Message });
            }

        }

        /// <summary>
        /// Envia um registro de SAC
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("enviarSac")] //gerar excessão
        public IHttpActionResult enviarSac([FromBody] SacModelPost request)
        {

            
            try
            {
                ResponseModel retorno = componente.enviarSac(request);
                return Ok(retorno);
            }
            catch (Exception ex)
            {
                return Content(
                    System.Net.HttpStatusCode.InternalServerError,
                    new { ex.Message });
            }
        }
        /// <summary>
        /// Busca os registros de SAC
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("buscarSac")]
        public IHttpActionResult buscarSac()
        {

            
            try
            {
                List<SacModelGet> retorno = componente.buscarSac();
                return Ok(retorno);
            }
            catch (Exception ex)
            {
                return Content(
                    System.Net.HttpStatusCode.InternalServerError,
                    new { ex.Message });
            }
        }
    }
}
