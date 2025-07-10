namespace Enterprise.MasterFiles.GUI
{
	partial class CommunicationDetailsUserControl
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
			if (disposing && (components != null))
			{
				components.Dispose();
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
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.SummaryGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.OverallDispositionLabel = new Enterprise.ZArchitecture.ZLabel();
			this.StaffCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.StatusDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.LocationDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.SubjectTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.TypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CategoryDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.DatesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CancelInvitationButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.LastInvitationTimeLabel = new Enterprise.ZArchitecture.ZLabel();
			this.RegistryCalendarIntegrationLabel = new Enterprise.ZArchitecture.ZLabel();
			this.SendInvitationButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.DurationTimeEdit = new Enterprise.ZArchitecture.GUI.ZTimeEdit();
			this.ActualDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.ScheduleDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.ClientDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ContactTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ClientTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.contactPhoneDiallerUserControl = new Enterprise.MasterFiles.GUI.CommunicationFormPhoneDiallerUserControl();
			this.ContactGuidDropEdit = new Enterprise.ZArchitecture.GUI.ZGuidDropEdit();
			this.ClientGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.AttendeesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.AdditionalAttendeeGridTableLayoutPanel = new CargoWise.Windows.UI.KTableLayoutPanel();
			this.AdditionalAttendeesContactsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.AdditionalAttendeesOtherGrid = new Enterprise.ZArchitecture.ZGrid();
			this.AdditionalAttendeesStaffGrid = new Enterprise.ZArchitecture.ZGrid();
			this.IsReminderClientFacingCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SummaryGroupBox.SuspendLayout();
			this.StaffCodeFindBox.SuspendLayout();
			this.StatusDropEdit.SuspendLayout();
			this.LocationDropEdit.SuspendLayout();
			this.TypeDropEdit.SuspendLayout();
			this.CategoryDropEdit.SuspendLayout();
			this.DatesGroupBox.SuspendLayout();
			this.ActualDateEdit.SuspendLayout();
			this.ScheduleDateEdit.SuspendLayout();
			this.ClientDetailsGroupBox.SuspendLayout();
			this.contactPhoneDiallerUserControl.SuspendLayout();
			this.ContactGuidDropEdit.SuspendLayout();
			this.ClientGuidFindBox.SuspendLayout();
			this.AttendeesGroupBox.SuspendLayout();
			this.AdditionalAttendeeGridTableLayoutPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.AdditionalAttendeesContactsGrid)).BeginInit();
			this.AdditionalAttendeesContactsGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.AdditionalAttendeesOtherGrid)).BeginInit();
			this.AdditionalAttendeesOtherGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.AdditionalAttendeesStaffGrid)).BeginInit();
			this.AdditionalAttendeesStaffGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.OrgSalesCall);
			// 
			// SummaryGroupBox
			// 
			this.SummaryGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("48a1ade4-cbd5-4f71-8f75-1f088df1f544", "Summary");
			this.SummaryGroupBox.Controls.Add(this.OverallDispositionLabel);
			this.SummaryGroupBox.Controls.Add(this.StaffCodeFindBox);
			this.SummaryGroupBox.Controls.Add(this.StatusDropEdit);
			this.SummaryGroupBox.Controls.Add(this.LocationDropEdit);
			this.SummaryGroupBox.Controls.Add(this.SubjectTextBox);
			this.SummaryGroupBox.Controls.Add(this.TypeDropEdit);
			this.SummaryGroupBox.Controls.Add(this.CategoryDropEdit);
			this.SummaryGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 101, true);
			this.SummaryGroupBox.Name = "SummaryGroupBox";
			this.SummaryGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(439, 183, true);
			this.SummaryGroupBox.TabIndex = 1;
			this.SummaryGroupBox.TabStop = false;
			// 
			// OverallDispositionLabel
			// 
			this.OverallDispositionLabel.BackColor = System.Drawing.Color.Red;
			this.BindingSource.SetBindingMember(this.OverallDispositionLabel, "OverallDispositionDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgSalesCall)(null)).OverallDispositionDescription)));
			this.OverallDispositionLabel.ForeColor = System.Drawing.Color.White;
			this.OverallDispositionLabel.IsFontBold = true;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.OverallDispositionLabel, false);
			this.OverallDispositionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(347, 99, true);
			this.OverallDispositionLabel.Name = "OverallDispositionLabel";
			this.OverallDispositionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(78, 20, true);
			this.OverallDispositionLabel.TabIndex = 3;
			this.OverallDispositionLabel.Text = "[Disposition]";
			this.OverallDispositionLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// StaffCodeFindBox
			// 
			this.StaffCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.StaffCodeFindBox, "OQ_GS_NKSalesRep");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgSalesCall)(null)).OQ_GS_NKSalesRep)));
			this.StaffCodeFindBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("13afa1e2-6b67-40d2-9615-6b23078f1b78", "Staff Coordinator");
			this.StaffCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(105, 125, true);
			this.StaffCodeFindBox.Name = "StaffCodeFindBox";
			this.StaffCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(236, 17, true);
			this.StaffCodeFindBox.TabIndex = 4;
			// 
			// StatusDropEdit
			// 
			this.StatusDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.StatusDropEdit, "OQ_Status");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgSalesCall)(null)).OQ_Status)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgSalesCall)(null)).OQ_StatusDescription)));
			this.StatusDropEdit.BindToForDescription = "OQ_StatusDescription";
			this.StatusDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("793c0ad0-a23d-4c69-9cc3-24a7f1b9ae86", "Status");
			this.StatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(105, 99, true);
			this.StatusDropEdit.Name = "StatusDropEdit";
			this.StatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(236, 17, true);
			this.StatusDropEdit.TabIndex = 3;
			// 
			// LocationDropEdit
			// 
			this.LocationDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.LocationDropEdit, "Location");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgSalesCall)(null)).Location)));
			this.LocationDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("eb03d6c7-c23d-43f2-b03e-f6230dbbcd22", "Location");
			this.LocationDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LocationDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(105, 151, true);
			this.LocationDropEdit.Name = "LocationDropEdit";
			this.LocationDropEdit.PreBoundMaxLength = 50;
			this.LocationDropEdit.ShowDescriptionBox = false;
			this.LocationDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(318, 17, true);
			this.LocationDropEdit.TabIndex = 5;
			// 
			// SubjectTextBox
			// 
			this.BindingSource.SetBindingMember(this.SubjectTextBox, "OQ_CallSummary");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgSalesCall)(null)).OQ_CallSummary)));
			this.SubjectTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("60559d90-c7d3-4ebb-bbee-c24d17c53f98", "Subject");
			this.SubjectTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.SubjectTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(105, 73, true);
			this.SubjectTextBox.Name = "SubjectTextBox";
			this.SubjectTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 17, true);
			this.SubjectTextBox.TabIndex = 2;
			// 
			// TypeDropEdit
			// 
			this.TypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TypeDropEdit, "OQ_TypeOfCall");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgSalesCall)(null)).OQ_TypeOfCall)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgSalesCall)(null)).OQ_TypeOfCallDescription)));
			this.TypeDropEdit.BindToForDescription = "OQ_TypeOfCallDescription";
			this.TypeDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("6326bd8f-f95e-45ab-a361-68e9c379cfd2", "Method");
			this.TypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(105, 21, true);
			this.TypeDropEdit.Name = "TypeDropEdit";
			this.TypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(236, 17, true);
			this.TypeDropEdit.TabIndex = 0;
			// 
			// CategoryDropEdit
			// 
			this.CategoryDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CategoryDropEdit, "OQ_Category");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgSalesCall)(null)).OQ_Category)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgSalesCall)(null)).OQ_CategoryDescription)));
			this.CategoryDropEdit.BindToForDescription = "OQ_CategoryDescription";
			this.CategoryDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("B7BCD091-07B2-4E03-A047-FF3F4C28EEB8", "Purpose");
			this.CategoryDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(105, 47, true);
			this.CategoryDropEdit.Name = "CategoryDropEdit";
			this.CategoryDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(236, 17, true);
			this.CategoryDropEdit.TabIndex = 1;
			// 
			// DatesGroupBox
			// 
			this.DatesGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.DatesGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ff4e04ed-b456-4e4d-bab2-5cbd25d88f7c", "Dates");
			this.DatesGroupBox.Controls.Add(this.IsReminderClientFacingCheckBox);
			this.DatesGroupBox.Controls.Add(this.CancelInvitationButton);
			this.DatesGroupBox.Controls.Add(this.LastInvitationTimeLabel);
			this.DatesGroupBox.Controls.Add(this.RegistryCalendarIntegrationLabel);
			this.DatesGroupBox.Controls.Add(this.SendInvitationButton);
			this.DatesGroupBox.Controls.Add(this.DurationTimeEdit);
			this.DatesGroupBox.Controls.Add(this.ActualDateEdit);
			this.DatesGroupBox.Controls.Add(this.ScheduleDateEdit);
			this.DatesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(449, 4, true);
			this.DatesGroupBox.Name = "DatesGroupBox";
			this.DatesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(600, 93, true);
			this.DatesGroupBox.TabIndex = 2;
			this.DatesGroupBox.TabStop = false;
			// 
			// CancelInvitationButton
			// 
			this.CancelInvitationButton.AutoSize = true;
			this.CancelInvitationButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("66e72589-fd35-48b2-bfd6-8c89c0767d61", "Cancel");
			this.CancelInvitationButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(414, 18, true);
			this.CancelInvitationButton.Name = "CancelInvitationButton";
			this.CancelInvitationButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(66, 23, true);
			this.CancelInvitationButton.TabIndex = 5;
			this.CancelInvitationButton.UseVisualStyleBackColor = true;
			this.CancelInvitationButton.Click += new System.EventHandler(this.CancelInvitationButton_Click);
			// 
			// LastInvitationTimeLabel
			// 
			this.BindingSource.SetBindingMember(this.LastInvitationTimeLabel, "LastInvitationActionDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgSalesCall)(null)).LastInvitationActionDescription)));
			this.LastInvitationTimeLabel.ForeColor = System.Drawing.Color.Green;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.LastInvitationTimeLabel, false);
			this.LastInvitationTimeLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(300, 66, true);
			this.LastInvitationTimeLabel.Name = "LastInvitationTimeLabel";
			this.LastInvitationTimeLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 19, true);
			this.LastInvitationTimeLabel.TabIndex = 4;
			this.LastInvitationTimeLabel.Text = "[Invitation Time]";
			this.LastInvitationTimeLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			//  RegistryCalendarIntegrationLabel
			// 
			this.RegistryCalendarIntegrationLabel.ForeColor = System.Drawing.Color.Red;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.RegistryCalendarIntegrationLabel, false);
			this.RegistryCalendarIntegrationLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(500, 18, true);
			this.RegistryCalendarIntegrationLabel.Name = "RegistryCalendarIntegrationLabel";
			this.RegistryCalendarIntegrationLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(227, 70, true);
			this.RegistryCalendarIntegrationLabel.TabIndex = 4;
			this.RegistryCalendarIntegrationLabel.Text = "[Enable Registry Setting Calendar Integration]";
			this.RegistryCalendarIntegrationLabel.TextAlign = System.Drawing.ContentAlignment.TopLeft;
			this.RegistryCalendarIntegrationLabel.Visible = false;
			// 
			// SendInvitationButton
			// 
			this.SendInvitationButton.AutoSize = true;
			this.SendInvitationButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("6cd01189-d9e6-4a66-b148-15df48eca99d", "Send");
			this.SendInvitationButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(342, 18, true);
			this.SendInvitationButton.Name = "SendInvitationButton";
			this.SendInvitationButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(67, 23, true);
			this.SendInvitationButton.TabIndex = 4;
			this.SendInvitationButton.UseVisualStyleBackColor = true;
			this.SendInvitationButton.Click += new System.EventHandler(this.SendInvitationButton_Click);
			// 
			// DurationTimeEdit
			// 
			this.DurationTimeEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.DurationTimeEdit, "OQ_Duration");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.MasterFiles.Business.OrgSalesCall)(null)).OQ_Duration)));
			this.DurationTimeEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("a404c7ec-0895-41d1-92d0-af21bc2eb8bc", "Duration");
			this.DurationTimeEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.DurationTimeEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(300, 20, true);
			this.DurationTimeEdit.Name = "DurationTimeEdit";
			this.DurationTimeEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(35, 17, true);
			this.DurationTimeEdit.TabIndex = 2;
			// 
			// ActualDateEdit
			// 
			this.ActualDateEdit.AllowDrop = true;
			this.ActualDateEdit.AutoCompleteMonthThreshold = 1;
			this.ActualDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.ActualDateEdit, "OQ_CallDateLocal");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgSalesCall)(null)).OQ_CallDateLocal)));
			this.ActualDateEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("7cda824b-0dcf-4502-b559-ee0df2baedb0", "Actual Date");
			this.ActualDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.ActualDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 47, true);
			this.ActualDateEdit.Name = "ActualDateEdit";
			this.ActualDateEdit.TabIndex = 1;
			// 
			// ScheduleDateEdit
			// 
			this.ScheduleDateEdit.AllowDrop = true;
			this.ScheduleDateEdit.AutoCompleteMonthThreshold = 1;
			this.ScheduleDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.ScheduleDateEdit, "OQ_NextCallLocal");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgSalesCall)(null)).OQ_NextCallLocal)));
			this.ScheduleDateEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("f8587296-8917-4c62-8029-4169f45b48f2", "Scheduled Date");
			this.ScheduleDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.ScheduleDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 20, true);
			this.ScheduleDateEdit.Name = "ScheduleDateEdit";
			this.ScheduleDateEdit.TabIndex = 0;
			// 
			// ClientDetailsGroupBox
			// 
			this.ClientDetailsGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("fd7d377b-2981-412a-b952-ee0c646c63d7", "Client Details");
			this.ClientDetailsGroupBox.Controls.Add(this.ContactTextBox);
			this.ClientDetailsGroupBox.Controls.Add(this.ClientTextBox);
			this.ClientDetailsGroupBox.Controls.Add(this.contactPhoneDiallerUserControl);
			this.ClientDetailsGroupBox.Controls.Add(this.ContactGuidDropEdit);
			this.ClientDetailsGroupBox.Controls.Add(this.ClientGuidFindBox);
			this.ClientDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 4, true);
			this.ClientDetailsGroupBox.Name = "ClientDetailsGroupBox";
			this.ClientDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(438, 93, true);
			this.ClientDetailsGroupBox.TabIndex = 0;
			this.ClientDetailsGroupBox.TabStop = false;
			// 
			// ContactTextBox
			// 
			this.BindingSource.SetBindingMember(this.ContactTextBox, "LinkedInquiry.O1_ContactName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgSalesCall)(null)).LinkedInquiry.O1_ContactName)));
			this.ContactTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("2c03cd46-d9d8-429b-9f5e-91cdce3dfb24", "Primary Contact");
			this.ContactTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 47, true);
			this.ContactTextBox.Name = "ContactTextBox";
			this.ContactTextBox.ReadOnly = true;
			this.ContactTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 17, true);
			this.ContactTextBox.TabIndex = 1;
			this.ContactTextBox.Visible = false;
			// 
			// ClientTextBox
			// 
			this.BindingSource.SetBindingMember(this.ClientTextBox, "LinkedInquiry.O1_CompanyName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgSalesCall)(null)).LinkedInquiry.O1_CompanyName)));
			this.ClientTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("6cbd67d1-f668-463d-8368-c2dec4705dda", "Client");
			this.ClientTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 20, true);
			this.ClientTextBox.Name = "ClientTextBox";
			this.ClientTextBox.ReadOnly = true;
			this.ClientTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 17, true);
			this.ClientTextBox.TabIndex = 0;
			this.ClientTextBox.Visible = false;
			// 
			// contactPhoneDiallerUserControl
			// 
			this.contactPhoneDiallerUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.contactPhoneDiallerUserControl, ".");
			this.contactPhoneDiallerUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(319, 48, true);
			this.contactPhoneDiallerUserControl.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(85, 18, true);
			this.contactPhoneDiallerUserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(85, 18, true);
			this.contactPhoneDiallerUserControl.Name = "contactPhoneDiallerUserControl";
			this.contactPhoneDiallerUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(85, 18, true);
			this.contactPhoneDiallerUserControl.TabIndex = 2;
			// 
			// ContactGuidDropEdit
			// 
			this.ContactGuidDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ContactGuidDropEdit, "OQ_OC");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgSalesCall)(null)).OQ_OC)));
			this.ContactGuidDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("51e9dd2b-70f6-4e5d-a67e-e23971184778", "Primary Contact");
			this.ContactGuidDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 47, true);
			this.ContactGuidDropEdit.Name = "ContactGuidDropEdit";
			this.ContactGuidDropEdit.PreBoundMaxLength = 30;
			this.ContactGuidDropEdit.ShowDescriptionBox = false;
			this.ContactGuidDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(199, 17, true);
			this.ContactGuidDropEdit.TabIndex = 1;
			// 
			// ClientGuidFindBox
			// 
			this.ClientGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ClientGuidFindBox, "OQ_OH");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OrgSalesCall)(null)).OQ_OH)));
			this.ClientGuidFindBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("0f843937-74d0-4f16-8562-b6a7defcc14f", "Client");
			this.ClientGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 20, true);
			this.ClientGuidFindBox.Name = "ClientGuidFindBox";
			this.ClientGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 17, true);
			this.ClientGuidFindBox.TabIndex = 0;
			// 
			// AttendeesGroupBox
			// 
			this.AttendeesGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.AttendeesGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("370b499e-d478-4fa6-ae89-e989b8ad150c", "Attendees");
			this.AttendeesGroupBox.Controls.Add(this.AdditionalAttendeeGridTableLayoutPanel);
			this.AttendeesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(449, 101, true);
			this.AttendeesGroupBox.Name = "AttendeesGroupBox";
			this.AttendeesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(489, 183, true);
			this.AttendeesGroupBox.TabIndex = 3;
			this.AttendeesGroupBox.TabStop = false;
			// 
			// AdditionalAttendeeGridTableLayoutPanel
			// 
			this.AdditionalAttendeeGridTableLayoutPanel.ColumnCount = 3;
			this.AdditionalAttendeeGridTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.3F));
			this.AdditionalAttendeeGridTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.3F));
			this.AdditionalAttendeeGridTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.4F));
			this.AdditionalAttendeeGridTableLayoutPanel.Controls.Add(this.AdditionalAttendeesContactsGrid, 0, 0);
			this.AdditionalAttendeeGridTableLayoutPanel.Controls.Add(this.AdditionalAttendeesOtherGrid, 2, 0);
			this.AdditionalAttendeeGridTableLayoutPanel.Controls.Add(this.AdditionalAttendeesStaffGrid, 1, 0);
			this.AdditionalAttendeeGridTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AdditionalAttendeeGridTableLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
			this.AdditionalAttendeeGridTableLayoutPanel.Name = "AdditionalAttendeeGridTableLayoutPanel";
			this.AdditionalAttendeeGridTableLayoutPanel.RowCount = 1;
			this.AdditionalAttendeeGridTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.AdditionalAttendeeGridTableLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(485, 167, true);
			this.AdditionalAttendeeGridTableLayoutPanel.TabIndex = 0;
			// 
			// AdditionalAttendeesContactsGrid
			// 
			this.AdditionalAttendeesContactsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.AdditionalAttendeesContactsGrid, "AdditionalAttendeesContact");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSalesCall)(null)).AdditionalAttendeesContact)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgSalesCallAdditionalAttendee)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSalesCall)(null)).AdditionalAttendeesContact)).SyncRoot)).O6_ReceiverReminder)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OrgSalesCallAdditionalAttendee)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSalesCall)(null)).AdditionalAttendeesContact)).SyncRoot)).O6_AttendeeID)));
			this.AdditionalAttendeesContactsGrid.CaptionVisible = false;
			zCheckBoxColumnStyleInfo3.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("e6ae5f39-6a1f-472a-9976-6ae33b263f28", "Reminder");
			zCheckBoxColumnStyleInfo3.ColumnName = "O6_ReceiverReminder";
			zCheckBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo3.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("9b544069-c075-4a38-8b5d-d2936ba03545", "Contact");
			zGuidFindBoxColumnStyleInfo3.ColumnName = "O6_AttendeeID";
			zGuidFindBoxColumnStyleInfo3.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.OrgContacts;
			zGuidFindBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.AdditionalAttendeesContactsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo3);
			this.AdditionalAttendeesContactsGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo3);
			this.AdditionalAttendeesContactsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AdditionalAttendeesContactsGrid.GridId = "d4100191-b133-4cf6-9abd-24891f491eec";
			this.AdditionalAttendeesContactsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.AdditionalAttendeesContactsGrid.LayoutKey = "AdditionalAttendeesContactsGrid";
			this.AdditionalAttendeesContactsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 2, true);
			this.AdditionalAttendeesContactsGrid.Name = "AdditionalAttendeesContactsGrid";
			this.AdditionalAttendeesContactsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(157, 163, true);
			this.AdditionalAttendeesContactsGrid.TabIndex = 0;
			// 
			// AdditionalAttendeesOtherGrid
			// 
			this.AdditionalAttendeesOtherGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.AdditionalAttendeesOtherGrid, "AdditionalAttendeesOther");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSalesCall)(null)).AdditionalAttendeesOther)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgSalesCallAdditionalAttendee)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSalesCall)(null)).AdditionalAttendeesOther)).SyncRoot)).O6_AttendeeName)));
			this.AdditionalAttendeesOtherGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("5e3e9b0f-928b-4024-b745-22124cdcc5b8", "Other Attendee");
			zTextBoxColumnStyleInfo3.ColumnName = "O6_AttendeeName";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.AdditionalAttendeesOtherGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.AdditionalAttendeesOtherGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AdditionalAttendeesOtherGrid.GridId = "5d60d486-9b14-4433-9669-4d1cb7bc22d7";
			this.AdditionalAttendeesOtherGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.AdditionalAttendeesOtherGrid.LayoutKey = "AdditionalAttendeesOtherGrid";
			this.AdditionalAttendeesOtherGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(325, 2, true);
			this.AdditionalAttendeesOtherGrid.Name = "AdditionalAttendeesOtherGrid";
			this.AdditionalAttendeesOtherGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(158, 163, true);
			this.AdditionalAttendeesOtherGrid.TabIndex = 2;
			// 
			// AdditionalAttendeesStaffGrid
			// 
			this.AdditionalAttendeesStaffGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.AdditionalAttendeesStaffGrid, "AdditionalAttendeesStaff");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSalesCall)(null)).AdditionalAttendeesStaff)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgSalesCallAdditionalAttendee)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSalesCall)(null)).AdditionalAttendeesStaff)).SyncRoot)).O6_ReceiverReminder)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OrgSalesCallAdditionalAttendee)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSalesCall)(null)).AdditionalAttendeesStaff)).SyncRoot)).O6_AttendeeID)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSalesCallAdditionalAttendee)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSalesCall)(null)).AdditionalAttendeesStaff)).SyncRoot)).Lookups.Staff)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgSalesCallAdditionalAttendee)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSalesCall)(null)).AdditionalAttendeesStaff)).SyncRoot)).Name)));
			this.AdditionalAttendeesStaffGrid.CaptionVisible = false;
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ff89fa78-5229-4390-9751-eac6983e3048", "Reminder");
			zCheckBoxColumnStyleInfo1.ColumnName = "O6_ReceiverReminder";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo1.BindToList = "Lookups.Staff";
			zGuidFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("2958cba4-05b9-4f1e-b366-5f4d3b9ba169", "Staff");
			zGuidFindBoxColumnStyleInfo1.ColumnName = "O6_AttendeeID";
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("feef0bff-687f-4648-9c26-e591971b5610", "Name");
			zTextBoxColumnStyleInfo1.ColumnName = "Name";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.AdditionalAttendeesStaffGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.AdditionalAttendeesStaffGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.AdditionalAttendeesStaffGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.AdditionalAttendeesStaffGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AdditionalAttendeesStaffGrid.GridId = "c05bab2e-19a0-4a0d-8f54-11e27c0c8f28";
			this.AdditionalAttendeesStaffGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.AdditionalAttendeesStaffGrid.LayoutKey = "AdditionalAttendeesStaffGrid";
			this.AdditionalAttendeesStaffGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(163, 2, true);
			this.AdditionalAttendeesStaffGrid.Name = "AdditionalAttendeesStaffGrid";
			this.AdditionalAttendeesStaffGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(157, 163, true);
			this.AdditionalAttendeesStaffGrid.TabIndex = 1;
			// 
			// IsReminderClientFacingCheckBox
			// 
			this.BindingSource.SetBindingMember(this.IsReminderClientFacingCheckBox, "OQ_IsReminderClientFacing");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgSalesCall)(null)).OQ_IsReminderClientFacing)));
			this.IsReminderClientFacingCheckBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("7ff4faf2-0c5f-432d-8439-24ac460fafb6", "Client Visible Invitation");
			this.IsReminderClientFacingCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsReminderClientFacingCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(300, 48, true);
			this.IsReminderClientFacingCheckBox.Name = "IsReminderClientFacingCheckBox";
			this.IsReminderClientFacingCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(180, 16, true);
			this.IsReminderClientFacingCheckBox.TabIndex = 3;
			this.IsReminderClientFacingCheckBox.UseVisualStyleBackColor = true;
			// 
			// CommunicationDetailsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.AttendeesGroupBox);
			this.Controls.Add(this.ClientDetailsGroupBox);
			this.Controls.Add(this.DatesGroupBox);
			this.Controls.Add(this.SummaryGroupBox);
			this.Name = "CommunicationDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1050, 285, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.SummaryGroupBox.ResumeLayout(false);
			this.SummaryGroupBox.PerformLayout();
			this.StaffCodeFindBox.ResumeLayout(true);
			this.StaffCodeFindBox.PerformLayout();
			this.StatusDropEdit.ResumeLayout(true);
			this.StatusDropEdit.PerformLayout();
			this.LocationDropEdit.ResumeLayout(true);
			this.LocationDropEdit.PerformLayout();
			this.TypeDropEdit.ResumeLayout(true);
			this.TypeDropEdit.PerformLayout();
			this.CategoryDropEdit.ResumeLayout(true);
			this.CategoryDropEdit.PerformLayout();
			this.DatesGroupBox.ResumeLayout(false);
			this.DatesGroupBox.PerformLayout();
			this.ActualDateEdit.ResumeLayout(true);
			this.ActualDateEdit.PerformLayout();
			this.ScheduleDateEdit.ResumeLayout(true);
			this.ScheduleDateEdit.PerformLayout();
			this.ClientDetailsGroupBox.ResumeLayout(false);
			this.ClientDetailsGroupBox.PerformLayout();
			this.contactPhoneDiallerUserControl.ResumeLayout(true);
			this.contactPhoneDiallerUserControl.PerformLayout();
			this.ContactGuidDropEdit.ResumeLayout(true);
			this.ContactGuidDropEdit.PerformLayout();
			this.ClientGuidFindBox.ResumeLayout(true);
			this.ClientGuidFindBox.PerformLayout();
			this.AttendeesGroupBox.ResumeLayout(false);
			this.AttendeesGroupBox.PerformLayout();
			this.AdditionalAttendeeGridTableLayoutPanel.ResumeLayout(false);
			this.AdditionalAttendeeGridTableLayoutPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.AdditionalAttendeesContactsGrid)).EndInit();
			this.AdditionalAttendeesContactsGrid.ResumeLayout(false);
			this.AdditionalAttendeesContactsGrid.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.AdditionalAttendeesOtherGrid)).EndInit();
			this.AdditionalAttendeesOtherGrid.ResumeLayout(false);
			this.AdditionalAttendeesOtherGrid.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.AdditionalAttendeesStaffGrid)).EndInit();
			this.AdditionalAttendeesStaffGrid.ResumeLayout(false);
			this.AdditionalAttendeesStaffGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox SummaryGroupBox;
		private ZArchitecture.GUI.ZGroupBox DatesGroupBox;
		internal ZArchitecture.GUI.ZGroupBox ClientDetailsGroupBox;
		private ZArchitecture.GUI.ZGroupBox AttendeesGroupBox;
		private ZArchitecture.ZTextBox SubjectTextBox;
		private ZArchitecture.GUI.ZDropEdit TypeDropEdit;
		private ZArchitecture.GUI.ZDropEdit CategoryDropEdit;
		private ZArchitecture.GUI.ZDropEdit LocationDropEdit;
		private ZArchitecture.GUI.ZDropEdit StatusDropEdit;
		private ZArchitecture.GUI.ZCodeFindBox StaffCodeFindBox;
		internal ZArchitecture.GUI.ZGuidDropEdit ContactGuidDropEdit;
		internal ZArchitecture.GUI.ZGuidFindBox ClientGuidFindBox;
		private ZArchitecture.GUI.ZDateEdit ActualDateEdit;
		private ZArchitecture.GUI.ZDateEdit ScheduleDateEdit;
		private ZArchitecture.ZGrid AdditionalAttendeesOtherGrid;
		private ZArchitecture.ZGrid AdditionalAttendeesStaffGrid;
		private ZArchitecture.ZGrid AdditionalAttendeesContactsGrid;
		private ZArchitecture.GUI.ZTimeEdit DurationTimeEdit;
		internal ZArchitecture.GUI.ZButton SendInvitationButton;
		private ZArchitecture.ZLabel OverallDispositionLabel;
		internal CargoWise.Windows.UI.KTableLayoutPanel AdditionalAttendeeGridTableLayoutPanel;
		internal ZArchitecture.ZLabel LastInvitationTimeLabel;
		internal ZArchitecture.ZLabel RegistryCalendarIntegrationLabel;
		private ZArchitecture.GUI.ZButton CancelInvitationButton;
		private CommunicationFormPhoneDiallerUserControl contactPhoneDiallerUserControl;
		internal ZArchitecture.ZTextBox ClientTextBox;
		internal ZArchitecture.ZTextBox ContactTextBox;
		private ZArchitecture.GUI.ZCheckBox IsReminderClientFacingCheckBox;
	}
}
