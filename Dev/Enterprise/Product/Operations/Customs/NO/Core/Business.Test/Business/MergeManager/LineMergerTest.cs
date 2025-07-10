using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Customs.NO.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.NO.Business.Testing;

[TestedType(typeof(LineMerger))]
sealed class LineMergerTest : Customs.Business.Testing.LineMergerTest
{
	protected override BaseJobDeclaration GetJobDeclaration() => Factory.New<JobDeclaration>();

	protected override Customs.Business.LineMerger GetNewLineMerger(BaseJobDeclaration declaration) => new LineMerger((JobDeclaration)declaration);

	protected override Type ExpectedDutyCalculatorStrategyType => typeof(DutyCalculatorStrategy);

	protected override Type[] ExpectedEntryCreationStrategiesType => [typeof(EntryCreationStrategy)];

	protected override bool ApplyClassificationToKeyForLineForMerge => false;

	protected override bool ApplyPartNumberToKeyForLineForMerge => false;

	#region No-merge merging

	public void TestSingleEntryLineForSingleInvoiceLine()
	{
		var invLine = AddLine();
		invLine.JI_LineNo = 1;

		merger.DoMerge();

		CombineAssertions(() =>
		{
			AssertEquals(1, declaration.CustomsEntryHeaders.Count);
			AssertEquals(1, declaration.CustomsEntryHeaders[0].AllEntryLines.Count);
		});
	}

	public void TestMultipleInvoiceLinesWithDifferentTariffs()
	{
		var invLine1 = AddLine();
		invLine1.JI_LineNo = 1;
		invLine1.JI_Tariff = TariffConstants.VfdTariff;

		var invLine2 = AddLine();
		invLine2.JI_LineNo = 2;
		invLine2.JI_Tariff = TariffConstants.KgTariff;

		merger.DoMerge();

		CombineAssertions(() =>
		{
			AssertEquals(1, declaration.CustomsEntryHeaders.Count);
			AssertEquals(2, declaration.CustomsEntryHeaders[0].AllEntryLines.Count);
		});
	}

	public void TestMultipleInvoiceLinesWithSameTariffs()
	{
		var invLine1 = AddLine();
		invLine1.JI_LineNo = 1;
		invLine1.JI_Tariff = TariffConstants.VfdTariff;

		var invLine2 = AddLine();
		invLine2.JI_LineNo = 2;
		invLine2.JI_Tariff = TariffConstants.VfdTariff;

		merger.DoMerge();

		CombineAssertions(() =>
		{
			AssertEquals("CusEntryHeaders Count", 1, declaration.CustomsEntryHeaders.Count);
			AssertEquals("CusEntryLines Count", 2, declaration.CustomsEntryHeaders[0].AllEntryLines.Count);
		});
	}

	public void TestMultipleInvoiceLinesWithSameTariffs_MergeByMAX()
	{
		var invLine1 = AddLine();
		invLine1.JI_LineNo = 1;
		invLine1.JI_Tariff = TariffConstants.VfdTariff;

		var invLine2 = AddLine();
		invLine2.JI_LineNo = 2;
		invLine2.JI_Tariff = TariffConstants.VfdTariff;

		declaration.JE_MergeBy = MergeInvoiceLinesConstants.Maximum;
		merger.DoMerge();

		CombineAssertions(() =>
		{
			AssertEquals("CusEntryHeaders Count", 1, declaration.CustomsEntryHeaders.Count);
			AssertEquals("CusEntryLines Count", 1, declaration.CustomsEntryHeaders[0].AllEntryLines.Count);
			AssertEquals("JobComInvoiceLines Count", 2, declaration.CustomsEntryHeaders[0].AllEntryLines[0].InvoiceLines.Count);
		});
	}

	public void TestMultipleInvoicesWithIdenticalSupportingDocuments_MergeBehavior()
	{
		AddInvoiceLineWithSupportingDocuments(1, ("ZZ", "123"), ("AA", "456"));
		AddInvoiceLineWithSupportingDocuments(2, ("ZZ", "123"), ("AA", "456"));

		declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
		merger.DoMerge();

		CombineAssertions(() =>
		{
			AssertEquals("Merge By NON : CusEntryHeaders Count", 1, declaration.CustomsEntryHeaders.Count);
			AssertEquals("Merge By NON : CusEntryLines Count", 2, declaration.CustomsEntryHeaders[0].AllEntryLines.Count);
			declaration.JE_MergeBy = MergeInvoiceLinesConstants.Maximum;
			merger.DoMerge();
			AssertEquals("Merge By MAX : CusEntryHeaders Count", 1, declaration.CustomsEntryHeaders.Count);
			AssertEquals("Merge By MAX : CusEntryLines Count", 1, declaration.CustomsEntryHeaders[0].AllEntryLines.Count);
		});
	}

	public void TestMultipleInvoicesWithDistinctSupportingDocuments_MergeBehavior()
	{
		AddInvoiceLineWithSupportingDocuments(1, ("ZZ", "123"), ("AA", "456"));
		AddInvoiceLineWithSupportingDocuments(2, ("ZZ", "456"), ("AA", "123"));

		declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
		merger.DoMerge();

		CombineAssertions(() =>
		{
			AssertEquals("Merge By NON : CusEntryHeaders Count", 1, declaration.CustomsEntryHeaders.Count);
			AssertEquals("Merge By NON : CusEntryLines Count", 2, declaration.CustomsEntryHeaders[0].AllEntryLines.Count);
			declaration.JE_MergeBy = MergeInvoiceLinesConstants.Maximum;
			merger.DoMerge();
			AssertEquals("Merge By MAX : CusEntryHeaders Count", 1, declaration.CustomsEntryHeaders.Count);
			AssertEquals("Merge By MAX : CusEntryLines Count", 2, declaration.CustomsEntryHeaders[0].AllEntryLines.Count);
		});
	}

	public void TestMultipleInvoicesWithDuplicateSupportingDocuments_MergeBehavior()
	{
		AddInvoiceLineWithSupportingDocuments(1, ("ZZ", "123"), ("ZZ", "123"));
		AddInvoiceLineWithSupportingDocuments(2, ("ZZ", "123"));

		declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
		merger.DoMerge();

		CombineAssertions(() =>
		{
			AssertEquals("Merge By NON : CusEntryHeaders Count", 1, declaration.CustomsEntryHeaders.Count);
			AssertEquals("Merge By NON : CusEntryLines Count", 2, declaration.CustomsEntryHeaders[0].AllEntryLines.Count);
			declaration.JE_MergeBy = MergeInvoiceLinesConstants.Maximum;
			merger.DoMerge();
			AssertEquals("Merge By MAX : CusEntryHeaders Count", 1, declaration.CustomsEntryHeaders.Count);
			AssertEquals("Merge By MAX : CusEntryLines Count", 2, declaration.CustomsEntryHeaders[0].AllEntryLines.Count);
		});
	}

	#endregion

	#region Customs and excises

	public void TestZeroCustomsDutyAndVAT()
	{
		_ = AddLine(line =>
		{
			line.JI_LinePrice = 0m;
			line.JI_Tariff = TariffConstants.VfdTariff;
			line.JI_PrimaryPreference = PrimaryPreferenceCodeList.Codes.G;
		});
		merger.DoMerge();

		var entryLine = declaration.CustomsEntryHeaders[0].AllEntryLines[0];

		AssertDutyOrder("No customs duty, no excises, only VAT.", entryLine, Array.Empty<ZString>());
	}

	public void TestZeroCustomsDuty()
	{
		_ = AddLine(line =>
		{
			line.JI_Tariff = TariffConstants.VfdTariff;
			line.JI_PrimaryPreference = PrimaryPreferenceCodeList.Codes.G;
		});
		merger.DoMerge();

		var entryLine = declaration.CustomsEntryHeaders[0].AllEntryLines[0];

		AssertDutyOrder("No customs duty, no excises, only VAT.", entryLine, RefCusTaxOrFee.MV1);
	}

	public void TestPercentTariffDefaultCustomsDuty()
	{
		var invLine = AddLine();
		invLine.JI_Tariff = TariffConstants.VfdTariff;
		merger.DoMerge();

		CombineAssertions(() =>
		{
			AssertSingleEntryLine(100m);
		});
	}

	public void TestKgTariffDefaultCustomsDuty()
	{
		var invLine = AddLine();
		invLine.JI_Tariff = TariffConstants.KgTariff;

		merger.DoMerge();

		AssertSingleEntryLine(20m);
	}

	public void TestPercentTariffOverrideCustomsRate()
	{
		var invLine = AddLine();
		invLine.JI_Tariff = TariffConstants.VfdTariff;
		invLine.CustomsRateIsOverridden = true;
		invLine.CustomsRate = 5m;

		merger.DoMerge();

		AssertSingleEntryLine(50m);
	}

	public void TestKgTariffOverrideCustomsRate()
	{
		var invLine = AddLine();
		invLine.JI_Tariff = TariffConstants.KgTariff;
		invLine.CustomsRateIsOverridden = true;
		invLine.CustomsRate = 1.5m;

		merger.DoMerge();

		AssertSingleEntryLine(15m);
	}

	public void TestOverrideCustomsRateAndType()
	{
		var invLine = AddLine();
		invLine.JI_Tariff = TariffConstants.MixedTariff;
		invLine.JI_CustomsUnitQty = "KGM";
		invLine.CustomsRateIsOverridden = true;
		invLine.CustomsRateType = "K";
		invLine.CustomsRate = 1m;

		merger.DoMerge();

		AssertSingleEntryLine(10m);
	}

	public void TestCustomsDuty_WhenMultipleInvoiceLines()
	{
		_ = AddLine(invLine1 =>
		{
			invLine1.JI_Tariff = TariffConstants.VfdTariff;
			invLine1.JI_LinePrice = 420;
		});
		_ = AddLine(invLine2 =>
		{
			invLine2.JI_Tariff = TariffConstants.VfdTariff;
			invLine2.JI_LinePrice = 69_000;
		});

		declaration.JE_MergeBy = "MAX";
		merger.DoMerge();
		var entryLine = declaration.CustomsEntryHeaders[0].AllEntryLines[0];

		AssertEquals("[PRE-CONDITION] CusEntryLine should contain two merged JobComInvoiceLines", 2, entryLine.InvoiceLines.Count);
		CombineAssertions(() =>
		{
			AssertCustomsDuty("TL + RT where TL is 10% * 69_420 and RT is zero", dutyRateTL: 10m, baseValueTL: 69_420m, dutyAmount: 6942m);
		});
	}

	#endregion

	#region VAT

	public void TestVat_CIFOnly()
	{
		_ = AddLine(line =>
		{
			line.JI_Tariff = TariffConstants.VfdTariff;
			line.JI_PrimaryPreference = PrimaryPreferenceCodeList.Codes.G;
		});
		merger.DoMerge();

		var entryLine = declaration.CustomsEntryHeaders[0].AllEntryLines[0];

		CombineAssertions(() =>
		{
			AssertVat("CIF only, VATable 1000, VAT 25% = 250", entryLine, RefCusTaxOrFee.MV1, 25m, 1000m, 250m);
			AssertEquals("VAT code is default MV1", RefCusTaxOrFee.MV1, entryLine.VatCode);
			AssertEquals("VATable amount is equal to CIF (1000)", 1000m, entryLine.CL_ValueForVAT);
			AssertEquals("VAT amount is 25% of CIF (1000)", 250m, entryLine.VatAmount);
		});
	}

	public void TestVat_CIFAndCustoms()
	{
		_ = AddLine(line =>
		{
			line.JI_Tariff = TariffConstants.VfdTariff;
		});

		merger.DoMerge();
		var entryLine = declaration.CustomsEntryHeaders[0].AllEntryLines[0];

		CombineAssertions(() =>
		{
			AssertVat("CIF and customs, VATable 1100, VAT 25% = 275", entryLine, RefCusTaxOrFee.MV1, 25m, 1100m, 275m);
			AssertEquals("VAT code is default MV1", RefCusTaxOrFee.MV1, entryLine.VatCode);
			AssertEquals("VATable amount is CIF (1000) + customs duty (100)", 1100m, entryLine.CL_ValueForVAT);
			AssertEquals("VAT amount is 25% of CIF + customs duty (1100)", 275m, entryLine.VatAmount);
		});
	}

	public void TestVat_IsLandedCostOnly()
	{
		AddLine(line =>
		{
			line.JI_Tariff = TariffConstants.KgTariff;
			line.JI_ZZF_NKTaxType = RefCusTaxOrFee.MV1;
		});
		AddLine(line =>
		{
			line.JI_Tariff = TariffConstants.VfdTariff;
			line.JI_ZZF_NKTaxType = RefCusTaxOrFee.MV2;
		});
		var importer = Factory.NewWithValidTestData<OrgHeader>();
		declaration.JE_OH_Importer = importer.PK;

		CombineAssertions(() =>
		{
			merger.DoMerge();
			var entryLine1 = declaration.CustomsEntryHeaders[0].AllEntryLines[0];
			var entryLine2 = declaration.CustomsEntryHeaders[0].AllEntryLines[1];
			AssertIsLandedCostOnly("importer is NOT MVA-registered and VAT code is 'MV1'", entryLine1, RefCusTaxOrFee.MV1, expected: false);
			AssertIsLandedCostOnly("importer is NOT MVA-registered and VAT code is 'MV2'", entryLine2, RefCusTaxOrFee.MV2, expected: false);

			importer.AsMVARegistered();
			merger.DoMerge();
			entryLine1 = declaration.CustomsEntryHeaders[0].AllEntryLines[0];
			entryLine2 = declaration.CustomsEntryHeaders[0].AllEntryLines[1];
			AssertIsLandedCostOnly("importer is MVA-registered and VAT code is 'MV1'", entryLine1, RefCusTaxOrFee.MV1, expected: true);
			AssertIsLandedCostOnly("importer is MVA-registered and VAT code is 'MV2'", entryLine2, RefCusTaxOrFee.MV2, expected: true);
		});
	}

	#endregion

	public void Test_StatisticalValue_IncludesVGEChargeType()
	{
		AssertMergedEntryLine("StatisticalValue includes VGE charges", 666m, entryLine => entryLine.CL_StatisticalValue);
	}

	public void Test_InvoiceAmount()
	{
		AssertMergedEntryLine("Invoice Amount, invoice line prices", 550m, entryLine => entryLine.CL_InvoiceAmount);
	}

	#region Constants

	static class TariffConstants
	{
		public const string VfdTariff = "10001000";
		public const string KgTariff = "10002000";
		public const string MixedTariff = "10003000";
	}

	static class DutyConstants
	{
		public const string Customs = NOCustomDutyCodeList.Codes.TL1;
		public const string RawProducts = "RT100";
		public const string PaperFee = "MA100";
		public const string RecycledPlasticFee = "MP113";
		public const string BaseBeerFee = "GA100";
	}

	#endregion

	#region Assert helpers

	void AssertSingleEntryLine(decimal amount)
	{
		AssertEquals(1, declaration.CustomsEntryHeaders.Count);
		var entryHeader = declaration.CustomsEntryHeaders[0];
		AssertEquals(1, entryHeader.AllEntryLines.Count);
		var entryLine = entryHeader.AllEntryLines[0];
		AssertEquals(amount, entryLine.DutyAmount);
	}

	void AssertCustomsDuty(string assertMessage, decimal dutyRateTL, decimal baseValueTL, decimal dutyAmount)
	{
		AssertEquals($"{assertMessage}, headers count", 1, declaration.CustomsEntryHeaders.Count);
		var entryHeader = declaration.CustomsEntryHeaders[0];
		AssertEquals($"{assertMessage}, lines count", 1, entryHeader.AllEntryLines.Count);
		var entryLine = entryHeader.AllEntryLines[0];
		var entryLineDuty = entryLine.Fees.SingleOrDefault(duty => duty.CF_ChargeType == DutyConstants.Customs);

		AssertEquals($"{assertMessage}, customs duty amount", dutyAmount, entryLine.DutyAmount);
		AssertEquals($"{assertMessage}, rate", dutyRateTL, entryLineDuty.CF_Rate);
		AssertEquals($"{assertMessage}, base value", baseValueTL, entryLineDuty.CF_BaseValue);
		AssertEquals($"{assertMessage}, customs duty macro\n - Should equal sum of TL and RT", dutyAmount, entryHeader.CustomsDutyAmount);
	}

	void AssertVat(string assertMessage, CusEntryLine entryLine, string vatCode, decimal vatRate, decimal baseValue, decimal vatAmount)
	{
		var vatFee = entryLine.Fees.SingleOrDefault(duty => duty.CF_ChargeType == vatCode);
		AssertNotNull($"{assertMessage}\n - Should have single fee with VAT code {vatCode}", vatFee);
		AssertEquals($"{assertMessage}, is VAT", expected: true, vatFee.IsVAT);
		AssertEquals($"{assertMessage}, rate type", "%", vatFee.CF_RateType);
		AssertEquals($"{assertMessage}, rate", vatRate, vatFee.CF_Rate);
		AssertEquals($"{assertMessage}, base value", baseValue, vatFee.CF_BaseValue);
		AssertEquals($"{assertMessage}, amount", vatAmount, vatFee.CF_ChargeAmount);

		var entryHeader = declaration.CustomsEntryHeaders[0];
		AssertEquals($"{assertMessage}, VAT macro\n - Should equal sum of MV", vatAmount, entryHeader.VatAmount);
	}

	void AssertDutyOrder(string assertMessage, CusEntryLine entryLine, params ZString[] dutyCodes)
	{
		var dutyCodeTypes = dutyCodes.Select(x => x.ToString() switch
			{
				var tl when tl.StartsWith("TL") => "1) Customs Duty",
				var rt when rt.StartsWith("RT") => "2) Agricultural Duty",
				var mv when mv.StartsWith("MV") => "4) Import VAT",
				_ => "3) Excise Duty",
			} + $" {x}").ToArray();
		AssertSequencesEqual($"[PRE-CONDITION] {assertMessage}, category sorting", dutyCodeTypes.OrderBy(x => x), dutyCodeTypes);
		var actualDutyCodes = entryLine.Fees.Select(x => x.CF_ChargeType).ToArray();
		AssertSequencesEqual(assertMessage, dutyCodes, actualDutyCodes);
	}

	void AssertIsLandedCostOnly(string assertMessage, CusEntryLine entryLine, string vatCode, bool expected)
	{
		var vatFee = entryLine.Fees.SingleOrDefault(duty => duty.CF_ChargeType == vatCode);
		AssertNotNull(assertMessage, vatFee);

		AssertEquals(assertMessage, expected, vatFee.CF_IsLandedCostOnly);
	}

	#endregion

	#region Setup

	protected override void SetUp()
	{
		base.SetUp();
		SetupTariffs();
		RefCusTaxOrFeeHelper.CreateRefCusTaxOrFeeList(Factory);
		declaration = (JobDeclaration)GetJobDeclaration();
		invoice = declaration.Invoices.AddNew();
		invoice.JZ_RX_NKInvoice_Currency = "NOK";
		merger = new LineMerger(declaration);
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
	}

	JobComInvoiceLine AddLine(Action<JobComInvoiceLine> setup = null)
	{
		var invLine = invoice.JobComInvoiceLines.AddNew();
		invLine.JI_LinePrice = 1000m;
		invLine.JI_CustomsUnitQty = "KGM";
		invLine.JI_CustomsQuantity = 10;
		invLine.JI_PrimaryPreference = "N";
		invLine.JI_CountryOfOrigin = "US";
		invLine.JI_ZZF_NKTaxType = RefCusTaxOrFee.MV1;
		setup?.Invoke(invLine);
		return invLine;
	}

	void SetupTariffs()
	{
		var tariffTestHelper = new RefCusTariffTestHelper(Factory);

		var vfdTariff = tariffTestHelper.CreateImportTariff(TariffConstants.VfdTariff);
		tariffTestHelper.AddRate(vfdTariff, PrimaryPreferenceCodeList.Codes.N, "0.1 * VFD", Core.Constants.CountryCodes.UnitedStates);
		tariffTestHelper.AddRate(vfdTariff, PrimaryPreferenceCodeList.Codes.G, "0", Core.Constants.CountryCodes.EuropeanUnion);

		var kgTariff = tariffTestHelper.CreateImportTariff(TariffConstants.KgTariff);
		tariffTestHelper.AddRate(kgTariff, PrimaryPreferenceCodeList.Codes.N, "2 * [KGM]", Core.Constants.CountryCodes.UnitedStates);

		var mixedTariff = tariffTestHelper.CreateImportTariff(TariffConstants.MixedTariff);
		tariffTestHelper.AddRate(mixedTariff, PrimaryPreferenceCodeList.Codes.N, "3 * [KGM]", Core.Constants.CountryCodes.UnitedStates);
		tariffTestHelper.AddRate(mixedTariff, PrimaryPreferenceCodeList.Codes.N, "0.2 * VFD", Core.Constants.CountryCodes.UnitedStates);
	}

	void AddInvoiceLineWithSupportingDocuments(ZShort lineNo, params (string Code, string ReferenceNumber)[] supportingDocs)
	{
		var invLine = AddLine();
		invLine.JI_LineNo = lineNo;
		invLine.JI_Tariff = TariffConstants.VfdTariff;
		foreach (var (code, refNumber) in supportingDocs)
		{
			var suppDoc = invLine.SupportingDocuments.AddNew();
			suppDoc.CSI_Code = code;
			suppDoc.CSI_ReferenceNumber = refNumber;
		}
	}

	void AssertMergedEntryLine(string message, decimal expectedValue, Func<CusEntryLine, decimal> propertySelector)
	{
		invoice.Charges.AddNew(NOInvoiceChargeTypesImport.Codes.ValueOfGoodsExported, 6.12m, Core.Constants.CurrencyCodes.Norway);

		var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
		invoiceLine1.JI_LinePrice = 50m;
		invoiceLine1.JI_Procedure = "4100";
		_ = invoiceLine1.Charges.AddNew(Universal.Constants.RateTypes.AntiDumping, 10.12m, Core.Constants.CurrencyCodes.Norway);

		var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
		invoiceLine2.JI_LinePrice = 500m;
		invoiceLine2.JI_Procedure = "4100";
		_ = invoiceLine2.Charges.AddNew(Universal.Constants.RateTypes.AntiDumping, 100m, Core.Constants.CurrencyCodes.Norway);

		declaration.JE_MergeBy = MergeInvoiceLinesConstants.Maximum;
		merger.DoMerge();
		var entryLine = declaration.CustomsEntryHeaders[0].MergedLines[0];

		AssertEquals(message, expectedValue, propertySelector(entryLine));
	}

	#endregion

	JobDeclaration declaration;
	JobComInvoiceHeader invoice;
	LineMerger merger;
}
