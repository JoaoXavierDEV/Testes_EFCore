using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.ComponentModel;
using System.Reflection;
using System.Linq;
using WebApplication1.Extensions;

namespace WebApplication1.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
            Database.EnsureDeleted();
            Database.EnsureCreated();
        }

        public DbSet<Solicitacao> Solicitacoes => Set<Solicitacao>();
        public DbSet<StatusSolicitacao> Statuses => Set<StatusSolicitacao>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            var status = modelBuilder.Entity<StatusSolicitacao>();
            status.ToTable("StatusSolicitacao");
            status.HasKey(s => s.Id);
            status.Property(s => s.Id).ValueGeneratedNever();
            status.Property(s => s.Descricao).IsRequired().HasMaxLength(200);

            // Seed lookup to keep IDs aligned with enum values
            status.HasData(
                new StatusSolicitacao { Id = (int)StatusSolicitacaoEnum.Novo, Descricao = nameof(StatusSolicitacaoEnum.Novo) },
                new StatusSolicitacao { Id = (int)StatusSolicitacaoEnum.EmAndamento, Descricao = nameof(StatusSolicitacaoEnum.EmAndamento) },
                new StatusSolicitacao { Id = (int)StatusSolicitacaoEnum.Cancelado, Descricao = nameof(StatusSolicitacaoEnum.Cancelado) },
                new StatusSolicitacao { Id = (int)StatusSolicitacaoEnum.Pendente, Descricao = nameof(StatusSolicitacaoEnum.Pendente) },
                new StatusSolicitacao { Id = (int)StatusSolicitacaoEnum.Resolvido, Descricao = nameof(StatusSolicitacaoEnum.Resolvido) },
                new StatusSolicitacao { Id = (int)StatusSolicitacaoEnum.Finalizado, Descricao = nameof(StatusSolicitacaoEnum.Finalizado) },
                new StatusSolicitacao { Id = (int)StatusSolicitacaoEnum.EmAnalise, Descricao = nameof(StatusSolicitacaoEnum.EmAnalise) },
                new StatusSolicitacao { Id = (int)StatusSolicitacaoEnum.Recusado, Descricao = nameof(StatusSolicitacaoEnum.Recusado) },
                new StatusSolicitacao { Id = (int)StatusSolicitacaoEnum.Reaberto, Descricao = nameof(StatusSolicitacaoEnum.Reaberto) }
            );

            var solicitacao = modelBuilder.Entity<Solicitacao>();
            solicitacao.ToTable("Solicitacao");
            solicitacao.HasKey(s => s.Id);
            solicitacao.Property(s => s.Id)
                       .ValueGeneratedOnAdd();

            // FK usando shadow property
            solicitacao.Property<int>("_idStatusSolicitacao")
                       .HasColumnName("IdStatusSolicitacao")
                       .IsRequired();

            solicitacao.HasOne<StatusSolicitacao>()
                       .WithMany(s => s.Solicitacoes)
                       .HasForeignKey("_idStatusSolicitacao")
                       .OnDelete(DeleteBehavior.Restrict);

            solicitacao.Ignore(x => x.Status);

            // Converter para persistir a Description do enum no banco
            //var prioridadeConverter = new ValueConverter<PrioridadeEnum, string>(
            //    v => EnumExtensions.GetEnumDescription(v),
            //    v => EnumExtensions.ParseFromDescription<PrioridadeEnum>(v)
            //);

            //solicitacao.Property(x => x.Prioridade)
            //           .HasConversion(prioridadeConverter) // salva "Baixa Prioridade", "Média Prioridade", etc.
            //           .HasMaxLength(50)
            //           .IsRequired();

            solicitacao.Property(x => x.Prioridade)
                       .HasConversion<string>()       // persiste o nome do enum (ex.: "Baixa")
                       .HasMaxLength(20)              // opcional: tamanho da coluna
                       .IsRequired();

            // funciona
            solicitacao.HasOne<StatusSolicitacao>()
                       .WithMany(s => s.Solicitacoes)
                       .HasForeignKey("_idStatusSolicitacao")
                       .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
