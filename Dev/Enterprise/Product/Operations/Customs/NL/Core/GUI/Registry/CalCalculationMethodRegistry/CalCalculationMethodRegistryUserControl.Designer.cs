namespace Enterprise.Customs.NL.GUI
{
	partial class CalCalculationMethodRegistryUserControl
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			this.CalCalculationMethodGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.CalCalculationMethodGrid)).BeginInit();
			this.CalCalculationMethodGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.NL.Business.CalCalculationMethodRegistry);
			// 
			// CalCalculationMethodGrid
			// 
			this.CalCalculationMethodGrid.AllowNavigation = false;
			this.CalCalculationMethodGrid.AllowReadOnlyToModifyTabStop = true;
			this.CalCalculationMethodGrid.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.CalCalculationMethodGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.NL.Business.CalCalculationMethodRegistry)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NL.Business.CalCalculationMethodRegistry)(null)).CalculationMethodName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.NL.Business.CalCalculationMethodRegistry)(null)).CalculationMethodValue)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.NL.Business.CalCalculationMethodRegistry)(null)).CalculationMethodDefault)));
			this.CalCalculationMethodGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.ColumnName = "CalculationMethodName";
			zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(75);
			zTextBoxColumnStyleInfo2.ColumnName = "CalculationMethodDescription";
			zTextBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "CalculationMethodValue";
			zCalcEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo1.ShowEmptyStringForEmptyValue = true;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(75);
			zCheckBoxColumnStyleInfo1.ColumnName = "CalculationMethodDefault";
			zCheckBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			this.CalCalculationMethodGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.CalCalculationMethodGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.CalCalculationMethodGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.CalCalculationMethodGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.CalCalculationMethodGrid.GridId = "9c37fbb5-8d12-4af0-8e99-ebf994af2c45";
			this.CalCalculationMethodGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.CalCalculationMethodGrid.LayoutKey = "CalCalculationMethodGrid";
			this.CalCalculationMethodGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CalCalculationMethodGrid.Name = "CalCalculationMethodGrid";
			this.CalCalculationMethodGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 100, true);
			this.CalCalculationMethodGrid.TabIndex = 0;
			// 
			// CalCalculationMethodRegistryUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.CalCalculationMethodGrid);
			this.Name = "CalCalculationMethodRegistryUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 100, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.CalCalculationMethodGrid)).EndInit();
			this.CalCalculationMethodGrid.ResumeLayout(false);
			this.CalCalculationMethodGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZGrid CalCalculationMethodGrid;
	}
}
