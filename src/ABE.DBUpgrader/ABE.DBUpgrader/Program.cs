using System.Reflection;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using CommandLine;
using DbUp;
using DbUp.Postgresql;
using Newtonsoft.Json.Linq;

namespace ABE.DBUpgrader
{
    internal class Program
    {
        static int Main(string[] args)
        {
            var retVal = 0;
            Parser.Default.ParseArguments<Options>(args)
                .WithParsed(o =>
                {
                    Console.WriteLine($"Upgrading {o.DbName}");

                    var connectionString = CreateConnectionString(o);

                    if (string.IsNullOrEmpty(connectionString))
                    {
                        WriteConsoleError($"Failed to create the connection string for {o.DbName}.  Exiting...");
                        retVal = -1;
                        return;
                    }

                    var scriptPrefix = $"ABE.DBUpgrader.Scripts.{o.DbName}-";

                    var builder = DeployChanges.To
                        .PostgresqlDatabase(connectionString)
                        .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly(), s => s.StartsWith(scriptPrefix));

                    builder.WithVariablesDisabled();

                    builder.Configure(
                        c => c.Journal = new PostgresqlTableJournal(() => c.ConnectionManager, () => c.Log, "public", "schemaversions"));
                    var upgrader = builder.Build();

                    if (!upgrader.TryConnect(out var errorMessage))
                    {
                        WriteConsoleError($"Cannot connect to {o.DbName} : {errorMessage}");
                        retVal = -1;
                    }

                    if (!upgrader.IsUpgradeRequired())
                    {
                        Console.WriteLine($"{o.DbName} is already up to date.  No upgrade required.");
                        return;
                    }

                    var result = upgrader.PerformUpgrade();

                    if (!result.Successful)
                    {
                        WriteConsoleError(result.Error.ToString());
                        retVal = -1;
                        return;
                    }

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Success!");
                    Console.ResetColor();
                    retVal = 0;
                });

            return retVal;
        }

        private static void WriteConsoleError(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        private static string CreateConnectionString(Options opts)
        {
            if (!string.IsNullOrEmpty(opts.ConnectionString))
            {
                return opts.ConnectionString;
            }

            string secretJson;

            // fetch the auto rotated DBO password secret from SecretsManager.
            var secretsManagerClient = new AmazonSecretsManagerClient();

            try
            {
                var secretRequest = new GetSecretValueRequest { SecretId = opts.SecretName };
                var secretValue = secretsManagerClient.GetSecretValueAsync(secretRequest).Result;
                if (secretValue is null)
                {
                    WriteConsoleError($"Failed to read the master users password from Secrets Manager ({opts.SecretName})");
                    return string.Empty;
                }

                secretJson = secretValue.SecretString;
            }
            catch (AggregateException aggEx)
            {
                //Secrets Manager can't find the specified secret.
                if (aggEx.InnerException?.Message == "Secrets Manager can't find the specified secret.")
                {
                    WriteConsoleError("Secrets Manager can't find the specified secret.");
                    WriteConsoleError($"Confirm that the secret named '{opts.SecretName}' exists.");
                    return string.Empty;
                }
                if (aggEx.InnerException?.Message == "The security token included in the request is expired")
                {
                    WriteConsoleError("your AWS session has expired.");
                    WriteConsoleError("Ensure you have a valid & current AWS session for the expected environment before continuing by running aws-mfa or equivalent.");
                    return string.Empty;
                }

                WriteConsoleError(aggEx.Message);
                return string.Empty;
            }
            catch (Exception e)
            {
                WriteConsoleError(e.Message);
                return string.Empty;
            }

            secretsManagerClient.Dispose();

            string? hostName;
            string? hostPort;
            string? dbName;
            string? userName;
            string? password;

            try
            {
                var secretJObject = JObject.Parse(secretJson);

                hostName = secretJObject["host"]?.ToString();
                hostPort = secretJObject["port"]?.ToString();
                dbName = secretJObject["dbname"]?.ToString();
                userName = secretJObject["username"]?.ToString();
                password = secretJObject["password"]?.ToString();
            }
            catch
            {
                WriteConsoleError($"{opts.SecretName} is malformed json or is missing required fields.");
                return string.Empty;
            }

            return $"Host={hostName};Port={hostPort};Username={userName};Password={password};Database={dbName};Timeout=14;Pooling=true;MinPoolSize=100;MaxPoolSize=500;SSL Mode=Require;Trust Server Certificate=true";
        }

    }
}