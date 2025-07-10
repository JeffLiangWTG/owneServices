namespace Enterprise.Warehouse.Environment.Module
{
	public partial class WarehouseFilterControl
	{
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
			this.grid.SuspendLayout();
			this.AddStripButton.SuspendLayout();
			this.RecentItemsPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// grid
			// 
			this.grid.AllowDrop = true;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Warehouse.Environment.Module.Res.GetData("WarehouseFilterControl|WW_WarehouseCode", "Code");
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "WW_WarehouseCode";
			zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Warehouse.Environment.Module.Res.GetData("WarehouseFilterControl|WW_WarehouseName", "Name");
			zTextBoxColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo2.ColumnName = "WW_WarehouseNameMultilingual";
			zTextBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Warehouse.Environment.Module.Res.GetData("WarehouseFilterControl|WW_OA_WarehouseAddress", "Address");
			zGuidFindBoxColumnStyleInfo1.ColumnName = "WW_OA_WarehouseAddress";
			zGuidFindBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Warehouse.Environment.Module.Res.GetData("WarehouseFilterControl|CountryCode", "Country/Region Code");
			zTextBoxColumnStyleInfo3.ColumnName = "CountryCode";
			zTextBoxColumnStyleInfo3.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Warehouse.Environment.Module.Res.GetData("WarehouseFilterControl|WW_GB_RelatedCompanyBranch", "Branch");
			zGuidFindBoxColumnStyleInfo2.ColumnName = "WW_GB_RelatedCompanyBranch";
			zGuidFindBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
			zGuidFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo3.ColumnName = "WW_GG_ReleaseGroup";
			zGuidFindBoxColumnStyleInfo3.DefaultCollectionIndex = 0;
			zGuidFindBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(85);
			zTextBoxColumnStyleInfo4.ColumnName = "WarehouseTypeDescription";
			zTextBoxColumnStyleInfo4.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(138);
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Warehouse.Environment.Module.Res.GetData("WarehouseFilterControl|WW_IsActive", "Is Active");
			zCheckBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zCheckBoxColumnStyleInfo1.ColumnName = "WW_IsActive";
			zCheckBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Warehouse.Environment.Module.Res.GetData("WarehouseFilterControl|IsWarehouseFreeStoreEnabled", "Free Store Enabled");
			zCheckBoxColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zCheckBoxColumnStyleInfo2.ColumnName = "IsWarehouseFreeStoreEnabled";
			zCheckBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
			zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Warehouse.Environment.Module.Res.GetData("WarehouseFilterControl|IsWarehouseBondEnabled", "Bond Enabled");
			zCheckBoxColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zCheckBoxColumnStyleInfo3.ColumnName = "IsWarehouseBondEnabled";
			zCheckBoxColumnStyleInfo3.DefaultCollectionIndex = 0;
			zCheckBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Warehouse.Environment.Module.Res.GetData("WarehouseFilterControl|IsWarehouseExciseEnabled", "Excise Enabled");
			zCheckBoxColumnStyleInfo4.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zCheckBoxColumnStyleInfo4.ColumnName = "IsWarehouseExciseEnabled";
			zCheckBoxColumnStyleInfo4.DefaultCollectionIndex = 0;
			zCheckBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Warehouse.Environment.Module.Res.GetData("WarehouseFilterControl|WW_IsVirtualWarehouse", "Virtual Warehouse");
			zCheckBoxColumnStyleInfo5.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zCheckBoxColumnStyleInfo5.ColumnName = "WW_IsVirtualWarehouse";
			zCheckBoxColumnStyleInfo5.DefaultCollectionIndex = 0;
			zCheckBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo6.CaptionResourceString = Enterprise.Warehouse.Environment.Module.Res.GetData("WarehouseFilterControl|IsInwardProcessingEnabled", "Inward Processing Enabled");
			zCheckBoxColumnStyleInfo6.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zCheckBoxColumnStyleInfo6.ColumnName = "IsInwardProcessingEnabled";
			zCheckBoxColumnStyleInfo6.DefaultCollectionIndex = 0;
			zCheckBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.grid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.grid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo2);
			this.grid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo3);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.grid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.grid.ColumnStyles.Add(zCheckBoxColumnStyleInfo3);
			this.grid.ColumnStyles.Add(zCheckBoxColumnStyleInfo4);
			this.grid.ColumnStyles.Add(zCheckBoxColumnStyleInfo5);
			this.grid.ColumnStyles.Add(zCheckBoxColumnStyleInfo6);
			this.grid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 88, true);
			this.grid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(745, 458, true);
			this.grid.TabIndex = 99;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Warehouse.Environment.Business.WhsWarehouse);
			// 
			// WarehouseFilterControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Name = "WarehouseFilterControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(745, 546, true);
			((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
			this.grid.ResumeLayout(false);
			this.grid.PerformLayout();
			this.AddStripButton.ResumeLayout(true);
			this.AddStripButton.PerformLayout();
			this.RecentItemsPanel.ResumeLayout(false);
			this.RecentItemsPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
	}
}
