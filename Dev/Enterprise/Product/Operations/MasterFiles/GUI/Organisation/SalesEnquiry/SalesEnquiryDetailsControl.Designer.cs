using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.GUI
{
	partial class SalesEnquiryDetailsControl
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
				if (cancellationToken != null)
				{
					cancellationToken.Cancel();
					cancellationToken.Dispose();
					cancellationToken = null;
				}
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
            this.NotesBox = new Enterprise.ZArchitecture.GUI.ZRichTextBox();
            this.ViewSalesOpportunityButton = new Enterprise.ZArchitecture.GUI.ZButton();
            this.CloseEnquiryButton = new Enterprise.ZArchitecture.GUI.ZButton();
            this.CreateSalesOpportunityButton = new Enterprise.ZArchitecture.GUI.ZButton();
            this.LinkOrgButton = new Enterprise.ZArchitecture.GUI.ZButton();
            this.IdTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.contactGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.FaxNumberControl = new Enterprise.MasterFiles.GUI.PhoneNumberUserControl();
            this.JobDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.ContactDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEditWithFixedWidth();
            this.MobilePhoneNumberControl = new Enterprise.MasterFiles.GUI.PhoneNumberUserControl();
            this.PhoneNumberControl = new Enterprise.MasterFiles.GUI.PhoneNumberUserControl();
            this.ContactEmailAddressTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.ContactNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.AdditionalDetailsTabControl = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
            this.DetailsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
            this.LeadInterestDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.OriginalCallDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
            this.LeadAssignedSalesRepCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
            this.AssignedSalesRepBranchCaption = new Enterprise.ZArchitecture.ZLabel();
            this.AssignedSalesRepBranchCode = new Enterprise.ZArchitecture.ZLabel();
            this.AssignedSalesRepBranchName = new Enterprise.ZArchitecture.ZLabel();
            this.ReferToTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
            this.ReferToOrgGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
            this.ReferToContactDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEditWithFixedWidth();
            this.CloseReasonDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.StatusButtonsGroupBox = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.zLabel1 = new Enterprise.ZArchitecture.ZLabel();
            this.StatusDescriptionLabel = new Enterprise.ZArchitecture.ZLabel();
            this.ReferringOrgGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
            this.SourceDetailsTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.SourceTypeDropEditBox = new Enterprise.ZArchitecture.GUI.ZDropEditWithFixedWidth();
            this.CompanyGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.CompanyGroupLayoutPanel = new CargoWise.Windows.UI.KTableLayoutPanel();
            this.CompanyNamePanel = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.CompanyNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.OrgSearchButton = new Enterprise.ZArchitecture.GUI.ZButton();
            this.OrgCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.CompanyAddressCodePanel = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.CompanyAddressDropEdit = new Enterprise.ZArchitecture.GUI.ZGuidDropEdit();
            this.CompanyGroupPanel3 = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.WebsiteURLTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.BusinessRegistrationTextbox = new Enterprise.ZArchitecture.ZTextBox();
            this.CompanyAddress1TextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.CompanyPortCountryCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
            this.CompanyAddress2TextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.CompanyStateDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEditWithFixedWidth();
            this.CompanyCityTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.CompanyPostCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.ValidateAddressButton = new Enterprise.ZArchitecture.GUI.ZButton();
            this.ClearFieldsButton = new Enterprise.ZArchitecture.GUI.ZButton();
            this.scrollPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.relatedCommunicationGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.relatedCommunicationGrid = new Enterprise.MasterFiles.GUI.RelatedCommunicationGrid();
            this.SalesEnquiryTabControl = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
            this.NotesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
            this.CustomFieldsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
            this.EnquiryCustomFieldsControl = new Enterprise.ZArchitecture.GUI.ProcessTemplateCustomFieldsControl();
            this.SalesRelationsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
            this.salesRelationControl = new Enterprise.MasterFiles.GUI.SalesRelationControl();
            this.LeadGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.ReferringContactDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEditWithFixedWidth();
            this.AdditionalEnquiryButtonsGroupBox = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.enquiryTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.NotesBox.SuspendLayout();
            this.contactGroupBox.SuspendLayout();
            this.FaxNumberControl.SuspendLayout();
            this.JobDropEdit.SuspendLayout();
            this.ContactDropEdit.SuspendLayout();
            this.MobilePhoneNumberControl.SuspendLayout();
            this.PhoneNumberControl.SuspendLayout();
            this.AdditionalDetailsTabControl.SuspendLayout();
            this.DetailsTabPage.SuspendLayout();
            this.LeadInterestDropEdit.SuspendLayout();
            this.OriginalCallDateEdit.SuspendLayout();
            this.LeadAssignedSalesRepCodeFindBox.SuspendLayout();
            this.ReferToTabPage.SuspendLayout();
            this.ReferToOrgGuidFindBox.SuspendLayout();
            this.ReferToContactDropEdit.SuspendLayout();
            this.CloseReasonDropEdit.SuspendLayout();
            this.StatusButtonsGroupBox.SuspendLayout();
            this.ReferringOrgGuidFindBox.SuspendLayout();
            this.SourceTypeDropEditBox.SuspendLayout();
            this.CompanyGroupBox.SuspendLayout();
            this.CompanyGroupLayoutPanel.SuspendLayout();
            this.CompanyNamePanel.SuspendLayout();
            this.CompanyAddressCodePanel.SuspendLayout();
            this.CompanyAddressDropEdit.SuspendLayout();
            this.CompanyGroupPanel3.SuspendLayout();
            this.CompanyPortCountryCodeFindBox.SuspendLayout();
            this.CompanyStateDropEdit.SuspendLayout();
            this.scrollPanel.SuspendLayout();
            this.relatedCommunicationGroupBox.SuspendLayout();
            this.relatedCommunicationGrid.SuspendLayout();
            this.SalesEnquiryTabControl.SuspendLayout();
            this.NotesTabPage.SuspendLayout();
            this.CustomFieldsTabPage.SuspendLayout();
            this.EnquiryCustomFieldsControl.SuspendLayout();
            this.SalesRelationsTabPage.SuspendLayout();
            this.salesRelationControl.SuspendLayout();
            this.LeadGroupBox.SuspendLayout();
            this.ReferringContactDropEdit.SuspendLayout();
            this.AdditionalEnquiryButtonsGroupBox.SuspendLayout();
            this.enquiryTypeDropEdit.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.SalesEnquiry);
            // 
            // NotesBox
            // 
            this.BindingSource.SetBindingMember(this.NotesBox, "EnquiryNotesContent");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBlob)(((Enterprise.MasterFiles.Business.SalesEnquiry)(null)).EnquiryNotesContent)));
            this.NotesBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.NotesBox.IsPopupButtonVisible = false;
            this.NotesBox.IsToolBarVisible = false;
            this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.NotesBox, false);
            this.NotesBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.NotesBox.MaxLength = 3000000;
            this.NotesBox.Name = "NotesBox";
            this.NotesBox.ParentZForm = null;
            this.NotesBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(761, 265, true);
            this.NotesBox.TabIndex = 13;
            // 
            // ViewSalesOpportunityButton
            // 
            this.ViewSalesOpportunityButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.ViewSalesOpportunityButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("111c2b33-37fe-4e5b-9c20-10afa6cbaa84", "View Sales Opportunity");
            this.ViewSalesOpportunityButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1008, 3, true);
            this.ViewSalesOpportunityButton.Name = "ViewSalesOpportunityButton";
            this.ViewSalesOpportunityButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(180, 23, true);
            this.ViewSalesOpportunityButton.TabIndex = 2;
            this.ViewSalesOpportunityButton.ToolTipCaption = null;
            this.ViewSalesOpportunityButton.UseVisualStyleBackColor = false;
            this.ViewSalesOpportunityButton.Click += new System.EventHandler(this.ViewSalesOpportunityButton_Click);
            // 
            // CloseEnquiryButton
            // 
            this.CloseEnquiryButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.CloseEnquiryButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ead8564a-518a-48c2-9c0b-efb04649db91", "Close Inquiry");
            this.CloseEnquiryButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(134, 4, true);
            this.CloseEnquiryButton.Name = "CloseEnquiryButton";
            this.CloseEnquiryButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(122, 23, true);
            this.CloseEnquiryButton.TabIndex = 2;
            this.CloseEnquiryButton.ToolTipCaption = null;
            this.CloseEnquiryButton.UseVisualStyleBackColor = false;
            this.CloseEnquiryButton.Click += new System.EventHandler(this.CloseEnquiryButton_Click);
            // 
            // CreateSalesOpportunityButton
            // 
            this.CreateSalesOpportunityButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.CreateSalesOpportunityButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("5e62d27e-6d59-4eb0-a091-eee2accfe169", "Create Sales Opportunity");
            this.CreateSalesOpportunityButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1008, 3, true);
            this.CreateSalesOpportunityButton.Name = "CreateSalesOpportunityButton";
            this.CreateSalesOpportunityButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(180, 23, true);
            this.CreateSalesOpportunityButton.TabIndex = 1;
            this.CreateSalesOpportunityButton.ToolTipCaption = null;
            this.CreateSalesOpportunityButton.UseVisualStyleBackColor = false;
            // 
            // LinkOrgButton
            // 
            this.LinkOrgButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.LinkOrgButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("819b87b0-5e7d-4db1-9886-90c07bff3e45", "Set Client Intelligence");
            this.LinkOrgButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(816, 3, true);
            this.LinkOrgButton.Name = "LinkOrgButton";
            this.LinkOrgButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(186, 23, true);
            this.LinkOrgButton.TabIndex = 0;
            this.LinkOrgButton.ToolTipCaption = null;
            this.LinkOrgButton.UseVisualStyleBackColor = false;
            this.LinkOrgButton.Click += new System.EventHandler(this.LinkOrgButton_Click);
            // 
            // IdTextBox
            // 
            this.BindingSource.SetBindingMember(this.IdTextBox, "O1_LeadUniqueReference");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.SalesEnquiry)(null)).O1_LeadUniqueReference)));
            this.IdTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("15d07e7c-3bd4-4ab4-bdc7-be8973de394c", "Inquiry ID");
            this.IdTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(99, 12, true);
            this.IdTextBox.Name = "IdTextBox";
            this.IdTextBox.ReadOnly = true;
            this.IdTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(205, 38, true);
            this.IdTextBox.TabIndex = 0;
            this.IdTextBox.TabStop = false;
            // 
            // contactGroupBox
            // 
            this.contactGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("bfb84ee8-cb97-4241-a33b-0d612754dde8", "Contact");
            this.contactGroupBox.Controls.Add(this.FaxNumberControl);
            this.contactGroupBox.Controls.Add(this.JobDropEdit);
            this.contactGroupBox.Controls.Add(this.ContactDropEdit);
            this.contactGroupBox.Controls.Add(this.MobilePhoneNumberControl);
            this.contactGroupBox.Controls.Add(this.PhoneNumberControl);
            this.contactGroupBox.Controls.Add(this.ContactEmailAddressTextBox);
            this.contactGroupBox.Controls.Add(this.ContactNameTextBox);
            this.contactGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(394, 3, true);
            this.contactGroupBox.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(383, 190, true);
            this.contactGroupBox.Name = "contactGroupBox";
            this.contactGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(383, 232, true);
            this.contactGroupBox.TabIndex = 1;
            this.contactGroupBox.TabStop = false;
            // 
            // FaxNumberControl
            // 
            this.FaxNumberControl.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.FaxNumberControl, "FaxNumber");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.PhoneNumber)(((Enterprise.MasterFiles.Business.SalesEnquiry)(null)).FaxNumber)));
            this.FaxNumberControl.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("1fd4021b-dec4-4704-a065-600df9c6d137", "Fax Number");
            this.FaxNumberControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(27, 127, true);
            this.FaxNumberControl.Name = "FaxNumberControl";
            this.FaxNumberControl.ShowDiallerControl = false;
            this.FaxNumberControl.ShowLocalNumberLabel = false;
            this.FaxNumberControl.ShowPublishedCheckBox = false;
            this.FaxNumberControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 20, true);
            this.FaxNumberControl.TabIndex = 7;
            // 
            // JobDropEdit
            // 
            this.JobDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.JobDropEdit, "JobCategoryDescription");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.SalesEnquiry)(null)).JobCategoryDescription)));
            this.JobDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("c6b8fa40-c4a7-4d53-ad23-52ff56e5c46d", "Job Description");
            this.JobDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.JobDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(99, 153, true);
            this.JobDropEdit.Name = "JobDropEdit";
            this.JobDropEdit.PreBoundMaxLength = 34;
            this.JobDropEdit.ShowDescriptionBox = false;
            this.JobDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowDescription;
            this.JobDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(224, 38, true);
            this.JobDropEdit.TabIndex = 8;
            // 
            // ContactDropEdit
            // 
            this.ContactDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.ContactDropEdit, "O1_ContactName");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.SalesEnquiry)(null)).O1_ContactName)));
            this.ContactDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("2a8bd230-5834-493d-beee-b48393fd479b", "Contact", "Inquiry Contact", "");
            this.ContactDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.ContactDropEdit.EnableShowEditOrViewForm = true;
            this.ContactDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(99, 19, true);
            this.ContactDropEdit.Name = "ContactDropEdit";
            this.ContactDropEdit.PreBoundMaxLength = 41;
            this.ContactDropEdit.ShowDescriptionBox = false;
            this.ContactDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
            this.ContactDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(265, 38, true);
            this.ContactDropEdit.TabIndex = 1;
            this.ContactDropEdit.Visible = false;
            // 
            // MobilePhoneNumberControl
            // 
            this.MobilePhoneNumberControl.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.MobilePhoneNumberControl, "MobilePhoneNumber");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.PhoneNumber)(((Enterprise.MasterFiles.Business.SalesEnquiry)(null)).MobilePhoneNumber)));
            this.MobilePhoneNumberControl.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("d44b280b-6cca-4ee9-b8ed-0f52fe929cf3", "Mobile", "Mobile Number", "");
            this.MobilePhoneNumberControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(27, 101, true);
            this.MobilePhoneNumberControl.Name = "MobilePhoneNumberControl";
            this.MobilePhoneNumberControl.ShowLocalNumberLabel = false;
            this.MobilePhoneNumberControl.ShowPublishedCheckBox = false;
            this.MobilePhoneNumberControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 20, true);
            this.MobilePhoneNumberControl.TabIndex = 5;
            // 
            // PhoneNumberControl
            // 
            this.PhoneNumberControl.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.PhoneNumberControl, "PhoneNumber");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.PhoneNumber)(((Enterprise.MasterFiles.Business.SalesEnquiry)(null)).PhoneNumber)));
            this.PhoneNumberControl.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("8aaaa19b-a1a8-418d-b36b-8ce4459aa59f", "Phone", "Phone Number", "");
            this.PhoneNumberControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(27, 49, true);
            this.PhoneNumberControl.Name = "PhoneNumberControl";
            this.PhoneNumberControl.ShowLocalNumberLabel = false;
            this.PhoneNumberControl.ShowPublishedCheckBox = false;
            this.PhoneNumberControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 20, true);
            this.PhoneNumberControl.TabIndex = 2;
            // 
            // ContactEmailAddressTextBox
            // 
            this.BindingSource.SetBindingMember(this.ContactEmailAddressTextBox, "O1_Email");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.SalesEnquiry)(null)).O1_Email)));
            this.ContactEmailAddressTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("9158d255-da12-4d60-8bd9-ee6f6eca59c8", "Email", "E-Mail Address", "");
            this.ContactEmailAddressTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.ContactEmailAddressTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(99, 75, true);
            this.ContactEmailAddressTextBox.Name = "ContactEmailAddressTextBox";
            this.ContactEmailAddressTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(222, 38, true);
            this.ContactEmailAddressTextBox.TabIndex = 4;
            // 
            // ContactNameTextBox
            // 
            this.BindingSource.SetBindingMember(this.ContactNameTextBox, "O1_ContactName");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.SalesEnquiry)(null)).O1_ContactName)));
            this.ContactNameTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("17224e44-89b0-4c3d-b6d8-38bd39b73c8a", "Contact", "Inquiry Contact", "");
            this.ContactNameTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.ContactNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(99, 19, true);
            this.ContactNameTextBox.Name = "ContactNameTextBox";
            this.ContactNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(267, 38, true);
            this.ContactNameTextBox.TabIndex = 0;
            // 
            // AdditionalDetailsTabControl
            // 
            this.AdditionalDetailsTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.AdditionalDetailsTabControl.Controls.Add(this.DetailsTabPage);
            this.AdditionalDetailsTabControl.Controls.Add(this.ReferToTabPage);
            this.AdditionalDetailsTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(783, 3, true);
            this.AdditionalDetailsTabControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(385, 150, true);
            this.AdditionalDetailsTabControl.Name = "AdditionalDetailsTabControl";
            this.AdditionalDetailsTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(415, 150, true);
            this.AdditionalDetailsTabControl.TabIndex = 2;
            this.AdditionalDetailsTabControl.TabStop = false;
            // 
            // DetailsTabPage
            // 
            this.DetailsTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("6a185cba-3606-42b8-bd1b-861aa543f84a", "Details");
            this.DetailsTabPage.Controls.Add(this.LeadInterestDropEdit);
            this.DetailsTabPage.Controls.Add(this.OriginalCallDateEdit);
            this.DetailsTabPage.Controls.Add(this.LeadAssignedSalesRepCodeFindBox);
            this.DetailsTabPage.Controls.Add(this.AssignedSalesRepBranchCaption);
            this.DetailsTabPage.Controls.Add(this.AssignedSalesRepBranchCode);
            this.DetailsTabPage.Controls.Add(this.AssignedSalesRepBranchName);
            this.DetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
            this.DetailsTabPage.Name = "DetailsTabPage";
            this.DetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(407, 123, true);
            this.DetailsTabPage.TabIndex = 0;
            // 
            // LeadInterestDropEdit
            // 
            this.LeadInterestDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.LeadInterestDropEdit, "O1_InterestLevel");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.SalesEnquiry)(null)).O1_InterestLevel)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.SalesEnquiry)(null)).InterestLevelDescription)));
            this.LeadInterestDropEdit.BindToForDescription = "InterestLevelDescription";
            this.LeadInterestDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("f645882c-3bc7-48a0-8b21-790424a9402a", "Lead Interest");
            this.LeadInterestDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(130, 91, true);
            this.LeadInterestDropEdit.Name = "LeadInterestDropEdit";
            this.LeadInterestDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(163, 38, true);
            this.LeadInterestDropEdit.TabIndex = 4;
            // 
            // OriginalCallDateEdit
            // 
            this.OriginalCallDateEdit.AllowDrop = true;
            this.OriginalCallDateEdit.AutoCompleteMonthThreshold = 1;
            this.BindingSource.SetBindingMember(this.OriginalCallDateEdit, "O1_LeadCalledDateLocal");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.SalesEnquiry)(null)).O1_LeadCalledDateLocal)));
            this.OriginalCallDateEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("04e7bc2a-07eb-4df7-951d-1f73000fa24b", "Original Call");
            this.OriginalCallDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(130, 65, true);
            this.OriginalCallDateEdit.Name = "OriginalCallDateEdit";
            this.OriginalCallDateEdit.TabIndex = 3;
            // 
            // LeadAssignedSalesRepCodeFindBox
            // 
            this.LeadAssignedSalesRepCodeFindBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.LeadAssignedSalesRepCodeFindBox, "O1_GS_NKRepAssigned");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.SalesEnquiry)(null)).O1_GS_NKRepAssigned)));
            this.LeadAssignedSalesRepCodeFindBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("97fc5d6f-a9bc-4c45-a2d8-ccede718234f", "Sales Rep", "Assigned Sales Rep", "");
            this.LeadAssignedSalesRepCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(130, 13, true);
            this.LeadAssignedSalesRepCodeFindBox.Name = "LeadAssignedSalesRepCodeFindBox";
            this.LeadAssignedSalesRepCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.LeadAssignedSalesRepCodeFindBox.ParentType = null;
            this.LeadAssignedSalesRepCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(246, 38, true);
            this.LeadAssignedSalesRepCodeFindBox.TabIndex = 0;
            // 
            // AssignedSalesRepBranchCaption
            // 
            this.AssignedSalesRepBranchCaption.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("97b79035-6e78-4474-89e8-972c66ad8874", "Sales Rep\'s Branch");
            this.AssignedSalesRepBranchCaption.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.AssignedSalesRepBranchCaption.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(20, 39, true);
            this.AssignedSalesRepBranchCaption.Name = "AssignedSalesRepBranchCaption";
            this.AssignedSalesRepBranchCaption.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(110, 23, true);
            this.AssignedSalesRepBranchCaption.TabIndex = 1;
            this.AssignedSalesRepBranchCaption.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.AssignedSalesRepBranchCaption.UseMnemonic = false;
            // 
            // AssignedSalesRepBranchCode
            // 
            this.BindingSource.SetBindingMember(this.AssignedSalesRepBranchCode, "AssignedSalesRepBranchCode");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.SalesEnquiry)(null)).AssignedSalesRepBranchCode)));
            this.AssignedSalesRepBranchCode.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
            this.AssignedSalesRepBranchCode.IsFontBold = true;
            this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.AssignedSalesRepBranchCode, false);
            this.AssignedSalesRepBranchCode.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(130, 39, true);
            this.AssignedSalesRepBranchCode.Name = "AssignedSalesRepBranchCode";
            this.AssignedSalesRepBranchCode.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 20, true);
            this.AssignedSalesRepBranchCode.TabIndex = 2;
            this.AssignedSalesRepBranchCode.UseMnemonic = false;
            // 
            // AssignedSalesRepBranchName
            // 
            this.BindingSource.SetBindingMember(this.AssignedSalesRepBranchName, "RepAssigned+HomeBranch+GB_BranchName");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.SalesEnquiry)(null)).RepAssigned.HomeBranch.GB_BranchName)));
            this.AssignedSalesRepBranchName.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.AssignedSalesRepBranchName.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(175, 39, true);
            this.AssignedSalesRepBranchName.Name = "AssignedSalesRepBranchName";
            this.AssignedSalesRepBranchName.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
            this.AssignedSalesRepBranchName.TabIndex = 2;
            this.AssignedSalesRepBranchName.UseMnemonic = false;
            // 
            // ReferToTabPage
            // 
            this.ReferToTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("8acc13e9-786f-419a-8c44-73d73ba5ea5d", "Refer To");
            this.ReferToTabPage.Controls.Add(this.ReferToOrgGuidFindBox);
            this.ReferToTabPage.Controls.Add(this.ReferToContactDropEdit);
            this.ReferToTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
            this.ReferToTabPage.Name = "ReferToTabPage";
            this.ReferToTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(407, 123, true);
            this.ReferToTabPage.TabIndex = 1;
            // 
            // ReferToOrgGuidFindBox
            // 
            this.ReferToOrgGuidFindBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.ReferToOrgGuidFindBox, "O1_OH_ReferTo");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.SalesEnquiry)(null)).O1_OH_ReferTo)));
            this.ReferToOrgGuidFindBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("62d9c78c-3a85-4958-b5f0-62461d888638", "Refer To Organization");
            this.ReferToOrgGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(130, 13, true);
            this.ReferToOrgGuidFindBox.Name = "ReferToOrgGuidFindBox";
            this.ReferToOrgGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.ReferToOrgGuidFindBox.ParentType = null;
            this.ReferToOrgGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(246, 38, true);
            this.ReferToOrgGuidFindBox.TabIndex = 0;
            // 
            // ReferToContactDropEdit
            // 
            this.ReferToContactDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.ReferToContactDropEdit, "ReferToContactName");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.SalesEnquiry)(null)).ReferToContactName)));
            this.ReferToContactDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ec586d68-75d4-434a-9a40-cb6ed9cb650e", "Refer To Contact");
            this.ReferToContactDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.ReferToContactDropEdit.EnableShowEditOrViewForm = true;
            this.ReferToContactDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(129, 39, true);
            this.ReferToContactDropEdit.Name = "ReferToContactDropEdit";
            this.ReferToContactDropEdit.PreBoundMaxLength = 38;
            this.ReferToContactDropEdit.ShowDescriptionBox = false;
            this.ReferToContactDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
            this.ReferToContactDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(247, 38, true);
            this.ReferToContactDropEdit.TabIndex = 1;
            // 
            // CloseReasonDropEdit
            // 
            this.CloseReasonDropEdit.AllowDrop = true;
            this.CloseReasonDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.BindingSource.SetBindingMember(this.CloseReasonDropEdit, "O1_CloseReason");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.SalesEnquiry)(null)).O1_CloseReason)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.SalesEnquiry)(null)).CloseReasonDescription)));
            this.CloseReasonDropEdit.BindToForDescription = "CloseReasonDescription";
            this.CloseReasonDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("b0d270b4-54a3-4300-9b4f-440dd5e38168", "Close Reason");
            this.CloseReasonDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1024, 11, true);
            this.CloseReasonDropEdit.Name = "CloseReasonDropEdit";
            this.CloseReasonDropEdit.PreBoundMaxLength = 3;
            this.CloseReasonDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(170, 38, true);
            this.CloseReasonDropEdit.TabIndex = 4;
            this.CloseReasonDropEdit.Visible = false;
            // 
            // StatusButtonsGroupBox
            // 
            this.StatusButtonsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.StatusButtonsGroupBox.Controls.Add(this.zLabel1);
            this.StatusButtonsGroupBox.Controls.Add(this.StatusDescriptionLabel);
            this.StatusButtonsGroupBox.Controls.Add(this.CloseEnquiryButton);
            this.StatusButtonsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(940, 5, true);
            this.StatusButtonsGroupBox.Name = "StatusButtonsGroupBox";
            this.StatusButtonsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(258, 29, true);
            this.StatusButtonsGroupBox.TabIndex = 2;
            // 
            // zLabel1
            // 
            this.zLabel1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.zLabel1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("e1767e35-16e9-4cb8-8396-457fddd8aeb9", "Status");
            this.zLabel1.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.zLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 4, true);
            this.zLabel1.Name = "zLabel1";
            this.zLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(42, 23, true);
            this.zLabel1.TabIndex = 0;
            this.zLabel1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.zLabel1.UseMnemonic = false;
            // 
            // StatusDescriptionLabel
            // 
            this.StatusDescriptionLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.BindingSource.SetBindingMember(this.StatusDescriptionLabel, "StatusDescription");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.SalesEnquiry)(null)).StatusDescription)));
            this.StatusDescriptionLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
            this.StatusDescriptionLabel.ForeColor = System.Drawing.Color.White;
            this.StatusDescriptionLabel.IsFontBold = true;
            this.StatusDescriptionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(47, 6, true);
            this.StatusDescriptionLabel.Name = "StatusDescriptionLabel";
            this.StatusDescriptionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(79, 20, true);
            this.StatusDescriptionLabel.TabIndex = 1;
            this.StatusDescriptionLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.StatusDescriptionLabel.UseMnemonic = false;
            // 
            // ReferringOrgGuidFindBox
            // 
            this.ReferringOrgGuidFindBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.ReferringOrgGuidFindBox, "O1_OH_SourceOfLead");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.SalesEnquiry)(null)).O1_OH_SourceOfLead)));
            this.ReferringOrgGuidFindBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("d2afd525-4ff9-4d9b-840e-2a22a58fe520", "Referring Organization");
            this.ReferringOrgGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(134, 71, true);
            this.ReferringOrgGuidFindBox.Name = "ReferringOrgGuidFindBox";
            this.ReferringOrgGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.ReferringOrgGuidFindBox.ParentType = null;
            this.ReferringOrgGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(246, 38, true);
            this.ReferringOrgGuidFindBox.TabIndex = 6;
            // 
            // SourceDetailsTextBox
            // 
            this.BindingSource.SetBindingMember(this.SourceDetailsTextBox, "O1_OpportunitySourceDetails");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.SalesEnquiry)(null)).O1_OpportunitySourceDetails)));
            this.SourceDetailsTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("7bbb2990-6726-4cef-b818-2d8998f99a8b", "Source Details");
            this.SourceDetailsTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.SourceDetailsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(134, 45, true);
            this.SourceDetailsTextBox.Name = "SourceDetailsTextBox";
            this.SourceDetailsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(246, 38, true);
            this.SourceDetailsTextBox.TabIndex = 2;
            // 
            // SourceTypeDropEditBox
            // 
            this.SourceTypeDropEditBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.SourceTypeDropEditBox, "O1_LeadSource");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.SalesEnquiry)(null)).O1_LeadSource)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.SalesEnquiry)(null)).SourceDescription)));
            this.SourceTypeDropEditBox.BindToForDescription = "SourceDescription";
            this.SourceTypeDropEditBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("cb9bbdb1-371b-4806-8d8b-082dd7b0c6ef", "Source");
            this.SourceTypeDropEditBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(134, 19, true);
            this.SourceTypeDropEditBox.Name = "SourceTypeDropEditBox";
            this.SourceTypeDropEditBox.PreBoundMaxLength = 5;
            this.SourceTypeDropEditBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(246, 38, true);
            this.SourceTypeDropEditBox.TabIndex = 1;
            // 
            // CompanyGroupBox
            // 
            this.CompanyGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("affdd3cf-a51c-456a-bfd8-700ded16877f", "Organization");
            this.CompanyGroupBox.Controls.Add(this.CompanyGroupLayoutPanel);
            this.CompanyGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 3, true);
            this.CompanyGroupBox.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(383, 190, true);
            this.CompanyGroupBox.Name = "CompanyGroupBox";
            this.CompanyGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(383, 232, true);
            this.CompanyGroupBox.TabIndex = 0;
            this.CompanyGroupBox.TabStop = false;
            // 
            // CompanyGroupLayoutPanel
            // 
            this.CompanyGroupLayoutPanel.ColumnCount = 2;
            this.CompanyGroupLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.CompanyGroupLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(20)));
            this.CompanyGroupLayoutPanel.Controls.Add(this.CompanyNamePanel, 0, 0);
            this.CompanyGroupLayoutPanel.Controls.Add(this.CompanyAddressCodePanel, 0, 1);
            this.CompanyGroupLayoutPanel.Controls.Add(this.CompanyGroupPanel3, 0, 2);
            this.CompanyGroupLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.CompanyGroupLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 34, true);
            this.CompanyGroupLayoutPanel.Name = "CompanyGroupLayoutPanel";
            this.CompanyGroupLayoutPanel.RowCount = 3;
            this.CompanyGroupLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.CompanyGroupLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.CompanyGroupLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.CompanyGroupLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(377, 195, true);
            this.CompanyGroupLayoutPanel.TabIndex = 9;
            // 
            // CompanyNamePanel
            // 
            this.CompanyNamePanel.Controls.Add(this.CompanyNameTextBox);
            this.CompanyNamePanel.Controls.Add(this.OrgSearchButton);
            this.CompanyNamePanel.Controls.Add(this.OrgCodeTextBox);
            this.CompanyNamePanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.CompanyNamePanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.CompanyNamePanel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
            this.CompanyNamePanel.Name = "CompanyNamePanel";
            this.CompanyNamePanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(371, 25, true);
            this.CompanyNamePanel.TabIndex = 0;
            // 
            // CompanyNameTextBox
            // 
            this.BindingSource.SetBindingMember(this.CompanyNameTextBox, "O1_CompanyName");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.SalesEnquiry)(null)).O1_CompanyName)));
            this.CompanyNameTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("14565746-e2db-4ec2-aa96-309ac41eaca0", "Name");
            this.CompanyNameTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.CompanyNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 2, true);
            this.CompanyNameTextBox.Name = "CompanyNameTextBox";
            this.CompanyNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(162, 38, true);
            this.CompanyNameTextBox.TabIndex = 0;
            this.CompanyNameTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.CompanyNameTextBox_KeyDown);
            this.CompanyNameTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.CompanyNameTextBox_KeyPress);
            // 
            // OrgSearchButton
            // 
            this.OrgSearchButton.Font = new System.Drawing.Font("Tahoma", 8F, System.Drawing.FontStyle.Bold);
            this.OrgSearchButton.IsCaptionOverridden = true;
            this.OrgSearchButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(253, 2, true);
            this.OrgSearchButton.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
            this.OrgSearchButton.Name = "OrgSearchButton";
            this.OrgSearchButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(24, 20, true);
            this.OrgSearchButton.TabIndex = 1;
            this.OrgSearchButton.Text = "...";
            this.OrgSearchButton.ToolTipCaption = null;
            this.OrgSearchButton.UseVisualStyleBackColor = false;
            this.OrgSearchButton.Click += new System.EventHandler(this.OrgSearchButton_Click);
            // 
            // OrgCodeTextBox
            // 
            this.BindingSource.SetBindingMember(this.OrgCodeTextBox, "OrgCode");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.SalesEnquiry)(null)).OrgCode)));
            this.OrgCodeTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.OrgCodeTextBox, false);
            this.OrgCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(277, 3, true);
            this.OrgCodeTextBox.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
            this.OrgCodeTextBox.Name = "OrgCodeTextBox";
            this.OrgCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 38, true);
            this.OrgCodeTextBox.TabIndex = 2;
            // 
            // CompanyAddressCodePanel
            // 
            this.CompanyAddressCodePanel.Controls.Add(this.CompanyAddressDropEdit);
            this.CompanyAddressCodePanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.CompanyAddressCodePanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 25, true);
            this.CompanyAddressCodePanel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
            this.CompanyAddressCodePanel.Name = "CompanyAddressCodePanel";
            this.CompanyAddressCodePanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(371, 27, true);
            this.CompanyAddressCodePanel.TabIndex = 1;
            // 
            // CompanyAddressDropEdit
            // 
            this.CompanyAddressDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.CompanyAddressDropEdit, "O1_OA_LinkedAddress");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.SalesEnquiry)(null)).O1_OA_LinkedAddress)));
            this.CompanyAddressDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("b4274f3b-65d3-4146-9324-ce3a958d4d88", "Address Code");
            this.CompanyAddressDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 5, true);
            this.CompanyAddressDropEdit.Name = "CompanyAddressDropEdit";
            this.CompanyAddressDropEdit.PreBoundMaxLength = 27;
            this.CompanyAddressDropEdit.ShowDescriptionBox = false;
            this.CompanyAddressDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(182, 38, true);
            this.CompanyAddressDropEdit.TabIndex = 0;
            // 
            // CompanyGroupPanel3
            // 
            this.CompanyGroupPanel3.Controls.Add(this.WebsiteURLTextBox);
            this.CompanyGroupPanel3.Controls.Add(this.BusinessRegistrationTextbox);
            this.CompanyGroupPanel3.Controls.Add(this.CompanyAddress1TextBox);
            this.CompanyGroupPanel3.Controls.Add(this.CompanyPortCountryCodeFindBox);
            this.CompanyGroupPanel3.Controls.Add(this.CompanyAddress2TextBox);
            this.CompanyGroupPanel3.Controls.Add(this.CompanyStateDropEdit);
            this.CompanyGroupPanel3.Controls.Add(this.CompanyCityTextBox);
            this.CompanyGroupPanel3.Controls.Add(this.CompanyPostCodeTextBox);
            this.CompanyGroupPanel3.Controls.Add(this.ValidateAddressButton);
            this.CompanyGroupPanel3.Controls.Add(this.ClearFieldsButton);
            this.CompanyGroupPanel3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 52, true);
            this.CompanyGroupPanel3.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
            this.CompanyGroupPanel3.Name = "CompanyGroupPanel3";
            this.CompanyGroupPanel3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(371, 161, true);
            this.CompanyGroupPanel3.TabIndex = 2;
            // 
            // WebsiteURLTextBox
            // 
            this.BindingSource.SetBindingMember(this.WebsiteURLTextBox, "O1_WebAddress");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.SalesEnquiry)(null)).O1_WebAddress)));
            this.WebsiteURLTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("e3c0f14c-8970-4cbf-a9ba-65323442ef08", "Website", "Website URL");
            this.WebsiteURLTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.WebsiteURLTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 112, true);
            this.WebsiteURLTextBox.Name = "WebsiteURLTextBox";
            this.WebsiteURLTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(269, 38, true);
            this.WebsiteURLTextBox.TabIndex = 10;
            // 
            // BusinessRegistrationTextbox
            // 
            this.BindingSource.SetBindingMember(this.BusinessRegistrationTextbox, "O1_BusinessRegNo");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.SalesEnquiry)(null)).O1_BusinessRegNo)));
            this.BusinessRegistrationTextbox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("6DDCBFD7-7E02-43FF-BB83-2FAC0335FB47", "Reg. Number");
            this.BusinessRegistrationTextbox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.BusinessRegistrationTextbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 138, true);
            this.BusinessRegistrationTextbox.Name = "BusinessRegistrationTextbox";
            this.BusinessRegistrationTextbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(269, 38, true);
            this.BusinessRegistrationTextbox.TabIndex = 11;
            // 
            // CompanyAddress1TextBox
            // 
            this.BindingSource.SetBindingMember(this.CompanyAddress1TextBox, "O1_Address1");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.SalesEnquiry)(null)).O1_Address1)));
            this.CompanyAddress1TextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("f6007fe3-9545-484a-a761-b99691486474", "Address 1");
            this.CompanyAddress1TextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.CompanyAddress1TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 7, true);
            this.CompanyAddress1TextBox.Name = "CompanyAddress1TextBox";
            this.CompanyAddress1TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(215, 38, true);
            this.CompanyAddress1TextBox.TabIndex = 3;
            // 
            // CompanyPortCountryCodeFindBox
            // 
            this.CompanyPortCountryCodeFindBox.AllowDrop = true;
            this.CompanyPortCountryCodeFindBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.BindingSource.SetBindingMember(this.CompanyPortCountryCodeFindBox, "O1_PortOrCountry");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.SalesEnquiry)(null)).O1_PortOrCountry)));
            this.CompanyPortCountryCodeFindBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("3f28e0d1-6f9a-4515-945b-97ec07efa5ca", "Port/Ctry./Rgn.", "Port / Country(Region)");
            this.CompanyPortCountryCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 59, true);
            this.CompanyPortCountryCodeFindBox.Name = "CompanyPortCountryCodeFindBox";
            this.CompanyPortCountryCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.CompanyPortCountryCodeFindBox.ParentType = null;
            this.CompanyPortCountryCodeFindBox.PreBoundMaxLength = 5;
            this.CompanyPortCountryCodeFindBox.ShowDescriptionBox = false;
            this.CompanyPortCountryCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(77, 38, true);
            this.CompanyPortCountryCodeFindBox.TabIndex = 6;
            // 
            // CompanyAddress2TextBox
            // 
            this.BindingSource.SetBindingMember(this.CompanyAddress2TextBox, "O1_Address2");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.SalesEnquiry)(null)).O1_Address2)));
            this.CompanyAddress2TextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("b59dacb1-310e-493c-8e2d-ffc8f2c880f8", "Address 2");
            this.CompanyAddress2TextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.CompanyAddress2TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 33, true);
            this.CompanyAddress2TextBox.Name = "CompanyAddress2TextBox";
            this.CompanyAddress2TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(269, 38, true);
            this.CompanyAddress2TextBox.TabIndex = 4;
            // 
            // CompanyStateDropEdit
            // 
            this.CompanyStateDropEdit.AllowDrop = true;
            this.CompanyStateDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.BindingSource.SetBindingMember(this.CompanyStateDropEdit, "O1_State");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.SalesEnquiry)(null)).O1_State)));
            this.CompanyStateDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("90b9a0d7-18ab-496d-935d-cf11260ced45", "State");
            this.CompanyStateDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.CompanyStateDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(203, 86, true);
            this.CompanyStateDropEdit.Name = "CompanyStateDropEdit";
            this.CompanyStateDropEdit.PreBoundMaxLength = 4;
            this.CompanyStateDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(154, 38, true);
            this.CompanyStateDropEdit.TabIndex = 9;
            // 
            // CompanyCityTextBox
            // 
            this.BindingSource.SetBindingMember(this.CompanyCityTextBox, "O1_City");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.SalesEnquiry)(null)).O1_City)));
            this.CompanyCityTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("2299bc70-961a-4c03-8fbe-855922c300dd", "City");
            this.CompanyCityTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.CompanyCityTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(228, 59, true);
            this.CompanyCityTextBox.Name = "CompanyCityTextBox";
            this.CompanyCityTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(129, 38, true);
            this.CompanyCityTextBox.TabIndex = 8;
            // 
            // CompanyPostCodeTextBox
            // 
            this.BindingSource.SetBindingMember(this.CompanyPostCodeTextBox, "O1_PostCode");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.SalesEnquiry)(null)).O1_PostCode)));
            this.CompanyPostCodeTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("7911b06f-159c-407c-8413-bb659fa0ab75", "Post Code");
            this.CompanyPostCodeTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.CompanyPostCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 86, true);
            this.CompanyPostCodeTextBox.Name = "CompanyPostCodeTextBox";
            this.CompanyPostCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(55, 38, true);
            this.CompanyPostCodeTextBox.TabIndex = 7;
            // 
            // ValidateAddressButton
            // 
            this.ValidateAddressButton.IsCaptionOverridden = true;
            this.ValidateAddressButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(323, 6, true);
            this.ValidateAddressButton.Name = "ValidateAddressButton";
            this.ValidateAddressButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(34, 22, true);
            this.ValidateAddressButton.TabIndex = 12;
            this.ValidateAddressButton.TabStop = false;
            this.ValidateAddressButton.Text = " ";
            this.ValidateAddressButton.ToolTipCaption = null;
            this.ValidateAddressButton.UseVisualStyleBackColor = true;
			this.ValidateAddressButton.Click += new System.EventHandler(this.ValidateAddressButton_Click);
			// 
			// ClearFieldsButton
			//
			this.ClearFieldsButton.Image = global::Enterprise.MasterFiles.GUI.Properties.Resources.xIcon;
			this.ClearFieldsButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(303, 6, true);
            this.ClearFieldsButton.Name = "ClearFieldsButton";
            this.ClearFieldsButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(20, 20, true);
            this.ClearFieldsButton.TabIndex = 13;
            this.ClearFieldsButton.TabStop = false;
            this.ClearFieldsButton.ToolTipCaption = null;
            this.ClearFieldsButton.UseVisualStyleBackColor = true;
			this.ClearFieldsButton.Click += new System.EventHandler(this.ClearFieldsButton_Click);
			// 
			// scrollPanel
			// 
			this.scrollPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.scrollPanel.AutoScroll = true;
            this.scrollPanel.AutoScrollMinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(976, 0, true);
            this.scrollPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.scrollPanel.Controls.Add(this.relatedCommunicationGroupBox);
            this.scrollPanel.Controls.Add(this.SalesEnquiryTabControl);
            this.scrollPanel.Controls.Add(this.LeadGroupBox);
            this.scrollPanel.Controls.Add(this.CompanyGroupBox);
            this.scrollPanel.Controls.Add(this.AdditionalDetailsTabControl);
            this.scrollPanel.Controls.Add(this.contactGroupBox);
            this.scrollPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 38, true);
            this.scrollPanel.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(976, 0, true);
            this.scrollPanel.Name = "scrollPanel";
            this.scrollPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1204, 536, true);
            this.scrollPanel.TabIndex = 6;
            // 
            // relatedCommunicationGroupBox
            // 
            this.relatedCommunicationGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.relatedCommunicationGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("329dc61d-83d0-4eb9-a3e6-f3deb3fc9910", "Related Communication");
            this.relatedCommunicationGroupBox.Controls.Add(this.relatedCommunicationGrid);
            this.relatedCommunicationGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(783, 295, true);
            this.relatedCommunicationGroupBox.Name = "relatedCommunicationGroupBox";
            this.relatedCommunicationGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(414, 235, true);
            this.relatedCommunicationGroupBox.TabIndex = 6;
            this.relatedCommunicationGroupBox.TabStop = false;
            // 
            // relatedCommunicationGrid
            // 
            this.relatedCommunicationGrid.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.relatedCommunicationGrid, "RelatedCommunicationCollection");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.OrgSalesCallCollection)(((Enterprise.MasterFiles.Business.SalesEnquiry)(null)).RelatedCommunicationCollection)));
            this.relatedCommunicationGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.relatedCommunicationGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 34, true);
            this.relatedCommunicationGrid.Name = "relatedCommunicationGrid";
            this.relatedCommunicationGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(408, 198, true);
            this.relatedCommunicationGrid.TabIndex = 0;
            // 
            // SalesEnquiryTabControl
            // 
            this.SalesEnquiryTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.SalesEnquiryTabControl.Controls.Add(this.NotesTabPage);
            this.SalesEnquiryTabControl.Controls.Add(this.CustomFieldsTabPage);
            this.SalesEnquiryTabControl.Controls.Add(this.SalesRelationsTabPage);
            this.SalesEnquiryTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 241, true);
            this.SalesEnquiryTabControl.Name = "SalesEnquiryTabControl";
            this.SalesEnquiryTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(769, 292, true);
            this.SalesEnquiryTabControl.TabIndex = 5;
            this.SalesEnquiryTabControl.TabStop = false;
            // 
            // NotesTabPage
            // 
            this.NotesTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("1cced39c-5053-49f2-bf53-acfb5ab5b719", "Notes");
            this.NotesTabPage.Controls.Add(this.NotesBox);
            this.NotesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
            this.NotesTabPage.Name = "NotesTabPage";
            this.NotesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(761, 265, true);
            this.NotesTabPage.TabIndex = 0;
            // 
            // CustomFieldsTabPage
            // 
            this.CustomFieldsTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("c2c80272-71a1-43d8-9df4-e542d5c4bd9b", "Custom Fields");
            this.CustomFieldsTabPage.Controls.Add(this.EnquiryCustomFieldsControl);
            this.CustomFieldsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
            this.CustomFieldsTabPage.Name = "CustomFieldsTabPage";
            this.CustomFieldsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, 7, 3, 3, true);
            this.CustomFieldsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(761, 265, true);
            this.CustomFieldsTabPage.TabIndex = 1;
            // 
            // EnquiryCustomFieldsControl
            // 
            this.EnquiryCustomFieldsControl.AllowDrop = true;
            this.EnquiryCustomFieldsControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.EnquiryCustomFieldsControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 7, true);
            this.EnquiryCustomFieldsControl.Name = "EnquiryCustomFieldsControl";
            this.EnquiryCustomFieldsControl.NothingSetupMessageLabelText = "";
            this.EnquiryCustomFieldsControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(755, 255, true);
            this.EnquiryCustomFieldsControl.TabIndex = 0;
            // 
            // SalesRelationsTabPage
            // 
            this.SalesRelationsTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("6308020c-20cf-49ff-97e1-a1d4d021dddf", "Sales Relations");
            this.SalesRelationsTabPage.Controls.Add(this.salesRelationControl);
            this.SalesRelationsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
            this.SalesRelationsTabPage.Name = "SalesRelationsTabPage";
            this.SalesRelationsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(761, 265, true);
            this.SalesRelationsTabPage.TabIndex = 2;
            // 
            // salesRelationControl
            // 
            this.salesRelationControl.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.salesRelationControl, "SalesRelationModel");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.SalesRelationModel)(((Enterprise.MasterFiles.Business.SalesEnquiry)(null)).SalesRelationModel)));
            this.salesRelationControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.salesRelationControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.salesRelationControl.Name = "salesRelationControl";
            this.salesRelationControl.NameOfATreeElement = Enterprise.MasterFiles.GUI.Res.GetData("e0bdcf6a-9125-4f4d-b8a1-f1a0ad65448c", "Relatable Activity");
            this.salesRelationControl.NameOfTreeElementsPlural = Enterprise.MasterFiles.GUI.Res.GetData("d5f8070a-7663-4373-9277-ccb79a2c0f65", "Relatable Activities");
            this.salesRelationControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(761, 265, true);
            this.salesRelationControl.TabIndex = 0;
            // 
            // LeadGroupBox
            // 
            this.LeadGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.LeadGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("c78c6c57-2003-4bb4-814f-7ec093e613e9", "Lead Source");
            this.LeadGroupBox.Controls.Add(this.ReferringContactDropEdit);
            this.LeadGroupBox.Controls.Add(this.ReferringOrgGuidFindBox);
            this.LeadGroupBox.Controls.Add(this.SourceTypeDropEditBox);
            this.LeadGroupBox.Controls.Add(this.SourceDetailsTextBox);
            this.LeadGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(783, 159, true);
            this.LeadGroupBox.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(383, 130, true);
            this.LeadGroupBox.Name = "LeadGroupBox";
            this.LeadGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(414, 130, true);
            this.LeadGroupBox.TabIndex = 4;
            this.LeadGroupBox.TabStop = false;
            // 
            // ReferringContactDropEdit
            // 
            this.ReferringContactDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.ReferringContactDropEdit, "ReferringContactName");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.SalesEnquiry)(null)).ReferringContactName)));
            this.ReferringContactDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("5485ced4-004f-4bcc-9a46-a037358f5a1e", "Referring Contact");
            this.ReferringContactDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.ReferringContactDropEdit.EnableShowEditOrViewForm = true;
            this.ReferringContactDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(133, 97, true);
            this.ReferringContactDropEdit.Name = "ReferringContactDropEdit";
            this.ReferringContactDropEdit.PreBoundMaxLength = 38;
            this.ReferringContactDropEdit.ShowDescriptionBox = false;
            this.ReferringContactDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
            this.ReferringContactDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(247, 38, true);
            this.ReferringContactDropEdit.TabIndex = 7;
            // 
            // AdditionalEnquiryButtonsGroupBox
            // 
            this.AdditionalEnquiryButtonsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.AdditionalEnquiryButtonsGroupBox.Controls.Add(this.ViewSalesOpportunityButton);
            this.AdditionalEnquiryButtonsGroupBox.Controls.Add(this.LinkOrgButton);
            this.AdditionalEnquiryButtonsGroupBox.Controls.Add(this.CreateSalesOpportunityButton);
            this.AdditionalEnquiryButtonsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 577, true);
            this.AdditionalEnquiryButtonsGroupBox.Name = "AdditionalEnquiryButtonsGroupBox";
            this.AdditionalEnquiryButtonsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1194, 29, true);
            this.AdditionalEnquiryButtonsGroupBox.TabIndex = 5;
            // 
            // enquiryTypeDropEdit
            // 
            this.enquiryTypeDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.enquiryTypeDropEdit, "O1_EnquiryType");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.SalesEnquiry)(null)).O1_EnquiryType)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.SalesEnquiry)(null)).EnquiryTypeDescription)));
            this.enquiryTypeDropEdit.BindToForDescription = "EnquiryTypeDescription";
            this.enquiryTypeDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("19b6cfdd-20f7-4833-bae3-3ea1ab3872c1", "Type");
            this.enquiryTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(394, 11, true);
            this.enquiryTypeDropEdit.Name = "enquiryTypeDropEdit";
            this.enquiryTypeDropEdit.PreBoundMaxLength = 3;
            this.enquiryTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 38, true);
            this.enquiryTypeDropEdit.TabIndex = 1;
            // 
            // SalesEnquiryDetailsControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.AutoScroll = true;
            this.AutoScrollMinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1200, 520, true);
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.enquiryTypeDropEdit);
            this.Controls.Add(this.AdditionalEnquiryButtonsGroupBox);
            this.Controls.Add(this.scrollPanel);
            this.Controls.Add(this.IdTextBox);
            this.Controls.Add(this.StatusButtonsGroupBox);
            this.Controls.Add(this.CloseReasonDropEdit);
            this.Name = "SalesEnquiryDetailsControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1207, 609, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.NotesBox.ResumeLayout(true);
            this.NotesBox.PerformLayout();
            this.contactGroupBox.ResumeLayout(false);
            this.contactGroupBox.PerformLayout();
            this.FaxNumberControl.ResumeLayout(true);
            this.FaxNumberControl.PerformLayout();
            this.JobDropEdit.ResumeLayout(true);
            this.JobDropEdit.PerformLayout();
            this.ContactDropEdit.ResumeLayout(true);
            this.ContactDropEdit.PerformLayout();
            this.MobilePhoneNumberControl.ResumeLayout(true);
            this.MobilePhoneNumberControl.PerformLayout();
            this.PhoneNumberControl.ResumeLayout(true);
            this.PhoneNumberControl.PerformLayout();
            this.AdditionalDetailsTabControl.ResumeLayout(false);
            this.AdditionalDetailsTabControl.PerformLayout();
            this.DetailsTabPage.ResumeLayout(false);
            this.DetailsTabPage.PerformLayout();
            this.LeadInterestDropEdit.ResumeLayout(true);
            this.LeadInterestDropEdit.PerformLayout();
            this.OriginalCallDateEdit.ResumeLayout(true);
            this.OriginalCallDateEdit.PerformLayout();
            this.LeadAssignedSalesRepCodeFindBox.ResumeLayout(true);
            this.LeadAssignedSalesRepCodeFindBox.PerformLayout();
            this.ReferToTabPage.ResumeLayout(false);
            this.ReferToTabPage.PerformLayout();
            this.ReferToOrgGuidFindBox.ResumeLayout(true);
            this.ReferToOrgGuidFindBox.PerformLayout();
            this.ReferToContactDropEdit.ResumeLayout(true);
            this.ReferToContactDropEdit.PerformLayout();
            this.CloseReasonDropEdit.ResumeLayout(true);
            this.CloseReasonDropEdit.PerformLayout();
            this.StatusButtonsGroupBox.ResumeLayout(false);
            this.StatusButtonsGroupBox.PerformLayout();
            this.ReferringOrgGuidFindBox.ResumeLayout(true);
            this.ReferringOrgGuidFindBox.PerformLayout();
            this.SourceTypeDropEditBox.ResumeLayout(true);
            this.SourceTypeDropEditBox.PerformLayout();
            this.CompanyGroupBox.ResumeLayout(false);
            this.CompanyGroupBox.PerformLayout();
            this.CompanyGroupLayoutPanel.ResumeLayout(false);
            this.CompanyGroupLayoutPanel.PerformLayout();
            this.CompanyNamePanel.ResumeLayout(false);
            this.CompanyNamePanel.PerformLayout();
            this.CompanyAddressCodePanel.ResumeLayout(false);
            this.CompanyAddressCodePanel.PerformLayout();
            this.CompanyAddressDropEdit.ResumeLayout(true);
            this.CompanyAddressDropEdit.PerformLayout();
            this.CompanyGroupPanel3.ResumeLayout(false);
            this.CompanyGroupPanel3.PerformLayout();
            this.CompanyPortCountryCodeFindBox.ResumeLayout(true);
            this.CompanyPortCountryCodeFindBox.PerformLayout();
            this.CompanyStateDropEdit.ResumeLayout(true);
            this.CompanyStateDropEdit.PerformLayout();
            this.scrollPanel.ResumeLayout(false);
            this.scrollPanel.PerformLayout();
            this.relatedCommunicationGroupBox.ResumeLayout(false);
            this.relatedCommunicationGroupBox.PerformLayout();
            this.relatedCommunicationGrid.ResumeLayout(true);
            this.relatedCommunicationGrid.PerformLayout();
            this.SalesEnquiryTabControl.ResumeLayout(false);
            this.SalesEnquiryTabControl.PerformLayout();
            this.NotesTabPage.ResumeLayout(false);
            this.NotesTabPage.PerformLayout();
            this.CustomFieldsTabPage.ResumeLayout(false);
            this.CustomFieldsTabPage.PerformLayout();
            this.EnquiryCustomFieldsControl.ResumeLayout(true);
            this.EnquiryCustomFieldsControl.PerformLayout();
            this.SalesRelationsTabPage.ResumeLayout(false);
            this.SalesRelationsTabPage.PerformLayout();
            this.salesRelationControl.ResumeLayout(true);
            this.salesRelationControl.PerformLayout();
            this.LeadGroupBox.ResumeLayout(false);
            this.LeadGroupBox.PerformLayout();
            this.ReferringContactDropEdit.ResumeLayout(true);
            this.ReferringContactDropEdit.PerformLayout();
            this.AdditionalEnquiryButtonsGroupBox.ResumeLayout(false);
            this.AdditionalEnquiryButtonsGroupBox.PerformLayout();
            this.enquiryTypeDropEdit.ResumeLayout(true);
            this.enquiryTypeDropEdit.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.GUI.ZPanel AdditionalEnquiryButtonsGroupBox;
		internal ZArchitecture.GUI.ZButton CloseEnquiryButton;
		internal ZArchitecture.GUI.ZButton CreateSalesOpportunityButton;
		internal ZArchitecture.GUI.ZButton LinkOrgButton;
		internal ZArchitecture.ZTextBox OrgCodeTextBox;
		private ZArchitecture.GUI.ZRichTextBox NotesBox;
		private ZArchitecture.ZTextBox IdTextBox;
		internal ZArchitecture.GUI.ZDropEdit CloseReasonDropEdit;
		private ZArchitecture.GUI.ZGroupBox contactGroupBox;
		protected PhoneNumberUserControl MobilePhoneNumberControl;
		protected PhoneNumberUserControl PhoneNumberControl;
		private ZArchitecture.ZTextBox ContactEmailAddressTextBox;
		internal ZArchitecture.ZTextBox ContactNameTextBox;
		private ZArchitecture.GUI.ZTemplateTabControl AdditionalDetailsTabControl;
		private ZArchitecture.GUI.ZTabPage DetailsTabPage;
		private ZArchitecture.GUI.ZTabPage ReferToTabPage;
		private ZArchitecture.ZTextBox SourceDetailsTextBox;
		private ZArchitecture.GUI.ZDropEditWithFixedWidth SourceTypeDropEditBox;
		private ZArchitecture.GUI.ZCodeFindBox LeadAssignedSalesRepCodeFindBox;
		private ZArchitecture.ZLabel AssignedSalesRepBranchCaption;
		private ZArchitecture.ZLabel AssignedSalesRepBranchCode;
		private ZArchitecture.ZLabel AssignedSalesRepBranchName;
		private ZArchitecture.GUI.ZGroupBox CompanyGroupBox;
		private ZArchitecture.GUI.ZCodeFindBox CompanyPortCountryCodeFindBox;
		private ZArchitecture.GUI.ZDropEditWithFixedWidth CompanyStateDropEdit;
		private ZArchitecture.ZTextBox CompanyPostCodeTextBox;
		private ZArchitecture.ZTextBox CompanyCityTextBox;
		private ZArchitecture.ZTextBox CompanyAddress2TextBox;
		private ZArchitecture.ZTextBox CompanyAddress1TextBox;
		internal ZArchitecture.ZTextBox CompanyNameTextBox;
		private ZArchitecture.GUI.ZButton OrgSearchButton;
		internal ZArchitecture.GUI.ZDropEditWithFixedWidth ContactDropEdit;
		internal ZArchitecture.GUI.ZDropEdit JobDropEdit;
		internal ZArchitecture.GUI.ZButton ViewSalesOpportunityButton;
		private PhoneNumberUserControl FaxNumberControl;
		private ZArchitecture.GUI.ZDateEdit OriginalCallDateEdit;
		private ZArchitecture.GUI.ZPanel scrollPanel;
		private ZArchitecture.GUI.ZDropEdit enquiryTypeDropEdit;
		private CargoWise.Windows.UI.KTableLayoutPanel CompanyGroupLayoutPanel;
		private ZArchitecture.GUI.ZPanel CompanyNamePanel;
		private ZArchitecture.GUI.ZPanel CompanyAddressCodePanel;
		private ZArchitecture.GUI.ZPanel CompanyGroupPanel3;
		protected ZArchitecture.GUI.ZGuidDropEdit CompanyAddressDropEdit;
		private ZArchitecture.GUI.ZGuidFindBox ReferringOrgGuidFindBox;
		private ZArchitecture.GUI.ZGroupBox LeadGroupBox;
		private ZArchitecture.GUI.ZDropEdit LeadInterestDropEdit;
		internal ZArchitecture.GUI.ZDropEditWithFixedWidth ReferringContactDropEdit;
		private ZArchitecture.GUI.ZGuidFindBox ReferToOrgGuidFindBox;
		internal ZArchitecture.GUI.ZDropEditWithFixedWidth ReferToContactDropEdit;
		internal ZArchitecture.GUI.ZPanel StatusButtonsGroupBox;
		public ZArchitecture.ZLabel StatusDescriptionLabel;
		private ZArchitecture.ZLabel zLabel1;
		internal ZArchitecture.GUI.ProcessTemplateCustomFieldsControl EnquiryCustomFieldsControl;
		private ZArchitecture.GUI.ZTemplateTabControl SalesEnquiryTabControl;
		private ZArchitecture.GUI.ZTabPage NotesTabPage;
		private ZArchitecture.GUI.ZTabPage CustomFieldsTabPage;
		private ZArchitecture.ZTextBox BusinessRegistrationTextbox;
		private ZArchitecture.ZTextBox WebsiteURLTextBox;
		private ZArchitecture.GUI.ZGroupBox relatedCommunicationGroupBox;
		private RelatedCommunicationGrid relatedCommunicationGrid;
		private ZArchitecture.GUI.ZTabPage SalesRelationsTabPage;
		private SalesRelationControl salesRelationControl;
		protected ZArchitecture.GUI.ZButton ValidateAddressButton;
		protected ZArchitecture.GUI.ZButton ClearFieldsButton;
	}
}
