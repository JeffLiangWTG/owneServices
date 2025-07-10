using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.MasterData.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterData.GUI
{
	public partial class DuplicationPersonCandidatesUserControl : ZUserControl, IDeduplicationCandidatesUserControl
	{
		DeduplicationPersonResultDetail resultDetail;
		readonly bool isAdminPanel;

		public DuplicationPersonCandidatesUserControl(bool isAdminPanel)
		{
			this.isAdminPanel = isAdminPanel;

			InitializeComponent();
			AddColumnsIntoGrid();
			DuplicationCandidatesGrid.AfterBind += DuplicationCandidatesGrid_AfterBind;
		}

		public void SetupDataContext(IDeduplicationResultDetail resultDetail)
		{
			this.resultDetail = (DeduplicationPersonResultDetail)resultDetail;

			SetDataBinding(resultDetail, string.Empty);
			DuplicationCandidatesGrid.Visible = !resultDetail.IsEmpty;

			if (resultDetail.SelectedCandidatePK != ZGuid.Empty && DuplicationCandidatesGrid.ListManager != null)
			{
				var rowIndexShouldBeSelected = DuplicationCandidatesGrid.ListManager.List.Cast<DuplicationPersonCandidate>().ToList()
					.FindIndex(candidate => candidate.TargetPK == resultDetail.SelectedCandidatePK);

				if (rowIndexShouldBeSelected != 0)
				{
					DuplicationCandidatesGrid.CurrentRowIndex = rowIndexShouldBeSelected;
				}
			}
		}

		void DuplicationCandidatesGrid_AfterBind(object sender, EventArgs e)
		{
			if (DuplicationCandidatesGrid.ListManager != null)
			{
				DuplicationCandidatesGrid.ListManager.CurrentItemChanged -= GridSelectedCandidateChangedHandler;
				DuplicationCandidatesGrid.ListManager.CurrentItemChanged += GridSelectedCandidateChangedHandler;
				DuplicationCandidatesGrid.MouseDown -= DuplicationCandidatesGridDoubleClickHandler;
				DuplicationCandidatesGrid.MouseDown += DuplicationCandidatesGridDoubleClickHandler;
			}
		}

		void GridSelectedCandidateChangedHandler(object sender, EventArgs e)
		{
			var candidatePK = ((DuplicationPersonCandidate)DuplicationCandidatesGrid.GetCurrent())?.TargetPK ?? Guid.Empty;
			if (resultDetail.SelectedCandidatePK != candidatePK)
			{
				resultDetail.SelectedCandidatePK = candidatePK;
			}
		}

		void DuplicationCandidatesGridDoubleClickHandler(object sender, MouseEventArgs e)
		{
			if (DuplicationCandidatesGrid.IsDoubleClickOnRow(e))
			{
				resultDetail.ShowFormForCandidate(resultDetail.Factory.Load<GlbPerson>(resultDetail.SelectedCandidatePK), typeof(GlbPerson));
			}
		}

		void AddColumnsIntoGrid()
		{
			DuplicationCandidatesGrid.ColumnStyles.Add(ConfidenceColumn);
			DuplicationCandidatesGrid.ColumnStyles.Add(ConfidenceScoreColumn);

			if (isAdminPanel)
			{
				DuplicationCandidatesGrid.ColumnStyles.Add(StatusColumn);
			}

			DuplicationCandidatesGrid.ColumnStyles.Add(NameColumn);
			DuplicationCandidatesGrid.ColumnStyles.Add(TypeColumn);
			DuplicationCandidatesGrid.ColumnStyles.Add(PhoneColumn);
			DuplicationCandidatesGrid.ColumnStyles.Add(TitleColumn);
			DuplicationCandidatesGrid.ColumnStyles.Add(ActiveColumn);
			DuplicationCandidatesGrid.ColumnStyles.Add(RelatedToColumn);
			DuplicationCandidatesGrid.ColumnStyles.Add(IsDissolvedColumn);

			if (isAdminPanel)
			{
				DuplicationCandidatesGrid.ColumnStyles.Add(IgnoredForEveryoneStaffColumn);
				DuplicationCandidatesGrid.ColumnStyles.Add(IgnoredByStaffColumn);
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}

			DuplicationCandidatesGrid.AfterBind -= DuplicationCandidatesGrid_AfterBind;
			if (DuplicationCandidatesGrid.ListManager != null)
			{
				DuplicationCandidatesGrid.ListManager.CurrentItemChanged -= GridSelectedCandidateChangedHandler;
			}

			base.Dispose(disposing);
		}
	}
}
