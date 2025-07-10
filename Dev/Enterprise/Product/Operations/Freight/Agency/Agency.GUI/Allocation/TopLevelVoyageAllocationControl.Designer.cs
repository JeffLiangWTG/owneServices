namespace Enterprise.Freight.Agency.GUI
{
	partial class TopLevelVoyageAllocationControl
	{
		void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			countriesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			countryGrid = new Enterprise.ZArchitecture.ZGrid();
			allocationPanel = new Enterprise.Freight.Agency.GUI.OuterVoyageAllocationControl();
			countryAllocationSplitPanel = new CargoWise.Windows.UI.KSplitContainer();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			countriesGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(countryGrid)).BeginInit();
			countryAllocationSplitPanel.Panel1.SuspendLayout();
			countryAllocationSplitPanel.Panel2.SuspendLayout();
			countryAllocationSplitPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Agency.Business.AgencyCountryCollection);
			// 
			// countriesGroupBox
			// 
			countriesGroupBox.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("TopLevelVoyageAllocationControl|0c38e944-b073-47dd-8e3c-12042542a148", "Countries/Regions");
			countriesGroupBox.Controls.Add(countryGrid);
			countriesGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			countriesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			countriesGroupBox.Name = "countriesGroupBox";
			countriesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(597, 95, true);
			countriesGroupBox.TabIndex = 2;
			countriesGroupBox.TabStop = false;
			// 
			// countryGrid
			// 
			countryGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(countryGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.AgencyCountry)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.AgencyCountry)(null)).CountryCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.AgencyCountry)(null)).CountryName)));
			countryGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.ColumnName = "CountryCode";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo2.ColumnName = "CountryName";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			countryGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			countryGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			countryGrid.GridId = "40db7116-ace4-4fd9-a396-68e7dc3ff1ef";
			countryGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			countryGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			countryGrid.LayoutKey = "countryGrid";
			countryGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			countryGrid.Name = "countryGrid";
			countryGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(591, 76, true);
			countryGrid.TabIndex = 1;
			// 
			// allocationPanel
			// 
			this.BindingSource.SetBindingMember(allocationPanel, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Freight.Agency.Business.AgencyCountry)(((Enterprise.Freight.Agency.Business.AgencyCountry)(null)))));
			allocationPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			allocationPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			allocationPanel.Name = "allocationPanel";
			allocationPanel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, 0, 3, 3, true);
			allocationPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(597, 298, true);
			allocationPanel.TabIndex = 0;
			// 
			// countryAllocationSplitPanel
			// 
			countryAllocationSplitPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			countryAllocationSplitPanel.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
			countryAllocationSplitPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			countryAllocationSplitPanel.Name = "countryAllocationSplitPanel";
			countryAllocationSplitPanel.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// countryAllocationSplitPanel.Panel1
			// 
			countryAllocationSplitPanel.Panel1.Controls.Add(countriesGroupBox);
			// 
			// countryAllocationSplitPanel.Panel2
			// 
			countryAllocationSplitPanel.Panel2.Controls.Add(allocationPanel);
			countryAllocationSplitPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(597, 397, true);
			countryAllocationSplitPanel.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(95);
			countryAllocationSplitPanel.TabIndex = 2;
			// 
			// TopLevelVoyageAllocationControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(countryAllocationSplitPanel);
			this.Name = "TopLevelVoyageAllocationControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(597, 397, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			countriesGroupBox.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(countryGrid)).EndInit();
			countryAllocationSplitPanel.Panel1.ResumeLayout(false);
			countryAllocationSplitPanel.Panel2.ResumeLayout(false);
			countryAllocationSplitPanel.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		Enterprise.ZArchitecture.GUI.ZGroupBox countriesGroupBox;
		Enterprise.ZArchitecture.ZGrid countryGrid;
		Enterprise.Freight.Agency.GUI.OuterVoyageAllocationControl allocationPanel;
		CargoWise.Windows.UI.KSplitContainer countryAllocationSplitPanel;
	}
}
