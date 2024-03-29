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
    /// Interface para a tabela K_SAC
    /// </summary>
    public partial interface ISac : IEntityBase
    {

        /// <summary>
        /// Cor (COR.)
        /// Opcional = N, Invisível = False
        /// </summary>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("BEF Code Generator", "20.0.90.3")]
        Benner.Tecnologia.Common.ColorField Cor
        {
            get;
            set;
        }

        /// <summary>
        /// Mensagem (MENSAGEM.)
        /// Opcional = N, Invisível = False
        /// </summary>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("BEF Code Generator", "20.0.90.3")]
        string Mensagem
        {
            get;
            set;
        }

        /// <summary>
        /// Numero (NUMERO.)
        /// Opcional = N, Invisível = False, Valor Mínimo = , Valor Máximo = , Tipo do Builder = Inteiro
        /// </summary>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("BEF Code Generator", "20.0.90.3")]
        System.Nullable<long> Numero
        {
            get;
            set;
        }

        /// <summary>
        /// Resposta (RESPOSTA.)
        /// Opcional = S, Invisível = False
        /// </summary>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("BEF Code Generator", "20.0.90.3")]
        string Resposta
        {
            get;
            set;
        }

        /// <summary>
        /// Status (STATUS.)
        /// Opcional = N, Invisível = False
        /// </summary>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("BEF Code Generator", "20.0.90.3")]
        SacStatusListaItens Status
        {
            get;
            set;
        }

        /// <summary>
        /// Título (TITULO.)
        /// Opcional = S, Invisível = False, Tamanho = 120
        /// </summary>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("BEF Code Generator", "20.0.90.3")]
        string Titulo
        {
            get;
            set;
        }



        [System.CodeDom.Compiler.GeneratedCodeAttribute("BEF Code Generator", "20.0.90.3")]
        Handle UsuarioqueenviouHandle
        {
            get;
            set;
        }



        [System.CodeDom.Compiler.GeneratedCodeAttribute("BEF Code Generator", "20.0.90.3")]
        Handle UsuariorespostaHandle
        {
            get;
            set;
        }
    }

    /// <summary>
    /// Interface para o DAO para a tabela K_SAC
    /// </summary>
    public partial interface ISacDao : IBusinessEntityDao<ISac>
    {
    }

    /// <summary>
    /// DAO para a tabela K_SAC
    /// </summary>
    public partial class SacDao : BusinessEntityDao<Sac, ISac>, ISacDao
    {

        public static SacDao CreateInstance()
        {
            return CreateInstance<SacDao>();
        }
    }

    /// <summary>
    /// Esta classe contém os itens do campo STATUS.
    /// </summary>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("BEF Code Generator", "20.0.90.3")]
    public class SacStatusListaItens : ListItems<SacStatusListaItens>
    {

        /// <summary>
        /// Valor = 1, Item = Aguardando resposta.
        /// </summary>
        public static SacStatusListaItens ItemAguardandoResposta;

        /// <summary>
        /// Valor = 2, Item = Respondido.
        /// </summary>
        public static SacStatusListaItens ItemRespondido;

        public static implicit operator SacStatusListaItens(int index)
        {
            return GetByIndex(index);
        }

        public static implicit operator int(SacStatusListaItens item)
        {
            return item.Index;
        }

        static SacStatusListaItens()
        {
            ItemAguardandoResposta = new SacStatusListaItens { Index = 1, Description = "Aguardando resposta" };
            ItemRespondido = new SacStatusListaItens { Index = 2, Description = "Respondido" };

            Items.Add(ItemAguardandoResposta);
            Items.Add(ItemRespondido);

        }
    }

    /// <summary>
    /// Sac
    /// </summary>
    [EntityDefinitionName("K_SAC")]
    [DataContract(Namespace = "http://Benner.Tecnologia.Common.DataContracts/2007/09", Name = "EntityBase")]
    public partial class Sac : BusinessEntity<Sac>, ISac
    {

        /// <summary>
        /// Possui constantes para retornarem o nome dos campos definidos no Builder para cada propriedade
        /// </summary>
        public static class FieldNames
        {
            public const string Cor = "COR";
            public const string Mensagem = "MENSAGEM";
            public const string Numero = "NUMERO";
            public const string Resposta = "RESPOSTA";
            public const string Status = "STATUS";
            public const string Titulo = "TITULO";
            public const string Usuarioqueenviou = "USUARIOENVIO";
            public const string Usuarioresposta = "USUARIORESPOSTA";
        }


        /// <summary>
        /// Cor (COR.)
        /// Opcional = N, Invisível = False
        /// </summary>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("BEF Code Generator", "20.0.90.3")]
        public Benner.Tecnologia.Common.ColorField Cor
        {
            get
            {
                return Fields["COR"] as Benner.Tecnologia.Common.ColorField;
            }
            set
            {
                Fields["COR"] = value;
            }
        }

        /// <summary>
        /// Mensagem (MENSAGEM.)
        /// Opcional = N, Invisível = False
        /// </summary>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("BEF Code Generator", "20.0.90.3")]
        public string Mensagem
        {
            get
            {
                return Fields["MENSAGEM"] as System.String;
            }
            set
            {
                Fields["MENSAGEM"] = value;
            }
        }

        /// <summary>
        /// Numero (NUMERO.)
        /// Opcional = N, Invisível = False, Valor Mínimo = , Valor Máximo = , Tipo do Builder = Inteiro
        /// </summary>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("BEF Code Generator", "20.0.90.3")]
        public System.Nullable<long> Numero
        {
            get
            {
                return Fields["NUMERO"] as System.Nullable<System.Int64>;
            }
            set
            {
                Fields["NUMERO"] = value;
            }
        }

        /// <summary>
        /// Resposta (RESPOSTA.)
        /// Opcional = S, Invisível = False
        /// </summary>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("BEF Code Generator", "20.0.90.3")]
        public string Resposta
        {
            get
            {
                return Fields["RESPOSTA"] as System.String;
            }
            set
            {
                Fields["RESPOSTA"] = value;
            }
        }

        /// <summary>
        /// Status (STATUS.)
        /// Opcional = N, Invisível = False
        /// </summary>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("BEF Code Generator", "20.0.90.3")]
        public SacStatusListaItens Status
        {
            get
            {
                ListItem listItem = Fields["STATUS"] as ListItem;
                if (listItem != null)
                    return new SacStatusListaItens { Index = listItem.Value, Description = listItem.Text };
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

        /// <summary>
        /// Título (TITULO.)
        /// Opcional = S, Invisível = False, Tamanho = 120
        /// </summary>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("BEF Code Generator", "20.0.90.3")]
        public string Titulo
        {
            get
            {
                return Fields["TITULO"] as System.String;
            }
            set
            {
                Fields["TITULO"] = value;
            }
        }

        public Handle UsuarioqueenviouHandle { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public Handle UsuariorespostaHandle { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        /// <summary>
        /// Usuario que enviou (USUARIOENVIO.)
        /// Opcional = S, Invisível = False, Pesquisar = Z_GRUPOUSUARIOS
        /// </summary>








    }
}
