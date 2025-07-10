using CargoWise.Types;

namespace Enterprise.Freight.Agency.Business.Testing
{
	internal static class DangerousGoodsManifestMessageTestHelper
	{
		internal static DangerousGoodsManifestPort CreateDangerousGoodsManifestPort(DangerousGoodsManifestPortCollection collection, ZString portCode, bool isEnabled = true, bool isEmptyPrincipalPK = true)
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
