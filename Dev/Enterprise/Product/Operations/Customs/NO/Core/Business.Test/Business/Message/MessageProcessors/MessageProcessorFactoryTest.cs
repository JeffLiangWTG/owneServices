using CargoWise.EntityFramework.Testing;
using Enterprise.BatchProcessor;
using Enterprise.Integration;
using Enterprise.Messaging.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing;

[TestedType(typeof(MessageProcessorFactory))]
sealed class MessageProcessorFactoryTest : TestCaseWithFactory
{
	public void TestGetMessageProcessor() => CombineAssertions(() =>
	{
		var loggerMock = new Mock<LoggingInformation>();
		var message = Factory.New<EDIMessage>();
		message.EM_MessageType = EDIMessageConstants.MessageTypes.CUSRES;
		AssertType<CUSRESMessageProcessor>("When message type is RES", MessageProcessorFactory.GetMessageProcessor(message, loggerMock.Object));

		message.EM_MessageType = UnknownMessageType;
		AssertNull("When MessageType is unknown", MessageProcessorFactory.GetMessageProcessor(message, loggerMock.Object));
	});

	[ExpectNoExceptions]
	public void TestLoggedInformation()
	{
		var loggerMock = new Mock<LoggingInformation>();
		var message = Factory.New<EDIMessage>();
		message.EM_MessageType = UnknownMessageType;
		_ = MessageProcessorFactory.GetMessageProcessor(message, loggerMock.Object);

		loggerMock
			.Verify(l => l.Log(It.Is<LogType>(t => t == LogType.Error), It.Is<string>(s => s.Contains($"EDIMessage with PK: [{message.PK}] with MessageType: [{UnknownMessageType}], Found no message processor."))),
				Times.Once);
	}

	const string UnknownMessageType = "UNW";
}
