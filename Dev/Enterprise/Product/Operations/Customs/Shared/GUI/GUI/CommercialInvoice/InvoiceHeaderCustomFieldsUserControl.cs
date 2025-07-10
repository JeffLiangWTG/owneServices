using System;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	public partial class InvoiceHeaderCustomFieldsUserControl : ZUserControl
	{
		public InvoiceHeaderCustomFieldsUserControl()
		{
			InitializeComponent();
			this.InvCustomFieldsDisplayControl.NothingSetupMessageLabelText = Res.GetString("0781e188-46ef-4433-ba4c-9c401a7a88dc", "To make use of this tab, please setup commercial invoice custom fields in Workflow Manager.");
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			this.InvCustomFieldsDisplayControl.SetDataBinding(this.CurrentDataItem, "");
			if (!this.InvCustomFieldsDisplayControl.IsDisposed)
			{
				this.InvCustomFieldsDisplayControl.ForceBindingIncludingParents();
			}
		}
	}
}
