using CarvedRock.Data.DatabaseContext;
using CarvedRock.Data.Repositories;
using CarvedRock.Domain.Mappings;
using CarvedRock.Domain.Services;
using CarvedRock.Domain.Validators;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Enrichers.Span;
using Serilog.Exceptions;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddHealthChecks()
        .AddDbContextCheck<CarvedRockContext>();

builder.Logging.ClearProviders();

builder.Host.UseSerilog((context, loggerConfig) =>
{
    loggerConfig.ReadFrom.Configuration(context.Configuration)
        .Enrich.WithExceptionDetails()
        .Enrich.FromLogContext()
        .Enrich.With<ActivityEnricher>()
        .WriteTo.Console();
       // .WriteTo.Seq(context.Configuration["seqUrl"]);
});

builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = (ctx) =>
    {
        if (!ctx.ProblemDetails.Extensions.ContainsKey("traceId"))
        {
            string? traceId = Activity.Current?.Id ?? ctx.HttpContext.TraceIdentifier;
            ctx.ProblemDetails.Extensions.Add(new KeyValuePair<string, object?>("traceId", traceId));
        }
        var exception = ctx.HttpContext.Features.Get<IExceptionHandlerFeature>()?.Error;
        if (ctx.ProblemDetails.Status == 500)
        {
            ctx.ProblemDetails.Detail = "An error occurred in our API. Use the trace id when contacting us.";
        }
    };
});

//Configure Identity Server for JWT Bearer Token
JwtSecurityTokenHandler.DefaultMapInboundClaims = false;
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
    {
        options.Authority = builder.Configuration["IdentityProvider.Authority"];
        options.Audience = builder.Configuration["IdentityProvider.Audience"];
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            NameClaimType = "email",
            RoleClaimType = "role"
        };
    });

//Add SQL Server DB Context
builder.Services.AddDbContext<CarvedRockContext>(options => {
    options.UseSqlServer(builder.Configuration.GetConnectionString("CarvedRockSqlServer"));
    options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
});


//Configure dependency injection
builder.Services.AddScoped<ICarvedRockRepository, CarvedRockRepository>();
builder.Services.AddScoped<IProductService, ProductService>();

//Configure auto mapper
builder.Services.AddAutoMapper(typeof(ProductMappingProfile));

//Configure Fluent validations
builder.Services.AddValidatorsFromAssemblyContaining<NewProductValidator>();


var app = builder.Build();

app.UseSerilogRequestLogging(options =>
{
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        diagnosticContext.Set("client_id", httpContext.User.Claims.FirstOrDefault(c => c.Type == "client_id")?.Value);
    };
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    SetupDevelopment();   
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();


void SetupDevelopment()
{
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        var context = services.GetRequiredService<CarvedRockContext>();
        context.MigrateAndCreateData();
    }
    app.MapOpenApi();
}
