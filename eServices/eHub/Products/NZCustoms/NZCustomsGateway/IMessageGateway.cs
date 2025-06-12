using System.ServiceModel;

namespace CargoWise.eHub.Products.NZCustoms.Gateway
{
	[ServiceContract(Namespace = "http://cargowise.com/ehub/products/")]
	public interface IMessageGateway
	{
		[OperationContract]
		Response SendLodgement(string reference, string messageType, string authentication, string message, string eHubTrackingID);
        bool Ping();
	}
}