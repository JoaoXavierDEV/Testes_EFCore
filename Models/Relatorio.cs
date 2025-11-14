namespace WebApplication1.Models;

public class Relatorio : BaseEntity
{
    public string Titulo { get; set; } = string.Empty;
    public string? Descricao { get; set; } = string.Empty;

    // Chave estrangeira
    public int DepartamentoId { get; set; }

    // Propriedade de navegação (opcional, mas recomendada)
    public Departamento Departamento { get; set; }


    public virtual ICollection<DepartamentoRelatorio> DepartamentoRelatorio { get; private set; } = new List<DepartamentoRelatorio>();




    /// <summary>
    /// Retorna a lista de departamentos vinculados ao relatório.
    /// </summary>
    /// <remarks>
    /// Para adicionar departamentos utilize o método <br></br>
    /// <see cref="Relatorio.AdicionarDepartamento(Departamento)"/> 
    /// </remarks>
    /// <returns>Lista de Departamentos </returns>
    public List<Departamento> ObterDepartamentos() => DepartamentoRelatorio.Select(rd => rd.Departamento).ToList();

    public Relatorio AdicionarDepartamento(Departamento dp)
    {
        if (!DepartamentoRelatorio.Any(dr => dr.DepartamentoId == dp.Id))
        {
            var dr = new DepartamentoRelatorio
            {
                DepartamentoId = dp.Id,
                RelatorioId = this.Id
            };

            DepartamentoRelatorio.Add(dr);
        }

        return this;
    }

    public Relatorio RemoverDepartamento(Departamento dp)
    {
        if (DepartamentoRelatorio.Any(dr => dr.DepartamentoId == dp.Id))
        {
            var dr = DepartamentoRelatorio.First(dr => dr.DepartamentoId == dp.Id);

            DepartamentoRelatorio.Remove(dr);
        }
        return this;
    }

    public Relatorio ClearDepartamentos()
    {
        DepartamentoRelatorio.Clear();
        return this;
    }
}
