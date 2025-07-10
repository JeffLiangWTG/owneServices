using Enterprise.Freight.Forwarding.DataTransfer;

namespace Enterprise.Freight.Forwarding.GUI
{
	partial class CargoIMPPhase2Control
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

		private Enterprise.ZArchitecture.GUI.ZPanel MainPanel;
		private Enterprise.ZArchitecture.GUI.ZPanel LeftPanel;
		private Enterprise.ZArchitecture.GUI.ZPanel RightPanel;
		private Enterprise.ZArchitecture.ZGrid EDIMessageGrid;
		private CargoWise.Windows.UI.KSplitter splitter1;
		private Enterprise.ZArchitecture.ZTextBox MessageTextTextBox;
		private Enterprise.ZArchitecture.ZTextBox CurrentStatusTextBox;

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
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			this.MainPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.RightPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.MessageTextTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.LeftPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.MessageProcessingLogGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.MessageProcessingLogTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CurrentStatusTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.EDIMessageGrid = new Enterprise.ZArchitecture.ZGrid();
			this.splitter1 = new CargoWise.Windows.UI.KSplitter();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.MainPanel.SuspendLayout();
			this.RightPanel.SuspendLayout();
			this.LeftPanel.SuspendLayout();
			this.MessageProcessingLogGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.EDIMessageGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(CargoIMPPhase2MessageManager);
			// 
			// MainPanel
			// 
			this.MainPanel.Controls.Add(this.RightPanel);
			this.MainPanel.Controls.Add(this.LeftPanel);
			this.MainPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MainPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MainPanel.Name = "MainPanel";
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1016, 464, true);
			this.MainPanel.TabIndex = 0;
			// 
			// RightPanel
			// 
			this.RightPanel.Controls.Add(this.MessageTextTextBox);
			this.RightPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.RightPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(632, 0, true);
			this.RightPanel.Name = "RightPanel";
			this.RightPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(384, 464, true);
			this.RightPanel.TabIndex = 1;
			// 
			// MessageTextTextBox
			// 
			this.BindingSource.SetBindingMember(this.MessageTextTextBox, "Messages.EM_MessageText");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CargoIMPPhase2EDIMessage)(((System.Collections.IList)(((CargoIMPPhase2MessageManager)(null)).Messages)).SyncRoot)).EM_MessageText)));
			this.MessageTextTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MessageTextTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MessageTextTextBox.Multiline = true;
			this.MessageTextTextBox.Name = "MessageTextTextBox";
			this.MessageTextTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.MessageTextTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(384, 464, true);
			this.MessageTextTextBox.TabIndex = 0;
			// 
			// LeftPanel
			// 
			this.LeftPanel.Controls.Add(this.MessageProcessingLogGroupBox);
			this.LeftPanel.Controls.Add(this.CurrentStatusTextBox);
			this.LeftPanel.Controls.Add(this.EDIMessageGrid);
			this.LeftPanel.Dock = System.Windows.Forms.DockStyle.Left;
			this.LeftPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.LeftPanel.Name = "LeftPanel";
			this.LeftPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(632, 464, true);
			this.LeftPanel.TabIndex = 0;
			// 
			// MessageProcessingLogGroupBox
			// 
			this.MessageProcessingLogGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.MessageProcessingLogGroupBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("CargoIMPPhase2Control|e78b467a-9ef0-4ee7-9de0-8e8fa3f955f2", "Last Message Creation Log");
			this.MessageProcessingLogGroupBox.Controls.Add(this.MessageProcessingLogTextBox);
			this.MessageProcessingLogGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 285, true);
			this.MessageProcessingLogGroupBox.Name = "MessageProcessingLogGroupBox";
			this.MessageProcessingLogGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(623, 176, true);
			this.MessageProcessingLogGroupBox.TabIndex = 2;
			this.MessageProcessingLogGroupBox.TabStop = false;
			// 
			// MessageProcessingLogTextBox
			// 
			this.BindingSource.SetBindingMember(this.MessageProcessingLogTextBox, "LastMessageCreationLog");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CargoIMPPhase2MessageManager)(null)).LastMessageCreationLog)));
			this.MessageProcessingLogTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.MessageProcessingLogTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.MessageProcessingLogTextBox, false);
			this.MessageProcessingLogTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.MessageProcessingLogTextBox.Multiline = true;
			this.MessageProcessingLogTextBox.Name = "MessageProcessingLogTextBox";
			this.MessageProcessingLogTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(617, 157, true);
			this.MessageProcessingLogTextBox.TabIndex = 0;
			// 
			// CurrentStatusTextBox
			// 
			this.BindingSource.SetBindingMember(this.CurrentStatusTextBox, "CurrentStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CargoIMPPhase2MessageManager)(null)).CurrentStatus)));
			this.CurrentStatusTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.CurrentStatusTextBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("CargoIMPPhase2Control|0e66303a-a15b-41b8-9243-6c3122b279c1", "Last Message Status");
			this.CurrentStatusTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 10, true);
			this.CurrentStatusTextBox.Name = "CurrentStatusTextBox";
			this.CurrentStatusTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(479, 20, true);
			this.CurrentStatusTextBox.TabIndex = 0;
			// 
			// EDIMessageGrid
			// 
			this.EDIMessageGrid.AllowNavigation = false;
			this.EDIMessageGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.EDIMessageGrid, "Messages");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((CargoIMPPhase2MessageManager)(null)).Messages)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CargoIMPPhase2EDIMessage)(((System.Collections.IList)(((CargoIMPPhase2MessageManager)(null)).Messages)).SyncRoot)).EM_MessageNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CargoIMPPhase2EDIMessage)(((System.Collections.IList)(((CargoIMPPhase2MessageManager)(null)).Messages)).SyncRoot)).EM_MessageType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CargoIMPPhase2EDIMessage)(((System.Collections.IList)(((CargoIMPPhase2MessageManager)(null)).Messages)).SyncRoot)).EM_Status)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CargoIMPPhase2EDIMessage)(((System.Collections.IList)(((CargoIMPPhase2MessageManager)(null)).Messages)).SyncRoot)).EM_User)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((CargoIMPPhase2EDIMessage)(((System.Collections.IList)(((CargoIMPPhase2MessageManager)(null)).Messages)).SyncRoot)).EM_MessageDateTime)));
			this.EDIMessageGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.ColumnName = "EM_MessageNum";
			zTextBoxColumnStyleInfo1.IsMandatory = true;
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zTextBoxColumnStyleInfo2.ColumnName = "EM_MessageType";
			zTextBoxColumnStyleInfo2.IsMandatory = true;
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo3.ColumnName = "EM_Status";
			zTextBoxColumnStyleInfo3.IsMandatory = true;
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zTextBoxColumnStyleInfo4.ColumnName = "EM_User";
			zTextBoxColumnStyleInfo4.IsMandatory = true;
			zTextBoxColumnStyleInfo4.IsReadOnly = true;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("CargoIMPPhase2Control|43fb4e2e-d354-4ae4-aae2-0ec558d862dc", "Date");
			zDateEditColumnStyleInfo1.ColumnName = "EM_MessageDateTime";
			zDateEditColumnStyleInfo1.IsMandatory = true;
			zDateEditColumnStyleInfo1.IsReadOnly = true;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			this.EDIMessageGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.EDIMessageGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.EDIMessageGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.EDIMessageGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.EDIMessageGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.EDIMessageGrid.GridId = "9c8c4001-f1e2-4416-99a0-fb1547164762";
			this.EDIMessageGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.EDIMessageGrid.LayoutKey = "EDIMessageGrid";
			this.EDIMessageGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 48, true);
			this.EDIMessageGrid.Name = "EDIMessageGrid";
			this.EDIMessageGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(632, 231, true);
			this.EDIMessageGrid.TabIndex = 1;
			// 
			// splitter1
			// 
			this.splitter1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.splitter1.Name = "splitter1";
			this.splitter1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(3, 3, true);
			this.splitter1.TabIndex = 0;
			this.splitter1.TabStop = false;
			// 
			// CargoIMPPhase2Control
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.MainPanel);
			this.Name = "CargoIMPPhase2Control";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1016, 464, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MainPanel.ResumeLayout(false);
			this.RightPanel.ResumeLayout(false);
			this.RightPanel.PerformLayout();
			this.LeftPanel.ResumeLayout(false);
			this.LeftPanel.PerformLayout();
			this.MessageProcessingLogGroupBox.ResumeLayout(false);
			this.MessageProcessingLogGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.EDIMessageGrid)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZGroupBox MessageProcessingLogGroupBox;
		private Enterprise.ZArchitecture.ZTextBox MessageProcessingLogTextBox;
	}
}
