using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Rating.Business;
using Enterprise.Rating.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Business
{
	public static class DeliveryDueDateCalculationHelper
	{
		public static string EmptyValueSignForLog
		{
			get
			{
				return Res.GetString("d8ad44df-5132-461f-af56-54ef582f9384", "-");
			}
		}

		public static ZString GetRateModeFromShipment(ForwardingShipment shipment)
		{
			var rateModes = new CodeDescriptionPairList(OLookUpEditType.RateModes);
			if (rateModes.ContainsCode(shipment.JS_PackingMode))
			{
				return shipment.JS_PackingMode;
			}

			if (rateModes.ContainsCode(shipment.JS_TransportMode))
			{
				return shipment.JS_TransportMode;
			}

			return Core.Constants.RateMode.ALL;
		}

		const string zoneOwnerByZoneItemPostCodeMatchSQL = @"
SELECT TP_OH_RelatedParty
FROM dbo.RateTransportProvider Provider
JOIN dbo.RateTransportZones Zones ON Zones.TZ_TP = Provider.TP_PK
JOIN dbo.RateTransportZoneItem ZoneItem ON ZoneItem.TQ_TZ_DomesticZone = Zones.TZ_PK
WHERE
    Provider.TP_ZoneType IN ('ALL', 'OPS')
    AND Provider.TP_IsActive = 1
    AND Zones.TZ_IsActive = 1
    AND ZoneItem.TQ_RN_NKCountry = @ZoneCountryCode
    AND
    (
        -- Postcode exact match
        (
            ZoneItem.TQ_FromPostCode = @FromPostcode
            AND ZoneItem.TQ_ToPostCode = ''
        )
        OR
        -- Postcode range match
        (
            ZoneItem.TQ_FromPostCode <> ''
            AND ZoneItem.TQ_ToPostCode <> ''
            AND ZoneItem.TQ_FromPostCode <= @FromPostcode
            AND ZoneItem.TQ_ToPostCode >= @ToPostcode
        )
        OR
        -- City match
        (
            ZoneItem.TQ_R9_CityTown IN
            (
                SELECT R9_PK FROM dbo.RefCityTown CityTown
                    INNER JOIN dbo.RefCityPCodepivot
                        ON RefCityPCodepivot.R0_R9 = CityTown.R9_PK
                    INNER JOIN dbo.RefPostCode
                        ON RefCityPCodePivot.R0_RK = RefPostCode.RK_PK
                        AND RefPostCode.RK_CityTownPostCode = @FromPostcode
                WHERE
                    CityTown.R9_RN_NKCountry = @CityCountryCode
            )
        )
    )
";

const string zoneOwnerByZoneItemCityMatchSQL = @"
SELECT TP_OH_RelatedParty
FROM dbo.RateTransportProvider Provider
JOIN dbo.RateTransportZones Zones ON Zones.TZ_TP = Provider.TP_PK
JOIN dbo.RateTransportZoneItem ZoneItem ON ZoneItem.TQ_TZ_DomesticZone = Zones.TZ_PK
WHERE
    Provider.TP_ZoneType IN ('ALL', 'OPS')
    AND Provider.TP_IsActive = 1
    AND Zones.TZ_IsActive = 1
    AND ZoneItem.TQ_RN_NKCountry = @ZoneCountryCode
    AND
    (
        ZoneItem.TQ_R9_CityTown IN
        (
            SELECT R9_PK FROM dbo.RefCityTown CityTown
            WHERE
                CityTown.R9_InternationalName <> ''
                AND CityTown.R9_InternationalName = @City
                AND CityTown.R9_RN_NKCountry = @CityCountryCode
        )
    )
";

		const string zoneOwnerByCityPostCodeMatchSQL = @"
SELECT TP_OH_RelatedParty
FROM dbo.RateTransportProvider Provider
JOIN dbo.RateTransportZones Zones ON Zones.TZ_TP = Provider.TP_PK
JOIN dbo.RefCityTown CityTown ON Provider.TP_R9_ZoneHubLocation = CityTown.R9_PK
WHERE
    Provider.TP_ZoneType IN ('ALL', 'OPS')
    AND Provider.TP_IsActive = 1
    AND Zones.TZ_IsActive = 1
    AND CityTown.R9_RN_NKCountry = @CityCountryCode
    AND CityTown.R9_InternationalName <> ''
    AND
    (
        CityTown.R9_PK IN
        (
            SELECT R0_R9 FROM dbo.RefCityPCodepivot
                INNER JOIN dbo.RefPostCode
                    ON RefCityPCodePivot.R0_RK = RefPostCode.RK_PK
                    AND RefPostCode.RK_CityTownPostCode = @FromPostcode
        )
    )
";

		const string zoneOwnerByCityNameMatchSQL = @"
SELECT TP_OH_RelatedParty
FROM dbo.RateTransportProvider Provider
JOIN dbo.RateTransportZones Zones ON Zones.TZ_TP = Provider.TP_PK
JOIN dbo.RefCityTown CityTown ON Provider.TP_R9_ZoneHubLocation = CityTown.R9_PK
WHERE
    Provider.TP_ZoneType IN ('ALL', 'OPS')
    AND Provider.TP_IsActive = 1
    AND Zones.TZ_IsActive = 1
    AND CityTown.R9_RN_NKCountry = @CityCountryCode
    AND CityTown.R9_InternationalName <> ''
    AND CityTown.R9_InternationalName = @City
";

		const string zoneOwnerByCityStateMatchSQL = @"
SELECT TP_OH_RelatedParty
FROM dbo.RateTransportProvider Provider
JOIN dbo.RateTransportZones Zones ON Zones.TZ_TP = Provider.TP_PK
JOIN dbo.RefCityTown CityTown ON Provider.TP_R9_ZoneHubLocation = CityTown.R9_PK
WHERE
    Provider.TP_ZoneType IN ('ALL', 'OPS')
    AND Provider.TP_IsActive = 1
    AND Zones.TZ_IsActive = 1
    AND CityTown.R9_RN_NKCountry = @CityCountryCode
	AND CityTown.R9_RW_NKState = @CityState
";

		const string zoneOwnerByCountryMatchSQL = @"
SELECT TP_OH_RelatedParty
FROM dbo.RateTransportProvider Provider
JOIN dbo.RateTransportZones Zones ON Zones.TZ_TP = Provider.TP_PK
LEFT JOIN dbo.RefCityTown CityTown ON Provider.TP_R9_ZoneHubLocation = CityTown.R9_PK
WHERE
	Provider.TP_ZoneType IN ('ALL', 'OPS')
	AND Provider.TP_IsActive = 1
	AND Zones.TZ_IsActive = 1
	AND
	(
		Provider.TP_RN_NKCountry = @ZoneCountryCode
		OR
		CityTown.R9_RN_NKCountry = @CityCountryCode
	)
";

		public static bool ShouldSkipFindingOpeningHours(BusinessObjectFactory factory, CalendarDayTypeProvider calendarDayTypeProvider, IDocAddress address, ZString operationType, ZDateTime dateTime, ZBool useArrivalTime, ZBool deliverOnWeekend, ZBool isXtoCFS, ZStringBuilder logger)
		{
			if (address == null)
			{
				return true;
			}

			if (operationType.EqualsIgnoringCase(OrgTimetableType.Codes.Pickup))
			{
				return false;
			}

			var isPublicHoliday = calendarDayTypeProvider.IsPublicHoliday(address, OrgTimetableType.Codes.Deliver, dateTime.Date, logger);

			if (useArrivalTime && !deliverOnWeekend && !isPublicHoliday && !IsClosedOrAfterClosingTime(address, dateTime))
			{
				return true;
			}

			if (isXtoCFS)
			{
				return false;
			}

			var (weekendDay1, weekendDay2) = DeliveryDueDateCalculationHelper.GetWeekendDays(address, factory);
			var shouldSkipFindingOpeningHours = deliverOnWeekend && (dateTime.DayOfWeek == weekendDay1 || (weekendDay2.HasValue && dateTime.DayOfWeek == weekendDay2));
			return shouldSkipFindingOpeningHours;
		}

		static bool IsClosedOrAfterClosingTime(IDocAddress address, ZDateTime dateTime)
		{
			if (address is OrgAddress orgAddress)
			{
				foreach (var timetable in orgAddress.Timetables)
				{
					if (timetable.OTT_Type == OrgTimetableType.Codes.Deliver &&
						timetable.IsAppliedToDayOfWeek(dateTime.DayOfWeek))
					{
						if (dateTime.TimeOfDay <= timetable.OTT_TimeTo.TimeOfDay)
						{
							return false;
						}
					}
				}

				return true;
			}

			if (address is JobDocAddress jobDocAddress)
			{
				return IsClosedOrAfterClosingTime(jobDocAddress.Address, dateTime);
			}

			return false;
		}

		public static ZDateTime AdjustTimeToDeliverDueTime(ZDateTime initialDateTime, ZStringBuilder calculationLogBuilder,
			DeliveryDueTime deliveryDueTime, CalendarDayTypeProvider calendarDayTypeProvider, IDocAddress deliveryAddress)
		{
			var adjustedDateTime = initialDateTime.FindNextAvailableTime(deliveryDueTime.Time);
			if (adjustedDateTime.Date != initialDateTime.Date)
			{
				calculationLogBuilder.AppendLine(
					Res.GetString("c8cafd06-5c4a-4299-b64f-6c3ead289641", "Adding one extra day: {0} adjusted to {1} because calculated delivery time is after delivery due time",
					initialDateTime, adjustedDateTime));
			}

			var finder = new ClosestOpeningHourFinder(deliveryAddress, OrgTimetableType.Codes.Deliver, calendarDayTypeProvider, calculationLogBuilder);
			(var adjustedDateTimeByCheckingOpeningDays, var nonWorkingDays) = finder.FindNextWorkingDay(adjustedDateTime.Date);

			var result = adjustedDateTimeByCheckingOpeningDays.Add(deliveryDueTime.Time);

			if (nonWorkingDays.Any())
			{
				calculationLogBuilder.AppendLine(
					Res.GetString("e453612d-347f-4793-884c-183c546c76e8", "Delivery Address Weekend Days/Public Holidays: {0}",
						nonWorkingDays.ToStringWithDayOfWeeks()));
				calculationLogBuilder.AppendLine(
					Res.GetString("97ba3f03-b89d-4531-a1f5-b61e8fde45b3", "{0} adjusted to {1} because of Delivery Address Weekends/Public Holidays", adjustedDateTime, result));
			}

			if (result > initialDateTime)
			{
				calculationLogBuilder.AppendLine(
				Res.GetString("f74f5407-a2fb-48f8-b0eb-373617a034eb", "Delivery due time adjusted from {0} to {1} based on {2}'s configured delivery due time",
					initialDateTime, result, deliveryDueTime.Source));
			}
			
			return result;
		}

		public static IDeliveryDueDateCalculationResult SetHoldForPickupTime(ZDateTime initialDateTime,ZStringBuilder calculationLogBuilder,
			TimeSpan? holdForPickupTime, CalendarDayTypeProvider calendarDayTypeProvider, IDocAddress cfsAddress)
		{
			calculationLogBuilder.AppendLine(Res.GetString("a3bc3529-4f16-4b52-98de-033b5fefa3f2", "Hold For Pickup Time: {0}", holdForPickupTime));

			var resultWithHoldForPickup = initialDateTime.FindNextAvailableTime(holdForPickupTime.Value);
			if (resultWithHoldForPickup.Date != initialDateTime.Date)
			{
				calculationLogBuilder.AppendLine(
					Res.GetString("192c4820-df35-405f-b07d-ee1b0406b98d", "Adding one extra day: {0} adjusted to {1} because calculated time is after hold for pickup", initialDateTime, resultWithHoldForPickup));
			}
			else if (resultWithHoldForPickup != initialDateTime)
			{
				calculationLogBuilder.AppendLine(
									Res.GetString("82fd120a-5057-449e-b26d-4c4dd051a411", "Set Hold For Pickup Time: {0} adjusted to {1} based on {2} CFS's Hold for pickup time",
										initialDateTime, resultWithHoldForPickup, OrgTimetableType.Codes.Deliver));
			}

			var finder = new ClosestOpeningHourFinder(cfsAddress, OrgTimetableType.Codes.Deliver, calendarDayTypeProvider, calculationLogBuilder);
			(var adjustedDateTimeByCheckingOpeningHours, var nonWorkingDays) = finder.GetClosestOpeningHour(resultWithHoldForPickup);

			if (nonWorkingDays.Any())
			{
				calculationLogBuilder.AppendLine(
					Res.GetString("09d3fd1a-c747-4c1c-b120-f097bd5d8450", "{0} CFS/Transit Warehouse Weekend Days/Public Holidays: {1}", OrgTimetableType.Codes.Deliver,
						nonWorkingDays.ToStringWithDayOfWeeks()));
			}

			if (adjustedDateTimeByCheckingOpeningHours != resultWithHoldForPickup)
			{
				calculationLogBuilder.AppendLine(
					Res.GetString("2019ebd4-2d7a-4898-890f-d30faf926a7c", "{0} adjusted to {1} because of {2} CFS Weekends/Public Holidays/Opening Hours",
					resultWithHoldForPickup, adjustedDateTimeByCheckingOpeningHours, OrgTimetableType.Codes.Deliver));

				resultWithHoldForPickup = adjustedDateTimeByCheckingOpeningHours.Date.Add(holdForPickupTime.Value);
				calculationLogBuilder.AppendLine(
					Res.GetString("2aa35cac-026c-4549-bad9-0e7f9387a89a", "Set Hold For Pickup Time: {0} adjusted to {1} based on {2} CFS's Hold for pickup time",
						adjustedDateTimeByCheckingOpeningHours, resultWithHoldForPickup, OrgTimetableType.Codes.Deliver));
			}

			return DeliveryDueDateCalculationResult.Success(resultWithHoldForPickup, calculationLogBuilder.ToString());
		}

		public static OrgHeader GetRelatedTransportZoneOwner(ZString postcode, ZString city, ZString state, ZString country, BusinessObjectFactory factory)
		{
			var parameter = new ZSqlParameterCollection
			{
				{ "@FromPostcode", postcode, RateTransportZoneItemSchema.TQ_FromPostCode },
				{ "@ToPostcode", postcode, RateTransportZoneItemSchema.TQ_ToPostCode },
				{ "@ZoneCountryCode", country, RateTransportZoneItemSchema.TQ_RN_NKCountry },
				{ "@CityCountryCode", country, RefCityTownSchema.R9_RN_NKCountry },
				{ "@City", city, RefCityTownSchema.R9_InternationalName },
				{ "@CityState", state, RefCityTownSchema.R9_RW_NKState }
			};

			var sqlQueries = new List<string>
			{
				zoneOwnerByZoneItemPostCodeMatchSQL,
				zoneOwnerByZoneItemCityMatchSQL,
				zoneOwnerByCityPostCodeMatchSQL,
				zoneOwnerByCityNameMatchSQL,
				zoneOwnerByCityStateMatchSQL,
				zoneOwnerByCountryMatchSQL
			};

			foreach (var sqlQuery in sqlQueries)
			{
				var result = GetOrgByRelatedPartyID(factory, sqlQuery, parameter, out var hasMultipleMatch);
				if (hasMultipleMatch || result != null)
				{
					return result;
				}
			}

			return null;
		}

		static OrgHeader GetOrgByRelatedPartyID(BusinessObjectFactory factory, string zoneOwnerSQL, ZSqlParameterCollection parameter, out bool hasMultipleMatch)
		{
			var orgHeaderQuery = new ZDBOnlyQuery(typeof(OrgHeader));
			orgHeaderQuery.MaximumRows = 2;
			orgHeaderQuery.AddFilterAndZSQLParameterCollection(
				string.Format(CultureInfo.InvariantCulture, "{0} IN ({1})", OrgHeaderSchema.PK.Name, zoneOwnerSQL),
				parameter);

			var results = factory.Load<OrgHeader>(orgHeaderQuery);
			hasMultipleMatch = results.Length > 1;
			return results.Length == 1 ? results[0] : null;
		}

		public static ZDateTime ConvertOriginLocalTimeToDestinationLocalTime(ZDateTime initialDateTime, IDocAddress sourceAddress, IDocAddress destinationAddress, BusinessObjectFactory factory)
		{
			var sourcePort = GetUNLOCOFromAddress(sourceAddress, factory);
			var destinationPort = GetUNLOCOFromAddress(destinationAddress, factory);

			if (sourcePort.IsEmpty || destinationPort.IsEmpty)
			{
				return initialDateTime;
			}

			var destinationLocalDateTime = new ZDateTime(Env.Time.GetTimeInOneZoneFromTimeInAnotherZone(sourcePort, initialDateTime.ToDateTime(), destinationPort));
			return destinationLocalDateTime;
		}

		static ZString GetUNLOCOFromAddress(IDocAddress address, BusinessObjectFactory factory)
		{
			var unloco = RefUNLOCO.GetPortFromNameAndCountryCode(factory, address.E2_City, address.E2_RN_NKCountryCode);
			if (unloco != null)
			{
				return unloco.Code;
			}

			if (address is OrgAddress orgAddress)
			{
				return !orgAddress.OA_RL_NKRelatedPortCode.IsEmpty
					? orgAddress.OA_RL_NKRelatedPortCode
					: orgAddress.HeaderClosestPort?.Code ?? ZString.Empty;
			}

			return ZString.Empty;
		}

		public static (ZDateTime ReadyDate, ZString WhichDateSelected) GetReadyDateFromShipment(ForwardingShipment shipment)
		{
			var pickuprequiredBy = Res.GetString("f47f8633-4b4b-449b-b228-e4cd5f31cb53", "Pickup Required By");
			var actualPickup = Res.GetString("904272ce-1ab7-4054-aa2b-5682f6bf7c16", "Actual Pickup");
			var interimReceipt = Res.GetString("351ca198-4d0e-420c-bfbe-359d67e94b3d", "Interim Receipt");
			var estimatedDeparture = Res.GetString("5207a24b-9201-4cbc-bca5-7eb7057c0e68", "Estimated Departure");
			var invalidDate = Res.GetString("06d0c813-85fa-428b-9904-2f9fdbb4f060", "Invalid Date");

			if (shipment.IsHBLContainerPackModeDOOR_X)
			{
				if (shipment.DocsAndCartage != null && !shipment.DocsAndCartage.IsDeleted)
				{
					if (!shipment.DocsAndCartage.JP_PickupCartageCompleted.IsEmpty)
					{
						return (shipment.DocsAndCartage.JP_PickupCartageCompleted, actualPickup);
					}

					if (!shipment.DocsAndCartage.JP_PickupRequiredBy.IsEmpty)
					{
						return (shipment.DocsAndCartage.JP_PickupRequiredBy, pickuprequiredBy);
					}
				}
				return (ZDateTime.Empty, invalidDate);
			}

			if (shipment.IsHBLContainerPackModeCFS_X)
			{
				if (!shipment.JS_A_RCV.IsEmpty)
				{
					return (shipment.JS_A_RCV, interimReceipt);
				}

				if (shipment.DocsAndCartage != null
					&& !shipment.DocsAndCartage.IsDeleted
					&& !shipment.DocsAndCartage.JP_PickupRequiredBy.IsEmpty)
				{
					return (shipment.DocsAndCartage.JP_PickupRequiredBy, pickuprequiredBy);
				}

				return (ZDateTime.Empty, invalidDate);
			}

			if (IsHBLContainerPackModeARPT_X(shipment.JS_HBLContainerPackModeOverride))
			{
				return (CalculateTimeOfDepartureFromAirport(shipment), estimatedDeparture);
			}

			return (ZDateTime.Empty, invalidDate);
		}

		public static IDocAddress LoadAddressByOrgCodeAndShortCode(BusinessObjectFactory factory, ZString orgCode, ZString addressShortCode)
		{
			var orgHeaderSubQuery = new ZDBOnlySubQuery(typeof(OrgHeader), OrgHeaderSchema.PK);
			orgHeaderSubQuery.AddToFilter(OrgHeaderSchema.OH_Code, orgCode);

			var orgAddressQuery = new ZDBOnlyQuery(typeof(OrgAddress));
			orgAddressQuery.AddToFilter(OrgAddressSchema.OA_Code, addressShortCode);
			orgAddressQuery.AddSubQuery(OrgAddressSchema.OA_OH, orgHeaderSubQuery, JoinCondition.And);

			return factory.LoadTop1<OrgAddress>(orgAddressQuery);
		}

		public static OrgTimetable GetMatchedTimetable(ZDateTime dateTime, OrgAddress address, ZString type, bool ignoreTime = false)
		{
			if (address.TimetablesRangeType == OrgTimeTableRangeType.NotApplicable)
			{
				return address.Timetables.FirstOrDefault(t => t.OTT_Type == type);
			}

			return address.Timetables.FirstOrDefault(t =>
				t.OTT_Type == type
				&& t.IsAppliedToDayOfWeek(dateTime.DayOfWeek)
				&& (ignoreTime ||
				(t.OTT_TimeFrom.TimeOfDay <= dateTime.TimeOfDay
				&& dateTime.TimeOfDay <= t.OTT_TimeTo.TimeOfDay)));
		}

		public static RateTransportZoneItem GetZoneItem(IDocAddress nonCFSAddress, IDocAddress cfsAddress, BusinessObjectFactory factory, bool ignoreNonCFSAddress = false)
		{
			if ((nonCFSAddress == null && !ignoreNonCFSAddress) || cfsAddress == null || factory == null)
			{
				return null;
			}
			return GetZoneItemForSpecialProviderType(nonCFSAddress, cfsAddress, RatingConstants.RatingZoneTypes.Operations, ignoreNonCFSAddress, factory)
				?? GetZoneItemForSpecialProviderType(nonCFSAddress, cfsAddress, RatingConstants.RatingZoneTypes.All, ignoreNonCFSAddress, factory);
		}

		public static ZDBOnlySubQuery GetZoneItemSubQueryFromPostcodeOrCity(ZString postcode, ZString city, ZString country)
		{
			var subQuery = GetZoneItemPostcodeExactMatchSubQuery(postcode, country);
			subQuery.AddSubQuery(GetZoneItemPostcodeRangeSubQuery(postcode, country), JoinCondition.Or);
			subQuery.AddSubQuery(GetZoneItemCitySubQuery(postcode, city, country), JoinCondition.Or);
			return subQuery;
		}

		public static RateTransportProvider GetTransportProvider(IDocAddress nonCFSAddress, IDocAddress cfsAddress, BusinessObjectFactory factory)
		{
			if (cfsAddress == null || factory == null)
			{
				return null;
			}

			if (nonCFSAddress == null)
			{
				return GetTransportProviderIgnoreNonCFSAddress(cfsAddress, RatingConstants.RatingZoneTypes.Operations, factory)
				?? GetTransportProviderIgnoreNonCFSAddress(cfsAddress, RatingConstants.RatingZoneTypes.All, factory);
			}

			return GetTransportProviderFromCityName(nonCFSAddress, cfsAddress, RatingConstants.RatingZoneTypes.Operations, factory)
				?? GetTransportProviderFromCityName(nonCFSAddress, cfsAddress, RatingConstants.RatingZoneTypes.All, factory)
				?? GetTransportProviderFromCityState(nonCFSAddress, cfsAddress, RatingConstants.RatingZoneTypes.Operations, factory)
				?? GetTransportProviderFromCityState(nonCFSAddress, cfsAddress, RatingConstants.RatingZoneTypes.All, factory)
				?? GetTransportProviderFromCityCountry(nonCFSAddress, cfsAddress, RatingConstants.RatingZoneTypes.Operations, factory)
				?? GetTransportProviderFromCityCountry(nonCFSAddress, cfsAddress, RatingConstants.RatingZoneTypes.All, factory)
				?? GetTransportProviderFromProviderCountry(nonCFSAddress, cfsAddress, RatingConstants.RatingZoneTypes.Operations, factory)
				?? GetTransportProviderFromProviderCountry(nonCFSAddress, cfsAddress, RatingConstants.RatingZoneTypes.All, factory)
				?? GetTransportProviderIgnoreNonCFSAddress(cfsAddress, RatingConstants.RatingZoneTypes.Operations, factory)
				?? GetTransportProviderIgnoreNonCFSAddress(cfsAddress, RatingConstants.RatingZoneTypes.All, factory);
		}

		public static ZString ToStringWithDayOfWeeks(this IEnumerable<ZDateTime> dateList)
		{
			var logger = new ZStringBuilder();
			foreach (var d in dateList)
			{
				logger.Append(Res.GetString("44f10e27-2b72-4300-b725-e951b719fc25", "{0} {1}", d.DayOfWeek, d.ToShortDateString()));
				logger.Append("; ");
			}
			return logger.ToString();
		}

		public static ZByte GetDayOfWeek(DayOfWeek dayOfWeek)
		{
			switch (dayOfWeek)
			{
				case DayOfWeek.Monday:
					return 1;
				case DayOfWeek.Tuesday:
					return 2;
				case DayOfWeek.Wednesday:
					return 3;
				case DayOfWeek.Thursday:
					return 4;
				case DayOfWeek.Friday:
					return 5;
				case DayOfWeek.Saturday:
					return 6;
				case DayOfWeek.Sunday:
					return 7;
				default:
					return 0;
			}
		}

		static RateTransportZoneItem GetZoneItemForSpecialProviderType(IDocAddress nonCFSAddress, IDocAddress cfsAddress, ZString providerType, bool ignoreNonCFSAddress, BusinessObjectFactory factory)
		{
			if (nonCFSAddress == null && !ignoreNonCFSAddress)
			{
				return null;
			}

			var zoneSubQuery = GetTransportZoneSubQuery(cfsAddress.Organisation.PK, providerType);
			var zoneItemQuery = new ZDBOnlyQuery(typeof(RateTransportZoneItem));
			zoneItemQuery.AddSubQuery(RateTransportZoneItemSchema.TQ_TZ_DomesticZone, zoneSubQuery, JoinCondition.And);

			if (!ignoreNonCFSAddress)
			{
				var postCode = nonCFSAddress.E2_Postcode;
				var country = nonCFSAddress.E2_RN_NKCountryCode;
				var city = nonCFSAddress.E2_City;
				zoneItemQuery.AddSubQuery(RateTransportZoneItemSchema.PK, GetZoneItemSubQueryFromPostcodeOrCity(postCode, city, country), JoinCondition.And);
			}

			return factory.LoadTop1<RateTransportZoneItem>(zoneItemQuery);
		}

		static ZDBOnlySubQuery GetZoneItemPostcodeExactMatchSubQuery(ZString addressPostCode, ZString country)
		{
			var zoneItemExactPostcodeMatchSubQuery = new ZDBOnlySubQuery(typeof(IRateTransportZoneItem), RateTransportZoneItemSchema.PK);
			zoneItemExactPostcodeMatchSubQuery.AddToFilter(RateTransportZoneItemSchema.TQ_FromPostCode, SQLComparisonOperator.Equal, addressPostCode);
			zoneItemExactPostcodeMatchSubQuery.AddToFilter(JoinCondition.And, RateTransportZoneItemSchema.TQ_ToPostCode, SQLComparisonOperator.Equal, string.Empty);
			zoneItemExactPostcodeMatchSubQuery.AddToFilter(JoinCondition.And, RateTransportZoneItemSchema.TQ_RN_NKCountry, country);
			return zoneItemExactPostcodeMatchSubQuery;
		}

		static ZDBOnlySubQuery GetZoneItemPostcodeRangeSubQuery(ZString addressPostCode, ZString country)
		{
			var zoneItemRangePostcodeMatchSubQuery = new ZDBOnlySubQuery(typeof(IRateTransportZoneItem), RateTransportZoneItemSchema.PK);
			zoneItemRangePostcodeMatchSubQuery.AddToFilter(RateTransportZoneItemSchema.TQ_RN_NKCountry, country);
			zoneItemRangePostcodeMatchSubQuery.AddToFilter(JoinCondition.And, RateTransportZoneItemSchema.TQ_FromPostCode, SQLComparisonOperator.NotEqual, string.Empty);
			zoneItemRangePostcodeMatchSubQuery.AddToFilter(JoinCondition.And, RateTransportZoneItemSchema.TQ_ToPostCode, SQLComparisonOperator.NotEqual, string.Empty);
			zoneItemRangePostcodeMatchSubQuery.AddToFilter(JoinCondition.And, RateTransportZoneItemSchema.TQ_FromPostCode, SQLComparisonOperator.LessThanOrEqualTo, addressPostCode);
			zoneItemRangePostcodeMatchSubQuery.AddToFilter(JoinCondition.And, RateTransportZoneItemSchema.TQ_ToPostCode, SQLComparisonOperator.GreaterThanOrEqualTo, addressPostCode);
			return zoneItemRangePostcodeMatchSubQuery;
		}

		static ZDBOnlySubQuery GetZoneItemCitySubQuery(ZString postCode, ZString city, ZString country)
		{
			var postCodeSubQuery = new ZDBOnlySubQuery(typeof(RefPostCode), RefPostCodeSchema.PK);
			postCodeSubQuery.AddToFilter(RefPostCodeSchema.RK_CityTownPostCode, postCode);

			var postCodePivotSubQuery = new ZDBOnlySubQuery(typeof(RefCityPCodePivot), RefCityPCodePivotSchema.R0_R9);
			postCodePivotSubQuery.AddSubQuery(RefCityPCodePivotSchema.R0_RK, postCodeSubQuery, JoinCondition.And);

			var cityPostCodeSubQuery = new ZDBOnlySubQuery(typeof(RefCityTown), RefCityTownSchema.PK);
			cityPostCodeSubQuery.AddSubQuery(RefCityTownSchema.PK, postCodePivotSubQuery, JoinCondition.And);
			cityPostCodeSubQuery.AddToFilter(RefCityTownSchema.R9_RN_NKCountry, country);

			var cityNameSubQuery = new ZDBOnlySubQuery(typeof(RefCityTown), RefCityTownSchema.PK);
			cityNameSubQuery.AddToFilter(RefCityTownSchema.R9_InternationalName, city);
			cityNameSubQuery.AddToFilter(RefCityTownSchema.R9_RN_NKCountry, country);

			var zoneItemCitySubQuery = new ZDBOnlySubQuery(typeof(IRateTransportZoneItem), RateTransportZoneItemSchema.PK);
			zoneItemCitySubQuery.AddToFilter(RateTransportZoneItemSchema.TQ_RN_NKCountry, country);
			zoneItemCitySubQuery.AddSubQuery(RateTransportZoneItemSchema.TQ_R9_CityTown, cityPostCodeSubQuery, JoinCondition.And);
			zoneItemCitySubQuery.AddSubQuery(RateTransportZoneItemSchema.TQ_R9_CityTown, cityNameSubQuery, JoinCondition.Or);
			return zoneItemCitySubQuery;
		}

		static ZDBOnlySubQuery GetTransportZoneSubQuery(ZGuid zoneOwner, ZString providerType)
		{
			var providerSubQuery = GetZonesProviderSubQueryFromZoneOwner(zoneOwner, providerType);
			var transportZoneSubQuery = new ZDBOnlySubQuery(typeof(IRateTransportZone), RateTransportZonesSchema.PK);
			transportZoneSubQuery.AddToFilter(RateTransportZonesSchema.TZ_IsActive, true);
			transportZoneSubQuery.AddSubQuery(RateTransportZonesSchema.TZ_TP, providerSubQuery, JoinCondition.And);
			return transportZoneSubQuery;
		}

		static ZDBOnlySubQuery GetZonesProviderSubQueryFromZoneOwner(ZGuid zoneOwnerPK, ZString zoneType)
		{
			var transportProviderQuery = new ZDBOnlySubQuery(typeof(IRateTransportProvider), RateTransportProviderSchema.PK);
			transportProviderQuery.AddToFilter(RateTransportProviderSchema.TP_OH_RelatedParty, zoneOwnerPK);
			transportProviderQuery.AddToFilter(RateTransportProviderSchema.TP_ZoneType, zoneType);
			transportProviderQuery.AddToFilter(RateTransportProviderSchema.TP_IsActive, true);
			return transportProviderQuery;
		}

		public static RateTransportProvider GetTransportProviderFromZoneHubLocation(IDocAddress nonCFSAddress, IDocAddress cfsAddress, ZString zoneType, BusinessObjectFactory factory)
		{
			var transportProviderQuery = GetTransportProviderFromZoneTypeAndZoneOwnerQuery(cfsAddress, zoneType);

			var country = nonCFSAddress.E2_RN_NKCountryCode;
			var city = nonCFSAddress.E2_City;

			var citySubQuery = new ZDBOnlySubQuery(typeof(RefCityTown), RefCityTownSchema.PK);
			citySubQuery.AddToFilter(RefCityTownSchema.R9_InternationalName, city);
			citySubQuery.AddToFilter(RefCityTownSchema.R9_RN_NKCountry, country);
			transportProviderQuery.AddSubQuery(RateTransportProviderSchema.TP_R9_ZoneHubLocation, citySubQuery, JoinCondition.And);

			return factory.LoadTop1<RateTransportProvider>(transportProviderQuery);
		}

		public static RateTransportProvider GetTransportProviderFromCountry(IDocAddress nonCFSAddress, IDocAddress cfsAddress, ZString zoneType, BusinessObjectFactory factory)
		{
			var transportProviderQuery = GetTransportProviderFromZoneTypeAndZoneOwnerQuery(cfsAddress, zoneType);

			var country = nonCFSAddress.E2_RN_NKCountryCode;

			transportProviderQuery.AddToFilter(JoinCondition.And, RateTransportProviderSchema.TP_RN_NKCountry, country);

			return factory.LoadTop1<RateTransportProvider>(transportProviderQuery);
		}

		static ZDBOnlyQuery GetTransportProviderFromZoneTypeAndZoneOwnerQuery(IDocAddress cfsAddress, ZString zoneType)
		{
			var transportProviderQuery = new ZDBOnlyQuery(typeof(RateTransportProvider));
			transportProviderQuery.AddToFilter(RateTransportProviderSchema.TP_OH_RelatedParty, cfsAddress.Organisation.PK);
			transportProviderQuery.AddToFilter(RateTransportProviderSchema.TP_ZoneType, zoneType);
			transportProviderQuery.AddToFilter(RateTransportProviderSchema.TP_IsActive, true);
			return transportProviderQuery;
		}

		static RateTransportProvider GetTransportProviderFromCityName(IDocAddress nonCFSAddress, IDocAddress cfsAddress, ZString zoneType, BusinessObjectFactory factory)
		{
			var transportProviderQuery = GetTransportProviderFromZoneTypeAndZoneOwnerQuery(cfsAddress, zoneType);

			var country = nonCFSAddress.E2_RN_NKCountryCode;
			var city = nonCFSAddress.E2_City;

			var citySubQuery = new ZDBOnlySubQuery(typeof(RefCityTown), RefCityTownSchema.PK);
			citySubQuery.AddToFilter(RefCityTownSchema.R9_InternationalName, city);
			citySubQuery.AddToFilter(RefCityTownSchema.R9_RN_NKCountry, country);
			transportProviderQuery.AddSubQuery(RateTransportProviderSchema.TP_R9_ZoneHubLocation, citySubQuery, JoinCondition.And);

			return factory.LoadTop1<RateTransportProvider>(transportProviderQuery);
		}

		static RateTransportProvider GetTransportProviderFromCityState(IDocAddress nonCFSAddress, IDocAddress cfsAddress, ZString zoneType, BusinessObjectFactory factory)
		{
			var transportProviderQuery = GetTransportProviderFromZoneTypeAndZoneOwnerQuery(cfsAddress, zoneType);

			var country = nonCFSAddress.E2_RN_NKCountryCode;
			var state = nonCFSAddress.E2_State;

			var citySubQuery = new ZDBOnlySubQuery(typeof(RefCityTown), RefCityTownSchema.PK);
			citySubQuery.AddToFilter(RefCityTownSchema.R9_RW_NKState, state);
			citySubQuery.AddToFilter(RefCityTownSchema.R9_RN_NKCountry, country);
			transportProviderQuery.AddSubQuery(RateTransportProviderSchema.TP_R9_ZoneHubLocation, citySubQuery, JoinCondition.And);

			return factory.LoadTop1<RateTransportProvider>(transportProviderQuery);
		}

		static RateTransportProvider GetTransportProviderFromCityCountry(IDocAddress nonCFSAddress, IDocAddress cfsAddress, ZString zoneType, BusinessObjectFactory factory)
		{
			var transportProviderQuery = GetTransportProviderFromZoneTypeAndZoneOwnerQuery(cfsAddress, zoneType);

			var country = nonCFSAddress.E2_RN_NKCountryCode;

			var citySubQuery = new ZDBOnlySubQuery(typeof(RefCityTown), RefCityTownSchema.PK);
			citySubQuery.AddToFilter(RefCityTownSchema.R9_RN_NKCountry, country);
			transportProviderQuery.AddSubQuery(RateTransportProviderSchema.TP_R9_ZoneHubLocation, citySubQuery, JoinCondition.And);

			return factory.LoadTop1<RateTransportProvider>(transportProviderQuery);
		}

		static RateTransportProvider GetTransportProviderFromProviderCountry(IDocAddress nonCFSAddress, IDocAddress cfsAddress, ZString zoneType, BusinessObjectFactory factory)
		{
			var transportProviderQuery = GetTransportProviderFromZoneTypeAndZoneOwnerQuery(cfsAddress, zoneType);

			var country = nonCFSAddress.E2_RN_NKCountryCode;

			transportProviderQuery.AddToFilter(JoinCondition.And, RateTransportProviderSchema.TP_RN_NKCountry, country);

			return factory.LoadTop1<RateTransportProvider>(transportProviderQuery);
		}

		static RateTransportProvider GetTransportProviderIgnoreNonCFSAddress(IDocAddress cfsAddress, ZString zoneType, BusinessObjectFactory factory)
		{
			var transportProviderQuery = GetTransportProviderFromZoneTypeAndZoneOwnerQuery(cfsAddress, zoneType);
			return factory.LoadTop1<RateTransportProvider>(transportProviderQuery);
		}

		public static ZDateTime FindNextAvailableTime(this ZDateTime dateTime, TimeSpan timespan)
		{
			var date = dateTime.Date;
			var timeOfDay = dateTime.TimeOfDay;
			if (timeOfDay > timespan)
			{
				return date.AddDays(1).Add(timespan);
			}
			else
			{
				return date.Add(timespan);
			}
		}

		public static ZString RemoveEndLines(this ZString str)
		{
			return str.Replace("\r", " ").Replace("\n", "");
		}

		public static ZDate GetNextDayOfWeek(this ZDateTime start, DayOfWeek dayOfWeek)
		{
			var daysToAdd = ((int)dayOfWeek - (int)start.DayOfWeek + 7) % 7;
			return start.AddDays(daysToAdd).Date;
		}

		static ZDateTime CalculateTimeOfDepartureFromAirport(ForwardingShipment shipment)
		{
			if (!IsHBLContainerPackModeARPT_X(shipment.JS_HBLContainerPackModeOverride) || shipment.JS_HBLContainerPackModeOverride == Core.Constants.HBLDeliveryModes.Codes.ARPT_ARPT)
			{
				return ZDateTime.Empty;
			}

			if (shipment.Consols.Count == 0)
			{
				return shipment.JS_E_DEP;
			}

			if (shipment.Consols.Count == 1)
			{
				var transports = shipment.Consols.Cast<ForwardingConsol>()
					.SelectMany(consol => consol.Transports.Cast<Transport>()).ToArray();
				var orderedTransports = new TransportOrderHelper(transports);
				return orderedTransports
					.FirstOrDefault(transport => transport.JW_RL_NKLoadPort == shipment.JS_RL_NKOrigin)?.JW_ETD ?? ZDateTime.Empty;
			}

			return ZDateTime.Empty;
		}

		public static bool IsHBLContainerPackModeARPT_X(string deliveryMode) => deliveryMode == Core.Constants.HBLDeliveryModes.Codes.ARPT_DOOR
			|| deliveryMode == Core.Constants.HBLDeliveryModes.Codes.ARPT_CFS
			|| deliveryMode == Core.Constants.HBLDeliveryModes.Codes.ARPT_ARPT;

		#region Weekends

		public static (DayOfWeek day1, DayOfWeek? day2) GetWeekendDays(IDocAddress address, BusinessObjectFactory factory)
		{
			var stateQuery = new ZQuery(RefCountryStatesSchema.RW_Code, address.E2_State);
			stateQuery.AddToFilter(RefCountryStatesSchema.RW_RN_NKCountryCode, address.E2_RN_NKCountryCode);
			var state = factory.LoadTop1<RefCountryStates>(stateQuery);

			if (state == null || !state.IsNonWorkingDaysOverrided)
			{
				var country = factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, address.E2_RN_NKCountryCode);
				if (country == null)
				{
					return (defaultDay1, defaultDay2);
				}

				return GetWeekendDaysFromWeekendsCollection(country.Weekends);
			}

			return GetWeekendDaysFromWeekendsCollection(state.Weekends);
		}

		static (DayOfWeek day1, DayOfWeek? day2) GetWeekendDaysFromWeekendsCollection(GlbHolidayCountryStatesCollection weekends)
		{
			var dayOfWeekCodeList = new DayOfWeekCodeList();

			if (weekends == null || !weekends.Any())
			{
				return (defaultDay1, defaultDay2);
			}

			var countryWeekends = weekends
				.Where(x => !x.GH_IsWorkingDay)
				.Select(x => dayOfWeekCodeList.GetDayOfWeek(x.GH_RecurrDay))
				.OrderBy(DeliveryDueDateCalculationHelper.GetDayOfWeek)
				.ToArray();

			if (countryWeekends.Length == 0)
			{
				return (defaultDay1, defaultDay2);
			}

			if (countryWeekends.Length > 1)
			{
				return (countryWeekends[0], countryWeekends[1]);
			}

			return (countryWeekends[0], null);
		}

		const DayOfWeek defaultDay1 = DayOfWeek.Saturday;
		const DayOfWeek defaultDay2 = DayOfWeek.Sunday;

		#endregion
	}
}
