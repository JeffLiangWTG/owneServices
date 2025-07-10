using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.DataRegistry.Business.Testing
{
	sealed class NominatedMessageErrorValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckProperties()
		{
			var supervisorOverrideData = new SupervisorOverrideData();
			var nominatedMessageError = supervisorOverrideData.NominatedMessageErrors.AddNew();
			nominatedMessageError.Validation.ValidateFieldName();
			AssertHasErrorContaining(nominatedMessageError.FieldNameInfo, MandatoryValidation.MustBeEntered);
			nominatedMessageError.Validation.ValidateMessageErrorText();
			AssertHasErrorContaining(nominatedMessageError.MessageErrorTextInfo, MandatoryValidation.MustBeEntered);

			nominatedMessageError.FieldName = "US_ADDCaseNo";
			AssertNoErrorContaining(nominatedMessageError.FieldNameInfo, MandatoryValidation.MustBeEntered);
			nominatedMessageError.MessageErrorText = "This invoice line may be subject to ADD";
			AssertNoErrorContaining(nominatedMessageError.MessageErrorTextInfo, MandatoryValidation.MustBeEntered);
		}
	}
}
