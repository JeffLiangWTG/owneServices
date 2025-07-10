using System;
using System.ComponentModel;
using Enterprise.Scheduler.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI.Scheduler
{
	public partial class RecurrenceControl : ZUserControl
	{
		public RecurrenceControl()
		{
			InitializeComponent();

#if DEBUG
			TypeDescriptor.AddAttributes(DailyLocalRunTimeUtcOffsetLabel, new SuppressFormsLocalizedTestAttribute());
#endif
		}

		StmScheduleTaskRecurrence Recurrence
		{
			get { return (StmScheduleTaskRecurrence)CurrentDataItem; }
		}

		#region Binding

		protected override void OnCurrentDataItemChanging(EventArgs e)
		{
			base.OnCurrentDataItemChanging(e);
			if (Recurrence != null)
			{
				Recurrence.TaskPeriodInfo.ValueChanged -= UpdatePanels;
			}
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			if (Recurrence != null)
			{
				Recurrence.TaskPeriodInfo.ValueChanged += UpdatePanels;
				UpdatePanels();
			}
		}

		void UpdatePanels(object sender, EventArgs e)
		{
			UpdatePanels();
		}

		void UpdatePanels()
		{
			AccountingPanel.Visible = Recurrence.AccountingRange;
			WeeklyPanel.Visible = Recurrence.WeeklyRange;
			YearlyPanel.Visible = Recurrence.YearlyRange;
			MonthlyPanel.Visible = Recurrence.MonthlyRange;
			DailyPanel.Visible = Recurrence.DailyRange;
		}

		#endregion

		public void HideStartTimeEditControls()
		{
			DailyRecurringStartTimeEdit.Visible = false;
			YearlyRecurringStartTimeEdit.Visible = false;
			AccountingRecurringStartTimeEdit.Visible = false;
			MonthlyRecurringStartTimeEdit.Visible = false;
			WeeklyRecurringStartTimeEdit.Visible = false;

			DailyLocalRunTimeUtcOffsetLabel.Visible = false;
			YearlyLocalRunTimeUtcOffsetLabel.Visible = false;
			AccountingLocalRunTimeUtcOffsetLabel.Visible = false;
			MonthlyLocalRunTimeUtcOffsetLabel.Visible = false;
			WeeklyLocalRunTimeUtcOffsetLabel.Visible = false;
		}
	}
}
