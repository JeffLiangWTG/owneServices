using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Business.EventManagement;
using Enterprise.ZArchitecture.Business.EventManagement;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Business
{
	public class ForwardingContainerProcessHandlingInfo : CommonContainerProcessHandlingInfo
	{
		public ForwardingContainerProcessHandlingInfo(ForwardingContainer container)
			: base(container)
		{
			this.container = container;
		}
		readonly ForwardingContainer container;

		protected override IEnumerable<CascadingLink> PopulateCascadingTargets(IStmALog logBeingAdded)
		{
			var result = new List<CascadingLink>();
			var shipments = container.GetParentShipments();
			var shipmentsAndParentShipments = new Dictionary<ZGuid, CommonShipment>();

			foreach (var shipment in shipments)
			{
				AddShipmentAndParents(shipmentsAndParentShipments, shipment);
			}

			if (shipmentsAndParentShipments.Any())
			{
				result.AddRange(GetCascadingLinkList(JobShipmentSchema.Constants.Prefix, logBeingAdded.SL_SE_NKEvent, (shipmentPK) => shipmentsAndParentShipments[shipmentPK], shipmentsAndParentShipments.Keys.ToArray()));
			}

			if (container.ContainerParent is Enterprise.Integration.Customs.IBaseJobDeclaration declaration)
			{
				result.AddRange(GetCascadingLinkList(JobDeclarationSchema.Constants.Prefix, logBeingAdded.SL_SE_NKEvent, (declarationPK) => declaration, declaration.PK));
			}

			return result;
		}

		List<CascadingLink> GetCascadingLinkList(ZString parentTablePrefix, ZString eventCode, Func<ZGuid, IBusiness> findParentBizObjFunc, params ZGuid[] parentPKs)
		{
			var result = new List<CascadingLink>();

			if (!parentTablePrefix.IsEmpty && findParentBizObjFunc != null)
			{
				var query = new ZQuery(ProcessTasksSchema.P9_ParentID, parentPKs);
				query.AddToFilter(ProcessTasksSchema.P9_ParentTableCode, parentTablePrefix);
				query.AddToFilter(ProcessTasksSchema.P9_SE_NKMilestoneEvent, eventCode);
				query.AddToFilter(ProcessTasksSchema.P9_RespondToCascadedEvents, ZBool.True);

				var groupedProcessTasks = container.Factory.Load<ProcessTask>(query).GroupBy(p => p.P9_ParentID);
				foreach (var group in groupedProcessTasks)
				{
					var parentBizObj = findParentBizObjFunc(group.Key);
					if (parentBizObj is IStmALogParent parent)
					{
						var target = new CascadingLink
						{
							Parent = parent,
							Triggers = group.ToArray()
						};

						result.Add(target);
					}
				}
			}

			return result;
		}

		void AddShipmentAndParents(Dictionary<ZGuid, CommonShipment> dict, CommonShipment shipment)
		{
			if (!dict.ContainsKey(shipment.PK))
			{
				dict.Add(shipment.PK, shipment);
			}

			if (shipment.CoLoadMasterShipment != null)
			{
				AddShipmentAndParents(dict, shipment.CoLoadMasterShipment);
			}
		}

		protected override IEnumerable<PropagationLink> PopulatePropagationTargets()
		{
			if (container.IsDeleted)
			{
				yield break;
			}

			if (container.Consol != null)
			{
				var query = new ZQuery(JobContainerSchema.JC_JK, container.JC_JK);
				var siblings = container.Factory.Load<ForwardingContainer>(query);

				yield return new PropagationLink(container.Consol, siblings, "Consol Containers");

				var shipments = container.PackLines
					.Cast<PackLine>()
					.Where(line => line.Shipment != null)
					.Select(line => line.Shipment)
					.Distinct(BusinessObjectEqualityComparer<CommonShipment>.PKOnlyComparer);

				foreach (var shipment in shipments)
				{
					yield return new PropagationLink(shipment, shipment.Containers, "Shipment Containers");
				}
			}

			var link = GetDeclarationPropagationLink();

			if (link != null)
			{
				yield return link;
			}
		}

		protected override IEnumerable<string> PopulateEventParametersToMatchDuringPropagation(ZString eventCode)
		{
			switch (eventCode)
			{
				case Events.GateInCode:
				case Events.GateOutCode:
					return new string[] { CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Facility };

				default:
					return base.PopulateEventParametersToMatchDuringPropagation(eventCode);
			}
		}
	}
}
