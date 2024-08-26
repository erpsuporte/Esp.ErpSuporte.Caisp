using Benner.Corporativo.Definicoes.Comercial;
using Benner.Tecnologia.Business;
using Benner.Tecnologia.Business.Validation;
using Benner.Tecnologia.Common;
using Esp.ErpSuporte.Caisp.Business.Entidades;
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


namespace Esp.Erpsuporte.Caisp.Business.Entidades
{
    
    
    /// <summary>
    /// Nome da Tabela: K_CM_PROCESSARANALISEFATURAMEN.
    /// Essa é uma classe parcial, os atributos, herança e propriedades estão definidos no arquivo CmProcessaranalisefaturamento.properties.cs
    /// </summary>
    public partial class CmProcessaranalisefaturamento
    {
        private readonly ICaisp gerenciador;
        public CmProcessaranalisefaturamento()
        {
            this.gerenciador = Benner.Tecnologia.Common.IoC.DependencyContainer.Get<ICaisp>();
        }
        public void Processar(BusinessArgs args)
        {
            
            if (this.Status.Index >= 2)
            {
                args.Message = "Processamento ja foi inciado anteriormente";
            }
            else
            {
                this.Status = CmProcessaranalisefaturamentoStatusListaItens.ItemProcessando;
                this.Save();
                ProcessarAnaliseModel request = new ProcessarAnaliseModel();
                request.Datafim = Convert.ToString(this.Datafinal);
                request.Datainicio = Convert.ToString(this.Datainicial);
                request.Processo = Convert.ToInt32(this.Handle);
                args.Message = this.gerenciador.AnaliseFaturamento(request);

            }

        }
        protected override void Created()
        {
            this.Status = CmProcessaranalisefaturamentoStatusListaItens.ItemAguadandoProcessamento;
            base.Created();
        }
        public override void Validate(ValidationResults validationResults)
        {
            if (this.Datainicial > this.Datafinal)
            {
                validationResults.AddResult(new EntityValidationResult("A data inicial não pode ser maior que a final"));
            }

            base.Validate(validationResults);
        }
    }
}
