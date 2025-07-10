using System;
using System.Linq;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.Customs.Universal.Testing;
using static Enterprise.Customs.Universal.Constants;
using static Enterprise.MasterFiles.Business.OrgConstants;
using RefCusCodeListTypesCodes = Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes;
using RefDataGroupingCodes = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes;

namespace Enterprise.Customs.PL.Business.Testing;

sealed class AESGoodsMeasureProviderTest : Customs.Business.Testing.DataProviderTestCase<AESGoodsMeasureProvider>
{
	public void TestConstructor() => AssertExceptionThrown<ArgumentNullException>("Null InvoiceLine", "Value cannot be null.\r\nParameter name: entryLine",
			() => new AESGoodsMeasureProvider(null));

	public void TestGrossMassValue() => CombineAssertions(() =>
	{
		var entryLine = GetMergedEntryLine(
			line1 => { line1.JI_Weight = 1500; line1.JI_WeightUQ = Core.Constants.Weight.Kilograms; },
			line2 => { line2.JI_Weight = 2000; line2.JI_WeightUQ = Core.Constants.Weight.Kilograms; });
		AssertEquals("Simple KG sum", 3500M, GetProvider(entryLine).GrossMassValue);

		entryLine = GetMergedEntryLine(
			line1 => { line1.JI_Weight = 1500; line1.JI_WeightUQ = Core.Constants.Weight.Kilograms; },
			line2 => { line2.JI_Weight = 2; line2.JI_WeightUQ = Core.Constants.Weight.Tonnes; });
		AssertEquals("T and KG sum", 3500M, GetProvider(entryLine).GrossMassValue);

		entryLine = GetMergedEntryLine(
			line1 => { line1.JI_Weight = 1500; line1.JI_WeightUQ = Core.Constants.Weight.Kilograms; },
			line2 => { line2.JI_Weight = 2000; });
		AssertEquals("T and not specified UQ sum", 3500M, GetProvider(entryLine).GrossMassValue);

		entryLine = GetMergedEntryLine(
			line1 => { line1.JI_Weight = 1500; line1.JI_WeightUQ = Core.Constants.Weight.Kilograms; },
			line2 => { line2.JI_Weight = 2000; line2.JI_WeightUQ = "FF"; });
		AssertEquals("T and wrong UQ sum", 1500M, GetProvider(entryLine).GrossMassValue);

		entryLine = GetMergedEntryLine(
			line1 => { line1.JI_Weight = 1500; line1.JI_WeightUQ = Core.Constants.Weight.Kilograms; },
			line2 => { line2.JI_Weight = 2000; line2.JI_WeightUQ = ZString.Empty; });
		AssertEquals("T and empty UQ sum", 1500M, GetProvider(entryLine).GrossMassValue);
	});

	public void TestGrossMassValueWithRuleR0222()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
		declaration.JE_MergeBy = MergeInvoiceLines.TariffAndDescription;

		var instruction = declaration.CustomsEntryInstructions.AddNew();
		var invoice = declaration.Invoices.AddNew();
		invoice.JZ_InvoiceNumber = "123";

		var invoiceLine1 = invoice.InvoiceLines.AddNew();
		var package = declaration.Packages.AddNew();
		var line1Package1 = invoiceLine1.PackagesForInvoiceLinesForBindingOnly.AddNew();
		invoiceLine1.JI_CEI = instruction.PK;
		invoiceLine1.JI_Description = "invoice line";
		invoiceLine1.JI_Tariff = "44123900";
		line1Package1.Package = package;
		line1Package1.IsLinked = true;
		line1Package1.PackQty = 0;

		var invoiceLine2 = invoice.InvoiceLines.AddNew();
		var line2Package2 = invoiceLine2.PackagesForInvoiceLinesForBindingOnly.AddNew();
		invoiceLine2.JI_CEI = instruction.PK;
		invoiceLine2.JI_Description = "invoice line";
		invoiceLine2.JI_Tariff = "44123900";
		line2Package2.Package = package;
		line2Package2.IsLinked = true;
		line2Package2.PackQty = 0;

		invoiceLine1.JI_Weight = 1500;
		invoiceLine1.JI_WeightUQ = Core.Constants.Weight.Kilograms;
		invoiceLine2.JI_Weight = 2000;
		invoiceLine2.JI_WeightUQ = Core.Constants.Weight.Kilograms;

		var lineMerger = new LineMerger(declaration);
		lineMerger.DoMerge();
		var entryHeader = declaration.CustomsEntryHeaders.First();
		var entryLine = entryHeader.AllEntryLines.FirstOrDefault();

		CombineAssertions(() =>
		{
			AssertEquals("Number of Packages is zero", Decimal.Zero, GetProvider(entryLine).GrossMassValue);

			line1Package1.PackQty = 10;
			lineMerger.DoMerge();
			entryHeader = declaration.CustomsEntryHeaders.First();
			entryLine = entryHeader.AllEntryLines.FirstOrDefault();
			AssertEquals("The number of packages in first entryLine is not zero, the gross value should sum all of the invoiceLine which has 0 packages", 3500M, GetProvider(entryLine).GrossMassValue);

			line2Package2.PackQty = 5;
			invoiceLine2.JI_Description = "invoice line 2";
			lineMerger.DoMerge();
			entryHeader = declaration.CustomsEntryHeaders.First();
			entryLine = entryHeader.AllEntryLines.FirstOrDefault();
			AssertEquals("The number of packages in first entryLine is not zero, but the second entryLine is also not zero", 1500M, GetProvider(entryLine).GrossMassValue);

			var invoiceLine3 = invoice.InvoiceLines.AddNew();
			var line3Package3 = invoiceLine3.PackagesForInvoiceLinesForBindingOnly.AddNew();
			invoiceLine3.JI_CEI = instruction.PK;
			invoiceLine3.JI_Description = "invoice line 3";
			invoiceLine3.JI_Tariff = "44123900";
			line3Package3.Package = package;
			line3Package3.IsLinked = true;
			line3Package3.PackQty = 0;
			invoiceLine3.JI_Weight = 444;
			invoiceLine3.JI_WeightUQ = Core.Constants.Weight.Kilograms;
			lineMerger.DoMerge();
			entryHeader = declaration.CustomsEntryHeaders.First();
			entryLine = entryHeader.AllEntryLines.FirstOrDefault();
			AssertEquals("The number of packages in first entryLine is not zero, the gross value should also sum invoice line under other entryLine", 1944M, GetProvider(entryLine).GrossMassValue);
		});
	}

	public void TestRuleC0060()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypesCodes.UnitedNationsPackageTypes, "A");
		helper.CreateCusCodeListWithAttribute(RefDataGroupingCodes.UnitedNationsRecommendations, RefCusCodeListTypesCodes.UnitedNationsPackageTypes,
			"VO", "VO", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Customs.Business.UniversalReferenceConstants.PackageUnitAttributes.Bulk, "");
		helper.CreateNewOrGetExistingDataGrouping(RefDataGroupingCodes.UnitedNationsRecommendations);

		Factory.Save();

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
		declaration.JE_MergeBy = MergeInvoiceLines.TariffAndDescription;

		var instruction = declaration.CustomsEntryInstructions.AddNew();
		var invoice = declaration.Invoices.AddNew();
		invoice.JZ_InvoiceNumber = "123";

		var invoiceLine1 = invoice.InvoiceLines.AddNew();
		var package = declaration.Packages.AddNew();
		package.CW_PackType =  "VO";
		var line1Package1 = invoiceLine1.PackagesForInvoiceLinesForBindingOnly.AddNew();
		invoiceLine1.JI_CEI = instruction.PK;
		invoiceLine1.JI_Description = "invoice line 1";
		invoiceLine1.JI_Tariff = "44123900";
		line1Package1.Package = package;
		line1Package1.IsLinked = true;
		line1Package1.PackQty = 0;

		var invoiceLine2 = invoice.InvoiceLines.AddNew();
		var line1Package2 = invoiceLine2.PackagesForInvoiceLinesForBindingOnly.AddNew();
		invoiceLine2.JI_CEI = instruction.PK;
		invoiceLine2.JI_Description = "invoice line 1";
		invoiceLine2.JI_Tariff = "44123900";
		line1Package2.Package = package;
		line1Package2.IsLinked = true;
		line1Package2.PackQty = 0;

		invoiceLine1.JI_Weight = 1500;
		invoiceLine1.JI_WeightUQ = Core.Constants.Weight.Kilograms;
		invoiceLine2.JI_Weight = 2000;
		invoiceLine2.JI_WeightUQ = Core.Constants.Weight.Kilograms;

		var lineMerger = new LineMerger(declaration);
		lineMerger.DoMerge();
		var entryHeader = declaration.CustomsEntryHeaders.First();
		var entryLine = entryHeader.AllEntryLines.FirstOrDefault();
		AssertEquals("The package is bulk, RuleR0222 is not applied", 3500M, GetProvider(entryLine).GrossMassValue);

		package.CW_PackType = "WW";
		lineMerger = new LineMerger(declaration);
		lineMerger.DoMerge();
		entryHeader = declaration.CustomsEntryHeaders.First();
		entryLine = entryHeader.AllEntryLines.FirstOrDefault();
		AssertEquals("The package type is not bulk, RuleR0222 is applied", Decimal.Zero, GetProvider(entryLine).GrossMassValue);
	}

	public void TestNetMass() => CombineAssertions(() =>
	{
		var entryLine = GetMergedEntryLine(
			line1 => { line1.JI_CustomsQuantity = 1500; line1.JI_CustomsUnitQty = Customs.Business.UniversalReferenceConstants.RefCusCodeList.CustomsUq.Weight.Kilogram; },
			line2 => { line2.JI_CustomsQuantity = 2000; line2.JI_CustomsUnitQty = Customs.Business.UniversalReferenceConstants.RefCusCodeList.CustomsUq.Weight.Kilogram; });
		AssertEquals("KG sum", 3500M, GetProvider(entryLine).NetMass);

		entryLine = GetMergedEntryLine(
			line1 => { line1.JI_CustomsQuantity = 15; line1.JI_CustomsUnitQty = Customs.Business.UniversalReferenceConstants.RefCusCodeList.CustomsUq.Weight.Tonne; },
			line2 => { line2.JI_CustomsQuantity = 2; line2.JI_CustomsUnitQty = Customs.Business.UniversalReferenceConstants.RefCusCodeList.CustomsUq.Weight.Tonne; });
		AssertEquals("T sum", 17000M, GetProvider(entryLine).NetMass);
	});

	public void TestSupplementaryUnitsValue()
	{
		SetupTariffData();

		CusEntryLine entryLine;

		CombineAssertions(() =>
		{
			foreach (ICodeDescription subStyle in new EntrySubStyleList())
			{
				entryLine = GetMergedEntryLine(
					line1 => { line1.JI_CustomsSecondQuantity = ZDecimal.Zero; line1.JI_Tariff = "22222222"; },
					line2 => { line2.JI_CustomsSecondQuantity = ZDecimal.Zero; line2.JI_Tariff = "22222222"; },
					entryInstruction => entryInstruction.CEI_SubStyle = subStyle.Code);
				AssertNull($"NO CU2 - {subStyle.Description}", GetProvider(entryLine).SupplementaryUnitsValue);

				entryLine = GetMergedEntryLine(
					line1 => { line1.JI_CustomsSecondQuantity = ZDecimal.Zero; line1.JI_Tariff = "11111111"; },
					line2 => { line2.JI_CustomsSecondQuantity = ZDecimal.Zero; line2.JI_Tariff = "11111111"; },
					entryInstruction => entryInstruction.CEI_SubStyle = subStyle.Code);
				AssertEquals($"CU2 exists - {subStyle.Description}", ZDecimal.Zero, GetProvider(entryLine).SupplementaryUnitsValue);
			}

			entryLine = GetMergedEntryLine(
				line1 => { line1.JI_CustomsSecondQuantity = ZDecimal.Zero; line1.JI_Tariff = "11111111"; },
				line2 => { line2.JI_CustomsSecondQuantity = ZDecimal.Zero; line2.JI_Tariff = "11111111"; },
				entryInstruction => entryInstruction.CEI_SubStyle = ZString.Empty);
			AssertEquals("Empty SubStyle", ZDecimal.Zero, GetProvider(entryLine).SupplementaryUnitsValue);
		});
	}

	void SetupTariffData()
	{
		var groupingCode = Core.Constants.CountryCodes.Poland;
		var helper = new UniversalReferenceTestDataHelper(Factory);
		var tariffType = helper.CreateNewOrGetExistingTariffType(groupingCode, TariffTypes.Export);
		Factory.Save();

		var tariff1 = helper.CreateTariff(groupingCode, tariffType.PK, "11111111", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
		helper.CreateTariffUOM(tariff1, UnitOfMeasureTypes.StatisticalUOMType, "ABC");
		helper.CreateTariffUOM(tariff1, UnitOfMeasureTypes.AdditionalUOMType, "QWE");

		var tariff2 = helper.CreateTariff(groupingCode, tariffType.PK, "22222222", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
		helper.CreateTariffUOM(tariff2, UnitOfMeasureTypes.StatisticalUOMType, "CU1");
		Factory.Save();
	}

	CusEntryLine GetMergedEntryLine(
		Action<JobComInvoiceLine> invoiceLine1Preparation = null,
		Action<JobComInvoiceLine> invoiceLine2Preparation = null,
		Action<CusEntryInstruction> entryInstructionPreparation = null)
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
		declaration.JE_MergeBy = MergeInvoiceLines.TariffAndDescription;

		var instruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstructionPreparation?.Invoke(instruction);
		var invoice = declaration.Invoices.AddNew();
		invoice.JZ_InvoiceNumber = "123";

		var invoiceLine1 = invoice.InvoiceLines.AddNew();
		invoiceLine1.JI_CEI = instruction.PK;
		invoiceLine1.JI_Description = "invoice line 1";
		invoiceLine1.JI_Tariff = "44123900";

		var invoiceLine2 = invoice.InvoiceLines.AddNew();
		invoiceLine2.JI_CEI = instruction.PK;
		invoiceLine2.JI_Description = "invoice line 1";
		invoiceLine2.JI_Tariff = "44123900";

		invoiceLine1Preparation?.Invoke(invoiceLine1);
		invoiceLine2Preparation?.Invoke(invoiceLine2);

		var lineMerger = new LineMerger(declaration);
		lineMerger.DoMerge();
		var entryHeader = declaration.CustomsEntryHeaders.First();

		return entryHeader.AllEntryLines.FirstOrDefault();
	}

	AESGoodsMeasureProvider GetProvider(CusEntryLine entryLine) => new(entryLine);

	protected override AESGoodsMeasureProvider GetProvider() => new(GetMergedEntryLine());
}
