using System;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.Telematics.ServiceTasks.TelematicsXmlMessageProcessors;
using Moq;
using NUnit.Framework;
using WTG.Telematics.Common;
using WTG.Telematics.Data.CargoWiseOne;

namespace Enterprise.Telematics.ServiceTasks.Test.TelematicsXmlMessageProcessors
{
	class TelematicsDataMessageProcessorTest : TestCase
	{
		protected override void SetUp()
		{
			base.SetUp();
			loggerMock = new Mock<ILogger>();
			deviceMessageProcessorMock = new Mock<ITelematicsMessageProcessor<DeviceMessage>>();
			telematicsDataMessageProcessor = new TelematicsDataMessageProcessor(loggerMock.Object, deviceMessageProcessorMock.Object);
		}

		public void TestWrongParamsCall()
		{
			CombineAssertions(() =>
			{
				var result = AssertExceptionThrown<ArgumentNullException>(() => new TelematicsDataMessageProcessor(null));
				AssertEquals("logger", result.ParamName);

				result = AssertExceptionThrown<ArgumentNullException>(() => new TelematicsDataMessageProcessor(null, deviceMessageProcessorMock.Object));
				AssertEquals("logger", result.ParamName);

				result = AssertExceptionThrown<ArgumentNullException>(() => new TelematicsDataMessageProcessor(loggerMock.Object, null));
				AssertEquals("deviceMessageProcessor", result.ParamName);

				result = AssertExceptionThrown<ArgumentNullException>(() => telematicsDataMessageProcessor.Process(null, string.Empty));
				AssertEquals("factory", result.ParamName);
			});
		}

		public void TestEmptyMessageReturnsInstantly()
		{
			CombineAssertions(() =>
			{
				Test(null);
				Test(string.Empty);
				Test("very weird message");
				Test("<TelematicsDataMessage />");
				Test("<TelematicsDataMessage></TelematicsDataMessage>");
			});

			void Test(string messageText)
			{
				// Arrange
				deviceMessageProcessorMock.Reset();
				loggerMock.Reset();
				var factory = new BusinessObjectFactory();

				// Act
				var result = telematicsDataMessageProcessor.Process(factory, messageText);

				// Assert
				AssertEquals(0, result);
				deviceMessageProcessorMock.Verify(processor => processor.Process(It.IsAny<BusinessObjectFactory>(), It.IsAny<DeviceMessage>()), Times.Never);
				loggerMock.Verify(logger => logger.Log(It.IsAny<LogType>(), It.IsAny<string>()), Times.Never);
				loggerMock.Verify(logger => logger.Log(It.IsAny<LogType>(), It.IsAny<string>(), It.IsAny<Exception>()), Times.Never);
			}
		}

		public void TestCallsProcessor()
		{
			CombineAssertions(() =>
			{
				Test("<TelematicsDataMessage>\r\n\t<DeviceMessages>\r\n\t\t<DeviceMessage DeviceId=\"01020304\">\r\n\t\t\t<GpsPayloadRecord DateTimeOffset=\"2020-01-02 03:04:05.006 +10:00\" Longitude=\"2\" Latitude=\"1\" Altitude=\"4\" AccuracyM=\"3\" Speed=\"7\" Course=\"5\" NumberOfSatellites=\"8\" HDop=\"6\" />\r\n\t\t</DeviceMessage>\r\n\t</DeviceMessages>\r\n</TelematicsDataMessage>", 3);
				Test("<TelematicsDataMessage>\r\n\t<DeviceMessages>\r\n\t\t<DeviceMessage DeviceId=\"020304\">\r\n\t\t\t<OdometerPayloadRecord DateTimeOffset=\"2020-01-02 03:04:05.006 +11:00\" Odometer=\"0\" />\r\n\t\t</DeviceMessage>\r\n\t</DeviceMessages>\r\n</TelematicsDataMessage>", 5);
				Test("<TelematicsDataMessage>\r\n\t<DeviceMessages>\r\n\t\t<DeviceMessage DeviceId=\"0304\">\r\n\t\t\t<BatteryPayloadRecord DateTimeOffset=\"2020-01-02 03:04:05.006 +11:00\" IsCharging=\"false\" Voltage=\"3\" Current=\"1\" Temperature=\"2\" ChargeRemaining=\"0\" />\r\n\t\t</DeviceMessage>\r\n\t</DeviceMessages>\r\n</TelematicsDataMessage>", 5);
				Test("<TelematicsDataMessage>\r\n\t<DeviceMessages>\r\n\t\t<DeviceMessage DeviceId=\"04030201\">\r\n\t\t\t<IgnitionPayloadRecord DateTimeOffset=\"2016-02-03 04:05:06.007 +10:00\" IgnitionState=\"true\" />\r\n\t\t\t<OnboardMassPayloadRecord DateTimeOffset=\"2016-02-03 04:05:06.007 +10:00\">\r\n\t\t\t\t<Obms>\r\n\t\t\t\t\t<ObmData DateTimeOffset=\"2016-02-03 04:05:06.007 +10:00\" RsaId=\"01020304\" ObmId=\"2\" Status=\"AxleRaised\" Pressure=\"3\" DeviationPercent=\"1\" />\r\n\t\t\t\t\t<ObmData DateTimeOffset=\"2016-02-03 04:05:06.007 +10:00\" RsaId=\"04030201\" ObmId=\"3\" Status=\"Disconnected\" Pressure=\"4\" DeviationPercent=\"2\" />\r\n\t\t\t\t</Obms>\r\n\t\t\t</OnboardMassPayloadRecord>\r\n\t\t</DeviceMessage>\r\n\t</DeviceMessages>\r\n</TelematicsDataMessage>", 5);
			});

			void Test(string messageText, int count)
			{
				// Arrange
				deviceMessageProcessorMock.Reset();
				loggerMock.Reset();
				var factory = new BusinessObjectFactory();
				string deviceMessage = null;
				deviceMessageProcessorMock
					.Setup(processor => processor.Process(It.IsAny<BusinessObjectFactory>(), It.IsAny<DeviceMessage>()))
					.Returns(count)
					.Callback<BusinessObjectFactory, DeviceMessage>((objectFactory, message) => deviceMessage = XmlDataSerializer.Serialize(message).Replace("\t", string.Empty));

				// Act
				var result = telematicsDataMessageProcessor.Process(factory, messageText);

				// Assert
				AssertEquals(count, result);
				deviceMessageProcessorMock.Verify(processor => processor.Process(It.IsAny<BusinessObjectFactory>(), It.IsAny<DeviceMessage>()), Times.Once);
				AssertXMLContains(deviceMessage, messageText.Replace("\t", string.Empty));
				loggerMock.Verify(logger => logger.Log(It.IsAny<LogType>(), It.IsAny<string>()), Times.Never);
				loggerMock.Verify(logger => logger.Log(It.IsAny<LogType>(), It.IsAny<string>(), It.IsAny<Exception>()), Times.Never);
			}
		}

		public void TestCallsProcessorSeveralTimes()
		{
			// Arrange
			var factory = new BusinessObjectFactory();
			deviceMessageProcessorMock
				.SetupSequence(processor => processor.Process(It.IsAny<BusinessObjectFactory>(), It.IsAny<DeviceMessage>()))
				.Returns(1)
				.Returns(3)
				.Returns(7);
			const string messageText = "<TelematicsDataMessage>\r\n\t<DeviceMessages>\r\n" +
										"\t\t<DeviceMessage DeviceId=\"01020304\">\r\n\t\t\t<GpsPayloadRecord DateTimeOffset=\"2020-01-02 03:04:05.006 +10:00\" Longitude=\"2\" Latitude=\"1\" Altitude=\"4\" AccuracyM=\"3\" Speed=\"7\" Course=\"5\" NumberOfSatellites=\"8\" HDop=\"6\" />\r\n\t\t</DeviceMessage>\r\n" +
										"\t\t<DeviceMessage DeviceId=\"05060708\">\r\n\t\t\t<GpsPayloadRecord DateTimeOffset=\"2020-01-02 03:04:05.006 +10:00\" Longitude=\"2\" Latitude=\"1\" Altitude=\"4\" AccuracyM=\"3\" Speed=\"7\" Course=\"5\" NumberOfSatellites=\"8\" HDop=\"6\" />\r\n\t\t</DeviceMessage>\r\n" +
										"\t\t<DeviceMessage DeviceId=\"ABC\">\r\n\t\t\t<GpsPayloadRecord DateTimeOffset=\"2020-01-02 03:04:05.006 +10:00\" Longitude=\"2\" Latitude=\"1\" Altitude=\"4\" AccuracyM=\"3\" Speed=\"7\" Course=\"5\" NumberOfSatellites=\"8\" HDop=\"6\" />\r\n\t\t</DeviceMessage>\r\n" +
										"\t</DeviceMessages>\r\n</TelematicsDataMessage>";

			// Act
			var result = telematicsDataMessageProcessor.Process(factory, messageText);

			// Assert
			AssertEquals(11, result);
			deviceMessageProcessorMock.Verify(processor => processor.Process(It.IsAny<BusinessObjectFactory>(), It.IsAny<DeviceMessage>()), Times.Exactly(3));
			loggerMock.Verify(logger => logger.Log(It.IsAny<LogType>(), It.IsAny<string>()), Times.Never);
			loggerMock.Verify(logger => logger.Log(It.IsAny<LogType>(), It.IsAny<string>(), It.IsAny<Exception>()), Times.Never);
		}

		Mock<ITelematicsMessageProcessor<DeviceMessage>> deviceMessageProcessorMock;
		Mock<ILogger> loggerMock;
		TelematicsDataMessageProcessor telematicsDataMessageProcessor;
	}
}
