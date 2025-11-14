using System.ComponentModel;

namespace WebApplication1.Models;

public class Solicitacao : BaseEntity
{
    // Campo persistido no banco de dados.
    private int _idStatusSolicitacao;

    // Exposto para a lógica de negócio, não persistido.
    public StatusSolicitacaoEnum Status
    {
        get => (StatusSolicitacaoEnum)_idStatusSolicitacao;
        set => _idStatusSolicitacao = (int)value;
    }

    public PrioridadeEnum Prioridade { get; set; }
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
