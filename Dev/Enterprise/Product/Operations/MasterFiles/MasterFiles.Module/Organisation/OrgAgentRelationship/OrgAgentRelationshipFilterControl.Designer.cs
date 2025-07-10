namespace Enterprise.MasterFiles.Module
{
	public partial class OrgAgentRelationshipFilterControl
	{
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			this.Splitter = new CargoWise.Windows.UI.KSplitter();
			this.ProfitShareControl = new Enterprise.MasterFiles.GUI.ProfitShareAgreementControl();
			((System.ComponentModel.ISupportInitialize)(this.FilteredGrid)).BeginInit();
			this.FilteredGrid.SuspendLayout();
			this.AddStripButton.SuspendLayout();
			this.RecentItemsPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.Splitter.SuspendLayout();
			this.ProfitShareControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// FilteredGrid
			// 
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("OrgProfitShareDetailsFilterControl|6a1406eb-30d8-4471-a427-4cf344fc6870", "Type");
			zTextBoxColumnStyleInfo1.ColumnName = "O3_ProfitShareType";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("OrgProfitShareDetailsFilterControl|d6f2f063-2ef9-48f4-b829-655070c49fbd", "Description");
			zTextBoxColumnStyleInfo2.ColumnName = "Description";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			zGuidFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("OrgProfitShareDetailsFilterControl|688df79e-318e-4eb1-8398-0c8956e639b1", "Send Agt.", "Send Agent");
			zGuidFindBoxColumnStyleInfo1.ColumnName = "O3_OH_SendingAgent";
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("OrgProfitShareDetailsFilterControl|cd797b19-3a26-4f0e-9c89-463d07ab2d68", "Sending Agent Name");
			zTextBoxColumnStyleInfo3.ColumnName = "SendingAgentName";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			zGuidFindBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("OrgProfitShareDetailsFilterControl|4353d5fa-b0d1-4022-9514-a7e10ea6f2f9", "Rcv. Agt.", "Receive Agent");
			zGuidFindBoxColumnStyleInfo2.ColumnName = "O3_OH_ReceivingAgent";
			zGuidFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("OrgProfitShareDetailsFilterControl|fa46ff75-0cff-4cb2-875c-3bf1c1ba5cc4", "Receiving Agent Name");
			zTextBoxColumnStyleInfo4.ColumnName = "ReceivingAgentName";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			zGuidFindBoxColumnStyleInfo3.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("OrgProfitShareDetailsFilterControl|efed608c-3d17-4739-bb2a-b47fb932a740", "Head Office/Franchisor");
			zGuidFindBoxColumnStyleInfo3.ColumnName = "O3_OH_GroupNetworkOrFranchise";
			zGuidFindBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.FilteredGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.FilteredGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo2);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.FilteredGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo3);
			this.FilteredGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 109, true);
			this.FilteredGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(680, 419, true);
			this.FilteredGrid.TabIndex = 5;
			this.FilteredGrid.AfterBind += new System.EventHandler(this.FilteredGrid_AfterBind);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.OrgAgentRelationship);
			// 
			// Splitter
			// 
			this.Splitter.Cursor = System.Windows.Forms.Cursors.HSplit;
			this.Splitter.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.Splitter.DoNotSaveSplitterLayout = false;
			this.Splitter.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 328, true);
			this.Splitter.MinExtra = 0;
			this.Splitter.MinSize = 0;
			this.Splitter.Name = "Splitter";
			this.Splitter.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(680, 8, true);
			this.Splitter.TabIndex = 6;
			this.Splitter.TabStop = false;
			this.Splitter.SplitterMoved += new System.Windows.Forms.SplitterEventHandler(this.Splitter_SplitterMoved);
			// 
			// ProfitShareControl
			// 
			this.ProfitShareControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ProfitShareControl, "OrgProfitShareDetailsCollection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Module.OrgAgentRelationshipFilterBusinessObject)(null)).OrgProfitShareDetailsCollection)));
			this.ProfitShareControl.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.ProfitShareControl.IncludeOrgOverrideColumn = true;
			this.ProfitShareControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 336, true);
			this.ProfitShareControl.Name = "ProfitShareControl";
			this.ProfitShareControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(680, 192, true);
			this.ProfitShareControl.TabIndex = 7;
			// 
			// OrgAgentRelationshipFilterControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.Splitter);
			this.Controls.Add(this.ProfitShareControl);
			this.Name = "OrgAgentRelationshipFilterControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(744, 480, true);
			this.Controls.SetChildIndex(this.Splitter, 0);
			this.Controls.SetChildIndex(this.ToolStripPermissionsLabel, 0);
			this.Controls.SetChildIndex(this.FilterStripsPanel, 0);
			this.Controls.SetChildIndex(this.AddStripButton, 0);
			this.Controls.SetChildIndex(this.ToolStripRecordsFoundLabel, 0);
			this.Controls.SetChildIndex(this.FilteredGrid, 0);
			((System.ComponentModel.ISupportInitialize)(this.FilteredGrid)).EndInit();
			this.FilteredGrid.ResumeLayout(false);
			this.FilteredGrid.PerformLayout();
			this.AddStripButton.ResumeLayout(true);
			this.AddStripButton.PerformLayout();
			this.RecentItemsPanel.ResumeLayout(false);
			this.RecentItemsPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.Splitter.ResumeLayout(false);
			this.Splitter.PerformLayout();
			this.ProfitShareControl.ResumeLayout(true);
			this.ProfitShareControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		public Enterprise.MasterFiles.GUI.ProfitShareAgreementControl ProfitShareControl;
		CargoWise.Windows.UI.KSplitter Splitter;
	}
}
