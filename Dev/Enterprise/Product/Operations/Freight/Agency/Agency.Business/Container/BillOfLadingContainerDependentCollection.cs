using System;
using CargoWise.Types;

namespace Enterprise.Freight.Agency.Business
{
	public class BillOfLadingContainerDependentCollection : AgencyShipmentContainerDependentCollection
	{
		public BillOfLadingContainerDependentCollection(BillOfLading shipment, ZString purpose)
			: base(shipment, purpose) { }

		public new BillOfLadingContainer AddNew()
		{
			return (BillOfLadingContainer)base.AddNew();
		}

		public new BillOfLadingContainer this[int index]
		{
			get { return (BillOfLadingContainer)Elements[index]; }
		}

		#region Implementation

		public override Type GetTypeOfElementsFromPK(ZGuid pK)
		{
			return typeof(BillOfLadingContainer);
		}

		#endregion
	}
}


