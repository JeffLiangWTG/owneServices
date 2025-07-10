using System;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	public partial class PickupUserControl : ZUserControl
	{
		public PickupUserControl()
		{
			InitializeComponent();
			PickupPartiesUserControl.UserControlType = GetPickupPartiesUserControlType();
			PickupDetailsUserControl.UserControlType = GetPickupDetailsUserControlType();
		}

		protected virtual Type GetPickupPartiesUserControlType() => typeof(PickupPartiesUserControl);

		protected virtual Type GetPickupDetailsUserControlType() => typeof(PickupDetailsUserControl);
	}
}
