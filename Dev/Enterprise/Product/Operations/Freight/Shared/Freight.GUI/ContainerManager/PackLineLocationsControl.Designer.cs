namespace Enterprise.Freight.GUI
{
	partial class PackLineLocationsControl
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
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo zGuidDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.locationsGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.locationsGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Business.PackLocation);
			// 
			// locationsGrid
			// 
			this.locationsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.locationsGrid, "PackLocations");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Freight.Business.PackLocation)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Business.PackLocation)(null)).JQ_NoPackages)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.Business.PackLocation)(null)).LocationWhsGuid)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.PackLocation)(null)).LocationString)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.PackLocation)(null)).JQ_WarehouseLocation)));
			this.locationsGrid.CaptionVisible = false;
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.Caption = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("ContainersUserControl|f743ccb0-26f2-4ffe-895a-7319ebaaaba3", "Packs");
			zCalcEditColumnStyleInfo1.ColumnName = "JQ_NoPackages";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zCalcEditColumnStyleInfo1.IsMandatory = true;
			zGuidDropEditColumnStyleInfo1.Caption = null;
			zGuidDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("ContainersUserControl|b5d8bfd5-fd19-4556-b241-9b8210490c9f", "Whs.", "Warehouse", "");
			zGuidDropEditColumnStyleInfo1.ColumnName = "LocationWhsGuid";
			zGuidDropEditColumnStyleInfo1.IsMandatory = true;
			zGuidDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo1.Caption = null;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("ContainersUserControl|f6c1d85a-6aae-48ae-b738-b6cdeb16ae73", "Location", "Warehouse Location", "");
			zTextBoxColumnStyleInfo1.ColumnName = "LocationString";
			zTextBoxColumnStyleInfo1.IsMandatory = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo2.Caption = null;
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("ContainersUserControl|9fb41b0f-5342-4d56-ad04-7907c1c59d2c", "Location", "Warehouse Location", "");
			zTextBoxColumnStyleInfo2.ColumnName = "JQ_WarehouseLocation";
			zTextBoxColumnStyleInfo2.IsMandatory = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.locationsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.locationsGrid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo1);
			this.locationsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.locationsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.locationsGrid.CopySelectedRowsAllowed = true;
			this.locationsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.locationsGrid.GridId = "d9882ea6-4df6-4406-b3c9-749ff0be2ce3";
			this.locationsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.locationsGrid.LayoutKey = "locationsGrid";
			this.locationsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.locationsGrid.Name = "locationsGrid";
			this.locationsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(443, 128, true);
			this.locationsGrid.TabIndex = 0;
			// 
			// PackLineLocationsControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.locationsGrid);
			this.Name = "PackLineLocationsControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(443, 128, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.locationsGrid)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private ZArchitecture.ZGrid locationsGrid;
	}
}
