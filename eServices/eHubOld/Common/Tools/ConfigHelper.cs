using System.Configuration;
using System.Data.EntityClient;
using System.Data.SqlClient;

namespace CargoWise.eServices.eHub.Common
{
	public static class ConfigHelper
	{
		public static string DbServerName
		{
			get { return dbServerName ?? (dbServerName = ConfigurationManager.AppSettings["eHubDbServerName"]); }
		}
		static string dbServerName;

		public static string DbServerConnectionString(string databaseName)
		{
			//return string.Format(@"Data Source={0};Initial Catalog={1};Persist Security Info=True;User ID=OdysseyApplication;Password=kwos&sa*13s34%mf", DbServerName, databaseName);
			var sqlBuilder = new SqlConnectionStringBuilder();
			sqlBuilder.MultipleActiveResultSets = true;
			sqlBuilder.DataSource = "SYD-WPAT-1";//DbServerName
			sqlBuilder.InitialCatalog = databaseName;
			sqlBuilder.IntegratedSecurity = false;
			sqlBuilder.UserID = "OdysseyApplication";
			sqlBuilder.Password = "kwos&sa*13s34%mf";
			sqlBuilder.ConnectTimeout = 60;

			var entityBuilder = new EntityConnectionStringBuilder();
			entityBuilder.ProviderConnectionString = sqlBuilder.ToString();
			entityBuilder.Metadata = "res://*/";
			entityBuilder.Provider = "System.Data.SqlClient";

			return entityBuilder.ConnectionString;
		}
	}
}
