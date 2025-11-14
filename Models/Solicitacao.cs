using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace WebApplication1.Models    
{
    public class Solicitacao : BaseEntity
    {
        // Backing fields to be mapped via EF Core Fluent API
        private int _idStatusSolicitacao;
        //private StatusSolicitacao _statusSolicitacao;       

        // Exposto para a lógica de negócio, não persistido.
        public StatusSolicitacaoEnum Status
        {
            get => (StatusSolicitacaoEnum)_idStatusSolicitacao;
            set => _idStatusSolicitacao = (int)value;
        }

        public PrioridadeEnum Prioridade { get; set; }


    }

    public class StatusSolicitacao : BaseEntity
    {
        public string Descricao { get; set; } = string.Empty;
        public virtual ICollection<Solicitacao> Solicitacoes { get; set; } = new HashSet<Solicitacao>();
    }

    public enum StatusSolicitacaoEnum
    {
        Novo = 1,
        EmAndamento,
        Cancelado,
        Pendente,
        Resolvido,
        Finalizado,
        EmAnalise,
        Recusado,
        Reaberto
    }

    public enum PrioridadeEnum
    {
        [Description("Baixa Prioridade")]
        Baixa = 1,
        [Description("Média Prioridade")]
        Media = 2,
        [Description("Alta Prioridade")]
        Alta = 3,
        [Description("Urgente")]
        Urgente = 4

    }

    public class BaseEntity
    {
        public int Id { get; set; }
    }
}
