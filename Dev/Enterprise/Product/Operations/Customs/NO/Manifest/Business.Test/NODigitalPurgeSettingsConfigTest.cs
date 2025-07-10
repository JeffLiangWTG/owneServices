using System.Collections;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Manifest.Business.Testing;

[TestedType(typeof(NODigitalPurgeSettingsConfig))]
sealed class NODigitalPurgeSettingsConfigTest : TestCaseWithFactory
{
	public void TestPurgeSettingsListContainsAllExpectedEntries()
	{
		var config = new NODigitalPurgeSettingsConfig();
		var purgeSettings = config.GetPurgeSettings().ToList();
		AssertEquals("Expected exactly one purge settings configuration.", 1, purgeSettings.Count);
		var setting = purgeSettings.First();
		AssertEquals("Application code should be NOCustomsDMO.", ApplicationCodeList.Codes.NOCustomsDMO, setting.ApplicationCode);

		var expectedCollection = new MessageSubTypePurgeTypeObjCollection()
			.Add(NODMOMessageFunctionList.Codes.NEW, NODMOMessageFunctionList.Descriptions.NEW, 5, TimeUnit.Year)
			.Add(NODMOMessageFunctionList.Codes.UPD, NODMOMessageFunctionList.Descriptions.UPD, 5, TimeUnit.Year)
			.Add(NODMOMessageFunctionList.Codes.DEL, NODMOMessageFunctionList.Descriptions.DEL, 5, TimeUnit.Year)
			.Add(NODMOMessageFunctionList.Codes.VAL, NODMOMessageFunctionList.Descriptions.VAL, 5, TimeUnit.Year);

		var actualCollection = setting.MessageTypes;

		AssertContainsExactElementsInAnyOrder(
			expectedCollection.Cast<MessageTypeObj>().Select(x =>
				(x.MessageSubType, x.MessageSubTypeDescription, x.PurgeTime, x.PurgeTimeUnit)),
			actualCollection.Cast<MessageTypeObj>().Select(x =>
				(x.MessageSubType, x.MessageSubTypeDescription, x.PurgeTime, x.PurgeTimeUnit)));
	}

	public void TestNODigitalPurgePresentInMessagePurgeConfigList()
	{
		var configs = ObjectFactory.Get<IEnumerable>("MessagePurgeSettingsConfigList");
		bool hasNODigitalConfig = configs
			.Cast<object>()
			.Any(config => config.GetType().Name == "NODigitalPurgeSettingsConfig" && config is NODigitalPurgeSettingsConfig);
		AssertEquals("NODigitalPurgeSettingsConfig is not present in MessagePurgeSettingsConfigList", true, hasNODigitalConfig);
	}
}
