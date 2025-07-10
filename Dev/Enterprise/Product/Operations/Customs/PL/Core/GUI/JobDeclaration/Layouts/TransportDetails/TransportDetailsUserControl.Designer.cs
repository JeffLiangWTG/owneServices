namespace Enterprise.Customs.PL.GUI
{
	partial class TransportDetailsUserControl
	{
		private System.ComponentModel.IContainer components = null;

		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		private void InitializeComponent()
		{
			this.TransportInlandRailUserControl = new Enterprise.Customs.PL.GUI.TransportInlandRailUserControl();
			this.VesselUserControl = new Enterprise.Customs.PL.GUI.VesselUserControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.TransportInlandRailUserControl.SuspendLayout();
			this.VesselUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.PL.Business.Declaration.JobDeclaration);
			// 
			// TransportInlandRailUserControl
			// 
			this.TransportInlandRailUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TransportInlandRailUserControl, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.Business.BaseJobDeclaration)(((Enterprise.Customs.PL.Business.Declaration.JobDeclaration)(null)))));
			this.TransportInlandRailUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 36, true);
			this.TransportInlandRailUserControl.Name = "TransportInlandRailUserControl";
			this.TransportInlandRailUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(347, 21, true);
			this.TransportInlandRailUserControl.TabIndex = 3;
			// 
			// VesselUserControl
			// 
			this.VesselUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.VesselUserControl, ".");
			this.VesselUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 8, true);
			this.VesselUserControl.Name = "VesselUserControl";
			this.VesselUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(347, 21, true);
			this.VesselUserControl.TabIndex = 4;
			// 
			// TransportDetailsUserControl
			// 
			this.Controls.Add(this.TransportInlandRailUserControl);
			this.Controls.Add(this.VesselUserControl);
			this.Name = "TransportDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(463, 65, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.TransportInlandRailUserControl.ResumeLayout(true);
			this.TransportInlandRailUserControl.PerformLayout();
			this.VesselUserControl.ResumeLayout(true);
			this.VesselUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		internal TransportInlandRailUserControl TransportInlandRailUserControl;
		internal VesselUserControl VesselUserControl;
	}
}
