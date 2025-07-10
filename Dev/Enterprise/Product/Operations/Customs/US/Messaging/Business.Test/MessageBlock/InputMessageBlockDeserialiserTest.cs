using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using NUnit.Framework;

namespace Enterprise.Customs.US.Messaging.Business.Testing
{
	sealed class InputMessageBlockDeserialiserTest : TestCase
	{
		public void TestGetDeserialisedBlock()
		{
			MessageBlockDeserialiser block = new InputMessageBlockDeserialiser();
			AssertEquals(typeof(ZZZA), block.GetDeserialisedBlock(new string[] { CBPEDIInterchange.ApplicationCodeForTesting }, ApplicationIdentifierCodeList.DummyForTesting1, "Z¿ºA".PadRight(80)).GetType());
			AssertEquals(typeof(ZZZA), block.GetDeserialisedBlock(new string[] { CBPEDIInterchange.ApplicationCodeForTesting }, ApplicationIdentifierCodeList.DummyForTesting2, "Z¿ºA".PadRight(80)).GetType());
			AssertEquals(typeof(ZZZB), block.GetDeserialisedBlock(new string[] { CBPEDIInterchange.ApplicationCodeForTesting }, ApplicationIdentifierCodeList.DummyForTesting1, "Z¿ºB".PadRight(80)).GetType());
			AssertEquals(typeof(ZZZB2), block.GetDeserialisedBlock(new string[] { CBPEDIInterchange.ApplicationCodeForTesting }, ApplicationIdentifierCodeList.DummyForTesting2, "Z¿ºB".PadRight(80)).GetType());
			AssertEquals(typeof(ZZZC), block.GetDeserialisedBlock(new string[] { CBPEDIInterchange.ApplicationCodeForTesting }, ApplicationIdentifierCodeList.DummyForTesting1, "Z¿ºC".PadRight(80)).GetType());
			AssertEquals(typeof(ZZZC2), block.GetDeserialisedBlock(new string[] { CBPEDIInterchange.ApplicationCodeForTesting }, ApplicationIdentifierCodeList.DummyForTesting2, "Z¿ºC".PadRight(80)).GetType());
			AssertEquals(typeof(ZZZY), block.GetDeserialisedBlock(new string[] { CBPEDIInterchange.ApplicationCodeForTesting }, ApplicationIdentifierCodeList.DummyForTesting1, "Z¿ºY".PadRight(80)).GetType());
			AssertEquals(typeof(ZZZY2), block.GetDeserialisedBlock(new string[] { CBPEDIInterchange.ApplicationCodeForTesting }, ApplicationIdentifierCodeList.DummyForTesting2, "Z¿ºY".PadRight(80)).GetType());
			AssertEquals(typeof(ZZZZ), block.GetDeserialisedBlock(new string[] { CBPEDIInterchange.ApplicationCodeForTesting }, ApplicationIdentifierCodeList.DummyForTesting1, "Z¿ºZ".PadRight(80)).GetType());
			AssertEquals(typeof(ZZZZ), block.GetDeserialisedBlock(new string[] { CBPEDIInterchange.ApplicationCodeForTesting }, ApplicationIdentifierCodeList.DummyForTesting2, "Z¿ºZ".PadRight(80)).GetType());
		}
	}
}
