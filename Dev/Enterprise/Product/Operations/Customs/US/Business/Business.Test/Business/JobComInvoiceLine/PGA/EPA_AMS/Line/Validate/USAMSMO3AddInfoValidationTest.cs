using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	internal class USAMSMO3AddInfoValidationTest : BusinessObjectValidationTestCase
	{
		public void TestUS_AuthorizationNumber()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceLine = declaration.InvoiceLines.AddNew();
			var testAMS = invoiceLine.AMSLines.AddNew();
			testAMS.US_Program = AMSProgramList.Codes.MO3;
			var mo3Line = testAMS.AMSLines.AddNew();
			mo3Line.US_AuthorizationNumber = "39323";
			AssertNoMessageErrorContaining(mo3Line.US_AuthorizationNumberInfo, MandatoryValidation.YouHaveNotEntered);

			mo3Line.US_AuthorizationNumber = "";
			AssertHasMessageErrorContaining(mo3Line.US_AuthorizationNumberInfo, MandatoryValidation.YouHaveNotEntered);

			var mo3LineForProduct = testAMS.AMSLines.AddNew();
			mo3LineForProduct.US_AuthorizationNumber = "";
			AssertHasMessageErrorContaining(mo3LineForProduct.US_AuthorizationNumberInfo, "You have not entered a value.");

			mo3Line.US_AuthorizationNumber = "393";
			AssertHasMessageError(mo3Line.US_AuthorizationNumberInfo, USAMSMO3AddInfoValidation.AuthorizationNumberInValid);

			mo3Line.US_AuthorizationNumber = "3931A";
			AssertHasMessageError(mo3Line.US_AuthorizationNumberInfo, USAMSMO3AddInfoValidation.AuthorizationNumberInValid);

			mo3Line.US_AuthorizationNumber = "39312VCFDA12";
			AssertNoMessageError(mo3Line.US_AuthorizationNumberInfo, USAMSMO3AddInfoValidation.AuthorizationNumberInValid);
		}
	}
}
