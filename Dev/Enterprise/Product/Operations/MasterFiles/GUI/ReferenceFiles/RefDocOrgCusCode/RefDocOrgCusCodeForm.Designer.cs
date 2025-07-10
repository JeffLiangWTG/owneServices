namespace Enterprise.MasterFiles.GUI
{
	partial class RefDocOrgCusCodeForm
	{
		private System.ComponentModel.IContainer components = null;

		protected override void Dispose(bool disposing)
		{
			if (disposing && components != null)
			{
				components.Dispose();
			}

			base.Dispose(disposing);
		}

		protected override void InitializeComponent()
		{
			this.MainPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.NotesTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.RegulatingCountryDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.RegistrationNumberCountryDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.OrganizationCodeAndDescriptionDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.DocumentTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.DirectionDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.DocumentRegistrationNumberShortLabelTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DocumentRegistrationNumberLongLabelTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DocumentRegistrationNumberDescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.PriorityTextBox = new Enterprise.ZArchitecture.GUI.ZNumericUpDown();
			this.PostingButtonsUserControl = new Enterprise.Core.Forms.ZPostingButtonsUserControl();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.MainPanel.SuspendLayout();
			this.RegulatingCountryDropEdit.SuspendLayout();
			this.RegistrationNumberCountryDropEdit.SuspendLayout();
			this.OrganizationCodeAndDescriptionDropEdit.SuspendLayout();
			this.DocumentTypeDropEdit.SuspendLayout();
			this.DirectionDropEdit.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.PriorityTextBox)).BeginInit();
			this.PriorityTextBox.SuspendLayout();
			this.PostingButtonsUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 257, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(684, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.RefDocOrgCusCode);
			// 
			// MainPanel
			// 
			this.MainPanel.BackColor = System.Drawing.SystemColors.Control;
			this.MainPanel.Controls.Add(this.NotesTextBox);
			this.MainPanel.Controls.Add(this.RegulatingCountryDropEdit);
			this.MainPanel.Controls.Add(this.RegistrationNumberCountryDropEdit);
			this.MainPanel.Controls.Add(this.OrganizationCodeAndDescriptionDropEdit);
			this.MainPanel.Controls.Add(this.DocumentTypeDropEdit);
			this.MainPanel.Controls.Add(this.DirectionDropEdit);
			this.MainPanel.Controls.Add(this.DocumentRegistrationNumberShortLabelTextBox);
			this.MainPanel.Controls.Add(this.DocumentRegistrationNumberLongLabelTextBox);
			this.MainPanel.Controls.Add(this.DocumentRegistrationNumberDescriptionTextBox);
			this.MainPanel.Controls.Add(this.PriorityTextBox);
			this.MainPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MainPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MainPanel.Name = "MainPanel";
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(684, 232, true);
			this.MainPanel.TabIndex = 0;
			// 
			// NotesTextBox
			// 
			this.BindingSource.SetBindingMember(this.NotesTextBox, "DOC_Notes");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefDocOrgCusCode)(null)).DOC_Notes)));
			this.NotesTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefDocOrgCusCodeForm|e841b068-7f24-4f1c-b280-d24a46790a15", "Comments");
			this.NotesTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(481, 133, true);
			this.NotesTextBox.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, 10, 3, 3, true);
			this.NotesTextBox.Name = "NotesTextBox";
			this.NotesTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(180, 20, true);
			this.NotesTextBox.TabIndex = 8;
			// 
			// RegulatingCountryDropEdit
			// 
			this.RegulatingCountryDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.RegulatingCountryDropEdit, "DOC_RN_NKRegulatingCountry");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.RefDocOrgCusCode)(null)).DOC_RN_NKRegulatingCountry)));
			this.RegulatingCountryDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefDocOrgCusCodeForm|1ca3a016-b855-4610-aabc-3e8cc0f9ddd7", "Regulating Country/Region");
			this.RegulatingCountryDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(146, 19, true);
			this.RegulatingCountryDropEdit.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, 10, 3, 3, true);
			this.RegulatingCountryDropEdit.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(125, 25, true);
			this.RegulatingCountryDropEdit.Name = "RegulatingCountryDropEdit";
			this.RegulatingCountryDropEdit.ShouldResizeByMaxLength = true;
			this.RegulatingCountryDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(180, 25, true);
			this.RegulatingCountryDropEdit.TabIndex = 1;
			// 
			// RegistrationNumberCountryDropEdit
			// 
			this.RegistrationNumberCountryDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.RegistrationNumberCountryDropEdit, "DOC_RN_NKCodeCountry");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.RefDocOrgCusCode)(null)).DOC_RN_NKCodeCountry)));
			this.RegistrationNumberCountryDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefDocOrgCusCodeForm|4e0c9a0f-21e2-41eb-bb91-1540b10d2fc9", "Registration No. Ctry/Rgn.", "Registration Number Country/Region");
			this.RegistrationNumberCountryDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(146, 57, true);
			this.RegistrationNumberCountryDropEdit.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, 10, 3, 3, true);
			this.RegistrationNumberCountryDropEdit.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(125, 25, true);
			this.RegistrationNumberCountryDropEdit.Name = "RegistrationNumberCountryDropEdit";
			this.RegistrationNumberCountryDropEdit.ShouldResizeByMaxLength = true;
			this.RegistrationNumberCountryDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(180, 25, true);
			this.RegistrationNumberCountryDropEdit.TabIndex = 2;
			// 
			// OrganizationCodeAndDescriptionDropEdit
			// 
			this.OrganizationCodeAndDescriptionDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OrganizationCodeAndDescriptionDropEdit, "DOC_CodeType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.RefDocOrgCusCode)(null)).DOC_CodeType)));
			this.OrganizationCodeAndDescriptionDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefDocOrgCusCodeForm|a2ef74ea-5433-476a-8ab4-09ae6bfe6695", "CargoWise Organization");
			this.OrganizationCodeAndDescriptionDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(146, 95, true);
			this.OrganizationCodeAndDescriptionDropEdit.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, 10, 3, 3, true);
			this.OrganizationCodeAndDescriptionDropEdit.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(125, 25, true);
			this.OrganizationCodeAndDescriptionDropEdit.Name = "OrganizationCodeAndDescriptionDropEdit";
			this.OrganizationCodeAndDescriptionDropEdit.ShouldResizeByMaxLength = true;
			this.OrganizationCodeAndDescriptionDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(180, 25, true);
			this.OrganizationCodeAndDescriptionDropEdit.TabIndex = 3;
			// 
			// DocumentTypeDropEdit
			// 
			this.DocumentTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DocumentTypeDropEdit, "DOC_DocumentType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.RefDocOrgCusCode)(null)).DOC_DocumentType)));
			this.DocumentTypeDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefDocOrgCusCodeForm|ff54d991-b305-44a1-961b-22e776f1e4d3", "Document Type");
			this.DocumentTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(146, 133, true);
			this.DocumentTypeDropEdit.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, 10, 3, 3, true);
			this.DocumentTypeDropEdit.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(125, 25, true);
			this.DocumentTypeDropEdit.Name = "DocumentTypeDropEdit";
			this.DocumentTypeDropEdit.ShouldResizeByMaxLength = true;
			this.DocumentTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(180, 25, true);
			this.DocumentTypeDropEdit.TabIndex = 4;
			// 
			// DocumentRegistrationNumberShortLabelTextBox
			// 
			this.BindingSource.SetBindingMember(this.DocumentRegistrationNumberShortLabelTextBox, "DOC_ShortLabel");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefDocOrgCusCode)(null)).DOC_ShortLabel)));
			this.DocumentRegistrationNumberShortLabelTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefDocOrgCusCodeForm|f9a323b2-7785-4401-8707-bebd5b4e6c2b", "Registration Short Label", "Document Registration Number Short Label");
			this.DocumentRegistrationNumberShortLabelTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(481, 19, true);
			this.DocumentRegistrationNumberShortLabelTextBox.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, 10, 3, 3, true);
			this.DocumentRegistrationNumberShortLabelTextBox.Name = "DocumentRegistrationNumberShortLabelTextBox";
			this.DocumentRegistrationNumberShortLabelTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(180, 20, true);
			this.DocumentRegistrationNumberShortLabelTextBox.TabIndex = 5;
			// 
			// DocumentRegistrationNumberLongLabelTextBox
			// 
			this.BindingSource.SetBindingMember(this.DocumentRegistrationNumberLongLabelTextBox, "DOC_LongLabel");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefDocOrgCusCode)(null)).DOC_LongLabel)));
			this.DocumentRegistrationNumberLongLabelTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("27cb73fb-6cd0-4365-a801-eac8f9bb9d04", "Registration Long Label", "Document Registration Number Long Label");
			this.DocumentRegistrationNumberLongLabelTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(481, 57, true);
			this.DocumentRegistrationNumberLongLabelTextBox.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, 10, 3, 3, true);
			this.DocumentRegistrationNumberLongLabelTextBox.Name = "DocumentRegistrationNumberLongLabelTextBox";
			this.DocumentRegistrationNumberLongLabelTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(180, 20, true);
			this.DocumentRegistrationNumberLongLabelTextBox.TabIndex = 6;
			// 
			// DocumentRegistrationNumberDescriptionTextBox
			// 
			this.BindingSource.SetBindingMember(this.DocumentRegistrationNumberDescriptionTextBox, "DOC_Description");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefDocOrgCusCode)(null)).DOC_Description)));
			this.DocumentRegistrationNumberDescriptionTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("5ca84b86-20c6-45e6-a800-fd54223914db", "Registration Description", "Document Registration Number Description");
			this.DocumentRegistrationNumberDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(481, 95, true);
			this.DocumentRegistrationNumberDescriptionTextBox.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, 10, 3, 3, true);
			this.DocumentRegistrationNumberDescriptionTextBox.Name = "DocumentRegistrationNumberDescriptionTextBox";
			this.DocumentRegistrationNumberDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(180, 20, true);
			this.DocumentRegistrationNumberDescriptionTextBox.TabIndex = 7;
			// 
			// DirectionDropEdit
			// 
			this.DirectionDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DirectionDropEdit, "DOC_Direction");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.RefDocOrgCusCode)(null)).DOC_Direction)));
			this.DirectionDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefDocOrgCusCodeForm|39E07057-41CE-430F-ADB1-224BA0EA1013", "Direction");
			this.DirectionDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(146, 171, true);
			this.DirectionDropEdit.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, 10, 3, 3, true);
			this.DirectionDropEdit.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(125, 25, true);
			this.DirectionDropEdit.Name = "DirectionDropEdit";
			this.DirectionDropEdit.ShouldResizeByMaxLength = true;
			this.DirectionDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(180, 25, true);
			this.DirectionDropEdit.TabIndex = 9;
			// 
			// PriorityTextBox
			// 
			this.BindingSource.SetBindingMember(this.PriorityTextBox, "DOC_Priority");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((decimal)(((Enterprise.MasterFiles.Business.RefDocOrgCusCode)(null)).DOC_Priority)));
			this.PriorityTextBox.BindTo = "DOC_Priority";
			this.PriorityTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefDocOrgCusCodeForm|84b782af-6df0-4df2-999e-85cd1a5b952f", "Priority");
			this.PriorityTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(481, 171, true);
			this.PriorityTextBox.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, 10, 3, 10, true);
			this.PriorityTextBox.Name = "PriorityTextBox";
			this.PriorityTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(43, 20, true);
			this.PriorityTextBox.TabIndex = 10;
			// 
			// PostingButtonsUserControl
			// 
			this.PostingButtonsUserControl.AllowDrop = true;
			this.PostingButtonsUserControl.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.PostingButtonsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 232, true);
			this.PostingButtonsUserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.PostingButtonsUserControl.Name = "PostingButtonsUserControl";
			this.PostingButtonsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(684, 25, true);
			this.PostingButtonsUserControl.TabIndex = 0;
			// 
			// RefDocOrgCusCodeForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefDocOrgCusCodeForm|5384af0d-995e-4ebd-9efe-0d2fe6a9debc", "Document Organization Registration Mapping");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(684, 281, true);
			this.Controls.Add(this.MainPanel);
			this.Controls.Add(this.PostingButtonsUserControl);
			this.DataSourceType = typeof(Enterprise.MasterFiles.Business.RefDocOrgCusCode);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(700, 320, true);
			this.Name = "RefDocOrgCusCodeForm";
			this.ShouldSerializeTabPageMethods = false;
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.PostingButtonsUserControl, 0);
			this.Controls.SetChildIndex(this.MainPanel, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MainPanel.ResumeLayout(false);
			this.MainPanel.PerformLayout();
			this.RegulatingCountryDropEdit.ResumeLayout(true);
			this.RegulatingCountryDropEdit.PerformLayout();
			this.RegistrationNumberCountryDropEdit.ResumeLayout(true);
			this.RegistrationNumberCountryDropEdit.PerformLayout();
			this.OrganizationCodeAndDescriptionDropEdit.ResumeLayout(true);
			this.OrganizationCodeAndDescriptionDropEdit.PerformLayout();
			this.DocumentTypeDropEdit.ResumeLayout(true);
			this.DocumentTypeDropEdit.PerformLayout();
			this.DirectionDropEdit.ResumeLayout(true);
			this.DirectionDropEdit.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.PriorityTextBox)).EndInit();
			this.PriorityTextBox.ResumeLayout(false);
			this.PriorityTextBox.PerformLayout();
			this.PostingButtonsUserControl.ResumeLayout(true);
			this.PostingButtonsUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		ZArchitecture.GUI.ZPanel MainPanel;

		Enterprise.ZArchitecture.GUI.ZDropEdit RegulatingCountryDropEdit;
		Enterprise.ZArchitecture.GUI.ZDropEdit RegistrationNumberCountryDropEdit;
		Enterprise.ZArchitecture.GUI.ZDropEdit OrganizationCodeAndDescriptionDropEdit;
		ZArchitecture.GUI.ZDropEdit DocumentTypeDropEdit;
		ZArchitecture.GUI.ZDropEdit DirectionDropEdit;

		ZArchitecture.ZTextBox DocumentRegistrationNumberShortLabelTextBox;
		Enterprise.ZArchitecture.ZTextBox DocumentRegistrationNumberLongLabelTextBox;
		ZArchitecture.ZTextBox DocumentRegistrationNumberDescriptionTextBox;
		ZArchitecture.GUI.ZNumericUpDown PriorityTextBox;

		Enterprise.Core.Forms.ZPostingButtonsUserControl PostingButtonsUserControl;
		private ZArchitecture.ZTextBox NotesTextBox;
	}
}
