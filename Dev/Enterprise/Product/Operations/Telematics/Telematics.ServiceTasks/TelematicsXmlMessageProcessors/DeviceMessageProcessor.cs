using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.Telematics.Business;
using Enterprise.Telematics.ServiceTasks.TelematicsEquipment;
using Enterprise.Telematics.ServiceTasks.TelematicsXmlMessageProcessors.PayloadProcessors;
using Enterprise.ZArchitecture.Schema;
using WTG.Telematics.Common.Validation;
using WTG.Telematics.Data.CargoWiseOne;
using WTG.Telematics.Data.Packets.V2.Commercial.Ingoing;
using WTG.Telematics.Interfaces.Packets.V2;

namespace Enterprise.Telematics.ServiceTasks.TelematicsXmlMessageProcessors
{
	class DeviceMessageProcessor : ITelematicsMessageProcessor<DeviceMessage>
	{
		public DeviceMessageProcessor(ILogger logger, IDbTreePlanner dbTreePlanner)
			: this(logger,
				new BatteryPayloadRecordProcessor(),
				new ConfigurationPayloadRecordProcessor(
					new VehicleConfigurationValidator(),
					new TelEquipmentTreeGenerator(
						new TelEdgeJsonEquipmentPlanner(),
						dbTreePlanner,
						logger),
					logger),
				new DeviceConnectionReportPayloadRecordProcessor(logger),
				new ExternalVoltagePayloadRecordProcessor(),
				new GpsPayloadRecordProcessor(),
				new IgnitionPayloadRecordProcessor(),
				new OdometerPayloadRecordProcessor(logger),
				new OnboardMassPayloadRecordProcessor(logger, dbTreePlanner),
				new TemperaturePayloadRecordProcessor(),
				new TirePressureReportPayloadRecordProcessor(logger, dbTreePlanner),
				new DeviceAlertPayloadRecordProcessor()
			)
		{
		}

		internal DeviceMessageProcessor(ILogger logger,
			IPayloadRecordProcessor<BatteryPayloadRecord> batteryPayloadRecordProcessor,
			IPayloadRecordProcessor<ConfigurationPayloadRecord> configurationPayloadRecordProcessor,
			IPayloadRecordProcessor<DeviceHeartbeatPayloadRecord> deviceConnectionReportPayloadRecordProcessor,
			IPayloadRecordProcessor<ExternalVoltagePayloadRecord> externalVoltagePayloadRecordProcessor,
			IPayloadRecordProcessor<GpsPayloadRecord> gpsPayloadRecordProcessor,
			IPayloadRecordProcessor<IgnitionPayloadRecord> ignitionPayloadRecordProcessor,
			IPayloadRecordProcessor<OdometerPayloadRecord> odometerPayloadRecordProcessor,
			IPayloadRecordProcessor<OnboardMassPayloadRecord> onboardMassPayloadRecordProcessor,
			IPayloadRecordProcessor<TemperaturePayloadRecord> temperaturePayloadRecordProcessor,
			IPayloadRecordProcessor<TirePressureReportPayloadRecord> tirePressureReportPayloadRecordProcessor,
			IPayloadRecordProcessor<DeviceAlertPayloadRecord> deviceAlertPayloadRecordProcessor)
		{
			this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
			this.batteryPayloadRecordProcessor = batteryPayloadRecordProcessor ?? throw new ArgumentNullException(nameof(batteryPayloadRecordProcessor));
			this.configurationPayloadRecordProcessor = configurationPayloadRecordProcessor ?? throw new ArgumentNullException(nameof(configurationPayloadRecordProcessor));
			this.deviceConnectionReportPayloadRecordProcessor = deviceConnectionReportPayloadRecordProcessor ?? throw new ArgumentNullException(nameof(deviceConnectionReportPayloadRecordProcessor));
			this.externalVoltagePayloadRecordProcessor = externalVoltagePayloadRecordProcessor ?? throw new ArgumentNullException(nameof(externalVoltagePayloadRecordProcessor));
			this.gpsPayloadRecordProcessor = gpsPayloadRecordProcessor ?? throw new ArgumentNullException(nameof(gpsPayloadRecordProcessor));
			this.ignitionPayloadRecordProcessor = ignitionPayloadRecordProcessor ?? throw new ArgumentNullException(nameof(ignitionPayloadRecordProcessor));
			this.odometerPayloadRecordProcessor = odometerPayloadRecordProcessor ?? throw new ArgumentNullException(nameof(odometerPayloadRecordProcessor));
			this.onboardMassPayloadRecordProcessor = onboardMassPayloadRecordProcessor ?? throw new ArgumentNullException(nameof(onboardMassPayloadRecordProcessor));
			this.temperaturePayloadRecordProcessor = temperaturePayloadRecordProcessor ?? throw new ArgumentNullException(nameof(temperaturePayloadRecordProcessor));
			this.tirePressureReportPayloadRecordProcessor = tirePressureReportPayloadRecordProcessor ?? throw new ArgumentNullException(nameof(tirePressureReportPayloadRecordProcessor));
			this.deviceAlertPayloadRecordProcessor = deviceAlertPayloadRecordProcessor ?? throw new ArgumentNullException(nameof(deviceAlertPayloadRecordProcessor));
		}

		static bool TryFindDevice(IFactory factory, string deviceId, out GlbDevice device)
		{
			device = factory
				.Load<GlbDevice>(new ZQuery(GlbDeviceSchema.V3_HardwareIdentifier, SQLComparisonOperator.Equal, deviceId))
				.SingleOrDefault();
			return device != null;
		}

		public int Process(BusinessObjectFactory factory, DeviceMessage message)
		{
			if (message == null)
			{
				return 0;
			}

			if (!TryFindDevice(factory, message.DeviceId, out var device))
			{
				logger.Log(LogType.Warning, FormattableString.Invariant($"Received data for unknown device [{message.DeviceId}]."));
				return 0;
			}

			var messages = message
				.Records
				.GroupBy(o => o.GetType(), o => o)
				.ToDictionary(objects => objects.Key, objects => objects.ToList());

			return ProcessMessages(factory, device, messages);
		}

		int ProcessMessages(BusinessObjectFactory factory, GlbDevice device, IDictionary<Type, List<object>> messages)
		{
			int ProcessRecordType<T>(BusinessObjectFactory objectFactory, GlbDevice reportingDevice, IDictionary<Type, List<object>> messageDict, IPayloadRecordProcessor<T> processor)
				where T : IPayloadRecord
			{
				return messageDict.TryGetValue(typeof(T), out var values)
					? values
						.Cast<T>()
						.Sum(record => processor.Process(objectFactory, reportingDevice, record))
					: 0;
			}

			return
				ProcessRecordType(factory, device, messages, batteryPayloadRecordProcessor) +
				ProcessRecordType(factory, device, messages, configurationPayloadRecordProcessor) +
				ProcessRecordType(factory, device, messages, deviceConnectionReportPayloadRecordProcessor) +
				ProcessRecordType(factory, device, messages, externalVoltagePayloadRecordProcessor) +
				ProcessRecordType(factory, device, messages, gpsPayloadRecordProcessor) +
				ProcessRecordType(factory, device, messages, ignitionPayloadRecordProcessor) +
				ProcessRecordType(factory, device, messages, odometerPayloadRecordProcessor) +
				ProcessRecordType(factory, device, messages, onboardMassPayloadRecordProcessor) +
				ProcessRecordType(factory, device, messages, temperaturePayloadRecordProcessor) +
				ProcessRecordType(factory, device, messages, tirePressureReportPayloadRecordProcessor) +
				ProcessRecordType(factory, device, messages, deviceAlertPayloadRecordProcessor);
		}

		readonly IPayloadRecordProcessor<BatteryPayloadRecord> batteryPayloadRecordProcessor;
		readonly IPayloadRecordProcessor<ConfigurationPayloadRecord> configurationPayloadRecordProcessor;
		readonly IPayloadRecordProcessor<DeviceHeartbeatPayloadRecord> deviceConnectionReportPayloadRecordProcessor;
		readonly IPayloadRecordProcessor<ExternalVoltagePayloadRecord> externalVoltagePayloadRecordProcessor;
		readonly IPayloadRecordProcessor<GpsPayloadRecord> gpsPayloadRecordProcessor;
		readonly IPayloadRecordProcessor<IgnitionPayloadRecord> ignitionPayloadRecordProcessor;
		readonly IPayloadRecordProcessor<OdometerPayloadRecord> odometerPayloadRecordProcessor;
		readonly IPayloadRecordProcessor<OnboardMassPayloadRecord> onboardMassPayloadRecordProcessor;
		readonly IPayloadRecordProcessor<TemperaturePayloadRecord> temperaturePayloadRecordProcessor;
		readonly IPayloadRecordProcessor<TirePressureReportPayloadRecord> tirePressureReportPayloadRecordProcessor;
		readonly IPayloadRecordProcessor<DeviceAlertPayloadRecord> deviceAlertPayloadRecordProcessor;
		readonly ILogger logger;
	}
}
