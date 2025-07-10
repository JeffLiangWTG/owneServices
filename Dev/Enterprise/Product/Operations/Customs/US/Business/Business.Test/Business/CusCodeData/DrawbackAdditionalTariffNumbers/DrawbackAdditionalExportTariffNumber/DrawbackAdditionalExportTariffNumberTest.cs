using System.Collections.Generic;
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(DrawbackAdditionalExportTariffNumber))]
	sealed class DrawbackAdditionalExportTariffNumberTest : Customs.Business.Testing.CusCodeDataTest<DrawbackAdditionalExportTariffNumber>
	{
		public void TestSetDefaultValues()
		{
			DrawbackAdditionalExportTariffNumber drawbackAdditionalExportTariff = (DrawbackAdditionalExportTariffNumber)GetNewBusinessObject();
			AssertEquals("default value", drawbackAdditionalExportTariff.CY_Type, CusCodeDataTypeList.Codes.DrawbackAdditionalExportTariffNumber);
			AssertEquals("default value", drawbackAdditionalExportTariff.CY_Code, CusCodeDataTypeList.Codes.DrawbackAdditionalExportTariffNumber);
		}

		public void TestGetsCorrectValidation()
		{
			DrawbackAdditionalExportTariffNumber drawbackAdditionalExportTariff = (DrawbackAdditionalExportTariffNumber)GetNewBusinessObject();
			AssertEquals("Correct validation", typeof(DrawbackAdditionalExportTariffNumberValidation), drawbackAdditionalExportTariff.Validation.GetType());
		}

		public void TestCY_FormattedTariff()
		{
			DrawbackAdditionalExportTariffNumber drawbackAdditionalExportTariff = (DrawbackAdditionalExportTariffNumber)GetNewBusinessObject();
			drawbackAdditionalExportTariff.CY_FormattedTariff = "1001101010";
			AssertEquals("1001.10.1010", drawbackAdditionalExportTariff.CY_FormattedTariff);
			AssertEquals("1001101010", drawbackAdditionalExportTariff.CY_Data);
			drawbackAdditionalExportTariff.CY_FormattedTariff = "1001.10.1010";
			AssertEquals("1001.10.1010", drawbackAdditionalExportTariff.CY_FormattedTariff);
			AssertEquals("1001101010", drawbackAdditionalExportTariff.CY_Data);
		}

		public void TestAdditionalTariffLineNumber()
		{
			var tariff1 = InvoiceLine.DrawbackAdditionalExportTariffNumbers.AddNew();
			tariff1.CY_FormattedTariff = "1";
			AssertEquals((short)1, tariff1.CY_Order);
			var tariff2 = InvoiceLine.DrawbackAdditionalExportTariffNumbers.AddNew();
			tariff2.CY_FormattedTariff = "2";
			AssertEquals((short)2, tariff2.CY_Order);
			var tariff3 = InvoiceLine.DrawbackAdditionalExportTariffNumbers.AddNew();
			tariff3.CY_FormattedTariff = "3";
			AssertEquals((short)3, tariff3.CY_Order);
			tariff2.Delete();
			AssertEquals((short)1, tariff1.CY_Order);
			AssertEquals((short)2, tariff3.CY_Order);
			var tariff4 = InvoiceLine.DrawbackAdditionalExportTariffNumbers.AddNew();
			tariff4.CY_FormattedTariff = "4";
			AssertEquals((short)3, tariff4.CY_Order);
		}

		public void TestExportTariffs()
		{
			InvoiceLine.US_TariffType = TariffTypeList.Codes.ScheduleB;
			var tariff1 = InvoiceLine.DrawbackAdditionalExportTariffNumbers.AddNew();
			AssertType<Universal.TariffViewCollection>("ScheduleBTariffs", tariff1.ExportTariffs);
			InvoiceLine.US_TariffType = TariffTypeList.Codes.HTS;
			AssertType<Universal.TariffViewCollection>("TariffViewCollection", tariff1.ExportTariffs);
		}

		protected override BusinessObject GetNewBusinessObject() => InvoiceLine.DrawbackAdditionalExportTariffNumbers.AddNew();

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => factory.NewWithValidTestData<DrawbackAdditionalExportTariffNumber>();

		protected override IEnumerable<DrawbackAdditionalExportTariffNumber> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var result = factory.NewWithValidTestData<DrawbackAdditionalExportTariffNumber>();
			var declaration = factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.DrawbackAdditionalExportTariffNumbers.Add(result);
			yield return result;
		}

		JobComInvoiceLine invoiceLine;
		JobComInvoiceLine InvoiceLine
		{
			get
			{
				if (invoiceLine == null)
				{
					var declaration = Factory.New<JobDeclaration>();
					var invoiceHeader = declaration.Invoices.AddNew();
					invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
				}

				return invoiceLine;
			}
		}
	}
}
