using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.PL.Business.Declaration.Testing;

[TestedType(typeof(CusEntryLine))]
sealed class CusEntryLineTest : EU.Business.Declaration.Testing.CusEntryLineTest<CusEntryLine, JobComInvoiceLine>
{
	public void TestFees() => AssertType<CusEntryLineFeeCollection>(Factory.New<CusEntryLine>().Fees);

	protected override string OverseasFreightCode => PLCustomsChargeTypeList.Codes.AK;

	public override EU.Business.Declaration.JobDeclaration GetJobDeclarationForTest()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		declaration.CustomsEntryInstructions.AddNew();
		return declaration;
	}

	public void TestValidationType()
	{
		var declaration = GetJobDeclarationForTest();
		var entryLine = (CusEntryLine)declaration.ActiveEntryHeaders.AddNew().AllEntryLines.AddNew();
		CombineAssertions(() =>
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.MiscellaneousCustoms;
			AssertType<CusEntryLineValidation>("Factory CusEntryLine", entryLine.Validation);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertType<ExportCusEntryLineValidation>("Export declaration", entryLine.Validation);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertType<ImportCusEntryLineValidation>("Import declaration", entryLine.Validation);
		});
	}

	protected override Type GetExpectedTaxBoxSupporterType() => typeof(CusEntryLineFee);

	protected override int ExpectedReadOnlySupportingDocumentsCount => 4;

	protected override SupportingDocTestHelper GetSupportingDocTestHelper() => new PLSupportingDocsTestHelper(Factory);

	protected override string StatisticalValueApplicableCharge => PLCustomsChargeTypeList.Codes.AK;

	public void TestTypeOfHeader()
	{
		var declaration = (JobDeclaration)ImportJobDeclaration;
		var instruction = declaration.CustomsEntryInstructions.AddNew();
		var invoice = declaration.Invoices.AddNew();
		invoice.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
		var line = invoice.InvoiceLines.AddNew();
		line.JI_CEI = instruction.PK;

		DoMerge(declaration);
		var entryLine = ((CusEntryHeader)declaration.ActiveEntryHeaders[0]).AllEntryLines[0];

		AssertType<CusEntryHeader>(entryLine.Header);
	}

	protected override BaseJobDeclaration ImportJobDeclaration
	{
		get
		{
			var result = base.ImportJobDeclaration;
			result.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			result.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			return result;
		}
	}

	protected override void DoMerge(BaseJobDeclaration declaration)
	{
		if (declaration.CustomsEntryInstructions.Count <= 0)
		{
			declaration.CustomsEntryInstructions.AddNew();
		}
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		base.DoMerge(declaration);
	}

	public override BaseJobDeclaration SetUpDeclarationForMoneyTest()
	{
		var dec = (JobDeclaration)ImportJobDeclaration;
		dec.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
		dec.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		return dec;
	}

	protected override BaseJobDeclaration SetUpDeclarationAndInvLinesForMoneyTest(RefCurrency currency)
	{
		var declaration = SetUpDeclarationForMoneyTest();
		declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
		declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();
		var invoiceHeader = declaration.Invoices.AddNew();
		invoiceHeader.JZ_RX_NKInvoice_Currency = currency.RX_Code;

		var line1 = invoiceHeader.JobComInvoiceLines.AddNew();
		var line2 = invoiceHeader.JobComInvoiceLines.AddNew();
		line1.JI_Tariff = "2203.10.10 10";
		line2.JI_Tariff = "2203.10.10 10";
		line1.JI_LinePrice = 100.0m;
		line2.JI_LinePrice = 200.0m;

		var line1ONS = line1.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance);
		line1ONS.J7_Amount = 5.0m;
		line1ONS.J7_IsDutiable = true;
		var line1OFT = line1.Charges.AddNew(OverseasFreightCode);
		line1OFT.J7_Amount = 10.0m;
		line1OFT.J7_IsDutiable = true;
		var line2ONS = line2.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance);
		line2ONS.J7_Amount = 10.0m;
		line2ONS.J7_IsDutiable = true;
		var line2OFT = line2.Charges.AddNew(OverseasFreightCode);
		line2OFT.J7_IsDutiable = true;
		line2OFT.J7_Amount = 20.0m;

		AssertEquals("PreReq, line 1 CIF is 110", 110.0m, line1.JI_CIF.Amount);
		AssertEquals("PreReq, line 2 CIF is 230", 220.0m, line2.JI_CIF.Amount);
		AssertEquals("PreReq, line 1 CIF currency is invoice currency", currency.Code, line1.JI_CIF.Currency.Code);
		AssertEquals("PreReq, line 2 CIF currency is invoice currency", currency.Code, line2.JI_CIF.Currency.Code);
		AssertEquals("PreReq, line 1 CIF is 110", 110.0m, line1.JI_Calc_CIF);
		AssertEquals("PreReq, line 2 CIF is 230", 220.0m, line2.JI_Calc_CIF);
		DoMerge(declaration);
		return declaration;
	}

	[TestDate(2005, 6, 2)]
	public override void TestMoneyInLocalCurrency()
	{
		var newCurrency = RefCurrency.New(Factory);
		newCurrency.RX_Code = "MDD";
		var from = new ZDateTime(2005, 6, 1);
		var to = new ZDateTime(2005, 6, 5);
		newCurrency.SetCustomsRate(from, to, RatesAreReciprocal ? 2m : 0.5m);
		var declaration = SetUpDeclarationAndInvLinesForMoneyTest(newCurrency);

		var entryLine = declaration.CustomsEntryHeaders[0].MergedLines[0];
		// Do not assert FOB.  FOB in base is wrong
		AssertEquals("CIF", 660.0m, entryLine.CIFInLocalCurrency.Amount);
		AssertEquals("OFT", 60.0m, entryLine.OverseasFreightInLocalCurrency.Amount);
		AssertEquals("ONS", 0m, entryLine.OverseasInsuranceInLocalCurrency.Amount);
		AssertEquals("T&I", 60.0m, entryLine.TAndIInLocalCurrency.Amount);
	}

	[TestDate(2005, 6, 2)]
	public override void TestMoneyInLocalCurrencyWhenAllChargesAreInLocalCurrency()
	{
		var declaration = SetUpDeclarationAndInvLinesForMoneyTest(GlbCompany.CurrentCompany.LocalCurrency);
		var entryLine = declaration.CustomsEntryHeaders[0].MergedLines[0];
		// Do not assert FOB.  FOB in base is wrong
		AssertEquals("CIF", 330.0m, entryLine.CIFInLocalCurrency.Amount);
		AssertEquals("OFT", 30.0m, entryLine.OverseasFreightInLocalCurrency.Amount);
		AssertEquals("ONS", 0m, entryLine.OverseasInsuranceInLocalCurrency.Amount);
		AssertEquals("T&I", 30.0m, entryLine.TAndIInLocalCurrency.Amount);
	}

	public void TestContainerCount()
	{
		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		declaration.JE_ContainerMode = ContainerModes.FCL;
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		var instruction = declaration.CustomsEntryInstructions.AddNew();
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		var invoiceLine2 = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		invoiceLine.JI_CEI = instruction.PK;
		invoiceLine2.JI_CEI = instruction.PK;

		for (var i = 0; i < 5; i++)
		{
			var container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = $"{i}";
			var package = declaration.Packages.AddNew();
			package.CW_PackQty = i;
			package.CW_PackType = $"{i}";
			package.CW_ContainerNoOrEquipmentNo = container.CO_ContainerNumber;
		}
		var packagesForInvoiceLinesForBindingOnly = invoiceLine.PackagesForInvoiceLinesForBindingOnly;
		for (var i = 0; i < 5; i++)
		{
			packagesForInvoiceLinesForBindingOnly[i].IsLinked = true;
		}

		declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

		var entryHeader = declaration.CustomsEntryHeaders.First();
		var entryLine1 = entryHeader.AllEntryLines.First(x => x.InvoiceLines.Contains(invoiceLine));
		var entryLine2 = entryHeader.AllEntryLines.First(x => x.InvoiceLines.Contains(invoiceLine2));
		CombineAssertions(() =>
		{
			AssertEquals("entryLine container amount - 5 linked", 5, entryLine1.ContainerCount);
			AssertEquals("entryLine container amount - 0 linked", 0, entryLine2.ContainerCount);
		});
	}

	public void TestAdditionalInfoCount()
	{
		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		var instruction = declaration.CustomsEntryInstructions.AddNew();
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		invoiceLine.JI_CEI = instruction.PK;

		for (var i = 0; i < 3; i++)
		{
			var document = declaration.AdditionalInfos.AddNew();
			document.CSI_Code = $"d{i}";
			document.CSI_Description = $"d{i}";
		}

		for (var i = 0; i < 3; i++)
		{
			var document = invoice.AdditionalInfos.AddNew();
			document.CSI_Code = $"i{i}";
			document.CSI_Description = $"i{i}";
		}

		for (var i = 0; i < 3; i++)
		{
			var document = invoiceLine.AdditionalInfos.AddNew();
			document.CSI_Code = $"l{i}";
			document.CSI_Description = $"l{i}";
		}
		declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

		var entryHeader = declaration.CustomsEntryHeaders.First();
		var entryLine = entryHeader.AllEntryLines.First();

		AssertEquals("9 Additional infos in EntryLine", 9, entryLine.AdditionalInfoCount);
	}

	public void TestPreviousDocumentCount()
	{
		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		var instruction = declaration.CustomsEntryInstructions.AddNew();
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		invoiceLine.JI_CEI = instruction.PK;

		for (var i = 0; i < 3; i++)
		{
			var document = declaration.PreviousDocuments.AddNew();
			document.CSI_Code = $"d{i}";
			document.CSI_Description = $"d{i}";
		}

		for (var i = 0; i < 3; i++)
		{
			var document = invoice.PreviousDocuments.AddNew();
			document.CSI_Code = $"i{i}";
			document.CSI_Description = $"i{i}";
		}

		for (var i = 0; i < 3; i++)
		{
			var document = invoiceLine.PreviousDocuments.AddNew();
			document.CSI_Code = $"l{i}";
			document.CSI_Description = $"l{i}";
		}
		declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

		var entryHeader = declaration.CustomsEntryHeaders.First();
		var entryLine = entryHeader.AllEntryLines.First();

		AssertEquals("9 PreviousDocuments in EntryLine", 9, entryLine.PreviousDocumentCount);
	}

	public void TestSupportingDocumentCount()
	{
		AddLineSupportingDocumentReferenceData();
		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		var instruction = declaration.CustomsEntryInstructions.AddNew();
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		invoiceLine.JI_CEI = instruction.PK;

		for (var i = 0; i < 3; i++)
		{
			var document = declaration.SupportingDocuments.AddNew();
			document.CSI_Code = $"d{i}";
			document.CSI_Description = $"d{i}";
		}

		for (var i = 0; i < 3; i++)
		{
			var document = invoice.SupportingDocuments.AddNew();
			document.CSI_Code = $"i{i}";
			document.CSI_Description = $"i{i}";
		}

		var invoiceLineDocument = invoiceLine.SupportingDocuments.AddNew();
		invoiceLineDocument.CSI_Code = "9001";
		invoiceLineDocument.CSI_Description = "9001";

		declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

		var entryHeader = declaration.CustomsEntryHeaders.First();
		var entryLine = entryHeader.AllEntryLines.First();

		AssertEquals("7 SupportingDocuments in EntryLine", 7, entryLine.SupportingDocumentCount);
	}

	void AddLineSupportingDocumentReferenceData()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		var eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
		helper.CreateNewOrGetExistingDataGrouping(GlbCompany.CurrentCompany.Country.Code, GlbCompany.CurrentCompany.Country.RN_Desc, eun);
		var attributeNameValuePairs = new Dictionary<string, string[]>();

		var importCodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection;
		var exportCodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection;
		attributeNameValuePairs.Add("Level", new string[] { "ITEM" });
		helper.CreateCusCodeListsForMultipleTypesWithAttributes(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN,
			new string[] { importCodeType, exportCodeType }, "9001", "9001 DES", attributeNameValuePairs, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

		Factory.Save();
	}

	public void TestCUDCurrencyConverterShouldNotRoundFinalResult()
	{
		PopulateExchangeRateData();
		var declaration = (JobDeclaration)ImportJobDeclaration;
		var instruction = declaration.CustomsEntryInstructions.AddNew();
		var invoice = declaration.Invoices.AddNew();
		invoice.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
		var line = invoice.InvoiceLines.AddNew();
		line.JI_CEI = instruction.PK;

		DoMerge(declaration);
		var entryLine = ((CusEntryHeader)declaration.ActiveEntryHeaders[0]).AllEntryLines[0];

		CombineAssertions(() =>
		{
			var result = entryLine.CUDCurrencyConverter
				.ConvertExact(new Money(ZArchitecture.Core.Utilities.Round(1.0m, 0), entryLine.LocalCurrency), entryLine.EURCurrency)
				.Amount;
			AssertEquals("Round 1.0 to 1.0", 3.3333m, result);

			result = entryLine.CUDCurrencyConverter
				.ConvertExact(new Money(ZArchitecture.Core.Utilities.Round(1.2m, 0), entryLine.LocalCurrency), entryLine.EURCurrency)
				.Amount;
			AssertEquals("Round 1.2 to 1.0", 3.3333m, result);

			result = entryLine.CUDCurrencyConverter
				.ConvertExact(new Money(ZArchitecture.Core.Utilities.Round(1.2m, 1), entryLine.LocalCurrency), entryLine.EURCurrency)
				.Amount;
			AssertEquals("Round 1.2 to 1.2", 3.99996m, result);

			result = entryLine.CUDCurrencyConverter
				.ConvertExact(new Money(ZArchitecture.Core.Utilities.Round(1.5m, 0), entryLine.LocalCurrency), entryLine.EURCurrency)
				.Amount;
			AssertEquals("Round 1.5 to 2.0", 6.6666m, result);

			result = entryLine.CUDCurrencyConverter
				.ConvertExact(new Money(ZArchitecture.Core.Utilities.Round(1.5m, 1), entryLine.LocalCurrency), entryLine.EURCurrency)
				.Amount;
			AssertEquals("Round 1.5 to 1.5", 4.99995m, result);

			result = entryLine.CUDCurrencyConverter
				.ConvertExact(new Money(ZArchitecture.Core.Utilities.Round(1.9m, 0), entryLine.LocalCurrency), entryLine.EURCurrency)
				.Amount;
			AssertEquals("Round 1.9 to 2.0", 6.6666m, result);
		});
	}

	void PopulateExchangeRateData()
	{
		RefExchangeRate exchangeRate = RefExchangeRate.New(Factory);
		exchangeRate.RE_StartDate = ZDateTime.Today.AddDays(-1);
		exchangeRate.RE_ExpiryDate = ZDateTime.Today.AddDays(2);
		exchangeRate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.CustomsMeasureEURExRate;
		exchangeRate.RE_SellRate = 3.3333m;
		exchangeRate.RE_RX_NKExCurrency = CurrencyCodes.EuropeanUnion;
		exchangeRate.RE_GC = GlbCompany.CurrentCompany.PK;

		Factory.Save();
	}

	public void TestStatisticalValueIsRoundedAfterMerge()
	{
		var declaration = (JobDeclaration)ImportJobDeclaration;
		var instruction = declaration.CustomsEntryInstructions.AddNew();
		var invoice = declaration.Invoices.AddNew();
		invoice.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
		var line = invoice.InvoiceLines.AddNew();
		line.JI_CEI = instruction.PK;
		line.JI_LinePrice = 0.0m;

		DoMerge(declaration);
		var entryLine = ((CusEntryHeader)declaration.ActiveEntryHeaders[0]).AllEntryLines[0];

		CombineAssertions(() =>
		{
			AssertEquals("0.0 statistical value - must be at least 1.0", 1.0m, entryLine.CL_StatisticalValue);
			line.JI_LinePrice = 100.1m;
			DoMerge(declaration);
			AssertEquals("JI_LinePrice is 100.1m", 100.0m, entryLine.CL_StatisticalValue);
			line.JI_LinePrice = 100.5m;
			DoMerge(declaration);
			AssertEquals("JI_LinePrice is 100.5m", 101.0m, entryLine.CL_StatisticalValue);
		});
	}

	public void TestCUDCurrencyConverter()
	{
		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		var instruction = declaration.CustomsEntryInstructions.AddNew();
		instruction.CEI_DateForDuty = ZDateTime.Today.AddMonths(2);
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		invoiceLine.JI_CEI = instruction.PK;
		declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

		var entryHeader = declaration.CustomsEntryHeaders.First();
		var entryLine = entryHeader.AllEntryLines.First();
		var currencyConverter = entryLine.CUDCurrencyConverter;

		CombineAssertions(() =>
		{
			AssertEquals("DateForRate ", ZDateTime.Today.AddMonths(2), currencyConverter.DateForRate);
			AssertEquals("RateType", ExchangeRateType.CustomsMeasureEURExRate, currencyConverter.RateType);
		});
	}

	public void TestLocalCurrency()
	{
		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		var instruction = declaration.CustomsEntryInstructions.AddNew();
		instruction.CEI_DateForDuty = ZDateTime.Today.AddMonths(2);
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		invoiceLine.JI_CEI = instruction.PK;
		declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

		var entryHeader = declaration.CustomsEntryHeaders.First();
		var entryLine = entryHeader.AllEntryLines.First();
		AssertEquals(invoiceLine.LocalCurrency, entryLine.LocalCurrency);
	}

	public void TestEURCurrency() => AssertEquals(CurrencyCodes.EuropeanUnion, Factory.New<CusEntryLine>().EURCurrency.Code);

	protected override string TestDutyRateDescriptionExpectedDutyAmountsAsStringA00A30_2 => "A00:35.00\r\nA30:171.00";
	protected override string TestDutyRateDescriptionExpectedDutyAmountsAsStringA30_1 => "A30:171.00";
	protected override string TestDutyRateDescriptionExpectedDutyAmountsAsStringA30_2 => "A30:171.00";

	public void TestApportionedCostsCurrencyConverter()
	{
		var dateForRate = ZDateTime.Today.AddDays(1);

		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		var instruction = declaration.CustomsEntryInstructions.AddNew();
		instruction.CEI_DateForDuty = dateForRate;
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		invoiceLine.JI_CEI = instruction.PK;
		declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

		var entryHeader = declaration.CustomsEntryHeaders.First();
		var entryLine = entryHeader.AllEntryLines.First();

		var entryLineApportionedCostsCurrencyConverter = entryLine.ApportionedCostsCurrencyConverter;

		CombineAssertions(() =>
		{
			AssertEquals("DateForRate", dateForRate, entryLineApportionedCostsCurrencyConverter.DateForRate);
			AssertEquals("RateType", ExchangeRateType.Customs, entryLineApportionedCostsCurrencyConverter.RateType);
		});
	}

	public void TestFiscalReferencesCombined()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction1.CEI_JE = declaration.PK;
		var entryHeader1 = declaration.CustomsEntryHeaders.AddNew();
		entryHeader1.CH_CEI_Instruction = entryInstruction1.PK;
		var entryLine1 = entryHeader1.AllEntryLines.AddNew();

		var entryInstruction1FiscalRef1 = entryInstruction1.FiscalReferences.AddNew();
		var entryInstruction2FiscalRef2 = entryInstruction1.FiscalReferences.AddNew();

		var invoiceLine1 = declaration.InvoiceLines.AddNew();
		var invoiceLine1FiscalRef1 = invoiceLine1.FiscalReferences.AddNew();
		var invoiceLine1FiscalRef2 = invoiceLine1.FiscalReferences.AddNew();
		entryLine1.InvoiceLines.Add(invoiceLine1);

		var invoiceLine2 = declaration.InvoiceLines.AddNew();
		var invoiceLine2FiscalRef1 = invoiceLine1.FiscalReferences.AddNew();
		entryLine1.InvoiceLines.Add(invoiceLine2);

		var entryLine2 = entryHeader1.AllEntryLines.AddNew();
		var invoiceLine3 = declaration.InvoiceLines.AddNew();
		invoiceLine3.FiscalReferences.AddNew();
		entryLine2.InvoiceLines.Add(invoiceLine3);

		var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction2.CEI_JE = declaration.PK;
		var entryHeader2 = declaration.CustomsEntryHeaders.AddNew();
		entryHeader2.CH_CEI_Instruction = entryInstruction2.PK;

		var entryLine3 = entryHeader2.AllEntryLines.AddNew();
		var invoiceLine4 = declaration.InvoiceLines.AddNew();
		invoiceLine4.FiscalReferences.AddNew();
		entryLine3.InvoiceLines.Add(invoiceLine4);

		AssertSequencesEqual(
			new[] { entryInstruction1FiscalRef1, entryInstruction2FiscalRef2, invoiceLine1FiscalRef1, invoiceLine1FiscalRef2, invoiceLine2FiscalRef1 },
			entryLine1.FiscalReferencesCombined);
	}

	public new void TestDutyDetailsForVAT()
	{
		const string rateCodeEA = EU.Business.UniversalReferenceConstants.RefCusRateCodes.AgriculturalComponent;
		const string rateCodeA00 = EU.Business.UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts;
		const string rateCodeA35 = EU.Business.UniversalReferenceConstants.RefCusRateCodes.ProvisionalAntiDumpingDuty;
		const string rateCodeA40 = EU.Business.UniversalReferenceConstants.RefCusRateCodes.DefinitiveCountervailingDuty;
		const string rateCodeB00 = EU.Business.UniversalReferenceConstants.RefCusRateCodes.Vat;

		CusRefRateCodeViewTestHelper.CreateAndSaveCusRateCodeForCountryIfNotExists(Factory, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, Customs.Business.UniversalReferenceConstants.RefCusRateTypes.Dty, rateCodeEA);
		CusRefRateCodeViewTestHelper.CreateAndSaveCusRateCodeForCountryIfNotExists(Factory, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, Customs.Business.UniversalReferenceConstants.RefCusRateTypes.AntiDumpingDuty, rateCodeA35);
		CusRefRateCodeViewTestHelper.CreateAndSaveCusRateCodeForCountryIfNotExists(Factory, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, Customs.Business.UniversalReferenceConstants.RefCusRateTypes.CountervailingDuty, rateCodeA40);
		CusRefRateCodeViewTestHelper.CreateAndSaveCusRateCodeForCountryIfNotExists(Factory, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, Customs.Business.UniversalReferenceConstants.RefCusRateTypes.Vat, rateCodeB00);

		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var entryLine = entryHeader.AllEntryLines.AddNew();

		var fee1 = entryLine.Fees.GetOrAddFeeByFeeType(rateCodeA00);
		fee1.CF_ChargeAmount = 200m;

		CombineAssertions("EntryLine with only 1 fee: A00.", () =>
		{
			AssertEquals("Customs Duty On Industrial Products (A00) fee", 200m, entryLine.Fees.GetAmount(rateCodeA00));
			AssertEquals("DutyDetailsForVAT", 200m, entryLine.DutyDetailsForVAT);
		});

		var fee2 = entryLine.Fees.GetOrAddFeeByFeeType(rateCodeEA);
		fee2.CF_ChargeAmount = 100m;

		CombineAssertions("EntryLine with 2 DTY type fees: A00, EA.", () =>
		{
			AssertEquals("Agricultural Component (EA) fee", 100m, entryLine.Fees.GetAmount(rateCodeEA));
			AssertEquals("DutyDetailsForVAT", 300m, entryLine.DutyDetailsForVAT);
		});

		var fee3 = entryLine.Fees.GetOrAddFeeByFeeType(rateCodeB00);
		fee3.CF_ChargeAmount = 50m;

		CombineAssertions("After adding non DTY type fee VAT, not included in the duty amount calculation.", () =>
		{
			AssertEquals("VAT fee", 50m, entryLine.Fees.GetAmount(rateCodeB00));
			AssertEquals("DutyDetailsForVAT", 300m, entryLine.DutyDetailsForVAT);
		});

		var fee4 = entryLine.Fees.GetOrAddFeeByFeeType(rateCodeA40);
		fee4.CF_ChargeAmount = 70m;

		CombineAssertions("EntryLine with 4 fees, 3 of which Duty types (DTY, ADD, CVD): A00, EA, A40.", () =>
		{
			AssertEquals("Definitive Countervailing Duty (A40) fee", 70m, entryLine.Fees.GetAmount(rateCodeA40));
			AssertEquals("DutyDetailsForVAT", 370m, entryLine.DutyDetailsForVAT);
		});

		var fee5 = entryLine.Fees.GetOrAddFeeByFeeType(rateCodeA35);
		fee5.CF_ChargeAmount = 40m;

		CombineAssertions("After adding fee A35, not included in the duty amount calculation. 3 of 5 are vatable duties: A00, EA, A40.", () =>
		{
			AssertEquals("Provisional Anti-Dumping Duty (A35) fee", 40m, entryLine.Fees.GetAmount(rateCodeA35));
			AssertEquals("DutyDetailsForVAT", 370m, entryLine.DutyDetailsForVAT);
		});

		var fee6 = entryLine.Fees.AddNew();
		fee6.CF_ChargeType = rateCodeA00;
		fee6.CF_ChargeAmount = 60;
		fee6.CF_RateOverrideReasonCode = RateOverrideReasonList.Codes.Additional;

		CombineAssertions("EntryLine with 6 fees, 4 of which are included in the duty amount calculation: A00, EA, A40, A00.", () =>
		{
			AssertEquals("Sum of Customs Duty On Industrial Products (A00) fees", 260m, entryLine.Fees.GetTotalAmount(rateCodeA00, false));
			AssertEquals("DutyDetailsForVAT", 430m, entryLine.DutyDetailsForVAT);
		});
	}

	protected override Type ExpectedTypeOfFees => typeof(CusEntryLineFeeCollection);

	class PLSupportingDocsTestHelper : SupportingDocTestHelper
	{
		public PLSupportingDocsTestHelper(BusinessObjectFactory factory) : base(factory)
		{
		}

		protected override Type GetSupportingDocumentType() => typeof(SupportingDocument);

		public override List<EU.Business.Declaration.MultiLineAddInfos.SupportingDocument> GetSupportingDocsForAggregationMergeTest() => new List<EU.Business.Declaration.MultiLineAddInfos.SupportingDocument>
		{
			GetSupportingDoc(1, "REF111", false),
			GetSupportingDoc(2, "REF222", false),
			GetSupportingDoc(4, "REF444", false),
			GetSupportingDoc(5, "REF555", false),
			GetSupportingDoc(6, "REF222", false),
			GetSupportingDoc(7, "REF444", false),
		};

		public override List<EU.Business.Declaration.MultiLineAddInfos.SupportingDocument> GetExpectedDocsAfterAggregationMerge() => expectedSupportingDocs ?? (expectedSupportingDocs = new List<EU.Business.Declaration.MultiLineAddInfos.SupportingDocument>
		{
			GetSupportingDoc(1, "REF111", false),
			GetSupportingDoc(8, "REF222", false, 20),
			GetSupportingDoc(11, "REF444", false, 20),
			GetSupportingDoc(5, "REF555", false)
		});
	}
}
