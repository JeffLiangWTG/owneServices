using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;
using BusinessCodeLists = Enterprise.Warehouse.Transactions.CodeLists;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal
{
	public class WhsReceiveDataObjectReader : WhsOrderAndReceiveDataObjectReader<WhsReceive, WhsReceiveLine>
	{
		public WhsReceiveDataObjectReader(UniversalShipment whsReceiveDataObject, IXmlImportLogger logger, UniversalObjectFactory factory, WhsOrder orderForChangeOfInventory = null)
			: base(whsReceiveDataObject, logger, factory)
		{
			OrderForChangeOfInventory = orderForChangeOfInventory;
		}

		readonly WhsOrder OrderForChangeOfInventory;

		#region Matching Job

		protected override string DocketType => ReceiveDocketType;

		internal static string ReceiveDocketType => Res.GetString("f0f0cb08-1265-4eae-bf70-12400fc2d8a2", "Receipt");

		protected override string DocketTypeCode => BusinessCodeLists.DocketType.Codes.Receive;

		public override DataContextType DataContextType => DataContextType.WarehouseReceive;

		#endregion

		#region Matching References (fallbacks)

		protected override WhsOrderAndReceiveLastResortMatcher<WhsReceive> GetMatcher(WhsOrderAndReceiveReferences references)
		{
			return new WhsReceiveMatcher(factory.BOFactory, references, logger);
		}

		#endregion

		// Create / Update

		#region Create / Update Job

		protected override void PopulateBusinessObjectCore(WhsReceive receive)
		{
			base.PopulateBusinessObjectCore(receive);

			if (receive.WD_CustomerReference.IsEmpty)
			{
				var customerRef = IsDataSourceForwardingShipment
					? dataObject.AdditionalReferenceCollection?.FirstOrDefault(x => x.Type.GetCodeAsUpperCase() == CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.CustomerLoadReference)?.ReferenceNumber.Value
					: dataObject.OwnerRef;
				SetValue(receive, WhsDocketSchema.WD_CustomerReference, customerRef);
			}

			var receiveDataObject = dataObject.Order;
			if (receiveDataObject != null)
			{
				SetValue(receive, WhsDocketSchema.WD_TotalPallets, receiveDataObject.PalletsSent);
				SetValue(receive, WhsDocketSchema.WD_ReceiveCategory, receiveDataObject.Category);
				SetValue(receive, WhsDocketSchema.WD_HoldPalletIDPutaway, receiveDataObject.HoldPalletIDPutaway);

				if (!string.IsNullOrEmpty(receiveDataObject.StagingArea) && WarehouseDataRegistry.Instance.ExposeWarehouseTaskManagement.Value)
				{
					var inboundDockDoorLocation = receive.Warehouse.FindLocation(receiveDataObject.StagingArea.GetValueOrDefault());
					if (inboundDockDoorLocation != null && inboundDockDoorLocation.IsDockDoorLocation)
					{
						SetValue(receive, WhsDocketSchema.WD_WL_InboundDockDoor, inboundDockDoorLocation.PK);
					}
					else
					{
						throw new DataObjectReadFailureException("Cannot import receive - The StagingArea is not a dock door location.");
					}
				}
			}

			SetValue(receive, WhsDocketSchema.WD_PackagesSent, dataObject.OuterPacks);
			SetValue(receive, WhsDocketSchema.WD_F3_NKTotalPackType, dataObject.OuterPacksPackageType);

			PopulateDates(receive);

			if (IsDataSourceForwardingShipment)
			{
				var bookings = dataObject.PostCarriageShipmentCollection?.SelectMany(x => x.SubShipmentCollection ?? Enumerable.Empty<UniversalShipment>());

				var validArrivalCartageRefBookings = bookings != null ? bookings.Where(b => !string.IsNullOrEmpty(b.LocalProcessing?.ArrivalCartageRef)).ToArray() : null;

				var validArrivalCartageRefBooking = validArrivalCartageRefBookings != null && validArrivalCartageRefBookings.Length > 0
					? validArrivalCartageRefBookings.MaxBy(b => b.DateCollection?.FirstOrDefault(x => x.Type == DateType.JobCreated)?.Value?.ToZDateTime()) : null;
				if (validArrivalCartageRefBooking != null)
				{
					SetValue(receive, WhsDocketSchema.WD_TransportReference, validArrivalCartageRefBooking.LocalProcessing?.ArrivalCartageRef);
				}
				SetValue(receive, WhsDocketSchema.WD_DropMode, dataObject.LocalProcessing?.FCLDeliveryEquipmentNeeded);
				SetValue(receive, WhsDocketSchema.WD_TotalWeight, dataObject.TotalWeight);
				var weightUnit = dataObject.TotalWeightUnit.GetCodeAsUpperCase();
				if (!weightUnit.IsEmpty)
				{
					SetValueIfNotReadOnly(receive, WhsDocketSchema.WD_TotalWeightUnit, weightUnit);
				}
				SetValue(receive, WhsDocketSchema.WD_TotalCubic, dataObject.TotalVolume);
				var volumeUnit = dataObject.TotalVolumeUnit.GetCodeAsUpperCase();
				if (!volumeUnit.IsEmpty)
				{
					SetValueIfNotReadOnly(receive, WhsDocketSchema.WD_TotalCubicUnit, volumeUnit);
				}
			}
		}

		protected override IEnumerable<ZString?> GetValidContainerIDs() => IsDataSourceForwardingShipment ? dataObject.PackingLineCollection?.Select(x => x.ContainerNumber) : null;

		protected override void CalculateTotalsIfNeededCore(WhsReceive docket)
		{
			var lines = new Lazy<IEnumerable<ILineWithProductAndQuantity>>(() => docket.Lines.Where(l => l.SupplierPart != null && l.Inventory.Count == 1).ToProductAndQuantities().ToArray());

			var currentTotalVolume = IsDataSourceForwardingShipment ? dataObject.TotalVolume : dataObject.Order?.TotalLineVolume;
			if (currentTotalVolume.GetValueOrDefault() == 0m)
			{
				var volumeMeasure = docket.GetVolumeMeasure();
				var totalVolume = UnitOfMeasureConverter.GetTotalQuantityFromLines(docket, volumeMeasure, lines.Value);
				if (totalVolume > 0)
				{
					SetValue(docket, WhsDocketSchema.WD_TotalCubic, totalVolume);
				}
			}

			var currentTotalWeight = IsDataSourceForwardingShipment ? dataObject.TotalWeight : dataObject.Order?.TotalLineWeight;
			if (currentTotalWeight.GetValueOrDefault() == 0m)
			{
				var weightMeasure = docket.GetWeightMeasure();
				var totalWeight = UnitOfMeasureConverter.GetTotalQuantityFromLines(docket, weightMeasure, lines.Value);
				if (totalWeight > 0)
				{
					SetValue(docket, WhsDocketSchema.WD_TotalWeight, totalWeight);
				}
			}

			SetDocketTotalUnits(docket);
		}

		void SetDocketTotalUnits(WhsReceive docket)
		{
			var totalUnits = dataObject.Order != null ? dataObject.Order.TotalUnits : docket.Lines.Sum(l => l.WE_StockOnHand);
			SetValue(docket, WhsDocketSchema.WD_TotalUnits, totalUnits);
		}

		#region LinkParent

		protected override IEnumerable<DataContextType> ParentDataContextTypes
		{
			get { return IsDataSourceForwardingShipment ? DataContextType.ForwardingShipment.Yield() : DataContextType.OrderManagerOrder.Yield(); }
		}

		#endregion

		#region Populate Related Entities

		#region Populate Lines

		protected override IEnumerable<IWarehouseCustomsLinePackDetails> GetPackDetailsFromCustomsLineDetails(IWarehouseCustomsLineDetails warehouseCustomsLineDetails)
		{
			return warehouseCustomsLineDetails.PackDetails;
		}

		protected override IEnumerable<OrderLine> GetDocketLinesCollectionCore(OrgHeader client)
		{
			var result = !CustomsHelper.IsDataSourceCustoms && IsDataSourceForwardingShipment
				? GetDocketLinesFromPackingLineCollection()
				: base.GetDocketLinesCollectionCore(client);

			// tested in WhsBondedChangeOfInventoryDataObjectReaderTest
			if (CustomsHelper.IsWarehouseBondedChangeOfInventory)
			{
				result = result.SelectMany(l => GetOrderLinesWithLocations(l));
			}

			return result;
		}

		IEnumerable<OrderLine> GetDocketLinesFromPackingLineCollection()
		{
			var packingLines = dataObject.PackingLineCollection;
			var packedArray = packingLines?.SelectMany(x => x.PackedItemCollection.Select(p => new { PackLine = x, Item = p })).ToArray();
			if (packedArray != null)
			{
				foreach (var packed in packedArray)
				{
					yield return GetDocketLinesFromPackingItem(packed.Item, packed.PackLine);
				}
			}
		}

		OrderLine GetDocketLinesFromPackingItem(PackedItem item, PackingLine line)
		{
			var orderLine = new OrderLine();
			orderLine.Product = item.Product;
			orderLine.PalletID = line.ReferenceNumber;
			orderLine.PackageQty = line.PackedItemCollection.Count > 1 ? item.PackedQuantity : (ZDecimal?)line.PackQty;
			orderLine.PackageQtyUnit = line.PackedItemCollection.Count > 1 ? item.UnitOfQuantity : line.PackType;

			return orderLine;
		}

		#region GetDocketLinesWithLocations

		IEnumerable<OrderLine> GetOrderLinesWithLocations(OrderLine orderLine)
		{
			var orderLineDetails = GetOrderLineDetails(orderLine);
			var firstOrderLine = orderLineDetails.FirstOrDefault();
			PopulateLocationAndQuantityAndCustomsSpecificInfo(orderLine, firstOrderLine?.Location, firstOrderLine?.Quantity, firstOrderLine?.IsMainInwardsProcessedItem, firstOrderLine?.IsSecondaryInwardsProcessedItem);
			yield return orderLine;

			var extraDetails = GetExtraOrderLineDetails(orderLine);
			foreach (var location in orderLineDetails.Skip(1))
			{
				var splitOrderLineWithLocation = (OrderLine)orderLine.Clone();
				GetOrAddExtraOrderLineDetails(splitOrderLineWithLocation, () => extraDetails);
				PopulateLocationAndQuantityAndCustomsSpecificInfo(splitOrderLineWithLocation, location.Location, location.Quantity, location.IsMainInwardsProcessedItem, location.IsSecondaryInwardsProcessedItem);
				yield return splitOrderLineWithLocation;
			}
		}

		OrderLineDetails[] GetOrderLineDetails(OrderLine orderLine)
		{
			var previousProductCode = orderLine.Product.GetCodeAsUpperCase();
			var previousPartAttrib1 = orderLine.PartAttribute1.GetValueOrDefault();
			var previousPartAttrib2 = orderLine.PartAttribute2.GetValueOrDefault();
			var previousPartAttrib3 = orderLine.PartAttribute3.GetValueOrDefault();
			var previousSerialNumber = orderLine.SerialNumber.GetValueOrDefault();
			var previousEntryKey = WhsBondedWarehouseAttribute.BuildKey(orderLine.CustomsData.InwardsEntryKey.GetValueOrDefault(), orderLine.CustomsData.InwardsEntryLineNumber.GetValueOrDefault());
			var orderedQty = orderLine.OrderedQty.GetValueOrDefault();

			return TakeAndReduceOrderLocationsByQuantity(previousProductCode, previousPartAttrib1, previousPartAttrib2, previousPartAttrib3, previousSerialNumber, previousEntryKey, orderedQty).ToArray();
		}

		void PopulateLocationAndQuantityAndCustomsSpecificInfo(OrderLine orderLine, WhsLocation location, ZDecimal? quantity, ZBool? isMainInwardsProcessedItem, ZBool? isSecondaryInwardsProcessedItem)
		{
			if (location != null)
			{
				orderLine.Location = new Location
				{
					Column = location.WLV_Column,
					Level = location.WLV_Level,
					Tray = location.WLV_Tray,
					Row = location.RowName
				};
			}

			orderLine.OrderedQty = quantity;

			orderLine.CustomsData.IsMainInwardsProcessedItem = isMainInwardsProcessedItem;
			orderLine.CustomsData.IsSecondaryInwardsProcessedItem = isSecondaryInwardsProcessedItem;
		}

		#endregion

		#region OrderInventories

		Dictionary<ZString, List<OrderInventory>> OrderInventories
		{
			get
			{
				if (orderInventories == null)
				{
					orderInventories = new Dictionary<ZString, List<OrderInventory>>();
					if (OrderForChangeOfInventory != null)
					{
						foreach (var line in OrderForChangeOfInventory.Lines)
						{
							foreach (var pickLine in line.PickLines)
							{
								var inventory = pickLine.Inventory;

								var isMainInwardProcessingJob = inventory.CustomsData.WB_IsMainInwardsProcessedItem;
								var isSecondaryInwardProcessingJob = inventory.CustomsData.WB_IsSecondaryInwardsProcessedItem;

								var location = ImportStrategy.IsWarehouseBondedChangeOfWarehouse ? null : inventory.Location;

								var orderInventory = new OrderInventory(
									location,
									pickLine.ProductCode,
									inventory.WI_PartAttrib1,
									inventory.WI_PartAttrib2,
									inventory.WI_PartAttrib3,
									inventory.WI_SerialNumber,
									inventory.WI_BondedEntryKey,
									pickLine.WZ_Units,
									isMainInwardProcessingJob,
									isSecondaryInwardProcessingJob);

								orderInventories.GetOrAdd(orderInventory.Key, () => new List<OrderInventory>()).Add(orderInventory);
							}
						}
					}
				}
				return orderInventories;
			}
		}
		Dictionary<ZString, List<OrderInventory>> orderInventories;

		class OrderInventory
		{
			public OrderInventory(WhsLocation location, ZString product, ZString partAttribute1, ZString partAttribute2, ZString partAttribute3, ZString serialNumber, ZString bondedEntryKey, ZDecimal quantity, bool isMainInwardsProcessedItem, bool isSecondaryInwardsProcessedItem)
			{
				Location = location;
				Product = product;
				PartAttribute1 = partAttribute1;
				PartAttribute2 = partAttribute2;
				PartAttribute3 = partAttribute3;
				SerialNumber = serialNumber;
				BondedEntryKey = bondedEntryKey;
				Quantity = quantity;

				IsMainInwardsProcessedItem = isMainInwardsProcessedItem;
				IsSecondaryInwardsProcessedItem = isSecondaryInwardsProcessedItem;
			}

			public readonly WhsLocation Location;
			public readonly ZString Product;
			public readonly ZString PartAttribute1;
			public readonly ZString PartAttribute2;
			public readonly ZString PartAttribute3;
			public readonly ZString SerialNumber;
			public readonly ZString BondedEntryKey;

			public readonly ZBool IsMainInwardsProcessedItem;
			public readonly ZBool IsSecondaryInwardsProcessedItem;

			public ZDecimal Quantity { get; set; }

			public ZString Key
			{
				get { return GetKey(Product, PartAttribute1, PartAttribute2, PartAttribute3, SerialNumber, BondedEntryKey); }
			}

			public static ZString GetKey(ZString product, ZString partAttribute1, ZString partAttribute2, ZString partAttribute3, ZString serialNumber, ZString bondedEntryKey)
			{
				return string.Join("|", product, partAttribute1, partAttribute2, partAttribute3, serialNumber, bondedEntryKey).ToUpperInvariant();
			}
		}

		#endregion

		#region GetLocationsAndReduceOrderInventory

		IEnumerable<OrderLineDetails> TakeAndReduceOrderLocationsByQuantity(ZString previousProduct, ZString previousAttrib1, ZString previousAttrib2, ZString previousAttrib3, ZString previousSerialNumber, ZString previousEntryKey, ZDecimal quantity)
		{
			var result = new List<OrderLineDetails>();
			if (OrderInventories.TryGetValue(OrderInventory.GetKey(previousProduct, previousAttrib1, previousAttrib2, previousAttrib3, previousSerialNumber, previousEntryKey), out var matchingInventory))
			{
				var exactMatch = matchingInventory.FirstOrDefault(i => i.Quantity == quantity);
				if (exactMatch != null)
				{
					result.Add(new OrderLineDetails(exactMatch.Location, quantity, exactMatch.IsMainInwardsProcessedItem, exactMatch.IsSecondaryInwardsProcessedItem));
					exactMatch.Quantity = 0;
				}
				else
				{
					var quantityRemaining = quantity;
					foreach (var match in matchingInventory.Where(i => i.Quantity != 0).OrderBy(i => i.Quantity))
					{
						if (match.Quantity <= quantityRemaining)
						{
							result.Add(new OrderLineDetails(match.Location, match.Quantity, match.IsMainInwardsProcessedItem, match.IsSecondaryInwardsProcessedItem));
							quantityRemaining -= match.Quantity;
							match.Quantity = 0;
						}
						else
						{
							result.Add(new OrderLineDetails(match.Location, quantityRemaining, match.IsMainInwardsProcessedItem, match.IsSecondaryInwardsProcessedItem));
							match.Quantity -= quantityRemaining;
							quantityRemaining = 0;
						}

						if (quantityRemaining == 0)
						{
							break;
						}
					}

					if (quantityRemaining > 0)
					{
						throw new DataObjectReadFailureException("Cannot do an amendment. Stock related properties affected.");
					}
				}
			}

			return result;
		}

		#endregion

		#region LocationAndQuantity

		public class OrderLineDetails
		{
			public OrderLineDetails(WhsLocation location, ZDecimal quantity, ZBool isMainInwardsProcessedItem, ZBool isSecondaryInwardsProcessedItem)
			{
				Location = location;
				Quantity = quantity;

				IsMainInwardsProcessedItem = isMainInwardsProcessedItem;
				IsSecondaryInwardsProcessedItem = isSecondaryInwardsProcessedItem;
			}

			public readonly WhsLocation Location;
			public readonly ZDecimal Quantity;

			public readonly ZBool IsMainInwardsProcessedItem;
			public readonly ZBool IsSecondaryInwardsProcessedItem;
		}

		#endregion

		#region PopulateDocketLineDataObjectFromInvoiceLineAndPackDetails

		protected override void PopulateDocketLineDataObjectFromInvoiceLineAndPackDetails(OrderLine docketLineDataObject, IWarehouseCustomsLineDetails warehouseCustomsLineDetails, IWarehouseCustomsLinePackDetails packDetail)
		{
			base.PopulateDocketLineDataObjectFromInvoiceLineAndPackDetails(docketLineDataObject, warehouseCustomsLineDetails, packDetail);

			if (packDetail != null)
			{
				docketLineDataObject.OrderedQty = packDetail.PackedQty;
			}

			if (packDetail != null && packDetail.PackageQty > 0 && packDetail.PackageQty <= packDetail.PackedQty)
			{
				docketLineDataObject.PackageGroupId = packDetail.PackID;
				docketLineDataObject.PerPackageQty = packDetail.PackedQty / packDetail.PackageQty;
			}
		}

		#endregion

		#region GetProductOwner

		protected override OrgHeader GetProductOwner(OrgHeader client)
		{
			return OrderForChangeOfInventory?.Client ?? base.GetProductOwner(client);
		}

		#endregion

		#region ShouldDeleteUnmatchedDocketLines

		protected override bool ShouldDeleteUnmatchedDocketLines(WhsReceive receive, IEnumerable<OrderLine> lines)
		{
			var shouldDeleteUnmatchedLines = IsOrderLineCollectionContentNullOrComplete;
			if (shouldDeleteUnmatchedLines
				&& (receive.StartedReceiving || receive.Lines.Cast<WhsReceiveLine>().Any(line => line.HasPutawayTransfer)))
			{
				var message = Res.GetString("f3d35ee8-c24f-4492-9f4c-56394579d3c9", "Cannot update Receive Line after receiving started.");
				throw new DataObjectReadFailureException(message);
			}

			return lines.Any() && shouldDeleteUnmatchedLines;
		}

		bool IsOrderLineCollectionContentNullOrComplete
			=> dataObject.Order?.OrderLineCollection?.Content == null
			|| dataObject.Order.OrderLineCollection.Content == CollectionContent.Complete;

		#endregion

		#region GetNewLineReader

		protected override DataObjectReader<OrderLine, WhsReceiveLine> GetNewLineReader(WhsReceive receive, OrderLine orderLineDataObject, IEnumerable<WhsReceiveLine> matchedLines)
		{
			return new WhsReceiveLineDataObjectReader(orderLineDataObject, logger, factory, receive, matchedLines, GetExtraOrderLineDetails(orderLineDataObject));
		}

		#endregion

		#region RemoveFromCollection

		protected override void RemoveFromCollection(WhsReceive docket, WhsReceiveLine docketLine)
		{
			if (docketLine.IsInDatabase && docket.Warehouse.WW_IsVirtualWarehouse)
			{
				// remove all Hold Code Change Logs in the DB, we don't need to keep them for virtual warehouse amendment.
				var query = new ZDBOnlyQuery(typeof(WhsInventoryHoldChangeLog));
				query.AddToFilter(WhsInventoryHoldChangeLogSchema.WHL_WE_ParentDocketLine, docketLine.PK);

				var holdCodeLogs = factory.RowFactory.Load(WhsInventoryHoldChangeLogSchema.Constants.TableName, query);
				holdCodeLogs.ForEach(d => factory.DeleteRowAndSetHasChanges<WhsInventoryHoldChangeLog>((IColumnIndexer)d, WhsInventoryHoldChangeLogSchema.PK));
			}

			base.RemoveFromCollection(docket, docketLine);
		}

		#endregion

		#endregion

		#region PopulateDates

		void PopulateDates(WhsReceive receive)
		{
			var hasEstimatedArrivalDate = false;
			var dataObjectHelper = new WhsDataObjectReaderHelper(receive.Warehouse);
			var dateCollection = IsDataSourceForwardingShipment ? dataObject.DateCollection : dataObject.Order?.DateCollection;
			if (dateCollection != null)
			{
				foreach (var dateDataObject in dateCollection)
				{
					switch (dateDataObject.Type)
					{
						case DateType.Departure:
							SetValueIfNotReadOnly(receive, WhsDocketSchema.WD_ETD, dataObjectHelper.ConvertToZDateTimeOffset(dateDataObject.Value));
							break;

						case DateType.BookingConfirmed:
							if (dateDataObject.Value != ZDateTime.Empty)
							{
								SetValueIfNotReadOnly(receive, WhsDocketSchema.WD_BookingDate, dataObjectHelper.ConvertToZDateTimeOffset(dateDataObject.Value));
							}
							break;

						case DateType.Arrival:
							if (dateDataObject.IsEstimate.GetValueOrDefault())
							{
								SetValueIfNotReadOnly(receive, WhsDocketSchema.WD_ETA, dataObjectHelper.ConvertToZDateTimeOffset(dateDataObject.Value));
								hasEstimatedArrivalDate = true;
							}
							else
							{
								SetValueIfNotReadOnly(receive, WhsDocketSchema.WD_ArrivalDate, dataObjectHelper.ConvertToZDateTimeOffset(dateDataObject.Value));
							}
							break;
					}
				}
			}

			if (receive.WD_ETD.IsEmpty && dataObject.DateCollection != null)
			{
				var requiredExWorks = dataObject.DateCollection.FirstOrDefault(o => o.Type == DateType.ExWorksRequiredBy);
				if (requiredExWorks != null)
				{
					receive.WD_ETD = dataObjectHelper.ConvertToZDateTimeOffset(requiredExWorks.Value).GetValueOrDefault();
				}
			}

			if (!hasEstimatedArrivalDate && dataObject.LocalProcessing != null && dataObject.LocalProcessing.DeliveryRequiredBy.HasValue)
			{
				SetValueIfNotReadOnly(receive, WhsDocketSchema.WD_ETA, dataObjectHelper.ConvertToZDateTimeOffset(dataObject.LocalProcessing.DeliveryRequiredBy.Value));
			}
		}

		#endregion

		#region PopulateAdditionalAddresses

		protected override bool PopulateAdditionalAddresses(OrganizationAddress organizationDataObject, WhsReceive receive)
		{
			bool result = base.PopulateAdditionalAddresses(organizationDataObject, receive);
			if (!result && IsAddressTypeNotPresentInAddressCollection(DocAddressType.SupplierDocumentaryAddress)
				&& organizationDataObject.AddressType.GetValueOrDefault().EqualsIgnoringCase(nameof(DocAddressType.ConsignorDocumentaryAddress)))
			{
				var jobDocAddress = new OrganisationDataObjectReader(organizationDataObject, logger, factory).GetMatchedOrNew(receive, DocAddressType.SupplierDocumentaryAddress);
				if (jobDocAddress != null)
				{
					result = true;
				}
			}
			else if (!result && IsAddressTypeNotPresentInAddressCollection(TransportCoConstants.AddressType)
				&& organizationDataObject.AddressType.GetValueOrDefault().EqualsIgnoringCase(AddressTypes.DeliveryLocalCartage))
			{
				var jobDocAddress = new OrganisationDataObjectReader(organizationDataObject, logger, factory).GetMatchedOrNew(receive, TransportCoConstants.AddressType);
				if (jobDocAddress != null)
				{
					result = true;
				}
			}

			return result;
		}

		#endregion

		#endregion

		#region ValidateDataObjectFieldsUseWesternEuropeanOnly

		protected override void ValidateDataObjectFieldsUseWesternEuropeanOnlyCore()
		{
			base.ValidateDataObjectFieldsUseWesternEuropeanOnlyCore();

			if ((dataObject.Order == null || dataObject.Order.ClientReference.GetValueOrDefault().IsEmpty)
				&& !dataObject.OwnerRef.GetValueOrDefault().IsWesternEuropeanOrEmpty)
			{
				ThrowErrorForInvalidCharactersInField(nameof(dataObject.OwnerRef));
			}
		}

		#endregion

		#endregion

		#region ClientDescriptionForErrorMessage

		protected override ZString ClientDescriptionForErrorMessage => CustomsHelper?.IsWarehouseBondedChangeOfOwnership ?? false ? (ZString)Res.GetString("WhsReceiveDataObjectReader|ChangeOfOwnership|OwnerDescription", "New Owner") : base.ClientDescriptionForErrorMessage;

		#endregion

		#region ImportStrategy

		protected override WhsImportStrategy GetNewImportStrategyCore()
		{
			return new ReceiveImportStrategy(this);
		}

		protected override ImportFromCustomsStrategy GetNewCustomsImportStrategy()
		{
			return new ReceiveImportFromCustomsStrategy(this);
		}

		#region ReceiveImportStrategy

		class ReceiveImportStrategy : ImportFromOrderAndReceiveStrategy
		{
			public ReceiveImportStrategy(WhsReceiveDataObjectReader reader)
				: base(reader)
			{
			}

			protected new WhsReceiveDataObjectReader Reader
			{
				get { return (WhsReceiveDataObjectReader)base.Reader; }
			}

			protected override DocAddressType ClientDocAddressTypeCore
			{
				get { return Reader.IsDataSourceOrderManager || Reader.IsDataSourceForwardingShipment ? DocAddressType.ConsigneeDocumentaryAddress : base.ClientDocAddressTypeCore; }
			}

			protected override ZString? ExternalReference => Reader.dataObject.GetMatchingDataSource(DataContextType.ForwardingShipment)?.Key ?? base.ExternalReference;

			protected override CodeDescriptionPair DocketSubTypeCore
			{
				get
				{
					var docketSubType = Reader.dataObject.Order?.Type;
					if (docketSubType != null && !new ReceiveType().ContainsCode(docketSubType.Code))
					{
						docketSubType = null;
					}

					return docketSubType;
				}
			}

			protected override ZString GetReasonForNotAbleToUpdateMatchedDocketCore(WhsReceive receive)
			{
				ZString result;

				if (receive.IsReadyForPlanningOrPlanned)
				{
					result = Res.GetString("cbeba391-bbd7-497d-9d12-d61dfc22eac1", "The Receive is Ready For Planning or Planned.");
				}
				else
				{
					result = base.GetReasonForNotAbleToUpdateMatchedDocketCore(receive);
				}

				return result;
			}
		}

		bool IsDataSourceOrderManager
		{
			get { return dataObject.GetMatchingDataSource(DataContextType.OrderManagerOrder) != null; }
		}

		bool IsDataSourceForwardingShipment
		{
			get { return dataObject.GetMatchingDataSource(DataContextType.ForwardingShipment) != null; }
		}

		#endregion

		#region ReceiveImportFromCustomsStrategy

		public class ReceiveImportFromCustomsStrategy : ImportFromCustomsStrategy
		{
			public ReceiveImportFromCustomsStrategy(WhsReceiveDataObjectReader reader)
				: base(reader)
			{
			}

			protected new WhsReceiveDataObjectReader Reader
			{
				get { return (WhsReceiveDataObjectReader)base.Reader; }
			}

			protected override string CustomsSubType
			{
				get { return ReceiveType.Codes.Customs; }
			}

			// tested in WhsBondedChangeOfOwnershipDataObjectReaderTest.TestPopulateBusinessObject_CreateFinalisedOrderAndReceive
			protected override OrganizationAddress ClientOrganizationAddressCore
			{
				get
				{
					return IsWarehouseBondedChangeOfOwnership
						? Reader.dataObject.GetWarehouseCustomsDetailsChangeOfOwnership(Reader.logger.TopLevelDataContext)?.NewOwner
						: base.ClientOrganizationAddressCore;
				}
			}

			protected override OrganizationAddress GetRelatedWarehouseAddressCore()
			{
				OrganizationAddress newWarehouseAddress = null;
				if (IsWarehouseBondedChangeOfOwnership)
				{
					newWarehouseAddress = NewWarehouseAddressForChangeOfOwnership;
				}
				else if (IsWarehouseBondedChangeOfRegime)
				{
					newWarehouseAddress = NewWarehouseAddressForChangeOfRegime;
				}

				return newWarehouseAddress ?? base.GetRelatedWarehouseAddressCore();
			}

			protected override bool IsChangeOfRegimeInwardProcessingJobForDocket()
			{
				return Reader.dataObject.GetWarehouseCustomsDetailsChangeOfRegime(Reader.logger.TopLevelDataContext)?.IntoRegimeType == CustomsRegime.InwardProcessing;
			}

			// change of inventory tested in WhsBondedChangeOfInventoryDataObjectReader
			protected override bool CanUpdateDocketLinesCore(WhsReceive docket)
			{
				return base.CanUpdateDocketLinesCore(docket) || docket.Warehouse.WW_IsVirtualWarehouse || IsWarehouseBondedChangeOfInventory;
			}

			// change of inventory tested in WhsBondedChangeOfInventoryDataObjectReader
			protected override bool ShouldAmendmentCancelOutDocket(WhsReceive receive) => (Reader.IsCheckingIfAmendmendIsValid || !Reader.logger.TopLevelDataContext.ContainsHoldCode()) && !IsWarehouseBondedChangeOfInventory && receive.IsUSBonded;

			// change of inventory tested in WhsBondedChangeOfInventoryDataObjectReader
			protected override bool ShouldPopulateBizOCore(UniversalShipment dataObject, WhsReceive receive)
			{
				var result = true;

				if (receive != null
						&& receive.IsInDatabase
						&& (receive.Warehouse.WW_IsVirtualWarehouse || IsWarehouseBondedChangeOfInventory)
						&& !Reader.IsCheckingIfAmendmendIsValid) // this is to prevent infinite recursion, as we going to read bizO again to check if it is a valid amendment
				{
					if (Reader.logger.TopLevelDataContext.ContainsHoldCode())
					{
						var newReader = new WhsReceiveDataObjectReader(dataObject, new WrappingLogger(Reader.logger), new UniversalObjectFactory(), Reader.OrderForChangeOfInventory);
						var receiveFromAnotherReader = newReader.ReadIntoBusinessObject();

						if (receiveFromAnotherReader.Warehouse.WW_IsVirtualWarehouse || IsWarehouseBondedChangeOfInventory)
						{
							if (ObjectFactory.Get<IWhsReceiveCustomsAmendmentChecker>().CanDoAnAmendment(receiveFromAnotherReader, IsWarehouseBondedChangeOfInventory))
							{
								// if we can do an Amendment we need to hold the stock but not save the changes
								ChangeHoldCodeOnAllLines(receive, InventoryHoldCodes.Codes.Held);
								Reader.logger.Log(LogType.Information, "Amendment allowed - no immediate changes made, but relevant stock put on hold.");
								result = false;
							}
							else
							{
								throw new DataObjectReadFailureException("Cannot do an amendment. Stock related properties affected.");
							}
						}
					}
					else
					{
						// we unhold stock so the import can proceed
						ChangeHoldCodeOnAllLines(receive, string.Empty);
					}
				}
				return result;
			}

			static void ChangeHoldCodeOnAllLines(WhsReceive receive, string heldCode)
			{
				foreach (var line in receive.Lines)
				{
					line.HeldCodeChangeQuantity = line.WE_TransactionQuantity; // this is important to avoid splitting transaction line!
					line.HeldCodeToChangeTo = heldCode;
					line.ChangeInventoryHeldCode(true);
				}
			}

			protected override ZString FinalisedCannotUpdateMessage(ZString? customsJobNo)
			{
				return new ZString(Res.GetString("c0992bb2-8ef5-4569-8dae-0234f18f177d",
						"Warehouse Receipt could not be amended for Customs Job {0} because it is already finalized.", customsJobNo));
			}

			protected override ZString GetReasonForNotAbleToUpdateMatchedDocketCore(WhsReceive receive)
			{
				ZString result;

				if (receive.IsReadyForPlanningOrPlanned)
				{
					result = Res.GetString("cbeba391-bbd7-497d-9d12-d61dfc22eac1", "The Receive is Ready For Planning or Planned.");
				}
				else
				{
					result = base.GetReasonForNotAbleToUpdateMatchedDocketCore(receive);
				}

				return result;
			}

			#region AfterPopulate

			// change of ownership tested in WhsBondedChangeOfOwnershipDataObjectReader
			protected override void AfterPopulateCore(WhsReceive docket)
			{
				bool isVirtualWarehouseOrChangeOfInventory = docket.Warehouse.WW_IsVirtualWarehouse || IsWarehouseBondedChangeOfInventory;
				if (isVirtualWarehouseOrChangeOfInventory && docket.IsInDatabase) // If we are amending existing receive for virtual warehouse
				{
					// this is a check when we don't have HLD code and are trying to apply an amendment
					if (!Reader.logger.TopLevelDataContext.ContainsHoldCode()
						&& !ObjectFactory.Get<IWhsReceiveCustomsAmendmentChecker>().CanDoAnAmendment(docket, IsWarehouseBondedChangeOfInventory))
					{
						throw new DataObjectReadFailureException("Cannot do an amendment. Stock related properties affected.");
					}

					ChangeDocketStatusIfDocketIsFinalised(docket);
				}
				else
				{
					base.AfterPopulateCore(docket);
				}
			}

			void ChangeDocketStatusIfDocketIsFinalised(WhsReceive receive)
			{
				if (receive.IsFinalised && receive.Lines.Any(l => !l.IsFinalised))
				{
					var originalFinaliseDate = receive.WD_FinalisedDate;

					// need to clear line status to be "Entered", in receive.FinaliseDocketCore try to set WE_OriginalInventoryStatus based on WE_WHC_NKOriginalInventoryHeldCode regardless if line status is Finalised or not
					// and in the setter of WE_OriginalInventoryStatus will throw exception if try to modify value for a Finalised line
					SetReceiveAndLinesStatus(receive, DocketStatus.Codes.Putaway, ZString.Empty);

					// Since docket.IsFinalised now checks WD_FinalisedDate. WD_FinalisedDate needs to be cleared too, otherwise re-finalisation will not occur.
					SetReceiveAndLinesFinalisedDate(receive, ZDateTimeOffset.Empty);

					ReceiveAllocationHelper.AllocateLocations(receive, receive.Warehouse);
					FinaliseDocket(receive);

					SetReceiveAndLinesFinalisedDate(receive, originalFinaliseDate);
				}
			}

			void SetReceiveAndLinesStatus(WhsReceive receive, string docketStatus, string docketLineStatus)
			{
				Reader.SetValue(GetColumnIndexerFromRow(receive), WhsDocketSchema.WD_DocketStatus, docketStatus);
				foreach (var line in receive.Lines)
				{
					Reader.SetValue(GetColumnIndexerFromRow(line), WhsDocketLineSchema.WE_DocketLineStatus, docketLineStatus);
				}
			}

			void SetReceiveAndLinesFinalisedDate(WhsReceive receive, ZDateTimeOffset finaliseDate)
			{
				Reader.SetValue(GetColumnIndexerFromRow(receive), WhsDocketSchema.WD_FinalisedDate, finaliseDate);
				foreach (var line in receive.Lines)
				{
					Reader.SetValue(GetColumnIndexerFromRow(line), WhsDocketLineSchema.WE_FinalisedDate, finaliseDate);
				}
			}

			protected override void AfterFinaliseDocket(WhsReceive docket)
			{
				base.AfterFinaliseDocket(docket);
				if (!docket.IsInDatabase && Reader.logger.TopLevelDataContext.ContainsHoldCode())
				{
					ChangeHoldCodeOnAllLines(docket, InventoryHoldCodes.Codes.Held);
				}
			}

			protected override void AfterPopulateWhenRealWarehouse(WhsReceive docket)
			{
				base.AfterPopulateWhenRealWarehouse(docket);

				if (IsWarehouseBondedChangeOfInventory && !docket.WD_ArrivalDate.IsValid)
				{
					docket.WD_ArrivalDate = docket.Warehouse.GetWarehouseBranchDateTimeOffset(ZDateTime.UtcNow);
				}
			}

			protected override void AfterPopulateWhenVirtualWarehouse(WhsReceive receive, IEnumerable<WhsInventoryView> inventory = null)
			{
				base.AfterPopulateWhenVirtualWarehouse(receive);
				ReceiveAllocationHelper.AllocateLocations(receive, receive.Warehouse); // default all stock to the first location (i.e. A-1);

				if (receive.Warehouse.WW_IsVirtualWarehouse && !receive.WD_ArrivalDate.IsValid)
				{
					receive.WD_ArrivalDate = receive.Warehouse.GetWarehouseBranchDateTimeOffset(ZDateTime.UtcNow);
				}
			}

			#endregion
		}

		#endregion

		#region CustomsHelper

		protected override CustomsDataSourceHelper<WhsReceive> GetNewCustomsDataSourceHelper()
		{
			return new CustomsDataSourceHelperForReceive(dataObject, logger.TopLevelDataContext);
		}

		#endregion

		#endregion

		#region GetValidWarehouse

		protected override WarehouseImportResult GetValidWarehouse()
		{
			WarehouseImportResult result;
			var destinationWarehouseAddressData = dataObject.OrganizationAddressCollection?.FirstOrDefault(nameof(DocAddressType.DestinationWarehouse));
			if (destinationWarehouseAddressData != null)
			{
				result = GetWarehouseFromDestinationWarehouseAddress(destinationWarehouseAddressData);
			}
			else
			{
				result = base.GetValidWarehouse();
			}

			return result;
		}

		WarehouseImportResult GetWarehouseFromDestinationWarehouseAddress(OrganizationAddress destinationWarehouseAddress)
		{
			WhsWarehouse warehouse = null;

			var warehouseAddress = new OrganisationDataObjectReader(destinationWarehouseAddress, logger, factory).GetMatched();
			if (warehouseAddress != null)
			{
				warehouse = factory.LoadTop1<WhsWarehouse>(GetWarehouseAddressQuery(warehouseAddress.PK));
			}

			return new WarehouseImportResult(warehouse != null, warehouse);
		}

		#endregion
	}
}
