using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	public class ForwardAirEDIMessagePurgeSettingsConfigTest : TestCaseWithFactory
	{
		public void TestGetPurgeSettings_ShouldReturnCorrectSettings()
		{
			var config = new ForwardAirEDIMessagePurgeSettingsConfig();
			var expectedDuration = 12;
			var expectedUnit = TimeUnit.Month;

			var result = config.GetPurgeSettings();

			AssertNotNull(result);
			var settingsList = new List<ApplicationCodeObj>(result);
			AssertEquals(1, settingsList.Count);

			var fwaRequestMessage = settingsList[0];
			AssertEquals(ApplicationCodeList.Codes.ForwardAir, fwaRequestMessage.ApplicationCode);
			AssertEquals(expectedDuration, fwaRequestMessage.Interchanges[0].PurgeTime);
			AssertEquals(expectedUnit, fwaRequestMessage.Interchanges[0].PurgeTimeUnit);
		}
	}
}
