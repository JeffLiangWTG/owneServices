using System;
using System.Diagnostics;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Module
{
	public partial class BulkScheduleCopyForm : ZChildForm
	{
		public BulkScheduleCopyForm(BulkCopyCriteria bulkCopyCriteria)
			: base(bulkCopyCriteria)
		{
			Argument.NotNull(bulkCopyCriteria, "bulkCopyCriteria");

			SetDailyPanelVisibility();
			SetWeeklyPanelVisibility();
			SetMonthlyPanelVisibility();

			CopyShipmentsCheckBox.Visible = !bulkCopyCriteria.ConsolDetails.CopyShipments_ReadOnly;
			CopyRoutingCheckBox.Visible = !bulkCopyCriteria.ConsolDetails.CopyRoutings_ReadOnly;
		}

		public BulkCopyCriteria BulkCopy
		{
			[DebuggerStepThrough]
			get { return (BulkCopyCriteria)BusinessEntity; }
		}

		public void PerformActionClick()
		{
			ActionButton.PerformClick();
		}

		#region Implementation

		protected override ZMessageBox CreateErrorMessageBox(IBusiness businessEntityForValidation, bool includeIgnoreOption)
		{
			return new ZErrorMessageBox(businessEntityForValidation,
				Res.GetString("f27b46d2-395b-4f4e-a786-83912ff7f357", "schedule"),
				Res.GetString("73c9daf5-855b-4850-9510-84cdd27e17f0", "copy"),
				Res.GetString("de2696b8-61df-40c3-8ed8-6100adf768b3", "copied"),
				includeIgnoreOption);
		}

		void SetDailyPanelVisibility()
		{
			DailyPanel.Visible = DailyRadioButton.Checked;
		}

		void BulkCopyDailyPatternInfo_ValueChanged(object sender, EventArgs e)
		{
			SetDailyPanelVisibility();
		}

		void SetWeeklyPanelVisibility()
		{
			WeeklyPanel.Visible = WeeklyRadioButton.Checked;
		}

		void BulkCopyWeeklyPatternInfo_ValueChanged(object sender, EventArgs e)
		{
			SetWeeklyPanelVisibility();
		}

		void SetMonthlyPanelVisibility()
		{
			MonthlyPanel.Visible = MonthlyRadioButton.Checked;
		}

		void BulkCopyMonthlyPatternInfo_ValueChanged(object sender, EventArgs e)
		{
			SetMonthlyPanelVisibility();
		}

		void CreateAndCopySchedule()
		{
			BulkCopy.RunPreSaveValidation();

			if (BulkCopy.HasErrors)
			{
				ShowErrorsDialog();
			}
			else
			{
				BulkCopy.Generate();

				if (BulkCopy.Schedules.Count > 0)
				{
					ZFormModaliser.ShowDialogAndDispose(new SchedulesForm(BulkCopy));
				}
				else
				{
					Globals.Message.Show(Res.GetString("a656ac4b-91b4-4857-8a7a-cfd2261f727c", "Based on the bulk copy criteria specified, no schedules have been created. This is likely to be because schedules already exist for all the dates specified."));
				}

				Close();
			}
		}

		void ActionButton_Click(object sender, EventArgs e)
		{
			CreateAndCopySchedule();
		}

		void CloseButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		#endregion
	}
}
