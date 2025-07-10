using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.BIRD;

namespace Enterprise.Customs.US.Business.MessageBuildingBlocks.Output.Testing
{
	sealed class ENQTest : TestCaseWithFactory
	{
		public void TestDeserialise()
		{
			string message = "AAJR  XJ58888B00004751                  20091207204452                          J18888XJ5 1000209191-013199000010000014000000000000000                  001     J2XJ5 10002091      0000000000000000000000                               808001 J3XJ5 10002091                                  89130010715000000000000B00004751J98888XJ5 10002091BILLING DATA NOT ON FILE                                      J98888XJ5 10002091COLLECTION DATA NOT ON FILE                                   ZZJR  00000000101";

			ImportOutputBlockControlGenerator<BRDAA, BRDZZ> generator = new ImportOutputBlockControlGenerator<BRDAA, BRDZZ>();
			AssertNoExceptionThrown(() => generator.Deserialise(BlockPadder.Pad(message)));

			InputBlockControlGenerator<BRDAA, BRDZZ> generator2 = new ImportInputBlockControlGenerator<BRDAA, BRDZZ>();
			AssertNoExceptionThrown(() => generator2.Deserialise(BlockPadder.Pad(message)));
		}
	}
}
