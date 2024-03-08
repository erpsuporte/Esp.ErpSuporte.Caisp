using Benner.Tecnologia.Business.Tasks;
using Esp.ErpSuporte.Caisp.Business.Modelos;
using Esp.ErpSuporte.Caisp.Business.Modelos.Caisp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Esp.ErpSuporte.Caisp.Business.Interfaces.Caisp
{
    public interface INotificacaoSac: IBusinessTaskAction<NotificacaoSacRequest>
    {
    }
}
