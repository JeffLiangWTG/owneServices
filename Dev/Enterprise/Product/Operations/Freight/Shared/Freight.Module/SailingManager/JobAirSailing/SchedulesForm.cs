using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.Windows.UI;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Module
{
	public partial class SchedulesForm : ZChildForm
	{
		public SchedulesForm(BulkCopyCriteria bulkCopyCriteria)
			: base(bulkCopyCriteria)
		{
			this.bulkCopy = bulkCopyCriteria;
		}

		#region Implementation

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			Schedules.Text = Res.GetString("SchedulesForm|Schedules", "{0} Schedule(s) will be created.", bulkCopy.Schedules.Count);

			if (bulkCopy.ConsolDetails.CreateConsol)
			{
				ConsolGroupBox.Text = Res.GetString("SchedulesForm|ConsolGroupBox", "{0} Consolidation(s) will be created.", bulkCopy.ConsolDetails.CreatedConsols.Count);
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

		readonly BulkCopyCriteria bulkCopy;

#if DEBUG
		public ZButton SaveButtonForTest
		{
			get { return SaveButton; }
		}
#endif

		#endregion

		#region Events

		void CancelButton_Click(object sender, EventArgs e)
		{
			Close();
			Dispose();
		}

		void SaveButton_Click(object sender, EventArgs e)
		{
			if (bulkCopy.ConsolDetails.CreatedConsols.OfType<IMAWBAllocationParent>().Any(c => c.MasterBillMAWBInfo.HasErrors()))
			{
				Globals.Message.ShowError(ResString.GetMultilingualString("6478bd25-0f33-4c38-a008-b289e38c82c1",
					"Consolidations could not be created with errors on the MAWB."));
			}
			else
			{
				try
				{
					bulkCopy.Factory.Save();
				}
				catch (MAWBAllocationException ex)
				{
					Globals.Message.ShowError(ex.Message, ex.Heading);
				}
			}

			SaveButton.Visible = false;
			CancelButton.CaptionResourceString = Res.GetData("92850bd1-99f3-4495-9876-edb216b84144", "Close");
			CancelButton.UpdateCaption();
		}

		#endregion
	}
}
