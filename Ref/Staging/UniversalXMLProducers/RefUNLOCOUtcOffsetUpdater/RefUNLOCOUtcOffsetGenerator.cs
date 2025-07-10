using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using NodaTime;
using NodaTime.Extensions;
using NodaTime.TimeZones;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.RefUNLOCOUtcOffsetUpdater
{
	public class RefUNLOCOUtcOffsetGenerator
	{
		readonly UnlocoDataHelper unlocoDataHelper;
		readonly DateTime requiredDataDate;
		readonly short requiredOffsetPeriodInMonths;
		readonly RefUNLOCOUtcOffsetXmlProducer xmlProducer;
		readonly string timezoneDbFilePath;

		public RefUNLOCOUtcOffsetGenerator(UnlocoDataHelper unlocoDataHelper, DateTime requiredDataDate, short requiredOffsetPeriodInMonths, RefUNLOCOUtcOffsetXmlProducer xmlProducer, string timezoneDbFilePath)
		{
			this.timezoneDbFilePath = timezoneDbFilePath;
			this.unlocoDataHelper = unlocoDataHelper;
			this.requiredDataDate = requiredDataDate;
			this.requiredOffsetPeriodInMonths = requiredOffsetPeriodInMonths;
			this.xmlProducer = xmlProducer;
		}

		Dictionary<string, List<RefUNLOCOUtcOffsetRecord>> ParseTimezoneDatabase()
		{
			var result = new Dictionary<string, List<RefUNLOCOUtcOffsetRecord>>();
			using (var stream = File.OpenRead(timezoneDbFilePath))
			{
				var source = TzdbDateTimeZoneSource.FromStream(stream);
				var provider = new DateTimeZoneCache(source);
				foreach (var zone in provider.GetAllZones())
				{
					result.Add(zone.Id, TransformTransitions(GetTransitions(zone)));
				}
			}
			return result;
		}

		static short ConvertSecondsToMinutes(int seconds)
		{
			return (short)(seconds / 60);
		}

		static DateTime ConvertLocalDateTimeToDateTime(LocalDateTime localDateTime)
		{
			return new DateTime(localDateTime.Year, localDateTime.Month, localDateTime.Day, localDateTime.Hour, localDateTime.Minute, localDateTime.Second);
		}

		static LocalDateTime ConvertDateTimeToLocalDateTime(DateTime dateTime)
		{
			return new LocalDateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, dateTime.Second);
		}

		static List<RefUNLOCOUtcOffsetRecord> GenerateTransitionBetweenDates(LocalDateTime startDate, LocalDateTime endDate, ZoneInterval interval)
		{
			var result = new List<RefUNLOCOUtcOffsetRecord>();
			var offset = interval.Savings.Seconds != 0 ? ConvertSecondsToMinutes(interval.WallOffset.Seconds) : ConvertSecondsToMinutes(interval.StandardOffset.Seconds);
			var startDateUsed = false;
			var fromYear = (interval.HasStart && interval.IsoLocalStart.Year > startDate.Year) ? interval.IsoLocalStart.Year : startDate.Year;
			var toYear = (interval.HasEnd && interval.IsoLocalEnd.Year < endDate.Year) ? interval.IsoLocalEnd.Year : endDate.Year;
			for (int i = fromYear; i < toYear; i++)
			{
				var record = new RefUNLOCOUtcOffsetRecord();
				record.UtcOffset = offset;
				if (startDateUsed || !interval.HasStart || interval.IsoLocalStart <= startDate)
				{
					record.StartDate = new DateTime(i, 1, 1, 0, 0, 0).AddMinutes(offset * -1);
				}
				else if (interval.HasStart)
				{
					record.StartDate = ConvertLocalDateTimeToDateTime(interval.IsoLocalStart).AddMinutes(offset * -1);
					startDateUsed = true;
				}
				if (!interval.HasEnd || i <= interval.IsoLocalEnd.Year)
				{
					record.EndDate = new DateTime(i + 1, 1, 1, 0, 0, 0).AddMinutes(offset * -1);
				}
				else if (interval.HasEnd)
				{
					record.EndDate = ConvertLocalDateTimeToDateTime(interval.IsoLocalEnd).AddMinutes(offset * -1);
				}
				result.Add(record);
			}
			return result;
		}

		protected List<RefUNLOCOUtcOffsetRecord> TransformTransitions(List<ZoneInterval> intervals)
		{
			var result = new List<RefUNLOCOUtcOffsetRecord>();
			foreach (var interval in intervals)
			{
				if (interval.HasStart && interval.HasEnd)
				{
					var offset = interval.Savings.Seconds != 0 ? ConvertSecondsToMinutes(interval.WallOffset.Seconds) : ConvertSecondsToMinutes(interval.StandardOffset.Seconds);
					result.Add(new RefUNLOCOUtcOffsetRecord()
					{
						UtcOffset = offset,
						StartDate = ConvertLocalDateTimeToDateTime(interval.IsoLocalStart).AddMinutes(offset * -1),
						EndDate = ConvertLocalDateTimeToDateTime(interval.IsoLocalEnd).AddMinutes(offset * -1)
					});
				}
				else
				{
					var startAndEndDate = GetDateRangeForNeededRecords();
					var startParameter = ConvertDateTimeToLocalDateTime(startAndEndDate.startDate);
					var endParameter = ConvertDateTimeToLocalDateTime(startAndEndDate.endDate);
					result.AddRange(GenerateTransitionBetweenDates(startParameter, endParameter, interval));
				}
			}
			return result;
		}

		protected List<ZoneInterval> GetTransitions(DateTimeZone timeZone)
		{
			var startAndEndDate = GetDateRangeForNeededRecords();
			var yearStart = new LocalDateTime(startAndEndDate.startDate.Year, startAndEndDate.startDate.Month, startAndEndDate.startDate.Day, 0, 0).InZoneLeniently(timeZone).ToInstant();
			var yearEnd = new LocalDateTime(startAndEndDate.endDate.Year, startAndEndDate.endDate.Month, startAndEndDate.endDate.Day, 0, 0).InZoneLeniently(timeZone).ToInstant();
			return timeZone.GetZoneIntervals(yearStart, yearEnd).ToList();
		}

		List<RefUNLOCOUtcOffset> GenerateRefUNLOCOUtcOffsetRecordsFromNodaTime()
		{
			var zoneIntervals = ParseTimezoneDatabase();
			var unlocos = unlocoDataHelper.GetRefUNLOCODictionary();
			var timezoneSets = unlocoDataHelper.GetRefTimezoneSetDictionary();
			var result = new List<RefUNLOCOUtcOffset>();
			foreach (var timezone in timezoneSets)
			{
				unlocos.TryGetValue(timezone.Key, out string[] locations);
				if (locations != null)
				{
					foreach (var unloco in locations)
					{
						foreach (var record in zoneIntervals[timezone.Value.R3_TimeZoneSetName])
						{
							result.Add(record.GetRefUNLOCOUtcOffsetObject(unloco));
						}
					}
				}
			}
			return result;
		}

		public void ExportXml()
		{
			xmlProducer.ExportXML(GenerateRefUNLOCOUtcOffsetRecordsFromNodaTime());
		}

		(DateTime startDate, DateTime endDate) GetDateRangeForNeededRecords()
		{
			return (startDate: requiredDataDate.AddMonths(-requiredOffsetPeriodInMonths), endDate: requiredDataDate.AddMonths(requiredOffsetPeriodInMonths));
		}
	}
}

