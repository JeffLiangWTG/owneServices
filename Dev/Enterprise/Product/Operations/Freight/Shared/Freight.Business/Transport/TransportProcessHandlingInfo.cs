using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Business.EventManagement;
using Enterprise.ZArchitecture.Business.EventManagement;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business
{
	public class TransportProcessHandlingInfo : ProcessHandlingInfo
	{
		internal TransportProcessHandlingInfo(Transport transport)
			: base(transport)
		{
			this.transport = transport;
		}
		readonly Transport transport;

		protected override bool ParentStateAllowsCascading
		{
			get { return true; } // Is normally logParent.IsInDatabase but we use Business Objects in this one case as we only cascade one level.
		}

		protected override IEnumerable<CascadingLink> PopulateCascadingTargets(IStmALog logBeingAdded)
		{
			if (transport.ParentType == null)
			{
				yield break;
			}

			var parent = transport.Parent as BusinessObject;
			if (parent != null)
			{
				var cascadeToParent = transport.Parent.TypeCode == Constants.TransportParentTypes.Declaration;

				var cascadeToParentChildren = transport.Parent.TypeCode == Constants.TransportParentTypes.Shipment
											|| transport.Parent.TypeCode == Constants.TransportParentTypes.Consol
											|| transport.Parent.TypeCode == Constants.TransportParentTypes.Declaration
											|| transport.Parent.TypeCode == Constants.TransportParentTypes.AgencyShipment
											|| transport.Parent.TypeCode == Constants.TransportParentTypes.ShipmentPreAdvice;

				if (cascadeToParent)
				{
					var query = new ZQuery(ProcessTasksSchema.P9_ParentID, parent.PK);
					query.AddToFilter(ProcessTasksSchema.P9_SE_NKMilestoneEvent, logBeingAdded.SL_SE_NKEvent);
					query.AddToFilter(ProcessTasksSchema.P9_ParentTableCode, parent.TablePrefix);
					query.AddToFilter(ProcessTasksSchema.P9_RespondToCascadedEvents, ZBool.True);
					var processTasks = transport.Factory.Load<ProcessTask>(query);

					if (processTasks.Length > 0)
					{
						yield return new CascadingLink
						{
							Parent = (IStmALogParent)parent,
							Triggers = processTasks
						};
					}
				}

				if (cascadeToParentChildren)
				{
					var provider = parent as IProcessHandlingInfoProvider;
					if (provider != null)
					{
						var handlingInfo = provider.ProcessHandlingInfo;
						if (handlingInfo != null)
						{
							var targets = handlingInfo.GetCascadingTargets(logBeingAdded);
							if (targets != null)
							{
								foreach (var target in targets)
								{
									yield return target;
								}
							}
						}
					}
				}
			}
		}
	}
}
