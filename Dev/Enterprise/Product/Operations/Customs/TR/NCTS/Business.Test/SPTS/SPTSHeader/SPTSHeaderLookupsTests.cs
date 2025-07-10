using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.TR.NCTS.Business.Testing
{
	class SPTSHeaderLookupsTests : BusinessObjectLookupsTestCase
	{
		public void TestTransportList()
		{
			var header = Factory.New<SPTSHeader>();
			var transportmodes = header.Lookups.SPTSTransportModes;
			var list1 = Factory.GetCachedValue<SPTSTransportModeList>();
			AssertEquals(list1.Count, transportmodes.Count);
		}

		public void TestMessageStatusList()
		{
			var header = Factory.New<SPTSHeader>();
			var messageStatusList = header.Lookups.MessageStatusList;
			CombineAssertions("SPTSMessageStatusList", () =>
			{
				AssertNotNull(messageStatusList);
				AssertEquals(typeof(SPTSMessageStatusList), messageStatusList.GetType());
				AssertEquals("CodeAsString", "AMD, MAN, MAR, MAS, AWA, MCA, MCF, MCR, MDN, MDS, MEE, MQU, MOK, REG, REJ, , MUR, MUS", messageStatusList.CodesAsString);
			});
		}
	}
}
