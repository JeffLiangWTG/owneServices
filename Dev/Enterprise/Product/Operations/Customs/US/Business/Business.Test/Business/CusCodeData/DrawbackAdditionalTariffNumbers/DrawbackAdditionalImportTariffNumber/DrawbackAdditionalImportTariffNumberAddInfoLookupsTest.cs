using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class DrawbackAdditionalImportTariffNumberAddInfoLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestImportTariffs()
		{
			AssertEquals(typeof(USCTariffCollection), AdditionalImportTariff.AddInfoLookups.ImportTariffs.GetType());
		}

		public void TestUnitOfMeasureCodes()
		{
			AssertEquals(typeof(ACEDrawbackUnitOfMeasureList), AdditionalImportTariff.AddInfoLookups.UnitOfMeasureCodes.GetType());
		}

		DrawbackAdditionalImportTariffNumber AdditionalImportTariff
		{
			get
			{
				if (additionalImportTariff == null)
				{
					var declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
					declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
					var invoice = declaration.Invoices.AddNew();
					var invoiceLine = invoice.JobComInvoiceLines.AddNew();
					additionalImportTariff = invoiceLine.DrawbackAdditionalImportTariffNumbers.AddNew();
				}
				return additionalImportTariff;
			}
		}

		DrawbackAdditionalImportTariffNumber additionalImportTariff;
	}
}
