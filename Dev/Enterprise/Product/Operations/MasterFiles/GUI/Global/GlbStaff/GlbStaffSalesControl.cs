using System;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class GlbStaffSalesControl : ZUserControl
	{
		public GlbStaffSalesControl()
		{
			InitializeComponent();

			if (!CommissionLookups.ShouldShowServicesAndSubModules)
			{
				CommissionEntitlementRulesGrid.RemoveFromAvailableColumns(OverallStaffCommissionRule.Schema.Service, OverallStaffCommissionRule.Schema.SubModule);
			}
		}

		#region CurrentDataItem

		new GlbStaff CurrentDataItem
		{
			get { return (GlbStaff)base.CurrentDataItem; }
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);

			var currentDataItem = CurrentDataItem;
			if (currentDataItem != null)
			{
				CurrentDataItem.OverallCommissionRulesView.IncludeExpiredRules = ShowExpiredCheckBox.Checked;
				CurrentDataItem.OverallCommissionRulesView.IncludeDisabledRules = ShowDisabledCheckBox.Checked;
			}
		}

		#endregion

		#region ShowExpiredCheckBox

		void ShowExpiredCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			var currentDataItem = CurrentDataItem;
			if (currentDataItem != null)
			{
				CurrentDataItem.OverallCommissionRulesView.IncludeExpiredRules = ShowExpiredCheckBox.Checked;
			}
		}

		#endregion

		#region HideDisabledCheckBox

		void ShowDisabledCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			var currentDataItem = CurrentDataItem;
			if (currentDataItem != null)
			{
				CurrentDataItem.OverallCommissionRulesView.IncludeDisabledRules = ShowDisabledCheckBox.Checked;
			}
		}

		#endregion
	}
}