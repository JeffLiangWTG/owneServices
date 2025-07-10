using System.Collections;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing;

[TestedType(typeof(NOCustomsPurgeSettingsConfig))]
sealed class NOCustomsPurgeSettingsConfigTest : TestCaseWithFactory
{
	public void TestPurgeSettings_ShouldReturnsExpectedApplicationCodes()
	{
		var purgeSettings = purgeSettingsConfig.GetPurgeSettings().ToList();
		AssertEquals("Expected 3 purge settings configurations.", 3, purgeSettings.Count);

		var expectedApplicationCode = new[]
			{
				ApplicationCodeList.Codes.NOCustoms,
				EDIMessageConstants.MessageTypes.CUSRES,
				ApplicationCodeList.Codes.NOCustomsEmma
			};

		var actualApplicationCodes = purgeSettings.Select(x => x.ApplicationCode).ToList();

		AssertContainsExactElementsInAnyOrder("Application Codes", expectedApplicationCode, actualApplicationCodes);
	}

	public void TestPurgeSettings_ShouldContainsNOCWithExpectedMessageSubTypes()
	{
		var expectedCollection = new MessageSubTypePurgeTypeObjCollection()
			.Add(MessageSendingMessageTypes.Codes.CompleteOrdinaryDeclaration, MessageSendingMessageTypes.Descriptions.CompleteOrdinaryDeclaration, 5, TimeUnit.Year)
			.Add(MessageSendingMessageTypes.Codes.ManualDeclaration, MessageSendingMessageTypes.Descriptions.ManualDeclaration, 5, TimeUnit.Year)
			.Add(MessageSendingMessageTypes.Codes.Correction, MessageSendingMessageTypes.Descriptions.Correction, 5, TimeUnit.Year)
			.Add(MessageSendingMessageTypes.Codes.PreliminaryDeclaration, MessageSendingMessageTypes.Descriptions.PreliminaryDeclaration, 5, TimeUnit.Year)
			.Add(MessageSendingMessageTypes.Codes.FinalDeclaration, MessageSendingMessageTypes.Descriptions.FinalDeclaration, 5, TimeUnit.Year)
			.Add(MessageSendingMessageTypes.Codes.PostDeclaration, MessageSendingMessageTypes.Descriptions.PostDeclaration, 5, TimeUnit.Year)
			.Add(MessageSendingMessageTypes.Codes.RefundDeclaration, MessageSendingMessageTypes.Descriptions.RefundDeclaration, 5, TimeUnit.Year)
			.Add(MessageSendingMessageTypes.Codes.StatisticalRecalculatedDeclaration, MessageSendingMessageTypes.Descriptions.StatisticalRecalculatedDeclaration, 5, TimeUnit.Year)
			.Add(MessageSendingMessageTypes.Codes.CollectiveCustomClearance, MessageSendingMessageTypes.Descriptions.CollectiveCustomClearance, 5, TimeUnit.Year)
			.Add(xTMessageConstants.MessageTypes.Codes.Acknowledgement, xTMessageConstants.MessageTypes.Descriptions.Acknowledgement, 5, TimeUnit.Year)
			.Add(xTMessageConstants.MessageTypes.Codes.Error, xTMessageConstants.MessageTypes.Descriptions.Error, 5, TimeUnit.Year);

		AssertPurgeSettingsWithExpectedMessageTypeSubTypes(ApplicationCodeList.Codes.NOCustoms, expectedCollection);
	}

	public void TestPurgeSettings_ShouldContainsNOEWithExpectedMessageTypes()
	{
		var expectedCollection = new MessageTypePurgeTypeObjCollection()
			.Add(EDIMessageConstants.MessageTypes.EMMA, "EMMA", 5, TimeUnit.Year)
			.Add(xTMessageConstants.MessageTypes.Codes.Acknowledgement, xTMessageConstants.MessageTypes.Descriptions.Acknowledgement, 5, TimeUnit.Year)
			.Add(xTMessageConstants.MessageTypes.Codes.Error, xTMessageConstants.MessageTypes.Descriptions.Error, 5, TimeUnit.Year);

		AssertPurgeSettingsWithExpectedMessageTypeSubTypes(ApplicationCodeList.Codes.NOCustomsEmma, expectedCollection);
	}

	void AssertPurgeSettingsWithExpectedMessageTypeSubTypes(string applicationCode, MessageTypeObjCollection expectedMessageSubTypes)
	{
		var purgeSettings = purgeSettingsConfig.GetPurgeSettings().ToList();
		var noCustomsPurgeSettings = purgeSettings.FirstOrDefault(r => r.ApplicationCode == applicationCode);

		AssertNotNull($"[PRE-CONDITION] {applicationCode} purge settings should not be null.", noCustomsPurgeSettings);

		var actualCollection = noCustomsPurgeSettings.MessageTypes;

		AssertContainsExactElementsInAnyOrder(
			expectedMessageSubTypes.Cast<MessageTypeObj>().Select(x =>
				(x.MessageSubType, x.MessageSubTypeDescription, x.PurgeTime, x.PurgeTimeUnit)),
			actualCollection.Cast<MessageTypeObj>().Select(x =>
				(x.MessageSubType, x.MessageSubTypeDescription, x.PurgeTime, x.PurgeTimeUnit)));
	}

	public void TestPurgeSettingsListContainsCUSRESEntry()
	{
		var purgeSettings = purgeSettingsConfig.GetPurgeSettings().ToList();
		var cusresPurgeSettings = purgeSettings.FirstOrDefault(r => r.ApplicationCode == EDIMessageConstants.MessageTypes.CUSRES);

		AssertNotNull("[PRE-CONDITION] CUSRES purge settings should not be null.", cusresPurgeSettings);

		var expectedCollection = new MessageTypePurgeTypeObjCollection()
			.Add(EDIMessageConstants.MessageTypes.CUSRES, "RES", 5, TimeUnit.Year);

		var actualCollection = cusresPurgeSettings.MessageTypes;

		AssertContainsExactElementsInAnyOrder(
			expectedCollection.Cast<MessageTypeObj>().Select(x =>
				(x.MessageType, x.MessageTypeDescription, x.PurgeTime, x.PurgeTimeUnit)),
			actualCollection.Cast<MessageTypeObj>().Select(x =>
				(x.MessageType, x.MessageTypeDescription, x.PurgeTime, x.PurgeTimeUnit)));
	}

	public void TestNOCustomsPurgePresentInMessagePurgeConfigList()
	{
		var configs = ObjectFactory.Get<IEnumerable>("MessagePurgeSettingsConfigList");
		bool hasNOCustomsConfig = configs
			  .Cast<object>()
			  .Any(config => config.GetType().Name == "NOCustomsPurgeSettingsConfig" && config is NOCustomsPurgeSettingsConfig);

		AssertEquals("NOCustomsPurgeSettingsConfig is not present in MessagePurgeSettingsConfigList", expected: true, hasNOCustomsConfig);
	}

	protected override void SetUp()
	{
		base.SetUp();
		purgeSettingsConfig = new NOCustomsPurgeSettingsConfig();
	}

	NOCustomsPurgeSettingsConfig purgeSettingsConfig;
}
