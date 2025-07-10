using System;
using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using Enterprise.BatchProcessor;
using NUnit.Framework;

namespace Enterprise.Customs.PL.Business.Testing;

[TestsSubclassesOf(typeof(MessageProcessorFactoryBase))]
public abstract class MessageProcessorFactoryTestBase<TEDIMessage> : TestCaseWithFactory
	where TEDIMessage : BaseEDIMessage
{
	public void TestResolvesMessageProcessors() => CombineAssertions(() =>
	{
		var message = Factory.New<TEDIMessage>();
		AssertNull("Default", MessageProcessorFactory.CreateProcessor(message, Logger));

		foreach (var (messageType, messageName, expectedProcessorType) in ExpectedProcessorTypes)
		{
			message.EM_MessageType = messageType;
			message.EM_MessageSubType = messageName;
			AssertType($"{messageType} : {messageName}", expectedProcessorType, MessageProcessorFactory.CreateProcessor(message, Logger));
		}
	});

	protected abstract IReadOnlyCollection<(string MessageType, string MessageName, Type ExpectedProcessorType)> ExpectedProcessorTypes { get; }

	protected abstract MessageProcessorFactoryBase CreateMessageProcessorFactory();

	protected override void SetUp()
	{
		base.SetUp();

		Logger = new LoggingInformation();
		MessageProcessorFactory = CreateMessageProcessorFactory();
	}

	protected MessageProcessorFactoryBase MessageProcessorFactory { get; private set; }
	protected LoggingInformation Logger { get; private set; }
}
