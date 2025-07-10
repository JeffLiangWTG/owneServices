using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.DocumentWrappers.Customs.Base.Testing;
using Enterprise.DocumentWrappersCore.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.DocumentWrappers.Testing
{
	[TestedType(typeof(DocJobComInvoiceHeader))]
	sealed class DocJobComInvoiceHeaderTest : DocBaseJobComInvoiceHeaderAbstractTest<JobComInvoiceHeader, DocJobComInvoiceHeader>
	{
		#region Overrides

		public override void TestTariffHeading()
		{
			AssertEquals("TariffHeader", "Tariff / Rebate / Trade Agreement / Sch 5/6 / Sch 2 / Sch 1 P 2A/2B", invoiceHeaderWrapper.TariffHeading);
		}

		public override void TestIncoTermDescription()
		{
			invoiceHeader.JZ_IncoTerm = "EXW";
			Assert("Inco term should be not be empty", invoiceHeaderWrapper.IncoTermDescription != ZString.Empty);

			invoiceHeader.JZ_IncoTerm = "CIF";
			Assert("Inco term should be not be empty", invoiceHeaderWrapper.IncoTermDescription != ZString.Empty);
		}

		#endregion

		#region ZDecimal Fields
		public void TestTotalFreightIsWrapped()
		{
			invoiceHeader.Charges.RemoveAll();
			invoiceHeader.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 100m, GlbCompany.CurrentCompany.Country.RN_RX_NKLocalCurrency);
			AssertEquals(invoiceHeaderWrapper.TotalOverseasFreight, 100m);
		}

		public void TestTotalInsuranceIsWrapped()
		{
			invoiceHeader.Charges.RemoveAll();
			invoiceHeader.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 100m, GlbCompany.CurrentCompany.Country.RN_RX_NKLocalCurrency);
			AssertEquals(invoiceHeaderWrapper.TotalOverseasInsurance, 100m);
		}

		public void TestInvoiceAmountWithExchangeRateIsWrapped()
		{
			var zARCurrency = Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, SQLComparisonOperator.Equal, "ZAR"));
			var aUDCurrency = Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, SQLComparisonOperator.Equal, "AUD"));

			invoiceHeader.JZ_InvoiceAmount = 150.00M;
			invoiceHeader.JZ_RX_NKInvoice_Currency = aUDCurrency.RX_Code;
			ZDecimal exchangeRate = CurrencyConverter.GetExchangeRate(aUDCurrency);

			AssertEquals("InvoiceAmountWithExchangeRate For AUD", "AUD 150.00 @ = " + exchangeRate.ToString(), invoiceHeaderWrapper.InvoiceAmountWithExchangeRate);

			invoiceHeader.JZ_RX_NKInvoice_Currency = zARCurrency.RX_Code;
			AssertEquals("InvoiceAmountWithExchangeRate For ZAR", ZString.Empty, invoiceHeaderWrapper.InvoiceAmountWithExchangeRate);
		}

		public override void TestConversionFactorIsWrapped()
		{
			InvoiceHeaderInternal.JZ_InvoiceAmount = 1000m;
			InvoiceHeaderInternal.JZ_RX_NKInvoice_Currency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			var invoiceLine = InvoiceHeaderInternal.InvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 1000m;
			AssertEquals(1m, InvoiceHeaderWrapperInternal.ConversionFactor);
		}
		#endregion

		public void TestSetTemplateConstants()
		{
			DocJobComInvoiceHeader wrapper = invoiceHeaderWrapper;
			AssertEquals("MarksAndNumbersWidth", 1, wrapper.MarksAndNumbersWidth);
			AssertEquals("GoodsDescriptionWidth", 1, wrapper.GoodsDescWidth);
			AssertEquals("MarksAndNumbersAndGoodsDescriptionHeight", 1, wrapper.MarksAndNumbsAndDescHeight);
			AssertEquals("NumberOfContainerRows", 1, wrapper.NoOfContainerRows);

			Dictionary<string, object> constants = new Dictionary<string, object>();
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.MarksAndNumbersWidth, 2);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.GoodsDescriptionWidth, 8);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.MarksAndNumbersAndGoodsDescriptionHeight, 13);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.NumberOfContainerRows, 17);

			wrapper.SetTemplateConstants(constants);
			AssertEquals("MarksAndNumbersWidth", 2, wrapper.MarksAndNumbersWidth);
			AssertEquals("GoodsDescriptionWidth", 8, wrapper.GoodsDescWidth);
			AssertEquals("MarksAndNumbersAndGoodsDescriptionHeight", 13, wrapper.MarksAndNumbsAndDescHeight);
			AssertEquals("NumberOfContainerRows", 17, wrapper.NoOfContainerRows);
		}

		public void TestSpiltDate()
		{
			ZString currentDate = ZDateTime.Today.Day.ToString().PadLeft(2, '0');
			ZString currentMonth = ZDateTime.Today.Month.ToString().PadLeft(2, '0');
			ZString currentYear = ZDateTime.Today.Year.ToString();
			ZString expectedResult = currentDate + currentMonth + currentYear;

			AssertEquals("Year Char 1", expectedResult[4].ToString(), invoiceHeaderWrapper.YearChar1);
			AssertEquals("Year Char 2", expectedResult[5].ToString(), invoiceHeaderWrapper.YearChar2);
			AssertEquals("Year Char 3", expectedResult[6].ToString(), invoiceHeaderWrapper.YearChar3);
			AssertEquals("Year Char 4", expectedResult[7].ToString(), invoiceHeaderWrapper.YearChar4);
			AssertEquals("Month Char 1", expectedResult[2].ToString(), invoiceHeaderWrapper.MonthChar1);
			AssertEquals("Month Char 2", expectedResult[3].ToString(), invoiceHeaderWrapper.MonthChar2);
			AssertEquals("Day Char 1", expectedResult[0].ToString(), invoiceHeaderWrapper.DayChar1);
			AssertEquals("Day Char 2", expectedResult[1].ToString(), invoiceHeaderWrapper.DayChar2);
		}

		public void TestTotalCIFAndCInLocalCurrency()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			invoiceHeader.JZ_InvoiceAmount = 1000m;
			invoiceHeader.JobComInvoiceLines.AddNew().JI_LinePrice = 1000m;

			DocJobComInvoiceHeader invoiceHeaderWrapper = DocJobComInvoiceHeader.New(invoiceHeader, Factory);
			AssertEquals("Precondition : InvoiceHeader.JZ_Calc_CIF_C_InLocalCurrency should return the expext value first", 1000m, invoiceHeader.JZ_Calc_CIFAmount_InLocalCurrency);
			AssertEquals(invoiceHeader.JZ_Calc_CIFAmount_InLocalCurrency, invoiceHeaderWrapper.TotalCIFInLocalCurrency);
		}

		public void TestConversionFactorAsString()
		{
			InvoiceHeaderInternal.JZ_InvoiceAmount = 1000m;
			InvoiceHeaderInternal.JZ_RX_NKInvoice_Currency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			var invoiceLine = InvoiceHeaderInternal.InvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 333m;
			AssertEquals("1", InvoiceHeaderWrapperInternal.ConversionFactorAsString);
		}

		#region Implementation

		protected override string TestingCountry
		{
			get { return Enterprise.Core.Constants.CountryCodes.SouthAfrica; }
		}

		JobComInvoiceHeader invoiceHeader;
		DocJobComInvoiceHeader invoiceHeaderWrapper;

		protected override void SetUp()
		{
			base.SetUp();
			invoiceHeader = InvoiceHeaderInternal;
			invoiceHeaderWrapper = InvoiceHeaderWrapperInternal;
		}

		CurrencyConverter CurrencyConverter
		{
			get { return invoiceHeader.CurrencyConverter; }
		}

		protected override DocJobComInvoiceHeader CreateInvoiceHeaderWrapper(JobComInvoiceHeader invoiceHeaderInternal)
		{
			return DocJobComInvoiceHeader.New(invoiceHeaderInternal, Factory);
		}

		#endregion
	}
}
