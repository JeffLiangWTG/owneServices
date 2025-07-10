using System.Collections.Generic;
using CargoWise.Definitions.Ecommerce;
using CargoWise.EntityFramework;
using Enterprise.eTail.Integration;
using Enterprise.Freight.Business.Extensions;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Schema;
using EventReferenceParameterTypes = Enterprise.Core.Constants.EventReferenceParameterTypes;

namespace Enterprise.eTail.Business
{
	public class HVLVShipmentConsignmentCollection : DependentBusinessObjectCollection<HVLVConsignment, HVLVConsignmentHeader>,
		IHVLVConsignmentCollection,
		IHVLVConsignmentCollectionForDocument
	{
		public HVLVShipmentConsignmentCollection(ForwardingShipment shipment)
			: this(shipment.GetOrCreateHVLVConsignmentHeader())
		{
		}

		public HVLVShipmentConsignmentCollection(HVLVConsignmentHeader header)
			: base(header)
		{
			shipment = header.Shipment;
		}

		readonly ForwardingShipment shipment;

		#region Related Business Objects
		IHVLVConsignment IHVLVConsignmentCollection.this[int i] => (IHVLVConsignment)Elements[i];
		IHVLVConsignmentForDocument IHVLVConsignmentCollectionForDocument.this[int i] => (IHVLVConsignmentForDocument)Elements[i];

		public HVLVConsignmentHeader Header => Master;

		protected override string FkColumnName => HVLVConsignmentSchema.HVC_HCH_Header.Name;

		#region Add and Remove

		protected override void SetDefaultsForNewChild(BusinessObject businessObject)
		{
			base.SetDefaultsForNewChild(businessObject);

			var consignment = (HVLVConsignment)businessObject;
			consignment.HVC_HCH_Header = Header.PK;
			consignment.ManagingShipment = shipment;
		}

		protected override void SetCollectionRelationships(BusinessObject dependent)
		{
			var consignment = dependent as HVLVConsignment;
			if (consignment != null && (consignment.HVC_JS_ManifestedOnShipment.IsEmpty || consignment.HVC_Status == HVLVConsignmentStatus.Codes.Detached))
			{
				base.SetCollectionRelationships(dependent);
			}
		}

		protected override void OnAdded(BusinessObject bizOAdded)
		{
			base.OnAdded(bizOAdded);
			var consignment = bizOAdded as HVLVConsignment;
			if (consignment.HVC_Status == HVLVConsignmentStatus.Codes.Detached)
			{
				consignment.HVC_IsActive = true;
				consignment.HVC_Status = HVLVConsignmentStatus.Codes.Booked;
				var parameters = new Dictionary<string, string>()
				{
					[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type] = EventReferenceParameterTypes.Shipment
				};

				consignment.Logs.AddATCEvent(consignment.IsInDatabase, shipment.JS_UniqueConsignRef, parameters);
			}
		}

		protected override void RemoveCollectionRelationshipsCore(BusinessObject child, bool forDelete)
		{
			var consignment = child as HVLVConsignment;
			consignment.HVC_IsActive = false;
			consignment.HVC_Status = HVLVConsignmentStatus.Codes.Detached;
			var parameters = new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type, EventReferenceParameterTypes.Shipment);
			consignment.Logs.AddDTCEvent(consignment.IsInDatabase, shipment.JS_UniqueConsignRef, parameters);
		}

		#endregion

		#endregion

		#region Load

		protected override ZQuery CreateAdditionalFilter()
		{
			var query = base.CreateAdditionalFilter();
			query.AddToFilter(HVLVConsignmentSchema.HVC_Status, SQLComparisonOperator.NotEqual, HVLVConsignmentStatus.Codes.Detached);
			query.IgnoreActiveFilter = true;
			return query;
		}

		public override void Load()
		{
			base.Load();
			Header.ConsignmentIDCache.Clear();
			foreach (HVLVConsignment consignment in this)
			{
				Header.ConsignmentIDCache.Add(consignment.HVC_ConsignmentId);
			}

			LoadConsignmentsHaveItemsLoadedOnThisShipment();
		}

		void LoadConsignmentsHaveItemsLoadedOnThisShipment()
		{
			var consignmentWithItemLoadedOnShipmentSubQuery = new ZDBOnlySubQuery(typeof(HVLVItem), HVLVItemSchema.HVI_HVC_Consignment);
			consignmentWithItemLoadedOnShipmentSubQuery.AddToFilter(HVLVItemSchema.HVI_JS_LoadedOnShipment, shipment.PK);
			consignmentWithItemLoadedOnShipmentSubQuery.AddToFilter(HVLVItemSchema.HVI_ClusterKey, SQLComparisonOperator.NotEqual, Header.HCH_ClusterKey);

			var query = new ZDBOnlyQuery(typeof(HVLVConsignment));
			query.AddSubQuery(consignmentWithItemLoadedOnShipmentSubQuery, JoinCondition.And);
			query.AddToFilter(HVLVConsignmentSchema.HVC_IsActive, true);
			query.AddToFilter(HVLVConsignmentSchema.HVC_ClusterKey, SQLComparisonOperator.NotEqual, Header.HCH_ClusterKey);

			AddRange(Factory.Load<HVLVConsignment>(query));
		}

		#endregion

		protected override ZQuery CreateRelationshipFilter()
		{
			var baseQuery = base.CreateRelationshipFilter();
			if (!Header.HCH_ClusterKey.IsEmpty)
			{
				return baseQuery.AddToFilter(HVLVConsignmentSchema.HVC_ClusterKey, Master.HCH_ClusterKey);
			}

			return baseQuery;
		}
	}
}
