using System.Windows.Forms;
using Enterprise.Scheduler.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Scheduler.Testing
{
	[TestedType(typeof(ZEmptyFormForBasherTest))]
	sealed class RecurrenceControlTest : ZFormBasherTest
	{
		[RequiresSTA]
		public void TestPanelVisibility()
		{
			var scheduleTask = Factory.NewWithValidTestData<StmScheduleTask>();
			scheduleTask.Recurrence.DailyRange = true;

			using (var form = new MockZForm(scheduleTask))
			{
				form.Show();
				AssertPanelVisibility(form, form.RecurrenceControl.DailyPanel);

				scheduleTask.Recurrence.WeeklyRange = true;
				AssertPanelVisibility(form, form.RecurrenceControl.WeeklyPanel);

				scheduleTask.Recurrence.MonthlyRange = true;
				AssertPanelVisibility(form, form.RecurrenceControl.MonthlyPanel);

				scheduleTask.Recurrence.AccountingRange = true;
				AssertPanelVisibility(form, form.RecurrenceControl.AccountingPanel);

				scheduleTask.Recurrence.YearlyRange = true;
				AssertPanelVisibility(form, form.RecurrenceControl.YearlyPanel);
			}
		}

		public void TestValidationShowErrorOnControl()
		{
			var scheduleTask = Factory.NewWithValidTestData<StmScheduleTask>();
			scheduleTask.Recurrence.DailyDay = true;

			using (var form = new MockZForm(scheduleTask))
			{
				form.Show();
				AssertPanelVisibility(form, form.RecurrenceControl.DailyPanel);

				var recurrenceControl = form.RecurrenceControl;
				recurrenceControl.DailyDaysNumber.Text = "0";

				AssertEquals("Pre-Validation TaskPeriodCount has no error. ", false, scheduleTask.Recurrence.TaskPeriodCountInfo.HasError("Please enter a value."));
				form.FireSaveButton();

				AssertEquals("Post-Validation TaskPeriodCount has error. ", true, scheduleTask.Recurrence.TaskPeriodCountInfo.HasError("Please enter a value."));

				AssertEquals("Control backcolor should have turned red.", "ffffd7d7", recurrenceControl.DailyDaysNumber.BackColor.Name);
			}
		}

		public void TestHideStartTimeEditControls()
		{
			using (var control = new RecurrenceControl())
			{
				control.HideStartTimeEditControls();

				AssertEquals(false, control.DailyRecurringStartTimeEdit.Visible);
				AssertEquals(false, control.YearlyRecurringStartTimeEdit.Visible);
				AssertEquals(false, control.AccountingRecurringStartTimeEdit.Visible);
				AssertEquals(false, control.MonthlyRecurringStartTimeEdit.Visible);
				AssertEquals(false, control.WeeklyRecurringStartTimeEdit.Visible);

				AssertEquals(false, control.DailyLocalRunTimeUtcOffsetLabel.Visible);
				AssertEquals(false, control.YearlyLocalRunTimeUtcOffsetLabel.Visible);
				AssertEquals(false, control.AccountingLocalRunTimeUtcOffsetLabel.Visible);
				AssertEquals(false, control.MonthlyLocalRunTimeUtcOffsetLabel.Visible);
				AssertEquals(false, control.WeeklyLocalRunTimeUtcOffsetLabel.Visible);
			}
		}

		public void TestEndAfterOccurrencesNotAllowNegativeCount()
		{
			using (var control = new RecurrenceControl())
			{
				var occurrencesControl = control.rrNumber;
				occurrencesControl.Focus();
				AssertEquals(false, occurrencesControl.AllowNegative);
			}
		}

		protected override Form GetFormToBashCore()
		{
			return new MockZForm(Factory.NewWithValidTestData<StmScheduleTask>());
		}

		void AssertPanelVisibility(MockZForm form, ZPanel visiblePanel)
		{
			ZPanel[] panels =
				{
					form.RecurrenceControl.DailyPanel,
					form.RecurrenceControl.WeeklyPanel,
					form.RecurrenceControl.MonthlyPanel,
					form.RecurrenceControl.AccountingPanel,
					form.RecurrenceControl.YearlyPanel
				};

			foreach (ZPanel panel in panels)
			{
				AssertEquals(panel.Name + ".Visible", (panel == visiblePanel), panel.Visible);
			}
		}

		#region class MockZForm

		class MockZForm : ZEmptyFormForBasherTest
		{
			public MockZForm(StmScheduleTask businessEntity)
				: base(businessEntity)
			{
				this.CaptionRenderingEnabled = true;
				InitializeComponent();
			}

			public RecurrenceControl RecurrenceControl
			{
				get { return recurrenceControl; }
			}

			new void InitializeComponent()
			{
				recurrenceControl = new RecurrenceControl();
				BindingSource.SetBindingMember(recurrenceControl, "Recurrence");
				Controls.Add(recurrenceControl);
				ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(920, 220, true);
			}

			RecurrenceControl recurrenceControl;
		}

		#endregion
	}
}
