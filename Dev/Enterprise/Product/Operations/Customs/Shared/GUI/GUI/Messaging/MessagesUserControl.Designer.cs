namespace Enterprise.Customs.GUI
{
	using System.ComponentModel;
	using System.Windows.Forms;
	using Enterprise.Messaging.GUI;
	using Enterprise.ZArchitecture;
	using Enterprise.ZArchitecture.GUI;
	using CargoWise.Windows.UI;

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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.MessagesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.MessagesBoundGrid = new Enterprise.Messaging.GUI.MessageZGrid();
			this.VerticalSplitter = new CargoWise.Windows.UI.KSplitter();
			this.MessageTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.MessageDetailsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.HtmlInterpretationBox = new Enterprise.Messaging.GUI.HtmlInterpretationBox();
			this.MessageTextTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.MessageTextTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.MessagesGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessagesBoundGrid)).BeginInit();
			this.MessageTabControl.SuspendLayout();
			this.MessageDetailsTabPage.SuspendLayout();
			this.MessageTextTabPage.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Messaging.Business.IEDIMessageCollectionProvider);
			// 
			// MessagesGroupBox
			// 
			this.MessagesGroupBox.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("MessagesUserControl|4e4e8fc8-f39e-4cd6-9fe8-9f781dcf2daf", "Messages");
			this.MessagesGroupBox.Controls.Add(this.MessagesBoundGrid);
			this.MessagesGroupBox.Controls.Add(this.VerticalSplitter);
			this.MessagesGroupBox.Controls.Add(this.MessageTabControl);
			this.MessagesGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MessagesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MessagesGroupBox.Name = "MessagesGroupBox";
			this.MessagesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(876, 509, true);
			this.MessagesGroupBox.TabIndex = 6;
			this.MessagesGroupBox.TabStop = false;
			// 
			// MessagesBoundGrid
			// 
			this.MessagesBoundGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.MessagesBoundGrid, "Messages");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Messaging.Business.IEDIMessageCollectionProvider)(null)).Messages)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Messaging.Business.IEDIMessageCollectionProvider)(null)).Messages)).SyncRoot)).EM_ReceiveTransmit)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Messaging.Business.IEDIMessageCollectionProvider)(null)).Messages)).SyncRoot)).EM_Status)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Messaging.Business.IEDIMessageCollectionProvider)(null)).Messages)).SyncRoot)).EM_MessageType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Messaging.Business.IEDIMessageCollectionProvider)(null)).Messages)).SyncRoot)).EM_MessageSubType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Messaging.Business.IEDIMessageCollectionProvider)(null)).Messages)).SyncRoot)).EM_MessageSubTypeDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Messaging.Business.IEDIMessageCollectionProvider)(null)).Messages)).SyncRoot)).EM_User)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Messaging.Business.IEDIMessageCollectionProvider)(null)).Messages)).SyncRoot)).EM_MessageDateTime)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Messaging.Business.IEDIMessageCollectionProvider)(null)).Messages)).SyncRoot)).EM_SystemCreateTimeUtc)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Messaging.Business.IEDIMessageCollectionProvider)(null)).Messages)).SyncRoot)).EM_MessageNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Messaging.Business.IEDIMessageCollectionProvider)(null)).Messages)).SyncRoot)).EM_InterchangeNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Messaging.Business.IEDIMessageCollectionProvider)(null)).Messages)).SyncRoot)).EM_DateTimeInterchangeSent)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Messaging.Business.IEDIMessageCollectionProvider)(null)).Messages)).SyncRoot)).EM_ApplicationReference)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Messaging.Business.IEDIMessageCollectionProvider)(null)).Messages)).SyncRoot)).EM_InterchangeStatus)));
			this.MessagesBoundGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.ColumnName = "EM_ReceiveTransmit";
			zTextBoxColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo2.ColumnName = "EM_Status";
			zTextBoxColumnStyleInfo3.ColumnName = "EM_MessageType";
			zTextBoxColumnStyleInfo4.ColumnName = "EM_MessageSubType";
			zTextBoxColumnStyleInfo5.ColumnName = "EM_MessageSubTypeDescription";
			zTextBoxColumnStyleInfo6.ColumnName = "EM_User";
			zDateEditColumnStyleInfo1.ColumnName = "EM_MessageDateTime";
			zDateEditColumnStyleInfo2.ColumnName = "EM_SystemCreateTimeUtc";
			zTextBoxColumnStyleInfo7.ColumnName = "EM_MessageNum";
			zTextBoxColumnStyleInfo8.ColumnName = "EM_InterchangeNumber";
			zDateEditColumnStyleInfo3.ColumnName = "EM_DateTimeInterchangeSent";
			zTextBoxColumnStyleInfo9.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("MessagesUserControl|367fdcc2-7601-44de-8abb-aa6a9a93eb90", "Application Reference");
			zTextBoxColumnStyleInfo9.ColumnName = "EM_ApplicationReference";
			zTextBoxColumnStyleInfo9.IsVisible = false;
			zTextBoxColumnStyleInfo10.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("MessagesUserControl|D1918C2A-3DF5-417B-99F9-F3FBE4DF5C7F", "Int. Status");
			zTextBoxColumnStyleInfo10.ColumnName = "EM_InterchangeStatus";
			zTextBoxColumnStyleInfo10.IsVisible = false;
			this.MessagesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.MessagesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.MessagesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.MessagesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.MessagesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.MessagesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.MessagesBoundGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.MessagesBoundGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.MessagesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.MessagesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.MessagesBoundGrid.ColumnStyles.Add(zDateEditColumnStyleInfo3);
			this.MessagesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.MessagesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.MessagesBoundGrid.GridId = "fa17b6d7-672b-4a12-835b-3a013b41c3f9";
			this.MessagesBoundGrid.CopySelectedRowsAllowed = true;
			this.MessagesBoundGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MessagesBoundGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.MessagesBoundGrid.LayoutKey = "MessagesBoundGrid";
			this.MessagesBoundGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.MessagesBoundGrid.Name = "MessagesBoundGrid";
			this.MessagesBoundGrid.ReadOnly = true;
			this.MessagesBoundGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(528, 490, true);
			this.MessagesBoundGrid.TabIndex = 4;
			// 
			// VerticalSplitter
			// 
			this.VerticalSplitter.Dock = System.Windows.Forms.DockStyle.Right;
			this.VerticalSplitter.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(531, 16, true);
			this.VerticalSplitter.Name = "VerticalSplitter";
			this.VerticalSplitter.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(3, 490, true);
			this.VerticalSplitter.TabIndex = 7;
			this.VerticalSplitter.TabStop = false;
			// 
			// MessageTabControl
			// 
			this.MessageTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.MessageTabControl.Controls.Add(this.MessageDetailsTabPage);
			this.MessageTabControl.Controls.Add(this.MessageTextTabPage);
			this.MessageTabControl.Dock = System.Windows.Forms.DockStyle.Right;
			this.MessageTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(534, 16, true);
			this.MessageTabControl.Name = "MessageTabControl";
			this.MessageTabControl.SelectedIndex = 0;
			this.MessageTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(339, 490, true);
			this.MessageTabControl.TabIndex = 6;
			// 
			// MessageDetailsTabPage
			// 
			this.MessageDetailsTabPage.Controls.Add(this.HtmlInterpretationBox);
			this.MessageDetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.MessageDetailsTabPage.Name = "MessageDetailsTabPage";
			this.MessageDetailsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.MessageDetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(331, 463, true);
			this.MessageDetailsTabPage.TabIndex = 0;
			this.MessageDetailsTabPage.Text = Enterprise.Customs.GUI.Res.GetString("61D52FBE-1146-4D5F-870F-F8903DB502E7", "Message Details");
			// 
			// HtmlInterpretationBox
			// 
			this.HtmlInterpretationBox.AllowWebBrowserDrop = false;
			this.BindingSource.SetBindingMember(this.HtmlInterpretationBox, "Messages.EM_MessageInterpretation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Messaging.Business.IEDIMessageCollectionProvider)(null)).Messages)).SyncRoot)).EM_MessageInterpretation)));
			this.HtmlInterpretationBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.HtmlInterpretationBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.HtmlInterpretationBox.Name = "HtmlInterpretationBox";
			this.HtmlInterpretationBox.ScriptErrorsSuppressed = true;
			this.HtmlInterpretationBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(325, 457, true);
			this.HtmlInterpretationBox.TabIndex = 0;
			// 
			// MessageTextTabPage
			// 
			this.MessageTextTabPage.Controls.Add(this.MessageTextTextBox);
			this.MessageTextTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.MessageTextTabPage.Name = "MessageTextTabPage";
			this.MessageTextTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.MessageTextTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(331, 463, true);
			this.MessageTextTabPage.TabIndex = 1;
			this.MessageTextTabPage.Text = Enterprise.Customs.GUI.Res.GetString("DAF27DD3-95CF-45C3-BAB2-83D77E2A2B11", "Message Text");
			// 
			// MessageTextTextBox
			// 
			this.MessageTextTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.MessageTextTextBox, "Messages.EM_FormattedMessageText");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Messaging.Business.IEDIMessageCollectionProvider)(null)).Messages)).SyncRoot)).EM_FormattedMessageText)));
			this.MessageTextTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.MessageTextTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MessageTextTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.MessageTextTextBox.Multiline = true;
			this.MessageTextTextBox.Name = "MessageTextTextBox";
			this.MessageTextTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.MessageTextTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(325, 457, true);
			this.MessageTextTextBox.TabIndex = 0;
			// 
			// MessagesUserControl
			//
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.MessagesGroupBox);
			this.Name = "MessagesUserControl";
			this.ShouldSerializeTabPageMethods = false;
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(876, 509, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MessagesGroupBox.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.MessagesBoundGrid)).EndInit();
			this.MessageTabControl.ResumeLayout(false);
			this.MessageDetailsTabPage.ResumeLayout(false);
			this.MessageTextTabPage.ResumeLayout(false);
			this.MessageTextTabPage.PerformLayout();
			this.ResumeLayout(false);

		}
		#endregion

		protected ZGroupBox MessagesGroupBox;
		protected KSplitter VerticalSplitter;
		protected ZTabControl MessageTabControl;
		protected ZTabPage MessageDetailsTabPage;
		protected ZTabPage MessageTextTabPage;
		protected ZTextBox MessageTextTextBox;
		public Messaging.GUI.MessageZGrid MessagesBoundGrid;
		protected HtmlInterpretationBox HtmlInterpretationBox;

	}
}
