using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Freight.Forwarding.Business
{
	public class CalendarDayTypeProvider
	{
		public const int CachedDays = 5;

		public CalendarDayType GetCalendarDayType(IDocAddress address, ZString pickupOrDeliveryType, ZDate date, ZStringBuilder logger)
		{
			if (address is OrgAddress orgAddress)
			{
				return GetCalendarDayType(orgAddress, pickupOrDeliveryType, date, logger);
			}

			if (address is JobDocAddress jobDocAddress)
			{
				return GetCalendarDayType(jobDocAddress, pickupOrDeliveryType, date, logger);
			}

			return CalendarDayType.WorkingDay;
		}

		CalendarDayType GetCalendarDayType(OrgAddress address, ZString pickupOrDeliveryType, ZDate date, ZStringBuilder logger)
		{
			var result = ReadFromCache(dateWithCalendarDayTypesForOrgAddressCache, address, date);

			if (result == null)
			{
				var toDate = date.AddDays(CachedDays);
				var calendarDayTypes = GetDatesWithCalendarDayTypes(address, pickupOrDeliveryType, date.ToDateTime(), toDate.ToDateTime(), logger);
				dateWithCalendarDayTypesForOrgAddressCache = new DateWithCalendarDayTypeCache(address, date, toDate, calendarDayTypes);
				result = ReadFromCache(dateWithCalendarDayTypesForOrgAddressCache, address, date);
			}

			return result.CalendarDayType ?? CalendarDayType.WorkingDay;
		}

		CalendarDayType GetCalendarDayType(JobDocAddress address, ZString pickupOrDeliveryType, ZDate date, ZStringBuilder logger)
		{
			var result = ReadFromCache(dateWithCalendarDayTypesForDocAddressCache, address, date);

			if (result == null)
			{
				var toDate = date.AddDays(CachedDays);
				var calendarDayTypes = GetDatesWithCalendarDayTypes(address, pickupOrDeliveryType, date.ToDateTime(), toDate.ToDateTime(), logger);
				dateWithCalendarDayTypesForDocAddressCache = new DateWithCalendarDayTypeCache(address, date, toDate, calendarDayTypes);
				result = ReadFromCache(dateWithCalendarDayTypesForDocAddressCache, address, date);
			}
			return result.CalendarDayType ?? CalendarDayType.WorkingDay;
		}

		public ZBool IsNonWorkingDay(IDocAddress address, ZString pickupOrDeliveryType, ZDate date, ZStringBuilder logger)
		{
			return GetCalendarDayType(address, pickupOrDeliveryType, date, logger).IsNonWorkingDay();
		}

		public ZBool IsPublicHoliday(IDocAddress address, ZString pickupOrDeliveryType, ZDate date, ZStringBuilder logger)
		{
			return GetCalendarDayType(address, pickupOrDeliveryType, date, logger).IsPublicHoliday();
		}

		public ZBool IsWeekend(IDocAddress address, ZString pickupOrDeliveryType, ZDate date, ZStringBuilder logger)
		{
			return GetCalendarDayType(address, pickupOrDeliveryType, date, logger).IsWeekend();
		}

		protected virtual DateWithCalendarDayType[] GetDatesWithCalendarDayTypes(IDocAddress address, ZString pickupOrDeliveryType, DateTime fromLocalTime, DateTime toLocalTime, ZStringBuilder logger)
		{
			try
			{
				if (address is OrgAddress orgAddress)
				{
					return new RefCalendarDayTypeHelper().GetDatesWithCalendarDayTypes(orgAddress, pickupOrDeliveryType, fromLocalTime, toLocalTime);
				}

				if (address is JobDocAddress jobDocAddress)
				{
					return new RefCalendarDayTypeHelper().GetDatesWithCalendarDayTypes(jobDocAddress, pickupOrDeliveryType, fromLocalTime, toLocalTime);
				}

				logger.AppendLine(Res.GetString("ccbbe03b-edb1-4e31-89c4-9b847d542bce", "Couldn't find non working days for invalid address type."));
			}
			catch (Exception ex)
			{
				logger.AppendLine(Res.GetString("ea993c60-dfa1-474a-8a6b-a05a274797ec", "Couldn't find non working days."));
				logger.AppendLine(ex.ToString());
			}

			var defaultResult = new List<DateWithCalendarDayType>();

			for (var day = fromLocalTime.Date; day <= toLocalTime.Date; day = day.AddDays(1))
			{
				defaultResult.Add(new DateWithCalendarDayType(day, CalendarDayType.WorkingDay));
			}
			return defaultResult.ToArray();
		}

		DateWithCalendarDayType ReadFromCache(DateWithCalendarDayTypeCache cache, IDocAddress address, ZDate date)
		{
			if (cache != null && cache.IsRelatedToCurrentCache(address))
			{
				return cache.GetCachedCalendarDayTypeForDate(date);
			}

			return null;
		}

		DateWithCalendarDayTypeCache dateWithCalendarDayTypesForOrgAddressCache;
		DateWithCalendarDayTypeCache dateWithCalendarDayTypesForDocAddressCache;
	}

	class DateWithCalendarDayTypeCache
	{
		public DateWithCalendarDayType[] DateWithCalendarDayTypesRange { get; private set; }
		public ZDate LastFrom { get; private set; }
		public ZDate LastTo { get; private set; }
		public IDocAddress Address { get; private set; }

		public DateWithCalendarDayTypeCache(IDocAddress address, ZDate lastFrom, ZDate lastTo, DateWithCalendarDayType[] dateWithCalendarDayTypesRange)
		{
			Address = address;
			LastFrom = lastFrom;
			LastTo = lastTo;
			DateWithCalendarDayTypesRange = dateWithCalendarDayTypesRange;
		}

		public DateWithCalendarDayType GetCachedCalendarDayTypeForDate(ZDate requestedDate)
		{
			if (requestedDate >= LastFrom && requestedDate <= LastTo)
			{
				var index = (requestedDate - LastFrom).Days;
				return DateWithCalendarDayTypesRange[index];
			}

			return null;
		}

		public bool IsRelatedToCurrentCache(IDocAddress address)
		{
			return Address.E2_RN_NKCountryCode.EqualsIgnoringCase(address.E2_RN_NKCountryCode)
				&& Address.E2_State.EqualsIgnoringCase(address.E2_State)
				&& Address.E2_City.EqualsIgnoringCase(address.E2_City)
				&& Address.E2_Postcode.EqualsIgnoringCase(address.E2_Postcode)
				&& Address.E2_Address1.Equals(address.E2_Address1);
		}
	}
}
