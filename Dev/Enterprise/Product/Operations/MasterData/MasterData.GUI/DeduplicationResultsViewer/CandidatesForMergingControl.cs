using System;
using System.Linq;
using System.Windows.Forms;
using Enterprise.MasterData.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterData.GUI
{
	public partial class CandidatesForMergingControl : ZUserControl
	{
		IDeduplicationResultDetail duplicationResultDetail;
		IDeduplicationCandidatesUserControl candidatesUserControl;

		public CandidatesForMergingControl()
		{
			InitializeComponent();
			SetButtonText();
		}

		internal void SetupDataContextCore(IDeduplicationResultDetail duplicationResultDetail, Func<IDeduplicationCandidatesUserControl> getCandidatesUserControl)
		{
			this.duplicationResultDetail = duplicationResultDetail;
			if (candidatesUserControl is null)
			{
				candidatesUserControl = getCandidatesUserControl.Invoke();
				AddNewCandidatesUserControlIntoPanel();
			}
			Visible = false;
			candidatesUserControl.SetupDataContext(duplicationResultDetail);
			SetVisibleForButton();
			UpdateDuplicationCandidatesLabelCaption();
			this.duplicationResultDetail.RegisterNotifyPropertyChangeEvent(nameof(duplicationResultDetail.ShowIgnoreAllButton), "SetVisibleForIgnoreAllButton", SetVisibleForIgnoreAllButton);
			this.duplicationResultDetail.RegisterNotifyPropertyChangeEvent(nameof(duplicationResultDetail.ShowRemoveIgnoresButton), "SetVisibleForIgnoreAllButton", SetVisibleForIgnoreAllButton);
			this.duplicationResultDetail.RegisterNotifyPropertyChangeEvent(nameof(duplicationResultDetail.CandidatesCount), "UpdateDuplicationCandidatesLabelCaption", UpdateDuplicationCandidatesLabelCaption);
			Visible = true;
		}

		void SetButtonText()
		{
			IgnoreButton.Text = TextConstant.IgnoreAll;
			ForMeMenuItem.Text = TextConstant.ForMe;
			ForEveryoneMenuItem.Text = TextConstant.ForEveryone;
			RemoveIgnoreMenuItem.Text = TextConstant.RemoveIgnores;
		}

		void SetVisibleForButton()
		{
			if (duplicationResultDetail == null)
			{
				return;
			}

			SetVisibleForIgnoreAllButton();

			MergeButton.Visible = duplicationResultDetail.ShowMergeAllSelectedButton;
			if (duplicationResultDetail.ShowMergeAllSelectedButton)
			{
				SetMergeAllSelectedButtonEnable();
				duplicationResultDetail.RegisterNotifyPropertyChangeEvent(nameof(duplicationResultDetail.EnableMergeAllSelectedButton), "SetMergeAllSelectedButtonEnable", SetMergeAllSelectedButtonEnable);
			}

			SelectButton.Visible = duplicationResultDetail.ShowSelectAll;
		}

		void SetVisibleForIgnoreAllButton()
		{
			if (duplicationResultDetail != null)
			{
				SplitButtonBorderPanel.Visible = duplicationResultDetail.ShowIgnoreAllButton;
				if (duplicationResultDetail.ShowIgnoreAllButton)
				{
					ForEveryoneMenuItem.Visible = duplicationResultDetail.ShowNotMatchButton;
					RemoveIgnoreMenuItem.Visible = duplicationResultDetail.ShowRemoveAllIgnoresButton;
				}
			}
		}

		void SetMergeAllSelectedButtonEnable()
		{
			MergeButton.Enabled = duplicationResultDetail.EnableMergeAllSelectedButton;
		}

		void UpdateDuplicationCandidatesLabelCaption()
		{
			if (duplicationResultDetail == null || duplicationResultDetail.IsEmpty)
			{
				TitleLabel.Visible = false;
			}
			else
			{
				if (duplicationResultDetail.CandidatesCount == 0)
				{
					TitleLabel.Text = Enterprise.MasterData.GUI.Res.GetString("9237c93b-ccad-4e42-a564-70088c56db52", "No Duplicates");
				}
				else if (duplicationResultDetail is DeduplicationPersonResultDetail personResultDetail)
				{
					TitleLabel.Text = Enterprise.MasterData.GUI.Res.GetString("F3868650-C840-446A-9023-E2679E7EA75E", "Candidates for Merging ({0} of {1})", duplicationResultDetail.CandidatesCount, personResultDetail?.UnfilteredResults.Count());
				}
				else if (duplicationResultDetail is DeduplicationOrganisationResultDetail organisationResultDetail)
				{
					TitleLabel.Text = Enterprise.MasterData.GUI.Res.GetString("55ed96eb-ad72-45d7-886f-e830e78a7729", "Candidates For Merging ({0} of {1})", organisationResultDetail.DuplicationCandidates.Count, organisationResultDetail.UnfilteredResults.Count());
				}

				TitleLabel.Visible = true;
			}

			TitleLabel.UpdateCaption();
		}

		void AddNewCandidatesUserControlIntoPanel()
		{
			if (candidatesUserControl is ZUserControl userControl)
			{
				userControl.Dock = DockStyle.Fill;
				userControl.Name = "CandidatesUserControl";
				userControl.AutoSize = true;
				userControl.AutoSizeMode = AutoSizeMode.GrowAndShrink;

				ContentPanel.Controls.Add(userControl);
			}
		}

		void SelectButton_Click(object sender, EventArgs e)
		{
			duplicationResultDetail.SelectOrDeSelectAll();
			MergeButton.Enabled = duplicationResultDetail.EnableMergeAllSelectedButton;
		}

		void MergeButton_Click(object sender, EventArgs e)
		{
			duplicationResultDetail.MergeAllSelected();
		}

		void ForMeMenuItem_Click(object sender, EventArgs e)
		{
			duplicationResultDetail.IgnoreOrNotMatch(PatternMatchingResult.StatusCodes.TemporaryIgnore, true);
		}

		void ForEveryoneMenuItem_Click(object sender, EventArgs e)
		{
			duplicationResultDetail.IgnoreOrNotMatch(PatternMatchingResult.StatusCodes.PermanentIgnore, true);
		}

		void RemoveIgnoreMenuItem_Click(object sender, EventArgs e)
		{
			duplicationResultDetail.RemoveIgnores(true);
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
