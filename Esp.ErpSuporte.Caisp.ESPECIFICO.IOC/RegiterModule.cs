using Ninject.Modules;
using Benner.Tecnologia.Business;

namespace Esp.ErpSuporte.Caisp.ESPECIFICO.IOC
{
    public class RegiterModule : NinjectModule
    {
        public override void Load()
        {
            BusinessComponent.Register<ICaisp, Caisp>(Kernel);
        }
    }
}
