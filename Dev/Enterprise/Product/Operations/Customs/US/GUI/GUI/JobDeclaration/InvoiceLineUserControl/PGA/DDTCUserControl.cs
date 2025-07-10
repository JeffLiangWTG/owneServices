using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.GUI
{
	public partial class DDTCUserControl : ZUserControl
	{
		public DDTCUserControl()
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

		public new JobComInvoiceLine CurrentDataItem
		{
			get { return (JobComInvoiceLine)base.CurrentDataItem; }
		}

		protected override void OnCurrentDataItemChanging(EventArgs e)
		{
			var invoiceLine = CurrentDataItem;
			if (invoiceLine != null)
			{
				invoiceLine.US_DDTCTrackingStatusInfo.ValueChanged -= US_DDTCTrackingStatusInfo_ValueChanged;
			}
			base.OnCurrentDataItemChanging(e);
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			var invoiceLine = CurrentDataItem;
			if (invoiceLine != null)
			{
				invoiceLine.US_DDTCTrackingStatusInfo.ValueChanged += US_DDTCTrackingStatusInfo_ValueChanged;
			}
			ChangeButtonsVisibility();
		}

		void ChangeButtonsVisibility()
		{
			var status = CurrentDataItem?.US_DDTCTrackingStatus ?? ZString.Empty;
			if (status.IsEmpty)
			{
				UpdateButton.Visible = false;
				DeleteButton.Visible = false;
			}
			else
			{
				UpdateButton.Visible = true;
				UpdateButton.Enabled = PGATrackingStatusList.CanBeChangedToBeUpdated(status);
				DeleteButton.Visible = true;
				DeleteButton.Enabled = status == PGATrackingStatusList.Codes.Added;
			}
		}

		void US_DDTCTrackingStatusInfo_ValueChanged(object sender, EventArgs e)
		{
			ChangeButtonsVisibility();
		}

		void UpdateButton_Click(object sender, EventArgs e)
		{
			var invoiceLine = CurrentDataItem;
			var status = invoiceLine?.US_DDTCTrackingStatus ?? ZString.Empty;
			if (PGATrackingStatusList.CanBeChangedToBeUpdated(status))
			{
				var releaseDate = invoiceLine?.InvoiceHeader?.JobDeclaration?.JE_EntryAuthorisationDate ?? ZDateTime.Empty;
				if (!releaseDate.IsEmpty && releaseDate < releaseDateThreshold)
				{
					if (Globals.Message.Show(ZGridPGADataCorrectionSupporter.PGAReleasedUpdateStatusChangeWarning, "Warning", MessageBoxButtons.YesNo, DialogResult.No) == DialogResult.Yes)
					{
						invoiceLine.US_DDTCTrackingStatus = PGATrackingStatusList.Codes.ToBeUpdated;
					}
				}
				else
				{
					if (Globals.Message.Show(ZGridPGADataCorrectionSupporter.UpdateStatusChangeWarning, "Warning", MessageBoxButtons.YesNo, DialogResult.No) == DialogResult.Yes)
					{
						invoiceLine.US_DDTCTrackingStatus = PGATrackingStatusList.Codes.ToBeUpdated;
					}
				}
			}
		}

		void DeleteButton_Click(object sender, EventArgs e)
		{
			var invoiceLine = CurrentDataItem;
			var status = invoiceLine?.US_DDTCTrackingStatus ?? ZString.Empty;
			if (status == PGATrackingStatusList.Codes.Added)
			{
				var releaseDate = invoiceLine?.InvoiceHeader?.JobDeclaration?.JE_EntryAuthorisationDate ?? ZDateTime.Empty;
				if (!releaseDate.IsEmpty && releaseDate < releaseDateThreshold)
				{
					if (Globals.Message.Show(ZGridPGADataCorrectionSupporter.PGAReleasedDeleteStatusChangeWarning, "Warning", MessageBoxButtons.YesNo, DialogResult.No) == DialogResult.Yes)
					{
						invoiceLine.US_DDTCTrackingStatus = PGATrackingStatusList.Codes.ToBeDeleted;
					}
				}
				else
				{
					if (Globals.Message.Show(ZGridPGADataCorrectionSupporter.DeleteStatusChangeWarning, "Warning", MessageBoxButtons.YesNo, DialogResult.No) == DialogResult.Yes)
					{
						invoiceLine.US_DDTCTrackingStatus = PGATrackingStatusList.Codes.ToBeDeleted;
					}
				}
			}
		}
	}
}
