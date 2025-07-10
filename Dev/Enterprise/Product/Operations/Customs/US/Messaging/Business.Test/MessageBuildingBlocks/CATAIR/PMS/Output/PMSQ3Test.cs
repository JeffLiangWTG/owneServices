namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Output.Testing
{
	sealed class PMSQ3Test : NUnit.Framework.TestCase
	{
		public void TestMaskSSNOnPMSQ3()
		{
			var message = "Q32723P056KT051523051923W69615-72-9798 0000023012400000000000                   ";
			var pmsq3 = new PMSQ3();
			pmsq3.Deserialise(message);
			AssertEquals("615-72-9798", pmsq3.PeriodicMonthlyStatementImporterNumber);

			Environment.Env.Security.OrgDetailsViewPersonalInformation.IsAllowed = false;
			pmsq3.Deserialise(message);
			AssertEquals("***-**-****", pmsq3.PeriodicMonthlyStatementImporterNumber);
		}
	}
}
