using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.Telematics.Business;
using Enterprise.Telematics.ServiceTasks.TelematicsEquipment;
using Enterprise.ZArchitecture.Schema;
using WTG.Telematics.Data.Packets.V2.Commercial.Ingoing;

namespace Enterprise.Telematics.ServiceTasks.TelematicsXmlMessageProcessors.PayloadProcessors
{
	class OnboardMassPayloadRecordProcessor : IPayloadRecordProcessor<OnboardMassPayloadRecord>
	{
		public OnboardMassPayloadRecordProcessor(ILogger logger, IDbTreePlanner dbTreePlanner)
		{
			this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
			this.dbTreePlanner = dbTreePlanner ?? throw new ArgumentNullException(nameof(dbTreePlanner));
		}

		public int Process(BusinessObjectFactory factory, GlbDevice device, OnboardMassPayloadRecord record)
		{
			var result = 0;
			var availableObms = dbTreePlanner
				.GetSubEquipmentFromTree(factory, device.V3_HardwareIdentifier, record.DateTimeOffset, TelSubEquipmentTypeList.Codes.OBM);

			if (availableObms.Count == 0)
			{
				return result;
			}

			foreach (var obm in record.Obms)
			{
				if (!availableObms.TryGetValue(obm.ObmId.ToString("X2"), out var storedEquipment))
				{
					logger.Log(
						LogType.Warning,
						$"Received data for unknown Subequipment [DeviceId: {device.V3_HardwareIdentifier}, Id: {obm.ObmId}, Type: {TelSubEquipmentTypeList.Codes.OBM}].");
					return 0;
				}

				var recordTime = new ZDateTime(obm.DateTimeOffset.UtcDateTime);
				var pressureKPa = new ZDecimal(obm.Pressure * PsiToKPaScalingFactor);
				var duplicateZQuery = new ZQuery(GlbDeviceOnboardMassSchema.GDM_V3_Device, device.PK);
				duplicateZQuery.AddToFilter(GlbDeviceOnboardMassSchema.GDM_TSE_SubEquipment, storedEquipment.Pk);
				duplicateZQuery.AddToFilter(GlbDeviceOnboardMassSchema.GDM_MeasurementTimeUtc, recordTime);
				duplicateZQuery.AddToFilter(GlbDeviceOnboardMassSchema.GDM_PressureKPa, SQLComparisonOperator.Equal, pressureKPa);

				if (!factory.Exists(typeof(GlbDeviceOnboardMass), duplicateZQuery))
				{
					var onboardMass = factory.New<GlbDeviceOnboardMass>();
					onboardMass.GDM_DeviationPercent = obm.DeviationPercent;
					onboardMass.GDM_MeasurementTimeUtc = new ZDateTime(obm.DateTimeOffset.UtcDateTime);
					onboardMass.GDM_PressureKPa = pressureKPa;
					onboardMass.GDM_V3_Device = device.PK;
					onboardMass.GDM_TSE_SubEquipment = storedEquipment.Pk;
					result++;
				}
				else
				{
					logger.Log(LogType.Debug, FormattableString.Invariant($"Received duplicate data for Device: {device.V3_HardwareIdentifier} on Obm: {obm.ObmId} with value: {obm.Pressure} at time: {obm.DateTimeOffset.ToString()}."));
				}
			}

			return result;
		}

		readonly ILogger logger;
		readonly IDbTreePlanner dbTreePlanner;
		const double PsiToKPaScalingFactor = 6.895;
	}
}
