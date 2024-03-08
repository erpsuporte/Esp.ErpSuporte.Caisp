using Ninject.Modules;
using Benner.Tecnologia.Business;
using Esp.ErpSuporte.Caisp.Business.Interfaces.Caisp;

namespace Esp.ErpSuporte.Wes.Caisp.ESPECIFICO.IOC
{
    public class RegiterModule : NinjectModule
    {
        public override void Load()
        {
            BusinessComponent.RegisterProxy<ICaisp>(Kernel);
            BusinessComponent.RegisterProxy<INotificacaoSac>(Kernel);
        }
    }
}
