using CargoWise.EntityFramework.Testing;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.MessageProcessors;
using Moq;

namespace Enterprise.Customs.PL.Business.Testing;

sealed class IncomingMessagesProcessingLegacyRouterTest : TestCaseWithFactory
{
	public void TestGetApplicationTypeProcessorCore_MessageTypeIsUnknown()
	{
		var message = Factory.New<EnterpriseEDIMessage>();
		AssertNull(router.GetApplicationTypeProcessorCore(message));
		messageProcessorFactoryResolver.Verify(resolver => resolver.ResolveFactory(It.IsAny<LoggingInformation>(), It.IsAny<BaseEDIMessage>()), Times.Never);
	}

	public void TestGetApplicationTypeProcessorCore_HandlesNullMessageProcessorFactory()
	{
		var message = Factory.New<EDIMessage>();
		var messageProcessorFactory = Mock.Of<Integration.Customs.PL.IMessageProcessorFactoryLegacy>();
		messageProcessorFactoryResolver.Setup(resolver => resolver.ResolveFactory(router.Logger, message)).Returns(messageProcessorFactory);
		AssertNull(router.GetApplicationTypeProcessorCore(message));
		messageProcessorFactoryResolver.Verify(resolver => resolver.ResolveFactory(router.Logger, message), Times.Once);
	}

	public void TestGetApplicationTypeProcessorCore_ResolvesMessageProcessor()
	{
		var message = Factory.New<EDIMessage>();
		var messageProcessorMock = new Mock<ApplicationTypeMessageProcessor>(router.Logger);
		var messageProcessorFactory = Mock.Of<Integration.Customs.PL.IMessageProcessorFactoryLegacy>(factory => factory.CreateProcessor(message) == messageProcessorMock.Object);
		messageProcessorFactoryResolver.Setup(resolver => resolver.ResolveFactory(router.Logger, message)).Returns(messageProcessorFactory);
		AssertSame(messageProcessorMock.Object, router.GetApplicationTypeProcessorCore(message));
	}

	protected override void SetUp()
	{
		base.SetUp();
		messageProcessorFactoryResolver = new();
		router = new(messageProcessorFactoryResolver.Object);
	}
	IncomingMessagesProcessingLegacyRouter router;
	Mock<IMessageProcessorFactoryLegacyResolver> messageProcessorFactoryResolver;
}
