using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(DrawbackAdditionalImportTariffNumberAddInfo))]
	sealed class DrawbackAdditionalImportTariffNumberAddInfoTest : NonPersistentBusinessObjectTestCase
	{
		[TestDate(2020, 08, 02)]
		public void TestDefaultDescriptionFromTariffUsingImportDate()
		{
			var tariff1 = Factory.New<USCTariff>();
			tariff1.UE_Tariff = "1100010001";
			tariff1.UE_ShortDescription = "TEST TARIFF 1";
			tariff1.UE_DateFrom = new ZDateTime(2019, 07, 01);
			tariff1.UE_DateTo = new ZDateTime(2020, 06, 30);
			var tariff2 = Factory.New<USCTariff>();
			tariff2.UE_Tariff = "1100010001";
			tariff2.UE_ShortDescription = "TEST TARIFF 2";
			tariff2.UE_DateFrom = new ZDateTime(2018, 07, 01);
			tariff2.UE_DateTo = new ZDateTime(2019, 06, 30);
			Factory.Save();
			InvoiceLine.US_DRWEntryDate = new ZDateTime(2020, 05, 31);
			var importTariff = InvoiceLine.DrawbackAdditionalImportTariffNumbers.AddNew();
			importTariff.US_FormattedTariff = "1100010001";
			AssertEquals(tariff1.UE_ShortDescription, importTariff.US_Description);
		}

		[TestDate(2020, 08, 02)]
		public void TestDefaultDescriptionFromTariffUsingCurrentDate()
		{
			var tariff1 = Factory.New<USCTariff>();
			tariff1.UE_Tariff = "1100010001";
			tariff1.UE_ShortDescription = "TEST TARIFF 1";
			tariff1.UE_DateFrom = new ZDateTime(2019, 07, 01);
			tariff1.UE_DateTo = new ZDateTime(2020, 06, 30);
			var tariff2 = Factory.New<USCTariff>();
			tariff2.UE_Tariff = "1100010001";
			tariff2.UE_ShortDescription = "TEST TARIFF 2";
			tariff2.UE_DateFrom = new ZDateTime(2018, 07, 01);
			tariff2.UE_DateTo = new ZDateTime(2019, 06, 30);
			Factory.Save();
			var importTariff = InvoiceLine.DrawbackAdditionalImportTariffNumbers.AddNew();
			importTariff.US_FormattedTariff = "1100010001";
			AssertEquals(tariff1.UE_ShortDescription, importTariff.US_Description);
		}

		protected override BusinessObject GetNewBusinessObject() => new DrawbackAdditionalImportTariffNumberAddInfo(InvoiceLine.DrawbackAdditionalImportTariffNumbers.AddNew().B7_AddInfoDataInfo);

		JobComInvoiceLine invoiceLine;
		JobComInvoiceLine InvoiceLine
		{
			get
			{
				if (invoiceLine == null)
				{
					var declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
					declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
					var invoice = declaration.Invoices.AddNew();
					invoiceLine = invoice.JobComInvoiceLines.AddNew();
				}
				return invoiceLine;
			}
		}
	}
}
