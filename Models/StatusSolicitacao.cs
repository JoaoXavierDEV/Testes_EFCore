namespace WebApplication1.Models
{
    public class StatusSolicitacao : BaseEntity
    {
        public string Descricao { get; set; } = string.Empty;
        public virtual ICollection<Solicitacao> Solicitacoes { get; set; } = new HashSet<Solicitacao>();
    }
}
