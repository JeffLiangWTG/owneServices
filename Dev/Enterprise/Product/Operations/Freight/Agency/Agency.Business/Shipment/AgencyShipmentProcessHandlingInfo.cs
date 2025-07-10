
namespace Enterprise.Freight.Agency.Business
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using CargoWise.EntityFramework;
	using CargoWise.Types;
	using Enterprise.Integration;
	using Enterprise.MasterFiles.Business;
	using Enterprise.ZArchitecture.Business.Business.EventManagement;
	using Enterprise.ZArchitecture.Business.EventManagement;
	using Enterprise.ZArchitecture.Schema;

	public class AgencyShipmentProcessHandlingInfo : ProcessHandlingInfo
	{
		internal AgencyShipmentProcessHandlingInfo(AgencyShipment shipment)
			: base(shipment)
		{
		}

		protected AgencyShipment Shipment
		{
			get
			{
				return (AgencyShipment)LogParent;
			}
		}

		protected override IEnumerable<CascadingLink> PopulateCascadingTargets(IStmALog logBeingAdded)
		{
			var result = new List<CascadingLink>();
			var containers = Shipment.ShippingContainers.Cast<AgencyShipmentContainer>()
				.Where(ShouldRespondToCascadedEvent(logBeingAdded.SL_SE_NKEvent))
				.ToList();

			if (containers.Any())
			{
				var query = new ZQuery(ProcessTasksSchema.P9_ParentID, containers.Select(c => c.PK));
				query.AddToFilter(ProcessTasksSchema.P9_ParentTableCode, containers[0].TablePrefix);
				query.AddToFilter(ProcessTasksSchema.P9_SE_NKMilestoneEvent, logBeingAdded.SL_SE_NKEvent);
				query.AddToFilter(ProcessTasksSchema.P9_RespondToCascadedEvents, ZBool.True);

				var grouppedProcessTasks = Shipment.Factory.Load<ProcessTask>(query).GroupBy(p => p.P9_ParentID);
				foreach (var group in grouppedProcessTasks)
				{
					var container = containers.First(c => c.PK == group.Key);
					var target = new CascadingLink
					{
						Parent = container,
						Triggers = group.ToArray()
					};

					result.Add(target);
					result.AddRange(GetChildTargets(container, logBeingAdded));
				}
			}

			return result;
		}

		static IEnumerable<CascadingLink> GetChildTargets(BusinessObject parent, IStmALog logBeingAdded)
		{
			var result = new List<CascadingLink>();

			var processHandlingInfoProvider = parent as IProcessHandlingInfoProvider;
			if (processHandlingInfoProvider != null)
			{
				var handlingInfo = processHandlingInfoProvider.ProcessHandlingInfo;
				if (handlingInfo != null)
				{
					var targets = handlingInfo.GetCascadingTargets(logBeingAdded);
					if (targets != null && targets.Any())
					{
						result.AddRange(targets);

						foreach (var target in targets)
						{
							result.AddRange(GetChildTargets((BusinessObject)target.Parent, logBeingAdded));
						}
					}
				}
			}

			return result;
		}

		static Func<AgencyShipmentContainer, bool> ShouldRespondToCascadedEvent(string eventCode)
		{
			var movementLinkedEvents = AgencyShipmentContainerMovementEventSynchroniser.MovementTypeCodeEventMap.SelectMany(p => p.Value).ToArray();

			return (AgencyShipmentContainer container) => !container.IsContainerised || !movementLinkedEvents.Contains(eventCode);
		}
	}
}
