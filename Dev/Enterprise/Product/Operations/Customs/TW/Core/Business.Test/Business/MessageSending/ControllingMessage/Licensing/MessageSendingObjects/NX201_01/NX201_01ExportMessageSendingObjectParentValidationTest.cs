using CargoWise.Types;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(NX201_01ExportMessageSendingObjectParentValidation))]
	sealed class NX201_01ExportMessageSendingObjectParentValidationTest : NX201_01MessageSendingObjectParentValidationAbstractTest
	{
		[ExpectNoExceptions]
		public void TestExportValidate()
		{
			(var declaration, var invoiceHeader, var invoiceLine, var messageHeader, var messageSendingObjectParent, var messageSendingObject) = GetData();
			messageSendingObject.ShouldSend = true;
			messageHeader.TW1_BusinessType = ZString.Empty;

			messageSendingObject.Action = ZString.Empty;
			invoiceLine.PermitCusSupportingCollection[0].CSI_LineNo = 0;
			var parentValidator = messageSendingObjectParent.MessageSendingValidation;
			var errors = parentValidator.CheckBusinessObjectLevelValidation();
			CombineAssertions(() =>
			{
				AssertHasContentError("Validator should check for empty value of JobDeclarationForm > Declaration > Shipment Details > Port Of Final Destination", "Declaration: You have not entered a Final Destination.", errors);
				AssertHasContentError("Validator should check for empty value of JobDeclarationForm > Declaration > Importer > Address > Name", "Declaration: You have not entered an Importer Name.", errors);
				AssertHasContentError("Validator should check for empty value of JobDeclarationForm > Declaration > Importer > Address > Country/Region Code", "Declaration: You have not entered an Importer Documentary Address: Country/Region Code.", errors);
				AssertHasContentError("Validator should check for empty value of JobDeclarationForm > Licensing > Organization > Applicant > Applicant > Address", "Licensing : You have not entered an Applicant Address.", errors);
			});

			var importerDocumentaryAddress = declaration.ImporterDocumentaryAddress;
			importerDocumentaryAddress.E2_AddressOverride = true;
			importerDocumentaryAddress.E2_RN_NKCountryCode = "A";
			declaration.JE_RL_NKFinalDestination = "AA";

			errors = parentValidator.CheckBusinessObjectLevelValidation();
			CombineAssertions(() =>
			{
				AssertHasContentError("Validator should check for empty value of JobDeclarationForm > Declaration > Shipment Details > Port Of Final Destination", "Declaration: Port Of Final Destination: The code you have selected is not in the list.", errors);
				AssertHasContentError("Validator should check for empty value of JobDeclarationForm > Declaration > Importer > Address > Country/Region Code", "Declaration: Importer Documentary Address: Country/Region Code: The code you have selected is not in the list.", errors);
			});

			importerDocumentaryAddress.E2_CompanyName = "Company Name";
			importerDocumentaryAddress.E2_RN_NKCountryCode = "TW";
			declaration.JE_RL_NKFinalDestination = "CNSHA";
			messageHeader.ApplicantDocumentaryAddress.E2_AddressOverride = true;
			messageHeader.ApplicantDocumentaryAddress.E2_Address1 = "Address1";
			errors = parentValidator.CheckBusinessObjectLevelValidation();
			CombineAssertions(() =>
			{
				AssertNoContentError("Validator should not check for empty value of JobDeclarationForm > Declaration > Shipment Details > Port Of Final Destination", "Declaration: You have not entered a Final Destination.", errors);
				AssertNoContentError("Validator should not check for empty value of JobDeclarationForm > Declaration > Shipment Details > Port Of Final Destination", "Declaration: Port Of Final Destination: The code you have selected is not in the list.", errors);
				AssertNoContentError("Validator should not check for empty value of JobDeclarationForm > Declaration > Importer > Address > Name", "Declaration: You have not entered an Importer Name.", errors);
				AssertNoContentError("Validator should not check for empty value of JobDeclarationForm > Declaration > Importer > Address > Country/Region Code", "Declaration: You have not entered an Importer Documentary Address: Country/Region Code.", errors);
				AssertNoContentError("Validator should not check for empty value of JobDeclarationForm > Declaration > Importer > Address > Country/Region Code", "Declaration: Importer Documentary Address: Country/Region Code: The code you have selected is not in the list.", errors);
				AssertNoContentError("Validator should not check for empty value of JobDeclarationForm > Licensing > Organization > Applicant > Applicant > Address", "Licensing : You have not entered an Applicant Address.", errors);
			});
		}

		protected override (JobDeclaration declaration, JobComInvoiceHeader invoiceHeader, JobComInvoiceLine invoiceLine, CusTWControllingMessageHeader messageHeader, LicensingMessageSendingObjectParent parent, LicensingMessageSendingObject sendingObject) GetData()
		{
			var data = SetupData();
			data.declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			return data;
		}
	}
}
