using Benner.Tecnologia.Business;
using Benner.Tecnologia.Business.Validation;
using Benner.Tecnologia.Common;
using Esp.ErpSuporte.Caisp.Business.Interfaces.Caisp;
using Esp.ErpSuporte.Caisp.Business.Modelos.Caisp;
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
    /// Nome da Tabela: K_GN_PROCESSARANALISE.
    /// Essa é uma classe parcial, os atributos, herança e propriedades estão definidos no arquivo GnProcessaranalise.properties.cs
    /// </summary>
    /// Não foi possível carregar o tipo Esp.ErpSuporte.Caisp.Business.Entidades.GnProcessaranalise do assembly Esp.ErpSuporte.Caisp.Business
    public partial class GnProcessaranalise
    {
        private readonly ICaisp gerenciador;
        public GnProcessaranalise()
        {
            this.gerenciador = Benner.Tecnologia.Common.IoC.DependencyContainer.Get<ICaisp>();
        }
        public void Processar(BusinessArgs args)
        {
            if(this.Status.Index >= 2)
            {
                args.Message = "Processamento ja foi inciado anteriormente";
            }
            else
            {
                this.Status = GnProcessaranaliseStatusListaItens.ItemProcessando;
                this.Save();
                ProcessarAnaliseModel request = new ProcessarAnaliseModel();
                request.Datafim = Convert.ToString(this.DataFinal);
                request.Datainicio = Convert.ToString(this.DataInicio);
                request.Processo = Convert.ToInt32(this.Handle);
                args.Message = this.gerenciador.ProcessarAnalise(request);
                
            }
            
        }
        protected override void Created()
        {
            this.Status = GnProcessaranaliseStatusListaItens.ItemAguardandoProcessamento;
            base.Created();
        }
        public override void Validate(ValidationResults validationResults)
        {
            if (this.DataInicio > this.DataFinal)
            {
                validationResults.AddResult(new EntityValidationResult("A data inicial não pode ser maior que a final"));
            }

            base.Validate(validationResults);
        }
    }
}
