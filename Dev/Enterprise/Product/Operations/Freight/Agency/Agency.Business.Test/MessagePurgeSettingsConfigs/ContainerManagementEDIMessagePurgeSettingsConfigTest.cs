using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business;

namespace Enterprise.Freight.Agency.Business.Testing
{
	public class ContainerManagementEDIMessagePurgeSettingsConfigTest : TestCaseWithFactory
	{
		public void TestGetPurgeSettings_ShouldReturnCorrectSettings()
		{
			var config = new ContainerManagementEDIMessagePurgeSettingsConfig();
			var expectedDuration = 12;
			var expectedUnit = TimeUnit.Month;

			var result = config.GetPurgeSettings();

			AssertNotNull(result);
			var settingsList = new List<ApplicationCodeObj>(result);
			AssertEquals(1, settingsList.Count);

			var cmgRequestMessage = settingsList[0];
			AssertEquals(ApplicationCodeList.Codes.ContainerManagement, cmgRequestMessage.ApplicationCode);
			AssertEquals(expectedDuration, cmgRequestMessage.Interchanges[0].PurgeTime);
			AssertEquals(expectedUnit, cmgRequestMessage.Interchanges[0].PurgeTimeUnit);
		}
	}
}
