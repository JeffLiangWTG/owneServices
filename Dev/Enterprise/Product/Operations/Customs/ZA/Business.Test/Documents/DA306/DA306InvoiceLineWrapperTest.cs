using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ZA.Business.Testing;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using static Enterprise.Customs.ZA.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.ZA.Business.Documents.DocDataObjects.Testing
{
	[TestedType(typeof(DA306InvoiceLineWrapper))]
	class DA306InvoiceLineWrapperTest : TestCaseWithFactory
	{
		public void TestCountryOfOrigin()
		{
			invoiceLine.JI_CountryOfOrigin = CountryCodes.SouthAfrica;
			AssertEquals(CountryCodes.SouthAfrica, wrapper.CountryOfOrigin);
		}

		public void TestInvoiceQuantity()
		{
			invoiceLine.JI_InvoiceQuantity = 123.4;
			AssertEquals(123.4m, wrapper.InvoiceQuantity);
		}

		public void TestInvoiceQuantityUnit()
		{
			invoiceLine.JI_InvoiceUQ = PkgUnit.Unit;
			AssertEquals(PkgUnit.Unit, wrapper.InvoiceQuantityUnit);
		}

		public void TestWeight()
		{
			invoiceLine.JI_Weight = 123.4m;
			AssertEquals(123.4m, wrapper.Weight);
		}

		public void TestWeightUnit()
		{
			invoiceLine.JI_WeightUQ = Weight.Kilograms;
			AssertEquals(Weight.Kilograms, wrapper.WeightUnit);
		}

		public void TestDescription()
		{
			invoiceLine.JI_Description = "ITEM 1";
			AssertEquals("ITEM 1", wrapper.Description);
		}

		public void TestHarmonisedCode()
		{
			invoiceLine.JI_FormattedTariff = "12345678";
			AssertEquals("1234.56.78", wrapper.HarmonisedCode);
		}

		public void TestLinePrice()
		{
			invoiceLine.JI_LinePrice = 123.4m;
			AssertEquals(123.4m, wrapper.LinePrice);
		}

		public void TestInvoiceCurrencyCode()
		{
			AssertEquals(CurrencyCodes.SouthAfrica, wrapper.InvoiceCurrencyCode);
		}

		public void TestValueInRand()
		{
			invoiceLine.JI_LinePrice = 123.4m;
			AssertEquals(123.4m, wrapper.ValueInRand);
		}

		public void TestCustomsDuty()
		{
			invoiceLineMock.Setup(line => line.JI_Calc_DutyAmountIncludingWHEstimate).Returns(123.4m);
			AssertEquals(123.4m, wrapper.CustomsDuty);
		}

		public void TestATV()
		{
			var testHelper = new ZAUniversalReferenceTestDataHelper(Factory, setupBasicTariffData: false);
			testHelper.CreateTaxOrFee(TaxOrFeeTypeCode.VAT, 0.1, CountryCodes.SouthAfrica);
			Factory.Save();

			invoiceLine.JI_ZZF_NKTaxType = TaxOrFeeTypeCode.VAT;
			invoiceLineMock.Protected().Setup<ZDecimal>("GetGSTVATAmountCore").Returns(123.4m);
			AssertEquals(1234m, wrapper.ATV);
		}

		public void TestVAT()
		{
			invoiceLineMock.Setup(line => line.JI_Calc_GSTVATAmountIncludingWHEstimate).Returns(123.4m);
			AssertEquals(123.4m, wrapper.VAT);
		}

		public void TestTotalPayable()
		{
			invoiceLineMock.Setup(line => line.JI_Calc_DutyAmountIncludingWHEstimate).Returns(123.4m);
			invoiceLineMock.Setup(line => line.JI_Calc_GSTVATAmountIncludingWHEstimate).Returns(234.5m);
			AssertEquals(357.9m, wrapper.TotalPayable);
		}

		JobComInvoiceLine invoiceLine;
		Mock<JobComInvoiceLine> invoiceLineMock;
		DA306InvoiceLineWrapper wrapper;

		protected override void SetUp()
		{
			base.SetUp();
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = CurrencyCodes.SouthAfrica;
			invoiceLineMock = Factory.NewMoq<JobComInvoiceLine>();
			invoiceLine = invoiceLineMock.Object;
			invoiceLine.JI_JZ = invoice.PK;
			invoice.InvoiceLines.Add(invoiceLine);
			wrapper = new DA306InvoiceLineWrapper(invoiceLine);
		}
	}
}
