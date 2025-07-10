namespace Enterprise.Freight.Forwarding.GUI
{
	partial class CargoIMPPhase2RouteMapRegistryControl
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
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			this.CargoIMPPhase2RouteMapGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.CargoIMPPhase2RouteMapGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// CargoIMPPhase2RouteMapGrid
			// 
			this.CargoIMPPhase2RouteMapGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.CargoIMPPhase2RouteMapGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			this.CargoIMPPhase2RouteMapGrid.CaptionVisible = false;
			zCodeFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("CargoIMPPhase2RouteMapRegistryControl|5c3a1751-dc01-4933-8318-5ec7200c4eaa", "Origin");
			zCodeFindBoxColumnStyleInfo1.ColumnName = "Origin";
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCodeFindBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("CargoIMPPhase2RouteMapRegistryControl|94466251-4ca4-4000-a44e-026bd76fc07e", "Destination");
			zCodeFindBoxColumnStyleInfo2.ColumnName = "Destination";
			zCodeFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCodeFindBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("CargoIMPPhase2RouteMapRegistryControl|fbc57cc7-ddb6-4044-a1d6-f3ce329ce479", "Airline 2-ch Code");
			zCodeFindBoxColumnStyleInfo3.ColumnName = "AirlineTwoCharacterCode";
			zCodeFindBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.CargoIMPPhase2RouteMapGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.CargoIMPPhase2RouteMapGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo2);
			this.CargoIMPPhase2RouteMapGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo3);
			this.CargoIMPPhase2RouteMapGrid.GridId = "a80b7b48-7d7f-4956-9f45-5be9fcaf20af";
			this.CargoIMPPhase2RouteMapGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CargoIMPPhase2RouteMapGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.CargoIMPPhase2RouteMapGrid.LayoutKey = "CargoIMPPhase2RouteMapGrid";
			this.CargoIMPPhase2RouteMapGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CargoIMPPhase2RouteMapGrid.Name = "CargoIMPPhase2RouteMapGrid";
			this.CargoIMPPhase2RouteMapGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(553, 396, true);
			this.CargoIMPPhase2RouteMapGrid.TabIndex = 0;
			// 
			// CargoIMPPhase2RouteMapRegistryControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.CargoIMPPhase2RouteMapGrid);
			this.Name = "CargoIMPPhase2RouteMapRegistryControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(553, 396, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.CargoIMPPhase2RouteMapGrid)).EndInit();
			this.ResumeLayout(false);
		}

		#endregion

		internal Enterprise.ZArchitecture.ZGrid CargoIMPPhase2RouteMapGrid;
	}
}
