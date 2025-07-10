
using Enterprise.MasterFiles.GUI;
namespace Enterprise.Customs.US.GUI
{
	partial class CusLiquidationForm
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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		protected override void InitializeComponent()
		{
			this.MessageTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.EntryTopPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.EntryDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.JobCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.EntryTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.EntryNumberZTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.EntryFilerCodeZTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.liquidationMessageUserControl = new Enterprise.Customs.US.GUI.LiquidationMessageUserControl();
			this.liquidationDetailsUserControl = new Enterprise.Customs.US.GUI.LiquidationDetailsUserControl();
			this.MainTabControl.SuspendLayout();
			this.MainTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.MessageTabPage.SuspendLayout();
			this.EntryTopPanel.SuspendLayout();
			this.EntryDetailsGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel
			// 
			this.PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel.TabIndex = 0;
			// 
			// MainTabControl
			// 
			this.MainTabControl.Controls.Add(this.MessageTabPage);
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(952, 405, true);
			this.MainTabControl.TabIndex = 0;
			this.MainTabControl.Controls.SetChildIndex(this.MessageTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.MainTabPage, 0);
			// 
			// MainTabPage
			// 
			this.MainTabPage.Controls.Add(this.liquidationDetailsUserControl);
			this.MainTabPage.Controls.Add(this.EntryTopPanel);
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(944, 378, true);
			this.MainTabPage.Text = "Details";
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(952, 24, true);
			this.MainStatusBar.TabIndex = 0;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.Business.CusLiquidation);
			// 
			// MessageTabPage
			// 
			this.MessageTabPage.BackColor = System.Drawing.SystemColors.Control;
			this.MessageTabPage.Controls.Add(this.liquidationMessageUserControl);
			this.MessageTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.MessageTabPage.Name = "MessageTabPage";
			this.MessageTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.MessageTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(944, 399, true);
			this.MessageTabPage.TabIndex = 3;
			this.MessageTabPage.Text = "Message";
			// 
			// EntryTopPanel
			// 
			this.EntryTopPanel.Controls.Add(this.EntryDetailsGroupBox);
			this.EntryTopPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.EntryTopPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.EntryTopPanel.Name = "EntryTopPanel";
			this.EntryTopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(944, 58, true);
			this.EntryTopPanel.TabIndex = 3;
			// 
			// EntryDetailsGroupBox
			// 
			this.EntryDetailsGroupBox.Controls.Add(this.JobCodeFindBox);
			this.EntryDetailsGroupBox.Controls.Add(this.EntryTypeDropEdit);
			this.EntryDetailsGroupBox.Controls.Add(this.EntryNumberZTextBox);
			this.EntryDetailsGroupBox.Controls.Add(this.EntryFilerCodeZTextBox);
			this.EntryDetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.EntryDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.EntryDetailsGroupBox.Name = "EntryDetailsGroupBox";
			this.EntryDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(944, 58, true);
			this.EntryDetailsGroupBox.TabIndex = 0;
			this.EntryDetailsGroupBox.TabStop = false;
			this.EntryDetailsGroupBox.Text = "Entry Details";
			// 
			// JobCodeFindBox
			// 
			this.BindingSource.SetBindingMember(this.JobCodeFindBox, "B8_BrokerReferenceNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.CusLiquidation)(null)).B8_BrokerReferenceNo)));
			this.JobCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(815, 23, true);
			this.JobCodeFindBox.Name = "JobCodeFindBox";
			this.JobCodeFindBox.PreBoundMaxLength = 12;
			this.JobCodeFindBox.ShowDescriptionBox = false;
			this.JobCodeFindBox.ShowNewFormWhenEmpty = false;
			this.JobCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(121, 20, true);
			this.JobCodeFindBox.TabIndex = 3;
			// 
			// EntryTypeDropEdit
			// 
			this.BindingSource.SetBindingMember(this.EntryTypeDropEdit, "B8_EntryType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.CusLiquidation)(null)).B8_EntryType)));
			this.EntryTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(413, 23, true);
			this.EntryTypeDropEdit.Name = "EntryTypeDropEdit";
			this.EntryTypeDropEdit.PreBoundMaxLength = 1;
			this.EntryTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(318, 20, true);
			this.EntryTypeDropEdit.TabIndex = 2;
			// 
			// EntryNumberZTextBox
			// 
			this.EntryNumberZTextBox.AcceptsReturn = true;
			this.BindingSource.SetBindingMember(this.EntryNumberZTextBox, "B8_EntryNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.CusLiquidation)(null)).B8_EntryNumber)));
			this.EntryNumberZTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(230, 23, true);
			this.EntryNumberZTextBox.Name = "EntryNumberZTextBox";
			this.EntryNumberZTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(99, 20, true);
			this.EntryNumberZTextBox.TabIndex = 1;
			// 
			// EntryFilerCodeZTextBox
			// 
			this.BindingSource.SetBindingMember(this.EntryFilerCodeZTextBox, "B8_EntryFilerCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.CusLiquidation)(null)).B8_EntryFilerCode)));
			this.EntryFilerCodeZTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 23, true);
			this.EntryFilerCodeZTextBox.Name = "EntryFilerCodeZTextBox";
			this.EntryFilerCodeZTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(42, 20, true);
			this.EntryFilerCodeZTextBox.TabIndex = 0;
			// 
			// liquidationMessageUserControl
			// 
			this.BindingSource.SetBindingMember(this.liquidationMessageUserControl, ".");
			this.liquidationMessageUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.liquidationMessageUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.liquidationMessageUserControl.Name = "liquidationMessageUserControl";
			this.liquidationMessageUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(938, 393, true);
			this.liquidationMessageUserControl.TabIndex = 7;
			// 
			// liquidationDetailsUserControl
			// 
			this.BindingSource.SetBindingMember(this.liquidationDetailsUserControl, ".");
			this.liquidationDetailsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.liquidationDetailsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 58, true);
			this.liquidationDetailsUserControl.Name = "liquidationDetailsUserControl";
			this.liquidationDetailsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(944, 320, true);
			this.liquidationDetailsUserControl.TabIndex = 1;
			// 
			// CusLiquidationForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("CusLiquidationForm|948e8d12-7563-40a8-a0c3-6c7990a9824c", "Liquidation Notice");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(952, 551, true);
			this.DataSourceType = typeof(Enterprise.Customs.US.Business.CusLiquidation);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(968, 610, true);
			this.Name = "CusLiquidationForm";
			this.ShouldSerializeTabPageMethods = false;
			this.Text = "Liquidation Notice Form";
			this.MainTabControl.ResumeLayout(false);
			this.MainTabPage.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MessageTabPage.ResumeLayout(false);
			this.EntryTopPanel.ResumeLayout(false);
			this.EntryDetailsGroupBox.ResumeLayout(false);
			this.EntryDetailsGroupBox.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZTabPage MessageTabPage;
		private Enterprise.ZArchitecture.GUI.ZPanel EntryTopPanel;
		private Enterprise.ZArchitecture.GUI.ZGroupBox EntryDetailsGroupBox;
		private Enterprise.ZArchitecture.GUI.ZDropEdit EntryTypeDropEdit;
		private Enterprise.ZArchitecture.ZTextBox EntryNumberZTextBox;
		private Enterprise.ZArchitecture.ZTextBox EntryFilerCodeZTextBox;
		private Enterprise.ZArchitecture.GUI.ZCodeFindBox JobCodeFindBox;
		private LiquidationMessageUserControl liquidationMessageUserControl;
		private LiquidationDetailsUserControl liquidationDetailsUserControl;
	}
}
