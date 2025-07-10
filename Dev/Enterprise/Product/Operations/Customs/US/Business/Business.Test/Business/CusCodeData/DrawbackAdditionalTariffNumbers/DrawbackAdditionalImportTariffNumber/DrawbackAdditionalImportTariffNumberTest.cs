using System.Collections.Generic;
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(DrawbackAdditionalImportTariffNumber))]
	sealed class DrawbackAdditionalImportTariffNumberTest : Customs.Business.MultiLineAddInfos.Testing.CusAddInfoTest<DrawbackAdditionalImportTariffNumber>
	{
		public void TestUS_FormattedTariff()
		{
			var importTariff = InvoiceLine.DrawbackAdditionalImportTariffNumbers.AddNew();
			importTariff.US_FormattedTariff = "1100000001";
			AssertEquals("1100.00.0001", importTariff.US_FormattedTariff);
			importTariff.US_FormattedTariff = "1100.00.0002";
			AssertEquals("1100.00.0002", importTariff.US_FormattedTariff);
			importTariff.US_Tariff = "1100000003";
			AssertEquals("1100.00.0003", importTariff.US_FormattedTariff);
		}

		public void TestAdditionalTariffLineNumber()
		{
			var tariff1 = InvoiceLine.DrawbackAdditionalImportTariffNumbers.AddNew();
			tariff1.US_Tariff = "1";
			AssertEquals(1, tariff1.US_LineNo);
			var tariff2 = InvoiceLine.DrawbackAdditionalImportTariffNumbers.AddNew();
			tariff2.US_Tariff = "2";
			AssertEquals(2, tariff2.US_LineNo);
			var tariff3 = InvoiceLine.DrawbackAdditionalImportTariffNumbers.AddNew();
			tariff3.US_Tariff = "3";
			AssertEquals(3, tariff3.US_LineNo);
			tariff2.Delete();
			AssertEquals(1, tariff1.US_LineNo);
			AssertEquals(2, tariff3.US_LineNo);
			var tariff4 = InvoiceLine.DrawbackAdditionalImportTariffNumbers.AddNew();
			tariff4.US_Tariff = "4";
			AssertEquals(3, tariff4.US_LineNo);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var tariff = InvoiceLine.DrawbackAdditionalImportTariffNumbers.AddNew();
			tariff.US_Tariff = "1100010001";
			return tariff;
		}

		protected override IEnumerable<DrawbackAdditionalImportTariffNumber> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			yield return factory.New<JobDeclaration>().Invoices.AddNew().JobComInvoiceLines.AddNew().DrawbackAdditionalImportTariffNumbers.AddNew();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var result = (DrawbackAdditionalImportTariffNumber)base.GetNewBusinessObjectForDeleteTest(factory);
			result.US_Tariff = "1100010001";
			return result;
		}

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
					var invoiceHeader = declaration.Invoices.AddNew();
					invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
				}

				return invoiceLine;
			}
		}
	}
}
