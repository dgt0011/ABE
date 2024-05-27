
using CommandLine;

namespace ABE.DBUpgrader
{
    internal class Options
    {
        [Option("dbname", Required=false, HelpText = "Database name to filter the upgrade scripts by name.  Does not need to correlate directly to the database name (which is set via connectionstring/AWS secret)", Default = "postgresdb")]
            public string? DbName { get; set; }

        [Option("secretName", Required = false, Default = "postgresdb-admin-user-password", HelpText = "The AWS Secrets Manager secret name that the ABE.DBUpgrader will try to use to run scripts using.")]
        public string? SecretName { get; set; }

        [Option("connectionString", Required = false, HelpText = "Typically not used, may be used in case of testing or development when the use of the AWS secret is impractical.  Not to be used for 'production' deployments/usage")]
        public string? ConnectionString { get; set; }
    }
}
