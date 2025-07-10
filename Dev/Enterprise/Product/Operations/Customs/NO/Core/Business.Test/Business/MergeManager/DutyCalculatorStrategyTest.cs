using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing;

[TestedType(typeof(DutyCalculatorStrategy))]
sealed class DutyCalculatorStrategyTest : DutyCalculatorStrategyAbstractTest<DutyCalculatorStrategy>
{
	public override void TestCalculateDuties()
	{
		var referenceData = SetupReferenceData(Factory);

		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

		var invoiceLineOne = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
		invoiceLineOne.JI_CEI = entryInstruction.PK;
		invoiceLineOne.JI_Procedure = "A";
		invoiceLineOne.JI_PrimaryPreference = referenceData.Preference.ZZS_Preference;
		invoiceLineOne.JI_Tariff = referenceData.ExciseTariffOne.ZZ1_TariffCode;
		invoiceLineOne.JI_SupplementaryCode1 = "MB200";
		invoiceLineOne.JI_SupplementaryCode2 = "MB220";
		invoiceLineOne.JI_CustomsQuantity = 90;
		invoiceLineOne.JI_CustomsUnitQty = "KGM";
		invoiceLineOne.JI_CustomsSecondQuantity = 90;
		invoiceLineOne.JI_CustomsSecondUnitQty = "LTR";
		invoiceLineOne.JI_CustomsThirdQuantity = 120;
		invoiceLineOne.JI_CustomsThirdUnitQty = "PCS";
		invoiceLineOne.JI_CustomsFourthQuantity = 12;
		invoiceLineOne.JI_CustomsFourthUnitQty = "ASV";
		invoiceLineOne.JI_CustomsFifthQuantity = 87;
		invoiceLineOne.JI_CustomsFifthUnitQty = "NMB";
		invoiceLineOne.JI_ZZF_NKTaxType = UniversalReferenceConstants.RefCusTaxOrFee.MV1;
		invoiceLineOne.JI_LinePrice = 1000;

		var invoiceLineTwo = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
		invoiceLineTwo.JI_CEI = entryInstruction.PK;
		invoiceLineTwo.JI_Procedure = "A";
		invoiceLineTwo.JI_PrimaryPreference = referenceData.Preference.ZZS_Preference;
		invoiceLineTwo.JI_Tariff = referenceData.ExciseTariffOne.ZZ1_TariffCode;
		invoiceLineTwo.JI_SupplementaryCode1 = "MB200";
		invoiceLineTwo.JI_SupplementaryCode2 = "MB220";
		invoiceLineTwo.JI_CustomsQuantity = 50;
		invoiceLineTwo.JI_CustomsUnitQty = "KGM";
		invoiceLineTwo.JI_CustomsSecondQuantity = 50;
		invoiceLineTwo.JI_CustomsSecondUnitQty = "LTR";
		invoiceLineTwo.JI_CustomsThirdQuantity = 100;
		invoiceLineTwo.JI_CustomsThirdUnitQty = "PCS";
		invoiceLineTwo.JI_CustomsFourthQuantity = 12;
		invoiceLineTwo.JI_CustomsFourthUnitQty = "ASV";
		invoiceLineTwo.JI_CustomsFifthQuantity = 55;
		invoiceLineTwo.JI_CustomsFifthUnitQty = "NMB";
		invoiceLineTwo.JI_ZZF_NKTaxType = UniversalReferenceConstants.RefCusTaxOrFee.MV1;
		invoiceLineTwo.JI_LinePrice = 800;

		var invoiceTwo = declaration.Invoices.AddNew();
		invoiceTwo.JZ_RX_NKInvoice_Currency = "NOK";

		var invoiceLineThree = (JobComInvoiceLine)invoiceTwo.InvoiceLines.AddNew();
		invoiceLineThree.JI_CEI = entryInstruction.PK;
		invoiceLineThree.JI_Procedure = "B";
		invoiceLineThree.JI_PrimaryPreference = referenceData.Preference.ZZS_Preference;
		invoiceLineThree.JI_Tariff = referenceData.ExciseTariffTwo.ZZ1_TariffCode;
		invoiceLineThree.JI_SupplementaryCode1 = "MA207";
		invoiceLineThree.JI_CustomsQuantity = 55;
		invoiceLineThree.JI_CustomsUnitQty = "KGM";
		invoiceLineThree.JI_CustomsSecondQuantity = 45;
		invoiceLineThree.JI_CustomsSecondUnitQty = "LTR";
		invoiceLineThree.JI_CustomsThirdQuantity = 122;
		invoiceLineThree.JI_CustomsThirdUnitQty = "PCS";
		invoiceLineThree.JI_CustomsFourthQuantity = 11;
		invoiceLineThree.JI_CustomsFourthUnitQty = "ASV";
		invoiceLineThree.JI_CustomsFifthQuantity = 22;
		invoiceLineThree.JI_CustomsFifthUnitQty = "RET";
		invoiceLineThree.JI_ZZF_NKTaxType = UniversalReferenceConstants.RefCusTaxOrFee.MV1;
		invoiceLineThree.JI_LinePrice = 900;

		var invoiceLineFour = (JobComInvoiceLine)invoiceTwo.InvoiceLines.AddNew();
		invoiceLineFour.JI_CEI = entryInstruction.PK;
		invoiceLineFour.JI_Procedure = "B";
		invoiceLineFour.JI_PrimaryPreference = referenceData.Preference.ZZS_Preference;
		invoiceLineFour.JI_Tariff = referenceData.ExciseTariffTwo.ZZ1_TariffCode;
		invoiceLineFour.JI_SupplementaryCode1 = "MA207";
		invoiceLineFour.JI_CustomsQuantity = 34;
		invoiceLineFour.JI_CustomsUnitQty = "KGM";
		invoiceLineFour.JI_CustomsSecondQuantity = 34;
		invoiceLineFour.JI_CustomsSecondUnitQty = "LTR";
		invoiceLineFour.JI_CustomsThirdQuantity = 56;
		invoiceLineFour.JI_CustomsThirdUnitQty = "PCS";
		invoiceLineFour.JI_CustomsFourthQuantity = 11;
		invoiceLineFour.JI_CustomsFourthUnitQty = "ASV";
		invoiceLineFour.JI_CustomsFifthQuantity = 12;
		invoiceLineFour.JI_CustomsFifthUnitQty = "RET";
		invoiceLineFour.JI_ZZF_NKTaxType = UniversalReferenceConstants.RefCusTaxOrFee.MV1;
		invoiceLineFour.JI_LinePrice = 678;

		_ = declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

		var entryHeaders = declaration.ActiveEntryHeaders.Cast<CusEntryHeader>().ToArray();
		AssertEquals("Merged Entry Header Count", 1, entryHeaders.Length);

		var entryLines = entryHeaders.Single().MergedLines.Cast<CusEntryLine>().ToArray();
		AssertEquals("Entry Lines Count", 2, entryLines.Length);

		var feeRounder = new FeeNoRounder();

		var entryLineOne = entryLines.Single(l => l.Procedure == "A");
		AssertEntryLineFee(entryLineOne, new []
		{
			new ExpectedFeeRecord("MB200", 953, 142, 6.71, "NMB", ZString.Empty),
			new ExpectedFeeRecord("MB220", 183, 142, 1.29, "NMB", ZString.Empty),
			new ExpectedFeeRecord("MV1", 734, 2936, 25, ZString.Empty, ZString.Empty),
		}, feeRounder);

		var entryLineTwo = entryLines.Single(l => l.Procedure == "B");
		AssertEntryLineFee(entryLineTwo, new[]
		{
			new ExpectedFeeRecord("MA207", 4467, 869, 5.14, "LTR", ZString.Empty),
			new ExpectedFeeRecord("MV1", 1511, 6045, 25, ZString.Empty, ZString.Empty),
		}, feeRounder);
	}

	public void TestCalculateDutiesWithCustomsRateOverride()
	{
		var referenceData = SetupReferenceData(Factory);

		CombineAssertions(() =>
		{
			AssertEquals("[Pre-Condition] RT200 Rate belongs to MixedTariff", referenceData.MixedTariff.PK, referenceData.RT200Rate.ZZ2_ZZ1_Tariff);
			AssertEquals("[Pre-Condition] MixedTariffDuty Rate belongs to MixedTariff", referenceData.MixedTariff.PK, referenceData.MixedTariffDutyRate.ZZ2_ZZ1_Tariff);
		});

		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

		var invoiceLine = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
		invoiceLine.JI_CEI = entryInstruction.PK;
		invoiceLine.JI_Procedure = "A";
		invoiceLine.JI_PrimaryPreference = referenceData.Preference.ZZS_Preference;
		invoiceLine.JI_Tariff = referenceData.MixedTariff.ZZ1_TariffCode;
		invoiceLine.JI_SupplementaryCode1 = "RT200";
		invoiceLine.JI_CustomsQuantity = 90;
		invoiceLine.JI_CustomsUnitQty = "KGM";
		invoiceLine.JI_CustomsSecondQuantity = 90;
		invoiceLine.JI_CustomsSecondUnitQty = "LTR";
		invoiceLine.CustomsRateIsOverridden = true;
		invoiceLine.CustomsRateType = "K";
		invoiceLine.CustomsRate = 1.5m;

		_ = declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

		var entryHeaders = declaration.ActiveEntryHeaders.Cast<CusEntryHeader>().ToArray();
		AssertEquals("Merged Entry Header Count", 1, entryHeaders.Length);

		var entryLines = entryHeaders.Single().MergedLines.Cast<CusEntryLine>().ToArray();
		AssertEquals("Entry Lines Count", 1, entryLines.Length);

		AssertEntryLineFee(entryLines[0], new []
		{
			new ExpectedFeeRecord("TL1", 135, 90, 1.5, ZString.Empty, ZString.Empty),
			new ExpectedFeeRecord("RT200", 692.1, 90, 7.69m, "KGM", ZString.Empty),
			new ExpectedFeeRecord("MV1", 207, 827, 25, ZString.Empty, ZString.Empty),
		}, new IntegerFeeRounder());
	}

	public void TestCalculateDutiesForExportDeclaration()
	{
		var referenceData = SetupReferenceData(Factory);

		CombineAssertions(() =>
		{
			AssertEquals("[Pre-Condition] EX101 Rate belongs to MixedTariff", referenceData.MixedTariff.PK, referenceData.EX101Rate.ZZ2_ZZ1_Tariff);
			AssertEquals("[Pre-Condition] RT200 Rate belongs to MixedTariff", referenceData.MixedTariff.PK, referenceData.RT200Rate.ZZ2_ZZ1_Tariff);
			AssertEquals("[Pre-Condition] MixedTariffDuty Rate belongs to MixedTariff", referenceData.MixedTariff.PK, referenceData.MixedTariffDutyRate.ZZ2_ZZ1_Tariff);
		});

		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;

		var invoiceLineOne = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
		invoiceLineOne.JI_CEI = entryInstruction.PK;
		invoiceLineOne.JI_Procedure = "A";
		invoiceLineOne.JI_PrimaryPreference = referenceData.Preference.ZZS_Preference;
		invoiceLineOne.JI_Tariff = referenceData.MixedTariff.ZZ1_TariffCode;
		invoiceLineOne.JI_SupplementaryCode1 = "EX101";
		invoiceLineOne.JI_SupplementaryCode2 = "RT200";
		invoiceLineOne.JI_CustomsQuantity = 25;
		invoiceLineOne.JI_CustomsUnitQty = "KGM";
		invoiceLineOne.JI_CustomsSecondQuantity = 24;
		invoiceLineOne.JI_CustomsSecondUnitQty = "PCS";

		var invoiceLineTwo = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
		invoiceLineTwo.JI_CEI = entryInstruction.PK;
		invoiceLineTwo.JI_Procedure = "A";
		invoiceLineTwo.JI_PrimaryPreference = referenceData.Preference.ZZS_Preference;
		invoiceLineTwo.JI_Tariff = referenceData.MixedTariff.ZZ1_TariffCode;
		invoiceLineTwo.JI_SupplementaryCode1 = "EX101";
		invoiceLineTwo.JI_SupplementaryCode2 = "RT200";
		invoiceLineTwo.JI_CustomsQuantity = 10;
		invoiceLineTwo.JI_CustomsUnitQty = "KGM";
		invoiceLineTwo.JI_CustomsSecondQuantity = 10;
		invoiceLineTwo.JI_CustomsSecondUnitQty = "PCS";
		invoiceLineTwo.JI_CustomsThirdQuantity = 10;
		invoiceLineTwo.JI_CustomsThirdUnitQty = "NMB";

		_ = declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

		var entryHeaders = declaration.ActiveEntryHeaders.Cast<CusEntryHeader>().ToArray();
		AssertEquals("Merged Entry Header Count", 1, entryHeaders.Length);

		var entryLines = entryHeaders.Single().MergedLines.Cast<CusEntryLine>().ToArray();
		AssertEquals("Entry Lines Count", 1, entryLines.Length);

		AssertEntryLineFee(entryLines[0], new[]
		{
			new ExpectedFeeRecord("EX101", 5, 34, 0.1532, "PCS", ZString.Empty),
		}, new IntegerFeeRounder());
	}

	public void TestCalculateDutiesWithMultipleAlcoholStrengths()
	{
		var referenceData = SetupReferenceData(Factory);

		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

		_ = AddNewInvoiceLineWithTestData(90, 90, 12, 550);
		_ = AddNewInvoiceLineWithTestData(50, 50, 12, 300);
		_ = AddNewInvoiceLineWithTestData(60, 150, 5, 765);

		_ = declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

		var entryHeaders = declaration.ActiveEntryHeaders.Cast<CusEntryHeader>().ToArray();
		AssertEquals("Merged Entry Header Count", 1, entryHeaders.Length);

		var entryLines = entryHeaders.Single().MergedLines.Cast<CusEntryLine>().ToArray();
		AssertEquals("Entry Lines Count", 2, entryLines.Length);

		var entryLineWithTwoInvoiceLinesHavingAlcoholStrength12 = entryLines.Single(e => e.InvoiceLines.Count == 2);
		AssertEntryLineFee(entryLineWithTwoInvoiceLinesHavingAlcoholStrength12, new []
		{
			// BaseValue: LTR (90 + 50) * ASV (12)
			new ExpectedFeeRecord("MA207", 8635, 1680, 5.14, "LTR", ZString.Empty),
			new ExpectedFeeRecord("MV1", 2371, 9485, 25, ZString.Empty, ZString.Empty),
		}, new IntegerFeeRounder());

		var entryLineWithSingleInvoiceLineHavingAlcoholStrength5 = entryLines.Single(e => e.InvoiceLines.Count == 1);
		AssertEntryLineFee(entryLineWithSingleInvoiceLineHavingAlcoholStrength5, new[]
		{
			// BaseValue: LTR (150) * ASV (5)
			new ExpectedFeeRecord("MA207", 3855, 750, 5.14, "LTR", ZString.Empty),
			new ExpectedFeeRecord("MV1", 1155, 4620, 25, ZString.Empty, ZString.Empty),
		}, new IntegerFeeRounder());

		JobComInvoiceLine AddNewInvoiceLineWithTestData(decimal kilograms, decimal liters, decimal alcoholStrength, decimal linePrice)
		{
			var invoiceLine = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			invoiceLine.JI_Procedure = "A";
			invoiceLine.JI_PrimaryPreference = referenceData.Preference.ZZS_Preference;
			invoiceLine.JI_Tariff = referenceData.ExciseTariffTwo.ZZ1_TariffCode;
			invoiceLine.JI_SupplementaryCode1 = "MA207";
			invoiceLine.JI_CustomsQuantity = kilograms;
			invoiceLine.JI_CustomsUnitQty = "KGM";
			invoiceLine.JI_CustomsSecondQuantity = liters;
			invoiceLine.JI_CustomsSecondUnitQty = "LTR";
			invoiceLine.JI_CustomsFourthQuantity = alcoholStrength;
			invoiceLine.JI_CustomsFourthUnitQty = "ASV";
			invoiceLine.JI_ZZF_NKTaxType = UniversalReferenceConstants.RefCusTaxOrFee.MV1;
			invoiceLine.JI_LinePrice = linePrice;
			return invoiceLine;
		}
	}

	public void TestCalculateDutiesAndVATWhenFlagIsFalse()
	{
		var referenceData = SetupReferenceData(Factory);
		var referenceTestDataHelper = new UniversalReferenceTestDataHelper(Factory);
		var procedure = referenceTestDataHelper.CreateRefCusProcedure(dataGroupingCode: Core.Constants.CountryCodes.Norway,
			category: ZString.Empty,
			procedureCode: "77",
			previousProcedureCode: "77",
			concession: "777",
			description: "Description",
			shipmentType: "IMP",
			intoWarehouse: true,
			calculateDuty: false);
		procedure.ZZ6_CalculateVAT = false;

		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_Procedure = "7777";
		entryInstruction.CEI_Style = "7";

		var invoiceLine = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
		invoiceLine.JI_CEI = entryInstruction.PK;
		invoiceLine.JI_PrimaryPreference = referenceData.Preference.ZZS_Preference;
		invoiceLine.JI_Tariff = referenceData.MixedTariff.ZZ1_TariffCode;
		invoiceLine.JI_SupplementaryCode1 = "RT200";
		invoiceLine.JI_CustomsQuantity = 90;
		invoiceLine.JI_CustomsUnitQty = "KGM";
		invoiceLine.JI_CustomsSecondQuantity = 90;
		invoiceLine.JI_CustomsSecondUnitQty = "LTR";
		invoiceLine.JI_LinePrice = 500;
		invoiceLine.JI_ZZF_NKTaxType = UniversalReferenceConstants.RefCusTaxOrFee.MV1;

		_ = declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

		var entryHeaders = declaration.ActiveEntryHeaders.Cast<CusEntryHeader>().ToArray();
		AssertEquals("Merged Entry Header Count", 1, entryHeaders.Length);

		var entryLines = entryHeaders.Single().MergedLines.Cast<CusEntryLine>().ToArray();
		AssertEquals("Entry Lines Count", 1, entryLines.Length);

		CombineAssertions(() =>
		{
			var entryLine = entryLines[0];
			AssertEquals("VAT amount", 0m, entryLine.VatAmount);
			AssertEquals("Duty amount", 0m, entryLine.DutyAmount);
			AssertEquals("Excise Duty amount", 0m, entryLine.ExciseDutyAmount);
		});
	}

	public void TestCalculateDuties_DateForDuty()
	{
		var referenceData = SetupReferenceData(Factory);

		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		entryInstruction.CEI_DateForDuty = ZDateTime.Now;

		var invoiceLineOne = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
		invoiceLineOne.JI_CEI = entryInstruction.PK;
		invoiceLineOne.JI_Procedure = "A";
		invoiceLineOne.JI_PrimaryPreference = referenceData.Preference.ZZS_Preference;
		invoiceLineOne.JI_Tariff = referenceData.MixedTariff.ZZ1_TariffCode;
		invoiceLineOne.JI_SupplementaryCode1 = "EX101";
		invoiceLineOne.JI_SupplementaryCode2 = "RT200";
		invoiceLineOne.JI_CustomsQuantity = 25;
		invoiceLineOne.JI_CustomsUnitQty = "KGM";
		invoiceLineOne.JI_CustomsSecondQuantity = 24;
		invoiceLineOne.JI_CustomsSecondUnitQty = "PCS";

		var invoiceLineTwo = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
		invoiceLineTwo.JI_CEI = entryInstruction.PK;
		invoiceLineTwo.JI_Procedure = "A";
		invoiceLineTwo.JI_PrimaryPreference = referenceData.Preference.ZZS_Preference;
		invoiceLineTwo.JI_Tariff = referenceData.MixedTariff.ZZ1_TariffCode;
		invoiceLineTwo.JI_SupplementaryCode1 = "EX101";
		invoiceLineTwo.JI_SupplementaryCode2 = "RT200";
		invoiceLineTwo.JI_CustomsQuantity = 10;
		invoiceLineTwo.JI_CustomsUnitQty = "KGM";
		invoiceLineTwo.JI_CustomsSecondQuantity = 10;
		invoiceLineTwo.JI_CustomsSecondUnitQty = "PCS";
		invoiceLineTwo.JI_CustomsThirdQuantity = 10;
		invoiceLineTwo.JI_CustomsThirdUnitQty = "NMB";

		AssertEquals("[Pre-Condition]: InvoiceLineOne uses CEI_DateForDuty", entryInstruction.CEI_DateForDuty, invoiceLineOne.EffectiveDateForDutyAndRate);
		AssertEquals("[Pre-Condition]: InvoiceLineTwo uses CEI_DateForDuty", entryInstruction.CEI_DateForDuty, invoiceLineTwo.EffectiveDateForDutyAndRate);

		_ = declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

		var entryHeaders = declaration.ActiveEntryHeaders.Cast<CusEntryHeader>().ToArray();
		AssertEquals("Merged Entry Header Count", 1, entryHeaders.Length);

		var entryLines = entryHeaders.Single().MergedLines.Cast<CusEntryLine>().ToArray();
		AssertEquals("Entry Lines Count", 1, entryLines.Length);

		AssertEntryLineFee(entryLines[0], [
			new ExpectedFeeRecord("EX101", 5, 34, 0.1532, "PCS", ZString.Empty)
		], new IntegerFeeRounder());
	}

	protected override DutyCalculatorStrategy GetDutyCalculatorStrategy() => new (declaration);

	protected override bool ExpectedShouldCalculateDuties => true;

	protected override void SetUp()
	{
		base.SetUp();

		declaration = Factory.New<JobDeclaration>();
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		declaration.JE_MergeBy = MergeInvoiceLinesConstants.Maximum;

		entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_Style = "4";

		invoice = declaration.Invoices.AddNew();
		invoice.JZ_RX_NKInvoice_Currency = "NOK";
	}
	JobDeclaration declaration;
	JobComInvoiceHeader invoice;
	CusEntryInstruction entryInstruction;

	static RateCalculatorDataHelper.RateCalculationReferenceData SetupReferenceData(BusinessObjectFactory factory)
	{
		RefCusTaxOrFeeHelper.CreateRefCusTaxOrFeeList(factory);
		return RateCalculatorDataHelper.SetupRefDataForDutyCalculation(factory);
	}
}
