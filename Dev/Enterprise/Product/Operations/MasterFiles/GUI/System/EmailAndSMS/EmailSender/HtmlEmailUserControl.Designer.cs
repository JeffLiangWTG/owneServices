
using Enterprise.ZArchitecture.GUI;
namespace Enterprise.MasterFiles.GUI
{
	public partial class HtmlEmailUserControl
	{
		#region Dispose

		protected override void Dispose(bool disposing)
		{
			if (openFileDialog != null)
			{
				openFileDialog.Dispose();
				openFileDialog = null;
			}
			base.Dispose(disposing);
		}

		#endregion

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
			this.RecipientsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.UseCurrentUserEmailCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.BccButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CcButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ToButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.UseCurrentUserNameCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.BccTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.TemplateGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.TemplateGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.SubjectGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.BodyGroupBox.SuspendLayout();
			this.AttachmentsGroupBox.SuspendLayout();
			this.AttachmentsChoosePanel.SuspendLayout();
			this.AttachmentsStatusPanel.SuspendLayout();
			this.RecipientsGroupBox.SuspendLayout();
			this.TemplateGroupBox.SuspendLayout();
			this.SubjectGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.HtmlEmailWithAttachment);
			// 
			// FromDropEdit
			//
			this.FromDropEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.FromDropEdit, "FromDisplayName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.HtmlEmailWithAttachment)(null)).FromDisplayName)));
			this.FromDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("HtmlEmailUserControl|52235b53-587b-4111-840f-c4431d6c352b", "From (Display Name)");
			this.FromDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.FromDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(117, 42, true);
			this.FromDropEdit.Name = "FromDropEdit";
			this.FromDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(190, 20, true);
			this.FromDropEdit.UseFullWidthForCodeBox = true;
			this.FromDropEdit.ShowDescriptionBox = false;
			this.FromDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			this.FromDropEdit.TabIndex = 4;
			// 
			// ToTextBox
			// 
			this.ToTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ToTextBox, "ToEmailAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.HtmlEmailWithAttachment)(null)).ToEmailAddress)));
			this.ToTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("HtmlEmailUserControl|83e55525-5578-472d-8f2d-121ce8d768b2", "To", "To (Email Address)", "Separate email addresses with a semi-colon (;).");
			this.ToTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ToTextBox, false);
			this.ToTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(81, 68, true);
			this.ToTextBox.Name = "ToTextBox";
			this.ToTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(655, 20, true);
			this.ToTextBox.TabIndex = 7;
			// 
			// CcTextBox
			// 
			this.CcTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.CcTextBox, "Cc");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.HtmlEmailWithAttachment)(null)).Cc)));
			this.CcTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("HtmlEmailUserControl|568cc2b2-bd58-4268-be27-3a9d732b43bc", "Cc", "Separate email addresses with a semi-colon (;).");
			this.CcTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.CcTextBox, false);
			this.CcTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(81, 94, true);
			this.CcTextBox.Name = "CcTextBox";
			this.CcTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(655, 20, true);
			this.CcTextBox.TabIndex = 9;
			// 
			// SubjectTextBox
			// 
			this.SubjectTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.SubjectTextBox, "Subject");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.HtmlEmailWithAttachment)(null)).Subject)));
			this.SubjectTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.SubjectTextBox, false);
			this.SubjectTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 19, true);
			this.SubjectTextBox.Name = "SubjectTextBox";
			this.SubjectTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(387, 20, true);
			this.SubjectTextBox.TabIndex = 14;
			// 
			// FromEmailAddressDropEdit
			//
			this.FromEmailAddressDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.FromEmailAddressDropEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.FromEmailAddressDropEdit, "FromEmailAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.HtmlEmailWithAttachment)(null)).FromEmailAddress)));
			this.FromEmailAddressDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("HtmlEmailUserControl|0fccf46b-a4df-4e5e-b77c-242fb2485fc9", "From (Email Address)");
			this.FromEmailAddressDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.FromEmailAddressDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(491, 42, true);
			this.FromEmailAddressDropEdit.Name = "FromEmailAddressDropEdit";
			this.FromEmailAddressDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(190, 20, true);
			this.FromEmailAddressDropEdit.UseFullWidthForCodeBox = true;
			this.FromEmailAddressDropEdit.ShowDescriptionBox = false;
			this.FromEmailAddressDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			this.FromEmailAddressDropEdit.TabIndex = 5;
			// 
			// BodyGroupBox
			// 
			this.BodyGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BodyGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("HtmlEmailUserControl|c32529d6-09e4-4610-a92a-5b087f626974", "Body");
			this.BodyGroupBox.Controls.Add(this.BodyTextBox);
			this.BodyGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 218, true);
			this.BodyGroupBox.Name = "BodyGroupBox";
			this.BodyGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(744, 239, true);
			this.BodyGroupBox.TabIndex = 15;
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
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.HtmlEmailWithAttachment)(null)).Body)));
			this.BodyTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("HtmlEmailUserControl|fcebe68c-73f3-42c5-b268-656eab911a3c", "Body");
			this.BodyTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.BodyTextBox, false);
			this.BodyTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 16, true);
			this.BodyTextBox.Multiline = true;
			this.BodyTextBox.Name = "BodyTextBox";
			this.BodyTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.BodyTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(728, 215, true);
			this.BodyTextBox.TabIndex = 16;
			// 
			// AttachmentsGroupBox
			// 
			this.AttachmentsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.AttachmentsGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("HtmlEmailUserControl|94fba8d6-e6a9-49ee-96f2-3308662ac990", "Attachments");
			this.AttachmentsGroupBox.Controls.Add(this.AttachmentsChoosePanel);
			this.AttachmentsGroupBox.Controls.Add(this.AttachmentsStatusPanel);
			this.AttachmentsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 463, true);
			this.AttachmentsGroupBox.Name = "AttachmentsGroupBox";
			this.AttachmentsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(744, 56, true);
			this.AttachmentsGroupBox.TabIndex = 17;
			this.AttachmentsGroupBox.TabStop = false;
			// 
			// AttachmentsChoosePanel
			// 
			this.AttachmentsChoosePanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.AttachmentsChoosePanel.Controls.Add(this.AttachmentsDropEdit);
			this.AttachmentsChoosePanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 16, true);
			this.AttachmentsChoosePanel.Name = "AttachmentsChoosePanel";
			this.AttachmentsChoosePanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(424, 37, true);
			this.AttachmentsChoosePanel.TabIndex = 5;
			// 
			// AttachmentsDropEdit
			// 
			this.AttachmentsDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AttachmentsDropEdit, "Attachment");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.HtmlEmailWithAttachment)(null)).Attachment)));
			this.AttachmentsDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("HtmlEmailUserControl|f7620fab-f843-40d2-91ec-1adfd97453b8", "Attachment");
			this.AttachmentsDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 6, true);
			this.AttachmentsDropEdit.Name = "AttachmentsDropEdit";
			this.AttachmentsDropEdit.PreBoundMaxLength = 60;
			this.AttachmentsDropEdit.ShowDescriptionBox = false;
			this.AttachmentsDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(378, 20, true);
			this.AttachmentsDropEdit.TabIndex = 18;
			// 
			// AttachmentsStatusPanel
			// 
			this.AttachmentsStatusPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.AttachmentsStatusPanel.Controls.Add(this.NumberOfAttachmentsLabel);
			this.AttachmentsStatusPanel.Controls.Add(this.RemoveAttachmentButton);
			this.AttachmentsStatusPanel.Controls.Add(this.AddAttachmentButton);
			this.AttachmentsStatusPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(437, 16, true);
			this.AttachmentsStatusPanel.Name = "AttachmentsStatusPanel";
			this.AttachmentsStatusPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(304, 37, true);
			this.AttachmentsStatusPanel.TabIndex = 7;
			// 
			// NumberOfAttachmentsLabel
			// 
			this.NumberOfAttachmentsLabel.AutoSize = true;
			this.BindingSource.SetBindingMember(this.NumberOfAttachmentsLabel, "NumberOfAttachmentsMessage");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.HtmlEmailWithAttachment)(null)).NumberOfAttachmentsMessage)));
			this.NumberOfAttachmentsLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("HtmlEmailUserControl|60c7c658-e131-417d-9ed8-598d337b989c", "There are 0 attachments.");
			this.NumberOfAttachmentsLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(15, 13, true);
			this.NumberOfAttachmentsLabel.Name = "NumberOfAttachmentsLabel";
			this.NumberOfAttachmentsLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(126, 13, true);
			this.NumberOfAttachmentsLabel.TabIndex = 4;
			// 
			// RemoveAttachmentButton
			// 
			this.RemoveAttachmentButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("HtmlEmailUserControl|4b2e87fb-d1f1-45d0-b4e2-e9761befd08a", "Remove");
			this.RemoveAttachmentButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(228, 8, true);
			this.RemoveAttachmentButton.Name = "RemoveAttachmentButton";
			this.RemoveAttachmentButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(67, 23, true);
			this.RemoveAttachmentButton.TabIndex = 20;
			this.RemoveAttachmentButton.Click += new System.EventHandler(this.RemoveAttachmentButton_Click);
			// 
			// AddAttachmentButton
			// 
			this.AddAttachmentButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("HtmlEmailUserControl|cf05b0f8-7a06-43ec-84c3-ee3833538c24", "Add");
			this.AddAttachmentButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(159, 8, true);
			this.AddAttachmentButton.Name = "AddAttachmentButton";
			this.AddAttachmentButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(67, 23, true);
			this.AddAttachmentButton.TabIndex = 19;
			this.AddAttachmentButton.Click += new System.EventHandler(this.AddAttachmentButton_Click);
			// 
			// RecipientsGroupBox
			// 
			this.RecipientsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.RecipientsGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("4720bcda-9606-4041-8b52-47f511d5e6ca", "Recipients");
			this.RecipientsGroupBox.Controls.Add(this.UseCurrentUserEmailCheckBox);
			this.RecipientsGroupBox.Controls.Add(this.BccButton);
			this.RecipientsGroupBox.Controls.Add(this.CcButton);
			this.RecipientsGroupBox.Controls.Add(this.ToButton);
			this.RecipientsGroupBox.Controls.Add(this.UseCurrentUserNameCheckBox);
			this.RecipientsGroupBox.Controls.Add(this.FromDropEdit);
			this.RecipientsGroupBox.Controls.Add(this.ToTextBox);
			this.RecipientsGroupBox.Controls.Add(this.FromEmailAddressDropEdit);
			this.RecipientsGroupBox.Controls.Add(this.BccTextBox);
			this.RecipientsGroupBox.Controls.Add(this.CcTextBox);
			this.RecipientsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 3, true);
			this.RecipientsGroupBox.Name = "RecipientsGroupBox";
			this.RecipientsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(744, 154, true);
			this.RecipientsGroupBox.TabIndex = 1;
			this.RecipientsGroupBox.TabStop = false;
			// 
			// UseCurrentUserEmailCheckBox
			// 
			this.UseCurrentUserEmailCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.UseCurrentUserEmailCheckBox, "UseCurrentUsersEmailAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.HtmlEmailWithAttachment)(null)).UseCurrentUsersEmailAddress)));
			this.UseCurrentUserEmailCheckBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("cd033a67-246f-47aa-a980-edaa2113f9f5", "Use Current User\'s Email Address");
			this.UseCurrentUserEmailCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.UseCurrentUserEmailCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(379, 19, true);
			this.UseCurrentUserEmailCheckBox.Name = "UseCurrentUserEmailCheckBox";
			this.UseCurrentUserEmailCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(183, 17, true);
			this.UseCurrentUserEmailCheckBox.TabIndex = 3;
			this.UseCurrentUserEmailCheckBox.UseVisualStyleBackColor = true;
			// 
			// BccButton
			// 
			this.BccButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("6a1cde29-10f0-48be-91fe-52d57354ca3c", "Bcc...");
			this.BccButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 118, true);
			this.BccButton.Name = "BccButton";
			this.BccButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(67, 23, true);
			this.BccButton.TabIndex = 10;
			this.BccButton.Click += new System.EventHandler(this.BccButton_Click);
			// 
			// CcButton
			// 
			this.CcButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("fc21a914-ebd3-4819-9ef1-f9493f9c3e86", "Cc...");
			this.CcButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 92, true);
			this.CcButton.Name = "CcButton";
			this.CcButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(67, 23, true);
			this.CcButton.TabIndex = 8;
			this.CcButton.Click += new System.EventHandler(this.CcButton_Click);
			// 
			// ToButton
			// 
			this.ToButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("802c930d-5e7d-417f-b3ad-be7723231b90", "To...");
			this.ToButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 66, true);
			this.ToButton.Name = "ToButton";
			this.ToButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(67, 23, true);
			this.ToButton.TabIndex = 6;
			this.ToButton.Click += new System.EventHandler(this.ToButton_Click);
			// 
			// UseCurrentUserNameCheckBox
			// 
			this.UseCurrentUserNameCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.UseCurrentUserNameCheckBox, "UseCurrentUsersNameAndTitle");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.HtmlEmailWithAttachment)(null)).UseCurrentUsersNameAndTitle)));
			this.UseCurrentUserNameCheckBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("cc7e2ed1-cd7a-458c-9f71-99fcc4ade623", "Use Current User\'s Name and Title");
			this.UseCurrentUserNameCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.UseCurrentUserNameCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 19, true);
			this.UseCurrentUserNameCheckBox.Name = "UseCurrentUserNameCheckBox";
			this.UseCurrentUserNameCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(189, 17, true);
			this.UseCurrentUserNameCheckBox.TabIndex = 2;
			this.UseCurrentUserNameCheckBox.UseVisualStyleBackColor = true;
			// 
			// BccTextBox
			// 
			this.BccTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.BccTextBox, "Bcc");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.HtmlEmailWithAttachment)(null)).Bcc)));
			this.BccTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("HtmlEmailUserControl|19387149-22ff-4a87-998b-08b719dbe338", "Bcc", "Separate email addresses with a semi-colon (;).");
			this.BccTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.BccTextBox, false);
			this.BccTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(81, 120, true);
			this.BccTextBox.Name = "BccTextBox";
			this.BccTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(655, 20, true);
			this.BccTextBox.TabIndex = 11;
			// 
			// TemplateGuidFindBox
			// 
			this.TemplateGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TemplateGuidFindBox, "Template");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.HtmlEmailWithAttachment)(null)).Template)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.TemplateGuidFindBox, false);
			this.TemplateGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 19, true);
			this.TemplateGuidFindBox.Name = "TemplateGuidFindBox";
			this.TemplateGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(317, 20, true);
			this.TemplateGuidFindBox.TabIndex = 13;
			// 
			// TemplateGroupBox
			// 
			this.TemplateGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("b90a87f4-ee60-4c16-9256-f51092a9ea21", "Email Template");
			this.TemplateGroupBox.Controls.Add(this.TemplateGuidFindBox);
			this.TemplateGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 163, true);
			this.TemplateGroupBox.Name = "TemplateGroupBox";
			this.TemplateGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(337, 49, true);
			this.TemplateGroupBox.TabIndex = 2;
			this.TemplateGroupBox.TabStop = false;
			// 
			// SubjectGroupBox
			// 
			this.SubjectGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.SubjectGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("HtmlEmailUserControl|ac963180-9ac0-451e-b469-33d185a000b5", "Subject");
			this.SubjectGroupBox.Controls.Add(this.SubjectTextBox);
			this.SubjectGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(343, 163, true);
			this.SubjectGroupBox.Name = "SubjectGroupBox";
			this.SubjectGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(401, 49, true);
			this.SubjectGroupBox.TabIndex = 3;
			this.SubjectGroupBox.TabStop = false;
			// 
			// HtmlEmailUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.SubjectGroupBox);
			this.Controls.Add(this.TemplateGroupBox);
			this.Controls.Add(this.AttachmentsGroupBox);
			this.Controls.Add(this.RecipientsGroupBox);
			this.Controls.Add(this.BodyGroupBox);
			this.Name = "HtmlEmailUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(744, 522, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.BodyGroupBox.ResumeLayout(false);
			this.BodyGroupBox.PerformLayout();
			this.AttachmentsGroupBox.ResumeLayout(false);
			this.AttachmentsChoosePanel.ResumeLayout(false);
			this.AttachmentsStatusPanel.ResumeLayout(false);
			this.AttachmentsStatusPanel.PerformLayout();
			this.RecipientsGroupBox.ResumeLayout(false);
			this.RecipientsGroupBox.PerformLayout();
			this.TemplateGroupBox.ResumeLayout(false);
			this.SubjectGroupBox.ResumeLayout(false);
			this.SubjectGroupBox.PerformLayout();
			this.ResumeLayout(false);

		}
		#endregion

		private ZGroupBox RecipientsGroupBox;
		private ZCheckBox UseCurrentUserNameCheckBox;
		internal ZOpenFileDialog openFileDialog;
		Enterprise.ZArchitecture.GUI.ZPanel AttachmentsStatusPanel;
		Enterprise.ZArchitecture.ZLabel NumberOfAttachmentsLabel;
		internal Enterprise.ZArchitecture.GUI.ZButton RemoveAttachmentButton;
		internal Enterprise.ZArchitecture.GUI.ZButton AddAttachmentButton;
		Enterprise.ZArchitecture.GUI.ZPanel AttachmentsChoosePanel;
		Enterprise.ZArchitecture.GUI.ZDropEdit AttachmentsDropEdit;
		private ZArchitecture.GUI.ZDropEdit FromDropEdit;
		protected ZArchitecture.ZTextBox ToTextBox;
		protected ZArchitecture.ZTextBox CcTextBox;
		private ZArchitecture.ZTextBox SubjectTextBox;
		private ZGroupBox BodyGroupBox;
		private ZGroupBox AttachmentsGroupBox;
		private ZArchitecture.ZTextBox BodyTextBox;
		private ZArchitecture.GUI.ZDropEdit FromEmailAddressDropEdit;
		private ZCheckBox UseCurrentUserEmailCheckBox;
		internal ZButton BccButton;
		internal ZButton CcButton;
		internal ZButton ToButton;
		protected ZArchitecture.ZTextBox BccTextBox;
		private ZGuidFindBox TemplateGuidFindBox;
		private ZGroupBox TemplateGroupBox;
		private ZGroupBox SubjectGroupBox;
	}
}
