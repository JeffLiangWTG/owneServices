namespace Enterprise.Freight.GUI
{
	partial class JobTradeLaneForm
	{
		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		new void InitializeComponent()
		{
			this.DirectionDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CarrierGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.DescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.Location1CodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.Location2CodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.MainTabControl.SuspendLayout();
			this.MainTabPage.SuspendLayout();
			this.MainPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.DirectionDropEdit.SuspendLayout();
			this.CarrierGuidFindBox.SuspendLayout();
			this.Location1CodeFindBox.SuspendLayout();
			this.Location2CodeFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainTabControl
			// 
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(793, 262, true);
			// 
			// MainTabPage
			// 
			this.MainTabPage.Controls.Add(this.Location2CodeFindBox);
			this.MainTabPage.Controls.Add(this.Location1CodeFindBox);
			this.MainTabPage.Controls.Add(this.DescriptionTextBox);
			this.MainTabPage.Controls.Add(this.CarrierGuidFindBox);
			this.MainTabPage.Controls.Add(this.DirectionDropEdit);
			this.MainTabPage.Controls.Add(this.CodeTextBox);
			this.MainTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 20, true);
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(786, 238, true);
			// 
			// LogsTabPage
			// 
			this.LogsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 20, true);
			this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(786, 238, true);
			// 
			// MainPanel
			// 
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(793, 262, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(793, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Business.JobTradeLane);
			// 
			// DirectionDropEdit
			// 
			this.BindingSource.SetBindingMember(this.DirectionDropEdit, "EJ_Direction");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Business.JobTradeLane)(null)).EJ_Direction)));
			this.DirectionDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 54, true);
			this.DirectionDropEdit.Name = "DirectionDropEdit";
			this.DirectionDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 19, true);
			this.DirectionDropEdit.TabIndex = 4;
			// 
			// CarrierGuidFindBox
			// 
			this.BindingSource.SetBindingMember(this.CarrierGuidFindBox, "EJ_OH_RelatedOrg");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.Business.JobTradeLane)(null)).EJ_OH_RelatedOrg)));
			this.CarrierGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 83, true);
			this.CarrierGuidFindBox.Name = "CarrierGuidFindBox";
			this.CarrierGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 19, true);
			this.CarrierGuidFindBox.TabIndex = 5;
			// 
			// DescriptionTextBox
			// 
			this.BindingSource.SetBindingMember(this.DescriptionTextBox, "EJ_Description");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.JobTradeLane)(null)).EJ_Description)));
			this.DescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(518, 26, true);
			this.DescriptionTextBox.Name = "DescriptionTextBox";
			this.DescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 19, true);
			this.DescriptionTextBox.TabIndex = 8;
			// 
			// CodeTextBox
			// 
			this.CodeTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.CodeTextBox, "EJ_Code");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.JobTradeLane)(null)).EJ_Code)));
			this.CodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 26, true);
			this.CodeTextBox.Name = "CodeTextBox";
			this.CodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 19, true);
			this.CodeTextBox.TabIndex = 3;
			// 
			// Location1CodeFindBox
			// 
			this.BindingSource.SetBindingMember(this.Location1CodeFindBox, "EJ_Location1");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.JobTradeLane)(null)).EJ_Location1)));
			this.Location1CodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(518, 54, true);
			this.Location1CodeFindBox.Name = "Location1CodeFindBox";
			this.Location1CodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 19, true);
			this.Location1CodeFindBox.TabIndex = 9;
			// 
			// Location2CodeFindBox
			// 
			this.BindingSource.SetBindingMember(this.Location2CodeFindBox, "EJ_Location2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.JobTradeLane)(null)).EJ_Location2)));
			this.Location2CodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(518, 83, true);
			this.Location2CodeFindBox.Name = "Location2CodeFindBox";
			this.Location2CodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 19, true);
			this.Location2CodeFindBox.TabIndex = 10;
			// 
			// JobTradeLaneForm
			// 
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(793, 318, true);
			this.DataSourceType = typeof(Enterprise.Freight.Business.JobTradeLane);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(807, 376, true);
			this.Name = "JobTradeLaneForm";
			this.ShouldSerializeTabPageMethods = false;
			this.MainTabControl.ResumeLayout(false);
			this.MainTabControl.PerformLayout();
			this.MainTabPage.ResumeLayout(false);
			this.MainTabPage.PerformLayout();
			this.MainPanel.ResumeLayout(false);
			this.MainPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.DirectionDropEdit.ResumeLayout(true);
			this.DirectionDropEdit.PerformLayout();
			this.CarrierGuidFindBox.ResumeLayout(true);
			this.CarrierGuidFindBox.PerformLayout();
			this.Location1CodeFindBox.ResumeLayout(true);
			this.Location1CodeFindBox.PerformLayout();
			this.Location2CodeFindBox.ResumeLayout(true);
			this.Location2CodeFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		private Enterprise.ZArchitecture.GUI.ZDropEdit DirectionDropEdit;
		private Enterprise.ZArchitecture.GUI.ZGuidFindBox CarrierGuidFindBox;
		private Enterprise.ZArchitecture.ZTextBox DescriptionTextBox;
		private Enterprise.ZArchitecture.ZTextBox CodeTextBox;
		private Enterprise.ZArchitecture.GUI.ZCodeFindBox Location1CodeFindBox;
		private Enterprise.ZArchitecture.GUI.ZCodeFindBox Location2CodeFindBox;
	}
}
