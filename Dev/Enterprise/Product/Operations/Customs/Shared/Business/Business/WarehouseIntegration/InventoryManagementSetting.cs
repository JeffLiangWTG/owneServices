using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business
{
	public struct InventoryManagementSetting
	{
		public InventoryManagementSetting(bool supportBondedWarehouse, bool supportInwardProcessing, bool supportOutwardProcessing)
		{
			SupportBondedWarehouse = supportBondedWarehouse;
			SupportInwardProcessing = supportInwardProcessing;
			SupportOutwardProcessing = false;
		}

		public static InventoryManagementSetting New(OrgHeader org)
		{
			var supportBondedWarehouse = false;
			var supportInwardProcessing = false;
			var supportOutwardProcessing = false;
			if (org?.CompanyData is OrgCompanyData companyData)
			{
				supportBondedWarehouse = companyData.OB_IMUsedBondedWhs;
				supportInwardProcessing = companyData.OB_CusInventoryForInwardProcessing;
				supportOutwardProcessing = false;
			}
			return new InventoryManagementSetting(supportBondedWarehouse, supportInwardProcessing, supportOutwardProcessing);
		}

		public bool SupportBondedWarehouse { get; }
		public bool SupportInwardProcessing { get; }
		public bool SupportOutwardProcessing { get; }
	}
}
