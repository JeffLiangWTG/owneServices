using System;

using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.GUI
{
	public partial class WarehouseUserControl : OrganisationSecurityContainerControl
	{
		public WarehouseUserControl()
		{
			InitializeComponent();
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			WarehouseTabControl.PlugIns.AddPlugInAtTabPageIndex(ControllerIDs.WhsConfigOrgReceive, 2);
			WarehouseTabControl.PlugIns.AddPlugInAtTabPageIndex(ControllerIDs.WhsConfigOrgPicking, 3);
		}

		#region Dispose

		protected override void Dispose(bool isNotFinalizing)
		{
			if (isNotFinalizing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(isNotFinalizing);
		}

		#endregion
	}
}
