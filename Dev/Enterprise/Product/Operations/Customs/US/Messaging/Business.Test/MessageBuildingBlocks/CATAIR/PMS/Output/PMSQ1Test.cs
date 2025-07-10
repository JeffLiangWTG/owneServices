namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Output.Testing
{
	sealed class PMSQ1Test : NUnit.Framework.TestCase
	{
		public void TestMaskSSNOnPMSQ1()
		{
			var message = "Q127231292BK2704W69615-72-9798 0509230519230000023012400000000000               ";
			var pmsq1 = new PMSQ1();
			pmsq1.Deserialise(message);
			AssertEquals("615-72-9798", pmsq1.PeriodicDailyStatementImporterNumber);

			Environment.Env.Security.OrgDetailsViewPersonalInformation.IsAllowed = false;
			pmsq1.Deserialise(message);
			AssertEquals("***-**-****", pmsq1.PeriodicDailyStatementImporterNumber);
		}
	}
}
