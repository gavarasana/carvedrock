using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Enrichers.Span;
using Serilog.Exceptions;
using System.IdentityModel.Tokens.Jwt;

var builder = WebApplication.CreateBuilder(args);

// Clear all logging providers
builder.Logging.ClearProviders();

//Configure Serilog
builder.Host.UseSerilog((context, loggerConfig) =>
    {
        loggerConfig
            .ReadFrom.Configuration(context.Configuration)
            .WriteTo.Console()
            .Enrich.WithExceptionDetails()
            .Enrich.With<ActivityEnricher>();

        var seqUrl = context.Configuration["Seq:Url"];
        if (string.IsNullOrEmpty(seqUrl) && Uri.IsWellFormedUriString(seqUrl, UriKind.Absolute))
        {
            loggerConfig.WriteTo.Seq(seqUrl);
        }
    });

//Clear out legacy inbound claims mapping
JwtSecurityTokenHandler.DefaultMapInboundClaims = false;

//Configure Identity Provider
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
})
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options => options.AccessDeniedPath = "/AccessDenied")
    .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
    {
        options.SignInScheme = OpenIdConnectDefaults.AuthenticationScheme;
        options.Authority = builder.Configuration["IdentityProvider:Authority"];
        options.ClientId = builder.Configuration["IdentityProvider:ClientId"];
        options.ClientSecret = builder.Configuration["IdentityProvider:ClientSecret"];
        options.ResponseType = "code";
        options.Scope.Add("openid");
        options.Scope.Add("profile");
        options.Scope.Add("email");
        options.Scope.Add("role");
        options.Scope.Add("carvedrockapi");
        options.Scope.Add("offline_access");
        options.GetClaimsFromUserInfoEndpoint = true;
        options.ClaimActions.MapJsonKey("role", "role", "role");
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            NameClaimType = "email",
            RoleClaimType = "role"
        };
        options.SaveTokens = true;
    });

// Configure HttpContextAccessor
builder.Services.AddHttpContextAccessor();

//Add health checks
builder.Services.AddHealthChecks();

//Add Http Client
builder.Services.AddHttpClient();

// Add services to the container.
builder.Services.AddRazorPages();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();

}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.MapHealthChecks("health").AllowAnonymous();

app.Run();
