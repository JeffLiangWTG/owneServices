using System;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using static Enterprise.Freight.Integration.Forwarding;

namespace Enterprise.Freight.Forwarding.GUI
{
	public partial class BulkConsolCreationForm : ZChildForm, IBulkConsolCreationForm
	{
		protected MultiDaysSelection MultiDaysSelection { get; }

		public BulkConsolCreationForm(MultiDaysSelection multiDaysSelection)
			: base(multiDaysSelection)
		{
			MultiDaysSelection = multiDaysSelection;

			SetDailyPanelVisibility();
			SetWeeklyPanelVisibility();
			SetMonthlyPanelVisibility();
			AdjustConsolDetailsGroupBox();
			AdjustRecurrenceGroupBox();
		}

		public BulkConsolCreationForm(IMultiDaysSelection multiDaysSelection)
			: this(multiDaysSelection as MultiDaysSelection)
		{
		}

		public override string FormCaption
		{
			get
			{
				if (!MultiDaysSelection.ImportAndCreateMAWB)
				{
					return base.FormCaption;
				}
				else
				{
					if (MultiDaysSelection.IncludeWeeklyTimetable)
					{
						return Enterprise.Freight.Forwarding.GUI.Res.GetString("BulkConsolCreationForm|3da1fc07-901b-401b-a2b4-1f70e79ff2b0", "Import Schedules and create MAWBs");
					}
					else
					{
						return Enterprise.Freight.Forwarding.GUI.Res.GetString("BulkConsolCreationForm|3d683095-f4c9-43fc-a89f-939afd52cf48", "Import Schedules and create MAWB");
					}
				}
			}
		}

		public override string FormVerb => string.Empty;

		void AdjustConsolDetailsGroupBox()
		{
			createConsolUserControl.Visible = MultiDaysSelection.ImportAndCreateMAWB;
			if (MultiDaysSelection.IncludeWeeklyTimetable)
			{
				createConsolUserControl.MawbConsolsLabel.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("BulkConsolCreationForm|f8ef6bb4-98d6-4c54-ac40-d97281c89b2e", "Specify the number of MAWB/Consols to be created for the selected flight/s");
			}

			if (!MultiDaysSelection.ImportAndCreateMAWB)
			{
				var newHeight = Height - createConsolUserControl.Height;
				ControlDpiScalingHelper.SetHeight(this, newHeight, false);
			}
		}

		void AdjustRecurrenceGroupBox()
		{
			if (MultiDaysSelection.ImportAndCreateMAWB)
			{
				MultipleFlightsLabel.Visible = RecurrencePatternGroupBox.Visible = RangeOfRecurrenceGroupBox.Visible = MultiDaysSelection.RecurrenceEnabled && MultiDaysSelection.IncludeWeeklyTimetable;
				if (!MultiDaysSelection.IncludeWeeklyTimetable)
				{
					createConsolUserControl.Top = ControlDpiScalingHelper.ScaleToCurrentDpiY(0);

					var newHeight = Height - MultipleFlightsLabel.Height - RecurrencePatternGroupBox.Height - RangeOfRecurrenceGroupBox.Height;
					ControlDpiScalingHelper.SetHeight(this, newHeight, false);
				}
			}
		}

		void CloseButton_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			Close();
		}

		void SelectButton_Click(object sender, EventArgs e)
		{
			MultiDaysSelection.RunPreSaveValidation();

			if (MultiDaysSelection.HasErrors)
			{
				ShowErrorsDialog();
				return;
			}

			MultiDaysSelection.ActiveTab = this.createConsolUserControl.CreateConsolTabControl.SelectedTab.Name;

			if (MultiDaysSelection.ConsolDetails.HasChanges && MultiDaysSelection.ConsolTemplateDetails.HasChanges)
			{
				var message = MultiDaysSelection.ActiveTab == MultiDaysSelection.CreateNewConsolsTabName
					? Res.GetString("aa47f9e3-1aa3-df80-434e-daecd77ec178", "You are about to create new consols, but have entered data into the 'Create Consols From Templates' tab. Data entered into the inactive tab will be ignored - do you wish to continue?")
					: Res.GetString("af659134-61e7-4ea6-4dd0-f9a1e0b516e0", "You are about to create consols from templates, but have entered data into the 'Create New Consols' tab. Data entered into the inactive tab will be ignored - do you wish to continue?");

				var result = Globals.Message.Show(message, Res.GetString("88f2f4e4-bce1-c382-4afc-44426a84344c", "Confirmation"), MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
				if (result != DialogResult.Yes)
				{
					return;
				}
			}

			DialogResult = DialogResult.Yes;
			Close();
		}

		void SetDailyPanelVisibility()
		{
			DailyPanel.Visible = MultiDaysSelection.RecurrenceEnabled && DailyRadioButton.Checked;
		}

		void SetWeeklyPanelVisibility()
		{
			WeeklyPanel.Visible = MultiDaysSelection.RecurrenceEnabled && WeeklyRadioButton.Checked;
		}

		void SetMonthlyPanelVisibility()
		{
			MonthlyPanel.Visible = MultiDaysSelection.RecurrenceEnabled && MonthlyRadioButton.Checked;
		}

		void DailyRadioButton_CheckedChanged(object sender, EventArgs e)
		{
			SetDailyPanelVisibility();
		}

		void WeeklyRadioButton_CheckedChanged(object sender, EventArgs e)
		{
			SetWeeklyPanelVisibility();
		}

		void MonthlyRadioButton_CheckedChanged(object sender, EventArgs e)
		{
			SetMonthlyPanelVisibility();
		}
	}
}
