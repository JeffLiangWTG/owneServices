using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.NZ.Business.Declaration.Testing
{
	using CargoWise.Types;
	using Enterprise.MasterFiles.Business;
	using NUnit.Framework;

	public class JobComInvoiceHeaderValidationFormalEntryTest : Customs.Business.Testing.InvoiceHeaderValidationTest
	{
		public void TestExchangeRateIndicator_OneForeignCurrency_OnlyOneAllowed()
		{
			var currencyAU = RefCurrency.LoadFromCurrencyCode(Factory, Core.Constants.CurrencyCodes.Australia);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;

			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = currencyAU.RX_Code;
			invoiceHeader.JZ_ExchangeRateIndicator = ExchangeRateIndicatorList.Codes.Floating;

			var invoiceHeader2 = declaration.Invoices.AddNew();
			invoiceHeader2.JZ_RX_NKInvoice_Currency = currencyAU.RX_Code;
			invoiceHeader2.JZ_ExchangeRateIndicator = ExchangeRateIndicatorList.Codes.ForwardCover;

			AssertHasError(invoiceHeader2.JZ_ExchangeRateIndicatorInfo, JobComInvoiceHeaderValidationFormalEntry.ErrorExchangeRateIndicatorMustBeSameForSameCurrency + currencyAU.RX_Code);
		}

		public void TestExchangeRateIndicator()
		{
			RefCurrency currencyNZ = RefCurrency.LoadFromCurrencyCode(Factory, Enterprise.Core.Constants.CurrencyCodes.NewZealand);
			RefCurrency currencyAU = RefCurrency.LoadFromCurrencyCode(Factory, Enterprise.Core.Constants.CurrencyCodes.Australia);

			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = currencyNZ.RX_Code;
			invoiceHeader.JZ_ExchangeRateIndicator = "";
			AssertHasMessageError(invoiceHeader.JZ_ExchangeRateIndicatorInfo, JobComInvoiceHeaderValidationFormalEntry.MessageErrorExchangeRateIndicatorMustBeNZD);
			AssertNoMessageError(invoiceHeader.JZ_ExchangeRateIndicatorInfo, JobComInvoiceHeaderValidationFormalEntry.MessageErrorExchangeRateIndicatorMustNotBeNZD);
			AssertNoMessageError(invoiceHeader.JZ_ExchangeRateIndicatorInfo, JobComInvoiceHeaderValidationFormalEntry.MessageErrorMustHaveCurrencyAndExchangeRateIndicator);

			invoiceHeader.JZ_RX_NKInvoice_Currency = currencyAU.RX_Code;
			AssertNoMessageError(invoiceHeader.JZ_ExchangeRateIndicatorInfo, JobComInvoiceHeaderValidationFormalEntry.MessageErrorExchangeRateIndicatorMustBeNZD);
			AssertHasMessageError(invoiceHeader.JZ_ExchangeRateIndicatorInfo, JobComInvoiceHeaderValidationFormalEntry.MessageErrorExchangeRateIndicatorMustNotBeNZD);
			AssertNoMessageError(invoiceHeader.JZ_ExchangeRateIndicatorInfo, JobComInvoiceHeaderValidationFormalEntry.MessageErrorMustHaveCurrencyAndExchangeRateIndicator);

			invoiceHeader.JZ_ExchangeRateIndicator = ExchangeRateIndicatorList.Codes.Floating;
			AssertNoMessageError(invoiceHeader.JZ_ExchangeRateIndicatorInfo, JobComInvoiceHeaderValidationFormalEntry.MessageErrorExchangeRateIndicatorMustBeNZD);
			AssertNoMessageError(invoiceHeader.JZ_ExchangeRateIndicatorInfo, JobComInvoiceHeaderValidationFormalEntry.MessageErrorExchangeRateIndicatorMustNotBeNZD);
			AssertNoMessageError(invoiceHeader.JZ_ExchangeRateIndicatorInfo, JobComInvoiceHeaderValidationFormalEntry.MessageErrorMustHaveCurrencyAndExchangeRateIndicator);

			invoiceHeader.JZ_ExchangeRateIndicator = ExchangeRateIndicatorList.Codes.ForwardCover;
			AssertNoMessageError(invoiceHeader.JZ_ExchangeRateIndicatorInfo, JobComInvoiceHeaderValidationFormalEntry.MessageErrorExchangeRateIndicatorMustBeNZD);
			AssertNoMessageError(invoiceHeader.JZ_ExchangeRateIndicatorInfo, JobComInvoiceHeaderValidationFormalEntry.MessageErrorExchangeRateIndicatorMustNotBeNZD);
			AssertNoMessageError(invoiceHeader.JZ_ExchangeRateIndicatorInfo, JobComInvoiceHeaderValidationFormalEntry.MessageErrorMustHaveCurrencyAndExchangeRateIndicator);

			invoiceHeader.JZ_ExchangeRateIndicator = ExchangeRateIndicatorList.Codes.NZD;
			AssertNoMessageError(invoiceHeader.JZ_ExchangeRateIndicatorInfo, JobComInvoiceHeaderValidationFormalEntry.MessageErrorExchangeRateIndicatorMustBeNZD);
			AssertHasMessageError(invoiceHeader.JZ_ExchangeRateIndicatorInfo, JobComInvoiceHeaderValidationFormalEntry.MessageErrorExchangeRateIndicatorMustNotBeNZD);
			AssertNoMessageError(invoiceHeader.JZ_ExchangeRateIndicatorInfo, JobComInvoiceHeaderValidationFormalEntry.MessageErrorMustHaveCurrencyAndExchangeRateIndicator);

			invoiceHeader.JZ_RX_NKInvoice_Currency = ZString.Empty;
			AssertNoMessageError(invoiceHeader.JZ_ExchangeRateIndicatorInfo, JobComInvoiceHeaderValidationFormalEntry.MessageErrorExchangeRateIndicatorMustBeNZD);
			AssertNoMessageError(invoiceHeader.JZ_ExchangeRateIndicatorInfo, JobComInvoiceHeaderValidationFormalEntry.MessageErrorExchangeRateIndicatorMustNotBeNZD);
			AssertHasMessageError(invoiceHeader.JZ_ExchangeRateIndicatorInfo, JobComInvoiceHeaderValidationFormalEntry.MessageErrorMustHaveCurrencyAndExchangeRateIndicator);

			invoiceHeader.JZ_RX_NKInvoice_Currency = currencyNZ.RX_Code;
			AssertNoMessageError(invoiceHeader.JZ_ExchangeRateIndicatorInfo, JobComInvoiceHeaderValidationFormalEntry.MessageErrorExchangeRateIndicatorMustBeNZD);
			AssertNoMessageError(invoiceHeader.JZ_ExchangeRateIndicatorInfo, JobComInvoiceHeaderValidationFormalEntry.MessageErrorExchangeRateIndicatorMustNotBeNZD);
			AssertNoMessageError(invoiceHeader.JZ_ExchangeRateIndicatorInfo, JobComInvoiceHeaderValidationFormalEntry.MessageErrorMustHaveCurrencyAndExchangeRateIndicator);

			invoiceHeader.JZ_ExchangeRateIndicator = ExchangeRateIndicatorList.Codes.Floating;
			AssertHasMessageError(invoiceHeader.JZ_ExchangeRateIndicatorInfo, JobComInvoiceHeaderValidationFormalEntry.MessageErrorExchangeRateIndicatorMustBeNZD);
			AssertNoMessageError(invoiceHeader.JZ_ExchangeRateIndicatorInfo, JobComInvoiceHeaderValidationFormalEntry.MessageErrorExchangeRateIndicatorMustNotBeNZD);
			AssertNoMessageError(invoiceHeader.JZ_ExchangeRateIndicatorInfo, JobComInvoiceHeaderValidationFormalEntry.MessageErrorMustHaveCurrencyAndExchangeRateIndicator);

			invoiceHeader.JZ_ExchangeRateIndicator = ExchangeRateIndicatorList.Codes.ForwardCover;
			AssertHasMessageError(invoiceHeader.JZ_ExchangeRateIndicatorInfo, JobComInvoiceHeaderValidationFormalEntry.MessageErrorExchangeRateIndicatorMustBeNZD);
			AssertNoMessageError(invoiceHeader.JZ_ExchangeRateIndicatorInfo, JobComInvoiceHeaderValidationFormalEntry.MessageErrorExchangeRateIndicatorMustNotBeNZD);
			AssertNoMessageError(invoiceHeader.JZ_ExchangeRateIndicatorInfo, JobComInvoiceHeaderValidationFormalEntry.MessageErrorMustHaveCurrencyAndExchangeRateIndicator);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			invoiceHeader.JZ_ExchangeRateIndicator = "";
			AssertNoMessageErrors(invoiceHeader.JZ_ExchangeRateIndicatorInfo);
		}

		public void TestSupplierNeedsSupplierCodeForNormalImportEntries()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			OrgHeader supplier = OrgHeader.New(Factory);
			invoiceHeader.JZ_OH_Supplier = supplier.PK;
			AssertHasMessageError(invoiceHeader.JZ_OH_SupplierInfo, JobComInvoiceHeaderValidationFormalEntry.MissingCustomsSupplierCode);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			invoiceHeader.Validation.ValidateJZ_OH_Supplier();
			AssertNoMessageError(invoiceHeader.JZ_OH_SupplierInfo, JobComInvoiceHeaderValidationFormalEntry.MissingCustomsSupplierCode);

			OrgCusCode supplierCode = supplier.CustomsCodes.AddNew();
			supplierCode.OK_CodeType = OrgCusCode.CodeTypes.SupplierCode;
			supplierCode.OK_CountryDefault = true;
			supplierCode.OK_CustomsRegNo = "00970890N";
			supplierCode.OK_RN_NKCodeCountry = "NZ";
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			invoiceHeader.Validation.ValidateJZ_OH_Supplier();
			AssertNoMessageError(invoiceHeader.JZ_OH_SupplierInfo, JobComInvoiceHeaderValidationFormalEntry.MissingCustomsSupplierCode);
		}

		public void TestSupplierDoesNotNeedSupplierCodeForSimplifiedImportEntries()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			OrgHeader supplier = OrgHeader.New(Factory);
			invoiceHeader.JZ_OH_Supplier = supplier.PK;
			AssertHasMessageError(invoiceHeader.JZ_OH_SupplierInfo, JobComInvoiceHeaderValidationFormalEntry.MissingCustomsSupplierCode);

			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Simplified;
			invoiceHeader.Validation.ValidateJZ_OH_Supplier();
			AssertNoMessageError(invoiceHeader.JZ_OH_SupplierInfo, JobComInvoiceHeaderValidationFormalEntry.MissingCustomsSupplierCode);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			invoiceHeader.Validation.ValidateJZ_OH_Supplier();
			AssertNoMessageError(invoiceHeader.JZ_OH_SupplierInfo, JobComInvoiceHeaderValidationFormalEntry.MissingCustomsSupplierCode);

			OrgCusCode supplierCode = supplier.CustomsCodes.AddNew();
			supplierCode.OK_CodeType = OrgCusCode.CodeTypes.SupplierCode;
			supplierCode.OK_CountryDefault = true;
			supplierCode.OK_CustomsRegNo = "00970890N";
			supplierCode.OK_RN_NKCodeCountry = "NZ";

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			invoiceHeader.Validation.ValidateJZ_OH_Supplier();
			AssertNoMessageError(invoiceHeader.JZ_OH_SupplierInfo, JobComInvoiceHeaderValidationFormalEntry.MissingCustomsSupplierCode);
		}

		public void TestSupplierDoesNotNeedSupplierCodeForIPIImportEntries()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			OrgHeader supplier = OrgHeader.New(Factory);
			invoiceHeader.JZ_OH_Supplier = supplier.PK;
			AssertHasMessageError(invoiceHeader.JZ_OH_SupplierInfo, JobComInvoiceHeaderValidationFormalEntry.MissingCustomsSupplierCode);

			declaration.JE_MessageSubType = MessageSubTypeCombinedList.Codes.IPI;
			invoiceHeader.Validation.ValidateJZ_OH_Supplier();
			AssertNoMessageError(invoiceHeader.JZ_OH_SupplierInfo, JobComInvoiceHeaderValidationFormalEntry.MissingCustomsSupplierCode);
		}

		public void TestImporterNotRequiredAtAllForExportEntries()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			invoiceHeader.Validation.ValidateJZ_OH_Supplier();
			AssertNoMessageErrors(invoiceHeader.JZ_OH_SupplierInfo);

			OrgHeader supplier = OrgHeader.New(Factory);
			invoiceHeader.JZ_OH_Supplier = supplier.PK;
			AssertNoMessageErrors(invoiceHeader.JZ_OH_SupplierInfo);
		}

		public void TestValidateCountryOfOriginForInvoiceHeader()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			invoiceHeader.JZ_RN_NKDefaultOrigin = ZString.Empty;
			AssertNoMessageErrors(invoiceHeader.JZ_RN_NKDefaultOriginInfo);
			invoiceHeader.JZ_RN_NKDefaultOrigin = "ZZ";
			AssertHasMessageError(invoiceHeader.JZ_RN_NKDefaultOriginInfo, JobComInvoiceHeaderValidationFormalEntry.MessageErrorRemoveOrFixDefaultCountryOfOrigin);
			invoiceHeader.JZ_RN_NKDefaultOrigin = "AU";
			AssertNoMessageErrors(invoiceHeader.JZ_RN_NKDefaultOriginInfo);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			invoiceHeader.JZ_RN_NKDefaultOrigin = ZString.Empty;
			AssertNoMessageErrors(invoiceHeader.JZ_RN_NKDefaultOriginInfo);
			invoiceHeader.JZ_RN_NKDefaultOrigin = "ZZ";
			AssertNoMessageErrors(invoiceHeader.JZ_RN_NKDefaultOriginInfo);
			invoiceHeader.JZ_RN_NKDefaultOrigin = "AU";
			AssertNoMessageErrors(invoiceHeader.JZ_RN_NKDefaultOriginInfo);
		}

		public void TestValidateSupplierAndDeclarationSupplierMustMatch()
		{
			declaration.JE_OH_Supplier = Factory.LoadTop1<OrgHeader>(new ZQuery()).PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			invoiceHeader.JZ_OH_Supplier = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.PK, SQLComparisonOperator.NotEqual, declaration.JE_OH_Supplier)).PK;
			AssertHasMessageError(invoiceHeader.JZ_OH_SupplierInfo, JobComInvoiceHeaderValidationFormalEntry.InvoiceSupplierAndDeclarationSupplierMustMatch);

			invoiceHeader.JZ_OH_Supplier = declaration.JE_OH_Supplier;
			AssertNoMessageError(invoiceHeader.JZ_OH_SupplierInfo, JobComInvoiceHeaderValidationFormalEntry.InvoiceSupplierAndDeclarationSupplierMustMatch);
		}

		public void TestValidateMustHaveSupplierForImportJob()
		{
			invoiceHeader.JZ_OH_Supplier = ZGuid.Empty;

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			invoiceHeader.Validation.ValidateJZ_OH_Supplier();
			AssertHasMessageError(invoiceHeader.JZ_OH_SupplierInfo, JobComInvoiceHeaderValidationFormalEntry.MustHaveSupplierForImportJob);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			invoiceHeader.Validation.ValidateJZ_OH_Supplier();
			AssertNoMessageError(invoiceHeader.JZ_OH_SupplierInfo, JobComInvoiceHeaderValidationFormalEntry.MustHaveSupplierForImportJob);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			invoiceHeader.JZ_OH_Supplier = OrgHeader.New(Factory).PK;
			AssertNoMessageError(invoiceHeader.JZ_OH_SupplierInfo, JobComInvoiceHeaderValidationFormalEntry.MustHaveSupplierForImportJob);
		}

		public void TestJZ_InvoiceDate()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			invoiceHeader.Validation.ValidateJZ_InvoiceDate();
			AssertNoMessageErrors(invoiceHeader.JZ_InvoiceDateInfo);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			invoiceHeader.JZ_InvoiceDate = ZDateTime.Empty;
			AssertHasMessageErrors(invoiceHeader.JZ_InvoiceDateInfo);

			invoiceHeader.JZ_InvoiceDate = ZDateTime.Today.AddDays(-14);
			AssertNoMessageErrors(invoiceHeader.JZ_InvoiceDateInfo);
		}

		public void TestValidateInvoiceImporterAndDeclarationImporterMustMatch()
		{
			declaration.JE_OH_Importer = Factory.LoadTop1<OrgHeader>(new ZQuery()).PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			invoiceHeader.JZ_OH_Buyer = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.PK, SQLComparisonOperator.NotEqual, declaration.JE_OH_Importer)).PK;
			AssertHasMessageError(invoiceHeader.JZ_OH_BuyerInfo, JobComInvoiceHeaderValidationFormalEntry.InvoiceImporterAndDeclarationImporterMustMatch);

			invoiceHeader.JZ_OH_Buyer = declaration.JE_OH_Importer;
			AssertNoMessageError(invoiceHeader.JZ_OH_BuyerInfo, JobComInvoiceHeaderValidationFormalEntry.InvoiceImporterAndDeclarationImporterMustMatch);
		}

		public void TestImporterIsMandatoryForTSWExportDec()
		{
			declaration.JE_OH_Importer = Factory.LoadTop1<OrgHeader>(new ZQuery()).PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			invoiceHeader.JZ_OH_Buyer = declaration.JE_OH_Importer;
			AssertNoMessageErrors(invoiceHeader.JZ_OH_BuyerInfo);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			invoiceHeader.JZ_OH_Buyer = ZGuid.Empty;
			AssertHasMessageErrors(invoiceHeader.JZ_OH_BuyerInfo);

			invoiceHeader.JZ_OH_Buyer = declaration.JE_OH_Importer;
			AssertNoMessageErrors(invoiceHeader.JZ_OH_BuyerInfo);
		}

		public void TestValidateExchangeRateIndicator()
		{
			RefCurrency currencyNZ = RefCurrency.LoadFromCurrencyCode(Factory, Enterprise.Core.Constants.CurrencyCodes.NewZealand);
			RefCurrency currencyAU = RefCurrency.LoadFromCurrencyCode(Factory, Enterprise.Core.Constants.CurrencyCodes.Australia);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			invoiceHeader.JZ_RX_NKInvoice_Currency = currencyNZ.RX_Code;

			invoiceHeader.JZ_ExchangeRateIndicator = ZString.Empty;
			AssertHasMessageErrorContaining(invoiceHeader.JZ_ExchangeRateIndicatorInfo, JobComInvoiceHeaderValidationFormalEntry.MessageErrorExchangeRateIndicatorMustBeNZD);

			invoiceHeader.JZ_ExchangeRateIndicator = "xxx";
			AssertHasMessageError(invoiceHeader.JZ_ExchangeRateIndicatorInfo, JobComInvoiceHeaderValidationFormalEntry.MessageErrorExchangeRateIndicatorMustBeNZD);

			invoiceHeader.JZ_ExchangeRateIndicator = ExchangeRateIndicatorList.Codes.NZD;
			AssertNoMessageErrors(invoiceHeader.JZ_ExchangeRateIndicatorInfo);

			invoiceHeader.JZ_RX_NKInvoice_Currency = currencyAU.RX_Code;
			AssertHasMessageError(invoiceHeader.JZ_ExchangeRateIndicatorInfo, JobComInvoiceHeaderValidationFormalEntry.MessageErrorExchangeRateIndicatorMustNotBeNZD);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			invoiceHeader.JZ_ExchangeRateIndicator = ZString.Empty;
			AssertNoMessageErrors(invoiceHeader.JZ_ExchangeRateIndicatorInfo);

			invoiceHeader.JZ_ExchangeRateIndicator = "xxx";
			AssertNoMessageErrors(invoiceHeader.JZ_ExchangeRateIndicatorInfo);

			invoiceHeader.JZ_ExchangeRateIndicator = ExchangeRateIndicatorList.Codes.NZD;
			AssertNoMessageErrors(invoiceHeader.JZ_ExchangeRateIndicatorInfo);
		}

		public void TestValidateRelationshipIndicator()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			invoiceHeader.JZ_RelationshipIndicator = "";
			AssertHasMessageError(invoiceHeader.JZ_RelationshipIndicatorInfo, JobComInvoiceHeaderValidation.MessageErrorMissingRelationshipIndicator);

			invoiceHeader.JZ_RelationshipIndicator = "I";
			AssertHasMessageError(invoiceHeader.JZ_RelationshipIndicatorInfo, JobComInvoiceHeaderValidation.MessageErrorMissingRelationshipIndicator);

			invoiceHeader.JZ_RelationshipIndicator = RelationshipIndicatorList.Codes.Related;
			AssertNoMessageErrors(invoiceHeader.JZ_RelationshipIndicatorInfo);

			invoiceHeader.JZ_RelationshipIndicator = RelationshipIndicatorList.Codes.RelatedDoesNotAffectPrice;
			AssertHasMessageError(invoiceHeader.JZ_RelationshipIndicatorInfo, JobComInvoiceHeaderValidation.MessageErrorMissingRelationshipIndicator);
		}

		public void TestValidateRelationshipIndicatorForTSW()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			invoiceHeader.JZ_RelationshipIndicator = "";
			AssertHasMessageError(invoiceHeader.JZ_RelationshipIndicatorInfo, JobComInvoiceHeaderValidation.MessageErrorMissingRelationshipIndicator);

			invoiceHeader.JZ_RelationshipIndicator = "I";
			AssertHasMessageError(invoiceHeader.JZ_RelationshipIndicatorInfo, JobComInvoiceHeaderValidation.MessageErrorMissingRelationshipIndicator);

			invoiceHeader.JZ_RelationshipIndicator = RelationshipIndicatorList.Codes.Related;
			AssertNoMessageErrors(invoiceHeader.JZ_RelationshipIndicatorInfo);

			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			invoiceHeader.JZ_RelationshipIndicator = RelationshipIndicatorList.Codes.RelatedDoesNotAffectPrice;
			AssertNoMessageErrors(invoiceHeader.JZ_RelationshipIndicatorInfo);
		}

		/// <summary>
		/// I know this sounds stupid, but it was happening to a client.
		/// </summary>
		public void TestChangingPermitCodeOnDeclarationDoesntCauseValidationErrorOnJobComInvoiceHeader()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;

			invoiceHeader.JZ_InvoiceNumber = "1001001";

			PermitCode permit = declaration.PermitCodes.AddNew();
			permit.ZO_Code = "MCD";
			permit.ZO_Data = "YYYNN";
			AssertEquals(true, permit.HasMessageErrors);
			permit.ZO_Code = "";
			permit.ZO_Data = "";
			declaration.PermitCodes.RemoveAndDelete(permit);
			declaration.RunPreSaveValidation();

			AssertEquals(false, permit.HasMessageErrors);
		}

		public void TestValidationMissingExchangeRate()
		{
			invoiceHeader.JZ_InvoiceCurrExRate = 0.00m;
			AssertHasMessageError(invoiceHeader.JZ_InvoiceCurrExRateInfo, JobComInvoiceHeaderValidationFormalEntry.MissingExchangeRate);
			invoiceHeader.JZ_InvoiceCurrExRate = 1.00m;
			AssertNoMessageError(invoiceHeader.JZ_InvoiceCurrExRateInfo, JobComInvoiceHeaderValidationFormalEntry.MissingExchangeRate);
		}

		public void TestExchangeRateWhenIPIDecTypeChangesBack()
		{
			TestCaseHelper.ClearTable("RefExchangeRate");
			var aUDCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "AUD");
			var testExchangeRate1 = aUDCurrency.ExchangeRates.AddNew();
			testExchangeRate1.RE_ExRateType = "CUS";
			testExchangeRate1.RE_StartDate = new ZDateTime(2018, 5, 29);
			testExchangeRate1.RE_ExpiryDate = new ZDateTime(2018, 6, 04);
			testExchangeRate1.RE_SellRate = 1.13m;
			var testExchangeRate2 = aUDCurrency.ExchangeRates.AddNew();
			testExchangeRate2.RE_ExRateType = "CUS";
			testExchangeRate2.RE_StartDate = new ZDateTime(2018, 6, 05);
			testExchangeRate2.RE_ExpiryDate = new ZDateTime(2018, 6, 12);
			testExchangeRate2.RE_SellRate = 1.14m;
			Factory.Save();

			var ipiDeclaration = Factory.New<JobDeclaration>();
			ipiDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			ipiDeclaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			ipiDeclaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;

			var invoice = ipiDeclaration.Invoices.AddNew();
			ipiDeclaration.JE_EDITransmitDate = new ZDateTime(2018, 5, 30);
			AssertEquals("Precondition: Currency set to a local currency", JobDeclaration.LocalCurrencyConstantCode, invoice.JZ_RX_NKInvoice_Currency);
			AssertEquals("Exchange Rate set", 1m, invoice.JZ_InvoiceCurrExRate);

			invoice.JZ_RX_NKInvoice_Currency = aUDCurrency.RX_Code;
			invoice.Validation.ValidateJZ_InvoiceCurrExRate();
			AssertEquals("Exchange Rate should now reflect AUD rate", 1.13m, invoice.JZ_InvoiceCurrExRate);

			ZString originalEntryNumber;
			ChangeEntryFromNormalToIPI(ipiDeclaration, out originalEntryNumber);
			AssertEquals("JE_EDITransmitDate cleared for new IPI entry", ZDateTime.Empty, ipiDeclaration.JE_EDITransmitDate);

			//simulate IPI entry being sent
			ipiDeclaration.JE_EDITransmitDate = new ZDateTime(2018, 6, 10);
			invoice.Validation.ValidateJZ_InvoiceCurrExRate();
			AssertEquals("Exchange Rate should not however have changed for the subsequent IPI entry", 1.13m, invoice.JZ_InvoiceCurrExRate);

			// change entry back to normal
			ipiDeclaration.JE_MessageSubType = MessageSubTypeCombinedList.Codes.Normal;
			invoice.Validation.ValidateJZ_InvoiceCurrExRate();
			AssertEquals("JE_EDITransmitDate should reflect original entry", new ZDateTime(2018, 5, 30), ipiDeclaration.JE_EDITransmitDate);
			AssertEquals("Exchange Rate should not have changed", 1.13m, invoice.JZ_InvoiceCurrExRate);

			// confirm current exchange rate is different to IPI reverted to Normal declaration rate.
			var currentDeclaration = Factory.New<JobDeclaration>();
			currentDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			currentDeclaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			currentDeclaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			currentDeclaration.JE_EDITransmitDate = new ZDateTime(2018, 6, 10);
			var currentInvoice = currentDeclaration.Invoices.AddNew();
			currentInvoice.JZ_RX_NKInvoice_Currency = aUDCurrency.RX_Code;
			currentInvoice.Validation.ValidateJZ_InvoiceCurrExRate();
			AssertEquals("This entries Exchange Rate should reflect the current AUD rate for this date", 1.14m, currentInvoice.JZ_InvoiceCurrExRate);
		}

		void ChangeEntryFromNormalToIPI(JobDeclaration declaration, out ZString originalEntryNumber)
		{
			declaration.CusEntryHeader.EntryNumber = "123";
			originalEntryNumber = declaration.CusEntryHeader.EntryNumber;
			declaration.CusEntryHeader.CH_LastEntryStyle = "";

			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			declaration.JE_MessageSubType = MessageSubTypeCombinedList.Codes.IPI;
			declaration.CusEntryHeader.EntryNumber = "456";
			declaration.CusEntryHeader.CH_LastEntryStyle = JobMessageSubTypeList.Codes.Normal;

			AssertHasWarning(declaration.JE_MessageSubTypeInfo, JobDeclarationValidationTest.AddingIPIEntry);

			var ipiHeader = this.declaration.CusEntryHeader; // IPI entry is now active entry.
			ipiHeader.CH_EDITransmitDate = new ZDateTime(2018, 6, 10);
		}

		public void TestWarningWhenExchangeRateStaleNotShownForEntryChangedToIPI()
		{
			var jjjCurrency = Factory.NewWithValidTestData<RefCurrency>();
			jjjCurrency.RX_Code = "JJJ";
			jjjCurrency.RX_Desc = "Dummy Currency";

			var exchRate1 = jjjCurrency.ExchangeRates.AddNew();
			exchRate1.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.CustomsRate;
			exchRate1.RE_RX_NKExCurrency = jjjCurrency.RX_Code;
			exchRate1.RE_StartDate = ZDateTime.Today.AddDays(-1);
			exchRate1.RE_ExpiryDate = ZDateTime.Today.AddDays(-1);
			exchRate1.RE_SellRate = 1.5555m;

			var exchRate2 = jjjCurrency.ExchangeRates.AddNew();
			exchRate2.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.CustomsRate;
			exchRate2.RE_RX_NKExCurrency = jjjCurrency.RX_Code;
			exchRate2.RE_StartDate = ZDateTime.Today;
			exchRate2.RE_ExpiryDate = ZDateTime.Today;
			exchRate2.RE_SellRate = 1.6111m;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ExportDate = ZDateTime.Today.AddDays(-1);
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = "JJJ";
			invoice.JZ_ValuationDateOverride = ZDateTime.Today;
			invoice.JZ_InvoiceCurrExRate = 1.5555m;
			invoice.Validation.ValidateJZ_InvoiceCurrExRate();
			AssertHasWarning(invoice.JZ_InvoiceCurrExRateInfo, "This exchange rate was set when the currency was set. But the current rate for the valuation date is 1.611100. This happens when latest exchange rates are imported after a currency is selected on the invoice. Please perform apportionment by clicking Brokerage > Perform Apportionment. This will update this exchange rate. Please also check if customs entries have correct figures.");

			declaration.CusEntryHeader.EntryNumber = "123";
			var originalEntryNumber = declaration.CusEntryHeader.EntryNumber;
			declaration.CusEntryHeader.CH_LastEntryStyle = "";

			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			declaration.JE_MessageSubType = MessageSubTypeCombinedList.Codes.IPI;
			declaration.CusEntryHeader.EntryNumber = "456";
			declaration.CusEntryHeader.CH_LastEntryStyle = JobMessageSubTypeList.Codes.Normal;

			invoice.JZ_RX_NKInvoice_Currency = "";
			invoice.JZ_RX_NKInvoice_Currency = "JJJ";
			invoice.JZ_InvoiceCurrExRate = 1.5555m;
			invoice.Validation.ValidateJZ_InvoiceCurrExRate();
			AssertEquals("JZ_InvoiceCurrExRate", 1.5555m, invoice.JZ_InvoiceCurrExRate);
			AssertNoWarning("Exchange rate should not have been updated for formal declaration changing to an IPI entry.", invoice.JZ_InvoiceCurrExRateInfo, "This exchange rate was set when the currency was set. But the current rate for the valuation date is 1.611100. This happens when latest exchange rates are imported after a currency is selected on the invoice. Please perform apportionment by clicking Brokerage > Perform Apportionment. This will update this exchange rate. Please also check if customs entries have correct figures.");
		}

		public void TestValidateJZ_CU_RelatedHouseBill()
		{
			var houseBill = invoiceHeader.JobDeclaration.Bills.AddNew();
			invoiceHeader.Validation.ValidateJZ_CU_RelatedHouseBill();

			var entry1 = invoiceHeader.JobDeclaration.ActiveEntryHeaders.AddNew();
			var entry2 = invoiceHeader.JobDeclaration.ActiveEntryHeaders.AddNew();
			invoiceHeader.Validation.ValidateJZ_CU_RelatedHouseBill();
			Assert(!invoiceHeader.JZ_CU_RelatedHouseBillInfo.HasMessageErrors());
		}

		#region Implementation
		JobDeclaration declaration;
		JobComInvoiceHeader invoiceHeader;
		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.CUS;
			invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
		}
		#endregion
	}
}
