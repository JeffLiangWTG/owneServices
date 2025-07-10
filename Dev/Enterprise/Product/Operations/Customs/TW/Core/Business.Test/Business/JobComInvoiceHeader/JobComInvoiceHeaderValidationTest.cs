using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class JobComInvoiceHeaderValidationTest : Customs.Business.Testing.JobComInvoiceHeaderValidationTest
	{
		public void TestCheckJZ_RX_NKInvoice_Currency()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeaders = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders;
			var invoice1 = invoiceHeaders.AddNew();
			invoice1.JZ_RX_NKInvoice_Currency = "TWD";
			var invoice2 = invoiceHeaders.AddNew();
			invoice2.JZ_RX_NKInvoice_Currency = ZString.Empty;
			AssertHasMessageErrorContaining(invoice2.JZ_RX_NKInvoice_CurrencyInfo, ValidationConstants.InvoiceHeader.MultipleCurrencyError);

			invoice2.JZ_RX_NKInvoice_Currency = "TWD";
			AssertNoMessageErrorContaining(invoice2.JZ_RX_NKInvoice_CurrencyInfo, ValidationConstants.InvoiceHeader.MultipleCurrencyError);

			invoice2.JZ_RX_NKInvoice_Currency = "USD";
			AssertHasMessageErrorContaining(invoice2.JZ_RX_NKInvoice_CurrencyInfo, ValidationConstants.InvoiceHeader.MultipleCurrencyError);
		}

		public void TestCheckJZ_InvoiceAmount()
		{
			var invoice = Factory.New<JobComInvoiceHeader>();
			var propertyInfo = invoice.JZ_InvoiceAmountInfo;
			var message = "Please enter an 'Inv. Total Amount' greater than 0.";
			invoice.JZ_InvoiceAmount = 0m;
			AssertHasMessageError(propertyInfo, message);

			invoice.JZ_InvoiceAmount = 0.1m;
			AssertNoMessageError(propertyInfo, message);

			invoice.JZ_InvoiceAmount = -1m;
			AssertHasMessageError(propertyInfo, message);
		}

		public void TestCheckJZ_NetWeight()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			var jobComInvoiceLine1 = invoiceHeader.JobComInvoiceLines.AddNew();
			jobComInvoiceLine1.JI_NetWeight = 3.25;
			jobComInvoiceLine1.JI_NetWeightUQ = "KG";

			var jobComInvoiceLine2 = invoiceHeader.JobComInvoiceLines.AddNew();
			jobComInvoiceLine2.JI_NetWeight = 3250;
			jobComInvoiceLine2.JI_NetWeightUQ = "G";

			invoiceHeader.JZ_NetWeightUQ = "KG";
			invoiceHeader.JZ_NetWeight = 3.25;
			AssertHasWarning(invoiceHeader.JZ_NetWeightInfo, "The sum of all Invoice Line Net Weight 6.5 KG does not balance with the Invoice Header total Net Weight 3.25 KG.");

			invoiceHeader.JZ_NetWeight = 6.5;
			AssertNoWarnings(invoiceHeader.JZ_NetWeightInfo);
		}

		public void TestCheckJZ_Weight()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			var jobComInvoiceLine1 = invoiceHeader.JobComInvoiceLines.AddNew();
			jobComInvoiceLine1.JI_Weight = 3.25;
			jobComInvoiceLine1.JI_WeightUQ = "KG";

			var jobComInvoiceLine2 = invoiceHeader.JobComInvoiceLines.AddNew();
			jobComInvoiceLine2.JI_Weight = 3250;
			jobComInvoiceLine2.JI_WeightUQ = "G";

			invoiceHeader.JZ_WeightUQ = "KG";
			invoiceHeader.JZ_Weight = 3.25;
			AssertHasWarning(invoiceHeader.JZ_WeightInfo, "The sum of all Invoice Line Gross Weight 6.5 KG does not balance with the Invoice Header total Gross Weight 3.25 KG.");

			invoiceHeader.JZ_Weight = 6.5;
			AssertNoWarnings(invoiceHeader.JZ_WeightInfo);
		}

		public override void TestCheckJZ_MarksAndNumbersIsWesternEuropeanIfRequired()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_MarksAndNumbers = "你好";
			var targetInfo = invoice.JZ_MarksAndNumbersInfo;
			AssertEquals(false, targetInfo.HasError(EnglishCharactersValidation.GetNotificationMessage(targetInfo)));
		}

		public void TestCheckTW_MarksAndNumbersLength()
		{
			var warning = "Only the first 512 characters will be sent to the customs.";
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var targetInfo = invoice.TW_MarksAndNumbersInfo;
			invoice.TW_MarksAndNumbers = new ZString('A', 512);
			AssertNoWarning(targetInfo, warning);

			invoice.TW_MarksAndNumbers = new ZString('A', 513);
			AssertHasWarning(targetInfo, warning);
		}

		public void TestParent()
		{
			JobComInvoiceHeader parent = Factory.New<JobComInvoiceHeader>();
			AssertEquals(parent.Validation.Parent, parent);
		}

		protected override Type GetTypeForTest()
		{
			return typeof(JobComInvoiceHeaderValidation);
		}

		public void TestCheckJZ_RelatedIndicator()
		{
			var info = invoiceHeader.JZ_RelatedIndicatorInfo;

			invoiceHeader.JZ_RelatedIndicator = "Z";
			AssertHasMessageError(info, ListValidation.InvalidCodeMessageError);

			invoiceHeader.JZ_RelatedIndicator = ((UntranslatableCodeDescriptionPairList)invoiceHeader.Lookups.RelatedIndicatorList)[0].Code;
			AssertNoMessageErrors(info);

			invoiceHeader.JZ_RelatedIndicator = ZString.Empty;
			AssertHasMessageErrorContaining(info, MandatoryValidation.YouHaveNotEntered);
		}

		public override void TestValidateAbsenceOfOFTOrONS()
		{
			Assert("ValidateAbsenceOfOFTOrONS", true);
		}

		public void TestCheckJZ_Calc_CIFAmount()
		{
			var declaration = Factory.New<JobDeclaration>();

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 1000m;
			invoice.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
			invoice.RunPreSaveValidation();
			AssertNoWarnings(invoice.JZ_Calc_CIFAmountInfo);
			AssertNoErrors(invoice.JZ_Calc_CIFAmountInfo);

			invoice.JZ_InvoiceAmount = -1m;
			invoice.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
			invoice.RunPreSaveValidation();
			AssertNoWarnings(invoice.JZ_Calc_CIFAmountInfo);
			AssertNoErrors(invoice.JZ_Calc_CIFAmountInfo);
		}

		public void TestCheckJZ_IncoTerm()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_IncoTerm = "FOB";
			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_IncoTerm = "FOB";
			var info2 = invoice2.JZ_IncoTermInfo;
			var messageErrorHavingMultipleINCOTermsOnSingleJob = ValidationConstants.InvoiceHeader.HavingMultipleINCOTermsOnSingleJob;

			AssertNoMessageErrorContaining(info2, messageErrorHavingMultipleINCOTermsOnSingleJob);
			invoice2.JZ_IncoTerm = "CFR";
			AssertHasMessageErrorContaining(info2, messageErrorHavingMultipleINCOTermsOnSingleJob);

			invoice2.Charges.RemoveAndDeleteAll();
			invoice2.GroupHeader.Charges.RemoveAndDeleteAll();
			var messageText = "International Freight is required.";
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			var charge = invoice2.Charges.AddNew();
			charge.J7_ChargeType = Common.CustomsChargeTypeList.Codes.OverseasInsurance;
			charge.J7_Amount = 1m;
			invoice2.Validation.ValidateJZ_IncoTerm();
			AssertHasMessageError(info2, messageText);

			charge.J7_ChargeType = Common.CustomsChargeTypeList.Codes.OverseasFreight;
			invoice2.Validation.ValidateJZ_IncoTerm();
			AssertNoMessageError(info2, messageText);

			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			charge.J7_ChargeType = Common.CustomsChargeTypeList.Codes.OverseasInsurance;
			invoice2.Validation.ValidateJZ_IncoTerm();
			AssertNoMessageError(info2, messageText);
		}

		public void TestCheckJZ_NoOfPacks()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_TotalNoOfPacksPackType = "CTN";
			declaration.JE_TotalNoOfPacks = 10;
			var invoice = declaration.Invoices.AddNew();
			invoice.Validation.ValidateJZ_NoOfPacks();
			AssertNoWarningContaining(invoice.JZ_NoOfPacksInfo, "The sum of all invoice header package numbers");
			AssertNoWarningContaining(invoice.JZ_NoOfPacksInfo, "does not balance with the declaration total package number");
			invoice.JZ_NoOfPacks = 5;

			AssertHasWarningContaining(invoice.JZ_NoOfPacksInfo, "The sum of all invoice header package numbers");
			AssertHasWarningContaining(invoice.JZ_NoOfPacksInfo, "does not balance with the declaration total package number");
			Assert(invoice.JZ_NoOfPacksInfo.HasWarning("The sum of all invoice header package numbers {5 CTN} does not balance with the declaration total package number {10 CTN}."));
		}

		public override void TestValidateJZ_OH_Supplier()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();

			invoiceHeader.JZ_JE = ZGuid.Empty;
			new Customs.Business.FakeDeclarationCreatorForInvoice(invoiceHeader);
			invoiceHeader.JZ_OH_Supplier = ZGuid.Empty;
			AssertNoErrors(invoiceHeader.JZ_OH_SupplierInfo);
			invoiceHeader.JZ_OH_Supplier = OrganisationsDataRegistry.Instance.MiscOrganisation.Value.Organisation;
			AssertNoErrors(invoiceHeader.JZ_OH_SupplierInfo);

			invoiceHeader.JZ_JE = declaration.PK;
			invoiceHeader.JZ_OH_Supplier = OrganisationsDataRegistry.Instance.MiscOrganisation.Value.Organisation;
			invoiceHeader.Validation.ValidateJZ_OH_Supplier();
			AssertNoErrors(invoiceHeader.JZ_OH_SupplierInfo);
			invoiceHeader.JZ_OH_Supplier = ZGuid.Empty;
			AssertNoErrors(invoiceHeader.JZ_OH_SupplierInfo);
		}

		public override void TestValidateJZ_OH_Buyer()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();

			invoiceHeader.JZ_JE = ZGuid.Empty;
			new Customs.Business.FakeDeclarationCreatorForInvoice(invoiceHeader);
			invoiceHeader.JZ_OH_Buyer = ZGuid.Empty;
			AssertNoErrors(invoiceHeader.JZ_OH_BuyerInfo);

			invoiceHeader.JZ_JE = declaration.PK;
			invoiceHeader.Validation.ValidateJZ_OH_Buyer();
			AssertNoErrors(invoiceHeader.JZ_OH_BuyerInfo);
		}

		public void TestCheckTW1_CertificateType_JZ_InvoiceDate()
		{
			AssertCheckTW1_CertificateType_JZ_InvoiceDateEmpty(true, CertificateTypeList.Codes.Code9, ControllingMessageTypeList.Codes.NX101, ZDateTime.Empty);
			AssertCheckTW1_CertificateType_JZ_InvoiceDateEmpty(true, CertificateTypeList.Codes.Code11, ControllingMessageTypeList.Codes.NX101, ZDateTime.Empty);
			AssertCheckTW1_CertificateType_JZ_InvoiceDateEmpty(true, CertificateTypeList.Codes.Code13, ControllingMessageTypeList.Codes.NX101, ZDateTime.Empty);
			AssertCheckTW1_CertificateType_JZ_InvoiceDateEmpty(true, CertificateTypeList.Codes.Code14, ControllingMessageTypeList.Codes.NX101, ZDateTime.Empty);
			AssertCheckTW1_CertificateType_JZ_InvoiceDateEmpty(true, CertificateTypeList.Codes.Code15, ControllingMessageTypeList.Codes.NX101, ZDateTime.Empty);
			AssertCheckTW1_CertificateType_JZ_InvoiceDateEmpty(true, CertificateTypeList.Codes.Code18, ControllingMessageTypeList.Codes.NX101, ZDateTime.Empty);
			AssertCheckTW1_CertificateType_JZ_InvoiceDateEmpty(true, CertificateTypeList.Codes.Code19, ControllingMessageTypeList.Codes.NX101, ZDateTime.Empty);
			AssertCheckTW1_CertificateType_JZ_InvoiceDateEmpty(false, CertificateTypeList.Codes.Code1, ControllingMessageTypeList.Codes.NX101, ZDateTime.Empty);
			AssertCheckTW1_CertificateType_JZ_InvoiceDateEmpty(false, CertificateTypeList.Codes.Code18, ControllingMessageTypeList.Codes.X101, ZDateTime.Empty);
			AssertCheckTW1_CertificateType_JZ_InvoiceDateEmpty(false, CertificateTypeList.Codes.Code18, ControllingMessageTypeList.Codes.NX101, ZDateTime.BrettsBirthday);
		}

		void AssertCheckTW1_CertificateType_JZ_InvoiceDateEmpty(bool expectShowError, ZString certificateType, ZString messageType, ZDateTime invoiceDate)
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CusEntryInstruction;
			var invoiceHeader1 = declaration.Invoices.AddNew();
			var invoiceHeader2 = declaration.Invoices.AddNew();
			invoiceHeader1.JZ_InvoiceDate = invoiceDate;
			invoiceHeader2.JZ_InvoiceDate = invoiceDate;
			var messageHeader = entryInstruction.ControllingMessageHeaders.AddNew();
			messageHeader.TW1_ControllingMessageType = messageType;
			messageHeader.TW1_CertificateType = certificateType;
			var invoiceLine = (JobComInvoiceLine)invoiceHeader1.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			messageHeader.TW1_ControllingAgency = "XX";
			var link = invoiceLine.InvoiceLineLinkControllingMsgHeaders.Cast<InvoiceLineLinkControllingMsgHeader>().First(x => x.ControllingAgency == "XX");
			link.IsLinkedCMHeader = true;

			messageHeader.Validation.ValidateTW1_CertificateType();
			CombineAssertions(() =>
			{
				AssertEquals("header1 Invoice Date", expectShowError, invoiceHeader1.JZ_InvoiceDateInfo.HasMessageError(MandatoryValidation.YouHaveNotEnteredMessage("Invoice Date")));
				AssertEquals("header2 : no link to CMHeader", false, invoiceHeader2.JZ_InvoiceDateInfo.HasMessageError(MandatoryValidation.YouHaveNotEnteredMessage("Invoice Date")));
			});
		}

		public void TestCheckTW1_CertificateType_JZ_InvoiceNumber()
		{
			AssertCheckTW1_CertificateType_JZ_InvoiceNumberEmpty(true, CertificateTypeList.Codes.Code9, ControllingMessageTypeList.Codes.NX101, ZString.Empty);
			AssertCheckTW1_CertificateType_JZ_InvoiceNumberEmpty(true, CertificateTypeList.Codes.Code11, ControllingMessageTypeList.Codes.NX101, ZString.Empty);
			AssertCheckTW1_CertificateType_JZ_InvoiceNumberEmpty(true, CertificateTypeList.Codes.Code13, ControllingMessageTypeList.Codes.NX101, ZString.Empty);
			AssertCheckTW1_CertificateType_JZ_InvoiceNumberEmpty(true, CertificateTypeList.Codes.Code14, ControllingMessageTypeList.Codes.NX101, ZString.Empty);
			AssertCheckTW1_CertificateType_JZ_InvoiceNumberEmpty(true, CertificateTypeList.Codes.Code15, ControllingMessageTypeList.Codes.NX101, ZString.Empty);
			AssertCheckTW1_CertificateType_JZ_InvoiceNumberEmpty(true, CertificateTypeList.Codes.Code18, ControllingMessageTypeList.Codes.NX101, ZString.Empty);
			AssertCheckTW1_CertificateType_JZ_InvoiceNumberEmpty(true, CertificateTypeList.Codes.Code19, ControllingMessageTypeList.Codes.NX101, ZString.Empty);
			AssertCheckTW1_CertificateType_JZ_InvoiceNumberEmpty(false, CertificateTypeList.Codes.Code1, ControllingMessageTypeList.Codes.NX101, ZString.Empty);
			AssertCheckTW1_CertificateType_JZ_InvoiceNumberEmpty(false, CertificateTypeList.Codes.Code18, ControllingMessageTypeList.Codes.X101, ZString.Empty);
			AssertCheckTW1_CertificateType_JZ_InvoiceNumberEmpty(false, CertificateTypeList.Codes.Code18, ControllingMessageTypeList.Codes.NX101, "TEST");
		}

		void AssertCheckTW1_CertificateType_JZ_InvoiceNumberEmpty(bool expectShowError, ZString certificateType, ZString messageType, ZString invoiceNumber)
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CusEntryInstruction;
			var invoiceHeader1 = declaration.Invoices.AddNew();
			var invoiceHeader2 = declaration.Invoices.AddNew();
			invoiceHeader1.JZ_InvoiceNumber = invoiceNumber;
			invoiceHeader2.JZ_InvoiceNumber = invoiceNumber;
			var messageHeader = entryInstruction.ControllingMessageHeaders.AddNew();
			messageHeader.TW1_ControllingMessageType = messageType;
			messageHeader.TW1_CertificateType = certificateType;
			var invoiceLine = (JobComInvoiceLine)invoiceHeader1.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			messageHeader.TW1_ControllingAgency = "XX";
			var link = invoiceLine.InvoiceLineLinkControllingMsgHeaders.Cast<InvoiceLineLinkControllingMsgHeader>().First(x => x.ControllingAgency == "XX");
			link.IsLinkedCMHeader = true;

			messageHeader.Validation.ValidateTW1_CertificateType();
			CombineAssertions(() =>
			{
				AssertEquals("header1 Invoice Number", expectShowError, invoiceHeader1.JZ_InvoiceNumberInfo.HasMessageError(MandatoryValidation.YouHaveNotEnteredMessage("Invoice No")));
				AssertEquals("header2 : no link to CMHeader", false, invoiceHeader2.JZ_InvoiceNumberInfo.HasMessageError(MandatoryValidation.YouHaveNotEnteredMessage("Invoice No")));
			});
		}

		public void TestCheckTW1_CertificateType_JZ_InvoiceDateLateThanSubmissionDate()
		{
			AssertCheckTW1_CertificateType_JZ_InvoiceDateLateThanSubmissionDate(true, CertificateTypeList.Codes.Code15, ControllingMessageTypeList.Codes.NX101, ZDateTime.BrettsBirthday, ZDateTime.BrettsBirthday.AddDays(-1));
			AssertCheckTW1_CertificateType_JZ_InvoiceDateLateThanSubmissionDate(false, CertificateTypeList.Codes.Code1, ControllingMessageTypeList.Codes.NX101, ZDateTime.BrettsBirthday, ZDateTime.BrettsBirthday.AddDays(-1));
			AssertCheckTW1_CertificateType_JZ_InvoiceDateLateThanSubmissionDate(false, CertificateTypeList.Codes.Code15, ControllingMessageTypeList.Codes.X101, ZDateTime.BrettsBirthday, ZDateTime.BrettsBirthday.AddDays(-1));
			AssertCheckTW1_CertificateType_JZ_InvoiceDateLateThanSubmissionDate(false, CertificateTypeList.Codes.Code15, ControllingMessageTypeList.Codes.NX101, ZDateTime.BrettsBirthday, ZDateTime.BrettsBirthday);
		}

		void AssertCheckTW1_CertificateType_JZ_InvoiceDateLateThanSubmissionDate(bool expectShowError, ZString certificateType, ZString messageType, ZDateTime invoiceDate, ZDateTime submissionDate)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_EntrySubmittedDate = submissionDate;
			var entryInstruction = declaration.CusEntryInstruction;
			var invoiceHeader1 = declaration.Invoices.AddNew();
			var invoiceHeader2 = declaration.Invoices.AddNew();
			invoiceHeader1.JZ_InvoiceDate = invoiceDate;
			invoiceHeader2.JZ_InvoiceDate = invoiceDate;
			var messageHeader = entryInstruction.ControllingMessageHeaders.AddNew();
			messageHeader.TW1_ControllingMessageType = messageType;
			messageHeader.TW1_CertificateType = certificateType;

			var invoiceLine = (JobComInvoiceLine)invoiceHeader1.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			messageHeader.TW1_ControllingAgency = "XX";
			var link = invoiceLine.InvoiceLineLinkControllingMsgHeaders.Cast<InvoiceLineLinkControllingMsgHeader>().First(x => x.ControllingAgency == "XX");
			link.IsLinkedCMHeader = true;

			messageHeader.Validation.ValidateTW1_CertificateType();
			CombineAssertions(() =>
			{
				AssertEquals("header1 Invoice Date late than submission date", expectShowError, invoiceHeader1.JZ_InvoiceDateInfo.HasMessageError("Invoice Date should before Submission Date"));
				AssertEquals("header2 : no link to CMHeader", false, invoiceHeader2.JZ_InvoiceDateInfo.HasMessageError("Invoice Date should before Submission Date"));
			});
		}

		public void TestLoadBeforeValationAll()
		{
			var invoiceHeader = Factory.New<JobComInvoiceHeader>();
			var supplierAddress = invoiceHeader.DocAddresses.AddNew();
			supplierAddress.E2_AddressType = AutoDocAddressTypes.Codes.SupplierDocumentaryAddress;
			supplierAddress.E2_AddressOverride = true;
			var supplierLocalAddress = invoiceHeader.DocAddresses.AddNew();
			supplierLocalAddress.E2_AddressType = AutoDocAddressTypes.Codes.SupplierTranslatedDocumentaryAddress;

			var buyerAddress = invoiceHeader.DocAddresses.AddNew();
			buyerAddress.E2_AddressType = AutoDocAddressTypes.Codes.BuyerDocumentaryAddress;
			buyerAddress.E2_AddressOverride = true;
			var buyerLocalAddress = invoiceHeader.DocAddresses.AddNew();
			buyerLocalAddress.E2_AddressType = AutoDocAddressTypes.Codes.BuyerTranslatedDocumentaryAddress;

			CombineAssertions("Pre req", () =>
			{
				AssertNull("supplierAddress", supplierAddress.OverrideRequirement);
				AssertNull("supplierLocalAddress", supplierLocalAddress.OverrideRequirement);
				AssertNull("buyerAddress", buyerAddress.OverrideRequirement);
				AssertNull("buyerLocalAddress", buyerLocalAddress.OverrideRequirement);
			});

			invoiceHeader.RunPreSaveValidation();

			CombineAssertions(() =>
			{
				AssertType<SupplierAddressRequirement>("Requirement of SupplierDocumentaryAddress", supplierAddress.OverrideRequirement);
				AssertEquals("SupplierDocumentaryAddress", supplierAddress, invoiceHeader.SupplierDocumentaryAddress);

				AssertType<SupplierLocalAddressRequirement>("Requirement of SupplierDocumentaryAddress.LocalAddress", supplierLocalAddress.OverrideRequirement);
				AssertEquals("SupplierDocumentaryAddress.LocalAddress", supplierLocalAddress, invoiceHeader.SupplierDocumentaryAddress.LocalAddress);

				AssertType<BuyerAddressRequirement>("Requirement of BuyerDocumentaryAddress", buyerAddress.OverrideRequirement);
				AssertEquals("BuyerDocumentaryAddress", buyerAddress, invoiceHeader.BuyerDocumentaryAddress);

				AssertType<BuyerLocalAddressRequirement>("Requirement of BuyerDocumentaryAddress.LocalAddress", buyerLocalAddress.OverrideRequirement);
				AssertEquals("BuyerDocumentaryAddress.LocalAddress", buyerLocalAddress, invoiceHeader.BuyerDocumentaryAddress.LocalAddress);
			});
		}
	}
}
