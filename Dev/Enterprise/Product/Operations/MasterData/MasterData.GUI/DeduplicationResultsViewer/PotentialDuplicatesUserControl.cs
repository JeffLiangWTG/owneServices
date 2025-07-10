using System.ComponentModel;
using Enterprise.MasterData.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterData.GUI
{
	public partial class PotentialDuplicatesUserControl : ZUserControl
	{
		IDeduplicationResultDetail duplicationResultDetail;

		public PotentialDuplicatesUserControl()
		{
			InitializeComponent();
		}

		public void SetupDataContext(DeduplicationOrganisationResultDetail duplicationResultDetail, bool isAdminPanel)
		{
			this.duplicationResultDetail = duplicationResultDetail;
			SetDataBinding(duplicationResultDetail, string.Empty);
			duplicationResultDetail.DuplicationCandidates.Sort("Score", ListSortDirection.Descending);
			CheckBoxTableLayoutPanel.Visible = duplicationResultDetail.ShowCheckBox;
			SetLabelCaption();
			this.duplicationResultDetail.RegisterNotifyPropertyChangeEvent(nameof(duplicationResultDetail.CandidatesCount), "UpdateTitleLabelCaption", SetLabelCaption);
			CandidatesForMergingControl.SetupDataContextCore(duplicationResultDetail, () => new DuplicationOrganisationCandidatesUserControl(isAdminPanel));
			AdvancedFilterCriteriaControl.Visible = !duplicationResultDetail.IsEmpty;
			AdvancedFilterCriteriaControl.ShowOrganisationFilterContentControl(duplicationResultDetail, this);
		}

		void SetLabelCaption()
		{
			if (duplicationResultDetail != null)
			{
				TitleLabel.Text = Enterprise.MasterData.GUI.Res.GetString("43C5D14A-34A8-4FD4-827A-49AE0FCF1F54", "Potential Duplicates (Total: {0})", duplicationResultDetail.CandidatesCount);
			}
		}

		public void SetupDataContext(DeduplicationPersonResultDetail duplicationResultDetail, bool isAdminPanel)
		{
			this.duplicationResultDetail = duplicationResultDetail;
			CheckBoxTableLayoutPanel.Visible = duplicationResultDetail.ShowCheckBox;
			CandidatesForMergingControl.SetupDataContextCore(duplicationResultDetail, () => new DuplicationPersonCandidatesUserControl(isAdminPanel));
			SetLabelCaption();
			AdvancedFilterCriteriaControl.Visible = !duplicationResultDetail.IsEmpty;
			AdvancedFilterCriteriaControl.ShowPersonFilterContentControl(duplicationResultDetail, this);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}

			base.Dispose(disposing);
		}
	}
}
