using System;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.GUI
{
	public partial class BaseCustomsPickupUserControl : BaseCustomsEntryUserControl
	{
		public BaseCustomsPickupUserControl()
		{
			InitializeComponent();
			PickupUserControl.UserControlType = GetPickupUserControlType();
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			PickTabControl.PlugIns.Add(ControllerIDs.DtbBookingTabPlugIn);
		}

		protected virtual Type GetPickupUserControlType() => typeof(PickupUserControl);
	}
}
