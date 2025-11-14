using Microsoft.AspNetCore.Mvc;
using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SolicitacaoController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IQueryable<Solicitacao> _dbSet;

        public SolicitacaoController(AppDbContext db)
        {
            _db = db;
            _dbSet = _db.Set<Solicitacao>();
        }

        [HttpGet(Name = "GetSolicitacao")]
        public IEnumerable<Solicitacao> Get()
        {
            var solicitacao1 = new Solicitacao
            {
                Status = StatusSolicitacaoEnum.Resolvido,
                Prioridade = PrioridadeEnum.Alta
            };

            var solicitacao2 = new Solicitacao
            {
                Status = StatusSolicitacaoEnum.Finalizado,
                Prioridade = PrioridadeEnum.Urgente
            };

            _db.AddRange(solicitacao1, solicitacao2);

            _db.SaveChanges();



            var dto = _dbSet.ToList();

            return dto;
        }
    }
}
