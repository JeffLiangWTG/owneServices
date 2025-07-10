using System;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.GUI
{
	public partial class SanctionsUserControl : ZUserControl
	{
		public SanctionsUserControl()
		{
			InitializeComponent();
		}

		void CopyNMFSDataToSanctions_Click(object sender, EventArgs e)
		{
			if (CurrentDataItem is JobComInvoiceLine invoiceLine)
			{
				invoiceLine.CopyNMFSDataToSanctions();
			}
		}

		protected override void OnCurrentDataItemChanging(EventArgs e)
		{
			if (CurrentDataItem is JobComInvoiceLine invoiceLine)
			{
				invoiceLine.JI_TariffInfo.ValueChanged -= new EventHandler(TariffInfo_ValueChanged);
				invoiceLine.US_UC_NKCountryOfOriginInfo.ValueChanged -= new EventHandler(TariffInfo_ValueChanged);
			}
			base.OnCurrentDataItemChanging(e);
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			if (CurrentDataItem is JobComInvoiceLine invoiceLine)
			{
				invoiceLine.JI_TariffInfo.ValueChanged += new EventHandler(TariffInfo_ValueChanged);
				invoiceLine.US_UC_NKCountryOfOriginInfo.ValueChanged += new EventHandler(TariffInfo_ValueChanged);
			}
		}

		void TariffInfo_ValueChanged(object sender, EventArgs e)
		{
			if (CurrentDataItem is JobComInvoiceLine invoiceLine)
			{
				ChangeSanctionsGroupVisibility(invoiceLine.TariffMatchesFishingCondition, invoiceLine.TariffMatchesMiningCondition);
			}
		}

		internal void ChangeSanctionsGroupVisibility(bool isFishingVisible, bool isMiningVisible)
		{
			FishingInformationGroupBox.Visible = isFishingVisible;

			MiningInformationGroupBox.Visible = isMiningVisible;

			CopyNMFSDataToSanctionsButton.Visible = isFishingVisible;
		}

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);
			if (CurrentDataItem is JobComInvoiceLine invoiceLine)
			{
				ChangeSanctionsGroupVisibility(invoiceLine.TariffMatchesFishingCondition, invoiceLine.TariffMatchesMiningCondition);
			}
		}
	}
}
