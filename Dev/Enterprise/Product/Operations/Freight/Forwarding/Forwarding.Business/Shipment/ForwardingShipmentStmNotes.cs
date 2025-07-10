using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Freight.Business;

namespace Enterprise.Freight.Forwarding.Business
{
	public class ForwardingShipmentStmNotes : CommonShipmentNotes
	{
		public ForwardingShipmentStmNotes(ForwardingShipment shipment) : base(shipment)
		{
			Argument.NotNull(shipment, nameof(shipment));
		}

		#region Implementation

		protected override Type ElementType => typeof(ForwardingShipmentStmNote);

		public new ForwardingShipment Parent => (ForwardingShipment)base.Parent;

		protected override BusinessObjectCollection GetNewElementsCollection()
		{
			return new ForwardingShipmentStmNoteCollection(Parent);
		}

		protected override BusinessObjectCollection GetNewAllElementsCollection()
		{
			return new ForwardingShipmentStmNoteCollectionWithRelatedElements(Parent);
		}

		protected override IBusinessObjectCollectionView GetNewVisibleElementsCollectionView()
		{
			return new ForwardingShipmentStmNoteCollectionView(Parent);
		}

		#endregion
	}
}
