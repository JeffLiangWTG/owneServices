namespace Enterprise.MasterFiles.GUI.Accounting.Netting
{
	partial class NettingSetupForm
	{
		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			this.mainPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.nettingPeriodStartDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.nettingSystemDescTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.nettingSystemCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.nettingCentreCompanyDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.saveButtonsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.ButtonsUserControl = new Enterprise.Core.Forms.ZPostingButtonsUserControl();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.mainPanel.SuspendLayout();
			this.nettingPeriodStartDateEdit.SuspendLayout();
			this.nettingCentreCompanyDropEdit.SuspendLayout();
			this.saveButtonsPanel.SuspendLayout();
			this.ButtonsUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 140, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(414, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.Accounting.Netting.NettingSetupManager);
			// 
			// mainPanel
			// 
			this.mainPanel.Controls.Add(this.nettingPeriodStartDateEdit);
			this.mainPanel.Controls.Add(this.nettingSystemDescTextBox);
			this.mainPanel.Controls.Add(this.nettingSystemCodeTextBox);
			this.mainPanel.Controls.Add(this.nettingCentreCompanyDropEdit);
			this.mainPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 14, true);
			this.mainPanel.Name = "mainPanel";
			this.mainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(395, 113, true);
			this.mainPanel.TabIndex = 0;
			// 
			// nettingPeriodStartDateEdit
			// 
			this.nettingPeriodStartDateEdit.AllowDrop = true;
			this.nettingPeriodStartDateEdit.AutoCompleteMonthThreshold = 1;
			this.nettingPeriodStartDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.nettingPeriodStartDateEdit, "NettingCycleStartDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.Accounting.Netting.NettingSetupManager)(null)).NettingCycleStartDate)));
			this.nettingPeriodStartDateEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("2711c09f-f7e4-4704-8962-22b07a5a9f1a", "First Netting Cycle Start Date");
			this.nettingPeriodStartDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(169, 87, true);
			this.nettingPeriodStartDateEdit.Name = "nettingPeriodStartDateEdit";
			this.nettingPeriodStartDateEdit.TabIndex = 5;
			// 
			// nettingSystemDescTextBox
			// 
			this.BindingSource.SetBindingMember(this.nettingSystemDescTextBox, "NettingSystemDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.Accounting.Netting.NettingSetupManager)(null)).NettingSystemDescription)));
			this.nettingSystemDescTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("b91eb19d-6c41-43c2-998b-38b7ae976285", "Netting System Description");
			this.nettingSystemDescTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(169, 65, true);
			this.nettingSystemDescTextBox.Name = "nettingSystemDescTextBox";
			this.nettingSystemDescTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(215, 17, true);
			this.nettingSystemDescTextBox.TabIndex = 2;
			// 
			// nettingSystemCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.nettingSystemCodeTextBox, "NettingSystemCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.Accounting.Netting.NettingSetupManager)(null)).NettingSystemCode)));
			this.nettingSystemCodeTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("1f2b7196-a4db-49da-9436-bfffd88c7106", "Netting System Code");
			this.nettingSystemCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(169, 44, true);
			this.nettingSystemCodeTextBox.Name = "nettingSystemCodeTextBox";
			this.nettingSystemCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(215, 17, true);
			this.nettingSystemCodeTextBox.TabIndex = 1;
			// 
			// nettingCentreCompanyDropEdit
			// 
			this.nettingCentreCompanyDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.nettingCentreCompanyDropEdit, "NettingSystemCompanyCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.Accounting.Netting.NettingSetupManager)(null)).NettingSystemCompanyCode)));
			this.nettingCentreCompanyDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("9cb02f4d-0f2a-47c7-9e3e-beae316961d4", "Netting System Company");
			this.nettingCentreCompanyDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(170, 23, true);
			this.nettingCentreCompanyDropEdit.Name = "nettingCentreCompanyDropEdit";
			this.nettingCentreCompanyDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(215, 17, true);
			this.nettingCentreCompanyDropEdit.TabIndex = 0;
			// 
			// saveButtonsPanel
			// 
			this.saveButtonsPanel.Controls.Add(this.ButtonsUserControl);
			this.saveButtonsPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.saveButtonsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 164, true);
			this.saveButtonsPanel.Name = "saveButtonsPanel";
			this.saveButtonsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(414, 37, true);
			this.saveButtonsPanel.TabIndex = 1;
			// 
			// ButtonsUserControl
			// 
			this.ButtonsUserControl.AllowDrop = true;
			this.ButtonsUserControl.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.ButtonsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(165, 7, true);
			this.ButtonsUserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.ButtonsUserControl.Name = "ButtonsUserControl";
			this.ButtonsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.ButtonsUserControl.TabIndex = 0;
			// 
			// NettingSetupForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("948452f3-8dc8-485c-abc2-a91e6fc9f707", "Setup Netting System");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(414, 201, true);
			this.Controls.Add(this.mainPanel);
			this.Controls.Add(this.saveButtonsPanel);
			this.DataSourceType = typeof(Enterprise.MasterFiles.Business.Accounting.Netting.NettingSetupManager);
			this.Name = "NettingSetupForm";
			this.Text = "NettingSetupForm";
			this.Controls.SetChildIndex(this.saveButtonsPanel, 0);
			this.Controls.SetChildIndex(this.mainPanel, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.mainPanel.ResumeLayout(false);
			this.mainPanel.PerformLayout();
			this.nettingPeriodStartDateEdit.ResumeLayout(true);
			this.nettingPeriodStartDateEdit.PerformLayout();
			this.nettingCentreCompanyDropEdit.ResumeLayout(true);
			this.nettingCentreCompanyDropEdit.PerformLayout();
			this.saveButtonsPanel.ResumeLayout(false);
			this.saveButtonsPanel.PerformLayout();
			this.ButtonsUserControl.ResumeLayout(true);
			this.ButtonsUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZPanel mainPanel;
		private Enterprise.ZArchitecture.GUI.ZPanel saveButtonsPanel;
		protected Enterprise.Core.Forms.ZPostingButtonsUserControl ButtonsUserControl;
		private Enterprise.ZArchitecture.ZTextBox nettingSystemDescTextBox;
		private Enterprise.ZArchitecture.ZTextBox nettingSystemCodeTextBox;
		private Enterprise.ZArchitecture.GUI.ZDropEdit nettingCentreCompanyDropEdit;
		private ZArchitecture.GUI.ZDateEdit nettingPeriodStartDateEdit;
	}
}
