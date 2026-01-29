using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using System;
using System.Reflection;
using Tasks.Data;
using Tasks.Data.Access;
using Tasks.Data.Access.Extensions;
using Tasks.Services.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<TasksDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Default")));

// Add memory cache for TaskRepository
builder.Services.AddMemoryCache();

// Register data access layer
builder.Services.AddTaskDataAccess();

// Register service layer
builder.Services.AddTaskServices();

builder.Services.AddControllers(options =>
{
    // If true, causes the framework to strip "Async" from action names when matching routes
    options.SuppressAsyncSuffixInActionNames = false;
})
.AddJsonOptions(options =>
{
    // The JsonStringEnumConverter controls how enum values are serialized/deserialized in JSON.
    // Without it, enums would be represented as integers by default.
    //  - The meaning of the integer value is unclear to the reader.
    //  - The numbers would need to be mapped to the meaning.
    //  - Harder to interpret.
    // With it, enums are represented as their string names, improving readability and interoperability.
    // This makes the API self-documenting and less error-prone for consumers
    options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
});

builder.Services.AddEndpointsApiExplorer();

// Enhanced Swagger configuration
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "Tasks API",
        Description = "A REST API for task management operations",
        Contact = new OpenApiContact
        {
            Name = "API Support",
            Email = "support@example.com"
        }
    });

    // Include XML comments from controller and models assemblies
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }

    // Include XML comments from Tasks.Models assembly
    var modelsXmlPath = Path.Combine(AppContext.BaseDirectory, "Tasks.Models.xml");
    if (File.Exists(modelsXmlPath))
    {
        options.IncludeXmlComments(modelsXmlPath);
    }
});

// Add CORS for development
// Cross-Origin Resource Sharing is a browser security feature that blocks web pages from making
// requests to a different domain/port than the one that served the page.
// Without this, we will get the error:
// Access to fetch at 'https://localhost:7001/api/tasks' (ASP.NET API) from origin 
// 'http://localhost:5173' (Vue App) has been blocked by CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("DevCors", policy =>
    {
        policy.WithOrigins("http://localhost:5173") // Vite dev server
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Tasks API v1");
        options.DisplayRequestDuration();
    });

    app.UseCors("DevCors"); // Enable CORS in development
}

// Only redirect in production
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// Serve static files from Vue build
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseAuthorization();

app.MapControllers();

// Fallback to index.html for SPA routing
app.MapFallbackToFile("index.html");

app.Run();
