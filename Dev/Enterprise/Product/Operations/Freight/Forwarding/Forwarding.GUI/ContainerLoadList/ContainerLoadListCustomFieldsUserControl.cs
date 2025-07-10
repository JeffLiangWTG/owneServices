using System;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.GUI
{
	public partial class ContainerLoadListCustomFieldsUserControl : ZUserControl
	{
		public ContainerLoadListCustomFieldsUserControl()
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
