using System;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MarketingManager.GUI
{
	public partial class SalesProductCustomFieldsControl : ZUserControl
	{
		public SalesProductCustomFieldsControl()
		{
			InitializeComponent();
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			salesProductCustomFieldsInnerControl.SetDataBinding(CurrentDataItem, "");
		}
	}
}
