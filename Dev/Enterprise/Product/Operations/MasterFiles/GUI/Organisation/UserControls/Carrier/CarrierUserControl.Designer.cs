using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.GUI
{
	partial class CarrierUserControl
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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.TransportTabControl = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
			this.TransportTabPage1 = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.TransportPageControl = new Enterprise.MasterFiles.GUI.CarrierConfigurationUserControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.TransportTabControl.SuspendLayout();
			this.TransportTabPage1.SuspendLayout();
			this.SuspendLayout();
			// 
			// SecurityPanel
			// 
			this.SecurityPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(900, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.OrgHeader);
			// 
			// TransportTabControl
			// 
			this.TransportTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.TransportTabControl.Controls.Add(this.TransportTabPage1);
			this.TransportTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TransportTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 24, true);
			this.TransportTabControl.Name = "TransportTabControl";
			this.TransportTabControl.SelectedIndex = 0;
			this.TransportTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(900, 592, true);
			this.TransportTabControl.TabIndex = 0;
			// 
			// TransportTabPage1
			// 
			this.TransportTabPage1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("CarrierUserControl|dfdcb140-6e9f-4898-a93d-c695d37aae74", "Configuration");
			this.TransportTabPage1.Controls.Add(this.TransportPageControl);
			this.TransportTabPage1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.TransportTabPage1.Name = "TransportTabPage1";
			this.TransportTabPage1.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(5, true);
			this.TransportTabPage1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(892, 565, true);
			this.TransportTabPage1.TabIndex = 0;
			// 
			// TransportPageControl
			// 
			this.BindingSource.SetBindingMember(this.TransportPageControl, ".");
			this.TransportPageControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TransportPageControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 5, true);
			this.TransportPageControl.Name = "TransportPageControl";
			this.TransportPageControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(882, 555, true);
			this.TransportPageControl.TabIndex = 0;
			// 
			// CarrierUserControl
			// 
			this.Controls.Add(this.TransportTabControl);
			this.IsModifyCarrier = true;
			this.Name = "CarrierUserControl";
			this.ShouldSerializeTabPageMethods = false;
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(900, 616, true);
			this.Controls.SetChildIndex(this.SecurityPanel, 0);
			this.Controls.SetChildIndex(this.TransportTabControl, 0);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.TransportTabControl.ResumeLayout(false);
			this.TransportTabPage1.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		public Enterprise.ZArchitecture.GUI.ZTemplateTabControl TransportTabControl;
		private Enterprise.ZArchitecture.GUI.ZTabPage TransportTabPage1;
		public CarrierConfigurationUserControl TransportPageControl;
	}
}
