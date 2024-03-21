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
    /// Nome da Tabela: K_GN_CARDS.
    /// Essa é uma classe parcial, os atributos, herança e propriedades estão definidos no arquivo KGnCards.properties.cs
    /// </summary>
    public partial class KGnCards
    {
        
        protected override void Saving()
        {
            var Valor = "";
            try
            {
                Query query = new Query($@"{this.Consultasql}");
                var registros = query.Execute();
                foreach (EntityBase registro in registros)
                {
                    Valor = Convert.ToString(registro.Fields["VALOR"]);

                }
                base.Saving();
            }
            catch (Exception ex)
            {
                //this.Consultasql = ex.Message;
                throw new BusinessException(ex.Message);
            }


            
        }
    }
}
