using CargoWise.EntityFramework;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.ComponentModel;

namespace Enterprise.Freight.Forwarding.Business
{
	/// <summary>
	/// Summary description for ForwardingConsolManyToManyCollection.
	/// </summary>
	[ModuleID(Enterprise.ZArchitecture.Modules.ModuleId.JobConsol)]
	public class ForwardingConsolManyToManyCollection : ConsolCollection
	{
		public ForwardingConsolManyToManyCollection(ForwardingShipment shipment) : base(shipment)
		{
		}

		public new ForwardingShipment ParentShipment
		{
			get { return (ForwardingShipment)base.ParentShipment; }
		}

		public new ForwardingConsol this[int index]
		{
			get
			{
				return (ForwardingConsol)Elements[index];
			}
		}

		public new ForwardingConsol AddNew()
		{
			return (ForwardingConsol)base.AddNew();
		}

		public new ForwardingConsol GetEarliestConsol()
		{
			return base.GetEarliestConsol() as ForwardingConsol;
		}

		public new ForwardingConsol GetLatestConsol()
		{
			return base.GetLatestConsol() as ForwardingConsol;
		}

		protected override void OnAdded(BusinessObject bizOAdded)
		{
			var consol = (ForwardingConsol)bizOAdded;

			base.OnAdded(consol);
			if (!IsLoading && !IsUpdatingByDataRefreshBus)
			{
				var shouldUpdateScreeningStatus = consol as IShouldUpdateScreeningStatus;
				if (shouldUpdateScreeningStatus != null)
				{
					ScreeningStatusUpdater.SetShouldUpdateScreeningStatusWhenPartAddToBizO(shouldUpdateScreeningStatus, consol, ParentShipment);
				}
				if (ParentShipment.AviationSecurity.SupplyChainSecurityConfiguration.IsRecalculationNeeded(ParentShipment.JS_InspectionTypeCode))
				{
					ParentShipment.SetApprovedShipperStatus(Res.GetString("8ff019f4-6fcc-4563-a9de-a37d3b33e352", "{0} has been added", consol.HumanReadableName));
					ParentShipment.ReDefaultPackLineInspectionTypeCodes();
				}
			}
		}

		protected override void OnRemoved(BusinessObject bizORemoved)
		{
			base.OnRemoved(bizORemoved);
			if (!IsUpdatingByDataRefreshBus)
			{
				var shouldUpdateScreeningStatus = bizORemoved as IShouldUpdateScreeningStatus;
				if (shouldUpdateScreeningStatus != null)
				{
					ScreeningStatusUpdater.SetShouldUpdateScreeningStatusWhenPartRemovedFromBizO(shouldUpdateScreeningStatus, bizORemoved);
				}
				if (ParentShipment.AviationSecurity.SupplyChainSecurityConfiguration.IsRecalculationNeeded(ParentShipment.JS_InspectionTypeCode))
				{
					ParentShipment.SetApprovedShipperStatus(Res.GetString("9953ffff-9202-436c-bb96-cd24f6984f69", "{0} has been removed", bizORemoved.HumanReadableName));
					ParentShipment.ReDefaultPackLineInspectionTypeCodes();
				}
			}
		}
	}
}
