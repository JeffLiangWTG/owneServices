using CargoWise.EntityFramework;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.Business
{
	public class ConsolForwardingShipmentCollection : ConsolShipmentCollection
	{
		public ConsolForwardingShipmentCollection(ForwardingConsol associatedObject)
			: base(associatedObject)
		{
		}

		protected new ForwardingConsol ParentConsol
		{
			get { return (ForwardingConsol)base.ParentConsol; }
		}

		public new ForwardingShipment this[int index]
		{
			get { return (ForwardingShipment)Elements[index]; }
		}

		public virtual new ForwardingShipment AddNew()
		{
			return (ForwardingShipment)base.AddNew();
		}

		#region Add

		protected override void OnAdded(BusinessObject bizOAdded)
		{
			base.OnAdded(bizOAdded);

			if (ParentConsol != null && !IsLoading && !IsUpdatingByDataRefreshBus)
			{
				ScreeningStatusUpdater.SetShouldUpdateScreeningStatusWhenPartAddToBizO(ParentConsol, ParentConsol, bizOAdded);
			}
		}

		#endregion

		#region Remove

		protected override void OnRemoved(BusinessObject bizORemoved)
		{
			base.OnRemoved(bizORemoved);
			if (!IsUpdatingByDataRefreshBus && ParentConsol != null)
			{
				ScreeningStatusUpdater.SetShouldUpdateScreeningStatusWhenPartRemovedFromBizO(ParentConsol, ParentConsol);
			}
		}

		#endregion

		#region Default Values

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);

			ForwardingShipment childShipment = child as ForwardingShipment;

			if (childShipment != null && ParentConsol != null)
			{
				if (!ParentConsol.JK_ReleaseType.IsEmpty && ParentConsol.JK_AgentType == Core.Constants.AgentType.Direct)
				{
					childShipment.JS_ReleaseType = ParentConsol.JK_ReleaseType;
				}
			}
		}

		#endregion
	}
}
