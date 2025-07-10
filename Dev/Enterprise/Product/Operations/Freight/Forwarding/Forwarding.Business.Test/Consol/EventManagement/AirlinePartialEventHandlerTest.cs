using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;
using Constants = CargoWise.EventReference.Constants;
using EventReferenceParameterTypes = Enterprise.Core.Constants.EventReferenceParameterTypes;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	[TestedType(typeof(AirlinePartialEventHandler))]
	sealed class AirlinePartialEventHandlerTest : PartialEventHandlerTest
	{
		#region Cancellation

		public override void TestCancelDuplicate()
		{
			AssertCancelDuplicatedEvent(Events.FreightLoadedCode);
			AssertCancelDuplicatedEvent(Events.FreightUnloadedCode);
			AssertCancelDuplicatedEvent(Events.ReceivedCode);
		}

		void AssertCancelDuplicatedEvent(string eventCode)
		{
			var dummy = Factory.New<DummyWithLogs>();

			AddLog(dummy, eventCode, 4, 12, "QF001");

			var otherLog = AddLog(dummy, eventCode, 4, 12, "QF001");

			var handler = new AirlinePartialEventHandler();
			handler.Handle(otherLog, dummy);

			AssertEvents(dummy, eventCode,
				new[]
				{
string.Format(@"{0}|11-Jan-15 10:00:00|Cancelled
Parameters
    FAC|CTO
    FDT|2015-01-10T07:57:00
    LOC|AUSYD
    PTL|4
    TTL|12
    TYP|PARTIAL
    VFL|QF001", eventCode),

string.Format(@"{0}|11-Jan-15 10:00:00|Not Cancelled
Parameters
    FAC|CTO
    FDT|2015-01-10T07:57:00
    LOC|AUSYD
    PTL|4
    TTL|12
    TYP|PARTIAL
    VFL|QF001", eventCode) });
		}

		#endregion

		#region Completion

		public override void TestCompletion()
		{
			AsserCompetionEvent(Events.FreightLoadedCode);
			AsserCompetionEvent(Events.FreightUnloadedCode);
			AsserCompetionEvent(Events.ReceivedCode);
		}

		void AsserCompetionEvent(string eventCode)
		{
			var dummy = Factory.NewWithValidTestData<DummyWithLogs>();

			AddLog(dummy, eventCode, 1, 5, "QF001");

			var otherLog = AddLog(dummy, eventCode, 4, 5, "AA001");

			var handler = new AirlinePartialEventHandler();
			handler.Handle(otherLog, dummy);

			AssertEvents(dummy, eventCode,
				new[]
				{
string.Format(@"{0}|11-Jan-15 10:00:00|Not Cancelled
Parameters
    FAC|CTO
    FDT|2015-01-10T07:57:00
    LOC|AUSYD
    PTL|1
    TTL|5
    TYP|PARTIAL
    VFL|QF001", eventCode),

string.Format(@"{0}|11-Jan-15 10:00:00|Not Cancelled
Parameters
    FAC|CTO
    FDT|2015-01-10T07:57:00
    LOC|AUSYD
    PTL|4
    TTL|5
    TYP|PARTIAL
    VFL|AA001", eventCode),

string.Format(@"{0}|11-Jan-15 10:00:00|Not Cancelled
Parameters
    FAC|CTO
    LOC|AUSYD
    TTL|5
    TYP|COMPLETE", eventCode) });
		}

		#endregion

		#region DuplicateMatching

		public void TestDuplicateMatching_SameDate()
		{
			const string eventCode = Events.FreightLoadedCode;

			var dummy = Factory.New<DummyWithLogs>();

			AddLog(dummy, eventCode, 4, 12, "QF001", new DateTime(2015, 1, 1));

			var otherLog = AddLog(dummy, eventCode, 4, 12, "QF001", new DateTime(2015, 1, 1));

			var handler = new AirlinePartialEventHandler();
			handler.Handle(otherLog, dummy);

			AssertEvents(dummy, eventCode,
				new[]
				{
string.Format(@"{0}|11-Jan-15 10:00:00|Cancelled
Parameters
    FAC|CTO
    FDT|2015-01-01T00:00:00
    LOC|AUSYD
    PTL|4
    TTL|12
    TYP|PARTIAL
    VFL|QF001", eventCode),

string.Format(@"{0}|11-Jan-15 10:00:00|Not Cancelled
Parameters
    FAC|CTO
    FDT|2015-01-01T00:00:00
    LOC|AUSYD
    PTL|4
    TTL|12
    TYP|PARTIAL
    VFL|QF001", eventCode) });
		}

		public void TestDuplicateMatching_DifferentDate()
		{
			const string eventCode = Events.FreightLoadedCode;

			var dummy = Factory.New<DummyWithLogs>();

			AddLog(dummy, eventCode, 4, 12, "QF001", new ZDateTime(2015, 1, 1));

			var otherLog = AddLog(dummy, eventCode, 4, 12, "QF001", new ZDateTime(2015, 1, 2));

			var handler = new AirlinePartialEventHandler();
			handler.Handle(otherLog, dummy);

			AssertEvents(dummy, eventCode,
				new[]
				{
string.Format(@"{0}|11-Jan-15 10:00:00|Not Cancelled
Parameters
    FAC|CTO
    FDT|2015-01-01T00:00:00
    LOC|AUSYD
    PTL|4
    TTL|12
    TYP|PARTIAL
    VFL|QF001", eventCode),

string.Format(@"{0}|11-Jan-15 10:00:00|Not Cancelled
Parameters
    FAC|CTO
    FDT|2015-01-02T00:00:00
    LOC|AUSYD
    PTL|4
    TTL|12
    TYP|PARTIAL
    VFL|QF001", eventCode) });
		}

		public void TestDuplicateMatching_FullMatch()
		{
			const string eventCode = Events.FreightLoadedCode;

			var dummy = Factory.New<DummyWithLogs>();
			AddLog(dummy, eventCode, 4, 12, "QF001", new ZDateTime(2018, 6, 6));

			var otherLog = AddLog(dummy, eventCode, 4, 12, "QF001", new ZDateTime(2018, 6, 6));

			var handler = new AirlinePartialEventHandler();
			handler.Handle(otherLog, dummy);

			AssertEvents(dummy, eventCode,
				new[]
				{
string.Format($@"{eventCode}|11-Jan-15 10:00:00|Not Cancelled
Parameters
    FAC|CTO
    FDT|2018-06-06T00:00:00
    LOC|AUSYD
    PTL|4
    TTL|12
    TYP|PARTIAL
    VFL|QF001"),

string.Format($@"{eventCode}|11-Jan-15 10:00:00|Cancelled
Parameters
    FAC|CTO
    FDT|2018-06-06T00:00:00
    LOC|AUSYD
    PTL|4
    TTL|12
    TYP|PARTIAL
    VFL|QF001") });
		}

		[TestDate(2018, 5, 5)]
		public void TestDuplicateMatching_LogTimeDifference_NotDuplicate()
		{
			const string eventCode = Events.FreightLoadedCode;
			var today = ZDateTime.Today;

			var dummy = Factory.New<DummyWithLogs>();
			var firstLog = AddLog(dummy, eventCode, 4, 12, "QF001", new ZDateTime(2018, 6, 6));
			var secondLog = AddLog(dummy, eventCode, 4, 12, "QF001", new ZDateTime(2018, 6, 6));

			using (firstLog.LockForUpdatingKeyFieldsForTesting())
			using (secondLog.LockForUpdatingKeyFieldsForTesting())
			{
				firstLog.SL_EventTime = today;
				secondLog.SL_EventTime = today.AddDays(1);
			}

			Factory.Save();

			var hander = new AirlinePartialEventHandler();
			hander.Handle(secondLog, dummy);

			AssertEvents(dummy, eventCode,
				new[]
				{
string.Format($@"{eventCode}|05-May-18 00:00:00|Not Cancelled
Parameters
    FAC|CTO
    FDT|2018-06-06T00:00:00
    LOC|AUSYD
    PTL|4
    TTL|12
    TYP|PARTIAL
    VFL|QF001"),

string.Format($@"{eventCode}|06-May-18 00:00:00|Not Cancelled
Parameters
    FAC|CTO
    FDT|2018-06-06T00:00:00
    LOC|AUSYD
    PTL|4
    TTL|12
    TYP|PARTIAL
    VFL|QF001") });
		}

		public void TestDuplicateMatching_PartialParameterMismatch_NotDuplicate()
		{
			const string eventCode = Events.FreightLoadedCode;

			var dummy = Factory.New<DummyWithLogs>();
			AddLog(dummy, eventCode, 4, 12, "QF001", new ZDateTime(2018, 6, 6));

			var otherLog = AddLog(dummy, eventCode, 6, 12, "QF001", new ZDateTime(2018, 6, 6));

			var handler = new AirlinePartialEventHandler();
			handler.Handle(otherLog, dummy);

			AssertEvents(dummy, eventCode,
				new[]
				{
string.Format($@"{eventCode}|11-Jan-15 10:00:00|Not Cancelled
Parameters
    FAC|CTO
    FDT|2018-06-06T00:00:00
    LOC|AUSYD
    PTL|4
    TTL|12
    TYP|PARTIAL
    VFL|QF001"),

string.Format($@"{eventCode}|11-Jan-15 10:00:00|Not Cancelled
Parameters
    FAC|CTO
    FDT|2018-06-06T00:00:00
    LOC|AUSYD
    PTL|6
    TTL|12
    TYP|PARTIAL
    VFL|QF001") });
		}

		public void TestDuplicateMatching_FlightDateMismatch_NotDuplicate()
		{
			const string eventCode = Events.FreightLoadedCode;

			var dummy = Factory.New<DummyWithLogs>();
			AddLog(dummy, eventCode, 4, 12, "QF001", new ZDateTime(2018, 6, 6));

			var otherLog = AddLog(dummy, eventCode, 4, 12, "QF001", new ZDateTime(2018, 6, 7));

			var handler = new AirlinePartialEventHandler();
			handler.Handle(otherLog, dummy);

			AssertEvents(dummy, eventCode,
				new[]
				{
string.Format($@"{eventCode}|11-Jan-15 10:00:00|Not Cancelled
Parameters
    FAC|CTO
    FDT|2018-06-06T00:00:00
    LOC|AUSYD
    PTL|4
    TTL|12
    TYP|PARTIAL
    VFL|QF001"),

string.Format($@"{eventCode}|11-Jan-15 10:00:00|Not Cancelled
Parameters
    FAC|CTO
    FDT|2018-06-07T00:00:00
    LOC|AUSYD
    PTL|4
    TTL|12
    TYP|PARTIAL
    VFL|QF001") });
		}

		#endregion

		#region EventSpecificparameter

		public void TestHandlePartialLog_ParameterTYP_isComplete_WhenFinalEvent()
		{
			var handler = new AirlinePartialEventHandler();
			const string eventCode = Events.FreightLoadedCode;
			var dummy = Factory.NewWithValidTestData<DummyWithLogs>();

			var partialLog1 = AddLog(dummy, eventCode, 1, 5, "QF001");
			handler.Handle(partialLog1, dummy);

			var partialLog2 = AddLog(dummy, eventCode, 4, 5, "AA001");
			handler.Handle(partialLog2, dummy);

			AssertEvents(dummy, eventCode,
				new[]
				{
					string.Format(@"{0}|11-Jan-15 10:00:00|Not Cancelled
Parameters
    FAC|CTO
    FDT|2015-01-10T07:57:00
    LOC|AUSYD
    PTL|1
    TTL|5
    TYP|PARTIAL
    VFL|QF001", eventCode),

					string.Format(@"{0}|11-Jan-15 10:00:00|Not Cancelled
Parameters
    FAC|CTO
    FDT|2015-01-10T07:57:00
    LOC|AUSYD
    PTL|4
    TTL|5
    TYP|PARTIAL
    VFL|AA001", eventCode),

					string.Format(@"{0}|11-Jan-15 10:00:00|Not Cancelled
Parameters
    FAC|CTO
    LOC|AUSYD
    TTL|5
    TYP|COMPLETE", eventCode) });
		}

		#endregion

		#region Implementation

		StmALog AddLog(IStmALogParent logParent, string eventCode, int partial, int total, ZString flightNumber, ZDateTime? flightDate = null)
		{
			flightDate = flightDate ?? new ZDateTime(2015, 1, 10, 7, 57, 0);

			return logParent.Logs.AddNew(Events.All[eventCode],
				ZString.Empty,
				new ZDateTimeOffset(2015, 1, 11, 10, 0, 0),
				new[]
				{
					new KeyValuePair<string, string>(
						Constants.EventReferenceParameters.Codes.Location,
						"AUSYD"),

					new KeyValuePair<string, string>(
						Constants.EventReferenceParameters.Codes.Partial,
						partial.ToString()),

					new KeyValuePair<string, string>(
						Constants.EventReferenceParameters.Codes.Total,
						total.ToString()),

					new KeyValuePair<string, string>(
						Constants.EventReferenceParameters.Codes.Type,
						EventReferenceParameterTypes.Partial),

					new KeyValuePair<string, string>(
						Constants.EventReferenceParameters.Codes.VoyageFlightNumber,
						flightNumber),

					new KeyValuePair<string, string>(
						Constants.EventReferenceParameters.Codes.FlightDate,
						flightDate.Value.ToISO8601String()),

					new KeyValuePair<string, string>(
						Constants.EventReferenceParameters.Codes.Facility,
						"CTO")
				});
		}

		void AssertEvents(IStmALogParent parent, string eventCode, IEnumerable<string> expectedEventLogs)
		{
			var formattedLogs = parent.Logs
				.GetAllLogs()
				.Cast<StmALog>()
				.Where(log => log.SL_SE_NKEvent == eventCode)
				.Select(FormatEventLog)
				.ToArray();

			AssertContainsExactElementsInAnyOrder(string.Format("{0} events", eventCode),
				expectedEventLogs,
				formattedLogs);
		}

		string FormatEventLog(StmALog log)
		{
			var parameters = log.Parameters
				.OrderBy(parameter => parameter.Key)
				.Select(parameter => string.Format("    {0}|{1}", parameter.Key, parameter.Value))
				.ToArray();

			var parametersCaption = string.Join("\r\n", parameters);

			var cancelledCaption = log.SL_IsCancelled
				? "Cancelled"
				: "Not Cancelled";

			return $"{log.SL_SE_NKEvent}|{log.SL_EventTime}|{cancelledCaption}\r\nParameters\r\n{parametersCaption}";
		}

		#endregion
	}
}
