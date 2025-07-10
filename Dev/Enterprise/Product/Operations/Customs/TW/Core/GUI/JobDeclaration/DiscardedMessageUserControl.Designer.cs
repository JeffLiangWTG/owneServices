
namespace Enterprise.Customs.TW.GUI
{
	partial class DiscardedMessageUserControl
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
			this.MessageTextGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.MessageTextBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.TheSplitter = new CargoWise.Windows.UI.KSplitter();
			this.MessageGrid = new Enterprise.Messaging.GUI.MessageZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.MessageTextGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageGrid)).BeginInit();
			this.MessageGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.TW.Business.JobDeclaration);
			// 
			// MessageTextGroupBox
			// 
			this.MessageTextGroupBox.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("00bb29db-2c4f-4437-8ca9-234d43e5abd9", "XML");
			this.MessageTextGroupBox.Controls.Add(this.MessageTextBoundTextBox);
			this.MessageTextGroupBox.Dock = System.Windows.Forms.DockStyle.Right;
			this.MessageTextGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(360, 0, true);
			this.MessageTextGroupBox.Name = "MessageTextGroupBox";
			this.MessageTextGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(304, 424, true);
			this.MessageTextGroupBox.TabIndex = 1;
			this.MessageTextGroupBox.TabStop = false;
			// 
			// MessageTextBoundTextBox
			// 
			this.BindingSource.SetBindingMember(this.MessageTextBoundTextBox, "DiscardedMessages.EM_FormattedMessageText");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).DiscardedMessages)).SyncRoot)).EM_FormattedMessageText)));
			this.MessageTextBoundTextBox.CaptionResourceString = null;
			this.MessageTextBoundTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MessageTextBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 17, true);
			this.MessageTextBoundTextBox.Multiline = true;
			this.MessageTextBoundTextBox.Name = "MessageTextBoundTextBox";
			this.MessageTextBoundTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.MessageTextBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 405, true);
			this.MessageTextBoundTextBox.TabIndex = 0;
			this.MessageTextBoundTextBox.HideSelection = false;
			this.MessageTextBoundTextBox.EnableFindDialog = true;
			// 
			// TheSplitter
			// 
			this.TheSplitter.Dock = System.Windows.Forms.DockStyle.Right;
			this.TheSplitter.DoNotSaveSplitterLayout = false;
			this.TheSplitter.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(357, 0, true);
			this.TheSplitter.Name = "TheSplitter";
			this.TheSplitter.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(3, 424, true);
			this.TheSplitter.TabIndex = 2;
			this.TheSplitter.TabStop = false;
			// 
			// MessageGrid
			// 
			this.MessageGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.MessageGrid, "DiscardedMessages");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).DiscardedMessages)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).DiscardedMessages)).SyncRoot)).EM_MessageType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).DiscardedMessages)).SyncRoot)).EM_MessageNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).DiscardedMessages)).SyncRoot)).EM_MessageDateTime)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).DiscardedMessages)).SyncRoot)).EM_SystemCreateTimeUtc)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).DiscardedMessages)).SyncRoot)).EM_InterchangeNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).DiscardedMessages)).SyncRoot)).EM_DateTimeInterchangeSent)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).DiscardedMessages)).SyncRoot)).EM_User)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).DiscardedMessages)).SyncRoot)).EM_Status)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).DiscardedMessages)).SyncRoot)).EM_ReceiveTransmit)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).DiscardedMessages)).SyncRoot)).EM_MessageSubType)));
			this.MessageGrid.CaptionVisible = false;
			messageCodeColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("e8e559d3-87d0-43f9-b613-ab21634d8aa3", "Type");
			messageCodeColumnStyleInfo1.ColumnName = "EM_MessageType";
			messageCodeColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(52);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("a257a1d1-9d72-4373-9cd7-e8d498558354", "Message No");
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "EM_MessageNum";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(88);
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("1b691a4a-d4dd-4bbe-9543-6b62aa2a617c", "Message Time");
			zDateEditColumnStyleInfo1.ColumnName = "EM_MessageDateTime";
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(98);
			zDateEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("337a357f-a682-4bbb-81fe-e22b8c11d527", "Create Time (UTC)");
			zDateEditColumnStyleInfo2.ColumnName = "EM_SystemCreateTimeUtc";
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(116);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("8c3b8119-f587-41d3-a989-224909af5344", "Interchange No");
			zTextBoxColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo2.ColumnName = "EM_InterchangeNumber";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(133);
			zDateEditColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("85cee0dc-57aa-4c81-a29b-8ac7bc929217", "Interchange Time");
			zDateEditColumnStyleInfo3.ColumnName = "EM_DateTimeInterchangeSent";
			zDateEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(112);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("4b4f570f-b2fd-4f48-a3e3-dab3ed48fec2", "Sender");
			zTextBoxColumnStyleInfo3.ColumnName = "EM_User";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130);
			messageCodeColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("a4138809-219b-4a1d-a85c-c75079ad2683", "Status");
			messageCodeColumnStyleInfo2.ColumnName = "EM_Status";
			messageCodeColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(58);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("2b566da8-df01-477f-9dcc-4e3f9b24f3d8", "Direction");
			zTextBoxColumnStyleInfo4.ColumnName = "EM_ReceiveTransmit";
			zTextBoxColumnStyleInfo4.ToolTip = "TRX:sent to Customs RCV: received from Customs";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(72);
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("e262bdd9-6c02-41c8-b3e9-f4e83074525d", "Sub Type");
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
			this.MessageGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MessageGrid.GridId = "D3D607C4-C9E9-44E8-BCD3-9B7E587F015D";
			this.MessageGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.MessageGrid.LayoutKey = "MessageGrid";
			this.MessageGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MessageGrid.Name = "MessageGrid";
			this.MessageGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(357, 424, true);
			this.MessageGrid.TabIndex = 3;
			// 
			// DiscardedMessageUserControl
			// 
			this.Controls.Add(this.MessageGrid);
			this.Controls.Add(this.TheSplitter);
			this.Controls.Add(this.MessageTextGroupBox);
			this.CaptionRenderingEnabled = true;
			this.Name = "DiscardedMessageUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(664, 424, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MessageTextGroupBox.ResumeLayout(false);
			this.MessageTextGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageGrid)).EndInit();
			this.MessageGrid.ResumeLayout(false);
			this.MessageGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		protected ZArchitecture.GUI.ZGroupBox MessageTextGroupBox;
		private ZArchitecture.ZTextBox MessageTextBoundTextBox;
		private CargoWise.Windows.UI.KSplitter TheSplitter;
		protected Enterprise.Messaging.GUI.MessageZGrid MessageGrid;
	}
}
