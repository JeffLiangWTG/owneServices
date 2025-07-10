using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Freight.Confirmations.Business
{
	public class QuickPODCollection : NonPersistentBusinessObjectCollection<QuickPOD>
	{
		public QuickPODCollection(QuickPODs quickPODs)
			: base(quickPODs.Factory)
		{
			QuickPODHost = quickPODs;
		}
		readonly QuickPODs QuickPODHost;

		#region BusinessObjectCollection Overrides

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new QuickPOD(QuickPODHost);
		}

		public QuickPOD this[ZGuid shipmentPK]
		{
			get
			{
				QuickPOD result = null;

				foreach (QuickPOD pod in Elements)
				{
					if (pod.Shipment != null && pod.Shipment.PK == shipmentPK)
					{
						result = pod;
						break;
					}
				}

				return result;
			}
		}

		#endregion

		#region ContainsShipment

		public bool ContainsShipment(ZGuid shipmentPK, ZGuid excludedPODPK)
		{
			foreach (QuickPOD pod in Elements)
			{
				if (pod.Shipment != null && pod.Shipment.PK == shipmentPK && pod.PK != excludedPODPK)
				{
					return true;
				}
			}
			return false;
		}

		#endregion

		#region ReleaseShipmentJobHeaderMutexes

		public void ReleaseShipmentJobHeaderMutexes()
		{
			foreach (QuickPOD pod in Elements)
			{
				pod.ReleaseShipmentJobHeaderMutex();
			}
		}

		#endregion
	}
}
