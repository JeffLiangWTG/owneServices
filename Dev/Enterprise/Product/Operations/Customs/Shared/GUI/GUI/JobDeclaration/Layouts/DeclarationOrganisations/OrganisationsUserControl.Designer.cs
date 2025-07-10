using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	partial class OrganisationsUserControl
	{
		void InitializeComponent()
		{
			this.DynamicOrganisationsPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.Business.BaseJobDeclaration);
			// 
			// DynamicOrganisationsPanel
			// 
			this.DynamicOrganisationsPanel.AllowDrop = true;
			this.DynamicOrganisationsPanel.AutoScroll = true;
			this.DynamicOrganisationsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DynamicOrganisationsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DynamicOrganisationsPanel.Name = "DynamicOrganisationsPanel";
			this.DynamicOrganisationsPanel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.DynamicOrganisationsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(464, 281, true);
			this.DynamicOrganisationsPanel.TabIndex = 1;
			// 
			// OrganisationsUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.DynamicOrganisationsPanel);
			this.Name = "OrganisationsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(464, 281, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		internal DynamicLayoutPanel DynamicOrganisationsPanel;
	}
}
