using System;
using CargoWise.Types;

namespace Enterprise.Freight.Agency.Business
{
	public class AgencyBookingContainerManyToManyCollection : AgencyShipmentContainerManyToManyCollection
	{
		public AgencyBookingContainerManyToManyCollection(AgencyBookingPackLine parent)
			: base(parent)
		{
		}

		public new AgencyBookingContainer this[int index]
		{
			get { return (AgencyBookingContainer)Elements[index]; }
		}

		public new AgencyBookingContainer AddNew()
		{
			return (AgencyBookingContainer)base.AddNew();
		}

		public override Type GetTypeOfElementsFromPK(ZGuid pK)
		{
			return typeof(AgencyBookingContainer);
		}
	}
}
