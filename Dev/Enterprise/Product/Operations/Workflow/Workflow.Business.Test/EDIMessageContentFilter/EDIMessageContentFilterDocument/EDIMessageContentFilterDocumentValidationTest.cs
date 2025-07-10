using CargoWise.EntityFramework.Testing;

namespace Enterprise.Workflow.Business.Test
{
	class EDIMessageContentFilterDocumentValidationTest : BusinessObjectValidationTestCase
	{
		public void TestDocumentTypeValidation()
		{
			var document = Factory.New<EDIMessageContentFilter>().UniversalShipment.Documents.AddNew();
			document.DocumentType = "zzz";

			AssertListValidationInvalidCodeError(document.DocumentTypeInfo, true);

			document.DocumentType = Core.Constants.RefDocTypes.CartageAdvice;
			AssertNoErrors(document.DocumentTypeInfo);
		}
	}
}
