using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.Telematics.ServiceTasks.TelematicsXmlMessageProcessors;
using Moq;
using NUnit.Framework;

namespace Enterprise.Telematics.ServiceTasks.Test.TelematicsXmlMessageProcessors
{
	class TelematicsXmlMessageTypeProcessorsFactoryTest : TestCase
	{
		protected override void SetUp()
		{
			base.SetUp();
			telematicsXmlMessageTypeProcessorFactory = new TelematicsXmlMessageTypeProcessorsFactory();
			loggerMock = new Mock<ILogger>();
		}

		public void TestDefaultMessageProcessors()
		{
			var result = telematicsXmlMessageTypeProcessorFactory.GetProcessors(loggerMock.Object)
				.Select(processor => processor.GetType());

			AssertContainsExactElementsInAnyOrder(
				new[]
				{
					typeof(TelematicsDataMessageProcessor),
					typeof(TcaRegistrationMessageProcessor),
				},
				result);
		}

		public void TestAddsMessageProcessors()
		{
			// Arrange
			telematicsXmlMessageTypeProcessorFactory.AddProcessorFunction(logger => new XmlMessageProcessorStub(logger));
			telematicsXmlMessageTypeProcessorFactory.AddProcessorFunction(logger => new XmlMessageProcessorStub(logger));
			telematicsXmlMessageTypeProcessorFactory.AddProcessorFunction(logger => new XmlMessageProcessorStub(logger));

			// Act
			var result = telematicsXmlMessageTypeProcessorFactory.GetProcessors(loggerMock.Object)
				.OfType<XmlMessageProcessorStub>()
				.ToList();

			// Assert
			AssertEquals(3, result.Count);
			AssertContainsExactElementsInAnyOrder(Enumerable.Repeat(loggerMock.Object, 3), result.Select(stub => stub.Logger));
		}

		public void TestWrongParamsCall()
		{
			CombineAssertions(() =>
			{
				var result = AssertExceptionThrown<ArgumentNullException>(() => telematicsXmlMessageTypeProcessorFactory.AddProcessorFunction(null));
				AssertEquals("func", result.ParamName);

				result = AssertExceptionThrown<ArgumentNullException>(() => telematicsXmlMessageTypeProcessorFactory.GetProcessors(null));
				AssertEquals("logger", result.ParamName);
			});
		}

		Mock<ILogger> loggerMock;
		TelematicsXmlMessageTypeProcessorsFactory telematicsXmlMessageTypeProcessorFactory;

		class XmlMessageProcessorStub : ITelematicsXmlMessageProcessor
		{
			public XmlMessageProcessorStub(ILogger logger)
			{
				Logger = logger;
			}

			public ILogger Logger { get; }

			public int Process(BusinessObjectFactory factory, string messageText)
			{
				throw new NotImplementedException();
			}
		}
	}
}
