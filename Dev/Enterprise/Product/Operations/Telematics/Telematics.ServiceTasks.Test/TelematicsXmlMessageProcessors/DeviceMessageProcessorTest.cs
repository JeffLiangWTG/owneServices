using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Integration;
using Enterprise.Telematics.Business;
using Enterprise.Telematics.ServiceTasks.TelematicsEquipment;
using Enterprise.Telematics.ServiceTasks.TelematicsXmlMessageProcessors;
using Enterprise.Telematics.ServiceTasks.TelematicsXmlMessageProcessors.PayloadProcessors;
using Moq;
using WTG.Telematics.Data.CargoWiseOne;
using WTG.Telematics.Data.Packets.V2.Commercial.Ingoing;
using WTG.Telematics.Interfaces.Packets.V2;

namespace Enterprise.Telematics.ServiceTasks.Test.TelematicsXmlMessageProcessors
{
	class DeviceMessageProcessorTest : TestCaseWithFactory
	{
		public void TestEmptyMessageReturnsInstantly()
		{
			// Arrange

			// Act
			var result = deviceMessageProcessor.Process(Factory, null);

			// Assert
			AssertEquals(0, result);
			loggerMock.Verify(logger => logger.Log(It.IsAny<LogType>(), It.IsAny<string>()), Times.Never);
			loggerMock.Verify(logger => logger.Log(It.IsAny<LogType>(), It.IsAny<string>(), It.IsAny<Exception>()), Times.Never);
			batteryPayloadRecordProcessorMock.Verify(processor => processor.Process(It.IsAny<BusinessObjectFactory>(), It.IsAny<GlbDevice>(), It.IsAny<BatteryPayloadRecord>()), Times.Never);
			configurationPayloadRecordProcessorMock.Verify(processor => processor.Process(It.IsAny<BusinessObjectFactory>(), It.IsAny<GlbDevice>(), It.IsAny<ConfigurationPayloadRecord>()), Times.Never);
			deviceConnectionReportPayloadRecordProcessorMock.Verify(processor => processor.Process(It.IsAny<BusinessObjectFactory>(), It.IsAny<GlbDevice>(), It.IsAny<DeviceHeartbeatPayloadRecord>()), Times.Never);
			externalVoltagePayloadRecordProcessorMock.Verify(processor => processor.Process(It.IsAny<BusinessObjectFactory>(), It.IsAny<GlbDevice>(), It.IsAny<ExternalVoltagePayloadRecord>()), Times.Never);
			gpsPayloadRecordProcessorMock.Verify(processor => processor.Process(It.IsAny<BusinessObjectFactory>(), It.IsAny<GlbDevice>(), It.IsAny<GpsPayloadRecord>()), Times.Never);
			ignitionPayloadRecordProcessorMock.Verify(processor => processor.Process(It.IsAny<BusinessObjectFactory>(), It.IsAny<GlbDevice>(), It.IsAny<IgnitionPayloadRecord>()), Times.Never);
			odometerPayloadRecordProcessorMock.Verify(processor => processor.Process(It.IsAny<BusinessObjectFactory>(), It.IsAny<GlbDevice>(), It.IsAny<OdometerPayloadRecord>()), Times.Never);
			onboardMassPayloadRecordProcessorMock.Verify(processor => processor.Process(It.IsAny<BusinessObjectFactory>(), It.IsAny<GlbDevice>(), It.IsAny<OnboardMassPayloadRecord>()), Times.Never);
			temperaturePayloadRecordProcessorMock.Verify(processor => processor.Process(It.IsAny<BusinessObjectFactory>(), It.IsAny<GlbDevice>(), It.IsAny<TemperaturePayloadRecord>()), Times.Never);
			tirePressureReportPayloadRecordProcessorMock.Verify(processor => processor.Process(It.IsAny<BusinessObjectFactory>(), It.IsAny<GlbDevice>(), It.IsAny<TirePressureReportPayloadRecord>()), Times.Never);
			deviceAlertPayloadRecordProcessorMock.Verify(processor => processor.Process(It.IsAny<BusinessObjectFactory>(), It.IsAny<GlbDevice>(), It.IsAny<DeviceAlertPayloadRecord>()), Times.Never);
		}

		public void TestEmptyDeviceReturnsInstantly()
		{
			// Arrange
			const string unknownDeviceId = "Unknown device Id";
			var deviceMessage = new DeviceMessage { DeviceId = unknownDeviceId };

			// Act
			var result = deviceMessageProcessor.Process(Factory, deviceMessage);

			// Assert
			AssertEquals(0, result);
			loggerMock.Verify(logger => logger.Log(It.IsAny<LogType>(), It.IsAny<string>()), Times.Once);
			loggerMock.Verify(logger => logger.Log(LogType.Warning, It.Is<string>(s => s.Contains(unknownDeviceId))), Times.Once);
			loggerMock.Verify(logger => logger.Log(It.IsAny<LogType>(), It.IsAny<string>(), It.IsAny<Exception>()), Times.Never);
			batteryPayloadRecordProcessorMock.Verify(processor => processor.Process(It.IsAny<BusinessObjectFactory>(), It.IsAny<GlbDevice>(), It.IsAny<BatteryPayloadRecord>()), Times.Never);
			configurationPayloadRecordProcessorMock.Verify(processor => processor.Process(It.IsAny<BusinessObjectFactory>(), It.IsAny<GlbDevice>(), It.IsAny<ConfigurationPayloadRecord>()), Times.Never);
			deviceConnectionReportPayloadRecordProcessorMock.Verify(processor => processor.Process(It.IsAny<BusinessObjectFactory>(), It.IsAny<GlbDevice>(), It.IsAny<DeviceHeartbeatPayloadRecord>()), Times.Never);
			externalVoltagePayloadRecordProcessorMock.Verify(processor => processor.Process(It.IsAny<BusinessObjectFactory>(), It.IsAny<GlbDevice>(), It.IsAny<ExternalVoltagePayloadRecord>()), Times.Never);
			gpsPayloadRecordProcessorMock.Verify(processor => processor.Process(It.IsAny<BusinessObjectFactory>(), It.IsAny<GlbDevice>(), It.IsAny<GpsPayloadRecord>()), Times.Never);
			ignitionPayloadRecordProcessorMock.Verify(processor => processor.Process(It.IsAny<BusinessObjectFactory>(), It.IsAny<GlbDevice>(), It.IsAny<IgnitionPayloadRecord>()), Times.Never);
			odometerPayloadRecordProcessorMock.Verify(processor => processor.Process(It.IsAny<BusinessObjectFactory>(), It.IsAny<GlbDevice>(), It.IsAny<OdometerPayloadRecord>()), Times.Never);
			onboardMassPayloadRecordProcessorMock.Verify(processor => processor.Process(It.IsAny<BusinessObjectFactory>(), It.IsAny<GlbDevice>(), It.IsAny<OnboardMassPayloadRecord>()), Times.Never);
			temperaturePayloadRecordProcessorMock.Verify(processor => processor.Process(It.IsAny<BusinessObjectFactory>(), It.IsAny<GlbDevice>(), It.IsAny<TemperaturePayloadRecord>()), Times.Never);
			tirePressureReportPayloadRecordProcessorMock.Verify(processor => processor.Process(It.IsAny<BusinessObjectFactory>(), It.IsAny<GlbDevice>(), It.IsAny<TirePressureReportPayloadRecord>()), Times.Never);
			deviceAlertPayloadRecordProcessorMock.Verify(processor => processor.Process(It.IsAny<BusinessObjectFactory>(), It.IsAny<GlbDevice>(), It.IsAny<DeviceAlertPayloadRecord>()), Times.Never);
		}

		public void TestMessagesAreEnumeratedOnce()
		{
			// Arrange
			var records = new List<object>
			{
				new BatteryPayloadRecord(),
				new ConfigurationPayloadRecord(),
				new DeviceHeartbeatPayloadRecord(),
				new ExternalVoltagePayloadRecord(),
				new GpsPayloadRecord(),
				new IgnitionPayloadRecord(),
				new OdometerPayloadRecord(),
				new OnboardMassPayloadRecord(),
				new TemperaturePayloadRecord(),
				new TirePressureReportPayloadRecord(),
				new DeviceAlertPayloadRecord(),
			};

			var recordsMock = new Mock<List<object>>();
			recordsMock
				.As<IEnumerable<object>>()
				.Setup(list => list.GetEnumerator())
				.Returns(() => records.AsEnumerable().GetEnumerator());

			var deviceMessage = new DeviceMessage
			{
				DeviceId = deviceId,
				Records = recordsMock.Object,
			};

			// Act
			var result = deviceMessageProcessor.Process(Factory, deviceMessage);

			// Assert
			AssertEquals(0, result);
			recordsMock.As<IEnumerable<object>>().Verify(list => list.GetEnumerator(), Times.Once);
		}

		public void TestCallsProcessors()
		{
			CombineAssertions(() =>
			{
				TestType(1, batteryPayloadRecordProcessorMock);
				TestType(3, batteryPayloadRecordProcessorMock);
				TestType(5, batteryPayloadRecordProcessorMock);

				TestType(1, configurationPayloadRecordProcessorMock);
				TestType(3, configurationPayloadRecordProcessorMock);
				TestType(5, configurationPayloadRecordProcessorMock);

				TestType(1, deviceConnectionReportPayloadRecordProcessorMock);
				TestType(3, deviceConnectionReportPayloadRecordProcessorMock);
				TestType(5, deviceConnectionReportPayloadRecordProcessorMock);

				TestType(1, externalVoltagePayloadRecordProcessorMock);
				TestType(3, externalVoltagePayloadRecordProcessorMock);
				TestType(5, externalVoltagePayloadRecordProcessorMock);

				TestType(1, gpsPayloadRecordProcessorMock);
				TestType(3, gpsPayloadRecordProcessorMock);
				TestType(5, gpsPayloadRecordProcessorMock);

				TestType(1, ignitionPayloadRecordProcessorMock);
				TestType(3, ignitionPayloadRecordProcessorMock);
				TestType(5, ignitionPayloadRecordProcessorMock);

				TestType(1, odometerPayloadRecordProcessorMock);
				TestType(3, odometerPayloadRecordProcessorMock);
				TestType(5, odometerPayloadRecordProcessorMock);

				TestType(1, onboardMassPayloadRecordProcessorMock);
				TestType(3, onboardMassPayloadRecordProcessorMock);
				TestType(5, onboardMassPayloadRecordProcessorMock);

				TestType(1, temperaturePayloadRecordProcessorMock);
				TestType(3, temperaturePayloadRecordProcessorMock);
				TestType(5, temperaturePayloadRecordProcessorMock);

				TestType(1, tirePressureReportPayloadRecordProcessorMock);
				TestType(3, tirePressureReportPayloadRecordProcessorMock);
				TestType(5, tirePressureReportPayloadRecordProcessorMock);

				TestType(1, deviceAlertPayloadRecordProcessorMock);
				TestType(3, deviceAlertPayloadRecordProcessorMock);
				TestType(5, deviceAlertPayloadRecordProcessorMock);
			});

			void TestType<T>(int count, Mock<IPayloadRecordProcessor<T>> processorMock) where T : IPayloadRecord, new()
			{
				// Arrange
				processorMock.Reset();
				processorMock
					.Setup(processor => processor.Process(It.IsAny<BusinessObjectFactory>(), It.IsAny<GlbDevice>(), It.IsAny<T>()))
					.Returns(1);
				var payloadRecords = Enumerable
					.Range(0, count)
					.Select(i => new T())
					.ToList();
				var deviceMessage = new DeviceMessage
				{
					DeviceId = deviceId,
					Records = payloadRecords.Cast<object>().ToList(),
				};

				// Act
				var result = deviceMessageProcessor.Process(Factory, deviceMessage);

				// Assert
				processorMock.Verify(processor => processor.Process(It.IsAny<BusinessObjectFactory>(), It.IsAny<GlbDevice>(), It.IsAny<T>()), Times.Exactly(count));
				foreach (var payloadRecord in payloadRecords)
				{
					processorMock.Verify(processor => processor.Process(Factory, device, payloadRecord), Times.Once);
				}

				AssertEquals(count, result);
			}
		}

		public void TestCallsProcessorsForCombinedPayload()
		{
			// Arrange
			var deviceMessage = new DeviceMessage
			{
				DeviceId = deviceId,
				Records = new List<object>
				{
					new BatteryPayloadRecord(),
					new ConfigurationPayloadRecord(),
					new DeviceHeartbeatPayloadRecord(),
					new ExternalVoltagePayloadRecord(),
					new GpsPayloadRecord(),
					new IgnitionPayloadRecord(),
					new OdometerPayloadRecord(),
					new OnboardMassPayloadRecord(),
					new TemperaturePayloadRecord(),
					new TirePressureReportPayloadRecord(),
					new DeviceAlertPayloadRecord(),
				},
			};

			// Act
			_ = deviceMessageProcessor.Process(Factory, deviceMessage);

			// Assert
			AssertNoExceptionThrown(() =>
			{
				AssertWasCalled(batteryPayloadRecordProcessorMock);
				AssertWasCalled(configurationPayloadRecordProcessorMock);
				AssertWasCalled(deviceConnectionReportPayloadRecordProcessorMock);
				AssertWasCalled(externalVoltagePayloadRecordProcessorMock);
				AssertWasCalled(gpsPayloadRecordProcessorMock);
				AssertWasCalled(ignitionPayloadRecordProcessorMock);
				AssertWasCalled(odometerPayloadRecordProcessorMock);
				AssertWasCalled(onboardMassPayloadRecordProcessorMock);
				AssertWasCalled(temperaturePayloadRecordProcessorMock);
				AssertWasCalled(tirePressureReportPayloadRecordProcessorMock);
				AssertWasCalled(deviceAlertPayloadRecordProcessorMock);
			});

			void AssertWasCalled<T>(Mock<IPayloadRecordProcessor<T>> processorMock) where T : IPayloadRecord
			{
				processorMock.Verify(processor => processor.Process(It.IsAny<BusinessObjectFactory>(), It.IsAny<GlbDevice>(), It.IsAny<T>()), Times.Once);
			}
		}

		public void WrongParamsCall()
		{
			CombineAssertions(() =>
			{
				var result = AssertExceptionThrown<ArgumentNullException>(() => new DeviceMessageProcessor(null, dbTreePlannerMock.Object));
				AssertEquals("logger", result.ParamName);

				result = AssertExceptionThrown<ArgumentNullException>(() => new DeviceMessageProcessor(loggerMock.Object, null));
				AssertEquals("logger", result.ParamName);

				result = AssertExceptionThrown<ArgumentNullException>(() => new DeviceMessageProcessor(null,
					batteryPayloadRecordProcessorMock.Object,
					configurationPayloadRecordProcessorMock.Object,
					deviceConnectionReportPayloadRecordProcessorMock.Object,
					externalVoltagePayloadRecordProcessorMock.Object,
					gpsPayloadRecordProcessorMock.Object,
					ignitionPayloadRecordProcessorMock.Object,
					odometerPayloadRecordProcessorMock.Object,
					onboardMassPayloadRecordProcessorMock.Object,
					temperaturePayloadRecordProcessorMock.Object,
					tirePressureReportPayloadRecordProcessorMock.Object,
					deviceAlertPayloadRecordProcessorMock.Object
				));
				AssertEquals("logger", result.ParamName);

				result = AssertExceptionThrown<ArgumentNullException>(() => new DeviceMessageProcessor(loggerMock.Object,
					null,
					configurationPayloadRecordProcessorMock.Object,
					deviceConnectionReportPayloadRecordProcessorMock.Object,
					externalVoltagePayloadRecordProcessorMock.Object,
					gpsPayloadRecordProcessorMock.Object,
					ignitionPayloadRecordProcessorMock.Object,
					odometerPayloadRecordProcessorMock.Object,
					onboardMassPayloadRecordProcessorMock.Object,
					temperaturePayloadRecordProcessorMock.Object,
					tirePressureReportPayloadRecordProcessorMock.Object,
					deviceAlertPayloadRecordProcessorMock.Object
				));
				AssertEquals("batteryPayloadRecordProcessor", result.ParamName);

				result = AssertExceptionThrown<ArgumentNullException>(() => new DeviceMessageProcessor(loggerMock.Object,
					batteryPayloadRecordProcessorMock.Object,
					null,
					deviceConnectionReportPayloadRecordProcessorMock.Object,
					externalVoltagePayloadRecordProcessorMock.Object,
					gpsPayloadRecordProcessorMock.Object,
					ignitionPayloadRecordProcessorMock.Object,
					odometerPayloadRecordProcessorMock.Object,
					onboardMassPayloadRecordProcessorMock.Object,
					temperaturePayloadRecordProcessorMock.Object,
					tirePressureReportPayloadRecordProcessorMock.Object,
					deviceAlertPayloadRecordProcessorMock.Object
				));
				AssertEquals("configurationPayloadRecordProcessor", result.ParamName);

				result = AssertExceptionThrown<ArgumentNullException>(() => new DeviceMessageProcessor(loggerMock.Object,
					batteryPayloadRecordProcessorMock.Object,
					configurationPayloadRecordProcessorMock.Object,
					null,
					externalVoltagePayloadRecordProcessorMock.Object,
					gpsPayloadRecordProcessorMock.Object,
					ignitionPayloadRecordProcessorMock.Object,
					odometerPayloadRecordProcessorMock.Object,
					onboardMassPayloadRecordProcessorMock.Object,
					temperaturePayloadRecordProcessorMock.Object,
					tirePressureReportPayloadRecordProcessorMock.Object,
					deviceAlertPayloadRecordProcessorMock.Object
				));
				AssertEquals("deviceConnectionReportPayloadRecordProcessor", result.ParamName);

				result = AssertExceptionThrown<ArgumentNullException>(() => new DeviceMessageProcessor(loggerMock.Object,
					batteryPayloadRecordProcessorMock.Object,
					configurationPayloadRecordProcessorMock.Object,
					deviceConnectionReportPayloadRecordProcessorMock.Object,
					null,
					gpsPayloadRecordProcessorMock.Object,
					ignitionPayloadRecordProcessorMock.Object,
					odometerPayloadRecordProcessorMock.Object,
					onboardMassPayloadRecordProcessorMock.Object,
					temperaturePayloadRecordProcessorMock.Object,
					tirePressureReportPayloadRecordProcessorMock.Object,
					deviceAlertPayloadRecordProcessorMock.Object
				));
				AssertEquals("externalVoltagePayloadRecordProcessor", result.ParamName);

				result = AssertExceptionThrown<ArgumentNullException>(() => new DeviceMessageProcessor(loggerMock.Object,
					batteryPayloadRecordProcessorMock.Object,
					configurationPayloadRecordProcessorMock.Object,
					deviceConnectionReportPayloadRecordProcessorMock.Object,
					externalVoltagePayloadRecordProcessorMock.Object,
					null,
					ignitionPayloadRecordProcessorMock.Object,
					odometerPayloadRecordProcessorMock.Object,
					onboardMassPayloadRecordProcessorMock.Object,
					temperaturePayloadRecordProcessorMock.Object,
					tirePressureReportPayloadRecordProcessorMock.Object,
					deviceAlertPayloadRecordProcessorMock.Object
				));
				AssertEquals("gpsPayloadRecordProcessor", result.ParamName);

				result = AssertExceptionThrown<ArgumentNullException>(() => new DeviceMessageProcessor(loggerMock.Object,
					batteryPayloadRecordProcessorMock.Object,
					configurationPayloadRecordProcessorMock.Object,
					deviceConnectionReportPayloadRecordProcessorMock.Object,
					externalVoltagePayloadRecordProcessorMock.Object,
					gpsPayloadRecordProcessorMock.Object,
					null,
					odometerPayloadRecordProcessorMock.Object,
					onboardMassPayloadRecordProcessorMock.Object,
					temperaturePayloadRecordProcessorMock.Object,
					tirePressureReportPayloadRecordProcessorMock.Object,
					deviceAlertPayloadRecordProcessorMock.Object
				));
				AssertEquals("ignitionPayloadRecordProcessor", result.ParamName);

				result = AssertExceptionThrown<ArgumentNullException>(() => new DeviceMessageProcessor(loggerMock.Object,
					batteryPayloadRecordProcessorMock.Object,
					configurationPayloadRecordProcessorMock.Object,
					deviceConnectionReportPayloadRecordProcessorMock.Object,
					externalVoltagePayloadRecordProcessorMock.Object,
					gpsPayloadRecordProcessorMock.Object,
					ignitionPayloadRecordProcessorMock.Object,
					null,
					onboardMassPayloadRecordProcessorMock.Object,
					temperaturePayloadRecordProcessorMock.Object,
					tirePressureReportPayloadRecordProcessorMock.Object,
					deviceAlertPayloadRecordProcessorMock.Object
				));
				AssertEquals("odometerPayloadRecordProcessor", result.ParamName);

				result = AssertExceptionThrown<ArgumentNullException>(() => new DeviceMessageProcessor(loggerMock.Object,
					batteryPayloadRecordProcessorMock.Object,
					configurationPayloadRecordProcessorMock.Object,
					deviceConnectionReportPayloadRecordProcessorMock.Object,
					externalVoltagePayloadRecordProcessorMock.Object,
					gpsPayloadRecordProcessorMock.Object,
					ignitionPayloadRecordProcessorMock.Object,
					odometerPayloadRecordProcessorMock.Object,
					null,
					temperaturePayloadRecordProcessorMock.Object,
					tirePressureReportPayloadRecordProcessorMock.Object,
					deviceAlertPayloadRecordProcessorMock.Object
				));
				AssertEquals("onboardMassPayloadRecordProcessor", result.ParamName);

				result = AssertExceptionThrown<ArgumentNullException>(() => new DeviceMessageProcessor(loggerMock.Object,
					batteryPayloadRecordProcessorMock.Object,
					configurationPayloadRecordProcessorMock.Object,
					deviceConnectionReportPayloadRecordProcessorMock.Object,
					externalVoltagePayloadRecordProcessorMock.Object,
					gpsPayloadRecordProcessorMock.Object,
					ignitionPayloadRecordProcessorMock.Object,
					odometerPayloadRecordProcessorMock.Object,
					onboardMassPayloadRecordProcessorMock.Object,
					null,
					tirePressureReportPayloadRecordProcessorMock.Object,
					deviceAlertPayloadRecordProcessorMock.Object
				));
				AssertEquals("temperaturePayloadRecordProcessor", result.ParamName);

				result = AssertExceptionThrown<ArgumentNullException>(() => new DeviceMessageProcessor(loggerMock.Object,
					batteryPayloadRecordProcessorMock.Object,
					configurationPayloadRecordProcessorMock.Object,
					deviceConnectionReportPayloadRecordProcessorMock.Object,
					externalVoltagePayloadRecordProcessorMock.Object,
					gpsPayloadRecordProcessorMock.Object,
					ignitionPayloadRecordProcessorMock.Object,
					odometerPayloadRecordProcessorMock.Object,
					onboardMassPayloadRecordProcessorMock.Object,
					temperaturePayloadRecordProcessorMock.Object,
					null,
					deviceAlertPayloadRecordProcessorMock.Object
				));
				AssertEquals("tirePressureReportPayloadRecordProcessor", result.ParamName);

				result = AssertExceptionThrown<ArgumentNullException>(() => new DeviceMessageProcessor(loggerMock.Object,
					batteryPayloadRecordProcessorMock.Object,
					configurationPayloadRecordProcessorMock.Object,
					deviceConnectionReportPayloadRecordProcessorMock.Object,
					externalVoltagePayloadRecordProcessorMock.Object,
					gpsPayloadRecordProcessorMock.Object,
					ignitionPayloadRecordProcessorMock.Object,
					odometerPayloadRecordProcessorMock.Object,
					onboardMassPayloadRecordProcessorMock.Object,
					temperaturePayloadRecordProcessorMock.Object,
					tirePressureReportPayloadRecordProcessorMock.Object,
					null
				));
				AssertEquals("deviceAlertPayloadRecordProcessor", result.ParamName);

				result = AssertExceptionThrown<ArgumentNullException>(() => deviceMessageProcessor.Process(null, new DeviceMessage()));
				AssertEquals("factory", result.ParamName);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			loggerMock = new Mock<ILogger>();

			batteryPayloadRecordProcessorMock = new Mock<IPayloadRecordProcessor<BatteryPayloadRecord>>();
			configurationPayloadRecordProcessorMock = new Mock<IPayloadRecordProcessor<ConfigurationPayloadRecord>>();
			deviceConnectionReportPayloadRecordProcessorMock = new Mock<IPayloadRecordProcessor<DeviceHeartbeatPayloadRecord>>();
			externalVoltagePayloadRecordProcessorMock = new Mock<IPayloadRecordProcessor<ExternalVoltagePayloadRecord>>();
			gpsPayloadRecordProcessorMock = new Mock<IPayloadRecordProcessor<GpsPayloadRecord>>();
			ignitionPayloadRecordProcessorMock = new Mock<IPayloadRecordProcessor<IgnitionPayloadRecord>>();
			odometerPayloadRecordProcessorMock = new Mock<IPayloadRecordProcessor<OdometerPayloadRecord>>();
			onboardMassPayloadRecordProcessorMock = new Mock<IPayloadRecordProcessor<OnboardMassPayloadRecord>>();
			temperaturePayloadRecordProcessorMock = new Mock<IPayloadRecordProcessor<TemperaturePayloadRecord>>();
			tirePressureReportPayloadRecordProcessorMock = new Mock<IPayloadRecordProcessor<TirePressureReportPayloadRecord>>();
			deviceAlertPayloadRecordProcessorMock = new Mock<IPayloadRecordProcessor<DeviceAlertPayloadRecord>>();
			dbTreePlannerMock = new Mock<IDbTreePlanner>();

			deviceMessageProcessor = new DeviceMessageProcessor(
				loggerMock.Object,
				batteryPayloadRecordProcessorMock.Object,
				configurationPayloadRecordProcessorMock.Object,
				deviceConnectionReportPayloadRecordProcessorMock.Object,
				externalVoltagePayloadRecordProcessorMock.Object,
				gpsPayloadRecordProcessorMock.Object,
				ignitionPayloadRecordProcessorMock.Object,
				odometerPayloadRecordProcessorMock.Object,
				onboardMassPayloadRecordProcessorMock.Object,
				temperaturePayloadRecordProcessorMock.Object,
				tirePressureReportPayloadRecordProcessorMock.Object,
				deviceAlertPayloadRecordProcessorMock.Object
			);

			deviceId = "DeviceId";
			device = Factory.New<GlbDevice>();
			device.V3_HumanReadableIdentifier = "TT00000001";
			device.V3_IsActive = true;
			device.V3_MobileServicesIdentifier = Array.Empty<byte>();
			device.V3_HardwareIdentifier = deviceId;
			device.V3_Model = string.Empty;
		}

		GlbDevice device;
		string deviceId;
		DeviceMessageProcessor deviceMessageProcessor;
		Mock<ILogger> loggerMock;
		Mock<IDbTreePlanner> dbTreePlannerMock;
		Mock<IPayloadRecordProcessor<BatteryPayloadRecord>> batteryPayloadRecordProcessorMock;
		Mock<IPayloadRecordProcessor<ConfigurationPayloadRecord>> configurationPayloadRecordProcessorMock;
		Mock<IPayloadRecordProcessor<DeviceHeartbeatPayloadRecord>> deviceConnectionReportPayloadRecordProcessorMock;
		Mock<IPayloadRecordProcessor<ExternalVoltagePayloadRecord>> externalVoltagePayloadRecordProcessorMock;
		Mock<IPayloadRecordProcessor<GpsPayloadRecord>> gpsPayloadRecordProcessorMock;
		Mock<IPayloadRecordProcessor<IgnitionPayloadRecord>> ignitionPayloadRecordProcessorMock;
		Mock<IPayloadRecordProcessor<OdometerPayloadRecord>> odometerPayloadRecordProcessorMock;
		Mock<IPayloadRecordProcessor<OnboardMassPayloadRecord>> onboardMassPayloadRecordProcessorMock;
		Mock<IPayloadRecordProcessor<TemperaturePayloadRecord>> temperaturePayloadRecordProcessorMock;
		Mock<IPayloadRecordProcessor<TirePressureReportPayloadRecord>> tirePressureReportPayloadRecordProcessorMock;
		Mock<IPayloadRecordProcessor<DeviceAlertPayloadRecord>> deviceAlertPayloadRecordProcessorMock;
	}
}
