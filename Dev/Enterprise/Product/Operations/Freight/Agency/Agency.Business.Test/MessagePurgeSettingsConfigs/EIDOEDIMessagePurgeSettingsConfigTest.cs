using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business;

namespace Enterprise.Freight.Agency.Business.Testing
{
	public class EIDOEDIMessagePurgeSettingsConfigTest : TestCaseWithFactory
	{
		public void TestGetPurgeSettings_ShouldReturnCorrectSettings()
		{
			var config = new EIDOEDIMessagePurgeSettingsCongfig();
			var expectedDuration = 12;
			var expectedUnit = TimeUnit.Month;

			var result = config.GetPurgeSettings();

			AssertNotNull(result);
			var settingsList = new List<ApplicationCodeObj>(result);
			AssertEquals(1, settingsList.Count);

			var edoRequestMessage = settingsList[0];
			AssertEquals(ApplicationCodeList.Codes.EIDO, edoRequestMessage.ApplicationCode);
			AssertEquals(expectedDuration, edoRequestMessage.Interchanges[0].PurgeTime);
			AssertEquals(expectedUnit, edoRequestMessage.Interchanges[0].PurgeTimeUnit);
		}
	}
}
