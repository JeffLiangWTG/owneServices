namespace Enterprise.Customs.US.AMS.GUI
{
	sealed partial class USAMSMessageSendingActionForm
	{
		#region Windows Form Designer generated code

		new void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.BottomPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.cancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.SendButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.MovementsAndBillsSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.MovementsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.MovementsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.BillsAndMessageContentSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.BillsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.BillsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.BA_MessageContentsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.MB_MessageContentsTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.BottomPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MovementsAndBillsSplitContainer)).BeginInit();
			this.MovementsAndBillsSplitContainer.Panel1.SuspendLayout();
			this.MovementsAndBillsSplitContainer.Panel2.SuspendLayout();
			this.MovementsAndBillsSplitContainer.SuspendLayout();
			this.MovementsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MovementsGrid)).BeginInit();
			this.MovementsGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BillsAndMessageContentSplitContainer)).BeginInit();
			this.BillsAndMessageContentSplitContainer.Panel1.SuspendLayout();
			this.BillsAndMessageContentSplitContainer.Panel2.SuspendLayout();
			this.BillsAndMessageContentSplitContainer.SuspendLayout();
			this.BillsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BillsGrid)).BeginInit();
			this.BillsGrid.SuspendLayout();
			this.BA_MessageContentsGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 440, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(600, 24, true);
			this.MainStatusBar.TabIndex = 1;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.AMS.Business.MessageSendingAction);
			// 
			// BottomPanel
			// 
			this.BottomPanel.Controls.Add(this.cancelButton);
			this.BottomPanel.Controls.Add(this.SendButton);
			this.BottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.BottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 403, true);
			this.BottomPanel.Name = "BottomPanel";
			this.BottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(600, 37, true);
			this.BottomPanel.TabIndex = 1;
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.CaptionResourceString = Enterprise.Customs.US.AMS.GUI.Res.GetData("USAMSMessageSendingActionForm|b7e9be38-be5b-45b0-999e-5a69d714fb5e", "&Cancel");
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(513, 6, true);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.cancelButton.TabIndex = 4;
			this.cancelButton.UseVisualStyleBackColor = true;
			this.cancelButton.Click += new System.EventHandler(this.CancelButton_Click);
			// 
			// SendButton
			// 
			this.SendButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.SendButton.CaptionResourceString = Enterprise.Customs.US.AMS.GUI.Res.GetData("USAMSMessageSendingActionForm|4a4f8bd7-f6d5-41b6-9ebc-af8bd2442900", "&Send");
			this.SendButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(432, 6, true);
			this.SendButton.Name = "SendButton";
			this.SendButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.SendButton.TabIndex = 3;
			this.SendButton.UseVisualStyleBackColor = true;
			this.SendButton.Click += new System.EventHandler(this.SendButton_Click);
			// 
			// MovementsAndBillsSplitContainer
			// 
			this.MovementsAndBillsSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MovementsAndBillsSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MovementsAndBillsSplitContainer.Name = "MovementsAndBillsSplitContainer";
			this.MovementsAndBillsSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// MovementsAndBillsSplitContainer.Panel1
			// 
			this.MovementsAndBillsSplitContainer.Panel1.Controls.Add(this.MovementsGroupBox);
			this.MovementsAndBillsSplitContainer.Panel1MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			// 
			// MovementsAndBillsSplitContainer.Panel2
			// 
			this.MovementsAndBillsSplitContainer.Panel2.Controls.Add(this.BillsAndMessageContentSplitContainer);
			this.MovementsAndBillsSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(600, 403, true);
			this.MovementsAndBillsSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(124);
			this.MovementsAndBillsSplitContainer.TabIndex = 2;
			// 
			// MovementsGroupBox
			// 
			this.MovementsGroupBox.CaptionResourceString = Enterprise.Customs.US.AMS.GUI.Res.GetData("a56441db-03dc-4b1b-b8c5-6e08bb36d166", "Movements");
			this.MovementsGroupBox.Controls.Add(this.MovementsGrid);
			this.MovementsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MovementsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MovementsGroupBox.Name = "MovementsGroupBox";
			this.MovementsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(600, 124, true);
			this.MovementsGroupBox.TabIndex = 0;
			this.MovementsGroupBox.TabStop = false;
			// 
			// MovementsGrid
			// 
			this.MovementsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.MovementsGrid, "Movements");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.AMS.Business.MessageSendingAction)(null)).Movements)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.AMS.Business.MessageSendingMovement)(((System.Collections.IList)(((Enterprise.Customs.US.AMS.Business.MessageSendingAction)(null)).Movements)).SyncRoot)).MM_Send)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.AMS.Business.MessageSendingMovement)(((System.Collections.IList)(((Enterprise.Customs.US.AMS.Business.MessageSendingAction)(null)).Movements)).SyncRoot)).MM_RelatedDetails)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.US.AMS.Business.MessageSendingMovement)(((System.Collections.IList)(((Enterprise.Customs.US.AMS.Business.MessageSendingAction)(null)).Movements)).SyncRoot)).MM_Date)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.AMS.Business.MessageSendingMovement)(((System.Collections.IList)(((Enterprise.Customs.US.AMS.Business.MessageSendingAction)(null)).Movements)).SyncRoot)).MM_ForeignDeparturePort)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.AMS.Business.MessageSendingMovement)(((System.Collections.IList)(((Enterprise.Customs.US.AMS.Business.MessageSendingAction)(null)).Movements)).SyncRoot)).MM_PTTFiler)));
			this.MovementsGrid.CaptionVisible = false;
			zCheckBoxColumnStyleInfo1.ColumnName = "MM_Send";
			zCheckBoxColumnStyleInfo1.IsMandatory = true;
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.ColumnName = "MM_RelatedDetails";
			zTextBoxColumnStyleInfo1.IsMandatory = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zDateEditColumnStyleInfo1.ColumnName = "MM_Date";
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCodeFindBoxColumnStyleInfo1.ColumnName = "MM_ForeignDeparturePort";
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo10.ColumnName = "MM_PTTFiler";
			zTextBoxColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(81);
			zTextBoxColumnStyleInfo10.IsReadOnly = true;
			this.MovementsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.MovementsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.MovementsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.MovementsGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.MovementsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.MovementsGrid.CopySelectedRowsAllowed = true;
			this.MovementsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MovementsGrid.GridId = "49014750-b3cb-4ee2-8cb8-8b8556dc1a6a";
			this.MovementsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.MovementsGrid.LayoutKey = "MovementsGrid";
			this.MovementsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.MovementsGrid.Name = "MovementsGrid";
			this.MovementsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(594, 105, true);
			this.MovementsGrid.TabIndex = 0;
			// 
			// BillsAndMessageContentSplitContainer
			// 
			this.BillsAndMessageContentSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.BillsAndMessageContentSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.BillsAndMessageContentSplitContainer.Name = "BillsAndMessageContentSplitContainer";
			this.BillsAndMessageContentSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// BillsAndMessageContentSplitContainer.Panel1
			// 
			this.BillsAndMessageContentSplitContainer.Panel1.Controls.Add(this.BillsGroupBox);
			this.BillsAndMessageContentSplitContainer.Panel1MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			// 
			// BillsAndMessageContentSplitContainer.Panel2
			// 
			this.BillsAndMessageContentSplitContainer.Panel2.Controls.Add(this.BA_MessageContentsGroupBox);
			this.BillsAndMessageContentSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(600, 275, true);
			this.BillsAndMessageContentSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(145);
			this.BillsAndMessageContentSplitContainer.TabIndex = 1;
			// 
			// BillsGroupBox
			// 
			this.BillsGroupBox.CaptionResourceString = Enterprise.Customs.US.AMS.GUI.Res.GetData("USAMSMessageSendingActionForm|cefc982c-7c74-43a8-b087-a30002293b17", "Bills");
			this.BillsGroupBox.Controls.Add(this.BillsGrid);
			this.BillsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.BillsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.BillsGroupBox.Name = "BillsGroupBox";
			this.BillsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(600, 145, true);
			this.BillsGroupBox.TabIndex = 0;
			this.BillsGroupBox.TabStop = false;
			// 
			// BillsGrid
			// 
			this.BillsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.BillsGrid, "MessageSendingObjects");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.AMS.Business.MessageSendingAction)(null)).MessageSendingObjects)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.AMS.Business.MessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.US.AMS.Business.MessageSendingAction)(null)).MessageSendingObjects)).SyncRoot)).MB_RelatedDetails)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.AMS.Business.MessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.US.AMS.Business.MessageSendingAction)(null)).MessageSendingObjects)).SyncRoot)).MB_Send)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.AMS.Business.MessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.US.AMS.Business.MessageSendingAction)(null)).MessageSendingObjects)).SyncRoot)).MB_IssuerCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.AMS.Business.MessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.US.AMS.Business.MessageSendingAction)(null)).MessageSendingObjects)).SyncRoot)).MB_BillOfLadingSequenceNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.AMS.Business.MessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.US.AMS.Business.MessageSendingAction)(null)).MessageSendingObjects)).SyncRoot)).MB_BillActionCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.AMS.Business.MessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.US.AMS.Business.MessageSendingAction)(null)).MessageSendingObjects)).SyncRoot)).MB_AmendmentCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.AMS.Business.MessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.US.AMS.Business.MessageSendingAction)(null)).MessageSendingObjects)).SyncRoot)).MB_CustomsStatus)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.AMS.Business.MessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.US.AMS.Business.MessageSendingAction)(null)).MessageSendingObjects)).SyncRoot)).MB_CustomsStatusDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.AMS.Business.MessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.US.AMS.Business.MessageSendingAction)(null)).MessageSendingObjects)).SyncRoot)).MB_MessageStatus)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.AMS.Business.MessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.US.AMS.Business.MessageSendingAction)(null)).MessageSendingObjects)).SyncRoot)).MB_MessageStatusDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.AMS.Business.MessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.US.AMS.Business.MessageSendingAction)(null)).MessageSendingObjects)).SyncRoot)).MB_PortOfUnlading)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.US.AMS.Business.MessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.US.AMS.Business.MessageSendingAction)(null)).MessageSendingObjects)).SyncRoot)).MB_Date)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.AMS.Business.MessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.US.AMS.Business.MessageSendingAction)(null)).MessageSendingObjects)).SyncRoot)).MB_VesselOverride)));
			this.BillsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo2.ColumnName = "MB_RelatedDetails";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCheckBoxColumnStyleInfo2.ColumnName = "MB_Send";
			zCheckBoxColumnStyleInfo2.IsMandatory = true;
			zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(52);
			zTextBoxColumnStyleInfo3.ColumnName = "MB_IssuerCode";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(81);
			zTextBoxColumnStyleInfo4.ColumnName = "MB_BillOfLadingSequenceNumber";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(87);
			zDropEditColumnStyleInfo1.ColumnName = "MB_BillActionCode";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(81);
			zDropEditColumnStyleInfo2.ColumnName = "MB_AmendmentCode";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zTextBoxColumnStyleInfo5.ColumnName = "MB_CustomsStatus";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(98);
			zTextBoxColumnStyleInfo6.ColumnName = "MB_CustomsStatusDescription";
			zTextBoxColumnStyleInfo6.IsVisible = false;
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(156);
			zTextBoxColumnStyleInfo7.ColumnName = "MB_MessageStatus";
			zTextBoxColumnStyleInfo7.IsVisible = false;
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(98);
			zTextBoxColumnStyleInfo8.ColumnName = "MB_MessageStatusDescription";
			zTextBoxColumnStyleInfo8.IsVisible = false;
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(156);
			zTextBoxColumnStyleInfo9.ColumnName = "MB_PortOfUnlading";
			zTextBoxColumnStyleInfo9.IsReadOnly = true;
			zTextBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zCodeFindBoxColumnStyleInfo2.ColumnName = "MB_PortOfUnladingOverride";
			zCodeFindBoxColumnStyleInfo2.IsReadOnly = false;
			zCodeFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zDateEditColumnStyleInfo2.ColumnName = "MB_Date";
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCheckBoxColumnStyleInfo3.ColumnName = "MB_VesselOverride";
			zCheckBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.BillsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.BillsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.BillsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.BillsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.BillsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.BillsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.BillsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.BillsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.BillsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.BillsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.BillsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.BillsGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo2);
			this.BillsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.BillsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo3);
			this.BillsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.BillsGrid.GridId = "f8ddacfb-fd44-4e8a-958e-5788c3e0b169";
			this.BillsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.BillsGrid.LayoutKey = "BillsGrid";
			this.BillsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.BillsGrid.Name = "BillsGrid";
			this.BillsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(594, 126, true);
			this.BillsGrid.TabIndex = 0;
			// 
			// BA_MessageContentsGroupBox
			// 
			this.BA_MessageContentsGroupBox.CaptionResourceString = Enterprise.Customs.US.AMS.GUI.Res.GetData("USAMSMessageSendingActionForm|6c86c956-9fb3-466d-b5a4-082c1ba8b3a5", "Message Data");
			this.BA_MessageContentsGroupBox.Controls.Add(this.MB_MessageContentsTextBox);
			this.BA_MessageContentsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.BA_MessageContentsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.BA_MessageContentsGroupBox.Name = "BA_MessageContentsGroupBox";
			this.BA_MessageContentsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(600, 126, true);
			this.BA_MessageContentsGroupBox.TabIndex = 1;
			this.BA_MessageContentsGroupBox.TabStop = false;
			// 
			// MB_MessageContentsTextBox
			// 
			this.BindingSource.SetBindingMember(this.MB_MessageContentsTextBox, "MessageSendingObjects.MB_MessageContents");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.AMS.Business.MessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.US.AMS.Business.MessageSendingAction)(null)).MessageSendingObjects)).SyncRoot)).MB_MessageContents)));
			this.MB_MessageContentsTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MB_MessageContentsTextBox.EnableValidStateColor = false;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.MB_MessageContentsTextBox, false);
			this.MB_MessageContentsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.MB_MessageContentsTextBox.Multiline = true;
			this.MB_MessageContentsTextBox.Name = "MB_MessageContentsTextBox";
			this.MB_MessageContentsTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.MB_MessageContentsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(594, 107, true);
			this.MB_MessageContentsTextBox.TabIndex = 0;
			// 
			// USAMSMessageSendingActionForm
			// 
			this.CancelButton = this.cancelButton;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Customs.US.AMS.GUI.Res.GetData("MessageSendingActionForm|1a54c2b1-cb91-4ee9-b35c-c0ff64b5daf2", "Send AMS Message");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(600, 464, true);
			this.Controls.Add(this.MovementsAndBillsSplitContainer);
			this.Controls.Add(this.BottomPanel);
			this.DataSourceType = typeof(Enterprise.Customs.US.AMS.Business.MessageSendingAction);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 400, true);
			this.Name = "USAMSMessageSendingActionForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.BottomPanel, 0);
			this.Controls.SetChildIndex(this.MovementsAndBillsSplitContainer, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.BottomPanel.ResumeLayout(false);
			this.BottomPanel.PerformLayout();
			this.MovementsAndBillsSplitContainer.Panel1.ResumeLayout(false);
			this.MovementsAndBillsSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.MovementsAndBillsSplitContainer)).EndInit();
			this.MovementsAndBillsSplitContainer.ResumeLayout(false);
			this.MovementsAndBillsSplitContainer.PerformLayout();
			this.MovementsGroupBox.ResumeLayout(false);
			this.MovementsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MovementsGrid)).EndInit();
			this.MovementsGrid.ResumeLayout(false);
			this.MovementsGrid.PerformLayout();
			this.BillsAndMessageContentSplitContainer.Panel1.ResumeLayout(false);
			this.BillsAndMessageContentSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.BillsAndMessageContentSplitContainer)).EndInit();
			this.BillsAndMessageContentSplitContainer.ResumeLayout(false);
			this.BillsAndMessageContentSplitContainer.PerformLayout();
			this.BillsGroupBox.ResumeLayout(false);
			this.BillsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BillsGrid)).EndInit();
			this.BillsGrid.ResumeLayout(false);
			this.BillsGrid.PerformLayout();
			this.BA_MessageContentsGroupBox.ResumeLayout(false);
			this.BA_MessageContentsGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZPanel BottomPanel;
		private ZArchitecture.GUI.ZButton cancelButton;
		private ZArchitecture.GUI.ZButton SendButton;
		private CargoWise.Windows.UI.KSplitContainer MovementsAndBillsSplitContainer;
		private CargoWise.Windows.UI.KSplitContainer BillsAndMessageContentSplitContainer;
		private ZArchitecture.GUI.ZGroupBox BillsGroupBox;
		private ZArchitecture.ZGrid BillsGrid;
		private ZArchitecture.GUI.ZGroupBox BA_MessageContentsGroupBox;
		private ZArchitecture.ZTextBox MB_MessageContentsTextBox;
		private ZArchitecture.GUI.ZGroupBox MovementsGroupBox;
		private ZArchitecture.ZGrid MovementsGrid;
	}
}
