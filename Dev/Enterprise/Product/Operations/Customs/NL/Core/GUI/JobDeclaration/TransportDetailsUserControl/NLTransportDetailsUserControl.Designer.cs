using Enterprise.Customs.GUI;

namespace Enterprise.Customs.NL.GUI
{
	partial class NLTransportDetailsUserControl
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
			this.TransportInlandModeAndTypeOfIdMailAndFixedInstallationUserControl = new TransportInlandModeAndTypeOfIdMailAndFixedInstallationUserControl();
			this.ImportTransportInlandRoadUserControl = new ImportTransportInlandRoadUserControl();
			this.FlightAndNationalityUserControl = new FlightAndNationalityUserControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.TransportInlandModeAndTypeOfIdMailAndFixedInstallationUserControl.SuspendLayout();
			this.ImportTransportInlandRoadUserControl.SuspendLayout();
			this.FlightAndNationalityUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.Business.BaseJobDeclaration);
			// 
			// TransportInlandModeAndTypeOfIdMailAndFixedInstallationUserControl
			// 
			this.TransportInlandModeAndTypeOfIdMailAndFixedInstallationUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TransportInlandModeAndTypeOfIdMailAndFixedInstallationUserControl, ".");
			this.TransportInlandModeAndTypeOfIdMailAndFixedInstallationUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(175, 523, true);
			this.TransportInlandModeAndTypeOfIdMailAndFixedInstallationUserControl.Name = "TransportInlandModeAndTypeOfIdMailAndFixedInstallationUserControl";
			this.TransportInlandModeAndTypeOfIdMailAndFixedInstallationUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(338, 22, true);
			this.TransportInlandModeAndTypeOfIdMailAndFixedInstallationUserControl.TabIndex = 23;
			// 
			// ImportTransportInlandRoadUserControl
			// 
			this.ImportTransportInlandRoadUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ImportTransportInlandRoadUserControl, ".");
			this.ImportTransportInlandRoadUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(175, 523, true);
			this.ImportTransportInlandRoadUserControl.Name = "ImportTransportInlandRoadUserControl";
			this.ImportTransportInlandRoadUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(338, 43, true);
			this.ImportTransportInlandRoadUserControl.TabIndex = 25;
			//
			// FlightAndNationalityUserControl
			// 
			this.FlightAndNationalityUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.FlightAndNationalityUserControl, ".");
			this.FlightAndNationalityUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(190, 523, true);
			this.FlightAndNationalityUserControl.Name = "FlightAndNationalityUserControl";
			this.FlightAndNationalityUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(338, 43, true);
			this.FlightAndNationalityUserControl.TabIndex = 25;
			//
			// TransportDetailsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ImportTransportInlandRoadUserControl);
			this.Controls.Add(this.TransportInlandModeAndTypeOfIdMailAndFixedInstallationUserControl);
			this.Controls.Add(this.FlightAndNationalityUserControl);
			this.Name = "TransportDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(654, 906, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.TransportInlandModeAndTypeOfIdMailAndFixedInstallationUserControl.ResumeLayout(true);
			this.TransportInlandModeAndTypeOfIdMailAndFixedInstallationUserControl.PerformLayout();
			this.ImportTransportInlandRoadUserControl.ResumeLayout(true);
			this.ImportTransportInlandRoadUserControl.PerformLayout();
			this.FlightAndNationalityUserControl.ResumeLayout(true);
			this.FlightAndNationalityUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal TransportInlandModeAndTypeOfIdMailAndFixedInstallationUserControl TransportInlandModeAndTypeOfIdMailAndFixedInstallationUserControl;
		internal ImportTransportInlandRoadUserControl ImportTransportInlandRoadUserControl;
		internal FlightAndNationalityUserControl FlightAndNationalityUserControl;
	}
}
