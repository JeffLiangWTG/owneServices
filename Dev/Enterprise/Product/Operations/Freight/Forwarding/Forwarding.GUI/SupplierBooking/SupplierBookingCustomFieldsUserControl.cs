using System;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.GUI
{
	public partial class SupplierBookingCustomFieldsUserControl : ZUserControl
	{
		public SupplierBookingCustomFieldsUserControl()
		{
			InitializeComponent();
		}

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);

			this.CustomFieldsUserControl.ForceBindingIncludingParents();
		}
	}
}
