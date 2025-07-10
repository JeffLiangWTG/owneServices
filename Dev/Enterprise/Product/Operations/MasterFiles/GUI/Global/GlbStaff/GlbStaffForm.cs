using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.ActiveDirectory;
using CargoWise.Application;
using CargoWise.BrandManager;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using CargoWiseOne.ResourceStrings;
using Enterprise.BufferManagement.Integration;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterData.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI.WebAddressValidation;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.Security.ActiveDirectory;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using WTG.AddressCleansing.Common;
using static Enterprise.MasterFiles.GUI.DuplicateAlertControlHelper;

namespace Enterprise.MasterFiles.GUI
{
	public partial class GlbStaffForm : ZForm, IGlbStaffForm, IPreviousNextControlOverrideProvider, IPreviousNextControlProvider, ISupportWebAddressValidationControl, IWorkflowTaskNavigationOverridable, ISupportDuplicationAlertControl
	{
		public GlbStaffForm()
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Registry item key")]
		public GlbStaffForm(GlbStaff businessEntity)
			: base(businessEntity)
		{
			ZFormPostingButtonsStrategy.SetupPosting(this, ButtonsUserControl);
			WorkflowTabPage.Initialize(businessEntity);
			var currentCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			DisplayCredentialsPageForCountry(currentCountryCode);
			SetupSecurityPermissions();
			businessEntity.StaffSecurityPermissionsCollection.Security = fStaffSecurity;

			if (!DesignModeFinder.IsDesigning)
			{
				AddPlugInsOnGlbStaffTabControl(currentCountryCode);
			}
			PlugIns.Add(ControllerIDs.eDocsPlugIn, Env.Security.ViewStaffeDocs);
			PlugIns.Add(ControllerIDs.DocDataPlugIn);
			PlugIns.Add(ControllerIDs.GlbStaffCommissionPlugIn);

			if (Env.Security.StaffModifyAll.IsAllowed)
			{
				PlugIns.Add(ControllerIDs.Audit);
			}

			if (ControllerID == null)
			{
				ControllerID = ControllerIDs.GlbStaff;
			}

			SetupTabPageSecurity();
			InitialiseActionsMenu();

			LoginAndAttributesSecurity();
			ChangePasswordButton.CaptionResourceString = Staff.IsCurrentUser ? Res.GetData("GlbStaffForm |6249c47c-ccb9-4919-9543-a4c103408ad9", "Change Password") : Res.GetData("40E8279B-A625-43BA-9A85-3E54AFA92047", "Reset Password");
			UpdateLockedOutControls();
			SetupPasswordControlsEnable();

			SetActivityLogTabVisibility();
			SetActivityLogTabReadOnlyInViewMode();
			CheckDeviceOnlyRelatedOptions();

			this.customFieldsControl.NothingSetupMessageLabelText = Res.GetString("31225637-b2b9-4bc1-86dd-dc3da5018813", "To make use of this tab, please setup Staff and Resources custom fields in Workflow Manager.");

			if (!Staff.IsADIntegrationEnabled)
			{
				this.IsADLinkedCheckBox.Visible = false;
			}

			DomainPanel.Visible = (ObjectFactory.Get<IADRegistry>().DomainCredentialsCollection.Any()) && (!EnvProxy.IsHostedWithCargowise || Env.CurrentUser.IsSupportUser);

			if (SystemDataRegistry.Instance.TwoFactorAuthenticationTypes.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty) == "None")
			{
				IsTwoFactorAuthenticationEnabledCheckBox.Visible = false;
			}

			SetPrivacyGroupBox();

			if (!ObjectFactory.Get<IBMSRegistry>().BufferManagementEnabled)
			{
				BMComponentMembershipTabPage.TabVisible = false;
			}

			if (!DesignModeFinder.IsDesigning)
			{
				if (Env.Instance.Registry.EnableAddressValidationWebService)
				{
					cancellationToken = new CancellationTokenSource();
					this.HandleCreated += (o, e) =>
					{
						if (this.ParentForm != null)
						{
							this.ParentForm.FormClosed += (x, y) =>
							{
								if (cancellationToken != null)
								{
									cancellationToken.Cancel();
								}
							};
						}
					};
					HookISupportWebAddressValidationControlChangeFocusEvents();
				}
				else
				{
					ValidateAddressButton.Visible = false;
					ClearFieldsButton.Visible = false;
				}
			}
			ValidationJustForced = false;

			SetEditPersonButtonAvailability();
			Saved += (sender, e) =>
			{
				UnHookThenHookEventsWhenPersonCreatedOrChanged();
				SetEditPersonButtonAvailability();
			};

#if DEBUG
			TypeDescriptor.AddAttributes(zLabel5, new SuppressFormsLocalizedTestAttribute());
#endif
			businessEntity.Groups.Cast<GlbGroup>().ToList().ForEach(g => g.CurrentGroupLink.Validation.ValidateDomainsForStaffAndGroup());

			MembersModuleButtonGrid.BeforeDetach += MembersModuleButtonGrid_BeforeDetaching;
			MembersModuleButtonGrid.Detached += MembersModuleButtonGrid_OnDetached;
			MembersModuleButtonGrid.InnerGrid.RowsDeleting += MembersModuleButtonGrid_InnerGrid_RowDeleteKeyDown;

			SetupScim();
		}

		void SetupScim()
		{
			if (Staff.IsControlledByScim)
			{
				MembersModuleButtonGrid.SetButtonsReadOnly(true);
			}
		}

		void SetPrivacyGroupBox()
		{
			var shouldShowSavePersonalDataToADCheckbox = Staff.IsADIntegrationEnabled && (ObjectFactory.Get<IADRegistry>().SyncMode == SyncMode.EnterpriseIsMaster || ObjectFactory.Get<IADRegistry>().SyncDirection == SyncDirection.TwoWay);
			if (shouldShowSavePersonalDataToADCheckbox)
			{
				SavePersonalDataToActiveDirectoryCheckbox.Visible = true;
			}
			else
			{
				SavePersonalDataToActiveDirectoryCheckbox.Visible = false;
				ControlDpiScalingHelper.SetHeight(ref PrivacyGroupBox, PrivacyGroupBox.Height - ControlDpiScalingHelper.ScaleToCurrentDpiY(20), false);
			}
			ControlDpiScalingHelper.SetTop(ref HomeBranchDepartmentGroupBox, PrivacyGroupBox.Top + PrivacyGroupBox.Height + ControlDpiScalingHelper.ScaleToCurrentDpiY(4), false);
		}

		void SetEditPersonButtonAvailability()
		{
			EditPersonButton.Available = Staff.IsInDatabase && Staff.Person != null && SystemDataRegistry.Instance.PersonIntelligenceModuleEnabled.Value;
		}

		bool PersonCreatedOrChanged => Person != null && Staff.GS_PER != OldStaff_GS_PER;

		ZGuid OldStaff_GS_PER { get; set; }

		void SetActivityLogTabVisibility()
		{
			if (Staff.GS_ActivityTrackingStatus.ToString() == ActivityTrackingStatus.No ||
				!Env.Instance.Security.StaffActivityLog.IsAllowed ||
				(Staff.GS_ActivityTrackingStatus.ToString() == ActivityTrackingStatus.BasedOnCompany && !EnvProxy.Instance.Registry.UserEventTrackingEnterprise && !EnvProxy.Instance.Registry.UserEventTrackingExternal))
			{
				ActivityLogTabPage.TabVisible = false;
			}
		}

		void SetActivityLogTabReadOnlyInViewMode()
		{
			if (Staff.IsSupportUser)
			{
				ActivityLogTabPage.ShouldBeReadOnlyInViewMode = false;
			}
		}

		void LoginAndAttributesSecurity()
		{
			GS_LoginNameBoundText.ReadOnly = !LoginTextBoxIsAllowed;
			DomainNameDropEdit.ReadOnly = !LoginTextBoxIsAllowed;
			IsCLUserCheckBox.ReadOnly = !LoginAttributesIsAllowed;
			IsDeviceOnlyCheckBox.ReadOnly = !LoginAttributesIsAllowed;
			GS_IsActiveBoundCheck.ReadOnly = !LoginAttributesIsAllowed;
		}

		bool IsHostedWithCargoWiseOrIsEdiprod()
		{
			return EnvProxy.IsHostedWithCargowise || ClientHookLoader.Instance.Client == Clients.EDI;
		}

		bool IsSystemCreatedAccount(GlbStaff staff)
		{
			return staff.GS_IsSystemAccount || (staff.GS_Code == "EDS" && IsHostedWithCargoWiseOrIsEdiprod());
		}

		public override ODisplayMode DisplayMode
		{
			get
			{
				return Staff != null && !Staff.IsDeleted && IsSystemCreatedAccount(Staff) ? ODisplayMode.ReadOnly : base.DisplayMode;
			}
			set
			{
				base.DisplayMode = value;
			}
		}

		internal GlbStaff Staff
		{
			get { return (GlbStaff)DataSource; }
		}

		internal GlbPerson Person => Staff?.Person;

		public override string FormCaption => $"{CaptionResourceString.Caption} {Staff?.GS_FullName}".Trim();

		void InitialiseActionsMenu()
		{
			ZFormMenuStrategy.AddActionsMenuItem(this, GetActionsMenuItems());
		}

		IEnumerable<ZMenuItem> GetActionsMenuItems()
		{
			var adMenuItem = ObjectFactory.Get<IADActionsMenuItemProvider>().GetFormMenuItem(Staff);
			if (adMenuItem != null)
			{
				yield return (ZMenuItem)adMenuItem;
				yield return new ZMenuItem("-");
			}

			yield return new ZMenuItem(ResString.GetMultilingualString("CEC0A02F-A64B-4DD9-8D68-4922C15F5714", "Find &Duplicates"), (obj, e) => Staff?.Person?.FindDuplicates(Staff))
			{
				Shortcut = Shortcut.CtrlG,
				Enabled = Env.Security.PersonIntelligenceDuplicateDetection.IsAllowed && Staff != null && Staff.GS_IsActive && SystemDataRegistry.Instance.PersonsEnableDuplicateDetection.Value
			};

			yield return new ZMenuItem(ResString.GetMultilingualString("MasterFiles.GlbStaffForm.ResetToDefaultGroupRights", "Reset Security Permissions to default Groups' rights"), ResetToDefaultGroupRightsButton_Click);
		}

		#region Top Level Tab Control

		protected override ZTabControl TopLevelTabControl
		{
			get { return GlbStaffTabControl; }
		}

		#endregion

		#region Group and Staff Events

		public void CLUserCheckBox_Click(object sender, EventArgs e)
		{
			if (this.IsCLUserCheckBox.Checked)
			{
				if (Globals.Message.ShowConfirmation(Res.GetString("7BBDB721-300E-4079-B910-4274346E8864", "This Staff profile will have login rights to {0}.", BrandingFactory.Instance.ProductName),
						Res.GetString("2FEB9C60-B73D-4674-983E-8B178BCD98C2", "Critical Warning"),
						Res.GetString("E0101A06-17B0-45EF-B7D0-C345CE84936C", "Yes"), MessageBoxIcon.Hand) != DialogResult.OK)
				{
					this.IsCLUserCheckBox.Checked = false;
				}
			}
			else
			{
				if (Globals.Message.ShowConfirmation(Res.GetString("F8182990-7C55-4739-A576-6B4BC7CF585E", "This Staff profile will not have login rights to {0}.", BrandingFactory.Instance.ProductName),
						Res.GetString("2FEB9C60-B73D-4674-983E-8B178BCD98C2", "Critical Warning"),
						Res.GetString("04D43925-58DC-4726-8A7D-C837433D7C8B", "Yes"), MessageBoxIcon.Hand) != DialogResult.OK)
				{
					this.IsCLUserCheckBox.Checked = true;
				}
			}
		}

		public void ReloadGroupPermissions(object sender, EventArgs e)
		{
			fStaffGroupSecurity.ResetData(Staff.GroupSecurityPermissionsCollection, Staff, Env.CurrentBranch.PK, Env.CurrentDepartment.PK, Env.CurrentCompany.PK, false);
		}

		void DeleteFromAllUsersError(object sender, EventArgs e)
		{
			throw new CannotDeleteException("Active staff members cannot be removed from the 'All Users' group.\r\nOnly inactive staff can be removed from this group.");
		}

		void DeleteFromDatabaseAccessError(object sender, EventArgs e)
		{
			throw new CannotDeleteException("Cannot detach a staff from a database access group.");
		}

		void DeleteFromSCIMError(object sender, EventArgs e)
		{
			throw new CannotDeleteException("Cannot detach a staff from a SCIM-mapped group.");
		}

		internal void SuccessfulSaveNewStaffMember(object sender, EventArgs e)
		{
			PasswordPanel.Visible = false;
			ChangePasswordButton.Enabled = ShouldEnableChangePasswordButton;
		}

		bool ShouldEnableChangePasswordButton => (!Staff.IsADIntegrationEnabled || Staff.IsADLinked && Staff.CanAccessDirectoryEntry() && AllowChangeADPassword) && ShowPasswordTabPage && (this.DisplayMode != ODisplayMode.ReadOnly);

		bool AllowChangeADPassword => (Staff.IsCurrentUser || !Staff.IsCurrentUser && (ObjectFactory.Get<IADRegistry>().SyncMode == SyncMode.EnterpriseIsMaster || ObjectFactory.Get<IADRegistry>().SyncDirection == SyncDirection.TwoWay))
			&& !ObjectFactory.Get<IADRegistry>().DisableADPasswordChange;

		void UpdateLockedOutControls()
		{
			UnlockButton.Visible = Staff.IsLockedOut;
			UnlockButton.Enabled = !Staff.IsADIntegrationEnabled || Staff.IsADIntegrationEnabled && (ObjectFactory.Get<IADRegistry>().SyncMode == SyncMode.EnterpriseIsMaster || ObjectFactory.Get<IADRegistry>().SyncDirection == SyncDirection.TwoWay);
			IsADLockedOut.Visible = Staff.IsADIntegrationEnabled && Staff.IsLockedOut;
			GS_LockoutDateTimeBoundDate.Visible = !Staff.IsADIntegrationEnabled && Staff.IsLockedOut;
		}

		void SetupPasswordControlsEnable()
		{
			if (Env.CurrentUser.IsController)
			{
				return;
			}

			if (ShouldHidePasswordControls)
			{
				DisablePasswordControlsLayout();
			}
		}

		protected bool IsOIDCEnabled
		{
			get
			{
				var oidcConfig = SystemDataRegistry.Instance.OIDCConfig.Value;
				return oidcConfig.IsOIDCEnabled;
			}
		}

		protected virtual bool ShouldHidePasswordControls => IsOIDCEnabled;

		void DisablePasswordControlsLayout()
		{
			ChangePasswordButton.Hide();
			PasswordOptionsGroupBox.Hide();
			SignatureGroupBox.Location = ControlDpiScalingHelper.NewScaledPoint(17, 48, true);
			PasswordTabPage.CaptionResourceString = Res.GetData("GlbStaffForm |2ED018EA-FEE2-47F7-AAE5-9CFBBBD58532", "Signature");
		}

		internal void UnlockButton_Click(object sender, EventArgs e)
		{
			try
			{
				Staff.UnlockAccount();
				UpdateLockedOutControls();
			}
			catch (DirectoryServicesException ex)
			{
				Globals.Message.ShowError(ex.Message);
			}
		}

		protected void EditPersonButton_Click(object sender, EventArgs e)
		{
			var controller = ZControllerFactory.Create(ControllerIDs.GlbPerson);
			controller.SetFormsModalTo(this);
			controller.ShowEditForm(Staff.Person);
		}

		#endregion

		#region Show Tab Pages based on Security Check Points

		#region SetControlsAndSecurityLabel

		void SetControlsAndSecurityLabel(Control topLevelControl, ZLabel securityWarningLabel, bool controlsEnabled)
		{
			if (!controlsEnabled)
			{
				foreach (Control ctrl in topLevelControl.Controls)
				{
					RecursivelySetControls(ctrl, controlsEnabled);
				}
			}

			if (securityWarningLabel != null)
			{
				if (!controlsEnabled)
				{
					securityWarningLabel.ForeColor = Color.Red;
					securityWarningLabel.Text = Res.GetString("124d2fa8-438e-4722-969e-806ba9333959", "You do not have sufficient privilege to access this page/section. Contact your system administrator for more details.");
					securityWarningLabel.Enabled = true;
					securityWarningLabel.Visible = true;
				}
				else if (Staff.IsControlledByScim)
				{
					securityWarningLabel.ForeColor = Color.Red;
					securityWarningLabel.Text = Res.GetString("C0AACD76-2CC4-4F9A-ADB7-9052CDF6589E", "Some fields cannot be edited because they are controlled externally.");
					securityWarningLabel.Enabled = true;
					securityWarningLabel.Visible = true;
				}
				else if (Staff.IsCurrentUserLocalAdminForThisStaff && !GlbStaff.CurrentUser.GS_IsController)
				{
					securityWarningLabel.ForeColor = Color.Blue;
					securityWarningLabel.Text = Res.GetString("adac3f85-086a-4747-b70b-f3d867ab26e5", "You have rights to view/edit this user because you (and/or a group you are member of) are its Local Administrator.");
					securityWarningLabel.Enabled = true;
					securityWarningLabel.Visible = true;
				}
				else
				{
					securityWarningLabel.Text = "";
					securityWarningLabel.Enabled = false;
					securityWarningLabel.Visible = false;
				}
			}
		}

		#endregion

		#region Show Tab Page

		bool ShowSalesTabPage
		{
			get
			{
				if (!Staff.GS_IsSalesRep)
				{
					return false;
				}

				return
					EditSalesTabPageAllowed ||
					(Staff.IsCurrentUser ? Env.Security.StaffViewOwnSales.IsAllowed : Env.Security.StaffViewSales.IsAllowed);
			}
		}

		bool EditSalesTabPageAllowed
		{
			get { return Env.Security.StaffSales.IsAllowed || (Env.Security.StaffOwnSales.IsAllowed && Staff.IsCurrentUser); }
		}

		bool SetIsSalesRepAllowed
		{
			get { return Env.Security.StaffSetSalesRep.IsAllowed || (Env.Security.StaffOwnSetSalesRep.IsAllowed && Staff.IsCurrentUser); }
		}

		bool EditOwnEmailAllowed
		{
			get { return Env.Security.StaffOwnEmail.IsAllowed; }
		}

		bool LoginTextBoxIsAllowed
		{
			get
			{
				return Env.Security.StaffAllLogin.IsAllowed ||
					Env.Security.StaffOwnLogin.IsAllowed && Staff.IsCurrentUser ||
					Staff.IsCurrentUserLocalAdminForThisStaff ||
					Staff.IsCurrentUserLocalAdminForAtLeastOneGroup && !Staff.IsInDatabase;
			}
		}

		bool LoginAttributesIsAllowed
		{
			get
			{
				return Env.Security.StaffLoginAttributesAll.IsAllowed || (Env.Security.StaffLoginAttributesOwn.IsAllowed &&
					(Staff.IsCurrentUser || Staff.IsCurrentUserLocalAdminForThisStaff || Staff.IsCurrentUserLocalAdminForAtLeastOneGroup));
			}
		}

		internal bool ShowDetailsTabPage => Staff.IsDetailsModifiable;

		bool EditInterfaceLanguageControl
		{
			get
			{
				return Env.Security.StaffInterfaceLanguage.IsAllowed ||
					Env.Security.StaffOwnInterfaceLanguage.IsAllowed && Staff.IsCurrentUser ||
					Staff.IsCurrentUserLocalAdminForThisStaff ||
					Staff.IsCurrentUserLocalAdminForAtLeastOneGroup && !Staff.IsInDatabase;
			}
		}

		internal bool ShowGroupsTabPage
		{
			get
			{
				return Env.Security.StaffGroups.IsAllowed || Staff.IsCurrentUserLocalAdminForThisStaff ||
					Staff.IsCurrentUserLocalAdminForAtLeastOneGroup && !Staff.IsInDatabase;
			}
		}

		internal bool ShowSecurityRightsTabPage
		{
			get
			{
				return Env.CurrentUser.IsController || !Staff.IsCurrentUser && Staff.IsCurrentUserLocalAdminForThisStaff ||
					Staff.IsCurrentUserLocalAdminForAtLeastOneGroup && !Staff.IsInDatabase;
			}
		}

		internal bool ShowPasswordTabPage
		{
			get
			{
				return Env.Security.StaffPasswordAndSignature.IsAllowed ||
					(Env.Security.StaffOwnPasswordAndSignature.IsAllowed && Staff.IsCurrentUser) ||
					Staff.IsCurrentUserLocalAdminForThisStaff ||
					Staff.IsCurrentUserLocalAdminForAtLeastOneGroup && !Staff.IsInDatabase;
			}
		}

		internal bool ShowTwoFactorAuthenticationCheckBox
		{
			get
			{
				return Env.Security.StaffTwoFactorAuthentication.IsAllowed ||
					(Env.Security.StaffOwnTwoFactorAuthentication.IsAllowed && Staff.IsCurrentUser) ||
					Staff.IsCurrentUserLocalAdminForThisStaff ||
					Staff.IsCurrentUserLocalAdminForAtLeastOneGroup && !Staff.IsInDatabase;
			}
		}

		internal bool ShowCredentialsTabPage
		{
			get
			{
				return Env.Security.StaffCredentials.IsAllowed ||
					(Env.Security.StaffOwnCredentials.IsAllowed && Staff.IsCurrentUser) ||
					Staff.IsCurrentUserLocalAdminForThisStaff ||
					Staff.IsCurrentUserLocalAdminForAtLeastOneGroup && !Staff.IsInDatabase;
			}
		}

		internal bool ShowCertificatesTabPage
		{
			get
			{
				return Staff.IsCurrentUser ||
					EditCertificatesTabPage ||
					Env.Security.StaffViewOtherCertificates.IsAllowed;
			}
		}

		bool EditCertificatesTabPage
		{
			get
			{
				return Env.Security.StaffCertificates.IsAllowed ||
					(Env.Security.StaffOwnCertificates.IsAllowed && Staff.IsCurrentUser) ||
					Staff.IsCurrentUserLocalAdminForThisStaff ||
					Staff.IsCurrentUserLocalAdminForAtLeastOneGroup && !Staff.IsInDatabase;
			}
		}

		internal bool ShowLeaveTabPage => GlbStaffVisibilityHelper.CanShowLeave(Staff);

		bool EditLeaveTabPage => GlbStaffVisibilityHelper.CanEditLeave(Staff);

		internal bool ShowWorkingHoursControl
		{
			get
			{
				return Staff.IsCurrentUser ||
					EditWorkingHoursControl ||
					Env.Security.StaffViewOtherWorkingHours.IsAllowed;
			}
		}

		bool EditWorkingHoursControl
		{
			get
			{
				return Env.Security.StaffWorkingHours.IsAllowed ||
					(Env.Security.StaffOwnWorkingHours.IsAllowed && Staff.IsCurrentUser) ||
					Staff.IsCurrentUserLocalAdminForThisStaff ||
					Staff.IsCurrentUserLocalAdminForAtLeastOneGroup && !Staff.IsInDatabase;
			}
		}

		bool ShowStaffManagerControl
		{
			get
			{
				return Staff.IsCurrentUser && Env.Security.StaffViewOwnReportingManagerRoles.IsAllowed ||
					Env.Security.StaffViewOtherReportingManagerRoles.IsAllowed ||
					Env.Security.StaffReportingManagerRoles.IsAllowed ||
					Staff.IsCurrentUserLocalAdminForThisStaff ||
					Staff.IsCurrentUserLocalAdminForAtLeastOneGroup && !Staff.IsInDatabase;
			}
		}

		bool ShowDirectReportsControl
		{
			get
			{
				return Staff.IsCurrentUser ||
				Env.Security.StaffViewOtherDirectReports.IsAllowed ||
				Staff.IsCurrentUserLocalAdminForThisStaff ||
				Staff.IsCurrentUserLocalAdminForAtLeastOneGroup && !Staff.IsInDatabase;
			}
		}

		internal bool ShowLanguagesTabPage
		{
			get
			{
				return Staff.IsCurrentUser ||
					EditLanguagesTabPage ||
					Env.Security.StaffViewOtherLanguages.IsAllowed;
			}
		}

		bool EditLanguagesTabPage
		{
			get
			{
				return Env.Security.StaffLanguages.IsAllowed ||
					(Env.Security.StaffOwnLanguages.IsAllowed && Staff.IsCurrentUser) ||
					Staff.IsCurrentUserLocalAdminForThisStaff ||
					Staff.IsCurrentUserLocalAdminForAtLeastOneGroup && !Staff.IsInDatabase;
			}
		}

		internal bool ShowTimeAllocationTabPage
		{
			get
			{
				return Staff.IsCurrentUser ||
					EditTimeAllocationTabPage ||
					Env.Security.StaffViewOtherTimeAllocation.IsAllowed;
			}
		}

		bool EditTimeAllocationTabPage
		{
			get
			{
				return Env.Security.StaffTimeAllocation.IsAllowed ||
					(Env.Security.StaffOwnTimeAllocation.IsAllowed && Staff.IsCurrentUser) ||
					Staff.IsCurrentUserLocalAdminForThisStaff ||
					Staff.IsCurrentUserLocalAdminForAtLeastOneGroup && !Staff.IsInDatabase;
			}
		}

		bool ShowWorkflowTabPage
		{
			get
			{
				return ((Env.Security.FindOrCreateWorkflowCheckpoint(Env.Security.Staff).IsAllowed && Staff.IsCurrentUser) || (Env.Security.StaffOwnWorkflow.IsAllowed && Staff.IsCurrentUser) || (Env.Security.StaffAllWorkflow.IsAllowed))
					||
					Staff.IsCurrentUserLocalAdminForThisStaff ||
					Staff.IsCurrentUserLocalAdminForAtLeastOneGroup && !Staff.IsInDatabase;
			}
		}

		#endregion

		#region ISupportDuplicationAlertControl

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1017", Justification = "The value's calculated from scaled value ")]
		public Point DuplicationAlertAnchorLocation
		{
			get
			{
				var margin = ControlDpiScalingHelper.ScaleToCurrentDpiX(2);
				var xLocation = GS_FullNameBoundTextBox.Location.X + GS_FullNameBoundTextBox.Width + EmployeeDetailsGroupBox.Location.X + margin;
				var yLocation = GS_FullNameBoundTextBox.Location.Y + EmployeeDetailsGroupBox.Location.Y;
				return new Point(xLocation, yLocation);
			}
		}

		public Control DuplicationAlertParentControl => DetailsTabPage;

		public Control DuplicationAlertReferenceControl => null;

		public string DeduplicationStatusText => DuplicateDetectionStatusLabel.Text;

		public bool DeduplicationStatusVisible => DuplicateDetectionStatusLabel.Visible;

		public DuplicateAlertControlHelper DeduplicationHelper => duplicateAlertControlHelper ?? (duplicateAlertControlHelper = new DuplicateAlertControlHelper());
		DuplicateAlertControlHelper duplicateAlertControlHelper;

		public void ShowDeduplicationStatus()
		{
			DuplicateDetectionStatusIcon.Image = Properties.Resources.loader;
			var getTextAndColor = GetTextAndColor(DuplicationConditions.ShowDeduplicationStatus);
			DuplicateDetectionStatusLabel.Text = getTextAndColor.Text;
			DuplicateDetectionStatusLabel.ForeColor = getTextAndColor.ForeColor;
			DuplicateDetectionStatusLabel.Visible = true;
			DuplicateDetectionStatusIcon.Visible = true;
		}

		public void ShowNoDuplicatesFound(IDuplicationEventArgs duplicationEventArgs)
		{
			var getTextAndColor = GetTextAndColor(DuplicationConditions.ShowNoDuplicatesFound);
			SetDuplicateDetectionStatusLabel(getTextAndColor.Text, getTextAndColor.ForeColor);
			DeregisterEventHandlers();
		}

		public void ShowExcludedDuplicationMessage()
		{
			var getTextAndColor = GetTextAndColor(DuplicationConditions.ShowExcludedDuplicationMessage);
			SetDuplicateDetectionStatusLabel(getTextAndColor.Text, getTextAndColor.ForeColor);
			DeregisterEventHandlers();
		}

		public void ShowNotEnoughInformation()
		{
			var getTextAndColor = GetTextAndColor(DuplicationConditions.ShowNotEnoughInformation);
			SetDuplicateDetectionStatusLabel(getTextAndColor.Text, getTextAndColor.ForeColor);
		}

		public void ShowDeduplicationTimeoutMessage()
		{
			var getTextAndColor = GetTextAndColor(DuplicationConditions.ShowDeduplicationTimeoutMessage);
			SetDuplicateDetectionStatusLabel(getTextAndColor.Text, getTextAndColor.ForeColor);
			DeregisterEventHandlers();
		}

		public void ShowDeduplicationErrorOccurredMessage()
		{
			var getTextAndColor = GetTextAndColor(DuplicationConditions.ShowDeduplicationErrorOccurredMessage);
			SetDuplicateDetectionStatusLabel(getTextAndColor.Text, getTextAndColor.ForeColor);
			DeregisterEventHandlers();
		}

		protected IDuplicationEventArgs currentDuplicationEventArgs;

		public void ShowDuplicatesFound(IDuplicationEventArgs duplicationEventArgs)
		{
			currentDuplicationEventArgs = duplicationEventArgs;
			var getTextAndColor = GetTextAndColor(DuplicationConditions.ShowDuplicatesFound);
			SetDuplicateDetectionStatusLabel(getTextAndColor.Text, getTextAndColor.ForeColor);
			DeregisterEventHandlers();
			RegisterEventHandlers();
		}

		public void SetDuplicateDetectionStatusLabel(ResourceString text, Color foreColor)
		{
			DuplicateDetectionStatusLabel.Text = text;
			DuplicateDetectionStatusLabel.ForeColor = foreColor;
			DuplicateDetectionStatusLabel.Visible = true;
			DuplicateDetectionStatusIcon.Visible = false;
		}

		void RegisterEventHandlers()
		{
			DuplicateDetectionStatusLabel.MouseEnter += DuplicateDetectionStatusLabelOnMouseEnter;
			DuplicateDetectionStatusLabel.MouseLeave += DuplicateDetectionStatusLabelOnMouseLeave;
			DuplicateDetectionStatusLabel.Click += DuplicateDetectionStatusLabelOnClick;
		}

		void DeregisterEventHandlers()
		{
			DuplicateDetectionStatusLabel.MouseEnter -= DuplicateDetectionStatusLabelOnMouseEnter;
			DuplicateDetectionStatusLabel.MouseLeave -= DuplicateDetectionStatusLabelOnMouseLeave;
			DuplicateDetectionStatusLabel.Click -= DuplicateDetectionStatusLabelOnClick;
		}

		public void DuplicateDetectionStatusLabelOnMouseEnter(object sender, EventArgs eventArgs)
		{
			DuplicateDetectionStatusLabel.Font = new Font(DuplicateDetectionStatusLabel.Font, FontStyle.Underline);
			DuplicateDetectionStatusLabel.Cursor = Cursors.Hand;
		}

		public void DuplicateDetectionStatusLabelOnMouseLeave(object sender, EventArgs e)
		{
			DuplicateDetectionStatusLabel.Font = new Font(DuplicateDetectionStatusLabel.Font, FontStyle.Regular);
			DuplicateDetectionStatusLabel.Cursor = Cursors.Default;
		}

		public void DuplicateDetectionStatusLabelOnClick(object sender, EventArgs eventArgs)
		{
			Person?.FindDuplicates(Staff);
		}

		public void DuplicationDetected(object sender, IDuplicationEventArgs e)
		{
			ShowDuplications(e);
		}

		public void ShowDuplications(IDuplicationEventArgs e)
		{
			HideDeduplicationStatus();
			DeduplicationHelper.ShowDuplicateAlert(this, e);
		}

		public void HideDeduplicationStatus()
		{
			DuplicateDetectionStatusLabel.Visible = false;
			DuplicateDetectionStatusIcon.Visible = false;
		}

		public void DeduplicationActionOccurred(object sender, IDuplicationEventArgs duplicationEventArgs)
		{
			if (duplicationEventArgs.InvokedAction != DeduplicationAction.None)
			{
				if (DuplicateDetectionStatusLabel.Visible)
				{
					HideDeduplicationStatus();
				}

				if (duplicationEventArgs.InvokedAction == DeduplicationAction.Merge)
				{
					UnHookThenHookEventsWhenPersonCreatedOrChanged();
					Person?.FindDuplicates(Staff);
				}
			}
		}

		void UnHookThenHookEventsWhenPersonCreatedOrChanged()
		{
			if (PersonCreatedOrChanged)
			{
				UnHookDuplicationDetectEvents();
				HookDuplicationDetectEvents();
				OldStaff_GS_PER = Staff.GS_PER;
			}
		}

		public void DuplicationEnded(object sender, IDuplicationEventArgs e)
		{
			if (e == null)
			{
				throw new ArgumentNullException(nameof(e));
			}

			if (!e.Results.IsNullOrEmpty())
			{
				ShowDuplicatesFound(e);
			}
			else
			{
				//resultsmodels == null means that minimum requirements are not met and dedup hasn't been performed. if it's performed the object is not null but empty.
				// TODO: refactor deduplication flow so it's clear to see if dedup has been perfomed or not and why
				if (e.IsErrorOccurred)
				{
					ShowDeduplicationErrorOccurredMessage();
				}
				else if (e.IsTimeout)
				{
					ShowDeduplicationTimeoutMessage();
				}
				else if (e.Results == null && e.ResultsModels == null && e.TargetObjects == null)
				{
					ShowExcludedDuplicationMessage();
				}
				else if (e.ResultsModels == null)
				{
					ShowNotEnoughInformation();
				}
				else
				{
					ShowNoDuplicatesFound(e);
				}

				DeduplicationHelper.CloseExistingDuplicateAlert();
			}
		}

		public void DuplicationStarted(object sender, EventArgs e)
		{
			ShowDeduplicationStatus();
		}

		public void UnHookDuplicationDetectEvents()
		{
			if (Person != null)
			{
				((IDeduplicatable)Person).ShouldRunDeduplication = false;
				Person.DuplicationDetected -= DuplicationDetected;
				Person.DeduplicationStarted -= DuplicationStarted;
				Person.DeduplicationEnded -= DuplicationEnded;
				Person.DeduplicationActionOccurred -= DeduplicationActionOccurred;
			}
		}

		public void HookDuplicationDetectEvents()
		{
			if (SystemDataRegistry.Instance.PersonsEnableDuplicateDetection.Value)
			{
				if (Staff != null && Person == null)
				{
					GlbPerson.CreateFromStaff(Staff.Factory, Staff);
					OldStaff_GS_PER = Staff.GS_PER;
				}

				if (Person != null)
				{
					((IDeduplicatable)Person).ShouldRunDeduplication = true;
					Person.DuplicationDetected += DuplicationDetected;
					Person.DeduplicationStarted += DuplicationStarted;
					Person.DeduplicationEnded += DuplicationEnded;
					Person.DeduplicationActionOccurred += DeduplicationActionOccurred;
				}
			}
		}

		#endregion

		#region Tab Control Security

		internal void GlbStaffTabControl_SelectedIndexChanged(object sender, EventArgs e)
		{
			SetupTabPageSecurity();
		}

		void SetupTabPageSecurity()
		{
			TabPage selectedTab = GlbStaffTabControl.SelectedTab;

			if (selectedTab != null)
			{
				if (selectedTab == DetailsTabPage)
				{
					DetailsHintLabel.BringToFront();
					SetControlsAndSecurityLabel(selectedTab, DetailsHintLabel, ShowDetailsTabPage);

					bool editInterfaceLanguageControl = EditInterfaceLanguageControl;
					WorkingLanguageDropEdit.ReadOnly = !editInterfaceLanguageControl;
					WorkingLanguageDropEdit.Enabled |= editInterfaceLanguageControl;

					if (!ShowDetailsTabPage && SetIsSalesRepAllowed)
					{
						DetailsLeftPanel.Enabled = true;
						RoleGroupBox.Enabled = true;
						SalesRepCheckBox.Enabled = true;
					}
				}
				else if (selectedTab == GroupsTabPage)
				{
					SetControlsAndSecurityLabel(selectedTab, GroupsHintLabel, ShowGroupsTabPage);
				}
				else if (selectedTab == SecurityRightsTabPage)
				{
					SecurityPermissionsRefresh();
					SetControlsAndSecurityLabel(selectedTab, SecurityRightsHintLabel, ShowSecurityRightsTabPage);
				}
				else if (selectedTab == PasswordTabPage)
				{
					SetControlsAndSecurityLabel(selectedTab, PasswordAndSignatureHintLabel, ShowPasswordTabPage);
					ChangePasswordButton.Enabled = Staff.IsInDatabase && ShouldEnableChangePasswordButton;
					IsTwoFactorAuthenticationEnabledCheckBox.Enabled = ShowTwoFactorAuthenticationCheckBox;
					if (!ShowPasswordTabPage && ShowTwoFactorAuthenticationCheckBox)
					{
						RecursivelyEnableParent(IsTwoFactorAuthenticationEnabledCheckBox);
					}
				}
				else if (selectedTab == HRTabPage)
				{
					SetupHRTabPageSecurity();
				}
				else if (selectedTab == SalesTabPage)
				{
					SetControlsAndSecurityLabel(selectedTab, SalesHintLabel, EditSalesTabPageAllowed);
				}
				else if (selectedTab == WorkflowTabPage)
				{
					SetControlsAndSecurityLabel(selectedTab, WorkflowHintLabel, ShowWorkflowTabPage);
				}
				else
				{
					var staffCredentialsPlugIn = GlbStaffTabControl.PlugIns.GetPlugIn(ControllerIDs.StaffCredentialsPlugIn);
					if (staffCredentialsPlugIn != null && selectedTab == staffCredentialsPlugIn.TabPage)
					{
						SetControlsAndSecurityLabel(selectedTab, (staffCredentialsPlugIn.UserControl as StaffCredentialsUserControl)?.CredentialsHintLabel, ShowCredentialsTabPage);
					}
				}
			}
		}

		#endregion

		#region HR Tab Control Security

		void HRTabControl_SelectedIndexChanged(object sender, EventArgs e)
		{
			SetupHRTabPageSecurity();
		}

		void SetupHRTabPageSecurity()
		{
			if (HRTabControl.SelectedTab == EmploymentTabPage)
			{
				SetControlVisible(WorkingHoursGroupBox, ShowWorkingHoursControl, CoveringLabel1);
				SetControlVisible(ReportingManagementGroupBox, ShowStaffManagerControl, CoveringLabel2);
				SetControlVisible(glbStaffDirectReportsControl, ShowDirectReportsControl, CoveringLabel3);
				SetControlsAndSecurityLabel(WorkingHoursGroupBox, WorkingHoursHintLabel, EditWorkingHoursControl);
			}
			else if (HRTabControl.SelectedTab == CertificatesTabPage)
			{
				ControlDpiScalingHelper.SetTop(ref CertificatesHintLabel, 0, true);
				ControlDpiScalingHelper.SetLeft(ref CertificatesHintLabel, 0, true);
				SetControlVisible(CertificatesTabPage, ShowCertificatesTabPage, CoveringLabel1);
				SetControlsAndSecurityLabel(CertificatesTabPage, CertificatesHintLabel, EditCertificatesTabPage);
				if (!ShowCertificatesTabPage)
				{
					ControlDpiScalingHelper.SetTop(ref CertificatesUserControl, CertificatesHintLabel.Bottom + ControlDpiScalingHelper.ScaleToCurrentDpiY(10), false);
				}
			}
			else if (HRTabControl.SelectedTab == LeaveTabPage)
			{
				SetControlVisible(LeaveTabPage, ShowLeaveTabPage, CoveringLabel1);
				SetControlsAndSecurityLabel(LeaveTabPage, LeaveHintLabel, EditLeaveTabPage);
				SetHolidaysGridReadOnly();
			}
			else if (HRTabControl.SelectedTab == LanguageTabPage)
			{
				SetControlVisible(LanguageTabPage, ShowLanguagesTabPage, CoveringLabel1);
				SetControlsAndSecurityLabel(LanguageTabPage, LanguagesHintLabel, EditLanguagesTabPage);
			}
			else if (HRTabControl.SelectedTab == TimeAllocationTabPage)
			{
				SetControlVisible(TimeAllocationTabPage, ShowTimeAllocationTabPage, CoveringLabel1);
				SetControlsAndSecurityLabel(TimeAllocationTabPage, TimeAllocationHintLabel, EditTimeAllocationTabPage);
			}
		}

		void SetHolidaysGridReadOnly()
		{
			if (Staff != null)
			{
				HolidaysGrid.ReadOnly |= Staff.IsLeaveEnabled;
			}
		}

		void SetControlVisible(Control control, bool isVisible, ZLabel label)
		{
			if (!isVisible)
			{
				ShowCoveringLabel(control, label);
			}
			else
			{
				label.Visible = false;
			}
		}

		public void ShowCoveringLabel(Control control, ZLabel label)
		{
			label.Visible = true;
			if (control != null && !control.Controls.Contains(label))
			{
				control.Controls.Add(label);
			}

			label.BringToFront();
		}

		static ZLabel CopyCoveringLabelAppearance(ZLabel labelToCopy)
		{
			var copyLabel = new ZLabel();
			copyLabel.Name = labelToCopy.Name;
			copyLabel.Dock = labelToCopy.Dock;
			copyLabel.Visible = labelToCopy.Visible;
			copyLabel.IsFontBold = labelToCopy.IsFontBold;
			copyLabel.TextAlign = labelToCopy.TextAlign;
			copyLabel.Text = labelToCopy.Text;
			return copyLabel;
		}

		ZLabel CoveringLabel1
		{
			get
			{
				if (coveringLabel1 == null)
				{
					coveringLabel1 = new ZLabel();
					coveringLabel1.Name = "CoveringLabel";
					coveringLabel1.Dock = DockStyle.Fill;
					coveringLabel1.Visible = false;
					coveringLabel1.IsFontBold = true;
					coveringLabel1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
					coveringLabel1.Text = ViewDeniedMessage;
				}
				return coveringLabel1;
			}
		}
		ZLabel coveringLabel1;

		ZLabel CoveringLabel2
		{
			get
			{
				if (coveringLabel2 == null)
				{
					coveringLabel2 = CopyCoveringLabelAppearance(CoveringLabel1);
				}
				return coveringLabel2;
			}
		}
		ZLabel coveringLabel2;

		ZLabel CoveringLabel3
		{
			get
			{
				if (coveringLabel3 == null)
				{
					coveringLabel3 = CopyCoveringLabelAppearance(CoveringLabel1);
				}
				return coveringLabel3;
			}
		}
		ZLabel coveringLabel3;
		readonly ZLabel coveringLabel4;

		ZString ViewDeniedMessage
		{
			get { return Res.GetString("c6668d15-c2bb-4f8c-bfc5-79feead685aa", "** View Denied due to Security Access **"); }
		}

		#endregion

		#endregion

		#region Sales Tab Page

		void SetupSalesTabPage()
		{
			if (ShowSalesTabPage)
			{
				if (!this.GlbStaffTabControl.Controls.Contains(SalesTabPage))
				{
					this.GlbStaffTabControl.Controls.Add(SalesTabPage);
				}
			}
			else
			{
				if (this.GlbStaffTabControl.Controls.Contains(SalesTabPage))
				{
					this.GlbStaffTabControl.Controls.Remove(SalesTabPage);
				}
			}
		}

		internal ZTabPage SalesTabPage;

		#endregion

		#region Initialisation

		#region Initialize Form

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();

			if (!DesignModeFinder.IsDesigning)
			{
				GlbSecurityBoundGrid.DataSourceChanged += new EventHandler(GlbSecurityBoundGrid_DataSourceChanged);
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				GlbStaff staff = BusinessEntity;
				if (staff != null)
				{
					staff.StaffSecurityPermissionsCollection.Security = null;
				}

				SalesTabPage.Dispose();

				if (coveringLabel1 != null && !coveringLabel1.IsDisposed)
				{
					coveringLabel1.Dispose();
				}

				if (coveringLabel2 != null && !coveringLabel2.IsDisposed)
				{
					coveringLabel2.Dispose();
				}

				if (coveringLabel3 != null && !coveringLabel3.IsDisposed)
				{
					coveringLabel3.Dispose();
				}

				if (coveringLabel4 != null && !coveringLabel4.IsDisposed)
				{
					coveringLabel4.Dispose();
				}

				if (components != null)
				{
					components.Dispose();
				}

				if (cancellationToken != null)
				{
					cancellationToken.Cancel();
					cancellationToken.Dispose();
					cancellationToken = null;
				}
			}
			base.Dispose(disposing);
		}

		void GlbSecurityBoundGrid_DataSourceChanged(object sender, EventArgs e)
		{
			if (IsHandleCreated && !IsDisposed)
			{
				BeginInvoke(new MethodInvoker(HookIsAllowedColumn));
			}
		}

		void HookIsAllowedColumn()
		{
			var isAllowedColumn = GlbSecurityBoundGrid.Columns[GlbSecuritySchema.GU_SecurityItemIsAllowed.Name];
			if (isAllowedColumn != null)
			{
				((CheckBox)((ZCheckBoxColumnStyle)isAllowedColumn.ColumnStyle).EditControl).CheckedChanged +=
					(sender, e) =>
					{
						GlbSecurityBoundGrid.ListManager.EndCurrentEdit();
						RefreshStaffSecurityLabel();
					};
			}
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			if (Staff != null)
			{
				Staff.Groups.AttemptToDeleteFromAllUsers -= new EventHandler(DeleteFromAllUsersError);
				Staff.Groups.AttemptToDeleteFromDatabaseAccess -= new EventHandler(DeleteFromDatabaseAccessError);
				Staff.Groups.AttemptToDeleteFromSCIM -= new EventHandler(DeleteFromSCIMError);
				Staff.ChangeActualSecurityPermissions -= new EventHandler(RefreshStaffSecurityLabel);
				Staff.SuccessfulSaveNewStaffMember -= new EventHandler(SuccessfulSaveNewStaffMember);
				Staff.Groups.GroupChanged -= new EventHandler(ReloadGroupPermissions);
				Staff.GS_IsControllerInfo.ValueChanged -= new EventHandler(GS_IsControllerInfo_ValueChanged);
				Staff.SalesRepStatusChanged -= new GlbStaff.SalesRepStatusChangeHandler(SetupSalesTabPage);
				Staff.GS_WorkingLanguageInfo.ValueChanged -= GS_WorkingLanguageChanged;
				Staff.GS_WorkingLanguageInfo.AdditionalValidation -= GS_WorkingLanguageValidation;
				Staff.GS_IsActiveInfo.ValueChanged -= new EventHandler(GS_IsActive_ValueChanged);

				if (!Staff.IsDeleted)
				{
					UnHookDuplicationDetectEvents();
				}
			}
			base.SetDataBinding(dataSource, dataMember);
			if (Staff != null)
			{
				OldStaff_GS_PER = Staff.GS_PER;
				Staff.Groups.AttemptToDeleteFromAllUsers += new EventHandler(DeleteFromAllUsersError);
				Staff.Groups.AttemptToDeleteFromDatabaseAccess += new EventHandler(DeleteFromDatabaseAccessError);
				Staff.Groups.AttemptToDeleteFromDatabaseAccess += new EventHandler(DeleteFromSCIMError);
				Staff.ChangeActualSecurityPermissions += new EventHandler(RefreshStaffSecurityLabel);
				Staff.SuccessfulSaveNewStaffMember += new EventHandler(SuccessfulSaveNewStaffMember);
				Staff.Groups.GroupChanged += new EventHandler(ReloadGroupPermissions);
				Staff.GS_IsControllerInfo.ValueChanged += new EventHandler(GS_IsControllerInfo_ValueChanged);
				Staff.SalesRepStatusChanged += new GlbStaff.SalesRepStatusChangeHandler(SetupSalesTabPage);
				Staff.GS_WorkingLanguageInfo.ValueChanged += GS_WorkingLanguageChanged;
				Staff.GS_WorkingLanguageInfo.AdditionalValidation += GS_WorkingLanguageValidation;
				Staff.Validation.ValidateGS_WorkingLanguage();
				Staff.GS_IsActiveInfo.ValueChanged += new EventHandler(GS_IsActive_ValueChanged);

				if (!Staff.IsDeleted)
				{
					HookDuplicationDetectEvents();
				}

				if (!initialized)
				{
					initialized = true;
					Initialise();
				}
			}
		}
		bool initialized;

		void Initialise()
		{
			SetupSalesTabPage();

			ActivityLogTabPage.RunWhenBindingOrFirstShown(
				delegate
				{
					BindOpenRelatedObjectButton();
				});

			SecurityRightsTabPage.RunWhenBindingOrFirstShown(
				delegate
				{
					SetSecurityControlsVisibleProperty();
					SecurityPermissionsTreeView.Populate(fStaffSecurity);
					GroupRightsPanel.Visible = false;
					SecurityGridsSplitter.Visible = false;
					GroupRightsCollapsedPanel.Visible = true;
					changeOthersSecurityControl.ChangeOthersSecurity = Staff;
				});

			DetailsTabPage.RunWhenBindingOrFirstShown(
				delegate
				{
					SetupTabPageSecurity();
					InitialiseControllerOnlyAccess();
				});

			HRTabPage.RunWhenBindingOrFirstShown(
				delegate
				{
					EmploymentTabPage.RunWhenBindingOrFirstShown(
						delegate
						{
							SetEmploymentBasisHint();
						});
				});

			PasswordTabPage.RunWhenBindingOrFirstShown(
				delegate
				{
					SignatureImageSelectionControl.ReadOnly = false;
					ChangePasswordButton.Enabled = BusinessEntity.IsInDatabase && ShouldEnableChangePasswordButton;
					Staff.Validation.ValidateChangePasswordAtNextLogin();
					Staff.Validation.ValidatePasswordNeverChanges();
				});
			PasswordPanel.Visible = !BusinessEntity.IsInDatabase;
		}

		protected virtual void BindOpenRelatedObjectButton()
		{
			if (OpenRelatedObjectButton != null)
			{
				OpenRelatedObjectButton.DataBindings.Add(new KBinding("Text", Staff, "ActivityLogsForUser.RelatedBusinessObjectButtonText"));
			}
		}

		#endregion

		internal static string EmploymentBasisRegistryHint { get { return Res.GetString("899bdb9a-223f-4e8e-8be8-164265104685", "Employment Basis Types can be changed in the Registry under") + " "; } }

		void SetupSecurityPermissions()
		{
			using (Staff.SuspendSettingHasChanges())
			{
				fStaffSecurity = new SecurityCore(Staff.StaffSecurityPermissionsCollection, Staff, Env.CurrentBranch.PK,
					Env.CurrentDepartment.PK, Env.CurrentCompany.PK, false);

				fStaffGroupSecurity = new SecurityCore(Staff.GroupSecurityPermissionsCollection, Staff, Env.CurrentBranch.PK,
					Env.CurrentDepartment.PK, Env.CurrentCompany.PK, false);
				fStaffSecurity.CachingEnabled = false;

				Staff.StaffSecurityPermissionsCollection.HasChanges = false;
			}
		}

		internal void GlbStaffForm_Load(object sender, EventArgs e)
		{
			if (this.DisplayMode != ODisplayMode.Delete && Staff != null)
			{
				Staff.HasChanges = false;
				Staff.Groups.HasChanges = false;
			}

			GlbStaffTabControl.SelectedTab = DetailsTabPage;

			if (this.IsDesignMode())
			{
				return;
			}

			ISupportWebAddressValidationControlInitialise();
		}

		void GS_WorkingLanguageChanged(object sender, EventArgs e)
		{
			if (BusinessEntity.PK == GlbStaff.CurrentUser.PK)
			{
				Globals.Message.ShowInformation(Res.GetString("874172ff-45aa-4c17-8ce3-2abbc40ab6ff", "The language will be effective after you restart {0}", BrandingFactory.Instance.ProductName));
			}
		}

		void GS_WorkingLanguageValidation()
		{
			if (!EditInterfaceLanguageControl && ShowDetailsTabPage)
			{
				((INotifications)Staff.GS_WorkingLanguageInfo).AddWarning(Res.GetString("19e38fa8-fc41-4bfc-98e1-9a57d9770d69", "You do not have sufficient privilege to change language pack. Contact your system administrator for more details."));
			}
		}

		void GS_IsActive_ValueChanged(object sender, EventArgs e)
		{
			if (!Staff.GS_IsActive && Staff.UserIsLoggedIn())
			{
				Globals.Message.ShowWarning(Res.GetString("0faeb750-a2f7-4bf8-9af7-1434ebaa1c41", "This user is currently logged in and their login will be immediately terminated."));
			}
		}

		void InitialiseControllerOnlyAccess()
		{
			bool shouldEnable = GlbStaff.CurrentUser.GS_IsController || !GlbStaff.CurrentUser.GS_IsOperational;
			//Besides controllers, sysadmin is the only other account allowed to set people as controllers (the only non-operational system account)
			GS_IsControllerBoundCheck.Enabled = GlbStaff.CurrentUser.GS_IsController || (GlbStaff.CurrentUser.GS_IsSystemAccount && !GlbStaff.CurrentUser.GS_IsOperational);
			IsDatabaseDeveloperBoundCheck.Enabled = shouldEnable;
			IsReadOnlyDBUserBoundCheck.Enabled = shouldEnable;
			IsBackupOperatorBoundCheck.Enabled = shouldEnable && !EnvProxy.IsHostedWithCargowise;
			GS_IsOperationalBoundCheck.Enabled = shouldEnable;
			SalesRepCheckBox.Enabled = shouldEnable || SetIsSalesRepAllowed;
			DriverCheckBox.Enabled = shouldEnable;
			RobotCheckbox.Enabled = shouldEnable;

			if (Staff.IsCurrentUser)
			{
				EmailAddressesGrid.ReadOnly = !EditOwnEmailAllowed;
				GS_PublishEmailCheckBox.ReadOnly = !EditOwnEmailAllowed;
			}
		}

		void SetEmploymentBasisHint()
		{
			ZString pathToRegistryItem = SystemDataRegistry.Instance.StaffEmploymentTypes.Category + "/" + SystemDataRegistry.Instance.StaffEmploymentTypes.Caption;
			FormToolTip.SetToolTip(EmploymentBasisDropEdit, EmploymentBasisRegistryHint + pathToRegistryItem);
		}

		#region Credentials Tabs

		void AddPlugInsOnGlbStaffTabControl(ZString currentCountryCode)
		{
			GlbStaffTabControl.PlugIns.AddPlugInAtTabPageIndex(ControllerIDs.StaffCredentialsPlugIn, GlbStaffTabControl.TabPages.IndexOf(GroupsTabPage) + 1);
			GlbStaffTabControl.PlugIns.AddPlugInAtTabPageIndex(ControllerIDs.StaffNumberRangesPlugIn, GlbStaffTabControl.TabPages.IndexOf(GroupsTabPage) + 1);
		}

		void DisplayCredentialsPageForCountry(string countryCode)
		{
			foreach (TabPage page in GlbStaffTabControl.TabPages)
			{
				if (page.Name.IndexOf("Credentials") > -1)
				{
					if (!page.Name.StartsWith(countryCode))
					{
						GlbStaffTabControl.TabPages.Remove(page);
						page.Dispose();
					}
				}
			}
		}

		#endregion

		#endregion

		#region Detaching Groups From Staff

		readonly ResourceStringData DefaultDetachMessage = Res.GetData("d0a61245-f89e-4191-ace1-cea83fe2e37a", "Are you sure you want to detach the selected groups?");

		ResourceStringData GetMessageForLastStaff(IEnumerable<BusinessObject> toDetachBusinessObjects)
		{
			var message = string.Empty;
			var groupList = toDetachBusinessObjects;

			if (WorkflowDataRegistry.Instance.CheckOnlyNonSecurityGroupsOnRemovalOfLastStaff.Value)
			{
				groupList = groupList.Where(g => !(g as GlbGroup).GG_IsSecurityEnabled);
			}

			if (!groupList.Any())
			{
				MessageBoxButtons = MessageBoxButtons.YesNo;
				return DefaultDetachMessage;
			}

			var groupCapabilities = StaffCapabilityGroupHelper.GetGRPCapabilitiesWithLastStaffPerGroup(Staff.Factory, groupList.ToArray(), Staff.PK)
				.OrderByDescending(g => g.Value.Count()).ThenBy(g => g.Key);

			if (!groupCapabilities.Any())
			{
				MessageBoxButtons = MessageBoxButtons.YesNo;
				return DefaultDetachMessage;
			}

			foreach (var pair in groupCapabilities)
			{
				message += Res.GetString("56cedffb-e18c-4172-997a-0280a0ca86c2", "- This staff member is the last member of group {0} with capability(s) {1}.{2}",
					pair.Key, string.Join(", ", pair.Value.OrderBy(s => s)), System.Environment.NewLine);
			}

			message += System.Environment.NewLine;
			var confirmationMessage = Env.Security.UserAdminFormsLastObjectRemoval.IsAllowed
				? Res.GetString("142c4d62-8561-486b-9728-26dec6b70248", "Removing the groups from this staff member will mean that work relying on the combinations of group and capability above may get lost. Do you still want to remove the groups from this staff member?")
				: Res.GetString("FF8E4324-4FFE-491C-A6B7-3297462DA0C4", "Removing the groups from this staff member will mean that work relying on the combinations of group and capability above may get lost. You do not have the relevant permission to make this change.");

			MessageBoxButtons = Env.Security.UserAdminFormsLastObjectRemoval.IsAllowed ? MessageBoxButtons.YesNo : MessageBoxButtons.OK;
			return Res.GetData("5777a966-7fe2-471d-ae7a-4a591ac1442d", "{0}{1}").Format(message, confirmationMessage);
		}

		internal void MembersModuleButtonGrid_BeforeDetaching(object sender, ModuleButtonGridBeforeDetachEventArgs eventArgs)
		{
			var message = GetMessageForLastStaff(eventArgs.ToDetachBusinessObjects);

			if (message != DefaultDetachMessage)
			{
				MembersModuleButtonGrid.DetachMessage = message;
			}
		}

		void MembersModuleButtonGrid_InnerGrid_RowDeleteKeyDown(object sender, RowsDeletingEventArgs e)
		{
			var message = GetMessageForLastStaff(e.Objects);
			var dialogResult = Globals.Message.Show(Res.GetString("ab9c7811-3d5b-43d7-8117-06fd0587be44", "{0}", message.Caption),
				Res.GetString("a8ad6648-5071-4b99-8a85-59e4fb613106", "Confirm Detach..."), MessageBoxButtons, MessageBoxIcon.Information);

			if (dialogResult != DialogResult.Yes)
			{
				e.Cancel = true;
			}
		}

		void MembersModuleButtonGrid_OnDetached(object sender, ModuleButtonGridOnDetachedEventArgs eventArgs)
		{
			MembersModuleButtonGrid.DetachMessage = DefaultDetachMessage;
		}

		internal MessageBoxButtons MessageBoxButtons
		{
			get
			{
				return messageBoxButtons;
			}
			set
			{
				messageBoxButtons = value;
				MembersModuleButtonGrid.MessageBoxButtons = messageBoxButtons;
			}
		}

		MessageBoxButtons messageBoxButtons;

		#endregion

		protected override ContinueWithSave ShowPreSaveDialogs()
		{
			ContinueWithSave result = base.ShowPreSaveDialogs();

			if (result == ContinueWithSave.Yes && Staff.ShouldDetachSalesTeams)
			{
				string warningText = Res.GetString("b1cc3c55-654a-433e-8e05-c4fce1b27f36", "Making this Staff a non-Sales Representative will also remove this staff from its current Sales Teams. Do you wish to continue?");
				DialogResult messageResult = Globals.Message.Show(warningText, Res.GetString("62562be4-7c83-496e-a168-99a12ff1a452", "Saving a non-Sales Representative"), MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
				result = messageResult == DialogResult.OK ? ContinueWithSave.Yes : ContinueWithSave.No;
			}

			if (result == ContinueWithSave.Yes && Staff.IsADIntegrationEnabled && Staff.IsADLinked && Staff.GS_LoginNameInfo.HasChanges)
			{
				if (Staff.IsLoginNameInConflictWithExistingADAccount())
				{
					result = ContinueWithSave.No;
					Globals.Message.ShowError(Res.GetString("4218fc3a-8bc1-43c3-9521-fab193779377", "Changing this Login Name will result in a conflict with another record in Active Directory. If you wish to link this staff with a different record in Active Directory, select Actions > Active Directory > Disconnect from Active Directory first."), Res.GetString("2db377b0-953f-4136-9318-b62275327ff7", "Cannot Save"));
				}
			}

			if (result == ContinueWithSave.Yes)
			{
				var credentialsPlugin = GlbStaffTabControl.PlugIns.GetPlugIn(ControllerIDs.StaffCredentialsPlugIn);
				if (credentialsPlugin != null)
				{
					result = credentialsPlugin.ShowPreSaveDialogs();
				}
			}

			return result;
		}

		protected override ContinueWithSave ValidateAndSave()
		{
			if (Staff.GS_LoginNameInfo.HasChanges)
			{
				if (Globals.Message.ShowConfirmation(Res.GetString("43484F53-B21F-4134-A64C-37363270094E", "You have made changes to the {0}. Please re-enter the new {0} to confirm your changes.", Staff.GS_LoginNameInfo.HumanReadableName),
						Res.GetString("0AA1F561-6E1D-40BC-900E-ED1D660CA2A0", "Change {0}", Staff.GS_LoginNameInfo.HumanReadableName),
						Staff.GS_LoginNameInfo.Value.ToString(), MessageBoxIcon.Warning) != DialogResult.OK)
				{
					return ContinueWithSave.No;
				}
			}

			if (Staff.GS_IsActive)
			{
				if (!AddMissingMandatoryRoles())
				{
					return ContinueWithSave.No;
				}
			}
			else
			{
				if (!GlbStaffDirectReportsRemovalHelper.TransferOrRemoveDirectReports(Staff, StaffPopupModuleHelper, ParentForm))
				{
					return ContinueWithSave.No;
				}
			}

			glbStaffDirectReportsControl?.ModelView?.BuildTree();
			glbStaffManagementControl?.ModelView?.BuildTree();

			return base.ValidateAndSave();
		}

		bool AddMissingMandatoryRoles()
		{
			var success = true;
			var treeChanged = false;
			var missingMandatoryRoles = Staff.GetMissingMandatoryReportingRoles().Cast<StaffReportingRole>().ToArray();

			if (!Env.Security.StaffReportingManagerRolesAdd.IsAllowed && missingMandatoryRoles.Any())
			{
				var missingRolesString = string.Join(", ", missingMandatoryRoles.Select(x => x.Description));
				Globals.Message.ShowError(Res.GetString("a37eae12-ebc3-40b6-9100-f98960abb99c", "The form cannot be saved as the staff member is missing the mandatory reporting role(s): {0}. Please ask your administrator to add this role for the staff member.", missingRolesString));
				return false;
			}

			foreach (var role in missingMandatoryRoles)
			{
				var messageResult = Globals.Message.Show(Res.GetString("c651287c-e971-4baa-8e67-8c3f0f52543c", "This staff member does not have a current {0}. Click OK to open the Staff Manager form which will allow you to set the {0}.", role.Description), Res.GetString("270e1a41-15c9-4889-a7df-fd4b3f2cf09d", "Missing Mandatory Role"), MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
				if (messageResult == DialogResult.OK)
				{
					var manager = Staff.Factory.New<GlbStaffManager>();
					manager.GSM_GS_Staff = Staff.PK;
					manager.GSM_ManagerType = role.Code;

					manager.GSM_EffectiveDate = ZDateTime.Today;
					manager.MustBeCurrent = true;

					var form = new GlbStaffManagerForm(manager, true);
					ZFormModaliser.ShowDialogAndDispose(form, ParentForm);
					success &= !manager.IsDeleted;
					treeChanged |= !manager.IsDeleted;
				}
				else
				{
					success = false;
				}
			}

			if (treeChanged)
			{
				glbStaffManagementControl?.ModelView?.BuildTree();
			}

			return success;
		}

		protected virtual GlbStaffPopupModuleHelper StaffPopupModuleHelper
		{
			get
			{
				if (staffPopupModuleHelper == null)
				{
					staffPopupModuleHelper = new GlbStaffPopupModuleHelper();
				}

				return staffPopupModuleHelper;
			}
		}

		internal GlbStaffPopupModuleHelper staffPopupModuleHelper;

		#region Show Password Dialogs

		internal void ChangePasswordButton_Click(object sender, EventArgs e)
		{
			if (Staff.IsCurrentUser)
			{
				ChangePasswordDialog.ChangePassword(Staff);
			}
			else
			{
				ChangePasswordDialog.ResetPassword(Staff);
			}
			Staff.ReloadPasswordSettingsFromAD();
			Staff.ChangePasswordAtNextLoginInfo.RefreshBinding();
		}

		#endregion

		#region Viewing the excel Password

		void ViewExcelOpeningPassWordButton_Click(object sender, EventArgs e)
		{
			ViewPassword(ExcelOpeningPasswordTextBox, Res.GetString("GlbStaffForm|c8c50150-93b3-4803-89ef-48336831d62f", "Excel Opening Password"));
		}

		void ViewExcelModifyingPassWordButton_Click(object sender, EventArgs e)
		{
			ViewPassword(ExcelModifyingPasswordTextBox, Res.GetString("GlbStaffForm|9da5f716-adb3-40fc-b336-173c99f36c7a", "Excel Modifying Password"));
		}

		void ViewPassword(ZTextBox passwordTextBox, string caption)
		{
			Globals.Message.ShowInformation(passwordTextBox.Text, caption);
		}

		#endregion

		#region Security

		internal SecurityCore fStaffGroupSecurity;
		internal SecurityCore fStaffSecurity;

		internal string EffectiveSecurityRightsMessage
		{
			get { return Res.GetString("GlbStaffForm|EffectiveSecurityRightsMessage", "Effective Security Right for:") + " "; }
		}

		void SecurityPermissionsRefresh()
		{
			SecurityPermissionsTreeView.Focus();
			SetupForController(Staff.GS_IsController);
			if (string.IsNullOrEmpty(SecBranchFindBox.CurrentCode))
			{
				Staff.SecurityBranch = Enterprise.MasterFiles.Business.GlbBranch.CurrentBranch.PK;
			}

			if (string.IsNullOrEmpty(SecDepFindBox.CurrentCode))
			{
				Staff.SecurityDepartment = Enterprise.MasterFiles.Business.GlbDepartment.CurrentDepartment.PK;
			}

			if (SecurityPermissionsTreeView.SelectedNode is ZSecurityPointNode)
			{
				RefreshGroupRightsGrid(SecurityPermissionsTreeView.SelectedNode as ZSecurityPointNode);
				RefreshStaffSecurityLabel();
			}
		}

		void GS_IsControllerInfo_ValueChanged(object sender, EventArgs e)
		{
			SetSecurityControlsVisibleProperty();
		}

		void SetSecurityControlsVisibleProperty()
		{
			if (MainSecurityRightsPanel != null)
			{
				MainSecurityRightsPanel.Visible = !Staff.GS_IsController;
			}
			if (SystemAdministratorsLabel != null)
			{
				SystemAdministratorsLabel.Visible = Staff.GS_IsController;
			}
		}

		void SecurityPermissionsTreeView_BeforeSelect(object sender, TreeViewCancelEventArgs e)
		{
			// Make sure there are no errors in GlbSecurity for the current node before selecting the new node
			if (SecurityPermissionsTreeView.SelectedNode is ZSecurityPointNode)
			{
				try
				{
					GlbSecurityBoundGrid.ListManager.EndCurrentEdit();
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					e.Cancel = true;
				}
			}
		}

		void SetPanelVisible(Control visiblePanel)
		{
			foreach (Control control in securityPermissionsPanel.Controls.Cast<Control>().ToArray())
			{
				if (control != visiblePanel)
				{
					if (control.Visible)
					{
						control.Visible = false;
						control.SendToBack();
					}
				}
			}

			if (visiblePanel != null)
			{
				visiblePanel.Visible = true;
				visiblePanel.BringToFront();
			}
		}

		internal void SecurityPermissionsTreeView_AfterSelect(object sender, TreeViewEventArgs e)
		{
			ZSecurityPointNode selectedNode = (ZSecurityPointNode)e.Node;
			SecurityPermissionsTreeView.Focus(); // Hack to make grid lose focus and commit new row.

			if (selectedNode.Checkpoint.LookupKey == Env.Security.StaffLocalAdministratorPlaceholder.LookupKey)
			{
				changeOthersSecurityControl.Mode = GlbSecurityChangeOthersView.ChangeOthersMode.LocalAdministrator;
				SetPanelVisible(ChangeOthersSecurityPanel);
				Staff.SecurityChangeOthersView.Mode = GlbSecurityChangeOthersView.ChangeOthersMode.LocalAdministrator;
				Staff.SecurityChangeOthersView.Rebuild();
			}
			else if (selectedNode.Checkpoint.LookupKey == Env.Security.StaffGroupOwnerPlaceholder.LookupKey)
			{
				changeOthersSecurityControl.Mode = GlbSecurityChangeOthersView.ChangeOthersMode.GroupOwner;
				SetPanelVisible(ChangeOthersSecurityPanel);
				Staff.SecurityChangeOthersView.Mode = GlbSecurityChangeOthersView.ChangeOthersMode.GroupOwner;
				Staff.SecurityChangeOthersView.Rebuild();
			}
			else
			{
				SetPanelVisible(ActualSecPanel);

				if (selectedNode.Checkpoint.LookupKey == Env.Security.AgencyPrincipalAccess.LookupKey ||
					selectedNode.Checkpoint.LookupKey == Env.Security.WhsAllowedClients.LookupKey ||
					selectedNode.Checkpoint.LookupKey == Env.Security.WhsAllowedWarehouses.LookupKey)
				{
					RefreshAllowedOrgsAndWarehousesSecurityPanelData(selectedNode);

					AllowedOrgsAndWarehousesSecurityPanel.Visible = true;
					OrgsAndWarehousesSplitter.Visible = true;
				}
				else
				{
					AllowedOrgsAndWarehousesSecurityPanel.Visible = false;
					OrgsAndWarehousesSplitter.Visible = false;
				}

				var parentText = (SecurityPermissionsTreeView.SelectedNode.Parent != null) ? SecurityPermissionsTreeView.SelectedNode.Parent.Text + "|" : "";
				var effectiveSecurityRightsGroupBoxText = EffectiveSecurityRightsMessage + parentText + SecurityPermissionsTreeView.SelectedNode.Text;
				EffSecRightsBox.Text = effectiveSecurityRightsGroupBoxText.TruncateToFit(EffSecRightsBox.Font, EffSecRightsBox.Width * 2 - ControlDpiScalingHelper.ScaleToCurrentDpiX(200));

				RefreshStaffSecurityLabel();

				CheckpointLookupKey key = selectedNode.Checkpoint.LookupKey;
				Staff.StaffSecurityPermissionsView.FilterBySecurityKey(key, Staff.PK);
				RefreshGroupRightsGrid(selectedNode);
			}
		}

		void RefreshStaffSecurityLabel(object sender, EventArgs e)
		{
			RefreshStaffSecurityLabel();
		}

		void RefreshStaffSecurityLabel()
		{
			FilterActualSecurityRights();
			if (SecurityPermissionsTreeView.SelectedNode is ZSecurityPointNode)
			{
				ZSecurityPointNode node = SecurityPermissionsTreeView.SelectedNode as ZSecurityPointNode;
				ActualSecurityRightLabel.Visible = true;

				if (Staff.SecurityBranch == Guid.Empty || Staff.SecurityDepartment == Guid.Empty)
				{
					ActualSecurityRightLabel.Text = Res.GetString("GlbStaffForm|NA", "N/A");
				}
				else
				{
					ActualSecurityRightLabel.Text = node.Checkpoint.IsAllowed ? Res.GetString("GlbStaffForm|Yes", "Yes") : Res.GetString("GlbStaffForm|No", "No");
				}
			}
			else
			{
				ActualSecurityRightLabel.Visible = false;
			}
		}

		void FilterActualSecurityRights()
		{
			fStaffSecurity.BranchPK =
				(Staff.SecurityBranch != ZGuid.Empty && Staff.SecurityBranch != ZGuid.Invalid) ? Staff.SecurityBranch.ToGuid() : Guid.Empty;
			fStaffSecurity.CompanyPK =
				(Staff.SecurityCompany != ZGuid.Empty && Staff.SecurityCompany != ZGuid.Invalid) ? Staff.SecurityCompany.ToGuid() : Guid.Empty;
			fStaffSecurity.DepartmentPK =
				(Staff.SecurityDepartment != ZGuid.Empty && Staff.SecurityDepartment != ZGuid.Invalid) ? Staff.SecurityDepartment.ToGuid() : Guid.Empty;
		}

		#region Show / Hide Group Rights

		void ShowGroupRightsButton_Click(object sender, EventArgs e)
		{
			GroupRightsCollapsedPanel.Visible = false;
			SecurityGridsSplitter.Visible = true;
			GroupRightsPanel.Visible = true;

			if (SecurityPermissionsTreeView.SelectedNode is ZSecurityPointNode)
			{
				RefreshGroupRightsGrid(SecurityPermissionsTreeView.SelectedNode as ZSecurityPointNode);
			}
			else
			{
				GlbGroupSecurityGrid.DataSource = null;
			}
		}

		void RefreshGroupRightsGrid(ZSecurityPointNode node)
		{
			fStaffGroupSecurity.GetGroupRightsWithImplicitRights((SecurityCheckpoint)node.Checkpoint, Staff.GroupSecurityPermissionsCollectionForBinding);
			Staff.GroupSecurityPermissionsCollectionForBinding.Sort(new GlbSecuritySortIndexCalculator());
		}

		void HideGroupRightsButton_Click(object sender, EventArgs e)
		{
			GroupRightsPanel.Visible = false;
			SecurityGridsSplitter.Visible = false;
			GroupRightsCollapsedPanel.Visible = true;
		}

		#endregion

		internal void GlbSecurityBoundGrid_CurrentCellChanged(object sender, EventArgs e)
		{
			RefreshStaffSecurityLabel();
		}

		void SetupForController(bool isController)
		{
			if (isController)
			{
				GroupRightsCollapsedPanel.Visible = false;
			}
		}

		#region Refresh Allowed Organisations And Warehouses Control

		void RefreshAllowedOrgsAndWarehousesSecurityPanelData(ZSecurityPointNode node)
		{
			AllowedOrgsAndWarehousesSecurityPanel.SetAllowedOrgsAndWarehousesControlData(node.Checkpoint);
			RefreshAllowedOrgsAndWarehousesSecurityGrid(node);
			AllowedOrgsAndWarehousesSecurityPanel.TypeOfFindBoxCollection = Staff.SecurityAllowedOrgsAndWarehousesView.FindBoxCollectionType;
		}

		internal void RefreshAllowedOrgsAndWarehousesSecurityGrid(ZSecurityPointNode node)
		{
			if (node.Checkpoint.LookupKey == Env.Security.AgencyPrincipalAccess.LookupKey)
			{
				Staff.SecurityAllowedOrgsAndWarehousesView.SecurityRight = GlbSecurity.AllowedPrincipalsSecurityRightName;
			}
			else if (node.Checkpoint.LookupKey == Env.Security.WhsAllowedClients.LookupKey)
			{
				Staff.SecurityAllowedOrgsAndWarehousesView.SecurityRight = GlbSecurity.AllowedClientsSecurityRightName;
			}
			else if (node.Checkpoint.LookupKey == Env.Security.WhsAllowedWarehouses.LookupKey)
			{
				Staff.SecurityAllowedOrgsAndWarehousesView.SecurityRight = GlbSecurity.AllowedWarehousesSecurityRightName;
			}
		}

		#endregion

		#endregion

		#region Activity Logs

		void OpenRelatedObjectButton_Click(object sender, EventArgs e)
		{
			if (ActivityLogGrid.ListManager.Position > -1)
			{
				StmActivityLog log = (StmActivityLog)ActivityLogGrid.ListManager.GetCurrent();

				if (!log.S7_EnterpriseActivity)
				{
					Globals.Message.Show(Res.GetString("58ccd7cf-746c-4b75-9c18-2190fb483e1d", "This activity is for an external non-{0} activity and so cannot be shown.", BrandingFactory.Instance.ProductName));
				}
				else if (log.S7_ControllerID.IsEmpty || log.S7_ParentTableCode.IsEmpty)
				{
					Globals.Message.Show(Res.GetString("3F518EC1-A246-42a0-8C06-CE8E38443481", "No further details are available to be shown."));
				}
				else
				{
					ControllerID controllerID = new ControllerList().GetRegisteredIdentifierByName(log.S7_ControllerID);
					ZController controller = null;
					if (controllerID != null && (controller = ZControllerFactory.Create(controllerID)) != null)
					{
						BusinessObject bizo = Staff.Factory.Load(controller.TypeOfTopLevelBusinessObject, log.S7_ParentID);
						if (bizo != null)
						{
							controller.SetFormsModalTo(this);
							controller.ShowEditForm(bizo);
						}
						else
						{
							Globals.Message.Show(Res.GetString("6c4c8bbd-9fbf-4bdc-b6b4-ad060da5849d", "This activity was performed on a record that no longer exists."));
						}
					}
					else
					{
						Globals.Message.Show(Res.GetString("38972cd7-21c4-4fe5-8e4a-f7846804a21b", "The area of {0} where this activity occurred has been replaced or removed.", BrandingFactory.Instance.ProductName));
					}
				}
			}
		}

		internal void FindButton_Click(object sender, EventArgs e)
		{
			if (Staff.ActivityLogForUserFilterProvider.HasErrors)
			{
				Globals.Message.Show(Res.GetString("C7AEC78C-4DF8-484E-86D3-2C118A2769E2", "There are errors. Please correct these before searching."), Res.GetString("1C6990CC-9240-4735-9B97-63AFE307D8E0", "Errors..."), MessageBoxButtons.OK, MessageBoxIcon.Error, DialogResult.OK);
			}
			else
			{
				Staff.ActivityLogsForUser.Load();
			}
		}

		void ClearButton_Click(object sender, EventArgs e)
		{
			Staff.ActivityLogForUserFilterProvider.ClearActivityLogFilters();
			Staff.ActivityLogsForUser.Load();
		}

		#endregion

		#region IPreviousNextControlOverrideProvider

#if DEBUG
		ZPreviousNextControl IPreviousNextControlProvider.PreviousNextControlForTesting
		{
			get { return PreviousNextControlForTesting; }
			set { PreviousNextControlForTesting = value; }
		}

#endif

		bool IPreviousNextControlOverrideProvider.OverridesSetPreviousNextControlParentAndPosition
		{
			get
			{
				return true;
			}
		}

		bool IPreviousNextControlOverrideProvider.ShouldDoBaseSetPreviousNextControlParentAndPosition
		{
			get
			{
				return false;
			}
		}

		bool IPreviousNextControlOverrideProvider.OverridesGetPreviousNextControl
		{
			get
			{
				return false;
			}
		}
		void IPreviousNextControlOverrideProvider.SetPreviousNextControlParentAndPosition(ZPreviousNextControl control)
		{
			ControlDpiScalingHelper.SetTop(ref control, 0, false);
			ControlDpiScalingHelper.SetLeft(ref control, 6, true);
			control.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
			this.BottomPanel.Controls.Add(control);
			control.Visible = true;
		}

		ZPreviousNextControl IPreviousNextControlOverrideProvider.GetPreviousNextControl(ModuleResultsBusinessObject bizObj, ZController controller)
		{
			return new ZPreviousNextControl(bizObj, controller);
		}

		#endregion

		#region IWorkflowForm Members

		void IWorkflowTaskNavigationOverridable.NavigateToWorkflowItem(ProcessTask task)
		{
			if (BusinessEntity != null && task.P9_ParentID == BusinessEntity.PK)
			{
				TopLevelTabControl.SelectedTab = WorkflowTabPage;
				WorkflowTabPage.NavigateToWorkflowItem(task);
			}
		}

		void IWorkflowTaskNavigationOverridable.NavigateToWorkflowItem(IProcessHeader workflow)
		{
			if (BusinessEntity != null && workflow.FH_ParentId == BusinessEntity.PK)
			{
				TopLevelTabControl.SelectedTab = WorkflowTabPage;
				WorkflowTabPage.NavigateToWorkflowItem(workflow);
			}
			else
			{
				var task = workflow.Tasks.OfType<ProcessTask>().FirstOrDefault();

				if (task != null)
				{
					((IWorkflowTaskNavigationOverridable)this).NavigateToWorkflowItem(task);
				}
			}
		}

		#endregion

		#region Implementation

		new GlbStaff BusinessEntity
		{
			get { return (GlbStaff)base.BusinessEntity; }
		}

		#region WebAddressValidation

		public ZTextBox AddressCodeControl => null;

		public ZTextBox AdditionalAddressInformationControl => null;

		public ISupportWebAddressValidation Address
		{
			get
			{
				return CurrentDataItem as ISupportWebAddressValidation;
			}
		}

		public ZTextBox Address1Control
		{
			get
			{
				return GS_UserAddress1BoundText;
			}
		}

		public ZTextBox Address2Control
		{
			get
			{
				return GS_UserAddress2BoundText;
			}
		}

		public ZTextBox CityControl
		{
			get
			{
				return GS_CityBoundText;
			}
		}

		public ZTextBox PostcodeControl
		{
			get
			{
				return GS_PostcodeBoundText;
			}
		}

		public ZDropEdit StateControl
		{
			get
			{
				return GS_StateBoundDropEdit;
			}
		}

		public ZCodeFindBox CountryControl
		{
			get
			{
				return GS_CountryFindBox;
			}
		}

		ZButton ISupportWebAddressValidationControl.ValidateButton
		{
			get
			{
				return ValidateAddressButton;
			}
		}

		public Control SuggestionWindowParentControl
		{
			get
			{
				return DetailsTabPage;
			}
		}

		void RefreshValidationStatus()
		{
			if (Address != null)
			{
				AddressValidationUIHelper.SetButtonValidationStatus(ValidateAddressButton, Address.ValidationStatus, Address.IsErrorSuppressed);
				AddressValidationUIHelper.SetAddressFieldValidationStatus(Address1Control, Address2Control, CityControl, PostcodeControl, StateControl, CountryControl, Address.ValidationStatus);
				EmployeeDetailsGroupBox.Invalidate();
			}
		}

		async void ISupportWebAddressValidationControlInitialise()
		{
			if (Env.Instance.Registry.EnableAddressValidationWebService && Address != null)
			{
				Staff.PreValidationForAddressValidationService();
				var topLevelZForm = this.TopLevelControl as ZForm;
				if (topLevelZForm != null && topLevelZForm.DisplayMode != ODisplayMode.ReadOnly && !string.IsNullOrEmpty(Address.Address1) && Address.ValidationStatus != AddressValidationStatus.ManuallyVerified && !Address.Address1Info.ReadOnly)
				{
					using (Staff.SuspendSettingHasChanges())
					{
						await ValidateAddress();
					}
				}

				RefreshValidationStatus();

				//The form may have closed or the binding may have changed by the time the address has been validated, nothing to do if null.
				if (Address != null)
				{
					Address.AddressValidationStatusChanged += Address_AddressValidationStatusChanged;
					AddressSuggestionControlHelper.RegisterPropertyChangedEvent(Address, ValidateAddress);
					CityTownSuggestionControlHelper.RegisterPropertyChangedEvent(Address, GetCityTownAsync);

					ValidateAddressButton.ReadOnly = ClearFieldsButton.ReadOnly = Address.Address1Info.ReadOnly;
				}
			}
		}

		void Address_AddressValidationStatusChanged(object sender, EventArgs e)
		{
			if (Address != null)
			{
				RefreshValidationStatus();
				if (AddressValidationService.IsAddressInValidStatus(Address))
				{
					AddressSuggestionControlHelper.CloseSuggestionForm(Controls);
				}
			}
		}

		async internal Task ValidateAddress()
		{
			//Due to race conditions, we need a non-getter provided reference to the control
			var suggestionWindowParentControl = SuggestionWindowParentControl;
			try
			{
				if (AddressValidationService.IsAddressNeedValidation(Address) && suggestionWindowParentControl != null)
				{
					CleanseAction cleanseAction;

					if (!ValidationJustForced)
					{
						AddressSuggestionControlHelper.CloseSuggestionForm(suggestionWindowParentControl.Controls);
						cleanseAction = CleanseAction.QuickValidate;
					}
					else
					{
						cleanseAction = CleanseAction.ValidateAndSuggest;
						ValidationJustForced = false;
					}

					await AddressSuggestionControlHelper.ValidateAddressAsync(cancellationToken, Address, this, this, suggestionWindowParentControl.Controls, RefreshValidationStatus, null, 0, cleanseAction);
				}
			}
			catch (NullReferenceException e)
			{
				Globals.Message.ShowDeveloperException("This should be fixed by WI00176748, please contact the ROPE team. The NullReferenceException was caught from ValidateAddress().", e);
			}
		}

		async Task GetCityTownAsync()
		{
			if ((!string.IsNullOrEmpty(Address.City) || !string.IsNullOrEmpty(Address.Postcode)))
			{
				var location = SuggestionWindowParentControl.PointToClient(EmployeeDetailsGroupBox.PointToScreen(ControlDpiScalingHelper.NewScaledPoint(StateControl.Left, StateControl.Bottom + ControlDpiScalingHelper.ScaleToCurrentDpiY(1), false)));
				int maxWidth = StateControl.Width;
				await CityTownSuggestionControlHelper.GetCityTownAsync(cancellationToken, Address, this, this, SuggestionWindowParentControl.Controls, async () => { ValidationJustForced = true; await ValidateAddress(); ValidationJustForced = false; }, maxWidth);
			}
		}

		void HookISupportWebAddressValidationControlChangeFocusEvents()
		{
			SupportWebAddressValidationControlHelper.HookISupportWebAddressValidationControlChangeFocusEvents(this, ISupportWebAddressValidationControl_GotFocus, ISupportWebAddressValidationControl_LostFocus);
		}

		void ISupportWebAddressValidationControl_GotFocus(object sender, EventArgs e)
		{
			SupportWebAddressValidationControlHelper.ShowSuggestionControlsUponGotFocus(sender, Address, this);
		}

		void ISupportWebAddressValidationControl_LostFocus(object sender, EventArgs e)
		{
			SupportWebAddressValidationControlHelper.HideSuggestionControlsUponLostFocus(this);
		}

		async void ValidateAddressButton_Click(object sender, EventArgs e)
		{
			ValidationJustForced = true;
			await ValidateAddress();
		}

		void ClearFieldsButton_Click(object sender, EventArgs e)
		{
			SupportWebAddressValidationControlHelper.ClearFields(Address, this);
		}

		public bool ValidationJustForced { get; set; }

		public ISupportWebAddressValidation AddressForValidation
		{
			get { return BusinessEntity; }
		}

		public ZButton ClearAddressFieldsButton => ClearFieldsButton;
		public Func<Keys, bool> AddressValidationProcessCmdKey { get; set; }

		CancellationTokenSource cancellationToken;

		#endregion

		void ResetToDefaultGroupRightsButton_Click(object sender, EventArgs e)
		{
			BusinessEntity.StaffSecurityPermissionsCollection.ResetToGroupsDefaults();
		}

		void ImageSelectionControl_ImageObjectChangedByUser(object sender, EventArgs e)
		{
			if (BusinessEntity != null && !BusinessEntity.HasChanges)
			{
				BusinessEntity.HasChanges = true;
			}
		}

		#region ProcessCmdKey
		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			if (ProcessManuallyVerifyShortCutKey(ref msg, keyData))
			{
				return true;
			}

			return base.ProcessCmdKey(ref msg, keyData);
		}

		public bool ProcessManuallyVerifyShortCutKey(ref Message msg, Keys keyData)
		{
			if (SupportWebAddressValidationControlHelper.ProcessCommandKey(keyData, this.GetReadOnly(), this, Address))
			{
				return true;
			}
			return false;
		}
		#endregion

		#region Device Only

		public void IsDeviceOnlyCheckBox_Click(object sender, EventArgs e)
		{
			CheckDeviceOnlyRelatedOptions();
		}

		void CheckDeviceOnlyRelatedOptions()
		{
			this.IsBackupOperatorBoundCheck.SetReadOnly(IsDeviceOnlyCheckBox.Checked || Staff.GS_IsDevice);
			this.IsReadOnlyDBUserBoundCheck.SetReadOnly(IsDeviceOnlyCheckBox.Checked || Staff.GS_IsDevice);
			this.IsDatabaseDeveloperBoundCheck.SetReadOnly(IsDeviceOnlyCheckBox.Checked || Staff.GS_IsDevice);
			this.GS_IsControllerBoundCheck.SetReadOnly(IsDeviceOnlyCheckBox.Checked || Staff.GS_IsDevice);
			
			if (IsDeviceOnlyCheckBox.Checked || Staff.GS_IsDevice)
			{
				Staff.IsBackupOperator = false;
				Staff.IsReadOnlyDBUser = false;
				Staff.IsDatabaseDeveloper = false;
				Staff.GS_IsController = false;
			}
		}

		#endregion

		#endregion
	}
}
