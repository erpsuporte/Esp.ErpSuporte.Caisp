using Benner.Tecnologia.Business;
using Benner.Tecnologia.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;


namespace Esp.ErpSuporte.Caisp.Business.Entidades
{


    /// <summary>
    /// Nome da Tabela: K_SAC.
    /// Essa é uma classe parcial, os atributos, herança e propriedades estão definidos no arquivo Sac.properties.cs
    /// </summary>
    public partial class Sac
    {
        protected override void Saving()
        {
            if (this.Resposta != null)
            {
                this.Status = SacStatusListaItens.ItemRespondido;
                this.Cor = new ColorField(32768);
                (this.Fields["USUARIORESPOSTA"] as EntityAssociation).Handle = BennerContext.Security.GetLoggedUserHandle();
                base.Saving();
            }
        }
    }
}
