using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Integration;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Business
{
	public class DeliveryDueDateWithExceptionsCalculator
	{
		public DeliveryDueDateWithExceptionsCalculator(ForwardingShipment shipment, IStmALog triggeringEvent = null)
		{
			agentDeliveryAddress = (shipment.JS_OA_ImportReleaseDepot_ZAddress?.OrgAddress as OrgAddress)
				?? shipment.DeliveryAgent?.GetAddressWithFallback(ZArchitecture.Business.AddressType.DLV);

			this.shipment = shipment;
			this.triggeringEvent = triggeringEvent;
			deliveryDueDate = shipment.JS_DeliveryDueDate;
			calculationLogBuilder = new ZStringBuilder();
			if (agentDeliveryAddress != null)
			{
				calendarDayTypeProvider = new CalendarDayTypeProvider();
				closestOpeningHourFinder = new ClosestOpeningHourFinder(agentDeliveryAddress, OrgTimetableType.Codes.Deliver, calendarDayTypeProvider, calculationLogBuilder);
			}
			deliveryDueDateCalculationContext = new DeliveryDueDateCalculationContext(shipment);
		}
		readonly DeliveryDueDateCalculationContext deliveryDueDateCalculationContext;
		readonly ForwardingShipment shipment;
		readonly OrgAddress agentDeliveryAddress;
		readonly IStmALog triggeringEvent;
		readonly ZDateTime deliveryDueDate;
		readonly ClosestOpeningHourFinder closestOpeningHourFinder;
		readonly CalendarDayTypeProvider calendarDayTypeProvider;
		readonly ZStringBuilder calculationLogBuilder;

		public IDeliveryDueDateCalculationResult CalculateDeliveryDueDateWithExceptions()
		{
			if (shipment.Factory?.IsInSaveTransaction ?? false)
			{
				RefreshDefaultTimetableForAddress(agentDeliveryAddress);
			}

			if (shipment.JS_DeliveryDueDate.IsEmpty)
			{
				var failureMesaage = Res.GetString("dc97053c-10cd-47a3-ae3c-494f19968fa3", "Checking Previous Delivery Due Date: Failed to calculate [Delivery Due Date with Exceptions] because Delivery Due Date is empty.");
				return DeliveryDueDateCalculationResult.Failure(failureMesaage, failureMesaage);
			}

			var serviceLevel = deliveryDueDateCalculationContext.Factory.LoadFromNaturalKey<RefServiceLevel>(RefServiceLevelSchema.RS_Code, shipment.JS_RS_NKServiceLevel);
			if (agentDeliveryAddress == null && (serviceLevel == null || serviceLevel.RS_DefaultTransitHours == 0))
			{
				var failureMesaage = Res.GetString("1cc2a969-5417-495d-be99-b467648276e0", "Finding CFS/Transit Warehouse and Service Level Default Transit Time: Failed to calculate [Delivery Due Date with Exceptions] because Delivery > CFS/Transit Warehouse is empty and Service Level does not have default transit time.");
				if (deliveryDueDateCalculationContext.IsDTC)
				{
					failureMesaage = Res.GetString("2cc2a960-5417-495d-be99-b467648276b1", "Finding Delivery Agent Address and Service Level Default Transit Time: Failed to calculate [Delivery Due Date with Exceptions] because Delivery > Delivery Agent Address is empty and Service Level does not have default transit time.");
				}
				return DeliveryDueDateCalculationResult.Failure(failureMesaage, failureMesaage);
			}

			calculationLogBuilder.AppendLine(Res.GetString("4103993e-d4d7-49a8-934d-692e38bc1653", "Calculating Delivery Due Date With Exceptions..."));

			var exceptions = GetExceptionsIncludingRelated();
			LogCalculationReasonDetail();
			calculationLogBuilder.AppendLine(Res.GetString("e08d2b02-0c70-4a93-8724-355f8edb8300", "Original Delivery Due Date: {0}", deliveryDueDate));
			var finalDateTime = ApplyExceptions(exceptions, deliveryDueDate);

			if (deliveryDueDateCalculationContext.DeliverOnWeekend)
			{
				calculationLogBuilder.AppendLine(
					Res.GetString("7b46008d-4705-4f55-a004-1b7793bdb12d", "Delivery due time adjusted as per the selected weekend service.",
					finalDateTime));
				return DeliveryDueDateCalculationResult.Success(finalDateTime, calculationLogBuilder.ToString());
			}

			if (deliveryDueDateCalculationContext.IsDTC)
			{
				calculationLogBuilder.AppendLine(
					Res.GetString("c8962df7-138a-4299-9f0f-5bf7feb4efbc", "Delivery due time has not been adjusted because it is DTC. Calculated revised delivery due date by applying exception delays: {0}",
					finalDateTime));
				return DeliveryDueDateCalculationResult.Success(finalDateTime, calculationLogBuilder.ToString());
			}

			if (deliveryDueDateCalculationContext.IsXtoAirport)
			{
				calculationLogBuilder.AppendLine(
					Res.GetString("c00a374b-dece-4cd9-ae46-5337c43f35e1", "Delivery due time has not been adjusted because delivery is to airport. Calculated revised delivery due date by applying exception delays: {0}",
					finalDateTime));
				return DeliveryDueDateCalculationResult.Success(finalDateTime, calculationLogBuilder.ToString());
			}

			if (deliveryDueDateCalculationContext.IsXtoCFS)
			{
				if (deliveryDueDateCalculationContext.HoldForPickupTime.HasValue)
				{
					return DeliveryDueDateCalculationHelper.SetHoldForPickupTime(finalDateTime, calculationLogBuilder, deliveryDueDateCalculationContext.HoldForPickupTime, calendarDayTypeProvider, deliveryDueDateCalculationContext.CFSDeliveryAddress);
				}
				else
				{
					return SetTimeBasedOnclosestOpeningHour();
				}
			}
			else if (deliveryDueDateCalculationContext.DeliveryDueTime != null && deliveryDueDateCalculationContext.IsXtoDoor)
			{
				var result = DeliveryDueDateCalculationHelper.AdjustTimeToDeliverDueTime(finalDateTime, calculationLogBuilder, deliveryDueDateCalculationContext.DeliveryDueTime, calendarDayTypeProvider, deliveryDueDateCalculationContext.DeliveryAddress);
				return DeliveryDueDateCalculationResult.Success(result, calculationLogBuilder.ToString());
			}
			else
			{
				return SetTimeBasedOnclosestOpeningHour();
			}

			IDeliveryDueDateCalculationResult SetTimeBasedOnclosestOpeningHour()
			{
				finalDateTime = FindClosestOpeningHour(finalDateTime);
				calculationLogBuilder.AppendLine(Res.GetString("40c7ee28-93e5-45c7-a123-80ec4c455ed6", "Calculated revised delivery due date by applying exception delays and finding closest opening hours: {0}",
					finalDateTime));
				return DeliveryDueDateCalculationResult.Success(finalDateTime, calculationLogBuilder.ToString());
			}

			void LogCalculationReasonDetail()
			{
				if (triggeringEvent != null && triggeringEvent.SL_Reference.Contains(RoutingSupportProcessTask.ExceptionDeletedReason))
				{
					calculationLogBuilder.AppendLine(Res.GetString("b2d630c5-ec68-47b7-b8e0-9898ed53380c", "Latest change: An exception has been deleted"));
				}
				else
				{
					if (triggeringEvent != null && triggeringEvent.SL_Reference.Contains(RoutingSupportProcessTask.ExceptionHoursChangedReason))
					{
						calculationLogBuilder.AppendLine(Res.GetString("054ecc18-da79-4204-8b0f-cc62b9ee1e58", "Latest change: Exception duration hours changed"));
					}

					var lastChangedException = exceptions.OrderBy(x => x.P9_SystemLastEditTimeUtc).LastOrDefault();
					if (lastChangedException != null)
					{
						calculationLogBuilder.AppendLine(Res.GetString("3f1046ea-6992-4e21-8499-aae5ac35485d", "Latest Added/Changed Exception [{0}]", GetExceptionDescription(lastChangedException)));
					}
				}
			}
		}

		ZDateTime FindClosestOpeningHour(ZDateTime finalDateTime)
		{
			if (closestOpeningHourFinder != null)
			{
				(var closestOpeningHour, var nonWorkingDays) = closestOpeningHourFinder.GetClosestOpeningHour(finalDateTime);
				if (nonWorkingDays.Any())
				{
					calculationLogBuilder.AppendLine(
						Res.GetString("01f3219d-a7d1-4bc7-b1f9-b1bc28d6688a", "Weekend Days/Public Holidays of [{0}]: {1}",
						agentDeliveryAddress.AddressFull.RemoveEndLines(), nonWorkingDays.ToStringWithDayOfWeeks()));
				}
				calculationLogBuilder.AppendLine(
						Res.GetString("c4b39c25-a25c-41da-a6bb-8697d34295c6", "Finding closest opening hour: {0} adjusted to {1}",
						finalDateTime, closestOpeningHour));
				finalDateTime = closestOpeningHour;
			}

			return finalDateTime;
		}

		ZDateTime ApplyExceptions(IEnumerable<ProcessTask> exceptions, ZDateTime initialDateTime)
		{
			var maxDurationPerDay = TimeSpan.FromHours(FreightDataRegistry.Instance.CalculateDeliveryDateWithExceptions.Value.MaximumDurationHours);
			var unlimitedDuration = FreightDataRegistry.Instance.CalculateDeliveryDateWithExceptions.Value.UnlimitedDuration || maxDurationPerDay <= TimeSpan.Zero;
			var exceptionsApplied = new Dictionary<ProcessTask, ExceptionDurationAppliedResult>();
			var previousDate = ZDateTime.Empty;

			var remainingDurationPerDay = TimeSpan.Zero;
			var durationAppliedPerDay = TimeSpan.Zero;
			var revisedDateTime = initialDateTime;

			foreach (var exception in exceptions)
			{
				var currentDate = exception.P9_ActualDate.Date;

				if (currentDate != previousDate)
				{
					remainingDurationPerDay = maxDurationPerDay;
					durationAppliedPerDay = TimeSpan.Zero;
				}

				var durationApplied = unlimitedDuration
					? TimeSpan.FromHours(exception.P9_ExceptionDurationHours)
					: TimeSpan.FromTicks(Math.Min(remainingDurationPerDay.Ticks, TimeSpan.FromHours(exception.P9_ExceptionDurationHours).Ticks));

				remainingDurationPerDay -= durationApplied;
				durationAppliedPerDay += durationApplied;

				revisedDateTime = AddDurationAndNonWorkingDays(exception, revisedDateTime, durationApplied);
				exceptionsApplied.Add(exception, new ExceptionDurationAppliedResult(exception, durationApplied, revisedDateTime));

				previousDate = currentDate;
			}
			var lastExceptionApplied = exceptionsApplied.LastOrDefault();
			var finalDateTime = lastExceptionApplied.Value?.DeliveryDueDate ?? deliveryDueDate;

			return finalDateTime;
		}

		ZDateTime AddDurationAndNonWorkingDays(ProcessTask exception, ZDateTime initialDateTime, TimeSpan duration)
		{
			const int maxIterations = 10;
			var durationRemaining = duration;
			var revisedDateTime = initialDateTime;
			var deliveryOnWeekendTickedTipsBuilder = new StringBuilder();
			var nonWorkingDays = new List<ZDateTime>();

			for (var i = 0; i < maxIterations; i++)
			{
				if (durationRemaining <= TimeSpan.Zero)
				{
					break;
				}

				if (!deliveryDueDateCalculationContext.IsXtoAirport && calendarDayTypeProvider.IsNonWorkingDay(agentDeliveryAddress, OrgTimetableType.Codes.Deliver, revisedDateTime.Date, calculationLogBuilder))
				{
					(durationRemaining, revisedDateTime) = AddExceptionInNonWorkingDay(durationRemaining, revisedDateTime, deliveryOnWeekendTickedTipsBuilder, nonWorkingDays);
				}
				else
				{
					var durationUntilMidnight = TimeSpan.FromHours(24) - revisedDateTime.TimeOfDay;
					var durationApplied = TimeSpan.FromTicks(Math.Min(durationUntilMidnight.Ticks, durationRemaining.Ticks));
					durationRemaining -= durationApplied;
					revisedDateTime += durationApplied;
				}
			}

			if (deliveryDueDateCalculationContext.IsXtoAirport)
			{
				return revisedDateTime;
			}

			var nextAvailableDay = Res.GetString("CC3EF1E0-9866-4436-B461-3181F389A167", "The next available business day for Revised Delivery Due Date was: {0} which is {1}.",
				revisedDateTime,
				revisedDateTime.DayOfWeek);

			var logDuringAddException = new StringBuilder();
			if (nonWorkingDays.Any())
			{
				logDuringAddException.Append(
						Res.GetString("81cc7ef2-dce5-4038-9bde-a1f4d5251516", "Weekend Days/Public Holidays of [{0}]: {1}",
						agentDeliveryAddress.AddressFull.RemoveEndLines(), nonWorkingDays.ToStringWithDayOfWeeks())).Append(" ");
			}
			if (deliveryOnWeekendTickedTipsBuilder.Length > 0)
			{
				logDuringAddException.Append(deliveryOnWeekendTickedTipsBuilder).Append(" ");
			}
			
			if (revisedDateTime.Date > initialDateTime.Date)
			{
				logDuringAddException.Append(nextAvailableDay).Append(" ");
			}

			if (logDuringAddException.Length > 0)
			{
				var showLog = Res.GetString("94586a76-12e2-4b34-82a5-73619b0d66aa", "Applying exception {0}: {1}",
					exception.P9_Description,
					logDuringAddException.Remove(logDuringAddException.Length - 1, 1));

				calculationLogBuilder.AppendLine(Res.GetString("AD39B8D9-F4A0-473D-943E-23D95B6AA73A", "{0}", showLog));
			}

			return revisedDateTime;
		}

		(TimeSpan DurationRemaining, ZDateTime RevisedDateTime) AddExceptionInNonWorkingDay(TimeSpan durationRemaining, ZDateTime revisedDateTime, StringBuilder deliveryOnWeekendTickedTipsBuilder, List<ZDateTime> nonWorkingDayList)
		{
			var (closestOpeningHour, nonWorkingDays) = closestOpeningHourFinder.FindNextWorkingDay(revisedDateTime);
			var durationUntilMidnight = TimeSpan.FromHours(24) - revisedDateTime.TimeOfDay;
			var (weekendDay1, weekendDay2) = DeliveryDueDateCalculationHelper.GetWeekendDays(agentDeliveryAddress, deliveryDueDateCalculationContext.Factory); // TODO can have more than two weekend days
			var durationApplied = TimeSpan.FromTicks(Math.Min(durationUntilMidnight.Ticks, durationRemaining.Ticks));
			
			if (revisedDateTime.DayOfWeek == weekendDay1 && deliveryDueDateCalculationContext.ServiceLevel.RS_DeliverOnSaturday)
			{
				durationRemaining -= durationApplied;
				revisedDateTime = HandleDeliveryOnFirstWeekendDay(revisedDateTime, closestOpeningHour, durationApplied, deliveryOnWeekendTickedTipsBuilder, nonWorkingDays);
			}
			else if (revisedDateTime.DayOfWeek == weekendDay2 && deliveryDueDateCalculationContext.ServiceLevel.RS_DeliverOnSunday)
			{
				durationRemaining -= durationApplied;
				revisedDateTime = HandleDeliveryOnSecondWeekendDay(revisedDateTime, closestOpeningHour, durationApplied, deliveryOnWeekendTickedTipsBuilder, nonWorkingDays);
			}
			else
			{
				revisedDateTime = closestOpeningHour;
			}

			if (nonWorkingDays.Any())
			{
				nonWorkingDayList.AddRange(nonWorkingDays);
			}

			return (durationRemaining, revisedDateTime);
		}

		ZDateTime HandleDeliveryOnFirstWeekendDay(ZDateTime revisedDateTime,
			ZDateTime closestOpeningHour,
			TimeSpan durationApplied,
			StringBuilder deliveryOnWeekendTickedTipsBuilder,
			List<ZDateTime> nonWorkingDayList)
		{
			if (!IsPublicHoliday(revisedDateTime.Date))
			{
				if (!deliveryDueDateCalculationContext.ServiceLevel.RS_DeliverOnSunday)
				{
					deliveryOnWeekendTickedTipsBuilder.Append(Res.GetString("85a167ba-bb26-4e5a-8187-ae455fa56e42",
						"Service Level {0}, has delivery on weekend ticked.",
						deliveryDueDateCalculationContext.ServiceLevel.RS_Code,
						closestOpeningHour,
						closestOpeningHour.DayOfWeek));

					nonWorkingDayList.RemoveAt(0);
					return closestOpeningHour;
				}
				else
				{
					var newRevisedDateTime = revisedDateTime.Add(durationApplied);
					if (IsPublicHoliday(newRevisedDateTime.Date))
					{
						deliveryOnWeekendTickedTipsBuilder.Append(Res.GetString("11a16dba-dd26-4e5a-2287-ae455fa56e42",
							"Service Level {0}, has delivery on weekend ticked. Delivery Due Date is {1} which is {2}.",
							deliveryDueDateCalculationContext.ServiceLevel.RS_Code,
							deliveryDueDate,
							deliveryDueDate.DayOfWeek));

						nonWorkingDayList.RemoveAt(0);
						return closestOpeningHour;
					}
					else
					{
						nonWorkingDayList.RemoveRange(0, 2);
						return newRevisedDateTime;
					}
				}
			}

			return closestOpeningHour;
		}

		ZDateTime HandleDeliveryOnSecondWeekendDay(ZDateTime revisedDateTime,
			ZDateTime closestOpeningHour,
			TimeSpan durationApplied,
			StringBuilder deliveryOnWeekendTickedTipsBuilder,
			List<ZDateTime> nonWorkingDayList)
		{
			if (!IsPublicHoliday(revisedDateTime) && durationApplied != TimeSpan.FromDays(1))
			{
				var result = revisedDateTime.Add(durationApplied);
				deliveryOnWeekendTickedTipsBuilder.Append(Res.GetString("85b167ba-dd26-4e5d-8187-ae455fa56e22",
						"Service Level {0}, has delivery on weekend ticked.",
						deliveryDueDateCalculationContext.ServiceLevel.RS_Code));
				nonWorkingDayList.RemoveAt(0);
				return result;
			}

			return closestOpeningHour;
		}

		bool IsPublicHoliday(ZDateTime revisedDateTime)
		{
			return calendarDayTypeProvider.IsPublicHoliday(agentDeliveryAddress, OrgTimetableType.Codes.Deliver, revisedDateTime.Date, calculationLogBuilder);
		}

		ZString GetExceptionDescription(ProcessTask exception)
		{
			return $"Type: {exception.ExceptionTypeDescription}|Description: {exception.P9_Description}|Time: {exception.P9_ActualDateForBinding.ToZDateTime()}|Duration: {exception.P9_ExceptionDurationHours} Hours";
		}

		List<ProcessTask> GetExceptionsIncludingRelated()
		{
			Func<ProcessTask, bool> isTriggeredByExceptionDeleted = x =>
			{
				if (triggeringEvent == null || x.PK.IsEmpty)
				{
					return false;
				}

				var exceptionPK = RoutingSupportProcessTask.GetExceptionDeletedPKFromTriggeringEvent(triggeringEvent);
				return x.PK.Equals(exceptionPK);
			};

			var shipmentExceptions = shipment.WorkflowItems.Exceptions.OfType<ProcessTask>();
			var consolExceptions = shipment.Consols.OfType<ForwardingConsol>().SelectMany(c => c.WorkflowItems.Exceptions).OfType<ProcessTask>();
			var containerExceptions = shipment.Containers.SelectMany(c => c.WorkflowItems.Exceptions).OfType<ProcessTask>();

			return shipmentExceptions.Concat(consolExceptions).Concat(containerExceptions)
				.Where(x => (x is IExceptionDurationProcessTask) && x.P9_ExceptionDurationHours > 0 && !isTriggeredByExceptionDeleted(x))
				.OrderBy(x => (x.P9_ActualDateOffset, x.P9_TaskID))
				.ToList();
		}

		static void RefreshDefaultTimetableForAddress(IDocAddress address)
		{
			if (address is OrgAddress orgAddress
				&& orgAddress.TimetablesRangeType == OrgTimeTableRangeType.Default
				&& orgAddress.Timetables.Count == 0)
			{
				orgAddress.Timetables.RefreshFromDb();
			}
		}
	}
}
