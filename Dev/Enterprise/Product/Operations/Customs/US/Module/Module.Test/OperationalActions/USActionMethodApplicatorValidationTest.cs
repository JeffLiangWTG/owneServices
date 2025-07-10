using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;

namespace Enterprise.Customs.US.Module.OperationalActions.Testing
{
	sealed class USActionMethodApplicatorValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateSecurity()
		{
			Env.Security.CustomsDeclarationSendWithMessageErrors.IsAllowed = true;
			var applicator = new SendEntrySummaryActionMethodApplicator(new BusinessObjectFactory());
			applicator.SendWithMessageErrors = true;
			var errorText = string.Format(USActionMethodApplicatorValidation.SendWithMessageErrorsSecurity, applicator.MessageDescription);
			AssertNoError(applicator.SendWithMessageErrorsInfo, errorText);
			Env.Security.CustomsDeclarationSendWithMessageErrors.IsAllowed = false;
			applicator.Validation.ValidateSendWithMessageErrors();
			AssertHasError(applicator.SendWithMessageErrorsInfo, errorText);
			applicator.SendWithMessageErrors = false;
			AssertNoError(applicator.SendWithMessageErrorsInfo, errorText);
		}
	}
}
