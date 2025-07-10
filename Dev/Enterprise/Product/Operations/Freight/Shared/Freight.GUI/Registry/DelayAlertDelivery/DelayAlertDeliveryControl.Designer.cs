namespace Enterprise.Freight.GUI
{
	partial class DelayAlertDeliveryControl
	{
		void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZGrid ruleGrid;
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			ruleGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(ruleGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Business.DelayAlertDeliveryRuleCollection);
			// 
			// ruleGrid
			// 
			ruleGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(ruleGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Freight.Business.DelayAlertDeliveryRule)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.DelayAlertDeliveryRule)(null)).TransportMode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.DelayAlertDeliveryRule)(null)).Module)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.DelayAlertDeliveryRule)(null)).Direction)));
			ruleGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.ColumnName = "TransportMode";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zDropEditColumnStyleInfo2.ColumnName = "Module";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zDropEditColumnStyleInfo3.ColumnName = "Direction";
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			ruleGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			ruleGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			ruleGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			ruleGrid.GridId = "be60a330-9e35-48f2-b15f-dee40cec3e61";
			ruleGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			ruleGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			ruleGrid.LayoutKey = "ruleGrid";
			ruleGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			ruleGrid.Name = "ruleGrid";
			ruleGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(336, 268, true);
			ruleGrid.TabIndex = 0;
			// 
			// DelayAlertDeliveryControl
			// 
			this.Controls.Add(ruleGrid);
			this.Name = "DelayAlertDeliveryControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(336, 268, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(ruleGrid)).EndInit();
			this.ResumeLayout(false);

		}
	}
}
