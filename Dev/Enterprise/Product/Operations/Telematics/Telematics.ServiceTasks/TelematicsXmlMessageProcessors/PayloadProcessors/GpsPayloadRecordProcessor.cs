using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Telematics.Business;
using Enterprise.Telematics.Business.Registry;
using WTG.Telematics.Data.Packets.V2.Commercial.Ingoing;

namespace Enterprise.Telematics.ServiceTasks.TelematicsXmlMessageProcessors.PayloadProcessors
{
	class GpsPayloadRecordProcessor : IPayloadRecordProcessor<GpsPayloadRecord>
	{
		public int Process(BusinessObjectFactory factory, GlbDevice device, GpsPayloadRecord record)
		{
			var measurementTimeUtc = record.DateTimeOffset.UtcDateTime;

			if (measurementTimeUtc > ZDateTime.UtcNow.AddMinutes(1))
			{
				ErrorReporter.ReportOnce(
					"GpsPayloadRecordProcessor_MeasurementTimeUtcInFuture",
					string.Format(
						"Location has V2_MeasurementTimeUtc in the future. V2_MeasurementTimeUtc: {0}, device.PK: {1}, device.V3_HumanReadableIdentifier: {2}, device.V3_HardwareIdentifier: {3}, device.V3_HardwareKind: {4}, V2_AccuracyInMetres: {5}, V2_CompassHeadingDegrees: {6}, V2_HDOP: {7}, V2_Location: {8}, V2_SatelliteQuantity: {9}, V2_Speedkmh: {10}",
						measurementTimeUtc.ToString("u"), device.PK, device.V3_HumanReadableIdentifier, device.V3_HardwareIdentifier, device.V3_HardwareKind, Convert.ToInt16(record.AccuracyM), record.Course, record.HDop, ZGeography.CreatePoint(record.Longitude, record.Latitude, record.Altitude), Convert.ToByte(record.NumberOfSatellites), record.Speed
					)
				);
				return 0;
			}

			var location = factory.New<GlbDeviceLocation>();
			location.V2_RoadType = TelematicsConfigurationRegistry.Instance.ProcessGlobalPositionDataOnRoadType.Value
				? GlbDeviceLocationRoadTypes.Codes.Unknown
				: GlbDeviceLocationRoadTypes.Codes.Public;

			location.V2_V3_Device = device.PK;
			location.V2_MeasurementTimeUtc = measurementTimeUtc;
			location.V2_AccuracyInMetres = Convert.ToInt16(record.AccuracyM);
			location.V2_CompassHeadingDegrees = record.Course;
			location.V2_HDOP = record.HDop;
			location.V2_Location = ZGeography.CreatePoint(record.Longitude, record.Latitude, record.Altitude);
			location.V2_SatelliteQuantity = Convert.ToByte(record.NumberOfSatellites);
			location.V2_Speedkmh = record.Speed;
			return 1;
		}
	}
}
