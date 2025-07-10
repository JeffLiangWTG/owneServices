using System;
using Enterprise.Customs.US.ForwarderManifest.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.ForwarderManifest.GUI
{
	public partial class LadingPortUserControl : ZUserControl
	{
		public LadingPortUserControl()
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
				bill.ABL_RL_NKPortOfLoadingInfo.ValueChanged -= ChangeUNLOCOPortsComponentType;
				if (bill.Header is USExportAsycudaManifestHeader header)
				{
					header.AMA_TransportModeInfo.ValueChanged -= ChangeUNLOCOPortsComponentType;
				}
			}
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			var bill = (USExportAsycudaBill)CurrentDataItem;
			if (bill != null && !bill.IsDeleted)
			{
				bill.ABL_RL_NKPortOfLoadingInfo.ValueChanged += ChangeUNLOCOPortsComponentType;
				if (bill.Header is USExportAsycudaManifestHeader header)
				{
					header.AMA_TransportModeInfo.ValueChanged += ChangeUNLOCOPortsComponentType;
				}
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
				LadingPortScheduleDDropEdit.Visible = bill.ABL_CustomsLoadPortIsDropEdit;
				LadingPortScheduleDTextBox.Visible = !bill.ABL_CustomsLoadPortIsDropEdit;
			}
		}
	}
}
