using System.Reflection;
using AnotherBlogEngine.Core.Data;
using AnotherBlogEngine.Core.Data.Dto;
using AnotherBlogEngine.Core.Data.Interfaces;
using AnotherBlogEngine.Core.Interfaces;
using AnotherBlogEngine.Core.Providers;
using AnotherBlogEngine.Ui.Client.Pages;
using AnotherBlogEngine.Ui.Components;

namespace AnotherBlogEngine.Ui
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents()
                .AddInteractiveWebAssemblyComponents();


            // -- logger --

            var loggerFactory = LoggerFactory.Create(build =>
            {
                build.AddConsole();
                build.AddDebug();
            });

            builder.Services.AddSingleton(loggerFactory.CreateLogger("AnotherBlogEngine"));


            // -- db connection --
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

            // -- repositories and providers --

            builder.Services.AddScoped<IPostRepository<PostDetailsDto>, PostRepository>();
            builder.Services.AddScoped<ITopicRepository<TopicDto>, TopicRepository>();
            builder.Services.AddScoped<ITermRepository<TermDto>, TermRepository>();
            builder.Services.AddScoped<ITagRepository<TagDto>, TagRepository>();

            builder.Services.AddScoped<ITermProvider, TermProvider>();
            builder.Services.AddScoped<ITopicProvider, TopicProvider>();
            builder.Services.AddScoped<IPostProvider, PostProvider>();
            builder.Services.AddScoped<ITagProvider, TagProvider>();







            var app = builder.Build();

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

            app.Run();
        }
    }
}
