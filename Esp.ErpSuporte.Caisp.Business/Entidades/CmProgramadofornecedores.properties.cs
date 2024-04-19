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
    /// Interface para a tabela K_CM_PROGRAMADOFORNECEDORES
    /// </summary>
    public partial interface ICmProgramadofornecedores : IEntityBase
    {
        
        /// <summary>
        /// Cota (COTA.)
        /// Opcional = N, Invisível = False, Valor Mínimo = , Valor Máximo = , Tipo no Builder = Número
        /// </summary>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("BEF Code Generator", "20.0.90.3")]
        System.Nullable<decimal> Cota
        {
            get;
            set;
        }
        
        /// <summary>
        /// Data alteração (DATAALTERACAO.)
        /// Opcional = S, Invisível = False, Formato Data = Dia, Mês, Ano - Formato Hora = Hora, Minuto
        /// </summary>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("BEF Code Generator", "20.0.90.3")]
        System.Nullable<System.DateTime> DataAlteracao
        {
            get;
            set;
        }
        
        /// <summary>
        /// Data inclusão (DATAINCLUSAO.)
        /// Opcional = S, Invisível = False, Formato Data = Dia, Mês, Ano - Formato Hora = Hora, Minuto
        /// </summary>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("BEF Code Generator", "20.0.90.3")]
        System.Nullable<System.DateTime> DataInclusao
        {
            get;
            set;
        }
        
        [System.CodeDom.Compiler.GeneratedCodeAttribute("BEF Code Generator", "20.0.90.3")]
        Benner.Corporativo.Definicoes.Genericos.IGNPessoa FornecedorInstance
        {
            get;
            set;
        }
        
        [System.CodeDom.Compiler.GeneratedCodeAttribute("BEF Code Generator", "20.0.90.3")]
        Handle FornecedorHandle
        {
            get;
            set;
        }
        

        
        [System.CodeDom.Compiler.GeneratedCodeAttribute("BEF Code Generator", "20.0.90.3")]
        Handle ProgramadoProdutoHandle
        {
            get;
            set;
        }
        
        /// <summary>
        /// Quantidade cota (QUANTIDADECOTA.)
        /// Opcional = N, Invisível = False, Valor Mínimo = , Valor Máximo = , Tipo no Builder = Número
        /// </summary>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("BEF Code Generator", "20.0.90.3")]
        System.Nullable<decimal> QuantidadeCota
        {
            get;
            set;
        }
        
        /// <summary>
        /// Quantidade programada (QUANTIDADEPROGRAMADA.)
        /// Opcional = S, Invisível = False, Valor Mínimo = , Valor Máximo = , Tipo no Builder = Número
        /// </summary>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("BEF Code Generator", "20.0.90.3")]
        System.Nullable<decimal> QuantidadeProgramada
        {
            get;
            set;
        }
        
        [System.CodeDom.Compiler.GeneratedCodeAttribute("BEF Code Generator", "20.0.90.3")]
        Benner.Tecnologia.Metadata.Entities.IZGrupoUsuarios UsuarioAlterouInstance
        {
            get;
            set;
        }
        
        [System.CodeDom.Compiler.GeneratedCodeAttribute("BEF Code Generator", "20.0.90.3")]
        Handle UsuarioAlterouHandle
        {
            get;
            set;
        }
        
        [System.CodeDom.Compiler.GeneratedCodeAttribute("BEF Code Generator", "20.0.90.3")]
        Benner.Tecnologia.Metadata.Entities.IZGrupoUsuarios UsuarioIncluiuInstance
        {
            get;
            set;
        }
        
        [System.CodeDom.Compiler.GeneratedCodeAttribute("BEF Code Generator", "20.0.90.3")]
        Handle UsuarioIncluiuHandle
        {
            get;
            set;
        }
    }
    
    /// <summary>
    /// Interface para o DAO para a tabela K_CM_PROGRAMADOFORNECEDORES
    /// </summary>
    public partial interface ICmProgramadofornecedoresDao : IBusinessEntityDao<ICmProgramadofornecedores>
    {
    }
    
    /// <summary>
    /// DAO para a tabela K_CM_PROGRAMADOFORNECEDORES
    /// </summary>
    public partial class CmProgramadofornecedoresDao : BusinessEntityDao<CmProgramadofornecedores, ICmProgramadofornecedores>, ICmProgramadofornecedoresDao
    {
        
        public static CmProgramadofornecedoresDao CreateInstance()
        {
            return CreateInstance<CmProgramadofornecedoresDao>();
        }
    }
    
    /// <summary>
    /// CmProgramadofornecedores
    /// </summary>
    [EntityDefinitionName("K_CM_PROGRAMADOFORNECEDORES")]
    [DataContract(Namespace = "http://Benner.Tecnologia.Common.DataContracts/2007/09", Name = "EntityBase")]
    public partial class CmProgramadofornecedores : BusinessEntity<CmProgramadofornecedores>, ICmProgramadofornecedores
    {
        
        /// <summary>
        /// Possui constantes para retornarem o nome dos campos definidos no Builder para cada propriedade
        /// </summary>
		public static class FieldNames
		{
			public const string Cota = "COTA";
			public const string DataAlteracao = "DATAALTERACAO";
			public const string DataInclusao = "DATAINCLUSAO";
			public const string Fornecedor = "FORNECEDOR";
			public const string ProgramadoProduto = "PROGRAMADOPRODUTO";
			public const string QuantidadeCota = "QUANTIDADECOTA";
			public const string QuantidadeProgramada = "QUANTIDADEPROGRAMADA";
			public const string UsuarioAlterou = "USUARIOALTEROU";
			public const string UsuarioIncluiu = "USUARIOINCLUIU";
		}

        
        /// <summary>
        /// Cota (COTA.)
        /// Opcional = N, Invisível = False, Valor Mínimo = , Valor Máximo = , Tipo no Builder = Número
        /// </summary>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("BEF Code Generator", "20.0.90.3")]
        public System.Nullable<decimal> Cota
        {
            get
            {
                return Fields["COTA"] as System.Nullable<System.Decimal>;
            }
            set
            {
                Fields["COTA"] = value;
            }
        }
        
        /// <summary>
        /// Data alteração (DATAALTERACAO.)
        /// Opcional = S, Invisível = False, Formato Data = Dia, Mês, Ano - Formato Hora = Hora, Minuto
        /// </summary>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("BEF Code Generator", "20.0.90.3")]
        public System.Nullable<System.DateTime> DataAlteracao
        {
            get
            {
                return Fields["DATAALTERACAO"] as System.Nullable<System.DateTime>;
            }
            set
            {
                Fields["DATAALTERACAO"] = value;
            }
        }
        
        /// <summary>
        /// Data inclusão (DATAINCLUSAO.)
        /// Opcional = S, Invisível = False, Formato Data = Dia, Mês, Ano - Formato Hora = Hora, Minuto
        /// </summary>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("BEF Code Generator", "20.0.90.3")]
        public System.Nullable<System.DateTime> DataInclusao
        {
            get
            {
                return Fields["DATAINCLUSAO"] as System.Nullable<System.DateTime>;
            }
            set
            {
                Fields["DATAINCLUSAO"] = value;
            }
        }
        
        [System.CodeDom.Compiler.GeneratedCodeAttribute("BEF Code Generator", "20.0.90.3")]
        public Benner.Corporativo.Definicoes.Genericos.IGNPessoa FornecedorInstance
        {
            get
            {
                if (Fornecedor.Handle == null)
                {
					return null;
                }
                return Fornecedor.Instance;
            }
            set
            {
                if (value == null)
                {
					Fornecedor = null;
					return;
                }
                Fornecedor.Instance = (Benner.Corporativo.Definicoes.Genericos.GNPessoa) value;
            }
        }
        
        [System.CodeDom.Compiler.GeneratedCodeAttribute("BEF Code Generator", "20.0.90.3")]
        public Handle FornecedorHandle
        {
            get
            {
                return Fornecedor.Handle;
            }
            set
            {
                Fornecedor.Handle = value;
            }
        }
        
        /// <summary>
        /// Fornecedor (FORNECEDOR.)
        /// Opcional = N, Invisível = False, Pesquisar = GN_PESSOAS
        /// </summary>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("BEF Code Generator", "20.0.90.3")]
        public Benner.Tecnologia.Common.EntityAssociation<Benner.Corporativo.Definicoes.Genericos.GNPessoa> Fornecedor
        {
            get
            {
                return (Fields["FORNECEDOR"] as EntityAssociation).Wrap<Benner.Corporativo.Definicoes.Genericos.GNPessoa>(Benner.Corporativo.Definicoes.Genericos.GNPessoa.Get);
            }
            set
            {
                if (value == null)
                {
                    this.Fornecedor.Handle = null;
                }
                else
                {
                    if (value.Association.IsLoaded)
                    {
                        this.Fornecedor.Instance = value.Instance;
                    }
                    else
                    {
                        this.Fornecedor.Handle = value.Handle;
                    }
                }
            }
        }
     

        
        /// <summary>
        /// Quantidade cota (QUANTIDADECOTA.)
        /// Opcional = N, Invisível = False, Valor Mínimo = , Valor Máximo = , Tipo no Builder = Número
        /// </summary>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("BEF Code Generator", "20.0.90.3")]
        public System.Nullable<decimal> QuantidadeCota
        {
            get
            {
                return Fields["QUANTIDADECOTA"] as System.Nullable<System.Decimal>;
            }
            set
            {
                Fields["QUANTIDADECOTA"] = value;
            }
        }
        
        /// <summary>
        /// Quantidade programada (QUANTIDADEPROGRAMADA.)
        /// Opcional = S, Invisível = False, Valor Mínimo = , Valor Máximo = , Tipo no Builder = Número
        /// </summary>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("BEF Code Generator", "20.0.90.3")]
        public System.Nullable<decimal> QuantidadeProgramada
        {
            get
            {
                return Fields["QUANTIDADEPROGRAMADA"] as System.Nullable<System.Decimal>;
            }
            set
            {
                Fields["QUANTIDADEPROGRAMADA"] = value;
            }
        }
        
        [System.CodeDom.Compiler.GeneratedCodeAttribute("BEF Code Generator", "20.0.90.3")]
        public Benner.Tecnologia.Metadata.Entities.IZGrupoUsuarios UsuarioAlterouInstance
        {
            get
            {
                if (UsuarioAlterou.Handle == null)
                {
					return null;
                }
                return UsuarioAlterou.Instance;
            }
            set
            {
                if (value == null)
                {
					UsuarioAlterou = null;
					return;
                }
                UsuarioAlterou.Instance = (Benner.Tecnologia.Metadata.Entities.ZGrupoUsuarios) value;
            }
        }
        
        [System.CodeDom.Compiler.GeneratedCodeAttribute("BEF Code Generator", "20.0.90.3")]
        public Handle UsuarioAlterouHandle
        {
            get
            {
                return UsuarioAlterou.Handle;
            }
            set
            {
                UsuarioAlterou.Handle = value;
            }
        }
        
        /// <summary>
        /// Usuario alterou (USUARIOALTEROU.)
        /// Opcional = S, Invisível = False, Pesquisar = Z_GRUPOUSUARIOS
        /// </summary>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("BEF Code Generator", "20.0.90.3")]
        public Benner.Tecnologia.Common.EntityAssociation<Benner.Tecnologia.Metadata.Entities.ZGrupoUsuarios> UsuarioAlterou
        {
            get
            {
                return (Fields["USUARIOALTEROU"] as EntityAssociation).Wrap<Benner.Tecnologia.Metadata.Entities.ZGrupoUsuarios>(Benner.Tecnologia.Metadata.Entities.ZGrupoUsuarios.Get);
            }
            set
            {
                if (value == null)
                {
                    this.UsuarioAlterou.Handle = null;
                }
                else
                {
                    if (value.Association.IsLoaded)
                    {
                        this.UsuarioAlterou.Instance = value.Instance;
                    }
                    else
                    {
                        this.UsuarioAlterou.Handle = value.Handle;
                    }
                }
            }
        }
        
        [System.CodeDom.Compiler.GeneratedCodeAttribute("BEF Code Generator", "20.0.90.3")]
        public Benner.Tecnologia.Metadata.Entities.IZGrupoUsuarios UsuarioIncluiuInstance
        {
            get
            {
                if (UsuarioIncluiu.Handle == null)
                {
					return null;
                }
                return UsuarioIncluiu.Instance;
            }
            set
            {
                if (value == null)
                {
					UsuarioIncluiu = null;
					return;
                }
                UsuarioIncluiu.Instance = (Benner.Tecnologia.Metadata.Entities.ZGrupoUsuarios) value;
            }
        }
        
        [System.CodeDom.Compiler.GeneratedCodeAttribute("BEF Code Generator", "20.0.90.3")]
        public Handle UsuarioIncluiuHandle
        {
            get
            {
                return UsuarioIncluiu.Handle;
            }
            set
            {
                UsuarioIncluiu.Handle = value;
            }
        }
        
        /// <summary>
        /// Usuário incluiu (USUARIOINCLUIU.)
        /// Opcional = S, Invisível = False, Pesquisar = Z_GRUPOUSUARIOS
        /// </summary>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("BEF Code Generator", "20.0.90.3")]
        public Benner.Tecnologia.Common.EntityAssociation<Benner.Tecnologia.Metadata.Entities.ZGrupoUsuarios> UsuarioIncluiu
        {
            get
            {
                return (Fields["USUARIOINCLUIU"] as EntityAssociation).Wrap<Benner.Tecnologia.Metadata.Entities.ZGrupoUsuarios>(Benner.Tecnologia.Metadata.Entities.ZGrupoUsuarios.Get);
            }
            set
            {
                if (value == null)
                {
                    this.UsuarioIncluiu.Handle = null;
                }
                else
                {
                    if (value.Association.IsLoaded)
                    {
                        this.UsuarioIncluiu.Instance = value.Instance;
                    }
                    else
                    {
                        this.UsuarioIncluiu.Handle = value.Handle;
                    }
                }
            }
        }

        public Handle ProgramadoProdutoHandle { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    }
}
