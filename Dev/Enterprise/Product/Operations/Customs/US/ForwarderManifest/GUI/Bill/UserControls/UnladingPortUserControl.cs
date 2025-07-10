using System;
using Enterprise.Customs.US.ForwarderManifest.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.ForwarderManifest.GUI
{
	public partial class UnladingPortUserControl : ZUserControl
	{
		public UnladingPortUserControl()
		{
			InitializeComponent();
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			ChangeUNLOCOPortsComponentVisibility();
		}

		protected override void OnCurrentDataItemChanging(EventArgs e)
		{
			base.OnCurrentDataItemChanging(e);
			var bill = (USExportAsycudaBill)CurrentDataItem;
			if (bill != null && !bill.IsDeleted)
			{
				bill.ABL_RL_NKPortOfDischargeInfo.ValueChanged -= ChangeUNLOCOPortsComponentType;
			}
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			var bill = (USExportAsycudaBill)CurrentDataItem;
			if (bill != null && !bill.IsDeleted)
			{
				bill.ABL_RL_NKPortOfDischargeInfo.ValueChanged += ChangeUNLOCOPortsComponentType;
			}
		}

		void ChangeUNLOCOPortsComponentType(object sender, EventArgs e)
		{
			ChangeUNLOCOPortsComponentVisibility();
		}

		void ChangeUNLOCOPortsComponentVisibility()
		{
			var bill = (USExportAsycudaBill)CurrentDataItem;
			if (bill != null && !bill.IsDeleted)
			{
				UnladingPortScheduleKDropEdit.Visible = bill.ABL_CustomsDischargePortIsDropEdit;
				UnladingPortScheduleKTextBox.Visible = !bill.ABL_CustomsDischargePortIsDropEdit;
			}
		}
	}
}
