using CargoWise.Data.Providers.Common;

namespace CargoWise.Blazor.Common.Data
{
	public static class ConnectionStringBuilder
	{
		public static string GetConnectionString(string serverName, string dbName, string applicationName)
		{
			var encrypt = SqlTlsSetting.ShouldEncryptSqlConnection(serverName);
			var builder = new SqlConnectionStringBuilder
			{
				PersistSecurityInfo = false,
				Pooling = true,
				ApplicationName = applicationName,
				DataSource = serverName,
				InitialCatalog = dbName,
				IntegratedSecurity = true,
				Encrypt = encrypt,
				TrustServerCertificate = !encrypt
			};

			if (SqlFailoverSettings.ShouldSpecifyMultiSubnetFailover(serverName))
			{
				builder.MultiSubnetFailover = true;
			}

			return builder.ToString();
		}
	}
}
