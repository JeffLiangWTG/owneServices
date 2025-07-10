using Enterprise.ZArchitecture.GUI;
using System.Windows.Forms;

namespace Enterprise.Customs.TR.NCTS.GUI
{
	partial class SPTSMessagesTabUserControl
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
			this.components = new System.ComponentModel.Container();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.splitter1 = new CargoWise.Windows.UI.KSplitter();
			this.MessagesSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.MessageGrid = new Enterprise.ZArchitecture.ZGrid();
			this.MessagesTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.InterpretationTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.MessageInterpretationWebBrowser = new Enterprise.Messaging.GUI.HtmlInterpretationBox();
			this.TextTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.MessageEdifactTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.MessagesSplitContainer)).BeginInit();
			this.MessagesSplitContainer.Panel1.SuspendLayout();
			this.MessagesSplitContainer.Panel2.SuspendLayout();
			this.MessagesSplitContainer.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageGrid)).BeginInit();
			this.MessageGrid.SuspendLayout();
			this.MessagesTabControl.SuspendLayout();
			this.InterpretationTabPage.SuspendLayout();
			this.TextTabPage.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.TR.NCTS.Business.SPTSHeader);
			// 
			// splitter1
			// 
			this.splitter1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.splitter1.Name = "splitter1";
			this.splitter1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(3, 575, true);
			this.splitter1.TabIndex = 32;
			this.splitter1.TabStop = false;
			// 
			// MessagesSplitContainer
			// 
			this.MessagesSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MessagesSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 0, true);
			this.MessagesSplitContainer.Name = "MessagesSplitContainer";
			this.MessagesSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// MessagesSplitContainer.Panel1
			// 
			this.MessagesSplitContainer.Panel1.Controls.Add(this.MessageGrid);
			// 
			// MessagesSplitContainer.Panel2
			// 
			this.MessagesSplitContainer.Panel2.Controls.Add(this.MessagesTabControl);
			this.MessagesSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(860, 575, true);
			this.MessagesSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(337);
			this.MessagesSplitContainer.TabIndex = 33;
			// 
			// MessageGrid
			// 
			this.MessageGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.MessageGrid, "Messages");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.TR.NCTS.Business.SPTSHeader)(null)).Messages)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.TR.NCTS.Business.SPTSHeader)(null)).Messages)).SyncRoot)).EM_MessageNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.TR.NCTS.Business.SPTSHeader)(null)).Messages)).SyncRoot)).EM_MessageDateTime)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.TR.NCTS.Business.SPTSHeader)(null)).Messages)).SyncRoot)).EM_MessageType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.TR.NCTS.Business.SPTSHeader)(null)).Messages)).SyncRoot)).EM_MessageSubType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.TR.NCTS.Business.SPTSHeader)(null)).Messages)).SyncRoot)).EM_Status)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.TR.NCTS.Business.SPTSHeader)(null)).Messages)).SyncRoot)).EM_ReceiveTransmit)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.TR.NCTS.Business.SPTSHeader)(null)).Messages)).SyncRoot)).EM_SystemCreateUser)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.TR.NCTS.Business.SPTSHeader)(null)).Messages)).SyncRoot)).EM_CreateUserFullName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.TR.NCTS.Business.SPTSHeader)(null)).Messages)).SyncRoot)).EM_SystemCreateTimeUtc)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.TR.NCTS.Business.SPTSHeader)(null)).Messages)).SyncRoot)).EM_InterchangeNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.TR.NCTS.Business.SPTSHeader)(null)).Messages)).SyncRoot)).EM_InterchangeStatus)));
			this.MessageGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.ColumnName = "EM_MessageNum";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo1.ColumnName = "EM_MessageDateTime";
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.ColumnName = "EM_MessageType";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo3.ColumnName = "EM_MessageSubType";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo4.ColumnName = "EM_Status";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo5.ColumnName = "EM_ReceiveTransmit";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo6.ColumnName = "EM_SystemCreateUser";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo9.ColumnName = "EM_CreateUserFullName";
			zTextBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zDateEditColumnStyleInfo2.ColumnName = "EM_SystemCreateTimeUtc";
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo7.ColumnName = "EM_InterchangeNumber";
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo8.ColumnName = "EM_InterchangeStatus";
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.MessageGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.MessageGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.MessageGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.MessageGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.MessageGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.MessageGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.MessageGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.MessageGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.MessageGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.MessageGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.MessageGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.MessageGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MessageGrid.GridId = "BF065016-2F96-40A4-80D8-DE20DF442859";
			this.MessageGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.MessageGrid.LayoutKey = "MessageGrid";
			this.MessageGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MessageGrid.Name = "MessageGrid";
			this.MessageGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(855, 295, true);
			this.MessageGrid.TabIndex = 32;
			// 
			// MessagesTabControl
			// 
			this.MessagesTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.MessagesTabControl.Controls.Add(this.InterpretationTabPage);
			this.MessagesTabControl.Controls.Add(this.TextTabPage);
			this.MessagesTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MessagesTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MessagesTabControl.Name = "MessagesTabControl";
			this.MessagesTabControl.SelectedIndex = 0;
			this.MessagesTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(860, 234, true);
			this.MessagesTabControl.TabIndex = 0;
			// 
			// InterpretationTabPage
			// 
			this.InterpretationTabPage.Controls.Add(this.MessageInterpretationWebBrowser);
			this.InterpretationTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.InterpretationTabPage.Name = "InterpretationTabPage";
			this.InterpretationTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.InterpretationTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(852, 207, true);
			this.InterpretationTabPage.TabIndex = 0;
			this.InterpretationTabPage.Text = Enterprise.Customs.TR.NCTS.GUI.Res.GetString("4DF12F06-BFBF-4657-ACFF-7E3DC07B0325", "Interpretation");
			this.InterpretationTabPage.UseVisualStyleBackColor = true;
			// 
			// MessageInterpretationWebBrowser
			// 
			this.BindingSource.SetBindingMember(this.MessageInterpretationWebBrowser, "Messages.EM_MessageInterpretation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.TR.NCTS.Business.SPTSHeader)(null)).Messages)).SyncRoot)).EM_MessageInterpretation)));
			this.MessageInterpretationWebBrowser.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MessageInterpretationWebBrowser.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.MessageInterpretationWebBrowser.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(20, 20, true);
			this.MessageInterpretationWebBrowser.Name = "MessageInterpretationWebBrowser";
			this.MessageInterpretationWebBrowser.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(846, 201, true);
			this.MessageInterpretationWebBrowser.TabIndex = 0;
			// 
			// TextTabPage
			// 
			this.TextTabPage.Controls.Add(this.MessageEdifactTextBox);
			this.TextTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.TextTabPage.Name = "TextTabPage";
			this.TextTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.TextTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(852, 207, true);
			this.TextTabPage.TabIndex = 1;
			this.TextTabPage.Text = Enterprise.Customs.TR.NCTS.GUI.Res.GetString("32E0304A-2C80-42D4-8275-BD1B5720EB8A", "Text");
			this.TextTabPage.UseVisualStyleBackColor = true;
			// 
			// MessageEdifactTextBox
			// 
			this.BindingSource.SetBindingMember(this.MessageEdifactTextBox, "Messages.EM_FormattedMessageText");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.TR.NCTS.Business.SPTSHeader)(null)).Messages)).SyncRoot)).EM_FormattedMessageText)));
			this.MessageEdifactTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MessageEdifactTextBox.CharacterCasing = CharacterCasing.Normal;
			this.MessageEdifactTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.MessageEdifactTextBox.Multiline = true;
			this.MessageEdifactTextBox.Name = "MessageEdifactTextBox";
			this.MessageEdifactTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.MessageEdifactTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(846, 201, true);
			this.MessageEdifactTextBox.TabIndex = 0;
			// 
			// SPTSMessagesTabUserControl
			//
			this.CaptionRenderingEnabled = true;
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.BackColor = System.Drawing.SystemColors.Control;
			this.Controls.Add(this.MessagesSplitContainer);
			this.Controls.Add(this.splitter1);
			this.Name = "SPTSMessagesTabUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(863, 575, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MessagesSplitContainer.Panel1.ResumeLayout(false);
			this.MessagesSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.MessagesSplitContainer)).EndInit();
			this.MessagesSplitContainer.ResumeLayout(false);
			this.MessagesSplitContainer.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageGrid)).EndInit();
			this.MessageGrid.ResumeLayout(false);
			this.MessageGrid.PerformLayout();
			this.MessagesTabControl.ResumeLayout(false);
			this.MessagesTabControl.PerformLayout();
			this.InterpretationTabPage.ResumeLayout(false);
			this.InterpretationTabPage.PerformLayout();
			this.TextTabPage.ResumeLayout(false);
			this.TextTabPage.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private CargoWise.Windows.UI.KSplitter splitter1;
		private CargoWise.Windows.UI.KSplitContainer MessagesSplitContainer;
		private ZArchitecture.ZGrid MessageGrid;
		private ZTabControl MessagesTabControl;
		private ZTabPage InterpretationTabPage;
		private Enterprise.Messaging.GUI.HtmlInterpretationBox MessageInterpretationWebBrowser;
		private ZTabPage TextTabPage;
		private ZArchitecture.ZTextBox MessageEdifactTextBox;
	}
}
