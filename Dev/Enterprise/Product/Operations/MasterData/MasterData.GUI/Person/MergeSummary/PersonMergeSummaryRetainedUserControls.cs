using System.Windows.Forms;
using Enterprise.MasterData.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterData.GUI
{
	public partial class PersonMergeSummaryRetainedUserControls : ZUserControl
	{
		public PersonMergeSummaryRetainedUserControls()
		{
			InitializeComponent();
		}

		void Grid_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyData == Keys.Enter)
			{
				HandleEnterOrDoubleClick();
			}
		}

		void Grid_MouseDown(object sender, MouseEventArgs e)
		{
			if (e.Clicks == 2 && e.Button == MouseButtons.Left)
			{
				if (RetainedBoundGrid.HitTest(e.X, e.Y).Row > -1)
				{
					HandleEnterOrDoubleClick();
				}
			}
			else
			{
				ResetCurrentSelections();
			}
		}

		protected void HandleEnterOrDoubleClick()
		{
			if (((PersonMergeSummaryForm)ParentForm).IsMergingInProgress)
			{
				Globals.Message.ShowError(Res.GetString("5f010de2-03bd-414c-ad23-cdd046e19e28", "Can not show the person while the merging is in progress."));
			}
			else
			{
				var bizO = (PersonMergeBusinessObject)RetainedBoundGrid.ListManager.GetCurrent();
				var person = bizO.Person;
				if (person != null)
				{
					var personForm = new GlbPersonForm(person)
					{
						DisplayMode = ODisplayMode.ReadOnly
					};
					ZFormModaliser.ShowDialogAndDispose(personForm);
				}
			}
		}

		void ResetCurrentSelections()
		{
			var candidatesController = ((PersonMergeSummaryForm)ParentForm).Controls.Find("CandidatesControls", true)[0] as PersonMergeSummaryCandidatesUserControls;
			var candidatesGrid = candidatesController.CandidatesBoundGrid;
			candidatesGrid.UnSelectAll();
		}
	}
}
