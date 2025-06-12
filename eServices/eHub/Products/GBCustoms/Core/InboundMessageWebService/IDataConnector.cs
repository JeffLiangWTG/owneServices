using System.Data.SqlClient;

namespace CargoWise.eHub.Products.GBCustoms.Core.InboundMessageWebService
{
	public interface IDataConnector
	{
		SqlConnection GeteHubTransactionsConnection(string connectionStringKey);
	}
}
