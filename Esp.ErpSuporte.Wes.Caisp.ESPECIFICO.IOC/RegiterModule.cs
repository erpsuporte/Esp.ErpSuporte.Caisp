using Ninject.Modules;
using Benner.Tecnologia.Business;

namespace Esp.ErpSuporte.Wes.Caisp.ESPECIFICO.IOC
{
    public class RegiterModule : NinjectModule
    {
        public override void Load()
        {
            BusinessComponent.RegisterProxy<ICaisp>(Kernel);
        }
    }
}
