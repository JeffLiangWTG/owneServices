using CargoWise.Types;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(NX201_01ImportMessageSendingObjectParentValidation))]
	sealed class NX201_01ImportMessageSendingObjectParentValidationTest : NX201_01MessageSendingObjectParentValidationAbstractTest
	{
		[ExpectNoExceptions]
		public void TestImportValidate()
		{
			var (declaration, invoiceHeader, invoiceLine, messageHeader, messageSendingObjectParent, messageSendingObject) = GetData();
			messageSendingObject.ShouldSend = true;
			messageHeader.TW1_BusinessType = ZString.Empty;

			messageSendingObject.Action = ZString.Empty;
			invoiceLine.AssignCMHeaderToInvoices(messageHeader);
			var parentValidator = messageSendingObjectParent.MessageSendingValidation;
			var errors = parentValidator.CheckBusinessObjectLevelValidation();
			CombineAssertions(() =>
			{
				AssertHasContentError("Validator should check for empty value of JobDeclarationForm > Inv. Lines > Main Details > Goods Origin", "Invoice Line  1: Goods Origin: You have not entered a Goods Origin.", errors);
				AssertHasContentError("Validator should check for empty value of JobDeclarationForm > Inv. Lines > Main Details > Unit Price", "Invoice Line  1: Please enter a 'Unit Price' greater than 0.", errors);
				AssertNoContentError("Validator should not check for empty value of JobDeclarationForm > Licensing > Processing Number", "Licensing : Processing Number is required when Action is '4' or '17'", errors);
			});

			invoiceLine.JI_CountryOfOrigin = "AA";
			invoiceLine.JI_EnteredUnitPrice = 1;
			messageHeader.ApplicantDocumentaryAddress.E2_AddressOverride = true;
			messageSendingObject.Action = NX201_01ActionCodeList.Codes._4;
			errors = parentValidator.CheckBusinessObjectLevelValidation();
			CombineAssertions(() =>
			{
				AssertHasContentError("Validator should check for if the value in list: JobDeclarationForm > Inv. Lines > Main Details > Goods Origin", "Invoice Line  1: Goods Origin: The code you have selected is not in the list.", errors);
				AssertHasContentError("Validator should check for empty value of JobDeclarationForm > Licensing > Processing Number", "Licensing : Processing Number is required when Action is '4' or '17'", errors);
				AssertNoContentError("Validator should not check for empty value of JobDeclarationForm > Inv. Lines > Main Details > Unit Price", "Invoice Line  1: Please enter a 'Unit Price' greater than 0.", errors);
			});

			invoiceLine.JI_CountryOfOrigin = "TW";
			messageSendingObject.Action = NX201_01ActionCodeList.Codes._17;
			errors = parentValidator.CheckBusinessObjectLevelValidation();
			CombineAssertions(() =>
			{
				AssertNoContentError("Validator should not check for empty value of JobDeclarationForm > Inv. Lines > Main Details > Goods Origin", "Invoice Line  1: Goods Origin: You have not entered a Goods Origin.", errors);
				AssertNoContentError("Validator should not check for if the value in list: JobDeclarationForm > Inv. Lines > Main Details > Goods Origin", "Invoice Line  1: Goods Origin: The code you have selected is not in the list.", errors);
				AssertHasContentError("Validator should check for empty value of JobDeclarationForm > Licensing > Processing Number", "Licensing : Processing Number is required when Action is '4' or '17'", errors);
			});

			messageSendingObject.Action = NX201_01ActionCodeList.Codes._5;
			errors = parentValidator.CheckBusinessObjectLevelValidation();
			AssertNoContentError("Validator should not check for empty value of JobDeclarationForm > Licensing > Processing Number", "Licensing : Processing Number is required when Action is '4' or '17'", errors);
		}

		protected override (JobDeclaration declaration, JobComInvoiceHeader invoiceHeader, JobComInvoiceLine invoiceLine, CusTWControllingMessageHeader messageHeader, LicensingMessageSendingObjectParent parent, LicensingMessageSendingObject sendingObject) GetData()
		{
			var data = SetupData();
			data.declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			return data;
		}
	}
}
