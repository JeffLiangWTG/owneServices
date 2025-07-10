namespace Enterprise.ContractManagement.Module
{
	partial class CarrierContractFilterStripControl
	{
		void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo contractNumberColumnStyleInfo = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo startDateColumnStyleInfo  = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo expiryDateColumnStyleInfo = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo serviceProviderColumnStyleInfo = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo descriptionColumnStyleInfo = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo containerTypeColumnStyleInfo = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo contractOwnerColumnStyleInfo = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();

			((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();

			contractNumberColumnStyleInfo.CaptionResourceString = Enterprise.ContractManagement.Module.Res.GetData("76c25af0-7068-1bb9-407d-6a8eb8480bc3", "Contract ID");
			contractNumberColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(93);
			contractNumberColumnStyleInfo.IsReadOnly = true;
			contractNumberColumnStyleInfo.ColumnName = "RCT_ContractNumber";

			serviceProviderColumnStyleInfo.CaptionResourceString = Enterprise.ContractManagement.Module.Res.GetData("9ad331f0-a5b8-fa86-4247-7d6e3d16f194", "Service Provider");
			serviceProviderColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(93);
			serviceProviderColumnStyleInfo.IsReadOnly = true;
			serviceProviderColumnStyleInfo.ColumnName = "RCT_OH";

			descriptionColumnStyleInfo.CaptionResourceString = Enterprise.ContractManagement.Module.Res.GetData("8d097512-814d-f4bc-44a8-e9808c41a22e", "Description");
			descriptionColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(93);
			descriptionColumnStyleInfo.IsReadOnly = true;
			descriptionColumnStyleInfo.ColumnName = "RCT_Description";

			startDateColumnStyleInfo.CaptionResourceString = Enterprise.ContractManagement.Module.Res.GetData("3b4275ff-8282-e488-403f-b81f37e27e54", "Start Date");
			startDateColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(93);
			startDateColumnStyleInfo.IsReadOnly = true;
			startDateColumnStyleInfo.DateTimeFormat = ZArchitecture.Core.ZDateTimePickerFormat.Short;
			startDateColumnStyleInfo.ColumnName = "RCT_StartDate";

			expiryDateColumnStyleInfo.CaptionResourceString = Enterprise.ContractManagement.Module.Res.GetData("76404a0f-bfbc-cbb0-47b1-53dc2ad9fdee", "Expiry Date");
			expiryDateColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(93);
			expiryDateColumnStyleInfo.IsReadOnly = true;
			expiryDateColumnStyleInfo.DateTimeFormat = ZArchitecture.Core.ZDateTimePickerFormat.Short;
			expiryDateColumnStyleInfo.ColumnName = "RCT_EndDate";

			containerTypeColumnStyleInfo.CaptionResourceString = Enterprise.ContractManagement.Module.Res.GetData("7a47e31b-483e-6fa2-442f-f228a8b63da2", "Container Type");
			containerTypeColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(93);
			containerTypeColumnStyleInfo.IsReadOnly = true;
			containerTypeColumnStyleInfo.ColumnName = "RCT_ContainerType";

			contractOwnerColumnStyleInfo.CaptionResourceString = Enterprise.ContractManagement.Module.Res.GetData("42d11da0-3f99-4657-91ab-7543e8d9efeb", "Contract Owner");
			contractOwnerColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(93);
			contractOwnerColumnStyleInfo.IsReadOnly = true;
			contractOwnerColumnStyleInfo.ColumnName = "RCT_GS_NKContractOwner";

			this.BindingSource.SetBindingMember(this.grid, "CarrierContracts");

			this.grid.ColumnStyles.Add(contractNumberColumnStyleInfo);
			this.grid.ColumnStyles.Add(serviceProviderColumnStyleInfo);
			this.grid.ColumnStyles.Add(descriptionColumnStyleInfo);
			this.grid.ColumnStyles.Add(startDateColumnStyleInfo);
			this.grid.ColumnStyles.Add(expiryDateColumnStyleInfo);
			this.grid.ColumnStyles.Add(containerTypeColumnStyleInfo);
			this.grid.ColumnStyles.Add(contractOwnerColumnStyleInfo);

			this.grid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 3, true);
			this.grid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(758, 592, true);
			this.grid.TabIndex = 23;
			this.BindingSource.DataSourceType = typeof(Enterprise.ContractManagement.Business.ViewCarrierContractsManager);

			this.CaptionRenderingEnabled = true;
			this.Name = "CarrierContractFilterStripControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1521, 400, true);
			((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
