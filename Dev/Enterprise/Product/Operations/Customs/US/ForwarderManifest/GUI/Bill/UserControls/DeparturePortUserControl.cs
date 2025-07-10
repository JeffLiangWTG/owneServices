using System;
using Enterprise.Customs.US.ForwarderManifest.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.ForwarderManifest.GUI
{
	public partial class DeparturePortUserControl : ZUserControl
	{
		public DeparturePortUserControl()
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
			if (bill != null && !bill.IsDeleted && bill.Header is USExportAsycudaManifestHeader header)
			{
				header.AMA_RL_NKPortOfFinalDepartureInfo.ValueChanged -= ChangeUNLOCOPortsComponentType;
				header.AMA_TransportModeInfo.ValueChanged -= ChangeUNLOCOPortsComponentType;
			}
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			var bill = (USExportAsycudaBill)CurrentDataItem;
			if (bill != null && !bill.IsDeleted && bill.Header is USExportAsycudaManifestHeader header)
			{
				header.AMA_RL_NKPortOfFinalDepartureInfo.ValueChanged += ChangeUNLOCOPortsComponentType;
				header.AMA_TransportModeInfo.ValueChanged += ChangeUNLOCOPortsComponentType;
			}
		}

		void ChangeUNLOCOPortsComponentType(object sender, EventArgs e)
		{
			ChangeUNLOCOPortsComponentVisibility();
		}

		void ChangeUNLOCOPortsComponentVisibility()
		{
			var bill = (USExportAsycudaBill)CurrentDataItem;
			if (bill != null && !bill.IsDeleted && bill.Header is USExportAsycudaManifestHeader header)
			{
				DeparturePortScheduleDDropEdit.Visible = header.AMA_CustomsFinalDeparturePortIsDropEdit;
				DeparturePortScheduleDTextBox.Visible = !header.AMA_CustomsFinalDeparturePortIsDropEdit;
			}
		}
	}
}
