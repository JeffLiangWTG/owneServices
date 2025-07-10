using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Definitions;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.MasterData.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterData.GUI
{
	public partial class DuplicationOrganisationCandidatesUserControl : ZUserControl, IDeduplicationCandidatesUserControl
	{
		DeduplicationOrganisationResultDetail resultDetail;
		readonly bool isAdminPanel;
		readonly bool isEdi;

		public DuplicationOrganisationCandidatesUserControl(bool isAdminPanel)
		{
			this.isAdminPanel = isAdminPanel;
			isEdi = ClientHookLoader.Instance?.Client is Clients.EDI;

			InitializeComponent();
			AddColumnsIntoGrid();
			DuplicationCandidatesGrid.AfterBind += DuplicationCandidatesGrid_AfterBind;

#if !WINZOR
			DuplicationCandidatesGrid.MouseWheel += (sender, args) =>
			{
				if (!DuplicationCandidatesGrid.IsVerticalScrollBarVisible)
				{
					var mainContentTableLayoutContainer = DuplicationCandidatesGrid.GetTopLevelNonParentedControl().Controls.Find("mainContentTableLayoutContainer", true)[0];
					if (mainContentTableLayoutContainer is ScrollableControl scrollableControl)
					{
						var minimum = scrollableControl.VerticalScroll.Minimum;
						var maximum = scrollableControl.VerticalScroll.Maximum;
						var scrollChangeStep = SystemInformation.VerticalScrollBarThumbHeight;
						if (args.Delta > 0)
						{
							if (minimum <= (scrollableControl.VerticalScroll.Value - scrollChangeStep))
							{
								scrollableControl.VerticalScroll.Value -= scrollChangeStep;

								if (minimum > scrollableControl.VerticalScroll.Value)
								{
									scrollableControl.VerticalScroll.Value = minimum;
								}
							}
							else
							{
								scrollableControl.VerticalScroll.Value = minimum;
							}
						}
						else
						{
							if (maximum >= (scrollableControl.VerticalScroll.Value + scrollChangeStep))
							{
								scrollableControl.VerticalScroll.Value += scrollChangeStep;

								if (maximum < scrollableControl.VerticalScroll.Value)
								{
									scrollableControl.VerticalScroll.Value = maximum;
								}
							}
							else
							{
								scrollableControl.VerticalScroll.Value = maximum;
							}
						}
					}
				}

				base.OnMouseWheel(args);
			};
#endif
		}

		public void SetupDataContext(IDeduplicationResultDetail resultDetail)
		{
			this.resultDetail = (DeduplicationOrganisationResultDetail)resultDetail;

			SetDataBinding(resultDetail, string.Empty);
			DuplicationCandidatesGrid.Visible = !resultDetail.IsEmpty;

			if (resultDetail.SelectedCandidatePK != ZGuid.Empty && DuplicationCandidatesGrid.ListManager != null)
			{
				var rowIndexShouldBeSelected = DuplicationCandidatesGrid.ListManager.List.Cast<DuplicationOrganisationCandidate>().ToList()
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
			var candidatePK = ((DuplicationOrganisationCandidate)DuplicationCandidatesGrid.GetCurrent())?.TargetPK ?? Guid.Empty;
			if (resultDetail.SelectedCandidatePK != candidatePK)
			{
				resultDetail.SelectedCandidatePK = candidatePK;
			}
		}

		void DuplicationCandidatesGridDoubleClickHandler(object sender, MouseEventArgs e)
		{
			if (DuplicationCandidatesGrid.IsDoubleClickOnRow(e))
			{
				resultDetail.ShowFormForCandidate(resultDetail.Factory.Load<OrgHeader>(resultDetail.SelectedCandidatePK), typeof(OrgHeader));
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

			DuplicationCandidatesGrid.ColumnStyles.Add(CodeColumn);
			DuplicationCandidatesGrid.ColumnStyles.Add(NameColumn);
			DuplicationCandidatesGrid.ColumnStyles.Add(UNLOCOColumn);
			DuplicationCandidatesGrid.ColumnStyles.Add(TypeColumn);
			DuplicationCandidatesGrid.ColumnStyles.Add(DebtorCompanyColumn);
			DuplicationCandidatesGrid.ColumnStyles.Add(CreditorCompanyColumn);
			DuplicationCandidatesGrid.ColumnStyles.Add(ActiveColumn);

			if (isAdminPanel)
			{
				DuplicationCandidatesGrid.ColumnStyles.Add(IgnoredForEveryoneStaffColumn);
				DuplicationCandidatesGrid.ColumnStyles.Add(IgnoredByStaffColumn);
				DuplicationCandidatesGrid.ColumnStyles.Add(AssociatedShipmentsCountColumn);
				DuplicationCandidatesGrid.ColumnStyles.Add(AssociatedConsolsCountColumn);
				DuplicationCandidatesGrid.ColumnStyles.Add(AssociatedDeclarationsCountColumn);

				if (isEdi)
				{
					DuplicationCandidatesGrid.ColumnStyles.Add(EnterpriseIdColumn);
					DuplicationCandidatesGrid.ColumnStyles.Add(EnterpriseCodeColumn);
					DuplicationCandidatesGrid.ColumnStyles.Add(CompanyCodeColumn);
					DuplicationCandidatesGrid.ColumnStyles.Add(ProductIdColumn);
				}
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
