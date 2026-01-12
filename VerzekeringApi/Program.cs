
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using VerzekeringApi.Data;

var builder = WebApplication.CreateBuilder(args);

// EF Core SQLite
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")
        ?? "Data Source=verzekeringen.db"));

builder.Services.AddControllers();          // Controllers i.p.v. minimal endpoints
builder.Services.AddEndpointsApiExplorer(); // Swagger

builder.Services.AddSwaggerGen(c =>
{
    // Basic info
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Verzekering API",
        Version = "v1",
        Description = "REST API voor Klanten en Opstalverzekeringen (SQLite + EF Core)"
    });

    // XML comments (for controller/action summaries)
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = System.IO.Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (System.IO.File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
    }

    // Optional: group actions by controller
    c.TagActionsBy(api =>
    {
        return new[] { api.GroupName ?? api.HttpMethod ?? "default" };
    });

    // Optional: show enums as strings
    // c.UseAllOfToExtendReferenceSchemas();
});


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers(); // Activeer attribute-routed controllers

app.Run();
