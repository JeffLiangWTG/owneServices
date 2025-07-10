
namespace Enterprise.Customs.US.GUI
{
	partial class FZEventMessageSendingForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		///// <summary>
		///// Clean up any resources being used.
		///// </summary>
		///// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		//protected override void Dispose(bool disposing)
		//{
		//	if (disposing && (components != null))
		//	{
		//		components.Dispose();
		//	}
		//	base.Dispose(disposing);
		//}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private new void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.SendButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.GiveUpButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ActionCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.MessageSendingObjectsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.FTZActionPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.FTZUnconcurrencePanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.ReasonsTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.PhoneNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ContactNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ReasonCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.MessageSendingObjectsGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ActionCodeDropEdit.SuspendLayout();
			this.MessageSendingObjectsGroupBox.SuspendLayout();
			this.FTZActionPanel.SuspendLayout();
			this.FTZUnconcurrencePanel.SuspendLayout();
			this.ReasonCodeDropEdit.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageSendingObjectsGrid)).BeginInit();
			this.MessageSendingObjectsGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 312, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(632, 24, true);
			this.MainStatusBar.TabIndex = 3;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.Business.MessageBuilders.FZEventAction);
			// 
			// SendButton
			// 
			this.SendButton.IsCaptionOverridden = true;
			this.SendButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(463, 282, true);
			this.SendButton.Name = "SendButton";
			this.SendButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.SendButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.SendButton.TabIndex = 1;
			this.SendButton.Text = "&Send";
			this.SendButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.SendButton.ToolTipCaption = null;
			this.SendButton.UseVisualStyleBackColor = true;
			this.SendButton.Click += new System.EventHandler(this.SendButton_Click);
			// 
			// GiveUpButton
			// 
			this.GiveUpButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.GiveUpButton.IsCaptionOverridden = true;
			this.GiveUpButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(544, 282, true);
			this.GiveUpButton.Name = "GiveUpButton";
			this.GiveUpButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.GiveUpButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.GiveUpButton.TabIndex = 2;
			this.GiveUpButton.Text = "&Cancel";
			this.GiveUpButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.GiveUpButton.ToolTipCaption = null;
			this.GiveUpButton.UseVisualStyleBackColor = true;
			this.GiveUpButton.Click += new System.EventHandler(this.GiveUpButton_Click);
			// 
			// ActionCodeDropEdit
			// 
			this.ActionCodeDropEdit.AllowDrop = true;
			this.ActionCodeDropEdit.AutoValidate = System.Windows.Forms.AutoValidate.EnableAllowFocusChange;
			this.BindingSource.SetBindingMember(this.ActionCodeDropEdit, "US_ActionCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.MessageBuilders.FZEventAction)(null)).US_ActionCode)));
			this.ActionCodeDropEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("aaac8d74-d2ea-44b0-9427-b6d788ac2172", "Action Code", "Code indicating event: concurrence or delivery of goods.");
			this.ActionCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(77, 12, true);
			this.ActionCodeDropEdit.Name = "ActionCodeDropEdit";
			this.ActionCodeDropEdit.PreBoundMaxLength = 2;
			this.ActionCodeDropEdit.ShouldResizeByMaxLength = true;
			this.ActionCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(542, 20, true);
			this.ActionCodeDropEdit.TabIndex = 0;
			// 
			// MessageSendingObjectsGroupBox
			// 
			this.MessageSendingObjectsGroupBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("42b04edc-115e-443e-a58b-0e8a44d8c32c", "Action Options");
			this.MessageSendingObjectsGroupBox.Controls.Add(this.MessageSendingObjectsGrid);
			this.MessageSendingObjectsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 38, true);
			this.MessageSendingObjectsGroupBox.Name = "MessageSendingObjectsGroupBox";
			this.MessageSendingObjectsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(607, 234, true);
			this.MessageSendingObjectsGroupBox.TabIndex = 4;
			this.MessageSendingObjectsGroupBox.TabStop = false;
			this.MessageSendingObjectsGroupBox.Text = "Action Options";
			// 
			// FTZActionPanel
			// 
			this.FTZActionPanel.Controls.Add(this.ActionCodeDropEdit);
			this.FTZActionPanel.Controls.Add(this.MessageSendingObjectsGroupBox);
			this.FTZActionPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 1, true);
			this.FTZActionPanel.Name = "FTZActionPanel";
			this.FTZActionPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(632, 275, true);
			this.FTZActionPanel.TabIndex = 1;
			// 
			// FTZUnconcurrencePanel
			// 
			this.FTZUnconcurrencePanel.Controls.Add(this.ReasonsTextBox);
			this.FTZUnconcurrencePanel.Controls.Add(this.PhoneNumberTextBox);
			this.FTZUnconcurrencePanel.Controls.Add(this.ContactNameTextBox);
			this.FTZUnconcurrencePanel.Controls.Add(this.ReasonCodeDropEdit);
			this.FTZUnconcurrencePanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 100, true);
			this.FTZUnconcurrencePanel.Name = "FTZUnconcurrencePanel";
			this.FTZUnconcurrencePanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(632, 114, true);
			this.FTZUnconcurrencePanel.TabIndex = 1;
			// 
			// ReasonsTextBox
			// 
			this.BindingSource.SetBindingMember(this.ReasonsTextBox, "US_Reasons");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.MessageBuilders.FZEventAction)(null)).US_Reasons)));
			this.ReasonsTextBox.CaptionResourceString = null;
			this.ReasonsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(186, 86, true);
			this.ReasonsTextBox.Name = "ReasonsTextBox";
			this.ReasonsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(269, 20, true);
			this.ReasonsTextBox.TabIndex = 3;
			this.ReasonsTextBox.Visible = false;
			// 
			// PhoneNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.PhoneNumberTextBox, "US_FTZContactPhone");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.MessageBuilders.FZEventAction)(null)).US_FTZContactPhone)));
			this.PhoneNumberTextBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("1048619d-cd13-44f8-b0cd-33ae5da519db", "Phone Number");
			this.PhoneNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(186, 35, true);
			this.PhoneNumberTextBox.Name = "PhoneNumberTextBox";
			this.PhoneNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(269, 20, true);
			this.PhoneNumberTextBox.TabIndex = 1;
			// 
			// ContactNameTextBox
			// 
			this.BindingSource.SetBindingMember(this.ContactNameTextBox, "US_FTZContactName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.MessageBuilders.FZEventAction)(null)).US_FTZContactName)));
			this.ContactNameTextBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("f6fd4a15-cf84-4de1-be16-1d6ae3e307ef", "Contact Name");
			this.ContactNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(186, 9, true);
			this.ContactNameTextBox.Name = "ContactNameTextBox";
			this.ContactNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(269, 20, true);
			this.ContactNameTextBox.TabIndex = 0;
			// 
			// ReasonCodeDropEdit
			// 
			this.ReasonCodeDropEdit.AllowDrop = true;
			this.ReasonCodeDropEdit.AutoValidate = System.Windows.Forms.AutoValidate.EnableAllowFocusChange;
			this.BindingSource.SetBindingMember(this.ReasonCodeDropEdit, "US_ReasonCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.MessageBuilders.FZEventAction)(null)).US_ReasonCode)));
			this.ReasonCodeDropEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("011d622f-6a9e-4116-896b-d42606c40d44", "Reason Code");
			this.ReasonCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(186, 60, true);
			this.ReasonCodeDropEdit.Name = "ReasonCodeDropEdit";
			this.ReasonCodeDropEdit.PreBoundMaxLength = 2;
			this.ReasonCodeDropEdit.ShouldResizeByMaxLength = true;
			this.ReasonCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(269, 20, true);
			this.ReasonCodeDropEdit.TabIndex = 2;
			// 
			// MessageSendingObjectsGrid
			// 
			this.MessageSendingObjectsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.MessageSendingObjectsGrid, "MessageSendingObjectsView");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.Business.MessageBuilders.FZEventAction)(null)).MessageSendingObjectsView)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.Business.MessageBuilders.FZConcurrenceMessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.US.Business.MessageBuilders.FZEventAction)(null)).MessageSendingObjectsView)).SyncRoot)).MB_Send)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.MessageBuilders.FZConcurrenceMessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.US.Business.MessageBuilders.FZEventAction)(null)).MessageSendingObjectsView)).SyncRoot)).MB_Identifier)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.Business.MessageBuilders.FZConcurrenceMessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.US.Business.MessageBuilders.FZEventAction)(null)).MessageSendingObjectsView)).SyncRoot)).MB_ManifestQty)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.MessageBuilders.FZConcurrenceMessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.US.Business.MessageBuilders.FZEventAction)(null)).MessageSendingObjectsView)).SyncRoot)).MB_ManifestUQ)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.Business.MessageBuilders.FZConcurrenceMessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.US.Business.MessageBuilders.FZEventAction)(null)).MessageSendingObjectsView)).SyncRoot)).MB_ConcurrenceQty)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.MessageBuilders.FZConcurrenceMessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.US.Business.MessageBuilders.FZEventAction)(null)).MessageSendingObjectsView)).SyncRoot)).MB_ConcurrenceUQ)));
			this.MessageSendingObjectsGrid.CaptionVisible = false;
			zCheckBoxColumnStyleInfo1.ColumnName = "MB_Send";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo1.ColumnName = "MB_Identifier";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "MB_ManifestQty";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo1.ColumnName = "MB_ManifestUQ";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "MB_ConcurrenceQty";
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zDropEditColumnStyleInfo2.ColumnName = "MB_ConcurrenceUQ";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			this.MessageSendingObjectsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.MessageSendingObjectsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.MessageSendingObjectsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.MessageSendingObjectsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.MessageSendingObjectsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.MessageSendingObjectsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.MessageSendingObjectsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MessageSendingObjectsGrid.GridId = "eef39014-b371-4fd5-8b46-204074b6763f";
			this.MessageSendingObjectsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.MessageSendingObjectsGrid.LayoutKey = "zGrid1";
			this.MessageSendingObjectsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.MessageSendingObjectsGrid.Name = "MessageSendingObjectsGrid";
			this.MessageSendingObjectsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(601, 215, true);
			this.MessageSendingObjectsGrid.TabIndex = 0;
			// 
			// FZEventMessageSendingForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(632, 336, true);
			this.Controls.Add(this.FTZActionPanel);
			this.Controls.Add(this.GiveUpButton);
			this.Controls.Add(this.SendButton);
			this.Controls.Add(this.FTZUnconcurrencePanel);
			this.DataSourceAssemblyName = "Enterprise.Customs.US.Business";
			this.DataSourceType = typeof(Enterprise.Customs.US.Business.MessageBuilders.FZEventAction);
			this.DataSourceTypeName = "Enterprise.Customs.US.Business.MessageBuilders.FZEventAction";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Name = "FZEventMessageSendingForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "FZ Event Reporting";
			this.Controls.SetChildIndex(this.FTZUnconcurrencePanel, 0);
			this.Controls.SetChildIndex(this.SendButton, 0);
			this.Controls.SetChildIndex(this.GiveUpButton, 0);
			this.Controls.SetChildIndex(this.FTZActionPanel, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ActionCodeDropEdit.ResumeLayout(true);
			this.ActionCodeDropEdit.PerformLayout();
			this.MessageSendingObjectsGroupBox.ResumeLayout(false);
			this.MessageSendingObjectsGroupBox.PerformLayout();
			this.FTZActionPanel.ResumeLayout(false);
			this.FTZActionPanel.PerformLayout();
			this.FTZUnconcurrencePanel.ResumeLayout(false);
			this.FTZUnconcurrencePanel.PerformLayout();
			this.ReasonCodeDropEdit.ResumeLayout(true);
			this.ReasonCodeDropEdit.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageSendingObjectsGrid)).EndInit();
			this.MessageSendingObjectsGrid.ResumeLayout(false);
			this.MessageSendingObjectsGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal Enterprise.ZArchitecture.GUI.ZButton SendButton;
		internal Enterprise.ZArchitecture.GUI.ZButton GiveUpButton;
		private ZArchitecture.GUI.ZDropEdit ActionCodeDropEdit;
		private ZArchitecture.GUI.ZGroupBox MessageSendingObjectsGroupBox;
		private ZArchitecture.ZGrid MessageSendingObjectsGrid;
		private ZArchitecture.GUI.ZPanel FTZActionPanel;
		private ZArchitecture.GUI.ZPanel FTZUnconcurrencePanel;
		private ZArchitecture.GUI.ZDropEdit ReasonCodeDropEdit;
		private ZArchitecture.ZTextBox ContactNameTextBox;
		private ZArchitecture.ZTextBox PhoneNumberTextBox;
		private ZArchitecture.ZTextBox ReasonsTextBox;
	}
}
