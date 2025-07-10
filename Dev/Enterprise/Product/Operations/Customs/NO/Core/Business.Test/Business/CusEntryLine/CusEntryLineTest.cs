using System;
using System.ComponentModel;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing;

[TestedType(typeof(CusEntryLine))]
sealed class CusEntryLineTest : CusEntryLineTest<CusEntryLine, JobComInvoiceLine>
{
	protected override BaseJobDeclaration ImportJobDeclaration
	{
		get
		{
			var result = base.ImportJobDeclaration;
			result.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			return result;
		}
	}

	protected override Type ExpectedTypeOfFees => typeof(CusEntryLineFeeCollection);

	public void TestDutyAmount_Attributes() => CombineAssertions(() =>
		AssertEntity<CusEntryLine>()
			.HasProperty(x => x.DutyAmount)
			.WithAttribute<DecimalPlacesAttribute>(x => x.DecimalPlaces == 0, because: "value is rounded")
			.WithAttribute<DecimalPrecisionAttribute>(x => x.DecimalPrecision == 16)
			.WithAttribute<ReadOnlyAttribute>(x => x.IsReadOnly, because: "underlying values are calculated during merge"));

	public void TestCustomsDutyAmount_Attributes() => CombineAssertions(() =>
		AssertEntity<CusEntryLine>()
			.HasProperty(x => x.CustomsDutyAmount)
			.WithAttribute<DecimalPlacesAttribute>(x => x.DecimalPlaces == 0, because: "value is rounded")
			.WithAttribute<DecimalPrecisionAttribute>(x => x.DecimalPrecision == 16)
			.WithAttribute<ReadOnlyAttribute>(x => x.IsReadOnly, because: "underlying values are calculated during merge"));

	public void TestExciseDutyAmount_Attributes() => CombineAssertions(() =>
		AssertEntity<CusEntryLine>()
			.HasProperty(x => x.ExciseDutyAmount)
			.WithAttribute<DecimalPlacesAttribute>(x => x.DecimalPlaces == 0, because: "value is rounded")
			.WithAttribute<DecimalPrecisionAttribute>(x => x.DecimalPrecision == 16)
			.WithAttribute<ReadOnlyAttribute>(x => x.IsReadOnly, because: "underlying values are calculated during merge"));

	public void TestVatAmount_Attributes() => CombineAssertions(() =>
		AssertEntity<CusEntryLine>()
			.HasProperty(x => x.VatAmount)
			.WithAttribute<DecimalPlacesAttribute>(x => x.DecimalPlaces == 0, because: "value is rounded")
			.WithAttribute<DecimalPrecisionAttribute>(x => x.DecimalPrecision == 16)
			.WithAttribute<ReadOnlyAttribute>(x => x.IsReadOnly, because: "underlying values are calculated during merge"));

	public void TestTotalAmount_Attributes() => CombineAssertions(() =>
		AssertEntity<CusEntryLine>()
			.HasProperty(x => x.TotalAmount)
			.WithAttribute<DecimalPlacesAttribute>(x => x.DecimalPlaces == 0, because: "value is rounded")
			.WithAttribute<DecimalPrecisionAttribute>(x => x.DecimalPrecision == 16)
			.WithAttribute<ReadOnlyAttribute>(x => x.IsReadOnly, because: "underlying values are calculated during merge"));

	public void TestDefaultValuationCode()
	{
		var declaration = ImportJobDeclaration;
		var invoiceHeader = (JobComInvoiceHeader)declaration.Invoices.AddNew();
		invoiceHeader.JZ_ValuationMethod = "5";
		invoiceHeader.InvoiceLines.AddNew();

		DoMerge(declaration);

		var entryLine = (CusEntryLine)declaration.CustomsEntryHeaders[0].AllEntryLines[0];

		AssertEquals("Entry line's default valuation code should be invoice header's valuation method.", "5", entryLine.ValuationCode);
	}

	public void TestAllInvoiceLines()
	{
		var entryLine = Factory.New<CusEntryLine>();
		var lineOne = entryLine.InvoiceLines.AddNew();
		var lineTwo = entryLine.InvoiceLines.AddNew();

		var allInvoiceLines = entryLine.AllInvoiceLines.ToArray();
		AssertContainsExactElementsInAnyOrder("AllInvoiceLines", [lineOne.PK, lineTwo.PK], allInvoiceLines.Select(l => l.PK));
	}

	public void TestAllFees()
	{
		var entryLine = Factory.New<CusEntryLine>();
		var feeOne = entryLine.Fees.AddNew();
		var feeTwo = entryLine.Fees.AddNew();

		var allFees = entryLine.AllFees.ToArray();
		AssertContainsExactElementsInAnyOrder("AllFees", [feeOne.PK, feeTwo.PK], allFees.Select(l => l.PK));
	}

	[TestDate(2024, 6, 12)]
	public void TestRoundingOfCustomsValueAndStatisticalValue()
	{
		var refCurrency = RefCurrency.New(Factory);
		refCurrency.RX_Code = "MDD";
		refCurrency.SetCustomsRate(new ZDateTime(2023, 1, 1), new ZDateTime(2025, 12, 31), RatesAreReciprocal ? 2m : 1.234m);
		var localCurrency = RefCurrency.LoadFromCurrencyCode(GlbCompany.CurrentCompany.Factory, GlbCompany.CurrentCompany.Country.RN_RX_NKLocalCurrency)?.RX_Code ?? ZString.Empty;

		var declaration = ImportJobDeclaration;
		declaration.JE_MergeBy = MergeInvoiceLinesConstants.Maximum;
		declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();
		var invoice = declaration.Invoices.AddNew();
		invoice.JZ_RX_NKInvoice_Currency = refCurrency.RX_Code;
		var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
		invoiceLine1.JI_Tariff = "2203.10.10 10";
		invoiceLine1.JI_LinePrice = 100.0m;

		AddChargeTo(invoiceLine1, CustomsChargeTypeList.Codes.OverseasFreight, j7_Amount: 1.23m, j7_IsDutiable: true);
		AddChargeTo(invoiceLine1, CustomsChargeTypeList.Codes.OverseasInsurance, j7_Amount: 4.56m, j7_IsDutiable: true);

		DoMerge(declaration);
		var entryLine = declaration.CustomsEntryHeaders[0].MergedLines[0];

		CombineAssertions(() =>
		{
			AssertEquals("CL_StatisticalValue, Without VGE (85.73 -> 86.00)", 86.0m, entryLine.CL_StatisticalValue);
			AssertEquals("CL_CustomsValue, Without VGE (85.73 -> 86.00)", 86.0m, entryLine.CL_CustomsValue);

			AddChargeTo(invoiceLine1, NOInvoiceChargeTypesImport.Codes.ValueOfGoodsExported, j7_Amount: 7.89m, j7_IsDutiable: false);
			DoMerge(declaration);

			AssertEquals("CL_StatisticalValue, With VGE (92.12 -> 92.00)", 92.0m, entryLine.CL_StatisticalValue);
			AssertEquals("CL_CustomsValue, With VGE (85.73 -> 86.00)", 86.0m, entryLine.CL_CustomsValue);

			var invoice2 = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = localCurrency;
			var invoiceLine2 = invoice2.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "2203.10.10 10";
			invoiceLine2.JI_LinePrice = 500.0m;
			AddChargeTo(invoiceLine2, CustomsChargeTypeList.Codes.OverseasFreight, j7_Amount: 1.23m, j7_IsDutiable: true);
			DoMerge(declaration);

			AssertEquals("CL_StatisticalValue, Two invoice lines, With VGE (517.26 -> 517.00)", 517.0m, entryLine.CL_StatisticalValue);
			AssertEquals("CL_CustomsValue, Two invoice lines, With VGE (510.87 -> 511.00)", 511.0m, entryLine.CL_CustomsValue);
		});
	}

	public void TestAdjustmentsRounded() => CombineAssertions(() =>
	{
		var entryLine = Factory.New<CusEntryLine>();
		AssertAdjustmentsRounded(4.6m, 1.2m, 3);
		AssertAdjustmentsRounded(4.8m, 1.2m, 4);

		void AssertAdjustmentsRounded(ZDecimal customsvalue, ZDecimal invoiceAmount, ZInt expected)
		{
			entryLine.CL_CustomsValue = customsvalue;
			entryLine.CL_InvoiceAmount = invoiceAmount;
			AssertEquals($"{customsvalue} - {invoiceAmount} => {expected}", expected: expected, entryLine.AdjustmentsRounded);
		}
	});

	#region Overseas carges (OFT/ONS) are dutiable in NO

	[TestDate(2023, 9, 2)]
	public override void TestMoneyInLocalCurrency()
	{
		var refCurrency = RefCurrency.New(Factory);
		refCurrency.RX_Code = "MDD";
		refCurrency.SetCustomsRate(new ZDateTime(2023, 9, 1), new ZDateTime(2023, 9, 5), RatesAreReciprocal ? 2m : 0.5m);
		var declaration = SetUpDeclarationAndInvLinesForMoneyTest(refCurrency);
		var entryLine = declaration.CustomsEntryHeaders[0].MergedLines[0];
		CombineAssertions(() =>
		{
			AssertEquals("FOB", 690.0m, entryLine.FOBInLocalCurrency.Amount);
			AssertEquals("CIF", 690.0m, entryLine.CIFInLocalCurrency.Amount);
			AssertEquals("OFT", 60.0m, entryLine.OverseasFreightInLocalCurrency.Amount);
			AssertEquals("ONS", 30.0m, entryLine.OverseasInsuranceInLocalCurrency.Amount);
			AssertEquals("T&I", 90.0m, entryLine.TAndIInLocalCurrency.Amount);
		});
	}

	BaseJobDeclaration SetUpDeclarationAndInvLinesForMoneyTest(RefCurrency refCurrency)
	{
		var declaration = ImportJobDeclaration;
		declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
		declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();
		var invoice = declaration.Invoices.AddNew();
		invoice.JZ_RX_NKInvoice_Currency = refCurrency.RX_Code;

		var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
		var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
		invoiceLine1.JI_Tariff = "2203.10.10 10";
		invoiceLine2.JI_Tariff = "2203.10.10 10";
		invoiceLine1.JI_LinePrice = 100.0m;
		invoiceLine2.JI_LinePrice = 200.0m;

		AddChargeTo(invoiceLine1, CustomsChargeTypeList.Codes.OverseasInsurance, j7_Amount: 5m, j7_IsDutiable: true);
		AddChargeTo(invoiceLine1, CustomsChargeTypeList.Codes.OverseasInsurance, j7_Amount: 10m, j7_IsDutiable: true);
		AddChargeTo(invoiceLine1, CustomsChargeTypeList.Codes.OverseasFreight, j7_Amount: 10m, j7_IsDutiable: true);
		AddChargeTo(invoiceLine1, CustomsChargeTypeList.Codes.OverseasFreight, j7_Amount: 20m, j7_IsDutiable: true);

		if (declaration.IsEntryInstructionRequired)
		{
			var cusEntryInstruction = declaration.CustomsEntryInstructions.AddNew();
			invoiceLine1.JI_CEI = cusEntryInstruction.PK;
			invoiceLine2.JI_CEI = cusEntryInstruction.PK;
		}

		DoMerge(declaration);
		return declaration;
	}

	BaseInvoiceLineCharge AddChargeTo(BaseJobComInvoiceLine invoiceLine, string chargeCode, ZDecimal j7_Amount, ZBool j7_IsDutiable)
	{
		var charge = invoiceLine.Charges.AddNew(chargeCode);
		charge.J7_Amount = j7_Amount;
		charge.J7_IsDutiable = j7_IsDutiable;
		return charge;
	}

	#endregion

	public void TestFeeRounder()
	{
		var entryLine = Factory.New<CusEntryLine>();
		AssertType<IntegerFeeRounder>(entryLine.FeeRounder);
	}
}
