using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ZA.Business.MessageBuilders;
using Enterprise.Customs.ZA.Business.Testing;

namespace Enterprise.Customs.ZA.Business.DocumentWrappers.Testing
{
	class DA63LineDetailWrapperTest : TestCaseWithFactory
	{
		public void TestLineNumber()
		{
			entryLine.CL_LineNumber = 20;
			AssertEquals("0020", (entryLine as ILineLevelInformation).LineNumber);
			var tester = new DA63LineDetailWrapper(entryLine);
			AssertEquals("20", tester.LineNumber);
		}

		public void TestAlphaOfficeCode()
		{
			invoiceLine1.JI_PreviousEntryNumber = "AAAPREVIOUSMRN1";
			var tester = new DA63LineDetailWrapper(entryLine);
			AssertEquals("AAA", tester.AlphaOfficeCode);
		}

		public void TestDA63ImportTariffCode()
		{
			CombineAssertions(() =>
			{
				var universalTestHelper = new ZAUniversalReferenceTestDataHelper(Factory);
				var tariffType1P1 = universalTestHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, UniversalReferenceConstants.CusTariffCode.Schedule1Part1);
				Factory.Save();
				var startDate = new ZDateTime(1990, 1, 1);
				var endDate = new ZDateTime(2075, 1, 1);
				var tariff = universalTestHelper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType1P1.PK, "99999999", startDate, endDate);
				universalTestHelper.CreateTariffAttribute(UniversalReferenceConstants.TariffAttributes.CheckDigit, "1", tariff);
				Factory.Save();
				invoiceLine1.JI_ImportTariff = "99999999";
				invoiceLine2.JI_ImportTariff = "99999999";
				var tester = new DA63LineDetailWrapper(entryLine);
				AssertEquals("9999.99.99 (1)", tester.DA63ImportTariffCode);
				invoiceLine1.JI_ImportTariff = "";
				invoiceLine2.JI_ImportTariff = "";
				tester = new DA63LineDetailWrapper(entryLine);
				AssertEquals("", tester.DA63ImportTariffCode);
			});
		}

		public void TestDA63CustomsValue()
		{
			invoiceLine1.JI_ImportCustomsValue = 1.02;
			invoiceLine1.JI_ImportDutyPaid = 1.03;
			invoiceLine1.DA63AdditionalDuties.AddOrUpdate("12A", 4.04m);
			invoiceLine1.DA63AdditionalDuties.AddOrUpdate(DA63AdditionalDuty.S1P2BDuty, 1.04m);
			invoiceLine1.JI_ImportVATPaid = 1.05;
			invoiceLine1.DA63AdditionalDuties.AddOrUpdate(LineLevelProvisionalPayments.Codes.PPA, 1.06m);
			invoiceLine1.DA63AdditionalDuties.AddOrUpdate(LineLevelProvisionalPayments.Codes.PEN, 1.07m);
			invoiceLine1.JI_ImportCustomsQty = 1.04m;
			invoiceLine1.JI_ImportCustomsQtyUQ = string.Empty;
			invoiceLine1.JI_ImportCustomsQty2 = 2.04m;
			invoiceLine1.JI_ImportCustomsQty2UQ = string.Empty;
			invoiceLine1.JI_ImportCustomsQty3 = 3.04m;
			invoiceLine1.JI_ImportCustomsQty3UQ = string.Empty;
			invoiceLine2.JI_ImportCustomsValue = 2.02;
			invoiceLine2.JI_ImportDutyPaid = 2.03;
			invoiceLine2.DA63AdditionalDuties.AddOrUpdate(DA63AdditionalDuty.S1P2BDuty, 2.04m);
			invoiceLine2.JI_ImportVATPaid = 2.05;
			invoiceLine2.DA63AdditionalDuties.AddOrUpdate(LineLevelProvisionalPayments.Codes.PPA, 2.06m);
			invoiceLine2.DA63AdditionalDuties.AddOrUpdate(LineLevelProvisionalPayments.Codes.PEN, 2.07m);
			invoiceLine2.JI_ImportCustomsQty = 4.04m;
			invoiceLine2.JI_ImportCustomsQtyUQ = "KG";
			invoiceLine2.JI_ImportCustomsQty2 = 5.04m;
			invoiceLine2.JI_ImportCustomsQty2UQ = "KG";
			invoiceLine2.JI_ImportCustomsQty3 = 6.04m;
			invoiceLine2.JI_ImportCustomsQty3UQ = "KG";
			CombineAssertions(() =>
			{
				var tester = new DA63LineDetailWrapper(entryLine);
				AssertEquals(3.04m, tester.DA63CustomsValue);
				AssertEquals(3.06m, tester.DA63CustomsDutyExcluding12B);
				AssertEquals(3.08m, tester.DA63S1P2BDuty);
				AssertEquals(3.10m, tester.DA63ValueAddedTax);
				AssertEquals(25.80m, tester.DA63TotalAmountClaimed);
				AssertEquals(5.08m, tester.DA63CustomsQuantity);
				AssertEquals("KG", tester.DA63CustomsUnitQty);
				AssertEquals(7.08m, tester.DA63AdditionalQuantity1);
				AssertEquals("KG", tester.DA63AdditionalUnitQty1);
				AssertEquals(9.08m, tester.DA63AdditionalQuantity2);
				AssertEquals("KG", tester.DA63AdditionalUnitQty2);
				AssertEquals("12A", tester.FirstOtherDutyTaxType);
				AssertEquals("4.04", tester.FirstOtherDutyAmountStr);
				AssertEquals("PEN", tester.SecondOtherDutyTaxType);
				AssertEquals("3.14", tester.SecondOtherDutyAmountStr);
				AssertEquals("PPA", tester.ThirdOtherDutyTaxType);
				AssertEquals("3.12", tester.ThirdOtherDutyAmountStr);
				AssertEquals("", tester.FourthOtherDutyTaxType);
				AssertEquals("0.00", tester.FourthOtherDutyAmountStr);
				AssertEquals(10.30m, tester.TotalOtherAmount);
			});
		}

		protected override void SetUp()
		{
			ZAUniversalReferenceTestDataHelper.SetupBasicTariffTypesForZATesting(Factory);
			base.SetUp();
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceLine1 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryLine = entryHeader.MergedLines.AddNew();
			entryLine.InvoiceLines.Add(invoiceLine1);
			entryLine.InvoiceLines.Add(invoiceLine2);
		}

		CusEntryLine entryLine;
		JobComInvoiceLine invoiceLine1;
		JobComInvoiceLine invoiceLine2;
	}
}
