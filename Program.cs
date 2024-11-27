using Microsoft.EntityFrameworkCore;
using API.Models;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Configuração do CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(
        policy =>
        {
            policy.WithOrigins("http://localhost:3000")
                   .AllowAnyHeader()
                   .AllowAnyMethod();
        });
});

// Configuração do DbContext
builder.Services.AddDbContext<AppDataContext>();

var app = builder.Build();

// Habilitar CORS
app.UseCors();

// Página de exceções do desenvolvedor (útil para depuração)
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.MapGet("/", () => "Prova A1");

// ENDPOINTS DE CATEGORIA

// GET: http://localhost:5273/api/categoria/listar
app.MapGet("/api/categoria/listar", async ([FromServices] AppDataContext ctx) =>
{
    var categorias = await ctx.Categorias.ToListAsync();
    return categorias.Any()
        ? Results.Ok(categorias)
        : Results.NotFound("Nenhuma categoria encontrada");
});

// POST: http://localhost:5273/api/categoria/cadastrar
app.MapPost("/api/categoria/cadastrar", async ([FromServices] AppDataContext ctx, [FromBody] Categoria categoria) =>
{
    await ctx.Categorias.AddAsync(categoria);
    await ctx.SaveChangesAsync();
    return Results.Created($"/api/categoria/{categoria.CategoriaId}", categoria);
});

// ENDPOINTS DE TAREFA

// GET: http://localhost:5273/api/tarefas/listar
app.MapGet("/api/tarefas/listar", async ([FromServices] AppDataContext ctx) =>
{
    var tarefas = await ctx.Tarefas.ToListAsync();
    return tarefas.Any()
        ? Results.Ok(tarefas)
        : Results.NotFound("Nenhuma tarefa encontrada");
});

// POST: http://localhost:5273/api/tarefas/cadastrar
app.MapPost("/api/tarefas/cadastrar", async ([FromServices] AppDataContext ctx, [FromBody] Tarefa tarefa) =>
{
    try
    {
        if (string.IsNullOrEmpty(tarefa.Titulo) || string.IsNullOrEmpty(tarefa.Descricao))
        {
            return Results.BadRequest("Título e descrição são obrigatórios.");
        }

        tarefa.Status = "Não iniciada";
        tarefa.CriadoEm = DateTime.Now;

        await ctx.Tarefas.AddAsync(tarefa);
        await ctx.SaveChangesAsync();

        return Results.Created($"/api/tarefas/{tarefa.TarefaId}", tarefa);
    }
    catch (Exception ex)
    {
        return Results.StatusCode(500);
    }
});

// PUT: http://localhost:5273/api/tarefas/alterar/{id}
app.MapPut("/api/tarefas/alterar/{id}", async ([FromServices] AppDataContext ctx, [FromRoute] string id, [FromBody] Tarefa tarefaAtualizada) =>
{
    var tarefa = await ctx.Tarefas.FindAsync(id);
    if (tarefa == null)
    {
        return Results.NotFound("Tarefa não encontrada");
    }

    tarefa.Titulo = tarefaAtualizada.Titulo;
    tarefa.Descricao = tarefaAtualizada.Descricao;
    tarefa.Status = tarefaAtualizada.Status;

    await ctx.SaveChangesAsync();
    return Results.NoContent();
});

// GET: http://localhost:5273/api/tarefas/naoconcluidas
app.MapGet("/api/tarefas/naoconcluidas", async ([FromServices] AppDataContext ctx) =>
{
    var tarefasNaoConcluidas = await ctx.Tarefas
        .Where(t => t.Status != "Concluída")
        .ToListAsync();

    return tarefasNaoConcluidas.Any()
        ? Results.Ok(tarefasNaoConcluidas)
        : Results.NotFound("Nenhuma tarefa não concluída encontrada");
});

// GET: http://localhost:5273/api/tarefas/concluidas
app.MapGet("/api/tarefas/concluidas", async ([FromServices] AppDataContext ctx) =>
{
    var tarefasConcluidas = await ctx.Tarefas
        .Where(t => t.Status == "Concluída")
        .ToListAsync();

    return tarefasConcluidas.Any()
        ? Results.Ok(tarefasConcluidas)
        : Results.NotFound("Nenhuma tarefa concluída encontrada");
});

app.Run();