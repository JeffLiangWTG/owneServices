namespace Enterprise.MasterFiles.GUI
{
	partial class AccChargeApportionmentMethodOverrideControl
	{
		void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo5 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo6 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.apportionmentMethodOverrideGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.apportionmentMethodOverrideGrid)).BeginInit();
			this.apportionmentMethodOverrideGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.AccChargeApportionmentMethodOverride);
			// 
			// apportionmentMethodOverrideGrid
			// 
			this.apportionmentMethodOverrideGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.apportionmentMethodOverrideGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccChargeApportionmentMethodOverride)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccChargeApportionmentMethodOverride)(null)).AAM_Module)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccChargeApportionmentMethodOverride)(null)).AAM_ConsolType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccChargeApportionmentMethodOverride)(null)).AAM_ContainerMode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccChargeApportionmentMethodOverride)(null)).AAM_Direction)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccChargeApportionmentMethodOverride)(null)).AAM_TransportMode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccChargeApportionmentMethodOverride)(null)).AAM_ApportionmentMethod)));
			this.apportionmentMethodOverrideGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.ColumnName = "AAM_Module";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo2.ColumnName = "AAM_ConsolType";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo3.ColumnName = "AAM_ContainerMode";
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo4.ColumnName = "AAM_Direction";
			zDropEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDropEditColumnStyleInfo5.ColumnName = "AAM_TransportMode";
			zDropEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDropEditColumnStyleInfo6.ColumnName = "AAM_ApportionmentMethod";
			zDropEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			this.apportionmentMethodOverrideGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.apportionmentMethodOverrideGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.apportionmentMethodOverrideGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.apportionmentMethodOverrideGrid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
			this.apportionmentMethodOverrideGrid.ColumnStyles.Add(zDropEditColumnStyleInfo5);
			this.apportionmentMethodOverrideGrid.ColumnStyles.Add(zDropEditColumnStyleInfo6);
			this.apportionmentMethodOverrideGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.apportionmentMethodOverrideGrid.GridId = "6AAB2EDB-B375-47AB-ADA8-D58AEAD9F3A4";
			this.apportionmentMethodOverrideGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.apportionmentMethodOverrideGrid.LayoutKey = "apportionmentMethodOverrideGrid";
			this.apportionmentMethodOverrideGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.apportionmentMethodOverrideGrid.Name = "apportionmentMethodOverrideGrid";
			this.apportionmentMethodOverrideGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(687, 283, true);
			this.apportionmentMethodOverrideGrid.TabIndex = 2;
			// 
			// AccChargeApportionmentMethodOverrideControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.apportionmentMethodOverrideGrid);
			this.Name = "AccChargeApportionmentMethodOverrideControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(687, 283, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.apportionmentMethodOverrideGrid)).EndInit();
			this.apportionmentMethodOverrideGrid.ResumeLayout(false);
			this.apportionmentMethodOverrideGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
