namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output.Testing
{
	sealed class ADSTQ1Test : NUnit.Framework.TestCase
	{
		public void TestMaskSSNOnADSTQ1()
		{
			var message = "Q11324XJ5  12345678   555-99-7777                                             AE";
			var adstQ1 = new ADSTQ1();
			adstQ1.Deserialise(message);
			AssertEquals("555-99-7777", adstQ1.ImporterOfRecordNumber);

			Environment.Env.Security.OrgDetailsViewPersonalInformation.IsAllowed = false;
			adstQ1.Deserialise(message);
			AssertEquals("***-**-****", adstQ1.ImporterOfRecordNumber);
		}
	}
}
