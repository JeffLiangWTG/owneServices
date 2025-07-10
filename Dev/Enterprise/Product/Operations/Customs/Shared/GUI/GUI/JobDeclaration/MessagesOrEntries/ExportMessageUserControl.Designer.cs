using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	public partial class ExportMessageUserControl
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private CargoWise.Windows.UI.KPanel HistoryPanel;
		private ZGroupBox HistoryGroupBox;
		private CargoWise.Windows.UI.KSplitter TheSplitter;
		private CargoWise.Windows.UI.KPanel MessageTextPanel;
		private ZGroupBox MessageTextGroupBox;
		private Enterprise.Messaging.GUI.MessageZGrid MessagesGrid;
		private Enterprise.ZArchitecture.ZTextBox MessageTextTextBox;
		private System.ComponentModel.Container components = null;

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
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.HistoryPanel = new CargoWise.Windows.UI.KPanel();
			this.HistoryGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.MessagesGrid = new Enterprise.Messaging.GUI.MessageZGrid();
			this.TheSplitter = new CargoWise.Windows.UI.KSplitter();
			this.MessageTextPanel = new CargoWise.Windows.UI.KPanel();
			this.MessageTextGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.MessageTextTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.HistoryPanel.SuspendLayout();
			this.HistoryGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessagesGrid)).BeginInit();
			this.MessageTextPanel.SuspendLayout();
			this.MessageTextGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.Business.BaseJobDeclaration);
			// 
			// HistoryPanel
			// 
			this.HistoryPanel.Controls.Add(this.HistoryGroupBox);
			this.HistoryPanel.Dock = System.Windows.Forms.DockStyle.Left;
			this.HistoryPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.HistoryPanel.Name = "HistoryPanel";
			this.HistoryPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(600, 544, true);
			this.HistoryPanel.TabIndex = 0;
			// 
			// HistoryGroupBox
			// 
			this.HistoryGroupBox.Controls.Add(this.MessagesGrid);
			this.HistoryGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.HistoryGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.HistoryGroupBox.Name = "HistoryGroupBox";
			this.HistoryGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(600, 544, true);
			this.HistoryGroupBox.TabIndex = 0;
			this.HistoryGroupBox.TabStop = false;
			this.HistoryGroupBox.Text = "History";
			// 
			// MessagesGrid
			// 
			this.MessagesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.MessagesGrid, "MessagesIncludingInterchangeRejections");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).MessagesIncludingInterchangeRejections)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).MessagesIncludingInterchangeRejections)).SyncRoot)).EM_MessageNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).MessagesIncludingInterchangeRejections)).SyncRoot)).EM_MessageType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).MessagesIncludingInterchangeRejections)).SyncRoot)).EM_MessageSubType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).MessagesIncludingInterchangeRejections)).SyncRoot)).EM_MessageDateTime)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).MessagesIncludingInterchangeRejections)).SyncRoot)).EM_SystemCreateTimeUtc)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).MessagesIncludingInterchangeRejections)).SyncRoot)).EM_InterchangeNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).MessagesIncludingInterchangeRejections)).SyncRoot)).EM_DateTimeInterchangeSent)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).MessagesIncludingInterchangeRejections)).SyncRoot)).EM_User)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).MessagesIncludingInterchangeRejections)).SyncRoot)).EM_Status)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).MessagesIncludingInterchangeRejections)).SyncRoot)).EM_ReceiveTransmit)));
			this.MessagesGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.Caption = "Message No";
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "EM_MessageNum";
			zTextBoxColumnStyleInfo1.IsMandatory = true;
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.ToolTip = "Message Number";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(88);
			zTextBoxColumnStyleInfo2.Caption = "Message Type";
			zTextBoxColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(52);
			zTextBoxColumnStyleInfo2.ColumnName = "EM_MessageType";
			zTextBoxColumnStyleInfo2.ToolTip = "Message Type";
			zTextBoxColumnStyleInfo3.Caption = "Message SubType";
			zTextBoxColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo3.ColumnName = "EM_MessageSubType";
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.ToolTip = "Message Sub Type";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zDateEditColumnStyleInfo1.Caption = "Message Time";
			zDateEditColumnStyleInfo1.ColumnName = "EM_MessageDateTime";
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(98);
			zDateEditColumnStyleInfo2.Caption = "Create Time (UTC)";
			zDateEditColumnStyleInfo2.ColumnName = "EM_SystemCreateTimeUtc";
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(116);
			zTextBoxColumnStyleInfo4.Caption = "Interchange No";
			zTextBoxColumnStyleInfo4.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo4.ColumnName = "EM_InterchangeNumber";
			zTextBoxColumnStyleInfo4.IsReadOnly = true;
			zTextBoxColumnStyleInfo4.ToolTip = "Interchange No in which this message was contained.";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(133);
			zDateEditColumnStyleInfo3.Caption = "Interchange Sent";
			zDateEditColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDateEditColumnStyleInfo3.ColumnName = "EM_DateTimeInterchangeSent";
			zDateEditColumnStyleInfo3.IsReadOnly = true;
			zDateEditColumnStyleInfo3.ToolTip = "It is when messages are actually sent to or received from Customs.";
			zDateEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(112);
			zTextBoxColumnStyleInfo5.Caption = "Sender";
			zTextBoxColumnStyleInfo5.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo5.ColumnName = "EM_User";
			zTextBoxColumnStyleInfo5.IsReadOnly = true;
			zTextBoxColumnStyleInfo5.ToolTip = "It is the person who created this message.";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130);
			zTextBoxColumnStyleInfo6.Caption = "Status";
			zTextBoxColumnStyleInfo6.ColumnName = "EM_Status";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(58);
			zTextBoxColumnStyleInfo7.Caption = "Direction";
			zTextBoxColumnStyleInfo7.ColumnName = "EM_ReceiveTransmit";
			zTextBoxColumnStyleInfo7.ToolTip = "TRX: sent to Customs RCV: received from Customs";
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(72);
			this.MessagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.MessagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.MessagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.MessagesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.MessagesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.MessagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.MessagesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo3);
			this.MessagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.MessagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.MessagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.MessagesGrid.GridId = "0c5c1539-6fe3-4542-8183-660e3aa1e851";
			this.MessagesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MessagesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.MessagesGrid.LayoutKey = "zGrid1";
			this.MessagesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.MessagesGrid.Name = "MessagesGrid";
			this.MessagesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(594, 525, true);
			this.MessagesGrid.TabIndex = 0;
			// 
			// TheSplitter
			// 
			this.TheSplitter.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(600, 0, true);
			this.TheSplitter.Name = "TheSplitter";
			this.TheSplitter.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(3, 544, true);
			this.TheSplitter.TabIndex = 1;
			this.TheSplitter.TabStop = false;
			// 
			// MessageTextPanel
			// 
			this.MessageTextPanel.Controls.Add(this.MessageTextGroupBox);
			this.MessageTextPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MessageTextPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(603, 0, true);
			this.MessageTextPanel.Name = "MessageTextPanel";
			this.MessageTextPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(349, 544, true);
			this.MessageTextPanel.TabIndex = 2;
			// 
			// MessageTextGroupBox
			// 
			this.MessageTextGroupBox.Controls.Add(this.MessageTextTextBox);
			this.MessageTextGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MessageTextGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MessageTextGroupBox.Name = "MessageTextGroupBox";
			this.MessageTextGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(349, 544, true);
			this.MessageTextGroupBox.TabIndex = 0;
			this.MessageTextGroupBox.TabStop = false;
			this.MessageTextGroupBox.Text = "Message Text";
			// 
			// MessageTextTextBox
			// 
			this.BindingSource.SetBindingMember(this.MessageTextTextBox, "MessagesIncludingInterchangeRejections.EM_FormattedMessageText");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).MessagesIncludingInterchangeRejections)).SyncRoot)).EM_FormattedMessageText)));
			this.MessageTextTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.MessageTextTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MessageTextTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.MessageTextTextBox.Multiline = true;
			this.MessageTextTextBox.Name = "MessageTextTextBox";
			this.MessageTextTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.MessageTextTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(343, 525, true);
			this.MessageTextTextBox.TabIndex = 0;
			// 
			// ExportMessageUserControl
			// 
			this.Controls.Add(this.MessageTextPanel);
			this.Controls.Add(this.TheSplitter);
			this.Controls.Add(this.HistoryPanel);
			this.Name = "ExportMessageUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(952, 544, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.HistoryPanel.ResumeLayout(false);
			this.HistoryGroupBox.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.MessagesGrid)).EndInit();
			this.MessageTextPanel.ResumeLayout(false);
			this.MessageTextGroupBox.ResumeLayout(false);
			this.MessageTextGroupBox.PerformLayout();
			this.ResumeLayout(false);
		}
		#endregion
	}
}
