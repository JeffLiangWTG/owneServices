namespace Enterprise.Customs.US.GUI
{
	partial class FTZMessageSendingForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2222:DoNotDecreaseInheritedMemberVisibility")]
		private new void InitializeComponent()
		{
			this.MessageOptionGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.PhoneNumberLabel = new Enterprise.ZArchitecture.ZLabel();
			this.PhoneNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ReplacementReasonsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.RemarksTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.OtherReasonCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.CancelAddPTTCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ChangeAdmittedQuantityCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.DeleteHTSLineCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ChangeAddHTSLineCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.DeleteBillOfLadingCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ChangeAddBillOfLadingCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.DeleteConveyanceCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ChangeAddConveyanceCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ContactNameLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ContactNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.BottomPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.OKButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.MessageOptionGroupBox.SuspendLayout();
			this.ReplacementReasonsGroupBox.SuspendLayout();
			this.BottomPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 319, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(690, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.Business.FTZMessageSendingObject);
			// 
			// MessageOptionGroupBox
			// 
			this.MessageOptionGroupBox.Controls.Add(this.PhoneNumberLabel);
			this.MessageOptionGroupBox.Controls.Add(this.PhoneNumberTextBox);
			this.MessageOptionGroupBox.Controls.Add(this.ReplacementReasonsGroupBox);
			this.MessageOptionGroupBox.Controls.Add(this.ContactNameLabel);
			this.MessageOptionGroupBox.Controls.Add(this.ContactNameTextBox);
			this.MessageOptionGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.MessageOptionGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MessageOptionGroupBox.Name = "MessageOptionGroupBox";
			this.MessageOptionGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(690, 289, true);
			this.MessageOptionGroupBox.TabIndex = 1;
			this.MessageOptionGroupBox.TabStop = false;
			this.MessageOptionGroupBox.Text = "Message Option";
			// 
			// PhoneNumberLabel
			// 
			this.PhoneNumberLabel.AutoSize = true;
			this.PhoneNumberLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.PhoneNumberLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(56, 51, true);
			this.PhoneNumberLabel.Name = "PhoneNumberLabel";
			this.PhoneNumberLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(78, 13, true);
			this.PhoneNumberLabel.TabIndex = 2;
			this.PhoneNumberLabel.Text = "Phone Number";
			// 
			// PhoneNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.PhoneNumberTextBox, "US_FTZContactPhone");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.FTZMessageSendingObject)(null)).US_FTZContactPhone)));
			this.ContactNameTextBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("E5C42B7E-0C7F-4EC1-8D01-E4553549D96F", "Phone Number");
			this.PhoneNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(137, 48, true);
			this.PhoneNumberTextBox.Name = "PhoneNumberTextBox";
			this.PhoneNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(207, 20, true);
			this.PhoneNumberTextBox.TabIndex = 3;
			// 
			// ReplacementReasonsGroupBox
			// 
			this.ReplacementReasonsGroupBox.Controls.Add(this.RemarksTextBox);
			this.ReplacementReasonsGroupBox.Controls.Add(this.OtherReasonCheckBox);
			this.ReplacementReasonsGroupBox.Controls.Add(this.CancelAddPTTCheckBox);
			this.ReplacementReasonsGroupBox.Controls.Add(this.ChangeAdmittedQuantityCheckBox);
			this.ReplacementReasonsGroupBox.Controls.Add(this.DeleteHTSLineCheckBox);
			this.ReplacementReasonsGroupBox.Controls.Add(this.ChangeAddHTSLineCheckBox);
			this.ReplacementReasonsGroupBox.Controls.Add(this.DeleteBillOfLadingCheckBox);
			this.ReplacementReasonsGroupBox.Controls.Add(this.ChangeAddBillOfLadingCheckBox);
			this.ReplacementReasonsGroupBox.Controls.Add(this.DeleteConveyanceCheckBox);
			this.ReplacementReasonsGroupBox.Controls.Add(this.ChangeAddConveyanceCheckBox);
			this.ReplacementReasonsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 85, true);
			this.ReplacementReasonsGroupBox.Name = "ReplacementReasonsGroupBox";
			this.ReplacementReasonsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(675, 198, true);
			this.ReplacementReasonsGroupBox.TabIndex = 8;
			this.ReplacementReasonsGroupBox.TabStop = false;
			this.ReplacementReasonsGroupBox.Text = "Replacement Reasons";
			// 
			// RemarksTextBox
			// 
			this.BindingSource.SetBindingMember(this.RemarksTextBox, "US_Remarks");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.FTZMessageSendingObject)(null)).US_Remarks)));
			this.RemarksTextBox.CaptionResourceString = null;
			this.RemarksTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(236, 167, true);
			this.RemarksTextBox.Name = "RemarksTextBox";
			this.RemarksTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(420, 20, true);
			this.RemarksTextBox.TabIndex = 13;
			// 
			// OtherReasonCheckBox
			// 
			this.BindingSource.SetBindingMember(this.OtherReasonCheckBox, "US_OtherReason");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.Business.FTZMessageSendingObject)(null)).US_OtherReason)));
			this.OtherReasonCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.OtherReasonCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.OtherReasonCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(37, 170, true);
			this.OtherReasonCheckBox.Name = "OtherReasonCheckBox";
			this.OtherReasonCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(180, 17, true);
			this.OtherReasonCheckBox.TabIndex = 12;
			this.OtherReasonCheckBox.Text = "Other/Remarks";
			this.OtherReasonCheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.OtherReasonCheckBox.UseVisualStyleBackColor = true;
			// 
			// CancelAddPTTCheckBox
			// 
			this.BindingSource.SetBindingMember(this.CancelAddPTTCheckBox, "US_CancelOrAddPTT");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.Business.FTZMessageSendingObject)(null)).US_CancelOrAddPTT)));
			this.CancelAddPTTCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.CancelAddPTTCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.CancelAddPTTCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(37, 150, true);
			this.CancelAddPTTCheckBox.Name = "CancelAddPTTCheckBox";
			this.CancelAddPTTCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(180, 17, true);
			this.CancelAddPTTCheckBox.TabIndex = 11;
			this.CancelAddPTTCheckBox.Text = "Cancel/Add PTT";
			this.CancelAddPTTCheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.CancelAddPTTCheckBox.UseVisualStyleBackColor = true;
			// 
			// ChangeAdmittedQuantityCheckBox
			// 
			this.BindingSource.SetBindingMember(this.ChangeAdmittedQuantityCheckBox, "US_ChangeAdmittedQuantity");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.Business.FTZMessageSendingObject)(null)).US_ChangeAdmittedQuantity)));
			this.ChangeAdmittedQuantityCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.ChangeAdmittedQuantityCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ChangeAdmittedQuantityCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(37, 131, true);
			this.ChangeAdmittedQuantityCheckBox.Name = "ChangeAdmittedQuantityCheckBox";
			this.ChangeAdmittedQuantityCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(180, 17, true);
			this.ChangeAdmittedQuantityCheckBox.TabIndex = 10;
			this.ChangeAdmittedQuantityCheckBox.Text = "Change Admitted Quantity";
			this.ChangeAdmittedQuantityCheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.ChangeAdmittedQuantityCheckBox.UseVisualStyleBackColor = true;
			// 
			// DeleteHTSLineCheckBox
			// 
			this.BindingSource.SetBindingMember(this.DeleteHTSLineCheckBox, "US_DeleteHTSLine");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.Business.FTZMessageSendingObject)(null)).US_DeleteHTSLine)));
			this.DeleteHTSLineCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.DeleteHTSLineCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.DeleteHTSLineCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(37, 113, true);
			this.DeleteHTSLineCheckBox.Name = "DeleteHTSLineCheckBox";
			this.DeleteHTSLineCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(180, 17, true);
			this.DeleteHTSLineCheckBox.TabIndex = 9;
			this.DeleteHTSLineCheckBox.Text = "Delete HTS Line(s)";
			this.DeleteHTSLineCheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.DeleteHTSLineCheckBox.UseVisualStyleBackColor = true;
			// 
			// ChangeAddHTSLineCheckBox
			// 
			this.BindingSource.SetBindingMember(this.ChangeAddHTSLineCheckBox, "US_ChangeOrAddHTSLine");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.Business.FTZMessageSendingObject)(null)).US_ChangeOrAddHTSLine)));
			this.ChangeAddHTSLineCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.ChangeAddHTSLineCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ChangeAddHTSLineCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(37, 94, true);
			this.ChangeAddHTSLineCheckBox.Name = "ChangeAddHTSLineCheckBox";
			this.ChangeAddHTSLineCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(180, 17, true);
			this.ChangeAddHTSLineCheckBox.TabIndex = 8;
			this.ChangeAddHTSLineCheckBox.Text = "Change/Add HTS Line(s)";
			this.ChangeAddHTSLineCheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.ChangeAddHTSLineCheckBox.UseVisualStyleBackColor = true;
			// 
			// DeleteBillOfLadingCheckBox
			// 
			this.BindingSource.SetBindingMember(this.DeleteBillOfLadingCheckBox, "US_DeleteBillOfLading");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.Business.FTZMessageSendingObject)(null)).US_DeleteBillOfLading)));
			this.DeleteBillOfLadingCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.DeleteBillOfLadingCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.DeleteBillOfLadingCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(37, 75, true);
			this.DeleteBillOfLadingCheckBox.Name = "DeleteBillOfLadingCheckBox";
			this.DeleteBillOfLadingCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(180, 17, true);
			this.DeleteBillOfLadingCheckBox.TabIndex = 7;
			this.DeleteBillOfLadingCheckBox.Text = "Delete Bill(s) of Lading";
			this.DeleteBillOfLadingCheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.DeleteBillOfLadingCheckBox.UseVisualStyleBackColor = true;
			// 
			// ChangeAddBillOfLadingCheckBox
			// 
			this.BindingSource.SetBindingMember(this.ChangeAddBillOfLadingCheckBox, "US_ChangeOrAddBillOfLading");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.Business.FTZMessageSendingObject)(null)).US_ChangeOrAddBillOfLading)));
			this.ChangeAddBillOfLadingCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.ChangeAddBillOfLadingCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ChangeAddBillOfLadingCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(37, 56, true);
			this.ChangeAddBillOfLadingCheckBox.Name = "ChangeAddBillOfLadingCheckBox";
			this.ChangeAddBillOfLadingCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(180, 17, true);
			this.ChangeAddBillOfLadingCheckBox.TabIndex = 6;
			this.ChangeAddBillOfLadingCheckBox.Text = "Change/Add Bill(s) of Lading";
			this.ChangeAddBillOfLadingCheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.ChangeAddBillOfLadingCheckBox.UseVisualStyleBackColor = true;
			// 
			// DeleteConveyanceCheckBox
			// 
			this.BindingSource.SetBindingMember(this.DeleteConveyanceCheckBox, "US_DeleteConveyance");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.Business.FTZMessageSendingObject)(null)).US_DeleteConveyance)));
			this.DeleteConveyanceCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.DeleteConveyanceCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.DeleteConveyanceCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(37, 37, true);
			this.DeleteConveyanceCheckBox.Name = "DeleteConveyanceCheckBox";
			this.DeleteConveyanceCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(180, 17, true);
			this.DeleteConveyanceCheckBox.TabIndex = 5;
			this.DeleteConveyanceCheckBox.Text = "Delete Conveyance(s)";
			this.DeleteConveyanceCheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.DeleteConveyanceCheckBox.UseVisualStyleBackColor = true;
			// 
			// ChangeAddConveyanceCheckBox
			// 
			this.BindingSource.SetBindingMember(this.ChangeAddConveyanceCheckBox, "US_ChangeOrAddConveyance");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.Business.FTZMessageSendingObject)(null)).US_ChangeOrAddConveyance)));
			this.ChangeAddConveyanceCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.ChangeAddConveyanceCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ChangeAddConveyanceCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(37, 19, true);
			this.ChangeAddConveyanceCheckBox.Name = "ChangeAddConveyanceCheckBox";
			this.ChangeAddConveyanceCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(180, 17, true);
			this.ChangeAddConveyanceCheckBox.TabIndex = 4;
			this.ChangeAddConveyanceCheckBox.Text = "Change/Add Conveyance(s)";
			this.ChangeAddConveyanceCheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.ChangeAddConveyanceCheckBox.UseVisualStyleBackColor = true;
			// 
			// ContactNameLabel
			// 
			this.ContactNameLabel.AutoSize = true;
			this.ContactNameLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.ContactNameLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(59, 23, true);
			this.ContactNameLabel.Name = "ContactNameLabel";
			this.ContactNameLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 13, true);
			this.ContactNameLabel.TabIndex = 0;
			this.ContactNameLabel.Text = "Contact Name";
			// 
			// ContactNameTextBox
			// 
			this.BindingSource.SetBindingMember(this.ContactNameTextBox, "US_FTZContactName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.FTZMessageSendingObject)(null)).US_FTZContactName)));
			this.ContactNameTextBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("67361D1F-74DA-4848-9ED7-DCCD464BD1E0", "Contact Name");
			this.ContactNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(137, 20, true);
			this.ContactNameTextBox.Name = "ContactNameTextBox";
			this.ContactNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(207, 20, true);
			this.ContactNameTextBox.TabIndex = 1;
			// 
			// BottomPanel
			// 
			this.BottomPanel.Controls.Add(this.OKButton);
			this.BottomPanel.Controls.Add(this.CancelButton);
			this.BottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.BottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 289, true);
			this.BottomPanel.Name = "BottomPanel";
			this.BottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(690, 30, true);
			this.BottomPanel.TabIndex = 9;
			// 
			// OKButton
			// 
			this.OKButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.OKButton.IsCaptionOverridden = true;
			this.OKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(529, 3, true);
			this.OKButton.Name = "OKButton";
			this.OKButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.OKButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.OKButton.TabIndex = 0;
			this.OKButton.Text = "&OK";
			this.OKButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.OKButton.ToolTipCaption = null;
			this.OKButton.UseVisualStyleBackColor = true;
			this.OKButton.Click += new System.EventHandler(this.OKButton_Click);
			// 
			// CancelButton
			// 
			this.CancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CancelButton.IsCaptionOverridden = true;
			this.CancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(609, 3, true);
			this.CancelButton.Name = "CancelButton";
			this.CancelButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.CancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CancelButton.TabIndex = 1;
			this.CancelButton.Text = "&Cancel";
			this.CancelButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.CancelButton.ToolTipCaption = null;
			this.CancelButton.UseVisualStyleBackColor = true;
			this.CancelButton.Click += new System.EventHandler(this.CancelButton_Click);
			// 
			// FTZMessageSendingForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(690, 343, true);
			this.Controls.Add(this.BottomPanel);
			this.Controls.Add(this.MessageOptionGroupBox);
			this.DataSourceType = typeof(Enterprise.Customs.US.Business.FTZMessageSendingObject);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(690, 357, true);
			this.Name = "FTZMessageSendingForm";
			this.Text = "Send Replacement / Amendment Message";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.MessageOptionGroupBox, 0);
			this.Controls.SetChildIndex(this.BottomPanel, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MessageOptionGroupBox.ResumeLayout(false);
			this.MessageOptionGroupBox.PerformLayout();
			this.ReplacementReasonsGroupBox.ResumeLayout(false);
			this.ReplacementReasonsGroupBox.PerformLayout();
			this.BottomPanel.ResumeLayout(false);
			this.BottomPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox MessageOptionGroupBox;
		private ZArchitecture.ZLabel ContactNameLabel;
		private ZArchitecture.ZTextBox ContactNameTextBox;
		private ZArchitecture.ZTextBox PhoneNumberTextBox;
		private ZArchitecture.GUI.ZGroupBox ReplacementReasonsGroupBox;
		private ZArchitecture.GUI.ZCheckBox DeleteHTSLineCheckBox;
		private ZArchitecture.GUI.ZCheckBox ChangeAddHTSLineCheckBox;
		private ZArchitecture.GUI.ZCheckBox DeleteBillOfLadingCheckBox;
		private ZArchitecture.GUI.ZCheckBox ChangeAddBillOfLadingCheckBox;
		private ZArchitecture.GUI.ZCheckBox DeleteConveyanceCheckBox;
		private ZArchitecture.GUI.ZCheckBox ChangeAddConveyanceCheckBox;
		private ZArchitecture.ZTextBox RemarksTextBox;
		private ZArchitecture.GUI.ZCheckBox OtherReasonCheckBox;
		private ZArchitecture.GUI.ZCheckBox CancelAddPTTCheckBox;
		private ZArchitecture.GUI.ZCheckBox ChangeAdmittedQuantityCheckBox;
		private ZArchitecture.GUI.ZPanel BottomPanel;
		private ZArchitecture.GUI.ZButton OKButton;
		private new ZArchitecture.GUI.ZButton CancelButton;
		private ZArchitecture.ZLabel PhoneNumberLabel;
	}
}
