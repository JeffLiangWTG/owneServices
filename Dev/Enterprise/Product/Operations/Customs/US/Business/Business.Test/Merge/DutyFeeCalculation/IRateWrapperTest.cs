using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class IRateWrapperExtensionTest : TestCaseWithFactory
	{
		[TestDate(2009, 1, 1)]
		public void TestIsInvalidDutyRate()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.US_SupTariff = "99100466";
			invoiceLine.US_UC_NKCountryOfOrigin = "SG";

			IRateWrapper rate = DutyRateWrapper.GetWrapper(new SupplementaryParentTariffIDutyData(invoiceLine));
			Assert(rate.IsInvalidDutyRate());

			invoiceLine.US_SPI = "SG";
			rate = DutyRateWrapper.GetWrapper(new SupplementaryParentTariffIDutyData(invoiceLine));
			Assert(!rate.IsInvalidDutyRate());
		}
	}
}
