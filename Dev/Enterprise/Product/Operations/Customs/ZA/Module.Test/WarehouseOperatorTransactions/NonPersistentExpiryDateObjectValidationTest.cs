using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.ZA.Module.Testing
{
	class NonPersistentExpiryDateObjectValidationTest : BusinessObjectValidationTestCase
	{
		public void TestDateValidation()
		{
			var bo = new NonPersistentExpiryDateObject();
			AssertNoNotifications(bo.DateInfo);
			bo.Date = ZDate.Today.AddMonths(-24);
			AssertNoNotifications(bo.DateInfo);
			bo.Date = ZDate.Invalid;
			AssertHasNotifications(bo.DateInfo);
		}
	}
}
