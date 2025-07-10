using System;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Warehouse.Transactions.GUI
{
	public partial class InventoryForm
	{
		ZTabPage CustomFieldsTabPage;
		MasterFiles.GUI.CustomLabelsUserControl customLabelsUserControl1;
		InventoryUserControl inventoryUserControl1;
		protected InventoryAllocationsUserControl inventoryAllocationsUserControl1;
		protected InventoryPackageUserControl inventoryPackageUserControl;
		protected InventoryBOMComponentsUserControl inventoryBOMComponentsUserControl;
		protected ZTabPage AllocationsTabPage;
		protected ZTabPage PackageTabPage;
		protected ZTabPage ComponentTabPage;

		protected override void InitializeComponent()
		{
			this.CustomFieldsTabPage = new ZTabPage();
			this.AllocationsTabPage = new ZTabPage();
			this.PackageTabPage = new ZTabPage();
			this.ComponentTabPage = new ZTabPage();
			this.MainTabControl.SuspendLayout();
			this.MainPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainTabControl
			// 
			this.MainTabControl.Controls.Add(this.AllocationsTabPage);
			this.MainTabControl.Controls.Add(this.PackageTabPage);
			this.MainTabControl.Controls.Add(this.ComponentTabPage);
			this.MainTabControl.Controls.Add(this.CustomFieldsTabPage);
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1034, 550, true);
			this.MainTabControl.TabIndex = 0;
			this.MainTabControl.Controls.SetChildIndex(this.NotesTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.LogsTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.CustomFieldsTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.AllocationsTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.PackageTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.ComponentTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.MainTabPage, 0);
			// 
			// MainTabPage
			// 
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1026, 530, true);
			this.MainTabPage.RunWhenBindingOrFirstShown(new EventHandler(this.MainTabPage_InitializeTab));
			// 
			// NotesTabPage
			// 
			this.NotesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1026, 478, true);
			// 
			// LogsTabPage
			// 
			this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1026, 478, true);
			// 
			// MainPanel
			// 
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1034, 533, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1034, 24, true);
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(348);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(349);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(WhsDocketLine);
			// 
			// CustomFieldsTabPage
			// 
			this.CustomFieldsTabPage.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("InventoryForm|f7fa8a56-a669-4877-bd97-794aaac359b3", "Additional Info");
			this.CustomFieldsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.CustomFieldsTabPage.Name = "CustomFieldsTabPage";
			this.CustomFieldsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1026, 478, true);
			this.CustomFieldsTabPage.TabIndex = 3;
			this.CustomFieldsTabPage.RunWhenBindingOrFirstShown(new EventHandler(this.CustomFieldsTabPage_InitializeTab));
			// 
			// AllocationsTabPage
			// 
			this.AllocationsTabPage.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("InventoryForm|790fc5db-9db5-4cde-b50f-99a1bb6c6c99", "Allocations");
			this.AllocationsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.AllocationsTabPage.Name = "AllocationsTabPage";
			this.AllocationsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1026, 478, true);
			this.AllocationsTabPage.TabIndex = 4;
			this.AllocationsTabPage.UseVisualStyleBackColor = true;
			this.AllocationsTabPage.RunWhenBindingOrFirstShown(new EventHandler(this.AllocationsTabPage_InitializeTab));
			// 
			// PackageTabPage
			// 
			this.PackageTabPage.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("InventoryForm|756f575c-644d-4e49-9bfa-6a331131f49c", "Package Details");
			this.PackageTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.PackageTabPage.Name = "PackageTabPage";
			this.PackageTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1026, 478, true);
			this.PackageTabPage.TabIndex = 5;
			this.PackageTabPage.UseVisualStyleBackColor = true;
			this.PackageTabPage.RunWhenBindingOrFirstShown(new EventHandler(this.PackageTabPage_InitializeTab));
			// 
			// ComponentTabPage
			// 
			this.ComponentTabPage.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("InventoryForm|aed81c5d-9f7b-4044-967a-725350d08fbc", "Components");
			this.ComponentTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.ComponentTabPage.Name = "ComponentTabPage";
			this.ComponentTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1026, 478, true);
			this.ComponentTabPage.TabIndex = 5;
			this.ComponentTabPage.UseVisualStyleBackColor = true;
			this.ComponentTabPage.RunWhenBindingOrFirstShown(new EventHandler(this.ComponentTabPage_InitializeTab));
			// 
			// InventoryForm
			// 
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("6cb119ce-7cb6-4262-b094-5647bb2c8b87", "Inventory");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1034, 610, true);
			this.DataSourceType = typeof(WhsDocketLine);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1048, 600, true);
			this.Name = "InventoryForm";
			this.ShouldSerializeTabPageMethods = true;
			this.Text = "Inventory";
			this.MainTabControl.ResumeLayout(false);
			this.MainTabControl.PerformLayout();
			this.MainPanel.ResumeLayout(false);
			this.MainPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
