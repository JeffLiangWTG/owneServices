using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business;

namespace Enterprise.DeniedPartyScreening.Business.Test
{
	public class DeniedPartyScreeningEDIMessagePurgeSettingsConfigTest : TestCaseWithFactory
	{
		public void TestGetPurgeSettings_ShouldReturnCorrectSettings()
		{
			var config = new DeniedPartyScreeningEDIMessagePurgeSettingsConfig();
			var expectedDuration = 6;
			var expectedUnit = TimeUnit.Month;

			var result = config.GetPurgeSettings();

			AssertNotNull(result);
			var settingsList = new List<ApplicationCodeObj>(result);
			AssertEquals(1, settingsList.Count);

			var dpsRequestMessage = settingsList[0];
			AssertEquals(ApplicationCodeList.Codes.DPSRequestMessage, dpsRequestMessage.ApplicationCode);
			AssertEquals(expectedDuration, dpsRequestMessage.Interchanges[0].PurgeTime);
			AssertEquals(expectedUnit, dpsRequestMessage.Interchanges[0].PurgeTimeUnit);
		}
	}
}
