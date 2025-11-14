Meu nome é GitHub Copilot.

```markdown
# Testes de Persistência de Enum e Relacionamentos N:N com EF Core

![Formas de persistir Enum](https://private-user-images.githubusercontent.com/40217599/514603826-443c4f3d-9737-401c-b73d-4d2b2e0ef841.png)

## Objetivo
Demonstrar diferentes estratégias para persistir valores de `enum` no banco de dados usando Entity Framework Core e abordar modelos de relacionamento muitos-para-muitos (N:N), incluindo armadilhas de cascata.

---

## 1. Modelo de Domínio Usado

### Enum de Status (persistido via tabela de lookup)
```
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
```

### Classe de domínio usando shadow property
```
public class Solicitacao : BaseEntity
{
    private int _idStatusSolicitacao; // shadow property mapeado em OnModelCreating

    public StatusSolicitacaoEnum Status
    {
        get => (StatusSolicitacaoEnum)_idStatusSolicitacao;
        set => _idStatusSolicitacao = (int)value;
    }

    public PrioridadeEnum Prioridade { get; set; } // diferentes formas de persistir
}
```

---

## 2. Formas de Persistir Enums

### 2.1. Padrão (int)
Sem configuração adicional:
```
builder.Entity<Solicitacao>()
       .Property(x => x.Prioridade); // Armazena como int (valor numérico do enum)
```
Vantagens: simples, performático.  
Desvantagens: pouco legível em consultas SQL.

### 2.2. Como string (nome do enum)
```
solicitacao.Property(x => x.Prioridade)
           .HasConversion<string>()    // salva "Baixa", "Media", ...
           .HasMaxLength(20)
           .IsRequired();
```
Vantagens: legível.  
Desvantagens: renomear membros quebra histórico; ocupa mais espaço.

### 2.3. Usando Description Attribute
```
var prioridadeConverter = new ValueConverter<PrioridadeEnum, string>(
    v => EnumExtensions.GetEnumDescription(v),
    v => EnumExtensions.ParseFromDescription<PrioridadeEnum>(v)
);

solicitacao.Property(x => x.Prioridade)
           .HasConversion(prioridadeConverter)
           .HasMaxLength(50)
           .IsRequired();
```
Vantagens: texto amigável.  
Desvantagens: parsing exige cuidado (possível exceção se texto não existir).

### 2.4. Tabela de Lookup (Status)
Cria tabela `StatusSolicitacao` sem depender de converter:
```
status.Property(s => s.Id).ValueGeneratedNever();
status.HasData(
    new StatusSolicitacao { Id = 1, Descricao = "Novo" }, /* ... */
);
solicitacao.Property<int>("_idStatusSolicitacao")
           .HasColumnName("IdStatusSolicitacao")
           .IsRequired();

solicitacao.HasOne<StatusSolicitacao>()
           .WithMany(s => s.Solicitacoes)
           .HasForeignKey("_idStatusSolicitacao")
           .OnDelete(DeleteBehavior.Restrict);
```
Vantagens: integridade referencial, fácil expandir para metadados adicionais.  
Desvantagens: join adicional em queries.

### 2.5. Shadow Property + Encapsulamento
O campo persistido (`_idStatusSolicitacao`) não aparece publicamente.  
Vantagens: controle de invariantes.  
Desvantagens: exige clareza e testes.

---

## 3. Many-to-Many (N:N)

### 3.1. Many-to-Many explícito com entidade de junção
Entidade:
```
public class DepartamentoRelatorio : BaseEntity
{
    public int RelatorioId { get; set; }
    public Relatorio Relatorio { get; set; }
    public int DepartamentoId { get; set; }
    public Departamento Departamento { get; set; }
}
```

Configuração:
```
modelBuilder.Entity<DepartamentoRelatorio>(entity =>
{
    entity.HasKey(rd => rd.Id);

    entity.HasOne(rd => rd.Relatorio)
          .WithMany(r => r.DepartamentoRelatorio)
          .HasForeignKey(rd => rd.RelatorioId)
          .OnDelete(DeleteBehavior.Restrict);

    entity.HasOne(rd => rd.Departamento)
          .WithMany(d => d.DepartamentoRelatorio)
          .HasForeignKey(rd => rd.DepartamentoId)
          .OnDelete(DeleteBehavior.Restrict);
});
```

Uso:
```
var relatorio = _context.Relatorios.Find(idRelatorio);
var departamento = _context.Departamentos.Find(idDepartamento);

relatorio.AdicionarDepartamento(departamento);
_context.SaveChanges();
```

Vantagens:
- Possibilidade de atributos adicionais na relação (ex.: DataVinculo).
- Controle explícito de cascatas.

### 3.2. Mistura de 1:N + N:N (armadilha)
Se existir simultaneamente:
- FK direta `Relatorio.DepartamentoId`
- Join `DepartamentoRelatorio`

O SQL Server pode detectar múltiplos caminhos em cascata para deleção (erro encontrado no projeto).  
Correção: usar `DeleteBehavior.Restrict` ou remover o relacionamento redundante.

### 3.3. Many-to-Many implícito (Skip Navigations)
Alternativo (não usado aqui):
```
public class Relatorio
{
    public ICollection<Departamento> Departamentos { get; set; } = new();
}

public class Departamento
{
    public ICollection<Relatorio> Relatorios { get; set; } = new();
}

// EF Core (>=5):
modelBuilder.Entity<Relatorio>()
    .HasMany(r => r.Departamentos)
    .WithMany(d => d.Relatorios)
    .UsingEntity(j => j.ToTable("DepartamentoRelatorio"));
```
Vantagens: menos código.  
Desvantagens: difícil adicionar campos extras; mais mágica.

---

## 4. Evitando Ciclos de Cascade
Erro original:
```
A introdução da restrição FOREIGN KEY ... pode causar ciclos ou vários caminhos em cascata.
```
Causa: múltiplas rotas de deleção atingindo a tabela de junção.  
Mitigação:
- Tornar deletes restritos (`DeleteBehavior.Restrict` / `NoAction`)
- Remover FK direta se não for necessária
- Definir deleções lógicas (flag `Ativo`) ao invés de cascata física.

---

## 5. Exemplos de Query

Buscar solicitações com status (lookup):
```
var lista = await _context.Solicitacoes
    .Select(s => new {
        s.Id,
        Status = s.Status.ToString(), // enum em memória
    })
    .ToListAsync();
```

Buscar relatórios por departamento (N:N):
```
var relatorios = await _context.Relatorios
    .Where(r => r.DepartamentoRelatorio.Any(dr => dr.DepartamentoId == idDepto))
    .ToListAsync();
```

---

## 6. Boas Práticas

- Seja explícito no `OnDelete` para evitar surpresas.
- Para enums raramente mutáveis → int ou lookup.
- Para exibição direta em relatórios → string ou description.
- Evite misturar vários padrões para o mesmo enum.
- Teste conversões com `ValueConverter` incluindo casos inválidos.
- Documente semântica de cada relação (1:N vs N:N) para evitar redundância.

---

## 7. Próximos Passos (Sugestões)

- Adicionar migrações ao invés de `EnsureCreated()`
- Implementar testes cobrindo conversão de enum com descrição
- Criar serviço para cache de tabela de lookup de status
- Avaliar remoção da FK direta `Relatorio.DepartamentoId` se N:N for o objetivo principal

---

## 8. Referências

- EF Core Docs: Value Conversions
- EF Core Docs: Relationships (Cascade Delete)
- Padrão Lookup Table para enums ricos

---

## 9. Resumo Rápido

| Estratégia Enum | Armazenamento | Uso | Trade-off |
|-----------------|---------------|-----|-----------|
| Int padrão | número | Simples | Pouca legibilidade |
| String | texto do nome | Leitura fácil | Renomear quebra histórico |
| Description Attribute | texto amigável | UX melhor | Conversão customizada |
| Lookup Table | FK | Flexível/expandível | Join extra |
| Shadow Property | encapsulado | Domínio limpo | Configuração extra |

---

Contribuições e melhorias são bem-vindas. Abra uma issue para sugestões.

```