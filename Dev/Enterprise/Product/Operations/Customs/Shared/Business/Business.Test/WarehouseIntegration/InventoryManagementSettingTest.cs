using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business.Testing
{
	public class InventoryManagementSettingTest : TestCaseWithFactory
	{
		public void TestProperties()
		{
			var setting = new InventoryManagementSetting(false, false, false);
			Assert(!setting.SupportBondedWarehouse);
			Assert(!setting.SupportInwardProcessing);
			Assert(!setting.SupportOutwardProcessing);

			setting = new InventoryManagementSetting(true, true, true);
			Assert(setting.SupportBondedWarehouse);
			Assert(setting.SupportInwardProcessing);
			Assert(!setting.SupportOutwardProcessing);

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.CompanyData.OB_IMUsedBondedWhs = true;
			orgHeader.CompanyData.OB_CusInventoryForInwardProcessing = true;
			orgHeader.CompanyData.OB_CusInventoryForOutwardProcessing = true;

			setting = InventoryManagementSetting.New(orgHeader);
			Assert(setting.SupportBondedWarehouse);
			Assert(setting.SupportInwardProcessing);
			Assert(!setting.SupportOutwardProcessing);

			orgHeader.CompanyData.OB_IMUsedBondedWhs = false;
			orgHeader.CompanyData.OB_CusInventoryForInwardProcessing = false;
			orgHeader.CompanyData.OB_CusInventoryForOutwardProcessing = false;
			setting = InventoryManagementSetting.New(orgHeader);
			Assert(!setting.SupportBondedWarehouse);
			Assert(!setting.SupportInwardProcessing);
			Assert(!setting.SupportOutwardProcessing);
		}
	}
}
