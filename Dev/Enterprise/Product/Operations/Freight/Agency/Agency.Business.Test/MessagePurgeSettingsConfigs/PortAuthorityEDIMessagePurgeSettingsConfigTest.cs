using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business;

namespace Enterprise.Freight.Agency.Business.Testing
{
	public class PortAuthorityEDIMessagePurgeSettingsConfigTest : TestCaseWithFactory
	{
		public void TestGetPurgeSettings_ShouldReturnCorrectSettings()
		{
			var config = new PortAuthorityEDIMessagePurgeSettingsConfig();
			var expectedDuration = 12;
			var expectedUnit = TimeUnit.Month;

			var result = config.GetPurgeSettings();

			AssertNotNull(result);
			var settingsList = new List<ApplicationCodeObj>(result);
			AssertEquals(1, settingsList.Count);

			var patRequestMessage = settingsList[0];
			AssertEquals(ApplicationCodeList.Codes.PortAuthority, patRequestMessage.ApplicationCode);
			AssertEquals(expectedDuration, patRequestMessage.Interchanges[0].PurgeTime);
			AssertEquals(expectedUnit, patRequestMessage.Interchanges[0].PurgeTimeUnit);
		}
	}
}
