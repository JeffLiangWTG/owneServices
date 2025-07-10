namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Output.Testing
{
	sealed class RECR01Test : NUnit.Framework.TestCase
	{
		public void TestRECR01()
		{
			RECR01 r01 = new RECR01();
			AssertNoExceptionThrown(() => r01.Deserialise("RERXJ500010556RBPRECON DDPP MUST = B REC DDPP                                   "));
			AssertEquals("XJ500010556", r01.ReconciliationNumber);
			AssertEquals("ER", r01.RecordType); //error
			IStatusesAndErrors errors = r01;
			AssertEquals("", errors.LineNumber);
			AssertEquals("", errors.ReferenceNumber);
			AssertEquals("RBP", errors.Code);
			AssertEquals("RECON DDPP MUST = B REC DDPP", errors.NarrativeMessage);
		}
	}
}
