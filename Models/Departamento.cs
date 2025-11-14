namespace WebApplication1.Models
{
    public class Departamento : BaseEntity
    {
        public string Nome { get; set; }

        public bool Ativo { get; set; } = true;

        public virtual ICollection<DepartamentoRelatorio> DepartamentoRelatorio { get; set; } = new List<DepartamentoRelatorio>();

        public List<Relatorio> ObterRelatorios() => DepartamentoRelatorio.Select(rd => rd.Relatorio).ToList();

    }
}
