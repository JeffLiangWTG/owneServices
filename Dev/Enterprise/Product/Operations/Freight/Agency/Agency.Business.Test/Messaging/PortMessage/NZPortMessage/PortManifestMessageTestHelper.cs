using CargoWise.Types;

namespace Enterprise.Freight.Agency.Business.Testing
{
	internal static class PortManifestMessageTestHelper
	{
		internal static PortManifestPort CreateLoadAndDischargeManifestPort(PortManifestPortCollection collection, ZString portCode, bool isEnabled = true, bool isEmptyPrincipalPK = true)
		{
			var port = collection.AddNew();

			port.Port = portCode;
			port.PrincipalPK = isEmptyPrincipalPK ? ZGuid.Empty : ZGuid.NewZGuid();
			port.SenderID = "SenderID_" + collection.Count.ToString();
			port.Enabled = isEnabled;

			return port;
		}
	}
}
