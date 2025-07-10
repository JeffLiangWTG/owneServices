using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class BulkUpdateP8_DateForExchangeRateActionValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateDate()
		{
			var opportunity1 = Factory.New<OrgOpportunity>();
			var opportunity2 = Factory.New<OrgOpportunity>();
			var updater = new BulkUpdateP8_DateForExchangeRateAction(new[] { opportunity1, opportunity2 });

			updater.Date = new ZDateTime(2022, 2, 2);
			AssertMandatoryValidationError(updater.DateInfo, false);

			updater.Date = ZDateTime.Empty;
			AssertMandatoryValidationError(updater.DateInfo, true);
		}
	}
}
