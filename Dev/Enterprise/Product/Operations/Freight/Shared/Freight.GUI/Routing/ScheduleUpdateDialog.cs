using System;
using CargoWise.Common;
using CargoWise.Windows.UI;
using Enterprise.Freight.Common.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.GUI
{
	public partial class ScheduleUpdateDialog : KForm, ICaptionRenderingSupport
	{
		SecurityCheckpoint AddCheckpoint { get; set; }
		SecurityCheckpoint EditCheckpoint { get; set; }

		#region ShowDialog

		public static SailingManagerUpdateMode ShowDialog(QueryFreshMatchBehaviourArgs queryArgs)
		{
			using (ScheduleUpdateDialog form = new ScheduleUpdateDialog())
			{
#if DEBUG
				form.result = ResultForTesting.Value;
#endif
				form.textLabel.Text = Res.GetString("C0A86B9B-D105-404A-AAC9-D4D7F44AA065", "{3} has found a more appropriate schedule to link to, but this new schedule has a different {0}.\r\nDo you want to keep the schedules existing date, update it or create an entirely new schedule?\r\n\r\nSchedule {0}: {1:dd-MMM-yy HH:mm}\r\nEntered {0}: {2:dd-MMM-yy HH:mm}", queryArgs.DateName, queryArgs.FoundDate, queryArgs.RequestedDate, Core.Constants.ProductName);
				form.AddCheckpoint = queryArgs.AddCheckpoint;
				form.EditCheckpoint = queryArgs.EditCheckpoint;

				ZFormModaliser.ShowDialogWithoutDispose(form);
				return form.result;
			}
		}

		#endregion

		internal ScheduleUpdateDialog()
		{
			InitializeComponent();
			this.Text = Res.GetString("2973c337-704f-44d4-bd82-e22f9f8f5b5f", "Linked to Schedule");
		}

		#region Events

		void keepScheduleDateButton_Click(object sender, EventArgs e)
		{
			result = SailingManagerUpdateMode.ScheduleUnchanged;
			Close();
		}

		void updateScheduleDateButton_Click(object sender, EventArgs e)
		{
			if (EditCheckpoint != null && !EditCheckpoint.IsAllowed)
			{
				EditCheckpoint.ShowError();
			}
			else
			{
				result = SailingManagerUpdateMode.UpdateSchedule;
				Close();
			}
		}

		void createNewScheduleButton_Click(object sender, EventArgs e)
		{
			if (AddCheckpoint != null && !AddCheckpoint.IsAllowed)
			{
				AddCheckpoint.ShowError();
			}
			else
			{
				result = SailingManagerUpdateMode.NewSchedule;
				Close();
			}
		}

		#endregion

		#region ForTesting

#if DEBUG
		public static readonly Overridable<SailingManagerUpdateMode> ResultForTesting = new Overridable<SailingManagerUpdateMode>(SailingManagerUpdateMode.ScheduleUnchanged);
#endif

		#endregion

		SailingManagerUpdateMode result = SailingManagerUpdateMode.ScheduleUnchanged;

		bool? ICaptionRenderingSupport.CaptionRenderingEnabled
		{
			get { return true; }
		}

		event EventHandler ICaptionRenderingSupport.CaptionRenderingEnabledChanged
		{
			add { }
			remove { }
		}
	}
}
