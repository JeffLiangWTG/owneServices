using System.Collections;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NO.NCTS.Business.Testing;

[TestedType(typeof(NONCTSPurgeSettingsConfig))]
sealed class NONCTSPurgeSettingsConfigTest : TestCaseWithFactory
{
	public void TestPurgeSettingsListContainsAllExpectedEntries()
	{
		var config = new NONCTSPurgeSettingsConfig();
		var purgeSettings = config.GetPurgeSettings().ToList();
		AssertEquals("Expected exactly one purge settings configuration.", 1, purgeSettings.Count);
		var setting = purgeSettings.First();
		AssertEquals("Application code should be NOCustoms.", ApplicationCodeList.Codes.NOCustomsNcts, setting.ApplicationCode);

		var expectedCollection = new MessageSubTypePurgeTypeObjCollection()
			.Add(NctsArrivalMessageTypeCodeList.Codes.ArrivalNotification, NctsArrivalMessageTypeCodeList.Descriptions.ArrivalNotification, 5, TimeUnit.Year)
			.Add(NctsArrivalMessageTypeCodeList.Codes.UnloadingRemarks, NctsArrivalMessageTypeCodeList.Descriptions.UnloadingRemarks, 5, TimeUnit.Year);

		var actualCollection = setting.MessageTypes;

		AssertContainsExactElementsInAnyOrder(
			expectedCollection.Cast<MessageTypeObj>().Select(x =>
				(x.MessageSubType, x.MessageSubTypeDescription, x.PurgeTime, x.PurgeTimeUnit)),
			actualCollection.Cast<MessageTypeObj>().Select(x =>
				(x.MessageSubType, x.MessageSubTypeDescription, x.PurgeTime, x.PurgeTimeUnit)));
	}

	public void TestNONCTSPurgePresentInMessagePurgeConfigList()
	{
		var configs = ObjectFactory.Get<IEnumerable>("MessagePurgeSettingsConfigList");
		bool hasNONCTSConfig = configs
			.Cast<object>()
			.Any(config => config.GetType().Name == "NONCTSPurgeSettingsConfig" && config is NONCTSPurgeSettingsConfig);

		AssertEquals("NONCTSPurgeSettingsConfig is not present in MessagePurgeSettingsConfigList", true, hasNONCTSConfig);
	}
}
