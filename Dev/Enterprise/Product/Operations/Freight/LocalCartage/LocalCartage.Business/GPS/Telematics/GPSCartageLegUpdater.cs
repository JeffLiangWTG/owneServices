using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.LocalCartage.Integration;
using Enterprise.GPS.Business;
using Enterprise.Integration.GPS;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Telematics.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.LocalCartage.Business.GPS
{
	public sealed class GPSCartageLegUpdater : IGPSCartageLegUpdater
	{
		public GPSCartageLegUpdater(BusinessObjectFactory factory)
		{
			this.factory = factory;
		}

		public Action<BusinessObjectFactory> CreateCurrencyDataForTest;

		public void Process(INotifications notifications, CancellationToken token)
		{
			if (LocalCartageDataRegistry.Instance.UseGlbDeviceLocationSubscriber.Value)
			{
				return;
			}
			notifications.Notify(new InfoNotification(Res.GetString("0674a6ad-48dc-4c83-9c50-960011283cc8", "Begin: Running GPS Cartage Leg Updater Processor.")));

			var savedLastProcessedUTCTime = (ZDateTime)LocalCartageDataRegistry.Instance.GetLastProcessedEventTime.Value;
			var lastProccessedEventUTCTime = savedLastProcessedUTCTime.IsValid ? savedLastProcessedUTCTime : ZDateTime.UtcToday;
			if (lastProccessedEventUTCTime.IsValid)
			{
				var locations = GetLocationsToProcess(notifications, lastProccessedEventUTCTime);
				if (locations.Any())
				{
					try
					{
						foreach (var location in locations)
						{
							token.ThrowIfCancellationRequested();
							ProcessLocation(location);
						}

#if DEBUG
						if (CreateCurrencyDataForTest != null)
						{
							CreateCurrencyDataForTest(factory);
						}
#endif

						factory.Save();

						UpdateLastProcessedEventTime(locations.Last().MeasurementTimeUtc);
					}
					catch (ZSaveConcurrencyException ex)
					{
						notifications.Notify(new InfoNotification(ex.Message));
					}
				}
			}

			notifications.Notify(new InfoNotification(Res.GetString("a21f9524-aa21-4af8-b65e-7d9f382d1078", "End: Running GPS Cartage Leg Updater Processor.")));
		}

		public void UpdateLastProcessedEventTime(ZDateTime dateTime)
		{
			var currentLastProcessedEventTime = (ZDateTime)LocalCartageDataRegistry.Instance.GetLastProcessedEventTime.Value;
			if (!currentLastProcessedEventTime.IsValid || dateTime > currentLastProcessedEventTime)
			{
				LocalCartageDataRegistry.Instance.GetLastProcessedEventTime.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, dateTime.ToDateTime());
			}
		}

		public void ProcessLocation(IDeviceLocationWithEntity location)
		{
			var localEventTime = EnvProxy.Instance.Time.GetLocalTimeFromUtc(location.MeasurementTimeUtc.ToDateTime());
			var runSheet = GetRunSheet(location, localEventTime);
			if (runSheet != null)
			{
				ProcessLocation(runSheet, location, localEventTime);
			}
		}

		void ProcessLocation(CommonWorkSheet runSheet, IDeviceLocationWithEntity location, DateTime locationLocalEventTime)
		{
			var currentAddresses = new List<LegSegment>();
			var nextAddresses = new List<LegSegment>();
			PopulateCurrentAndNextAddresses(runSheet, currentAddresses, nextAddresses);

			var allCurrentAndNextAddresses = currentAddresses.Concat(nextAddresses);
			TrySetInAndOutTimesForCurrentAddresses(location, locationLocalEventTime, runSheet, currentAddresses, allCurrentAndNextAddresses);

			// when a Next Address is really close to a Current Address (within 100m), we will allow a Time In on the Next Address to 
			// be set before it's preceeding addresses are completed.
			TrySetInTimesForReallyCloseNextAddresses(location, locationLocalEventTime, runSheet, nextAddresses, allCurrentAndNextAddresses);

			// if we completed (Timed Out) of some Current Addresses, the same event could be used to Time In on one of their Next Addresses
			if (currentAddresses.Any(a => a.HasTimeInAndOut))
			{
				ProcessLocation(runSheet, location, locationLocalEventTime);
			}
		}

		/// <summary>
		/// Current Addresses to contain all started Addresses, or the first not started address (for each Leg in the Group).
		/// Next Addresses to contain second incomplete Address that doesn't have time in (for each Leg in the Group, may need to use 2nd leg).  
		/// </summary>
		static void PopulateCurrentAndNextAddresses(CommonWorkSheet runSheet, List<LegSegment> currentAddresses, List<LegSegment> nextAddresses)
		{
			var availableLegs = GetOrderedAvailableLegs(runSheet);
			var availableSequences = availableLegs.Select(l => l.Sequence).Distinct().OrderBy(s => s);
			var lowestSequence = availableSequences.FirstOrDefault();
			var secondLowestSequence = availableSequences.Skip(1).FirstOrDefault();

			var firstLegs = availableLegs.Where(l => l.Sequence == lowestSequence || l.Pickup.HasTimeInOnly || l.Delivery.HasTimeInOnly || l.WaitPoint.HasTimeInOnly);
			var secondLegs = secondLowestSequence != lowestSequence ? availableLegs.Where(l => l.Sequence == secondLowestSequence) : Array.Empty<Leg>();

			foreach (var firstLeg in firstLegs)
			{
				bool firstPassCompleted = false;
				if (secondLegs.Any())
				{
					foreach (var secondLeg in secondLegs)
					{
						PopulateCurrentAndNextAddresses(firstPassCompleted, currentAddresses, nextAddresses, firstLeg, secondLeg);
					}
				}
				else
				{
					PopulateCurrentAndNextAddresses(firstPassCompleted, currentAddresses, nextAddresses, firstLeg);
				}
			}
		}

		/// <summary>
		/// Current Addresses to contain all started Addresses, or the first not started address.
		/// Next Addresses to contain second incomplete Address that doesn't have time in (may need to use 2nd leg). 
		/// </summary>
		static void PopulateCurrentAndNextAddresses(bool firstPassCompleted, List<LegSegment> currentAddresses, List<LegSegment> nextAddresses, Leg firstLeg, Leg secondLeg = null)
		{
			var currents = new List<LegSegment>();

			LegSegment previousAddress = null;
			var orderedAddresses = GetOrderedAddresses(firstLeg, secondLeg);
			foreach (var possibleAddress in orderedAddresses.Where(a => !a.HasTimeInAndOut))
			{
				if (previousAddress != null && previousAddress.IsSameAddress(possibleAddress) && previousAddress.Leg != possibleAddress.Leg)
				{
					previousAddress.GroupedAddresses.Add(possibleAddress);
				}
				else
				{
					if (firstPassCompleted || possibleAddress.HasValidGeoLocation)
					{
						if (!currents.Any())
						{
							currents.Add(possibleAddress);
						}
						else if (possibleAddress.HasTimeInOnly)
						{
							// leg could have more than 1 concecutive address with a time in (when they are really close together). Need to treat them both as Current.
							currents.Add(possibleAddress);
						}
						else
						{
							nextAddresses.Add(possibleAddress);
							possibleAddress.PreceedingAddresses.AddRange(currents);
							break;
						}
					}
					previousAddress = possibleAddress;
				}
			}

			currentAddresses.AddRange(currents);
		}

		void TrySetInAndOutTimesForCurrentAddresses(IDeviceLocationWithEntity location, DateTime localEventTime, CommonWorkSheet runSheet, IEnumerable<LegSegment> currentAddresses, IEnumerable<LegSegment> otherAddresses)
		{
			foreach (var currentAddress in currentAddresses)
			{
				var hasNoTimeIn = currentAddress.HasNoTimeIn;
				var hasTimeInOnly = currentAddress.HasTimeInOnly;

				if (hasNoTimeIn || hasTimeInOnly)
				{
					var isInCurrentAddress = IsEventWithinAddress(location, currentAddress, otherAddresses);
					if (hasNoTimeIn && isInCurrentAddress)
					{
						SetTime(location, localEventTime, currentAddress, GPSConstants.GPSInOutActivityType.Codes.GIN, runSheet.EY_RunSheetNumber);
					}
					else if (hasTimeInOnly && !isInCurrentAddress)
					{
						SetTime(location, localEventTime, currentAddress, GPSConstants.GPSInOutActivityType.Codes.GOT, runSheet.EY_RunSheetNumber);
					}
				}
			}
		}

		void TrySetInTimesForReallyCloseNextAddresses(IDeviceLocationWithEntity location, DateTime localEventTime, CommonWorkSheet runSheet, IEnumerable<LegSegment> nextAddresses, IEnumerable<LegSegment> otherAddresses)
		{
			foreach (var nextAddress in nextAddresses)
			{
				var hasNotStartedAndAllItsPreceedingAddressesHaveATimeIn = nextAddress.HasNoTimeIn && nextAddress.PreceedingAddresses.All(p => !p.TimeIn.IsEmpty);
				if (hasNotStartedAndAllItsPreceedingAddressesHaveATimeIn)
				{
					var couldThisGeofenceBeOverlappingACurrentAddress = GetGeofenceRadius(nextAddress, otherAddresses) == MinimumGeofenceRadiusInMeters;
					if (couldThisGeofenceBeOverlappingACurrentAddress)
					{
						var isInNextAddress = IsEventWithinAddress(location, nextAddress, otherAddresses);
						if (isInNextAddress)
						{
							SetTime(location, localEventTime, nextAddress, GPSConstants.GPSInOutActivityType.Codes.GIN, runSheet.EY_RunSheetNumber);
						}
					}
				}
			}
		}

		void SetTime(IDeviceLocationWithEntity location, DateTime localEventTime, LegSegment currentAddress, ZString activityType, ZString runSheetNumber)
		{
			if (activityType == GPSConstants.GPSInOutActivityType.Codes.GIN)
			{
				currentAddress.TimeIn = localEventTime;
			}
			else
			{
				currentAddress.TimeOut = localEventTime;
			}

			CreateClientActivity(location, localEventTime, activityType, currentAddress, currentAddress.Leg.PK, runSheetNumber);
		}

		void CreateClientActivity(IDeviceLocationWithEntity deviceLocation, DateTime localEventTime, ZString activityType, LegSegment address, ZGuid leg, ZString runSheetNumber)
		{
			var activity = factory.New<IGPSSupporterActivity>();
			activity.EN_JU = leg;
			activity.EN_EventType = GPSConstants.GPSEventTypeList.Codes.Custom;
			activity.EN_ActivityType = activityType;
			activity.EN_ActivityInformation = address.AddressCode;
			activity.EN_ActivityTime = localEventTime;
			activity.EN_ActivityID = runSheetNumber;
			activity.EN_RQ_Vehicle = deviceLocation.EntityId;
			activity.EN_Latitude = new ZDecimal(deviceLocation.Location.Latitude);
			activity.EN_Longitude = new ZDecimal(deviceLocation.Location.Longitude);
			activity.EN_SpeedKmhMph = (NoResString)"Kmh";
			activity.EN_Speed = ZByte.ParseSafe(deviceLocation.Speedkmh.Round(0).ToString(), byte.MinValue);
			activity.EN_Heading = ZShort.ParseSafe(deviceLocation.CompassHeadingDegrees.Round(0).ToString(), short.MinValue);
		}

		ZBool IsEventWithinAddress(IDeviceLocation location, LegSegment address, IEnumerable<LegSegment> otherAddresses)
		{
			return address.GeofencePolygon != null
				? IsEventWithinAddressGeofence(location, address)
				: IsEventWithinAddressRadius(location, address, otherAddresses);
		}

		/// <summary>
		/// Is the Event within the Address Radius? An Address radius is registry defined at 500m, but maybe reduced to 100m if it is too close to other possible addresses that could also be updated using this event. 
		/// This reduces a chance that a Leg is Delivered before being Picked up.
		/// </summary>
		ZBool IsEventWithinAddressRadius(IDeviceLocation location, LegSegment address, IEnumerable<LegSegment> otherAddresses)
		{
			var distanceBetweenAddressAndEvent = CalculateDistanceInMeters(address, location);
			return distanceBetweenAddressAndEvent <= MinimumGeofenceRadiusInMeters || distanceBetweenAddressAndEvent <= GetGeofenceRadius(address, otherAddresses);
		}

		static ZBool IsEventWithinAddressGeofence(IDeviceLocation location, LegSegment address)
		{
			return address.GeofencePolygon.Intersects(ZGeography.CreatePoint(location.Location.Longitude.Value, location.Location.Latitude.Value));
		}

		/// <summary>
		/// Get the Geofence Radius around an address, where not overlapping any other address geofences (with the exception of: shrink radius to a minimum of 100m)
		/// </summary>
		ZInt GetGeofenceRadius(LegSegment address, IEnumerable<LegSegment> otherAddresses)
		{
			var radius = DefaultGeofenceRadiusInMeters;
			var diameter = DefaultGeofenceRadiusInMeters * 2;

			foreach (var otherAddress in otherAddresses.Where(a => !a.IsSameAddress(address)))
			{
				var distanceBetweenAddresses = CalculateDistanceInMeters(address, otherAddress);
				if (distanceBetweenAddresses < diameter)
				{
					// if we are calculating the distance between a Lat/Long (address centre) and Geofence (address polygon edge), the radius we give the Lat/Long Address should be the whole distance, 
					// otherwise if both have Lat/Long then both should share the distance (divide by 2).
					var distanceDivider = address.GeofencePolygon != null || otherAddress.GeofencePolygon != null ? 1 : 2;
					radius = (ZInt)Math.Max(MinimumGeofenceRadiusInMeters, Math.Min(radius, distanceBetweenAddresses / distanceDivider));
					if (radius == MinimumGeofenceRadiusInMeters)
					{
						break;
					}
				}
			}

			return radius;
		}

		ZDecimal CalculateDistanceInMeters(LegSegment firstAddress, LegSegment secondAddress)
		{
			var firstGeography = firstAddress.GeofencePolygon != null ? firstAddress.GeofencePolygon : GetAddressPoint(firstAddress);
			var secondGeography = secondAddress.GeofencePolygon != null ? secondAddress.GeofencePolygon : GetAddressPoint(secondAddress);
			return firstGeography.IsValid && secondGeography.IsValid && (!firstGeography.IsEmpty && !secondGeography.IsEmpty) ? firstGeography.Distance(secondGeography) ?? 0 : DefaultGeofenceRadiusInMeters;
		}

		static ZGeography GetAddressPoint(LegSegment address)
		{
			return ZGeography.ParseSafe(string.Format(CultureInfo.InvariantCulture, "{0},{1}", address.Longitude, address.Latitude), ZGeography.Empty);
		}

		static ZDecimal CalculateDistanceInMeters(LegSegment address, IDeviceLocation location)
		{
			return CalculateDistanceInMeters(address.Latitude, address.Longitude, location.Location.Latitude.Value, location.Location.Longitude.Value);
		}

		static ZDecimal CalculateDistanceInMeters(ZDecimal firstLatitude, ZDecimal firstLongitude, double secondLatitude, double secondLongitude)
		{
			var distanceInKilometers = RefLatLongPostcode.CalculateDistance((double)firstLatitude, (double)firstLongitude, secondLatitude, secondLongitude);
			return distanceInKilometers * 1000;
		}

		static IEnumerable<Leg> GetOrderedAvailableLegs(CommonWorkSheet runSheet)
		{
			return runSheet.CartageLegs.Select(l => new Leg(l)).Where(l => !l.IsComplete).OrderBy(l => l.Sequence);
		}

		static IEnumerable<LegSegment> GetOrderedAddresses(Leg firstLeg, Leg secondLeg)
		{
			var firstLegAddresses = firstLeg.OrderedGeofenceAddresses();
			var secondLegAddresses = secondLeg?.OrderedGeofenceAddresses() ?? Array.Empty<LegSegment>();

			return firstLegAddresses.Concat(secondLegAddresses);
		}

		CommonWorkSheet GetRunSheet(IDeviceLocationWithEntity location, ZDateTime localEventTime)
		{
			var truckQuery = new ZQuery(JobCartageRunSheetSchema.EY_RQ_Truck, location.EntityId);
			truckQuery.AddToFilter(JobCartageRunSheetSchema.EY_StartTime, SQLComparisonOperator.LessThanOrEqualTo, localEventTime);
			truckQuery.AddToFilter(JobCartageRunSheetSchema.EY_EndTime, SQLComparisonOperator.GreaterThanOrEqualTo, localEventTime);

			var result = factory.LoadTop1<CommonWorkSheet>(truckQuery);
			if (result == null)
			{
				var op = location.GetOperator();
				if (op != null)
				{
					var driver = factory.Load<GlbStaff>(op.TE_EntityIdTo);

					if (driver != null)
					{
						var driverQuery = new ZQuery(JobCartageRunSheetSchema.EY_GS_NKTruckDriver, driver.GS_Code);
						driverQuery.AddToFilter(JobCartageRunSheetSchema.EY_StartTime, SQLComparisonOperator.LessThanOrEqualTo, localEventTime);
						driverQuery.AddToFilter(JobCartageRunSheetSchema.EY_EndTime, SQLComparisonOperator.GreaterThanOrEqualTo, localEventTime);

						driverQuery.AddToFilter(JobCartageRunSheetSchema.EY_RQ_Truck, null);
						result = factory.LoadTop1<CommonWorkSheet>(driverQuery);
					}
				}
			}

			return result;
		}

		IDeviceLocationWithEntity[] GetLocationsToProcess(INotifications notifications, ZDateTime lastProcessedTime)
		{
			var query = new ZQuery();
			query.AddToFilter(TelDeviceLocationWithEntitySchema.TLL_MeasurementTimeUtc, SQLComparisonOperator.GreaterThan, lastProcessedTime);
			query.AddToFilter(TelDeviceLocationWithEntitySchema.TLL_MeasurementTimeUtc, SQLComparisonOperator.LessThan, ZDateTime.UtcToday.AddDays(1));
			query.AddToFilter(TelDeviceLocationWithEntitySchema.TLL_ParentTableCode, SQLComparisonOperator.Equal, RefEquipmentSchema.Constants.Prefix);
			query.OrderBy = TelDeviceLocationWithEntitySchema.Constants.TLL_MeasurementTimeUtc + OrderByClause.Ascending;

			return FilterLocationsToProcess(notifications, factory.Load<IDeviceLocationWithEntity>(query));
		}

		static IDeviceLocationWithEntity[] FilterLocationsToProcess(INotifications notifications, IDeviceLocationWithEntity[] locations)
		{
			IDeviceLocationWithEntity[] validLocations;

			if (locations.Length > 0)
			{
				validLocations = locations.Where(location => location.MeasurementTimeUtc.IsValidSmallDateTime).ToArray();

				var invalidLocationNum = locations.Length - validLocations.Length;
				if (invalidLocationNum > 0)
				{
					notifications.Notify(new InfoNotification(Res.GetString("BCC3D450-3E97-4392-8BBB-516E205155EB", "Ignored {0} location(s) with out of range measurement time", invalidLocationNum)));
				}
			}
			else
			{
				validLocations = locations;
			}

			return validLocations;
		}

		ZInt DefaultGeofenceRadiusInMeters
		{
			get
			{
				if (!defaultGeofenceRadiusInMeters.HasValue)
				{
					defaultGeofenceRadiusInMeters = LocalCartageDataRegistry.Instance.GeoFenceRadiusSize.Value;
				}
				return defaultGeofenceRadiusInMeters.Value;
			}
		}
		ZInt? defaultGeofenceRadiusInMeters;

		const int MinimumGeofenceRadiusInMeters = 100;

		readonly BusinessObjectFactory factory;
	}
}
