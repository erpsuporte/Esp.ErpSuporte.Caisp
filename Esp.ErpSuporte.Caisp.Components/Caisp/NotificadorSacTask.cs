using Benner.Tecnologia.Business;
using Benner.Tecnologia.Business.Services;
using Benner.Tecnologia.Common;
using Benner.Tecnologia.Metadata.Entities;
using Esp.ErpSuporte.Caisp.Business.Interfaces.Caisp;
using Esp.ErpSuporte.Caisp.Business.Modelos.Caisp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Esp.ErpSuporte.Caisp.Components.Caisp
{
    public class NotificadorSacTask: BusinessComponent<NotificadorSacTask>, INotificacaoSac
    {
        private readonly IMailService _mailService;

        public NotificadorSacTask(IMailService mailService)
        {
            _mailService = mailService;
            
        }

        public void Run(NotificacaoSacRequest request)
        {
            Query query = new Query(@"SELECT K_USUARIOEMAIL,
                                            K_DESTINATARIOS
                                        FROM GN_PARAMETROS");

            var registros = query.Execute();
            var Destinatarios = "";
            var usuario = 0;
            foreach (EntityBase registro in registros)
            {
                 Destinatarios = Convert.ToString(registro.Fields["K_DESTINATARIOS"]);
                 usuario = Convert.ToInt32(registro.Fields["K_USUARIOEMAIL"]);
            }
            //var usuario = ZAgendamentos.Get(new Criteria("A.TIPO = 6")).Usuario.Instance; //engine de emails 6
            var msg = _mailService.NewMailMessage();
            msg.SendTo = Destinatarios;//string.Join(", ", request.Destinatarios);
            msg.Subject = request.Titulo;
            msg.Body = request.Mensagem;
            msg.SystemUser = usuario;//usuario.Handle; //BennerContext.Security.GetLoggedUserHandle();

            _mailService.Send(msg);
            
        }
    }
}
