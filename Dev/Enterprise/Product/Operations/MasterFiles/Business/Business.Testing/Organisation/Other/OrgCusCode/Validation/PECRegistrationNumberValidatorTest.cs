using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class PECRegistrationNumberValidatorTest : TestCaseWithDummy
	{
		public void TestValidateEntered()
		{
			using (Dummy.SuspendValidationTesting())
			{
				AssertEquals(false, Dummy.HasErrors);

				Dummy.Z0_Description = "rarar";
				PECRegistrationNumberValidator.ValidatePECRegistrationNumber(Dummy.Z0_DescriptionInfo);
				AssertEquals("random string", true, Dummy.HasErrors);

				Dummy.Z0_Description = "alex@edi.com.au";
				PECRegistrationNumberValidator.ValidatePECRegistrationNumber(Dummy.Z0_DescriptionInfo);
				AssertEquals("correct email", false, Dummy.HasErrors);

				Dummy.Z0_Description = "alex@edi,com.au";
				PECRegistrationNumberValidator.ValidatePECRegistrationNumber(Dummy.Z0_DescriptionInfo);
				AssertEquals("comman instead of dot", true, Dummy.HasErrors);

				Dummy.Z0_Description = "alex:edi.com.au";
				PECRegistrationNumberValidator.ValidatePECRegistrationNumber(Dummy.Z0_DescriptionInfo);
				AssertEquals("incorrect characters", true, Dummy.HasErrors);

				Dummy.Z0_Description = "alex@domain.xyz";
				PECRegistrationNumberValidator.ValidatePECRegistrationNumber(Dummy.Z0_DescriptionInfo);
				AssertEquals("still allow non-existent domain", false, Dummy.HasErrors);

				Dummy.Z0_Description = "alex@domain.xyz; alex@edi.com.au";
				PECRegistrationNumberValidator.ValidatePECRegistrationNumber(Dummy.Z0_DescriptionInfo);
				AssertEquals("multiple email addresses in Outlook style are not supported", true, Dummy.HasErrors);

				Dummy.Z0_Description = "";
				PECRegistrationNumberValidator.ValidatePECRegistrationNumber(Dummy.Z0_DescriptionInfo);
				AssertEquals("Allow empty email address", false, Dummy.HasErrors);
			}
		}

		public void TestValidatePECRegistrationNumberWithUnderscore()
		{
			using (Dummy.SuspendValidationTesting())
			{
				AssertEquals(false, Dummy.HasErrors);

				Dummy.Z0_Description = "test_1.com";
				PECRegistrationNumberValidator.ValidatePECRegistrationNumber(Dummy.Z0_DescriptionInfo);
				Assert("Invalid PEC value", Dummy.HasErrors);

				Dummy.Z0_Description = "test_1@test.com";
				PECRegistrationNumberValidator.ValidatePECRegistrationNumber(Dummy.Z0_DescriptionInfo);
				Assert("Underscore in name part", !Dummy.HasErrors);

				Dummy.Z0_Description = "test@test_1.com";
				PECRegistrationNumberValidator.ValidatePECRegistrationNumber(Dummy.Z0_DescriptionInfo);
				Assert("Underscore in domain part", !Dummy.HasErrors);

				Dummy.Z0_Description = "test_1@test_1.com";
				PECRegistrationNumberValidator.ValidatePECRegistrationNumber(Dummy.Z0_DescriptionInfo);
				Assert("Underscore in both parts", !Dummy.HasErrors);
			}
		}
	}
}
