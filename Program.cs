using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Task = TaskHandler.Task; // Use alias to avoid confusion with System.Threading.Tasks

// In-memory task storage (acts like a temporary database)
var tasks = new List<Task>();

// Create the builder and configure services
var builder = WebApplication.CreateBuilder(args);

// Register services with the Dependency Injection container
builder.Services.AddEndpointsApiExplorer(); // Enables minimal API discovery for Swagger
builder.Services.AddSwaggerGen();          // Adds Swagger/OpenAPI support

// Build the application pipeline
var app = builder.Build();

// Define a route group for versioned task API endpoints
var taskApi = app.MapGroup("/api/v1/tasks").WithTags("Tasks API");

// --- Example Task Objects for Swagger/OpenAPI documentation ---

var exampleTask = new OpenApiObject
{
    ["id"] = new OpenApiString("d3b9f0f5-e2c1-4a7e-a2cf-72e578a6e7df"),
    ["title"] = new OpenApiString("Buy groceries"),
    ["description"] = new OpenApiString("Milk, Bread, Eggs"),
    ["isCompleted"] = new OpenApiBoolean(false)
};

var anotherExampleTask = new OpenApiObject
{
    ["id"] = new OpenApiString("5a1e2b3f-6c4d-7890-1234-abcdefabcdef"),
    ["title"] = new OpenApiString("Finish assignment"),
    ["description"] = new OpenApiString("Complete the Swagger/OpenAPI docs task"),
    ["isCompleted"] = new OpenApiBoolean(true)
};

// --- POST: Create a new task ---
taskApi.MapPost("/", (Task task) =>
{
    task.Id = Guid.NewGuid();  // Assign a unique ID
    tasks.Add(task);           // Add to the list
    return Results.Created($"/api/v1/tasks/{task.Id}", task); // Return 201 Created
})
.Produces<Task>(201) // Specifies that the endpoint returns a Task object with 201 status
.WithOpenApi(op =>
{
    op.Summary = "Create a new task";
    op.Description = "Creates a new task with a unique ID.";
    op.RequestBody = new OpenApiRequestBody
    {
        Content = {
            ["application/json"] = new OpenApiMediaType
            {
                Example = exampleTask // Example request body
            }
        }
    };
    op.Responses["201"] = new OpenApiResponse
    {
        Description = "Task created successfully.",
        Content = {
            ["application/json"] = new OpenApiMediaType
            {
                Example = exampleTask // Example response body
            }
        }
    };
    return op;
});

// --- GET: Retrieve all tasks ---
taskApi.MapGet("/", () => Results.Ok(tasks))
.Produces<List<Task>>(200)
.WithOpenApi(op =>
{
    op.Summary = "Retrieve all tasks";
    op.Description = "Returns a list of all existing tasks.";
    op.Responses["200"] = new OpenApiResponse
    {
        Description = "List of tasks",
        Content = {
            ["application/json"] = new OpenApiMediaType
            {
                Example = new OpenApiArray { exampleTask, anotherExampleTask } // Multiple examples
            }
        }
    };
    return op;
});

// --- GET: Retrieve a single task by ID ---
taskApi.MapGet("/{id}", (Guid id) =>
{
    var task = tasks.FirstOrDefault(t => t.Id == id);
    return task is not null ? Results.Ok(task) : Results.NotFound();
})
.Produces<Task>(200)
.Produces(404)
.WithOpenApi(op =>
{
    op.Summary = "Get a task by ID";
    op.Description = "Returns a specific task by its ID.";
    op.Parameters[0].Description = "Unique task ID (GUID)";
    op.Responses["200"] = new OpenApiResponse
    {
        Description = "Task found",
        Content = {
            ["application/json"] = new OpenApiMediaType
            {
                Example = exampleTask
            }
        }
    };
    op.Responses["404"] = new OpenApiResponse { Description = "Task not found" };
    return op;
});

// --- PUT: Update an existing task by ID ---
taskApi.MapPut("/{id}", (Guid id, Task updatedTask) =>
{
    var task = tasks.FirstOrDefault(t => t.Id == id);
    if (task is null) return Results.NotFound();

    // Input validation
    if (string.IsNullOrWhiteSpace(updatedTask.Title))
        return Results.BadRequest("Title is required.");

    // Update task fields
    task.Title = updatedTask.Title;
    task.IsCompleted = updatedTask.IsCompleted;
    task.Description = updatedTask.Description;

    return Results.Ok(task); // Return updated task
})
.Produces<Task>(200)
.Produces(400)
.Produces(404)
.WithOpenApi(op =>
{
    op.Summary = "Update an existing task";
    op.Description = "Updates a task with the specified ID.";
    op.RequestBody = new OpenApiRequestBody
    {
        Content = {
            ["application/json"] = new OpenApiMediaType
            {
                Example = exampleTask // Example request body
            }
        }
    };
    op.Parameters[0].Description = "ID of the task to update";
    op.Responses["200"] = new OpenApiResponse
    {
        Description = "Updated task",
        Content = {
            ["application/json"] = new OpenApiMediaType
            {
                Example = exampleTask // Example response
            }
        }
    };
    op.Responses["400"] = new OpenApiResponse { Description = "Invalid input" };
    op.Responses["404"] = new OpenApiResponse { Description = "Task not found" };
    return op;
});

// --- DELETE: Remove a task by ID ---
taskApi.MapDelete("/{id}", (Guid id) =>
{
    var task = tasks.FirstOrDefault(t => t.Id == id);
    if (task is null) return Results.NotFound();

    tasks.Remove(task); // Remove from the list
    return Results.NoContent(); // Return 204 No Content
})
.Produces(204)
.Produces(404)
.WithOpenApi(op =>
{
    op.Summary = "Delete a task";
    op.Description = "Deletes a task by its unique ID.";
    op.Parameters[0].Description = "ID of the task to delete";
    op.Responses["204"] = new OpenApiResponse { Description = "Task deleted" };
    op.Responses["404"] = new OpenApiResponse { Description = "Task not found" };
    return op;
});

// Enable Swagger only in development mode
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();    // Generate Swagger JSON
    app.UseSwaggerUI();  // Serve Swagger UI
}

// Run the application
app.Run();
