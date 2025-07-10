using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.NO.GUI
{
	partial class TransportDetailsLayoutsUserControl
	{
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.TransportDetailsFlightUserControl = new Enterprise.Customs.NO.GUI.TransportDetailsFlightUserControl();
			this.TransportDetailsNationalityUserControl = new Enterprise.Customs.NO.GUI.TransportDetailsNationalityUserControl();
			this.TransportDetailsVoyageUserControl = new Enterprise.Customs.NO.GUI.TransportDetailsVoyageUserControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.TransportDetailsFlightUserControl.SuspendLayout();
			this.TransportDetailsNationalityUserControl.SuspendLayout();
			this.TransportDetailsVoyageUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.NO.Business.JobDeclaration);
			// 
			// TransportDetailsFlightUserControl
			// 
			this.TransportDetailsFlightUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TransportDetailsFlightUserControl, ".");
			this.TransportDetailsFlightUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(106, 30, true);
			this.TransportDetailsFlightUserControl.Name = "TransportDetailsFlightUserControl";
			this.TransportDetailsFlightUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(338, 23, true);
			this.TransportDetailsFlightUserControl.TabIndex = 2;
			// 
			// TransportDetailsNationalityUserControl
			// 
			this.TransportDetailsNationalityUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TransportDetailsNationalityUserControl, ".");
			this.TransportDetailsNationalityUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(106, 59, true);
			this.TransportDetailsNationalityUserControl.Name = "TransportDetailsNationalityUserControl";
			this.TransportDetailsNationalityUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(338, 23, true);
			this.TransportDetailsNationalityUserControl.TabIndex = 3;
			// 
			// TransportDetailsVoyageUserControl
			// 
			this.TransportDetailsVoyageUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TransportDetailsVoyageUserControl, ".");
			this.TransportDetailsVoyageUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(106, 88, true);
			this.TransportDetailsVoyageUserControl.Name = "TransportDetailsVoyageUserControl";
			this.TransportDetailsVoyageUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(338, 23, true);
			this.TransportDetailsVoyageUserControl.TabIndex = 3;
			// 
			// TransportDetailsLayoutsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.TransportDetailsFlightUserControl);
			this.Controls.Add(this.TransportDetailsNationalityUserControl);
			this.Controls.Add(this.TransportDetailsVoyageUserControl);
			this.Name = "TransportDetailsLayoutsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(503, 152, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.TransportDetailsFlightUserControl.ResumeLayout(true);
			this.TransportDetailsFlightUserControl.PerformLayout();
			this.TransportDetailsNationalityUserControl.ResumeLayout(true);
			this.TransportDetailsNationalityUserControl.PerformLayout();
			this.TransportDetailsVoyageUserControl.ResumeLayout(true);
			this.TransportDetailsVoyageUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		internal TransportDetailsFlightUserControl TransportDetailsFlightUserControl;
		internal TransportDetailsNationalityUserControl TransportDetailsNationalityUserControl;
		internal TransportDetailsVoyageUserControl TransportDetailsVoyageUserControl;
	}
}
