using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.UY.Manifest.Business.Testing
{
	class AsycudaBillForMasterChildValidationTest : BusinessObjectValidationTestCase
	{
		public void TestABL_E_ARV()
		{
			var bill = Factory.New<AsycudaBill>();
			bill.ABL_E_ARV = ZDateTime.Empty;

			AssertNoNotifications(bill.ABL_E_ARVInfo);
		}
	}
}
