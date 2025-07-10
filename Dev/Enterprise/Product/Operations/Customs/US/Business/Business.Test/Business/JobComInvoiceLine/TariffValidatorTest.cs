using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class TariffValidatorTest : TestCaseWithFactory
	{
		public void TestValidateWhenImportTariffIsNullWhenNotApplicable()
		{
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			invoiceLine.US_SupTariff = TariffViewAsCodeDescription.NotApplicableCode;
			TariffValidator.Validate(invoiceLine, null, invoiceLine.US_SupTariffInfo);
			AssertNoNotifications(invoiceLine.US_SupTariffInfo);
		}

		public void TestValidateWhenTariffViewIsNullWhenNotApplicable()
		{
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			invoiceLine.US_SupTariff = TariffViewAsCodeDescription.NotApplicableCode;
			TariffValidator.ValidateWhenTariffViewIsNull(invoiceLine, invoiceLine.US_SupTariffInfo, ZString.Empty, ZDateTime.Now);
			AssertNoNotifications(invoiceLine.US_SupTariffInfo);
		}

		public void TestEnsure98_99IsEnteredWhenNotApplicable()
		{
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			invoiceLine.US_SupTariff = TariffViewAsCodeDescription.NotApplicableCode;
			TariffValidator.Ensure98_99IsEntered(invoiceLine.US_SupTariffInfo);
			AssertNoNotifications(invoiceLine.US_SupTariffInfo);
		}
	}
}
