namespace Enterprise.MasterFiles.GUI
{
	public partial class AccComplianceSequenceSplitForm
	{
		#region Windows Form Designer generated code

		public new void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo11 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo12 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo13 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo14 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			this.BottomPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.PostingButtonsUserControl = new Enterprise.Core.Forms.ZPostingButtonsUserControl();
			this.MainPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.MainGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.BottomPanel.SuspendLayout();
			this.PostingButtonsUserControl.SuspendLayout();
			this.MainPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MainGrid)).BeginInit();
			this.MainGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 140, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(590, 24, true);
			this.MainStatusBar.TabIndex = 2;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.AccComplianceSequenceSplitViewModel);
			// 
			// BottomPanel
			// 
			this.BottomPanel.Controls.Add(this.PostingButtonsUserControl);
			this.BottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.BottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 103, true);
			this.BottomPanel.Name = "BottomPanel";
			this.BottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(590, 36, true);
			this.BottomPanel.TabIndex = 1;
			// 
			// PostingButtonsUserControl
			// 
			this.PostingButtonsUserControl.AllowDrop = true;
			this.PostingButtonsUserControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.PostingButtonsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(283, 8, true);
			this.PostingButtonsUserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.PostingButtonsUserControl.Name = "PostingButtonsUserControl";
			this.PostingButtonsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(301, 25, true);
			this.PostingButtonsUserControl.TabIndex = 0;
			// 
			// MainPanel
			// 
			this.MainPanel.Controls.Add(this.MainGrid);
			this.MainPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MainPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MainPanel.Name = "MainPanel";
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(590, 103, true);
			this.MainPanel.TabIndex = 0;
			// 
			// MainGrid
			// 
			this.MainGrid.AllowNavigation = false;
			this.MainGrid.AllowSorting = false;
			this.BindingSource.SetBindingMember(this.MainGrid, "Sequences");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccComplianceSequenceSplitViewModel)(null)).Sequences)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccComplianceSequenceForSplit)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccComplianceSequenceSplitViewModel)(null)).Sequences)).SyncRoot)).XD_Calc_BookType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccComplianceSequenceForSplit)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccComplianceSequenceSplitViewModel)(null)).Sequences)).SyncRoot)).XD_Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccComplianceSequenceForSplit)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccComplianceSequenceSplitViewModel)(null)).Sequences)).SyncRoot)).XD_Description)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccComplianceSequenceForSplit)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccComplianceSequenceSplitViewModel)(null)).Sequences)).SyncRoot)).XD_AllocationLevel)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.AccComplianceSequenceForSplit)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccComplianceSequenceSplitViewModel)(null)).Sequences)).SyncRoot)).XD_GB_BranchOwner)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.AccComplianceSequenceForSplit)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccComplianceSequenceSplitViewModel)(null)).Sequences)).SyncRoot)).XD_GE_Department)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccComplianceSequenceForSplit)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccComplianceSequenceSplitViewModel)(null)).Sequences)).SyncRoot)).XD_Prefix)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccComplianceSequenceForSplit)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccComplianceSequenceSplitViewModel)(null)).Sequences)).SyncRoot)).XD_Calc_StartNumberString)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccComplianceSequenceForSplit)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccComplianceSequenceSplitViewModel)(null)).Sequences)).SyncRoot)).XD_Calc_EndNumberString)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccComplianceSequenceForSplit)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccComplianceSequenceSplitViewModel)(null)).Sequences)).SyncRoot)).XD_Calc_NextNumberString)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDate)(((Enterprise.MasterFiles.Business.AccComplianceSequenceForSplit)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccComplianceSequenceSplitViewModel)(null)).Sequences)).SyncRoot)).XD_StartDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.MasterFiles.Business.AccComplianceSequenceForSplit)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccComplianceSequenceSplitViewModel)(null)).Sequences)).SyncRoot)).XD_ExpiryDate)));
			this.MainGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo8.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("81c26c59-5ac8-4e4c-a57b-28a54454b1d6", "Book");
			zTextBoxColumnStyleInfo8.ColumnName = "XD_Calc_BookType";
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo9.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("8a34d60e-b105-4a30-8712-69645292f605", "Code");
			zTextBoxColumnStyleInfo9.ColumnName = "XD_Code";
			zTextBoxColumnStyleInfo9.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo10.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ef6942e6-85fe-4cbe-9942-8cf0efc9c170", "Description");
			zTextBoxColumnStyleInfo10.ColumnName = "XD_Description";
			zTextBoxColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("21a7495f-bca7-47cc-879f-8a24272cadb0", "Allocation Level");
			zDropEditColumnStyleInfo2.ColumnName = "XD_AllocationLevel";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo3.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("eeecf806-f65e-4da0-87a7-5060139380f9", "Branch");
			zGuidFindBoxColumnStyleInfo3.ColumnName = "XD_GB_BranchOwner";
			zGuidFindBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo4.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("a23408b2-1d35-492b-b200-e0a58b713beb", "Department");
			zGuidFindBoxColumnStyleInfo4.ColumnName = "XD_GE_Department";
			zGuidFindBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo11.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("e51f58bf-f6d3-4566-97da-1fd94b926df8", "Prefix");
			zTextBoxColumnStyleInfo11.ColumnName = "XD_Prefix";
			zTextBoxColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo12.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("86206107-1e39-4533-a144-0d2153d59452", "Start Number");
			zTextBoxColumnStyleInfo12.ColumnName = "XD_Calc_StartNumberString";
			zTextBoxColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo13.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("a1839061-d5da-4556-a826-87478123fee6", "End Number");
			zTextBoxColumnStyleInfo13.ColumnName = "XD_Calc_EndNumberString";
			zTextBoxColumnStyleInfo13.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo14.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("12d7df22-aad1-436e-b5a7-8e261ee745a1", "Next Number");
			zTextBoxColumnStyleInfo14.ColumnName = "XD_Calc_NextNumberString";
			zTextBoxColumnStyleInfo14.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo3.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("e98ef9aa-5e9d-4cc6-aa6e-18b43fce51de", "Valid Date");
			zDateEditColumnStyleInfo3.ColumnName = "XD_StartDate";
			zDateEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDateEditColumnStyleInfo4.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("9405fbe6-b144-4131-b766-2ace0d91e3f6", "Expiry Date");
			zDateEditColumnStyleInfo4.ColumnName = "XD_ExpiryDate";
			zDateEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.MainGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.MainGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.MainGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.MainGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.MainGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo3);
			this.MainGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo4);
			this.MainGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo11);
			this.MainGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo12);
			this.MainGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo13);
			this.MainGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo14);
			this.MainGrid.ColumnStyles.Add(zDateEditColumnStyleInfo3);
			this.MainGrid.ColumnStyles.Add(zDateEditColumnStyleInfo4);
			this.MainGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MainGrid.GridId = "90f0689d-490a-4b3c-8413-9c5ec90abec1";
			this.MainGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.MainGrid.LayoutKey = "InvoicesGrid";
			this.MainGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MainGrid.Name = "MainGrid";
			this.MainGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.MainGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(590, 103, true);
			this.MainGrid.TabIndex = 0;
			// 
			// AccComplianceSequenceSplitForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("cdc106de-5418-4968-a6da-680908347869", "Split Compliance Sequence Book");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(590, 164, true);
			this.Controls.Add(this.MainPanel);
			this.Controls.Add(this.BottomPanel);
			this.DataSourceType = typeof(Enterprise.MasterFiles.Business.AccComplianceSequenceSplitViewModel);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(375, 200, true);
			this.Name = "AccComplianceSequenceSplitForm";
			this.Text = "OverrideTransactionDescriptionForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.BottomPanel, 0);
			this.Controls.SetChildIndex(this.MainPanel, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.BottomPanel.ResumeLayout(false);
			this.BottomPanel.PerformLayout();
			this.PostingButtonsUserControl.ResumeLayout(true);
			this.PostingButtonsUserControl.PerformLayout();
			this.MainPanel.ResumeLayout(false);
			this.MainPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MainGrid)).EndInit();
			this.MainGrid.ResumeLayout(false);
			this.MainGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZPanel BottomPanel;
		private Core.Forms.ZPostingButtonsUserControl PostingButtonsUserControl;
		private ZArchitecture.GUI.ZPanel MainPanel;
		protected ZArchitecture.ZGrid MainGrid;
	}
}
