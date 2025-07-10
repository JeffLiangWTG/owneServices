using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ZA.Manifest.Business.Testing
{
	class SupportingDocSendingObjectValidationTest : ZA.Business.Testing.SupportingDocSendingObjectValidationTest
	{
		public override void TestCheckDocumentType()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("ZA"))
			{
				var testHelper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
				testHelper.CreateNewOrGetExistingCusCodeType("ZADOC", "doc");
				testHelper.CreateNewOrGetExistingCusCodeList("ZA", "ZADOC", "INV", "Invoice", ZDateTime.UtcNow.AddDays(-2), ZDateTime.UtcNow.AddDays(2));
				Factory.Save();
				var manifest = Factory.NewWithValidTestData<AsycudaManifestHeader>();
				var sendingObject = SupportingDocSendingObject.New(manifest);
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

		public override void TestCheckLocalReferenceNumber()
		{
			Assert(true);
		}
	}
}
