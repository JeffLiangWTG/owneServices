namespace Enterprise.Customs.US.AMS.GUI
{
	partial class MessagesUserControl
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

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo11 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo12 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.MovementsAndBillsSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.RelatedRecordsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.RelatedRecordsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.MessagesBottomSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.MessagesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.CusInBondMoveHeaderMessageTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.CusInBondMoveHeaderMessageDetailsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.MessageDetailsTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CusInBondMoveHeaderMessageTextTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.FormattedMessageTextTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.MovementsAndBillsSplitContainer)).BeginInit();
			this.MovementsAndBillsSplitContainer.Panel1.SuspendLayout();
			this.MovementsAndBillsSplitContainer.Panel2.SuspendLayout();
			this.MovementsAndBillsSplitContainer.SuspendLayout();
			this.RelatedRecordsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.RelatedRecordsGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.MessagesBottomSplitContainer)).BeginInit();
			this.MessagesBottomSplitContainer.Panel1.SuspendLayout();
			this.MessagesBottomSplitContainer.Panel2.SuspendLayout();
			this.MessagesBottomSplitContainer.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessagesGrid)).BeginInit();
			this.CusInBondMoveHeaderMessageTabControl.SuspendLayout();
			this.CusInBondMoveHeaderMessageDetailsTabPage.SuspendLayout();
			this.CusInBondMoveHeaderMessageTextTabPage.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.AMS.Business.CusInBondHeader);
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
			this.MovementsAndBillsSplitContainer.Panel1.Controls.Add(this.RelatedRecordsGroupBox);
			this.MovementsAndBillsSplitContainer.Panel1MinSize = 100;
			// 
			// MovementsAndBillsSplitContainer.Panel2
			// 
			this.MovementsAndBillsSplitContainer.Panel2.Controls.Add(this.MessagesBottomSplitContainer);
			this.MovementsAndBillsSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(971, 741, true);
			this.MovementsAndBillsSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(228);
			this.MovementsAndBillsSplitContainer.TabIndex = 3;
			// 
			// RelatedRecordsGroupBox
			// 
			this.RelatedRecordsGroupBox.CaptionResourceString = Enterprise.Customs.US.AMS.GUI.Res.GetData("USAMSConsolManifestUserControl|61b88fc6-16f3-4dac-9c73-e17c29138aeb", "Messages");
			this.RelatedRecordsGroupBox.Controls.Add(this.RelatedRecordsGrid);
			this.RelatedRecordsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.RelatedRecordsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.RelatedRecordsGroupBox.Name = "RelatedRecordsGroupBox";
			this.RelatedRecordsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(971, 228, true);
			this.RelatedRecordsGroupBox.TabIndex = 4;
			this.RelatedRecordsGroupBox.TabStop = false;
			// 
			// RelatedRecordsGrid
			// 
			this.RelatedRecordsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.RelatedRecordsGrid, "MessageAttacheesRelatedRecords");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.AMS.Business.CusInBondHeader)(null)).MessageAttacheesRelatedRecords)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.AMS.Business.MessageActionRelatedRecordWrapper)(((System.Collections.IList)(((Enterprise.Customs.US.AMS.Business.CusInBondHeader)(null)).MessageAttacheesRelatedRecords)).SyncRoot)).RecordTypeDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.AMS.Business.MessageActionRelatedRecordWrapper)(((System.Collections.IList)(((Enterprise.Customs.US.AMS.Business.CusInBondHeader)(null)).MessageAttacheesRelatedRecords)).SyncRoot)).RecordIdentifier)));
			this.RelatedRecordsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.US.AMS.GUI.Res.GetData("28860364-2af7-4ecf-82da-4dc438c90d51", "Type");
			zTextBoxColumnStyleInfo1.ColumnName = "RecordTypeDescription";
			zTextBoxColumnStyleInfo1.IsMandatory = true;
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(170);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.US.AMS.GUI.Res.GetData("810a719f-0528-4573-8dbb-7d9391ce1a00", "Reference");
			zTextBoxColumnStyleInfo2.ColumnName = "RecordIdentifier";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(340);
			this.RelatedRecordsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.RelatedRecordsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.RelatedRecordsGrid.CopySelectedRowsAllowed = true;
			this.RelatedRecordsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.RelatedRecordsGrid.GridId = "22275e02-8b50-407a-a899-d229c0cadac6";
			this.RelatedRecordsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.RelatedRecordsGrid.LayoutKey = "RelatedRecords";
			this.RelatedRecordsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.RelatedRecordsGrid.Name = "RelatedRecordsGrid";
			this.RelatedRecordsGrid.ReadOnly = true;
			this.RelatedRecordsGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.RelatedRecordsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(965, 209, true);
			this.RelatedRecordsGrid.TabIndex = 0;
			// 
			// MessagesBottomSplitContainer
			// 
			this.MessagesBottomSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MessagesBottomSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MessagesBottomSplitContainer.Name = "MessagesBottomSplitContainer";
			// 
			// MessagesBottomSplitContainer.Panel1
			// 
			this.MessagesBottomSplitContainer.Panel1.Controls.Add(this.MessagesGrid);
			this.MessagesBottomSplitContainer.Panel1MinSize = 200;
			// 
			// MessagesBottomSplitContainer.Panel2
			// 
			this.MessagesBottomSplitContainer.Panel2.Controls.Add(this.CusInBondMoveHeaderMessageTabControl);
			this.MessagesBottomSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(971, 509, true);
			this.MessagesBottomSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(438);
			this.MessagesBottomSplitContainer.TabIndex = 3;
			// 
			// MessagesGrid
			// 
			this.MessagesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.MessagesGrid, "MessageAttacheesRelatedRecords.Messages");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.AMS.Business.MessageActionRelatedRecordWrapper)(((System.Collections.IList)(((Enterprise.Customs.US.AMS.Business.CusInBondHeader)(null)).MessageAttacheesRelatedRecords)).SyncRoot)).Messages)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.AMS.Messaging.Business.AMSEDIMessage)(((System.Collections.IList)(((Enterprise.Customs.US.AMS.Business.MessageActionRelatedRecordWrapper)(((System.Collections.IList)(((Enterprise.Customs.US.AMS.Business.CusInBondHeader)(null)).MessageAttacheesRelatedRecords)).SyncRoot)).Messages)).SyncRoot)).EM_MessageNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.AMS.Messaging.Business.AMSEDIMessage)(((System.Collections.IList)(((Enterprise.Customs.US.AMS.Business.MessageActionRelatedRecordWrapper)(((System.Collections.IList)(((Enterprise.Customs.US.AMS.Business.CusInBondHeader)(null)).MessageAttacheesRelatedRecords)).SyncRoot)).Messages)).SyncRoot)).EM_MessageType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.AMS.Messaging.Business.AMSEDIMessage)(((System.Collections.IList)(((Enterprise.Customs.US.AMS.Business.MessageActionRelatedRecordWrapper)(((System.Collections.IList)(((Enterprise.Customs.US.AMS.Business.CusInBondHeader)(null)).MessageAttacheesRelatedRecords)).SyncRoot)).Messages)).SyncRoot)).EM_MessageSubType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.US.AMS.Messaging.Business.AMSEDIMessage)(((System.Collections.IList)(((Enterprise.Customs.US.AMS.Business.MessageActionRelatedRecordWrapper)(((System.Collections.IList)(((Enterprise.Customs.US.AMS.Business.CusInBondHeader)(null)).MessageAttacheesRelatedRecords)).SyncRoot)).Messages)).SyncRoot)).EM_MessageDateTime)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.US.AMS.Messaging.Business.AMSEDIMessage)(((System.Collections.IList)(((Enterprise.Customs.US.AMS.Business.MessageActionRelatedRecordWrapper)(((System.Collections.IList)(((Enterprise.Customs.US.AMS.Business.CusInBondHeader)(null)).MessageAttacheesRelatedRecords)).SyncRoot)).Messages)).SyncRoot)).EM_SystemCreateTimeUtc)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.AMS.Messaging.Business.AMSEDIMessage)(((System.Collections.IList)(((Enterprise.Customs.US.AMS.Business.MessageActionRelatedRecordWrapper)(((System.Collections.IList)(((Enterprise.Customs.US.AMS.Business.CusInBondHeader)(null)).MessageAttacheesRelatedRecords)).SyncRoot)).Messages)).SyncRoot)).EM_InterchangeNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.US.AMS.Messaging.Business.AMSEDIMessage)(((System.Collections.IList)(((Enterprise.Customs.US.AMS.Business.MessageActionRelatedRecordWrapper)(((System.Collections.IList)(((Enterprise.Customs.US.AMS.Business.CusInBondHeader)(null)).MessageAttacheesRelatedRecords)).SyncRoot)).Messages)).SyncRoot)).EM_DateTimeInterchangeSent)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.AMS.Messaging.Business.AMSEDIMessage)(((System.Collections.IList)(((Enterprise.Customs.US.AMS.Business.MessageActionRelatedRecordWrapper)(((System.Collections.IList)(((Enterprise.Customs.US.AMS.Business.CusInBondHeader)(null)).MessageAttacheesRelatedRecords)).SyncRoot)).Messages)).SyncRoot)).EM_User)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.AMS.Messaging.Business.AMSEDIMessage)(((System.Collections.IList)(((Enterprise.Customs.US.AMS.Business.MessageActionRelatedRecordWrapper)(((System.Collections.IList)(((Enterprise.Customs.US.AMS.Business.CusInBondHeader)(null)).MessageAttacheesRelatedRecords)).SyncRoot)).Messages)).SyncRoot)).EM_Status)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.AMS.Messaging.Business.AMSEDIMessage)(((System.Collections.IList)(((Enterprise.Customs.US.AMS.Business.MessageActionRelatedRecordWrapper)(((System.Collections.IList)(((Enterprise.Customs.US.AMS.Business.CusInBondHeader)(null)).MessageAttacheesRelatedRecords)).SyncRoot)).Messages)).SyncRoot)).EM_InterchangeStatus)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.AMS.Messaging.Business.AMSEDIMessage)(((System.Collections.IList)(((Enterprise.Customs.US.AMS.Business.MessageActionRelatedRecordWrapper)(((System.Collections.IList)(((Enterprise.Customs.US.AMS.Business.CusInBondHeader)(null)).MessageAttacheesRelatedRecords)).SyncRoot)).Messages)).SyncRoot)).EM_ReceiveTransmit)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.AMS.Messaging.Business.AMSEDIMessage)(((System.Collections.IList)(((Enterprise.Customs.US.AMS.Business.MessageActionRelatedRecordWrapper)(((System.Collections.IList)(((Enterprise.Customs.US.AMS.Business.CusInBondHeader)(null)).MessageAttacheesRelatedRecords)).SyncRoot)).Messages)).SyncRoot)).EM_MessageSubTypeDescription)));
			this.MessagesGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo3.ColumnName = "EM_MessageNum";
			zTextBoxColumnStyleInfo4.ColumnName = "EM_MessageType";
			zTextBoxColumnStyleInfo5.ColumnName = "EM_MessageSubType";
			zDateEditColumnStyleInfo1.ColumnName = "EM_MessageDateTime";
			zDateEditColumnStyleInfo2.ColumnName = "EM_SystemCreateTimeUtc";
			zTextBoxColumnStyleInfo6.ColumnName = "EM_InterchangeNumber";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(99);
			zDateEditColumnStyleInfo3.ColumnName = "EM_DateTimeInterchangeSent";
			zDateEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(122);
			zTextBoxColumnStyleInfo7.ColumnName = "EM_User";
			zTextBoxColumnStyleInfo8.ColumnName = "EM_Status";
			zTextBoxColumnStyleInfo9.ColumnName = "EM_InterchangeStatus";
			zTextBoxColumnStyleInfo10.ColumnName = "EM_ReceiveTransmit";
			zTextBoxColumnStyleInfo11.ColumnName = "EM_MessageSubTypeDescription";
			zTextBoxColumnStyleInfo12.ColumnName = "EM_SendWithMessageErrorsFormatted";
			zTextBoxColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(112);
			this.MessagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.MessagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.MessagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.MessagesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.MessagesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.MessagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.MessagesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo3);
			this.MessagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.MessagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.MessagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.MessagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.MessagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo11);
			this.MessagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo12);
			this.MessagesGrid.CopySelectedRowsAllowed = true;
			this.MessagesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MessagesGrid.GridId = "d98549e5-32bf-4074-a0a9-406f6c54dd53";
			this.MessagesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.MessagesGrid.LayoutKey = "CusInBondMoveHeaderMessagesGrid";
			this.MessagesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MessagesGrid.Name = "MessagesGrid";
			this.MessagesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(438, 509, true);
			this.MessagesGrid.TabIndex = 1;
			// 
			// CusInBondMoveHeaderMessageTabControl
			// 
			this.CusInBondMoveHeaderMessageTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.CusInBondMoveHeaderMessageTabControl.Controls.Add(this.CusInBondMoveHeaderMessageDetailsTabPage);
			this.CusInBondMoveHeaderMessageTabControl.Controls.Add(this.CusInBondMoveHeaderMessageTextTabPage);
			this.CusInBondMoveHeaderMessageTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CusInBondMoveHeaderMessageTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CusInBondMoveHeaderMessageTabControl.Name = "CusInBondMoveHeaderMessageTabControl";
			this.CusInBondMoveHeaderMessageTabControl.SelectedIndex = 0;
			this.CusInBondMoveHeaderMessageTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(529, 509, true);
			this.CusInBondMoveHeaderMessageTabControl.TabIndex = 1;
			// 
			// CusInBondMoveHeaderMessageDetailsTabPage
			// 
			this.CusInBondMoveHeaderMessageDetailsTabPage.BackColor = System.Drawing.SystemColors.Control;
			this.CusInBondMoveHeaderMessageDetailsTabPage.CaptionResourceString = Enterprise.Customs.US.AMS.GUI.Res.GetData("81c2f260-0089-4718-8157-6bcebe879207", "Message Details");
			this.CusInBondMoveHeaderMessageDetailsTabPage.Controls.Add(this.MessageDetailsTextBox);
			this.CusInBondMoveHeaderMessageDetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.CusInBondMoveHeaderMessageDetailsTabPage.Name = "CusInBondMoveHeaderMessageDetailsTabPage";
			this.CusInBondMoveHeaderMessageDetailsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.CusInBondMoveHeaderMessageDetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(521, 482, true);
			this.CusInBondMoveHeaderMessageDetailsTabPage.TabIndex = 0;
			// 
			// MessageDetailsTextBox
			// 
			this.MessageDetailsTextBox.BackColor = System.Drawing.SystemColors.Control;
			this.BindingSource.SetBindingMember(this.MessageDetailsTextBox, "MessageAttacheesRelatedRecords.Messages.EM_MessageInterpretation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.AMS.Messaging.Business.AMSEDIMessage)(((System.Collections.IList)(((Enterprise.Customs.US.AMS.Business.MessageActionRelatedRecordWrapper)(((System.Collections.IList)(((Enterprise.Customs.US.AMS.Business.CusInBondHeader)(null)).MessageAttacheesRelatedRecords)).SyncRoot)).Messages)).SyncRoot)).EM_MessageInterpretation)));
			this.MessageDetailsTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MessageDetailsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.MessageDetailsTextBox.Multiline = true;
			this.MessageDetailsTextBox.Name = "MessageDetailsTextBox";
			this.MessageDetailsTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.MessageDetailsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(515, 476, true);
			this.MessageDetailsTextBox.TabIndex = 0;
			// 
			// CusInBondMoveHeaderMessageTextTabPage
			// 
			this.CusInBondMoveHeaderMessageTextTabPage.BackColor = System.Drawing.SystemColors.Control;
			this.CusInBondMoveHeaderMessageTextTabPage.CaptionResourceString = Enterprise.Customs.US.AMS.GUI.Res.GetData("620dbaca-f0d7-427e-b950-ac189feec359", "Message Text");
			this.CusInBondMoveHeaderMessageTextTabPage.Controls.Add(this.FormattedMessageTextTextBox);
			this.CusInBondMoveHeaderMessageTextTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.CusInBondMoveHeaderMessageTextTabPage.Name = "CusInBondMoveHeaderMessageTextTabPage";
			this.CusInBondMoveHeaderMessageTextTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.CusInBondMoveHeaderMessageTextTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(521, 482, true);
			this.CusInBondMoveHeaderMessageTextTabPage.TabIndex = 1;
			// 
			// FormattedMessageTextTextBox
			// 
			this.FormattedMessageTextTextBox.BackColor = System.Drawing.SystemColors.Control;
			this.BindingSource.SetBindingMember(this.FormattedMessageTextTextBox, "MessageAttacheesRelatedRecords.Messages.EM_FormattedMessageText");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.AMS.Messaging.Business.AMSEDIMessage)(((System.Collections.IList)(((Enterprise.Customs.US.AMS.Business.MessageActionRelatedRecordWrapper)(((System.Collections.IList)(((Enterprise.Customs.US.AMS.Business.CusInBondHeader)(null)).MessageAttacheesRelatedRecords)).SyncRoot)).Messages)).SyncRoot)).EM_FormattedMessageText)));
			this.FormattedMessageTextTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.FormattedMessageTextTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.FormattedMessageTextTextBox.Multiline = true;
			this.FormattedMessageTextTextBox.Name = "FormattedMessageTextTextBox";
			this.FormattedMessageTextTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.FormattedMessageTextTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(515, 476, true);
			this.FormattedMessageTextTextBox.TabIndex = 0;
			// 
			// MessagesUserControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.MovementsAndBillsSplitContainer);
			this.Name = "MessagesUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(971, 741, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MovementsAndBillsSplitContainer.Panel1.ResumeLayout(false);
			this.MovementsAndBillsSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.MovementsAndBillsSplitContainer)).EndInit();
			this.MovementsAndBillsSplitContainer.ResumeLayout(false);
			this.RelatedRecordsGroupBox.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.RelatedRecordsGrid)).EndInit();
			this.MessagesBottomSplitContainer.Panel1.ResumeLayout(false);
			this.MessagesBottomSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.MessagesBottomSplitContainer)).EndInit();
			this.MessagesBottomSplitContainer.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.MessagesGrid)).EndInit();
			this.CusInBondMoveHeaderMessageTabControl.ResumeLayout(false);
			this.CusInBondMoveHeaderMessageDetailsTabPage.ResumeLayout(false);
			this.CusInBondMoveHeaderMessageDetailsTabPage.PerformLayout();
			this.CusInBondMoveHeaderMessageTextTabPage.ResumeLayout(false);
			this.CusInBondMoveHeaderMessageTextTabPage.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private CargoWise.Windows.UI.KSplitContainer MovementsAndBillsSplitContainer;
		private ZArchitecture.GUI.ZGroupBox RelatedRecordsGroupBox;
		private ZArchitecture.ZGrid RelatedRecordsGrid;
		private CargoWise.Windows.UI.KSplitContainer MessagesBottomSplitContainer;
		private ZArchitecture.ZGrid MessagesGrid;
		private ZArchitecture.GUI.ZTabControl CusInBondMoveHeaderMessageTabControl;
		private ZArchitecture.GUI.ZTabPage CusInBondMoveHeaderMessageDetailsTabPage;
		private ZArchitecture.ZTextBox MessageDetailsTextBox;
		private ZArchitecture.GUI.ZTabPage CusInBondMoveHeaderMessageTextTabPage;
		private ZArchitecture.ZTextBox FormattedMessageTextTextBox;

	}
}
