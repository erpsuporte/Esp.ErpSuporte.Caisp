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
    /// Nome da Tabela: K_GN_DOCUMENTOS.
    /// Essa é uma classe parcial, os atributos, herança e propriedades estão definidos no arquivo GnDocumentos.properties.cs
    /// </summary>
    /// private bool editingCalled = false;
    public partial class GnDocumentos
    {


        private void AtribuiSomenteLeitura()
        {
            base.Visualization.Fields["PRIVACIDADE"].ReadOnly = true;
            base.Visualization.Fields["PESSOASAUTORIZADAS"].ReadOnly = true;
        }
        
        protected override void Edited()
        {
            this.Editado = true;
            this.Save();
            base.Edited();
            AtribuiSomenteLeitura();

        }
        protected override void Saving()
        {
            string doc = this.ArquivoPdf.ToString();
            if (!doc.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            {
                throw new BusinessException("Documento fora do formato .pdf");
            }

            base.Saving();
        }
        protected override void Saved()
        {   
            if (this.Editado == false && this.PrivacidadeDoDocumento != 1)
            {

                //List<Handle> handleList = (this.Fields["CAMPOFILTRO"] as EntityAggregation).ToHandleList();
                //Handle[] handleArray = (this.Fields["CAMPOFILTRO"] as EntityAggregation).ToHandleList().ToArray();
                foreach (Handle handle in (this.Fields["PESSOASAUTORIZADAS"] as EntityAggregation).ToHandleList())
                {
                    EntityBase documentopessoas = Entity.Create(EntityDefinition.GetByName("K_GN_DOCUMENTOPESSOAS"));
                    documentopessoas.Fields["PESSOA"] = new EntityAssociation(handle, EntityDefinition.GetByName("GN_PESSOAS"));
                    documentopessoas.Fields["DOCUMENTO"] = new EntityAssociation(base.Handle, this.Definition);
                    documentopessoas.Save();
                }

                

            }


            base.Saved();
        }
    }

}

