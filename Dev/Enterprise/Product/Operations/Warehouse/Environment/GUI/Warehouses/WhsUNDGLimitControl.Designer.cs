using Enterprise.ZArchitecture.Core;

namespace Enterprise.Warehouse.Environment.GUI
{
	partial class WhsUNDGLimitControl
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
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.UNDGLimitGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.UNDGLimitGrid)).BeginInit();
			this.UNDGLimitGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Warehouse.Environment.Business.WhsWarehouse);
			// 
			// UNDGLimitGrid
			// 
			this.UNDGLimitGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.UNDGLimitGrid, "UNDGLimits");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Warehouse.Environment.Business.WhsWarehouse)(null)).UNDGLimits)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Warehouse.Environment.Business.WhsUNDGLimit)(((System.Collections.IList)(((Enterprise.Warehouse.Environment.Business.WhsWarehouse)(null)).UNDGLimits)).SyncRoot)).WWD_DG)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Warehouse.Environment.Business.WhsUNDGLimit)(((System.Collections.IList)(((Enterprise.Warehouse.Environment.Business.WhsWarehouse)(null)).UNDGLimits)).SyncRoot)).WWD_DCR_UNDGCountryReference)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Warehouse.Environment.Business.WhsUNDGLimit)(((System.Collections.IList)(((Enterprise.Warehouse.Environment.Business.WhsWarehouse)(null)).UNDGLimits)).SyncRoot)).WWD_UNDGClass)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Warehouse.Environment.Business.WhsUNDGLimit)(((System.Collections.IList)(((Enterprise.Warehouse.Environment.Business.WhsWarehouse)(null)).UNDGLimits)).SyncRoot)).WWD_TotalVolumeLimit)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Warehouse.Environment.Business.WhsUNDGLimit)(((System.Collections.IList)(((Enterprise.Warehouse.Environment.Business.WhsWarehouse)(null)).UNDGLimits)).SyncRoot)).WWD_TotalVolumeLimitUQ)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Warehouse.Environment.Business.WhsUNDGLimit)(((System.Collections.IList)(((Enterprise.Warehouse.Environment.Business.WhsWarehouse)(null)).UNDGLimits)).SyncRoot)).WWD_TotalWeightLimit)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Warehouse.Environment.Business.WhsUNDGLimit)(((System.Collections.IList)(((Enterprise.Warehouse.Environment.Business.WhsWarehouse)(null)).UNDGLimits)).SyncRoot)).WWD_TotalWeightLimitUQ)));
			this.UNDGLimitGrid.CaptionVisible = false;
			zGuidFindBoxColumnStyleInfo1.ColumnName = "WWD_DG";
			zGuidFindBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(65);
			zGuidFindBoxColumnStyleInfo2.ColumnName = "WWD_DCR_UNDGCountryReference";
			zGuidFindBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
			zGuidFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zDropEditColumnStyleInfo1.ColumnName = "WWD_UNDGClass";
			zDropEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "WWD_TotalVolumeLimit";
			zCalcEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDropEditColumnStyleInfo2.ColumnName = "WWD_TotalVolumeLimitUQ";
			zDropEditColumnStyleInfo2.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "WWD_TotalWeightLimit";
			zCalcEditColumnStyleInfo2.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDropEditColumnStyleInfo3.ColumnName = "WWD_TotalWeightLimitUQ";
			zDropEditColumnStyleInfo3.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			this.UNDGLimitGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.UNDGLimitGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo2);
			this.UNDGLimitGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.UNDGLimitGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.UNDGLimitGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.UNDGLimitGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.UNDGLimitGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.UNDGLimitGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.UNDGLimitGrid.GridId = "e4259276-4b69-4224-b56c-3dfdce780bcd";
			this.UNDGLimitGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.UNDGLimitGrid.LayoutKey = "UNDGLimitGrid";
			this.UNDGLimitGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.UNDGLimitGrid.Name = "UNDGLimitGrid";
			this.UNDGLimitGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(333, 200, true);
			this.UNDGLimitGrid.TabIndex = 8;
			this.UNDGLimitGrid.SelectedRowsChangedInMouseDown += new System.EventHandler(this.UNDGLimitGrid_SelectedRowsChangedInMouseDown);
			// 
			// WhsUNDGLimitControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.UNDGLimitGrid);
			this.Name = "WhsUNDGLimitControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(333, 200, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.UNDGLimitGrid)).EndInit();
			this.UNDGLimitGrid.ResumeLayout(false);
			this.UNDGLimitGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		public ZArchitecture.ZGrid UNDGLimitGrid;
	}
}
