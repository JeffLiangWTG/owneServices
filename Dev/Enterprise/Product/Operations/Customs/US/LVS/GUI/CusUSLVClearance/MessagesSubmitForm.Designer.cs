namespace Enterprise.Customs.US.LVS.GUI
{
	partial class MessagesSubmitForm
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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2222:DoNotDecreaseInheritedMemberVisibility")]
		private new void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.ButtonsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.ButtonSelectAllValid = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ButtonSelectAll = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ButtonSend = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ButtonCancel = new Enterprise.ZArchitecture.GUI.ZButton();
			this.GridCusUSLVConsignments = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ButtonsPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.GridCusUSLVConsignments)).BeginInit();
			this.GridCusUSLVConsignments.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 264, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(500, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.LVS.Business.CusUSLVClearanceMessageWrapper);
			// 
			// ButtonsPanel
			// 
			this.ButtonsPanel.Controls.Add(this.ButtonSelectAllValid);
			this.ButtonsPanel.Controls.Add(this.ButtonSelectAll);
			this.ButtonsPanel.Controls.Add(this.ButtonSend);
			this.ButtonsPanel.Controls.Add(this.ButtonCancel);
			this.ButtonsPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.ButtonsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 214, true);
			this.ButtonsPanel.Name = "ButtonsPanel";
			this.ButtonsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(500, 50, true);
			this.ButtonsPanel.TabIndex = 3;
			// 
			// ButtonSelectAllValid
			// 
			this.ButtonSelectAllValid.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.ButtonSelectAllValid.IsCaptionOverridden = true;
			this.ButtonSelectAllValid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(143, 12, true);
			this.ButtonSelectAllValid.Name = "ButtonSelectAllValid";
			this.ButtonSelectAllValid.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.ButtonSelectAllValid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(180, 24, true);
			this.ButtonSelectAllValid.TabIndex = 1;
			this.ButtonSelectAllValid.Text = "Select/Deselect All Valid Entries";
			this.ButtonSelectAllValid.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.ButtonSelectAllValid.ToolTipCaption = null;
			this.ButtonSelectAllValid.UseVisualStyleBackColor = true;
			this.ButtonSelectAllValid.Click += new System.EventHandler(this.ButtonSelectAllValid_Click);
			// 
			// ButtonSelectAll	
			// 
			this.ButtonSelectAll.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.ButtonSelectAll.IsCaptionOverridden = true;
			this.ButtonSelectAll.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 12, true);
			this.ButtonSelectAll.Name = "ButtonSelectAll";
			this.ButtonSelectAll.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.ButtonSelectAll.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(125, 24, true);
			this.ButtonSelectAll.TabIndex = 0;
			this.ButtonSelectAll.Text = "Select/Deselect All";
			this.ButtonSelectAll.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.ButtonSelectAll.ToolTipCaption = null;
			this.ButtonSelectAll.UseVisualStyleBackColor = true;
			this.ButtonSelectAll.Click += new System.EventHandler(this.ButtonSelectAll_Click);
			// 
			// ButtonSend
			// 
			this.ButtonSend.Anchor = System.Windows.Forms.AnchorStyles.Right;
			this.ButtonSend.IsCaptionOverridden = true;
			this.ButtonSend.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(329, 13, true);
			this.ButtonSend.Name = "ButtonSend";
			this.ButtonSend.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.ButtonSend.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.ButtonSend.TabIndex = 2;
			this.ButtonSend.Text = "Send";
			this.ButtonSend.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.ButtonSend.ToolTipCaption = null;
			this.ButtonSend.UseVisualStyleBackColor = true;
			this.ButtonSend.Click += new System.EventHandler(this.SendButton_Click);
			// 
			// ButtonCancel
			// 
			this.ButtonCancel.Anchor = System.Windows.Forms.AnchorStyles.Right;
			this.ButtonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.ButtonCancel.IsCaptionOverridden = true;
			this.ButtonCancel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(413, 13, true);
			this.ButtonCancel.Name = "ButtonCancel";
			this.ButtonCancel.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.ButtonCancel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.ButtonCancel.TabIndex = 2;
			this.ButtonCancel.Text = "Cancel";
			this.ButtonCancel.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.ButtonCancel.ToolTipCaption = null;
			this.ButtonCancel.UseVisualStyleBackColor = true;
			this.ButtonCancel.Click += new System.EventHandler(this.CancelButton_Click);
			// 
			// GridCusUSLVConsignments
			// 
			this.GridCusUSLVConsignments.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.GridCusUSLVConsignments, "CusUSLVConsignmentsToSend");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.LVS.Business.CusUSLVClearance)(null)).CusUSLVConsignmentsToSend)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignmentForMessaging)(((System.Collections.IList)(((Enterprise.Customs.US.LVS.Business.CusUSLVClearance)(null)).CusUSLVConsignmentsToSend)).SyncRoot)).HouseBill)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignmentForMessaging)(((System.Collections.IList)(((Enterprise.Customs.US.LVS.Business.CusUSLVClearance)(null)).CusUSLVConsignmentsToSend)).SyncRoot)).SendToCustoms)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignmentForMessaging)(((System.Collections.IList)(((Enterprise.Customs.US.LVS.Business.CusUSLVClearance)(null)).CusUSLVConsignmentsToSend)).SyncRoot)).ReasonCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignmentForMessaging)(((System.Collections.IList)(((Enterprise.Customs.US.LVS.Business.CusUSLVClearance)(null)).CusUSLVConsignmentsToSend)).SyncRoot)).RequiredReference)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignmentForMessaging)(((System.Collections.IList)(((Enterprise.Customs.US.LVS.Business.CusUSLVClearance)(null)).CusUSLVConsignmentsToSend)).SyncRoot)).ReferenceNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignmentForMessaging)(((System.Collections.IList)(((Enterprise.Customs.US.LVS.Business.CusUSLVClearance)(null)).CusUSLVConsignmentsToSend)).SyncRoot)).FilesSubmittedToDIS)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignmentForMessaging)(((System.Collections.IList)(((Enterprise.Customs.US.LVS.Business.CusUSLVClearance)(null)).CusUSLVConsignmentsToSend)).SyncRoot)).DISReference)));

			this.GridCusUSLVConsignments.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("3F54FE81-A273-4995-BC13-9A0A6D4024F2", "House Bill");
			zTextBoxColumnStyleInfo1.ColumnName = "HouseBill";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("BFA275A7-9A4B-4183-A97C-92950018091F", "Send ?");
			zCheckBoxColumnStyleInfo1.ColumnName = "SendToCustoms";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("82ae111b-930b-437b-a3dd-c6d64baa2c5c", "Reason Code");
			zDropEditColumnStyleInfo1.ColumnName = "ReasonCode";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			zDropEditColumnStyleInfo1.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.ShowCodeAndDescription;
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("3023cf42-2d62-4ff1-bc4b-419e6b1ec18e", "Required Reference");
			zTextBoxColumnStyleInfo2.ColumnName = "RequiredReference";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("1d87327e-2a16-45f8-a3d5-4f1f86fce9b9", "Reference Number");
			zTextBoxColumnStyleInfo3.ColumnName = "ReferenceNumber";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			zCheckBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("43d9bebb-fdfa-4f81-8180-86e65ca2e2eb", "Files submitted to DIS");
			zCheckBoxColumnStyleInfo2.ColumnName = "FilesSubmittedToDIS";
			zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("19de9ce2-2e6c-46f5-b613-89abdba5d124", "DIS Reference");
			zTextBoxColumnStyleInfo4.ColumnName = "DISReference";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			this.GridCusUSLVConsignments.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.GridCusUSLVConsignments.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.GridCusUSLVConsignments.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.GridCusUSLVConsignments.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.GridCusUSLVConsignments.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.GridCusUSLVConsignments.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.GridCusUSLVConsignments.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.GridCusUSLVConsignments.Dock = System.Windows.Forms.DockStyle.Fill;
			this.GridCusUSLVConsignments.GridId = "ece0badf-5b94-4e68-9f0d-c247105b263f";
			this.GridCusUSLVConsignments.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.GridCusUSLVConsignments.LayoutKey = "GridCusUSLVConsignments";
			this.GridCusUSLVConsignments.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.GridCusUSLVConsignments.Name = "GridCusUSLVConsignments";
			this.GridCusUSLVConsignments.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.GridCusUSLVConsignments.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(500, 214, true);
			this.GridCusUSLVConsignments.TabIndex = 1;
			this.GridCusUSLVConsignments.ColorContextKey = "CusUSLVConsignmentMessageSubmit";
			// 
			// MessagesSubmitForm
			// 
			this.AcceptButton = this.ButtonSend;
			this.CancelButton = this.ButtonCancel;
			this.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("813BF7B0-B51C-4B9D-9A69-624FF767ED7E", "Send Messages");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(500, 288, true);
			this.Controls.Add(this.GridCusUSLVConsignments);
			this.Controls.Add(this.ButtonsPanel);
			this.DataSourceType = typeof(Enterprise.Customs.US.LVS.Business.CusUSLVClearance);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(500, 288, true);
			this.Name = "MessagesSubmitForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.ButtonsPanel, 0);
			this.Controls.SetChildIndex(this.GridCusUSLVConsignments, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ButtonsPanel.ResumeLayout(false);
			this.ButtonsPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.GridCusUSLVConsignments)).EndInit();
			this.GridCusUSLVConsignments.ResumeLayout(false);
			this.GridCusUSLVConsignments.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZGrid GridCusUSLVConsignments;
		private ZArchitecture.GUI.ZButton ButtonSelectAll;
		private ZArchitecture.GUI.ZButton ButtonSend;
		private ZArchitecture.GUI.ZButton ButtonCancel;
		private ZArchitecture.GUI.ZPanel ButtonsPanel;
		private ZArchitecture.GUI.ZButton ButtonSelectAllValid;
	}
}
