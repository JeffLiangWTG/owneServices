using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.DataRegistry.Business.Testing
{
	[TestedType(typeof(ZAEDIMessagePurgeSettingsConfig))]
	class ZAEDIMessagePurgeSettingsConfigTest : TestCaseWithFactory
	{
		public void TestGetPurgeSettings()
		{
			var config = new ZAEDIMessagePurgeSettingsConfig();
			var expectedDuration = 10;
			var expectedUnit = TimeUnit.Year;
			var settings = config.GetPurgeSettings().ToArray();
			AssertEquals(2, settings.Length);
			AssertApplicationCode(ApplicationCodeList.Codes.ZATransactionOrders, expectedDuration, expectedUnit, settings[0]);
			AssertApplicationCode(ApplicationCodeList.Codes.ZACustoms, expectedDuration, expectedUnit, settings[1]);
		}

		static void AssertApplicationCode(string applicationCode, int expectedDuration, ZGuid expectedUnit, ApplicationCodeObj settingEntry)
		{
			AssertEquals(applicationCode, settingEntry.ApplicationCode);
			AssertEquals(expectedDuration, settingEntry.Interchanges[0].PurgeTime);
			AssertEquals(expectedUnit, settingEntry.Interchanges[0].PurgeTimeUnit);
		}
	}
}
