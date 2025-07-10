namespace Enterprise.Rating.GUI
{
	partial class HighestRateControl
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
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo5 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo6 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.RateLineItemsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.UnitAsFreightedDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEditWithFixedWidth();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.RateLineItemsGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = DataSourceTypeForBinding;
			// 
			// RateLineItemsGrid
			// 
			this.RateLineItemsGrid.AllowNavigation = false;
			this.RateLineItemsGrid.AllowSorting = false;
			this.RateLineItemsGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.RateLineItemsGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Rating.Business.RateLineItem)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Rating.Business.RateLineItem)(null)).TM_Type)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Rating.Business.RateLineItem)(null)).TM_BreakWeightVolume)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Rating.Business.RateLineItem)(null)).TM_RelevantValue)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Rating.Business.RateLineItem)(null)).TM_FlatAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Rating.Business.RateLineItem)(null)).UnitMultipleString)));
			this.RateLineItemsGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo4.ColumnName = "TM_Type";
			zDropEditColumnStyleInfo5.ColumnName = "TM_BreakWeightVolume";
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("HighestRateControl|8c19a10b-949c-42cf-bbe8-a754c8af49eb", "Rate");
			zCalcEditColumnStyleInfo3.ColumnName = "TM_RelevantValue";
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.ColumnName = "TM_FlatAmount";
			zCalcEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDropEditColumnStyleInfo6.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("59b6e7e0-45de-4931-8b11-6f9b782016f6", "Multiple");
			zDropEditColumnStyleInfo6.ColumnName = "UnitMultipleString";
			this.RateLineItemsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
			this.RateLineItemsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo5);
			this.RateLineItemsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.RateLineItemsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.RateLineItemsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo6);
			this.RateLineItemsGrid.CopySelectedRowsAllowed = true;
			this.RateLineItemsGrid.GridId = "3e51839e-90b4-4961-ac16-e8f3735b9400";
			this.RateLineItemsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.RateLineItemsGrid.LayoutKey = "RateLineItemsGridHighestRate";
			this.RateLineItemsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.RateLineItemsGrid.Name = "RateLineItemsGrid";
			this.RateLineItemsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(448, 115, true);
			this.RateLineItemsGrid.TabIndex = 0;
			// 
			// UnitAsFreightedDropEdit
			// 
			this.UnitAsFreightedDropEdit.AllowDrop = true;
			this.UnitAsFreightedDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.UnitAsFreightedDropEdit.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("ee7bee55-b5cf-490a-89b9-e0324a6fefec", "Unit As Freighted");
			this.UnitAsFreightedDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 115, true);
			this.UnitAsFreightedDropEdit.Name = "UnitAsFreightedDropEdit";
			this.UnitAsFreightedDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
			this.UnitAsFreightedDropEdit.PreBoundMaxLength = 3;
			this.UnitAsFreightedDropEdit.TabIndex = 2;
			// 
			// HighestRateControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.UnitAsFreightedDropEdit);
			this.Controls.Add(this.RateLineItemsGrid);
			this.Name = "HighestRateControl";
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.RateLineItemsGrid)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private Enterprise.ZArchitecture.ZGrid RateLineItemsGrid;
		private Enterprise.ZArchitecture.GUI.ZDropEdit UnitAsFreightedDropEdit;
	}
}
