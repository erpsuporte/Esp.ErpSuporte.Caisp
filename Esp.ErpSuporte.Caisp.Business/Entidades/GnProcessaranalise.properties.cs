﻿//------------------------------------------------------------------------------
// <auto-generated>
//     O código foi gerado por uma ferramenta.
//     Versão de Tempo de Execução:4.0.30319.42000
//
//     As alterações ao arquivo poderão causar comportamento incorreto e serão perdidas se
//     o código for gerado novamente.
// </auto-generated>
//------------------------------------------------------------------------------

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
    /// Interface para a tabela K_GN_PROCESSARANALISE
    /// </summary>
    public partial interface IGnProcessaranalise : IEntityBase
    {
        
        /// <summary>
        /// Data final (DATAFIM.)
        /// Opcional = N, Invisível = False, Formato Data = Dia, Mês, Ano - Formato Hora = Sem hora
        /// </summary>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("BEF Code Generator", "20.0.90.3")]
        System.Nullable<System.DateTime> DataFinal
        {
            get;
            set;
        }
        
        /// <summary>
        /// Data de início (DATAINICIO.)
        /// Opcional = N, Invisível = False, Formato Data = Dia, Mês, Ano - Formato Hora = Sem hora
        /// </summary>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("BEF Code Generator", "20.0.90.3")]
        System.Nullable<System.DateTime> DataInicio
        {
            get;
            set;
        }
        
        /// <summary>
        /// Nome (NOME.)
        /// Opcional = N, Invisível = False, Valor Mínimo = , Valor Máximo = , Tipo do Builder = Inteiro
        /// </summary>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("BEF Code Generator", "20.0.90.3")]
        System.Nullable<long> Nome
        {
            get;
            set;
        }
        
        /// <summary>
        /// Status (STATUS.)
        /// Opcional = N, Invisível = False
        /// </summary>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("BEF Code Generator", "20.0.90.3")]
        GnProcessaranaliseStatusListaItens Status
        {
            get;
            set;
        }
    }
    
    /// <summary>
    /// Interface para o DAO para a tabela K_GN_PROCESSARANALISE
    /// </summary>
    public partial interface IGnProcessaranaliseDao : IBusinessEntityDao<IGnProcessaranalise>
    {
    }
    
    /// <summary>
    /// DAO para a tabela K_GN_PROCESSARANALISE
    /// </summary>
    public partial class GnProcessaranaliseDao : BusinessEntityDao<GnProcessaranalise, IGnProcessaranalise>, IGnProcessaranaliseDao
    {
        
        public static GnProcessaranaliseDao CreateInstance()
        {
            return CreateInstance<GnProcessaranaliseDao>();
        }
    }
    
    /// <summary>
    /// Esta classe contém os itens do campo STATUS.
    /// </summary>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("BEF Code Generator", "20.0.90.3")]
    public class GnProcessaranaliseStatusListaItens : ListItems<GnProcessaranaliseStatusListaItens>
    {
        
        /// <summary>
        /// Valor = 1, Item = Aguardando Processamento.
        /// </summary>
        public static GnProcessaranaliseStatusListaItens ItemAguardandoProcessamento;
        
        /// <summary>
        /// Valor = 2, Item = Processando.
        /// </summary>
        public static GnProcessaranaliseStatusListaItens ItemProcessando;
        
        /// <summary>
        /// Valor = 3, Item = Processado.
        /// </summary>
        public static GnProcessaranaliseStatusListaItens ItemProcessado;
        
		public static implicit operator GnProcessaranaliseStatusListaItens(int index)
		{
			return GetByIndex(index);
		}

		public static implicit operator int(GnProcessaranaliseStatusListaItens item)
		{
			return item.Index;
		}
        
        static GnProcessaranaliseStatusListaItens()
        {
			ItemAguardandoProcessamento = new GnProcessaranaliseStatusListaItens {Index = 1, Description ="Aguardando Processamento"};
			ItemProcessando = new GnProcessaranaliseStatusListaItens {Index = 2, Description ="Processando"};
			ItemProcessado = new GnProcessaranaliseStatusListaItens {Index = 3, Description ="Processado"};

			Items.Add(ItemAguardandoProcessamento);
			Items.Add(ItemProcessando);
			Items.Add(ItemProcessado);

        }
    }
    
    /// <summary>
    /// GnProcessaranalise
    /// </summary>
    [EntityDefinitionName("K_GN_PROCESSARANALISE")]
    [DataContract(Namespace = "http://Benner.Tecnologia.Common.DataContracts/2007/09", Name = "EntityBase")]
    public partial class GnProcessaranalise : BusinessEntity<GnProcessaranalise>, IGnProcessaranalise
    {
        
        /// <summary>
        /// Possui constantes para retornarem o nome dos campos definidos no Builder para cada propriedade
        /// </summary>
		public static class FieldNames
		{
			public const string DataFinal = "DATAFIM";
			public const string DataInicio = "DATAINICIO";
			public const string Nome = "NOME";
			public const string Status = "STATUS";
		}

        
        /// <summary>
        /// Data final (DATAFIM.)
        /// Opcional = N, Invisível = False, Formato Data = Dia, Mês, Ano - Formato Hora = Sem hora
        /// </summary>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("BEF Code Generator", "20.0.90.3")]
        public System.Nullable<System.DateTime> DataFinal
        {
            get
            {
                return Fields["DATAFIM"] as System.Nullable<System.DateTime>;
            }
            set
            {
                Fields["DATAFIM"] = value;
            }
        }
        
        /// <summary>
        /// Data de início (DATAINICIO.)
        /// Opcional = N, Invisível = False, Formato Data = Dia, Mês, Ano - Formato Hora = Sem hora
        /// </summary>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("BEF Code Generator", "20.0.90.3")]
        public System.Nullable<System.DateTime> DataInicio
        {
            get
            {
                return Fields["DATAINICIO"] as System.Nullable<System.DateTime>;
            }
            set
            {
                Fields["DATAINICIO"] = value;
            }
        }
        
        /// <summary>
        /// Nome (NOME.)
        /// Opcional = N, Invisível = False, Valor Mínimo = , Valor Máximo = , Tipo do Builder = Inteiro
        /// </summary>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("BEF Code Generator", "20.0.90.3")]
        public System.Nullable<long> Nome
        {
            get
            {
                return Fields["NOME"] as System.Nullable<System.Int64>;
            }
            set
            {
                Fields["NOME"] = value;
            }
        }
        
        /// <summary>
        /// Status (STATUS.)
        /// Opcional = N, Invisível = False
        /// </summary>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("BEF Code Generator", "20.0.90.3")]
        public GnProcessaranaliseStatusListaItens Status
        {
            get
            {
                ListItem listItem = Fields["STATUS"] as ListItem;
				if (listItem != null)
					return new GnProcessaranaliseStatusListaItens { Index = listItem.Value, Description = listItem.Text };
				return null;
            }
            set
            {
                if (value != null)
					Fields["STATUS"] = new ListItem(value.Index, value.Description);
				else
					Fields["STATUS"] = null;
            }
        }
    }
}
