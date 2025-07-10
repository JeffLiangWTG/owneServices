using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ZA.Business.MessageBuilders.Testing
{
	sealed class MessageBuilderHelperTest : TestCaseWithFactory
	{
		public void TestUnlocoToIata()
		{
			var unloco = Factory.New<RefUNLOCO>();
			unloco.RL_Code = "ZAVWG";
			unloco.RL_IATA = "XXX";
			Factory.Save();
			AssertEquals("XXX", MessageBuilderHelper.UnlocoToIata(Factory, Core.Constants.TransportModes.Air, "ZAVWG"));
			AssertEquals("XYZ", MessageBuilderHelper.UnlocoToIata(Factory, Core.Constants.TransportModes.Air, "ZAXYZ"));
			AssertEquals("ZAVWG", MessageBuilderHelper.UnlocoToIata(Factory, Core.Constants.TransportModes.Sea, "ZAVWG"));
		}

		public void TestUnlocoToIata_IsAir()
		{
			var unloco = Factory.New<RefUNLOCO>();
			unloco.RL_Code = "ZAVWG";
			unloco.RL_IATA = "XXX";
			Factory.Save();
			AssertEquals("XXX", MessageBuilderHelper.UnlocoToIata(Factory, true, "ZAVWG"));
			AssertEquals("XYZ", MessageBuilderHelper.UnlocoToIata(Factory, true, "ZAXYZ"));
			AssertEquals("ZAVWG", MessageBuilderHelper.UnlocoToIata(Factory, false, "ZAVWG"));
		}
	}
}
