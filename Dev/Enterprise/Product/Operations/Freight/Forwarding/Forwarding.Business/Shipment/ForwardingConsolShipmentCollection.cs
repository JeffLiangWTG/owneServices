using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Business
{
	public class ForwardingConsolShipmentCollection : ConsolForwardingShipmentCollection
	{
		public ForwardingConsolShipmentCollection(ForwardingConsol parent) : base(parent)
		{
			this.parent = parent;
		}

		readonly ForwardingConsol parent;

		public new ForwardingShipment this[int index]
		{
			get { return (ForwardingShipment)Elements[index]; }
		}

		public new ForwardingShipment AddNew()
		{
			return base.AddNew();
		}

		protected override void OnAdded(BusinessObject bizOAdded)
		{
			var shipment = bizOAdded as ForwardingShipment;
			if (shipment != null)
			{
				HookShipmentForConsolDensity(shipment);
			}

			base.OnAdded(bizOAdded);

			if (shipment != null && (!IsLoading || !shipment.IsInDatabase) && !IsUpdatingByDataRefreshBus)
			{
				shipment.SynchronizeShipmentGatewayFromConsols(parent, ZGuid.Empty, isDetachedConsol: false);
			}
		}

		protected override void OnRemoved(BusinessObject bizO)
		{
			if (bizO is ForwardingShipment shipment)
			{
				UnHookShipmentForConsolDensity(shipment);

				if (!IsUpdatingByDataRefreshBus && !shipment.IsDeleting)
				{
					shipment.SynchronizeShipmentGatewayFromConsols(parent, ZGuid.Empty, isDetachedConsol: true);
				}
			}

			base.OnRemoved(bizO);
		}

		void HookShipmentForConsolDensity(ForwardingShipment shipment)
		{
			shipment.JS_Calc_ActualVolumeWeightInfo.ValueChanged += JS_Calc_ActualVolumeWeightChanged;
		}

		void UnHookShipmentForConsolDensity(ForwardingShipment shipment)
		{
			shipment.JS_Calc_ActualVolumeWeightInfo.ValueChanged -= JS_Calc_ActualVolumeWeightChanged;
		}

		void JS_Calc_ActualVolumeWeightChanged(object sender, EventArgs e)
		{
			parent.Density.RefreshAllValues();
			parent.JK_Calc_CostFreePercentageInfo.RefreshBinding();
		}

		public override Type GetTypeOfElementsFromPK(ZGuid pK)
		{
			return typeof(ForwardingShipment);
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			if (parent == null || parent.JK_AgentType == Constants.AgentType.AWBMaster)
			{
				return ZQuery.NoResultQuery;
			}

			var result = base.CreateRelationshipFilter();

			if (!result.IsEmpty)
			{
				result.AddToFilter(JobShipmentSchema.JS_IsForwardRegistered, ZBool.True);
			}

			return result;
		}
	}
}
