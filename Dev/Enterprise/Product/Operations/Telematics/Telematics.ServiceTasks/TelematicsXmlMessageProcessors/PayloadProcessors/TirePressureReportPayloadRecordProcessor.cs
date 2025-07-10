using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.Telematics.Business;
using Enterprise.Telematics.ServiceTasks.TelematicsEquipment;
using WTG.Telematics.Data.Packets.V2.Commercial.Ingoing;

namespace Enterprise.Telematics.ServiceTasks.TelematicsXmlMessageProcessors.PayloadProcessors
{
	class TirePressureReportPayloadRecordProcessor : IPayloadRecordProcessor<TirePressureReportPayloadRecord>
	{
		public TirePressureReportPayloadRecordProcessor(ILogger logger, IDbTreePlanner dbTreePlanner)
		{
			this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
			this.dbTreePlanner = dbTreePlanner ?? throw new ArgumentNullException(nameof(dbTreePlanner));
		}

		public int Process(BusinessObjectFactory factory, GlbDevice device, TirePressureReportPayloadRecord record)
		{
			var result = 0;
			var availableTpms = dbTreePlanner
				.GetSubEquipmentFromTree(factory, device.V3_HardwareIdentifier, record.DateTimeOffset, TelSubEquipmentTypeList.Codes.OBM);

			if (availableTpms.Count == 0)
			{
				return result;
			}

			foreach (var tpm in record.Tpms)
			{
				if (!availableTpms.TryGetValue(tpm.TpmId.ToString("X"), out var storedEquipment))
				{
					logger.Log(
						LogType.Warning,
						$"Received data for unknown Subequipment [DeviceId: {device.V3_HardwareIdentifier}, Id: {tpm.TpmId}, Type: {TelSubEquipmentTypeList.Codes.Wheel}].");
					continue;
				}

				var tirePressure = factory.New<GlbDeviceTyreReport>();
				tirePressure.GDR_V3_Device = device.PK;
				tirePressure.GDR_MeasurementTimeUtc = new ZDateTime(tpm.DateTimeOffset.UtcDateTime);
				tirePressure.GDR_TSE_SubEquipment = storedEquipment.Pk;
				tirePressure.GDR_PressureKPa = tpm.Pressure * PsiToKPaScalingFactor;
				tirePressure.GDR_TemperatureC = tpm.Temperature;
				result++;
			}

			return result;
		}

		readonly ILogger logger;
		readonly IDbTreePlanner dbTreePlanner;
		const double PsiToKPaScalingFactor = 6.895;
	}
}
