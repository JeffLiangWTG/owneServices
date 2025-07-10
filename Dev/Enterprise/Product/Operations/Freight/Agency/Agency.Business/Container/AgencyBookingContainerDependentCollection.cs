using System;
using CargoWise.Types;

namespace Enterprise.Freight.Agency.Business
{
	public class AgencyBookingContainerDependentCollection : AgencyShipmentContainerDependentCollection
	{
		public AgencyBookingContainerDependentCollection(AgencyShipment shipment, ZString purpose)
			: base(shipment, purpose) { }

		public new AgencyBookingContainer AddNew()
		{
			return (AgencyBookingContainer)base.AddNew();
		}

		public new AgencyBookingContainer this[int index]
		{
			get { return (AgencyBookingContainer)Elements[index]; }
		}

		#region Implementation

		public override Type GetTypeOfElementsFromPK(ZGuid pK)
		{
			return typeof(AgencyBookingContainer);
		}

		protected override bool AllowNewCore
		{
			get { return purpose == ContainerBookedStatus.Codes.Booked; }
		}

		#endregion
	}
}
