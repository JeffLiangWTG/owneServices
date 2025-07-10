namespace Enterprise.MasterFiles.GUI
{
	public partial class CYDUserControl
	{

		#region Component Designer generated code

		internal Enterprise.ZArchitecture.GUI.ZTemplateTabControl CYDTabControl;
		internal Enterprise.ZArchitecture.GUI.ZTabPage InvoicingTabPage;
		internal Enterprise.ZArchitecture.GUI.ZTabPage MaintenanceTabPage;
		private CYDInvoicingUserControl CYDInvoicingUserControl;
		private CYDMaintenanceUserControl CYDMaintenanceUserControl;
		private System.ComponentModel.IContainer components;

		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.CYDTabControl = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
			this.InvoicingTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.MaintenanceTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.SecurityPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.CYDTabControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// SecurityPanel
			// 
			this.SecurityPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(736, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.OrgHeader);
			// 
			// CYDTabControl
			// 
			this.CYDTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.CYDTabControl.Controls.Add(this.InvoicingTabPage);
			this.CYDTabControl.Controls.Add(this.MaintenanceTabPage);
			this.CYDTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CYDTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 24, true);
			this.CYDTabControl.Name = "CYDTabControl";
			this.CYDTabControl.SelectedIndex = 0;
			this.CYDTabControl.ShowToolTips = true;
			this.CYDTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(736, 427, true);
			this.CYDTabControl.TabIndex = 0;
			// 
			// InvoicingTabPage
			// 
			this.InvoicingTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("WarehouseUserControl|5129f1d1-4672-450b-912c-8b2c1104c87f", "Invoicing");
			this.InvoicingTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.InvoicingTabPage.Name = "InvoicingTabPage";
			this.InvoicingTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(728, 400, true);
			this.InvoicingTabPage.TabIndex = 3;
			this.InvoicingTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.InvoicingTabPage_InitializeTab));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.OrgHeader)(((Enterprise.MasterFiles.Business.OrgHeader)(null)))));
			// 
			// MaintenanceTabPage
			// 
			this.MaintenanceTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("WarehouseUserControl|4ed7507b-0b20-416a-8f12-c617357c45e1", "Maintenance && Repair");
			this.MaintenanceTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 20, true);
			this.MaintenanceTabPage.Name = "MaintenanceTabPage";
			this.MaintenanceTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(730, 404, true);
			this.MaintenanceTabPage.TabIndex = 4;
			this.MaintenanceTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.MaintenanceTabPage_InitializeTab));
			// 
			// CYDUserControl
			// 
			this.Controls.Add(this.CYDTabControl);
			this.IsModifyWarehouse = true;
			this.Name = "CYDUserControl";
			this.ShouldSerializeTabPageMethods = true;
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(736, 451, true);
			this.Controls.SetChildIndex(this.SecurityPanel, 0);
			this.Controls.SetChildIndex(this.CYDTabControl, 0);
			this.SecurityPanel.ResumeLayout(false);
			this.SecurityPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.CYDTabControl.ResumeLayout(false);
			this.CYDTabControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		private void InvoicingTabPage_InitializeTab(object sender, System.EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.CYDInvoicingUserControl = new Enterprise.MasterFiles.GUI.CYDInvoicingUserControl();
			this.InvoicingTabPage.SuspendLayout();
			this.CYDInvoicingUserControl.SuspendLayout();
			this.InvoicingTabPage.Controls.Add(this.CYDInvoicingUserControl);
			// 
			// CYDInvoicingUserControl
			// 
			this.CYDInvoicingUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CYDInvoicingUserControl, ".");
			this.CYDInvoicingUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CYDInvoicingUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CYDInvoicingUserControl.Name = "CYDInvoicingUserControl";
			this.CYDInvoicingUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(192, 73, true);
			this.CYDInvoicingUserControl.TabIndex = 0;
			this.InvoicingTabPage.PerformLayout();
			this.CYDInvoicingUserControl.ResumeLayout(true);
			this.CYDInvoicingUserControl.PerformLayout();
			this.InvoicingTabPage.ResumeLayout(true);

		}

		private void MaintenanceTabPage_InitializeTab(object sender, System.EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.CYDMaintenanceUserControl = new Enterprise.MasterFiles.GUI.CYDMaintenanceUserControl();
			this.MaintenanceTabPage.SuspendLayout();
			this.CYDMaintenanceUserControl.SuspendLayout();
			this.MaintenanceTabPage.Controls.Add(this.CYDMaintenanceUserControl);
			// 
			// CYDMaintenanceUserControl
			// 
			this.CYDMaintenanceUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CYDMaintenanceUserControl, ".");
			this.CYDMaintenanceUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CYDMaintenanceUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CYDMaintenanceUserControl.Name = "CYDMaintenanceUserControl";
			this.CYDMaintenanceUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(192, 73, true);
			this.CYDMaintenanceUserControl.TabIndex = 1;
			this.MaintenanceTabPage.PerformLayout();
			this.CYDMaintenanceUserControl.ResumeLayout(true);
			this.CYDMaintenanceUserControl.PerformLayout();
			this.MaintenanceTabPage.ResumeLayout(true);

		}

		#endregion
	}
}
