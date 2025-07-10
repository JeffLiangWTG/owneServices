using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	partial class ContactsUserControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}

				UnHookOC_OA_OrgAddressValueChangedEvent();
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ContactsUserControl));
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo13 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo22 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo23 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo24 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo14 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo10 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo11 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo12 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo7 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo15 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo25 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo26 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo27 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo28 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo16 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo17 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo8 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo9 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo11 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo5 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo6 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo12 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo13 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo7 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo14 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo13 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo14 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo9 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			this.DuplicateDetectionStatusLabel = new Enterprise.ZArchitecture.ZLabel();
			this.DuplicateDetectionStatusIcon = new Enterprise.ZArchitecture.GUI.ZPictureBox();
			this.ConPersonalInfoGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.PersonalInfoTabControl = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
			this.PersonalDetailsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.NationalityCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.GenderDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.OC_PersonalInfoBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.OC_BirthdayBoundDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.AttributesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.AttributesTopPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.AttributesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.AttributesBottomPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.OpenURLButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.OrgContactBoundGrid = new Enterprise.ZArchitecture.ZGrid();
			this.AutoDeliveryButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.OrgDocumentBoundGrid = new Enterprise.ZArchitecture.ZGrid();
			this.DocDeliveryGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.DocumentsLabel = new Enterprise.ZArchitecture.ZLabel();
			this.SuppressedDocsButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.OC_AttachmentTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.OC_NotifyModeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.OC_SalutationTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DocDeliveryPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.PersonalInfoPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.OrgContactPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.ContactsFilterStringTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ContactsFilterOptionDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ShowInactiveContactsCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.OnlyShowWebAccessEnabledContactsCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ContactDetailsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.ContactDetailsTabControl = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
			this.DetailsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.ContactItemsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.zPanelContactDetails = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.SuggestedJobCategoriesLabel = new Enterprise.ZArchitecture.GUI.ZLinkLabel();
			this.ContactNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.JobTitleTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.OC_JobCategoryDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEditWithFixedWidth();
			this.ContactItemsListControl = new Enterprise.MasterFiles.GUI.ContactItemsListControl();
			this.LanguageDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ContactInfoPanel = new ZPanel();
			this.ContactSourceDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.DetailsVerifiedDropEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.JoinedIndustryYears = new Enterprise.ZArchitecture.ZLabel();
			this.JoinedCompanyYearsLabel = new Enterprise.ZArchitecture.ZLabel();
			this.JoinedIndustryDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.JoinedCompanyDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.ExcelOpeningPasswordTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ViewExcelOpeningPasswordButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ExcelModifyingPasswordTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ViewExcelModifyingPasswordButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.AddressOverrideAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.OC_OH_AddressOverrideBoundGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.SecurityTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.WebSecurityBottomPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.WebSecurityLabel = new Enterprise.ZArchitecture.ZLabel();
			this.WebSecuritySplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.ContactSecurityGrid = new Enterprise.ZArchitecture.ZGrid();
			this.WebWarehouseSecurityGrid = new Enterprise.ZArchitecture.ZGrid();
			this.WebSecurityTopPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.SendPasswordInstructionsButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.SendPasswordInstructionsLabel = new Enterprise.ZArchitecture.ZLabel();
			this.WebAccessCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.webSecurityLinkLabel = new Enterprise.ZArchitecture.GUI.ZLinkLabel();
			this.zCampaignsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.CampaignsTabControl = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
			this.CampaignTrackingTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.CampaignTrackingTopPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.CampaignsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.CampaignTrackingBottomPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.ResitExamButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ViewExamHistoryButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ViewExamButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ResendCampaignButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.SendCampaignButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ViewCampaignDocumentButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ViewCampaignButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.EditCampaignButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CampaignSubscriptionsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.SubscriptionsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.CertificatesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.CertificatesUserControl = new Enterprise.MasterFiles.GUI.CertificatesUserControl();
			this.AllocatedContactTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.AllocatedContactPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.AllocatedContactGrid = new Enterprise.ZArchitecture.ZGrid();
			this.ContactsTopSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.ContactsBottomSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.ContactsSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.EditToolStrip = new Enterprise.ZArchitecture.GUI.ZToolStrip();
			this.EditPersonButton = new Enterprise.ZArchitecture.GUI.ZToolStripButton();
			this.UnlockButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.LockoutDateTimeBoundDate = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			SendPasswordInstructionToolStripPanel = new ZPanel();
			SendPasswordInstructionStrip = new ZToolStrip();
			SendPasswordInstructionToolStripSplitButton = new ZToolStripSplitButton();
			UseWebTrackerStripButton = new ZToolStripButton();
			UseCargoWiseWebPortalsStripButton = new ZToolStripButton();
			this.SecurityPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ConPersonalInfoGroupBox.SuspendLayout();
			this.PersonalInfoTabControl.SuspendLayout();
			this.PersonalDetailsTabPage.SuspendLayout();
			this.NationalityCodeFindBox.SuspendLayout();
			this.GenderDropEdit.SuspendLayout();
			this.OC_BirthdayBoundDateEdit.SuspendLayout();
			this.AttributesTabPage.SuspendLayout();
			this.AttributesTopPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.AttributesGrid)).BeginInit();
			this.AttributesGrid.SuspendLayout();
			this.AttributesBottomPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.OrgContactBoundGrid)).BeginInit();
			this.OrgContactBoundGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.OrgDocumentBoundGrid)).BeginInit();
			this.OrgDocumentBoundGrid.SuspendLayout();
			this.DocDeliveryGroupBox.SuspendLayout();
			this.OC_AttachmentTypeDropEdit.SuspendLayout();
			this.OC_NotifyModeDropEdit.SuspendLayout();
			this.DocDeliveryPanel.SuspendLayout();
			this.PersonalInfoPanel.SuspendLayout();
			this.OrgContactPanel.SuspendLayout();
			this.ContactDetailsPanel.SuspendLayout();
			this.ContactDetailsTabControl.SuspendLayout();
			this.DetailsTabPage.SuspendLayout();
			this.ContactItemsGroupBox.SuspendLayout();
			this.zPanelContactDetails.SuspendLayout();
			this.OC_JobCategoryDropEdit.SuspendLayout();
			this.ContactItemsListControl.SuspendLayout();
			this.LanguageDropEdit.SuspendLayout();
			this.ContactInfoPanel.SuspendLayout();
			this.ContactSourceDropEdit.SuspendLayout();
			this.DetailsVerifiedDropEdit.SuspendLayout();
			this.JoinedIndustryDateEdit.SuspendLayout();
			this.ExcelOpeningPasswordTextBox.SuspendLayout();
			this.ViewExcelOpeningPasswordButton.SuspendLayout();
			this.ExcelModifyingPasswordTextBox.SuspendLayout();
			this.ViewExcelModifyingPasswordButton.SuspendLayout();
			this.JoinedCompanyDateEdit.SuspendLayout();
			this.AddressOverrideAddressControl.SuspendLayout();
			this.OC_OH_AddressOverrideBoundGuidFindBox.SuspendLayout();
			this.SecurityTabPage.SuspendLayout();
			this.WebSecurityBottomPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.WebSecuritySplitContainer)).BeginInit();
			this.WebSecuritySplitContainer.Panel1.SuspendLayout();
			this.WebSecuritySplitContainer.Panel2.SuspendLayout();
			this.WebSecuritySplitContainer.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ContactSecurityGrid)).BeginInit();
			this.ContactSecurityGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.WebWarehouseSecurityGrid)).BeginInit();
			this.WebWarehouseSecurityGrid.SuspendLayout();
			this.WebSecurityTopPanel.SuspendLayout();
			this.zCampaignsTabPage.SuspendLayout();
			this.CampaignsTabControl.SuspendLayout();
			this.CampaignTrackingTabPage.SuspendLayout();
			this.CampaignTrackingTopPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.CampaignsGrid)).BeginInit();
			this.CampaignsGrid.SuspendLayout();
			this.CampaignTrackingBottomPanel.SuspendLayout();
			this.CampaignSubscriptionsTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.SubscriptionsGrid)).BeginInit();
			this.SubscriptionsGrid.SuspendLayout();
			this.CertificatesTabPage.SuspendLayout();
			this.CertificatesUserControl.SuspendLayout();
			this.AllocatedContactTabPage.SuspendLayout();
			this.AllocatedContactPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.AllocatedContactGrid)).BeginInit();
			this.AllocatedContactGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ContactsTopSplitContainer)).BeginInit();
			this.ContactsTopSplitContainer.Panel1.SuspendLayout();
			this.ContactsTopSplitContainer.Panel2.SuspendLayout();
			this.ContactsTopSplitContainer.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ContactsBottomSplitContainer)).BeginInit();
			this.ContactsBottomSplitContainer.Panel1.SuspendLayout();
			this.ContactsBottomSplitContainer.Panel2.SuspendLayout();
			this.ContactsBottomSplitContainer.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ContactsSplitContainer)).BeginInit();
			this.ContactsSplitContainer.Panel1.SuspendLayout();
			this.ContactsSplitContainer.Panel2.SuspendLayout();
			this.ContactsSplitContainer.SuspendLayout();
			this.EditToolStrip.SuspendLayout();
			this.SuspendLayout();
			// 
			// SecurityPanel
			// 
			this.SecurityPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(975, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.OrgHeader);
			// 
			// ConPersonalInfoGroupBox
			// 
			this.ConPersonalInfoGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ContactsUserControl|139db808-4e77-4c86-aa3c-9b1231a38080", "Personal Information");
			this.ConPersonalInfoGroupBox.Controls.Add(this.PersonalInfoTabControl);
			this.ConPersonalInfoGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ConPersonalInfoGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ConPersonalInfoGroupBox.Name = "ConPersonalInfoGroupBox";
			this.ConPersonalInfoGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(338, 276, true);
			this.ConPersonalInfoGroupBox.TabIndex = 0;
			this.ConPersonalInfoGroupBox.TabStop = false;
			// 
			// PersonalInfoTabControl
			// 
			this.PersonalInfoTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.PersonalInfoTabControl.Controls.Add(this.PersonalDetailsTabPage);
			this.PersonalInfoTabControl.Controls.Add(this.AttributesTabPage);
			this.PersonalInfoTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PersonalInfoTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.PersonalInfoTabControl.Name = "PersonalInfoTabControl";
			this.PersonalInfoTabControl.SelectedIndex = 0;
			this.PersonalInfoTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(332, 257, true);
			this.PersonalInfoTabControl.TabIndex = 0;
			// 
			// PersonalDetailsTabPage
			// 
			this.PersonalDetailsTabPage.BackColor = System.Drawing.SystemColors.Control;
			this.PersonalDetailsTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ContactsUserControl|092838bb-d367-48c0-ae7e-6e608cb490e3", "Details");
			this.PersonalDetailsTabPage.Controls.Add(this.NationalityCodeFindBox);
			this.PersonalDetailsTabPage.Controls.Add(this.GenderDropEdit);
			this.PersonalDetailsTabPage.Controls.Add(this.OC_PersonalInfoBoundTextBox);
			this.PersonalDetailsTabPage.Controls.Add(this.OC_BirthdayBoundDateEdit);
			this.PersonalDetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.PersonalDetailsTabPage.Name = "PersonalDetailsTabPage";
			this.PersonalDetailsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.PersonalDetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(324, 230, true);
			this.PersonalDetailsTabPage.TabIndex = 0;
			// 
			// NationalityCodeFindBox
			// 
			this.NationalityCodeFindBox.AllowDrop = true;
			this.NationalityCodeFindBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.NationalityCodeFindBox, "FilteredContacts.OC_RN_NKNationality");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgContact)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).FilteredContacts)).SyncRoot)).OC_RN_NKNationality)));
			this.NationalityCodeFindBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ec3b24d3-7d1d-47f5-b442-69ee1284970f", "Nationality");
			this.NationalityCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(67, 26, true);
			this.NationalityCodeFindBox.Name = "NationalityCodeFindBox";
			this.NationalityCodeFindBox.PreBoundMaxLength = 2;
			this.NationalityCodeFindBox.ShouldResize = true;
			this.NationalityCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(252, 20, true);
			this.NationalityCodeFindBox.TabIndex = 2;
			// 
			// GenderDropEdit
			// 
			this.GenderDropEdit.AllowDrop = true;
			this.GenderDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.GenderDropEdit, "FilteredContacts.OC_Gender");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgContact)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).FilteredContacts)).SyncRoot)).OC_Gender)));
			this.GenderDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("7809ead3-5cc2-4607-b878-f32d3e4eb3b5", "Gender");
			this.GenderDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(202, 3, true);
			this.GenderDropEdit.Name = "GenderDropEdit";
			this.GenderDropEdit.PreBoundMaxLength = 1;
			this.GenderDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(116, 20, true);
			this.GenderDropEdit.TabIndex = 1;
			// 
			// OC_PersonalInfoBoundTextBox
			// 
			this.OC_PersonalInfoBoundTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.OC_PersonalInfoBoundTextBox, "FilteredContacts.OC_PersonalInfo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgContact)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).FilteredContacts)).SyncRoot)).OC_PersonalInfo)));
			this.OC_PersonalInfoBoundTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ContactsUserControl|8be15044-4ffd-4dc3-92e4-03997ac26165", "Personal / Family Information");
			this.OC_PersonalInfoBoundTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.OC_PersonalInfoBoundTextBox, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.OC_PersonalInfoBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 65, true);
			this.OC_PersonalInfoBoundTextBox.Multiline = true;
			this.OC_PersonalInfoBoundTextBox.Name = "OC_PersonalInfoBoundTextBox";
			this.OC_PersonalInfoBoundTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.OC_PersonalInfoBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(311, 161, true);
			this.OC_PersonalInfoBoundTextBox.TabIndex = 3;
			// 
			// OC_BirthdayBoundDateEdit
			// 
			this.OC_BirthdayBoundDateEdit.AllowDrop = true;
			this.OC_BirthdayBoundDateEdit.AutoCompleteMonthThreshold = 1;
			this.OC_BirthdayBoundDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.OC_BirthdayBoundDateEdit, "FilteredContacts.OC_Birthday");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgContact)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).FilteredContacts)).SyncRoot)).OC_Birthday)));
			this.OC_BirthdayBoundDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(67, 3, true);
			this.OC_BirthdayBoundDateEdit.Name = "OC_BirthdayBoundDateEdit";
			this.OC_BirthdayBoundDateEdit.TabIndex = 0;
			// 
			// EditToolStrip
			// 
			this.EditToolStrip.BackColor = System.Drawing.Color.Transparent;
			this.EditToolStrip.Dock = System.Windows.Forms.DockStyle.None;
			this.EditToolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.EditToolStrip.ImageScalingSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(21, 21, true);
			this.EditToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
			this.EditPersonButton});
			this.EditToolStrip.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(277, 3, true);
			this.EditToolStrip.Name = "EditToolStrip";
			this.EditToolStrip.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(25, 25, true);
			this.EditToolStrip.AutoSize = false;
			this.EditToolStrip.TabIndex = 23;
			// 
			// EditPersonButton
			// 
			this.EditPersonButton.Image = ((System.Drawing.Image)(resources.GetObject("EditPersonButton.Image")));
			this.EditPersonButton.ImageAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.EditPersonButton.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
			this.EditPersonButton.Name = "EditPersonButton";
			this.EditPersonButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(21, 21, true);
			this.EditPersonButton.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.EditPersonButton.Click += new System.EventHandler(this.EditPersonButton_Click);
			this.EditPersonButton.AutoSize = false;
			// 
			// AttributesTabPage
			// 
			this.AttributesTabPage.BackColor = System.Drawing.SystemColors.Control;
			this.AttributesTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ContactsUserControl|90a302d1-1e86-499b-8e5e-27e428e09444", "Interests && Social Networks");
			this.AttributesTabPage.Controls.Add(this.AttributesTopPanel);
			this.AttributesTabPage.Controls.Add(this.AttributesBottomPanel);
			this.AttributesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.AttributesTabPage.Name = "AttributesTabPage";
			this.AttributesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(324, 230, true);
			this.AttributesTabPage.TabIndex = 1;
			// 
			// AttributesTopPanel
			// 
			this.AttributesTopPanel.Controls.Add(this.AttributesGrid);
			this.AttributesTopPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AttributesTopPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.AttributesTopPanel.Name = "AttributesTopPanel";
			this.AttributesTopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(324, 200, true);
			this.AttributesTopPanel.TabIndex = 1;
			// 
			// AttributesGrid
			// 
			this.AttributesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.AttributesGrid, "FilteredContacts.Attributes");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgContact)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).FilteredContacts)).SyncRoot)).Attributes)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgContactAttribute)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgContact)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).FilteredContacts)).SyncRoot)).Attributes)).SyncRoot)).PC_Type)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgContactAttribute)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgContact)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).FilteredContacts)).SyncRoot)).Attributes)).SyncRoot)).AttributeDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgContactAttribute)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgContact)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).FilteredContacts)).SyncRoot)).Attributes)).SyncRoot)).PC_URL)));
			this.AttributesGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo13.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ContactsUserControl|001d8311-c2da-4e9a-a1f7-5e9405415a18", "Attribute");
			zDropEditColumnStyleInfo13.ColumnName = "PC_Type";
			zDropEditColumnStyleInfo13.ToolTip = "Specify any attributes that relate to this contact.";
			zDropEditColumnStyleInfo13.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo22.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ContactsUserControl|2d6bc9ae-b843-4ecc-8f22-887573344717", "Description");
			zTextBoxColumnStyleInfo22.ColumnName = "AttributeDescription";
			zTextBoxColumnStyleInfo22.ToolTip = "Shows a detailed description of the contact attribute.";
			zTextBoxColumnStyleInfo22.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(145);
			zTextBoxColumnStyleInfo23.ColumnName = "PC_URL";
			zTextBoxColumnStyleInfo23.ToolTip = "Shows the URL of the Social Networking Link.";
			zTextBoxColumnStyleInfo23.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.AttributesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo13);
			this.AttributesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo22);
			this.AttributesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo23);
			this.AttributesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AttributesGrid.GridId = "9de74b31-3782-4622-a82a-7520cfc669c7";
			this.AttributesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.AttributesGrid.LayoutKey = "OrgDocumentBoundGrid";
			this.AttributesGrid.LimitedColumns = null;
			this.AttributesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.AttributesGrid.Name = "AttributesGrid";
			this.AttributesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(324, 200, true);
			this.AttributesGrid.TabIndex = 0;
			this.AttributesGrid.DoubleClick += new System.EventHandler(this.AttributesGrid_DoubleClick);
			// 
			// AttributesBottomPanel
			// 
			this.AttributesBottomPanel.Controls.Add(this.OpenURLButton);
			this.AttributesBottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.AttributesBottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 200, true);
			this.AttributesBottomPanel.Name = "AttributesBottomPanel";
			this.AttributesBottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(324, 30, true);
			this.AttributesBottomPanel.TabIndex = 0;
			// 
			// OpenURLButton
			// 
			this.OpenURLButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.OpenURLButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ContactsUserControl|03c4cd82-a9e9-4958-b5c6-63f600274f30", "Open Web Page");
			this.OpenURLButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(220, 4, true);
			this.OpenURLButton.Name = "OpenURLButton";
			this.OpenURLButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.OpenURLButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 23, true);
			this.OpenURLButton.TabIndex = 0;
			this.OpenURLButton.ToolTipCaption = null;
			this.OpenURLButton.Click += new System.EventHandler(this.OpenURLButton_Click);
			// 
			// OrgContactBoundGrid
			// 
			this.OrgContactBoundGrid.AllowNavigation = false;
			this.OrgContactBoundGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.OrgContactBoundGrid, "FilteredContacts");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).FilteredContacts)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgContact)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).FilteredContacts)).SyncRoot)).OC_ContactName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgContact)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).FilteredContacts)).SyncRoot)).OC_Title)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgContact)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).FilteredContacts)).SyncRoot)).Location)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgContact)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).FilteredContacts)).SyncRoot)).JobCategoryDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgContact)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).FilteredContacts)).SyncRoot)).OC_Salutation)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgContact)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).FilteredContacts)).SyncRoot)).IsCustomerServiceContact)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgContact)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).FilteredContacts)).SyncRoot)).OC_IsActive)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgContact)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).FilteredContacts)).SyncRoot)).IsNDR)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.MasterFiles.Business.OrgContact)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).FilteredContacts)).SyncRoot)).OC_Birthday)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgContact)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).FilteredContacts)).SyncRoot)).OC_Gender)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgContact)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).FilteredContacts)).SyncRoot)).OC_RN_NKNationality)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgContact)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).FilteredContacts)).SyncRoot)).OC_Email)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgContact)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).FilteredContacts)).SyncRoot)).OC_Phone_Formatted)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgContact)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).FilteredContacts)).SyncRoot)).OC_HomePhone_Formatted)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgContact)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).FilteredContacts)).SyncRoot)).OC_Mobile_Formatted)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.MasterFiles.Business.OrgContact)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).FilteredContacts)).SyncRoot)).OC_DetailsVerified)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgContact)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).FilteredContacts)).SyncRoot)).OC_WebAccessEnabled)));
			this.OrgContactBoundGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.ColumnName = "OC_ContactName";
			zTextBoxColumnStyleInfo1.IsMandatory = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			zTextBoxColumnStyleInfo2.ColumnName = "OC_Title";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(190);
			zDropEditColumnStyleInfo1.ColumnName = "JobCategoryDescription";
			zDropEditColumnStyleInfo1.IsVisible = false;
			zDropEditColumnStyleInfo1.MaxDropDownItems = 20;
			zDropEditColumnStyleInfo1.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDropEditColumnStyleInfo14.ColumnName = "OC_Salutation";
			zDropEditColumnStyleInfo14.IsVisible = false;
			zDropEditColumnStyleInfo14.MaxDropDownItems = 20;
			zDropEditColumnStyleInfo14.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			zDropEditColumnStyleInfo14.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(230);
			zCheckBoxColumnStyleInfo10.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ContactsUserControl|dae22ba9-0315-417d-a727-3ef4e6cb6032", "CSV", "Specifies whether this contact been set up to receive Customer Service (CSV) documents.");
			zCheckBoxColumnStyleInfo10.ColumnName = "IsCustomerServiceContact";
			zCheckBoxColumnStyleInfo10.IsVisible = false;
			zCheckBoxColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zCheckBoxColumnStyleInfo11.ColumnName = "OC_IsActive";
			zCheckBoxColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zDateEditColumnStyleInfo4.ColumnName = "OC_DetailsVerified";
			zDateEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo4.IsVisible = true;
			zDateEditColumnStyleInfo4.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo4.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ContactsUserControl|a0aca92f-d09d-4de1-Bdd9-dF60cdacdf46", "Verified");
			zCheckBoxColumnStyleInfo12.ColumnName = "IsNDR";
			zCheckBoxColumnStyleInfo12.IsReadOnly = true;
			zCheckBoxColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo24.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ContactsUserControl|5b779ed4-2fcc-4b5a-a517-0a4efa2ebc85", "Location");
			zTextBoxColumnStyleInfo24.ColumnName = "Location";
			zTextBoxColumnStyleInfo24.IsReadOnly = true;
			zTextBoxColumnStyleInfo24.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo7.ColumnName = "OC_Birthday";
			zDateEditColumnStyleInfo7.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo7.IsVisible = false;
			zDateEditColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zDropEditColumnStyleInfo15.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("f5383aea-72db-42da-93b2-6d52258dff9f", "Sex", "Gender", "");
			zDropEditColumnStyleInfo15.ColumnName = "OC_Gender";
			zDropEditColumnStyleInfo15.IsVisible = false;
			zDropEditColumnStyleInfo15.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zCodeFindBoxColumnStyleInfo4.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("3c898117-bf83-4666-9682-52b4a89cce90", "Nat.", "Nationality", "");
			zCodeFindBoxColumnStyleInfo4.ColumnName = "OC_RN_NKNationality";
			zCodeFindBoxColumnStyleInfo4.IsVisible = false;
			zCodeFindBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zTextBoxColumnStyleInfo25.ColumnName = "OC_Email";
			zTextBoxColumnStyleInfo25.IsVisible = false;
			zTextBoxColumnStyleInfo25.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo26.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("42f3ceca-abdf-4846-9bf0-17deed5530eb", "Work Phone");
			zTextBoxColumnStyleInfo26.ColumnName = "OC_Phone_Formatted";
			zTextBoxColumnStyleInfo26.IsVisible = false;
			zTextBoxColumnStyleInfo26.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo27.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("7646d4e3-cebe-48c9-bed4-76f14f53c793", "Home Phone");
			zTextBoxColumnStyleInfo27.ColumnName = "OC_HomePhone_Formatted";
			zTextBoxColumnStyleInfo27.IsVisible = false;
			zTextBoxColumnStyleInfo27.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo28.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("8f691c4c-f246-4adb-a7b0-3cced3ae6926", "Mobile");
			zTextBoxColumnStyleInfo28.ColumnName = "OC_Mobile_Formatted";
			zTextBoxColumnStyleInfo28.IsVisible = false;
			zTextBoxColumnStyleInfo28.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCheckBoxColumnStyleInfo13.ColumnName = "OC_WebAccessEnabled";
			zCheckBoxColumnStyleInfo13.IsReadOnly = true;
			zCheckBoxColumnStyleInfo13.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("60742BD8-EC39-4308-9C84-DFBD208A67F1", "Web Access");
			zCheckBoxColumnStyleInfo13.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zCheckBoxColumnStyleInfo14.ColumnName = "WebAccessSuperseded";
			zCheckBoxColumnStyleInfo14.IsVisible = false;
			zCheckBoxColumnStyleInfo14.IsReadOnly = true;
			zCheckBoxColumnStyleInfo14.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("3b7bbf9d-f8cb-4fda-9d78-b6a21edf0766", "Web Access Superseded");
			zCheckBoxColumnStyleInfo14.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130);
			this.OrgContactBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.OrgContactBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.OrgContactBoundGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.OrgContactBoundGrid.ColumnStyles.Add(zDropEditColumnStyleInfo14);
			this.OrgContactBoundGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo10);
			this.OrgContactBoundGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo11);
			this.OrgContactBoundGrid.ColumnStyles.Add(zDateEditColumnStyleInfo4);
			this.OrgContactBoundGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo12);
			this.OrgContactBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo24);
			this.OrgContactBoundGrid.ColumnStyles.Add(zDateEditColumnStyleInfo7);
			this.OrgContactBoundGrid.ColumnStyles.Add(zDropEditColumnStyleInfo15);
			this.OrgContactBoundGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo4);
			this.OrgContactBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo25);
			this.OrgContactBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo26);
			this.OrgContactBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo27);
			this.OrgContactBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo28);
			this.OrgContactBoundGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo13);
			this.OrgContactBoundGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo14);
			this.OrgContactBoundGrid.GridId = "26b93be5-9298-40bb-8deb-30c7aaddcc13";
			this.OrgContactBoundGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.OrgContactBoundGrid.LayoutKey = "OrgContactBoundGrid";
			this.OrgContactBoundGrid.LimitedColumns = null;
			this.OrgContactBoundGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 26, true);
			this.OrgContactBoundGrid.Name = "OrgContactBoundGrid";
			this.OrgContactBoundGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(362, 293, true);
			this.OrgContactBoundGrid.TabIndex = 10;
			// 
			// AutoDeliveryButton
			// 
			this.AutoDeliveryButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.AutoDeliveryButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ContactsUserControl|d6708eec-31da-463b-97aa-c833503aea42", "Auto-Delivery Query Tool");
			this.AutoDeliveryButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(463, 44, true);
			this.AutoDeliveryButton.Name = "AutoDeliveryButton";
			this.AutoDeliveryButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.AutoDeliveryButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(151, 24, true);
			this.AutoDeliveryButton.TabIndex = 7;
			this.AutoDeliveryButton.ToolTipCaption = null;
			this.AutoDeliveryButton.Click += new System.EventHandler(this.AutoDeliveryButton_Click);
			// 
			// OrgDocumentBoundGrid
			// 
			this.OrgDocumentBoundGrid.AllowNavigation = false;
			this.OrgDocumentBoundGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.OrgDocumentBoundGrid, "FilteredContacts.Documents");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgContact)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).FilteredContacts)).SyncRoot)).Documents)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgDocument)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgContact)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).FilteredContacts)).SyncRoot)).Documents)).SyncRoot)).OD_DocumentGroup)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OrgDocument)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgContact)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).FilteredContacts)).SyncRoot)).Documents)).SyncRoot)).OD_SU_MenuItem)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgDocument)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgContact)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).FilteredContacts)).SyncRoot)).Documents)).SyncRoot)).OD_DefaultContact)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgDocument)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgContact)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).FilteredContacts)).SyncRoot)).Documents)).SyncRoot)).OD_DeliverBy)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgDocument)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgContact)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).FilteredContacts)).SyncRoot)).Documents)).SyncRoot)).OD_AttachmentType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgDocument)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgContact)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).FilteredContacts)).SyncRoot)).Documents)).SyncRoot)).OD_SendIndividually)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgDocument)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgContact)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).FilteredContacts)).SyncRoot)).Documents)).SyncRoot)).OD_FilterShipmentMode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OrgDocument)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgContact)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).FilteredContacts)).SyncRoot)).Documents)).SyncRoot)).OD_OH_RelatedFilterByParty)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgDocument)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgContact)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).FilteredContacts)).SyncRoot)).Documents)).SyncRoot)).OD_FilterLocalPort)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgDocument)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgContact)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).FilteredContacts)).SyncRoot)).Documents)).SyncRoot)).OD_FilterForeignPort)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgDocument)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgContact)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).FilteredContacts)).SyncRoot)).Documents)).SyncRoot)).OD_FilterDirection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OrgDocument)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgContact)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).FilteredContacts)).SyncRoot)).Documents)).SyncRoot)).OD_GB_FilterBranch)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OrgDocument)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgContact)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).FilteredContacts)).SyncRoot)).Documents)).SyncRoot)).OD_GC_FilterCompany)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OrgDocument)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgContact)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).FilteredContacts)).SyncRoot)).Documents)).SyncRoot)).OD_GE_FilterDepartment)));
			this.OrgDocumentBoundGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ContactsUserControl|71A7A2DB-E410-4125-8F64-389967AB01E7", "Group");
			zDropEditColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo2.ColumnName = "OD_DocumentGroup";
			zDropEditColumnStyleInfo2.IsMandatory = true;
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zGuidFindBoxColumnStyleInfo4.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ContactsUserControl|DCA3F8A0-A04A-4801-980B-00D23A14E9E9", "Document");
			zGuidFindBoxColumnStyleInfo4.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			zGuidFindBoxColumnStyleInfo4.ColumnName = "OD_SU_MenuItem";
			zGuidFindBoxColumnStyleInfo4.IsMandatory = true;
			zGuidFindBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ContactsUserControl|944BA9C5-355E-482C-AF66-FF2F60E244BC", "Official", "The official contact who will appear within the body of certain documents.");
			zCheckBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zCheckBoxColumnStyleInfo1.ColumnName = "OD_DefaultContact";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zDropEditColumnStyleInfo3.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ContactsUserControl|58b6a46a-f3ea-4f2a-8852-954d4d4646b8", "Deliver", "The method of delivery.");
			zDropEditColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo3.ColumnName = "OD_DeliverBy";
			zDropEditColumnStyleInfo3.IsMandatory = true;
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zDropEditColumnStyleInfo4.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ContactsUserControl|FDF0E786-C4A1-439A-8527-456CBC18D7EC", "Attach", "The attachment type, if Delivery Mode is Email.");
			zDropEditColumnStyleInfo4.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo4.ColumnName = "OD_AttachmentType";
			zDropEditColumnStyleInfo4.IsMandatory = true;
			zDropEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zCheckBoxColumnStyleInfo8.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ContactsUserControl|b8706bcc-aaf1-4345-b4d4-a9b25443726c", "Send Individually", "Multiple documents will be sent individually.");
			zCheckBoxColumnStyleInfo8.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zCheckBoxColumnStyleInfo8.ColumnName = "OD_SendIndividually";
			zCheckBoxColumnStyleInfo8.IsMandatory = true;
			zCheckBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zDropEditColumnStyleInfo16.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ContactsUserControl|D5DBABD0-03A5-41F6-94CB-9931B3F78916", "Mode", "Contact will receive documents only if the shipment mode matches this value.");
			zDropEditColumnStyleInfo16.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo16.ColumnName = "OD_FilterShipmentMode";
			zDropEditColumnStyleInfo16.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(37);
			zGuidFindBoxColumnStyleInfo5.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ContactsUserControl|66BC18FB-908E-43FA-ABCF-75486B7407AA", "Related Party");
			zGuidFindBoxColumnStyleInfo5.ColumnName = "OD_OH_RelatedFilterByParty";
			zGuidFindBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zCodeFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ContactsUserControl|93A9E98C-1633-46B9-B945-380E83F146FF", "Local", "Contact will receive documents only if the local port on the job matches this value.");
			zCodeFindBoxColumnStyleInfo1.ColumnName = "OD_FilterLocalPort";
			zCodeFindBoxColumnStyleInfo1.PopupCaption = "Local Port for documents";
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(55);
			zCodeFindBoxColumnStyleInfo5.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ContactsUserControl|BE39CE96-9D50-45E6-8416-57D271CCC3BD", "Foreign", "Contact will receive documents only if the foreign port on the job matches this value.");
			zCodeFindBoxColumnStyleInfo5.ColumnName = "OD_FilterForeignPort";
			zCodeFindBoxColumnStyleInfo5.PopupCaption = "Foreign Port for documents";
			zCodeFindBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(55);
			zDropEditColumnStyleInfo17.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ContactsUserControl|110CBAD2-F9F5-446A-BEC6-277BF1303186", "Direction", "Direction for documents - i.e. Import or Export.");
			zDropEditColumnStyleInfo17.ColumnName = "OD_FilterDirection";
			zDropEditColumnStyleInfo17.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zGuidFindBoxColumnStyleInfo7.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ContactsUserControl|bd36ae63-fe4b-42ff-a0e7-7ea62a7105ba", "Company", "Contact will receive documents only if the Company nominated on the job matches this value.");
			zGuidFindBoxColumnStyleInfo7.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			zGuidFindBoxColumnStyleInfo7.ColumnName = "OD_GC_FilterCompany";
			zGuidFindBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zGuidFindBoxColumnStyleInfo8.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ContactsUserControl|896935ab-611f-4672-a8dc-71f638c08402", "Branch", "Contact will receive documents only if the Branch nominated on the job matches this value.");
			zGuidFindBoxColumnStyleInfo8.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			zGuidFindBoxColumnStyleInfo8.ColumnName = "OD_GB_FilterBranch";
			zGuidFindBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zGuidFindBoxColumnStyleInfo9.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ContactsUserControl|76f84a48-91f2-497b-9c28-dd0415c881a0", "Department", "Contact will receive documents only if the Department nominated on the job matches this value.");
			zGuidFindBoxColumnStyleInfo9.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			zGuidFindBoxColumnStyleInfo9.ColumnName = "OD_GE_FilterDepartment";
			zGuidFindBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.OrgDocumentBoundGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.OrgDocumentBoundGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo4);
			this.OrgDocumentBoundGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.OrgDocumentBoundGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.OrgDocumentBoundGrid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
			this.OrgDocumentBoundGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo8);
			this.OrgDocumentBoundGrid.ColumnStyles.Add(zDropEditColumnStyleInfo16);
			this.OrgDocumentBoundGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo5);
			this.OrgDocumentBoundGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.OrgDocumentBoundGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo5);
			this.OrgDocumentBoundGrid.ColumnStyles.Add(zDropEditColumnStyleInfo17);
			this.OrgDocumentBoundGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo7);
			this.OrgDocumentBoundGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo8);
			this.OrgDocumentBoundGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo9);
			this.OrgDocumentBoundGrid.GridId = "3dbd9564-ea70-4a8a-915c-21f6cf8da581";
			this.OrgDocumentBoundGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.OrgDocumentBoundGrid.LayoutKey = "OrgDocumentBoundGrid";
			this.OrgDocumentBoundGrid.LimitedColumns = null;
			this.OrgDocumentBoundGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 95, true);
			this.OrgDocumentBoundGrid.Name = "OrgDocumentBoundGrid";
			this.OrgDocumentBoundGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(613, 176, true);
			this.OrgDocumentBoundGrid.TabIndex = 9;
			// 
			// DocDeliveryGroupBox
			// 
			this.DocDeliveryGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ContactsUserControl|0679b2bc-89a8-4e82-be86-808da1321feb", "Document Delivery Details");
			this.DocDeliveryGroupBox.Controls.Add(this.DocumentsLabel);
			this.DocDeliveryGroupBox.Controls.Add(this.SuppressedDocsButton);
			this.DocDeliveryGroupBox.Controls.Add(this.OC_AttachmentTypeDropEdit);
			this.DocDeliveryGroupBox.Controls.Add(this.OC_NotifyModeDropEdit);
			this.DocDeliveryGroupBox.Controls.Add(this.OC_SalutationTextBox);
			this.DocDeliveryGroupBox.Controls.Add(this.OrgDocumentBoundGrid);
			this.DocDeliveryGroupBox.Controls.Add(this.AutoDeliveryButton);
			this.DocDeliveryGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DocDeliveryGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 0, true);
			this.DocDeliveryGroupBox.Name = "DocDeliveryGroupBox";
			this.DocDeliveryGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(628, 276, true);
			this.DocDeliveryGroupBox.TabIndex = 0;
			this.DocDeliveryGroupBox.TabStop = false;
			// 
			// DocumentsLabel
			// 
			this.DocumentsLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ContactsUserControl|e629c966-6167-4622-b8db-a116abbfd2b6", "Documents To Receive");
			this.DocumentsLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.DocumentsLabel.IsFontBold = true;
			this.DocumentsLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 72, true);
			this.DocumentsLabel.Name = "DocumentsLabel";
			this.DocumentsLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 20, true);
			this.DocumentsLabel.TabIndex = 8;
			// 
			// SuppressedDocsButton
			// 
			this.SuppressedDocsButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.SuppressedDocsButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ContactsUserControl|f267e907-6409-4bcd-94fb-9237d2d0fc67", "Documents to Never Deliver");
			this.SuppressedDocsButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(463, 16, true);
			this.SuppressedDocsButton.Name = "SuppressedDocsButton";
			this.SuppressedDocsButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.SuppressedDocsButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(151, 24, true);
			this.SuppressedDocsButton.TabIndex = 6;
			this.SuppressedDocsButton.ToolTipCaption = null;
			this.SuppressedDocsButton.Click += new System.EventHandler(this.SuppressedDocsButton_Click);
			// 
			// OC_AttachmentTypeDropEdit
			// 
			this.OC_AttachmentTypeDropEdit.AllowDrop = true;
			this.OC_AttachmentTypeDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.OC_AttachmentTypeDropEdit, "FilteredContacts.OC_AttachmentType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgContact)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).FilteredContacts)).SyncRoot)).OC_AttachmentType)));
			this.OC_AttachmentTypeDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ContactsUserControl|5dedb2f7-7e16-409e-8de6-2cd1975c173c", "Default Attachment Type");
			this.OC_AttachmentTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(399, 45, true);
			this.OC_AttachmentTypeDropEdit.Name = "OC_AttachmentTypeDropEdit";
			this.OC_AttachmentTypeDropEdit.PreBoundMaxLength = 3;
			this.OC_AttachmentTypeDropEdit.ShowDescriptionBox = false;
			this.OC_AttachmentTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.OC_AttachmentTypeDropEdit.TabIndex = 5;
			// 
			// OC_NotifyModeDropEdit
			// 
			this.OC_NotifyModeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OC_NotifyModeDropEdit, "FilteredContacts.OC_NotifyMode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgContact)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).FilteredContacts)).SyncRoot)).OC_NotifyMode)));
			this.OC_NotifyModeDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ContactsUserControl|d19ca8d5-9a23-4cf8-9609-ea788e1ddba2", "Default Delivery Method");
			this.OC_NotifyModeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 45, true);
			this.OC_NotifyModeDropEdit.Name = "OC_NotifyModeDropEdit";
			this.OC_NotifyModeDropEdit.PreBoundMaxLength = 3;
			this.OC_NotifyModeDropEdit.ShowDescriptionBox = false;
			this.OC_NotifyModeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.OC_NotifyModeDropEdit.TabIndex = 3;
			// 
			// OC_SalutationTextBox
			// 
			this.OC_SalutationTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.OC_SalutationTextBox, "FilteredContacts.OC_Salutation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgContact)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).FilteredContacts)).SyncRoot)).OC_Salutation)));
			this.OC_SalutationTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ContactsUserControl|54d17286-b17f-4887-8dc1-68e2a010e1ec", "Document Salutation", "Name to appear on documents following the salutation. For example, \'John\' will produce \'Dear John\' on documents.");
			this.OC_SalutationTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.OC_SalutationTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 19, true);
			this.OC_SalutationTextBox.Name = "OC_SalutationTextBox";
			this.OC_SalutationTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(315, 20, true);
			this.OC_SalutationTextBox.TabIndex = 1;
			// 
			// DocDeliveryPanel
			// 
			this.DocDeliveryPanel.Controls.Add(this.DocDeliveryGroupBox);
			this.DocDeliveryPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DocDeliveryPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DocDeliveryPanel.Name = "DocDeliveryPanel";
			this.DocDeliveryPanel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(5, 0, 0, 0, true);
			this.DocDeliveryPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(633, 276, true);
			this.DocDeliveryPanel.TabIndex = 0;
			// 
			// PersonalInfoPanel
			// 
			this.PersonalInfoPanel.Controls.Add(this.ConPersonalInfoGroupBox);
			this.PersonalInfoPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PersonalInfoPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.PersonalInfoPanel.Name = "PersonalInfoPanel";
			this.PersonalInfoPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(338, 276, true);
			this.PersonalInfoPanel.TabIndex = 0;
			// 
			// OrgContactPanel
			// 
			this.OrgContactPanel.Controls.Add(this.ContactsFilterStringTextBox);
			this.OrgContactPanel.Controls.Add(this.ContactsFilterOptionDropEdit);
			this.OrgContactPanel.Controls.Add(this.ShowInactiveContactsCheckBox);
			this.OrgContactPanel.Controls.Add(this.OnlyShowWebAccessEnabledContactsCheckBox);
			this.OrgContactPanel.Controls.Add(this.OrgContactBoundGrid);
			this.OrgContactPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.OrgContactPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.OrgContactPanel.Name = "OrgContactPanel";
			this.OrgContactPanel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, 0, 5, 0, true);
			this.OrgContactPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(364, 325, true);
			this.OrgContactPanel.TabIndex = 0;
			// 
			// ContactsFilterOptionDropEdit
			// 
			this.ContactsFilterOptionDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ContactsFilterOptionDropEdit, "ContactsFilterOption");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ContactsFilterOption)));
			this.ContactsFilterOptionDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ContactsFilterOptionDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ContactsUserControl|96777BEE-02AA-F56F-BB09-9852F13B8ECC", "Filter");
			this.ContactsFilterOptionDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(33, 4, true);
			this.ContactsFilterOptionDropEdit.Name = "ContactsFilterOptionDropEdit";
			this.ContactsFilterOptionDropEdit.PreBoundMaxLength = 3;
			this.ContactsFilterOptionDropEdit.ShowDescriptionBox = false;
			this.ContactsFilterOptionDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(60, 20, true);
			this.ContactsFilterOptionDropEdit.TabIndex = 5;
			this.ContactsFilterOptionDropEdit.Visible = false;
			// 
			// ContactsFilterStringTextBox
			// 
			this.ContactsFilterStringTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
				| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ContactsFilterStringTextBox, "ContactsFilterString");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ContactsFilterString)));
			this.ContactsFilterStringTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ContactsUserControl|96777BEE-02AA-F56F-BB09-9852F13B8ECC", "Filter");
			this.ContactsFilterStringTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(35, 4, true);
			this.ContactsFilterStringTextBox.Name = "ContactsFilterStringTextBox";
			this.ContactsFilterStringTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(70, 20, true);
			this.ContactsFilterStringTextBox.TabIndex = 6;
			this.ContactsFilterStringTextBox.TextChanged += new System.EventHandler(this.ContactsFilterStringTextBox_TextChanged);
			this.ContactsFilterStringTextBox.ReadOnlyChanged += ContactsFilterStringTextBox_ReadOnlyChanged;
			// 
			// ShowInactiveContactsCheckBox
			// 
			this.ShowInactiveContactsCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ShowInactiveContactsCheckBox, "IncludeInactiveContacts");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((bool)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).IncludeInactiveContacts)));
			this.ShowInactiveContactsCheckBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ContactsUserControl|96B45BEE-A228-44FF-8509-3DC4F13B8E7D", "Show Inactive", "Show Inactive Contacts. Inactive contacts with errors or unsaved contents will always be displayed until they are saved.");
			this.ShowInactiveContactsCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ShowInactiveContactsCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 6, true);
			this.ShowInactiveContactsCheckBox.Name = "ShowInactiveContactsCheckBox";
			this.ShowInactiveContactsCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(94, 18, true);
			this.ShowInactiveContactsCheckBox.TabIndex = 7;
			this.ShowInactiveContactsCheckBox.UseVisualStyleBackColor = true;
			this.ShowInactiveContactsCheckBox.ReadOnlyChanged += ShowInactiveContactsCheckBox_ReadOnlyChanged;
			// 
			// OnlyShowWebAccessEnabledContactsCheckBox
			// 
			this.OnlyShowWebAccessEnabledContactsCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.OnlyShowWebAccessEnabledContactsCheckBox, "OnlyShowWebAccessEnabledContacts");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((bool)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).OnlyShowWebAccessEnabledContacts)));
			this.OnlyShowWebAccessEnabledContactsCheckBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ContactsUserControl|75a494a1-3481-488a-aeb8-64c90281c2f2", "Web Access Enabled Only", "Show Web Access Enabled Contacts.");
			this.OnlyShowWebAccessEnabledContactsCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.OnlyShowWebAccessEnabledContactsCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(210, 6, true);
			this.OnlyShowWebAccessEnabledContactsCheckBox.Name = "OnlyShowWebAccessEnabledContactsCheckBox";
			this.OnlyShowWebAccessEnabledContactsCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 18, true);
			this.OnlyShowWebAccessEnabledContactsCheckBox.TabIndex = 8;
			this.OnlyShowWebAccessEnabledContactsCheckBox.UseVisualStyleBackColor = true;
			this.OnlyShowWebAccessEnabledContactsCheckBox.ReadOnlyChanged += OnlyShowWebAccessEnabledContactsCheckBox_ReadOnlyChanged;
			// 
			// ContactDetailsPanel
			// 
			this.ContactDetailsPanel.Controls.Add(this.ContactDetailsTabControl);
			this.ContactDetailsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ContactDetailsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ContactDetailsPanel.Name = "ContactDetailsPanel";
			this.ContactDetailsPanel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(5, 0, 0, 0, true);
			this.ContactDetailsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(607, 325, true);
			this.ContactDetailsPanel.TabIndex = 1;
			// 
			// ContactDetailsTabControl
			// 
			this.ContactDetailsTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.ContactDetailsTabControl.Controls.Add(this.DetailsTabPage);
			this.ContactDetailsTabControl.Controls.Add(this.SecurityTabPage);
			this.ContactDetailsTabControl.Controls.Add(this.zCampaignsTabPage);
			this.ContactDetailsTabControl.Controls.Add(this.CertificatesTabPage);
			this.ContactDetailsTabControl.Controls.Add(this.AllocatedContactTabPage);
			this.ContactDetailsTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ContactDetailsTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 0, true);
			this.ContactDetailsTabControl.Name = "ContactDetailsTabControl";
			this.ContactDetailsTabControl.SelectedIndex = 0;
			this.ContactDetailsTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(602, 325, true);
			this.ContactDetailsTabControl.TabIndex = 0;
			// 
			// DetailsTabPage
			// 
			this.DetailsTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ContactsUserControl|2db369d3-3c01-4139-93fb-c167aece32a2", "Details");
			this.DetailsTabPage.Controls.Add(this.ContactItemsGroupBox);
			this.DetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.DetailsTabPage.Name = "DetailsTabPage";
			this.DetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(594, 298, true);
			this.DetailsTabPage.TabIndex = 0;
			// 
			// ContactItemsGroupBox
			// 
			this.ContactItemsGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("79e1757b-2df2-492c-aa2e-85de0f029e25", "Contact Details");
			this.ContactItemsGroupBox.Controls.Add(this.zPanelContactDetails);
			this.ContactItemsGroupBox.Controls.Add(this.ContactInfoPanel);
			this.ContactItemsGroupBox.Controls.Add(this.DuplicateDetectionStatusLabel);
			this.ContactItemsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ContactItemsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ContactItemsGroupBox.Name = "ContactItemsGroupBox";
			this.ContactItemsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(710, 298, true);
			this.ContactItemsGroupBox.TabIndex = 34;
			this.ContactItemsGroupBox.TabStop = false;
			// 
			// DuplicateDetectionStatusLabel
			// 
			this.DuplicateDetectionStatusLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.DuplicateDetectionStatusLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 0, true);
			this.DuplicateDetectionStatusLabel.Name = "DuplicateDetectionStatusLabel";
			this.DuplicateDetectionStatusLabel.AutoSize = true;
			this.DuplicateDetectionStatusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.DuplicateDetectionStatusLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ECF03C60-8216-4EC8-BBA9-3CB30F0A8E4A", "Detecting duplicates");
			this.DuplicateDetectionStatusLabel.Visible = false;
			// 
			// DuplicateDetectionStatusIcon
			// 
			this.DuplicateDetectionStatusIcon.Image = global::Enterprise.MasterFiles.GUI.Properties.Resources.loader;
			this.DuplicateDetectionStatusIcon.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(284, 5, true);
			this.DuplicateDetectionStatusIcon.Name = "DuplicateDetectionStatusIcon";
			this.DuplicateDetectionStatusIcon.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(20, 20, true);
			this.DuplicateDetectionStatusIcon.TabIndex = 42;
			this.DuplicateDetectionStatusIcon.Visible = false;
			this.DuplicateDetectionStatusIcon.SizeMode = PictureBoxSizeMode.StretchImage;
			// 
			// zPanelContactDetails
			// 
			this.zPanelContactDetails.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left))));
			this.zPanelContactDetails.AutoScroll = true;
			this.zPanelContactDetails.Controls.Add(this.SuggestedJobCategoriesLabel);
			this.zPanelContactDetails.Controls.Add(this.ContactNameTextBox);
			this.zPanelContactDetails.Controls.Add(this.JobTitleTextBox);
			this.zPanelContactDetails.Controls.Add(this.OC_JobCategoryDropEdit);
			this.zPanelContactDetails.Controls.Add(this.ContactItemsListControl);
			this.zPanelContactDetails.Controls.Add(this.LanguageDropEdit);
			this.zPanelContactDetails.Controls.Add(this.DuplicateDetectionStatusIcon);
			this.zPanelContactDetails.Controls.Add(this.EditToolStrip);
			this.zPanelContactDetails.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 17, true);
			this.zPanelContactDetails.Name = "zPanelContactDetails";
			this.zPanelContactDetails.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(350, 277, true);
			this.zPanelContactDetails.TabIndex = 0;
			// 
			// SuggestedJobCategoriesLabel
			// 
			this.SuggestedJobCategoriesLabel.AutoSize = true;
			this.SuggestedJobCategoriesLabel.IsFontBold = false;
			this.SuggestedJobCategoriesLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(278, 54, true);
			this.SuggestedJobCategoriesLabel.Name = "SuggestedJobCategoriesLabel";
			this.SuggestedJobCategoriesLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("aaa0bbdc-1ac7-45dd-b6ea-64efbdd17d82", "Suggestion");
			this.SuggestedJobCategoriesLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(60, 13, true);
			this.SuggestedJobCategoriesLabel.TabIndex = 5;
			this.SuggestedJobCategoriesLabel.Visible = false;
			// 
			// ContactNameTextBox
			// 
			this.BindingSource.SetBindingMember(this.ContactNameTextBox, "FilteredContacts.OC_ContactName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgContact)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).FilteredContacts)).SyncRoot)).OC_ContactName)));
			this.ContactNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(75, 5, true);
			this.ContactNameTextBox.Name = "ContactNameTextBox";
			this.ContactNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(199, 20, true);
			this.ContactNameTextBox.TabIndex = 0;
			// 
			// JobTitleTextBox
			// 
			this.BindingSource.SetBindingMember(this.JobTitleTextBox, "FilteredContacts.OC_Title");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgContact)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).FilteredContacts)).SyncRoot)).OC_Title)));
			this.JobTitleTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(75, 28, true);
			this.JobTitleTextBox.Name = "JobTitleTextBox";
			this.JobTitleTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(199, 20, true);
			this.JobTitleTextBox.TabIndex = 1;
			// 
			// OC_JobCategoryDropEdit
			// 
			this.OC_JobCategoryDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OC_JobCategoryDropEdit, "FilteredContacts.JobCategoryDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgContact)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).FilteredContacts)).SyncRoot)).JobCategoryDescription)));
			this.OC_JobCategoryDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.OC_JobCategoryDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(75, 51, true);
			this.OC_JobCategoryDropEdit.Name = "OC_JobCategoryDropEdit";
			this.OC_JobCategoryDropEdit.PreBoundMaxLength = 30;
			this.OC_JobCategoryDropEdit.ShowDescriptionBox = false;
			this.OC_JobCategoryDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowDescription;
			this.OC_JobCategoryDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
			this.OC_JobCategoryDropEdit.TabIndex = 2;
			// 
			// ContactItemsListControl
			// 
			this.ContactItemsListControl.AllowDrop = true;
			this.ContactItemsListControl.AutoScroll = true;
			this.ContactItemsListControl.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ContactItemsListControl, "FilteredContacts");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.OrgContact)(((Enterprise.MasterFiles.Business.OrgContact)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).FilteredContacts)).SyncRoot)))));
			this.ContactItemsListControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 102, true);
			this.ContactItemsListControl.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, 0, 3, 3, true);
			this.ContactItemsListControl.Name = "ContactItemsListControl";
			this.ContactItemsListControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(342, 177, true);
			this.ContactItemsListControl.TabIndex = 4;
			// 
			// LanguageDropEdit
			// 
			this.LanguageDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.LanguageDropEdit, "FilteredContacts.OC_Language");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgContact)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).FilteredContacts)).SyncRoot)).OC_Language)));
			this.LanguageDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(75, 77, true);
			this.LanguageDropEdit.Name = "LanguageDropEdit";
			this.LanguageDropEdit.PreBoundMaxLength = 3;
			this.LanguageDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
			this.LanguageDropEdit.TabIndex = 3;
			// 
			// ContactInfoPanel
			// 
			this.ContactInfoPanel.Controls.Add(this.ContactSourceDropEdit);
			this.ContactInfoPanel.Controls.Add(this.DetailsVerifiedDropEdit);
			this.ContactInfoPanel.Controls.Add(this.JoinedIndustryYears);
			this.ContactInfoPanel.Controls.Add(this.JoinedCompanyYearsLabel);
			this.ContactInfoPanel.Controls.Add(this.JoinedIndustryDateEdit);
			this.ContactInfoPanel.Controls.Add(this.ExcelOpeningPasswordTextBox);
			this.ContactInfoPanel.Controls.Add(this.ViewExcelOpeningPasswordButton);
			this.ContactInfoPanel.Controls.Add(this.ExcelModifyingPasswordTextBox);
			this.ContactInfoPanel.Controls.Add(this.ViewExcelModifyingPasswordButton);
			this.ContactInfoPanel.Controls.Add(this.JoinedCompanyDateEdit);
			this.ContactInfoPanel.Controls.Add(this.AddressOverrideAddressControl);
			this.ContactInfoPanel.Controls.Add(this.OC_OH_AddressOverrideBoundGuidFindBox);
			this.ContactInfoPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(350, 17, true);
			this.ContactInfoPanel.Name = "ContactInfoPanel";
			this.ContactInfoPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(330, 250, true);
			this.ContactInfoPanel.TabIndex = 0;
			this.ContactInfoPanel.TabStop = false;
			// 
			// ContactSourceDropEdit
			// 
			this.ContactSourceDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ContactSourceDropEdit, "FilteredContacts.OC_ContactSource");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgContact)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).FilteredContacts)).SyncRoot)).OC_ContactSource)));
			this.ContactSourceDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ContactSourceDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(134, 53, true);
			this.ContactSourceDropEdit.Name = "ContactSourceDropEdit";
			this.ContactSourceDropEdit.PreBoundMaxLength = 20;
			this.ContactSourceDropEdit.ShowDescriptionBox = false;
			this.ContactSourceDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			this.ContactSourceDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(161, 20, true);
			this.ContactSourceDropEdit.TabIndex = 2;
			// 
			// DetailsVerifiedDropEdit
			// 
			this.DetailsVerifiedDropEdit.AllowDrop = true;
			this.DetailsVerifiedDropEdit.AutoCompleteMonthThreshold = 1;
			this.DetailsVerifiedDropEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.DetailsVerifiedDropEdit, "FilteredContacts.OC_DetailsVerified");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgContact)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).FilteredContacts)).SyncRoot)).OC_DetailsVerified)));
			this.DetailsVerifiedDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ContactsUserControl|aaa0bbdc-7ac7-45dd-b6ea-64efbdd17d82", "Verified");
			this.DetailsVerifiedDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(134, 77, true);
			this.DetailsVerifiedDropEdit.Name = "DetailsVerifiedDropEdit";
			this.DetailsVerifiedDropEdit.TabIndex = 3;
			// 
			// JoinedIndustryYears
			//
			this.BindingSource.SetBindingMember(this.JoinedIndustryYears, "FilteredContacts.YearsInIndustry");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgContact)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).FilteredContacts)).SyncRoot)).YearsInIndustry)));
			this.JoinedIndustryYears.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ContactsUserControl|b52a357d-5652-4f18-8c47-294362ac773e", "( X Years )");
			this.JoinedIndustryYears.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.JoinedIndustryYears.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(219, 125, true);
			this.JoinedIndustryYears.Name = "JoinedIndustryYears";
			this.JoinedIndustryYears.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(57, 18, true);
			this.JoinedIndustryYears.TabIndex = 6;
			// 
			// JoinedCompanyYearsLabel
			//
			this.BindingSource.SetBindingMember(this.JoinedCompanyYearsLabel, "FilteredContacts.YearsInCompany");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgContact)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).FilteredContacts)).SyncRoot)).YearsInCompany)));
			this.JoinedCompanyYearsLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ContactsUserControl|368ad975-243d-4c21-a4cf-7199f938950d", "( X Years )");
			this.JoinedCompanyYearsLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.JoinedCompanyYearsLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(219, 101, true);
			this.JoinedCompanyYearsLabel.Name = "JoinedCompanyYearsLabel";
			this.JoinedCompanyYearsLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(57, 19, true);
			this.JoinedCompanyYearsLabel.TabIndex = 4;
			// 
			// JoinedIndustryDateEdit
			// 
			this.JoinedIndustryDateEdit.AllowDrop = true;
			this.JoinedIndustryDateEdit.AutoCompleteMonthThreshold = 1;
			this.JoinedIndustryDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.JoinedIndustryDateEdit, "FilteredContacts.OC_YearJoinedIndustry");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgContact)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).FilteredContacts)).SyncRoot)).OC_YearJoinedIndustry)));
			this.JoinedIndustryDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(134, 125, true);
			this.JoinedIndustryDateEdit.Name = "JoinedIndustryDateEdit";
			this.JoinedIndustryDateEdit.TabIndex = 7;
			//
			// ExcelOpeningPasswordTextBox
			// 
			this.ExcelOpeningPasswordTextBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ExcelOpeningPasswordTextBox, "FilteredContacts.ExcelPasswordForOpening");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgContact)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).FilteredContacts)).SyncRoot)).ExcelPasswordForOpening)));
			this.ExcelOpeningPasswordTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ContactsUserControl|59E21799-1FF0-4867-AA4B-0E0D68C54AEB", "Excel Open Password");
			this.ExcelOpeningPasswordTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(134, 155, true);
			this.ExcelOpeningPasswordTextBox.Name = "ExcelOpenPasswordTextBox";
			this.ExcelOpeningPasswordTextBox.TabIndex = 8;
			this.ExcelOpeningPasswordTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 19, true);
			this.ExcelOpeningPasswordTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ExcelOpeningPasswordTextBox.PasswordChar = '*';
			//
			// View Excel Opening Password Button
			// 
			this.ViewExcelOpeningPasswordButton.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
			this.ViewExcelOpeningPasswordButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(240, 155, true);
			this.ViewExcelOpeningPasswordButton.Name = "ViewExcelOpeningPasswordButton";
			this.ViewExcelOpeningPasswordButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(60, 19, true);
			this.ViewExcelOpeningPasswordButton.TabIndex = 9;
			this.ViewExcelOpeningPasswordButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ContactsUserControl|67DECC62-9050-4AAF-BDD7-8E59018AD145", "View");
			this.ViewExcelOpeningPasswordButton.Click += new System.EventHandler(this.ViewExcelOpeningPassWordButton_Click);
			//
			// ExcelModifyPasswordTextBox
			// 
			this.ExcelModifyingPasswordTextBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ExcelModifyingPasswordTextBox, "FilteredContacts.ExcelPasswordForModifying");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgContact)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).FilteredContacts)).SyncRoot)).ExcelPasswordForModifying)));
			this.ExcelModifyingPasswordTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ContactsUserControl|F824E4FC-73BF-447D-B5EE-F7043CCC0FA7", "Excel Modify Password");
			this.ExcelModifyingPasswordTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(134, 185, true);
			this.ExcelModifyingPasswordTextBox.Name = "ExcelModifyPasswordTextBox";
			this.ExcelModifyingPasswordTextBox.TabIndex = 10;
			this.ExcelModifyingPasswordTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 19, true);
			this.ExcelModifyingPasswordTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ExcelModifyingPasswordTextBox.PasswordChar = '*';
			// 
			// View Excel Modifying Password Button
			// 
			this.ViewExcelModifyingPasswordButton.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
			this.ViewExcelModifyingPasswordButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(240, 185, true);
			this.ViewExcelModifyingPasswordButton.Name = "ViewExcelModifyingPasswordButton";
			this.ViewExcelModifyingPasswordButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(60, 19, true);
			this.ViewExcelModifyingPasswordButton.TabIndex = 11;
			this.ViewExcelModifyingPasswordButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ContactsUserControl|67DECC62-9050-4AAF-BDD7-8E59018AD145", "View");
			this.ViewExcelModifyingPasswordButton.Click += new System.EventHandler(this.ViewExcelModifyingPassWordButton_Click);
			//
			// JoinedCompanyDateEdit
			// 
			this.JoinedCompanyDateEdit.AllowDrop = true;
			this.JoinedCompanyDateEdit.AutoCompleteMonthThreshold = 1;
			this.JoinedCompanyDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.JoinedCompanyDateEdit, "FilteredContacts.OC_YearJoinedCompany");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgContact)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).FilteredContacts)).SyncRoot)).OC_YearJoinedCompany)));
			this.JoinedCompanyDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(134, 101, true);
			this.JoinedCompanyDateEdit.Name = "JoinedCompanyDateEdit";
			this.JoinedCompanyDateEdit.TabIndex = 4;
			// 
			// AddressOverrideAddressControl
			// 
			this.AddressOverrideAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AddressOverrideAddressControl, "FilteredContacts.WorkingAddressPK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OrgContact)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).FilteredContacts)).SyncRoot)).WorkingAddressPK)));
			this.AddressOverrideAddressControl.BindToOrgList = "FilteredContacts.OrganisationOverrides";
			this.AddressOverrideAddressControl.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ContactsUserControl|4cc63002-781a-439a-b738-cd10eebdecc6", "Working Address");
			this.AddressOverrideAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(134, 29, true);
			this.AddressOverrideAddressControl.Name = "AddressOverrideAddressControl";
			this.AddressOverrideAddressControl.PopupCaption = "";
			this.AddressOverrideAddressControl.ReadOnly = false;
			this.AddressOverrideAddressControl.ShowAddress = false;
			this.AddressOverrideAddressControl.ShowOrganisation = false;
			this.AddressOverrideAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(188, 20, true);
			this.AddressOverrideAddressControl.TabIndex = 1;
			// 
			// OC_OH_AddressOverrideBoundGuidFindBox
			// 
			this.OC_OH_AddressOverrideBoundGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OC_OH_AddressOverrideBoundGuidFindBox, "FilteredContacts.OC_OH_AddressOverride");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OrgContact)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).FilteredContacts)).SyncRoot)).OC_OH_AddressOverride)));
			this.OC_OH_AddressOverrideBoundGuidFindBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ContactsUserControl|77003343-4283-4e84-adfa-9ba4712b75f1", "Organization");
			this.OC_OH_AddressOverrideBoundGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(134, 5, true);
			this.OC_OH_AddressOverrideBoundGuidFindBox.Name = "OC_OH_AddressOverrideBoundGuidFindBox";
			this.OC_OH_AddressOverrideBoundGuidFindBox.PopupCaption = "Select Organisation Override";
			this.OC_OH_AddressOverrideBoundGuidFindBox.ShouldResize = true;
			this.OC_OH_AddressOverrideBoundGuidFindBox.ShowDescriptionBox = false;
			this.OC_OH_AddressOverrideBoundGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.OC_OH_AddressOverrideBoundGuidFindBox.TabIndex = 0;
			// 
			// SecurityTabPage
			// 
			this.SecurityTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ContactsUserControl|38832622-9a58-486d-a92b-99f98169466d", "Web Security");
			this.SecurityTabPage.Controls.Add(this.WebSecurityBottomPanel);
			this.SecurityTabPage.Controls.Add(this.WebSecurityTopPanel);
			this.SecurityTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.SecurityTabPage.Name = "SecurityTabPage";
			this.SecurityTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(594, 298, true);
			this.SecurityTabPage.TabIndex = 1;
			this.SecurityTabPage.VisibleChanged += SecurityTabPage_VisibleChanged;
			// 
			// WebSecurityBottomPanel
			// 
			this.WebSecurityBottomPanel.Controls.Add(this.WebSecurityLabel);
			this.WebSecurityBottomPanel.Controls.Add(this.WebSecuritySplitContainer);
			this.WebSecurityBottomPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.WebSecurityBottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 112, true);
			this.WebSecurityBottomPanel.Name = "WebSecurityBottomPanel";
			this.WebSecurityBottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(594, 186, true);
			this.WebSecurityBottomPanel.TabIndex = 24;
			// 
			// WebSecurityLabel
			// 
			this.WebSecurityLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.WebSecurityLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ContactsUserControl|5503b4a0-d0be-4d83-ad17-b8952cb9ddc5", "You can only specify security rights if you tick Web Access.");
			this.WebSecurityLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.WebSecurityLabel.IsFontBold = true;
			this.WebSecurityLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 67, true);
			this.WebSecurityLabel.Name = "WebSecurityLabel";
			this.WebSecurityLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(585, 24, true);
			this.WebSecurityLabel.TabIndex = 3;
			this.WebSecurityLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// WebSecuritySplitContainer
			// 
			this.WebSecuritySplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.WebSecuritySplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.WebSecuritySplitContainer.Name = "WebSecuritySplitContainer";
			// 
			// WebSecuritySplitContainer.Panel1
			// 
			this.WebSecuritySplitContainer.Panel1.Controls.Add(this.ContactSecurityGrid);
			// 
			// WebSecuritySplitContainer.Panel2
			// 
			this.WebSecuritySplitContainer.Panel2.Controls.Add(this.WebWarehouseSecurityGrid);
			this.WebSecuritySplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(594, 186, true);
			this.WebSecuritySplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(367);
			this.WebSecuritySplitContainer.TabIndex = 2;
			// 
			// ContactSecurityGrid
			// 
			this.ContactSecurityGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ContactSecurityGrid, "FilteredContacts.SecurityRightsView");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgContact)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).FilteredContacts)).SyncRoot)).SecurityRightsView)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgSecurityContacts)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgContact)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).FilteredContacts)).SyncRoot)).SecurityRightsView)).SyncRoot)).SecurityItemName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgSecurityContacts)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgContact)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).FilteredContacts)).SyncRoot)).SecurityRightsView)).SyncRoot)).OZ_Granted)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgSecurityContacts)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgContact)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).FilteredContacts)).SyncRoot)).SecurityRightsView)).SyncRoot)).HasDifferentSecurityToParent)));
			this.ContactSecurityGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("6573A672-D524-42F9-AE85-0C30A2AFC673", "Security Right");
			zTextBoxColumnStyleInfo3.ColumnName = "SecurityItemName";
			zTextBoxColumnStyleInfo3.IsMandatory = true;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(270);
			zCheckBoxColumnStyleInfo2.ColumnName = "OZ_Granted";
			zCheckBoxColumnStyleInfo2.IsMandatory = true;
			zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo7.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("47a8bf0b-f50d-4d8e-b2c7-b8f5bc5ea7da", "Overridden");
			zCheckBoxColumnStyleInfo7.ColumnName = "HasDifferentSecurityToParent";
			zCheckBoxColumnStyleInfo7.IsMandatory = true;
			zCheckBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.ContactSecurityGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.ContactSecurityGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.ContactSecurityGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo7);
			this.ContactSecurityGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ContactSecurityGrid.GridId = "e0c9ec58-db78-4343-bca2-01e4859e2258";
			this.ContactSecurityGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ContactSecurityGrid.LayoutKey = "ContactSecurityGrid";
			this.ContactSecurityGrid.LimitedColumns = null;
			this.ContactSecurityGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ContactSecurityGrid.Name = "ContactSecurityGrid";
			this.ContactSecurityGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.ContactSecurityGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(367, 186, true);
			this.ContactSecurityGrid.TabIndex = 2;
			// 
			// WebWarehouseSecurityGrid
			// 
			this.WebWarehouseSecurityGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.WebWarehouseSecurityGrid, "FilteredContacts.WebWarehouseEligibility");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgContact)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).FilteredContacts)).SyncRoot)).WebWarehouseEligibility)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgContactWebWarehouseEligibility)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgContact)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).FilteredContacts)).SyncRoot)).WebWarehouseEligibility)).SyncRoot)).WarehouseName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgContactWebWarehouseEligibility)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgContact)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).FilteredContacts)).SyncRoot)).WebWarehouseEligibility)).SyncRoot)).IsGranted)));
			this.WebWarehouseSecurityGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("B2F231DD-76A2-475E-B46F-659F560167E2", "Warehouse Name");
			zTextBoxColumnStyleInfo4.ColumnName = "WarehouseName";
			zTextBoxColumnStyleInfo4.IsMandatory = true;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(107);
			zCheckBoxColumnStyleInfo3.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("94dc9ae1-979c-4e46-bf9b-7359c59270df", "Granted");
			zCheckBoxColumnStyleInfo3.ColumnName = "IsGranted";
			zCheckBoxColumnStyleInfo3.IsMandatory = true;
			zCheckBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(47);
			this.WebWarehouseSecurityGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.WebWarehouseSecurityGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo3);
			this.WebWarehouseSecurityGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.WebWarehouseSecurityGrid.GridId = "e0c9ec58-db78-4343-aaa2-01e4859e2258";
			this.WebWarehouseSecurityGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.WebWarehouseSecurityGrid.LayoutKey = "WebWarehouseSecurityGrid";
			this.WebWarehouseSecurityGrid.LimitedColumns = null;
			this.WebWarehouseSecurityGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.WebWarehouseSecurityGrid.Name = "WebWarehouseSecurityGrid";
			this.WebWarehouseSecurityGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.WebWarehouseSecurityGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(223, 186, true);
			this.WebWarehouseSecurityGrid.TabIndex = 3;
			// 
			// WebSecurityTopPanel
			// 
			this.WebSecurityTopPanel.Controls.Add(this.LockoutDateTimeBoundDate);
			this.WebSecurityTopPanel.Controls.Add(this.UnlockButton);
			this.WebSecurityTopPanel.Controls.Add(this.SendPasswordInstructionsButton);
			this.WebSecurityTopPanel.Controls.Add(this.SendPasswordInstructionsLabel);
			this.WebSecurityTopPanel.Controls.Add(this.WebAccessCheckBox);
			this.WebSecurityTopPanel.Controls.Add(this.webSecurityLinkLabel);
			this.WebSecurityTopPanel.Controls.Add(this.SendPasswordInstructionToolStripPanel);
			this.WebSecurityTopPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.WebSecurityTopPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.WebSecurityTopPanel.Name = "WebSecurityTopPanel";
			this.WebSecurityTopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(594, 112, true);
			this.WebSecurityTopPanel.TabIndex = 0;
			// 
			// UnlockButton
			// 
			this.UnlockButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ContactsUserControl|b7522d6f-f5a3-49c7-928a-c811326a0855", "Unlock");
			this.UnlockButton.IsCaptionOverridden = false;
			this.UnlockButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(494, 82, true);
			this.UnlockButton.Name = "UnlockButton";
			this.UnlockButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.UnlockButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(94, 23, true);
			this.UnlockButton.TabIndex = 37;
			this.UnlockButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.UnlockButton.ToolTipCaption = null;
			this.UnlockButton.Click += new System.EventHandler(this.UnlockButton_Click);
			//
			// LockoutDateTimeBoundDate
			// 
			this.LockoutDateTimeBoundDate.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ContactsUserControl|129a4b7c-c91d-4b68-aed6-7e040bddea64", "Locked Out Until");
			this.LockoutDateTimeBoundDate.AllowDrop = true;
			this.LockoutDateTimeBoundDate.AutoCompleteMonthThreshold = 1;
			this.LockoutDateTimeBoundDate.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.LockoutDateTimeBoundDate, "FilteredContacts.LockoutDateTimeLocal");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgContact)(null)).Person.PER_LoginDisabledUntilUtc)));
			this.LockoutDateTimeBoundDate.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.LockoutDateTimeBoundDate.Enabled = false;
			this.LockoutDateTimeBoundDate.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(372, 82, true);
			this.LockoutDateTimeBoundDate.Name = "LockoutDateTimeBoundDate";
			this.LockoutDateTimeBoundDate.TabIndex = 38;
			// 
			// SendPasswordInstructions
			// 
			this.SendPasswordInstructionsButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ContactsUserControl|2bb1c0e1-a969-419c-84dc-59002b60cb82", "Send Password Instructions");
			this.SendPasswordInstructionsButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(15, 82, true);
			this.SendPasswordInstructionsButton.Name = "SendPasswordInstructions";
			this.SendPasswordInstructionsButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.SendPasswordInstructionsButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(180, 24, true);
			this.SendPasswordInstructionsButton.TabIndex = 5;
			this.SendPasswordInstructionsButton.ToolTipCaption = null;
			this.SendPasswordInstructionsButton.Click += new System.EventHandler(this.SendPasswordInstructionsButton_Click);
			// 
			// SendPasswordInstructionsLabel
			// 
			this.SendPasswordInstructionsLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.SendPasswordInstructionsLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(200, 82, true);
			this.SendPasswordInstructionsLabel.Name = "SendPasswordInstructionsLabel";
			this.SendPasswordInstructionsLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(180, 24, true);
			this.SendPasswordInstructionsLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.SendPasswordInstructionsLabel.TabIndex = 0;
			// 
			// SendPasswordInstructionToolStripPanel
			// 
			SendPasswordInstructionToolStripPanel.Name = "SendPasswordInstructionToolStripPanel";
			SendPasswordInstructionToolStripPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(15, 82, true);
			SendPasswordInstructionToolStripPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(180, 24, true);
			SendPasswordInstructionToolStripPanel.Controls.Add(SendPasswordInstructionStrip);
			// 
			// SendPasswordInstructionStrip
			// 
			SendPasswordInstructionStrip.Items.Add(SendPasswordInstructionToolStripSplitButton);
			// 
			// SendPasswordInstructionToolStripSplitButton
			// 
			SendPasswordInstructionToolStripSplitButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ContactsUserControl|8f403d1a-e571-4cb5-8af7-06838580d446", "Send Password Instructions");
			SendPasswordInstructionToolStripSplitButton.DisplayStyle = ToolStripItemDisplayStyle.Text;
			SendPasswordInstructionToolStripSplitButton.DropDownItems.AddRange(new ToolStripItem[]
			{
				UseWebTrackerStripButton,
				UseCargoWiseWebPortalsStripButton
			});
			//
			// UseWebTrackerStripButton
			//
			UseWebTrackerStripButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ContactsUserControl|441d302d-a21d-4a74-9e2e-73411276038e", "Use WebTracker");
			UseWebTrackerStripButton.Click += SendPasswordInstructionsButton_Click;
			// 
			// UseCargoWiseWebPortalsStripButton
			// 
			UseCargoWiseWebPortalsStripButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ContactsUserControl|bb582e6b-6dcd-4261-9a5d-e1ada7191e06", "Use CargoWise Web Portals");
			UseCargoWiseWebPortalsStripButton.Width = SendPasswordInstructionToolStripSplitButton.Width;
			UseCargoWiseWebPortalsStripButton.Click += UseCargoWiseWebPortalsStripButton_Click;
			// 
			// WebAccessCheckBox
			// 
			this.WebAccessCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.WebAccessCheckBox, "FilteredContacts.OC_WebAccessEnabled");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgContact)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).FilteredContacts)).SyncRoot)).OC_WebAccessEnabled)));
			this.WebAccessCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.WebAccessCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 56, true);
			this.WebAccessCheckBox.Name = "WebAccessCheckBox";
			this.WebAccessCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.WebAccessCheckBox.TabIndex = 1;
			this.WebAccessCheckBox.CheckedChanged += new System.EventHandler(this.WebAccessCheckBox_CheckedChanged);
			// 
			// zLabel1
			//
			this.webSecurityLinkLabel.Font = Enterprise.ZArchitecture.Core.OFont.GetFont();
			this.webSecurityLinkLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 8, true);
			this.webSecurityLinkLabel.Name = "webSecurityLinkLabel";
			this.webSecurityLinkLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(580, 45, true);
			this.webSecurityLinkLabel.TabIndex = 0;
			this.webSecurityLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.GlowPortalLabel_LinkClicked);
			// 
			// zCampaignsTabPage
			// 
			this.zCampaignsTabPage.BackColor = System.Drawing.SystemColors.Control;
			this.zCampaignsTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("9e06a0b0-be59-4e70-834b-20055422ad81", "Campaigns");
			this.zCampaignsTabPage.Controls.Add(this.CampaignsTabControl);
			this.zCampaignsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.zCampaignsTabPage.Name = "zCampaignsTabPage";
			this.zCampaignsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.zCampaignsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(594, 298, true);
			this.zCampaignsTabPage.TabIndex = 2;
			// 
			// CampaignsTabControl
			// 
			this.CampaignsTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.CampaignsTabControl.Controls.Add(this.CampaignTrackingTabPage);
			this.CampaignsTabControl.Controls.Add(this.CampaignSubscriptionsTabPage);
			this.CampaignsTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CampaignsTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.CampaignsTabControl.Name = "CampaignsTabControl";
			this.CampaignsTabControl.SelectedIndex = 0;
			this.CampaignsTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(588, 292, true);
			this.CampaignsTabControl.TabIndex = 0;
			// 
			// CampaignTrackingTabPage
			// 
			this.CampaignTrackingTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ContactsUserControl|c7d1ecae-3826-4925-aafc-b074b24186d6", "Campaign Tracking");
			this.CampaignTrackingTabPage.Controls.Add(this.CampaignTrackingTopPanel);
			this.CampaignTrackingTabPage.Controls.Add(this.CampaignTrackingBottomPanel);
			this.CampaignTrackingTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.CampaignTrackingTabPage.Name = "CampaignTrackingTabPage";
			this.CampaignTrackingTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(580, 265, true);
			this.CampaignTrackingTabPage.TabIndex = 0;
			// 
			// CampaignTrackingTopPanel
			// 
			this.CampaignTrackingTopPanel.Controls.Add(this.CampaignsGrid);
			this.CampaignTrackingTopPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CampaignTrackingTopPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CampaignTrackingTopPanel.Name = "CampaignTrackingTopPanel";
			this.CampaignTrackingTopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(580, 198, true);
			this.CampaignTrackingTopPanel.TabIndex = 22;
			// 
			// CampaignsGrid
			// 
			this.CampaignsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.CampaignsGrid, "FilteredContacts.Campaigns");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgContact)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).FilteredContacts)).SyncRoot)).Campaigns)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.MasterFiles.Integration.IGlbCompanyCampaignItem)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgContact)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).FilteredContacts)).SyncRoot)).Campaigns)).SyncRoot)).G8_LastSentTimeUtc)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Integration.IGlbCompanyCampaignItem)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgContact)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).FilteredContacts)).SyncRoot)).Campaigns)).SyncRoot)).CampaignName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Integration.IGlbCompanyCampaignItem)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgContact)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).FilteredContacts)).SyncRoot)).Campaigns)).SyncRoot)).G8_DeliveryMethod)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Integration.IGlbCompanyCampaignItem)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgContact)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).FilteredContacts)).SyncRoot)).Campaigns)).SyncRoot)).G8_SystemCreateUser)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Integration.IGlbCompanyCampaignItem)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgContact)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).FilteredContacts)).SyncRoot)).Campaigns)).SyncRoot)).TrackingStatusDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Integration.IGlbCompanyCampaignItem)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgContact)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).FilteredContacts)).SyncRoot)).Campaigns)).SyncRoot)).CampaignID)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.MasterFiles.Integration.IGlbCompanyCampaignItem)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgContact)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).FilteredContacts)).SyncRoot)).Campaigns)).SyncRoot)).G8_FollowedUp)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Integration.IGlbCompanyCampaignItem)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgContact)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).FilteredContacts)).SyncRoot)).Campaigns)).SyncRoot)).G8_GS_NKFollowedUpBy)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.MasterFiles.Integration.IGlbCompanyCampaignItem)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgContact)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).FilteredContacts)).SyncRoot)).Campaigns)).SyncRoot)).LastCommunicationDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Integration.IGlbCompanyCampaignItem)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgContact)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).FilteredContacts)).SyncRoot)).Campaigns)).SyncRoot)).LastCommunicationStaffCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Integration.IGlbCompanyCampaignItem)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgContact)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).FilteredContacts)).SyncRoot)).Campaigns)).SyncRoot)).IsUnsubscribed)));
			this.CampaignsGrid.CaptionVisible = false;
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ContactsUserControl|ff6ac3d4-80e3-4ec6-9d89-ac5c0ca9e1b7", "Last Sent Time");
			zDateEditColumnStyleInfo1.ColumnName = "G8_LastSentTimeUtc";
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ContactsUserControl|a6acb710-23a2-460c-ae7e-7ed9a0d8bb5c", "Campaign Name");
			zTextBoxColumnStyleInfo5.ColumnName = "CampaignName";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ContactsUserControl|0c20a1e2-ef0a-4e87-a7e2-cf419ae83186", "Method");
			zTextBoxColumnStyleInfo6.ColumnName = "G8_DeliveryMethod";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo7.ColumnName = "G8_SystemCreateUser";
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo8.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ContactsUserControl|849dec5e-4110-4e48-a074-a39f1a6cfde6", "Delivery Status");
			zTextBoxColumnStyleInfo8.ColumnName = "TrackingStatusDescription";
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo9.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ContactsUserControl|668183e1-c582-457b-9dc9-3520c0c6f0c5", "ID");
			zTextBoxColumnStyleInfo9.ColumnName = "CampaignID";
			zTextBoxColumnStyleInfo9.IsVisible = false;
			zTextBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo8.ColumnName = "G8_FollowedUp";
			zDateEditColumnStyleInfo8.IsVisible = false;
			zDateEditColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo10.ColumnName = "G8_GS_NKFollowedUpBy";
			zTextBoxColumnStyleInfo10.IsVisible = false;
			zTextBoxColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zDateEditColumnStyleInfo9.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ContactsUserControl|7346D792-6C13-4C4F-9E5B-CBF1D3AEA5C2", "Comm Date", "Comm Date", "Last Communication Date", "The last communication date");
			zDateEditColumnStyleInfo9.ColumnName = "LastCommunicationDate";
			zDateEditColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo11.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ContactsUserControl|9B5340A1-6A59-470B-B0B3-170D8862A3D7", "Comm Staff", "Comm Staff", "Last Communication Staff", "The last communication staff");
			zTextBoxColumnStyleInfo11.ColumnName = "LastCommunicationStaffCode";
			zTextBoxColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zCheckBoxColumnStyleInfo4.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ContactsUserControl|ed74c8ec-efbe-4e93-b3ee-304a43d05a4b", "Unsubscribed", "Unsubscribed from this campaign", "");
			zCheckBoxColumnStyleInfo4.ColumnName = "IsUnsubscribed";
			zCheckBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.CampaignsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.CampaignsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.CampaignsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.CampaignsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.CampaignsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.CampaignsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.CampaignsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo8);
			this.CampaignsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.CampaignsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo9);
			this.CampaignsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo11);
			this.CampaignsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo4);
			this.CampaignsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CampaignsGrid.GridId = "47436305-a75e-48e8-b0cc-433a5e5d3fa2";
			this.CampaignsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.CampaignsGrid.LayoutKey = "ContactSecurityGrid";
			this.CampaignsGrid.LimitedColumns = null;
			this.CampaignsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CampaignsGrid.Name = "CampaignsGrid";
			this.CampaignsGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.CampaignsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(580, 198, true);
			this.CampaignsGrid.TabIndex = 0;
			this.CampaignsGrid.DoubleClick += new System.EventHandler(this.CampaignsGrid_DoubleClick);
			// 
			// CampaignTrackingBottomPanel
			// 
			this.CampaignTrackingBottomPanel.Controls.Add(this.ResitExamButton);
			this.CampaignTrackingBottomPanel.Controls.Add(this.ViewExamHistoryButton);
			this.CampaignTrackingBottomPanel.Controls.Add(this.ViewExamButton);
			this.CampaignTrackingBottomPanel.Controls.Add(this.ResendCampaignButton);
			this.CampaignTrackingBottomPanel.Controls.Add(this.SendCampaignButton);
			this.CampaignTrackingBottomPanel.Controls.Add(this.ViewCampaignDocumentButton);
			this.CampaignTrackingBottomPanel.Controls.Add(this.ViewCampaignButton);
			this.CampaignTrackingBottomPanel.Controls.Add(this.EditCampaignButton);
			this.CampaignTrackingBottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.CampaignTrackingBottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 198, true);
			this.CampaignTrackingBottomPanel.Name = "CampaignTrackingBottomPanel";
			this.CampaignTrackingBottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(580, 67, true);
			this.CampaignTrackingBottomPanel.TabIndex = 0;
			// 
			// ResitExamButton
			// 
			this.ResitExamButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.ResitExamButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("84b2d8bb-ab4c-482f-aced-77d3d671e399", "Resit Exam");
			this.ResitExamButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(470, 8, true);
			this.ResitExamButton.Name = "ResitExamButton";
			this.ResitExamButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.ResitExamButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 23, true);
			this.ResitExamButton.TabIndex = 8;
			this.ResitExamButton.ToolTipCaption = null;
			this.ResitExamButton.Visible = false;
			this.ResitExamButton.Click += new System.EventHandler(this.ResitExamButton_Click);
			// 
			// ViewExamHistoryButton
			// 
			this.ViewExamHistoryButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.ViewExamHistoryButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("f02d31b1-e672-46bd-a470-e514aa739411", "View Exam Results");
			this.ViewExamHistoryButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(240, 8, true);
			this.ViewExamHistoryButton.Name = "ViewExamHistoryButton";
			this.ViewExamHistoryButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.ViewExamHistoryButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(110, 23, true);
			this.ViewExamHistoryButton.TabIndex = 6;
			this.ViewExamHistoryButton.ToolTipCaption = null;
			this.ViewExamHistoryButton.Visible = false;
			this.ViewExamHistoryButton.Click += new System.EventHandler(this.ViewExamHistoryButton_Click);
			// 
			// ViewExamButton
			// 
			this.ViewExamButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.ViewExamButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("eea93bde-c7e6-430d-8dcb-e539e5f120a3", "Open Exam");
			this.ViewExamButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(360, 8, true);
			this.ViewExamButton.Name = "ViewExamButton";
			this.ViewExamButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.ViewExamButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 23, true);
			this.ViewExamButton.TabIndex = 7;
			this.ViewExamButton.ToolTipCaption = null;
			this.ViewExamButton.Visible = false;
			this.ViewExamButton.Click += new System.EventHandler(this.ViewExamButton_Click);
			// 
			// ResendCampaignButton
			// 
			this.ResendCampaignButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.ResendCampaignButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ContactsUserControl|20f8c264-3dec-4fee-bfc5-74b6f6a165ca", "Resend", "Resend Campaign", "");
			this.ResendCampaignButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(188, 8, true);
			this.ResendCampaignButton.Name = "ResendCampaignButton";
			this.ResendCampaignButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.ResendCampaignButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(129, 23, true);
			this.ResendCampaignButton.TabIndex = 1;
			this.ResendCampaignButton.ToolTipCaption = null;
			this.ResendCampaignButton.Click += new System.EventHandler(this.ResendCampaignButton_Click);
			// 
			// SendCampaignButton
			// 
			this.SendCampaignButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.SendCampaignButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ContactsUserControl|2d83bee7-3865-4ee9-b843-7eecd8900b9e", "Send Campaign");
			this.SendCampaignButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(324, 8, true);
			this.SendCampaignButton.Name = "SendCampaignButton";
			this.SendCampaignButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.SendCampaignButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(121, 23, true);
			this.SendCampaignButton.TabIndex = 2;
			this.SendCampaignButton.ToolTipCaption = null;
			this.SendCampaignButton.Click += new System.EventHandler(this.SendCampaignButton_Click);
			// 
			// ViewCampaignDocumentButton
			// 
			this.ViewCampaignDocumentButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.ViewCampaignDocumentButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ContactsUserControl|ec095614-6b87-4a97-b43c-73dac432d6d8", "Campaign Document");
			this.ViewCampaignDocumentButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(438, 37, true);
			this.ViewCampaignDocumentButton.Name = "ViewCampaignDocumentButton";
			this.ViewCampaignDocumentButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.ViewCampaignDocumentButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(134, 23, true);
			this.ViewCampaignDocumentButton.TabIndex = 5;
			this.ViewCampaignDocumentButton.ToolTipCaption = null;
			this.ViewCampaignDocumentButton.Click += new System.EventHandler(this.ViewCampaignDocumentButton_Click);
			// 
			// ViewCampaignButton
			// 
			this.ViewCampaignButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.ViewCampaignButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ContactsUserControl|15b492b4-60d3-49e6-8fcf-010bdc141a57", "View Campaign");
			this.ViewCampaignButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(452, 8, true);
			this.ViewCampaignButton.Name = "ViewCampaignButton";
			this.ViewCampaignButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.ViewCampaignButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(119, 23, true);
			this.ViewCampaignButton.TabIndex = 3;
			this.ViewCampaignButton.ToolTipCaption = null;
			this.ViewCampaignButton.Click += new System.EventHandler(this.ViewCampaignButton_Click);
			// 
			// EditCampaignButton
			// 
			this.EditCampaignButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.EditCampaignButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ContactsUserControl|b81cb7dc-6de5-4cc3-9cca-b6a0f6347d95", "Campaign Tracking");
			this.EditCampaignButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(302, 37, true);
			this.EditCampaignButton.Name = "EditCampaignButton";
			this.EditCampaignButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.EditCampaignButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(130, 23, true);
			this.EditCampaignButton.TabIndex = 4;
			this.EditCampaignButton.ToolTipCaption = null;
			this.EditCampaignButton.Click += new System.EventHandler(this.EditCampaignButton_Click);
			// 
			// CampaignSubscriptionsTabPage
			// 
			this.CampaignSubscriptionsTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ContactsUserControl|78baeaa8-a6cb-4203-811c-f7ddd36a7ee4", "Subscription List");
			this.CampaignSubscriptionsTabPage.Controls.Add(this.SubscriptionsGrid);
			this.CampaignSubscriptionsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.CampaignSubscriptionsTabPage.Name = "CampaignSubscriptionsTabPage";
			this.CampaignSubscriptionsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(580, 265, true);
			this.CampaignSubscriptionsTabPage.TabIndex = 1;
			// 
			// SubscriptionsGrid
			// 
			this.SubscriptionsGrid.AllowNavigation = false;
			this.SubscriptionsGrid.AllowReadOnlyRowsToBeDeleted = true;
			this.BindingSource.SetBindingMember(this.SubscriptionsGrid, "FilteredContacts.Subscriptions");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgContact)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).FilteredContacts)).SyncRoot)).Subscriptions)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Integration.IGlbCompanyCampaignSubscription)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgContact)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).FilteredContacts)).SyncRoot)).Subscriptions)).SyncRoot)).GCS_IsSubscribed)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Integration.IGlbCompanyCampaignSubscription)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgContact)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).FilteredContacts)).SyncRoot)).Subscriptions)).SyncRoot)).GCS_MediaCategoryWithAll)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Integration.IGlbCompanyCampaignSubscription)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgContact)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).FilteredContacts)).SyncRoot)).Subscriptions)).SyncRoot)).GCS_MediaTypeWithAll)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Integration.IGlbCompanyCampaignSubscription)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgContact)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).FilteredContacts)).SyncRoot)).Subscriptions)).SyncRoot)).GCS_IsOrgLevel)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Integration.IGlbCompanyCampaignSubscription)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgContact)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).FilteredContacts)).SyncRoot)).Subscriptions)).SyncRoot)).GCS_G0)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.MasterFiles.Integration.IGlbCompanyCampaignSubscription)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgContact)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).FilteredContacts)).SyncRoot)).Subscriptions)).SyncRoot)).GCS_SystemCreateTimeUtc)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Integration.IGlbCompanyCampaignSubscription)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgContact)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).FilteredContacts)).SyncRoot)).Subscriptions)).SyncRoot)).GCS_SystemCreateUser)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.MasterFiles.Integration.IGlbCompanyCampaignSubscription)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgContact)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).FilteredContacts)).SyncRoot)).Subscriptions)).SyncRoot)).GCS_SystemLastEditTimeUtc)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Integration.IGlbCompanyCampaignSubscription)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgContact)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).FilteredContacts)).SyncRoot)).Subscriptions)).SyncRoot)).GCS_SystemLastEditUser)));
			this.SubscriptionsGrid.CaptionVisible = false;
			zCheckBoxColumnStyleInfo5.ColumnName = "GCS_IsSubscribed";
			zCheckBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(33);
			zDropEditColumnStyleInfo5.ColumnName = "GCS_MediaCategoryWithAll";
			zDropEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDropEditColumnStyleInfo6.ColumnName = "GCS_MediaTypeWithAll";
			zDropEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCheckBoxColumnStyleInfo6.ColumnName = "GCS_IsOrgLevel";
			zCheckBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(33);
			zGuidFindBoxColumnStyleInfo1.ColumnName = "GCS_G0";
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDateEditColumnStyleInfo2.ColumnName = "GCS_SystemCreateTimeUtc";
			zDateEditColumnStyleInfo2.IsReadOnly = true;
			zDateEditColumnStyleInfo2.IsVisible = false;
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo12.ColumnName = "GCS_SystemCreateUser";
			zTextBoxColumnStyleInfo12.IsReadOnly = true;
			zTextBoxColumnStyleInfo12.IsVisible = false;
			zTextBoxColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo3.ColumnName = "GCS_SystemLastEditTimeUtc";
			zDateEditColumnStyleInfo3.IsReadOnly = true;
			zDateEditColumnStyleInfo3.IsVisible = false;
			zDateEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo13.ColumnName = "GCS_SystemLastEditUser";
			zTextBoxColumnStyleInfo13.IsReadOnly = true;
			zTextBoxColumnStyleInfo13.IsVisible = false;
			zTextBoxColumnStyleInfo13.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.SubscriptionsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo5);
			this.SubscriptionsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo5);
			this.SubscriptionsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo6);
			this.SubscriptionsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo6);
			this.SubscriptionsGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.SubscriptionsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.SubscriptionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo12);
			this.SubscriptionsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo3);
			this.SubscriptionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo13);
			this.SubscriptionsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SubscriptionsGrid.GridId = "47436305-a75e-48e8-b0cc-433a5e5d3fa2";
			this.SubscriptionsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.SubscriptionsGrid.LayoutKey = "SubscriptionsGrid";
			this.SubscriptionsGrid.LimitedColumns = null;
			this.SubscriptionsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SubscriptionsGrid.Name = "SubscriptionsGrid";
			this.SubscriptionsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(580, 265, true);
			this.SubscriptionsGrid.TabIndex = 0;
			// 
			// CertificatesTabPage
			// 
			this.CertificatesTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("dd9220be-845c-48e8-a3e5-95587185adce", "Certificates and ID Numbers");
			this.CertificatesTabPage.Controls.Add(this.CertificatesUserControl);
			this.CertificatesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.CertificatesTabPage.Name = "CertificatesTabPage";
			this.CertificatesTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.CertificatesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(594, 298, true);
			this.CertificatesTabPage.TabIndex = 3;
			// 
			// CertificatesUserControl
			// 
			this.CertificatesUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CertificatesUserControl, "FilteredContacts");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.ICertificatesProvider)(((Enterprise.MasterFiles.Business.OrgContact)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).FilteredContacts)).SyncRoot)))));
			this.CertificatesUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CertificatesUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.CertificatesUserControl.Name = "CertificatesUserControl";
			this.CertificatesUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(588, 292, true);
			this.CertificatesUserControl.TabIndex = 0;
			// 
			// AllocatedContactTabPage
			// 
			this.AllocatedContactTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ContactsUserControl|7B9A3B55-54B0-4FF9-9E6B-898E803FA7B2", "Allocated Contact");
			this.AllocatedContactTabPage.Controls.Add(this.AllocatedContactPanel);
			this.AllocatedContactTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.AllocatedContactTabPage.Name = "AllocatedContactTabPage";
			this.AllocatedContactTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.AllocatedContactTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(594, 298, true);
			this.AllocatedContactTabPage.TabIndex = 4;
			// 
			// AllocatedContactPanel
			// 
			this.AllocatedContactPanel.Controls.Add(this.AllocatedContactGrid);
			this.AllocatedContactPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AllocatedContactPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.AllocatedContactPanel.Name = "AllocatedContactPanel";
			this.AllocatedContactPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(588, 292, true);
			this.AllocatedContactPanel.TabIndex = 24;
			// 
			// AllocatedContactGrid
			// 
			this.AllocatedContactGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.AllocatedContactGrid, "FilteredContacts.Allocations");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgContact)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).FilteredContacts)).SyncRoot)).Allocations)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgContactAllocation)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgContact)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).FilteredContacts)).SyncRoot)).Allocations)).SyncRoot)).PC_Type)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgContactAllocation)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgContact)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).FilteredContacts)).SyncRoot)).Allocations)).SyncRoot)).AllocationDescription)));
			this.AllocatedContactGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo7.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ContactsUserControl|6D658E8C-6532-4367-9145-C418458A7519", "Allocation");
			zDropEditColumnStyleInfo7.ColumnName = "PC_Type";
			zDropEditColumnStyleInfo7.ToolTip = "Select the external organization for which this contact is this organizations con" +
	"tact point.";
			zDropEditColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo14.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ContactsUserControl|4A87C3F6-E80B-432A-8739-5FCD5337D290", "Allocation Description");
			zTextBoxColumnStyleInfo14.ColumnName = "AllocationDescription";
			zTextBoxColumnStyleInfo14.ToolTip = "Shows the description of the External Organization to which this contact is alloc" +
	"ated.";
			zTextBoxColumnStyleInfo14.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(145);
			this.AllocatedContactGrid.ColumnStyles.Add(zDropEditColumnStyleInfo7);
			this.AllocatedContactGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo14);
			this.AllocatedContactGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AllocatedContactGrid.GridId = "EE3EC6A8-13E9-4C42-AB0A-5BD0DA701DA7";
			this.AllocatedContactGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.AllocatedContactGrid.LayoutKey = "AllocatedContactGrid";
			this.AllocatedContactGrid.LimitedColumns = null;
			this.AllocatedContactGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.AllocatedContactGrid.Name = "AllocatedContactGrid";
			this.AllocatedContactGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(588, 292, true);
			this.AllocatedContactGrid.TabIndex = 0;
			// 
			// ContactsTopSplitContainer
			// 
			this.ContactsTopSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ContactsTopSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ContactsTopSplitContainer.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(872, 255, true);
			this.ContactsTopSplitContainer.Name = "ContactsTopSplitContainer";
			// 
			// ContactsTopSplitContainer.Panel1
			// 
			this.ContactsTopSplitContainer.Panel1.Controls.Add(this.OrgContactPanel);
			this.ContactsTopSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(975, 325, true);
			this.ContactsTopSplitContainer.Panel1MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(218);
			// 
			// ContactsTopSplitContainer.Panel2
			// 
			this.ContactsTopSplitContainer.Panel2.Controls.Add(this.ContactDetailsPanel);
			this.ContactsTopSplitContainer.Panel2MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(610);
			this.ContactsTopSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(364);
			this.ContactsTopSplitContainer.TabIndex = 14;
			// 
			// ContactsBottomSplitContainer
			// 
			this.ContactsBottomSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ContactsBottomSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ContactsBottomSplitContainer.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(872, 215, true);
			this.ContactsBottomSplitContainer.Name = "ContactsBottomSplitContainer";
			// 
			// ContactsBottomSplitContainer.Panel1
			// 
			this.ContactsBottomSplitContainer.Panel1.AutoScroll = true;
			this.ContactsBottomSplitContainer.Panel1.Controls.Add(this.PersonalInfoPanel);
			this.ContactsBottomSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(975, 276, true);
			this.ContactsBottomSplitContainer.Panel1MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(169);
			// 
			// ContactsBottomSplitContainer.Panel2
			// 
			this.ContactsBottomSplitContainer.Panel2.Controls.Add(this.DocDeliveryPanel);
			this.ContactsBottomSplitContainer.Panel2MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(376);
			this.ContactsBottomSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(338);
			this.ContactsBottomSplitContainer.TabIndex = 15;
			// 
			// ContactsSplitContainer
			// 
			this.ContactsSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ContactsSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 24, true);
			this.ContactsSplitContainer.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(872, 480, true);
			this.ContactsSplitContainer.Name = "ContactsSplitContainer";
			this.ContactsSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// ContactsSplitContainer.Panel1
			// 
			this.ContactsSplitContainer.Panel1.Controls.Add(this.ContactsTopSplitContainer);
			this.ContactsSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(975, 605, true);
			this.ContactsSplitContainer.Panel1MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(173);
			// 
			// ContactsSplitContainer.Panel2
			// 
			this.ContactsSplitContainer.Panel2.Controls.Add(this.ContactsBottomSplitContainer);
			this.ContactsSplitContainer.Panel2MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(143);
			this.ContactsSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(325);
			this.ContactsSplitContainer.TabIndex = 16;
			// 
			// ContactsUserControl
			// 
			this.Controls.Add(this.ContactsSplitContainer);
			this.IsModifyContact = true;
			this.IsModifyContactContactDetails = true;
			this.IsModifyContactDocDeliveryDetails = true;
			this.IsModifyContactPersonalInformation = true;
			this.Name = "ContactsUserControl";
			this.ShouldSerializeTabPageMethods = false;
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(975, 629, true);
			this.Load += new System.EventHandler(this.ContactsUserControl_Load);
			this.Controls.SetChildIndex(this.SecurityPanel, 0);
			this.Controls.SetChildIndex(this.ContactsSplitContainer, 0);
			this.SecurityPanel.ResumeLayout(false);
			this.SecurityPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ConPersonalInfoGroupBox.ResumeLayout(false);
			this.ConPersonalInfoGroupBox.PerformLayout();
			this.PersonalInfoTabControl.ResumeLayout(false);
			this.PersonalInfoTabControl.PerformLayout();
			this.PersonalDetailsTabPage.ResumeLayout(false);
			this.PersonalDetailsTabPage.PerformLayout();
			this.NationalityCodeFindBox.ResumeLayout(true);
			this.NationalityCodeFindBox.PerformLayout();
			this.GenderDropEdit.ResumeLayout(true);
			this.GenderDropEdit.PerformLayout();
			this.OC_BirthdayBoundDateEdit.ResumeLayout(true);
			this.OC_BirthdayBoundDateEdit.PerformLayout();
			this.AttributesTabPage.ResumeLayout(false);
			this.AttributesTabPage.PerformLayout();
			this.AttributesTopPanel.ResumeLayout(false);
			this.AttributesTopPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.AttributesGrid)).EndInit();
			this.AttributesGrid.ResumeLayout(false);
			this.AttributesGrid.PerformLayout();
			this.AttributesBottomPanel.ResumeLayout(false);
			this.AttributesBottomPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.OrgContactBoundGrid)).EndInit();
			this.OrgContactBoundGrid.ResumeLayout(false);
			this.OrgContactBoundGrid.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.OrgDocumentBoundGrid)).EndInit();
			this.OrgDocumentBoundGrid.ResumeLayout(false);
			this.OrgDocumentBoundGrid.PerformLayout();
			this.DocDeliveryGroupBox.ResumeLayout(false);
			this.DocDeliveryGroupBox.PerformLayout();
			this.OC_AttachmentTypeDropEdit.ResumeLayout(true);
			this.OC_AttachmentTypeDropEdit.PerformLayout();
			this.OC_NotifyModeDropEdit.ResumeLayout(true);
			this.OC_NotifyModeDropEdit.PerformLayout();
			this.DocDeliveryPanel.ResumeLayout(false);
			this.DocDeliveryPanel.PerformLayout();
			this.PersonalInfoPanel.ResumeLayout(false);
			this.PersonalInfoPanel.PerformLayout();
			this.OrgContactPanel.ResumeLayout(false);
			this.OrgContactPanel.PerformLayout();
			this.ContactDetailsPanel.ResumeLayout(false);
			this.ContactDetailsPanel.PerformLayout();
			this.ContactDetailsTabControl.ResumeLayout(false);
			this.ContactDetailsTabControl.PerformLayout();
			this.DetailsTabPage.ResumeLayout(false);
			this.DetailsTabPage.PerformLayout();
			this.ContactItemsGroupBox.ResumeLayout(false);
			this.ContactItemsGroupBox.PerformLayout();
			this.zPanelContactDetails.ResumeLayout(false);
			this.zPanelContactDetails.PerformLayout();
			this.OC_JobCategoryDropEdit.ResumeLayout(true);
			this.OC_JobCategoryDropEdit.PerformLayout();
			this.ContactItemsListControl.ResumeLayout(true);
			this.ContactItemsListControl.PerformLayout();
			this.LanguageDropEdit.ResumeLayout(true);
			this.LanguageDropEdit.PerformLayout();
			this.ContactInfoPanel.ResumeLayout(false);
			this.ContactInfoPanel.PerformLayout();
			this.ContactSourceDropEdit.ResumeLayout(true);
			this.ContactSourceDropEdit.PerformLayout();
			this.DetailsVerifiedDropEdit.ResumeLayout(true);
			this.DetailsVerifiedDropEdit.PerformLayout();
			this.JoinedIndustryDateEdit.ResumeLayout(true);
			this.JoinedIndustryDateEdit.PerformLayout();
			this.ExcelOpeningPasswordTextBox.ResumeLayout(true);
			this.ExcelOpeningPasswordTextBox.PerformLayout();
			this.ExcelModifyingPasswordTextBox.ResumeLayout(true);
			this.ExcelModifyingPasswordTextBox.PerformLayout();
			this.JoinedCompanyDateEdit.ResumeLayout(true);
			this.JoinedCompanyDateEdit.PerformLayout();
			this.AddressOverrideAddressControl.ResumeLayout(true);
			this.AddressOverrideAddressControl.PerformLayout();
			this.OC_OH_AddressOverrideBoundGuidFindBox.ResumeLayout(true);
			this.OC_OH_AddressOverrideBoundGuidFindBox.PerformLayout();
			this.SecurityTabPage.ResumeLayout(false);
			this.SecurityTabPage.PerformLayout();
			this.WebSecurityBottomPanel.ResumeLayout(false);
			this.WebSecurityBottomPanel.PerformLayout();
			this.WebSecuritySplitContainer.Panel1.ResumeLayout(false);
			this.WebSecuritySplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.WebSecuritySplitContainer)).EndInit();
			this.WebSecuritySplitContainer.ResumeLayout(false);
			this.WebSecuritySplitContainer.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ContactSecurityGrid)).EndInit();
			this.ContactSecurityGrid.ResumeLayout(false);
			this.ContactSecurityGrid.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.WebWarehouseSecurityGrid)).EndInit();
			this.WebWarehouseSecurityGrid.ResumeLayout(false);
			this.WebWarehouseSecurityGrid.PerformLayout();
			this.WebSecurityTopPanel.ResumeLayout(false);
			this.WebSecurityTopPanel.PerformLayout();
			this.zCampaignsTabPage.ResumeLayout(false);
			this.zCampaignsTabPage.PerformLayout();
			this.CampaignsTabControl.ResumeLayout(false);
			this.CampaignsTabControl.PerformLayout();
			this.CampaignTrackingTabPage.ResumeLayout(false);
			this.CampaignTrackingTabPage.PerformLayout();
			this.CampaignTrackingTopPanel.ResumeLayout(false);
			this.CampaignTrackingTopPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.CampaignsGrid)).EndInit();
			this.CampaignsGrid.ResumeLayout(false);
			this.CampaignsGrid.PerformLayout();
			this.CampaignTrackingBottomPanel.ResumeLayout(false);
			this.CampaignTrackingBottomPanel.PerformLayout();
			this.CampaignSubscriptionsTabPage.ResumeLayout(false);
			this.CampaignSubscriptionsTabPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.SubscriptionsGrid)).EndInit();
			this.SubscriptionsGrid.ResumeLayout(false);
			this.SubscriptionsGrid.PerformLayout();
			this.CertificatesTabPage.ResumeLayout(false);
			this.CertificatesTabPage.PerformLayout();
			this.CertificatesUserControl.ResumeLayout(true);
			this.CertificatesUserControl.PerformLayout();
			this.AllocatedContactTabPage.ResumeLayout(false);
			this.AllocatedContactTabPage.PerformLayout();
			this.AllocatedContactPanel.ResumeLayout(false);
			this.AllocatedContactPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.AllocatedContactGrid)).EndInit();
			this.AllocatedContactGrid.ResumeLayout(false);
			this.AllocatedContactGrid.PerformLayout();
			this.ContactsTopSplitContainer.Panel1.ResumeLayout(false);
			this.ContactsTopSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.ContactsTopSplitContainer)).EndInit();
			this.ContactsTopSplitContainer.ResumeLayout(false);
			this.ContactsTopSplitContainer.PerformLayout();
			this.ContactsBottomSplitContainer.Panel1.ResumeLayout(false);
			this.ContactsBottomSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.ContactsBottomSplitContainer)).EndInit();
			this.ContactsBottomSplitContainer.ResumeLayout(false);
			this.ContactsBottomSplitContainer.PerformLayout();
			this.ContactsSplitContainer.Panel1.ResumeLayout(false);
			this.ContactsSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.ContactsSplitContainer)).EndInit();
			this.ContactsSplitContainer.ResumeLayout(false);
			this.ContactsSplitContainer.PerformLayout();
			this.EditToolStrip.ResumeLayout(false);
			this.EditToolStrip.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		protected ZGroupBox ConPersonalInfoGroupBox;
		protected Enterprise.ZArchitecture.ZGrid OrgContactBoundGrid;
		protected Enterprise.ZArchitecture.GUI.ZButton AutoDeliveryButton;
		protected Enterprise.ZArchitecture.ZGrid OrgDocumentBoundGrid;
		protected Enterprise.ZArchitecture.GUI.ZGroupBox DocDeliveryGroupBox;
		protected Enterprise.ZArchitecture.ZTextBox OC_SalutationTextBox;
		protected Enterprise.ZArchitecture.GUI.ZDropEdit OC_NotifyModeDropEdit;
		protected Enterprise.ZArchitecture.GUI.ZDropEdit OC_AttachmentTypeDropEdit;
		protected Enterprise.ZArchitecture.GUI.ZButton SuppressedDocsButton;
		protected Enterprise.ZArchitecture.ZLabel DocumentsLabel;
		private Enterprise.ZArchitecture.GUI.ZPanel PersonalInfoPanel;
		private Enterprise.ZArchitecture.GUI.ZPanel DocDeliveryPanel;
		private Enterprise.ZArchitecture.GUI.ZPanel OrgContactPanel;
		private Enterprise.ZArchitecture.GUI.ZPanel ContactDetailsPanel;
		protected internal Enterprise.ZArchitecture.GUI.ZTemplateTabControl ContactDetailsTabControl;
		private Enterprise.ZArchitecture.GUI.ZTabPage DetailsTabPage;
		protected internal Enterprise.ZArchitecture.GUI.ZTabPage SecurityTabPage;
		protected ZPanel ContactInfoPanel;
		private Enterprise.ZArchitecture.ZLabel JoinedIndustryYears;
		private Enterprise.ZArchitecture.ZLabel JoinedCompanyYearsLabel;
		private Enterprise.ZArchitecture.GUI.ZDateEdit JoinedIndustryDateEdit;
		private Enterprise.ZArchitecture.ZTextBox ExcelOpeningPasswordTextBox;
		private Enterprise.ZArchitecture.GUI.ZButton ViewExcelOpeningPasswordButton;
		private Enterprise.ZArchitecture.ZTextBox ExcelModifyingPasswordTextBox;
		private Enterprise.ZArchitecture.GUI.ZButton ViewExcelModifyingPasswordButton;
		private Enterprise.ZArchitecture.GUI.ZDateEdit JoinedCompanyDateEdit;
		protected Enterprise.ZArchitecture.GUI.ZDropEdit LanguageDropEdit;
		protected Enterprise.ZArchitecture.GUI.ZAddressControl AddressOverrideAddressControl;
		protected Enterprise.ZArchitecture.GUI.ZGuidFindBox OC_OH_AddressOverrideBoundGuidFindBox;
		private Enterprise.ZArchitecture.GUI.ZLinkLabel webSecurityLinkLabel;
		protected internal Enterprise.ZArchitecture.GUI.ZCheckBox WebAccessCheckBox;
		protected Enterprise.ZArchitecture.GUI.ZButton SendPasswordInstructionsButton;
		private Enterprise.ZArchitecture.ZLabel SendPasswordInstructionsLabel;
		private Enterprise.ZArchitecture.GUI.ZTabPage CampaignTrackingTabPage;
		protected Enterprise.ZArchitecture.ZGrid CampaignsGrid;
		private Enterprise.ZArchitecture.GUI.ZButton EditCampaignButton;
		protected Enterprise.ZArchitecture.GUI.ZDropEdit ContactSourceDropEdit;
		private Enterprise.ZArchitecture.GUI.ZDateEdit DetailsVerifiedDropEdit;
		protected Enterprise.ZArchitecture.GUI.ZDropEditWithFixedWidth OC_JobCategoryDropEdit;
		private Enterprise.ZArchitecture.GUI.ZPanel WebSecurityTopPanel;
		private Enterprise.ZArchitecture.GUI.ZPanel WebSecurityBottomPanel;
		private Enterprise.ZArchitecture.GUI.ZPanel CampaignTrackingBottomPanel;
		private Enterprise.ZArchitecture.GUI.ZPanel CampaignTrackingTopPanel;
		private Enterprise.ZArchitecture.GUI.ZPanel AllocatedContactPanel;
		protected Enterprise.ZArchitecture.ZGrid AllocatedContactGrid;
		protected ZTemplateTabControl PersonalInfoTabControl;
		private ZTabPage PersonalDetailsTabPage;
		protected Enterprise.ZArchitecture.ZTextBox OC_PersonalInfoBoundTextBox;
		protected ZDateEdit OC_BirthdayBoundDateEdit;
		private ZTabPage AttributesTabPage;
		private ZButton OpenURLButton;
		protected Enterprise.ZArchitecture.ZGrid AttributesGrid;
		protected CargoWise.Windows.UI.KSplitContainer ContactsTopSplitContainer;
		private CargoWise.Windows.UI.KSplitContainer ContactsBottomSplitContainer;
		private CargoWise.Windows.UI.KSplitContainer ContactsSplitContainer;
		private ZPanel AttributesTopPanel;
		private ZPanel AttributesBottomPanel;
		private ZButton ViewCampaignDocumentButton;
		private ZButton ViewCampaignButton;
		private ZButton SendCampaignButton;
		private ZButton ResendCampaignButton;
		private ZCodeFindBox NationalityCodeFindBox;
		private ZDropEdit GenderDropEdit;
		private ZTabPage CertificatesTabPage;
		private Enterprise.MasterFiles.GUI.CertificatesUserControl CertificatesUserControl;
		private ZButton ViewExamHistoryButton;
		private ZButton ViewExamButton;
		private ZButton ResitExamButton;
		private ZTabPage AllocatedContactTabPage;
		protected ZGroupBox ContactItemsGroupBox;
		protected ZButton UnlockButton;
		protected ZDateEdit LockoutDateTimeBoundDate;

		#endregion
		private ContactItemsListControl ContactItemsListControl;
		protected ZCheckBox ShowInactiveContactsCheckBox;
		protected ZCheckBox OnlyShowWebAccessEnabledContactsCheckBox;
		protected internal CargoWise.Windows.UI.KSplitContainer WebSecuritySplitContainer;
		private ZArchitecture.ZLabel WebSecurityLabel;
		private ZArchitecture.ZGrid ContactSecurityGrid;
		protected internal ZArchitecture.ZGrid WebWarehouseSecurityGrid;
		private ZTabPage CampaignSubscriptionsTabPage;
		protected ZArchitecture.ZGrid SubscriptionsGrid;
		private ZTabPage zCampaignsTabPage;
		protected ZTemplateTabControl CampaignsTabControl;
		protected ZPanel zPanelContactDetails;
		protected ZArchitecture.ZTextBox JobTitleTextBox;
		protected ZArchitecture.ZTextBox ContactNameTextBox;
		protected ZArchitecture.ZTextBox ContactsFilterStringTextBox;
		protected ZDropEdit ContactsFilterOptionDropEdit;
		public ZArchitecture.ZLabel DuplicateDetectionStatusLabel;
		protected Enterprise.ZArchitecture.GUI.ZPictureBox DuplicateDetectionStatusIcon;
		private ZToolStrip EditToolStrip;
		protected ZToolStripButton EditPersonButton;
		protected ZLinkLabel SuggestedJobCategoriesLabel;
		protected ZPanel SendPasswordInstructionToolStripPanel;
		ZToolStrip SendPasswordInstructionStrip;
		ZToolStripSplitButton SendPasswordInstructionToolStripSplitButton;
		ZToolStripButton UseWebTrackerStripButton;
		ZToolStripButton UseCargoWiseWebPortalsStripButton;
	}
}
