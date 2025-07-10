using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Freight.Integration
{
	public static partial class Forwarding
	{
		public interface IPortMessagingForwardingHelper
		{
			void SyncroniseMRN(BusinessObjectFactory factory, ZGuid shipmentPK, ZString oldValue, ZString newValue);
			void OnPacklineDelete(BusinessObjectFactory factory, ZGuid packlinePK);
		}
	}
}
