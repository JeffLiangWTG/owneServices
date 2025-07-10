#pragma warning disable 0809

using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using static Enterprise.Freight.Integration.Forwarding;

namespace Enterprise.Freight.Forwarding.GUI
{
	public partial class SchedulesConsolsForm : ZChildForm, ISchedulesConsolsForm
	{
		public SchedulesConsolsForm(MultiDaysSelection multiDaysSelection)
			: base(multiDaysSelection)
		{
			this.multiDaysSelection = multiDaysSelection;
		}

		public SchedulesConsolsForm(IMultiDaysSelection multiDaysSelection)
			: this(multiDaysSelection as MultiDaysSelection)
		{
		}

		readonly MultiDaysSelection multiDaysSelection;
		bool ImportIsDone;

		#region Implementation

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			Schedules.Text = Res.GetString("SchedulesConsolsForm|Schedules", "{0} Schedule(s) will be created.", multiDaysSelection.SailingCollection.Count);
			var consolsCreated = multiDaysSelection.CreatedConsols.Count;
			if (consolsCreated != 0)
			{
				ConsolGroupBox.Text = Res.GetString("SchedulesConsolsForm|ConsolGroupBox", "{0} Consolidation(s) will be created.", consolsCreated);
				ControlDpiScalingHelper.SetHeight(this, Schedules.Height * 2 + ControlDpiScalingHelper.ScaleToCurrentDpiY(40), false);
				MinimumSize = ControlDpiScalingHelper.NewScaledSize(MinimumSize.Width, Height, false);
			}
			else
			{
				ControlDpiScalingHelper.SetHeight(this, Schedules.Height + ControlDpiScalingHelper.ScaleToCurrentDpiY(20), false);
				Schedules.Anchor = Schedules.Anchor | AnchorStyles.Bottom;
				ConsolGroupBox.Visible = false;
			}
		}

		#endregion

		#region Events

		void CancelButton_Click(object sender, EventArgs e)
		{
			DialogResult = ImportIsDone ? DialogResult.OK : DialogResult.Cancel;
			Close();
		}

		bool ValidateConsols()
		{
			if (multiDaysSelection.CreatedConsols.Count > 0)
			{
				if (multiDaysSelection.CreatedConsols.OfType<IMAWBAllocationParent>().Any(c => c.MasterBillMAWBInfo.HasErrors()))
				{
					Globals.Message.ShowError(ResString.GetMultilingualString("E9DFA953-F007-431E-8375-7CEFA3C1BFED",
						"Consolidations could not be created with errors on the MAWB."));

					return false;
				}

				if (multiDaysSelection.CreatedConsols.Any(c => c.HasErrors()))
				{
					Globals.Message.ShowError(ResString.GetMultilingualString("9bf1dc78-de28-409e-a939-8542f0bff846",
						"Consolidations could not be created with errors."));

					return false;
				}
			}

			return true;
		}

		void SaveButton_Click(object sender, EventArgs e)
		{
			if (ValidateConsols())
			{
				try
				{
					FireSaveButton();
					ImportIsDone = true;
				}
				catch (MAWBAllocationException ex)
				{
					Globals.Message.ShowError(ex.Message, ex.Heading);
				}
				catch (ZSaveConcurrencyException)
				{
					try
					{
						ZExceptionReporting.ProcessWithSaveExceptionHandling(base.BusinessEntity.Factory.Save, null, true);
					}
					catch (ZSaveConcurrencyException)
					{
						Globals.Message.Show(ResString.GetMultilingualString("3D819D35-AC6C-43EF-85CF-BA4A38D70C3D", @"While you have been working with this form, another user has made changes to the same schedules."));
					}
				}
			}

			ConsolGroupBox.Text = Res.GetString("5dd911e8-7cf7-471f-9b01-b6c1012d5062", "{0} Consolidation(s) have been created.", multiDaysSelection.CreatedConsols.Count);
			SaveButton.Visible = false;
			CancelButton.Text = Res.GetString("10D66116-1169-4473-B3EF-1077A22AF854", "Close");
		}

		#endregion
	}
}
