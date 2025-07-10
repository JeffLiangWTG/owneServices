using System;
using System.IO;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.Telematics.Business;
using Enterprise.Telematics.ServiceTasks.TelematicsEquipment;
using WTG.Telematics.Common.Validation;
using WTG.Telematics.Data.Packets.V2.Commercial.Ingoing;

namespace Enterprise.Telematics.ServiceTasks.TelematicsXmlMessageProcessors.PayloadProcessors
{
	class ConfigurationPayloadRecordProcessor : IPayloadRecordProcessor<ConfigurationPayloadRecord>
	{
		public ConfigurationPayloadRecordProcessor(
			IVehicleConfigurationValidator vehicleConfigurationValidator,
			IDbTreeGenerator dbTreeGenerator,
			ILogger logger)
		{
			this.vehicleConfigurationValidator = vehicleConfigurationValidator ?? throw new ArgumentNullException(nameof(vehicleConfigurationValidator));
			this.dbTreeGenerator = dbTreeGenerator ?? throw new ArgumentNullException(nameof(dbTreeGenerator));
			this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		public int Process(BusinessObjectFactory factory, GlbDevice device, ConfigurationPayloadRecord record)
		{
			if (!record.Configuration.TryGetValue(vehicleConfigurationValidator.ConfigurationHeader, out var configurationJson))
			{
				return 0;
			}
			try
			{
				vehicleConfigurationValidator.Validate(configurationJson.ToString());
			}
			catch (InvalidDataException error)
			{
				logger.Log(LogType.Warning, FormattableString.Invariant($"Invalid configuration data received from device [{device.V3_HardwareIdentifier}]:{System.Environment.NewLine}{error.Message}"));
				logger.Log(LogType.Debug, FormattableString.Invariant($"Device [{device.V3_HardwareIdentifier}] sent a configuration:\r\n{configurationJson}"));
				return 0;
			}

			var time = record.DateTimeOffset;
			dbTreeGenerator.GenerateTree(factory, device.V3_HardwareIdentifier, time, configurationJson.ToString());
			return 1;
		}

		readonly IVehicleConfigurationValidator vehicleConfigurationValidator;
		readonly IDbTreeGenerator dbTreeGenerator;
		readonly ILogger logger;
	}
}
