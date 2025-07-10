namespace Enterprise.MasterFiles.GUI
{
	public partial class ServicesControl
	{
		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			this.ServicesUserControl = new Enterprise.MasterFiles.GUI.DocServicesUserControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// ServicesUserControl
			// 
			this.BindingSource.SetBindingMember(this.ServicesUserControl, ".");
			this.ServicesUserControl.BindToServices = "Services";
			this.ServicesUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ServicesUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ServicesUserControl.Name = "ServicesUserControl";
			this.ServicesUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(356, 233, true);
			this.ServicesUserControl.TabIndex = 0;
			// 
			// ServicesControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ServicesUserControl);
			this.Name = "ServicesControl";
			this.ShouldSerializeTabPageMethods = false;
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(356, 233, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
		}

		#endregion

		DocServicesUserControl ServicesUserControl;
	}
}
