//using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Business
{
	abstract class OrderTrackingDatesMap
	{
		protected OrderTrackingDatesMap(BusinessObject businessObject)
		{
			this.businessObject = Argument.NotNull(businessObject, "businessObject");
		}

		readonly BusinessObject businessObject;

		public abstract ZDateTime DepartureActualDate { get; }
		public abstract ZDateTime ArrivalActualDate { get; }
		public abstract ZDateTime CargoAvailableActualDate { get; }
		public abstract ZDateTime DeliveryCartageAdvisedActualDate { get; }
		public abstract ZDateTime DeliveryCartageCompleteFinalizedActualDate { get; }

		public abstract ZDateTime DepartureScheduledDate { get; }
		public abstract ZDateTime ArrivalScheduledDate { get; }
		public abstract ZDateTime DeliveryCartageCompleteFinalizedScheduledDate { get; }

		public ZDateTimeOffset FindLatestEventLogTime(string eventCode)
		{
			var result = ZDateTimeOffset.Empty;
			var query = CreateEventQuery(eventCode);

			var logs = businessObject.GetLogs()
				.Find(query)
				.OrderByDescending(log => log.SL_EventTime);

			if (logs.Any())
			{
				result = GetCountrySpecificLogEventTime(logs);

				if (!result.IsValid)
				{
					result = GetNonCountrySpecificLogEventTime(logs);
				}
			}

			// Log Event Time is stored in the DB as datetime whereas Order's date properties are in smalldatetime
			return result.IsValid
				? result.ToSmallDateTime()
				: result;
		}

		ZQuery CreateEventQuery(string eventCode)
		{
			var filter = new ZQuery(StmALogSchema.SL_SE_NKEvent, SQLComparisonOperator.Equal, eventCode);
			filter.AddToFilter(JoinCondition.And, StmALogSchema.SL_IsCancelled, SQLComparisonOperator.Equal, ZBool.False.ToString());
			filter.AddToFilter(JoinCondition.And, StmALogSchema.SL_IsEstimate, SQLComparisonOperator.Equal, ZBool.False.ToString());
			return filter;
		}

		ZDateTimeOffset GetCountrySpecificLogEventTime(IEnumerable<StmALog> logs)
		{
			var countrySpecificLog = logs.FirstOrDefault(log => log.SL_Reference.Left(2) == GlbCompany.CurrentCompany.GC_RN_NKCountryCode);

			return countrySpecificLog != null ? countrySpecificLog.EventTimeOffset : ZDateTimeOffset.Empty;
		}

		ZDateTimeOffset GetNonCountrySpecificLogEventTime(IEnumerable<StmALog> logs)
		{
			var firstLog = logs.First();

			return firstLog.SL_Reference.IsEmpty ? firstLog.EventTimeOffset : ZDateTimeOffset.Empty;
		}

		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		public ZDateTime GetDate(OrderProcessTasks task, bool isActual)
		{
			var result = ZDateTime.Empty;

			if (task.IsMilestoneOrWorkflowTrigger)
			{
				switch (task.P9_SE_NKMilestoneEvent)
				{
					case Events.CustomsClearedCode:
					case Events.CustomsCommencedCode:
					case Events.ExportCustomsClearedCode:
					case Events.ExportCustomsCommencedCode:
						if (isActual)
						{
							result = FindLatestEventLogTime(task.P9_SE_NKMilestoneEvent).ToZDateTime();
						}
						break;

					case Events.DepartureCode:
						result = isActual ? DepartureActualDate : DepartureScheduledDate;
						break;

					case Events.ArrivalCode:
						result = isActual ? ArrivalActualDate : ArrivalScheduledDate;
						break;

					case Events.CargoAvailableCode:
						if (isActual)
						{
							result = CargoAvailableActualDate;
						}
						break;

					case Events.DeliveryCartageAdvisedCode:
						if (isActual)
						{
							result = DeliveryCartageAdvisedActualDate;
						}
						break;

					case Events.DeliveryCartageCompleteFinalisedCode:
						result = isActual ? DeliveryCartageCompleteFinalizedActualDate : DeliveryCartageCompleteFinalizedScheduledDate;
						break;
				}
			}

			return result;
		}
	}
}
