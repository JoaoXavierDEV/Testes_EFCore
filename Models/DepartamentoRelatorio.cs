namespace WebApplication1.Models
{
    public class DepartamentoRelatorio : BaseEntity
    {
        public int RelatorioId { get; set; }
        public Relatorio Relatorio { get; set; }
        public int DepartamentoId { get; set; }
        public Departamento Departamento { get; set; }
    }
}