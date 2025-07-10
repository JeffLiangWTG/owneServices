namespace Enterprise.MasterFiles.GUI
{
	public partial class EmailToContactUserControl
	{
		#region Windows Form Designer generated code

		void InitializeComponent()
		{
			this.FromDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ToTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CcTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.SubjectTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.FromEmailAddressDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.BodyGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.BodyTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.AttachmentsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.AttachmentsChoosePanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.AttachmentsDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.AttachmentsStatusPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.NumberOfAttachmentsLabel = new Enterprise.ZArchitecture.ZLabel();
			this.RemoveAttachmentButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.AddAttachmentButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.PriorityDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.UseCurrentUserNameCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.UseCurrentUserEmailCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.CcButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ToButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.FromDropEdit.SuspendLayout();
			this.FromEmailAddressDropEdit.SuspendLayout();
			this.BodyGroupBox.SuspendLayout();
			this.AttachmentsGroupBox.SuspendLayout();
			this.AttachmentsChoosePanel.SuspendLayout();
			this.AttachmentsDropEdit.SuspendLayout();
			this.AttachmentsStatusPanel.SuspendLayout();
			this.PriorityDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.EmailToContactBusinessObject);
			// 
			// FromDropEdit
			// 
			this.FromDropEdit.AllowDrop = true;
			this.FromDropEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.FromDropEdit, "FromDisplayName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.EmailToContactBusinessObject)(null)).FromDisplayName)));
			this.FromDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("EmailToContactUserControl|52235b53-587b-4111-840f-c4431d6c352b", "From (Display Name)");
			this.FromDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.FromDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 32, true);
			this.FromDropEdit.Name = "FromDropEdit";
			this.FromDropEdit.ShowDescriptionBox = false;
			this.FromDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			this.FromDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 17, true);
			this.FromDropEdit.TabIndex = 2;
			this.FromDropEdit.UseFullWidthForCodeBox = true;
			// 
			// ToTextBox
			// 
			this.ToTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ToTextBox, "ToEmailAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.EmailToContactBusinessObject)(null)).ToEmailAddress)));
			this.ToTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("EmailToContactUserControl|83e55525-5578-472d-8f2d-121ce8d768b2", "To", "To (Email Address)", "Separate email addresses with a semi-colon (;).");
			this.ToTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ToTextBox.Enabled = false;
			this.ToTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(79, 56, true);
			this.ToTextBox.Name = "ToTextBox";
			this.ToTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(649, 17, true);
			this.ToTextBox.TabIndex = 12;
			// 
			// CcTextBox
			// 
			this.CcTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.CcTextBox, "Cc");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.EmailToContactBusinessObject)(null)).Cc)));
			this.CcTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("EmailToContactUserControl|568cc2b2-bd58-4268-be27-3a9d732b43bc", "Cc", "Separate email addresses with a semi-colon (;)");
			this.CcTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.CcTextBox.Enabled = false;
			this.CcTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(79, 80, true);
			this.CcTextBox.Name = "CcTextBox";
			this.CcTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(649, 17, true);
			this.CcTextBox.TabIndex = 13;
			// 
			// SubjectTextBox
			// 
			this.BindingSource.SetBindingMember(this.SubjectTextBox, "Subject");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.EmailToContactBusinessObject)(null)).Subject)));
			this.SubjectTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("EmailToContactUserControl|ac963180-9ac0-451e-b469-33d185a000b5", "Subject");
			this.SubjectTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.SubjectTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(56, 104, true);
			this.SubjectTextBox.Name = "SubjectTextBox";
			this.SubjectTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(440, 17, true);
			this.SubjectTextBox.TabIndex = 9;
			// 
			// FromEmailAddressDropEdit
			// 
			this.FromEmailAddressDropEdit.AllowDrop = true;
			this.FromEmailAddressDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.FromEmailAddressDropEdit, "FromEmailAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.EmailToContactBusinessObject)(null)).FromEmailAddress)));
			this.FromEmailAddressDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("EmailToContactUserControl|0fccf46b-a4df-4e5e-b77c-242fb2485fc9", "From (Email Address)");
			this.FromEmailAddressDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.FromEmailAddressDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(488, 32, true);
			this.FromEmailAddressDropEdit.Name = "FromEmailAddressDropEdit";
			this.FromEmailAddressDropEdit.ShowDescriptionBox = false;
			this.FromEmailAddressDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			this.FromEmailAddressDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 17, true);
			this.FromEmailAddressDropEdit.TabIndex = 3;
			this.FromEmailAddressDropEdit.UseFullWidthForCodeBox = true;
			// 
			// BodyGroupBox
			// 
			this.BodyGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
			| System.Windows.Forms.AnchorStyles.Left) 
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BodyGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("EmailToContactUserControl|c32529d6-09e4-4610-a92a-5b087f626974", "Body");
			this.BodyGroupBox.Controls.Add(this.BodyTextBox);
			this.BodyGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 130, true);
			this.BodyGroupBox.Name = "BodyGroupBox";
			this.BodyGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(728, 326, true);
			this.BodyGroupBox.TabIndex = 10;
			this.BodyGroupBox.TabStop = false;
			// 
			// BodyTextBox
			// 
			this.BodyTextBox.AcceptsReturn = true;
			this.BodyTextBox.AcceptsTab = true;
			this.BodyTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
			| System.Windows.Forms.AnchorStyles.Left) 
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.BodyTextBox, "Body");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.EmailToContactBusinessObject)(null)).Body)));
			this.BodyTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("EmailToContactUserControl|fcebe68c-73f3-42c5-b268-656eab911a3c", "Body");
			this.BodyTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.BodyTextBox, false);
			this.BodyTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 16, true);
			this.BodyTextBox.Multiline = true;
			this.BodyTextBox.Name = "BodyTextBox";
			this.BodyTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.BodyTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(712, 302, true);
			this.BodyTextBox.TabIndex = 0;
			// 
			// AttachmentsGroupBox
			// 
			this.AttachmentsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
			| System.Windows.Forms.AnchorStyles.Right)));
			this.AttachmentsGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("EmailToContactUserControl|94fba8d6-e6a9-49ee-96f2-3308662ac990", "Attachments");
			this.AttachmentsGroupBox.Controls.Add(this.AttachmentsChoosePanel);
			this.AttachmentsGroupBox.Controls.Add(this.AttachmentsStatusPanel);
			this.AttachmentsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 464, true);
			this.AttachmentsGroupBox.Name = "AttachmentsGroupBox";
			this.AttachmentsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(728, 56, true);
			this.AttachmentsGroupBox.TabIndex = 11;
			this.AttachmentsGroupBox.TabStop = false;
			// 
			// AttachmentsChoosePanel
			// 
			this.AttachmentsChoosePanel.Controls.Add(this.AttachmentsDropEdit);
			this.AttachmentsChoosePanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 16, true);
			this.AttachmentsChoosePanel.Name = "AttachmentsChoosePanel";
			this.AttachmentsChoosePanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(424, 37, true);
			this.AttachmentsChoosePanel.TabIndex = 7;
			// 
			// AttachmentsDropEdit
			// 
			this.AttachmentsDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AttachmentsDropEdit, "Attachment");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.EmailToContactBusinessObject)(null)).Attachment)));
			this.AttachmentsDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("EmailToContactUserControl|f7620fab-f843-40d2-91ec-1adfd97453b8", "Attachment");
			this.AttachmentsDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 6, true);
			this.AttachmentsDropEdit.Name = "AttachmentsDropEdit";
			this.AttachmentsDropEdit.PreBoundMaxLength = 60;
			this.AttachmentsDropEdit.ShowDescriptionBox = false;
			this.AttachmentsDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(378, 17, true);
			this.AttachmentsDropEdit.TabIndex = 7;
			// 
			// AttachmentsStatusPanel
			// 
			this.AttachmentsStatusPanel.Controls.Add(this.NumberOfAttachmentsLabel);
			this.AttachmentsStatusPanel.Controls.Add(this.RemoveAttachmentButton);
			this.AttachmentsStatusPanel.Controls.Add(this.AddAttachmentButton);
			this.AttachmentsStatusPanel.Dock = System.Windows.Forms.DockStyle.Right;
			this.AttachmentsStatusPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(422, 15, true);
			this.AttachmentsStatusPanel.Name = "AttachmentsStatusPanel";
			this.AttachmentsStatusPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(304, 39, true);
			this.AttachmentsStatusPanel.TabIndex = 5;
			// 
			// NumberOfAttachmentsLabel
			// 
			this.NumberOfAttachmentsLabel.AutoSize = true;
			this.BindingSource.SetBindingMember(this.NumberOfAttachmentsLabel, "NumberOfAttachmentsMessage");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.EmailToContactBusinessObject)(null)).NumberOfAttachmentsMessage)));
			this.NumberOfAttachmentsLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("EmailToContactUserControl|60c7c658-e131-417d-9ed8-598d337b989c", "There are 0 attachments.");
			this.NumberOfAttachmentsLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.NumberOfAttachmentsLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(15, 13, true);
			this.NumberOfAttachmentsLabel.Name = "NumberOfAttachmentsLabel";
			this.NumberOfAttachmentsLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(125, 13, true);
			this.NumberOfAttachmentsLabel.TabIndex = 4;
			// 
			// RemoveAttachmentButton
			// 
			this.RemoveAttachmentButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("EmailToContactUserControl|4b2e87fb-d1f1-45d0-b4e2-e9761befd08a", "Remove");
			this.RemoveAttachmentButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(228, 8, true);
			this.RemoveAttachmentButton.Name = "RemoveAttachmentButton";
			this.RemoveAttachmentButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(67, 23, true);
			this.RemoveAttachmentButton.TabIndex = 6;
			this.RemoveAttachmentButton.ToolTipCaption = null;
			this.RemoveAttachmentButton.Click += new System.EventHandler(this.RemoveAttachmentButton_Click);
			// 
			// AddAttachmentButton
			// 
			this.AddAttachmentButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("EmailToContactUserControl|cf05b0f8-7a06-43ec-84c3-ee3833538c24", "Add");
			this.AddAttachmentButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(159, 8, true);
			this.AddAttachmentButton.Name = "AddAttachmentButton";
			this.AddAttachmentButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(67, 23, true);
			this.AddAttachmentButton.TabIndex = 5;
			this.AddAttachmentButton.ToolTipCaption = null;
			this.AddAttachmentButton.Click += new System.EventHandler(this.AddAttachmentButton_Click);
			// 
			// PriorityDropEdit
			// 
			this.PriorityDropEdit.AllowDrop = true;
			this.PriorityDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.PriorityDropEdit, "Priority");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.EmailToContactBusinessObject)(null)).Priority)));
			this.PriorityDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("EmailToContactUserControl|f87940e8-1dd3-4476-9ed3-ccd0dbb0b5d0", "Priority");
			this.PriorityDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(592, 104, true);
			this.PriorityDropEdit.Name = "PriorityDropEdit";
			this.PriorityDropEdit.PreBoundMaxLength = 3;
			this.PriorityDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(136, 17, true);
			this.PriorityDropEdit.TabIndex = 15;
			// 
			// UseCurrentUserNameCheckBox
			// 
			this.UseCurrentUserNameCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.UseCurrentUserNameCheckBox, "UseCurrentUsersNameAndTitle");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.EmailToContactBusinessObject)(null)).UseCurrentUsersNameAndTitle)));
			this.UseCurrentUserNameCheckBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("61ba1501-44a8-4bc7-970f-267b87454506", "Use Current User\'s Name and Title");
			this.UseCurrentUserNameCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 9, true);
			this.UseCurrentUserNameCheckBox.Name = "UseCurrentUserNameCheckBox";
			this.UseCurrentUserNameCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(187, 16, true);
			this.UseCurrentUserNameCheckBox.TabIndex = 0;
			this.UseCurrentUserNameCheckBox.UseVisualStyleBackColor = true;
			// 
			// UseCurrentUserEmailCheckBox
			// 
			this.UseCurrentUserEmailCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.UseCurrentUserEmailCheckBox, "UseCurrentUsersEmailAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.EmailToContactBusinessObject)(null)).UseCurrentUsersEmailAddress)));
			this.UseCurrentUserEmailCheckBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("7e05e24b-10c9-49f7-aa6a-f9ffdbebc1b3", "Use Current User\'s Email Address");
			this.UseCurrentUserEmailCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(377, 9, true);
			this.UseCurrentUserEmailCheckBox.Name = "UseCurrentUserEmailCheckBox";
			this.UseCurrentUserEmailCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(184, 16, true);
			this.UseCurrentUserEmailCheckBox.TabIndex = 1;
			this.UseCurrentUserEmailCheckBox.UseVisualStyleBackColor = true;
			// 
			// CcButton
			// 
			this.CcButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("744207f4-9280-424e-aa66-8822667f4293", "Cc...");
			this.CcButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 77, true);
			this.CcButton.Name = "CcButton";
			this.CcButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(67, 23, true);
			this.CcButton.TabIndex = 8;
			this.CcButton.ToolTipCaption = null;
			this.CcButton.Click += new System.EventHandler(this.ToOrCcButton_Click);
			// 
			// ToButton
			// 
			this.ToButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("3b41e064-9a8b-4369-bda5-0a91958597bf", "To...");
			this.ToButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 53, true);
			this.ToButton.Name = "ToButton";
			this.ToButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(67, 23, true);
			this.ToButton.TabIndex = 6;
			this.ToButton.ToolTipCaption = null;
			this.ToButton.Click += new System.EventHandler(this.ToOrCcButton_Click);
			// 
			// EmailToContactUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.UseCurrentUserEmailCheckBox);
			this.Controls.Add(this.UseCurrentUserNameCheckBox);
			this.Controls.Add(this.PriorityDropEdit);
			this.Controls.Add(this.AttachmentsGroupBox);
			this.Controls.Add(this.BodyGroupBox);
			this.Controls.Add(this.FromEmailAddressDropEdit);
			this.Controls.Add(this.SubjectTextBox);
			this.Controls.Add(this.CcTextBox);
			this.Controls.Add(this.ToTextBox);
			this.Controls.Add(this.FromDropEdit);
			this.Controls.Add(this.CcButton);
			this.Controls.Add(this.ToButton);
			this.Name = "EmailToContactUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(744, 528, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.FromDropEdit.ResumeLayout(true);
			this.FromDropEdit.PerformLayout();
			this.FromEmailAddressDropEdit.ResumeLayout(true);
			this.FromEmailAddressDropEdit.PerformLayout();
			this.BodyGroupBox.ResumeLayout(false);
			this.BodyGroupBox.PerformLayout();
			this.AttachmentsGroupBox.ResumeLayout(false);
			this.AttachmentsGroupBox.PerformLayout();
			this.AttachmentsChoosePanel.ResumeLayout(false);
			this.AttachmentsChoosePanel.PerformLayout();
			this.AttachmentsDropEdit.ResumeLayout(true);
			this.AttachmentsDropEdit.PerformLayout();
			this.AttachmentsStatusPanel.ResumeLayout(false);
			this.AttachmentsStatusPanel.PerformLayout();
			this.PriorityDropEdit.ResumeLayout(true);
			this.PriorityDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
		#endregion

		Enterprise.ZArchitecture.GUI.ZDropEdit FromDropEdit;
		internal Enterprise.ZArchitecture.ZTextBox ToTextBox;
		internal Enterprise.ZArchitecture.ZTextBox CcTextBox;
		Enterprise.ZArchitecture.ZTextBox SubjectTextBox;
		Enterprise.ZArchitecture.GUI.ZGroupBox BodyGroupBox;
		Enterprise.ZArchitecture.GUI.ZGroupBox AttachmentsGroupBox;
		Enterprise.ZArchitecture.GUI.ZDropEdit PriorityDropEdit;
		Enterprise.ZArchitecture.ZTextBox BodyTextBox;
		Enterprise.ZArchitecture.GUI.ZDropEdit FromEmailAddressDropEdit;
		Enterprise.ZArchitecture.GUI.ZPanel AttachmentsStatusPanel;
		Enterprise.ZArchitecture.ZLabel NumberOfAttachmentsLabel;
		internal Enterprise.ZArchitecture.GUI.ZButton RemoveAttachmentButton;
		internal Enterprise.ZArchitecture.GUI.ZButton AddAttachmentButton;
		Enterprise.ZArchitecture.GUI.ZPanel AttachmentsChoosePanel;
		Enterprise.ZArchitecture.GUI.ZDropEdit AttachmentsDropEdit;
		Enterprise.ZArchitecture.GUI.ZCheckBox UseCurrentUserNameCheckBox;
		Enterprise.ZArchitecture.GUI.ZCheckBox UseCurrentUserEmailCheckBox;
		internal Enterprise.ZArchitecture.GUI.ZButton CcButton;
		internal Enterprise.ZArchitecture.GUI.ZButton ToButton;
	}
}
