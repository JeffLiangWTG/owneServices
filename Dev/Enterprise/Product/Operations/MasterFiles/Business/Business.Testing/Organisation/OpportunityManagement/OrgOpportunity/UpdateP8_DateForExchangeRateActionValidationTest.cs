using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class UpdateP8_DateForExchangeRateActionValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateDate()
		{
			var opportunity = Factory.New<OrgOpportunity>();
			var updater = new UpdateP8_DateForExchangeRateAction(opportunity);

			updater.Date = new ZDateTime(2022, 2, 2);
			AssertMandatoryValidationError(updater.DateInfo, false);

			updater.Date = ZDateTime.Empty;
			AssertMandatoryValidationError(updater.DateInfo, true);
		}
	}
}
