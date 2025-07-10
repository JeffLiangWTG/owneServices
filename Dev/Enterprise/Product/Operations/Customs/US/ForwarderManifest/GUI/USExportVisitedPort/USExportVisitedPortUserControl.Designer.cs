using Enterprise.Customs.US.ForwarderManifest.Business;

namespace Enterprise.Customs.US.ForwarderManifest.GUI
{
	partial class USExportVisitedPortUserControl
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
			components = new System.ComponentModel.Container();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZMultiControlColumnStyleInfo zMultiControlColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZMultiControlColumnStyleInfo();
			this.visitedPortsForManifestHeaderUserControlGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.visitedPortsForManifestHeaderUserControlGrid)).BeginInit();
			this.visitedPortsForManifestHeaderUserControlGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(USExportAsycudaBill);
			// 
			// visitedPortsForManifestHeaderUserControlGrid
			// 
			this.visitedPortsForManifestHeaderUserControlGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.visitedPortsForManifestHeaderUserControlGrid, "VisitedPorts");
			this.visitedPortsForManifestHeaderUserControlGrid.CaptionVisible = false;
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "CY_Order";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zCodeFindBoxColumnStyleInfo1.ColumnName = "CY_Data";
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zMultiControlColumnStyleInfo1.ColumnName = "CY_Code";
			zMultiControlColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zMultiControlColumnStyleInfo1.FieldTypeColumnName = "PortCodeFieldType";
			this.visitedPortsForManifestHeaderUserControlGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.visitedPortsForManifestHeaderUserControlGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.visitedPortsForManifestHeaderUserControlGrid.ColumnStyles.Add(zMultiControlColumnStyleInfo1);
			this.visitedPortsForManifestHeaderUserControlGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.visitedPortsForManifestHeaderUserControlGrid.GridId = "d7b35950-3d94-4d0b-b081-27acead8f7a7";
			this.visitedPortsForManifestHeaderUserControlGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.visitedPortsForManifestHeaderUserControlGrid.LayoutKey = "visitedPortsForManifestHeaderUserControlGrid";
			this.visitedPortsForManifestHeaderUserControlGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.visitedPortsForManifestHeaderUserControlGrid.Name = "visitedPortsForManifestHeaderUserControlGrid";
			this.visitedPortsForManifestHeaderUserControlGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(521, 265, true);
			this.visitedPortsForManifestHeaderUserControlGrid.TabIndex = 1;
			// 
			// VisitedPortsForBillUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.visitedPortsForManifestHeaderUserControlGrid);
			this.Name = "VisitedPortsForBillUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(521, 265, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.visitedPortsForManifestHeaderUserControlGrid)).EndInit();
			this.visitedPortsForManifestHeaderUserControlGrid.ResumeLayout(false);
			this.visitedPortsForManifestHeaderUserControlGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		private ZArchitecture.ZGrid visitedPortsForManifestHeaderUserControlGrid;
		#endregion
	}
}
