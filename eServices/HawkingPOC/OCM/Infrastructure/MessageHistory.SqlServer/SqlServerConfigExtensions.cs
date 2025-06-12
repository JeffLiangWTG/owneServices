using System.Data.SqlClient;
using OcmPoc.Utils.Config;

namespace OcmPoc.Infrastructure.MessageHistory.SqlServer
{
	static class SqlServerConfigExtensions
	{
		public static string ToConnectionString(this SqlServerConfig config)
		{
			if (config.Host == null)
			{
				return "Server=localhost;Database=MessageHistory;Trusted_Connection=True;";
			}

			return new SqlConnectionStringBuilder($"Server={config.Host};Database={config.Database};Trusted_Connection=True;")
			{
				UserID = config.Username,
				Password = config.Password,
				IntegratedSecurity = false,
			}.ConnectionString;
		}
	}
}
