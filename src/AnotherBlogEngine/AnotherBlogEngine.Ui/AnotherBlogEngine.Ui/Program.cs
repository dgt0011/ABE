using AnotherBlogEngine.Core.Data;
using AnotherBlogEngine.Core.Data.Dto;
using AnotherBlogEngine.Core.Data.Interfaces;
using AnotherBlogEngine.Core.Interfaces;
using AnotherBlogEngine.Core.Providers;
using AnotherBlogEngine.Ui.Components;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System.Reflection;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AnotherBlogEngine.Ui
{
    public static class Program
    {

        const string AWS_OIDC_SCHEME = "CognitoOidc";

        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            RegisterServices(builder);

            var app = builder.Build();

            ConfigureApplication(app);

            app.Run();
        }


        private static void RegisterServices(WebApplicationBuilder builder)
        {
            IConfigurationRoot? config;

            var assName = Assembly.GetExecutingAssembly().GetName().Name;
            if (assName != null)
            {
                var appAssembly = Assembly.Load(new AssemblyName(assName));
                config = new ConfigurationBuilder()
                    .AddUserSecrets(appAssembly, true)
                    .AddJsonFile("appsettings.json")
                    .Build();
            }
            else
            {
                config = new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json")
                    .Build();
            }

            RegisterAuthenticationServices(builder, config);

            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents()
                .AddInteractiveWebAssemblyComponents();

            RegisterLoggingServices(builder);


            // -- db connection --
            var context = new PostgresqlDbContext(config);
            builder.Services.AddSingleton<IDbContext>(context);

            // -- repositories and providers --
            builder.Services.AddScoped<IPostRepository<PostDetailsDto>, PostRepository>();
            builder.Services.AddScoped<ITopicRepository<TopicDto>, TopicRepository>();
            builder.Services.AddScoped<ITermRepository<TermDto>, TermRepository>();
            builder.Services.AddScoped<ITagRepository<TagDto>, TagRepository>();

            builder.Services.AddScoped<ITermProvider, TermProvider>();
            builder.Services.AddScoped<ITopicProvider, TopicProvider>();
            builder.Services.AddScoped<IPostProvider, PostProvider>();
            builder.Services.AddScoped<ITagProvider, TagProvider>();
        }

        private static void RegisterAuthenticationServices(WebApplicationBuilder builder, IConfigurationRoot? config)
        {
            var cognitoDomain = string.Empty;
            var clientId = string.Empty;
            var clientSecret = string.Empty;
            var userPoolId = string.Empty;
            var region = string.Empty;

            if (config is not null)
            {
                var section = config.GetSection("Authentication").GetSection("Cognito");

                cognitoDomain = section!.GetValue<string>("CognitoDomain");
                clientId = section!.GetValue<string>("ClientId");
                userPoolId = section!.GetValue<string>("UserPoolId");
                clientSecret = section!.GetValue<string>("ClientSecret");
                region = section!.GetValue<string>("Region");

                if (string.IsNullOrEmpty(clientSecret))
                {
                    //TODO: refactor this into a helper method

                    //attempt secrets manager retrieval
                    var secretName = section!.GetValue<string>("AWS-SecretName");

                    var secretManagerClient = new AmazonSecretsManagerClient();

                    var secret = secretManagerClient.GetSecretValueAsync(
                        new GetSecretValueRequest
                            { SecretId = secretName }
                    ).Result;

                    if (secret is not null && !string.IsNullOrEmpty(secret!.SecretString))
                    {
                        try
                        {
                            var secretJson = JObject.Parse(secret.SecretString);
                            if (secretJson["client_secret"] != null)
                            {
                                clientSecret = secretJson["client_secret"]!.ToString();
                            }
                        }
                        catch (JsonReaderException)
                        {
                            return;
                        }
                    }
                }
            }

            builder.Services.AddAuthentication(AWS_OIDC_SCHEME)
                .AddOpenIdConnect(AWS_OIDC_SCHEME, oidcOptions =>
                {
                    oidcOptions.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    oidcOptions.Scope.Clear();
                    // required OIDC scopes
                    oidcOptions.Scope.Add("openid");
                    oidcOptions.Scope.Add("profile");

                    // default Cognito Scope
                    //TODO: pull additional scopes from config
                    oidcOptions.Scope.Add("email");

                    oidcOptions.MetadataAddress = $"https://cognito-idp.{region}.amazonaws.com/{userPoolId}/.well-known/openid-configuration";

                    oidcOptions.Events = new OpenIdConnectEvents
                    {
                        OnRedirectToIdentityProviderForSignOut = context =>
                        {
                            var uri = new Uri($"{cognitoDomain}/logout", UriKind.Absolute);

                            var logoutUrl = $"{context.Request.Scheme}://{context.Request.Host}/";

                            context.ProtocolMessage.IssuerAddress = uri.AbsoluteUri;
                            context.ProtocolMessage.ResponseType = OpenIdConnectResponseType.Code;

                            context.ProtocolMessage.SetParameter("client_id", clientId);
                            context.ProtocolMessage.SetParameter("logout_uri", logoutUrl);
                            context.ProtocolMessage.SetParameter("redirect_uri", logoutUrl);

                            return Task.CompletedTask;
                        }
                    };

                    oidcOptions.CallbackPath = new PathString("/signin-oidc");
                    oidcOptions.SignedOutCallbackPath = new PathString("/signout-callback-oidc");
                    oidcOptions.RemoteSignOutPath = new PathString("/signout-oidc");

                    oidcOptions.Authority = $"{cognitoDomain}/oauth2/authorize";
                    oidcOptions.ClientId = clientId;
                    oidcOptions.ClientSecret = clientSecret;
                    oidcOptions.ResponseType = OpenIdConnectResponseType.Code;
                    oidcOptions.MapInboundClaims = false;
                    oidcOptions.TokenValidationParameters.NameClaimType = JwtRegisteredClaimNames.Name;
                    oidcOptions.TokenValidationParameters.RoleClaimType = "role";
                })
                .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme);

            // ConfigureCookieOidcRefresh attaches a cookie OnValidatePrincipal callback to get
            // a new access token when the current one expires, and reissue a cookie with the
            // new access token saved inside. If the refresh fails, the user will be signed
            // out. OIDC connect options are set for saving tokens and the offline access
            // scope. 

            //TODO: Currently Cognito does not support the OpenID offline_access scope
            //builder.Services.ConfigureCookieOidcRefresh(CookieAuthenticationDefaults.AuthenticationScheme, AWS_OIDC_SCHEME);

            builder.Services.AddAuthorization();

            builder.Services.AddCascadingAuthenticationState();
        }

        private static void RegisterLoggingServices(WebApplicationBuilder builder)
        {
            var loggerFactory = LoggerFactory.Create(build =>
            {
                build.AddConsole();
                build.AddDebug();
            });

            builder.Services.AddSingleton(loggerFactory.CreateLogger("AnotherBlogEngine"));
        }

        private static void ConfigureApplication(WebApplication app)
        {
            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseWebAssemblyDebugging();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseStaticFiles();
            app.UseAntiforgery();

            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode()
                .AddInteractiveWebAssemblyRenderMode()
                .AddAdditionalAssemblies(typeof(Client._Imports).Assembly);

            app.MapGroup("/authentication").MapLoginAndLogout();
        }
    }
}
