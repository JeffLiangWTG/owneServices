namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output.Testing
{
	sealed class ADSTQ3Test : NUnit.Framework.TestCase
	{
		public void TestMaskSSNOnADSTQ3()
		{
			var message = "Q30123456789  120525XJ5   555-99-7777                                 1234      ";
			var adstQ3 = new ADSTQ3();
			adstQ3.Deserialise(message);
			AssertEquals("555-99-7777", adstQ3.ImporterOfRecordNumber);

			Environment.Env.Security.OrgDetailsViewPersonalInformation.IsAllowed = false;
			adstQ3.Deserialise(message);
			AssertEquals("***-**-****", adstQ3.ImporterOfRecordNumber);
		}
	}
}
