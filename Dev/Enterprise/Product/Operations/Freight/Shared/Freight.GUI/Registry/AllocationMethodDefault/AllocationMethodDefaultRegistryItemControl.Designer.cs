namespace Enterprise.Freight.GUI
{
	partial class AllocationMethodDefaultRegistryItemControl
	{
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZDropEdit defaultMethodDropEdit;
			CargoWise.Windows.UI.KPanel topPanel;
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.rulesGrid = new Enterprise.ZArchitecture.ZGrid();
			defaultMethodDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			topPanel = new CargoWise.Windows.UI.KPanel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			topPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.rulesGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Business.AllocationMethodDefaultHeader);
			// 
			// defaultMethodDropEdit
			// 
			this.BindingSource.SetBindingMember(defaultMethodDropEdit, "DefaultAllocationMethod");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Business.AllocationMethodDefaultHeader)(null)).DefaultAllocationMethod)));
			defaultMethodDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			defaultMethodDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 8, true);
			defaultMethodDropEdit.Name = "defaultMethodDropEdit";
			defaultMethodDropEdit.PreBoundMaxLength = 3;
			defaultMethodDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
			defaultMethodDropEdit.TabIndex = 1;
			// 
			// topPanel
			// 
			topPanel.Controls.Add(defaultMethodDropEdit);
			topPanel.Dock = System.Windows.Forms.DockStyle.Top;
			topPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			topPanel.Name = "topPanel";
			topPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(315, 37, true);
			topPanel.TabIndex = 4;
			// 
			// rulesGrid
			// 
			this.rulesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.rulesGrid, "Rules");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Freight.Business.AllocationMethodDefaultHeader)(null)).Rules)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.AllocationMethodDefaultRule)(((System.Collections.IList)(((Enterprise.Freight.Business.AllocationMethodDefaultHeader)(null)).Rules)).SyncRoot)).CountryCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.AllocationMethodDefaultRule)(((System.Collections.IList)(((Enterprise.Freight.Business.AllocationMethodDefaultHeader)(null)).Rules)).SyncRoot)).AllocationMethod)));
			this.rulesGrid.CaptionVisible = false;
			zCodeFindBoxColumnStyleInfo1.ColumnName = "CountryCode";
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zDropEditColumnStyleInfo1.ColumnName = "AllocationMethod";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			this.rulesGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.rulesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.rulesGrid.GridId = "432636a2-5fd5-4a21-81c6-6041e54af57c";
			this.rulesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.rulesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.rulesGrid.LayoutKey = "passwordTextBox";
			this.rulesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 40, true);
			this.rulesGrid.Name = "rulesGrid";
			this.rulesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(315, 120, true);
			this.rulesGrid.TabIndex = 3;
			// 
			// AllocationMethodDefaultRegistryItemControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.rulesGrid);
			this.Controls.Add(topPanel);
			this.Name = "AllocationMethodDefaultRegistryItemControl";
			this.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(321, 163, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			topPanel.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.rulesGrid)).EndInit();
			this.ResumeLayout(false);

		}

		private Enterprise.ZArchitecture.ZGrid rulesGrid;
	}
}
