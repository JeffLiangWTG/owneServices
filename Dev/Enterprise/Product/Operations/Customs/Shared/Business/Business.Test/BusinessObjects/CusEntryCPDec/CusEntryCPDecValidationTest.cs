using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.Business.Testing
{
	sealed class CusEntryCPDecValidationTest : BusinessObjectValidationTestCase
	{
		public void TestNoStartDateValidation()
		{
			BaseCusEntryCPDec cpDec = Factory.New<BaseCusEntryCPDec>();
			cpDec.ON_CPDecStartDate = new ZDateTime(2000, 1, 1);
			AssertNoNotifications(cpDec.ON_CPDecStartDateInfo);
		}

		public void TestNoEndDateValidation()
		{
			BaseCusEntryCPDec cpDec = Factory.New<BaseCusEntryCPDec>();
			cpDec.ON_CPDecEndDate = new ZDateTime(2000, 1, 1);
			AssertNoNotifications(cpDec.ON_CPDecEndDateInfo);
		}
	}
}
