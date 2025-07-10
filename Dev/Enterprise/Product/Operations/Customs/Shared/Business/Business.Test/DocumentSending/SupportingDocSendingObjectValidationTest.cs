using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	public class SupportingDocSendingObjectValidationTest : TestCaseWithFactory
	{
		public virtual void TestCheckLocalReferenceNumber()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.MovementReferenceNumberSetter("123", ZDateTime.UtcNow);
			var sendingObject = SupportingDocSendingObject.New(declaration);

			CombineAssertions(() =>
			{
				sendingObject.LocalReferenceNumber = ZString.Empty;
				AssertHasError("LocalReferenceNumber is mandatory", sendingObject.LocalReferenceNumberInfo, MandatoryValidation.MustBeEnteredMessage(sendingObject.LocalReferenceNumberInfo.HumanReadableName));
				sendingObject.LocalReferenceNumber = "AAA";
				AssertNoError("LocalReferenceNumber is mandatory", sendingObject.LocalReferenceNumberInfo, MandatoryValidation.MustBeEnteredMessage(sendingObject.LocalReferenceNumberInfo.HumanReadableName));
				AssertHasErrorContaining(sendingObject.LocalReferenceNumberInfo, ListValidation.InvalidCodeError);
				sendingObject.LocalReferenceNumber = "123";
				AssertNoErrorContaining(sendingObject.LocalReferenceNumberInfo, ListValidation.InvalidCodeError);
			});
		}

		public virtual void TestCheckDocumentType()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("GB"))
			{
				var testHelper = new UniversalReferenceTestDataHelper(Factory);
				testHelper.CreateNewOrGetExistingCusCodeType("DOC44", "doc");
				testHelper.CreateNewOrGetExistingCusCodeList("Gb", "DOC44", "INV", "Invoice", ZDateTime.UtcNow.AddDays(-2), ZDateTime.UtcNow.AddDays(2));
				Factory.Save();

				var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
				var sendingObject = new SupportingDocSendingObjectForTest(declaration);

				CombineAssertions(() =>
				{
					sendingObject.Validation.ValidateDocumentType();
					AssertHasError("DocumentType is mandatory", sendingObject.DocumentTypeInfo, MandatoryValidation.MustBeEnteredMessage(sendingObject.DocumentTypeInfo.HumanReadableName));
					sendingObject.DocumentType = "AAA";
					AssertNoError("DocumentType is mandatory", sendingObject.DocumentTypeInfo, MandatoryValidation.MustBeEnteredMessage(sendingObject.DocumentTypeInfo.HumanReadableName));
					AssertHasErrorContaining(sendingObject.DocumentTypeInfo, ListValidation.InvalidCodeError);
					sendingObject.DocumentType = "INV";
					AssertNoErrorContaining(sendingObject.DocumentTypeInfo, ListValidation.InvalidCodeError);
				});
			}
		}

		public virtual void TestCheckEDoc()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			var eDoc = declaration.DocManagerInfo.AddFileOrDocument(new byte[1], "Invoice.pdf", "CIV");
			var sendingObject = SupportingDocSendingObject.New(declaration);

			var declaration2 = Factory.NewWithValidTestData<BaseJobDeclaration>();
			var eDoc2 = declaration2.DocManagerInfo.AddFileOrDocument(new byte[1], "Invoice.pdf", "CIV");

			CombineAssertions(() =>
			{
				sendingObject.Validation.ValidateEDoc();
				AssertHasError("EDoc is mandatory", sendingObject.EDocInfo, MandatoryValidation.MustBeEnteredMessage(sendingObject.EDocInfo.HumanReadableName));

				sendingObject.EDoc = eDoc2.UniqueKey;
				AssertNoError("EDoc is mandatory", sendingObject.EDocInfo, MandatoryValidation.MustBeEnteredMessage(sendingObject.EDocInfo.HumanReadableName));
				AssertHasErrorContaining(sendingObject.EDocInfo, ListValidation.InvalidCodeError);

				sendingObject.EDoc = eDoc.UniqueKey;
				AssertNoErrorContaining(sendingObject.EDocInfo, ListValidation.InvalidCodeError);
				AssertNoNotifications(sendingObject.EDocInfo);
			});
		}

		[ExpectNoExceptions]
		public virtual void TestCheckEDocFileSizeInMB()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			var sendingObject = SupportingDocSendingObject.New(declaration);
			if (sendingObject.ShouldCheckSizeInEdocField)
			{
				var eDocNormal = declaration.DocManagerInfo.AddFileOrDocument(new byte[sendingObject.Validation.MaxEDocFileSizeInBytes], "Invoice.pdf", "CIV");
				sendingObject.EDoc = eDocNormal.UniqueKey;
				sendingObject.Validation.ValidateEDocFileSizeInMB();
				AssertNoError(sendingObject.EDocFileSizeInMBInfo, sendingObject.Validation.EDocTooLargeError);

				var eDocTooLarge = declaration.DocManagerInfo.AddFileOrDocument(new byte[sendingObject.Validation.MaxEDocFileSizeInBytes + 1], "Invoice (too large).pdf", "CIV");
				sendingObject.EDoc = eDocTooLarge.UniqueKey;
				sendingObject.Validation.ValidateEDocFileSizeInMB();
				AssertHasError(sendingObject.EDocFileSizeInMBInfo, sendingObject.Validation.EDocTooLargeError);
			}
		}
	}
}
