using System;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.eManifest.Business
{
	public sealed class ShipmentActionCollection : NonPersistentBusinessObjectCollection<ShipmentAction>
	{
		public ShipmentActionCollection(Trip trip, string messageType)
			: base(trip.Factory)
		{
			foreach (var shipment in trip.Shipments)
			{
				Add(new ShipmentAction(shipment, messageType));
			}
		}

		#region Implementation

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
	}
}
