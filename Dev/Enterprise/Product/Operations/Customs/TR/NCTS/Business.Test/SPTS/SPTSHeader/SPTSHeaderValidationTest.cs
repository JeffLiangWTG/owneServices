using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.TR.NCTS.Business.Testing
{
	class SPTSHeaderValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckBH_VoyageNumber()
		{
			var header = Factory.New<SPTSHeader>();
			header.BH_VoyageNumber = "XXX";
			AssertNoMessageErrorContaining(header.BH_VoyageNumberInfo, MandatoryValidation.YouHaveNotEntered);
			header.BH_VoyageNumber = "";
			AssertHasMessageErrorContaining(header.BH_VoyageNumberInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckBH_SailingDate()
		{
			var header = Factory.New<SPTSHeader>();
			header.BH_SailingDate = ZDateTime.Today;
			AssertNoMessageErrorContaining(header.BH_SailingDateInfo, MandatoryValidation.YouHaveNotEntered);
			header.BH_SailingDate = ZDate.Empty;
			AssertHasMessageErrorContaining(header.BH_SailingDateInfo, MandatoryValidation.YouHaveNotEntered);
		}
	}
}
