namespace Enterprise.MasterFiles.GUI
{
	partial class AccComplianceSequenceBulkForm
	{
		#region Windows Form Designer generated code

		ZArchitecture.GUI.ZPanel BottomPanel;
		ZArchitecture.GUI.ZPanel MainPanel;
		private Core.Forms.ZPostingButtonsUserControl PostingButtonsUserControl;
		Enterprise.ZArchitecture.ZGrid ComplianceSequenceBulkGrid;

		new void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			this.ComplianceSequenceBulkGrid = new Enterprise.ZArchitecture.ZGrid();
			this.BottomPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.PostingButtonsUserControl = new Enterprise.Core.Forms.ZPostingButtonsUserControl();
			this.MainPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ComplianceSequenceBulkGrid)).BeginInit();
			this.ComplianceSequenceBulkGrid.SuspendLayout();
			this.BottomPanel.SuspendLayout();
			this.PostingButtonsUserControl.SuspendLayout();
			this.MainPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 140, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(590, 24, true);
			this.MainStatusBar.TabIndex = 2;
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
			this.MainPanel.Controls.Add(this.ComplianceSequenceBulkGrid);
			this.MainPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MainPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MainPanel.Name = "MainPanel";
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(590, 103, true);
			this.MainPanel.TabIndex = 0;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.AccComplianceSequenceBulkCreator);
			// 
			// ComplianceSequenceBulkGrid
			// 
			this.ComplianceSequenceBulkGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ComplianceSequenceBulkGrid, "ComplianceSequences");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccComplianceSequenceBulkCreator)(null)).ComplianceSequences)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccComplianceSequence)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccComplianceSequenceBulkCreator)(null)).ComplianceSequences)).SyncRoot)).XD_SequenceClass)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccComplianceSequence)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccComplianceSequenceBulkCreator)(null)).ComplianceSequences)).SyncRoot)).XD_Calc_SequenceClassDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccComplianceSequence)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccComplianceSequenceBulkCreator)(null)).ComplianceSequences)).SyncRoot)).XD_Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccComplianceSequence)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccComplianceSequenceBulkCreator)(null)).ComplianceSequences)).SyncRoot)).XD_Description)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccComplianceSequence)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccComplianceSequenceBulkCreator)(null)).ComplianceSequences)).SyncRoot)).XD_AllocationLevel)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.AccComplianceSequence)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccComplianceSequenceBulkCreator)(null)).ComplianceSequences)).SyncRoot)).XD_GB_BranchOwner)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.AccComplianceSequence)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccComplianceSequenceBulkCreator)(null)).ComplianceSequences)).SyncRoot)).XD_GE_Department)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccComplianceSequence)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccComplianceSequenceBulkCreator)(null)).ComplianceSequences)).SyncRoot)).XD_Prefix)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.AccComplianceSequence)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccComplianceSequenceBulkCreator)(null)).ComplianceSequences)).SyncRoot)).XD_MaximumNumberDigits)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccComplianceSequence)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccComplianceSequenceBulkCreator)(null)).ComplianceSequences)).SyncRoot)).XD_Calc_StartNumberString)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccComplianceSequence)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccComplianceSequenceBulkCreator)(null)).ComplianceSequences)).SyncRoot)).XD_Calc_EndNumberString)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccComplianceSequence)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccComplianceSequenceBulkCreator)(null)).ComplianceSequences)).SyncRoot)).XD_PrintingAuthorizationNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccComplianceSequence)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccComplianceSequenceBulkCreator)(null)).ComplianceSequences)).SyncRoot)).XD_Calc_NextNumberString)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDate)(((Enterprise.MasterFiles.Business.AccComplianceSequence)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccComplianceSequenceBulkCreator)(null)).ComplianceSequences)).SyncRoot)).XD_StartDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.MasterFiles.Business.AccComplianceSequence)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccComplianceSequenceBulkCreator)(null)).ComplianceSequences)).SyncRoot)).XD_ExpiryDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccComplianceSequence)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccComplianceSequenceBulkCreator)(null)).ComplianceSequences)).SyncRoot)).XD_NumberFormat)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.AccComplianceSequence)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccComplianceSequenceBulkCreator)(null)).ComplianceSequences)).SyncRoot)).XD_SU_MenuItem)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.AccComplianceSequence)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccComplianceSequenceBulkCreator)(null)).ComplianceSequences)).SyncRoot)).XD_MaxChargesPerTransaction)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccComplianceSequence)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccComplianceSequenceBulkCreator)(null)).ComplianceSequences)).SyncRoot)).XD_RollupBehaviourWhenMaxExceeded)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.AccComplianceSequence)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccComplianceSequenceBulkCreator)(null)).ComplianceSequences)).SyncRoot)).XD_SQ_DocumentPrintQueue)));
			this.ComplianceSequenceBulkGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("892a09e1-dfc1-4be7-8d07-598793ba8d69", "Sub Type");
			zDropEditColumnStyleInfo1.ColumnName = "XD_SequenceClass";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("bea5a089-ea9a-4007-bab4-b0a393d369dc", "Sub Type Description");
			zTextBoxColumnStyleInfo1.ColumnName = "XD_Calc_SequenceClassDescription";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("3441a63b-b183-4137-8b31-4ff95d4de01a", "Code");
			zTextBoxColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo2.ColumnName = "XD_Code";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("09097c18-73d1-47d7-8b52-1181d6d38d72", "Description");
			zTextBoxColumnStyleInfo3.ColumnName = "XD_Description";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("91a0fdfa-a984-448e-84a0-8b300e82a502", "Allocation Level");
			zDropEditColumnStyleInfo2.ColumnName = "XD_AllocationLevel";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zGuidFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("1098fb97-c3a9-41a3-9ac3-6e7a180a7b42", "Allocation / Printing Branch");
			zGuidFindBoxColumnStyleInfo1.ColumnName = "XD_GB_BranchOwner";
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
			zGuidFindBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("143f758a-5286-4708-a665-46cc34375690", "Allocation / Printing Department");
			zGuidFindBoxColumnStyleInfo2.ColumnName = "XD_GE_Department";
			zGuidFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("d363c062-9651-42f3-894e-11aeb53b74c5", "Series Prefix");
			zTextBoxColumnStyleInfo4.ColumnName = "XD_Prefix";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("a296a7e3-c183-4bc3-83f8-6d89c9217a7d", "Max Number Digits");
			zCalcEditColumnStyleInfo1.ColumnName = "XD_MaximumNumberDigits";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("d4c50746-00b8-496c-b0b8-2f1ff25374e1", "Start Number");
			zTextBoxColumnStyleInfo5.ColumnName = "XD_Calc_StartNumberString";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("e8ebf877-3325-4ed3-b76d-cb461932ea2e", "End Number");
			zTextBoxColumnStyleInfo6.ColumnName = "XD_Calc_EndNumberString";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo7.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("3d1ac52f-0602-4e61-b83e-d3a0ddf9bf7a", "Printing Authorization Number");
			zTextBoxColumnStyleInfo7.ColumnName = "XD_PrintingAuthorizationNumber";
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zTextBoxColumnStyleInfo8.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("aa550388-6510-4579-a8c0-11d17ae2a784", "Next Number");
			zTextBoxColumnStyleInfo8.ColumnName = "XD_Calc_NextNumberString";
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("1b76451d-c146-4abc-9f18-ecc840a72eee", "Valid From");
			zDateEditColumnStyleInfo1.ColumnName = "XD_StartDate";
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zDateEditColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("471dc677-5b44-4388-a2cb-2113bf9f053e", "Expiry Date");
			zDateEditColumnStyleInfo2.ColumnName = "XD_ExpiryDate";
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zDropEditColumnStyleInfo3.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("657b4191-ca10-4d36-8dda-6e821e7c78b0", "Number Format");
			zDropEditColumnStyleInfo3.ColumnName = "XD_NumberFormat";
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zGuidFindBoxColumnStyleInfo3.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("dee59787-0866-4244-a796-3fa8fb8f7da6", "Document Menu");
			zGuidFindBoxColumnStyleInfo3.ColumnName = "XD_SU_MenuItem";
			zGuidFindBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("3f308646-d87b-44da-9e6d-831c18bfb27f", "Maximum Charge Lines");
			zCalcEditColumnStyleInfo2.ColumnName = "XD_MaxChargesPerTransaction";
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDropEditColumnStyleInfo4.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("7bbc0478-25ca-43da-a6a6-7eccc2cf5f5b", "Document Printing Style");
			zDropEditColumnStyleInfo4.ColumnName = "XD_RollupBehaviourWhenMaxExceeded";
			zDropEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130);
			zGuidFindBoxColumnStyleInfo4.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("e6af3036-302d-4106-9eef-437b09f6e2f5", "Printing Queue");
			zGuidFindBoxColumnStyleInfo4.ColumnName = "XD_SQ_DocumentPrintQueue";
			zGuidFindBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			this.ComplianceSequenceBulkGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.ComplianceSequenceBulkGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ComplianceSequenceBulkGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.ComplianceSequenceBulkGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.ComplianceSequenceBulkGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.ComplianceSequenceBulkGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.ComplianceSequenceBulkGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo2);
			this.ComplianceSequenceBulkGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.ComplianceSequenceBulkGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.ComplianceSequenceBulkGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.ComplianceSequenceBulkGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.ComplianceSequenceBulkGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.ComplianceSequenceBulkGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.ComplianceSequenceBulkGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.ComplianceSequenceBulkGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.ComplianceSequenceBulkGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.ComplianceSequenceBulkGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo3);
			this.ComplianceSequenceBulkGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.ComplianceSequenceBulkGrid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
			this.ComplianceSequenceBulkGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo4);
			this.ComplianceSequenceBulkGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ComplianceSequenceBulkGrid.GridId = "62a8cf7a-22ab-481d-989b-851d099a0877";
			this.ComplianceSequenceBulkGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ComplianceSequenceBulkGrid.LayoutKey = "ComplianceSequenceBulkGrid";
			this.ComplianceSequenceBulkGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ComplianceSequenceBulkGrid.Name = "ComplianceSequenceBulkGrid";
			this.ComplianceSequenceBulkGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(590, 103, true);
			this.ComplianceSequenceBulkGrid.TabIndex = 0;
			// 
			// AccComplianceSequenceBulkCreateForm
			//
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("4701a13f-1767-48d4-9e3a-e6b32dc19a2d", "Compliance Sequence Bulk Create");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(590, 164, true);
			this.Controls.Add(this.MainPanel);
			this.Controls.Add(this.BottomPanel);
			this.DataSourceType = typeof(Enterprise.MasterFiles.Business.AccComplianceSequenceBulkCreator);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(375, 200, true);
			this.Name = "AccComplianceSequenceBulkCreateForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.BottomPanel, 0);
			this.Controls.SetChildIndex(this.MainPanel, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ComplianceSequenceBulkGrid)).EndInit();
			this.ComplianceSequenceBulkGrid.ResumeLayout(false);
			this.ComplianceSequenceBulkGrid.PerformLayout();
			this.BottomPanel.ResumeLayout(false);
			this.BottomPanel.PerformLayout();
			this.PostingButtonsUserControl.ResumeLayout(true);
			this.PostingButtonsUserControl.PerformLayout();
			this.MainPanel.ResumeLayout(false);
			this.MainPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion
	}
}
