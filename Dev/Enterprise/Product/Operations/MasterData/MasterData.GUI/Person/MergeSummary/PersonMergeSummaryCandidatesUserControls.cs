using System.Windows.Forms;
using Enterprise.MasterData.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterData.GUI
{
	public partial class PersonMergeSummaryCandidatesUserControls : ZUserControl
	{
		public PersonMergeSummaryCandidatesUserControls()
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
				if (CandidatesBoundGrid.HitTest(e.X, e.Y).Row > -1)
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
				var bizO = (PersonMergeBusinessObject)CandidatesBoundGrid.ListManager.GetCurrent();
				var person = bizO.Person;
				if (person == null || person.IsDeleted || person.IsDeleting)
				{
					Globals.Message.ShowError(Res.GetString("b7f901c3-dabb-48b8-9730-37b29e2b9b3e", "Can not show the person that is being deleted or has been deleted."));
				}
				else
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
			var retainedController = ((PersonMergeSummaryForm)ParentForm).Controls.Find("RetainedControls", true)[0] as PersonMergeSummaryRetainedUserControls;
			var retainedGrid = retainedController.RetainedBoundGrid;
			retainedGrid.UnSelectAll();

			CandidatesBoundGrid.UnSelectAll();
		}
	}
}
