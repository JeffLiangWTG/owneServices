      using System;
using System.ServiceModel;

[assembly: CLSCompliant(true)]
namespace CargoWise.eHub.Products.NZCustoms.Common
{
	[ServiceContract(Namespace = "http://cargowise.com/ehub/products/nzcustomsreply")]
	public interface INZCustomsReply
	{
		[OperationContract(Action = "SendMessage", ReplyAction = "*")]  
		[XmlSerializerFormat()]
		void SendMessage(NZCustomsReply message);
	}
}
