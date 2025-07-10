using System;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Customs.Universal.Constants;
using static Enterprise.Customs.ZA.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.ZA.Business.Testing
{
	sealed class JobComInvoiceHeaderValidationTest : Customs.Business.Testing.JobComInvoiceHeaderValidationTest
	{
		public void TestValidateEntryInstructionDetails()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction1 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			instruction1.CEI_Style = "A";
			instruction1.CEI_Description = "Aa";
			var instruction2 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			instruction2.CEI_Style = "B";
			instruction2.CEI_Description = "Bb";
			instruction2.CEI_ExchangeRateDate = new ZDateTime(2017, 12, 15);
			var instruction3 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			instruction3.CEI_Style = "C";
			instruction3.CEI_Description = "Cc";
			instruction3.CEI_ExchangeRateDate = new ZDateTime(2017, 12, 2);
			var instruction4 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			instruction4.CEI_Style = "D";
			instruction4.CEI_Description = "Dd";
			instruction4.CEI_ExchangeRateDate = new ZDateTime(2017, 12, 2);
			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_InvoiceNumber = "INV001";
			var line1_1 = invoice1.InvoiceLines.AddNew();
			line1_1.JI_CEI = instruction2.PK;
			var line1_2 = invoice1.InvoiceLines.AddNew();
			line1_2.JI_CEI = instruction2.PK;
			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_InvoiceNumber = "INV002";
			var line2_1 = invoice2.InvoiceLines.AddNew();
			var line2_2 = invoice2.InvoiceLines.AddNew();
			line2_1.JI_CEI = instruction1.PK;
			line2_2.JI_CEI = instruction2.PK;
			var invoice3 = declaration.Invoices.AddNew();
			invoice3.JZ_InvoiceNumber = "INV003";
			var line3_1 = invoice3.InvoiceLines.AddNew();
			var line3_2 = invoice3.InvoiceLines.AddNew();
			var line3_3 = invoice3.InvoiceLines.AddNew();
			line3_1.JI_CEI = instruction1.PK;
			line3_2.JI_CEI = instruction2.PK;
			line3_3.JI_CEI = instruction3.PK;
			var invoice4 = declaration.Invoices.AddNew();
			invoice4.JZ_InvoiceNumber = "INV004";
			var line4_1 = invoice4.InvoiceLines.AddNew();
			var line4_2 = invoice4.InvoiceLines.AddNew();
			line4_1.JI_CEI = instruction3.PK;
			line4_2.JI_CEI = instruction4.PK;
			CombineAssertions("Check When it's ZAR Invoice", () =>
			{
				invoice1.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.SouthAfrica;
				invoice2.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.SouthAfrica;
				invoice3.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.SouthAfrica;
				invoice4.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.SouthAfrica;
				declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
				invoice1.RunPreSaveValidation();
				invoice2.RunPreSaveValidation();
				invoice3.RunPreSaveValidation();
				invoice4.RunPreSaveValidation();
				Assert("Invoice1:", !invoice1.RowMessageErrors.Contains(ValidationConstants.InvoiceHeader.SingleInvoiceToEntryForExportsHasMultipleExchangeRateDate));
				Assert("Invoice2:", !invoice2.RowMessageErrors.Contains(ValidationConstants.InvoiceHeader.SingleInvoiceToEntryForExportsHasMultipleExchangeRateDate));
				Assert("Invoice3:", !invoice3.RowMessageErrors.Contains(ValidationConstants.InvoiceHeader.SingleInvoiceToEntryForExportsHasMultipleExchangeRateDate));
				Assert("Invoice4:", !invoice4.RowMessageErrors.Contains(ValidationConstants.InvoiceHeader.SingleInvoiceToEntryForExportsHasMultipleExchangeRateDate));
				Assert("Invoice1:", !invoice1.RowMessageErrors.Contains(ValidationConstants.InvoiceHeader.SingleForeignCurrencyInvoiceToEntryForExports));
				Assert("Invoice2:", !invoice2.RowMessageErrors.Contains(ValidationConstants.InvoiceHeader.SingleForeignCurrencyInvoiceToEntryForExports));
				Assert("Invoice3:", !invoice3.RowMessageErrors.Contains(ValidationConstants.InvoiceHeader.SingleForeignCurrencyInvoiceToEntryForExports));
				Assert("Invoice4:", !invoice4.RowMessageErrors.Contains(ValidationConstants.InvoiceHeader.SingleForeignCurrencyInvoiceToEntryForExports));
				declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
				invoice1.RunPreSaveValidation();
				invoice2.RunPreSaveValidation();
				invoice3.RunPreSaveValidation();
				invoice4.RunPreSaveValidation();
				Assert("Invoice1:", !invoice1.RowMessageErrors.Contains(ValidationConstants.InvoiceHeader.SingleInvoiceToEntryForExportsHasMultipleExchangeRateDate));
				Assert("Invoice2:", invoice2.RowMessageErrors.Contains(ValidationConstants.InvoiceHeader.SingleInvoiceToEntryForExportsHasMultipleExchangeRateDate));
				Assert("Invoice3:", invoice3.RowMessageErrors.Contains(ValidationConstants.InvoiceHeader.SingleInvoiceToEntryForExportsHasMultipleExchangeRateDate));
				Assert("Invoice4:", !invoice4.RowMessageErrors.Contains(ValidationConstants.InvoiceHeader.SingleInvoiceToEntryForExportsHasMultipleExchangeRateDate));
				Assert("Invoice1:", !invoice1.RowMessageErrors.Contains(ValidationConstants.InvoiceHeader.SingleForeignCurrencyInvoiceToEntryForExports));
				Assert("Invoice2:", !invoice2.RowMessageErrors.Contains(ValidationConstants.InvoiceHeader.SingleForeignCurrencyInvoiceToEntryForExports));
				Assert("Invoice3:", !invoice3.RowMessageErrors.Contains(ValidationConstants.InvoiceHeader.SingleForeignCurrencyInvoiceToEntryForExports));
				Assert("Invoice4:", !invoice4.RowMessageErrors.Contains(ValidationConstants.InvoiceHeader.SingleForeignCurrencyInvoiceToEntryForExports));
			});
			CombineAssertions("Check When it's not ZAR Invoice", () =>
			{
				invoice1.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
				invoice2.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
				invoice3.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
				invoice4.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
				declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
				invoice1.RunPreSaveValidation();
				invoice2.RunPreSaveValidation();
				invoice3.RunPreSaveValidation();
				invoice4.RunPreSaveValidation();
				Assert("Invoice1:", !invoice1.RowMessageErrors.Contains(ValidationConstants.InvoiceHeader.SingleInvoiceToEntryForExportsHasMultipleExchangeRateDate));
				Assert("Invoice2:", !invoice2.RowMessageErrors.Contains(ValidationConstants.InvoiceHeader.SingleInvoiceToEntryForExportsHasMultipleExchangeRateDate));
				Assert("Invoice3:", !invoice3.RowMessageErrors.Contains(ValidationConstants.InvoiceHeader.SingleInvoiceToEntryForExportsHasMultipleExchangeRateDate));
				Assert("Invoice4:", !invoice4.RowMessageErrors.Contains(ValidationConstants.InvoiceHeader.SingleInvoiceToEntryForExportsHasMultipleExchangeRateDate));
				Assert("Invoice1:", !invoice1.RowMessageErrors.Contains(ValidationConstants.InvoiceHeader.SingleForeignCurrencyInvoiceToEntryForExports));
				Assert("Invoice2:", !invoice2.RowMessageErrors.Contains(ValidationConstants.InvoiceHeader.SingleForeignCurrencyInvoiceToEntryForExports));
				Assert("Invoice3:", !invoice3.RowMessageErrors.Contains(ValidationConstants.InvoiceHeader.SingleForeignCurrencyInvoiceToEntryForExports));
				Assert("Invoice4:", !invoice4.RowMessageErrors.Contains(ValidationConstants.InvoiceHeader.SingleForeignCurrencyInvoiceToEntryForExports));
				declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
				invoice1.RunPreSaveValidation();
				invoice2.RunPreSaveValidation();
				invoice3.RunPreSaveValidation();
				invoice4.RunPreSaveValidation();
				Assert("Invoice1:", !invoice1.RowMessageErrors.Contains(ValidationConstants.InvoiceHeader.SingleInvoiceToEntryForExportsHasMultipleExchangeRateDate));
				Assert("Invoice2:", !invoice2.RowMessageErrors.Contains(ValidationConstants.InvoiceHeader.SingleInvoiceToEntryForExportsHasMultipleExchangeRateDate));
				Assert("Invoice3:", !invoice3.RowMessageErrors.Contains(ValidationConstants.InvoiceHeader.SingleInvoiceToEntryForExportsHasMultipleExchangeRateDate));
				Assert("Invoice4:", !invoice4.RowMessageErrors.Contains(ValidationConstants.InvoiceHeader.SingleInvoiceToEntryForExportsHasMultipleExchangeRateDate));
				Assert("Invoice1:", !invoice1.RowMessageErrors.Contains(ValidationConstants.InvoiceHeader.SingleForeignCurrencyInvoiceToEntryForExports));
				Assert("Invoice2:", invoice2.RowMessageErrors.Contains(ValidationConstants.InvoiceHeader.SingleForeignCurrencyInvoiceToEntryForExports));
				Assert("Invoice3:", invoice3.RowMessageErrors.Contains(ValidationConstants.InvoiceHeader.SingleForeignCurrencyInvoiceToEntryForExports));
				Assert("Invoice4:", invoice4.RowMessageErrors.Contains(ValidationConstants.InvoiceHeader.SingleForeignCurrencyInvoiceToEntryForExports));
			});
		}

		public void TestValidateJZ_RX_NKInvoice_Currency()
		{
			InvoiceHeader.JobDeclaration.JE_MessageType = ZAJobMessageTypeList.Codes.ImportByExternalBroker;
			InvoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.SouthAfrica;
			InvoiceHeader.Validation.ValidateJZ_RX_NKInvoice_Currency();
			AssertNoMessageErrorContaining(InvoiceHeader.JZ_RX_NKInvoice_CurrencyInfo, ValidationConstants.InvoiceHeader.MustBeZARCurrencyForImportByExternalBroker);
			InvoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Australia;
			InvoiceHeader.Validation.ValidateJZ_RX_NKInvoice_Currency();
			AssertHasMessageErrorContaining(InvoiceHeader.JZ_RX_NKInvoice_CurrencyInfo, ValidationConstants.InvoiceHeader.MustBeZARCurrencyForImportByExternalBroker);
		}

		public override void TestValidateJZ_CU_RelatedHouseBill()
		{
			var houseBill = InvoiceHeader.JobDeclaration.Bills.AddNew();
			InvoiceHeader.Validation.ValidateJZ_CU_RelatedHouseBill();
			var entry1 = InvoiceHeader.JobDeclaration.ActiveEntryHeaders.AddNew();
			var entry2 = InvoiceHeader.JobDeclaration.ActiveEntryHeaders.AddNew();
			InvoiceHeader.Validation.ValidateJZ_CU_RelatedHouseBill();
			Assert(!InvoiceHeader.JZ_CU_RelatedHouseBillInfo.HasMessageErrors());
		}

		public void TestCheckJZ_RN_NKDefaultOrigin()
		{
			InvoiceHeader.JZ_RN_NKDefaultOrigin = ZString.Empty;
			AssertNoMessageErrors(InvoiceHeader.JZ_RN_NKDefaultOriginInfo);
			InvoiceHeader.JZ_RN_NKDefaultOrigin = "!@";
			AssertHasMessageErrorContaining(InvoiceHeader.JZ_RN_NKDefaultOriginInfo, ListValidation.InvalidCodeMessageError);
			InvoiceHeader.JZ_RN_NKDefaultOrigin = Core.Constants.CountryCodes.Australia;
			AssertNoMessageErrorContaining(InvoiceHeader.JZ_RN_NKDefaultOriginInfo, ListValidation.InvalidCodeMessageError);
		}

		public override void TestValidateJZ_InvoiceNumber()
		{
			InvoiceHeader.JobDeclaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			InvoiceHeader.JZ_InvoiceNumber = ZString.Empty;
			AssertHasMessageError("Invoice number is required in imports", InvoiceHeader.JZ_InvoiceNumberInfo, ValidationConstants.InvoiceHeader.InvoiceNumberRequiredForImportShipments);
			InvoiceHeader.JobDeclaration.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
			InvoiceHeader.JZ_InvoiceNumber = ZString.Empty;
			AssertNoMessageError("Invoice number is not required in exports", InvoiceHeader.JZ_InvoiceNumberInfo, ValidationConstants.InvoiceHeader.InvoiceNumberRequiredForImportShipments);
			InvoiceHeader.JobDeclaration.JE_MessageType = ZAJobMessageTypeList.Codes.ExBond;
			InvoiceHeader.JZ_InvoiceNumber = ZString.Empty;
			AssertNoMessageError("Invoice number is not required on ex-bonds", InvoiceHeader.JZ_InvoiceNumberInfo, ValidationConstants.InvoiceHeader.InvoiceNumberRequiredForImportShipments);
		}

		public void TestCheckJZ_InvoiceDate()
		{
			InvoiceHeader.JobDeclaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			InvoiceHeader.JZ_InvoiceDate = ZDateTime.Empty;
			AssertHasMessageError("Invoice Date required for Invoice", InvoiceHeader.JZ_InvoiceDateInfo, ValidationConstants.InvoiceHeader.InvoiceDateRequiredForImportAndExportShipments);
			InvoiceHeader.JobDeclaration.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
			InvoiceHeader.JZ_InvoiceDate = ZDateTime.Empty;
			AssertHasMessageError("Invoice Date required for Invoice", InvoiceHeader.JZ_InvoiceDateInfo, ValidationConstants.InvoiceHeader.InvoiceDateRequiredForImportAndExportShipments);
		}

		public void TestCheckJZ_InvoiceDate_WhenExBond()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.ExBond;
			var invoiceHeader1 = declaration.Invoices.AddNew();
			var invoiceLine1 = declaration.InvoiceLines.AddNew();
			invoiceLine1.JI_JZ = invoiceHeader1.PK;

			var entryInstruction11 = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction11.CEI_Style = ProcedureCodes._11;

			var entryInstruction46 = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction46.CEI_Style = ProcedureCodes._46;

			var entryInstruction47 = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction47.CEI_Style = ProcedureCodes._47;

			var invoiceHeader2 = declaration.Invoices.AddNew();
			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_JZ = invoiceHeader2.PK;
			invoiceLine2.JI_CEI = entryInstruction47.PK;

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.ZAAddInvoiceDetailsToCUSDECMessage, GlbCompany.CurrentCompany.Country.Code, ZDateTime.Today, value: false))
			{
				Assert("ZAINVDET OFF", entryInstruction11, false);
				Assert("ZAINVDET OFF", entryInstruction46, false);
				Assert("ZAINVDET OFF", entryInstruction47, false);
			}

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.ZAAddInvoiceDetailsToCUSDECMessage, GlbCompany.CurrentCompany.Country.Code, ZDateTime.Today, value: true))
			{
				Assert("ZAINVDET ON", entryInstruction11, false);
				Assert("ZAINVDET ON", entryInstruction46, true);
				Assert("ZAINVDET ON", entryInstruction47, true);
			}

			void Assert(string message, CusEntryInstruction entryInstruction, bool errorsExpected)
			{
				invoiceLine1.JI_CEI = entryInstruction.PK;

				invoiceHeader1.JZ_InvoiceDate = ZDateTime.Empty;
				if (errorsExpected)
				{
					AssertHasMessageErrorContaining($"{message}: when CEI_Style is {entryInstruction.CEI_Style}, JZ_InvoiceDate isn't set", invoiceHeader1.JZ_InvoiceDateInfo, MandatoryValidation.YouHaveNotEntered);
				}
				else
				{
					AssertNoMessageErrors($"{message}: when CEI_Style is {entryInstruction.CEI_Style}, JZ_InvoiceDate isn't set", invoiceHeader1.JZ_InvoiceDateInfo);
				}

				invoiceHeader1.JZ_InvoiceDate = ZDateTime.Today;
				AssertNoMessageErrors($"{message}: when CEI_Style is {entryInstruction.CEI_Style}, JZ_InvoiceDate is set", invoiceHeader1.JZ_InvoiceDateInfo);
			}
		}

		[TestDate(2000, 3, 2, 11, 5, 3)]
		public void TestCheckJZ_InvoiceDateIsNotInTheFuture()
		{
			var dateInTheFuture = ZDateTime.Now.AddDays(1);
			InvoiceHeader.JobDeclaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			InvoiceHeader.JZ_InvoiceDate = ZDateTime.Empty;
			AssertNoMessageError("Invoice Date cannot be a future date.", InvoiceHeader.JZ_InvoiceDateInfo, ValidationConstants.InvoiceHeader.InvoiceDateCannotBeInTheFuture);
			InvoiceHeader.JZ_InvoiceDate = dateInTheFuture;
			AssertHasMessageError("Invoice Date cannot be a future date.", InvoiceHeader.JZ_InvoiceDateInfo, ValidationConstants.InvoiceHeader.InvoiceDateCannotBeInTheFuture);
			InvoiceHeader.JZ_InvoiceDate = ZDateTime.Now;
			AssertNoMessageError("Invoice Date cannot be a future date.", InvoiceHeader.JZ_InvoiceDateInfo, ValidationConstants.InvoiceHeader.InvoiceDateCannotBeInTheFuture);
			InvoiceHeader.JobDeclaration.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
			InvoiceHeader.JZ_InvoiceDate = ZDateTime.Empty;
			AssertNoMessageError("Invoice Date cannot be a future date.", InvoiceHeader.JZ_InvoiceDateInfo, ValidationConstants.InvoiceHeader.InvoiceDateCannotBeInTheFuture);
			InvoiceHeader.JZ_InvoiceDate = dateInTheFuture;
			AssertHasMessageError("Invoice Date cannot be a future date.", InvoiceHeader.JZ_InvoiceDateInfo, ValidationConstants.InvoiceHeader.InvoiceDateCannotBeInTheFuture);
			InvoiceHeader.JZ_InvoiceDate = ZDateTime.Now;
			AssertNoMessageError("Invoice Date cannot be a future date.", InvoiceHeader.JZ_InvoiceDateInfo, ValidationConstants.InvoiceHeader.InvoiceDateCannotBeInTheFuture);
		}

		public void TestCheckJZ_RelatedIndicator()
		{
			InvoiceHeader.JobDeclaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			InvoiceHeader.JZ_RelatedIndicator = ZString.Empty;
			AssertHasMessageError("Related Indicator", InvoiceHeader.JZ_RelatedIndicatorInfo, ValidationConstants.InvoiceHeader.RelationshipIndicatorRequiredForImports);
			InvoiceHeader.JZ_RelatedIndicator = RelatedIndicatorList.Codes.Yes;
			AssertNoMessageError("Related Indicator", InvoiceHeader.JZ_RelatedIndicatorInfo, ValidationConstants.InvoiceHeader.RelationshipIndicatorRequiredForImports);
			InvoiceHeader.JobDeclaration.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
			InvoiceHeader.JZ_RelatedIndicator = ZString.Empty;
			AssertNoMessageError("Related Indicator", InvoiceHeader.JZ_RelatedIndicatorInfo, ValidationConstants.InvoiceHeader.RelationshipIndicatorRequiredForImports);
		}

		public void TestCheckJZ_PaymentTerms()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.ZAAddInvoiceDetailsToCUSDECMessage, Core.Constants.CountryCodes.SouthAfrica, ZDateTime.Today, value: true))
			{
				const string paymentTermsInvalid = "Payment Terms invalid. Please choose a value from the list";

				InvoiceHeader.JZ_PaymentTerms = "???";
				AssertHasMessageErrorContaining(InvoiceHeader.JZ_PaymentTermsInfo, paymentTermsInvalid);

				InvoiceHeader.JZ_PaymentTerms = "-99";
				AssertNoMessageErrorContaining(InvoiceHeader.JZ_PaymentTermsInfo, paymentTermsInvalid);

				const string paymentTermsRequired = "Payment Terms is Required";

				var instruction1 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
				InvoiceHeader.InvoiceLines.AddNew().JI_CEI = instruction1.PK;

				InvoiceHeader.JobDeclaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
				InvoiceHeader.JZ_PaymentTerms = ZString.Empty;
				AssertHasMessageErrorContaining(InvoiceHeader.JZ_PaymentTermsInfo, paymentTermsRequired);

				InvoiceHeader.JobDeclaration.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
				InvoiceHeader.JZ_PaymentTerms = ZString.Empty;
				AssertHasMessageErrorContaining(InvoiceHeader.JZ_PaymentTermsInfo, paymentTermsRequired);

				InvoiceHeader.JobDeclaration.JE_MessageType = ZAJobMessageTypeList.Codes.ExBond;
				InvoiceHeader.JZ_PaymentTerms = ZString.Empty;
				AssertNoMessageErrorContaining(InvoiceHeader.JZ_PaymentTermsInfo, paymentTermsRequired);

				instruction1.CEI_Style = "46";
				InvoiceHeader.Validation.ValidateJZ_PaymentTerms();
				AssertHasMessageErrorContaining(InvoiceHeader.JZ_PaymentTermsInfo, paymentTermsRequired);

				instruction1.CEI_Style = "47";
				InvoiceHeader.Validation.ValidateJZ_PaymentTerms();
				AssertHasMessageErrorContaining(InvoiceHeader.JZ_PaymentTermsInfo, paymentTermsRequired);
			}
		}

		public void TestSupplierIsRequiredWithNoDeclaration()
		{
			InvoiceHeader.JZ_JE = ZGuid.Empty;
			InvoiceHeader.JZ_OH_Supplier = ZGuid.Empty;
			AssertHasMessageError(InvoiceHeader.JZ_OH_SupplierInfo, "Please enter a supplier");
		}

		public void TestEmptyCustomsSupplierCodeWithVDNIsMessageErrorForImport()
		{
			OrgHeader importer = OrgHeader.New(Factory);
			importer.OH_RL_NKClosestPort = "ZAAAM";
			Assert("Precondition", !InvoiceHeader.JZ_OH_SupplierInfo.HasMessageErrors());
			JobDeclaration declaration = InvoiceHeader.JobDeclaration;
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			OrgHeader supplier = OrgHeader.New(Factory);
			OrgSupplierBuyerLink link = supplier.BuyerLinks.AddNew(importer);
			link.OL_ValuationBasisDeterminationNum = "123";
			InvoiceHeader.JZ_OH_Supplier = supplier.PK;
			InvoiceHeader.Validation.ValidateJZ_OH_Supplier();
			AssertHasMessageErrors(InvoiceHeader.JZ_OH_SupplierInfo);
		}

		public void TestEmptyCustomsSupplierCodeWithoutVDNWhenImport_NoMessageError()
		{
			AssertNoMessageErrors("Precondition", InvoiceHeader.JZ_OH_SupplierInfo);
			InvoiceHeader.JobDeclaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			InvoiceHeader.JZ_OH_Supplier = OrgHeader.New(Factory).PK;
			AssertNoMessageErrors(InvoiceHeader.JZ_OH_SupplierInfo);
		}

		public void TestEmptyCustomsSupplierCodeIsMessageErrorForExport_NoMatterVDNIsSpecified()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			JobComInvoiceHeader header = declaration.Invoices.AddNew();
			OrgHeader supplier = OrgHeader.New(Factory);
			header.JZ_OH_Supplier = supplier.PK;
			AssertHasMessageErrors(header.JZ_OH_SupplierInfo);
			OrgSupplierBuyerLink link = supplier.SupplierLinks.AddNew(supplier);
			link.OL_ValuationBasisDeterminationNum = "123";
			InvoiceHeader.JobDeclaration.JE_OH_Importer = OrgHeader.New(Factory).PK;
			link.OL_OH_Buyer = InvoiceHeader.JobDeclaration.Importer.PK;
			header.JZ_OH_Supplier = supplier.PK;
			AssertHasMessageErrors(header.JZ_OH_SupplierInfo);
		}

		public void TestCustomsSupplierCodeNoVDNIsFineForExport()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
			JobComInvoiceHeader header = declaration.Invoices.AddNew();
			OrgHeader supplier = OrgHeader.New(Factory);
			OrgCusCode cusCode = supplier.CustomsCodes.AddNew();
			cusCode.OK_CodeType = "CSC";
			cusCode.OK_CustomsRegNo = "123";
			cusCode.OK_RN_NKCodeCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			header.JZ_OH_Supplier = supplier.PK;
			AssertNoMessageErrors(header.JZ_OH_SupplierInfo); //Supplier with CSC
		}

		public void TestMessageErrorIfNoSupplier()
		{
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			InvoiceHeader.JZ_OH_Supplier = ZGuid.Empty;
			AssertHasMessageErrors(InvoiceHeader.JZ_OH_SupplierInfo);
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
			InvoiceHeader.JZ_OH_Supplier = ZGuid.Empty;
			AssertHasMessageErrors(InvoiceHeader.JZ_OH_SupplierInfo);
		}

		public void TestWarningIfMissingMandatoryChargesForIncoTerm()
		{
			InvoiceHeader.JZ_IncoTerm = "CIF";
			AssertEquals("CIF needs OFT/ONS", true, InvoiceHeader.JZ_IncoTermInfo.HasWarnings());
		}

		public override void TestValidateAbsenceOfOFTOrONS()
		{
			BaseJobDeclaration testDec = BaseJobDeclaration.New(Factory);
			BaseJobComInvoiceHeader invoice = testDec.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 1000m;
			invoice.JZ_RX_NKInvoice_Currency = testDec.LocalCurrencyCode;
			invoice.JobDeclaration.JE_MessageType = ZAJobMessageTypeList.Codes.ExBond;
			invoice.RunPreSaveValidation();
			AssertNoWarning(invoice.JZ_Calc_CIFAmountInfo, "The CIF amount is the same as the FOB amount and you have not entered any Overseas Freight or Insurance.");
			invoice.JobDeclaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			invoice.RunPreSaveValidation();
			AssertHasWarning(invoice.JZ_Calc_CIFAmountInfo, "The CIF amount is the same as the FOB amount and you have not entered any Overseas Freight or Insurance.");
			invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 100m);
			invoice.RunPreSaveValidation();
			AssertNoWarning(invoice.JZ_Calc_CIFAmountInfo, "The CIF amount is the same as the FOB amount and you have not entered any Overseas Freight or Insurance.");
		}

		public void TestNoExeptionsOnPreSaveValidation()
		{
			BaseJobComInvoiceHeader baseJobComInvoiceHeader = Factory.New<BaseJobComInvoiceHeader>();
			baseJobComInvoiceHeader.JZ_IncoTerm = "CIF";
			baseJobComInvoiceHeader.JZ_InvoiceAmount = 1000m;
			AssertNoExceptionThrown(() => baseJobComInvoiceHeader.RunPreSaveValidation());
		}

		public void TestCurrencyRangeAndState()
		{
			RefCurrency currency = Factory.New<RefCurrency>();
			currency.RX_Code = "ZZZ";
			RefExchangeRate rate = Factory.New<RefExchangeRate>();
			rate.RE_ExRateType = "CUS";
			rate.RE_StartDate = ZDateTime.Today.AddDays(-2);
			rate.RE_ExpiryDate = rate.RE_StartDate;
			rate.RE_SellRate = 1.36m;
			rate.RE_RX_NKExCurrency = currency.Code;
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MasterBillIssuedDate = ZDateTime.Today;
			var header = declaration.Invoices.AddNew();
			header.JZ_RX_NKInvoice_Currency = currency.Code;
			var invoiceLine = header.InvoiceLines.AddNew();
			declaration.ResumeApportionment();
			AssertHasMessageErrorContaining(header.JZ_RX_NKInvoice_CurrencyInfo, "There is no valid exchange rate for this currency");
			AssertEquals("Rate Check 01", 0m, header.JZ_InvoiceCurrExRate);
			declaration.JE_MasterBillIssuedDate = ZDateTime.Today.AddDays(-2);
			AssertNoNotifications(header.JZ_RX_NKInvoice_CurrencyInfo);
			AssertEquals("Rate Check 02", 1.36m, header.JZ_InvoiceCurrExRate);
			declaration.JE_MasterBillIssuedDate = ZDateTime.Today;
			header.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
			AssertNoNotifications(header.JZ_RX_NKInvoice_CurrencyInfo);
			AssertEquals("Rate Check 02", 1m, header.JZ_InvoiceCurrExRate);
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
			header.JZ_RX_NKInvoice_Currency = currency.Code;
			header.Validation.ValidateJZ_RX_NKInvoice_Currency();
			AssertHasMessageErrorContaining(header.JZ_RX_NKInvoice_CurrencyInfo, "There is no valid exchange rate for this currency");
			AssertEquals("Rate Check 01", 0m, header.JZ_InvoiceCurrExRate);
			rate = Factory.New<RefExchangeRate>();
			rate.RE_ExRateType = "CUS";
			rate.RE_StartDate = ZDateTime.Today.AddDays(-1);
			rate.RE_ExpiryDate = rate.RE_StartDate;
			rate.RE_SellRate = 1.2;
			rate.RE_RX_NKExCurrency = currency.Code;
			Factory.Save();
			header.Validation.ValidateJZ_RX_NKInvoice_Currency();
			AssertNoNotifications(header.JZ_RX_NKInvoice_CurrencyInfo);
			AssertEquals("Rate Check 02", 1.2m, header.JZ_InvoiceCurrExRate);
			rate.RE_ExRateType = "SEL";
			Factory.Save();
			declaration.DoMerge();
			AssertNoWarnings(header.JZ_RX_NKInvoice_CurrencyInfo);
			AssertHasMessageErrorContaining(header.JZ_RX_NKInvoice_CurrencyInfo, "There is no valid exchange rate for this currency");
			AssertEquals("Rate Check 03", 0m, header.JZ_InvoiceCurrExRate);
			header.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
			declaration.DoMerge();
			AssertNoNotifications(header.JZ_RX_NKInvoice_CurrencyInfo);
			AssertEquals("Rate Check 04", 1m, header.JZ_InvoiceCurrExRate);
		}

		public void TestCheckJZ_VDN()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.LocalCustomsSupplierCode = "432111";
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_RelatedIndicator = RelatedIndicatorList.Codes.Yes;
			invoiceHeader.JZ_OH_Supplier = orgHeader.PK;
			invoiceHeader.Validation.ValidateJZ_VDN();
			AssertHasMessageError(invoiceHeader.JZ_VDNInfo, ValidationConstants.InvoiceHeader.VDNRequiredWhenSupplierHasCSCCode);
			invoiceHeader.JZ_VDN = "123";
			AssertNoMessageError(invoiceHeader.JZ_VDNInfo, ValidationConstants.InvoiceHeader.VDNRequiredWhenSupplierHasCSCCode);
			invoiceHeader.JZ_RelatedIndicator = RelatedIndicatorList.Codes.No;
			invoiceHeader.JZ_VDN = ZString.Empty;
			invoiceHeader.Validation.ValidateJZ_VDN();
			AssertNoMessageError(invoiceHeader.JZ_VDNInfo, ValidationConstants.InvoiceHeader.VDNRequiredWhenSupplierHasCSCCode);
			invoiceHeader.JZ_RelatedIndicator = RelatedIndicatorList.Codes.Exempt;
			invoiceHeader.JZ_VDN = ZString.Empty;
			invoiceHeader.Validation.ValidateJZ_VDN();
			AssertNoMessageError(invoiceHeader.JZ_VDNInfo, ValidationConstants.InvoiceHeader.VDNRequiredWhenSupplierHasCSCCode);
		}

		public void TestCheckJZ_ROOType()
		{
			var helper = new ZAUniversalReferenceTestDataHelper(Factory, setupBasicTariffData: false);
			helper.CreateAdditionalInformationCusCodeEntry("EUR");
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_ROOCert = "123";
			AssertHasMessageErrorContaining(invoiceHeader.JZ_ROOTypeInfo, ValidationConstants.InvoiceLine.NoROOTypeEnteredForCert);
			invoiceHeader.JZ_ROOType = "@@";
			AssertHasMessageErrorContaining(invoiceHeader.JZ_ROOTypeInfo, ListValidation.InvalidCodeMessageError);
			invoiceHeader.JZ_ROOType = "EUR";
			AssertNoNotifications(invoiceHeader.JZ_ROOTypeInfo);
		}

		public void TestCheckJZ_ValuationMarkup()
		{
			var declaration = Factory.New<JobDeclaration>();
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JobDeclaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			invoiceHeader.JZ_VDN = "10";
			invoiceHeader.JZ_ValuationMarkup = -10.0;
			invoiceHeader.Validation.ValidateAll();
			AssertHasError(invoiceHeader.JZ_ValuationMarkupInfo, "value cannot be negative.");
			invoiceHeader.JobDeclaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			invoiceHeader.JZ_VDN = "10";
			invoiceHeader.JZ_ValuationMarkup = 0.0;
			invoiceHeader.Validation.ValidateAll();
			AssertNoError(invoiceHeader.JZ_ValuationMarkupInfo, "value cannot be negative.");
			invoiceHeader.JobDeclaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			invoiceHeader.JZ_VDN = ZString.Empty;
			invoiceHeader.JZ_ValuationMarkup = 10;
			invoiceHeader.Validation.ValidateAll();
			AssertHasMessageErrorContaining(invoiceHeader.JZ_ValuationMarkupInfo, "Please do not enter a value.");
			invoiceHeader.JZ_ValuationMarkup = 0;
			invoiceHeader.Validation.ValidateAll();
			AssertNoMessageErrors(invoiceHeader.JZ_ValuationMarkupInfo);
		}

		public void TestValidateSingleVDNPerEntryInstruction()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			var cei = declaration.CustomsEntryInstructions.AddNew();
			var invoiceHeader1 = declaration.Invoices.AddNew();
			var line1 = invoiceHeader1.InvoiceLines.AddNew();
			line1.JI_CEI = cei.PK;
			var invoiceHeader2 = declaration.Invoices.AddNew();
			var line2 = invoiceHeader2.InvoiceLines.AddNew();
			line2.JI_CEI = cei.PK;
			invoiceHeader1.JZ_VDN = "0001";
			invoiceHeader2.JZ_VDN = "0002";
			AssertHasRowMessageError(cei, ValidationConstants.EntryInstruction.InvoicesWithDifferentVDNNumberOnEntryInstruction);
			invoiceHeader2.JZ_VDN = "0001";
			AssertNoRowMessageError(cei, ValidationConstants.EntryInstruction.InvoicesWithDifferentVDNNumberOnEntryInstruction);
		}

		public void TestExchangeRateValidDateOfExportWarning()
		{
			TestCaseHelper.ClearTable(RefExchangeRateSchema.Constants.TableName);
			var foreignCurrency = Factory.New<RefCurrency>();
			foreignCurrency.RX_Code = "~~~";

			var exchangeRate = foreignCurrency.ExchangeRates.AddNew();
			exchangeRate.RE_ExRateType = "CUS";
			exchangeRate.RE_SellRate = 0.5m;
			exchangeRate.RE_StartDate = new ZDateTime(2005, 1, 1);
			exchangeRate.RE_ExpiryDate = exchangeRate.RE_StartDate;

			var testDec = Factory.New<JobDeclarationForTest>();
			testDec.JE_ExportDate = exchangeRate.RE_StartDate.AddDays(1);
			var invoice = testDec.Invoices.AddNew();

			AssertNoWarningContaining(invoice.JZ_RX_NKInvoice_CurrencyInfo, "A valid date of export is required to calculate exchange rates.");
			testDec.IsValidDateOfValuation = false;
			invoice.JZ_RX_NKInvoice_Currency = foreignCurrency.RX_Code;
			AssertHasWarning(invoice.JZ_RX_NKInvoice_CurrencyInfo, "A valid date of export is required to calculate exchange rates.");
		}

		public void TestCheckJZ_OH_Supplier_WhenExBond()
		{
			var missingAddressOrg = Factory.New<OrgHeader>();

			var missingCountryOrg = Factory.New<OrgHeader>();
			var address1 = missingCountryOrg.Addresses.AddNew();
			address1.OA_Address1 = "SUPPLIER ADDRESS";
			address1.OA_RN_NKCountryCode = ZString.Empty;
			address1.AddressCapability.SetCapabilityEnabled(OrgAddressType.Office.Code);
			address1.AddressCapability.SetIsMainAddress(OrgAddressType.Office.Code);

			var validOrg = Factory.New<OrgHeader>();
			var address2 = validOrg.Addresses.AddNew();
			address2.OA_Address1 = "SUPPLIER ADDRESS";
			address2.OA_RN_NKCountryCode = "ZA";
			address2.AddressCapability.SetCapabilityEnabled(OrgAddressType.Office.Code);
			address2.AddressCapability.SetIsMainAddress(OrgAddressType.Office.Code);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.ExBond;
			var invoiceHeader1 = declaration.Invoices.AddNew();
			var invoiceLine1 = declaration.InvoiceLines.AddNew();
			invoiceLine1.JI_JZ = invoiceHeader1.PK;

			var entryInstruction11 = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction11.CEI_Style = ProcedureCodes._11;

			var entryInstruction46 = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction46.CEI_Style = ProcedureCodes._46;

			var entryInstruction47 = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction47.CEI_Style = ProcedureCodes._47;

			var invoiceHeader2 = declaration.Invoices.AddNew();
			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_JZ = invoiceHeader2.PK;
			invoiceLine2.JI_CEI = entryInstruction47.PK;

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.ZAAddInvoiceDetailsToCUSDECMessage, GlbCompany.CurrentCompany.Country.Code, ZDateTime.Today, value: false))
			{
				Assert("ZAINVDET OFF", entryInstruction11, false);
				Assert("ZAINVDET OFF", entryInstruction46, false);
				Assert("ZAINVDET OFF", entryInstruction47, false);
			}

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.ZAAddInvoiceDetailsToCUSDECMessage, GlbCompany.CurrentCompany.Country.Code, ZDateTime.Today, value: true))
			{
				Assert("ZAINVDET ON", entryInstruction11, false);
				Assert("ZAINVDET ON", entryInstruction46, true);
				Assert("ZAINVDET ON", entryInstruction47, true);
			}

			void Assert(string message, CusEntryInstruction entryInstruction, bool errorsExpected)
			{
				invoiceLine1.JI_CEI = entryInstruction.PK;

				invoiceHeader1.JZ_OH_Supplier = ZGuid.Empty;
				if (errorsExpected)
				{
					AssertHasMessageError($"{message}: when CEI_Style is {entryInstruction.CEI_Style}, JZ_OH_Supplier isn't set", invoiceHeader1.JZ_OH_SupplierInfo, "A Supplier is required for this declaration.");
				}
				else
				{
					AssertNoMessageErrors($"{message}: when CEI_Style is {entryInstruction.CEI_Style}, JZ_OH_Supplier isn't set", invoiceHeader1.JZ_OH_SupplierInfo);
				}

				invoiceHeader1.JZ_OH_Supplier = ZGuid.BrettsGuid;
				if (errorsExpected)
				{
					AssertHasErrorContaining($"{message}: when CEI_Style is {entryInstruction.CEI_Style}, JZ_OH_Supplier is invalid", invoiceHeader1.JZ_OH_SupplierInfo, ListValidation.InvalidCodeError);
				}
				else
				{
					AssertNoErrors($"{message}: when CEI_Style is {entryInstruction.CEI_Style}, JZ_OH_Supplier is invalid", invoiceHeader1.JZ_OH_SupplierInfo);
				}

				invoiceHeader1.JZ_OH_Supplier = missingAddressOrg.PK;
				if (errorsExpected)
				{
					AssertHasMessageError($"{message}: when CEI_Style is {entryInstruction.CEI_Style}, JZ_OH_Supplier is set, OA_Address1 isn't set", invoiceHeader1.JZ_OH_SupplierInfo, "A Supplier's address is required for this declaration.");
				}
				else
				{
					AssertNoMessageErrors($"{message}: when CEI_Style is {entryInstruction.CEI_Style}, JZ_OH_Supplier is set, OA_Address1 isn't set", invoiceHeader1.JZ_OH_SupplierInfo);
				}

				invoiceHeader1.JZ_OH_Supplier = missingCountryOrg.PK;
				if (errorsExpected)
				{
					AssertHasMessageError($"{message}: when CEI_Style is {entryInstruction.CEI_Style}, JZ_OH_Supplier is set, OA_Address1 is set, OA_RN_NKCountryCode isn't set", invoiceHeader1.JZ_OH_SupplierInfo, "A Supplier's address is required for this declaration.");
				}
				else
				{
					AssertNoMessageErrors($"{message}: when CEI_Style is {entryInstruction.CEI_Style}, JZ_OH_Supplier is set, OA_Address1 is set, OA_RN_NKCountryCode isn't set", invoiceHeader1.JZ_OH_SupplierInfo);
				}

				invoiceHeader1.JZ_OH_Supplier = validOrg.PK;
				AssertNoMessageErrors($"{message}: when CEI_Style is {entryInstruction.CEI_Style}, JZ_OH_Supplier is set, OA_Address1 is set, OA_RN_NKCountryCode is set", invoiceHeader1.JZ_OH_SupplierInfo);
			}
		}

		protected override string GetValidSupplierCode() => "144466";

		protected override Type GetTypeForTest() => typeof(JobComInvoiceHeaderValidation);

		JobComInvoiceHeader InvoiceHeader => invoiceHeader as JobComInvoiceHeader;

		class JobDeclarationForTest : JobDeclaration
		{
			public JobDeclarationForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public override ZDateTime DateOfValuation => IsValidDateOfValuation ? base.DateOfValuation : ZDateTime.Invalid;
			public bool IsValidDateOfValuation = true;
		}
	}
}
