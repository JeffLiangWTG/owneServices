using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(EntryLineUniversalRate))]
	sealed class EntryLineUniversalRateTest : EntryLineUniversalRateAbstractTest<EntryLineUniversalRate>
	{
		[TestDate(2021, 01, 02)]
		public void TestEntryLineUniversalRate()
		{
			var entryHeader = GetExportCostAndInsuranceEntryHeaderTestCase();
			var entryLine = entryHeader.MergedLines.Cast<CusEntryLine>().FirstOrDefault(x => x.CL_EntryLineQty == 32);
			var universalRate = new EntryLineUniversalRate(entryLine);
			AssertEquals(1931630m, universalRate.CustomsValue);
			AssertEquals(1931630m, universalRate.ValueForDuty);
			universalRate.CustomsValueFormula = "CV / 2";
			AssertEquals("CustomsValue", 1931630m, universalRate.CustomsValue);
			AssertEquals("ValueForDuty", 965815m, universalRate.ValueForDuty);
			universalRate.CustomsValueFormula = "CV * 1.5";
			AssertEquals("CustomsValue", 1931630m, universalRate.CustomsValue);
			AssertEquals("ValueForDuty", 2897445m, universalRate.ValueForDuty);
			universalRate.CustomsValueFormula = "CV / 3";
			AssertEquals("ValueForDuty 3 decimal places", 643876.667m, universalRate.ValueForDuty);
		}

		CusEntryHeader GetExportCostAndInsuranceEntryHeaderTestCase()
		{
			GlbCompany.CurrentCompany.GC_IsReciprocal = true;
			CurrencyConverterTestHelper.SetExchangeRate(Factory, Core.Constants.CurrencyCodes.UnitedStates, 30.13m, new ZDateTime(2021, 01, 02), Core.Constants.ExchangeRateTypes.Code.CustomsRateSecondary);
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			invoice.JZ_InvoiceAmount = 345241.02m;
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.CostAndInsurance;
			var charges = invoice.Charges;
			charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OverseasFreight, 18898.00m, Core.Constants.CurrencyCodes.UnitedStates);
			var insurance = charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OverseasInsurance, 250.80m, Core.Constants.CurrencyCodes.UnitedStates);
			insurance.J7_IsIncludedInITOT = true;
			charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.AdditionCharge, 18700.00m, Core.Constants.CurrencyCodes.UnitedStates);
			charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.DeductionCharge, 2839.02, Core.Constants.CurrencyCodes.UnitedStates);
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_InvoiceQuantity = 32;
			invoiceLine1.JI_InvoiceUQ = "PCE";
			invoiceLine1.JI_EnteredUnitPrice = 2004.89m;
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_InvoiceQuantity = 24;
			invoiceLine2.JI_InvoiceUQ = "PCE";
			invoiceLine2.JI_EnteredUnitPrice = 2426.68m;
			var invoiceLine3 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine3.JI_InvoiceQuantity = 16;
			invoiceLine3.JI_InvoiceUQ = "PCE";
			invoiceLine3.JI_EnteredUnitPrice = 3605.33m;
			var invoiceLine4 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine4.JI_InvoiceQuantity = 3;
			invoiceLine4.JI_InvoiceUQ = "PCE";
			invoiceLine4.JI_EnteredUnitPrice = 7395.55m;
			var invoiceLine5 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine5.JI_InvoiceQuantity = 3;
			invoiceLine5.JI_InvoiceUQ = "PCE";
			invoiceLine5.JI_EnteredUnitPrice = 15282.23m;
			var invoiceLine6 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine6.JI_InvoiceQuantity = 6;
			invoiceLine6.JI_InvoiceUQ = "PCE";
			invoiceLine6.JI_EnteredUnitPrice = 2205.37m;
			var invoiceLine7 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine7.JI_InvoiceQuantity = 6;
			invoiceLine7.JI_InvoiceUQ = "PCE";
			invoiceLine7.JI_EnteredUnitPrice = 13982.23m;
			declaration.ResumeApportionment();
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer(true));
			Factory.Save();
			var entryHeader = declaration.CustomsEntryHeaders[0];
			return entryHeader;
		}

		public void TestUnitOfMeasureValueListCalculation()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceLine1 = declaration.Invoices.AddNew().InvoiceLines.AddNew() as JobComInvoiceLine;
			var entryLine = declaration.ActiveEntryHeaders.AddNew().MergedLines.AddNew();
			invoiceLine1.JI_CL = entryLine.PK;
			invoiceLine1.JI_CustomsQuantity = 1m;
			invoiceLine1.JI_CustomsUnitQty = "KGM";
			var universalRate = new EntryLineUniversalRate(entryLine);
			var unitOfMeasureValueList = universalRate.UnitOfMeasureValueList;
			AssertEquals(true, unitOfMeasureValueList.ContainsKey("KGM"));
			AssertEquals(1m, unitOfMeasureValueList["KGM"]);
			AssertEquals(true, unitOfMeasureValueList.ContainsKey("TNE"));
			AssertEquals(0.001m, unitOfMeasureValueList["TNE"]);
			invoiceLine1.JI_CustomsSecondQuantity = 2000m;
			invoiceLine1.JI_CustomsSecondUnitQty = "LTR";
			universalRate = new EntryLineUniversalRate(entryLine);
			unitOfMeasureValueList = universalRate.UnitOfMeasureValueList;
			AssertEquals(true, unitOfMeasureValueList.ContainsKey("KGM"));
			AssertEquals(1m, unitOfMeasureValueList["KGM"]);
			AssertEquals(true, unitOfMeasureValueList.ContainsKey("LTR"));
			AssertEquals(2000m, unitOfMeasureValueList["LTR"]);
			AssertEquals(true, unitOfMeasureValueList.ContainsKey("TNE"));
			AssertEquals(0.001m, unitOfMeasureValueList["TNE"]);
			AssertEquals(true, unitOfMeasureValueList.ContainsKey("KLT"));
			AssertEquals(2m, unitOfMeasureValueList["KLT"]);
			invoiceLine1.JI_CustomsQuantity = 1000m;
			invoiceLine1.JI_CustomsSecondQuantity = 2000m;
			invoiceLine1.JI_CustomsSecondUnitQty = "KGM";
			universalRate = new EntryLineUniversalRate(entryLine);
			unitOfMeasureValueList = universalRate.UnitOfMeasureValueList;
			AssertEquals(true, unitOfMeasureValueList.ContainsKey("KGM"));
			AssertEquals(1000m, unitOfMeasureValueList["KGM"]);
			AssertEquals(false, unitOfMeasureValueList.ContainsKey("LTR"));
			AssertEquals(true, unitOfMeasureValueList.ContainsKey("TNE"));
			AssertEquals(1m, unitOfMeasureValueList["TNE"]);
			AssertEquals(false, unitOfMeasureValueList.ContainsKey("KLT"));
			invoiceLine1.JI_CustomsSecondUnitQty = "KSK";
			invoiceLine1.JI_CustomsQuantity = 1200m;
			universalRate = new EntryLineUniversalRate(entryLine);
			unitOfMeasureValueList = universalRate.UnitOfMeasureValueList;
			AssertEquals(true, unitOfMeasureValueList.ContainsKey("KGM"));
			AssertEquals(1200m, unitOfMeasureValueList["KGM"]);
			AssertEquals(true, unitOfMeasureValueList.ContainsKey("KSK"));
			AssertEquals(2000m, unitOfMeasureValueList["KSK"]);
			AssertEquals(true, unitOfMeasureValueList.ContainsKey("TNE"));
			AssertEquals(1.2m, unitOfMeasureValueList["TNE"]);
			invoiceLine1.JI_CustomsSecondUnitQty = "TNE";
			universalRate = new EntryLineUniversalRate(entryLine);
			unitOfMeasureValueList = universalRate.UnitOfMeasureValueList;
			AssertEquals(true, unitOfMeasureValueList.ContainsKey("KGM"));
			AssertEquals(1200m, unitOfMeasureValueList["KGM"]);
			AssertEquals(false, unitOfMeasureValueList.ContainsKey("KSK"));
			AssertEquals(true, unitOfMeasureValueList.ContainsKey("TNE"));
			AssertEquals(2000m, unitOfMeasureValueList["TNE"]);
			invoiceLine1.JI_CustomsUnitQty = "KSK";
			invoiceLine1.JI_CustomsSecondQuantity = 2000.1m;
			universalRate = new EntryLineUniversalRate(entryLine);
			unitOfMeasureValueList = universalRate.UnitOfMeasureValueList;
			AssertEquals(true, unitOfMeasureValueList.ContainsKey("KGM"));
			AssertEquals(2000100m, unitOfMeasureValueList["KGM"]);
			AssertEquals(true, unitOfMeasureValueList.ContainsKey("KSK"));
			AssertEquals(true, unitOfMeasureValueList.ContainsKey("TNE"));
			AssertEquals(2000.1m, unitOfMeasureValueList["TNE"]);
			var invoiceLine2 = declaration.Invoices.AddNew().InvoiceLines.AddNew() as JobComInvoiceLine;
			invoiceLine2.JI_CL = entryLine.PK;
			entryLine.InvoiceLines.Add(invoiceLine2);
			invoiceLine2.JI_CustomsQuantity = 3000m;
			invoiceLine2.JI_CustomsUnitQty = "KGM";
			universalRate = new EntryLineUniversalRate(entryLine);
			unitOfMeasureValueList = universalRate.UnitOfMeasureValueList;
			AssertEquals(true, unitOfMeasureValueList.ContainsKey("KGM"));
			AssertEquals(2003100m, unitOfMeasureValueList["KGM"]);
			AssertEquals(true, unitOfMeasureValueList.ContainsKey("TNE"));
			AssertEquals(2003.1m, unitOfMeasureValueList["TNE"]);
			AssertEquals(true, unitOfMeasureValueList.ContainsKey("KSK"));
			invoiceLine2.JI_CustomsSecondQuantity = 1000m;
			invoiceLine2.JI_CustomsSecondUnitQty = "KGM";
			universalRate = new EntryLineUniversalRate(entryLine);
			unitOfMeasureValueList = universalRate.UnitOfMeasureValueList;
			AssertEquals(true, unitOfMeasureValueList.ContainsKey("KGM"));
			AssertEquals(2003100m, unitOfMeasureValueList["KGM"]);
			AssertEquals(true, unitOfMeasureValueList.ContainsKey("TNE"));
			AssertEquals(2003.1m, unitOfMeasureValueList["TNE"]);
			AssertEquals(true, unitOfMeasureValueList.ContainsKey("KSK"));
			invoiceLine2.JI_CustomsSecondQuantity = 1000m;
			invoiceLine2.JI_CustomsSecondUnitQty = "TNE";
			universalRate = new EntryLineUniversalRate(entryLine);
			unitOfMeasureValueList = universalRate.UnitOfMeasureValueList;
			AssertEquals(true, unitOfMeasureValueList.ContainsKey("KGM"));
			AssertEquals(2003100m, unitOfMeasureValueList["KGM"]);
			AssertEquals(true, unitOfMeasureValueList.ContainsKey("TNE"));
			AssertEquals(3000.1m, unitOfMeasureValueList["TNE"]);
			AssertEquals(true, unitOfMeasureValueList.ContainsKey("KSK"));
			invoiceLine2.JI_CustomsSecondUnitQty = "LTR";
			universalRate = new EntryLineUniversalRate(entryLine);
			unitOfMeasureValueList = universalRate.UnitOfMeasureValueList;
			AssertEquals(true, unitOfMeasureValueList.ContainsKey("KGM"));
			AssertEquals(2003100m, unitOfMeasureValueList["KGM"]);
			AssertEquals(true, unitOfMeasureValueList.ContainsKey("TNE"));
			AssertEquals(2003.1m, unitOfMeasureValueList["TNE"]);
			AssertEquals(true, unitOfMeasureValueList.ContainsKey("KSK"));
			AssertEquals(true, unitOfMeasureValueList.ContainsKey("LTR"));
			AssertEquals(1000m, unitOfMeasureValueList["LTR"]);
			AssertEquals(true, unitOfMeasureValueList.ContainsKey("KLT"));
			AssertEquals(1m, unitOfMeasureValueList["KLT"]);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var hsnTariffType = helper.CreateNewOrGetExistingTariffType("TW", "HSN");
			var tariffType = helper.CreateTariffType("TW", "TXX");
			Factory.Save();
			var tariff1 = helper.CreateTariff("TW", hsnTariffType.PK, "0000000021", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			helper.CreateTariffUOM(tariff1.PK, "CU1", "KSK");
			helper.CreateTariffUOM(tariff1.PK, "CU2", "TNE");
			var tariff_1 = helper.CreateTariff("TW", tariffType.PK, "1000000021", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			helper.CreateTariffUOM(tariff_1.PK, "CU1", "KSK");
			var tariff_2 = helper.CreateTariff("TW", tariffType.PK, "1000000022", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			helper.CreateTariffUOM(tariff_2.PK, "CU1", "KGM");
			var tariff_3 = helper.CreateTariff("TW", tariffType.PK, "1000000023", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			helper.CreateTariffUOM(tariff_3.PK, "CU1", "A1");
			Factory.Save();
			invoiceLine1.JI_Tariff = "0000000021";
			invoiceLine2.JI_Tariff = "0000000021";
			invoiceLine1.JI_CustomsUnitQty = "KSK";
			invoiceLine1.JI_CustomsQuantity = 1000M;
			invoiceLine1.JI_CustomsSecondUnitQty = "TNE";
			invoiceLine1.JI_CustomsSecondQuantity = 2000M;
			invoiceLine2.JI_CustomsUnitQty = "KGM";
			invoiceLine2.JI_CustomsQuantity = 3000M;
			invoiceLine2.JI_CustomsSecondUnitQty = "TNE";
			invoiceLine2.JI_CustomsSecondQuantity = 4000M;
			universalRate = new EntryLineUniversalRate(entryLine);
			unitOfMeasureValueList = universalRate.UnitOfMeasureValueList;
			AssertEquals(1000M, unitOfMeasureValueList["KSK"]);
			AssertEquals(6000M, unitOfMeasureValueList["TNE"]);
			AssertEquals(2003000M, unitOfMeasureValueList["KGM"]);
			var invoiceLineTax = invoiceLine1.Taxes.AddNew();
			invoiceLineTax.JLT_Type = "TXX";
			invoiceLineTax.JLT_Tariff = "1000000021";
			AssertEquals("KSK", invoiceLineTax.JLT_BaseQuantityUQ);
			universalRate = new EntryLineUniversalRate(entryLine);
			unitOfMeasureValueList = universalRate.UnitOfMeasureValueList;
			AssertEquals(1000M, unitOfMeasureValueList["KSK"]);
			invoiceLineTax.JLT_BaseQuantity = 99M;
			universalRate = new EntryLineUniversalRate(entryLine);
			unitOfMeasureValueList = universalRate.UnitOfMeasureValueList;
			AssertEquals(1000M, unitOfMeasureValueList["KSK"]);
			invoiceLineTax.JLT_Tariff = "1000000022";
			AssertEquals("KGM", invoiceLineTax.JLT_BaseQuantityUQ);
			invoiceLineTax.JLT_BaseQuantity = 66M;
			universalRate = new EntryLineUniversalRate(entryLine);
			unitOfMeasureValueList = universalRate.UnitOfMeasureValueList;
			AssertEquals(true, unitOfMeasureValueList.ContainsKey("KGM"));
			AssertEquals(2003000M, unitOfMeasureValueList["KGM"]);
			invoiceLineTax.JLT_Tariff = "1000000023";
			AssertEquals("A1", invoiceLineTax.JLT_BaseQuantityUQ);
			invoiceLineTax.JLT_BaseQuantity = 66M;
			universalRate = new EntryLineUniversalRate(entryLine);
			unitOfMeasureValueList = universalRate.UnitOfMeasureValueList;
			AssertEquals(true, unitOfMeasureValueList.ContainsKey("A1"));
			AssertEquals(66m, unitOfMeasureValueList["A1"]);
		}

		public void TestGetUnitOfMeasureValueListByInvoiceLine()
		{
			var invoiceLine = (JobComInvoiceLine)Factory.New<JobDeclaration>().Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.JI_CustomsUnitQty = "KGM";
			invoiceLine.JI_CustomsSecondUnitQty = "B";
			invoiceLine.JI_CustomsQuantity = 500M;
			invoiceLine.JI_CustomsSecondQuantity = 100M;
			var unitOfMeasureValueList = EntryLineUniversalRate.GetUnitOfMeasureValueListByInvoiceLine(invoiceLine);
			AssertEquals(3, unitOfMeasureValueList.Count);
			AssertEquals(invoiceLine.JI_CustomsQuantity, unitOfMeasureValueList["KGM"]);
			AssertEquals(invoiceLine.JI_CustomsSecondQuantity, unitOfMeasureValueList["B"]);
			AssertEquals(invoiceLine.JI_CustomsQuantity * 0.001M, unitOfMeasureValueList["TNE"]);
		}

		protected override EntryLineUniversalRate GetEntryLineUniversalRate()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew() as JobComInvoiceLine;
			invoiceLine.JI_AdditionalDutyRate = 10m;
			var entryLine = declaration.ActiveEntryHeaders.AddNew().MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			return new EntryLineUniversalRate(entryLine);
		}

		protected override IEnumerable<(string uq, decimal qty)> ExpectCountrySpecificValues
		{
			get
			{
				yield return (TWSpecificRateParameterList.Codes.AlcoholPercentage, ZDecimal.Zero);
				yield return (TWSpecificRateParameterList.Codes.UnitCustomsValue, ZDecimal.Zero);
				var chargeTypeList = ChargeTypeHelper.GetChargeTypes(Factory, ZDateTime.Today);
				foreach (var chargeType in chargeTypeList)
				{
					yield return (chargeType.RateCode, ZDecimal.Zero);
				}
			}
		}
	}
}
