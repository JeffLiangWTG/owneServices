using System.ServiceModel;

namespace CargoWise.eHub.Share.eHubServices.eHubSender.Common
{
	[ServiceContract(Namespace = "http://cargowise.com/ehub/product/2013/04")]
	public interface ISendToService
	{
		[OperationContract]
		void Send(string senderId, string recepientId, string message);
	}

}
