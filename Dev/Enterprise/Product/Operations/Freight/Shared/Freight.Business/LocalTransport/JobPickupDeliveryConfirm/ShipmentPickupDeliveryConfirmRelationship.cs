using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business
{
	public sealed class ShipmentPickupDeliveryConfirmRelationship : ICollectionRelationship
	{
		public ShipmentPickupDeliveryConfirmRelationship(CommonShipment shipment, string pickupDeliveryType)
		{
			this.pickupDeliveryType = pickupDeliveryType;
			this.shipment = shipment;
			this.additionalFilter = new ZQuery();
			Hook();
		}

		public override bool Equals(object obj)
		{
			var relationship = obj as ShipmentPickupDeliveryConfirmRelationship;
			return relationship != null
				&& relationship.shipment == shipment
				&& relationship.pickupDeliveryType == pickupDeliveryType;
		}

		public override int GetHashCode()
		{
			return this.shipment.GetHashCode() ^ this.pickupDeliveryType.GetHashCode();
		}

		#region Events

		void Hook()
		{
			shipment.OuterPackLines.CountChanged += new CollectionCountChangedEventHandler(OuterPackLines_CountChanged);
		}

		void OuterPackLines_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			if (!rebuilding)
			{
				OnRefreshed();
			}
		}

		#endregion

		bool rebuilding;

		ZQuery BuildRelationshipFilter(bool ignoreActiveFilter, bool checkPickupDeliveryType)
		{
			try
			{
				rebuilding = true;

				var result = new ZQuery();

				var shipmentPks = new[] { shipment.PK };

				if (!shipment.IsDeleted && shipment.IsMasterShipmentRepresentingAllChildShipments)
				{
					shipmentPks = shipmentPks.Concat(shipment.GetPksFromAllSubShipmentsWithoutChildren()).ToArray();
				}

				result.AddToFilter(JobPickupDeliveryConfirmSchema.EU_JS, shipmentPks);

				if (checkPickupDeliveryType)
				{
					result.AddToFilter(JobPickupDeliveryConfirmSchema.EU_PickupDeliveryType, pickupDeliveryType);
				}

				result.IgnoreActiveFilter = ignoreActiveFilter;
				result.FetchOnlyFromLocalCache = !shipment.IsInDatabase;
				result.ModificationsEnabled = false;

				return result;
			}
			finally
			{
				rebuilding = false;
			}
		}

		void OnRefreshed()
		{
			if (Refreshed != null)
			{
				Refreshed(this, EventArgs.Empty);
			}
		}

		#region ICollectionRelationship Members

		public ZQuery RelationshipFilter
		{
			get { return BuildRelationshipFilter(false, true); }
		}

		event EventHandler ICollectionRelationship.RelationshipFilterChanged
		{
			add { Refreshed += value; }
			remove { Refreshed -= value; }
		}

		public ICollectionRelationship AddFilter(ZQuery additionalFilter)
		{
			ShipmentPickupDeliveryConfirmRelationship result = (ShipmentPickupDeliveryConfirmRelationship)MemberwiseClone();

			if (additionalFilter != null)
			{
				result.additionalFilter = new ZQuery(this.additionalFilter, additionalFilter);
				result.additionalFilter.ModificationsEnabled = false;
			}

			return result;
		}

		public bool MatchesRelationshipFilter(BusinessObject businessObject, bool ignoreActiveFilter, bool fetchOnlyFromLocalCache)
		{
			if (businessObject == null)
			{
				throw new ArgumentNullException(nameof(businessObject));
			}

			return businessObject.MatchesFilter(BuildRelationshipFilter(ignoreActiveFilter, true));
		}

		public BusinessObject[] LoadBusinessObjects(BusinessObjectFactory factory, ZQuery filter)
		{
			var results = factory.Load<CommonPickupDeliveryConfirm>(new ZQuery(filter, BuildRelationshipFilter(false, false)));

			foreach (var confirm in results)
			{
				factory.AddFetchHint(JobTransportLegPackLineDivotSchema.J8_EU_PickupDeliverConfirm, confirm.PK);
			}

			return results.Where(x => x.EU_PickupDeliveryType == pickupDeliveryType).ToArray();
		}

		public event EventHandler Refreshed;

		public BusinessObject Master
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return shipment; }
		}

		public bool HasChangesIncludingRelationship(BusinessObject businessObject)
		{
			if (businessObject == null)
			{
				throw new ArgumentNullException(nameof(businessObject));
			}

			CommonPickupDeliveryConfirm pickupDelivery = (CommonPickupDeliveryConfirm)businessObject;

			if (pickupDelivery.HasChanges)
			{
				return true;
			}

			foreach (CommonConfirmDivot divot in pickupDelivery.Divots)
			{
				if (divot.HasChanges)
				{
					return true;
				}
			}

			foreach (CommonConfirmDivot divot in pickupDelivery.Divots)
			{
				if (divot.PackLine != null && divot.PackLine.HasChanges)
				{
					return true;
				}
			}

			return false;
		}

		public void ClearHasChangesIncludingRelationship(BusinessObject businessObject)
		{
			IBusinessObjectState businessObjectState = businessObject;
			businessObjectState.ClearHasChangesIncludingChildren();
		}

		public bool SupportsAddToRelationship()
		{
			return shipment.OuterPackLines.Count > 0 && ConfirmTimesSyncHelper.IsValidShipmentForAddingConfirmations(shipment);
		}

		public void AddToRelationship(BusinessObject businessObject)
		{
			if (businessObject == null)
			{
				throw new ArgumentNullException(nameof(businessObject));
			}

			var pickupDelivery = (CommonPickupDeliveryConfirm)businessObject;
			using (pickupDelivery.SuspendSettingHasChanges())
			{
				pickupDelivery.EU_PickupDeliveryType = pickupDeliveryType;

				if (!pickupDelivery.IsInDatabase && pickupDelivery.EU_JS.IsEmpty)
				{
					pickupDelivery.EU_JS = shipment.PK;
				}
			}

			var parentDropMode = GetParentDropMode(pickupDelivery);
			if (pickupDelivery.EU_DropMode.IsEmpty && !parentDropMode.IsEmpty)
			{
				pickupDelivery.EU_DropMode = parentDropMode;
			}

			OnRefreshed();
		}

		ZString GetParentDropMode(CommonPickupDeliveryConfirm pickupDelivery)
		{
			if (pickupDelivery.FirstShipment == null)
			{
				return "";
			}

			var firstShipment = pickupDelivery.FirstShipment;

			switch (pickupDelivery.EU_PickupDeliveryType)
			{
				case Constants.PickupDeliveryConfirmTypes.OriginPickup:
					return firstShipment.DocsAndCartage.JP_FCLPickupEquipmentNeeded;
				case Constants.PickupDeliveryConfirmTypes.DestinationDelivery:
					return firstShipment.DocsAndCartage.JP_FCLDeliveryEquipmentNeeded;
				default:
					return "";
			}
		}

		public void RemoveFromRelationship(BusinessObject businessObject)
		{
			if (businessObject == null)
			{
				throw new ArgumentNullException(nameof(businessObject));
			}

			CommonPickupDeliveryConfirm pickupDelivery = (CommonPickupDeliveryConfirm)businessObject;
			pickupDelivery.Divots.DeleteAll();
			OnRefreshed();
		}

		void ICollectionRelationship.Clear()
		{
			throw new NotSupportedException();
		}

		#endregion

		ZQuery additionalFilter;
		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		readonly CommonShipment shipment;
		readonly string pickupDeliveryType;
	}
}
