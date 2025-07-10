using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	partial class DiscardedMessageUserControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;
		private ZGroupBox MessageGridGroupBox;
		private ZGroupBox MessageTextGroupBox;
		private Enterprise.ZArchitecture.ZTextBox MessageTextBoundTextBox;
		private CargoWise.Windows.UI.KSplitter TheSplitter;
		private Enterprise.Messaging.GUI.MessageZGrid MessageGrid;

		#region Component Designer generated code
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.Customs.GUI.MessageCodeColumnStyleInfo messageCodeColumnStyleInfo1 = new Enterprise.Customs.GUI.MessageCodeColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.Customs.GUI.MessageCodeColumnStyleInfo messageCodeColumnStyleInfo2 = new Enterprise.Customs.GUI.MessageCodeColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.MessageGridGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.MessageGrid = new Enterprise.Messaging.GUI.MessageZGrid();
			this.MessageTextGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.MessageTextBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.TheSplitter = new CargoWise.Windows.UI.KSplitter();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.MessageGridGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageGrid)).BeginInit();
			this.MessageTextGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.Business.BaseJobDeclaration);
			// 
			// MessageGridGroupBox
			// 
			this.MessageGridGroupBox.Controls.Add(this.MessageGrid);
			this.MessageGridGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MessageGridGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MessageGridGroupBox.Name = "MessageGridGroupBox";
			this.MessageGridGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(357, 424, true);
			this.MessageGridGroupBox.TabIndex = 0;
			this.MessageGridGroupBox.TabStop = false;
			this.MessageGridGroupBox.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("0C417C00-3B50-4FE8-8292-24133DFBDAC5", "Messages");
			// 
			// MessageGrid
			// 
			this.MessageGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.MessageGrid, "DiscardedMessages");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).DiscardedMessages)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).DiscardedMessages)).SyncRoot)).EM_MessageType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).DiscardedMessages)).SyncRoot)).EM_MessageNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).DiscardedMessages)).SyncRoot)).EM_MessageDateTime)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).DiscardedMessages)).SyncRoot)).EM_SystemCreateTimeUtc)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).DiscardedMessages)).SyncRoot)).EM_InterchangeNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).DiscardedMessages)).SyncRoot)).EM_DateTimeInterchangeSent)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).DiscardedMessages)).SyncRoot)).EM_User)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).DiscardedMessages)).SyncRoot)).EM_Status)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).DiscardedMessages)).SyncRoot)).EM_ReceiveTransmit)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).DiscardedMessages)).SyncRoot)).EM_MessageSubType)));
			this.MessageGrid.CaptionVisible = false;
			messageCodeColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("9F43FBEA-A258-4081-9EDE-A3AEA4FA1E68", "Type");
			messageCodeColumnStyleInfo1.ColumnName = "EM_MessageType";
			messageCodeColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(52);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("59105B09-0059-452C-9D79-DD836F5DF87B", "Message No");
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "EM_MessageNum";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(88);
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("1F169BD5-B63F-4FB2-860C-24980D054E2B", "Message Time");
			zDateEditColumnStyleInfo1.ColumnName = "EM_MessageDateTime";
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(98);
			zDateEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("44C467E4-D87F-4320-BA18-E54B81F0CCB1", "Create Time (UTC)");
			zDateEditColumnStyleInfo2.ColumnName = "EM_SystemCreateTimeUtc";
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(116);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("7C7D5989-FB37-445F-9FCA-F460F9E4532C", "Interchange No");
			zTextBoxColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo2.ColumnName = "EM_InterchangeNumber";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(133);
			zDateEditColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("20040DA1-D113-4828-ACA6-7E2472522100", "Interchange Time");
			zDateEditColumnStyleInfo3.ColumnName = "EM_DateTimeInterchangeSent";
			zDateEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(112);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("3C70A94D-C9A7-4ADF-B704-701F0722A4E2", "Sender");
			zTextBoxColumnStyleInfo3.ColumnName = "EM_User";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130);
			messageCodeColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("8705B3B1-C10C-487C-91B7-C6D5D392363C", "Status");
			messageCodeColumnStyleInfo2.ColumnName = "EM_Status";
			messageCodeColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(58);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("810F2E7E-9BE5-4616-8350-00FB5A66C08B", "Direction");
			zTextBoxColumnStyleInfo4.ColumnName = "EM_ReceiveTransmit";
			zTextBoxColumnStyleInfo4.ToolTip = "TRX:sent to Customs RCV: received from Customs";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(72);
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("3FDE62A0-7D4A-4D31-86AF-0066EA92EEE8", "Sub Type");
			zTextBoxColumnStyleInfo5.ColumnName = "EM_MessageSubType";
			zTextBoxColumnStyleInfo5.ToolTip = "Message sub type";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			this.MessageGrid.ColumnStyles.Add(messageCodeColumnStyleInfo1);
			this.MessageGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.MessageGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.MessageGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.MessageGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.MessageGrid.ColumnStyles.Add(zDateEditColumnStyleInfo3);
			this.MessageGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.MessageGrid.ColumnStyles.Add(messageCodeColumnStyleInfo2);
			this.MessageGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.MessageGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.MessageGrid.GridId = "5851601e-cecd-4c8a-b555-8f1ac0fd853a";
			this.MessageGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MessageGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.MessageGrid.LayoutKey = "MessageGrid";
			this.MessageGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.MessageGrid.Name = "MessageGrid";
			this.MessageGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(351, 405, true);
			this.MessageGrid.TabIndex = 0;
			// 
			// MessageTextGroupBox
			// 
			this.MessageTextGroupBox.Controls.Add(this.MessageTextBoundTextBox);
			this.MessageTextGroupBox.Dock = System.Windows.Forms.DockStyle.Right;
			this.MessageTextGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(360, 0, true);
			this.MessageTextGroupBox.Name = "MessageTextGroupBox";
			this.MessageTextGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(304, 424, true);
			this.MessageTextGroupBox.TabIndex = 1;
			this.MessageTextGroupBox.TabStop = false;
			this.MessageTextGroupBox.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("16B8AF46-A1D2-4D1A-A22F-16C70D48A2C5", "Message Text");
			// 
			// MessageTextBoundTextBox
			// 
			this.BindingSource.SetBindingMember(this.MessageTextBoundTextBox, "DiscardedMessages.EM_FormattedMessageText");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).DiscardedMessages)).SyncRoot)).EM_FormattedMessageText)));
			this.MessageTextBoundTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MessageTextBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.MessageTextBoundTextBox.Multiline = true;
			this.MessageTextBoundTextBox.Name = "MessageTextBoundTextBox";
			this.MessageTextBoundTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.MessageTextBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(298, 405, true);
			this.MessageTextBoundTextBox.TabIndex = 0;
			// 
			// TheSplitter
			// 
			this.TheSplitter.Dock = System.Windows.Forms.DockStyle.Right;
			this.TheSplitter.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(357, 0, true);
			this.TheSplitter.Name = "TheSplitter";
			this.TheSplitter.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(3, 424, true);
			this.TheSplitter.TabIndex = 2;
			this.TheSplitter.TabStop = false;
			// 
			// DiscardedMessageUserControl
			//
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.MessageGridGroupBox);
			this.Controls.Add(this.TheSplitter);
			this.Controls.Add(this.MessageTextGroupBox);
			this.Name = "DiscardedMessageUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(664, 424, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MessageGridGroupBox.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.MessageGrid)).EndInit();
			this.MessageTextGroupBox.ResumeLayout(false);
			this.MessageTextGroupBox.PerformLayout();
			this.ResumeLayout(false);
		}
		#endregion
	}
}
