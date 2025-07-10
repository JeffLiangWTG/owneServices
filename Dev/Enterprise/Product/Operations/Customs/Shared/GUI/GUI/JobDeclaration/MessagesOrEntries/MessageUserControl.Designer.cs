using System.ComponentModel;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	partial class MessageUserControl
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		protected ZPanel MainPanel;
		protected Enterprise.ZArchitecture.GUI.ZTabControl MessagesTabControl;
		protected Enterprise.ZArchitecture.GUI.ZTabPage MessageTextTabPage;
		protected ZTextBox MessageTextTextBox;
		CargoWise.Windows.UI.KSplitter HistoryMessageSplitter;
		protected ZGroupBox HistoryGroupBox;
		public ZGrid MessagesGrid;
		IContainer components;

		#region Component Designer generated code
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.MainPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.MessagesTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.MessageTextTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.MessageTextTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.HistoryMessageSplitter = new CargoWise.Windows.UI.KSplitter();
			this.HistoryGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.MessagesGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.MainPanel.SuspendLayout();
			this.MessagesTabControl.SuspendLayout();
			this.MessageTextTabPage.SuspendLayout();
			this.HistoryGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessagesGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.Business.BaseJobDeclaration);
			// 
			// MainPanel
			// 
			this.MainPanel.Controls.Add(this.MessagesTabControl);
			this.MainPanel.Controls.Add(this.HistoryMessageSplitter);
			this.MainPanel.Controls.Add(this.HistoryGroupBox);
			this.MainPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MainPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MainPanel.Name = "MainPanel";
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(952, 544, true);
			this.MainPanel.TabIndex = 12;
			// 
			// MessagesTabControl
			// 
			this.MessagesTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.MessagesTabControl.Controls.Add(this.MessageTextTabPage);
			this.MessagesTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MessagesTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(562, 0, true);
			this.MessagesTabControl.Name = "MessagesTabControl";
			this.MessagesTabControl.SelectedIndex = 0;
			this.MessagesTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(390, 544, true);
			this.MessagesTabControl.TabIndex = 12;
			// 
			// MessageTextTabPage
			// 
			this.MessageTextTabPage.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("MessageUserControl|432258c2-90e9-484e-96bc-d4769f04c67d", "Message Text");
			this.MessageTextTabPage.Controls.Add(this.MessageTextTextBox);
			this.MessageTextTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.MessageTextTabPage.Name = "MessageTextTabPage";
			this.MessageTextTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.MessageTextTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(382, 517, true);
			this.MessageTextTabPage.TabIndex = 0;
			// 
			// MessageTextTextBox
			// 
			this.MessageTextTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.MessageTextTextBox, "Messages.EM_FormattedMessageText");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).Messages)).SyncRoot)).EM_FormattedMessageText)));
			this.MessageTextTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MessageTextTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.MessageTextTextBox.Multiline = true;
			this.MessageTextTextBox.Name = "MessageTextTextBox";
			this.MessageTextTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.MessageTextTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(376, 511, true);
			this.MessageTextTextBox.TabIndex = 1;
			// 
			// HistoryMessageSplitter
			// 
			this.HistoryMessageSplitter.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(559, 0, true);
			this.HistoryMessageSplitter.Name = "HistoryMessageSplitter";
			this.HistoryMessageSplitter.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(3, 544, true);
			this.HistoryMessageSplitter.TabIndex = 11;
			this.HistoryMessageSplitter.TabStop = false;
			// 
			// HistoryGroupBox
			// 
			this.HistoryGroupBox.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("MessageUserControl|069ae9d9-70a8-4aa5-892e-9225f1831902", "History");
			this.HistoryGroupBox.Controls.Add(this.MessagesGrid);
			this.HistoryGroupBox.Dock = System.Windows.Forms.DockStyle.Left;
			this.HistoryGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.HistoryGroupBox.Name = "HistoryGroupBox";
			this.HistoryGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(559, 544, true);
			this.HistoryGroupBox.TabIndex = 10;
			this.HistoryGroupBox.TabStop = false;
			// 
			// MessagesGrid
			// 
			this.MessagesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.MessagesGrid, "Messages");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).Messages)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).Messages)).SyncRoot)).EM_MessageNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).Messages)).SyncRoot)).EM_MessageSubType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).Messages)).SyncRoot)).EM_MessageDateTime)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).Messages)).SyncRoot)).EM_SystemCreateTimeUtc)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).Messages)).SyncRoot)).EM_InterchangeNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).Messages)).SyncRoot)).EM_DateTimeInterchangeSent)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).Messages)).SyncRoot)).EM_User)));
			this.MessagesGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "EM_MessageNum";
			zTextBoxColumnStyleInfo1.IsMandatory = true;
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.ToolTip = "Message Number";
			zTextBoxColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo2.ColumnName = "EM_MessageSubType";
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.ToolTip = "Message Type";
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("MessageUserControl|a628c967-4f61-435f-b46c-844d65ccec78", "Message Time");
			zDateEditColumnStyleInfo1.ColumnName = "EM_MessageDateTime";
			zDateEditColumnStyleInfo2.ColumnName = "EM_SystemCreateTimeUtc";
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("MessageUserControl|214f12ec-477e-4b7b-a15c-1f985e15bb92", "Interchange No");
			zTextBoxColumnStyleInfo3.ColumnName = "EM_InterchangeNumber";
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.ToolTip = "Interchange No in which this message was contained.";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zDateEditColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDateEditColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("MessageUserControl|451acad4-ae33-43b8-b505-847e5698e2c2", "Interchange Sent");
			zDateEditColumnStyleInfo3.ColumnName = "EM_DateTimeInterchangeSent";
			zDateEditColumnStyleInfo3.IsReadOnly = true;
			zDateEditColumnStyleInfo3.ToolTip = "It is when messages are actually sent to or received from Customs.";
			zDateEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo4.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("MessageUserControl|b3c4f438-40be-48c1-bee4-a79a27cac8a1", "Sender");
			zTextBoxColumnStyleInfo4.ColumnName = "EM_User";
			zTextBoxColumnStyleInfo4.IsReadOnly = true;
			zTextBoxColumnStyleInfo4.ToolTip = "It is the person who created this message.";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			this.MessagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.MessagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.MessagesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.MessagesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.MessagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.MessagesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo3);
			this.MessagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.MessagesGrid.GridId = "3648b479-848d-4526-a7a2-62960d9d637b";
			this.MessagesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MessagesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.MessagesGrid.LayoutKey = "zGrid1";
			this.MessagesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.MessagesGrid.Name = "MessagesGrid";
			this.MessagesGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.MessagesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(553, 525, true);
			this.MessagesGrid.TabIndex = 2;
			// 
			// MessageUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.MainPanel);
			this.Name = "MessageUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(952, 544, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MainPanel.ResumeLayout(false);
			this.MessagesTabControl.ResumeLayout(false);
			this.MessageTextTabPage.ResumeLayout(false);
			this.MessageTextTabPage.PerformLayout();
			this.HistoryGroupBox.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.MessagesGrid)).EndInit();
			this.ResumeLayout(false);
		}
		#endregion
	}
}
