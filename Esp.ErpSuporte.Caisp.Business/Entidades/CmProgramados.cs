using Benner.Tecnologia.Business;
using Benner.Tecnologia.Business.Validation;
using Benner.Tecnologia.Common;
using Esp.ErpSuporte.Caisp.Business.Entidades;
using Microsoft.Practices.EnterpriseLibrary.Validation;
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
    /// Nome da Tabela: K_CM_PROGRAMADOS.
    /// Essa é uma classe parcial, os atributos, herança e propriedades estão definidos no arquivo CmProgramados.properties.cs
    /// </summary>
    public partial class CmProgramados
    {
        protected override void Created()
        {
            this.Fields["USUARIOINCLUIU"] = new EntityAssociation(Convert.ToInt32(BennerContext.Security.GetLoggedUserHandle()), EntityDefinition.GetByName("Z_GRUPOUSUARIOS"));
            base.Created();
        }
        protected override void Edited()
        {
            this.Fields["USUARIOALTEROU"] = new EntityAssociation(Convert.ToInt32(BennerContext.Security.GetLoggedUserHandle()), EntityDefinition.GetByName("Z_GRUPOUSUARIOS"));
            base.Edited();
        }
        public override void Validate(ValidationResults validationResults)
        {
            if (this.Datainicio > this.Datafim)
            {
                validationResults.AddResult(new EntityValidationResult("A data inicial não pode ser maior que a final"));
            }

            base.Validate(validationResults);
        }
    }
}
