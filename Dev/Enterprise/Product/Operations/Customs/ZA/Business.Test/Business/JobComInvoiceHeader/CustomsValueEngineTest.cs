using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Integration.ZArchitecture;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using CustomsChargeTypeList = Enterprise.Customs.Business.CustomsChargeTypeList;

namespace Enterprise.Customs.ZA.Business.Testing
{
	sealed class CustomsValueEngineTest : TestCaseWithFactory
	{
		public void TestCalculatingConversionFactorUsingNewCalculator()
		{
			var helper = new TestHelper(Factory);
			CombineAssertions("CIF", () =>
			{
				var uSDCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "USD");
				helper.SetExchangeRate(uSDCurrency, 0.5m, ZDateTime.Now);
				var testDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
				var testInvoice = testDeclaration.Invoices.AddNew();
				var testInvoiceLine = testInvoice.InvoiceLines.AddNew();
				testInvoice.JZ_InvoiceAmount = 10000m;
				testInvoice.JZ_RX_NKInvoice_Currency = uSDCurrency.RX_Code;
				testInvoice.JZ_IncoTerm = "CIF";
				testInvoice.Charges.RemoveAll();
				BaseJobComInvHeaderCharge oFT = testInvoice.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 500m);
				oFT.J7_IsIncludedInITOT = true;
				testInvoice.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 50m);
				testDeclaration.ResumeApportionment();
				var line = testInvoice.InvoiceLines.AddNew();
				line.JI_LinePrice = 900;
				testDeclaration.ResumeApportionment();
				AssertEquals(0.88888889m, testInvoice.JZ_Calc_ConversionFactor);
				var conf = new ZACustomsValuationByFactorConfigurationForTesting();
				var newValuator = new CustomsValueByFactorCalculator(new HeaderCustomsValueCalculationDataProviderForTesting(testInvoice), conf);
				AssertEquals(0.88888889m, newValuator.CustomsFactor);
			});
			CombineAssertions("FOB", () =>
			{
				var uSDCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "USD");
				helper.SetExchangeRate(uSDCurrency, 0.5m, ZDateTime.Now);
				var testDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
				var testInvoice = testDeclaration.Invoices.AddNew();
				var testInvoiceLine = testInvoice.InvoiceLines.AddNew();
				testInvoice.JZ_InvoiceAmount = 10000m;
				testInvoice.JZ_RX_NKInvoice_Currency = uSDCurrency.RX_Code;
				testInvoice.JZ_IncoTerm = "FOB";
				testInvoice.Charges.RemoveAll();
				testInvoice.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 500m);
				testInvoice.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 50m);
				testDeclaration.ResumeApportionment();
				var line = testInvoice.InvoiceLines.AddNew();
				line.JI_LinePrice = 900;
				testDeclaration.ResumeApportionment();
				AssertEquals(2m, testInvoice.JZ_Calc_ConversionFactor);
				var conf = new ZACustomsValuationByFactorConfigurationForTesting();
				var newValuator = new CustomsValueByFactorCalculator(new HeaderCustomsValueCalculationDataProviderForTesting(testInvoice), conf);
				AssertEquals(2m, newValuator.CustomsFactor);
			});
			CombineAssertions("FOB with INT ON Line", () =>
			{
				var uSDCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "USD");
				helper.SetExchangeRate(uSDCurrency, 0.5m, ZDateTime.Now);
				var testDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
				var testInvoice = testDeclaration.Invoices.AddNew();
				var testInvoiceLine = testInvoice.InvoiceLines.AddNew();
				testInvoice.JZ_InvoiceAmount = 10000m;
				testInvoice.JZ_RX_NKInvoice_Currency = uSDCurrency.RX_Code;
				testInvoice.JZ_IncoTerm = "FOB";
				testInvoice.Charges.RemoveAll();
				testInvoice.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 500m);
				testInvoice.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 50m);
				testDeclaration.ResumeApportionment();
				var line = testInvoice.InvoiceLines.AddNew();
				line.JI_LinePrice = 2900;
				var lineINT = line.Charges.AddNew();
				lineINT.J7_ChargeType = "INT";
				lineINT.J7_Amount = 2000;
				testDeclaration.ResumeApportionment();
				AssertEquals("Old", 2m, testInvoice.JZ_Calc_ConversionFactor);
				var conf = new ZACustomsValuationByFactorConfigurationForTesting();
				var newValuator = new CustomsValueByFactorCalculator(new HeaderCustomsValueCalculationDataProviderForTesting(testInvoice), conf);
				AssertEquals("New", 2m, newValuator.CustomsFactor);
			});
			CombineAssertions("CIF IN LINE with INT", () =>
			{
				var uSDCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "USD");
				helper.SetExchangeRate(uSDCurrency, 0.5m, ZDateTime.Now);
				var testDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
				var testInvoice = testDeclaration.Invoices.AddNew();
				var testInvoiceLine = testInvoice.InvoiceLines.AddNew();
				testInvoice.JZ_InvoiceAmount = 10000m;
				testInvoice.JZ_RX_NKInvoice_Currency = uSDCurrency.RX_Code;
				testInvoice.JZ_IncoTerm = "CIF";
				testInvoice.Charges.RemoveAll();
				testInvoice.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 500m);
				testInvoice.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 50m);
				testDeclaration.ResumeApportionment();
				var line = testInvoice.InvoiceLines.AddNew();
				line.JI_LinePrice = 2900;
				var lineINT = line.Charges.AddNew();
				lineINT.J7_ChargeType = "INT";
				lineINT.J7_Amount = 2000;
				testDeclaration.ResumeApportionment();
				AssertEquals(2m, testInvoice.JZ_Calc_ConversionFactor);
				var conf = new ZACustomsValuationByFactorConfigurationForTesting();
				var newValuator = new CustomsValueByFactorCalculator(new HeaderCustomsValueCalculationDataProviderForTesting(testInvoice), conf);
				AssertEquals(2m, newValuator.CustomsFactor);
			});
		}

		[TestDate(2016, 01, 01)]
		public void TestCalculateInvoiceLineCustomsValueUsingNewCalculator()
		{
			CombineAssertions("FOB", () =>
			{
				var testDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
				testDeclaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
				var testInvoice = testDeclaration.Invoices.AddNew();
				testInvoice.JZ_IncoTerm = "FOB";
				testInvoice.JZ_InvoiceAmount = 400;
				testInvoice.JZ_RX_NKInvoice_Currency = "ZAR";
				var testInvoiceLine1 = testInvoice.InvoiceLines.AddNew();
				testInvoiceLine1.JI_LinePrice = 200m;
				var testInvoiceLine2 = testInvoice.InvoiceLines.AddNew();
				testInvoiceLine2.JI_LinePrice = 200m;
				var testCharge2 = testInvoiceLine2.Charges.AddNew();
				testCharge2.J7_ChargeType = "INT";
				testCharge2.J7_Amount = 20m;
				var testInvoiceLine3 = testInvoice.InvoiceLines.AddNew();
				testInvoiceLine3.JI_LinePrice = 200m;
				var testCharge3 = testInvoiceLine3.Charges.AddNew();
				testCharge3.J7_ChargeType = "DIS";
				testCharge3.J7_Amount = 20m;
				var testInvoiceLine4 = testInvoice.InvoiceLines.AddNew();
				testInvoiceLine4.JI_LinePrice = 200m;
				testInvoiceLine4.JI_ValuationMarkup = 10;
				var testInvoiceLine5 = testInvoice.InvoiceLines.AddNew();
				testInvoiceLine5.JI_LinePrice = 200m;
				testInvoiceLine5.JI_CustomsValueOverride = 100m;
				testInvoiceLine5.JI_RX_NKCustomsValueCurrencyOverride = "ZAR";
				testDeclaration.ResumeApportionment();
				AssertEquals(200m, testInvoiceLine1.JI_CustomsValue);
				AssertEquals(180m, testInvoiceLine2.JI_CustomsValue);
				AssertEquals(180m, testInvoiceLine3.JI_CustomsValue);
				var conf = new ZACustomsValuationByFactorConfigurationForTesting();
				var newValuator = new CustomsValueByFactorCalculator(new HeaderCustomsValueCalculationDataProviderForTesting(testInvoice), conf);
				AssertEquals(testInvoice.JZ_Calc_ConversionFactor, newValuator.CustomsFactor);
				AssertEquals("New 1", 200m, newValuator.GetCustomsValue(new LineCustomsValueCalculationDataProviderForTesting(testInvoiceLine1)));
				AssertEquals("New 2", 180m, newValuator.GetCustomsValue(new LineCustomsValueCalculationDataProviderForTesting(testInvoiceLine2)));
				AssertEquals("New 3", 180m, newValuator.GetCustomsValue(new LineCustomsValueCalculationDataProviderForTesting(testInvoiceLine3)));
			});
			CombineAssertions("CIF not in LINE", () =>
			{
				var testDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
				testDeclaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
				var testInvoice = testDeclaration.Invoices.AddNew();
				testInvoice.JZ_IncoTerm = "CIF";
				testInvoice.JZ_InvoiceAmount = 400;
				testInvoice.JZ_RX_NKInvoice_Currency = "ZAR";
				testInvoice.Charges.RemoveAndDeleteAll();
				var headerCharge = testInvoice.Charges.AddNew();
				headerCharge.J7_ChargeType = "ONS";
				headerCharge.J7_Amount = 100;
				var testInvoiceLine1 = testInvoice.InvoiceLines.AddNew();
				testInvoiceLine1.JI_LinePrice = 200m;
				var testInvoiceLine2 = testInvoice.InvoiceLines.AddNew();
				testInvoiceLine2.JI_LinePrice = 200m;
				var testCharge2 = testInvoiceLine2.Charges.AddNew();
				testCharge2.J7_ChargeType = "INT";
				testCharge2.J7_Amount = 20m;
				var testInvoiceLine3 = testInvoice.InvoiceLines.AddNew();
				testInvoiceLine3.JI_LinePrice = 200m;
				var testCharge3 = testInvoiceLine3.Charges.AddNew();
				testCharge3.J7_ChargeType = "DIS";
				testCharge3.J7_Amount = 20m;
				var testInvoiceLine4 = testInvoice.InvoiceLines.AddNew();
				testInvoiceLine4.JI_LinePrice = 100m;
				testInvoiceLine4.JI_ValuationMarkup = 10;
				var testInvoiceLine5 = testInvoice.InvoiceLines.AddNew();
				testInvoiceLine5.JI_LinePrice = 100m;
				testInvoiceLine5.JI_CustomsValueOverride = 50m;
				testInvoiceLine5.JI_RX_NKCustomsValueCurrencyOverride = "ZAR";
				testDeclaration.ResumeApportionment();
				var conf = new ZACustomsValuationByFactorConfigurationForTesting();
				var newValuator = new CustomsValueByFactorCalculator(new HeaderCustomsValueCalculationDataProviderForTesting(testInvoice), conf);
				AssertEquals("New 1", 200m, newValuator.GetCustomsValue(new LineCustomsValueCalculationDataProviderForTesting(testInvoiceLine1)));
				AssertEquals("New 2", 180m, newValuator.GetCustomsValue(new LineCustomsValueCalculationDataProviderForTesting(testInvoiceLine2)));
				AssertEquals("New 3", 180m, newValuator.GetCustomsValue(new LineCustomsValueCalculationDataProviderForTesting(testInvoiceLine3)));
			});
			CombineAssertions("CIF in LINE", () =>
			{
				var testDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
				testDeclaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
				var testInvoice = testDeclaration.Invoices.AddNew();
				testInvoice.JZ_IncoTerm = "CIF";
				testInvoice.JZ_InvoiceAmount = 400;
				testInvoice.JZ_RX_NKInvoice_Currency = "ZAR";
				testInvoice.Charges.RemoveAndDeleteAll();
				var headerCharge = testInvoice.Charges.AddNew();
				headerCharge.J7_ChargeType = "ONS";
				headerCharge.J7_Amount = 100;
				headerCharge.J7_IsIncludedInITOT = true;
				var testInvoiceLine1 = testInvoice.InvoiceLines.AddNew();
				testInvoiceLine1.JI_LinePrice = 200m;
				var testInvoiceLine2 = testInvoice.InvoiceLines.AddNew();
				testInvoiceLine2.JI_LinePrice = 200m;
				var testCharge2 = testInvoiceLine2.Charges.AddNew();
				testCharge2.J7_ChargeType = "INT";
				testCharge2.J7_Amount = 20m;
				var testInvoiceLine3 = testInvoice.InvoiceLines.AddNew();
				testInvoiceLine3.JI_LinePrice = 200m;
				var testCharge3 = testInvoiceLine3.Charges.AddNew();
				testCharge3.J7_ChargeType = "DIS";
				testCharge3.J7_Amount = 20m;
				var testInvoiceLine4 = testInvoice.InvoiceLines.AddNew();
				testInvoiceLine4.JI_LinePrice = 100m;
				testInvoiceLine4.JI_ValuationMarkup = 10;
				var testInvoiceLine5 = testInvoice.InvoiceLines.AddNew();
				testInvoiceLine5.JI_LinePrice = 100m;
				testInvoiceLine5.JI_CustomsValueOverride = 50m;
				testInvoiceLine5.JI_RX_NKCustomsValueCurrencyOverride = "ZAR";
				testDeclaration.ResumeApportionment();
				AssertEquals("Old 1", 174.358974m, testInvoiceLine1.JI_CustomsValue);
				AssertEquals("Old 2", 156.9230766m, testInvoiceLine2.JI_CustomsValue);
				AssertEquals("Old 3", 156.9230766m, testInvoiceLine3.JI_CustomsValue);
				var conf = new ZACustomsValuationByFactorConfigurationForTesting();
				var newValuator = new CustomsValueByFactorCalculator(new HeaderCustomsValueCalculationDataProviderForTesting(testInvoice), conf);
				AssertEquals(0.87179487m, newValuator.CustomsFactor);
				AssertEquals("New 1", 174.358974m, newValuator.GetCustomsValue(new LineCustomsValueCalculationDataProviderForTesting(testInvoiceLine1)));
				AssertEquals("New 2", 156.9230766m, newValuator.GetCustomsValue(new LineCustomsValueCalculationDataProviderForTesting(testInvoiceLine2)));
				AssertEquals("New 3", 156.9230766m, newValuator.GetCustomsValue(new LineCustomsValueCalculationDataProviderForTesting(testInvoiceLine3)));
			});
			CombineAssertions("FOB with Dutiable Charges", () =>
			{
				var testDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
				testDeclaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
				var testInvoice = testDeclaration.Invoices.AddNew();
				testInvoice.JZ_IncoTerm = "FOB";
				testInvoice.JZ_InvoiceAmount = 400;
				testInvoice.JZ_RX_NKInvoice_Currency = "ZAR";
				var testInvoiceLine1 = testInvoice.InvoiceLines.AddNew();
				testInvoiceLine1.JI_LinePrice = 200m;
				var testCharge1 = testInvoice.Charges.AddNew();
				testCharge1.J7_ChargeType = CustomsChargeTypeList.Codes.Commission;
				testCharge1.J7_Amount = 21m;
				testCharge1.J7_IsDutiable = true;
				testCharge1.J7_IsIncludedInITOT = false;
				var testCharge2 = testInvoice.Charges.AddNew();
				testCharge2.J7_ChargeType = CustomsChargeTypeList.Codes.ForeignInlandFreight;
				testCharge2.J7_Amount = 22m;
				testCharge2.J7_IsDutiable = true;
				testCharge2.J7_IsIncludedInITOT = true;
				var testCharge3 = testInvoice.Charges.AddNew();
				testCharge3.J7_ChargeType = CustomsChargeTypeList.Codes.LandingCharges;
				testCharge3.J7_Amount = 23m;
				testCharge3.J7_IsDutiable = false;
				testCharge3.J7_IsIncludedInITOT = true;
				var testCharge4 = testInvoice.Charges.AddNew();
				testCharge4.J7_ChargeType = CustomsChargeTypeList.Codes.OtherCharges;
				testCharge4.J7_Amount = 24m;
				testCharge4.J7_IsDutiable = false;
				testCharge4.J7_IsIncludedInITOT = false;
				testDeclaration.ResumeApportionment();
				AssertEquals("old", 0.99m, testInvoice.JZ_Calc_ConversionFactor);
				AssertEquals("old", 198m, testInvoiceLine1.JI_CustomsValue);
				var conf = new ZACustomsValuationByFactorConfigurationForTesting();
				var newValuator = new CustomsValueByFactorCalculator(new HeaderCustomsValueCalculationDataProviderForTesting(testInvoice), conf);
				AssertEquals("new", 0.99m, newValuator.CustomsFactor);
				AssertEquals("new", 198m, newValuator.GetCustomsValue(new LineCustomsValueCalculationDataProviderForTesting(testInvoiceLine1)));
			});
		}
	}

	sealed class LineCustomsValueCalculationDataProviderForTesting : ICustomsValueCalculationDataProvider
	{
		public LineCustomsValueCalculationDataProviderForTesting(BaseJobComInvoiceLine line)
		{
			this.line = line;
		}

		internal BaseJobComInvoiceLine line;
		public CurrencyConverter CurrencyConverter
		{
			get
			{
				return line.CurrencyConverter;
			}
		}

		public ZDecimal LinePriceAmount => line.JI_LinePrice;
		public IEnumerable<JobComInvCharge> AllCharges
		{
			get
			{
				var result = Enumerable.Empty<JobComInvCharge>();
				var chargeApportionee = (line as IChargeApportionee);
				if (chargeApportionee != null)
				{
					result = result.Union(chargeApportionee.Charges.OfType<JobComInvCharge>());
					result = result.Union(chargeApportionee.ApportionedCharges.OfType<JobComInvCharge>());
				}

				return result;
			}
		}
	}

	sealed class HeaderCustomsValueCalculationDataProviderForTesting : ICustomsValueCalculationDataProviderForInvoiceHeader
	{
		public HeaderCustomsValueCalculationDataProviderForTesting(BaseJobComInvoiceHeader header)
		{
			this.header = header;
		}

		readonly BaseJobComInvoiceHeader header;
		public CurrencyConverter CurrencyConverter => header.CurrencyConverter;
		public IEnumerable<JobComInvCharge> AllCharges
		{
			get
			{
				var result = Enumerable.Empty<JobComInvCharge>();
				var chargeApportionee = (header as IChargeApportionee);
				if (chargeApportionee != null)
				{
					result = result.Union(chargeApportionee.Charges.OfType<JobComInvCharge>());
					result = result.Union(chargeApportionee.ApportionedCharges.OfType<JobComInvCharge>());
				}

				return result;
			}
		}

		public ICurrency InvoiceCurrency
		{
			get
			{
				return header.Invoice_Currency;
			}
		}

		public ZDecimal LinePriceAmount
		{
			get
			{
				return header.InvoiceLines.OfType<JobComInvoiceLine>().Sum(line => line.JI_LinePrice);
			}
		}
	}

	sealed class ZACustomsValuationByFactorConfigurationForTesting : CustomsValuationByFactorConfiguration
	{
		public override ZInt NumberOfDecimalPlacesForCustomsFactor => 8;
		public ZACustomsValuationByFactorConfigurationForTesting()
		{
		}

		public override bool ApplyDiscountBeforeFactorCalculation => true;
		public override IEnumerable<string> LineLevelCharges
		{
			get
			{
				if (lineLevelCharges == null)
				{
					lineLevelCharges = new string[] { InvoiceLineCustomsChargeTypeList.Codes.IntellectualValue };
				}

				return lineLevelCharges;
			}
		}

		IEnumerable<string> lineLevelCharges;
		public override ICurrency LocalCurrency => GlbCompany.CurrentCompany.LocalCurrency;
	}
}
