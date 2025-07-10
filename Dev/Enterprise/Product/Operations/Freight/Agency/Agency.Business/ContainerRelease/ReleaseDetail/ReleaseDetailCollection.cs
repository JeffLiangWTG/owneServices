using System;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Freight.Agency.Business
{
	public class ReleaseDetailCollection : NonPersistentBusinessObjectCollection<ReleaseDetail>
	{
		public ReleaseDetailCollection(AgencyShipment shipment)
		{
			this.shipment = shipment;
		}

		public void RePopulate(ZString releaseNumber)
		{
			RemoveAllButLeaveRelationshipsIntact();

			ZGuid containerYard = ContainerYardForRelease(releaseNumber);

			foreach (AgencyShipmentContainer container in shipment.BookedContainers)
			{
				if (container.JC_IsShipperOwned)
				{
					continue;
				}

				bool matchContainerYard = containerYard.IsEmpty || container.JC_OA_DepartureContainerYardAddress == containerYard;

				if (container.JC_ReleaseNum == releaseNumber || (container.JC_ReleaseNum.IsEmpty && matchContainerYard))
				{
					Add(new ReleaseDetail(container));
				}
			}
		}

		#region Collection Overrides

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override bool AllowRemoveCore
		{
			get { return false; }
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new NotSupportedException();
		}

		#endregion

		#region Implementation

		ZGuid ContainerYardForRelease(ZString releaseNumber)
		{
			if (!releaseNumber.IsEmpty)
			{
				foreach (AgencyShipmentContainer container in shipment.BookedContainers)
				{
					if (container.JC_ReleaseNum == releaseNumber)
					{
						return container.JC_OA_DepartureContainerYardAddress;
					}
				}
			}

			return ZGuid.Empty;
		}

		#endregion

		readonly AgencyShipment shipment;
	}
}



