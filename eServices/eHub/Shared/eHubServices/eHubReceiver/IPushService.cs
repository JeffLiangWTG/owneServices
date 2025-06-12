using System.IO;
using System.ServiceModel;
using System.ServiceModel.Web;

namespace CargoWise.eHub.Share.eHubServices.eHubReceiver
{
	[ServiceContract]
	public interface IPushService
	{
		[WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Bare)]
		void PostContainerEvent(Stream dataStream);
	}
}
