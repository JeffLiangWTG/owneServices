using System.Linq;
using CargoWise.Types;
using Enterprise.Registry.Business;

namespace Enterprise.Freight.Agency.Business
{
	public static class ShippingPortsMessagingEHubIDHelper
	{
		public static ZString GetEHubID(ZString port)
		{
			var eHubIDs = AgencyRegistry.Instance.ShippingPortsMessagingEHubID.Value
				.Cast<ShippingPortsMessagingEHubID>();

			var recipient = eHubIDs.FirstOrDefault(x => x.Port == port)?.RecipientID;

			if (string.IsNullOrEmpty(recipient))
			{
				recipient = eHubIDs.FirstOrDefault(x => x.Port.IsEmpty)?.RecipientID;
			}

			return recipient ?? ZString.Empty;
		}
	}
}
