using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.TR.NCTS.Business.Testing
{
	class NctsPackageValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckB5_UnitCount_ValueMustBeInteger()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);

			var goodsItem = nctsHeader.MovementHeader.GoodsItems.AddNew();
			var nctsPackage = goodsItem.Packages.AddNew();

			nctsPackage.B5_UnitCount = 2147483648;
			AssertHasErrorContaining(nctsPackage.B5_UnitCountInfo, "The maximum quantity should be 2147483647.");

			nctsPackage.B5_UnitCount = 2147483647;
			AssertNoErrorContaining(nctsPackage.B5_UnitCountInfo, "The maximum quantity should be 2147483647.");

			var nctsPackage2 = goodsItem.Packages.AddNew();
			nctsPackage2.B5_UnitCount = 1;
			AssertHasErrorContaining(nctsPackage2.B5_UnitCountInfo, "The maximum quantity should be 2147483647.");
		}
	}
}


