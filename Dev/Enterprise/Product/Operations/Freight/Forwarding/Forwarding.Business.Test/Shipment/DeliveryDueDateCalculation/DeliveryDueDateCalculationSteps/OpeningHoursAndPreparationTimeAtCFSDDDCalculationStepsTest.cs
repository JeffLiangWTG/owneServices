using System;
using System.Reflection;
using System.Text;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using static Enterprise.Freight.Forwarding.Business.DeliveryDueDateCalculator;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	abstract class OpeningHoursAndPreparationTimeAtCFSDDDCalculationStepsTest : TestCaseWithFactory
	{
		protected CalendarDayTypeProvider CalendarDayTypeProvider { get; set; } = new CalendarDayTypeProvider();

		protected void ExecutePickupTest(OrgAddress orgAddress, ZDateTime arrival, ZDateTime expected, TimeSpan? holdForPickupTime = null, bool isXtoCFS = false, bool arrivalTimeUsedForDeliveryCFS = false, int allowedError = 0, string expectedLog = null)
		{
			Factory.Save();

			var openingHoursStep = new OpeningHoursDDDCalculationStep(CreateDeliveryDueDateCalculationContext(true, orgAddress), AddressType.PickupAddress, OrgTimetableType.Codes.Pickup, CalendarDayTypeProvider);

			var openingHoursResult = openingHoursStep.Calculate(DeliveryDueDateCalculationResult.Success(arrival, ZString.Empty, arrivalTimeUsedForDeliveryCFS: arrivalTimeUsedForDeliveryCFS));
			AssertEquals("OpeningHoursDDDCalculationStep calculation succeeded", true, openingHoursResult.IsSuccess);

			var cfsStep = new PreparationTimeAtCFSDDDCalculationStep(CreateDeliveryDueDateCalculationContext(true, orgAddress), OrgTimetableType.Codes.Pickup, CalendarDayTypeProvider);
			var cfsResult = cfsStep.Calculate(DeliveryDueDateCalculationResult.Success(openingHoursResult.DeliveryDueDate, ZString.Empty, arrivalTimeUsedForDeliveryCFS: arrivalTimeUsedForDeliveryCFS));
			AssertEquals("PreparationTimeAtCFSDDDCalculationStep calculation succeeded", true, cfsResult.IsSuccess);

			var (message, matchedDateTime) = MatchedDeliveryDate(expected, cfsResult.DeliveryDueDate, allowedError);
			var (actualLog, exceptionMatch) = CombineLogs(expectedLog, openingHoursResult, cfsResult);
			CombineAssertions(() =>
			{
				AssertEquals(message, expected, matchedDateTime);
				AssertEquals("Log check", expectedLog, actualLog);
				AssertEquals("Log exception check", "No exception found", exceptionMatch);
			});
		}

		protected void ExecuteDeliveryTest(OrgAddress cfsAddress, ZDateTime arrival, ZDateTime expected, TimeSpan? holdForPickupTime = null, bool isXtoCFS = false, bool deliverOnWeekend = false, bool arrivalTimeUsedForDeliveryCFS = false, int allowedError = 0, string expectedLog = null)
		{
			Factory.Save();

			var openingHoursStep = new OpeningHoursDDDCalculationStep(CreateDeliveryDueDateCalculationContext(false, cfsAddress), AddressType.CFSDeliveryAddress, OrgTimetableType.Codes.Deliver, CalendarDayTypeProvider);

			FieldInfo isXtoCFSField = typeof(OpeningHoursDDDCalculationStep).GetField("isXtoCFS", BindingFlags.NonPublic | BindingFlags.Instance);
			isXtoCFSField.SetValue(openingHoursStep, isXtoCFS);

			FieldInfo deliverOnWeekendField = typeof(OpeningHoursDDDCalculationStep).GetField("deliverOnWeekend", BindingFlags.NonPublic | BindingFlags.Instance);
			deliverOnWeekendField.SetValue(openingHoursStep, deliverOnWeekend);

			var openingHoursResult = openingHoursStep.Calculate(DeliveryDueDateCalculationResult.Success(arrival, ZString.Empty, arrivalTimeUsedForDeliveryCFS: arrivalTimeUsedForDeliveryCFS));
			AssertEquals("OpeningHoursDDDCalculationStep calculation succeeded", true, openingHoursResult.IsSuccess);

			var cfsStep = new PreparationTimeAtCFSDDDCalculationStep(CreateDeliveryDueDateCalculationContext(false, cfsAddress), OrgTimetableType.Codes.Deliver, CalendarDayTypeProvider);

			FieldInfo holdForPickupTimeField = typeof(PreparationTimeAtCFSDDDCalculationStep).GetField("holdForPickupTime", BindingFlags.NonPublic | BindingFlags.Instance);
			holdForPickupTimeField.SetValue(cfsStep, holdForPickupTime);

			FieldInfo isXtoCFSFieldCFS = typeof(PreparationTimeAtCFSDDDCalculationStep).GetField("isXtoCFS", BindingFlags.NonPublic | BindingFlags.Instance);
			isXtoCFSFieldCFS.SetValue(cfsStep, isXtoCFS);

			FieldInfo deliverOnWeekendFieldCFS = typeof(PreparationTimeAtCFSDDDCalculationStep).GetField("deliverOnWeekend", BindingFlags.NonPublic | BindingFlags.Instance);
			deliverOnWeekendFieldCFS.SetValue(cfsStep, deliverOnWeekend);

			var cfsResult = cfsStep.Calculate(DeliveryDueDateCalculationResult.Success(openingHoursResult.DeliveryDueDate, ZString.Empty, arrivalTimeUsedForDeliveryCFS: arrivalTimeUsedForDeliveryCFS));
			AssertEquals("PreparationTimeAtCFSDDDCalculationStep calculation succeeded", true, cfsResult.IsSuccess);

			var (message, matchedDateTime) = MatchedDeliveryDate(expected, cfsResult.DeliveryDueDate, allowedError);
			var (actualLog, exceptionMatch) = CombineLogs(expectedLog, openingHoursResult, cfsResult);
			CombineAssertions(() =>
			{
				AssertEquals(message, expected, matchedDateTime);
				AssertEquals("Log check", expectedLog, actualLog);
				AssertEquals("Log exception check", "No exception found", exceptionMatch);
			});
		}

		protected void ExecuteDeliverOnWeekendsTest(RefServiceLevel serviceLevel, OrgAddress cfsAddress, ZDateTime arrival, ZDateTime expected, TimeSpan? holdForPickupTime = null, bool arrivalTimeUsedForDeliveryCFS = false, int allowedError = 0, string expectedLog = null)
		{
			Factory.Save();

			var openingHoursStep = new OpeningHoursDDDCalculationStep(CreateDeliveryDueDateCalculationContext(false, cfsAddress), AddressType.CFSDeliveryAddress, OrgTimetableType.Codes.Deliver, CalendarDayTypeProvider);
			FieldInfo deliverOnWeekendField = typeof(OpeningHoursDDDCalculationStep).GetField("deliverOnWeekend", BindingFlags.NonPublic | BindingFlags.Instance);
			deliverOnWeekendField.SetValue(openingHoursStep, true);

			var openingHoursResult = openingHoursStep.Calculate(DeliveryDueDateCalculationResult.Success(arrival, ZString.Empty, arrivalTimeUsedForDeliveryCFS: arrivalTimeUsedForDeliveryCFS));
			AssertEquals("OpeningHoursDDDCalculationStep calculation succeeded", true, openingHoursResult.IsSuccess);

			var cfsStep = new PreparationTimeAtCFSDDDCalculationStep(CreateDeliveryDueDateCalculationContext(false, cfsAddress), OrgTimetableType.Codes.Deliver, CalendarDayTypeProvider);
			FieldInfo holdForPickupTimeField = typeof(PreparationTimeAtCFSDDDCalculationStep).GetField("holdForPickupTime", BindingFlags.NonPublic | BindingFlags.Instance);
			holdForPickupTimeField.SetValue(cfsStep, holdForPickupTime);

			FieldInfo deliverOnWeekendFieldCFS = typeof(PreparationTimeAtCFSDDDCalculationStep).GetField("deliverOnWeekend", BindingFlags.NonPublic | BindingFlags.Instance);
			deliverOnWeekendFieldCFS.SetValue(cfsStep, true);

			var cfsResult = cfsStep.Calculate(DeliveryDueDateCalculationResult.Success(openingHoursResult.DeliveryDueDate, ZString.Empty, arrivalTimeUsedForDeliveryCFS: arrivalTimeUsedForDeliveryCFS));
			AssertEquals("PreparationTimeAtCFSDDDCalculationStep calculation succeeded", true, cfsResult.IsSuccess);

			var deliveryAddress = Factory.NewWithValidTestData<OrgAddress>();
			var deliveryStep = new DeliverOnWeekendCalculationStep(CreateDeliveryDueDateCalculationContext(false, deliveryAddress));

			var deliveryResult = deliveryStep.Calculate(DeliveryDueDateCalculationResult.Success(cfsResult.DeliveryDueDate, ZString.Empty, arrivalTimeUsedForDeliveryCFS: arrivalTimeUsedForDeliveryCFS));

			var (message, matchedDateTime) = MatchedDeliveryDate(expected, deliveryResult.DeliveryDueDate, allowedError);
			var (actualLog, exceptionMatch) = CombineLogs(expectedLog, openingHoursResult, cfsResult, deliveryResult);
			CombineAssertions(() =>
				{
					AssertEquals(message, expected, matchedDateTime);
					AssertEquals("Log check", expectedLog, actualLog);
					AssertEquals("Log exception check", "No exception found", exceptionMatch);
				});
		}

		(string, string) CombineLogs(string expectedLog, params IDeliveryDueDateCalculationResult[] results)
		{
			var actualLog = new StringBuilder();
			foreach (var result in results)
			{
				actualLog.Append(result.CalculationLog);
			}

			var logMatch = expectedLog != null ? actualLog.ToString() : null;

			var exceptionMatch =
				actualLog.ToString().Contains("Exception") ? actualLog.ToString() : "No exception found";

			return (logMatch, exceptionMatch);
		}

		(string, ZDateTime) MatchedDeliveryDate(ZDateTime expected, ZDateTime actual, int allowedError)
		{
			if (allowedError == 0)
			{
				return ("Expected delivery due date", actual);
			}

			var message = $"Expected delivery due date (allowed error = {allowedError})";
			var diff = Math.Round((actual - expected).TotalMinutes);
			if (diff == 0)
			{
				return (message, actual);
			}

			if (allowedError > 0 && diff > 0 && diff <= allowedError)
			{
				// force a match
				return (message, expected);
			}

			if (allowedError < 0 && diff < 0 && diff >= allowedError)
			{
				// force a match
				return (message, expected);
			}

			return (message, actual);
		}

		/// <summary>
		/// Three time blocks A [B] Z with optional cutoff B
		/// </summary>
		protected OrgAddress CreateAddressNoBreaks(int processingTime, DateTime cutoffTime = default, string countryCode = "XX", string stateCode = "YY", bool saturday = false, bool sunday = false)
		{
			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();

			// This must be set before the timetables are deleted
			orgAddress.OA_RN_NKCountryCode = countryCode;
			orgAddress.State = stateCode;

			orgAddress.Timetables.allowDeleteLastTimeTable = true;
			orgAddress.Timetables.DeleteAll();

			orgAddress.SetTimetablesRangeType(OrgTimeTableRangeType.Advanced);

			var days = new[] { "MON", "TUE", "WED", "THU", "FRI" };
			foreach (var day in days)
			{
				DeliveryDueDateCalculationTestHelper.AddTimetable(orgAddress, OrgTimetableType.Codes.Deliver, 9, 0, 17, 0, isAdvanced: true, advancedDay: day, processingTime: processingTime, cutoffTime);
				DeliveryDueDateCalculationTestHelper.AddTimetable(orgAddress, OrgTimetableType.Codes.Pickup, 9, 0, 17, 0, isAdvanced: true, advancedDay: day, processingTime: processingTime, cutoffTime);
			}

			if (saturday)
			{
				DeliveryDueDateCalculationTestHelper.AddTimetable(orgAddress, OrgTimetableType.Codes.Deliver, 9, 30, 15, 30, isAdvanced: true, advancedDay: "SAT", processingTime: processingTime, cutoffTime);
				DeliveryDueDateCalculationTestHelper.AddTimetable(orgAddress, OrgTimetableType.Codes.Pickup, 9, 30, 15, 30, isAdvanced: true, advancedDay: "SAT", processingTime: processingTime, cutoffTime);
			}

			if (sunday)
			{
				DeliveryDueDateCalculationTestHelper.AddTimetable(orgAddress, OrgTimetableType.Codes.Deliver, 10, 0, 15, 0, isAdvanced: true, advancedDay: "SUN", processingTime: processingTime, cutoffTime);
				DeliveryDueDateCalculationTestHelper.AddTimetable(orgAddress, OrgTimetableType.Codes.Pickup, 10, 0, 15, 0, isAdvanced: true, advancedDay: "SUN", processingTime: processingTime, cutoffTime);
			}

			orgAddress.Timetables.ReInitializeRangeType();

			Factory.Save();

			return orgAddress;
		}

		/// <summary>
		/// Five time blocks: A [B] C [D] Z with optional cutoff B or D
		/// </summary>
		protected OrgAddress CreateAddressOneBreak(int processingTime, DateTime cutoffTime = default, string countryCode = "XX", string stateCode = "YY", bool saturday = false, bool sunday = false)
		{
			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();

			// This must be set before the timetables are deleted
			orgAddress.OA_RN_NKCountryCode = countryCode;
			orgAddress.State = stateCode;

			orgAddress.Timetables.allowDeleteLastTimeTable = true;
			orgAddress.Timetables.DeleteAll();

			orgAddress.SetTimetablesRangeType(OrgTimeTableRangeType.Advanced);

			var days = new[] { "MON", "TUE", "WED", "THU", "FRI" };
			foreach (var day in days)
			{
				DeliveryDueDateCalculationTestHelper.AddTimetable(orgAddress, OrgTimetableType.Codes.Deliver, 9, 0, 12, 15, isAdvanced: true, advancedDay: day, processingTime: processingTime, cutoffTime);
				DeliveryDueDateCalculationTestHelper.AddTimetable(orgAddress, OrgTimetableType.Codes.Deliver, 13, 0, 17, 0, isAdvanced: true, advancedDay: day, processingTime: 0, default);
				DeliveryDueDateCalculationTestHelper.AddTimetable(orgAddress, OrgTimetableType.Codes.Pickup, 9, 0, 12, 15, isAdvanced: true, advancedDay: day, processingTime: 0, default);
				DeliveryDueDateCalculationTestHelper.AddTimetable(orgAddress, OrgTimetableType.Codes.Pickup, 13, 0, 17, 0, isAdvanced: true, advancedDay: day, processingTime: processingTime, cutoffTime);
			}

			if (saturday)
			{
				DeliveryDueDateCalculationTestHelper.AddTimetable(orgAddress, OrgTimetableType.Codes.Deliver, 9, 30, 12, 15, isAdvanced: true, advancedDay: "SAT", processingTime: processingTime, cutoffTime);
				DeliveryDueDateCalculationTestHelper.AddTimetable(orgAddress, OrgTimetableType.Codes.Deliver, 13, 0, 15, 30, isAdvanced: true, advancedDay: "SAT", processingTime: processingTime, cutoffTime);
				DeliveryDueDateCalculationTestHelper.AddTimetable(orgAddress, OrgTimetableType.Codes.Pickup, 9, 30, 12, 15, isAdvanced: true, advancedDay: "SAT", processingTime: processingTime, cutoffTime);
				DeliveryDueDateCalculationTestHelper.AddTimetable(orgAddress, OrgTimetableType.Codes.Pickup, 13, 0, 15, 30, isAdvanced: true, advancedDay: "SAT", processingTime: processingTime, cutoffTime);
			}

			if (sunday)
			{
				DeliveryDueDateCalculationTestHelper.AddTimetable(orgAddress, OrgTimetableType.Codes.Deliver, 10, 0, 15, 0, isAdvanced: true, advancedDay: "SUN", processingTime: processingTime, cutoffTime);
				DeliveryDueDateCalculationTestHelper.AddTimetable(orgAddress, OrgTimetableType.Codes.Deliver, 13, 0, 15, 0, isAdvanced: true, advancedDay: "SUN", processingTime: processingTime, cutoffTime);
				DeliveryDueDateCalculationTestHelper.AddTimetable(orgAddress, OrgTimetableType.Codes.Pickup, 10, 0, 12, 15, isAdvanced: true, advancedDay: "SUN", processingTime: processingTime, cutoffTime);
				DeliveryDueDateCalculationTestHelper.AddTimetable(orgAddress, OrgTimetableType.Codes.Pickup, 13, 0, 15, 0, isAdvanced: true, advancedDay: "SUN", processingTime: processingTime, cutoffTime);
			}

			orgAddress.Timetables.ReInitializeRangeType();

			Factory.Save();

			return orgAddress;
		}

		/// <summary>
		/// One time block [A] with optional cutoff. Processing time is normal (within one day) or overlapping midnight.
		/// </summary>
		protected OrgAddress CreateAddressTwentyFourSeven(int processingTime, DateTime cutoffTime = default, string countryCode = "XX", string stateCode = "YY")
		{
			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();

			// This must be set before the timetables are deleted
			orgAddress.OA_RN_NKCountryCode = countryCode;
			orgAddress.State = stateCode;

			orgAddress.Timetables.allowDeleteLastTimeTable = true;
				orgAddress.Timetables.DeleteAll();

			orgAddress.SetTimetablesRangeType(OrgTimeTableRangeType.Advanced);

			var days = new[] { "MON", "TUE", "WED", "THU", "FRI", "SAT", "SUN" };
			foreach (var day in days)
			{
				DeliveryDueDateCalculationTestHelper.AddTimetable(orgAddress, OrgTimetableType.Codes.Deliver, 0, 0, 23, 59, isAdvanced: true, advancedDay: day, processingTime: processingTime, cutoffTime);
				DeliveryDueDateCalculationTestHelper.AddTimetable(orgAddress, OrgTimetableType.Codes.Pickup, 0, 0, 23, 59, isAdvanced: true, advancedDay: day, processingTime: processingTime, cutoffTime);
			}

			orgAddress.Timetables.ReInitializeRangeType();

			Factory.Save();

			return orgAddress;
		}

		#region Helper Methods

		DeliveryDueDateCalculationContext CreateDeliveryDueDateCalculationContext(bool isPickup, OrgAddress orgAddress)
		{
			return new DeliveryDueDateCalculationContext(Factory,
				ZDateTime.Now,
				"STD",
				ZString.Empty,
				isPickup ? orgAddress.Header.OH_Code : ZString.Empty,
				isPickup ? orgAddress.AddressCode : ZString.Empty,
				isPickup ? orgAddress.Header.OH_Code : ZString.Empty,
				isPickup ? orgAddress.AddressCode : ZString.Empty,
				isPickup ? ZString.Empty : orgAddress.Header.OH_Code,
				isPickup ? ZString.Empty : orgAddress.AddressCode,
				ZString.Empty,
				ZString.Empty,
				ZString.Empty,
				ZString.Empty
			);
		}

		#endregion
	}
}
