using System;
using Enterprise.Customs.US.ForwarderManifest.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.ForwarderManifest.GUI
{
	public partial class ArrivalPortUserControl : ZUserControl
	{
		public ArrivalPortUserControl()
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
				header.AMA_RL_NKPortOfFirstArrivalInfo.ValueChanged -= ChangeUNLOCOPortsComponentType;
			}
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			var bill = (USExportAsycudaBill)CurrentDataItem;
			if (bill != null && !bill.IsDeleted && bill.Header is USExportAsycudaManifestHeader header)
			{
				header.AMA_RL_NKPortOfFirstArrivalInfo.ValueChanged += ChangeUNLOCOPortsComponentType;
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
				ArrivalPortScheduleKDropEdit.Visible = header.AMA_CustomsFirstArrivalPortIsDropEdit;
				ArrivalPortScheduleKTextBox.Visible = !header.AMA_CustomsFirstArrivalPortIsDropEdit;
			}
		}
	}
}
