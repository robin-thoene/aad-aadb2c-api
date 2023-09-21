using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Identity.Web;
using Microsoft.Net.Http.Headers;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Configuration.AddEnvironmentVariables();
builder.Services.AddAuthentication().AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AAD"), jwtBearerScheme: "AAD");
builder.Services.AddAuthentication().AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AADB2C"), jwtBearerScheme: "B2C");
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AAD", new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .AddAuthenticationSchemes("AAD")
        .Build());
    options.AddPolicy("B2C", new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .AddAuthenticationSchemes("B2C")
        .Build());
});
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityRequirement(
                        new OpenApiSecurityRequirement
                        {
                            {
                                new OpenApiSecurityScheme
                                {
                                    Name = nameof(SecuritySchemeType.OAuth2),
                                    Reference = new OpenApiReference
                                    {
                                        Type = ReferenceType.SecurityScheme,
                                        Id = "AAD"
                                    }
                                },
                                new List<string>()
                            }
                        });
    options.AddSecurityRequirement(
    new OpenApiSecurityRequirement
    {
                            {
                                new OpenApiSecurityScheme
                                {
                                    Name = nameof(SecuritySchemeType.OAuth2),
                                    Reference = new OpenApiReference
                                    {
                                        Type = ReferenceType.SecurityScheme,
                                        Id = "B2C"
                                    }
                                },
                                new List<string>()
                            }
    });
    options.AddSecurityDefinition("AAD", new OpenApiSecurityScheme
    {
        Name = HeaderNames.Authorization,
        Type = SecuritySchemeType.OAuth2,
        Scheme = "AAD",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Flows = new OpenApiOAuthFlows
        {
            Implicit = new OpenApiOAuthFlow
            {
                AuthorizationUrl = new Uri(
                                        $"{builder.Configuration["AAD:Instance"]}/{builder.Configuration["AAD:Domain"]}/oauth2/v2.0/authorize"),
                Scopes = new Dictionary<string, string>
                                    {
                                        { builder.Configuration["AAD:Scope"] ?? "", builder.Configuration["AAD:Scope"] ?? "" }
                                    }
            }
        },
    });
    options.AddSecurityDefinition("B2C", new OpenApiSecurityScheme
    {
        Name = HeaderNames.Authorization,
        Type = SecuritySchemeType.OAuth2,
        Scheme = "B2C",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Flows = new OpenApiOAuthFlows
        {
            Implicit = new OpenApiOAuthFlow
            {
                AuthorizationUrl = new Uri(
                                        $"{builder.Configuration["AADB2C:Instance"]}/{builder.Configuration["AADB2C:Domain"]}/oauth2/v2.0/authorize?p={builder.Configuration["AADB2C:SignUpSignInPolicyId"]}"),
                Scopes = new Dictionary<string, string>
                                    {
                                        { builder.Configuration["AADB2C:Scope"] ?? "", builder.Configuration["AADB2C:Scope"] ?? "" }
                                    }
            }
        },
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
