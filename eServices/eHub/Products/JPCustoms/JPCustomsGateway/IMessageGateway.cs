using System.ServiceModel;

namespace CargoWise.eHub.Products.JPCustoms.Gateway
{
	[ServiceContract(Namespace = "http://cargowise.com/ehub/products/jpcustoms")]
	public interface IMessageGateway
	{
		[OperationContract]
		void SendLodgement(string message, string audit);
	}
}
