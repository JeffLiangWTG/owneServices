using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.GUI
{
	public partial class ODSAndTSCAControl : ZUserControl
	{
		public ODSAndTSCAControl()
		{
			InitializeComponent();
			releaseDateThreshold = ZDateTime.Today.AddDays(-10);
			var workingDays = CustomsWorkingDays.GetInstance(new BusinessObjectFactory { NameForDebugging = "WorkingDays" });
			if (workingDays != null)
			{
				releaseDateThreshold = workingDays.GetAnotherStandardWorkingDay(ZDate.Today.ToDateTime(), -10);
			}
		}
		readonly ZDateTime releaseDateThreshold;

		public void SetPropertyForProduct()
		{
			PGAContactNameTextBox.Visible = false;
			PGAContactPhoneTextBox.Visible = false;
			PGAContactEmailTextBox.Visible = false;
			txtODSLineStatus.Visible = false;
			txtTscaLineStatus.Visible = false;
			DateEditODSStatusDate.Visible = false;
			DateEditTSCAStatusDate.Visible = false;
		}

		protected override void OnCurrentDataItemChanging(EventArgs e)
		{
			var invoiceLine = CurrentDataItem as JobComInvoiceLine;
			if (invoiceLine != null)
			{
				invoiceLine.US_ODSTrackingStatusInfo.ValueChanged -= TrackingStatusInfo_ValueChanged;
				invoiceLine.US_TSCATrackingStatusInfo.ValueChanged -= TrackingStatusInfo_ValueChanged;
			}
			base.OnCurrentDataItemChanging(e);
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			var invoiceLine = CurrentDataItem as JobComInvoiceLine;
			if (invoiceLine != null)
			{
				invoiceLine.US_ODSTrackingStatusInfo.ValueChanged += TrackingStatusInfo_ValueChanged;
				invoiceLine.US_TSCATrackingStatusInfo.ValueChanged += TrackingStatusInfo_ValueChanged;
			}
			ChangeButtonsVisibility();
		}

		void ChangeButtonsVisibility()
		{
			var invoiceLine = CurrentDataItem as JobComInvoiceLine;
			if (invoiceLine == null)
			{
				US_ODSTrackingStatusDescTextBox.Visible = false;
				US_TSCATrackingStatusDescTextBox.Visible = false;
				UpdateButton.Visible = false;
				DeleteButton.Visible = false;
			}
			else
			{
				var odsStatus = invoiceLine.US_ODSTrackingStatus;
				var tscaStatus = invoiceLine.US_TSCATrackingStatus;
				if (odsStatus.IsEmpty && tscaStatus.IsEmpty)
				{
					UpdateButton.Visible = false;
					DeleteButton.Visible = false;
				}
				else
				{
					UpdateButton.Visible = true;
					UpdateButton.Enabled = PGATrackingStatusList.CanBeChangedToBeUpdated(odsStatus) || PGATrackingStatusList.CanBeChangedToBeUpdated(tscaStatus);
					DeleteButton.Visible = true;
					DeleteButton.Enabled = odsStatus == PGATrackingStatusList.Codes.Added || tscaStatus == PGATrackingStatusList.Codes.Added;
				}
			}
		}

		void TrackingStatusInfo_ValueChanged(object sender, EventArgs e)
		{
			ChangeButtonsVisibility();
		}

		void UpdateButton_Click(object sender, EventArgs e)
		{
			var invoiceLine = CurrentDataItem as JobComInvoiceLine;
			if (invoiceLine != null)
			{
				var odsStatus = invoiceLine.US_ODSTrackingStatus;
				var tscaStatus = invoiceLine.US_TSCATrackingStatus;
				var shouldUpdateODS = PGATrackingStatusList.CanBeChangedToBeUpdated(odsStatus);
				var shouldUpdateTSCA = PGATrackingStatusList.CanBeChangedToBeUpdated(tscaStatus);

				if (shouldUpdateODS || shouldUpdateTSCA)
				{
					var releaseDate = invoiceLine.InvoiceHeader?.JobDeclaration?.JE_EntryAuthorisationDate ?? ZDateTime.Empty;
					if (!releaseDate.IsEmpty && releaseDate < releaseDateThreshold)
					{
						if (Globals.Message.Show(ZGridPGADataCorrectionSupporter.PGAReleasedUpdateStatusChangeWarning, "Warning", MessageBoxButtons.YesNo, DialogResult.No) == DialogResult.Yes)
						{
							UpdateODSAndTSCATrackingStatus(invoiceLine, shouldUpdateODS, shouldUpdateTSCA, PGATrackingStatusList.Codes.ToBeUpdated);
						}
					}
					else
					{
						if (Globals.Message.Show(ZGridPGADataCorrectionSupporter.UpdateStatusChangeWarning, "Warning", MessageBoxButtons.YesNo, DialogResult.No) == DialogResult.Yes)
						{
							UpdateODSAndTSCATrackingStatus(invoiceLine, shouldUpdateODS, shouldUpdateTSCA, PGATrackingStatusList.Codes.ToBeUpdated);
						}
					}
				}
			}
		}

		void DeleteButton_Click(object sender, EventArgs e)
		{
			var invoiceLine = CurrentDataItem as JobComInvoiceLine;
			if (invoiceLine != null)
			{
				var odsStatus = invoiceLine.US_ODSTrackingStatus;
				var tscaStatus = invoiceLine.US_TSCATrackingStatus;
				if (PGATrackingStatusList.CanBeChangedToBeUpdated(odsStatus) || PGATrackingStatusList.CanBeChangedToBeUpdated(tscaStatus))
				{
					var shouldUpdateODS = odsStatus == PGATrackingStatusList.Codes.Added;
					var shouldUpdateTSCA = tscaStatus == PGATrackingStatusList.Codes.Added;
					var releaseDate = invoiceLine.InvoiceHeader?.JobDeclaration?.JE_EntryAuthorisationDate ?? ZDateTime.Empty;

					if (!releaseDate.IsEmpty && releaseDate < releaseDateThreshold)
					{
						if (Globals.Message.Show(ZGridPGADataCorrectionSupporter.PGAReleasedDeleteStatusChangeWarning, "Warning", MessageBoxButtons.YesNo, DialogResult.No) == DialogResult.Yes)
						{
							UpdateODSAndTSCATrackingStatus(invoiceLine, shouldUpdateODS, shouldUpdateTSCA, PGATrackingStatusList.Codes.ToBeDeleted);
						}
					}
					else
					{
						if (Globals.Message.Show(ZGridPGADataCorrectionSupporter.DeleteStatusChangeWarning, "Warning", MessageBoxButtons.YesNo, DialogResult.No) == DialogResult.Yes)
						{
							UpdateODSAndTSCATrackingStatus(invoiceLine, shouldUpdateODS, shouldUpdateTSCA, PGATrackingStatusList.Codes.ToBeDeleted);
						}
					}
				}
			}
		}

		void UpdateODSAndTSCATrackingStatus(JobComInvoiceLine invoiceLine, bool shouldUpdateODS, bool shouldUpdateTSCA, ZString status)
		{
			if (shouldUpdateODS)
			{
				invoiceLine.US_ODSTrackingStatus = status;
			}
			if (shouldUpdateTSCA)
			{
				invoiceLine.US_TSCATrackingStatus = status;
			}
		}
	}
}
