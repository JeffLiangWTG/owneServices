namespace Enterprise.Freight.Forwarding.GUI
{
	using System;
	using CargoWise.EntityFramework;
	using CargoWise.Windows.UI.Layout;
	using Enterprise.Freight.Forwarding.Business;

	public class JobPresenceBasedVisibilityProvider : IVisibilityProvider
	{
		public JobPresenceBasedVisibilityProvider(ForwardingShipment shipment, bool visibleWhenJobPresent)
		{
			this.shipment = shipment;
			this.visibleWhenJobPresent = visibleWhenJobPresent;

			shipment.JobCreated += ShipmentOnJobCreatedAndDeleted;
			shipment.JobDeleted += ShipmentOnJobCreatedAndDeleted;
		}

		#region IVisibilityDependencyProvider

		public bool Visible
		{
			get
			{
				return visibleWhenJobPresent
					? !BusinessObject.IsNullOrDeleted(shipment.ShipmentJobHeader)
					: BusinessObject.IsNullOrDeleted(shipment.ShipmentJobHeader);
			}
		}

		public event EventHandler VisibleChanged;

		#endregion

		#region IDisposable

		public void Dispose()
		{
			if (shipment != null)
			{
				shipment.JobCreated -= RaiseVisibleChanged;
				shipment.JobDeleting -= RaiseVisibleChanged;
			}
		}

		#endregion

		#region Implementation

		void RaiseVisibleChanged(object sender, EventArgs eventArgs)
		{
			if (VisibleChanged != null)
			{
				VisibleChanged(sender, eventArgs);
			}
		}

		void ShipmentOnJobCreatedAndDeleted(object sender, EventArgs eventArgs)
		{
			RaiseVisibleChanged(sender, eventArgs);
		}

		readonly ForwardingShipment shipment;
		readonly bool visibleWhenJobPresent;

		#endregion
	}
}
