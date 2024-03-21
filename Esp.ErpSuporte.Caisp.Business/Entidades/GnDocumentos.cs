using Benner.Tecnologia.Business;
using Benner.Tecnologia.Common;
using Benner.Tecnologia.Common.EnterpriseServiceLibrary;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
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
            if (this.PrivacidadeDoDocumento == 2)
            {
                var handlesList = (this.Fields["PESSOASAUTORIZADAS"] as EntityAggregation).ToHandleList();
                Handle[] handlesArray = handlesList.ToArray();
                string handlesString = string.Join(",", handlesArray.Select(h => h.ToString()));

                Query query = new Query($@"SELECT * FROM GN_PESSOAS A WHERE HANDLE IN ({handlesString}) AND NOT EXISTS (SELECT *  FROM K_GN_DOCUMENTOPESSOAS B WHERE A.HANDLE =B.PESSOA AND  DOCUMENTO = :HANDLE)");
                query.Parameters.Add(new Parameter("HANDLE", this.Handle));

                var registros = query.Execute();

                if (registros != null)
                {
                    foreach (EntityBase registro in registros)
                    {
                        EntityBase documentopessoas = Entity.Create(EntityDefinition.GetByName("K_GN_DOCUMENTOPESSOAS"));
                        documentopessoas.Fields["PESSOA"] = new EntityAssociation(Convert.ToInt32(registro.Fields["HANDLE"]), EntityDefinition.GetByName("GN_PESSOAS"));
                        documentopessoas.Fields["DOCUMENTO"] = new EntityAssociation(base.Handle, this.Definition);
                        documentopessoas.Save();
                    }
                }

                Query query2 = new Query($@"DELETE K_GN_DOCUMENTOPESSOAS WHERE DOCUMENTO = :HANDLE AND PESSOA NOT IN ({handlesString})");
                query2.Parameters.Add(new Parameter("HANDLE", this.Handle));
                query2.Execute();
            }
            else
            {
                Query query2 = new Query($@"DELETE K_GN_DOCUMENTOPESSOAS WHERE DOCUMENTO = :HANDLE ");
                query2.Parameters.Add(new Parameter("HANDLE", this.Handle));
                query2.Execute();
                

            }

            base.Saved();
        }
    }

}

