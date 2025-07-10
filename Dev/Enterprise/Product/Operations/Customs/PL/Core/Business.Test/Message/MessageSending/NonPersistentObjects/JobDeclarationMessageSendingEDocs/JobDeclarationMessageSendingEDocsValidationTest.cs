using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.PL.Business.Testing;

sealed class JobDeclarationMessageSendingEDocsValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckEDoc()
	{
		var emptyMessage = "Please enter a Document.";
		var wrongCodeMessage = "Enter a valid Document.";
		var testingObject = GetJobDeclarationMessageSendingEDocs();
		CombineAssertions(() =>
		{
			testingObject.Validation.ValidateEDoc();
			AssertHasError("Is empty", testingObject.EDocInfo, emptyMessage);
			testingObject.EDoc = new ZGuid("6B2315CD-930E-48A0-A85E-F30253B069F4");
			testingObject.Validation.ValidateEDoc();
			AssertNoError("No empty message", testingObject.EDocInfo, emptyMessage);
			AssertHasError("Wrong code", testingObject.EDocInfo, wrongCodeMessage);
			testingObject.Validation.ValidateEDoc();
			testingObject.EDoc = new ZGuid("4B6A943B-50BE-44CE-9E7F-8882D3CFF8B3");
			AssertNoError("Valid code", testingObject.EDocInfo, emptyMessage);
		});
	}

	public void TestCheckAdditionalInformation()
	{
		var message = "The Additional Info or Supporting document code is required.";
		var testingObject = GetJobDeclarationMessageSendingEDocs();
		CombineAssertions(() =>
		{
			testingObject.Validation.ValidateAdditionalInformation();
			AssertHasMessageError("Is empty", testingObject.AdditionalInformationInfo, message);
			testingObject.AdditionalInformation = "NotEmpty";
			testingObject.Validation.ValidateAdditionalInformation();
			AssertNoMessageError("No empty AdditionalInformation", testingObject.AdditionalInformationInfo, message);
			testingObject.AdditionalInformation = string.Empty;
			testingObject.SupportingDocument = "NotEmpty";
			testingObject.Validation.ValidateAdditionalInformation();
			AssertNoMessageError("No empty SupportingDocuments", testingObject.AdditionalInformationInfo, message);
		});
	}

	public void TestCheckDocumentDescription()
	{
		var message = "You have not entered a Document Description.";
		var testingObject = GetJobDeclarationMessageSendingEDocs();
		CombineAssertions(() =>
		{
			testingObject.Validation.ValidateDocumentDescription();
			AssertHasMessageError("Is empty", testingObject.DocumentDescriptionInfo, message);
			testingObject.DocumentDescription = "NotEmpty";
			testingObject.Validation.ValidateDocumentDescription();
			AssertNoMessageError("No empty", testingObject.DocumentDescriptionInfo, message);
		});
	}

	public void TestCheckSupportingDocument()
	{
		var message = "The Additional Info or Supporting document code is required.";
		var testingObject = GetJobDeclarationMessageSendingEDocs();
		CombineAssertions(() =>
		{
			testingObject.Validation.ValidateSupportingDocument();
			AssertHasMessageError("Is empty", testingObject.SupportingDocumentInfo, message);
			testingObject.AdditionalInformation = "NotEmpty";
			testingObject.Validation.ValidateSupportingDocument();
			AssertNoMessageError("No empty AdditionalInformation", testingObject.SupportingDocumentInfo, message);
			testingObject.AdditionalInformation = string.Empty;
			testingObject.SupportingDocument = "NotEmpty";
			testingObject.Validation.ValidateSupportingDocument();
			AssertNoMessageError("No empty SupportingDocuments", testingObject.SupportingDocumentInfo, message);
		});
	}

	public void TestCheckCrossValidation()
	{
		var message = "The Additional Info or Supporting document code is required.";
		var testingObject = GetJobDeclarationMessageSendingEDocs();
		CombineAssertions(() =>
		{
			testingObject.Validation.ValidateAdditionalInformation();
			testingObject.Validation.ValidateSupportingDocument();
			AssertHasMessageError("AdditionalInformation is empty", testingObject.AdditionalInformationInfo, message);
			AssertHasMessageError("SupportingDocument is empty", testingObject.SupportingDocumentInfo, message);

			testingObject.SupportingDocument = "NotEmpty";
			AssertNoMessageError("AdditionalInformation is empty but SupportingDocument set", testingObject.AdditionalInformationInfo, message);
			AssertNoMessageError("SupportingDocument set, AdditionalInformation not empty", testingObject.SupportingDocumentInfo, message);

			testingObject.SupportingDocument = string.Empty;
			AssertHasMessageError("SupportingDocument updated - AdditionalInformation is empty", testingObject.AdditionalInformationInfo, message);
			AssertHasMessageError("SupportingDocument updated - SupportingDocument is empty", testingObject.SupportingDocumentInfo, message);

			testingObject.AdditionalInformation = "NotEmpty";
			AssertNoMessageError("AdditionalInformation set", testingObject.AdditionalInformationInfo, message);
			AssertNoMessageError("SupportingDocument is empty but AdditionalInformation set", testingObject.SupportingDocumentInfo, message);

			testingObject.AdditionalInformation = string.Empty;
			AssertHasMessageError("AdditionalInformation updated - AdditionalInformation is empty", testingObject.AdditionalInformationInfo, message);
			AssertHasMessageError("AdditionalInformation updated - SupportingDocument is empty", testingObject.SupportingDocumentInfo, message);
		});
	}

	JobDeclarationMessageSendingEDocs GetJobDeclarationMessageSendingEDocs() => eDocs;

	protected override void SetUp()
	{
		base.SetUp();
		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		var sendingObjectParent = new BaseMessageSendingObjectParent(declaration);
		var storageDocs = new CodeDescriptionPairList();
		storageDocs.AddPair("4B6A943B-50BE-44CE-9E7F-8882D3CFF8B3", "Code1", "Description1");
		var collection = new JobDeclarationMessageSendingEDocsCollection(Factory, () => storageDocs, sendingObjectParent, () => new Dictionary<ZGuid, ZString>());
		eDocs = collection.AddNew();
	}

	JobDeclarationMessageSendingEDocs eDocs;
}
