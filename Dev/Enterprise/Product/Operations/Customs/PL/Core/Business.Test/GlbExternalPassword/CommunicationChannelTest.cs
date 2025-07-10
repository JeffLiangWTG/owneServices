using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.PL.Business.Testing;

[TestedType(typeof(CommunicationChannel))]
sealed class CommunicationChannelTest : MasterFiles.Business.Testing.GlbExternalPasswordTest<CommunicationChannel>
{
	public void TestGP_UserID_Caption()
	{
		var communicationChannel = Factory.New<CommunicationChannel>();
		CombineAssertions(() =>
		{
			var resourceString = DataBoundResourceStrings.GetDataForProperty(communicationChannel.GP_MailBoxIDInfo);
			AssertEquals("Caption", "Communication Channel Email", resourceString.Caption);
			AssertEquals("MediumCaption", "Comm. Chan. Email", resourceString.MediumCaption);
			AssertEquals("ShortCaption", "Comm. Email", resourceString.ShortCaption);
		});
	}

	public void TestSetDefaultValues()
	{
		var password = Factory.New<CommunicationChannel>();

		CombineAssertions(() =>
		{
			AssertEquals("Password type", PasswordTypesList.Codes.PLC, password.GP_PasswordType);
			AssertEquals("Name", "CommunicationChannel", password.GP_Name);
		});
	}

	public void TestValidation() => AssertType<CommunicationChannelValidation>(Factory.New<CommunicationChannel>().Validation);
}
