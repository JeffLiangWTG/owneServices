namespace Enterprise.MasterFiles.GUI
{
	public abstract partial class ChargeGroupSettingControl
	{
		void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.ChargeGroupGrid = new Enterprise.ZArchitecture.ZGrid();
			this.ChargeGroupSetupGrid = new Enterprise.ZArchitecture.ZGrid();
			this.splitContainer1 = new CargoWise.Windows.UI.KSplitContainer();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ChargeGroupGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ChargeGroupSetupGrid)).BeginInit();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			this.SuspendLayout();
			// 
			// ChargeGroupGrid
			// 
			this.ChargeGroupGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ChargeGroupGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.RevenueRecognitionByChargeGroup)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RevenueRecognitionByChargeGroup)(null)).ChargeGroup)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RevenueRecognitionByChargeGroup)(null)).ChargeGroupDescription)));
			this.ChargeGroupGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ChargeGroupSettingControl|f98685c0-fa7f-431d-8c2e-3bb080200419", "Charge Group");
			zTextBoxColumnStyleInfo1.ColumnName = "ChargeGroup";
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ChargeGroupSettingControl|c61efd9f-bed9-4255-a309-73cc578d1ee1", "Charge Group Description");
			zTextBoxColumnStyleInfo2.ColumnName = "ChargeGroupDescription";
			this.ChargeGroupGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ChargeGroupGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.ChargeGroupGrid.GridId = "A9FD316B-1A41-48F5-BBF8-A4B530FC8010";
			this.ChargeGroupGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ChargeGroupGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ChargeGroupGrid.LayoutKey = "ChargeGroupGrid";
			this.ChargeGroupGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ChargeGroupGrid.Name = "ChargeGroupGrid";
			this.ChargeGroupGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(634, 161, true);
			this.ChargeGroupGrid.TabIndex = 1;
			// 
			// RevenueRecognitionGrid
			// 
			this.ChargeGroupSetupGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ChargeGroupSetupGrid, "ChargeGroupSettings");
			this.ChargeGroupSetupGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ChargeGroupSettingControl|1d183ab6-1a99-4a38-b1c0-83e77df01e34", "Job Type");
			zDropEditColumnStyleInfo1.ColumnName = "JobType";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(55);
			zDropEditColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ChargeGroupSettingControl|f143f9da-7bc3-4810-86af-909b64a1fead", "Direction");
			zDropEditColumnStyleInfo2.ColumnName = "DirectionCode";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(55);
			zDropEditColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo3.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ChargeGroupSettingControl|c78bdde0-0ff1-455e-891b-f0d678228947", "Transport Mode");
			zDropEditColumnStyleInfo3.ColumnName = "Mode";
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			this.ChargeGroupSetupGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.ChargeGroupSetupGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.ChargeGroupSetupGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.ChargeGroupSetupGrid.GridId = "728c6f2f-8c00-4d39-9b5c-2c9e6e7769a4";
			this.ChargeGroupSetupGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ChargeGroupSetupGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ChargeGroupSetupGrid.LayoutKey = "ChargeGroupSetupGrid";
			this.ChargeGroupSetupGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ChargeGroupSetupGrid.Name = "ChargeGroupSetupGrid";
			this.ChargeGroupSetupGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(634, 157, true);
			this.ChargeGroupSetupGrid.TabIndex = 0;
			// 
			// splitContainer1
			// 
			this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.splitContainer1.Name = "splitContainer1";
			this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitContainer1.Panel1
			// 
			this.splitContainer1.Panel1.Controls.Add(this.ChargeGroupGrid);
			// 
			// splitContainer1.Panel2
			// 
			this.splitContainer1.Panel2.Controls.Add(this.ChargeGroupSetupGrid);
			this.splitContainer1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(634, 322, true);
			this.splitContainer1.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(161);
			this.splitContainer1.TabIndex = 2;
			// 
			// RevenueRecognitionByChargeGroupControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.splitContainer1);
			this.Name = "ChargeGroupSettingControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(634, 322, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ChargeGroupGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ChargeGroupSetupGrid)).EndInit();
			this.splitContainer1.Panel1.ResumeLayout(false);
			this.splitContainer1.Panel2.ResumeLayout(false);
			this.splitContainer1.ResumeLayout(false);
			this.ResumeLayout(false);
		}

		protected internal Enterprise.ZArchitecture.ZGrid ChargeGroupSetupGrid;
		CargoWise.Windows.UI.KSplitContainer splitContainer1;
		Enterprise.ZArchitecture.ZGrid ChargeGroupGrid;
	}
}
