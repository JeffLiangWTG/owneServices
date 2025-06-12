using System.ServiceModel;

namespace CargoWise.eHub.Products.JPCustoms.Client
{
	[ServiceContract(Namespace = "http://cargowise.com/ehub/products/jpcustomsreply")]
	public interface IJPCustomsReply
	{
		[OperationContract(Action = "SendMessage", ReplyAction = "*")]
		[XmlSerializerFormat()]
		void SendMessage(string message);
	}
}
