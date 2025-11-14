using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data; // ajuste se namespace do contexto for diferente
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DepartamentosController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DepartamentosController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/departamentos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetAll()
        {
            var departamentos = await _context.Set<Departamento>()
                .AsNoTracking()
                .Select(d => new { d.Id, d.Nome, d.Ativo })
                .ToListAsync();

            var relatoriosLookup = await _context.Set<Relatorio>()
                .AsNoTracking()
                .GroupBy(r => r.DepartamentoId)
                .ToDictionaryAsync(g => g.Key, g => g.Select(r => new { r.Id, r.Titulo, r.Descricao }).ToList());

            var result = departamentos.Select(d => new
            {
                d.Id,
                d.Nome,
                d.Ativo,
                Relatorios = relatoriosLookup
            });

            return Ok(result);
        }

        // GET: api/departamentos/{id}
        [HttpGet("{id:int}")]
        public async Task<ActionResult<object>> GetById(int id)
        {
            var departamento = await _context.Set<Departamento>()
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.Id == id);

            if (departamento == null)
                return NotFound();

            var relatorios = await _context.Set<Relatorio>()
                .AsNoTracking()
                .Where(r => r.DepartamentoId == id)
                .Select(r => new { r.Id, r.Titulo, r.Descricao })
                .ToListAsync();

            return Ok(new
            {
                departamento.Id,
                departamento.Nome,
                departamento.Ativo,
                Relatorios = relatorios
            });
        }

        // POST: api/departamentos
        [HttpPost]
        public async Task<ActionResult<object>> Create([FromBody] DepartamentoCreateDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Nome))
                return BadRequest("Nome é obrigatório.");

            var departamento = new Departamento { Nome = dto.Nome, Ativo = dto.Ativo };

            _context.Add(departamento);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = departamento.Id }, new
            {
                departamento.Id,
                departamento.Nome,
                departamento.Ativo
            });
        }

        // POST: api/departamentos/{id}/relatorios
        [HttpPost("{id:int}/relatorios")]
        public async Task<ActionResult<object>> AddRelatorios(int id, [FromBody] List<RelatorioCreateDto> dtos)
        {
            var departamentoExiste = await _context.Set<Departamento>()
                .AsNoTracking()
                .AnyAsync(d => d.Id == id);

            if (!departamentoExiste)
                return NotFound("Departamento não encontrado.");

            if (dtos == null || dtos.Count == 0)
                return BadRequest("Lista de relatórios vazia.");

            foreach (var dto in dtos)
            {
                if (string.IsNullOrWhiteSpace(dto.Titulo))
                    return BadRequest("Título é obrigatório em todos os relatórios.");

                var relatorio = new Relatorio
                {
                    Titulo = dto.Titulo,
                    Descricao = dto.Descricao,
                    DepartamentoId = id
                };

                _context.Add(relatorio);
            }

            await _context.SaveChangesAsync();

            var relatorios = await _context.Set<Relatorio>()
                .AsNoTracking()
                .Where(r => r.DepartamentoId == id)
                .Select(r => new { r.Id, r.Titulo, r.Descricao })
                .ToListAsync();

            return Ok(new
            {
                Id = id,
                Relatorios = relatorios
            });
        }
    }

    // Relatorios independentes
    [ApiController]
    [Route("api/[controller]")]
    public class RelatoriosController : ControllerBase
    {
        private readonly AppDbContext _context;

        public RelatoriosController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/relatorios
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetAll()
        {
            var relatorios = await _context.Set<Relatorio>()
                .Include(r => r.Departamento)
                .AsNoTracking()
                .Select(r => new
                {
                    r.Id,
                    r.Titulo,
                    r.Descricao,
                    Departamento = r.Departamento == null ? null : new { r.Departamento.Id, r.Departamento.Nome }
                })
                .ToListAsync();

            return Ok(relatorios);
        }

        // GET: api/relatorios/{id}
        [HttpGet("{id:int}")]
        public async Task<ActionResult<object>> GetById(int id)
        {
            var relatorio = await _context.Set<Relatorio>()
                .Include(r => r.Departamento)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (relatorio == null)
                return NotFound();

            return Ok(new
            {
                relatorio.Id,
                relatorio.Titulo,
                relatorio.Descricao,
                Departamento = relatorio.Departamento == null ? null : new { relatorio.Departamento.Id, relatorio.Departamento.Nome }
            });
        }

        // POST: api/relatorios
        [HttpPost]
        public async Task<ActionResult<object>> Create([FromBody] RelatorioCreateDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Titulo))
                return BadRequest("Título é obrigatório.");

            if (!dto.DepartamentoId.HasValue)
                return BadRequest("DepartamentoId é obrigatório.");

            var departamentoExiste = await _context.Set<Departamento>()
                .AsNoTracking()
                .AnyAsync(d => d.Id == dto.DepartamentoId.Value);

            if (!departamentoExiste)
                return BadRequest("Departamento informado não existe.");

            var relatorio = new Relatorio
            {
                Titulo = dto.Titulo,
                Descricao = dto.Descricao,
                DepartamentoId = dto.DepartamentoId.Value
            };

            _context.Add(relatorio);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = relatorio.Id }, new
            {
                relatorio.Id,
                relatorio.Titulo,
                relatorio.Descricao,
                relatorio.DepartamentoId
            });
        }
    }

    // DTOs
    public record DepartamentoCreateDto(string Nome, bool Ativo = true);
    public record RelatorioCreateDto(string Titulo, string? Descricao, int? DepartamentoId);
}