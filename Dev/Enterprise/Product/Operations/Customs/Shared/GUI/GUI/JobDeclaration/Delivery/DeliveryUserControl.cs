using System;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	public partial class DeliveryUserControl : ZUserControl
	{
		public DeliveryUserControl()
		{
			InitializeComponent();
			DeliveryPartiesUserControl.UserControlType = GetDeliveryPartiesUserControlType();
			DeliveryDetailsUserControl.UserControlType = GetDeliveryDetailsUserControlType();
		}

		protected virtual Type GetDeliveryPartiesUserControlType() => typeof(DeliveryPartiesUserControl);

		protected virtual Type GetDeliveryDetailsUserControlType() => typeof(DeliveryDetailsUserControl);
	}
}
