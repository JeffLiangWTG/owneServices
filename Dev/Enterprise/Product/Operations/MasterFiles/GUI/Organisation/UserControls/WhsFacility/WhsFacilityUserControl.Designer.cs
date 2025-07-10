using Enterprise.Registry.Business.Warehouse;

namespace Enterprise.MasterFiles.GUI
{
	public partial class WhsFacilityUserControl
	{

		#region Component Designer generated code

		internal Enterprise.ZArchitecture.GUI.ZTemplateTabControl WhsFacilityTabControl;
		internal Enterprise.ZArchitecture.GUI.ZTabPage ProductWarehouseTabPage;
		private Enterprise.ZArchitecture.GUI.ZTabPage ContainerYardTabPage;
		internal WarehouseUserControl WarehouseUserControl;
		private CYDUserControl CYDUserControl;
		private System.ComponentModel.IContainer components;

		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.WhsFacilityTabControl = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
			this.ProductWarehouseTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.ContainerYardTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.SecurityPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.WhsFacilityTabControl.SuspendLayout();
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
			// WhsFacilityTabControl
			// 
			this.WhsFacilityTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.WhsFacilityTabControl.Controls.Add(this.ProductWarehouseTabPage);
			this.WhsFacilityTabControl.Controls.Add(this.ContainerYardTabPage);
			this.WhsFacilityTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.WhsFacilityTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 24, true);
			this.WhsFacilityTabControl.Name = "WhsFacilityTabControl";
			this.WhsFacilityTabControl.SelectedIndex = 0;
			this.WhsFacilityTabControl.ShowToolTips = true;
			this.WhsFacilityTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(736, 427, true);
			this.WhsFacilityTabControl.TabIndex = 0;
			// 
			// ProductWarehouseTabPage
			// 
			this.ProductWarehouseTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("WhsFacilityUserControl|a93a30d7-466f-4f8d-bb4e-f15998772ebe", "Product Warehouse");
			this.ProductWarehouseTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.ProductWarehouseTabPage.Name = "ProductWarehouseTabPage";
			this.ProductWarehouseTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(728, 400, true);
			this.ProductWarehouseTabPage.TabIndex = 0;
			this.ProductWarehouseTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.ProductWarehouseTabPage_InitializeTab));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.OrgHeader)(((Enterprise.MasterFiles.Business.OrgHeader)(null)))));
			// 
			// ContainerYardTabPage
			// 
			this.ContainerYardTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("WhsFacilityUserControl|25ce3635-0a55-45f1-b4a9-ebb71f663dc8", "Container Yard");
			this.ContainerYardTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.ContainerYardTabPage.Name = "ContainerYardTabPage";
			this.ContainerYardTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(728, 400, true);
			this.ContainerYardTabPage.TabIndex = 1;
			this.ContainerYardTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.ContainerYardTabPage_InitializeTab));
			this.ContainerYardTabPage.TabVisible = WarehouseDataRegistry.Instance.EnableContainerYard.Value;
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.OrgHeader)(((Enterprise.MasterFiles.Business.OrgHeader)(null)))));
			// 
			// WhsFacilityUserControl
			// 
			this.Controls.Add(this.WhsFacilityTabControl);
			this.IsModifyWarehouse = true;
			this.Name = "WhsFacilityUserControl";
			this.ShouldSerializeTabPageMethods = true;
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(736, 451, true);
			this.Controls.SetChildIndex(this.SecurityPanel, 0);
			this.Controls.SetChildIndex(this.WhsFacilityTabControl, 0);
			this.SecurityPanel.ResumeLayout(false);
			this.SecurityPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.WhsFacilityTabControl.ResumeLayout(false);
			this.WhsFacilityTabControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		private void ProductWarehouseTabPage_InitializeTab(object sender, System.EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.WarehouseUserControl = new Enterprise.MasterFiles.GUI.WarehouseUserControl();
			this.ProductWarehouseTabPage.SuspendLayout();
			this.WarehouseUserControl.SuspendLayout();
			this.ProductWarehouseTabPage.Controls.Add(this.WarehouseUserControl);
			//
			// warehouseUserControl
			//
			this.WarehouseUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.WarehouseUserControl, ".");
			this.WarehouseUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.WarehouseUserControl.IsModifyWarehouse = true;
			this.WarehouseUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.WarehouseUserControl.Name = "warehouseUserControl";
			this.WarehouseUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(192, 73, true);
			this.WarehouseUserControl.TabIndex = 0;
			this.ProductWarehouseTabPage.PerformLayout();
			this.WarehouseUserControl.ResumeLayout(true);
			this.WarehouseUserControl.PerformLayout();
			this.ProductWarehouseTabPage.ResumeLayout(true);
		}

		private void ContainerYardTabPage_InitializeTab(object sender, System.EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.CYDUserControl = new Enterprise.MasterFiles.GUI.CYDUserControl();
			this.ContainerYardTabPage.SuspendLayout();
			this.CYDUserControl.SuspendLayout();
			this.ContainerYardTabPage.Controls.Add(this.CYDUserControl);
			// 
			// CYDUserControl
			// 
			this.CYDUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CYDUserControl, ".");
			this.CYDUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CYDUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CYDUserControl.Name = "CYDUserControl";
			this.CYDUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(192, 73, true);
			this.CYDUserControl.TabIndex = 0;
			this.ContainerYardTabPage.PerformLayout();
			this.CYDUserControl.ResumeLayout(true);
			this.CYDUserControl.PerformLayout();
			this.ContainerYardTabPage.ResumeLayout(true);
		}

		#endregion
	}
}
