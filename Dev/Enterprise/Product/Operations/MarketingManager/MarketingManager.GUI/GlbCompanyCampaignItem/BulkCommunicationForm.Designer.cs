namespace Enterprise.MarketingManager.GUI
{
	partial class BulkCommunicationForm
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
		new void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.Internal.ZTimeEditExColumnStyleInfo zTimeEditExColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.Internal.ZTimeEditExColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.postingButtonsUserControl = new Enterprise.Core.Forms.ZPostingButtonsUserControl();
			this.bottomPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.bulkCommunicationControlPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.DataEntryPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.bulkCommunicationEntryUserControl1 = new Enterprise.MarketingManager.GUI.BulkCommunicationEntryUserControl();
			this.UpdateSelectedButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.BottomGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CommunicationGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.bottomPanel.SuspendLayout();
			this.bulkCommunicationControlPanel.SuspendLayout();
			this.DataEntryPanel.SuspendLayout();
			this.BottomGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.CommunicationGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 496, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1032, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MarketingManager.Business.BulkCommunication);
			// 
			// postingButtonsUserControl
			// 
			this.postingButtonsUserControl.AllowDrop = true;
			this.postingButtonsUserControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.postingButtonsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(788, 1, true);
			this.postingButtonsUserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.postingButtonsUserControl.Name = "postingButtonsUserControl";
			this.postingButtonsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.postingButtonsUserControl.TabIndex = 0;
			// 
			// bottomPanel
			// 
			this.bottomPanel.Controls.Add(this.postingButtonsUserControl);
			this.bottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.bottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 520, true);
			this.bottomPanel.Name = "bottomPanel";
			this.bottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1032, 28, true);
			this.bottomPanel.TabIndex = 1;
			// 
			// bulkCommunicationControlPanel
			// 
			this.bulkCommunicationControlPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.bulkCommunicationControlPanel.Controls.Add(this.DataEntryPanel);
			this.bulkCommunicationControlPanel.Controls.Add(this.UpdateSelectedButton);
			this.bulkCommunicationControlPanel.Controls.Add(this.BottomGroupBox);
			this.bulkCommunicationControlPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.bulkCommunicationControlPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.bulkCommunicationControlPanel.Name = "bulkCommunicationControlPanel";
			this.bulkCommunicationControlPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1032, 520, true);
			this.bulkCommunicationControlPanel.TabIndex = 2;
			// 
			// DataEntryPanel
			// 
			this.DataEntryPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right | System.Windows.Forms.AnchorStyles.Left)));
			this.DataEntryPanel.Controls.Add(this.bulkCommunicationEntryUserControl1);
			this.DataEntryPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 14, true);
			this.DataEntryPanel.Name = "DataEntryPanel";
			this.DataEntryPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1015, 165, true);
			this.DataEntryPanel.TabIndex = 6;
			// 
			// bulkCommunicationEntryUserControl1
			// 
			this.bulkCommunicationEntryUserControl1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.bulkCommunicationEntryUserControl1, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			this.bulkCommunicationEntryUserControl1.CaptionRenderingEnabled = true;
			this.bulkCommunicationEntryUserControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.bulkCommunicationEntryUserControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.bulkCommunicationEntryUserControl1.Name = "bulkCommunicationEntryUserControl1";
			this.bulkCommunicationEntryUserControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1015, 115, true);
			this.bulkCommunicationEntryUserControl1.TabIndex = 0;
			// 
			// UpdateSelectedButton
			// 
			this.UpdateSelectedButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.UpdateSelectedButton.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("d3c38abe-74b2-48f1-950c-699a03484d7d", "Update Selected");
			this.UpdateSelectedButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(903, 180, true);
			this.UpdateSelectedButton.Name = "UpdateSelectedButton";
			this.UpdateSelectedButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(116, 23, true);
			this.UpdateSelectedButton.TabIndex = 5;
			this.UpdateSelectedButton.UseVisualStyleBackColor = true;
			this.UpdateSelectedButton.Click += new System.EventHandler(this.UpdateSelectedButton_Click);
			// 
			// BottomGroupBox
			// 
			this.BottomGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BottomGroupBox.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("8d54bafc-2bc0-48ae-8d82-a63f3e41b3ff", "New Communications");
			this.BottomGroupBox.Controls.Add(this.CommunicationGrid);
			this.BottomGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(11, 205, true);
			this.BottomGroupBox.Name = "BottomGroupBox";
			this.BottomGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1008, 304, true);
			this.BottomGroupBox.TabIndex = 1;
			this.BottomGroupBox.TabStop = false;
			// 
			// CommunicationGrid
			// 
			this.CommunicationGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.CommunicationGrid, "CommunicationCollection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MarketingManager.Business.BulkCommunication)(null)).CommunicationCollection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgSalesCall)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.BulkCommunication)(null)).CommunicationCollection)).SyncRoot)).Header.OH_FullName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OrgSalesCall)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.BulkCommunication)(null)).CommunicationCollection)).SyncRoot)).OQ_OC)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgSalesCall)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.BulkCommunication)(null)).CommunicationCollection)).SyncRoot)).OQ_TypeOfCall)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSalesCall)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.BulkCommunication)(null)).CommunicationCollection)).SyncRoot)).Lookups.OQ_TypeOfCall_ActiveList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgSalesCall)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.BulkCommunication)(null)).CommunicationCollection)).SyncRoot)).OQ_Category)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSalesCall)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.BulkCommunication)(null)).CommunicationCollection)).SyncRoot)).Lookups.OQ_Category_ActiveList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgSalesCall)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.BulkCommunication)(null)).CommunicationCollection)).SyncRoot)).OQ_Status)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSalesCall)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.BulkCommunication)(null)).CommunicationCollection)).SyncRoot)).Lookups.OQ_Status_ActiveList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgSalesCall)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.BulkCommunication)(null)).CommunicationCollection)).SyncRoot)).OQ_GS_NKSalesRep)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSalesCall)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.BulkCommunication)(null)).CommunicationCollection)).SyncRoot)).Lookups.SalesReps)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgSalesCall)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.BulkCommunication)(null)).CommunicationCollection)).SyncRoot)).Location)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSalesCall)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.BulkCommunication)(null)).CommunicationCollection)).SyncRoot)).Lookups.LocationList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.MasterFiles.Business.OrgSalesCall)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.BulkCommunication)(null)).CommunicationCollection)).SyncRoot)).OQ_NextCallLocal)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.MasterFiles.Business.OrgSalesCall)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.BulkCommunication)(null)).CommunicationCollection)).SyncRoot)).OQ_CallDateLocal)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.MasterFiles.Business.OrgSalesCall)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.BulkCommunication)(null)).CommunicationCollection)).SyncRoot)).OQ_Duration)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgSalesCall)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.BulkCommunication)(null)).CommunicationCollection)).SyncRoot)).ShouldSendInvitation)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgSalesCall)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.BulkCommunication)(null)).CommunicationCollection)).SyncRoot)).OQ_CallSummary)));
			this.CommunicationGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("a29e84b7-d8ad-4ab5-9603-2135871ffec6", "Client Name");
			zTextBoxColumnStyleInfo1.ColumnName = "OrgName";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("3911d36c-0c3d-4ac6-b172-f39da0010958", "Primary Contact");
			zTextBoxColumnStyleInfo3.ColumnName = "ContactName";
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("021f14f5-a9a0-4be0-a051-66ebba50dc9a", "Method");
			zDropEditColumnStyleInfo1.BindToList = "Lookups.OQ_TypeOfCall_ActiveList";
			zDropEditColumnStyleInfo1.ColumnName = "OQ_TypeOfCall";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("4c6fb7f6-f2bb-4d03-85fa-5ec3fbdc385b", "Purpose");
			zDropEditColumnStyleInfo2.BindToList = "Lookups.OQ_Category_ActiveList";
			zDropEditColumnStyleInfo2.ColumnName = "OQ_Category";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zDropEditColumnStyleInfo3.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("977211a2-f329-476b-bc62-78e5998dbf82", "Status");
			zDropEditColumnStyleInfo3.BindToList = "Lookups.OQ_Status_ActiveList";
			zDropEditColumnStyleInfo3.ColumnName = "OQ_Status";
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zCodeFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("b229bf85-c855-4a6f-a677-e30c5565656d", "Staff", "Staff", "Staff Coordinator", "The staff who made the call");
			zCodeFindBoxColumnStyleInfo1.BindToList = "Lookups.SalesReps";
			zCodeFindBoxColumnStyleInfo1.ColumnName = "OQ_GS_NKSalesRep";
			zCodeFindBoxColumnStyleInfo1.Width = 50;
			zCodeFindBoxColumnStyleInfo1.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.GlbStaff;
			zDropEditColumnStyleInfo4.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("0b1c6a00-e8bd-4067-817b-0cf126c3c647", "Location");
			zDropEditColumnStyleInfo4.BindToList = "Lookups.LocationList";
			zDropEditColumnStyleInfo4.ColumnName = "Location";
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("a0c36b0c-8fcf-4d19-a2e7-fcf8e4ded945", "Schedule Date");
			zDateEditColumnStyleInfo1.ColumnName = "OQ_NextCallLocal";
			zDateEditColumnStyleInfo1.Width = 100;
			zDateEditColumnStyleInfo2.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("f39c33dd-a50a-4f79-8a37-3a0afaf9e87e", "Actual Date");
			zDateEditColumnStyleInfo2.ColumnName = "OQ_CallDateLocal";
			zDateEditColumnStyleInfo2.Width = 100;
			zTimeEditExColumnStyleInfo1.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("bf0c3f59-91fe-404c-ba39-abcd2534e554", "Duration");
			zTimeEditExColumnStyleInfo1.ColumnName = "OQ_Duration";
			zTimeEditExColumnStyleInfo1.AllowNegative = false;
			zTimeEditExColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("4a2a5b54-4e4e-42fa-9eea-81903b8dd1dd", "Send");
			zCheckBoxColumnStyleInfo1.ColumnName = "ShouldSendInvitation";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("8ce69dce-3114-4737-a355-b754e3187c92", "Subject");
			zTextBoxColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			zTextBoxColumnStyleInfo2.ColumnName = "OQ_CallSummary";
			zTextBoxColumnStyleInfo2.Width = 180;
			this.CommunicationGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.CommunicationGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.CommunicationGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.CommunicationGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.CommunicationGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.CommunicationGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.CommunicationGrid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
			this.CommunicationGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.CommunicationGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.CommunicationGrid.ColumnStyles.Add(zTimeEditExColumnStyleInfo1);
			this.CommunicationGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.CommunicationGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.CommunicationGrid.CopySelectedRowsAllowed = true;
			this.CommunicationGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CommunicationGrid.GridId = "bc8e2fb2-aa9a-4519-978f-ebf5e14b48f1";
			this.CommunicationGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.CommunicationGrid.LayoutKey = "CommunicationGrid";
			this.CommunicationGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.CommunicationGrid.Name = "CommunicationGrid";
			this.CommunicationGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1002, 249, true);
			this.CommunicationGrid.TabIndex = 0;
			// 
			// BulkCommunicationForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1200, 600, true);
			this.Controls.Add(this.bulkCommunicationControlPanel);
			this.Controls.Add(this.bottomPanel);
			this.DataSourceType = typeof(Enterprise.MarketingManager.Business.BulkCommunication);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1200, 600, true);
			this.Name = "BulkCommunicationForm";
			this.Controls.SetChildIndex(this.bottomPanel, 0);
			this.Controls.SetChildIndex(this.bulkCommunicationControlPanel, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.bottomPanel.ResumeLayout(false);
			this.bulkCommunicationControlPanel.ResumeLayout(false);
			this.DataEntryPanel.ResumeLayout(false);
			this.BottomGroupBox.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.CommunicationGrid)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private Core.Forms.ZPostingButtonsUserControl postingButtonsUserControl;
		private ZArchitecture.GUI.ZPanel bottomPanel;
		private ZArchitecture.GUI.ZPanel bulkCommunicationControlPanel;
		private ZArchitecture.GUI.ZGroupBox BottomGroupBox;
		private ZArchitecture.GUI.ZButton UpdateSelectedButton;
		internal ZArchitecture.ZGrid CommunicationGrid;
		private ZArchitecture.GUI.ZPanel DataEntryPanel;
		private BulkCommunicationEntryUserControl bulkCommunicationEntryUserControl1;

	}
}