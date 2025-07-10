using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Rating.GUI
{
	partial class SellRatesDecimalsControl
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
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.NumberOfDecimalsGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.NumberOfDecimalsGrid)).BeginInit();
			this.NumberOfDecimalsGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Rating.Business.SellRatesDecimalsCollection);
			// 
			// NumberOfDecimalsGrid
			// 
			this.NumberOfDecimalsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.NumberOfDecimalsGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Rating.Business.SellRatesDecimals)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Rating.Business.SellRatesDecimals)(null)).Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Rating.Business.SellRatesDecimals)(null)).RateCategoriesAndGroupsList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Rating.Business.SellRatesDecimals)(null)).Description)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Rating.Business.SellRatesDecimals)(null)).Decimals)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Rating.Business.SellRatesDecimals)(null)).DecimalsList)));
			this.NumberOfDecimalsGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.ColumnName = "Code";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(45);
			zTextBoxColumnStyleInfo1.ColumnName = "Description";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(250);
			zDropEditColumnStyleInfo2.ColumnName = "Decimals";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.NumberOfDecimalsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.NumberOfDecimalsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.NumberOfDecimalsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.NumberOfDecimalsGrid.CopySelectedRowsAllowed = true;
			this.NumberOfDecimalsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.NumberOfDecimalsGrid.GridId = "84fdf4ec-f065-4c8b-ad5a-00d5d786e13b";
			this.NumberOfDecimalsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.NumberOfDecimalsGrid.LayoutKey = "NumberOfDecimalsGrid";
			this.NumberOfDecimalsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.NumberOfDecimalsGrid.Name = "NumberOfDecimalsGrid";
			this.NumberOfDecimalsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(553, 396, true);
			this.NumberOfDecimalsGrid.TabIndex = 0;
			// 
			// NumberOfDecimalsControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.NumberOfDecimalsGrid);
			this.Name = "NumberOfDecimalsControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(553, 396, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.NumberOfDecimalsGrid)).EndInit();
			this.NumberOfDecimalsGrid.ResumeLayout(false);
			this.NumberOfDecimalsGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.ZGrid NumberOfDecimalsGrid;

	}
}
