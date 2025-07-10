using System;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.PL;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;
using static Enterprise.Customs.PL.Business.Constants;

namespace Enterprise.Customs.PL.Business.Declaration.Testing;

class JobComInvoiceHeaderValidationTest : EU.Business.Declaration.Testing.JobComInvoiceHeaderValidationTest
{
	public void TestCheckJZ_IncoTermPlace()
	{
		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Export;

		var header = declaration.Invoices.AddNew();

		header.JZ_IncoTerm = string.Empty;
		AssertHasMessageErrorContaining(header.JZ_IncoTermInfo, "Please enter an Incoterm.");

		header.JZ_IncoTerm = JZIncoTermList.Codes.CFR;
		AssertNoMessageErrors(header.JZ_IncoTermInfo);

		declaration.JE_MessageType = MessageTypeList.Codes.Import;

		header.JZ_IncoTerm = string.Empty;
		AssertHasMessageErrorContaining(header.JZ_IncoTermInfo, "Please enter an Incoterm.");

		header.JZ_IncoTerm = JZIncoTermList.Codes.CFR;
		AssertNoMessageErrors(header.JZ_IncoTermInfo);

		declaration.JE_MessageType = PLJobMessageTypeList.Codes.ExitSummary;

		header.JZ_IncoTerm = string.Empty;
		AssertNoNotifications("ExitSummaryDeclaration declaration", invoiceHeader.JZ_IncoTermPlaceInfo);
	}

	public void TestCheckAdditionalTranCircumstanceCodesAsString()
	{
		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Export;

		var header = declaration.Invoices.AddNew();
		AssertNoNotifications(header.AdditionalTranCircumstanceCodesAsStringInfo);

		var additionalTranCircumstanceCode1 = header.AdditionalTranCircumstanceCodes.AddNew();
		AssertNoNotifications(header.AdditionalTranCircumstanceCodesAsStringInfo);

		additionalTranCircumstanceCode1.CY_Code = TranCircumstancesList.Codes.A00PL;
		AssertNoNotifications(header.AdditionalTranCircumstanceCodesAsStringInfo);

		var additionalTranCircumstanceCode2 = header.AdditionalTranCircumstanceCodes.AddNew();
		additionalTranCircumstanceCode2.CY_Code = TranCircumstancesList.Codes.A00PL;
		AssertHasMessageError(additionalTranCircumstanceCode2.CY_CodeInfo, "Duplicate transaction circumstance found.");
		header.Validation.ValidateAdditionalTranCircumstanceCodesAsString();
		AssertHasMessageError(header.AdditionalTranCircumstanceCodesAsStringInfo, "Duplicate transaction circumstance found.");
	}

	public void TestCheckJZ_ValuationCode_InvalidCodeValidation()
	{
		var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
		const string codeType = UniversalReferenceConstants.RefCusCodeListType.Codes.InvoiceHeaderValuationCodes;
		const string country = Core.Constants.CountryCodes.Poland;
		helper.CreateNewOrGetExistingDataGrouping(country, "Poland");
		helper.CreateNewOrGetExistingCusCodeType(codeType, "Invoice Header Valuation Codes");
		helper.CreateNewOrGetExistingCusCodeList(country, codeType, "11", "11 Desc", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingCusCodeList(country, codeType, "12", "12 Desc", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

		Factory.Save();

		var declaration = Factory.New<JobDeclaration>();
		var invoiceHeader = declaration.Invoices.AddNew();
		ValidationTestHelper.AssertInvalidCodeMessageError(invoiceHeader.JZ_ValuationCodeInfo, new ZString[] { "!@" }, new ZString[] { "11", "12" });
	}

	public void TestCheckJZ_OA_SupplierAddress()
	{
		const string errorMessage = "Enter a valid Supplier Address";

		var org = Factory.New<OrgHeader>();
		invoiceHeader.SupplierOrgPK = org.PK;
		CombineAssertions(() =>
		{
			invoiceHeader.JZ_OA_SupplierAddress = ZGuid.Empty;
			AssertHasMessageError("Supplier Address is empty", invoiceHeader.JZ_OA_SupplierAddressInfo, errorMessage);

			invoiceHeader.JZ_OA_SupplierAddress = org.MainAddress.PK;
			AssertNoMessageError("Valid Supplier Address", invoiceHeader.JZ_OA_SupplierAddressInfo, errorMessage);
		});
	}

	public void TestCheckJZ_OA_BuyerAddress()
	{
		const string errorMessage = "Enter a valid Buyer Address";

		var org = Factory.New<OrgHeader>();
		invoiceHeader.BuyerOrgPK = org.PK;
		CombineAssertions(() =>
		{
			invoiceHeader.JZ_OA_BuyerAddress = ZGuid.Empty;
			AssertHasMessageError("Buyer Address is empty", invoiceHeader.JZ_OA_BuyerAddressInfo, errorMessage);

			invoiceHeader.JZ_OA_BuyerAddress = org.MainAddress.PK;
			AssertNoMessageError("Valid Buyer Address", invoiceHeader.JZ_OA_BuyerAddressInfo, errorMessage);
		});
	}

	public void TestCheckJZ_OA_ExporterAddress()
	{
		const string errorMessage = "Enter a valid Exporter Address";

		var org = Factory.New<OrgHeader>();
		invoiceHeader.ExporterOrgPK = org.PK;
		CombineAssertions(() =>
		{
			invoiceHeader.JZ_OA_ExporterAddress = ZGuid.Empty;
			AssertHasMessageError("Exporter Address is empty", invoiceHeader.JZ_OA_ExporterAddressInfo, errorMessage);

			invoiceHeader.JZ_OA_ExporterAddress = org.MainAddress.PK;
			AssertNoMessageError("Valid Exporter Address", invoiceHeader.JZ_OA_ExporterAddressInfo, errorMessage);
		});
	}

	public void TestCheckJZ_OA_SellerAddress()
	{
		const string errorMessage = "Enter a valid Seller Address";

		var org = Factory.New<OrgHeader>();
		invoiceHeader.SellerOrgPK = org.PK;
		CombineAssertions(() =>
		{
			invoiceHeader.JZ_OA_SellerAddress = ZGuid.Empty;
			AssertHasMessageError("Seller Address is empty", invoiceHeader.JZ_OA_SellerAddressInfo, errorMessage);

			invoiceHeader.JZ_OA_SellerAddress = org.MainAddress.PK;
			AssertNoMessageError("Valid Seller Address", invoiceHeader.JZ_OA_SellerAddressInfo, errorMessage);
		});
	}

	public void TestCheckJZ_OA_ConsigneeAddress()
	{
		const string errorMessage = "Enter a valid Consignee Address";

		var org = Factory.New<OrgHeader>();
		invoiceHeader.ConsigneeOrgPK = org.PK;
		CombineAssertions(() =>
		{
			invoiceHeader.JZ_OA_ConsigneeAddress = ZGuid.Empty;
			AssertHasMessageError("Consignee Address is empty", invoiceHeader.JZ_OA_ConsigneeAddressInfo, errorMessage);

			invoiceHeader.JZ_OA_ConsigneeAddress = org.MainAddress.PK;
			AssertNoMessageError("Valid Consignee Address", invoiceHeader.JZ_OA_ConsigneeAddressInfo, errorMessage);
		});
	}

	public void TestCheckJZ_OA_InvoicerAddress()
	{
		const string errorMessage = "Enter a valid Invoicer Address";

		var org = Factory.New<OrgHeader>();
		((JobComInvoiceHeader)invoiceHeader).InvoicerOrgPK = org.PK;
		CombineAssertions(() =>
		{
			invoiceHeader.JZ_OA_InvoicerAddress = ZGuid.Empty;
			AssertHasMessageError("Invoicer Address is empty", invoiceHeader.JZ_OA_InvoicerAddressInfo, errorMessage);

			invoiceHeader.JZ_OA_InvoicerAddress = org.MainAddress.PK;
			AssertNoMessageError("Valid Invoicer Address", invoiceHeader.JZ_OA_InvoicerAddressInfo, errorMessage);
		});
	}

	public void TestCheckJZ_OA_ManufacturerAddress()
	{
		const string errorMessage = "Enter a valid Manufacturer Address";

		var org = Factory.New<OrgHeader>();
		invoiceHeader.ManufacturerOrgPK = org.PK;
		CombineAssertions(() =>
		{
			invoiceHeader.JZ_OA_ManufacturerAddress = ZGuid.Empty;
			AssertHasMessageError("Manufacturer Address is empty", invoiceHeader.JZ_OA_ManufacturerAddressInfo, errorMessage);

			invoiceHeader.JZ_OA_ManufacturerAddress = org.MainAddress.PK;
			AssertNoMessageError("Valid Manufacturer Address", invoiceHeader.JZ_OA_ManufacturerAddressInfo, errorMessage);
		});
	}

	public void TestCheckJZ_IncoTerm()
	{
		JobDeclaration declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
		var invoiceHeader = declaration.Invoices.AddNew();
		invoiceHeader.Validation.ValidateJZ_IncoTerm();
		AssertHasNotifications("Export declaration", invoiceHeader.JZ_IncoTermInfo);

		declaration.JE_MessageType = PLJobMessageTypeList.Codes.ExitSummary;
		invoiceHeader.Validation.ValidateJZ_IncoTerm();
		AssertNoNotifications("ExitSummaryDeclaration declaration", invoiceHeader.JZ_IncoTermInfo);
	}

	public void TestCheckJZ_ValuationCode()
	{
		JobDeclaration declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
		var invoiceHeader = declaration.Invoices.AddNew();
		invoiceHeader.Validation.ValidateJZ_ValuationCode();
		AssertHasNotifications("Export declaration", invoiceHeader.JZ_ValuationCodeInfo);

		declaration.JE_MessageType = PLJobMessageTypeList.Codes.ExitSummary;
		invoiceHeader.Validation.ValidateJZ_ValuationCode();
		AssertNoNotifications("ExitSummaryDeclaration declaration", invoiceHeader.JZ_ValuationCodeInfo);
	}

	public void TestCheckJZ_NetWeight()
	{
		JobDeclaration declaration = Factory.New<JobDeclaration>();
		var invoiceHeader = declaration.Invoices.AddNew();

		declaration.JE_MessageType = PLJobMessageTypeList.Codes.ExitSummary;
		invoiceHeader.Validation.ValidateJZ_NetWeight();
		AssertNoNotifications("ExitSummaryDeclaration declaration", invoiceHeader.JZ_NetWeightInfo);
	}

	public void TestCheckJZ_NetWeightUQ()
	{
		JobDeclaration declaration = Factory.New<JobDeclaration>();
		var invoiceHeader = declaration.Invoices.AddNew();

		declaration.JE_MessageType = PLJobMessageTypeList.Codes.ExitSummary;
		invoiceHeader.Validation.ValidateJZ_NetWeightUQ();
		AssertNoNotifications("ExitSummaryDeclaration declaration", invoiceHeader.JZ_NetWeightUQInfo);
	}

	public void TestCheckRuleR2010()
	{
		const string messageError = "[R2010] The Exchange Rate for retrospective declaration (Sub Style = 'R') is required";

		JobDeclaration declaration = Factory.New<JobDeclaration>();
		JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
		JobComInvoiceLine invoiceLine = invoice.InvoiceLines.AddNew();
		CusEntryInstruction entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		invoiceLine.JI_CEI = entryInstruction.PK;

		CombineAssertions(() =>
		{
			entryInstruction.CEI_SubStyle = SubStyleCodes.R;
			invoice.JZ_InvoiceCurrExRate = 0;
			invoice.Validation.ValidateJZ_InvoiceCurrExRate();
			AssertHasMessageError("CEI_SubStyle is R and exchange rate is empty", invoice.JZ_InvoiceCurrExRateInfo,  messageError);

			invoice.JZ_InvoiceCurrExRate = 1.23;
			invoice.Validation.ValidateJZ_InvoiceCurrExRate();
			AssertNoMessageError("CEI_SubStyle is R and exchange rate is provided", invoice.JZ_InvoiceCurrExRateInfo, messageError);

			entryInstruction.CEI_SubStyle = SubStyleCodes.A;
			invoice.JZ_InvoiceCurrExRate = 0;
			invoice.Validation.ValidateJZ_InvoiceCurrExRate();
			AssertNoMessageError("CEI_SubStyle is not R and exchange rate is empty", invoice.JZ_InvoiceCurrExRateInfo, messageError);
		});
	}

	protected override Type GetTypeForTest() => typeof(JobComInvoiceHeaderValidation);

	protected override BaseJobComInvoiceHeader GetInvoiceHeader()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.MiscellaneousCustoms;
		var invoice = declaration.Invoices.AddNew();
		return invoice;
	}
}
