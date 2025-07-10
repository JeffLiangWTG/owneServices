namespace Enterprise.Customs.TR.Manifest.GUI
{
	partial class VisitedPortsForBillUserControl
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
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			this.visitedPortsForManifestHeaderUserControlGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.visitedPortsForManifestHeaderUserControlGrid)).BeginInit();
			this.visitedPortsForManifestHeaderUserControlGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.TR.Manifest.Business.AsycudaBill);
			// 
			// visitedPortsForManifestHeaderUserControlGrid
			// 
			this.visitedPortsForManifestHeaderUserControlGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.visitedPortsForManifestHeaderUserControlGrid, "VisitedPorts");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.TR.Manifest.Business.AsycudaBill)(null)).VisitedPorts)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.TR.Manifest.Business.VisitedPort)(((System.Collections.IList)(((Enterprise.Customs.TR.Manifest.Business.AsycudaBill)(null)).VisitedPorts)).SyncRoot)).CY_Order)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.Manifest.Business.VisitedPort)(((System.Collections.IList)(((Enterprise.Customs.TR.Manifest.Business.AsycudaBill)(null)).VisitedPorts)).SyncRoot)).CY_Data)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.Manifest.Business.VisitedPort)(((System.Collections.IList)(((Enterprise.Customs.TR.Manifest.Business.AsycudaBill)(null)).VisitedPorts)).SyncRoot)).CY_Code)));
			this.visitedPortsForManifestHeaderUserControlGrid.CaptionVisible = false;
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "CY_Order";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zCodeFindBoxColumnStyleInfo1.ColumnName = "CY_Data";
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zCodeFindBoxColumnStyleInfo2.ColumnName = "CY_Code";
			zCodeFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			this.visitedPortsForManifestHeaderUserControlGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.visitedPortsForManifestHeaderUserControlGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.visitedPortsForManifestHeaderUserControlGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo2);
			this.visitedPortsForManifestHeaderUserControlGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.visitedPortsForManifestHeaderUserControlGrid.GridId = "d7b35950-3d94-4d0b-b081-27acead8f7a7";
			this.visitedPortsForManifestHeaderUserControlGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.visitedPortsForManifestHeaderUserControlGrid.LayoutKey = "carrierRoutingGrid";
			this.visitedPortsForManifestHeaderUserControlGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.visitedPortsForManifestHeaderUserControlGrid.Name = "visitedPortsForManifestHeaderUserControlGrid";
			this.visitedPortsForManifestHeaderUserControlGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(521, 265, true);
			this.visitedPortsForManifestHeaderUserControlGrid.TabIndex = 1;
			// 
			// VisitedPortsForBillUserControl
			//
			this.CaptionRenderingEnabled = true;
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

		#endregion

		private ZArchitecture.ZGrid visitedPortsForManifestHeaderUserControlGrid;
	}
}
