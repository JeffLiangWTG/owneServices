using System;
using CargoWise.Types;

namespace Enterprise.Freight.Agency.Business
{
	public class BillOfLadingContainerManyToManyCollection : AgencyShipmentContainerManyToManyCollection
	{
		public BillOfLadingContainerManyToManyCollection(BillOfLadingPackLine parent)
			: base(parent) { }

		public new BillOfLadingContainer this[int index]
		{
			get { return (BillOfLadingContainer)Elements[index]; }
		}

		public new BillOfLadingContainer AddNew()
		{
			return (BillOfLadingContainer)base.AddNew();
		}

		public override Type GetTypeOfElementsFromPK(ZGuid pK)
		{
			return typeof(BillOfLadingContainer);
		}
	}
}


