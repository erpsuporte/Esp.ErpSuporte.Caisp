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
        [Route("buscarContatos")] //Criar pagina de cadastro no Wes com contato
        public IHttpActionResult buscarContatos()
        {
            List<ContatosModel> retorno = componente.buscarContatos();
            return Ok(retorno);
        }
        /// <summary>
        /// Busca os documentos do Administrativo da Caisp
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("buscarDoc")]// Criar pagina de cadastro de pessoas e pessoausuarios no Wes
        public IHttpActionResult buscarDoc(int tipo)
        {

            List<DocModel> retorno = componente.buscarDoc(tipo);
            return Ok(retorno);
        }

        /// <summary>
        /// Busca as entregas do dia
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("buscarEntregasDia")]// SELECT PARA ver o que recebeu, verificar qual usuario que tem entregas
        public IHttpActionResult buscarEntregasDia([FromUri] EntregasDiaBuscarModel request)
        {

            List<EntregasDiaModel> retorno = componente.buscarEntregasDia(request);
            return Ok(retorno);
        }
        /// <summary>
        /// Busca as entregas do periodo informado
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("buscarEntregasPeriodo")]
        public IHttpActionResult buscarEntregasPeriodo([FromUri] EntregasPeriodoBuscaModel request)
        {

            List<EntregasItensModel> retorno = componente.buscarEntregasPeriodo(request);
            return Ok(retorno);
        }
        /// <summary>
        /// Busca os proximos enventos da Caisp
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("buscarEventos")]
        public IHttpActionResult buscarEventos()
        {

            List<Eventos> retorno = componente.buscarEventos();
            return Ok(retorno);
        }
        /// <summary>
        /// Busca os documentod financeiros em Aberto do Cooperado
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("buscarFinanceiro")] // adicionar selects que estão na área de trabalho 
        public IHttpActionResult buscarFinanceiro()
        {

            FinanceiroModel retorno = componente.buscarFinanceiro();
            return Ok(retorno);
        }
        /// <summary>
        /// Busca os cards confirgurados no sistema 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("buscarCard")]
        public IHttpActionResult buscarCard()
        {

            List<CardModel> retorno = componente.buscarCard();
            return Ok(retorno);
        }

        /// <summary>
        /// Busca as informações do usuario logado
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("buscarUserInfo")]
        public IHttpActionResult buscarUserInfo()
        {

            UserInfoModel retorno = componente.buscarUserInfo();
            return Ok(retorno);
        }
        /// <summary>
        /// Busca a programação do periodo
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("buscarProgramacao")]
        public IHttpActionResult buscarProgramacao([FromUri] ProgramacaoBuscarModel request)
        {

            List<ProgramacaoModel> retorno = componente.buscarProgramacao(request);
            return Ok(retorno);
        }

        /// <summary>
        /// Envia um registro de SAC
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("enviarSac")]
        public IHttpActionResult enviarSac([FromBody] SacModelPost request)
        {

            ResponseModel retorno = componente.enviarSac(request);
            return Ok(retorno);
        }
        /// <summary>
        /// Busca os registros de SAC
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("buscarSac")]
        public IHttpActionResult buscarSac()
        {

            List<SacModelGet> retorno = componente.buscarSac();
            return Ok(retorno);
        }
    }
}
