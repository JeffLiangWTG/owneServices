using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business;

namespace Enterprise.Freight.Business.Testing
{
	public class ComTracEDIMessagePurgeSettingsConfigTest : TestCaseWithFactory
	{
		public void TestGetPurgeSettings_ShouldReturnCorrectSettings()
		{
			var config = new ComTracEDIMessagePurgeSettingsConfig();
			var expectedDuration = 12;
			var expectedUnit = TimeUnit.Month;

			var result = config.GetPurgeSettings();

			AssertNotNull(result);
			var settingsList = new List<ApplicationCodeObj>(result);
			AssertEquals(1, settingsList.Count);

			var ctrRequestMessage = settingsList[0];
			AssertEquals(ApplicationCodeList.Codes.ComTrac, ctrRequestMessage.ApplicationCode);
			AssertEquals(expectedDuration, ctrRequestMessage.Interchanges[0].PurgeTime);
			AssertEquals(expectedUnit, ctrRequestMessage.Interchanges[0].PurgeTimeUnit);
		}
	}
}
