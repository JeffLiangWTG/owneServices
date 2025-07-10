using System;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.MobileServices.Common;
using CargoWise.MobileServices.Common.Messages.EHub;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.Telematics.Business;
using Enterprise.Telematics.Business.Registry;

namespace Enterprise.Telematics.ServiceTasks.MessageProcessing
{
	class M2CDeviceLocationDataNotificationProcessor : IMessageProcessor
	{
		public M2CDeviceLocationDataNotificationProcessor(ILogger logger)
		{
			this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		public EHubMessageType MessageType { get; } = EHubMessageType.M2CDeviceLocationDataNotification;

		public void Process(BusinessObjectFactory factory, string from, EHubMessageContainer messageContainer)
		{
			var message = messageContainer.GetInternalMessage<M2CDeviceLocationDataNotificationMessage>();

			if (!message.device_locations.Any())
			{
				return;
			}

			var device = GlbDevice.FindDeviceByMobileServicesIdentifier(factory, message.device_identifier);
			if (device == null)
			{
				logger.Log(LogType.Warning, FormattableString.Invariant($"Received location data for unknown device [{BitConverter.ToString(message.device_identifier).Replace("-", "")}]. Locations record(s) skipped: {message.device_locations.Count}."));
				return;
			}

			device.UpdateDeviceKey(message.device_key);

			foreach (var locationBatch in message.device_locations
				.Select((location, i) => new { location, groupId = i / 100 })
				.GroupBy(arg => arg.groupId, arg => arg.location))
			{
				foreach (var locationMessage in locationBatch)
				{
					var measurementTimeUtc = DateTimeExtensions.GetDateTime(locationMessage.time_from_utc);
					if (measurementTimeUtc > ZDateTime.UtcNow.AddHours(2))
					{
						ErrorReporter.ReportOnce(
							"M2CDeviceLocationDataNotificationProcessor_MeasurementTimeUtcInFuture",
							string.Format(
								"Location has time_from_utc in the future. time_from_utc: {0}, device.PK: {1}, device.V3_HumanReadableIdentifier: {2}, device.V3_HardwareIdentifier: {3}, device.V3_HardwareKind: {4}, accuracy_in_metres: {5}, compass_heading_degrees_decimal: {6}, longitude_decimal: {7}, latitude_decimal: {8}, altitude_in_metres_decimal: {9}, speed_kmph_decimal: {10}",
								measurementTimeUtc.ToString("u"), device.PK, device.V3_HumanReadableIdentifier, device.V3_HardwareIdentifier, device.V3_HardwareKind, (short)locationMessage.accuracy_in_metres, locationMessage.compass_heading_degrees_decimal, locationMessage.longitude_decimal, locationMessage.latitude_decimal, locationMessage.altitude_in_metres_decimal, locationMessage.speed_kmph_decimal
							)
						);
						continue;
					}

					var location = factory.New<GlbDeviceLocation>();

					location.V2_RoadType = TelematicsConfigurationRegistry.Instance.ProcessGlobalPositionDataOnRoadType.Value
						? GlbDeviceLocationRoadTypes.Codes.Unknown
						: GlbDeviceLocationRoadTypes.Codes.Public;
					location.V2_V3_Device = device.PK;
					location.V2_MeasurementTimeUtc = measurementTimeUtc;
					if (locationMessage.accuracy_in_metres > short.MaxValue)
					{
						logger.Log(LogType.Warning, FormattableString.Invariant($"Location has accuracy_in_metres too high: value is {locationMessage.accuracy_in_metres}, maximum is {short.MaxValue}. Value has been set to maximum."));
						location.V2_AccuracyInMetres = short.MaxValue;
					}
					else if (locationMessage.accuracy_in_metres < 0)
					{
						logger.Log(LogType.Warning, FormattableString.Invariant($"Location has accuracy_in_metres too low: value is {locationMessage.accuracy_in_metres}, minimum is 0. Value has been set to minimum."));
						location.V2_AccuracyInMetres = 0;
					}
					else
					{
						location.V2_AccuracyInMetres = (short)locationMessage.accuracy_in_metres;
					}
					SetCompassHeading(location, TryParseDecimal(locationMessage.compass_heading_degrees_decimal));
					location.V2_Location = ZGeography.CreatePoint(
						longitude: TryParseDouble(locationMessage.longitude_decimal),
						latitude: TryParseDouble(locationMessage.latitude_decimal),
						altitudeMetres: TryParseDouble(locationMessage.altitude_in_metres_decimal));
					location.V2_Speedkmh = TryParseDecimal(locationMessage.speed_kmph_decimal);
				}

				factory.Save();
			}
		}

		static void SetCompassHeading(GlbDeviceLocation location, decimal compassHeadingDegrees)
		{
			location.V2_CompassHeadingDegrees = compassHeadingDegrees;
			if (location.V2_CompassHeadingDegrees == 360.0m && compassHeadingDegrees < 360.0m)
			{
				location.V2_CompassHeadingDegrees = 0;
			}
		}

		static double TryParseDouble(string value, double defaultValue = 0d)
		{
			if (string.IsNullOrEmpty(value))
			{
				return defaultValue;
			}

			if (!double.TryParse(value, NumberStyles.Integer | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var result))
			{
				return defaultValue;
			}

			return result;
		}

		static decimal TryParseDecimal(string value, decimal defaultValue = 0m)
		{
			if (string.IsNullOrEmpty(value))
			{
				return defaultValue;
			}

			if (!decimal.TryParse(value, NumberStyles.Integer | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var result))
			{
				return defaultValue;
			}

			return result;
		}

		readonly ILogger logger;
	}
}
