using System;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	public partial class InvoiceLineCustomFieldsControl : ZUserControl
	{
		public InvoiceLineCustomFieldsControl()
		{
			InitializeComponent();
			CustomFieldsControl.NothingSetupMessageLabelText = Res.GetString("74d22bb0-9839-4e2a-84d7-7e219afd5da4", "To make use of this tab, please setup Commercial Invoice Lines custom fields in Workflow Manager");
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			CustomFieldsControl.SetDataBinding(CurrentDataItem, string.Empty);
		}
	}
}
