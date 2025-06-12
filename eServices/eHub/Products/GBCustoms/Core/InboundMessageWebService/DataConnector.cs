using System.Configuration;
using System.Data.SqlClient;

namespace CargoWise.eHub.Products.GBCustoms.Core.InboundMessageWebService
{
	public class DataConnector : IDataConnector
	{
		public SqlConnection GeteHubTransactionsConnection(string connectionStringKey)
		{
			var connectionString = ConfigurationManager.ConnectionStrings[connectionStringKey].ConnectionString;
			return new SqlConnection(connectionString);
		}
	}
}
