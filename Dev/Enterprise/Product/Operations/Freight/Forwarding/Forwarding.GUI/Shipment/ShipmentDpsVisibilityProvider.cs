using System;
using CargoWise.Windows.UI.Layout;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.GUI
{
	public class ShipmentDpsVisibilityProvider : IVisibilityProvider
	{
		public ShipmentDpsVisibilityProvider(Func<bool> currentlyVisible)
		{
			this.currentlyVisible = currentlyVisible;
			Visible = !ComplianceRiskHelper.CheckIfComplianceRiskEnabled(typeof(ForwardingShipment), false);
		}

		bool visible;
		readonly Func<bool> currentlyVisible;

		public bool Visible
		{
			get => (currentlyVisible?.Invoke() ?? true) && visible;
			set
			{
				if (visible != value)
				{
					visible = value;
					if (VisibleChanged != null)
					{
						VisibleChanged(this, EventArgs.Empty);
					}
				}
			}
		}

		public event EventHandler VisibleChanged;
	}
}
