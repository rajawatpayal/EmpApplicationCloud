using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add controllers and API explorer
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
    
    builder.Services.AddEndpointsApiExplorer();

// Add authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.Authority = "https://accounts.google.com";  // Google OAuth issuer
        options.Audience = builder.Configuration["Authentication:Google:ClientId"];       // Your Google Client ID
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = "https://accounts.google.com",
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Authentication:Google:ClientId"],  // Must match the Client ID in Google Console
            ValidateLifetime = true
        };
});

builder.Services.AddAuthorization(options=>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("UserOnly", policy => policy.RequireRole("User"));
});

// Add Swagger
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.OAuth2,
        Flows = new OpenApiOAuthFlows
        {
            AuthorizationCode = new OpenApiOAuthFlow
            {
                AuthorizationUrl = new Uri("https://accounts.google.com/o/oauth2/auth"),
                TokenUrl = new Uri("https://oauth2.googleapis.com/token"),
                Scopes = new Dictionary<string, string>
                {
                    { "openid", "User OpenID" },
                    { "email", "User Email" },
                    { "profile", "User Profile" }
                }
            }
        }
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "oauth2" }
            },
            new[] { "openid", "email", "profile" }
        }
    });
});


var app = builder.Build();
app.UseStaticFiles();
// Serve static files first
app.UseRouting();     // Enable routing

app.UseAuthentication();
app.UseAuthorization();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"); 

    app.Use(async (context, next) =>
{
    var user = context.User;
    if (user.Identity.IsAuthenticated)
    {
        var identity = (ClaimsIdentity)user.Identity;
        var email = identity.FindFirst(ClaimTypes.Email)?.Value;

        // Example: Assign roles based on email
        if (email == "rajawatpayal9@gmail.com")
        {
            identity.AddClaim(new Claim(ClaimTypes.Role, "Admin"));
        }
        else
        {
            identity.AddClaim(new Claim(ClaimTypes.Role, "User"));
        }
    }

    await next();
});

app.UseEndpoints(endpoints => endpoints.MapControllers()); 
app.UseStatusCodePagesWithReExecute("/error/{0}");
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.OAuthClientId(builder.Configuration["Authentication:Google:ClientId"]);
    options.OAuthClientSecret(builder.Configuration["Authentication:Google:ClientSecret"]);
    options.OAuthUsePkce();
});

app.MapGet("/", () => "Hello, authenticated world!")
    .RequireAuthorization();

app.Run();
