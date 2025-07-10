using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.Business;

namespace Enterprise.Customs.US.AES.Testing
{
	abstract class AESPrintCommodityLineTest : NonPersistentBusinessObjectTestCase
	{
		public void TestGrossWtUQ()
		{
			var commodityLineForTest = GetLineForTest();
			AssertEquals("Gross Wt UQ always KG", "KG", commodityLineForTest.GrossWtUQ);
		}

		public virtual void TestLicenseTypeDescription()
		{
			var commodityLineForTest = GetLineForTest();
			AssertEquals("License Type description", ZString.Empty, commodityLineForTest.LicenseTypeDescription);
		}

		public void TestGetVehicleIDTypeDescription()
		{
			var commodityLineForTest = GetLineForTest();
			AssertEquals("Vehicle ID Type description", VehicleIDTypeList.Descriptions.ProductID, commodityLineForTest.GetVehicleIDTypeDescription(VehicleIDTypeList.Codes.ProductID));
			AssertEquals("Vehicle ID Type description", VehicleIDTypeList.Descriptions.VIN, commodityLineForTest.GetVehicleIDTypeDescription(VehicleIDTypeList.Codes.VIN));
			AssertEquals("Vehicle ID Type description", ZString.Empty, commodityLineForTest.GetVehicleIDTypeDescription(ZString.Empty));
			AssertEquals("Vehicle ID Type description", ZString.Empty, commodityLineForTest.GetVehicleIDTypeDescription("!E"));
		}

		public void TestGetStateDescription()
		{
			var commodityLineForTest = GetLineForTest();
			AssertEquals("State description", "!E", commodityLineForTest.GetStateDescription("!E"));
			AssertEquals("State description", ZString.Empty, commodityLineForTest.GetStateDescription(ZString.Empty));
			AssertEquals("State description", "Alabama (AL)", commodityLineForTest.GetStateDescription("AL"));
		}

		public void TestGetUSMLCategoryDescription()
		{
			var commodityLineForTest = GetLineForTest();
			AssertEquals("USML Category description", ZString.Empty, commodityLineForTest.GetUSMLCategoryDescription(ZString.Empty));
			AssertEquals("USML Category description", USMLCategoryCodes.Descriptions.AuxiliaryMilitaryEquipment, commodityLineForTest.GetUSMLCategoryDescription(USMLCategoryCodes.Codes.AuxiliaryMilitaryEquipment));
		}

		protected virtual AESPrintCommodityLine GetLineForTest() => null;

		protected JobDeclaration Declaration
		{
			get
			{
				if (declaration == null)
				{
					declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				}
				return declaration;
			}
		}
		JobDeclaration declaration;

		protected CusEntryHeader Entry => entry ?? (entry = Declaration.CustomsEntryHeaders.AddNew());
		CusEntryHeader entry;
	}
}
