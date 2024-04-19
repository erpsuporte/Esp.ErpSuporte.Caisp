using Benner.Tecnologia.Business;
using Benner.Tecnologia.Common;
using Esp.ErpSuporte.Caisp.Business.Interfaces.Caisp;
using Esp.ErpSuporte.Caisp.Business.Modelos.Caisp;
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
    /// Nome da Tabela: K_CM_PROGRAMADOPRODUTOS.
    /// Essa é uma classe parcial, os atributos, herança e propriedades estão definidos no arquivo CmProgramadoprodutos.properties.cs
    /// </summary>
    public partial class CmProgramadoprodutos
    {
        private readonly ICaisp gerenciador;
        public CmProgramadoprodutos()
        {
            this.gerenciador = Benner.Tecnologia.Common.IoC.DependencyContainer.Get<ICaisp>();
        }
        protected override void Created()
        {
            this.Fields["USUARIOINCLUIU"] = new EntityAssociation(Convert.ToInt32(BennerContext.Security.GetLoggedUserHandle()), EntityDefinition.GetByName("Z_GRUPOUSUARIOS"));
            this.Fields["STATUS"] = new ListItem(1, "");
            base.Created();
        }
        protected override void Edited()
        {
            this.Fields["USUARIOALTEROU"] = new EntityAssociation(Convert.ToInt32(BennerContext.Security.GetLoggedUserHandle()), EntityDefinition.GetByName("Z_GRUPOUSUARIOS"));
            base.Edited();
        }
        public void CarregarFonecedores(BusinessArgs args)
        {
            if (((ListItem)this.Fields["STATUS"]).Value >= 2)
            {
                args.Message = "Processamento ja foi inciado anteriormente";
            }

            RequestCarregarFornecerdor request = new RequestCarregarFornecerdor();
            request.Produto = Convert.ToInt32(this.Produto.Handle);
            request.Handle = Convert.ToInt32(this.Handle);
            request.Quantidade = Convert.ToInt32(this.Quantidade);
            request.DataInicio = Convert.ToString(this.Programados.Instance.Fields["DATAINICIO"]);
            request.DataFim = Convert.ToString(this.Programados.Instance.Fields["DATAFIM"]);
            args.Message = this.gerenciador.CarregarFornecedores(request);
        }
        public static void CarregarFonecedoresSelecionados(BusinessArgs args)
        {
            
            var dataEntity = args.DataEntity;
            ICaisp gerenciador = Benner.Tecnologia.Common.IoC.DependencyContainer.Get<ICaisp>();
            args.Message = gerenciador.CarregarFonecedoresSelecionados(args);

        }
    }
}
