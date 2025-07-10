using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.Freight.Forwarding.ServiceTasks.Orders;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Freight.Forwarding.ServiceTasks
{
	class OrderManagerConvertService
	{
		internal void FinalizingSupplierBookingShipmentCreation(JobSupplierBooking finalizingBooking)
		{
			var factory = finalizingBooking.Factory;
			var availableQuery = new ZQuery(ContainerLoadListHeaderSchema.CLH_JSB_Booking, finalizingBooking.PK);
			availableQuery.AddToFilter(ContainerLoadListHeaderSchema.CLH_Status, ContainerLoadListHeaderStatus.Placed);
			var loadLists = factory.Load<CYContainerLoadList>(availableQuery);

			foreach (var loadList in loadLists)
			{
				loadList.Logs.AddNew(AutoEvents.StatusUpdated, $"|TYP=CLL|OLD={loadList.CLH_Status}|NEW={ContainerLoadListHeaderStatus.Converted}");
				ConvertContainerLoadListToShipments(loadList.PK, factory, isFinalizing: true);
			}

			ReleaseUnusedContainersFromCYBooking(finalizingBooking, factory);

			finalizingBooking.JSB_Status = SupplierBookingStatus.Converted;
		}

		internal void ConvertOrderShipmentPlanning(JobSupplierBooking supplierBooking)
		{
			var converter = new OrderShipmentPlanningConverter();
			supplierBooking.OrderShipmentPlannings.ForEach((shipmentPlanning) => converter.ConvertToShipment(shipmentPlanning));

			// We do not keep OrderShipmentPlannings and its lines after conversion because shipments are the single source of truth
			supplierBooking.OrderShipmentPlannings.DeleteAll();
		}

		internal void ConvertContainerLoadListToShipments(ZGuid containerLoadListPK, BusinessObjectFactory factory, bool isFinalizing = false)
		{
			var loadListHeader = factory.Load<CommonContainerLoadList>(containerLoadListPK);
			var packedContainerLoadListLines = loadListHeader.LoadListLines.Where(loadListLine => !loadListLine.CLL_JC_Container.IsEmpty);

			var plannedLoadListLinePackLinesMap = new List<(ContainerLoadListLine, ForwardingPackLine[])>();
			var unplannedLoadListLines = new List<ContainerLoadListLine>(); // Including non-SPT-CLL and SPT-CLL packed to the wrong consol

			foreach (var loadListLine in packedContainerLoadListLines)
			{
				var packLines = FindPackLinesUnderSameConsolAndBookingLine(loadListLine);
				if (packLines.Length > 0)
				{
					plannedLoadListLinePackLinesMap.Add((loadListLine, packLines));
				}
				else
				{
					unplannedLoadListLines.Add(loadListLine);
				}
			}

			AllocatePlannedLoadListLinesToShipmentPackLines(plannedLoadListLinePackLinesMap, isFinalizing);
			PackUnplannedLoadListLinesIntoNewShipments(unplannedLoadListLines, isFinalizing);

			loadListHeader.CLH_Status = ContainerLoadListHeaderStatus.Converted;
		}

		void AllocatePlannedLoadListLinesToShipmentPackLines(
			List<(ContainerLoadListLine LoadListLine, ForwardingPackLine[] PackLines)> plannedLoadListLinePackLinesMap, bool isFinalizing)
		{
			var modifiedShipments = new HashSet<ForwardingShipment>();
			foreach (var (loadListLine, packLines) in plannedLoadListLinePackLinesMap)
			{
				var supplierBookingLine = loadListLine.SupplierBookingLine;
				var orderLine = supplierBookingLine.OrderLine;
				ForwardingPackLine convertedPackLine;

				// Try to use the earliest created planned packline that is not converted as a template
				var sortedPackLines = packLines.OrderBy(line => line.JL_SystemCreateTimeUtc).ToArray();
				var templatePackLine = sortedPackLines.FirstOrDefault(line => !line.IsPackedForOrderPlanning);

				if (templatePackLine != null)
				{
					var matchedProduct = templatePackLine.Products.FirstOrDefault(product => product.D2_JO == orderLine.PK);
					if (matchedProduct != null && matchedProduct.D2_ProductQuantity > loadListLine.CLL_PackedQuantity)
					{
						// Split from planned pack line when short packed
						templatePackLine.JL_ActualVolume = Math.Max(0m, templatePackLine.JL_ActualVolume - loadListLine.CLL_Volume);
						templatePackLine.JL_ActualWeight = Math.Max(0m, templatePackLine.JL_ActualWeight - loadListLine.CLL_Weight);
						templatePackLine.JL_PackageCount = Math.Max(0, templatePackLine.JL_PackageCount - loadListLine.CLL_Packages);
						matchedProduct.D2_ProductQuantity = Math.Max(0m, matchedProduct.D2_ProductQuantity - loadListLine.CLL_PackedQuantity);
						convertedPackLine = (ForwardingPackLine)templatePackLine.Clone();
						convertedPackLine.JL_JSL_BookingLine = templatePackLine.JL_JSL_BookingLine;
						templatePackLine.Shipment.OuterPackLines.Add(convertedPackLine);
					}
					else
					{
						// Convert planned pack line in place when over packed or no product found
						convertedPackLine = templatePackLine;
					}
				}
				else
				{
					// All planned pack lines are used by previous conversion. Use converted planned packline as a template
					templatePackLine = sortedPackLines.FirstOrDefault();
					convertedPackLine = (ForwardingPackLine)templatePackLine.Clone();
					convertedPackLine.JL_JSL_BookingLine = templatePackLine.JL_JSL_BookingLine;
					templatePackLine.Shipment.OuterPackLines.Add(convertedPackLine);
				}

				PopulatePropertiesOnConvertedLine(loadListLine, supplierBookingLine, orderLine, convertedPackLine);
				loadListLine.CLL_JL_PackLine = convertedPackLine.PK;

				modifiedShipments.Add(templatePackLine.Shipment);
			}

			modifiedShipments.ForEach(shipment =>
			{
				shipment.UpdateShipmentFromOuterPackLines();
				shipment.UpdateOverflowingContainerWeightMeasureUnit();
			});

			if (!isFinalizing)
			{
				modifiedShipments.ForEach(shipment =>
					shipment.UpdateShipmentFromContainerLoadListHeader(plannedLoadListLinePackLinesMap[0].LoadListLine.LoadListHeader));
			}
		}

		static void PopulatePropertiesOnConvertedLine(ContainerLoadListLine loadListLine, JobSupplierBookingLine supplierBookingLine, OrderLine orderLine, ForwardingPackLine convertedPackLine)
		{
			convertedPackLine.SetContainer(loadListLine.Container.Consol, loadListLine.Container);

			convertedPackLine.JL_Length = supplierBookingLine.JSL_PackLength;
			convertedPackLine.JL_Width = supplierBookingLine.JSL_PackWidth;
			convertedPackLine.JL_Height = supplierBookingLine.JSL_PackHeight;
			convertedPackLine.JL_UnitOfDimension = supplierBookingLine.JSL_PackUnitOfDimension;

			convertedPackLine.JL_PackageCount = loadListLine.CLL_Packages;
			convertedPackLine.JL_ActualVolume = loadListLine.CLL_Volume;
			convertedPackLine.JL_ActualWeight = loadListLine.CLL_Weight;
			convertedPackLine.JL_Description = loadListLine.CLL_Description;
			convertedPackLine.JL_MarksAndNumbers = loadListLine.CLL_MarksAndNumbers;

			var packProduct = convertedPackLine.Products.FirstOrDefault(product => product.D2_JO == orderLine.PK) ?? convertedPackLine.Products.AddNew();
			packProduct.D2_JO = orderLine.PK;
			packProduct.D2_ProductQuantity = loadListLine.CLL_PackedQuantity;
			packProduct.D2_ProductUnitOfQty = orderLine.JO_F3_NKPackType;
		}

		void PackUnplannedLoadListLinesIntoNewShipments(List<ContainerLoadListLine> loadListLines, bool isFinalizing)
		{
			var additionalShipmentsPerBooking = new Dictionary<JobSupplierBooking, NewShipmentTracker>();

			var containerLoadListLineConverter = new ContainerLoadListLineConverter();
			foreach (var convertLookupItem in loadListLines.ToLookup(ShipmentMatchKey.BuildKey, new ShipmentMatchKeyComparer()))
			{
				var key = convertLookupItem.Key;
				var shipment =
					SupplierBookingConverter.ConvertToShipmentForLoadList(
						key.SupplierBooking, convertLookupItem.First().SupplierBookingLine.OrderLine.Order, key.PackedConsol);

				AddExceptionLogForWhenSavingAdditionalShipments(key, shipment);

				convertLookupItem
					.OrderBy(cll => cll.CLL_LoadSequence)
					.ForEach(loadListLine =>
						containerLoadListLineConverter.ConvertLoadListLineToPackLine(shipment, loadListLine));

				if (!isFinalizing)
				{
					shipment.UpdateShipmentFromContainerLoadListHeader(loadListLines[0].LoadListHeader);
				}
				shipment.UpdateShipmentMeasuresFromPackLines();
				shipment.UpdateShipmentFromOuterPackLines();
				shipment.UpdateOverflowingContainerWeightMeasureUnit();
			}

			void AddExceptionLogForWhenSavingAdditionalShipments(ShipmentMatchKey key, ForwardingShipment newShipment)
			{
				var booking = key.SupplierBooking;
				var newShipmentTracker = additionalShipmentsPerBooking.GetOrAdd(booking, () => new NewShipmentTracker());
				newShipmentTracker.Counter++;
				newShipment.OnSavingShipment += addExceptionLog;

				void addExceptionLog(object sender, EventArgs e)
				{
					var shipment = sender as ForwardingShipment;
					try
					{
						newShipmentTracker.SavedShipmentIDs.Add(shipment.JS_UniqueConsignRef);
						if (newShipmentTracker.SavedShipmentIDs.Count == newShipmentTracker.Counter)
						{
							booking.AddMismatchedPackingCompletedEvent(newShipmentTracker.SavedShipmentIDs);
						}
					}
					finally
					{
						shipment.OnSavingShipment -= addExceptionLog;
					}
				}
			}
		}

		ForwardingPackLine[] FindPackLinesUnderSameConsolAndBookingLine(ContainerLoadListLine loadListLine)
		{
			var packedConsol = loadListLine.Container.Consol;

			var packedConShipPivotSubQuery = new ZDBOnlySubQuery(typeof(JobConShipLink), JobConShipLinkSchema.JN_JS);
			packedConShipPivotSubQuery.AddToFilter(JobConShipLinkSchema.JN_JK, packedConsol.PK);

			var plannedPackLineSubQuery = new ZDBOnlySubQuery(typeof(ForwardingPackLine), JobPackLinesSchema.PK);
			plannedPackLineSubQuery.AddToFilter(JobPackLinesSchema.JL_JSL_BookingLine, loadListLine.CLL_JSL_BookingLine);

			var packLinesQuery = new ZDBOnlyQuery(typeof(ForwardingPackLine));
			packLinesQuery.OrderBy = JobPackLinesSchema.JL_SystemCreateTimeUtc.Name;
			packLinesQuery.AddSubQuery(JobPackLinesSchema.JL_JS, packedConShipPivotSubQuery, JoinCondition.And);
			packLinesQuery.AddSubQuery(plannedPackLineSubQuery, JoinCondition.And);

			return loadListLine.Factory.Load<ForwardingPackLine>(packLinesQuery);
		}

		internal void ConvertLooseCargoSupplierBooking(JobSupplierBooking booking, BusinessObjectFactory factory)
		{
			if (booking.JSB_Status == SupplierBookingStatus.Converted)
			{
				return;
			}

			var shipment = factory.New<ForwardingShipment>();
			SupplierBookingConverter.ConvertToShipmentForBooking(booking, shipment);
			booking.SupplierBookingLines.ForEach(bookingLine => SupplierBookingLineConverter.ConvertDispatchedLooseCargoBookingLineToPackLine(shipment, bookingLine));

			shipment.UpdateShipmentFromOuterPackLines();
			shipment.JS_GoodsValue = shipment.OuterPackLines.OfType<ForwardingPackLine>().Sum(line => line.JL_LinePrice);

			booking.JSB_Status = SupplierBookingStatus.Converted;
		}

		void ReleaseUnusedContainersFromCYBooking(JobSupplierBooking supplierBooking, BusinessObjectFactory factory)
		{
			var containersAllocatedToBooking = supplierBooking.Containers.OfType<ForwardingContainer>();

			var cllsLinkedToBookingLinesQuery = new ZDBOnlyQuery(typeof(ContainerLoadListLine))
				.AddToFilter(ContainerLoadListLineSchema.CLL_JSL_BookingLine, supplierBooking.SupplierBookingLines.Select(x => x.PK));
			var cllsLinkedToBookingLines = factory.Load<ContainerLoadListLine>(cllsLinkedToBookingLinesQuery).Select(x => x.CLL_JC_Container).ToHashSet();

			foreach (var container in containersAllocatedToBooking)
			{
				if (!cllsLinkedToBookingLines.Contains(container.PK))
				{
					container.JC_JSB_SupplierBooking = ZGuid.Empty;
				}
			}
		}

		JobSupplierBookingConverter supplierBookingConverter;
		protected virtual JobSupplierBookingConverter SupplierBookingConverter => supplierBookingConverter ??= new JobSupplierBookingConverter();

		JobSupplierBookingLineConverter supplierBookingLineConverter;
		protected virtual JobSupplierBookingLineConverter SupplierBookingLineConverter => supplierBookingLineConverter ??= new JobSupplierBookingLineConverter();

		class NewShipmentTracker
		{
			public int Counter;
			public List<string> SavedShipmentIDs = new List<string>();
		}
	}
}
