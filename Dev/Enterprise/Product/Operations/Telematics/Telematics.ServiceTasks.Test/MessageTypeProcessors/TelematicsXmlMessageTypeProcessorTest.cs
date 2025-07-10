using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Integration;
using Enterprise.Telematics.ServiceTasks.MessageTypeProcessors;
using Enterprise.Telematics.ServiceTasks.TelematicsXmlMessageProcessors;
using Moq;

namespace Enterprise.Telematics.ServiceTasks.Test.MessageTypeProcessors
{
	public class TelematicsMessageTypeProcessorTest : TestCaseWithFactory
	{
		public void TestWrongConstructorParamsCall()
		{
			CombineAssertions(() =>
			{
				var result = AssertExceptionThrown<ArgumentNullException>(() => _ = new TelematicsXmlMessageTypeProcessor(null));
				AssertEquals("logger", result.ParamName);

				result = AssertExceptionThrown<ArgumentNullException>(() => _ = new TelematicsXmlMessageTypeProcessor(loggerMock.Object, null));
				AssertEquals("telematicsXmlMessageProcessors", result.ParamName);

				result = AssertExceptionThrown<ArgumentNullException>(() => _ = new TelematicsXmlMessageTypeProcessor(loggerMock.Object, (ITelematicsXmlMessageProcessor)null));
				AssertEquals("telematicsXmlMessageProcessors[0]", result.ParamName);

				result = AssertExceptionThrown<ArgumentNullException>(() => _ = new TelematicsXmlMessageTypeProcessor(loggerMock.Object, telematicsXmlMessageProcessorMocks[0].Object, null));
				AssertEquals("telematicsXmlMessageProcessors[1]", result.ParamName);
			});
		}

		public void TestWrongParamsCallProcess()
		{
			CombineAssertions(() =>
			{
				var result = AssertExceptionThrown<ArgumentNullException>(() => telematicsXmlMessageTypeProcessor.Process(null, string.Empty, string.Empty));
				AssertEquals("factory", result.ParamName);

				result = AssertExceptionThrown<ArgumentNullException>(() => telematicsXmlMessageTypeProcessor.Process(Factory, string.Empty, null));
				AssertEquals("messageText", result.ParamName);
			});
		}

		public void TestProcessCallsAllProcessors()
		{
			CombineAssertions(() =>
			{
				Test("<TelematicsXmlData>\r\n      \r\n    </TelematicsXmlData>", null);
				Test("<TelematicsXmlData>\r\n      <ServerRegistrationRequestMessage EHubId=\"TELMIDSERV\" />\r\n</TelematicsXmlData>", "<ServerRegistrationRequestMessage EHubId=\"TELMIDSERV\" />");
			});

			void Test(string xml, string message)
			{
				// Arrange
				foreach (var telematicsMessageProcessorMock in telematicsXmlMessageProcessorMocks)
				{
					telematicsMessageProcessorMock.Reset();
				}

				// Act
				telematicsXmlMessageTypeProcessor.Process(Factory, string.Empty, xml);

				// Assert
				AssertNoExceptionThrown(() =>
				{
					foreach (var telematicsMessageProcessorMock in telematicsXmlMessageProcessorMocks)
					{
						telematicsMessageProcessorMock.Verify(processor => processor.Process(It.IsAny<BusinessObjectFactory>(), It.IsAny<string>()), Times.Once);
						telematicsMessageProcessorMock.Verify(processor => processor.Process(Factory, message), Times.Once);
					}
				});
			}
		}

		public void TestProcessReturnsSumOfMessages()
		{
			// Arrange
			telematicsXmlMessageProcessorMocks[0]
				.Setup(processor => processor.Process(It.IsAny<BusinessObjectFactory>(), It.IsAny<string>()))
				.Returns(3);
			telematicsXmlMessageProcessorMocks[1]
				.Setup(processor => processor.Process(It.IsAny<BusinessObjectFactory>(), It.IsAny<string>()))
				.Returns(7);
			telematicsXmlMessageProcessorMocks[2]
				.Setup(processor => processor.Process(It.IsAny<BusinessObjectFactory>(), It.IsAny<string>()))
				.Returns(13);

			// Act
			var result = telematicsXmlMessageTypeProcessor.Process(Factory, string.Empty, "<TelematicsXmlData />");

			// Assert
			AssertEquals(23, result);
		}

		public void TestProcessIgnoresEmptyMessages()
		{
			// Arrange

			// Act
			var result = telematicsXmlMessageTypeProcessor.Process(Factory, string.Empty, string.Empty);

			// Assert
			AssertNoExceptionThrown(() =>
			{
				AssertEquals(0, result);
				foreach (var telematicsMessageProcessorMock in telematicsXmlMessageProcessorMocks)
				{
					telematicsMessageProcessorMock.Verify(processor => processor.Process(It.IsAny<BusinessObjectFactory>(), It.IsAny<string>()), Times.Never);
				}
			});
		}

		public void TestCallsTelematicsXmlMessageTypeProcessorsFactory()
		{
			// Arrange
			var factoryMock = new Mock<ITelematicsXmlMessageTypeProcessorsFactory>();
			using (ObjectFactory.Substitute(factoryMock.Object))
			{
				// Act
				_ = new TelematicsXmlMessageTypeProcessor(loggerMock.Object);
			}

			// Assert
			AssertNoExceptionThrown(() =>
			{
				factoryMock.Verify(factory => factory.GetProcessors(It.IsAny<ILogger>()), Times.Once);
				factoryMock.Verify(factory => factory.GetProcessors(loggerMock.Object), Times.Once);
				factoryMock.Verify(factory => factory.AddProcessorFunction(It.IsAny<Func<ILogger, ITelematicsXmlMessageProcessor>>()), Times.Never);
			});
		}

		public void TestMessageType()
		{
			AssertEquals("TXD", telematicsXmlMessageTypeProcessor.MessageType);
		}

		protected override void SetUp()
		{
			base.SetUp();

			loggerMock = new Mock<ILogger>();
			telematicsXmlMessageProcessorMocks = Enumerable.Range(0, 3).Select(i => new Mock<ITelematicsXmlMessageProcessor>()).ToArray();
			telematicsXmlMessageTypeProcessor = new TelematicsXmlMessageTypeProcessor(loggerMock.Object, telematicsXmlMessageProcessorMocks.Select(mock => mock.Object).ToArray());
		}

		Mock<ILogger> loggerMock;
		Mock<ITelematicsXmlMessageProcessor>[] telematicsXmlMessageProcessorMocks;
		TelematicsXmlMessageTypeProcessor telematicsXmlMessageTypeProcessor;
	}
}
