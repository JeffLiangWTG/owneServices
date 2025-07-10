using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.Messaging.Business.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuilders.Testing
{
	class ApplicationControlGeneratorTest : TestCaseWithFactory
	{
		[TestDate(1971, 9, 18)]
		public void TestAddMessage()
		{
			var blockControlGenerator = new BlockControlGeneratorForTesting();
			blockControlGenerator.AddMessageBlock(new ZZZC() { StringC = "C1" });
			var ediMessage = blockControlGenerator.CreateMessage<CBPMessageForTesting>(Factory);
			var applicationControlGenerator = ApplicationControlGenerator.New(CBPEDIInterchange.ApplicationCodeForTesting, "", GlbBranch.CurrentBranch);
			applicationControlGenerator.AddMessage(ediMessage);

			AssertEquals("Z¿ºA".PadRight(80), applicationControlGenerator.A.Serialise());
			AssertEquals("Z¿ºB".PadRight(80) +
				"Z¿ºC                          C1".PadRight(80) +
				"Z¿ºY".PadRight(80), applicationControlGenerator.GetBody());
			AssertEquals("Z¿ºZ".PadRight(80), applicationControlGenerator.Z.Serialise());
		}
	}
}
