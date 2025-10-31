using Microsoft.EntityFrameworkCore;
using TodoApi.Data;
using TodoApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Services
builder.Services.AddDbContext<TodoContext>(opt =>
    opt.UseSqlite("Data Source=todos.db"));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Middleware
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "To-Do API v1");
    c.RoutePrefix = "swagger";
});

app.UseHttpsRedirection();

// API Group
var todoItems = app.MapGroup("/api/todo").WithTags("To-Do");

// GET all
todoItems.MapGet("/", async (TodoContext db) =>
    await db.Todos.ToListAsync())
    .WithName("GetAllTodos");

// GET by ID
todoItems.MapGet("/{id:int}", async (int id, TodoContext db) =>
    await db.Todos.FindAsync(id) is TodoItem todo
        ? Results.Ok(todo)
        : Results.NotFound())
    .WithName("GetTodoById");

// POST
todoItems.MapPost("/", async (TodoItem todo, TodoContext db) =>
{
    db.Todos.Add(todo);
    await db.SaveChangesAsync();
    return Results.Created($"/api/todo/{todo.Id}", todo);
})
.WithName("CreateTodo");

// PUT
todoItems.MapPut("/{id:int}", async (int id, TodoItem input, TodoContext db) =>
{
    var todo = await db.Todos.FindAsync(id);
    if (todo is null) return Results.NotFound();

    todo.Title = input.Title;
    todo.IsCompleted = input.IsCompleted;
    await db.SaveChangesAsync();
    return Results.NoContent();
})
.WithName("UpdateTodo");

// DELETE
todoItems.MapDelete("/{id:int}", async (int id, TodoContext db) =>
{
    if (await db.Todos.FindAsync(id) is TodoItem todo)
    {
        db.Todos.Remove(todo);
        await db.SaveChangesAsync();
        return Results.Ok(todo);
    }
    return Results.NotFound();
})
.WithName("DeleteTodo");

app.Run();