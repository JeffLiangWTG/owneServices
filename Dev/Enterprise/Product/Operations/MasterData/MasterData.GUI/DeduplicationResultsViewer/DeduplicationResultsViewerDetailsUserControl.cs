using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Windows.UI;
using Enterprise.MasterData.Business;
using Enterprise.MasterData.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterData.GUI
{
	public partial class DeduplicationResultsViewerDetailsUserControl : ZUserControl
	{
		IDeduplicationResultDetail duplicationResultDetail;
		IEnumerable<DuplicationModelDetailGroup> modelDetailGroups;
		MergeControl mergeControl;
		ZButton confirmMergeButton;
		IDeduplicationMaster master;
		const string showTargetRelatedButtonsName = "ShowTargetRelatedButtons";
		bool modelDetailLoaded;
		bool isAdminPanel;

		public DeduplicationResultsViewerDetailsUserControl()
		{
			InitializeComponent();
			SetButtonText();
		}

		void SetButtonText()
		{
			MergeButton.Text = TextConstant.Merge;
			RetainMaster.Text = TextConstant.MergeAll;
			RetainCandidate.Text = TextConstant.MergeSelected;

			IgnoreSplitButton.Text = TextConstant.Ignore;
			ForMeMenuItem.Text = TextConstant.ForMe;
			ForEveryoneMenuItem.Text = TextConstant.ForEveryone;
			RemoveIgnoreMenuItem.Text = TextConstant.RemoveIgnores;

			OpenButtonSplitButton.Text = TextConstant.Open;
			PersonMenuItem.Text = TextConstant.Person;
			StaffMenuItem.Text = TextConstant.Staff;
			ContactMenuItem.Text = TextConstant.Contact;
			ApplicantMenuItem.Text = TextConstant.Applicant;

			MergeButton.Text = TextConstant.Merge;
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			if (duplicationResultDetail != null)
			{
				base.SetDataBinding(duplicationResultDetail, string.Empty);
			}
		}

		public void SetupDataContext(DeduplicationOrganisationResultDetail duplicationResultDetail, bool isAdminPanel)
		{
			if (duplicationResultDetail == null)
			{
				return;
			}
			this.duplicationResultDetail = duplicationResultDetail;
			this.master = duplicationResultDetail.Master;
			duplicationResultDetail.LoadCandidatesAndUpdateSelectedItem();
			PotentialDuplicatesUserControl.SetupDataContext(duplicationResultDetail, isAdminPanel);
			SetupResultDetail(duplicationResultDetail);
			SetMasterRelatedButtons();
			SetTargetRelatedButtons();
			RegisterNotifyPropertyChangeEventForButton(duplicationResultDetail);
		}

		public void SetupDataContext(DeduplicationPersonResultDetail duplicationResultDetail, bool isAdminPanel)
		{
			if (duplicationResultDetail == null)
			{
				return;
			}
			this.duplicationResultDetail = duplicationResultDetail;
			this.master = duplicationResultDetail.Master;
			this.isAdminPanel = isAdminPanel;
			duplicationResultDetail.LoadCandidatesAndUpdateSelectedItem();
			PotentialDuplicatesUserControl.SetupDataContext(duplicationResultDetail, isAdminPanel);
			SetupResultDetail(duplicationResultDetail);
			SetMasterRelatedButtons();
			SetTargetRelatedButtons();
			RegisterNotifyPropertyChangeEventForButton(duplicationResultDetail);
		}

		void SetMasterRelatedButtons()
		{
			OpenMasterButton.Visible = duplicationResultDetail.ShowOpenMasterButton;
			ExclusionButton.Visible = duplicationResultDetail.ShowToggleExclusionButton;
			if (ExclusionButton.Visible)
			{
				SetTextForExclusion();
			}

			DeactivateMasterButton.Visible = duplicationResultDetail.ShowDeactivateMasterButton;
		}

		void SetTargetRelatedButtons()
		{
			DeactivateCandidateButton.Visible = duplicationResultDetail.ShowDeactivateButton;
			LinkButton.Visible = duplicationResultDetail.ShowLinkButton;
			if (duplicationResultDetail is DeduplicationPersonResultDetail)
			{
				MergeButtonPanel.Visible = false;
				OpenTargetButton.Visible = false;
				var personResultDetail = duplicationResultDetail as DeduplicationPersonResultDetail;
				OpenButtonPanel.Visible = duplicationResultDetail.ShowOpenTargetButton;
				StaffMenuItem.Visible = personResultDetail.OpenStaffMenuItemVisible;
				ContactMenuItem.Visible = personResultDetail.OpenContactMenuItemVisible;
				ApplicantMenuItem.Visible = personResultDetail.OpenApplicantMenuItemVisible;
			}
			else
			{
				MergeButtonPanel.Visible = duplicationResultDetail.ShowLinkButton;
				RetainMaster.Text = Enterprise.MasterData.GUI.Res.GetString("053BDC79-3AFD-4B02-8D6C-3537E92E46A0", "Retain {0}", duplicationResultDetail.MasterCode);
				RetainCandidate.Text = Enterprise.MasterData.GUI.Res.GetString("17BC6747-3F85-4A3C-B54C-4F8457039E07", "Retain {0}", duplicationResultDetail.CandidateCode);
				OpenTargetButton.Visible = duplicationResultDetail.ShowOpenTargetButton;
				OpenButtonPanel.Visible = false;
			}
			IgnoreSplitButtonPanel.Visible = duplicationResultDetail.ShowIgnoreButton;
			ForEveryoneMenuItem.Visible = duplicationResultDetail.ShowNotMatchButton;
			RemoveIgnoreMenuItem.Visible = duplicationResultDetail.ShowRemoveIgnoresButton;
		}

		void SetTextForExclusion()
		{
			if (master != null && master.Master.IsExcludedFromDeduplication)
			{
				ExclusionButton.Text = TextConstant.Include;
			}
			else
			{
				ExclusionButton.Text = TextConstant.Exclude;
			}
		}

		void RegisterNotifyPropertyChangeEventForButton(IDeduplicationResultDetail duplicationResultDetail)
		{
			duplicationResultDetail.RegisterNotifyPropertyChangeEvent(nameof(duplicationResultDetail.ShowDeactivateMasterButton), "SetDeactivateMasterButton", () =>
				{
					DeactivateMasterButton.Visible = duplicationResultDetail.ShowDeactivateMasterButton;
				});
			duplicationResultDetail.RegisterNotifyPropertyChangeEvent(nameof(duplicationResultDetail.ShowRemoveIgnoresButton), "SetVisibleForRemoveIgnoreButton", () =>
				{
					RemoveIgnoreMenuItem.Visible = duplicationResultDetail.ShowRemoveIgnoresButton;
				});
			duplicationResultDetail.RegisterNotifyPropertyChangeEvent(nameof(master.Master.IsExcludedFromDeduplication), "SetTextForExclusion", SetTextForExclusion);
			duplicationResultDetail.RegisterNotifyPropertyChangeEvent(showTargetRelatedButtonsName, "SetTargetRelatedButtons", SetTargetRelatedButtons);
		}

		public void SetViewBusy(bool busy)
		{
			if (duplicationResultDetail is null)
			{
				return;
			}

			duplicationResultDetail.IsLoadingData = busy;
		}

		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			if (keyData is (Keys.Control | Keys.Enter))
			{
				ExecuteFilterRequest();
				return true;
			}

			return base.ProcessCmdKey(ref msg, keyData);
		}

		void SetupResultDetail(IDeduplicationResultDetail duplicationResultDetail)
		{
			CurrentSelectedCandidateChangedHandler();
			this.duplicationResultDetail.RegisterNotifyPropertyChangeEvent(nameof(duplicationResultDetail.SelectedCandidatePK), "CurrentSelectedCandidateChangedHandler", CurrentSelectedCandidateChangedHandler);
		}

		void CurrentSelectedCandidateChangedHandler()
		{
			if (IsDisposed)
			{
				return;
			}

			using (SuspendLoadingMergeMode())
			{
				ResetDynamicPanel();

				if (MergeButtonPanel.Visible)
				{
					RetainMaster.Text = $"Retain {duplicationResultDetail.MasterCode}";
					RetainCandidate.Text = $"Retain {duplicationResultDetail.CandidateCode}";
				}

				modelDetailGroups = duplicationResultDetail.GetSelectedCandidateDetails().ToArray();

				MasterInformationControl.UpdateInformationControls(modelDetailGroups, withConfidence: false, WaterMarkType.None);
				CandidateInformationControl.UpdateInformationControls(modelDetailGroups, withConfidence: true, duplicationResultDetail.WaterMark);
			}
		}

		IDisposable SuspendLoadingMergeMode()
		{
			modelDetailLoaded = false;

			return new DisposableAction(() =>
			{
				modelDetailLoaded = true;
			});
		}

		#region Implementation

		void ExecuteFilterRequest()
		{
			PotentialDuplicatesUserControl.AdvancedFilterCriteriaControl.FindSingleOrDefault<ZButton>("FindButton").PerformClick();
		}

		void RetainMaster_Click(object sender, EventArgs e) => RetainButtonClickCore(
			columnIndexForShow: 3,
			columnIndexForHide: 1,
			() => mergeControl.UpdateMergeModeControls(modelDetailGroups, CandidateInformationControl.ClonedTableLayoutRowStyles, group => group.CandidateModels),
			ConfirmMergeCandidateIntoMaster_Click);

		void RetainCandidate_Click(object sender, EventArgs e) => RetainButtonClickCore(
			columnIndexForShow: 1,
			columnIndexForHide: 3,
			() => mergeControl.UpdateMergeModeControls(modelDetailGroups, MasterInformationControl.ClonedTableLayoutRowStyles, group => group.MasterModels),
			ConfirmMergeMasterIntoCandidate_Click);

		void ConfirmMergeCandidateIntoMaster_Click(object sender, EventArgs e)
		{
			confirmMergeButton.ReadOnly = true;
			var mergeSuccess = duplicationResultDetail.ConfirmMergeCandidateIntoMaster(modelDetailGroups, group => group.CandidateModels);
			ResetDynamicPanel();
			if (mergeSuccess)
			{
				CurrentSelectedCandidateChangedHandler();
				MergeButtonPanel.Visible = duplicationResultDetail.ShowLinkButton;
			}
		}

		void ConfirmMergeMasterIntoCandidate_Click(object sender, EventArgs e)
		{
			confirmMergeButton.ReadOnly = true;
			var mergeSuccess = duplicationResultDetail.ConfirmMergeMasterIntoCandidate(modelDetailGroups, group => group.MasterModels);
			ResetDynamicPanel();
			if (mergeSuccess && duplicationResultDetail is DeduplicationOrganisationResultDetail orgResultDetail)
			{
				SetupDataContext(new DeduplicationOrganisationResultDetail(), isAdminPanel);
			}
		}

		void RetainButtonClickCore(int columnIndexForShow, int columnIndexForHide, Action updateMergeModeControlsAction, EventHandler confirmMergeClicked)
		{
			if (!modelDetailLoaded)
			{
				return;
			}

			HeaderPanel.ColumnStyles[columnIndexForHide].Width = 0;
			HeaderPanel.ColumnStyles[columnIndexForShow].Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(ContentPanel.ColumnStyles[columnIndexForShow].Width == 0 ? 150 : 0);
			ContentPanel.ColumnStyles[columnIndexForHide].Width = 0;
			ContentPanel.ColumnStyles[columnIndexForShow].Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(ContentPanel.ColumnStyles[columnIndexForShow].Width == 0 ? 150 : 0);

			if (ContentPanel.ColumnStyles[columnIndexForShow].Width > 0)
			{
				InitializeDynamicControlsIfNeeded();

				confirmMergeButton.ReadOnly = false;
				confirmMergeButton.Click -= ConfirmMergeCandidateIntoMaster_Click;
				confirmMergeButton.Click -= ConfirmMergeMasterIntoCandidate_Click;
				confirmMergeButton.Click -= confirmMergeClicked;
				confirmMergeButton.Click += confirmMergeClicked;

				updateMergeModeControlsAction.Invoke();

				HeaderPanel.Controls.Add(confirmMergeButton, columnIndexForShow, 0);
				ContentPanel.Controls.Add(mergeControl, columnIndexForShow, 0);

				HeaderPanel.PerformLayout();
				ContentPanel.PerformLayout();
			}
		}

		void ResetDynamicPanel()
		{
			HeaderPanel.ColumnStyles[1].Width = 0;
			HeaderPanel.ColumnStyles[3].Width = 0;
			ContentPanel.ColumnStyles[1].Width = 0;
			ContentPanel.ColumnStyles[3].Width = 0;

			HeaderPanel.PerformLayout();
			ContentPanel.PerformLayout();
		}

		void InitializeDynamicControlsIfNeeded()
		{
			if (mergeControl == null || confirmMergeButton == null)
			{
				confirmMergeButton = new ZButton();
				confirmMergeButton.Dock = DockStyle.Fill;
				confirmMergeButton.Margin = ControlDpiScalingHelper.NewScaledPadding(5);
				confirmMergeButton.Text = TextConstant.ConfirmMerge;
				confirmMergeButton.Name = "ConfirmMergeButton";

				mergeControl = new MergeControl();
				mergeControl.AutoSize = true;
				mergeControl.AutoSizeMode = AutoSizeMode.GrowAndShrink;
				mergeControl.Dock = DockStyle.Fill;
				mergeControl.Margin = ControlDpiScalingHelper.NewScaledPadding(0);
				mergeControl.Padding = ControlDpiScalingHelper.NewScaledPadding(4);
				mergeControl.Name = "MergeControl";
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}

			if (duplicationResultDetail != null)
			{
				duplicationResultDetail.ClearNotifyPropertyChangeEvent(nameof(duplicationResultDetail.SelectedCandidatePK), "CurrentSelectedCandidateChangedHandler");
			}

			base.Dispose(disposing);
		}

		void OnSizeChanged(object sender, EventArgs e)
		{
			if (ContentPanel != null && ContentPanel.VerticalScroll.Visible)
			{
				HeaderPanel.ColumnCount = 5;
				HeaderPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, ControlDpiScalingHelper.ScaleToCurrentDpiX(17)));
			}
			else
			{
				HeaderPanel.ColumnCount = 4;
			}

			ContentPanel?.PerformLayout();
		}

		#endregion Implementation

		void OpenMasterButton_Click(object sender, EventArgs e)
		{
			duplicationResultDetail.OpenMaster();
		}

		void ExclusionButton_Click(object sender, EventArgs e)
		{
			duplicationResultDetail.ToggleExclusion();
		}

		void DeactivateMasterButton_Click(object sender, EventArgs e)
		{
			duplicationResultDetail.DeactivateMaster();
		}

		void DeactivateCandidateButton_Click(object sender, EventArgs e)
		{
			duplicationResultDetail.DeactivateTarget();
		}

		void LinkButton_Click(object sender, EventArgs e)
		{
			duplicationResultDetail.ExecuteLink();
		}

		void PersonMenuItem_Click(object sender, EventArgs e)
		{
			duplicationResultDetail.OpenTarget(TextConstant.Person);
		}

		void StaffMenuItem_Click(object sender, EventArgs e)
		{
			duplicationResultDetail.OpenTarget(TextConstant.Staff);
		}

		void ContactMenuItem_Click(object sender, EventArgs e)
		{
			duplicationResultDetail.OpenTarget(TextConstant.Contact);
		}

		void ApplicantMenuItem_Click(object sender, EventArgs e)
		{
			duplicationResultDetail.OpenTarget(TextConstant.Applicant);
		}

		void ForMeMenuItem_Click(object sender, EventArgs e)
		{
			duplicationResultDetail.IgnoreOrNotMatch(PatternMatchingResult.StatusCodes.TemporaryIgnore, false);
		}

		void ForEveryoneMenuItem_Click(object sender, EventArgs e)
		{
			duplicationResultDetail.IgnoreOrNotMatch(PatternMatchingResult.StatusCodes.PermanentIgnore, false);
		}

		void RemoveIgnoreMenuItem_Click(object sender, EventArgs e)
		{
			duplicationResultDetail.RemoveIgnores(false);
		}

		void OpenTargetButton_Click(object sender, EventArgs e)
		{
			duplicationResultDetail.OpenTarget(null);
		}
	}
}
