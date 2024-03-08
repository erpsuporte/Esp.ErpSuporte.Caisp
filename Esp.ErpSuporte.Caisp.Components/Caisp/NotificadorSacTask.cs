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

            var usuario = ZAgendamentos.Get(new Criteria("A.TIPO = 6")).Usuario.Instance; //engine de emails 6
            var msg = _mailService.NewMailMessage();
            msg.SendTo = string.Join(", ", request.Destinatarios);
            msg.Subject = request.Titulo;
            msg.Body = request.Mensagem;
            msg.SystemUser = usuario.Handle;
            _mailService.Send(msg);
            
        }
    }
}
