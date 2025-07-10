namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.BIRD.Testing
{
	sealed class BRDENTest : NUnit.Framework.TestCase
	{
		public void TestDeserialiseEntryNumber()
		{
			string message = "ENB00004760           XJ5 33032792Q815                                          ";
			BRDEN en = new BRDEN();
			en.Deserialise(message);
			AssertEquals("XJ5", en.FilerCode);
			AssertEquals("Entry Number should be right justified", "33032792", en.EntryNumber);
		}
	}
}
