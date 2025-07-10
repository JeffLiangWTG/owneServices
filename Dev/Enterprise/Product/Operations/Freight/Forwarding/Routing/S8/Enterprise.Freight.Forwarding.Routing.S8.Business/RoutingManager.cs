using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;

namespace Enterprise.Freight.Forwarding.Routing.S8.Business
{
	public class RoutingManager : NonPersistentBusinessObject, IObsoleteValidation
	{
		public RoutingManager(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#region Properties

		#region Include Weekly Timetable

		public ZBool IncludeWeeklyTimetable
		{
			get => RegistryIncludeWeeklyTimetable;
			set => RegistryIncludeWeeklyTimetable = value;
		}

		ZBool RegistryIncludeWeeklyTimetable
		{
			get { return (Env.Registry.GetFilterCriteria(IncludeWeeklyTimetable_Name) != ZBool.False.ToString()); }
			set
			{
				if (RegistryIncludeWeeklyTimetable != value)
				{
					Env.Registry.SetFilterCriteria(IncludeWeeklyTimetable_Name, value.ToString());
				}
			}
		}

		public ZBool ExcludeWeeklyTimetableForBinding
		{
			get => !RegistryIncludeWeeklyTimetable;
			set => RegistryIncludeWeeklyTimetable = !value;
		}

		public ZDateTime DepartureDate
		{
			get => Requests?.FirstOrDefault()?.DepartureDate ?? ZDateTime.Empty;
		}

		const string IncludeWeeklyTimetable_Name = "GlobalFlightSchedule_IncludeWeeklyTimeTable";

		#endregion

		#endregion

		#region Routings

		public IReadOnlyCollection<RoutingRequest> Requests { get; set; }

		public RoutingResponseHeaderCollection Routings
		{
			get
			{
				if (fRoutings == null)
				{
					fRoutings = new RoutingResponseHeaderCollection(Factory);
				}

				return fRoutings;
			}
		}

		RoutingResponseHeaderCollection fRoutings;

		public RoutingManager CloneSelectedSchedules(RoutingResponseHeader[] selectedHeaders)
		{
			Argument.NotNull(selectedHeaders, "selectedHeaders");

			var newFactory = new BusinessObjectFactory();
			var routineManager = new RoutingManager(newFactory);
			routineManager.Routings.AddRange(selectedHeaders.Select(x => x.Clone(newFactory)));

			return routineManager;
		}

		public void TryCreateEnterpriseVoyages(IEnumerable<ZDateTime> departureDates)
		{
			if (HasErrors)
			{
				return;
			}

			foreach (RoutingResponseHeader header in Routings)
			{
				RoutingResponseHeader.TryCreateEnterpriseVoyagesSailings(header, departureDates, header.Factory);
				RouteCreationLogs.AddRange(header.RouteCreationLogs);
			}
		}

		public List<string> RouteCreationLogs { get; } = new List<string>();

		#endregion
	}
}
