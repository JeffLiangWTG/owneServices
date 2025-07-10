namespace Enterprise.MasterFiles.GUI
{
	partial class ManualDataExportForm
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
			DisposeCore();
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private new void InitializeComponent()
		{
			this.Calc_RecipientTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.EventTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.PurposeCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.TriggerDescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.SendAndCloseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CloseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.RecipientTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.RecipientPKGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.RecipientServiceDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.Calc_RecipientTypeDropEdit.SuspendLayout();
			this.EventTypeDropEdit.SuspendLayout();
			this.PurposeCodeDropEdit.SuspendLayout();
			this.RecipientTypeDropEdit.SuspendLayout();
			this.RecipientPKGuidFindBox.SuspendLayout();
			this.RecipientServiceDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 209, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(377, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.UniversalData.ManualDataExport);
			// 
			// Calc_RecipientTypeDropEdit
			// 
			this.Calc_RecipientTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.Calc_RecipientTypeDropEdit, "Calc_RecipientType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.UniversalData.ManualDataExport)(null)).Calc_RecipientType)));
			this.Calc_RecipientTypeDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ManualDataExportForm|f8a748d5-0c1a-4930-b6e7-2e8f1510db83", "Recipient Type", "Recipient Type to send XML Universal Data to. NB: The Recipient Organization must have Communications Modes setup before sending Universal Data.");
			this.Calc_RecipientTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 12, true);
			this.Calc_RecipientTypeDropEdit.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
			this.Calc_RecipientTypeDropEdit.Name = "Calc_RecipientTypeDropEdit";
			this.Calc_RecipientTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 18, true);
			this.Calc_RecipientTypeDropEdit.TabIndex = 1;
			// 
			// EventTypeDropEdit
			// 
			this.EventTypeDropEdit.AllowDrop = true;
			this.EventTypeDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.EventTypeDropEdit, "EventCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.UniversalData.ManualDataExport)(null)).EventCode)));
			this.EventTypeDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ManualDataExportForm|44e6bc26-cd6a-456d-a8f6-3040f6363fc8", "Event Type", "Event Type sent with the XML Universal Data.");
			this.EventTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 103, true);
			this.EventTypeDropEdit.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
			this.EventTypeDropEdit.Name = "EventTypeDropEdit";
			this.EventTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 18, true);
			this.EventTypeDropEdit.TabIndex = 4;
			// 
			// PurposeCodeDropEdit
			// 
			this.PurposeCodeDropEdit.AllowDrop = true;
			this.PurposeCodeDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.PurposeCodeDropEdit, "PurposeCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.UniversalData.ManualDataExport)(null)).PurposeCode)));
			this.PurposeCodeDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ManualDataExportForm|eeb02f04-a1bb-4954-9cfb-5a487c4dcb9c", "Purpose Code", "Purpose Code  sent with the XML Universal Data.");
			this.PurposeCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 125, true);
			this.PurposeCodeDropEdit.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
			this.PurposeCodeDropEdit.Name = "PurposeCodeDropEdit";
			this.PurposeCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 18, true);
			this.PurposeCodeDropEdit.TabIndex = 5;
			// 
			// TriggerDescriptionTextBox
			// 
			this.TriggerDescriptionTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.TriggerDescriptionTextBox, "TriggerDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UniversalData.ManualDataExport)(null)).TriggerDescription)));
			this.TriggerDescriptionTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ManualDataExportForm|b057e2d6-ca79-42ec-bd8e-eb8a975b2a49", "Trigger Description", "Trigger Description sent with the XML Universal Data.");
			this.TriggerDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 147, true);
			this.TriggerDescriptionTextBox.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
			this.TriggerDescriptionTextBox.Name = "TriggerDescriptionTextBox";
			this.TriggerDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 18, true);
			this.TriggerDescriptionTextBox.TabIndex = 6;
			// 
			// SendAndCloseButton
			// 
			this.SendAndCloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.SendAndCloseButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ManualDataExportForm|d92beadd-1213-4739-8af5-e95659dcf8d0", "Send && Close", "Send XML Universal Data message and if successful, close the form.");
			this.SendAndCloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(210, 177, true);
			this.SendAndCloseButton.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
			this.SendAndCloseButton.Name = "SendAndCloseButton";
			this.SendAndCloseButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.SendAndCloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(82, 23, true);
			this.SendAndCloseButton.TabIndex = 7;
			this.SendAndCloseButton.ToolTipCaption = null;
			this.SendAndCloseButton.UseVisualStyleBackColor = true;
			this.SendAndCloseButton.Click += new System.EventHandler(this.SendAndCloseButton_Click);
			// 
			// CloseButton
			// 
			this.CloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.CloseButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ManualDataExportForm|b096f692-aa32-446f-b73d-814e70b5a2cb", "Close", "Close the form.");
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(294, 177, true);
			this.CloseButton.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(82, 23, true);
			this.CloseButton.TabIndex = 8;
			this.CloseButton.ToolTipCaption = null;
			this.CloseButton.UseVisualStyleBackColor = true;
			this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
			// 
			// RecipientTypeDropEdit
			// 
			this.RecipientTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.RecipientTypeDropEdit, "RecipientType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.UniversalData.ManualDataExport)(null)).RecipientType)));
			this.RecipientTypeDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("d1a18f8f-89bd-43df-a95b-a05deb6ce87c", "Alternate Recipient Type", "Alternate Recipient Type to send XML Universal Data to. NB: The Recipient Organization must have Communications Modes setup before sending Universal Data.");
			this.RecipientTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 34, true);
			this.RecipientTypeDropEdit.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
			this.RecipientTypeDropEdit.Name = "RecipientTypeDropEdit";
			this.RecipientTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 18, true);
			this.RecipientTypeDropEdit.TabIndex = 2;
			// 
			// RecipientPKGuidFindBox
			// 
			this.RecipientPKGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.RecipientPKGuidFindBox, "RecipientPK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.UniversalData.ManualDataExport)(null)).RecipientPK)));
			this.RecipientPKGuidFindBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("d643ecf4-2ed6-4db6-b776-13e5b376ad20", "Recipient Organization");
			this.RecipientPKGuidFindBox.IsPrimaryKeyFromCodeRequired = false;
			this.RecipientPKGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 57, true);
			this.RecipientPKGuidFindBox.Name = "RecipientPKGuidFindBox";
			this.RecipientPKGuidFindBox.PreBoundMaxLength = 10;
			this.RecipientPKGuidFindBox.ShouldResize = true;
			this.RecipientPKGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 18, true);
			this.RecipientPKGuidFindBox.TabIndex = 3;
			// 
			// RecipientServiceDropEdit
			// 
			this.RecipientServiceDropEdit.AllowDrop = true;
			this.RecipientServiceDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.RecipientServiceDropEdit, "RecipientService");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.UniversalData.ManualDataExport)(null)).RecipientService)));
			this.RecipientServiceDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("90a8328f-2c56-41ad-beb8-79a14c05f449", "Recipient Service");
			this.RecipientServiceDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 80, true);
			this.RecipientServiceDropEdit.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
			this.RecipientServiceDropEdit.Name = "RecipientServiceDropEdit";
			this.RecipientServiceDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 18, true);
			this.RecipientServiceDropEdit.TabIndex = 2;
			// 
			// ManualDataExportForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(377, 233, true);
			this.Controls.Add(this.RecipientServiceDropEdit);
			this.Controls.Add(this.RecipientPKGuidFindBox);
			this.Controls.Add(this.RecipientTypeDropEdit);
			this.Controls.Add(this.Calc_RecipientTypeDropEdit);
			this.Controls.Add(this.TriggerDescriptionTextBox);
			this.Controls.Add(this.CloseButton);
			this.Controls.Add(this.SendAndCloseButton);
			this.Controls.Add(this.PurposeCodeDropEdit);
			this.Controls.Add(this.EventTypeDropEdit);
			this.DataSourceType = typeof(Enterprise.MasterFiles.Business.UniversalData.ManualDataExport);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Name = "ManualDataExportForm";
			this.Text = "ManualDataExportForm";
			this.Controls.SetChildIndex(this.EventTypeDropEdit, 0);
			this.Controls.SetChildIndex(this.PurposeCodeDropEdit, 0);
			this.Controls.SetChildIndex(this.SendAndCloseButton, 0);
			this.Controls.SetChildIndex(this.CloseButton, 0);
			this.Controls.SetChildIndex(this.TriggerDescriptionTextBox, 0);
			this.Controls.SetChildIndex(this.Calc_RecipientTypeDropEdit, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.RecipientTypeDropEdit, 0);
			this.Controls.SetChildIndex(this.RecipientPKGuidFindBox, 0);
			this.Controls.SetChildIndex(this.RecipientServiceDropEdit, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.Calc_RecipientTypeDropEdit.ResumeLayout(true);
			this.Calc_RecipientTypeDropEdit.PerformLayout();
			this.EventTypeDropEdit.ResumeLayout(true);
			this.EventTypeDropEdit.PerformLayout();
			this.PurposeCodeDropEdit.ResumeLayout(true);
			this.PurposeCodeDropEdit.PerformLayout();
			this.RecipientTypeDropEdit.ResumeLayout(true);
			this.RecipientTypeDropEdit.PerformLayout();
			this.RecipientPKGuidFindBox.ResumeLayout(true);
			this.RecipientPKGuidFindBox.PerformLayout();
			this.RecipientServiceDropEdit.ResumeLayout(true);
			this.RecipientServiceDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.GUI.ZDropEdit Calc_RecipientTypeDropEdit;
		internal ZArchitecture.GUI.ZDropEdit EventTypeDropEdit;
		internal ZArchitecture.GUI.ZDropEdit PurposeCodeDropEdit;
		internal ZArchitecture.ZTextBox TriggerDescriptionTextBox;
		internal ZArchitecture.GUI.ZButton SendAndCloseButton;
		internal ZArchitecture.GUI.ZButton CloseButton;
		internal ZArchitecture.GUI.ZDropEdit RecipientTypeDropEdit;
		internal ZArchitecture.GUI.ZGuidFindBox RecipientPKGuidFindBox;
		internal ZArchitecture.GUI.ZDropEdit RecipientServiceDropEdit;
	}
}
