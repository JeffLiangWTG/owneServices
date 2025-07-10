namespace Enterprise.Customs.TW.GUI
{
	public partial class TranshipmentMessageDetailsUserControl
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
		void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.MessagesTopSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.InBondMessagesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.InBondMessagesGrid = new Enterprise.Messaging.GUI.MessageZGrid();
			this.InBondMessageTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.InBondMessageDetailsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.HtmlInterpretationBox = new Enterprise.Messaging.GUI.HtmlInterpretationBox();
			this.InBondMessageTextTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.InBondMessageTextTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.MessagesTopSplitContainer)).BeginInit();
			this.MessagesTopSplitContainer.Panel1.SuspendLayout();
			this.MessagesTopSplitContainer.Panel2.SuspendLayout();
			this.MessagesTopSplitContainer.SuspendLayout();
			this.InBondMessagesGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.InBondMessagesGrid)).BeginInit();
			this.InBondMessagesGrid.SuspendLayout();
			this.InBondMessageTabControl.SuspendLayout();
			this.InBondMessageDetailsTabPage.SuspendLayout();
			this.InBondMessageTextTabPage.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.TW.Business.CusInBondHeader);
			// 
			// MessagesTopSplitContainer
			// 
			this.MessagesTopSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MessagesTopSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MessagesTopSplitContainer.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(996, 526, true);
			this.MessagesTopSplitContainer.Name = "MessagesTopSplitContainer";
			this.MessagesTopSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// MessagesTopSplitContainer.Panel1
			// 
			this.MessagesTopSplitContainer.Panel1.Controls.Add(this.InBondMessagesGroupBox);
			this.MessagesTopSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(996, 526, true);
			this.MessagesTopSplitContainer.Panel1MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(100);
			// 
			// MessagesTopSplitContainer.Panel2
			// 
			this.MessagesTopSplitContainer.Panel2.Controls.Add(this.InBondMessageTabControl);
			this.MessagesTopSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(200);
			this.MessagesTopSplitContainer.TabIndex = 0;
			// 
			// InBondMessagesGroupBox
			// 
			this.InBondMessagesGroupBox.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("TWInBondMessageDetailsUserControl|43809E63-5900-4182-8719-59E997FF43E7", "Messages");
			this.InBondMessagesGroupBox.Controls.Add(this.InBondMessagesGrid);
			this.InBondMessagesGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.InBondMessagesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.InBondMessagesGroupBox.Name = "InBondMessagesGroupBox";
			this.InBondMessagesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(996, 200, true);
			this.InBondMessagesGroupBox.TabIndex = 0;
			this.InBondMessagesGroupBox.TabStop = false;
			// 
			// InBondMessagesGrid
			// 
			this.InBondMessagesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.InBondMessagesGrid, "Messages");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.TW.Business.CusInBondHeader)(null)).Messages)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.TWMessage)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.CusInBondHeader)(null)).Messages)).SyncRoot)).EM_MessageType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.TWMessage)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.CusInBondHeader)(null)).Messages)).SyncRoot)).MessageCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.TWMessage)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.CusInBondHeader)(null)).Messages)).SyncRoot)).MessageTypeDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.TWMessage)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.CusInBondHeader)(null)).Messages)).SyncRoot)).EM_Status)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.TWMessage)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.CusInBondHeader)(null)).Messages)).SyncRoot)).EM_ReceiveTransmit)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.TW.Business.TWMessage)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.CusInBondHeader)(null)).Messages)).SyncRoot)).EM_SystemCreateTimeUtc)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.TWMessage)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.CusInBondHeader)(null)).Messages)).SyncRoot)).EM_SystemCreateUser)));
			this.InBondMessagesGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.Caption = Enterprise.Customs.TW.GUI.Res.GetString("TWInBondMessageDetailsUserControl|03AB8D8A-6B28-4B18-943D-DDE1B9EB482A", "Message Type");
			zTextBoxColumnStyleInfo1.ColumnName = "EM_MessageType";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo2.Caption = Enterprise.Customs.TW.GUI.Res.GetString("TWInBondMessageDetailsUserControl|48E97758-5B94-441F-8F5B-641E64A8654F", "Message Code");
			zTextBoxColumnStyleInfo2.ColumnName = "MessageCode";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo3.Caption = Enterprise.Customs.TW.GUI.Res.GetString("TWInBondMessageDetailsUserControl|29E72F01-91B7-4F71-A786-3094AAD02A27", "Message Type Description");
			zTextBoxColumnStyleInfo3.ColumnName = "MessageTypeDescription";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			zTextBoxColumnStyleInfo4.Caption = Enterprise.Customs.TW.GUI.Res.GetString("TWInBondMessageDetailsUserControl|FD41C024-188A-4489-8025-2172591C8255", "Status");
			zTextBoxColumnStyleInfo4.ColumnName = "EM_Status";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo5.Caption = Enterprise.Customs.TW.GUI.Res.GetString("TWInBondMessageDetailsUserControl|D13CCBB9-CE8C-4BE8-8782-0EECFA1F7BCD", "Direction");
			zTextBoxColumnStyleInfo5.ColumnName = "EM_ReceiveTransmit";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo1.Caption = Enterprise.Customs.TW.GUI.Res.GetString("TWInBondMessageDetailsUserControl|0AC9A412-4658-4555-84CB-0D226EF29791", "Create Time UTC");
			zDateEditColumnStyleInfo1.ColumnName = "EM_SystemCreateTimeUtc";
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo6.Caption = Enterprise.Customs.TW.GUI.Res.GetString("TWInBondMessageDetailsUserControl|0D6AE57F-6F50-4865-B816-0F0FAE4D9E0A", "Create User");
			zTextBoxColumnStyleInfo6.ColumnName = "EM_SystemCreateUser";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.InBondMessagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.InBondMessagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.InBondMessagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.InBondMessagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.InBondMessagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.InBondMessagesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.InBondMessagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.InBondMessagesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.InBondMessagesGrid.GridId = "273AF974-946C-4882-A940-5B7C7AEAB202";
			this.InBondMessagesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.InBondMessagesGrid.LayoutKey = "InBondMessagesGrid";
			this.InBondMessagesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.InBondMessagesGrid.Name = "InBondMessagesGrid";
			this.InBondMessagesGrid.ReadOnly = true;
			this.InBondMessagesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(990, 181, true);
			this.InBondMessagesGrid.TabIndex = 1;
			// 
			// InBondMessageTabControl
			// 
			this.InBondMessageTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.InBondMessageTabControl.Controls.Add(this.InBondMessageDetailsTabPage);
			this.InBondMessageTabControl.Controls.Add(this.InBondMessageTextTabPage);
			this.InBondMessageTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.InBondMessageTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.InBondMessageTabControl.Name = "InBondMessageTabControl";
			this.InBondMessageTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(996, 322, true);
			this.InBondMessageTabControl.TabIndex = 1;
			// 
			// InBondMessageDetailsTabPage
			// 
			this.InBondMessageDetailsTabPage.BackColor = System.Drawing.SystemColors.Control;
			this.InBondMessageDetailsTabPage.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("TWInBondMessageDetailsUserControl|DB903289-3F32-4F94-88A6-9A35E11B0DFF", "Message Details");
			this.InBondMessageDetailsTabPage.Controls.Add(this.HtmlInterpretationBox);
			this.InBondMessageDetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.InBondMessageDetailsTabPage.Name = "InBondMessageDetailsTabPage";
			this.InBondMessageDetailsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.InBondMessageDetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(988, 295, true);
			this.InBondMessageDetailsTabPage.TabIndex = 0;
			// 
			// HtmlInterpretationBox
			// 
			this.HtmlInterpretationBox.AllowWebBrowserDrop = false;
			this.BindingSource.SetBindingMember(this.HtmlInterpretationBox, "Messages.EM_MessageInterpretation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.TWMessage)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.CusInBondHeader)(null)).Messages)).SyncRoot)).EM_MessageInterpretation)));
			this.HtmlInterpretationBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.HtmlInterpretationBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.HtmlInterpretationBox.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(20, 20, true);
			this.HtmlInterpretationBox.Name = "HtmlInterpretationBox";
			this.HtmlInterpretationBox.ScriptErrorsSuppressed = true;
			this.HtmlInterpretationBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(982, 289, true);
			this.HtmlInterpretationBox.TabIndex = 0;
			// 
			// InBondMessageTextTabPage
			// 
			this.InBondMessageTextTabPage.BackColor = System.Drawing.SystemColors.Control;
			this.InBondMessageTextTabPage.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("TWSInBondMessageDetailsUserControl|4EC62137-6841-443C-A244-5AE49E3D634E", "Message Text");
			this.InBondMessageTextTabPage.Controls.Add(this.InBondMessageTextTextBox);
			this.InBondMessageTextTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.InBondMessageTextTabPage.Name = "InBondMessageTextTabPage";
			this.InBondMessageTextTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.InBondMessageTextTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(988, 295, true);
			this.InBondMessageTextTabPage.TabIndex = 1;
			// 
			// InBondMessageTextTextBox
			// 
			this.InBondMessageTextTextBox.BackColor = System.Drawing.SystemColors.Control;
			this.BindingSource.SetBindingMember(this.InBondMessageTextTextBox, "Messages.EM_FormattedMessageText");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.TWMessage)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.CusInBondHeader)(null)).Messages)).SyncRoot)).EM_FormattedMessageText)));
			this.InBondMessageTextTextBox.CaptionResourceString = null;
			this.InBondMessageTextTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.InBondMessageTextTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.InBondMessageTextTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.InBondMessageTextTextBox.Multiline = true;
			this.InBondMessageTextTextBox.Name = "InBondMessageTextTextBox";
			this.InBondMessageTextTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.InBondMessageTextTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(982, 289, true);
			this.InBondMessageTextTextBox.TabIndex = 0;
			this.InBondMessageTextTextBox.HideSelection = false;
			this.InBondMessageTextTextBox.EnableFindDialog = true;
			// 
			// TWInBondMessageDetailsUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.MessagesTopSplitContainer);
			this.Name = "TWInBondMessageDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(996, 526, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MessagesTopSplitContainer.Panel1.ResumeLayout(false);
			this.MessagesTopSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.MessagesTopSplitContainer)).EndInit();
			this.MessagesTopSplitContainer.ResumeLayout(false);
			this.MessagesTopSplitContainer.PerformLayout();
			this.InBondMessagesGroupBox.ResumeLayout(false);
			this.InBondMessagesGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.InBondMessagesGrid)).EndInit();
			this.InBondMessagesGrid.ResumeLayout(false);
			this.InBondMessagesGrid.PerformLayout();
			this.InBondMessageTabControl.ResumeLayout(false);
			this.InBondMessageTabControl.PerformLayout();
			this.InBondMessageDetailsTabPage.ResumeLayout(false);
			this.InBondMessageDetailsTabPage.PerformLayout();
			this.InBondMessageTextTabPage.ResumeLayout(false);
			this.InBondMessageTextTabPage.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private CargoWise.Windows.UI.KSplitContainer MessagesTopSplitContainer;
		private Enterprise.ZArchitecture.GUI.ZGroupBox InBondMessagesGroupBox;
		private Enterprise.Messaging.GUI.MessageZGrid InBondMessagesGrid;
		private Enterprise.ZArchitecture.GUI.ZTabControl InBondMessageTabControl;
		private Enterprise.ZArchitecture.GUI.ZTabPage InBondMessageDetailsTabPage;
		private Enterprise.ZArchitecture.GUI.ZTabPage InBondMessageTextTabPage;
		private Enterprise.ZArchitecture.ZTextBox InBondMessageTextTextBox;
		private Enterprise.Messaging.GUI.HtmlInterpretationBox HtmlInterpretationBox;
	}
}
