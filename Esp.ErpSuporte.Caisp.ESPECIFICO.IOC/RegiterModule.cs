﻿using Ninject.Modules;
using Benner.Tecnologia.Business;
using Esp.ErpSuporte.Caisp.Business.Interfaces.Caisp;
using Esp.ErpSuporte.Caisp.Components.Caisp;

namespace Esp.ErpSuporte.Caisp.ESPECIFICO.IOC
{
    public class RegiterModule : NinjectModule
    {
        public override void Load()
        {
            BusinessComponent.Register<ICaisp, CaispComponente>(Kernel);
        }
    }
}
