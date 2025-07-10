using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestsSubclassesOf(typeof(NX201_01MessageSendingObjectParentValidation))]
	abstract class NX201_01MessageSendingObjectParentValidationAbstractTest : LicensingMessageSendingObjectParentValidationTest<NX201_01MessageSendingObjectParentValidation, NX201_01MessageSendingObject>
	{
		[ExpectNoExceptions]
		public void TestValidate()
		{
			var (declaration, invoiceHeader, invoiceLine, messageHeader, messageSendingObjectParent, messageSendingObject) = GetData();
			messageSendingObject.ShouldSend = true;
			messageHeader.TW1_BusinessType = ZString.Empty;
			messageSendingObject.Action = ZString.Empty;
			declaration.JE_OA_DeclarantAddress = ZGuid.Empty;
			invoiceLine.PermitCusSupportingCollection[0].CSI_LineNo = 0;
			var parentValidator = messageSendingObjectParent.MessageSendingValidation;
			var errors = parentValidator.CheckBusinessObjectLevelValidation();
			CombineAssertions(() =>
			{
				AssertNoContentError("Validator should not check for empty value of JobDeclarationForm > Inv. Headers > Commercial Invoice Details > Incoterm", "Invoice Header : Please enter an Incoterm.", errors);
				AssertNoContentError("Validator should not check for empty value of JobDeclarationForm > Inv. Headers > Commercial Invoice Details > Inv. Total Amount （JobComInvoiceHeader.JZ_RX_NKInvoice_Currency）", "Invoice Header : Please enter a Currency.", errors);
				AssertNoContentError("Validator should not check for empty value of JobDeclarationForm > Inv. Lines > Main Details > English Description", "Invoice Line  1: You have not entered an English Name.", errors);
				AssertNoContentError("Validator should not check for empty value of JobDeclarationForm > Inv. Lines > Main Details > Tariff", "Invoice Line  1: Tariff may not be empty.", errors);
				AssertNoContentError("Validator should not check for empty value of JobDeclarationForm > Inv. Lines > Main Details > Quantity", "Invoice Line  1: Please enter a 'Quantity' greater than 0.", errors);
				AssertNoContentError("Validator should not check for empty value of JobDeclarationForm > Inv. Lines > Main Details > Quantity (JobComInvoiceLine.JI_InvoiceUQ)", "Invoice Line  1: You have not entered a UQ.", errors);
				AssertNoContentError("Validator should not check for empty value of JobDeclarationForm > Inv. Lines > Main Details > Price", "Invoice Line  1: Please enter a 'Price' greater than 0.", errors);
				AssertNoContentError("Validator should not check for Local Company Name value of JobDeclarationForm > Organizations > Declaration", "Declaration: You have not entered a Declarant Local Company Name.", errors);
				AssertNoContentError("Validator should not check for Local Address value of JobDeclarationForm > Organizations > Declaration", "Declaration: You have not entered a Declarant Local Address.", errors);
				AssertNoContentError("Validator should not check for telephone number value of JobDeclarationForm > Organizations > Declaration", "Declaration: You have not entered a Declarant Local Company Name.", errors);
				AssertNoContentError("Validator should not check for Contact Name value of JobDeclarationForm > Organizations > Declaration", "Declaration: You have not entered a Declarant Local Address.", errors);
				AssertNoContentError("Validator should check for empty value of JobDeclarationForm > Licensing > Organization > Applicant > Applicant > Local Address", "Licensing : You have not entered an Applicant Local Address Information.", errors);
				AssertHasContentError("Validator should check for empty value of JobDeclarationForm > Licensing > Processing Unit", "Licensing : You have not entered a Processing Unit.", errors);
				AssertHasContentError("Validator should check for empty value of JobDeclarationForm > Licensing > Business Type", "Licensing : You have not entered a Business Type.", errors);
				AssertHasContentError("Validator should check for empty value of JobDeclarationForm > Licensing > Organization > Applicant > Applicant > Code > ID", "Licensing : You have not entered an Applicant ID.", errors);
				AssertHasContentError("Validator should check for empty value of JobDeclarationForm > Licensing > Organization > Applicant > Applicant > Contact > Telephone Number", "Licensing : You have not entered an Applicant Telephone Number.", errors);
				AssertHasContentError("Validator should check for empty value of JobDeclarationForm > Licensing > Organization > Applicant > Applicant > Contact > Email Address", "Licensing : You have not entered an Applicant Email Address.", errors);
				AssertHasContentError("Validator should check for empty value of JobDeclarationForm > Licensing > Organization > Applicant > Applicant > Contact > Ct. Name", "Licensing : You have not entered an Applicant Contact Name.", errors);
				AssertHasContentError("Validator should check for empty value of NXMDocumentForm > Customized Message Box Form > Messages to be sent > Action", "Licensing : You have not entered an Action.", errors);
			});

			var declarantOrg = Factory.New<OrgHeader>();
			var declarantAddress = declarantOrg.Addresses.AddNew();
			declaration.JE_OA_DeclarantAddress = declarantAddress.PK;
			invoiceLine.AssignCMHeaderToInvoices(messageHeader);
			errors = parentValidator.CheckBusinessObjectLevelValidation();
			CombineAssertions(() =>
			{
				AssertHasContentError("Validator should check for empty value of JobDeclarationForm > Inv. Headers > Commercial Invoice Details > Incoterm", "Invoice Header : Please enter an Incoterm.", errors);
				AssertHasContentError("Validator should check for empty value of JobDeclarationForm > Inv. Headers > Commercial Invoice Details > Inv. Total Amount （JobComInvoiceHeader.JZ_RX_NKInvoice_Currency）", "Invoice Header : Please enter a Currency.", errors);
				AssertHasContentError("Validator should check for empty value of JobDeclarationForm > Inv. Lines > Main Details > English Description", "Invoice Line  1: You have not entered an English Name.", errors);
				AssertHasContentError("Validator should check for empty value of JobDeclarationForm > Inv. Lines > Main Details > Tariff", "Invoice Line  1: Tariff may not be empty.", errors);
				AssertHasContentError("Validator should check for empty value of JobDeclarationForm > Inv. Lines > Main Details > Quantity", "Invoice Line  1: Please enter a 'Quantity' greater than 0.", errors);
				AssertHasContentError("Validator should check for empty value of JobDeclarationForm > Inv. Lines > Main Details > Quantity (JobComInvoiceLine.JI_InvoiceUQ)", "Invoice Line  1: You have not entered a UQ.", errors);
				AssertHasContentError("Validator should check for empty value of JobDeclarationForm > Inv. Lines > Main Details > Price", "Invoice Line  1: Please enter a 'Price' greater than 0.", errors);
				AssertHasContentError("Validator should check for Local Company Name value of JobDeclarationForm > Organizations > Declaration", "Declaration: You have not entered a Declarant Local Company Name.", errors);
				AssertHasContentError("Validator should check for Local Address value of JobDeclarationForm > Organizations > Declaration", "Declaration: You have not entered a Declarant Local Address.", errors);
				AssertHasContentError("Validator should check for telephone number value of JobDeclarationForm > Organizations > Declaration", "Declaration: You have not entered a Declarant Telephone Number.", errors);
				AssertHasContentError("Validator should check for Contact Name value of JobDeclarationForm > Organizations > Declaration", "Declaration: You have not entered a Declarant Contact Name.", errors);
			});

			messageHeader.TW1_BusinessType = "2";
			var applicantDocumentaryAddress = messageHeader.ApplicantDocumentaryAddress;
			applicantDocumentaryAddress.E2_AddressOverride = true;
			messageSendingObject.Action = NX201_01ActionCodeList.Codes._4;
			var declarantLocalAddress = declarantAddress.TranslatedAddresses.AddNew();
			declarantLocalAddress.Language = Core.SharedConstants.Languages.ChineseTraditional;
			declarantLocalAddress.Address1 = "TestAddress";
			declarantLocalAddress.CompanyName = "TestCompany";
			declarantAddress.OA_Phone = "11111111111";
			var contact = declarantOrg.ContactsActive.AddNew();
			contact.OC_ContactName = "AAA";
			errors = parentValidator.CheckBusinessObjectLevelValidation();
			CombineAssertions(() =>
			{
				AssertHasContentError("Validator should check for empty value of JobDeclarationForm > Shipment Details > Port Of Origin", "Declaration: You have not entered a Port of Origin.", errors);
				AssertHasContentError("Validator should check for empty value of JobDeclarationForm > Licensing > Organization > Applicant > Applicant > Local Address > Name", "Licensing : You have not entered an Applicant Local Name.", errors);
				AssertHasContentError("Validator should check for empty value of JobDeclarationForm > Licensing > Organization > Applicant > Applicant > Local Address > Address", "Licensing : You have not entered an Applicant Local Address.", errors);
				AssertNoContentError("Validator should not check for Local Company Name value of JobDeclarationForm > Organizations > Declaration", "Declaration: You have not entered a Declarant Local Company Name.", errors);
				AssertNoContentError("Validator should not check for Local Address value of JobDeclarationForm > Organizations > Declaration", "Declaration: You have not entered a Declarant Local Address.", errors);
				AssertNoContentError("Validator should not check for telephone number value of JobDeclarationForm > Organizations > Declaration", "Declaration: You have not entered a Declarant Telephone Number.", errors);
				AssertNoContentError("Validator should not check for Contact Name value of JobDeclarationForm > Organizations > Declaration", "Declaration: You have not entered a Declarant Contact Name.", errors);
			});

			invoiceHeader.JZ_RX_NKInvoice_Currency = "UNK";
			invoiceHeader.JZ_IncoTerm = "UNK";
			invoiceLine.JI_CountryOfOrigin = "AA";
			invoiceLine.JI_Tariff = "20064000001";
			invoiceLine.JI_InvoiceUQ = "UNK";
			messageHeader.TW1_ProcessingUnit = "UNP";
			messageHeader.TW1_BusinessType = "UNB";
			messageSendingObject.Action = "BB";
			errors = parentValidator.CheckBusinessObjectLevelValidation();
			CombineAssertions(() =>
			{
				AssertHasContentError("Validator should check for if the value in list: JobDeclarationForm > Inv. Headers > Commercial Invoice Details > Inv. Total Amount （JobComInvoiceHeader.JZ_RX_NKInvoice_Currency）", "Invoice Header : Enter a valid Curr..", errors);
				AssertHasContentError("Validator should check for if the value in list: JobDeclarationForm > Inv. Headers > Commercial Invoice Details > Incoterm", "Invoice Header : Incoterm: The code you have selected is not in the list.", errors);
				AssertHasContentError("Validator should check for if the value in list: JobDeclarationForm > Inv. Lines > Main Details > Tariff", "Invoice Line  1: The Tariff Code entered is not valid for the current context.", errors);
				AssertHasContentError("Validator should check for if the value in list: JobDeclarationForm > Inv. Lines > Main Details > Quantity (JobComInvoiceLine.JI_InvoiceUQ)", "Invoice Line  1: UQ: The code you have selected is not in the list.", errors);
				AssertHasContentError("Validator should check for if the value in list: JobDeclarationForm > Licensing > Processing Unit", "Licensing : Processing Unit: The code you have selected is not in the list.", errors);
				AssertHasContentError("Validator should check for if the value in list: JobDeclarationForm > Licensing > Business Type", "Licensing : Business Type: The code you have selected is not in the list.", errors);
				AssertHasContentError("Validator should check for if the value in list: NXMDocumentForm > Customized Message Box Form > Messages to be sent > Action", "Licensing : Action: The code you have selected is not in the list.", errors);
			});

			invoiceHeader.JZ_IncoTerm = "FOB";
			invoiceHeader.JZ_RX_NKInvoice_Currency = "TWD";
			invoiceLine.JI_CountryOfOrigin = "TW";
			invoiceLine.JI_Description = "Description";
			invoiceLine.JI_Tariff = "10064000004";
			invoiceLine.JI_InvoiceQuantity = 1;
			invoiceLine.JI_InvoiceUQ = "AMP";
			invoiceLine.JI_EnteredUnitPrice = 1;
			invoiceLine.JI_LinePrice = 1;
			messageHeader.TW1_ProcessingUnit = "10";
			messageHeader.TW1_BusinessType = "0";
			messageSendingObject.Action = NX201_01ActionCodeList.Codes._17;
			messageHeader.ProcessingNumber = "987654";
			var importerDocumentaryAddress = declaration.ImporterDocumentaryAddress;
			importerDocumentaryAddress.E2_AddressOverride = true;
			importerDocumentaryAddress.E2_CompanyName = "Company Name";
			importerDocumentaryAddress.E2_RN_NKCountryCode = "TW";
			declaration.JE_RL_NKFinalDestination = "CNSHA";
			applicantDocumentaryAddress.E2_Address1 = "Address1";
			applicantDocumentaryAddress.IDCode = "987654";
			applicantDocumentaryAddress.E2_Contact = "Contact";
			applicantDocumentaryAddress.E2_Email = "qwe@asd.zxc";
			applicantDocumentaryAddress.E2_Phone = "1234567";
			applicantDocumentaryAddress.LocalAddress.CompanyName = "Company Name";
			applicantDocumentaryAddress.LocalAddress.Address1 = "Address 1";
			declaration.JE_OA_DeclarantAddress = ZGuid.Empty;
			errors = parentValidator.CheckBusinessObjectLevelValidation();
			NUnit.Framework.Assert.That(errors.Count, NUnit.Framework.Is.EqualTo(0), "There should be not have any error");
		}

		[ExpectNoExceptions]
		public void TestApplicantLocalAddressValidateWhenNonOverride()
		{
			var (_, _, _, messageHeader, messageSendingObjectParent, messageSendingObject) = GetData();
			messageSendingObject.ShouldSend = true;
			var declarantOrg = Factory.New<OrgHeader>();
			var declarantAddress = declarantOrg.MainAddress;
			var applicantDocumentaryAddress = messageHeader.ApplicantDocumentaryAddress;
			applicantDocumentaryAddress.OrganisationPK = declarantOrg.PK;
			applicantDocumentaryAddress.E2_OA_Address = declarantAddress.PK;
			var parentValidator = messageSendingObjectParent.MessageSendingValidation;
			var errors = parentValidator.CheckBusinessObjectLevelValidation();
			AssertNoContentError("Validator should check for empty value of JobDeclarationForm > Licensing > Organization > Applicant > Applicant > Local Address", "Licensing : You have not entered an Applicant Local Address Information.", errors);
		}

		[ExpectNoExceptions]
		protected void AssertHasContentError(string message, string expected, MessageSendingNotificationCollection errors)
		{
			NUnit.Framework.Assert.That(errors.Select(e => e.Message), NUnit.Framework.Has.Some.EqualTo(expected).Using(CustomComparers.TypeComparison), message);
		}

		[ExpectNoExceptions]
		protected void AssertNoContentError(string message, string expected, MessageSendingNotificationCollection errors)
		{
			NUnit.Framework.Assert.That(errors.Select(e => e.Message), NUnit.Framework.Has.None.EqualTo(expected).Using(CustomComparers.TypeComparison), message);
		}

		protected abstract (JobDeclaration declaration, JobComInvoiceHeader invoiceHeader, JobComInvoiceLine invoiceLine, CusTWControllingMessageHeader messageHeader, LicensingMessageSendingObjectParent parent, LicensingMessageSendingObject sendingObject) GetData();

		protected override string MessageType => ControllingMessageTypeList.Codes.NX201_01;
	}
}
