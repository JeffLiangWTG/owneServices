using System.Drawing;
using CargoWise.Windows.UI;
using Enterprise.ProcessManagement.Integration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ProcessManagement.GUI
{
	public class JiraImporterProgressForm : ProgressForm
	{
		public JiraImporterProgressForm(string initialMessage, IJiraImporterProgressTracker tracker)
			: base(initialMessage)
		{
			Text = ResString.GetMultilingualString("f152a58b-a420-4b2f-bdd6-28b62ca928f0", "Jira Data Import");
			ControlDpiScalingHelper.SetHeight(ProgressLabel, 200, true);
			ProgressLabel.TextAlign = ContentAlignment.TopLeft;
			ShowCancelButton = false;

			tracker.ProgressUpdated += ViewOnProgressUpdated;
			this.tracker = tracker;
		}

		void ViewOnProgressUpdated(object sender, JiraProgressUpdatedEventArgs e)
		{
			SetStatusAndPercentComplete(e.Status, e.Percentage);
		}

		readonly IJiraImporterProgressTracker tracker;

		protected override void Dispose(bool isNotFinalizing)
		{
			if (isNotFinalizing && tracker != null)
			{
				tracker.ProgressUpdated -= ViewOnProgressUpdated;
			}

			base.Dispose(isNotFinalizing);
		}

		#region For Test

		JiraImporterProgressForm()
		{
		}

		public static JiraImporterProgressForm CreateNonfunctional_ForBasherTestOnly()
		{
			return new JiraImporterProgressForm();
		}

		#endregion
	}
}
