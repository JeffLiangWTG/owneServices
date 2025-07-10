using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using static Enterprise.Customs.NO.Business.JobComInvoiceLine;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.NO.Business.Testing;

[TestedType(typeof(JobComInvoiceLine))]
sealed class JobComInvoiceLineTest : BaseJobComInvoiceLineAbstractTest
{
	new JobComInvoiceLine InvoiceLine => (JobComInvoiceLine)base.InvoiceLine;

	public void TestCustomsCountryCode()
	{
		AssertEquals("CustomsCountryCodeCore should be NO", Core.Constants.CountryCodes.Norway, InvoiceLine.CustomsCountryCode);
	}

	public void TestIInvoiceLinePartDetailsMembers()
	{
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
		{
			IInvoiceLinePartDetails partDetails = Factory.New<JobComInvoiceLine>();
			AssertEquals(Core.Constants.CountryCodes.Norway, partDetails.CustomsCountryCode);
			AssertEquals(typeof(OrgSupplierPart), partDetails.TypeOfPartUsed);
		}
	}

	public void TestTotalOtherChargesInNOKCaption() => CombineAssertions(() =>
		AssertEntity<JobComInvoiceLine>()
			.HasProperty(x => x.TotalOtherChargesInNOK)
			.WithCaption("Other Charges")
			.WithFullDescription("Other charges for current line item."));

	public void TestTotalDeductionsInNOKCaption() => CombineAssertions(() =>
		AssertEntity<JobComInvoiceLine>()
			.HasProperty(x => x.TotalDeductionsInNOK)
			.WithCaption("Deductions")
			.WithFullDescription("Total deductions for current line item."));

	public void TestJI_ProcedureCaption() => CombineAssertions(() =>
		AssertEntity<JobComInvoiceLine>()
			.HasProperty(x => x.JI_Procedure)
			.WithCaption("Procedure Code"));

	public void TestJI_ReducedCustomsFlag() => CombineAssertions(() =>
		AssertEntity<JobComInvoiceLine>()
			.HasProperty(x => x.JI_ReducedCustomsFlag)
			.WithMaxLength(1)
			.WithCaption("Reduced custom")
			.WithFullDescription("Special exemption/duty reduction"));

	public void TestJI_MergeOverride() => CombineAssertions(() =>
		AssertEntity<JobComInvoiceLine>()
			.HasProperty(x => x.JI_MergeOverride)
			.WithMaxLength(10)
			.WithCaption("Merge override")
			.WithFullDescription("Add value (of own choice) here to avoid merging (when all other merge-fields are equal). If the same value is added to multiple lines, they will be merged (when all other merge-fields are equal)."));

	public void TestJI_PackageType() => CombineAssertions(() =>
		AssertEntity<JobComInvoiceLine>()
			.HasProperty(x => x.JI_PackageType)
			.WithMaxLength(1)
			.WithCaption("Beverage packing type")
			.WithShortCaption("Packing type")
			.WithFullDescription("Filter the beverage packing excise duty codes by selecting pack type."));

	public void TestJI_Calc_DutyAmountIncludingWHEstimateCaption() => CombineAssertions(() =>
		AssertEntity<JobComInvoiceLine>()
			.HasProperty(x => x.JI_Calc_DutyAmountIncludingWHEstimate)
			.WithCaption("Customs Duty")
			.WithFullDescription("Customs duties for current line item."));

	public void TestJI_LinePriceInLocalCurrencyCaption() => CombineAssertions(() =>
		AssertEntity<JobComInvoiceLine>()
			.HasProperty(x => x.JI_LinePriceInLocalCurrency)
			.WithCaption("Inv. Line Value")
			.WithFullDescription("Invoice line value for current line item."));

	public void TestTotalExciseDutiesCaption() => CombineAssertions(() =>
		AssertEntity<JobComInvoiceLine>()
			.HasProperty(x => x.TotalExciseDuties)
			.WithCaption("Excise Duties")
			.WithFullDescription("Excise duties for current line item."));

	public void TestJI_GoodsMarks() => CombineAssertions(() =>
		AssertEntity<JobComInvoiceLine>()
			.HasProperty(x => x.JI_GoodsMarks)
			.WithMaxLength(28)
			.WithCaption("Goods Marks")
			.WithMediumCaption("Goods Item Marks")
			.WithFullDescription("Specific goods item marks. This is to help customs identify the goods item in case of a physical control. If left blank “ADRESSE” will be sent to customs/printed on SAD."));

	public void TestTotalExciseDutiesCached()
	{
		var data = RateCalculatorDataHelper.SetupRefDataForDutyCalculation(Factory);
		AssertEquals("[Pre-Condition] Excise Tariff One and Rate MB200", data.ExciseTariffTwo.PK, data.MA207Rate.ZZ2_ZZ1_Tariff);

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

		var invoiceLine = (JobComInvoiceLine)declaration.Invoices.AddNew().InvoiceLines.AddNew();
		invoiceLine.JI_PrimaryPreference = data.Preference.ZZS_Preference;
		invoiceLine.JI_Tariff = data.ExciseTariffTwo.ZZ1_TariffCode;
		invoiceLine.JI_SupplementaryCode1 = "MA207";

		invoiceLine.JI_CustomsThirdQuantity = 10;
		invoiceLine.JI_CustomsThirdUnitQty = "LTR";

		invoiceLine.JI_CustomsFourthQuantity = 1;
		invoiceLine.JI_CustomsFourthUnitQty = "ASV";

		var totalExciseDuties = invoiceLine.TotalExciseDuties;
		CombineAssertions(() =>
		{
			AssertEquals("Calculated Value", 51m, totalExciseDuties);
			AssertEquals("Cached Value", totalExciseDuties, invoiceLine.TotalExciseDuties);
		});
	}

	public void TestPartType()
	{
		var factory2 = new BusinessObjectFactory();
		var importer = factory2.New<OrgHeader>();
		importer.FillWithValidTestData();
		var product = (MasterFiles.Business.OrgSupplierPart)factory2.New<AU.IOrgSupplierPart>();
		product.OP_PartNum = "TestTEST";
		product.RelatedOrganisations.AddOrganisationIfNotExist(importer.PK, OrgPartRelation.RelationshipTypes.Owner);
		factory2.Save();

		var customsTemplate_Company = Factory.New<GlbCompany>();
		customsTemplate_Company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Norway;
		var customsTemplate_Branch = customsTemplate_Company.Branches.AddNew();
		customsTemplate_Branch.GB_RL_NKHomePort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, Core.Constants.CountryCodes.Norway)).RL_Code;

		declaration.JE_OH_Importer = importer.PK;
		declaration.JE_GB = customsTemplate_Branch.PK;
		invoiceLine.JI_PartNo = "TestTEST";
		AssertType<OrgSupplierPart>("Product type gets changed depending on who is requesting", invoiceLine.Part);
		Factory.Save();

		GlbCompany.CurrentCompany.SetCountry("AU");
		var factory3 = new BusinessObjectFactory();
		var declarationLoaded = factory3.Load<JobDeclaration>(declaration.PK);
		AssertType<OrgSupplierPart>("product type still the type", declarationLoaded.InvoiceLines[0].Part);
	}

	public void TestJI_PrimaryPreference_CreateOrDeleteDefaultSupportingDoc()
	{
		CombineAssertions(() =>
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			invoiceLine.JI_PrimaryPreference = ZString.Empty;
			AssertEquals("Export, Empty", expected: false, HasAnySupportingDocumentWithTypeSER());

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			invoiceLine.JI_PrimaryPreference = PrimaryPreferenceCodeList.Codes.A;
			AssertEquals("Import, 'A'", expected: true, HasAnySupportingDocumentWithTypeSER());

			invoiceLine.JI_PrimaryPreference = ZString.Empty;
			AssertEquals("Import, changed to Empty", expected: true, HasAnySupportingDocumentWithTypeSER());

			invoiceLine.JI_PrimaryPreference = PrimaryPreferenceCodeList.Codes.N;
			AssertEquals("Import, changed to 'N'", expected: false, HasAnySupportingDocumentWithTypeSER());

			invoiceLine.JI_PrimaryPreference = ZString.Empty;
			AssertEquals("Import, Empty", expected: false, HasAnySupportingDocumentWithTypeSER());
		});

		bool HasAnySupportingDocumentWithTypeSER() => invoiceLine.SupportingDocuments.Cast<SupportingDocument>().Any(x => x.CSI_Code == SupportingDocumentCodeList.CertificateForOrigin);
	}

	public void TestJI_PrimaryPreferenceExport_Replace_Blank()
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		invoiceLine.JI_PrimaryPreference = ZString.Empty;
		Factory.Save();
		AssertEquals("N", invoiceLine.JI_PrimaryPreference);
	}

	public void TestJI_PrimaryPreferenceImport_Logic()
	{
		RefCusProcedureHelper.CreateCusPreference(Factory);

		const string TARIFF_CODE_1 = "66666666";
		const string TARIFF_CODE_2 = "77777777";
		const string TARIFF_CODE_3 = "88888888";
		const string TARIFF_CODE_4 = "99999999";
		var tariffTestHelper = new RefCusTariffTestHelper(Factory);
		var tariffTest1 = tariffTestHelper.CreateImportTariff(TARIFF_CODE_1);
		tariffTestHelper.AddRate(tariffTest1, PrimaryPreferenceCodeList.Codes.A, "1", Core.Constants.CountryCodes.Sweden, Core.Constants.CountryCodes.AlandIslands);
		tariffTestHelper.AddRate(tariffTest1, PrimaryPreferenceCodeList.Codes.G, "2", Core.Constants.CountryCodes.Sweden, Core.Constants.CountryCodes.Brazil);
		tariffTestHelper.AddRate(tariffTest1, PrimaryPreferenceCodeList.Codes.C, "3", Core.Constants.CountryCodes.Sweden, Core.Constants.CountryCodes.Brazil, Core.Constants.CountryCodes.Denmark);
		tariffTestHelper.AddRate(tariffTest1, PrimaryPreferenceCodeList.Codes.B, "3", Core.Constants.CountryCodes.Sweden, Core.Constants.CountryCodes.Brazil, Core.Constants.CountryCodes.Denmark);
		tariffTestHelper.AddGeneralRate(tariffTest1, PrimaryPreferenceCodeList.Codes.N, "15");
		var tariffTest2 = tariffTestHelper.CreateImportTariff(TARIFF_CODE_2);
		tariffTestHelper.AddRate(tariffTest2, PrimaryPreferenceCodeList.Codes.G, "4", Core.Constants.CountryCodes.Brazil);
		tariffTestHelper.AddGeneralRate(tariffTest2, PrimaryPreferenceCodeList.Codes.N, "15");
		var tariffTest3 = tariffTestHelper.CreateImportTariff(TARIFF_CODE_3);
		tariffTestHelper.AddRate(tariffTest3, PrimaryPreferenceCodeList.Codes.A, "5", Core.Constants.CountryCodes.Sweden);
		tariffTestHelper.AddRate(tariffTest3, PrimaryPreferenceCodeList.Codes.B, "0", Core.Constants.CountryCodes.Sweden);
		tariffTestHelper.AddGeneralRate(tariffTest3, PrimaryPreferenceCodeList.Codes.N, "15");
		var tariffTest4 = tariffTestHelper.CreateImportTariff(TARIFF_CODE_4);
		tariffTestHelper.AddRate(tariffTest4, PrimaryPreferenceCodeList.Codes.A, "0", Core.Constants.CountryCodes.Sweden);
		tariffTestHelper.AddRate(tariffTest4, PrimaryPreferenceCodeList.Codes.B, "5", Core.Constants.CountryCodes.Sweden);
		tariffTestHelper.AddGeneralRate(tariffTest4, PrimaryPreferenceCodeList.Codes.N, "15");

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		var invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();

		CombineAssertions(() =>
		{
			TriggerAutoSuggestion(ZString.Empty, ZString.Empty);
			AssertJI_PrimaryPreference("no tariff, no country", ZString.Empty);

			TriggerAutoSuggestion(TARIFF_CODE_1, ZString.Empty);
			AssertJI_PrimaryPreference("no country", ZString.Empty);

			TriggerAutoSuggestion(ZString.Empty, Core.Constants.CountryCodes.Sweden);
			AssertJI_PrimaryPreference("no tariff", ZString.Empty);

			TriggerAutoSuggestion(TARIFF_CODE_1, Core.Constants.CountryCodes.Mexico);
			AssertJI_PrimaryPreference("rate is zero or no rate is found", "N");

			TriggerAutoSuggestion(TARIFF_CODE_1, Core.Constants.CountryCodes.Sweden, ReducedCustomsFlagList.Codes.S);
			AssertJI_PrimaryPreference("reducedCustoms is 'S'", "N");

			TriggerAutoSuggestion(TARIFF_CODE_2, Core.Constants.CountryCodes.Brazil);
			AssertJI_PrimaryPreference("based on tradegroup", "G");

			TriggerAutoSuggestion(TARIFF_CODE_1, Core.Constants.CountryCodes.Brazil);
			AssertJI_PrimaryPreference("multiple tradegroups", "G");

			TriggerAutoSuggestion(TARIFF_CODE_1, Core.Constants.CountryCodes.Sweden);
			AssertJI_PrimaryPreference("multiple tradegroups, one is EU, but 'B' is not lowest rate", "A");

			TriggerAutoSuggestion(TARIFF_CODE_1, Core.Constants.CountryCodes.Denmark);
			AssertJI_PrimaryPreference("multiple tradegroups, one is EU", "B");

			TriggerAutoSuggestion(TARIFF_CODE_3, Core.Constants.CountryCodes.Sweden);
			AssertJI_PrimaryPreference("country has rate preference where 'B' is zero", "B");

			TriggerAutoSuggestion(TARIFF_CODE_4, Core.Constants.CountryCodes.Sweden);
			AssertJI_PrimaryPreference("country has rate preference where 'A' is zero", "A");
		});

		void TriggerAutoSuggestion(ZString tariff, ZString countryOfOrigin, ZString? reducedCustomsFlag = null)
		{
			invoiceLine.JI_Tariff = ZString.Empty;
			invoiceLine.JI_CountryOfOrigin = ZString.Empty;
			invoiceLine.JI_PrimaryPreference = ZString.Empty;
			invoiceLine.JI_ReducedCustomsFlag = reducedCustomsFlag ?? ZString.Empty;
			invoiceLine.JI_Tariff = tariff;
			invoiceLine.JI_CountryOfOrigin = countryOfOrigin;
		}

		void AssertJI_PrimaryPreference(string message, string expected)
		{
			var assertMessage = $"[{invoiceLine.JI_Tariff}-{invoiceLine.JI_CountryOfOrigin}] Preference should be '{expected}', {message}";
			AssertEquals(assertMessage, expected, invoiceLine.JI_PrimaryPreference);
		}
	}

	public void TestJI_PrimaryPreferenceJ_SimulatesN()
	{
		invoiceLine.JI_PrimaryPreference = PrimaryPreferenceCodeList.Codes.J;
		AssertEquals("Preference J should be simulated as N", PrimaryPreferenceCodeList.Codes.N, invoiceLine.EffectivePrimaryPreference);
	}

	public void TestCalculateNetWeight()
	{
		var factor = JobComInvoiceLine.CalculateBetweenNetWeightAndGrossWeightFactor;
		var numFiveSixSeven = 567m;
		var numFiveSixSevenTimesFactor = ZArchitecture.Core.Utilities.Round(numFiveSixSeven * factor, 3);

		CombineAssertions(() =>
		{
			invoiceLine.JI_NetWeight = 0;
			invoiceLine.JI_WeightUQ = "G";
			invoiceLine.JI_Weight = 600m;
			AssertEquals("NetWeight is not calculated", 0m, invoiceLine.JI_NetWeight);
			AssertNotEquals("UQ for Gross and NetWeight are different", invoiceLine.JI_NetWeightUQ, invoiceLine.JI_WeightUQ);

			invoiceLine.JI_WeightUQ = "KG";
			invoiceLine.JI_Weight = numFiveSixSeven;
			AssertEquals("NetWeight is calculated", numFiveSixSevenTimesFactor, invoiceLine.JI_NetWeight);
			AssertEquals("UQ for Gross and NetWeight are identical", invoiceLine.JI_NetWeightUQ, invoiceLine.JI_WeightUQ);

			invoiceLine.JI_Weight = 600m;
			AssertEquals("NetWeight is is not re-calculated", numFiveSixSevenTimesFactor, invoiceLine.JI_NetWeight);
		});
	}

	public void TestCalculateGrossWeight()
	{
		var factor = 1 / JobComInvoiceLine.CalculateBetweenNetWeightAndGrossWeightFactor;
		var numFiveSixSeven = 567m;
		var numFiveSixSevenTimesFactor = ZArchitecture.Core.Utilities.Round(numFiveSixSeven * factor, 3);

		CombineAssertions(() =>
		{
			invoiceLine.JI_Weight = 0;
			invoiceLine.JI_NetWeightUQ = "G";
			invoiceLine.JI_NetWeight = 600m;
			AssertEquals("GrossWeight is not calculated", 0m, invoiceLine.JI_Weight);
			AssertNotEquals("UQ for Gross and NetWeight are different", invoiceLine.JI_WeightUQ, invoiceLine.JI_NetWeightUQ);

			invoiceLine.JI_NetWeightUQ = "KG";
			invoiceLine.JI_NetWeight = numFiveSixSeven;
			AssertEquals("GrossWeight is calculated", numFiveSixSevenTimesFactor, invoiceLine.JI_Weight);
			AssertEquals("UQ for Gross and NetWeight are identical", invoiceLine.JI_WeightUQ, invoiceLine.JI_NetWeightUQ);

			invoiceLine.JI_NetWeight = 600m;
			AssertEquals("GrossWeight is is not re-calculated", numFiveSixSevenTimesFactor, invoiceLine.JI_Weight);
		});
	}

	public void TestEntryInstructionDescription()
	{
		var instruction = declaration.CustomsEntryInstructions.AddNew();
		invoiceLine.JI_CEI = instruction.PK;
		instruction.CEI_Description = "Test description";

		AssertEquals("Test description", invoiceLine.EntryInstructionDescription);
	}

	public void TestStateCodeUpdatedOnChangingOriginCountry()
	{
		CombineAssertions(() =>
		{
			InvoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Norway;
			AssertEquals("Default value for Norway", ZString.Empty, InvoiceLine.JI_StateOrRegionOfOrigin);
			InvoiceLine.JI_StateOrRegionOfOrigin = "03";
			InvoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Sweden;
			AssertEquals("Not Norway", "91", InvoiceLine.JI_StateOrRegionOfOrigin);
			InvoiceLine.JI_CountryOfOrigin = ZString.Empty;
			AssertEquals("Empty country", "91", InvoiceLine.JI_StateOrRegionOfOrigin);
		});
	}

	public void TestJI_Calc_GSTVATAmountIncludingWHEstimate_EXPORT_ShouldEqualZero()
	{
		var invoiceLine = Factory.New<JobComInvoiceLine_ForTesting>();
		invoiceLine.SetDeclarationForTesting(declaration);
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		CombineAssertions(() =>
		{
			invoiceLine.JI_Calc_CIF_Override = 9001;
			invoiceLine.JI_Calc_DutyAmount_Override = 420;

			AssertZDecimalEquals("MVA Value", 0, invoiceLine.JI_Calc_GSTVATAmountIncludingWHEstimate);
		});
	}

	[TestDate(2025, 4, 11)]
	public void TestJI_Calc_GSTVATAmountIncludingWHEstimate_IMPORT_ShouldEqualTotalVAT()
	{
		var invoiceLine = (JobComInvoiceLine_ForTesting)invoiceHeader.JobComInvoiceLines.AddNew((typeof(JobComInvoiceLine_ForTesting)));
		const decimal exchangeRate = 10m;
		var data = RateCalculatorDataHelper.SetupRefDataForDutyCalculation(Factory);
		RefCusTaxOrFeeHelper.CreateRefCusTaxOrFeeList(Factory);
		invoiceLine.JI_ZZF_NKTaxType = UniversalReferenceConstants.RefCusTaxOrFee.MV1;
		AssertNotNull("[PRE-CONDITION] Applied Tax and Fee for MV1", invoiceLine.AppliedTaxAndFee);
		invoiceLine.JI_ZZF_NKTaxType = UniversalReferenceConstants.RefCusTaxOrFee.MV2;
		AssertNotNull("[PRE-CONDITION] Applied Tax and Fee for MV2", invoiceLine.AppliedTaxAndFee);

		CurrencyConverterTestHelper.SetExchangeRate(Factory, Core.Constants.CurrencyCodes.EuropeanUnion, exchangeRate);
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.EuropeanUnion;
		invoiceLine.JI_LinePrice = 2000m;

		CombineAssertions(() =>
		{
			AssertNotNull(invoiceLine.CurrencyConverter);
			AssertNotNull(invoiceHeader.CurrencyConverter);
			invoiceLine.JI_PrimaryPreference = data.Preference.ZZS_Preference;
			invoiceLine.JI_Tariff = data.ExciseTariffTwo.ZZ1_TariffCode;
			invoiceLine.JI_SupplementaryCode1 = "MA207";

			invoiceLine.JI_CustomsThirdQuantity = 10;
			invoiceLine.JI_CustomsThirdUnitQty = "LTR";

			invoiceLine.JI_CustomsFourthQuantity = 1;
			invoiceLine.JI_CustomsFourthUnitQty = "ASV";
			invoiceLine.CustomsRateType = RateTypeCodeList.Codes.PercentSign;
			invoiceLine.CustomsRate = 25m;
			invoiceLine.JI_Calc_CIF_Override = 199;
			invoiceHeader.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;

			invoiceLine.JI_ZZF_NKTaxType = "MV1";
			AssertZDecimalEquals("(mv1): MVA Value", 75m, invoiceLine.JI_Calc_GSTVATAmountIncludingWHEstimate);

			invoiceLine.JI_ZZF_NKTaxType = "MV2";
			AssertZDecimalEquals("(mv2): MVA Value", 45m, invoiceLine.JI_Calc_GSTVATAmountIncludingWHEstimate);
		});
	}

	[TestDate(2025, 4, 11)]
	public void TestJI_Calc_GSTVATAmountIncludingWHEstimate_IMPORT_ShouldEqualZero()
	{
		const decimal exchangeRate = 10m;
		RefCusTaxOrFeeHelper.CreateRefCusTaxOrFeeList(Factory);
		invoiceLine.JI_ZZF_NKTaxType = UniversalReferenceConstants.RefCusTaxOrFee.MV1;
		AssertNotNull("[PRE-CONDITION] Applied Tax and Fee for MV1", invoiceLine.AppliedTaxAndFee);

		CurrencyConverterTestHelper.SetExchangeRate(Factory, Core.Constants.CurrencyCodes.EuropeanUnion, exchangeRate);

		var referenceTestDataHelper = new UniversalReferenceTestDataHelper(Factory);
		var procedure = referenceTestDataHelper.CreateRefCusProcedure(dataGroupingCode: Core.Constants.CountryCodes.Norway,
			category: ZString.Empty,
			procedureCode: "77",
			previousProcedureCode: "77",
			concession: "777",
			description: "Description",
			shipmentType: "IMP");
		procedure.ZZ6_CalculateVAT = false;

		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_Procedure = "7777";

		invoiceLine.JI_CEI = entryInstruction.PK;
		invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.EuropeanUnion;
		invoiceLine.JI_LinePrice = 2000m;

		CombineAssertions(() =>
		{
			var basis = invoiceLine.JI_LinePrice / exchangeRate;
			AssertNotNull(invoiceLine.CurrencyConverter);
			AssertNotNull(invoiceHeader.CurrencyConverter);
			invoiceLine.JI_ZZF_NKTaxType = "MV1";
			AssertZDecimalEquals("(mv1): MVA Value", 0m, invoiceLine.JI_Calc_GSTVATAmountIncludingWHEstimate);
		});
	}

	public void TestJI_Calc_ValueForVAT()
	{
		invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Norway;
		invoiceLine.JI_LinePrice = 420m;
		CombineAssertions(() =>
		{
			invoiceLine.JI_ZZF_NKTaxType = UniversalReferenceConstants.RefCusTaxOrFee.MV1;
			AssertEquals("Base value for Non-Art (tax code MV1) is 100%", 420m, invoiceLine.JI_Calc_ValueForVat);

			invoiceLine.JI_ZZF_NKTaxType = UniversalReferenceConstants.RefCusTaxOrFee.MVK;
			AssertEquals("Base value for Art (tax code MVK) is 20%", 84m, invoiceLine.JI_Calc_ValueForVat);
		});
	}

	public void TestJI_ZZF_NKTaxTypeUpdatedToEmptyWhenChangedToExport()
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		invoiceLine.JI_ZZF_NKTaxType = "MV1";
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		AssertEquals(ZString.Empty, invoiceLine.JI_ZZF_NKTaxType);
	}

	public void TestJI_StateOrRegionOfOrigin_ReadOnly()
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		CombineAssertions(() =>
		{
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Denmark;
			AssertEquals("DK - Should NOT be ReadOnly", expected: false, invoiceLine.JI_StateOrRegionOfOriginInfo.ReadOnly);
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Norway;
			AssertEquals("NO - Should NOT be ReadOnly", expected: false, invoiceLine.JI_StateOrRegionOfOriginInfo.ReadOnly);
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Sweden;
			AssertEquals("Empty Country - Should NOT be ReadOnly", expected: false, invoiceLine.JI_StateOrRegionOfOriginInfo.ReadOnly);
		});
	}

	public void TestJI_CountryOfOriginIsAutoPopulated()
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		AssertEquals("Country Of Origin default should be NO", "NO", invoiceLine.JI_CountryOfOrigin);
	}

	public void TestJI_StateOrRegionOfOriginIsAutopopulated()
	{
		var supplierNoCounty = Factory.NewWithValidTestData<OrgHeader>();
		supplierNoCounty.MainAddress.OA_RN_NKCountryCode = "NO";
		supplierNoCounty.MainAddress.OA_State = ZString.Empty;

		var supplierWithCounty = Factory.NewWithValidTestData<OrgHeader>();
		supplierWithCounty.MainAddress.OA_RN_NKCountryCode = "NO";
		supplierWithCounty.MainAddress.OA_State = "03";

		var supplierForeign = Factory.NewWithValidTestData<OrgHeader>();
		supplierForeign.MainAddress.OA_RN_NKCountryCode = "US";

		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;

		CombineAssertions(() =>
		{
			declaration.JE_OH_Supplier = supplierNoCounty.PK;
			AssertEquals("supplierNoCounty: No change to existing invoice if we change supplier", ZString.Empty, invoiceLine.JI_StateOrRegionOfOrigin);
			var invoiceLine2 = invoiceHeader.JobComInvoiceLines.AddNew();
			AssertEquals("supplierNoCounty: Chosen supplier do not have state info", ZString.Empty, invoiceLine2.JI_StateOrRegionOfOrigin);

			declaration.JE_OH_Supplier = supplierWithCounty.PK;
			AssertEquals("supplierWithCounty: No change to existing invoice if we change supplier", ZString.Empty, invoiceLine.JI_StateOrRegionOfOrigin);
			invoiceLine2 = invoiceHeader.JobComInvoiceLines.AddNew();
			AssertEquals("supplierWithCounty: Chosen supplier do have state info", "03", invoiceLine2.JI_StateOrRegionOfOrigin);

			declaration.JE_OH_Supplier = supplierForeign.PK;
			AssertEquals("supplierForeign: No change to existing invoice if we change supplier", ZString.Empty, invoiceLine.JI_StateOrRegionOfOrigin);
			invoiceLine2 = invoiceHeader.JobComInvoiceLines.AddNew();
			AssertEquals("supplierForeign: Chosen supplier do not have state info", ZString.Empty, invoiceLine2.JI_StateOrRegionOfOrigin);
		});
	}

	public void TestJI_DescriptionIsNotAutoPopulated()
	{
		invoiceLine.JI_Tariff = "111111111";
		AssertEquals(ZString.Empty, invoiceLine.JI_Description);
	}

	public void TestCustomsRateOverride_NoTariff()
	{
		CombineAssertions(() =>
		{
			invoiceLine.CustomsRateIsOverridden = true;
			invoiceLine.JI_CustomsRateOverrideValue = 123.45m;
			invoiceLine.JI_CustomsRateOverrideType = RateTypeCodeList.Codes.Kilogram;
			AssertEquals("CustomsRate when set", 123.45m, invoiceLine.JI_CustomsRateOverrideValue);
			AssertEquals("CustomsType when set", RateTypeCodeList.Codes.Kilogram, invoiceLine.JI_CustomsRateOverrideType);
			Factory.Save();

			var reloadedDeclaration = Factory.CreateNewFactory().Load<JobDeclaration>(declaration.PK);
			AssertEquals(1, reloadedDeclaration.InvoiceLines.Count);
			var reloadedInvLine = reloadedDeclaration.InvoiceLines[0];
			AssertNotNull(reloadedInvLine);

			AssertEquals("CustomsRateIsOverriden when reloaded", expected: true, reloadedInvLine.CustomsRateIsOverridden);
			AssertEquals("CustomsRate when reloaded", 123.45m, reloadedInvLine.JI_CustomsRateOverrideValue);
			AssertEquals("CustomsType when reloaded", RateTypeCodeList.Codes.Kilogram, reloadedInvLine.JI_CustomsRateOverrideType);

			reloadedInvLine.CustomsRateIsOverridden = false;
			AssertEquals("CustomsRate when reset", ZDecimal.Zero, reloadedInvLine.JI_CustomsRateOverrideValue);
			AssertEquals("CustomsType when reset", ZString.Empty, reloadedInvLine.JI_CustomsRateOverrideType);
		});
	}

	public void TestCustomsRateOverride_WithTariff()
	{
		var testHelper = new RefCusTariffTestHelper(Factory);
		testHelper.SetupTariffWithMultipleTariffRates();
		var merger = new LineMerger(declaration);

		declaration.JE_MessageType = "IMP";
		invoiceHeader.JZ_RX_NKInvoice_Currency = "NOK";
		invoiceLine.JI_Tariff = "21069060";
		invoiceLine.JI_PrimaryPreference = "N";
		invoiceLine.JI_CountryOfOrigin = Enterprise.Core.Constants.CountryCodes.Latvia;
		invoiceLine.JI_LinePrice = 1000m;
		invoiceLine.JI_CustomsUnitQty = "KGM";
		invoiceLine.JI_CustomsQuantity = 50;

		invoiceLine.CustomsRateIsOverridden = true;

		CombineAssertions("Override KGM", () =>
		{
			invoiceLine.CustomsRateType = "K";
			merger.DoMerge();

			AssertEquals("RateValue", 10m, invoiceLine.CustomsRate);
			AssertEquals("RateType", "K", invoiceLine.CustomsRateType);
			AssertCustomsDuty("Override KGM", 500m);
		});

		CombineAssertions("Override %", () =>
		{
			invoiceLine.CustomsRateType = "%";
			merger.DoMerge();

			AssertEquals("RateValue", 20m, invoiceLine.CustomsRate);
			AssertEquals("RateType", "%", invoiceLine.CustomsRateType);
			AssertCustomsDuty("Override %", 200m);
		});

		void AssertCustomsDuty(string because, decimal amount)
		{
			AssertEquals($"{because} headers count", 1, declaration.CustomsEntryHeaders.Count);
			var entryHeader = declaration.CustomsEntryHeaders[0];
			AssertEquals($"{because} lines count", 1, entryHeader.AllEntryLines.Count);
			var entryLine = entryHeader.AllEntryLines[0];
			AssertEquals($"{because} customs duty amount", amount, entryLine.DutyAmount);
		}
	}

	public void TestReducedCustomsFlagTogglesCustomsOverride()
	{
		invoiceLine.JI_ReducedCustomsFlag = ZString.Empty;
		AssertEquals(expected: false, invoiceLine.CustomsRateIsOverridden);

		invoiceLine.JI_ReducedCustomsFlag = "S";
		AssertEquals(expected: true, invoiceLine.CustomsRateIsOverridden);
	}

	public void TestCustomsRateIsOverridden()
	{
		invoiceLine.CustomsRateIsOverridden = false;
		AssertEquals(expected: true, invoiceLine.CustomsRateReadOnly);

		invoiceLine.CustomsRateIsOverridden = true;
		AssertEquals(expected: false, invoiceLine.CustomsRateReadOnly);
	}

	public void TestJI_CustomsUnitQty_Attributes() => CombineAssertions(() =>
		AssertEntity<JobComInvoiceLine>()
			.HasProperty(line => line.JI_CustomsUnitQty)
			.WithList($"{nameof(JobComInvoiceLine.Lookups)}.{nameof(JobComInvoiceLineLookups.CustomsUnitQtyList)}"));

	public void TestJI_CustomsSecondUnitQty_Attributes() => CombineAssertions(() =>
		AssertEntity<JobComInvoiceLine>()
			.HasProperty(line => line.JI_CustomsSecondUnitQty)
			.WithList($"{nameof(JobComInvoiceLine.Lookups)}.{nameof(JobComInvoiceLineLookups.CustomsSecondUnitQtyList)}"));

	public void TestJI_CustomsSecondUnitQty()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		var invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();

		CombineAssertions(() =>
		{
			invoiceLine.JI_Tariff = "11111111";
			AssertEquals("11111111 - Should have CU2", "LTR", invoiceLine.JI_CustomsSecondUnitQty);

			invoiceLine.JI_Tariff = "22222222";
			AssertEquals("22222222 - Should not have CU2", string.Empty, invoiceLine.JI_CustomsSecondUnitQty);
		});
	}

	public void TestJI_CustomsSecondQuantityAutoPopulatedFromVolumeIfNeeded()
	{
		CombineAssertions(() =>
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Norway, Universal.Constants.TariffTypes.Export);
			var tariff = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Norway, tariffType.PK, "11111111", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateTariffUOM(tariff, Universal.Constants.UnitOfMeasureTypes.AdditionalUOMType, NOCustomsFormulaUnitCodeList.Codes.LTR);
			Factory.Save();
			var invoiceLine = Factory.New<JobComInvoiceLine_ForTesting>();
			invoiceLine.JI_Tariff = "11111111";

			invoiceLine.JI_Volume = 100;
			invoiceLine.JI_VolumeUQ = "L";
			ZDecimal expected = 100;
			AssertEquals("CustomsSecondQuantity should be populated with volume (LTR)", expected, invoiceLine.JI_CustomsSecondQuantity);

			invoiceLine.JI_CustomsSecondQuantity = 0m;
			invoiceLine.JI_Volume = 100;
			expected = 100;
			AssertEquals("CustomsSecondQuantity should be populated with volume (LTR)", expected, invoiceLine.JI_CustomsSecondQuantity);

			invoiceLine.JI_CustomsSecondQuantity = 50;
			invoiceLine.JI_Volume = 100;
			expected = 50;
			AssertEquals("CustomsSecondQuantity should not change when already populated", expected, invoiceLine.JI_CustomsSecondQuantity);
		});
	}

	public void TestJI_CustomsSecondQuantityAutoPopulatedFromInvoiceQuantityIfNeeded()
	{
		CombineAssertions(() =>
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Norway, Universal.Constants.TariffTypes.Export);
			var tariff = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Norway, tariffType.PK, "55555555", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateTariffUOM(tariff, Universal.Constants.UnitOfMeasureTypes.AdditionalUOMType, NOCustomsFormulaUnitCodeList.Codes.NMB);
			Factory.Save();
			var invoiceLine = Factory.New<JobComInvoiceLine_ForTesting>();
			invoiceLine.JI_Tariff = "55555555";

			invoiceLine.JI_InvoiceQuantity = 100;
			invoiceLine.JI_InvoiceUQ = "UNT";
			ZDecimal expected = 100;
			AssertEquals("CustomsSecondQuantity should be populated from Invoice Qty when type is NMB", expected, invoiceLine.JI_CustomsSecondQuantity);

			invoiceLine.JI_CustomsSecondQuantity = 0m;
			invoiceLine.JI_InvoiceQuantity = 100;
			expected = 100;
			AssertEquals("CustomsSecondQuantity should be populated with volume (LTR)", expected, invoiceLine.JI_CustomsSecondQuantity);

			invoiceLine.JI_CustomsSecondQuantity = 50;
			invoiceLine.JI_InvoiceQuantity = 100;
			expected = 50;
			AssertEquals("CustomsSecondQuantity should not change when it's already populated", expected, invoiceLine.JI_CustomsSecondQuantity);
		});
	}

	public void TestJI_CustomsQuantity_DefaultsFromNetQty()
	{
		var tariffNonKGM = testHelper.CreateImportTariff("12341234");
		testHelper.AddGeneralRate(tariffNonKGM, PrimaryPreferenceCodeList.Codes.N, "0.5 * VFD");
		var tariffKGM = testHelper.CreateImportTariff("11111111");
		testHelper.AddGeneralRate(tariffKGM, PrimaryPreferenceCodeList.Codes.N, "10.29 * [KGM]");

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		var invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();

		CombineAssertions(() =>
		{
			invoiceLine.JI_Tariff = tariffKGM.ZZ1_TariffCode;
			invoiceLine.JI_NetWeight = 12.34m;
			AssertEquals("Should inherit from Net Qty", 12.34m, invoiceLine.JI_CustomsQuantity);
			AssertEquals("Should be KGM", "KGM", invoiceLine.JI_CustomsUnitQty);

			invoiceLine.JI_CustomsQuantity = 0m;
			AssertEquals("Should re-sync from Net Qty", 12.34m, invoiceLine.JI_CustomsQuantity);
			AssertEquals("Should be KGM", "KGM", invoiceLine.JI_CustomsUnitQty);

			invoiceLine.JI_NetWeight = 0m;
			invoiceLine.JI_Tariff = tariffNonKGM.ZZ1_TariffCode;
			invoiceLine.JI_CustomsQuantity = 0m;
			invoiceLine.JI_CustomsUnitQty = ZString.Empty;
			invoiceLine.JI_NetWeight = 23.45m;
			AssertEquals("Non KGM tariff - should remain 0", 0m, invoiceLine.JI_CustomsQuantity);
			AssertEquals("Non KGM tariff - should be empty", ZString.Empty, invoiceLine.JI_CustomsUnitQty);
		});
	}

	public void TestJI_CustomsQuantity_CanOverride()
	{
		var tariffKGM = testHelper.CreateImportTariff("11111111");
		testHelper.AddGeneralRate(tariffKGM, PrimaryPreferenceCodeList.Codes.N, "10.29 * [KGM]");

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		var invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();

		CombineAssertions(() =>
		{
			invoiceLine.JI_Tariff = tariffKGM.ZZ1_TariffCode;
			invoiceLine.JI_NetWeight = 12.34m;
			AssertEquals("Should inherit from Net Qty", 12.34m, invoiceLine.JI_CustomsQuantity);
			AssertEquals("Should be KGM", "KGM", invoiceLine.JI_CustomsUnitQty);
			invoiceLine.JI_CustomsQuantity = 23.45m;
			AssertEquals("Should allow value override", 23.45m, invoiceLine.JI_CustomsQuantity);
		});
	}

	public void TestCustomsRateIsByTariffUnlessOverridden()
	{
		SetupTariffs();
		invoiceLine.CustomsRateIsOverridden = false;
		SetNonPreferentialImport();

		CombineAssertions(() =>
		{
			invoiceLine.JI_Tariff = TariffConstants.KGTariff;
			AssertEquals("KG tariff | CustomsRateType", RateTypeCodeList.Codes.Kilogram, invoiceLine.CustomsRateType);
			AssertEquals("KG tariff | CustomsRate", 2.5m, invoiceLine.CustomsRate);
			AssertEquals("KG tariff | JI_CustomsRateOverrideType", ZString.Empty, invoiceLine.JI_CustomsRateOverrideType);
			AssertEquals("KG tariff | JI_CustomsRateOverrideValue", ZDecimal.Zero, invoiceLine.JI_CustomsRateOverrideValue);

			invoiceLine.JI_Tariff = TariffConstants.VFDTariff;
			AssertEquals("% tariff | CustomsRateType", RateTypeCodeList.Codes.PercentSign, invoiceLine.CustomsRateType);
			AssertEquals("% tariff | CustomsRate", 15m, invoiceLine.CustomsRate);
			AssertEquals("% tariff | JI_CustomsRateOverrideType", ZString.Empty, invoiceLine.JI_CustomsRateOverrideType);
			AssertEquals("% tariff | JI_CustomsRateOverrideValue", ZDecimal.Zero, invoiceLine.JI_CustomsRateOverrideValue);

			invoiceLine.CustomsRateIsOverridden = true;
			AssertEquals("Override ticked | CustomsRateType", RateTypeCodeList.Codes.PercentSign, invoiceLine.CustomsRateType);
			AssertEquals("Override ticked | CustomsRate", 15m, invoiceLine.CustomsRate);
			AssertEquals("Override ticked | JI_CustomsRateOverrideType", RateTypeCodeList.Codes.PercentSign, invoiceLine.JI_CustomsRateOverrideType);
			AssertEquals("Override ticked | JI_CustomsRateOverrideValue", 15m, invoiceLine.JI_CustomsRateOverrideValue);

			invoiceLine.JI_CustomsRateOverrideType = RateTypeCodeList.Codes.CubicMeter;
			invoiceLine.JI_CustomsRateOverrideValue = 2.0m;
			AssertEquals("Override changed | CustomsRateType", RateTypeCodeList.Codes.CubicMeter, invoiceLine.CustomsRateType);
			AssertEquals("Override changed | CustomsRate", 2.0m, invoiceLine.CustomsRate);
			AssertEquals("Override changed | JI_CustomsRateOverrideType", RateTypeCodeList.Codes.CubicMeter, invoiceLine.JI_CustomsRateOverrideType);
			AssertEquals("Override changed | JI_CustomsRateOverrideValue", 2.0m, invoiceLine.JI_CustomsRateOverrideValue);

			invoiceLine.JI_Tariff = TariffConstants.KGTariff;
			AssertEquals("Tariff changed | CustomsRateType", RateTypeCodeList.Codes.Kilogram, invoiceLine.CustomsRateType);
			AssertEquals("Tariff changed | CustomsRate", 2.5m, invoiceLine.CustomsRate);
			AssertEquals("Tariff changed | JI_CustomsRateOverrideType", ZString.Empty, invoiceLine.JI_CustomsRateOverrideType);
			AssertEquals("Tariff changed | JI_CustomsRateOverrideValue", ZDecimal.Zero, invoiceLine.JI_CustomsRateOverrideValue);
		});
	}

	public void TestJI_OtherChargesCurrency_LocalCurrency()
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		invoiceHeader.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
		invoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
		invoiceLine.JI_LinePrice = 1000m;

		invoiceLine.Charges.AddNew(Common.CustomsChargeTypeList.Codes.OtherCharges, 120m);

		CombineAssertions(() =>
		{
			AssertEquals("Linceprice NOK", 1000m, invoiceLine.JI_LinePriceInLocalCurrency);
			AssertEquals("JI_OtherChargesCurrency", 120m, invoiceLine.TotalOtherChargesInNOK);
			AssertEquals("JI_Calc_CIF", 1120m, invoiceLine.JI_Calc_CIF_InLocalCurrency);
		});
	}

	public void TestRateFormulaDescription()
	{
		SetupTariffs();
		SetNonPreferentialImport();
		CombineAssertions(() =>
		{
			AssertRateFormulaDescription(TariffConstants.VFDTariff, "15.0% of the Value for Duty");
			AssertRateFormulaDescription(TariffConstants.KGTariff, "2.5 NOK per Kilogram");
			AssertRateFormulaDescription(TariffConstants.LTRAndASVTariff, "4.95 NOK per Liter and Volume percentage");
			AssertRateFormulaDescription(TariffConstants.ASVAndLTRTariff, "4.95 NOK per Volume percentage and Liter");
			AssertRateFormulaDescription(TariffConstants.LTRByASVByNOKTariff, "4.95 NOK per Volume percentage and Liter");
		});

		void AssertRateFormulaDescription(string tariffCode, string expected)
		{
			invoiceLine.JI_Tariff = tariffCode;
			var rate = invoiceLine.UniversalDutyRate;
			AssertEquals($"RateFormulaDescription of '{rate.ZZ2_RateFormula}'", expected, rate.RateFormulaDescription);
		}
	}

	public void TestTotalDeductionsInNOK()
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		invoiceHeader.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
		invoiceLine.JI_LinePrice = 1000m;
		var charge1 = invoiceLine.Charges.AddNew(Common.CustomsChargeTypeList.Codes.DeductionCharge, 12m);
		var charge2 = invoiceLine.Charges.AddNew(Common.CustomsChargeTypeList.Codes.DeductionCharge, 13m);

		CombineAssertions(() =>
		{
			AssertEquals("Lineprice NOK", 1000m, invoiceLine.JI_LinePriceInLocalCurrency);
			AssertEquals("TotalDeductions 25", 25m, invoiceLine.TotalDeductionsInNOK);
			AssertEquals("JI_Calc_CIF 975", 975m, invoiceLine.JI_Calc_CIF_InLocalCurrency);

			var chargePercent = invoiceLine.Charges.AddNew(Common.CustomsChargeTypeList.Codes.DeductionCharge);
			chargePercent.J7_Percentage = 5m;

			AssertEquals("TotalDeductions 5%", 75m, invoiceLine.TotalDeductionsInNOK);
			AssertEquals("JI_Calc_CIF 925", 925m, invoiceLine.JI_Calc_CIF_InLocalCurrency);
		});
	}

	public void TestTotalDeductionsInNOK_LocalCurrency()
	{
		_ = new RefCurrencyTestHelper(Factory).USDCurrency;
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		invoiceHeader.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
		invoiceLine.JI_LinePrice = 1000m;
		invoiceLine.Charges.AddNew(Common.CustomsChargeTypeList.Codes.DeductionCharge, 10m, Core.Constants.CurrencyCodes.UnitedStates);

		CombineAssertions(() =>
		{
			AssertEquals("Lineprice NOK", 1000m, invoiceLine.JI_LinePriceInLocalCurrency);
			AssertEquals("TotalDeductionsInNOK", 125m, invoiceLine.TotalDeductionsInNOK);
			AssertEquals("JI_Calc_CIF", 875m, invoiceLine.JI_Calc_CIF_InLocalCurrency);
		});
	}

	public void TestJI_OtherChargesCurrency_ExternalCurrency()
	{
		_ = new RefCurrencyTestHelper(Factory).USDCurrency;
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		invoiceHeader.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
		invoiceLine.JI_LinePrice = 1000m;
		invoiceLine.Charges.AddNew(Common.CustomsChargeTypeList.Codes.OtherCharges, 10m, Core.Constants.CurrencyCodes.UnitedStates);

		CombineAssertions(() =>
		{
			AssertEquals("Lineprice NOK", 1000m, invoiceLine.JI_LinePriceInLocalCurrency);
			AssertEquals("JI_OtherCharges NOK", 125m, invoiceLine.TotalOtherChargesInNOK);
			AssertEquals("JI_Calc_CIF", 1125m, invoiceLine.JI_Calc_CIF_InLocalCurrency);
		});
	}

	public void TestJI_Calc_FreightInLocalCurrency()
	{
		_ = new RefCurrencyTestHelper(Factory).USDCurrency;
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

		invoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
		invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
		invoiceHeader.JZ_InvoiceAmount = 1000m;
		invoiceHeader.Charges.AddNew(Common.CustomsChargeTypeList.Codes.OverseasFreight, 10m, Core.Constants.CurrencyCodes.UnitedStates);
		invoiceLine.JI_LinePrice = 1000m;
		Factory.Save();

		CombineAssertions(() =>
		{
			AssertEquals("JI_LinePriceInLocalCurrency NOK", 12500m, invoiceLine.JI_LinePriceInLocalCurrency);
			AssertEquals("JI_Calc_FreightInLocalCurrency NOK", 125m, invoiceLine.JI_Calc_FreightInLocalCurrency);
			AssertEquals("JI_Calc_CIF_InLocalCurrency NOK", 12625m, invoiceLine.JI_Calc_CIF_InLocalCurrency);
		});
	}

	public void TestJI_Calc_InsuranceInLocalCurrency()
	{
		_ = new RefCurrencyTestHelper(Factory).USDCurrency;
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

		invoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
		invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
		invoiceHeader.JZ_InvoiceAmount = 1000m;
		invoiceHeader.Charges.AddNew(Common.CustomsChargeTypeList.Codes.OverseasInsurance, 10m, Core.Constants.CurrencyCodes.UnitedStates);
		invoiceLine.JI_LinePrice = 1000m;
		Factory.Save();

		CombineAssertions(() =>
		{
			AssertEquals("JI_LinePriceInLocalCurrency NOK", 12500m, invoiceLine.JI_LinePriceInLocalCurrency);
			AssertEquals("JI_Calc_InsuranceInLocalCurrency NOK", 125m, invoiceLine.JI_Calc_InsuranceInLocalCurrency);
			AssertEquals("JI_Calc_CIF_InLocalCurrency NOK", 12625m, invoiceLine.JI_Calc_CIF_InLocalCurrency);
		});
	}

	public override void TestChargeTypeList()
	{
		var dec = Factory.New<BaseJobDeclaration>();
		dec.JE_MessageType = JobMessageTypeList.Codes.Import;
		Customs.Business.ICommonInvoice commonInvoice = dec.Invoices.AddNew().JobComInvoiceLines.AddNew();
		var chargeTypeList1 = commonInvoice.ChargeTypeList;
		var chargeTypeList2 = commonInvoice.ChargeTypeList;
		CombineAssertions(() =>
		{
			AssertEquals(true, object.ReferenceEquals(chargeTypeList1, chargeTypeList2));
			var customsChargeTypeList = new Customs.Business.CustomsChargeTypeList();
			customsChargeTypeList.AddPair(NOInvoiceChargeTypesImport.Codes.ValueOfGoodsExported, NOInvoiceChargeTypesImport.Descriptions.ValueOfGoodsExported);
			customsChargeTypeList.Sort();
			AssertEquals(customsChargeTypeList.CodesAsString, chargeTypeList1.CodesAsString);

			dec.JE_MessageType = JobMessageTypeList.Codes.Export;
			chargeTypeList1 = commonInvoice.ChargeTypeList;
			chargeTypeList2 = commonInvoice.ChargeTypeList;
			AssertEquals(true, object.ReferenceEquals(chargeTypeList1, chargeTypeList2));
			AssertEquals(customsChargeTypeList.CodesAsString, chargeTypeList1.CodesAsString);
		});
	}

	public void TestJI_SupplementaryCode1_Attributes() => CombineAssertions(() =>
		AssertEntity<JobComInvoiceLine>()
			.HasProperty(l => l.JI_SupplementaryCode1)
			.WithCaption("Excise Code 1")
			.WithFullDescription("Excise Code")
			.WithAttribute<MaxLengthAttribute>(l => l.MaxLength == 35)
			.WithList("Lookups.AdditionalCodesList"));

	public void TestJI_SupplementaryCode1() => CombineAssertions(() =>
	{
		invoiceLine.JI_SupplementaryCode1 = "CD01";
		var code = new BaseSupplementaryCode.Loader(Factory).Load(invoiceLine, 1);
		AssertEquals("CY_Code", "CD01", code.CY_Code);
		AssertEquals("CY_Order", (short)1, code.CY_Order);
		AssertEquals("CY_Type", BaseCusCodeDataTypeList.Codes.SupplementaryCode, code.CY_Type);
	});

	public void TestJI_SupplementaryCode2_Attributes() => CombineAssertions(() =>
		AssertEntity<JobComInvoiceLine>()
			.HasProperty(l => l.JI_SupplementaryCode2)
			.WithCaption("Excise Code 2")
			.WithFullDescription("Excise Code")
			.WithAttribute<MaxLengthAttribute>(l => l.MaxLength == 35)
			.WithList("Lookups.AdditionalCodesList"));

	public void TestJI_SupplementaryCode2() => CombineAssertions(() =>
	{
		invoiceLine.JI_SupplementaryCode2 = "CD01";
		var code = new BaseSupplementaryCode.Loader(Factory).Load(invoiceLine, 2);
		AssertEquals("CY_Code", "CD01", code.CY_Code);
		AssertEquals("CY_Order", (short)2, code.CY_Order);
		AssertEquals("CY_Type", BaseCusCodeDataTypeList.Codes.SupplementaryCode, code.CY_Type);
	});

	public void TestAdditionalSupplementaryCodes()
	{
		var additionalSupplementaryCode = invoiceLine.AdditionalSupplementaryCodes;
		AssertType<SupplementaryCodeCollection>(additionalSupplementaryCode);

		var supplementaryCode = additionalSupplementaryCode.AddNew("CD1");
		AssertType<SupplementaryCode>(supplementaryCode);
	}

	public void TestJI_AdditionalSupplements_Attributes() => CombineAssertions(() =>
		AssertEntity<JobComInvoiceLine>()
			.HasProperty(l => l.JI_AdditionalSupplements)
			.WithCaption("Add. Exc. Codes")
			.WithFullDescription("Additional excise codes")
			.WithAttribute<ReadOnlyAttribute>(r => r.IsReadOnly));

	public void TestSupplementaryCodeSupporter_Tariff()
	{
		_ = testHelper.CreateImportTariff("11111111");
		invoiceLine.JI_Tariff = "11111111";
		AssertNotNull("Invoice Line Universal Tariff", invoiceLine.UniversalTariff);

		ISupplementaryCodeSupporter supplementaryCodeSupporter = invoiceLine;
		AssertNotNull("Invoice Line as SupplementaryCodeSupporter", supplementaryCodeSupporter);
		AssertSame(invoiceLine.UniversalTariff, supplementaryCodeSupporter.Tariff);
	}

	public void TestSupplementaryCodeSupporter_SupplementaryCodesFieldType()
	{
		ISupplementaryCodeSupporter supplementaryCodeSupporter = invoiceLine;
		AssertNotNull("Invoice Line as SupplementaryCodeSupporter", supplementaryCodeSupporter);
		AssertEquals("SupplementaryCodesFieldType", "TextDropEdit", supplementaryCodeSupporter.SupplementaryCodesFieldType);
	}

	public void TestSupplementaryCodeSupporter_SupplementaryCodeCaption()
	{
		ISupplementaryCodeSupporter supplementaryCodeSupporter = invoiceLine;
		AssertNotNull("Invoice Line as SupplementaryCodeSupporter", supplementaryCodeSupporter);
		AssertNotNull("SupplementaryCodeCaption", supplementaryCodeSupporter.SupplementaryCodeCaption);
		AssertEquals("Caption", "Excise Code", supplementaryCodeSupporter.SupplementaryCodeCaption.Caption);
	}

	public void TestSupplementaryCodeSupporter_GetCountryCodeForCodeProvider()
	{
		ISupplementaryCodeSupporter supplementaryCodeSupporter = invoiceLine;
		AssertNotNull("Invoice Line as SupplementaryCodeSupporter", supplementaryCodeSupporter);
		CombineAssertions(() =>
		{
			AssertEquals("When Declaration Country Code is present", declaration.Country.RN_Code, supplementaryCodeSupporter.GetCountryCodeForCodeProvider());

			declaration.JE_GC = ZGuid.Empty;
			AssertEquals("When Invoice Header Country Code is present", invoiceHeader.InvoiceCountry.RN_Code, supplementaryCodeSupporter.GetCountryCodeForCodeProvider());

			invoiceHeader.JZ_GB = ZGuid.Empty;
			AssertEquals("When No other codes are present", GlbCompany.CurrentCompany.Country.Code, supplementaryCodeSupporter.GetCountryCodeForCodeProvider());
		});
	}

	public void TestSupplementaryCodeSupporter_RateSelectionCriteria()
	{
		ISupplementaryCodeSupporter supplementaryCodeSupporter = invoiceLine;
		AssertNotNull("Invoice Line as SupplementaryCodeSupporter", supplementaryCodeSupporter);
		AssertType<RateSelectionCriteria>(supplementaryCodeSupporter.RateSelectionCriteria);
	}

	public void TestSupplementaryCodeSupporter_SupplementaryCodes()
	{
		invoiceLine.JI_SupplementaryCode1 = "SUP1";
		invoiceLine.JI_SupplementaryCode2 = "SUP2";
		invoiceLine.AdditionalSupplementaryCodes.AddNew().CY_Code = "SUP3";

		ISupplementaryCodeSupporter supplementaryCodeSupporter = invoiceLine;
		AssertNotNull("Invoice Line as SupplementaryCodeSupporter", supplementaryCodeSupporter);
		AssertArrayEqualsByElements(new ZString[] { "SUP3", "SUP1", "SUP2" }, supplementaryCodeSupporter.SupplementaryCodes.Select(c => c.CY_Code).ToArray());
	}

	public void TestSupplementaryCodeSupporter_CachedListOfAdditionalCodeDescriptions()
	{
		ISupplementaryCodeSupporter supplementaryCodeSupporter = invoiceLine;
		AssertNotNull("Invoice Line as SupplementaryCodeSupporter", supplementaryCodeSupporter);
		AssertEquals(invoiceLine.Lookups.CachedListOfAdditionalCodeDescriptions.CodesAsString, supplementaryCodeSupporter.CachedListOfAdditionalCodeDescriptions.CodesAsString);
	}

	[ExpectNoExceptions]
	public void TestSupplementaryCodeSupporter_OnCodesChangedDoesNotExecuteCustomsUnitDefaulting()
	{
		var invoiceLineForTest = Factory.New<JobComInvoiceLine_ForTesting>();
		invoiceLineForTest.JI_SupplementaryCode1 = "CD1";
		invoiceLineForTest.CustomsUnitDefaultingStrategy
			.Verify(s => s.DefaultUOMs(It.IsAny<JobComInvoiceLine>()), Times.Never);
	}

	public void TestCustomsUnitDefaultingStrategy()
	{
		var invoiceLineForTest = Factory.New<JobComInvoiceLine_ForTesting>();
		var defaultingStrategy = invoiceLineForTest.GetCustomsUnitDefaultingStrategyExposed();
		AssertType<UniversalRateCustomsUnitDefaultingStrategy<JobComInvoiceLine>>(defaultingStrategy);
	}

	public void TestSupplementaryCodeSupporter_GetCountryCodeFromAdditionalCode()
	{
		ISupplementaryCodeSupporter supplementaryCodeSupporter = invoiceLine;
		AssertNotNull("Invoice Line as SupplementaryCodeSupporter", supplementaryCodeSupporter);
		AssertEquals(invoiceLine.CountryCode, supplementaryCodeSupporter.GetCountryCodeFromAdditionalCode("CD1"));
	}

	public void TestCusCodeDataTypeSupporter_GetCusCodeDataTypes()
	{
		ICusCodeDataTypeSupporter cusCodeDataTypeSupporter = invoiceLine;
		AssertNotNull("InvoiceLine as ICusCodeDataTypeSupporter", cusCodeDataTypeSupporter);

		var cusCodeDataTypes = cusCodeDataTypeSupporter.GetCusCodeDataTypes();
		AssertNotNull("CusCodeDataTypes Dictionary", cusCodeDataTypes);
		CombineAssertions(() =>
		{
			AssertDictionaryItem(cusCodeDataTypes, BaseCusCodeDataTypeList.Codes.SupplementaryCode, typeof(SupplementaryCode));
		});
	}

	[TestDate(2025, 01, 01, 0, 0, 0)]
	public void TestGetCurrencyConverter()
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

		const decimal exchangeRateIn2024 = 15.23m;
		const decimal exchangeRateIn2025 = 17.23m;
		const decimal exchangeRateOverride = 16m;

		var invoiceHeader = declaration.Invoices.AddNew();
		invoiceHeader.IsJZ_InvoiceCurrExRateUserEnterable = true;
		invoiceHeader.JZ_InvoiceCurrExRate = exchangeRateOverride;

		SetUpExchangeRate();

		CombineAssertions(() =>
		{
			var invLine = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			invLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.UnitedKingdom;
			AssertCurrencyConverter(invLine, "When JobComInvoiceHeader.IsJZ_InvoiceCurrExRateUserEnterable is true", ZDateTime.Today, exchangeRateOverride);

			invoiceHeader.IsJZ_InvoiceCurrExRateUserEnterable = false;
			AssertCurrencyConverter(invLine, "When JobComInvoiceHeader.IsJZ_InvoiceCurrExRateUserEnterable is false and EntryInstruction is not linked", ZDateTime.Today, exchangeRateIn2025);

			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			invLine.JI_CEI = entryInstruction.PK;
			AssertCurrencyConverter(invLine, "When JobComInvoiceHeader.IsJZ_InvoiceCurrExRateUserEnterable is false, EntryInstruction is linked and CEI_DateForDuty is empty", ZDateTime.Today, exchangeRateIn2025);

			var dateForDuty = new ZDateTime(2024, 10, 02);
			entryInstruction.CEI_DateForDuty = dateForDuty;
			AssertCurrencyConverter(invLine, "When JobComInvoiceHeader.IsJZ_InvoiceCurrExRateUserEnterable is false, EntryInstruction is linked and CEI_DateForDuty is not empty", dateForDuty, exchangeRateIn2024);
		});

		void AssertCurrencyConverter(JobComInvoiceLine invoiceLine, string message, ZDateTime dateForRate, decimal exchangeRate)
		{
			var result = invoiceLine.CurrencyConverter;

			AssertEquals($"{message} GetExchangeRate", exchangeRate, result.GetExchangeRate(RefCurrency.LoadFromCurrencyCode(Factory, CurrencyCodes.UnitedKingdom)));
			AssertEquals($"{message} DateForRate", dateForRate, result.DateForRate);
			AssertEquals($"{message} RateType", ExchangeRateType.Customs, result.RateType);
		}

		void SetUpExchangeRate()
		{
			var nok = RefCurrency.LoadFromCurrencyCode(Factory, CurrencyCodes.Norway);
			nok.ExchangeRates.DeleteAll();

			var gbp = RefCurrency.LoadFromCurrencyCode(Factory, CurrencyCodes.UnitedKingdom);
			gbp.ExchangeRates.DeleteAll();

			var gbpExchangeRate1 = gbp.ExchangeRates.AddNew();
			gbpExchangeRate1.RE_ExRateType = "CUS";
			gbpExchangeRate1.RE_StartDate = new ZDateTime(2024, 1, 1, 0, 0, 0);
			gbpExchangeRate1.RE_ExpiryDate = new ZDateTime(2024, 12, 31, 0, 0, 0);
			gbpExchangeRate1.RE_SellRate = exchangeRateIn2024;

			var gbpExchangeRate2 = gbp.ExchangeRates.AddNew();
			gbpExchangeRate2.RE_ExRateType = "CUS";
			gbpExchangeRate2.RE_StartDate = new ZDateTime(2025, 1, 1, 0, 0, 0);
			gbpExchangeRate2.RE_ExpiryDate = new ZDateTime(2025, 12, 31, 0, 0, 0);
			gbpExchangeRate2.RE_SellRate = exchangeRateIn2025;

			Factory.Save();
		}
	}

	public void TestIsRateGivenInFractionsOfKroner()
	{
		declaration.JE_MessageType = "IMP";
		invoiceLine.JI_Tariff = "22222222";

		CombineAssertions(() =>
		{
			AssertEquals("ZX201 - Has RateGivenInFractionsOfKroner", expected: true, invoiceLine.IsRateGivenInFractionsOfKroner("ZX201"));
			AssertEquals("ZX200 - No RateGivenInFractionsOfKroner", expected: false, invoiceLine.IsRateGivenInFractionsOfKroner("ZX200"));
		});
	}

	public void TestInvoiceLineCurrencyExchangeRateForCustoms()
	{
		CurrencyConverterTestHelper.SetExchangeRate(Factory, Core.Constants.CurrencyCodes.Sweden, 1.041111112m);
		CurrencyConverterTestHelper.SetExchangeRate(Factory, Core.Constants.CurrencyCodes.UnitedStates, 11.08m);
		CurrencyConverterTestHelper.SetExchangeRate(Factory, Core.Constants.CurrencyCodes.Denmark, 1.23456789m);
		CurrencyConverterTestHelper.SetExchangeRate(Factory, Core.Constants.CurrencyCodes.Australia, 7.20001m);
		CombineAssertions(() =>
		{
			AssertExchangeRate("SEK", "100", 104.1111112m);
			AssertExchangeRate("USD", "1", 11.08);
			AssertExchangeRate("DKK", "100", 123.456789m);
			AssertExchangeRate("AUD", "1", 7.20001m);
		});

		void AssertExchangeRate(ZString currency, ZString multiplier, ZDecimal amount)
		{
			invoiceHeader.JZ_RX_NKInvoice_Currency = currency;
			AssertEquals($"{currency} -> multiplier {multiplier}", amount, invoiceLine.InvoiceLineCurrencyExchangeRateForCustoms);
		}
	}

	public void TestCurrenciesWhereRateIsMultipliedBy100() => CombineAssertions(() =>
	{
		var invoice = Factory.New<JobComInvoiceLine_ForTesting>();
		var expectedCurrencies = ImmutableHashSet.Create(
			Core.Constants.CurrencyCodes.Switzerland,
			Core.Constants.CurrencyCodes.CzechRepublic,
			Core.Constants.CurrencyCodes.Denmark,
			Core.Constants.CurrencyCodes.Hungary,
			Core.Constants.CurrencyCodes.India,
			Core.Constants.CurrencyCodes.Japan,
			Core.Constants.CurrencyCodes.RomaniaNew,
			Core.Constants.CurrencyCodes.Sweden,
			Core.Constants.CurrencyCodes.Thailand,
			Core.Constants.CurrencyCodes.SouthAfrica);
		AssertEquals("List of currencies, count", expectedCurrencies.Count, invoice.CurrenciesWhereRateIsMultipliedBy100_Exposed.Count);
		AssertContainsExactElementsInAnyOrder(expectedCurrencies, invoice.CurrenciesWhereRateIsMultipliedBy100_Exposed);
	});

	public void TestExciseRateType() => CombineAssertions(() =>
	{
		declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
		AssertEquals("Import Declaration", Universal.Constants.RateTypes.Excise, invoiceLine.ExciseRateType);

		declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;
		AssertEquals("Import Declaration", Universal.Constants.RateTypes.ExportDuty, invoiceLine.ExciseRateType);
	});

	public void TestGetValuationCalculator()
	{
		var invoice = Factory.New<JobComInvoiceLine_ForTesting>();
		var result = invoice.ValuationCalculatorExposed;
		AssertType<CustomsValuationCalculator>(result);
	}

	public void TestJI_Calc_StatisticalValue()
	{
		invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Norway;
		invoiceLine.JI_Procedure = UniversalReferenceConstants.ChargeTypes.VGE_ProcedureCode;
		invoiceLine.JI_LinePrice = 50m;
		_ = invoiceLine.Charges.AddNew(Universal.Constants.RateTypes.AntiDumping, 10m, CurrencyCodes.Norway);

		CombineAssertions(() =>
		{
			AssertEquals("JI_Calc_StatisticalValue, when VGE is not present.", 60m, invoiceLine.JI_Calc_StatisticalValue);
			_ = invoiceLine.ApportionedCharges.AddNew(NOInvoiceChargeTypesImport.Codes.ValueOfGoodsExported, 6m, CurrencyCodes.Norway);
			AssertEquals("JI_Calc_StatisticalValue, when VGE is present.", 66m, invoiceLine.JI_Calc_StatisticalValue);
		});
	}

	public void TestCanCalculateVAT() => CombineAssertions(() =>
	{
		AssertEquals("When Entry Instruction is not selected", expected: true, invoiceLine.CanCalculateVAT());

		var referenceTestDataHelper = new UniversalReferenceTestDataHelper(Factory);
		var procedure = referenceTestDataHelper.CreateRefCusProcedure(dataGroupingCode: Core.Constants.CountryCodes.Norway,
			category: ZString.Empty,
			procedureCode: "77",
			previousProcedureCode: "77",
			concession: "777",
			description: "Description",
			shipmentType: "IMP");

		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_Procedure = "7777";

		invoiceLine.JI_CEI = entryInstruction.PK;
		AssertEquals("When Entry Instruction is present and ZZ6_CalculateVAT is true", expected: true, invoiceLine.CanCalculateVAT());
		procedure.ZZ6_CalculateVAT = false;
		AssertEquals("When Entry Instruction is present and ZZ6_CalculateVAT is false", expected: false, invoiceLine.CanCalculateVAT());
	});

	public void TestCanCalculateDuty() => CombineAssertions(() =>
	{
		AssertEquals("When Entry Instruction is not selected", expected: true, invoiceLine.CanCalculateDuty());

		var referenceTestDataHelper = new UniversalReferenceTestDataHelper(Factory);
		var procedure = referenceTestDataHelper.CreateRefCusProcedure(dataGroupingCode: Core.Constants.CountryCodes.Norway,
			category: ZString.Empty,
			procedureCode: "77",
			previousProcedureCode: "77",
			concession: "777",
			description: "Description",
			shipmentType: "IMP");

		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_Procedure = "7777";

		invoiceLine.JI_CEI = entryInstruction.PK;
		AssertEquals("When Entry Instruction is present and ZZ6_CalculateVAT is true", expected: true, invoiceLine.CanCalculateDuty());
		procedure.ZZ6_CalculateDuty = false;
		AssertEquals("When Entry Instruction is present and ZZ6_CalculateVAT is false", expected: false, invoiceLine.CanCalculateDuty());
	});

	static void AssertDictionaryItem<TKeyType, TValueType>(IDictionary<TKeyType, TValueType> sourceDictionary, TKeyType expectedKey, TValueType expectedValue)
	{
		AssertEquals("Key", true, sourceDictionary.ContainsKey(expectedKey));
		AssertEquals("Value", expectedValue, sourceDictionary[expectedKey]);
	}

	protected override void DoMerge(BaseJobDeclaration declaration)
	{
		SetupDataEligibleForMerging(declaration);
		base.DoMerge(declaration);
	}

	protected override Type ExpectedTypeOfApportionedCharges => typeof(JobComInvApportionedChargeCollection<InvoiceLineApportionCharge>);

	protected override Type ExpectedTypeOfCharges => typeof(JobComInvChargeCollection<InvoiceLineCharge>);

	protected override BusinessObject GetBusinessObjectForFetchForLoad() => InvoiceLine;

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => InvoiceLine;

	protected override ZString OFTChargeDescription => "FREIGHT from Entry";

	protected override BusinessObject GetNewBusinessObject()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		var invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
		invoiceLine.CustomsRateIsOverridden = true;
		return invoiceLine;
	}

	void SetupDataEligibleForMerging(BaseJobDeclaration declaration)
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
	}

	public void TestHasImporterWithMVARegistration()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Without an importer", expected: false, invoiceLine.HasImporterWithMVARegistration);
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			declaration.JE_OH_Importer = importer.PK;
			AssertEquals("With importer that is not MVA-registered", expected: false, invoiceLine.HasImporterWithMVARegistration);
			importer.AsMVARegistered();
			AssertEquals("With importer that is MVA-registered", expected: true, invoiceLine.HasImporterWithMVARegistration);
		});
	}

	public void TestCustomsRateIsOverriddenAfterCloning()
	{
		CombineAssertions(() =>
		{
			invoiceLine.JI_CustomsRateOverrideValue = 1m;
			invoiceLine.JI_CustomsRateOverrideType = "K";
			Factory.Save();

			var declarationCloned = (JobDeclaration)declaration.TemplateCopy();
			var invoiceCloned = declarationCloned.Invoices[0];
			var linecloned = ((JobComInvoiceLineViewCollection)invoiceCloned.InvoiceLines)[0];
			AssertEquals("CustomsRateIsOverridden is true", expected: true, linecloned.CustomsRateIsOverridden);

			invoiceLine.JI_CustomsRateOverrideValue = ZDecimal.Zero;
			invoiceLine.JI_CustomsRateOverrideType = ZString.Empty;
			Factory.Save();

			declarationCloned = (JobDeclaration)declaration.TemplateCopy();
			invoiceCloned = declarationCloned.Invoices[0];
			linecloned = ((JobComInvoiceLineViewCollection)invoiceCloned.InvoiceLines)[0];
			AssertEquals("CustomsRateIsOverridden is false", expected: false, linecloned.CustomsRateIsOverridden);
		});
	}

	public void TestJI_MergeOverrideAfterCloning()
	{
		invoiceLine.JI_MergeOverride = "ABC";
		Factory.Save();
		var declarationCloned = (JobDeclaration)declaration.TemplateCopy();
		var invoiceCloned = declarationCloned.Invoices[0];
		var linecloned = ((JobComInvoiceLineViewCollection)invoiceCloned.InvoiceLines)[0];
		AssertEquals(expected: "ABC", linecloned.JI_MergeOverride);
	}

	public void TestJI_RTOValue()
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		AssertEquals("Precondition: No RTOValue yet", 0m, invoiceLine.JI_RTOValue);
		AssertEquals("Precondition: RTOValue readonly is true", expected: true, invoiceLine.JI_RTOValueReadOnly);

		CombineAssertions(() =>
		{
			invoiceLine.JI_SupplementaryCode1 = "RT100";
			AssertEquals("RTOValue readonly is false", expected: false, invoiceLine.JI_RTOValueReadOnly);

			invoiceLine.JI_RTOValue = 123.45m;
			AssertEquals("RTOValue readonly is false", expected: false, invoiceLine.JI_RTOValueReadOnly);
			AssertEquals("RTOValue value is set", 123.45m, invoiceLine.JI_RTOValue);

			invoiceLine.JI_SupplementaryCode1 = ZString.Empty;
			AssertEquals("RTOValue readonly is true", expected: true, invoiceLine.JI_RTOValueReadOnly);
			AssertEquals("RTOValue value is set", 0m, invoiceLine.JI_RTOValue);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		testHelper = new RefCusTariffTestHelper(Factory);
		testHelper.SetupGenericTariffData();

		declaration = Factory.New<JobDeclaration>();
		invoiceHeader = declaration.Invoices.AddNew();
		invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
	}

	void SetupTariffs()
	{
		var tariffTestHelper = new RefCusTariffTestHelper(Factory);
		tariffTestHelper.GetOrCreateCodeListCustomsUQ("KGM", "Kilogram");
		tariffTestHelper.GetOrCreateCodeListCustomsUQ("LTR", "Liter");
		tariffTestHelper.GetOrCreateCodeListCustomsUQ("ASV", "Volume percentage");
		var vfdTariff = tariffTestHelper.CreateImportTariff(TariffConstants.VFDTariff);
		tariffTestHelper.AddRate(vfdTariff, PrimaryPreferenceCodeList.Codes.N, "0.15 * VFD", Core.Constants.CountryCodes.UnitedStates);
		var kgTariff = tariffTestHelper.CreateImportTariff(TariffConstants.KGTariff);
		tariffTestHelper.AddRate(kgTariff, PrimaryPreferenceCodeList.Codes.N, "[KGM] * 2.5", Core.Constants.CountryCodes.UnitedStates);
		var ltrasvTariff = tariffTestHelper.CreateImportTariff(TariffConstants.LTRAndASVTariff);
		tariffTestHelper.AddRate(ltrasvTariff, PrimaryPreferenceCodeList.Codes.N, "4.95 * [LTR] * [ASV]", Core.Constants.CountryCodes.UnitedStates);
		var asvltrTariff = tariffTestHelper.CreateImportTariff(TariffConstants.ASVAndLTRTariff);
		tariffTestHelper.AddRate(asvltrTariff, PrimaryPreferenceCodeList.Codes.N, "4.95 * [ASV] * [LTR]", Core.Constants.CountryCodes.UnitedStates);
		var ltrasvnokTariff = tariffTestHelper.CreateImportTariff(TariffConstants.LTRByASVByNOKTariff);
		tariffTestHelper.AddRate(ltrasvnokTariff, PrimaryPreferenceCodeList.Codes.N, "[LTR] * [ASV] * 4.95", Core.Constants.CountryCodes.UnitedStates);
	}

	internal IFeeRounder FeeRounder => feeRounder ??= new IntegerFeeRounder();
	IFeeRounder feeRounder;

	void SetNonPreferentialImport()
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		invoiceLine.JI_PrimaryPreference = "N";
		invoiceLine.JI_CountryOfOrigin = "US";
	}

	RefCusTariffTestHelper testHelper;
	JobDeclaration declaration;
	JobComInvoiceHeader invoiceHeader;
	JobComInvoiceLine invoiceLine;

	static class TariffConstants
	{
		public const string VFDTariff = "10001000";
		public const string KGTariff = "10002000";
		public const string LTRAndASVTariff = "10003000";
		public const string ASVAndLTRTariff = "10004000";
		public const string LTRByASVByNOKTariff = "10005000";
	}

	class JobComInvoiceLine_ForTesting : JobComInvoiceLine
	{
		public JobComInvoiceLine_ForTesting(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public ZDecimal JI_Calc_CIF_Override { get; set; }
		protected override ZDecimal GetJI_Calc_CIF() => JI_Calc_CIF_Override;

		public override ZDecimal JI_Calc_DutyAmount => JI_Calc_DutyAmount_Override ?? base.JI_Calc_DutyAmount;
		public ZDecimal? JI_Calc_DutyAmount_Override { get; set; }

		public override ZDecimal AppliedTaxAndFeeRate => AppliedTaxAndFeeRate_Override ?? base.AppliedTaxAndFeeRate;
		public ZDecimal? AppliedTaxAndFeeRate_Override { get; set; }
		protected override ICustomsUnitDefaultingStrategy GetCustomsUnitDefaultingStrategy()
		{
			return CustomsUnitDefaultingStrategy.Object ?? base.GetCustomsUnitDefaultingStrategy();
		}

		public ICustomsUnitDefaultingStrategy GetCustomsUnitDefaultingStrategyExposed() => base.GetCustomsUnitDefaultingStrategy();

		public Mock<ICustomsUnitDefaultingStrategy> CustomsUnitDefaultingStrategy => customsUnitDefaultingStrategy ??= new();
		Mock<ICustomsUnitDefaultingStrategy> customsUnitDefaultingStrategy;

		public ImmutableHashSet<string> CurrenciesWhereRateIsMultipliedBy100_Exposed => CurrenciesWhereRateIsMultipliedBy100;

		public CustomsValuationCalculator ValuationCalculatorExposed => (CustomsValuationCalculator)base.ValuationCalculator;
	}
}
