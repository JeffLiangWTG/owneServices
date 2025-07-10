using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing
{
	[TestedType(typeof(OutboundMessageBuilder))]
	sealed class OutboundMessageBuilderTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new OutboundMessageBuilder(null));
		}

		public void TestCreate()
		{
			var (messageInformationProvider, messageNumberStrategy) = CreateFakeDataForTest("THIS IS GENERATED MESSAGE TEXT");
			var messageBuilder = CreateMessageBuilder();
			var message = messageBuilder.Create(messageInformationProvider);

			AssertNotNull("OutboundEDIMessage", message);
			CombineAssertions(() =>
			{
				AssertEquals("Message Type", "T1", message.EM_MessageType);
				AssertEquals("Message Sub Type", "ST1", message.EM_MessageSubType);
				AssertEquals("Message Text", "THIS IS GENERATED MESSAGE TEXT", message.EM_MessageText);
				AssertEquals("Application Reference", "AR", message.EM_ApplicationReference);
				AssertEquals("Message Status", "QUE", message.EM_Status);
				AssertEquals("Application Code", "NOC", message.EM_ApplicationCode);
				AssertSame("Message Number Strategy", messageNumberStrategy, message.MessageNumberStrategy);
			});
		}

		public void TestCreate_WithEmptyMessageText()
		{
			var (messageInformationProvider, messageNumberStrategy) = CreateFakeDataForTest(string.Empty);
			var messageBuilder = CreateMessageBuilder();
			var message = messageBuilder.Create(messageInformationProvider);

			AssertNull("OutboundEDIMessage", message);
		}

		(IMessageInformationProvider InformationProvider, IMessageNumberStrategy NumberStrategy) CreateFakeDataForTest(string message)
		{
			var messageNumberStrategy = Mock.Of<IMessageNumberStrategy>(
				s => s.GetMessageReferenceNumber() == "MRF112233");

			var messageInformationProvider = Mock.Of<IMessageInformationProvider>(
				p => p.MessageType == (ZString)"T1"
					&& p.MessageSubType == (ZString)"ST1"
					&& p.MessageText == (ZString)message
					&& p.ApplicationReference == (ZString)"AR"
					&& p.MessageNumberStrategy == messageNumberStrategy
					&& p.ApplicationCode == (ZString)"NOC");

			return (messageInformationProvider, messageNumberStrategy);
		}

		IOutboundMessageBuilder CreateMessageBuilder() => new OutboundMessageBuilder(Factory);
	}
}
