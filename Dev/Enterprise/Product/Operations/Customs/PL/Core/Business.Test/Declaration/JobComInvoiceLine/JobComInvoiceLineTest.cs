using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.Business.Testing;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.Common.PL;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.PL.Business.Testing;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Customs.Business.UniversalReferenceConstants;
using static Enterprise.Customs.Universal.Constants;

namespace Enterprise.Customs.PL.Business.Declaration.Testing;

[TestedType(typeof(JobComInvoiceLine))]
sealed class JobComInvoiceLineTest : EU.Business.Declaration.Testing.JobComInvoiceLineTest<JobComInvoiceLine>
{
	[ExpectNoExceptions]
	public override void TestAllAddInfoColumnsAreInModelView()
	{
		var jobComInvoiceLine = Factory.New<JobComInvoiceLine>();

		ModelViewTestHelper.AssertAllAddInfoColumnsAreInModelView(jobComInvoiceLine, "PLJobComInvoiceLine", (fieldName) => !fieldName.EndsWith("MaxLength"));
	}

	public void TestEffectiveAdditionalInfos() => CombineAssertions(() =>
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		var invoice1 = declaration.Invoices.AddNew();
		var invoice1Line1 = invoice1.InvoiceLines.AddNew();
		invoice1Line1.JI_CEI = entryInstruction.PK;
		var invoice2 = declaration.Invoices.AddNew();
		var invoice2Line1 = invoice2.InvoiceLines.AddNew();
		invoice2Line1.JI_CEI = entryInstruction.PK;

		AssertEquals(0, invoice1Line1.EffectiveAdditionalInfos().Count);
		AssertEquals(0, invoice2Line1.EffectiveAdditionalInfos().Count);

		invoice1Line1.AdditionalInfos.AddNew();
		AssertEquals(1, invoice1Line1.EffectiveAdditionalInfos().Count);
		AssertEquals(0, invoice2Line1.EffectiveAdditionalInfos().Count);

		entryInstruction.AdditionalInfos.AddNew();
		AssertEquals(2, invoice1Line1.EffectiveAdditionalInfos().Count);
		AssertEquals(1, invoice2Line1.EffectiveAdditionalInfos().Count);

		invoice2.AdditionalInfos.AddNew();
		invoice2Line1.AdditionalInfos.AddNew();
		AssertEquals(2, invoice1Line1.EffectiveAdditionalInfos().Count);
		AssertEquals(3, invoice2Line1.EffectiveAdditionalInfos().Count);

		declaration.AdditionalInfos.AddNew();
		AssertEquals(2, invoice1Line1.EffectiveAdditionalInfos().Count);
		AssertEquals(3, invoice2Line1.EffectiveAdditionalInfos().Count);
	});

	public void TestCusAuthorizationUsages()
	{
		AssertType<EU.Business.CusAuthorizationUsageCollection<CusAuthorizationUsage, JobComInvoiceLine>>(InvoiceLine.CusAuthorizationUsages);
	}

	public void TestFiscalReferences()
	{
		AssertType<CusFiscalReferenceCollection<CusFiscalReference>>(InvoiceLine.FiscalReferences);
	}

	public void TestCustomsCountryCode()
	{
		AssertEquals("CustomsCountryCodeCore should be PL", Core.Constants.CountryCodes.Poland, InvoiceLine.CustomsCountryCode);
	}

	public void TestAdditionalProcedureCodesAsString_Caption()
	{
		AssertEquals("Additional Procedure Code", DataBoundResourceStrings.GetDataForProperty(InvoiceLine.AdditionalProcedureCodesAsStringInfo).Caption);
	}

	public void TestIInvoiceLinePartDetailsMembers()
	{
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
		{
			IInvoiceLinePartDetails partDetails = Factory.New<JobComInvoiceLine>();
			AssertEquals(Core.Constants.CountryCodes.Poland, partDetails.CustomsCountryCode);
			AssertEquals(typeof(OrgSupplierPart), partDetails.TypeOfPartUsed);
		}
	}

	public void TestExciseRateSelectionCriteria()
	{
		var invoiceLine = InvoiceLine;

		CombineAssertions(() =>
		{
			AssertEquals("Data Group", "PL", invoiceLine.ExciseRateSelectionCriteria.DataGrouping);
			AssertEquals("Rate Code", "1A1", invoiceLine.ExciseRateSelectionCriteria.RateCode);
			AssertEquals("Rate Type", "EXC", invoiceLine.ExciseRateSelectionCriteria.RateType);
		});
	}

	public void TestEmptyOverseasFreightExpressedInItsCurrency()
	{
		var plnCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, Core.Constants.CurrencyCodes.Poland);
		Declaration.JE_ExportDate = ZDateTime.Today;
		SetExchangeRate(Declaration.JE_ExportDate.AddDays(-1), Declaration.JE_ExportDate.AddDays(1), 2m, plnCurrency);

		InvoiceHeader.JZ_InvoiceAmount = 1000;
		InvoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Poland;
		InvoiceHeader.JZ_IncoTerm = "CIF";

		InvoiceHeader.Charges.RemoveAndDeleteAll();
		Declaration.TopGroupInvoice.Charges.RemoveAndDeleteAll();

		InvoiceHeader.Charges.AddNew(OverseasFreightCode, 0, Core.Constants.CurrencyCodes.Poland);
		InvoiceHeader.Charges.AddNew(OverseasInsuranceCode, 20, Declaration.LocalCurrencyCode);

		InvoiceLine.JI_LinePrice = 980m;
		Declaration.ResumeApportionment();
		CombineAssertions(() =>
		{
			AssertEquals("Two apportioned charges", 1, InvoiceLine.ApportionedCharges.Count);
			AssertEquals("Line OFT amount", 20m, InvoiceLine.JI_OverseasFreight.Amount);
			AssertEquals("Line OFT currency", Core.Constants.CurrencyCodes.Poland, InvoiceLine.JI_OverseasFreight.Currency.Code);
			AssertEquals("Line ONS currency", Declaration.LocalCurrencyCode, InvoiceLine.JI_OverseasInsurance.Currency.Code);
			AssertEquals("JI_Calc_Freight", 20m, InvoiceLine.JI_Calc_FreightInInvoiceCurr);
		});
	}

	#region JI_MarkModel

	public void TestJI_MarkModel()
	{
		AddCarMarkModelCodesForTests();

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		AssertEquals(typeof(ZString), invoiceLine.JI_MarkModelInfo.PropertyType);
		AssertEquals(typeof(ZString), invoiceLine.FirstVehicle.CVH_ModelNameInfo.PropertyType);
		AssertEquals(typeof(ZString), invoiceLine.JI_BrandNameInfo.PropertyType);
		AssertEquals(50, invoiceLine.JI_BrandNameInfo.MaxLength);

		AssertEquals(true, invoiceLine.JI_BrandName.IsEmpty);
		AssertEquals(true, invoiceLine.FirstVehicle.CVH_ModelName.IsEmpty);
		AssertEquals(true, invoiceLine.JI_MarkModel.IsEmpty);

		invoiceLine.JI_MarkModel = "ASD,BCD";
		AssertEquals(false, invoiceLine.JI_BrandName.IsEmpty);
		AssertEquals("ASD", invoiceLine.JI_BrandName);
		AssertEquals(false, invoiceLine.FirstVehicle.CVH_ModelName.IsEmpty);
		AssertEquals("BCD", invoiceLine.FirstVehicle.CVH_ModelName);
		AssertEquals(false, invoiceLine.JI_MarkModel.IsEmpty);
		AssertEquals("ASD,BCD", invoiceLine.JI_MarkModel);

		invoiceLine.JI_MarkModel = ",";
		AssertEquals(true, invoiceLine.JI_BrandName.IsEmpty);
		AssertEquals(true, invoiceLine.FirstVehicle.CVH_ModelName.IsEmpty);
		AssertEquals(true, invoiceLine.JI_MarkModel.IsEmpty);

		invoiceLine.JI_MarkModel = "asd";
		AssertEquals(false, invoiceLine.JI_BrandName.IsEmpty);
		AssertEquals("asd", invoiceLine.JI_BrandName);
		AssertEquals(true, invoiceLine.FirstVehicle.CVH_ModelName.IsEmpty);
		AssertEquals(false, invoiceLine.JI_MarkModel.IsEmpty);
		AssertEquals("asd,", invoiceLine.JI_MarkModel);

		invoiceLine.JI_MarkModel = "asd,";
		AssertEquals(false, invoiceLine.JI_BrandName.IsEmpty);
		AssertEquals("asd", invoiceLine.JI_BrandName);
		AssertEquals(true, invoiceLine.FirstVehicle.CVH_ModelName.IsEmpty);
		AssertEquals(false, invoiceLine.JI_MarkModel.IsEmpty);
		AssertEquals("asd,", invoiceLine.JI_MarkModel);

		invoiceLine.JI_MarkModel = "1234567890qwertyuiopasdfghjklzxcvbnm1234567890qwertyuiopasdfghjklzxcvbnm,";
		AssertEquals(false, invoiceLine.JI_BrandName.IsEmpty);
		AssertEquals("1234567890qwertyuiopasdfghjklzxcvbnm1234567890qwer", invoiceLine.JI_BrandName);
		AssertEquals(true, invoiceLine.FirstVehicle.CVH_ModelName.IsEmpty);
		AssertEquals(false, invoiceLine.JI_MarkModel.IsEmpty);
		AssertEquals("1234567890qwertyuiopasdfghjklzxcvbnm1234567890qwer,", invoiceLine.JI_MarkModel);

		invoiceLine.JI_MarkModel = ",1234567890qwertyuiopasdfghjklzxcvbnm1234567890qwertyuiopasdfghjklzxcvbnm";
		AssertEquals(true, invoiceLine.JI_BrandName.IsEmpty);
		AssertEquals(false, invoiceLine.FirstVehicle.CVH_ModelName.IsEmpty);
		AssertEquals("1234567890qwertyuiopasdfghjklzxcvbnm1234567890qwer", invoiceLine.FirstVehicle.CVH_ModelName);
		AssertEquals(false, invoiceLine.JI_MarkModel.IsEmpty);
		AssertEquals(",1234567890qwertyuiopasdfghjklzxcvbnm1234567890qwer", invoiceLine.JI_MarkModel);

		invoiceLine.JI_MarkModel = "1111";
		AssertEquals(false, invoiceLine.JI_BrandName.IsEmpty);
		AssertEquals("mark1", invoiceLine.JI_BrandName);
		AssertEquals(false, invoiceLine.FirstVehicle.CVH_ModelName.IsEmpty);
		AssertEquals("model1", invoiceLine.FirstVehicle.CVH_ModelName);
		AssertEquals(false, invoiceLine.JI_MarkModel.IsEmpty);
		AssertEquals("mark1,model1", invoiceLine.JI_MarkModel);

		invoiceLine.JI_MarkModel = "2222";
		AssertEquals(false, invoiceLine.JI_BrandName.IsEmpty);
		AssertEquals("mark2", invoiceLine.JI_BrandName);
		AssertEquals(false, invoiceLine.FirstVehicle.CVH_ModelName.IsEmpty);
		AssertEquals("model2", invoiceLine.FirstVehicle.CVH_ModelName);
		AssertEquals(false, invoiceLine.JI_MarkModel.IsEmpty);
		AssertEquals("mark2,model2", invoiceLine.JI_MarkModel);
	}

	void AddCarMarkModelCodesForTests()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		const string codeType = UniversalReferenceConstants.RefCusCodeListType.Codes.CarMarkModel;
		helper.CreateNewOrGetExistingCusCodeType(codeType, "Mark, Model");
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Poland, codeType, "1111", "mark1,model1", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Poland, codeType, "2222", "mark2,model2", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
		Factory.Save();
	}

	public void TestMarkModel()
	{
		AddCarMarkModelCodesForTests();
		var declaration = Factory.New<JobDeclaration>();
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		CombineAssertions(() =>
		{
			AssertNull("Null", invoiceLine.MarkModel);
			invoiceLine.JI_MarkModel = "mark2,model2";
			AssertNotNull("Not Null", invoiceLine.MarkModel);
		});
	}

	public void TestMarkModelCode()
	{
		AddCarMarkModelCodesForTests();
		var declaration = Factory.New<JobDeclaration>();
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		CombineAssertions(() =>
		{
			AssertEquals("Empty", ZString.Empty, invoiceLine.MarkModelCode);
			invoiceLine.JI_MarkModel = "mark2,model2";
			AssertEquals("Not Empty", "2222", invoiceLine.MarkModelCode);
		});
	}

	public void TestMarkModel_Caption() => AssertEquals("Make, Model", DataBoundResourceStrings.GetDataForProperty(InvoiceLine.JI_MarkModelInfo).Caption);

	#endregion

	public void TestJI_NDescription() => CombineAssertions(() =>
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		invoiceLine.JI_CEI = entryInstruction.PK;

		AssertDescriptionMaxLength(declaration, entryInstruction, invoiceLine.JI_NDescriptionInfo);
	});

	public void TestJI_Description() => CombineAssertions(() =>
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		invoiceLine.JI_CEI = entryInstruction.PK;

		AssertDescriptionMaxLength(declaration, entryInstruction, invoiceLine.JI_DescriptionInfo);
	});

	void AssertDescriptionMaxLength(JobDeclaration jobDeclaration,
		CusEntryInstruction entryInstruction,
		ZPropertyInfo propertyInfo)
	{
		jobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		AssertEquals("Import", 512, propertyInfo.MaxLength);

		jobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		using (jobDeclaration.TemporarilySetIsAESTransitionPeriod(true))
		{
			AssertEquals("Export Transition Period", 280, propertyInfo.MaxLength);
		}

		using (jobDeclaration.TemporarilySetIsAESTransitionPeriod(false))
		{
			AssertEquals("Export Outside Transition Period", 512, propertyInfo.MaxLength);
		}
	}

	public void TestPacksMeasure()
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();

		var packageGroup = declaration.Bills.AddNew().PackingGroups.AddNew();
		var basePackage1 = packageGroup.Packages.AddNew();
		var basePackage2 = packageGroup.Packages.AddNew();
		var linePackage1 = invoiceLine.PackagesForInvoiceLinesForBindingOnly.AddNew();
		var linePackage2 = invoiceLine.PackagesForInvoiceLinesForBindingOnly.AddNew();

		linePackage1.Package = basePackage1;
		linePackage1.IsLinked = true;
		linePackage1.PackQty = 2;
		AssertEquals("All packqty greater than 0", true, invoiceLine.PacksMeasure);
		linePackage1.PackQty = 0;
		AssertEquals("Not all packqty greater than 0", false, invoiceLine.PacksMeasure);

		linePackage2.Package = basePackage2;
		linePackage2.IsLinked = true;
		linePackage2.PackQty = 3;
		AssertEquals("Not all packqty greater than 0", false, invoiceLine.PacksMeasure);
		linePackage1.PackQty = 2;
		AssertEquals("All packqty greater than 0", true, invoiceLine.PacksMeasure);
	}

	public void TestAdditionalInfos() => AssertType<AdditionalInfoCollection>(InvoiceLine.AdditionalInfos);

	public new void TestSupportingDocuments() => AssertType<SupportingDocumentCollection>(InvoiceLine.SupportingDocuments);

	public void TestPreviousDocuments() => AssertType<PreviousDocumentCollection>(InvoiceLine.PreviousDocuments);

	protected override Type ExpectedAdditionalProcedureCode => typeof(AdditionalProcedureCode);

	#region AdditionalProcedureCodes

	public void TestAdditionalProcedureCodes_Type()
	{
		AssertType<AdditionalProcedureCodeCollection>(InvoiceLine.AdditionalProcedureCodes);
	}

	public void TestAdditionalProcedureCodesAsString()
	{
		CombineAssertions(() =>
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("IMP, no element", ZString.Empty, InvoiceLine.AdditionalProcedureCodesAsString);

			var addProc = InvoiceLine.AdditionalProcedureCodes.AddNew();
			addProc.CY_Code = "1234A01";
			AssertEquals("IMP, one element", "1234A01", InvoiceLine.AdditionalProcedureCodesAsString);

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals("Change to EXP", ZString.Empty, InvoiceLine.AdditionalProcedureCodesAsString);
		});
	}

	public void TestMaxNumberOfAdditionalProcedureCode()
	{
		CombineAssertions(() =>
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("Import", 99, InvoiceLine.MaxNumberOfAdditionalProcedureCode);

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals("Export", 99, InvoiceLine.MaxNumberOfAdditionalProcedureCode);
		});
	}

	#endregion

	public void TestGetNewValidation()
	{
		CombineAssertions(() =>
		{
			Declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
			AssertType<ImportJobComInvoiceLineValidation>("Import Declaration", InvoiceLine.Validation);

			Declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
			AssertType<ExportJobComInvoiceLineValidation>("Export Declaration", InvoiceLine.Validation);

			Declaration.JE_MessageType = PLJobMessageTypeList.Codes.ExitSummary;
			AssertType<JobComInvoiceLineValidation>("Exit Summary Declaration", InvoiceLine.Validation);

			Declaration.JE_MessageType = EUJobMessageTypeList.Codes.MiscellaneousCustoms;
			AssertType<JobComInvoiceLineValidation>("Miscellaneous Declaration", InvoiceLine.Validation);
		});
	}

	public void TestCommercialInvoiceLinetGetNewValidation()
	{
		var nonPersistentDeclaration = Factory.New<JobDeclaration>();
		nonPersistentDeclaration.MakeNonPersistent();
		var nonPersistentInvoiceLine = nonPersistentDeclaration.Invoices.AddNew().InvoiceLines.AddNew();

		AssertType<CommercialInvoiceLineValidation>(nonPersistentInvoiceLine.Validation);
	}

	public void TestPackagesForInvoiceLinesForBindingOnly()
	{
		var invoiceLine = Factory.NewWithValidTestData<JobComInvoiceLine>();
		AssertEquals(typeof(InvoiceLineCusLinkPackageCollection), invoiceLine.PackagesForInvoiceLinesForBindingOnly.GetType());
	}

	public override void TestGetNewLinkPackValidation()
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		var package = new InvoiceLineCusLinkPackage(invoiceLine);
		CombineAssertions(() =>
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.MiscellaneousCustoms;
			AssertType<InvoiceLinePackageValidation>("Misc", package.GetNewValidation());

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertType<ExportInvoiceLinePackageValidation>("Export", package.GetNewValidation());

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertType<ImportInvoiceLinePackageValidation>("Import", package.GetNewValidation());
		});
	}

	public void TestPackagesPivot()
	{
		var invoiceLine = Factory.NewWithValidTestData<JobComInvoiceLine>();
		AssertEquals(typeof(InvoiceLinePackagePivotCollection), invoiceLine.PackagesPivot.GetType());
	}

	public void TestIsSupportEmptyPackType()
	{
		var declaration = Factory.New<JobDeclaration>();
		var basePackage = declaration.Packages.AddNew();
		var invoiceLine1 = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		var invoiceLine1Package = invoiceLine1.PackagesForInvoiceLinesForBindingOnly.AddNew();
		invoiceLine1Package.Package = basePackage;
		var invLinePackagePivot = (InvoiceLinePackagePivot)basePackage.InvoiceLinePivotCollection.FirstOrDefault();
		invoiceLine1Package.IsLinked = true;

		var bulkPackageCodeList = new List<ZString> { "VQ", "VG", "VL", "VY", "VR", "VO", "VS" };

		basePackage.CW_PackQty = 0;
		foreach (var packType in bulkPackageCodeList)
		{
			basePackage.CW_PackType = packType;
			invoiceLine1Package.PackQty = 0;
			AssertEquals(false, invLinePackagePivot.CHC_NumberOfPacksInfo.HasNotifications());
		}

		basePackage.CW_PackQty = 1;
		basePackage.CW_PackType = "AA";
		invoiceLine1Package.PackQty = 0;
		AssertEquals(true, invLinePackagePivot.CHC_NumberOfPacksInfo.HasNotifications());

		invoiceLine1Package.PackQty = 1;
		AssertEquals(false, invLinePackagePivot.CHC_NumberOfPacksInfo.HasNotifications());

		var invoiceLine2 = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		var invoiceLine2Package = invoiceLine2.PackagesForInvoiceLinesForBindingOnly.AddNew();
		invoiceLine2Package.Package = basePackage;
		invoiceLine2Package.IsLinked = true;
		invoiceLine2Package.PackQty = 0;
		foreach (InvoiceLinePackagePivot linePivot in basePackage.InvoiceLinePivotCollection)
		{
			AssertEquals(false, linePivot.CHC_NumberOfPacksInfo.HasNotifications());
		}
	}

	public void TestJI_FormattedProcedure()
	{
		var dec = Factory.New<JobDeclaration>();
		var invoice = dec.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		CombineAssertions(() =>
		{
			invoiceLine.JI_FormattedProcedure = "12345678";
			AssertEquals("Formatted", "12 34 567", invoiceLine.JI_FormattedProcedure);

			invoiceLine.JI_FormattedProcedure = "123.4567";
			AssertEquals("Formatted and invalid removed", "12 34 567", invoiceLine.JI_FormattedProcedure);
		});
	}

	public void TestIsCarDetailsDataRequired()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		const string codeType = UniversalReferenceConstants.RefCusCodeListType.Codes.CarDetailsRequiredCodes;
		helper.CreateNewOrGetExistingCusCodeType(codeType, "PLCodesForCarDetails");
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Poland, codeType, "1111", "code1", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Poland, codeType, "2222", "code2", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedKingdom, codeType, "3333", "code3", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
		Factory.Save();

		var declaration = Factory.New<JobDeclaration>();
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.JobComInvoiceLines.AddNew();

		CombineAssertions(() =>
		{
			invoiceLine.JI_Tariff = "XXX";
			AssertEquals("Wrong value", false, invoiceLine.IsCarDetailsDataRequired);
			invoiceLine.JI_Tariff = "1111";
			AssertEquals("Correct value", true, invoiceLine.IsCarDetailsDataRequired);
		});
	}

	public void TestProcedureCodeBaseGet()
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoiceHeader = declaration.Invoices.AddNew();
		var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
		CombineAssertions(() =>
		{
			AssertEquals("Empty string", string.Empty, invoiceLine.ProcedureCodeBase);
			invoiceLine.JI_Procedure = "1000";
			AssertEquals("Value mapped", "10", invoiceLine.ProcedureCodeBase);
		});
	}

	public void TestProcedureCodeBaseSet()
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoiceHeader = declaration.Invoices.AddNew();
		var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
		CombineAssertions(() =>
		{
			invoiceLine.ProcedureCodeBase = "10";
			AssertEquals("Empty before", "10", invoiceLine.JI_Procedure);
			invoiceLine.JI_Procedure = "1000";
			invoiceLine.ProcedureCodeBase = "20";
			AssertEquals("Not empty before", "2000", invoiceLine.JI_Procedure);
			invoiceLine.ProcedureCodeBase = "";
			AssertEquals("Set empty", string.Empty, invoiceLine.JI_Procedure);
		});
	}

	public void TestPreviousProcedureCodeGet()
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoiceHeader = declaration.Invoices.AddNew();
		var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
		CombineAssertions(() =>
		{
			AssertEquals("Empty string", string.Empty, invoiceLine.PreviousProcedureCode);
			invoiceLine.JI_Procedure = "10";
			AssertEquals("Previous procedure empty", string.Empty, invoiceLine.PreviousProcedureCode);
			invoiceLine.JI_Procedure = "1000";
			AssertEquals("Value mapped", "00", invoiceLine.PreviousProcedureCode);
		});
	}

	public void TestPreviousProcedureCodeSet()
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoiceHeader = declaration.Invoices.AddNew();
		var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
		CombineAssertions(() =>
		{
			invoiceLine.PreviousProcedureCode = "10";
			AssertEquals("Empty before", "0010", invoiceLine.JI_Procedure);
			invoiceLine.JI_Procedure = "10";
			invoiceLine.PreviousProcedureCode = "20";
			AssertEquals("Previous procedure empty before", "1020", invoiceLine.JI_Procedure);
			invoiceLine.JI_Procedure = "1010";
			invoiceLine.PreviousProcedureCode = "30";
			AssertEquals("Not empty before", "1030", invoiceLine.JI_Procedure);
			invoiceLine.PreviousProcedureCode = "";
			AssertEquals("Set empty", "10", invoiceLine.JI_Procedure);
		});
	}

	public void TestJI_DateForDutyOverride_Caption()
	{
		AssertEquals("Date for Duty", DataBoundResourceStrings.GetDataForProperty(typeof(JobComInvoiceLine), nameof(JobComInvoiceLine.JI_DateForDutyOverride)).Caption);
	}

	public void TestJI_CustomsThirdQuantity_Caption()
	{
		AssertEquals("Additional Qty 2", DataBoundResourceStrings.GetDataForProperty(typeof(JobComInvoiceLine), nameof(JobComInvoiceLine.JI_CustomsThirdQuantity)).Caption);
	}

	public void TestJI_CustomsFifthQuantity_Caption()
	{
		AssertEquals("Additional Qty 4", DataBoundResourceStrings.GetDataForProperty(typeof(JobComInvoiceLine), nameof(JobComInvoiceLine.JI_CustomsFifthQuantity)).Caption);
	}

	public void TestJI_ValuationDateOverride_Caption()
	{
		AssertEquals("Valuation Date", DataBoundResourceStrings.GetDataForProperty(typeof(JobComInvoiceLine), nameof(JobComInvoiceLine.JI_ValuationDateOverride)).Caption);
	}

	public void TestGetCurrencyConverter()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		invoiceLine.JI_CEI = entryInstruction.PK;

		CombineAssertions(() =>
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertCurrencyConverter("Import fallback CurrencyConverter", ZDateTime.Today);

			entryInstruction.CEI_DateForDuty = new ZDateTime(2022, 02, 11);
			AssertCurrencyConverter("Import EntryInstruction CurrencyConverter", new ZDateTime(2022, 02, 11));

			invoiceLine.JI_ValuationDateOverride = new ZDateTime(2022, 02, 02);
			AssertCurrencyConverter("Import InvoiceLine CurrencyConverter", new ZDateTime(2022, 02, 02));

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			entryInstruction.CEI_DateForDuty = ZDateTime.Empty;
			AssertCurrencyConverter("Export fallback CurrencyConverter", ZDateTime.Today);

			entryInstruction.CEI_DateForDuty = new ZDateTime(2022, 02, 11);
			AssertCurrencyConverter("Export EntryInstruction CurrencyConverter", new ZDateTime(2022, 02, 11));
		});

		void AssertCurrencyConverter(string message, ZDateTime dateForRate)
		{
			var result = invoiceLine.CurrencyConverter;
			AssertEquals($"{message} DateForRate", dateForRate, result.DateForRate);
			AssertEquals($"{message} RateType", ExchangeRateType.Customs, result.RateType);
		}
	}

	public void TestEffectiveAssessmentDate()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		invoiceLine.JI_CEI = entryInstruction.PK;

		CombineAssertions(() =>
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("Import fallback EffectiveAssessmentDate", ZDateTime.Today, invoiceLine.EffectiveAssessmentDate);

			entryInstruction.CEI_DateForDuty = new ZDateTime(2022, 02, 11);
			AssertEquals("Import EntryInstruction EffectiveAssessmentDate", new ZDateTime(2022, 02, 11), invoiceLine.EffectiveAssessmentDate);

			invoiceLine.JI_DateForDutyOverride = new ZDateTime(2022, 02, 02);
			AssertEquals("Import InvoiceLine EffectiveAssessmentDate", new ZDateTime(2022, 02, 02), invoiceLine.EffectiveAssessmentDate);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			entryInstruction.CEI_DateForDuty = ZDateTime.Empty;
			AssertEquals("Export fallback EffectiveAssessmentDate", ZDateTime.Today, invoiceLine.EffectiveAssessmentDate);

			entryInstruction.CEI_DateForDuty = new ZDateTime(2022, 02, 11);
			AssertEquals("Export EntryInstruction EffectiveAssessmentDate", new ZDateTime(2022, 02, 11), invoiceLine.EffectiveAssessmentDate);
		});
	}

	public void TestJI_CustomsSecondUnitQty()
	{
		SetupTariffData();
		var declaration = Factory.New<JobDeclaration>();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		invoiceLine.JI_CEI = entryInstruction.PK;

		CombineAssertions(() =>
		{
			var messageTypesToTest = new List<string> { JobMessageTypeList.Codes.Import, JobMessageTypeList.Codes.Export };
			foreach (var messageType in messageTypesToTest)
			{
				declaration.JE_MessageType = messageType;
				invoiceLine.JI_Tariff = ZString.Empty;
				AssertEquals($"{messageType} Empty tariff", false, invoiceLine.JI_CustomsSecondUnitQtyInfo.ReadOnly);

				invoiceLine.JI_Tariff = "33333333";
				AssertEquals($"{messageType} Invalid tariff", false, invoiceLine.JI_CustomsSecondUnitQtyInfo.ReadOnly);

				invoiceLine.JI_Tariff = "22222222";
				AssertEquals($"{messageType} No ZZ8_Type CU2", false, invoiceLine.JI_CustomsSecondUnitQtyInfo.ReadOnly);
				AssertEquals($"{messageType} JI_CustomsSecondUnitQty is empty", ZString.Empty, invoiceLine.JI_CustomsSecondUnitQty);

				invoiceLine.JI_Tariff = "11111111";
				AssertEquals($"{messageType} ZZ8_Type CU2 with ZZ8_UOM", true, invoiceLine.JI_CustomsSecondUnitQtyInfo.ReadOnly);
				AssertEquals($"{messageType} JI_CustomsSecondUnitQty is type QWE", "QWE", invoiceLine.JI_CustomsSecondUnitQty);
			}
		});
	}

	public void TestJI_CustomsUnitQty()
	{
		SetupTariffData();
		var declaration = Factory.New<JobDeclaration>();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		invoiceLine.JI_CEI = entryInstruction.PK;

		CombineAssertions(() =>
		{
			var messageTypesToTest = new List<string> { JobMessageTypeList.Codes.Import, JobMessageTypeList.Codes.Export };
			foreach (var messageType in messageTypesToTest)
			{
				declaration.JE_MessageType = messageType;
				invoiceLine.JI_Tariff = ZString.Empty;
				AssertEquals($"{messageType} empty tariff", false, invoiceLine.JI_CustomsUnitQtyInfo.ReadOnly);

				invoiceLine.JI_CustomsUnitQty = "KGM";
				invoiceLine.JI_Tariff = "33333333";
				AssertEquals($"{messageType} Invalid tariff", false, invoiceLine.JI_CustomsUnitQtyInfo.ReadOnly);
				AssertEquals($"{messageType} JI_CustomsUnitQty for invalid tariff", "KGM", invoiceLine.JI_CustomsUnitQty);

				invoiceLine.JI_Tariff = "44444444";
				AssertEquals($"{messageType} No ZZ8_Type CU1 but CU2 exists", true, invoiceLine.JI_CustomsUnitQtyInfo.ReadOnly);
				AssertEquals($"{messageType} JI_CustomsUnitQty is CU2 - 1st avaliable", "CU2", invoiceLine.JI_CustomsUnitQty);

				invoiceLine.JI_Tariff = "11111111";
				AssertEquals($"{messageType} ZZ8_Type CU1 with ZZ8_UOM", true, invoiceLine.JI_CustomsUnitQtyInfo.ReadOnly);
				AssertEquals($"{messageType} JI_CustomsSecondUnitQty is type ABC", "ABC", invoiceLine.JI_CustomsUnitQty);
			}
		});
	}

	public void TestCountryOfOrigin_Caption() =>
		CombineAssertions(() =>
		{
			AssertEquals("Caption", "[34] Goods Origin (pref.)", DataBoundResourceStrings.GetDataForProperty(InvoiceLine.JI_CountryOfOriginInfo).Caption);
			AssertEquals("Medium Caption", "[34] Origin (pref.)", DataBoundResourceStrings.GetDataForProperty(InvoiceLine.JI_CountryOfOriginInfo).MediumCaption);
			AssertEquals("Short Caption", "Origin (p.)", DataBoundResourceStrings.GetDataForProperty(InvoiceLine.JI_CountryOfOriginInfo).ShortCaption);
		});

	public void TestCountryOfSupply_Caption() =>
		CombineAssertions(() =>
		{
			AssertEquals("Caption", "Goods Origin", DataBoundResourceStrings.GetDataForProperty(InvoiceLine.ZG_CountryOfSupplyInfo).Caption);
			AssertEquals("Medium Caption", "Country/Region of Origin of the goods being moved.", DataBoundResourceStrings.GetDataForProperty(InvoiceLine.ZG_CountryOfSupplyInfo).MediumCaption);
			AssertEquals("Short Caption", "Origin", DataBoundResourceStrings.GetDataForProperty(InvoiceLine.ZG_CountryOfSupplyInfo).ShortCaption);
		});

	public void TestZG_CountryOfSupplyMaxLength() => AssertEquals(4, InvoiceLine.ZG_CountryOfSupplyInfo.MaxLength);

	public void TestPreviousProcedureCode_Caption() =>
		AssertEquals("Previous Procedure", DataBoundResourceStrings.GetDataForProperty(InvoiceLine.PreviousProcedureCodeInfo).Caption);

	public void TestZG_CountryOfDestination_Caption() =>
		AssertEquals("Destination", DataBoundResourceStrings.GetDataForProperty(InvoiceLine.ZG_CountryOfDestinationInfo).Caption);

	public void TestGetQuotaCustomsUnitDefaultingStrategy()
	{
		var plDataGroupingCode = Core.Constants.CountryCodes.Poland;
		var euDataGroupingCode = Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN;
		var helper = new UniversalReferenceTestDataHelper(Factory);
		var euDataGrouping = helper.CreateNewOrGetExistingDataGrouping(euDataGroupingCode, "EUN");
		helper.CreateNewOrGetExistingDataGrouping(plDataGroupingCode, "PL", euDataGrouping);
		Factory.Save();

		Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		var date = InvoiceLine.EntryInstruction.CEI_DateForDuty;
		var plOrderNumber = "111";
		UniversalReferenceTestDataHelper.CreateOrFindExistingRefCusQuota(Factory, plDataGroupingCode, 10, 5, plOrderNumber,
			Customs.Business.UniversalReferenceConstants.RefCusCodeList.CustomsUq.Weight.Kilogram, date.AddDays(-1), date.AddDays(1));

		var eunOrderNumber = "222";
		UniversalReferenceTestDataHelper.CreateOrFindExistingRefCusQuota(Factory, euDataGroupingCode, 10, 5, eunOrderNumber,
			Customs.Business.UniversalReferenceConstants.RefCusCodeList.CustomsUq.Weight.Hectokilogram, date.AddDays(-1), date.AddDays(1));

		CombineAssertions(() =>
		{
			InvoiceLine.JI_CustomsUnitQty = ZString.Empty;
			InvoiceLine.JI_ConcessionOrder = plOrderNumber;
			AssertEquals("Should work for import PL grouping", Customs.Business.UniversalReferenceConstants.RefCusCodeList.CustomsUq.Weight.Kilogram,
				InvoiceLine.JI_CustomsUnitQty);

			InvoiceLine.JI_CustomsUnitQty = ZString.Empty;
			InvoiceLine.JI_ConcessionOrder = eunOrderNumber;
			AssertEquals("Should work for EUN grouping", Customs.Business.UniversalReferenceConstants.RefCusCodeList.CustomsUq.Weight.Hectokilogram, InvoiceLine.JI_CustomsUnitQty);

			InvoiceLine.JI_ConcessionOrder = ZString.Empty;
			InvoiceLine.JI_CustomsUnitQty = ZString.Empty;
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			InvoiceLine.JI_ConcessionOrder = plOrderNumber;
			AssertEquals("Should not work for export", string.Empty, InvoiceLine.JI_CustomsUnitQty);
		});
	}

	public void TestJI_CustomsUnitQtyWithoutTariff()
	{
		var invoiceLineExport = SetupTestDeclarationWithInsructionAndGetInvoiceLine(JobMessageTypeList.Codes.Export);
		invoiceLineExport.JI_Tariff = string.Empty;
		var invoiceLineImport = SetupTestDeclarationWithInsructionAndGetInvoiceLine(JobMessageTypeList.Codes.Import);
		invoiceLineImport.JI_Tariff = string.Empty;
		var orderNumber = "123";
		UniversalReferenceTestDataHelper.CreateOrFindExistingRefCusQuota(Factory, GroupingCode, 10, 5, orderNumber, "UM9", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));

		CombineAssertions(() =>
		{
			AssertAllJI_CustomsUnitQty("without tarrif", invoiceLineExport, false, "KGM", false, string.Empty, false, string.Empty, false, string.Empty, false, string.Empty);
			AssertAllJI_CustomsUnitQty("without tarrif", invoiceLineImport, false, "KGM", false, string.Empty, false, string.Empty, false, string.Empty, false, string.Empty);
			invoiceLineImport.JI_ConcessionOrder = orderNumber;
			AssertAllJI_CustomsUnitQty("without tarrif but with quota", invoiceLineImport, false, "KGM", true, "UM9", false, string.Empty, false, string.Empty, false, string.Empty);
		});
	}

	public void TestJI_CustomsUnitQtyWithInvalidTariff()
	{
		var invoiceLineExport = SetupTestDeclarationWithInsructionAndGetInvoiceLine(JobMessageTypeList.Codes.Export);
		invoiceLineExport.JI_Tariff = "12345678";
		var invoiceLineImport = SetupTestDeclarationWithInsructionAndGetInvoiceLine(JobMessageTypeList.Codes.Import);
		invoiceLineImport.JI_Tariff = "12345678";
		var orderNumber = "123";
		UniversalReferenceTestDataHelper.CreateOrFindExistingRefCusQuota(Factory, GroupingCode, 10, 5, orderNumber, "UM9", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));

		CombineAssertions(() =>
		{
			AssertAllJI_CustomsUnitQty("invalid tarrif", invoiceLineExport, false, "KGM", false, string.Empty, false, string.Empty, false, string.Empty, false, string.Empty);
			AssertAllJI_CustomsUnitQty("invalid tarrif", invoiceLineImport, false, "KGM", false, string.Empty, false, string.Empty, false, string.Empty, false, string.Empty);
			invoiceLineImport.JI_ConcessionOrder = orderNumber;
			AssertAllJI_CustomsUnitQty("invalid tarrif", invoiceLineImport, false, "KGM", true, "UM9", false, string.Empty, false, string.Empty, false, string.Empty);
		});
	}

	public void TestJI_CustomsUnitQtyForTarrifs()
	{
		(string, string[])[] testTariffs = new[]
		{
			("00000000", new string[] { null, null, null,null, null }),
			("10000000", new string[] { "CU1", null, null, null, null }),
			("20000000", new string[] { null, "CU2", null, null, null }),
			("30000000", new string[] { null, null, "CU3", null, null }),
			("40000000", new string[] { null, null, null, "CU4", null }),
			("50000000", new string[] { null, null, null, null, "CU5" }),
			("12000000", new string[] { "CU1", "CU2", null, null, null }),
			("13000000", new string[] { "CU1", null, "CU3", null, null }),
			("23000000", new string[] { null, "CU2", "CU3", null, null }),
			("12300000", new string[] { "CU1", "CU2", "CU3", null, null }),
			("12345000", new string[] { "CU1", "CU2", "CU3", "CU4", "CU5" }),
		};

		(string, string, string, string, (bool, string)[])[] testCasesForCustomsUnitQty = new[]
		{
			("empty tariff",        "00000000", "EXP", null, new[] { (false, string.Empty), (false, string.Empty), (false, string.Empty), (false, string.Empty), (false, string.Empty) }),
			("CU1 tariff",          "10000000", "EXP", null, new[] { (true, "CU1"), (false, string.Empty), (false, string.Empty), (false, string.Empty), (false, string.Empty) }),
			("CU2 tariff",          "20000000", "EXP", null, new[] { (true, "CU2"), (false, string.Empty), (false, string.Empty), (false, string.Empty), (false, string.Empty) }),
			("CU3 tariff",          "30000000", "EXP", null, new[] { (true, "CU3"), (false, string.Empty), (false, string.Empty), (false, string.Empty), (false, string.Empty) }),
			("CU1&CU2 tariff",      "12000000", "EXP", null, new[] { (true, "CU1"), (true, "CU2"), (false, string.Empty), (false, string.Empty), (false, string.Empty) }),
			("CU1&CU3 tariff",      "13000000", "EXP", null, new[] { (true, "CU1"), (true, "CU3"), (false, string.Empty), (false, string.Empty), (false, string.Empty) }),
			("CU2&CU3 tariff",      "23000000", "EXP", null, new[] { (true, "CU2"), (true, "CU3"), (false, string.Empty), (false, string.Empty), (false, string.Empty) }),
			("CU1&CU2&CU3 tariff",  "12300000", "EXP", null, new[] { (true, "CU1"), (true, "CU2"), (true, "CU3"), (false, string.Empty), (false, string.Empty) }),

			("empty tariff",        "00000000", "IMP", null, new[] { (false, string.Empty), (false, string.Empty), (false, string.Empty), (false, string.Empty), (false, string.Empty) }),
			("CU1 tariff",          "10000000", "IMP", null, new[] { (true, "CU1"), (false, string.Empty), (false, string.Empty), (false, string.Empty), (false, string.Empty) }),
			("CU2 tariff",          "20000000", "IMP", null, new[] { (true, "CU2"), (false, string.Empty), (false, string.Empty), (false, string.Empty), (false, string.Empty) }),
			("CU3 tariff",          "30000000", "IMP", null, new[] { (true, "CU3"), (false, string.Empty), (false, string.Empty), (false, string.Empty), (false, string.Empty) }),
			("CU4 tariff",          "40000000", "IMP", null, new[] { (true, "CU4"), (false, string.Empty), (false, string.Empty), (false, string.Empty), (false, string.Empty) }),
			("CU5 tariff",          "50000000", "IMP", null, new[] { (true, "CU5"), (false, string.Empty), (false, string.Empty), (false, string.Empty), (false, string.Empty) }),
			("CU1&CU2 tariff",      "12000000", "IMP", null, new[] { (true, "CU1"), (true, "CU2"), (false, string.Empty), (false, string.Empty), (false, string.Empty) }),
			("CU1&CU3 tariff",      "13000000", "IMP", null, new[] { (true, "CU1"), (true, "CU3"), (false, string.Empty), (false, string.Empty), (false, string.Empty) }),
			("CU2&CU3 tariff",      "23000000", "IMP", null, new[] { (true, "CU2"), (true, "CU3"), (false, string.Empty), (false, string.Empty), (false, string.Empty) }),
			("CU1&CU2&CU3 tariff",  "12300000", "IMP", null, new[] { (true, "CU1"), (true, "CU2"), (true, "CU3"), (false, string.Empty), (false, string.Empty) }),
			("CU1&CU2&CU3&CU4&CU5 tariff", "12345000", "IMP", null, new[] { (true, "CU1"), (true, "CU2"), (true, "CU3"), (true, "CU4"), (true, "CU5") }),

			("empty tariff and quota",       "00000000", "IMP", "CU9", new[] { (true, "CU9"), (false, string.Empty), (false, string.Empty), (false, string.Empty), (false, string.Empty) }),
			("CU1 tariff and quota",         "10000000", "IMP", "CU9", new[] { (true, "CU1"), (true, "CU9"), (false, string.Empty), (false, string.Empty), (false, string.Empty) }),
			("CU1 tariff and quota CU1",     "10000000", "IMP", "CU1", new[] { (true, "CU1"), (false, string.Empty), (false, string.Empty), (false, string.Empty), (false, string.Empty) }),
			("CU2 tariff and quota",         "20000000", "IMP", "CU9", new[] { (true, "CU2"), (true, "CU9"), (false, string.Empty), (false, string.Empty), (false, string.Empty) }),
			("CU2 tariff and quota CU2",     "20000000", "IMP", "CU2", new[] { (true, "CU2"), (false, string.Empty), (false, string.Empty), (false, string.Empty), (false, string.Empty) }),
			("CU3 tariff and quota",         "30000000", "IMP", "CU9", new[] { (true, "CU3"), (true, "CU9"), (false, string.Empty), (false, string.Empty), (false, string.Empty) }),
			("CU3 tariff and quota CU3",     "30000000", "IMP", "CU3", new[] { (true, "CU3"), (false, string.Empty), (false, string.Empty), (false, string.Empty), (false, string.Empty) }),
			("CU1&CU2 tariff and quota",     "12000000", "IMP", "CU9", new[] { (true, "CU1"), (true, "CU2"), (true, "CU9"), (false, string.Empty), (false, string.Empty) }),
			("CU1&CU2 tariff and quota CU1", "12000000", "IMP", "CU1", new[] { (true, "CU1"), (true, "CU2"), (false, string.Empty), (false, string.Empty), (false, string.Empty) }),
			("CU1&CU2 tariff and quota CU2", "12000000", "IMP", "CU2", new[] { (true, "CU1"), (true, "CU2"), (false, string.Empty), (false, string.Empty), (false, string.Empty) }),
			("CU1&CU3 tariff and quota",     "13000000", "IMP", "CU9", new[] { (true, "CU1"), (true, "CU3"), (true, "CU9"), (false, string.Empty), (false, string.Empty) }),
			("CU1&CU3 tariff and quota CU1", "13000000", "IMP", "CU1", new[] { (true, "CU1"), (true, "CU3"), (false, string.Empty), (false, string.Empty), (false, string.Empty) }),
			("CU1&CU3 tariff and quota CU3", "13000000", "IMP", "CU3", new[] { (true, "CU1"), (true, "CU3"), (false, string.Empty), (false, string.Empty), (false, string.Empty) }),
			("CU2&CU3 tariff and quota",     "23000000", "IMP", "CU9", new[] { (true, "CU2"), (true, "CU3"), (true, "CU9"), (false, string.Empty), (false, string.Empty) }),
			("CU2&CU3 tariff and quota CU2", "23000000", "IMP", "CU2", new[] { (true, "CU2"), (true, "CU3"), (false, string.Empty), (false, string.Empty), (false, string.Empty) }),
			("CU2&CU3 tariff and quota CU3", "23000000", "IMP", "CU3", new[] { (true, "CU2"), (true, "CU3"), (false, string.Empty), (false, string.Empty), (false, string.Empty) }),
			("CU1&CU2&CU3 tariff and quota", "12300000", "IMP", "CU9", new[] { (true, "CU1"), (true, "CU2"), (true, "CU3"), (true, "CU9"), (false, string.Empty) }),
			("CU1&CU2&CU3&CU4&CU5 tariff and quota", "12345000", "IMP", "CU9", new[] { (true, "CU1"), (true, "CU2"), (true, "CU3"), (true, "CU4"), (true, "CU5") }),
		};

		var helper = new UniversalReferenceTestDataHelper(Factory);
		var tariffTypeImport = helper.CreateNewOrGetExistingTariffType(GroupingCode, TariffTypes.Import);
		var tariffTypeExport = helper.CreateNewOrGetExistingTariffType(GroupingCode, TariffTypes.Export);
		Factory.Save();

		CombineAssertions(() =>
		{
			foreach (var (description, tariffCode, tariffType, quotaUom, expectedUnitQtyStates) in testCasesForCustomsUnitQty)
			{
				var (_, uoms) = testTariffs.First(x => x.Item1 == tariffCode);
				var tariffTypePK = tariffType == "IMP" ? tariffTypeImport.PK : tariffTypeExport.PK;
				var messageType = tariffType == "IMP" ? JobMessageTypeList.Codes.Import : JobMessageTypeList.Codes.Export;

				CreateTariffIfNotExists(helper, tariffTypePK, tariffCode, uoms[0], uoms[1], uoms[2], uoms[3], uoms[4]);
				Factory.Save();

				var invoiceLine = SetupTestDeclarationWithInsructionAndGetInvoiceLine(messageType);
				invoiceLine.JI_Tariff = tariffCode;

				if (!string.IsNullOrEmpty(quotaUom))
				{
					var orderNumber = "123" + quotaUom;
					UniversalReferenceTestDataHelper.CreateOrFindExistingRefCusQuota(Factory, GroupingCode, 10, 5, orderNumber, quotaUom, ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
					invoiceLine.JI_ConcessionOrder = orderNumber;
				}

				var (uomReadOnly1, uom1) = expectedUnitQtyStates[0];
				var (uomReadOnly2, uom2) = expectedUnitQtyStates[1];
				var (uomReadOnly3, uom3) = expectedUnitQtyStates[2];
				var (uomReadOnly4, uom4) = expectedUnitQtyStates[3];
				var (uomReadOnly5, uom5) = expectedUnitQtyStates[4];
				AssertAllJI_CustomsUnitQty(description, invoiceLine, uomReadOnly1, uom1, uomReadOnly2, uom2, uomReadOnly3, uom3, uomReadOnly4, uom4, uomReadOnly5, uom5);
			}
		});
	}

	public void TestJI_CustomsUnitQtyWithQuotaSwitchBackToLowTariff()
	{
		var orderNumber = "123";
		UniversalReferenceTestDataHelper.CreateOrFindExistingRefCusQuota(Factory, GroupingCode, 10, 5, orderNumber, "UM9", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));

		var helper = new UniversalReferenceTestDataHelper(Factory);
		var tariffTypeImport = helper.CreateNewOrGetExistingTariffType(GroupingCode, TariffTypes.Import);
		Factory.Save();

		const string tariffWithCu1Code = "11111111";
		const string tariffWithCu2Code = "22222222";
		const string tariffWithCu3Code = "33333333";
		CreateTariffIfNotExists(helper, tariffTypeImport.PK, tariffWithCu1Code, "UM1", null, null, null, null);
		CreateTariffIfNotExists(helper, tariffTypeImport.PK, tariffWithCu2Code, "UM1", "UM2", null, null, null);
		CreateTariffIfNotExists(helper, tariffTypeImport.PK, tariffWithCu3Code, "UM1", "UM2", "UM3", null, null);
		Factory.Save();

		var invoiceLine = SetupTestDeclarationWithInsructionAndGetInvoiceLine(JobMessageTypeList.Codes.Import);

		CombineAssertions(() =>
		{
			invoiceLine.JI_Tariff = tariffWithCu3Code;
			invoiceLine.JI_ConcessionOrder = orderNumber;
			AssertAllJI_CustomsUnitQty("CU3 tariff and Quota", invoiceLine, true, "UM1", true, "UM2", true, "UM3", true, "UM9", false, ZString.Empty);
			invoiceLine.JI_Tariff = tariffWithCu2Code;
			AssertAllJI_CustomsUnitQty("CU2 tariff after CU3&Quota ignors quota", invoiceLine, true, "UM1", true, "UM2", false, ZString.Empty, false, ZString.Empty, false, ZString.Empty);
			invoiceLine.JI_ConcessionOrder = string.Empty;
			invoiceLine.JI_ConcessionOrder = orderNumber;
			AssertAllJI_CustomsUnitQty("CU2 tariff and Quota", invoiceLine, true, "UM1", true, "UM2", true, "UM9", false, ZString.Empty, false, ZString.Empty);
			invoiceLine.JI_Tariff = tariffWithCu1Code;
			AssertAllJI_CustomsUnitQty("CU1 tariff after CU2&Quota ignors quota", invoiceLine, true, "UM1", false, ZString.Empty, false, ZString.Empty, false, ZString.Empty, false, ZString.Empty);
			invoiceLine.JI_ConcessionOrder = string.Empty;
			invoiceLine.JI_ConcessionOrder = orderNumber;
			AssertAllJI_CustomsUnitQty("CU1 tariff and Quota", invoiceLine, true, "UM1", true, "UM9", false, ZString.Empty, false, ZString.Empty, false, ZString.Empty);
		});
	}

	public void TestJI_CustomsUnitQtyWithRateUOMsReadOnly()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		var stdTradeGroup = helper.CreateTradeGroup(GroupingCode, "STANDARD", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
		var impTariffType = helper.CreateNewOrGetExistingTariffType(GroupingCode, CusTariffTypes.ImportTariff);
		var stdPreference = helper.CreatePreferenceView("STD", "Standard", GroupingCode);
		var tariff = CreateTariffIfNotExists(helper, impTariffType.PK, "2208905400", "CU1", "CU2", ZString.Empty, ZString.Empty, ZString.Empty, "DTY");
		var rateType = helper.CreateNewOrGetExistingRateType(GroupingCode, RefCusRateTypes.Dty, "Duty");
		var rateCode = helper.LoadOrCreateNewCusRateCode(Factory, EU.Business.UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts, rateType.PK);
		var rate = helper.CreateRate(tariff, rateCode.PK, ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1), rateFormula: "0.500*[NAR]+0.500*[KGM]+0.500*[MIL]", preferencePk: stdPreference.PK);
		var milUOM = helper.CreateRateUOM(rate.PK, "MIL");

		helper.CreateCusApplicability(rate.PK, stdTradeGroup, ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
		Factory.Save();

		var invoiceLine = SetupTestDeclarationWithInsructionAndGetInvoiceLine(JobMessageTypeList.Codes.Import);

		CombineAssertions(() =>
		{
			invoiceLine.JI_PrimaryPreference = stdPreference.ZZS_Preference;
			invoiceLine.JI_Tariff = tariff.ZZ1_TariffCode;

			invoiceLine.ExecuteCustomsUnitDefaultingStrategy();
			AssertAllJI_CustomsUnitQty("MIL makes it readonly because is a valid RateUOM", invoiceLine, true, "CU1", true, "CU2", true, "MIL", false, ZString.Empty, false, ZString.Empty);

			milUOM.Delete();
			invoiceLine.ExecuteCustomsUnitDefaultingStrategy();
			AssertAllJI_CustomsUnitQty("MIL removed from DB, it isn't readonly", invoiceLine, true, "CU1", true, "CU2", false, ZString.Empty, false, ZString.Empty, false, ZString.Empty);
		});
	}

	public void TestJI_CustomsUnitQtyWithExciseRateUOMsReadOnly()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		var stdTradeGroup = helper.CreateTradeGroup(GroupingCode, "STANDARD", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
		var impTariffType = helper.CreateNewOrGetExistingTariffType(GroupingCode, CusTariffTypes.ImportTariff);
		var stdPreference = helper.CreatePreferenceView("STD", "Standard", GroupingCode);
		var tariff = CreateTariffIfNotExists(helper, impTariffType.PK, "2208905400", "CU1", "CU2", ZString.Empty, ZString.Empty, ZString.Empty, "DTY");
		var rateType = helper.CreateNewOrGetExistingRateType(GroupingCode, RefCusRateTypes.Excise, "Excise");
		var rateCode = helper.LoadOrCreateNewCusRateCode(Factory, TaxTypeList.Codes.ExciseTax, rateType.PK);
		var rate = helper.CreateRate(tariff, rateCode.PK, ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1), rateFormula: "0.500*[NAR]+0.500*[KGM]+0.500*[LPX]", preferencePk: stdPreference.PK);
		var lpxUOM = helper.CreateRateUOM(rate.PK, "LPX");

		helper.CreateCusApplicability(rate.PK, stdTradeGroup, ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
		Factory.Save();

		var invoiceLine = SetupTestDeclarationWithInsructionAndGetInvoiceLine(JobMessageTypeList.Codes.Import);

		CombineAssertions(() =>
		{
			invoiceLine.JI_PrimaryPreference = stdPreference.ZZS_Preference;
			invoiceLine.JI_Tariff = tariff.ZZ1_TariffCode;

			invoiceLine.ExecuteCustomsUnitDefaultingStrategy();
			AssertAllJI_CustomsUnitQty("LPX makes it readonly because is a valid RateUOM", invoiceLine, true, "CU1", true, "CU2", true, "LPX", false, ZString.Empty, false, ZString.Empty);

			lpxUOM.Delete();
			invoiceLine.ExecuteCustomsUnitDefaultingStrategy();
			AssertAllJI_CustomsUnitQty("LPX removed from DB, it isn't readonly", invoiceLine, true, "CU1", true, "CU2", false, ZString.Empty, false, ZString.Empty, false, ZString.Empty);
		});
	}

	public void TestCountryOfOriginFallback_CountryOfOriginIsEmtpy()
	{
		var invoiceLine = SetupTestDeclarationWithInsructionAndGetInvoiceLine(JobMessageTypeList.Codes.Import);
		invoiceLine.JI_CountryOfOrigin = ZString.Empty;

		AssertNull(invoiceLine.CountryOfOriginFallback);
	}

	public void TestCountryOfOriginFallback_CountryOfOriginFoundInCountryOfOrigins()
	{
		var invoiceLine = SetupTestDeclarationWithInsructionAndGetInvoiceLine(JobMessageTypeList.Codes.Import);
		invoiceLine.JI_CountryOfOrigin = "US";

		CombineAssertions(() =>
		{
			AssertEquals("US", invoiceLine.CountryOfOriginFallback.RN_Code);
			AssertEquals("United States", invoiceLine.CountryOfOriginFallback.RN_Desc);
		});
	}

	public void TestCountryOfOriginFallback_CountryOfOriginNotFoundInCountryOfOrigins()
	{
		var invoiceLine = SetupTestDeclarationWithInsructionAndGetInvoiceLine(JobMessageTypeList.Codes.Import);
		invoiceLine.JI_CountryOfOrigin = "ZZ";

		CombineAssertions(() =>
		{
			AssertEquals("ZZ", invoiceLine.CountryOfOriginFallback.RN_Code);
			AssertEquals(string.Empty, invoiceLine.CountryOfOriginFallback.RN_Desc);
		});
	}

	protected override Type GetExpectedPartType() => typeof(OrgSupplierPart);

	protected override CodeDescriptionPairList GetExpectedCustomsChargeTypeList() => new PLCustomsChargeTypeList();

	protected override BaseJobDeclaration GetJobDeclaration()
	{
		var dec = Factory.New<JobDeclaration>();
		dec.JE_MessageType = JobMessageTypeList.Codes.Import;
		var entry = dec.CustomsEntryInstructions.AddNew();
		entry.CEI_DateForDuty = ZDateTime.UtcToday.AddDays(2);
		return dec;
	}

	protected override BaseJobDeclaration ImportJobDeclaration => GetJobDeclaration();

	protected override string OverseasInsuranceCode => PLCustomsChargeTypeList.Codes.AK;

	protected override string OverseasFreightCode => PLCustomsChargeTypeList.Codes.AK;

	new JobDeclaration Declaration => (JobDeclaration)base.Declaration;

	new JobComInvoiceHeader InvoiceHeader => (JobComInvoiceHeader)base.InvoiceHeader;

	protected override Type GetExpectedCusEntryLineType() => typeof(CusEntryLine);

	protected override void DoMerge(BaseJobDeclaration declaration)
	{
		SetupDataEligibleForMerging(declaration);
		base.DoMerge(declaration);
	}

	protected override Type ExpectedTypeOfApportionedCharges => typeof(JobComInvApportionedChargeCollection<InvoiceLineApportionCharge>);

	protected override Type ExpectedTypeOfCharges => typeof(InvoiceLineChargeCollection<InvoiceLineCharge>);

	protected override Type GetExpectedEntryInstructionType() => typeof(CusEntryInstruction);

	static void SetupDataEligibleForMerging(BaseJobDeclaration declaration)
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
	}

	const string GroupingCode = Core.Constants.CountryCodes.Poland;

	void SetupTariffData()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		var tariffTypeExport = helper.CreateNewOrGetExistingTariffType(GroupingCode, TariffTypes.Export);
		var tariffTypeImport = helper.CreateNewOrGetExistingTariffType(GroupingCode, TariffTypes.Import);
		Factory.Save();

		var tariffImport = helper.CreateTariff(GroupingCode, tariffTypeExport.PK, "11111111", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
		helper.CreateTariffUOM(tariffImport, UnitOfMeasureTypes.StatisticalUOMType, "ABC");
		helper.CreateTariffUOM(tariffImport, UnitOfMeasureTypes.AdditionalUOMType, "QWE");

		var tariffExport = helper.CreateTariff(GroupingCode, tariffTypeImport.PK, "11111111", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
		helper.CreateTariffUOM(tariffExport, UnitOfMeasureTypes.StatisticalUOMType, "ABC");
		helper.CreateTariffUOM(tariffExport, UnitOfMeasureTypes.AdditionalUOMType, "QWE");

		var tariffImport2 = helper.CreateTariff(GroupingCode, tariffTypeExport.PK, "22222222", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
		helper.CreateTariffUOM(tariffImport2, UnitOfMeasureTypes.StatisticalUOMType, "CU1");

		var tariffExport2 = helper.CreateTariff(GroupingCode, tariffTypeImport.PK, "22222222", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
		helper.CreateTariffUOM(tariffExport2, UnitOfMeasureTypes.StatisticalUOMType, "CU1");

		var tariff3 = helper.CreateTariff(GroupingCode, tariffTypeExport.PK, "44444444", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
		helper.CreateTariffUOM(tariff3, UnitOfMeasureTypes.AdditionalUOMType, "CU2");

		tariff3 = helper.CreateTariff(GroupingCode, tariffTypeImport.PK, "44444444", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
		helper.CreateTariffUOM(tariff3, UnitOfMeasureTypes.AdditionalUOMType, "CU2");

		Factory.Save();
	}

	JobComInvoiceLine SetupTestDeclarationWithInsructionAndGetInvoiceLine(ZString messageType)
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = messageType;
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		invoiceLine.JI_CEI = entryInstruction.PK;
		return invoiceLine;
	}

	TariffView CreateTariffIfNotExists(UniversalReferenceTestDataHelper helper, ZGuid tariffTypePk, ZString code, ZString uom1, ZString uom2, ZString uom3, ZString uom4, ZString uom5, string taxFeeCode = "")
	{
		var zQuery = new ZQuery(RefCusTariffSchema.ZZ1_TariffCode, code);
		zQuery.AddToFilter(RefCusTariffSchema.ZZ1_ZZI_TariffType, tariffTypePk);
		zQuery.AddToFilter(RefCusTariffSchema.ZZ1_ZZZ_NKDataGrouping, GroupingCode);
		zQuery.AddToFilter(RefCusTariffSchema.ZZ1_StartDate, ZDateTime.Today.AddDays(-1));
		zQuery.AddToFilter(RefCusTariffSchema.ZZ1_EndDate, ZDateTime.Today.AddDays(1));

		var tariff = Factory.LoadTop1<TariffView>(zQuery);
		if (tariff == null)
		{
			tariff = helper.CreateTariff(GroupingCode, tariffTypePk, code, ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1), taxOrFeeCode: taxFeeCode);
			if (!string.IsNullOrEmpty(uom1))
			{
				helper.CreateTariffUOM(tariff, UnitOfMeasureTypes.StatisticalUOMType, uom1);
			}
			if (!string.IsNullOrEmpty(uom2))
			{
				helper.CreateTariffUOM(tariff, UnitOfMeasureTypes.AdditionalUOMType, uom2);
			}
			if (!string.IsNullOrEmpty(uom3))
			{
				helper.CreateTariffUOM(tariff, UnitOfMeasureTypes.CustomsUOM3Type, uom3);
			}
			if (!string.IsNullOrEmpty(uom4))
			{
				helper.CreateTariffUOM(tariff, UnitOfMeasureTypes.CustomsUOM4Type, uom4);
			}
			if (!string.IsNullOrEmpty(uom5))
			{
				helper.CreateTariffUOM(tariff, UnitOfMeasureTypes.CustomsUOM5Type, uom5);
			}
		}
		return tariff;
	}

	void AssertAllJI_CustomsUnitQty(ZString tariffDescription, JobComInvoiceLine invoiceLine,
		ZBool expectedUnitQty1ReadOnly, ZString expectedUnitQty1,
		ZBool expectedUnitQty2ReadOnly, ZString expectedUnitQty2,
		ZBool expectedUnitQty3ReadOnly, ZString expectedUnitQty3,
		ZBool expectedUnitQty4ReadOnly, ZString expectedUnitQty4,
		ZBool expectedUnitQty5ReadOnly, ZString expectedUnitQty5)
	{
		var messageType = invoiceLine.Declaration.JE_MessageType;
		AssertEquals($"{messageType} with {tariffDescription} - {nameof(invoiceLine.JI_CustomsUnitQtyInfo)}.ReadOnly", expectedUnitQty1ReadOnly, invoiceLine.JI_CustomsUnitQtyInfo.ReadOnly);
		AssertEquals($"{messageType} with {tariffDescription} - {nameof(invoiceLine.JI_CustomsUnitQty)}", expectedUnitQty1, invoiceLine.JI_CustomsUnitQty);
		AssertEquals($"{messageType} with {tariffDescription} - {nameof(invoiceLine.JI_CustomsSecondUnitQtyInfo)}.ReadOnly", expectedUnitQty2ReadOnly, invoiceLine.JI_CustomsSecondUnitQtyInfo.ReadOnly);
		AssertEquals($"{messageType} with {tariffDescription} - {nameof(invoiceLine.JI_CustomsSecondUnitQty)}", expectedUnitQty2, invoiceLine.JI_CustomsSecondUnitQty);
		AssertEquals($"{messageType} with {tariffDescription} - {nameof(invoiceLine.JI_CustomsThirdUnitQtyInfo)}.ReadOnly", expectedUnitQty3ReadOnly, invoiceLine.JI_CustomsThirdUnitQtyInfo.ReadOnly);
		AssertEquals($"{messageType} with {tariffDescription} - {nameof(invoiceLine.JI_CustomsThirdUnitQty)}", expectedUnitQty3, invoiceLine.JI_CustomsThirdUnitQty);
		AssertEquals($"{messageType} with {tariffDescription} - {nameof(invoiceLine.JI_CustomsFourthUnitQtyInfo)}.ReadOnly", expectedUnitQty4ReadOnly, invoiceLine.JI_CustomsFourthUnitQtyInfo.ReadOnly);
		AssertEquals($"{messageType} with {tariffDescription} - {nameof(invoiceLine.JI_CustomsFourthUnitQty)}", expectedUnitQty4, invoiceLine.JI_CustomsFourthUnitQty);
		AssertEquals($"{messageType} with {tariffDescription} - {nameof(invoiceLine.JI_CustomsFifthUnitQtyInfo)}.ReadOnly", expectedUnitQty5ReadOnly, invoiceLine.JI_CustomsFifthUnitQtyInfo.ReadOnly);
		AssertEquals($"{messageType} with {tariffDescription} - {nameof(invoiceLine.JI_CustomsFifthUnitQty)}", expectedUnitQty5, invoiceLine.JI_CustomsFifthUnitQty);
	}

	public override void TestNationalRateSelectionCriteria()
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		CombineAssertions(() =>
		{
			AssertEquals("NationalRateSelectionCriteria count", 1, invoiceLine.NationalRateSelectionCriteria.Count());
			AssertEquals("Only NationalRateSelectionCriteria in PL is ExciseRateSelectionCriteria", invoiceLine.ExciseRateSelectionCriteria, invoiceLine.NationalRateSelectionCriteria.Single());
		});
	}
}
