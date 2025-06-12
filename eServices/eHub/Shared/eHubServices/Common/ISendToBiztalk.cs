using System.ServiceModel;
using System.ServiceModel.Channels;

namespace CargoWise.eHub.Share.eHubServices.eHubSender.Common
{
	[ServiceContract(Namespace = "http://cargowise.com/ehub/product/2013/04")]
	public interface ISendToBiztalk
	{
		[OperationContract(Action = "SendMessage", ReplyAction = "*")]
		[XmlSerializerFormat()]
		void SendMessage(Message message);
	}
}