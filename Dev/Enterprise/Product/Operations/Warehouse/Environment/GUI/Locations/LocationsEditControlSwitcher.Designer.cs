using Enterprise.ZArchitecture.Core;

namespace Enterprise.Warehouse.Environment.GUI
{
	partial class LocationsEditControlSwitcher
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
			this.rowLocationsEditControl = new Enterprise.Warehouse.Environment.GUI.LocationsEditBaseControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.rowLocationsEditControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Warehouse.Environment.Business.WhsRow);
			// 
			// rowLocationsEditControl
			// 
			this.rowLocationsEditControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.rowLocationsEditControl, ".");
			this.rowLocationsEditControl.BindTo = "Locations";
			this.rowLocationsEditControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.rowLocationsEditControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.rowLocationsEditControl.Name = "rowLocationsEditControl";
			this.rowLocationsEditControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1004, 513, true);
			this.rowLocationsEditControl.TabIndex = 0;
			// 
			// LocationsEditControlSwitcher
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.rowLocationsEditControl);
			this.Name = "LocationsEditControlSwitcher";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1004, 513, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.rowLocationsEditControl.ResumeLayout(true);
			this.rowLocationsEditControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private LocationsEditBaseControl rowLocationsEditControl;
	}
}
