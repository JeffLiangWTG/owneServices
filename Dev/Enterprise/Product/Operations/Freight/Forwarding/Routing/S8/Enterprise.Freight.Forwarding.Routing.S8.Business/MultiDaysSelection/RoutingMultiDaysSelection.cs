namespace Enterprise.Freight.Forwarding.Routing.S8.Business
{
	using System.Collections.Generic;
	using System.Linq;
	using CargoWise.EntityFramework;
	using CargoWise.Types;
	using Enterprise.Freight.Business;
	using Enterprise.Freight.Forwarding.Business;

	public class RoutingMultiDaysSelection : MultiDaysSelection
	{
		public static RoutingMultiDaysSelection Create(
			ZDateTime requestedDate,
			RoutingResponseHeaderCollection routingResponseHeaders,
			bool includeWeeklyTimetable,
			bool importAndCreateMawb,
			BusinessObjectFactory factory)
		{
			var multiDaysSelection = new RoutingMultiDaysSelection(requestedDate, routingResponseHeaders, factory);

			multiDaysSelection.Initialize(includeWeeklyTimetable, importAndCreateMawb);

			return multiDaysSelection;
		}

		RoutingMultiDaysSelection(ZDateTime requestedDate, RoutingResponseHeaderCollection routingResponseHeaders, BusinessObjectFactory factory)
			: base(factory)
		{
			RecurrenceEnabled = true;
			RoutingResponseHeaders.AddRange(routingResponseHeaders);

			FromDateLimit = requestedDate;
			ToDateLimit = routingResponseHeaders.Cast<RoutingResponseHeader>().Max(x => x.DiscontinuedDate);

			if (ToDateLimit > FromDateLimit.AddYears(1))
			{
				ToDateLimit = FromDateLimit.AddYears(1);
			}
		}

		public void CreateVoyagesSailings(IEnumerable<ZDateTime> departureDates)
		{
			foreach (RoutingResponseHeader header in RoutingResponseHeaders)
			{
				var createdSailings = RoutingResponseHeader.TryCreateEnterpriseVoyagesSailings(header, departureDates, Factory).Cast<JobSailingCollection>();

				foreach (var collection in createdSailings)
				{
					foreach (JobSailing sailing in collection)
					{
						sailing.JX_OnlineScheduleStatus = Core.Constants.FlightScheduleStatus.Matched;
					}
				}

				sailingList.AddRange(createdSailings);
			}
		}

		#region Implementation

		protected override bool HasOperation(ZDateTime date)
		{
			return RoutingResponseHeaders.Cast<RoutingResponseHeader>().Any(x => x.HasOperation(date));
		}

		protected override bool IsWeekDayApplied(int dayNumber)
		{
			return RoutingResponseHeaders.Cast<RoutingResponseHeader>().Any(x => x.OperationDayHasNumber(dayNumber));
		}

		protected override int GetDayNumber(ZDateTime date)
		{
			return RoutingResponseHeader.GetDayNumber(date);
		}

		public override bool HasMultipleCarriers
		{
			get
			{
				var carriers = RoutingResponseHeaders.Cast<RoutingResponseHeader>()
					.Select(h => h.Carrier1)
					.Distinct();
				return carriers.Count() > 1;
			}
		}

		public override ZString FirstCarrier => RoutingResponseHeaders.Cast<RoutingResponseHeader>().FirstOrDefault()?.Carrier1 ?? ZString.Empty;

		protected override int GetRecurrenceNumberOfFlights()
		{
			int numberOfFlights = 0;
			var headers = RoutingResponseHeaders.Cast<RoutingResponseHeader>().ToArray();
			CalculateRecurrence((date) =>
			{
				numberOfFlights += headers.Count(x => x.HasOperation(date));
			});

			return numberOfFlights;
		}

		public RoutingResponseHeaderCollection RoutingResponseHeaders
		{
			get { return routingResponseHeaders ?? (routingResponseHeaders = new RoutingResponseHeaderCollection(Factory)); }
		}

		RoutingResponseHeaderCollection routingResponseHeaders;

		#endregion
	}
}
