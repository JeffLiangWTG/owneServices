using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using CargoWise.EntityFramework.Testing;
using Enterprise.Integration;
using Enterprise.Telematics.Business;
using Enterprise.Telematics.ServiceTasks.TelematicsEquipment;
using Enterprise.Telematics.ServiceTasks.TelematicsXmlMessageProcessors.PayloadProcessors;
using Moq;
using Newtonsoft.Json;
using WTG.Telematics.Common.Conversion;
using WTG.Telematics.Common.Validation;
using WTG.Telematics.Data.Packets.V2.Commercial.Ingoing;

namespace Enterprise.Telematics.ServiceTasks.Test.TelematicsXmlMessageProcessors.PayloadProcessors
{
	public class ConfigurationPayloadRecordProcessorTest : TestCaseWithFactory
	{
		public void TestWrongConstructorParamsCall()
		{
			// Arrange
			// Act
			// Assert
			var result = AssertExceptionThrown<ArgumentNullException>(() => new ConfigurationPayloadRecordProcessor(null, dbTreeGeneratorMock.Object, loggerMock.Object));
			AssertEquals("vehicleConfigurationValidator", result.ParamName);

			result = AssertExceptionThrown<ArgumentNullException>(() => new ConfigurationPayloadRecordProcessor(vehicleConfigurationValidatorMock.Object, null, loggerMock.Object));
			AssertEquals("dbTreeGenerator", result.ParamName);

			result = AssertExceptionThrown<ArgumentNullException>(() => new ConfigurationPayloadRecordProcessor(vehicleConfigurationValidatorMock.Object, dbTreeGeneratorMock.Object, null));
			AssertEquals("logger", result.ParamName);
		}

		public void TestProcessLogsWarningIfReceivedInvalidData()
		{
			// Arrange
			var failureMessage = "Oh no!";
			var testConfiguration = "Blah";
			var config = new Dictionary<string, object> { { "VehicleInfo", testConfiguration } }.ToImmutableDictionary();
			var configurationJson = JsonConvert.SerializeObject(config["VehicleInfo"]);
			var message = CreateMessage(config, new DateTime(2015, 07, 17, 11, 34, 00, DateTimeKind.Utc));
			vehicleConfigurationValidatorMock.Setup(validator => validator.Validate(It.IsAny<string>())).Throws(new InvalidDataException(failureMessage));
			loggerMock.Setup(logger => logger.Log(It.IsAny<LogType>(), It.IsAny<string>()));

			// Act
			processor.Process(Factory, device, message);

			// Assert
			AssertNoExceptionThrown(() =>
			{
				loggerMock.Verify(logger => logger.Log(It.IsAny<LogType>(), It.IsAny<string>()), Times.Exactly(2));
				loggerMock.Verify(logger => logger.Log(LogType.Warning, $"Invalid configuration data received from device [010407FF]:{System.Environment.NewLine}{failureMessage}"), Times.Once);
				loggerMock.Verify(logger => logger.Log(LogType.Debug, $"Device [010407FF] sent a configuration:\r\n{testConfiguration}"), Times.Once);
			});
		}

		public void TestProcessSendsTheRightConfigurationToValidator()
		{
			CombineAssertions(() =>
			{
				Test(new Dictionary<string, object> { { "VehicleInfo", "010407FF" } }.ToImmutableDictionary());
				Test(new Dictionary<string, object> { { "VehicleInfo", "FFFFFFFF" } }.ToImmutableDictionary());
				Test(new Dictionary<string, object> { { "VehicleInfo", "AAAAAAAA" } }.ToImmutableDictionary());
			});

			void Test(IImmutableDictionary<string, object> configuration)
			{
				// Arrange
				vehicleConfigurationValidatorMock.Reset();
				vehicleConfigurationValidatorMock.SetupGet(validator => validator.ConfigurationHeader).Returns(new VehicleConfigurationValidator().ConfigurationHeader);
				var message = CreateMessage(configuration, new DateTime(2015, 07, 17, 11, 34, 00, DateTimeKind.Utc));
				vehicleConfigurationValidatorMock.Setup(validator => validator.Validate(It.IsAny<string>())).Throws(new InvalidDataException("Oh no!"));

				// Act
				processor.Process(Factory, device, message);

				// Assert
				AssertNoExceptionThrown(() =>
				{
					vehicleConfigurationValidatorMock.Verify(validator => validator.Validate(It.IsAny<string>()), Times.Once);
					vehicleConfigurationValidatorMock.Verify(validator => validator.Validate((string)configuration["VehicleInfo"]), Times.Once);
				});
			}
		}

		public void TestProcessRegeneratesEquipmentTreeIfConfigIsValid()
		{
			// Arrange
			var hardwareId = "0102030405060708090A0B0C";
			var config = new Dictionary<string, object> { { "VehicleInfo", hardwareId } }.ToImmutableDictionary();
			var time = new DateTime(2015, 07, 17, 11, 34, 00, DateTimeKind.Utc);
			var message = CreateMessage(config, time);
			vehicleConfigurationValidatorMock.Setup(validator => validator.Validate(It.IsAny<string>()));
			device.V3_HardwareIdentifier = hardwareId;

			var equipment = Factory.New<TelSubEquipment>();
			equipment.TSE_Configuration = "<EmptyXml />";
			equipment.TSE_Type = "RQ";
			equipment.TSE_Id = hardwareId;

			Factory.Save();

			// Act
			processor.Process(Factory, device, message);

			// Assert
			AssertNoExceptionThrown(() =>
			{
				dbTreeGeneratorMock.Verify(
					generator => generator.GenerateTree(
						Factory,
						It.Is<string>(value => value == hardwareId),
						time,
						hardwareId),
					Times.Once);
			});
		}

		static ConfigurationPayloadRecord CreateMessage(IImmutableDictionary<string, object> configuration, DateTimeOffset dateTimeOffset)
		{
			return new ConfigurationPayloadRecord
			{
				Configuration = configuration,
				DateTimeOffset = dateTimeOffset,
			};
		}

		protected override void SetUp()
		{
			base.SetUp();
			deviceIdentifier = new byte[] { 0x01, 0x04, 0x07, 0xFF }; // No significance

			device = Factory.New<GlbDevice>();
			device.V3_HumanReadableIdentifier = "TT00000001";
			device.V3_IsActive = true;
			device.V3_MobileServicesIdentifier = deviceIdentifier;
			device.V3_HardwareIdentifier = BinaryDataConverter.ByteArrayToHexString(deviceIdentifier);
			device.V3_Model = "GLaDOS v3.1";

			Factory.Save();

			loggerMock = new Mock<ILogger>();
			dbTreeGeneratorMock = new Mock<IDbTreeGenerator>();
			vehicleConfigurationValidatorMock = new Mock<IVehicleConfigurationValidator>();
			vehicleConfigurationValidatorMock.SetupGet(validator => validator.ConfigurationHeader).Returns(new VehicleConfigurationValidator().ConfigurationHeader);
			processor = new ConfigurationPayloadRecordProcessor(vehicleConfigurationValidatorMock.Object, dbTreeGeneratorMock.Object, loggerMock.Object);
		}

		GlbDevice device;
		byte[] deviceIdentifier;
		Mock<ILogger> loggerMock;
		ConfigurationPayloadRecordProcessor processor;
		Mock<IVehicleConfigurationValidator> vehicleConfigurationValidatorMock;
		Mock<IDbTreeGenerator> dbTreeGeneratorMock;
	}
}
