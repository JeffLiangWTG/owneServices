using System;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.GUI
{
	public partial class BaseCustomsDeliveryUserControl : BaseCustomsEntryUserControl
	{
		public BaseCustomsDeliveryUserControl()
		{
			InitializeComponent();
			DeliveryUserControl.UserControlType = GetDeliveryUserControlType();
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			DeliveryTabControl.PlugIns.Add(ControllerIDs.DtbBookingTabPlugIn);
		}

		protected virtual Type GetDeliveryUserControlType() => typeof(DeliveryUserControl);
	}
}
