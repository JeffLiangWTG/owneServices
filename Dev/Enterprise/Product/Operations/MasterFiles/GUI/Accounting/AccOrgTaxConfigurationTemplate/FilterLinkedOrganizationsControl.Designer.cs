namespace Enterprise.MasterFiles.GUI
{
	partial class FilterLinkedOrganizationsControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.FilterPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.LinkedOrganizationsControl = new Enterprise.MasterFiles.GUI.LinkedOrganizationsControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.FilterPanel.SuspendLayout();
			this.LinkedOrganizationsControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.AccOrgTaxConfigurationTemplate);
			// 
			// FilterPanel
			// 
			this.FilterPanel.Controls.Add(this.LinkedOrganizationsControl);
			this.FilterPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.FilterPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.FilterPanel.Name = "FilterPanel";
			this.FilterPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(640, 314, true);
			this.FilterPanel.TabIndex = 1;
			// 
			// LinkedOrganizationsControl
			// 
			this.LinkedOrganizationsControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.LinkedOrganizationsControl, ".");
			this.LinkedOrganizationsControl.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.LinkedOrganizationsControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 49, true);
			this.LinkedOrganizationsControl.Name = "LinkedOrganizationsControl";
			this.LinkedOrganizationsControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(640, 265, true);
			this.LinkedOrganizationsControl.TabIndex = 3;
			this.LinkedOrganizationsControl.OnAttached += new System.EventHandler<Enterprise.ZArchitecture.GUI.ModuleButtonGridOnAttachEventArgs>(this.LinkedOrganizationsControl_OnAttached);
			this.LinkedOrganizationsControl.BeforeAttach += new System.EventHandler<Enterprise.ZArchitecture.GUI.ModuleButtonGridOnAttachEventArgs>(this.LinkedOrganizationsControl_BeforeAttach);
			this.LinkedOrganizationsControl.OnAttaching += new Enterprise.ZArchitecture.GUI.ModuleButtonGridOperationCancelEventHandler(this.LinkedOrganizationsControl_OnAttaching);
			this.LinkedOrganizationsControl.OnDetached += new System.EventHandler<Enterprise.ZArchitecture.GUI.ModuleButtonGridOnDetachedEventArgs>(this.LinkedOrganizationsControl_OnDetached);
			// 
			// FilterLinkedOrganizationsControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.FilterPanel);
			this.Name = "FilterLinkedOrganizationsControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(640, 314, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.FilterPanel.ResumeLayout(false);
			this.FilterPanel.PerformLayout();
			this.LinkedOrganizationsControl.ResumeLayout(true);
			this.LinkedOrganizationsControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private ZArchitecture.GUI.ZPanel FilterPanel;
		private LinkedOrganizationsControl LinkedOrganizationsControl;
	}
}
