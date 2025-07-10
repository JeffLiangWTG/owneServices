using CargoWise.EntityFramework;

namespace Enterprise.Freight.Forwarding.GUI
{
	public class DashboardShipmentModuleButtonGrid : ShipmentModuleButtonGrid
	{
		public DashboardShipmentModuleButtonGrid()
			: this(null)
		{
		}

		public DashboardShipmentModuleButtonGrid(IBusinessObjectCollection gridCollection)
			: base(gridCollection, false)
		{
			ReadOnly = true;
			ShowNewButton = false;
			ShowEditButton = false;
			ShowTotalsPanel = true;
		}

		#region Buttons Readonly'ness

		protected override void UpdateButtonsReadOnly()
		{
			base.UpdateButtonsReadOnly();

			var detachButton = FindToolStripButton(Buttons.Detach);
			if (detachButton != null && detachButton.Enabled && ForceDisableDetachButton)
			{
				detachButton.Enabled = false;
			}
		}

		public bool ForceDisableDetachButton
		{
			get => forceDisableDetachButton;
			set
			{
				forceDisableDetachButton = value;
				UpdateButtonsReadOnly();
			}
		}
		bool forceDisableDetachButton;

		#endregion
	}
}
