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
    /// Nome da Tabela: K_CM_PROGRAMADOFORNECEDORES.
    /// Essa é uma classe parcial, os atributos, herança e propriedades estão definidos no arquivo CmProgramadofornecedores.properties.cs
    /// </summary>
    public partial class CmProgramadofornecedores
    {
        protected override void Edited()
        {
            this.Fields["USUARIOALTEROU"] = new EntityAssociation(Convert.ToInt32(BennerContext.Security.GetLoggedUserHandle()), EntityDefinition.GetByName("Z_GRUPOUSUARIOS"));
            base.Edited();
        }
    }
}
