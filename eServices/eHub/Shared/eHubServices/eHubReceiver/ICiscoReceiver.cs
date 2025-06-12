using System.ServiceModel;

namespace CargoWise.eHub.Share.eHubServices.eHubReceiver
{
	[ServiceContract(Namespace = "http://scm.cisco.com")]
	public interface ICiscoReceiver
	{
		[OperationContract(Name = "receiveRequest", Action = "urn:serviceRequest")]
		[return: MessageParameter(Name = "return")]
		string Receive3B14(string sMsgIdentifier, string sMsgType, int iPriority, string sRequestXML, string sSender, string sReceiver);
	}
}
