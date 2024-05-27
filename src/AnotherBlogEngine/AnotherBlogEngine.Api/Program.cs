
using System.Reflection;
using AnotherBlogEngine.Core.Data;
using AnotherBlogEngine.Core.Data.Dto;
using AnotherBlogEngine.Core.Data.Interfaces;
using AnotherBlogEngine.Core.Interfaces;
using AnotherBlogEngine.Core.Providers;
using FastEndpoints;

namespace AnotherBlogEngine.Api
{
    public class Program
    {
        const string AllowedOrigins = "_allowSpecificOrigins";

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

            var loggerFactory = LoggerFactory.Create(build =>
            {
                build.AddConsole();
                build.AddDebug();
            });

            IConfigurationRoot? config = null;

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

            var context = new PostgresqlDbContext(config);

            builder.Services.AddSingleton<IDbContext>(context);

            builder.Services.AddSingleton(loggerFactory.CreateLogger("AnotherBlogEngine"));

            builder.Services.AddScoped<IPostRepository<PostDetailsDto>, PostRepository>();
            
            builder.Services.AddScoped<IPostProvider, PostProvider>();

            // Add services to the container.
            builder.Services.AddAuthorization();

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddFastEndpoints();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy(name: AllowedOrigins,
                    policy =>
                    {
                        policy.WithOrigins("https://localhost:5049", "http://localhost:5049");
                    });
            });
        }

        private static void ConfigureApplication(WebApplication app)
        {
            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseCors(AllowedOrigins);

            app.UseAuthorization();

            app.UseFastEndpoints(a =>
            {
                a.Endpoints.RoutePrefix = "api";
            });
        }
    }
}
