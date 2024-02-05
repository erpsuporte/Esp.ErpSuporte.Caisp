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

    }
}
