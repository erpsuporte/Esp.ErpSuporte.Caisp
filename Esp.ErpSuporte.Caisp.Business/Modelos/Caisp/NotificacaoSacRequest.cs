using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Esp.ErpSuporte.Caisp.Business.Modelos.Caisp
{
    public class NotificacaoSacRequest
    {
        public IList<String> Destinatarios { get; set; }
        public string Titulo { get; set; }
        public string Mensagem { get; set; }

        public NotificacaoSacRequest()
        {
            this.Destinatarios = new List<String>();
        }
        public NotificacaoSacRequest(string email, string titulo, string mensagem) : this()
        {
            this.Destinatarios.Add(email);
            this.Titulo = titulo;
            this.Mensagem = mensagem;
        }
    }
}
