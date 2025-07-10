using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Module.OperationalActions.Testing
{
	sealed class SendEntrySummaryActionMethodApplicatorValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateContactName()
		{
			var applicator = new SendEntrySummaryActionMethodApplicator(new BusinessObjectFactory());
			applicator.SendWithMessageErrors = true;
			applicator.ContactName = "";
			applicator.Validation.ValidateAll();
			AssertHasMessageErrors(applicator.ContactNameInfo);
			applicator.ContactName = "MPX";
			AssertNoMessageErrors(applicator.ContactNameInfo);
		}

		public void TestValidateContactPhone()
		{
			var applicator = new SendEntrySummaryActionMethodApplicator(new BusinessObjectFactory());
			applicator.SendWithMessageErrors = true;
			applicator.ContactPhone = "";
			applicator.Validation.ValidateAll();
			AssertHasMessageErrors(applicator.ContactPhoneInfo);
			applicator.ContactPhone = "1234ABCD";
			AssertHasMessageError(applicator.ContactPhoneInfo, SendEntrySummaryActionMethodApplicatorValidation.InvalidContactPhoneMessage);
			applicator.ContactPhone = "1234567980";
			AssertNoMessageErrors(applicator.ContactPhoneInfo);
		}
	}
}
