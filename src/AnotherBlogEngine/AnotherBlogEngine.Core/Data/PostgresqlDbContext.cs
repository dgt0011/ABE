using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using AnotherBlogEngine.Core.Data.Interfaces;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Npgsql;
using System.Data;

namespace AnotherBlogEngine.Core.Data
{
    public class PostgresqlDbContext : IDbContext
    {
        private readonly IConfiguration _configuration;

        private string? _connectionString = null;

        public PostgresqlDbContext(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IDbConnection? CreateConnection()
        {
            // if the connection string isn't set - create it (or attempt to anyway)
            if (_connectionString == null)
            {
                SetConnectionString();
            }

            if (_connectionString == string.Empty)
            {
                throw new ApplicationException("Connection to the database cannot be established.");
            }

            var retVal = new NpgsqlConnection(_connectionString);
            if (retVal.State != ConnectionState.Closed)
            {
                throw new ApplicationException("Connection to the database cannot be established.");
            }
            return retVal;
        }

        public IDbConnection? CreateOpenConnection()
        {
            var connection = CreateConnection();
            if (connection != null)
            {
                connection.Open();
                return connection;
            }

            return null;
        }

        private void SetConnectionString()
        {
            if (_connectionString == string.Empty)
            {
                // a previous attempt has been made and was unsuccessful.  Exit early
                return;
            }

            var section = _configuration.GetSection("DBCredentials");

            var localDbConnectionString = section!.GetValue<string>("ConnectionString");
            if (!string.IsNullOrEmpty(localDbConnectionString))
            {
                _connectionString = localDbConnectionString;
                return;
            }

            var awsSecretName = section!.GetValue<string>("AWS-SecretName");
            if (string.IsNullOrEmpty(awsSecretName))
            {
                return;
            }

            // assuming that the secret contains the standard fields set when auto credential rotation is configured
            // we can get just about all the connection string values from the AWS Secrets Manager secret.
            // DOCO: document the 'standard' credentials fields provided so manual configuration of secret values can be set as well

            var secretManagerClient = new AmazonSecretsManagerClient();

            var secret = secretManagerClient.GetSecretValueAsync(
                new GetSecretValueRequest
                { SecretId = awsSecretName }
            ).Result;

            if (secret == null || string.IsNullOrEmpty(secret!.SecretString))
            {
                _connectionString = string.Empty;
                return;
            }

            JObject? secretJson;
            try
            {
                secretJson = JObject.Parse(secret.SecretString);
            }
            catch (JsonReaderException)
            {
                //TODO: Log these 
                _connectionString = string.Empty;
                return;
            }

            if (secretJson["host"] != null &&
                secretJson["dbInstanceIdentifier"] != null &&
                secretJson["port"] != null &&
                secretJson["dbname"] != null &&
                secretJson["username"] != null &&
                secretJson["password"] != null)
            {
                var host = secretJson["host"]!.ToString();
                var port = secretJson["port"]!.ToString();
                var userName = secretJson["username"]!.ToString();
                var passWord = secretJson["password"]!.ToString();
                var database = secretJson["dbname"]!.ToString();

                _connectionString = $"Host={host};Port={port};Username={userName};Password={passWord};Database={database};Timeout=14;Pooling=true;MinPoolSize=100;MaxPoolSize=200;";
            }
            else
            {
                _connectionString = string.Empty;
            }
        }
    }
}
