using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Business.EventManagement;
using Enterprise.ZArchitecture.Business.EventManagement;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Yard.Business
{
	public class CYDTransportationUnitProcessHandlingInfoProvider : ProcessHandlingInfo
	{
		public CYDTransportationUnitProcessHandlingInfoProvider(CYDTransportationUnit transportationUnit)
			: base(transportationUnit)
		{
			this.transportationUnit = Argument.NotNull(transportationUnit, "CYDTransportationUnit");
		}

		readonly CYDTransportationUnit transportationUnit;

		protected override IEnumerable<CascadingLink> PopulateCascadingTargets(IStmALog logBeingAdded)
		{
			if (logBeingAdded.SL_SE_NKEvent == Events.GateIn.Code)
			{
				return GetCascadingLinks(transportationUnit.ReceiveYardUnits, logBeingAdded);
			}
			else if (logBeingAdded.SL_SE_NKEvent == Events.GateOut.Code)
			{
				return GetCascadingLinks(transportationUnit.ReleaseYardUnits, logBeingAdded);
			}

			return Enumerable.Empty<CascadingLink>();
		}

		IEnumerable<CascadingLink> GetCascadingLinks(IEnumerable<CYDYardUnitState> yardUnits, IStmALog logBeingAdded)
		{
			if (yardUnits.Any())
			{
				var processTasks = GetProcessTasks(yardUnits, logBeingAdded);
				var links = yardUnits.Select(yardUnit => new CascadingLink
				{
					Parent = yardUnit,
					Triggers = processTasks.Where(processTask => processTask.P9_ParentID == yardUnit.PK).ToArray()
				});

				return links;
			}
			return Enumerable.Empty<CascadingLink>();
		}

		IEnumerable<ProcessTask> GetProcessTasks(IEnumerable<CYDYardUnitState> yardUnits, IStmALog logBeingAdded)
		{
			var query = new ZQuery(ProcessTasksSchema.P9_ParentID, yardUnits.Select(d => d.PK));
			query.AddToFilter(ProcessTasksSchema.P9_SE_NKMilestoneEvent, logBeingAdded.SL_SE_NKEvent);
			query.AddToFilter(ProcessTasksSchema.P9_ParentTableCode, CYDYardUnitStateSchema.Constants.Prefix);
			query.AddToFilter(ProcessTasksSchema.P9_RespondToCascadedEvents, true);
			return transportationUnit.Factory.Load<ProcessTask>(query);
		}
	}
}
