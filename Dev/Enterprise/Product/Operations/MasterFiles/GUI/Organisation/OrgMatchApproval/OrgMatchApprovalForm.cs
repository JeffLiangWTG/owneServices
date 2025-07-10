using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.Windows.UI;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.GUI
{
	public partial class OrgMatchApprovalForm : ZChildForm, IPreviousNextControlOverrideProvider
	{
		public OrgMatchApprovalForm()
		{
		}

		public OrgMatchApprovalForm(OrgMatchApproval businessEntity)
			: base(businessEntity)
		{
			MinimumSize = Size;

			businessEntity.P2_OH_MatchOrg1Info.ValueChanged += UpdateStatusBar;
			businessEntity.P2_OH_MatchOrg2Info.ValueChanged += UpdateStatusBar;
			businessEntity.P2_MatchUser1Info.ValueChanged += UpdateStatusBar;
			businessEntity.P2_MatchUser2Info.ValueChanged += UpdateStatusBar;
			businessEntity.UpdatedWithDataRefresh += UpdateStatusBar;

			Text += " - " + businessEntity.HumanReadableName;

#if DEBUG
			TypeDescriptor.AddAttributes(OwnerCodeBoundTextBox, new SuppressFormsLocalizedTestAttribute());
#endif

			OrgAddressGrid.GetColumnStyle(OrgAddressSchema.OA_Code.Name).CaptionResourceString = Res.GetData("OrgMatchApprovalForm|0DAF5395-EE07-4ac5-B6DC-B09129594D51", "Address Short Code");
			OrgAddressGrid.GetColumnStyle(OrgAddressSchema.OA_CompanyNameOverride.Name).CaptionResourceString = Res.GetData("OrgMatchApprovalForm|2FEF06F9-7D78-4e45-A44A-DA3BF1370EBA", "Company Name Override");

			ManuallySelectedOrganisationGuidFindBox.AllowOverlap(SimilarOrgMatchesModuleButtonGrid);
		}

		public new OrgMatchApproval BusinessEntity
		{
			get { return (OrgMatchApproval)base.BusinessEntity; }
		}

		#region Implementation

		protected override void Dispose(bool isDisposing)
		{
			if (isDisposing)
			{
				UnsubscribeHandlers();
			}
			base.Dispose(isDisposing);
		}

		void UnsubscribeHandlers()
		{
			if (BusinessEntity != null)
			{
				BusinessEntity.P2_OH_MatchOrg1Info.ValueChanged -= UpdateStatusBar;
				BusinessEntity.P2_OH_MatchOrg2Info.ValueChanged -= UpdateStatusBar;
				BusinessEntity.P2_MatchUser1Info.ValueChanged -= UpdateStatusBar;
				BusinessEntity.P2_MatchUser2Info.ValueChanged -= UpdateStatusBar;
				BusinessEntity.UpdatedWithDataRefresh -= UpdateStatusBar;
			}
		}

		#region Form Caption

		public override string FormVerb
		{
			get { return ""; }
		}

		public override string FormCaption
		{
			get { return BusinessEntity.HumanReadableName; }
		}

		#endregion

		#region Supervisor Only Button Clicks

		void OnSupervisorApprove_Click(object sender, EventArgs e)
		{
			if (SimilarOrgMatchesModuleButtonGrid.InnerGrid.SelectedElements.Length == 0 || SimilarOrgMatchesModuleButtonGrid.InnerGrid.SelectedElements.Length > 1)
			{
				Globals.Message.ShowError(Res.GetString("672397e6-036d-41b5-9650-d773b7756573", "Please select a suitable organization match in the grid"));
			}
			else
			{
				SimilarOrgMatchForApproval match = (SimilarOrgMatchForApproval)SimilarOrgMatchesModuleButtonGrid.InnerGrid.SelectedElements[0];
				try
				{
					BusinessEntity.ApproveMatchBySupervisorAndSaveAtomically(match.OrgPatternMatch.Header);
				}
				catch (OrgMatchApproval.MatchingException ex)
				{
					Globals.Message.ShowError(ex.Message);
				}
			}
		}

		void OnNewOrganisation_Click(object sender, EventArgs e)
		{
			if (BusinessEntity.IsApproved)
			{
				Globals.Message.ShowError(Res.GetString("2ff1ccaa-7c2f-4a35-a6bf-4a4222b504e2", "You shouldn't create a new organization now that a match has been approved"));
			}
			else
			{
				ShowCreateNewOrganisationForm();
			}
		}

		protected virtual IOrgMatchApprovalCreateNewOrgController NewOrgMatchApprovalCreateNewOrgController()
		{
			return (IOrgMatchApprovalCreateNewOrgController)ZControllerFactory.Create(ControllerIDs.OrgMatchApprovalCreateNewOrg);
		}

		#endregion

		#region Match Operators Button Clicks

		void OnNoMatchFound_Click(object sender, EventArgs e)
		{
			try
			{
				BusinessEntity.NotifyNoMatchFoundAndSaveAtomically();
				if (BusinessEntity.ShouldCreateTemporaryOrganisation())
				{
					ShowCreateNewOrganisationForm();
				}
				else
				{
					MoveToNextRecordIfPossible();
				}
			}
			catch (OrgMatchApproval.MatchingException ex)
			{
				Globals.Message.ShowError(ex.Message);
			}
		}

		public void OnMatch_Click(object sender, EventArgs e)
		{
			if (SimilarOrgMatchesModuleButtonGrid.InnerGrid.SelectedElements.Length != 1)
			{
				Globals.Message.ShowError(Res.GetString("672397e6-036d-41b5-9650-d773b7756573", "Please select a suitable organization match in the grid"));
			}
			else
			{
				SimilarOrgMatchForApproval match = (SimilarOrgMatchForApproval)SimilarOrgMatchesModuleButtonGrid.InnerGrid.SelectedElements[0];
				if (RevertModuleFilterToUnmatchedByCurrentUser())
				{
					try
					{
						BusinessEntity.MatchAndSaveAtomically(match.OrgPatternMatch.OS_OH);
						MoveToNextRecordIfPossible();
					}
					catch (OrgMatchApproval.MatchingException ex)
					{
						Globals.Message.ShowError(ex.Message);
					}
				}
			}
		}

		bool RevertModuleFilterToUnmatchedByCurrentUser()
		{
			bool result = true;
			IOrgMatchApprovalModule module = (IOrgMatchApprovalModule)ZCurrentModules.Instance.GetCurrentModule(ModuleIDs.OrgMatchApproval);
			if (module != null)
			{
				result = module.RevertFilterToUnmatchedByCurrentUserAfterWarningUser();
			}
			else
			{
				using (module = (IOrgMatchApprovalModule)ZModuleFactory.Instance.Create(ModuleIDs.OrgMatchApproval))
				{
					module.RevertFilterToUnmatchedByCurrentUser();
				}
			}
			return result;
		}

		void MoveToNextRecordIfPossible()
		{
			if (ModuleResultsBusinessObject != null)
			{
				if (!ModuleResultsBusinessObject.CanMoveNext)
				{
					IOrgMatchApprovalModule module = (IOrgMatchApprovalModule)ZCurrentModules.Instance.GetCurrentModule(ModuleIDs.OrgMatchApproval);
					if (module != null)
					{
						module.PerformSearch();
					}
					PreviousNextControl.Show();
					if (ModuleResultsBusinessObject.PKList.Count > 0)
					{
						ModuleResultsBusinessObject.CurrentRecordNumber = 1;
					}
				}
				else
				{
					ModuleResultsBusinessObject.MoveNext();
				}
			}
		}

		#endregion

		#region Skipping over Already Matched Records

		void OnModuleResultsBusinessObject_CurrentRecordNumberChanging(ModuleResultsBusinessObject.CurrentRecordNumberChangingEventArgs args)
		{
			RemoveApprovalFromModuleIfApprovedByOtherUsers(ModuleResultsBusinessObject.CurrentRecordNumber - 1);
			RemoveApprovalFromModuleIfApprovedByOtherUsers(ModuleResultsBusinessObject.CurrentRecordNumber + 1);
		}

		void RemoveApprovalFromModuleIfApprovedByOtherUsers(int recordNumber)
		{
			IOrgMatchApprovalModule module = (IOrgMatchApprovalModule)ZCurrentModules.Instance.GetCurrentModule(ModuleIDs.OrgMatchApproval);
			int pKIndex = recordNumber - 1;

			if (pKIndex >= 0 && pKIndex < ModuleResultsBusinessObject.PKList.Count)
			{
				OrgMatchApproval matchApproval = (OrgMatchApproval)BusinessEntity.Factory.Load(typeof(OrgMatchApproval), ModuleResultsBusinessObject.PKList[pKIndex].PK);
				if (module != null && module.IsUnmatchForCurrentUserOnly && matchApproval.IsApprovedByOtherUsers)
				{
					module.GridCollection.Remove(matchApproval.PK);
				}
			}
		}

		#endregion

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			if (!this.IsDesignMode())
			{
				UpdateControlVisibilityForSupervisor();
				UpdateStatusBar(null, null);
				SelectFirstRankMatchInGrid();
				OrgToBeMatchedDetailsGroupBox.Text += " - " + BusinessEntity.OrganisationType;

				if (!BusinessEntity.IsCurrentUserSupervisor && BusinessEntity.ShouldCreateTemporaryOrganisation())
				{
					ShowCreateNewOrganisationForm();
				}
			}
		}

		#region IPreviousNextControlOverrideProvider Members

		bool IPreviousNextControlOverrideProvider.OverridesSetPreviousNextControlParentAndPosition
		{
			get { return true; }
		}

		bool IPreviousNextControlOverrideProvider.ShouldDoBaseSetPreviousNextControlParentAndPosition
		{
			get { return false; }
		}

		bool IPreviousNextControlOverrideProvider.OverridesGetPreviousNextControl
		{
			get { return false; }
		}

		void IPreviousNextControlOverrideProvider.SetPreviousNextControlParentAndPosition(ZPreviousNextControl control)
		{
			ControlDpiScalingHelper.SetTop(ref control, MainStatusBar.Top - control.Height - ControlDpiScalingHelper.ScaleToCurrentDpiY(5), false);
			ControlDpiScalingHelper.SetLeft(ref control, 6, true);
			control.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
			Controls.Add(control);

			ModuleResultsBusinessObject.CurrentRecordNumberChanging -= new ModuleResultsBusinessObject.NumberChangingEvent(OnModuleResultsBusinessObject_CurrentRecordNumberChanging);
			ModuleResultsBusinessObject.CurrentRecordNumberChanging += new ModuleResultsBusinessObject.NumberChangingEvent(OnModuleResultsBusinessObject_CurrentRecordNumberChanging);
		}

		ZPreviousNextControl IPreviousNextControlOverrideProvider.GetPreviousNextControl(ModuleResultsBusinessObject bizObj, ZController controller)
		{
			throw new NotImplementedException();
		}

		#endregion

		protected override bool ProcessCmdKey(ref Message msg, Keys key)
		{
			bool result = true;
			if (key == (Keys.Control | Keys.M))
			{
				MatchBoundButton.PerformClick();
			}
			else
			{
				result = base.ProcessCmdKey(ref msg, key);
			}
			return result;
		}

		void UpdateControlVisibilityForSupervisor()
		{
			SupervisorMatchBoundButton.Enabled = BusinessEntity.IsCurrentUserSupervisor;
			NewOrganisationBoundButton.Enabled = BusinessEntity.IsCurrentUserSupervisor;

			if (!BusinessEntity.IsCurrentUserSupervisor)
			{
				int dif = OrgToBeMatchedDetailsGroupBox.Top - ActiveMatchesGroupBox.Top;
				ControlDpiScalingHelper.SetTop(OrgToBeMatchedDetailsGroupBox, OrgToBeMatchedDetailsGroupBox.Top - dif, false);
				ControlDpiScalingHelper.SetTop(PossibleMatchingOrgsGroupBox, PossibleMatchingOrgsGroupBox.Top - dif, false);
				ControlDpiScalingHelper.SetTop(ManuallySelectedOrganisationGuidFindBox, ManuallySelectedOrganisationGuidFindBox.Top + dif, false);
				ControlDpiScalingHelper.SetHeight(PossibleMatchingOrgsGroupBox, PossibleMatchingOrgsGroupBox.Height + dif, false);
				ActiveMatchesGroupBox.Visible = false;
			}
		}

		void SelectFirstRankMatchInGrid()
		{
			if (SimilarOrgMatchesModuleButtonGrid.InnerGrid.SelectedElements.Length == 0)
			{
				if (SimilarOrgMatchesModuleButtonGrid.InnerGrid.ListManager.Count > 0)
				{
					ActiveControl = SimilarOrgMatchesModuleButtonGrid;
					SimilarOrgMatchesModuleButtonGrid.InnerGrid.Select(0);
				}
			}
		}

		void ShowCreateNewOrganisationForm()
		{
			IOrgMatchApprovalCreateNewOrgController controller = NewOrgMatchApprovalCreateNewOrgController();
			controller.UnmatchedOrgMatchApproval = BusinessEntity;

			((ZController)controller).SetFormsModalTo(this);
			IZForm form = ((ZController)controller).ShowNewForm();
		}

		void OnClose_Click(object sender, EventArgs e)
		{
			Close();
		}

		void OnSimilarOrgGrid_ColourDeciding(object sender, ColourDecidingEventArgs e)
		{
			SimilarOrgMatchForApproval orgMatchForApproval = (SimilarOrgMatchForApproval)e.ObjectAtRow;
			if (orgMatchForApproval.MatchApproval.IsCurrentUserSupervisor && !orgMatchForApproval.MatchedByUserInitials.IsEmpty)
			{
				e.Colour = Color.Orange;
			}
		}

		protected void UpdateStatusBar(object sender, EventArgs e)
		{
			if (MessageStatusBarPanel != null && BusinessEntity != null)
			{
				MessageStatusBarPanel.Text = BusinessEntity.P2_MatchStatus;
			}
		}

		protected override void UpdateStatusBar(string notification, INotificationType notificationType)
		{
			if (MessageStatusBarPanel != null && BusinessEntity != null)
			{
				MessageStatusBarPanel.Text = BusinessEntity.P2_MatchStatus;
			}
		}

		protected ZPreviousNextControl PreviousNextControl
		{
			get
			{
				if (fPreviousNextControl == null)
				{
					foreach (Control control in Controls)
					{
						fPreviousNextControl = control as ZPreviousNextControl;
						if (fPreviousNextControl != null)
						{
							break;
						}
					}
				}
				return fPreviousNextControl;
			}
		}
		ZPreviousNextControl fPreviousNextControl;

		#endregion
	}
}
