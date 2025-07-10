using NUnit.Framework;

namespace Enterprise.Customs.PL.Business.Testing;

[TestedType(typeof(CommunicationChannelValidation))]
sealed class CommunicationChannelValidationTest : MasterFiles.Business.Testing.GlbExternalPasswordValidationTest<CommunicationChannel, CommunicationChannelValidation>
{
	public void TestCheckGP_MailBoxID()
	{
		var communicationChannel = Factory.New<CommunicationChannel>();
		var validation = communicationChannel.Validation;
		var propertyInfo = communicationChannel.GP_MailBoxIDInfo;
		var partialErrorText = "Email Address is not valid";

		CombineAssertions(() =>
		{
			AssertNoNotifications("Empty email", propertyInfo);

			communicationChannel.GP_MailBoxID = "asd@asd";
			AssertHasErrorContaining("Invalid email", propertyInfo, partialErrorText);

			communicationChannel.GP_MailBoxID = "asd@asd.com";
			AssertNoErrorContaining("Valid email", propertyInfo, partialErrorText);
		});
	}
}
