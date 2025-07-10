using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Order = Enterprise.Freight.Forwarding.Orders.Business.Order;
using OrderLine = Enterprise.Freight.Forwarding.Orders.Business.OrderLine;
using UniversalOrderLine = Enterprise.UniversalDataBuss.DataObjects.Universal.OrderLine;
using UniversalPackingLine = Enterprise.UniversalDataBuss.DataObjects.Universal.PackingLine;

namespace Enterprise.Freight.Forwarding.Business
{
	public class LineRelatedDataWriterHelper
	{
		public LineRelatedDataWriterHelper(ForwardingShipment shipment)
		{
			if (!AdvOrmFeatureHelper.IsEnabled)
			{
				return;
			}
			var collection = GetCollection(shipment, JobShipmentSchema.Constants.PK);
			if (collection.IsNullOrEmpty())
			{
				return;
			}
			BuildBasicMap(collection);
			BuildOrderAndLineKeyMap(collection, shipment.Factory, true);
		}

		public LineRelatedDataWriterHelper(JobSupplierBooking supplierBooking)
		{
			if (!AdvOrmFeatureHelper.IsEnabled)
			{
				return;
			}
			var collection = GetCollection(supplierBooking, JobSupplierBookingSchema.Constants.PK);
			if (collection.IsNullOrEmpty())
			{
				return;
			}
			BuildBasicMap(collection);
			BuildOrderAndLineKeyMap(collection, supplierBooking.Factory, false);
		}

		public LineRelatedDataWriterHelper(Order order)
		{
			if (!AdvOrmFeatureHelper.IsEnabled)
			{
				return;
			}
			var collection = GetCollection(order, JobOrderHeaderSchema.Constants.PK);
			if (collection.IsNullOrEmpty())
			{
				return;
			}
			BuildBasicMap(collection);
		}

		void BuildBasicMap(DynamicBusinessObjectCollection collection)
		{
			LoadListLineToKeyAndPackingLineMap = CreateMapFromCollection(collection, ContainerLoadListLineSchema.Constants.PK, JobPackLinesSchema.Constants.PK, JobContainerSchema.Constants.JC_ContainerNum, ContainerLoadListHeaderSchema.Constants.CLH_LoadListId, ContainerLoadListHeaderSchema.Constants.CLH_Status);

			LoadListLineToKeyAndBookingLineMap = CreateMapFromCollection(collection, ContainerLoadListLineSchema.Constants.PK, JobSupplierBookingLineSchema.Constants.PK, JobContainerSchema.Constants.JC_ContainerNum, ContainerLoadListHeaderSchema.Constants.CLH_LoadListId, ContainerLoadListHeaderSchema.Constants.CLH_Status);

			PackingLineToKeyAndLoadListLineMap = CreateMapFromCollection(collection, JobPackLinesSchema.Constants.PK, ContainerLoadListLineSchema.Constants.PK, JobPackLinesSchema.Constants.JL_PackLineId, JobShipmentSchema.Constants.JS_UniqueConsignRef, null);

			BookingLineToKeyAndOrderLineMap = CreateMapFromCollection(collection, JobSupplierBookingLineSchema.Constants.PK, JobOrderLineSchema.Constants.PK, JobSupplierBookingLineSchema.Constants.JSL_BookingLineId, JobSupplierBookingSchema.Constants.JSB_BookingId, JobSupplierBookingSchema.Constants.JSB_Status);
		}

		Dictionary<ZGuid, LineAndRelatedData> CreateMapFromCollection(DynamicBusinessObjectCollection collection, string selfPK, string parentPK, string selfKey, string headerKey, string selfStatus)
		{
			return collection.Where(data => !((ZGuid)data[selfPK]).IsEmpty).GroupBy(data => (ZGuid)data[selfPK]).ToDictionary(group => group.Key, grp =>
			{
				var data = grp.First();
				return new LineAndRelatedData(
					(ZGuid)data[parentPK],
					selfKey != null ? Convert.ToString(data[selfKey]) : null,
					selfKey != null ? Convert.ToString(data[headerKey]) : null,
					selfStatus != null ? Convert.ToString(data[selfStatus]) : null);
			});
		}

		void BuildOrderAndLineKeyMap(DynamicBusinessObjectCollection collection, BusinessObjectFactory factory, bool toBuildOrder)
		{
			OrderLineToKeyAndOrderMap = CreateMapFromCollection(collection, JobOrderLineSchema.Constants.PK, JobOrderHeaderSchema.Constants.PK, null, null, JobOrderHeaderSchema.Constants.JD_OrderStatus);
			var orderLines = factory.Load<OrderLine>(new ZQuery(JobOrderLineSchema.PK, OrderLineToKeyAndOrderMap.Keys));
			orderLines.ForEach(orderLine => OrderLineToKeyAndOrderMap[orderLine.PK].SelfKey = orderLine.GetUniversalDataContextManager().DataContextKey);

			if (toBuildOrder)
			{
				var orderPKs = orderLines.GroupBy(data => data.Order.PK).Select(data => data.Key);
				var orders = factory.Load<Order>(new ZQuery(JobOrderHeaderSchema.PK, orderPKs));

				var orderAndOrderKeyMap = orders.ToDictionary(order => order.PK, order => order.GetUniversalDataContextManager().DataContextKey);

				OrderLineToKeyAndOrderMap.ForEach(orderLineMap => orderLineMap.Value.HeaderKey = orderAndOrderKeyMap[orderLineMap.Value.RelatedPK]);
			}
		}

		Entity GetRelatedEntityFromLoadListLine(IDataObjectWriterStrategy writeManagerWriterStrategy, ZGuid loadListLinePK)
		{
			var loadListLineEntity = new Entity(writeManagerWriterStrategy);
			loadListLineEntity.SetEntityKeyCollection(() => new List<EntityKey>
			{
				new EntityKey { Type = "ContainerLoadList", Key = LoadListLineToKeyAndPackingLineMap[loadListLinePK].HeaderKey },
				new EntityKey { Type = "ContainerNumber", Key = LoadListLineToKeyAndPackingLineMap[loadListLinePK].SelfKey },
			});

			if (LoadListLineToKeyAndPackingLineMap.TryGetValue(loadListLinePK, out var relatedPackingLine) && relatedPackingLine.RelatedPK.IsValid)
			{
				var packingLineEntity = new Entity(writeManagerWriterStrategy);
				packingLineEntity.SetEntityKeyCollection(() => new List<EntityKey>
				{
					new EntityKey { Type = "ForwardingShipment", Key = PackingLineToKeyAndLoadListLineMap[relatedPackingLine.RelatedPK].HeaderKey },
					new EntityKey { Type = "PackingLineID", Key = PackingLineToKeyAndLoadListLineMap[relatedPackingLine.RelatedPK].SelfKey },
				});
				loadListLineEntity.SetRelatedEntityCollection(() => new List<Entity> { packingLineEntity });
			}
			return loadListLineEntity;
		}

		public void SetRelatedEntityCollection(IDataObjectWriterStrategy writeManagerWriterStrategy, ForwardingPackLine packLineBO, UniversalPackingLine packLineDataObject)
		{
			if (!PackingLineToKeyAndLoadListLineMap.TryGetValue(packLineBO.PK, out var packingLineRelatedData) || !LoadListLineToKeyAndBookingLineMap.TryGetValue(packingLineRelatedData.RelatedPK, out var loadListLineRelatedData) || !BookingLineToKeyAndOrderLineMap.TryGetValue(loadListLineRelatedData.RelatedPK, out var bookingLineRelatedData))
			{
				return;
			}
			if (OrderLineToKeyAndOrderMap[bookingLineRelatedData.RelatedPK].SelfStatus == Constants.OrderStatus.Cancelled)
			{
				return;
			}
			var orderLineRelatedEntity = new Entity(writeManagerWriterStrategy);
			orderLineRelatedEntity.SetEntityKeyCollection(() => new List<EntityKey>()
			{
				new EntityKey { Type = "OrderContextKey", Key = OrderLineToKeyAndOrderMap[bookingLineRelatedData.RelatedPK].HeaderKey },
				new EntityKey { Type = "OrderLineContextKey", Key = OrderLineToKeyAndOrderMap[bookingLineRelatedData.RelatedPK].SelfKey },
			});
			packLineDataObject.SetRelatedEntityCollection(() => new List<Entity> { orderLineRelatedEntity });

			if (BookingLineToKeyAndOrderLineMap[loadListLineRelatedData.RelatedPK].SelfStatus == Constants.SupplierBookingStatus.Cancelled)
			{
				return;
			}
			var bookingLineRelatedEntity = new Entity(writeManagerWriterStrategy);
			bookingLineRelatedEntity.SetEntityKeyCollection(() => new List<EntityKey>()
			{
				new EntityKey { Type = "SupplierBooking", Key = BookingLineToKeyAndOrderLineMap[loadListLineRelatedData.RelatedPK].HeaderKey },
				new EntityKey { Type = "BookingLineID", Key = BookingLineToKeyAndOrderLineMap[loadListLineRelatedData.RelatedPK].SelfKey }
			});
			orderLineRelatedEntity.SetRelatedEntityCollection(() => new List<Entity> { bookingLineRelatedEntity });

			if (LoadListLineToKeyAndPackingLineMap[packingLineRelatedData.RelatedPK].SelfStatus != Constants.ContainerLoadListHeaderStatus.Cancelled)
			{
				var loadListLineRelatedEntity = new Entity(writeManagerWriterStrategy);
				bookingLineRelatedEntity.SetRelatedEntityCollection(() => new List<Entity> { loadListLineRelatedEntity });
				loadListLineRelatedEntity.SetEntityKeyCollection(() => new List<EntityKey>()
				{
					new EntityKey { Type = "ContainerLoadList", Key = LoadListLineToKeyAndPackingLineMap[packingLineRelatedData.RelatedPK].HeaderKey },
					new EntityKey { Type = "ContainerNumber", Key = LoadListLineToKeyAndPackingLineMap[packingLineRelatedData.RelatedPK].SelfKey },
				});
			}
		}

		public void SetRelatedEntityCollection(IDataObjectWriterStrategy writeManagerWriterStrategy, OrderLine orderLineBo, UniversalOrderLine orderLineDataObject)
		{
			var allBookingLinePKMap = BookingLineToKeyAndOrderLineMap.Where(keyValuePair => keyValuePair.Value.RelatedPK.Equals(orderLineBo.PK));
			if (allBookingLinePKMap.IsNullOrEmpty())
			{
				return;
			}
			var bookingLineObjectDataList = new List<Entity>();
			foreach (var bookingLinePKMap in allBookingLinePKMap)
			{
				var currentBookingLinePK = bookingLinePKMap.Key;
				var bookingLineRelatedEntity = new Entity(writeManagerWriterStrategy);
				if (BookingLineToKeyAndOrderLineMap[currentBookingLinePK].SelfStatus == Constants.SupplierBookingStatus.Cancelled)
				{
					continue;
				}
				bookingLineRelatedEntity.SetEntityKeyCollection(() => new List<EntityKey>
				{
					new EntityKey { Type = "SupplierBooking", Key = BookingLineToKeyAndOrderLineMap[currentBookingLinePK].HeaderKey },
					new EntityKey { Type = "BookingLineID", Key = BookingLineToKeyAndOrderLineMap[currentBookingLinePK].SelfKey },
				});
				bookingLineObjectDataList.Add(bookingLineRelatedEntity);

				var allLoadListLinePKMap = LoadListLineToKeyAndBookingLineMap.Where(keyValuePair => keyValuePair.Value.RelatedPK.Equals(currentBookingLinePK));
				if (allLoadListLinePKMap.IsNullOrEmpty())
				{
					continue;
				}
				var loadListLineObjectDataList = GetLoadListLineDataObjectList(writeManagerWriterStrategy, allLoadListLinePKMap);
				if (!loadListLineObjectDataList.IsNullOrEmpty())
				{
					bookingLineRelatedEntity.SetRelatedEntityCollection(() => loadListLineObjectDataList);
				}
			}
			if (!bookingLineObjectDataList.IsNullOrEmpty())
			{
				orderLineDataObject.SetRelatedEntityCollection(() => bookingLineObjectDataList);
			}
		}

		public void SetRelatedEntityCollection(IDataObjectWriterStrategy writeManagerWriterStrategy, JobSupplierBookingLine bookingLineBo, UniversalPackingLine packLineDataObject)
		{
			var allLoadListLineIDMap = LoadListLineToKeyAndBookingLineMap.Where(keyValuePair => keyValuePair.Value.RelatedPK.Equals(bookingLineBo.PK));
			if (allLoadListLineIDMap.IsNullOrEmpty() || OrderLineToKeyAndOrderMap[bookingLineBo.JSL_JO_OrderLine].SelfStatus == Constants.ContainerLoadListHeaderStatus.Cancelled)
			{
				return;
			}
			var orderLineRelatedEntity = new Entity(writeManagerWriterStrategy);
			orderLineRelatedEntity.SetEntityKeyCollection(() => new List<EntityKey>
			{
				new EntityKey { Type = "OrderLineContextKey", Key = OrderLineToKeyAndOrderMap[bookingLineBo.JSL_JO_OrderLine].SelfKey },
			});
			var bookingLineRelatedEntityCollection = new List<Entity> { orderLineRelatedEntity };

			var loadListLineRelatedEntityCollection = GetLoadListLineDataObjectList(writeManagerWriterStrategy, allLoadListLineIDMap);
			if (!loadListLineRelatedEntityCollection.IsNullOrEmpty())
			{
				var loadListLineRelatedEntity = new Entity(writeManagerWriterStrategy);
				loadListLineRelatedEntity.SetRelatedEntityCollection(() => loadListLineRelatedEntityCollection);
				bookingLineRelatedEntityCollection.Add(loadListLineRelatedEntity);
			}
			packLineDataObject.SetRelatedEntityCollection(() => bookingLineRelatedEntityCollection);
		}

		List<Entity> GetLoadListLineDataObjectList(IDataObjectWriterStrategy writeManagerWriterStrategy, IEnumerable<KeyValuePair<ZGuid, LineAndRelatedData>> allLoadListLinePKMap)
		{
			var loadListLineObjectDataList = new List<Entity>();
			foreach (var loadListLinePKMap in allLoadListLinePKMap)
			{
				if (LoadListLineToKeyAndPackingLineMap[loadListLinePKMap.Key].SelfStatus == Constants.ContainerLoadListHeaderStatus.Cancelled)
				{
					continue;
				}
				var relatedEntity = GetRelatedEntityFromLoadListLine(writeManagerWriterStrategy, loadListLinePKMap.Key);
				loadListLineObjectDataList.Add(relatedEntity);
			}
			return loadListLineObjectDataList;
		}

		static DynamicBusinessObjectCollection GetCollection(ForwardingShipment shipment, string pkString)
		{
			var queryString = FormattableString.Invariant($@"
				FROM {JobShipmentSchema.Constants.SqlSchemaName}.{JobShipmentSchema.Constants.TableName}
				JOIN {JobPackLinesSchema.Constants.SqlSchemaName}.{JobPackLinesSchema.Constants.TableName}
				ON {JobPackLinesSchema.Constants.JL_JS} = {JobShipmentSchema.Constants.PK}
				JOIN {ContainerLoadListLineSchema.Constants.SqlSchemaName}.{ContainerLoadListLineSchema.Constants.TableName}
				ON {ContainerLoadListLineSchema.Constants.CLL_JL_PackLine} = {JobPackLinesSchema.Constants.PK}
				JOIN {ContainerLoadListHeaderSchema.Constants.SqlSchemaName}.{ContainerLoadListHeaderSchema.Constants.TableName}
				ON {ContainerLoadListLineSchema.Constants.CLL_CLH_LoadListHeader} = {ContainerLoadListHeaderSchema.Constants.PK}
				JOIN {JobSupplierBookingLineSchema.Constants.SqlSchemaName}.{JobSupplierBookingLineSchema.Constants.TableName}
				ON {ContainerLoadListLineSchema.Constants.CLL_JSL_BookingLine} = {JobSupplierBookingLineSchema.Constants.PK}
				JOIN {JobSupplierBookingSchema.Constants.SqlSchemaName}.{JobSupplierBookingSchema.Constants.TableName}
				ON {JobSupplierBookingLineSchema.Constants.JSL_JSB_Booking} = {JobSupplierBookingSchema.Constants.PK}
				JOIN {JobOrderLineSchema.Constants.SqlSchemaName}.{JobOrderLineSchema.Constants.TableName}
				ON {JobSupplierBookingLineSchema.Constants.JSL_JO_OrderLine} = {JobOrderLineSchema.Constants.PK}
				JOIN {JobOrderHeaderSchema.Constants.SqlSchemaName}.{JobOrderHeaderSchema.Constants.TableName}
				ON {JobOrderLineSchema.Constants.JO_JD} = {JobOrderHeaderSchema.Constants.PK}
				JOIN {JobContainerSchema.Constants.SqlSchemaName}.{JobContainerSchema.Constants.TableName}
				ON {ContainerLoadListLineSchema.Constants.CLL_JC_Container} = {JobContainerSchema.Constants.PK}
			");
			return GetCollectionCore(shipment, pkString, queryString);
		}

		static DynamicBusinessObjectCollection GetCollection(JobSupplierBooking booking, string pkString)
		{
			var queryString = FormattableString.Invariant($@"
				FROM {JobSupplierBookingSchema.Constants.SqlSchemaName}.{JobSupplierBookingSchema.Constants.TableName}
				JOIN {JobSupplierBookingLineSchema.Constants.SqlSchemaName}.{JobSupplierBookingLineSchema.Constants.TableName}
				ON {JobSupplierBookingLineSchema.Constants.JSL_JSB_Booking} = {JobSupplierBookingSchema.Constants.PK}
				JOIN {JobOrderLineSchema.Constants.SqlSchemaName}.{JobOrderLineSchema.Constants.TableName}
				ON {JobSupplierBookingLineSchema.Constants.JSL_JO_OrderLine} = {JobOrderLineSchema.Constants.PK}
				JOIN {JobOrderHeaderSchema.Constants.SqlSchemaName}.{JobOrderHeaderSchema.Constants.TableName}
				ON {JobOrderLineSchema.Constants.JO_JD} = {JobOrderHeaderSchema.Constants.PK}
				LEFT JOIN {ContainerLoadListLineSchema.Constants.SqlSchemaName}.{ContainerLoadListLineSchema.Constants.TableName}
				ON {ContainerLoadListLineSchema.Constants.CLL_JSL_BookingLine} = {JobSupplierBookingLineSchema.Constants.PK}
				LEFT JOIN {ContainerLoadListHeaderSchema.Constants.SqlSchemaName}.{ContainerLoadListHeaderSchema.Constants.TableName}
				ON {ContainerLoadListLineSchema.Constants.CLL_CLH_LoadListHeader} = {ContainerLoadListHeaderSchema.Constants.PK}
				LEFT JOIN {JobPackLinesSchema.Constants.SqlSchemaName}.{JobPackLinesSchema.Constants.TableName}
				ON {ContainerLoadListLineSchema.Constants.CLL_JL_PackLine} = {JobPackLinesSchema.Constants.PK}
				LEFT JOIN {JobShipmentSchema.Constants.SqlSchemaName}.{JobShipmentSchema.Constants.TableName}
				ON {JobPackLinesSchema.Constants.JL_JS} = {JobShipmentSchema.Constants.PK}
				LEFT JOIN {JobContainerSchema.Constants.SqlSchemaName}.{JobContainerSchema.Constants.TableName}
				ON {ContainerLoadListLineSchema.Constants.CLL_JC_Container} = {JobContainerSchema.Constants.PK}
			");
			return GetCollectionCore(booking, pkString, queryString);
		}

		static DynamicBusinessObjectCollection GetCollection(Order order, string pkString)
		{
			var queryString = FormattableString.Invariant($@"
				FROM {JobOrderHeaderSchema.Constants.SqlSchemaName}.{JobOrderHeaderSchema.Constants.TableName}
				JOIN {JobOrderLineSchema.Constants.SqlSchemaName}.{JobOrderLineSchema.Constants.TableName}
				ON {JobOrderLineSchema.Constants.JO_JD} = {JobOrderHeaderSchema.Constants.PK}
				LEFT JOIN {JobSupplierBookingLineSchema.Constants.SqlSchemaName}.{JobSupplierBookingLineSchema.Constants.TableName}
				ON {JobSupplierBookingLineSchema.Constants.JSL_JO_OrderLine} = {JobOrderLineSchema.Constants.PK}
				LEFT JOIN {JobSupplierBookingSchema.Constants.SqlSchemaName}.{JobSupplierBookingSchema.Constants.TableName}
				ON {JobSupplierBookingLineSchema.Constants.JSL_JSB_Booking} = {JobSupplierBookingSchema.Constants.PK}
				LEFT JOIN {ContainerLoadListLineSchema.Constants.SqlSchemaName}.{ContainerLoadListLineSchema.Constants.TableName}
				ON {ContainerLoadListLineSchema.Constants.CLL_JSL_BookingLine} = {JobSupplierBookingLineSchema.Constants.PK}
				LEFT JOIN {ContainerLoadListHeaderSchema.Constants.SqlSchemaName}.{ContainerLoadListHeaderSchema.Constants.TableName}
				ON {ContainerLoadListLineSchema.Constants.CLL_CLH_LoadListHeader} = {ContainerLoadListHeaderSchema.Constants.PK}
				LEFT JOIN {JobContainerSchema.Constants.SqlSchemaName}.{JobContainerSchema.Constants.TableName}
				ON {ContainerLoadListLineSchema.Constants.CLL_JC_Container} = {JobContainerSchema.Constants.PK}
				LEFT JOIN {JobPackLinesSchema.Constants.SqlSchemaName}.{JobPackLinesSchema.Constants.TableName}
				ON {ContainerLoadListLineSchema.Constants.CLL_JL_PackLine} = {JobPackLinesSchema.Constants.PK}
				LEFT JOIN {JobShipmentSchema.Constants.SqlSchemaName}.{JobShipmentSchema.Constants.TableName}
				ON {JobPackLinesSchema.Constants.JL_JS} = {JobShipmentSchema.Constants.PK}
			");
			return GetCollectionCore(order, pkString, queryString);
		}

		static DynamicBusinessObjectCollection GetCollectionCore(BusinessObject businessObject, string pkString, string queryString)
		{
			var collection = new DynamicBusinessObjectCollection(businessObject.Factory);
			var query = FormattableString.Invariant($@"
				SELECT {JobPackLinesSchema.Constants.PK}, {JobPackLinesSchema.Constants.JL_PackLineId}, {ContainerLoadListLineSchema.Constants.PK}, {JobSupplierBookingLineSchema.Constants.PK}, {JobSupplierBookingLineSchema.Constants.JSL_BookingLineId}, {JobOrderLineSchema.Constants.PK}, {JobShipmentSchema.Constants.PK}, {JobShipmentSchema.Constants.JS_UniqueConsignRef}, {JobSupplierBookingSchema.Constants.PK}, {JobSupplierBookingSchema.Constants.JSB_BookingId}, {ContainerLoadListHeaderSchema
					.Constants.CLH_LoadListId}, {JobOrderHeaderSchema.Constants.PK}, {JobContainerSchema.Constants.JC_ContainerNum}, {JobOrderHeaderSchema.Constants.JD_OrderStatus}, {JobSupplierBookingSchema.Constants.JSB_Status}, {ContainerLoadListHeaderSchema.Constants.CLH_Status}
				{queryString}
				WHERE {pkString} = '{businessObject.PK}'
			");
			collection.Load(query);
			return collection;
		}

		#region Related Object

		Dictionary<ZGuid, LineAndRelatedData> LoadListLineToKeyAndPackingLineMap { get; set; } = new Dictionary<ZGuid, LineAndRelatedData>();

		Dictionary<ZGuid, LineAndRelatedData> LoadListLineToKeyAndBookingLineMap { get; set; } = new Dictionary<ZGuid, LineAndRelatedData>();

		Dictionary<ZGuid, LineAndRelatedData> PackingLineToKeyAndLoadListLineMap { get; set; } = new Dictionary<ZGuid, LineAndRelatedData>();

		Dictionary<ZGuid, LineAndRelatedData> BookingLineToKeyAndOrderLineMap { get; set; } = new Dictionary<ZGuid, LineAndRelatedData>();

		Dictionary<ZGuid, LineAndRelatedData> OrderLineToKeyAndOrderMap { get; set; } = new Dictionary<ZGuid, LineAndRelatedData>();

		#endregion

		class LineAndRelatedData
		{
			public ZGuid RelatedPK { get; set; }
			public string SelfKey { get; set; }
			public string HeaderKey { get; set; }
			public string SelfStatus { get; set; }

			public LineAndRelatedData(ZGuid relatedPk, string selfKey, string headerKey, string selfStatus)
			{
				RelatedPK = relatedPk;
				SelfKey = selfKey;
				HeaderKey = headerKey;
				SelfStatus = selfStatus;
			}
		}
	}
}
