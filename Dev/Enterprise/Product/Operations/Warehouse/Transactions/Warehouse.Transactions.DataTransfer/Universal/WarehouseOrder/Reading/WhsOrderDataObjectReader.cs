using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Integration.CodeLists;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;
using BusinessCodeLists = Enterprise.Warehouse.Transactions.CodeLists;
using CustomsDataTransferConstants = Enterprise.Customs.DataTransfer.Universal.Constants;
using UniversalCodeDescriptionPair = Enterprise.UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal
{
	public class WhsOrderDataObjectReader : WhsOrderAndReceiveDataObjectReader<WhsOrder, WhsOrderLine>
	{
		public WhsOrderDataObjectReader(UniversalShipment whsOrderDataObject, IXmlImportLogger logger, UniversalObjectFactory factory)
			: base(whsOrderDataObject, logger, factory)
		{
		}

		#region Matching Job

		protected override string DocketType => OrderDocketType;
		internal static string OrderDocketType => Res.GetString("9d59b371-7926-4c31-923c-a399a632e052", "Order");

		protected override string DocketTypeCode => BusinessCodeLists.DocketType.Codes.Order;

		public override DataContextType DataContextType => DataContextType.WarehouseOrder;

		#endregion

		#region Matching References (fallbacks)

		protected override WhsOrderAndReceiveLastResortMatcher<WhsOrder> GetMatcher(WhsOrderAndReceiveReferences references)
			=> new WhsOrderMatcher(factory.BOFactory, references, logger);

		#endregion

		protected override bool PopulateJobDocAddressIsResidential => true;

		// Create / Update

		#region Create / Update Job

		protected override void PopulateBusinessObjectCore(WhsOrder order)
		{
			base.PopulateBusinessObjectCore(order);

			var orderDataObject = dataObject.Order;
			if (orderDataObject != null)
			{
				var crossDockLocation = order.Warehouse.FindLocation(orderDataObject.StagingArea.GetValueOrDefault());
				if (crossDockLocation != null)
				{
					order.WD_WL_CrossDock = crossDockLocation.PK;
				}

				var status = orderDataObject.Status.GetCodeAsUpperCase();
				if (status == DocketStatus.Codes.Held)
				{
					if (IsNewBO || order.WD_DocketStatus == DocketStatus.Codes.Entered)
					{
						order.WD_DocketStatus = DocketStatus.Codes.Held;
					}
				}
				else if (IsNewBO)
				{
					order.WD_DocketStatus = DocketStatus.Codes.Entered;
				}

				SetValue(order, WhsDocketSchema.WD_LocalCartInsuranceCost, orderDataObject.LocalCartageInsuranceValue);
				SetValueIfNotReadOnly(order, WhsDocketSchema.WD_PickOption, order.Lookups.PickOptions, orderDataObject.PickOption);
				SetValueIfNotReadOnly(order, WhsDocketSchema.WD_WhsOrderFulfillmentRule, order.Lookups.WhsOrderFulfillmentRules, orderDataObject.FulfillmentRule);
				SetValue(order, WhsDocketSchema.WD_PackingAfterPickingRequired, orderDataObject.RequiresPacking);
				PopulatePickPriority(order, orderDataObject);

				if (orderDataObject.UnitsSent.HasValue)
				{
					if (order.WD_WP.IsValid)
					{
						SetValue(order, WhsDocketSchema.WD_UnitsSent, (ZDecimal)orderDataObject.UnitsSent);
					}
					else
					{
						logger.Log(LogType.Warning, Res.GetString("a57b5b20-589d-4155-a6f6-4190dfb59d88", "Element '{0}' with value '{1}' was ignored as the Order is not allocated.", nameof(orderDataObject.UnitsSent), orderDataObject.UnitsSent.Value));
					}
				}

				SetValue(order, WhsDocketSchema.WD_QualityAuditRequired, orderDataObject.RequiresQualityAudit);
				SetValue(order, WhsDocketSchema.WD_ExcludeFromTotePicking, orderDataObject.ExcludeFromTotePicking);
				PopulateSalesChannnel(order, orderDataObject);
				SetValue(order, WhsDocketSchema.WD_UseDirectedPackingConsolidation, orderDataObject.UseDirectedPackingConsolidation ?? order.ClientPickingParams?.WPP_UseDirectedPackingConsolidation);
			}

			SetValue(order, WhsDocketSchema.WD_ContainerMode, dataObject.ContainerMode);
			SetValue(order, WhsDocketSchema.WD_TransportMode, ImportStrategy.TransportMode);
			SetValue(order, WhsDocketSchema.WD_GoodsDescription, dataObject.GoodsDescription);
			SetValue(order, WhsDocketSchema.WD_CODPayMethod, dataObject.ShipperCODPayMethod);
			SetValue(order, WhsDocketSchema.WD_ShipperCODAmount, dataObject.ShipperCODAmount);
			SetValue(order, WhsDocketSchema.WD_INCO, dataObject.ShipmentIncoTerm);

			// IsAuthorisedToLeave
			SetValue(order, WhsDocketSchema.WD_IsAuthorisedToLeave, dataObject.IsAuthorizedToLeave);

			PopulateRelatedEntities(order);
		}

		void PopulateSalesChannnel(WhsOrder order, Order orderDataObject)
		{
			var salesChannel = orderDataObject.SalesChannel;
			if (salesChannel != null)
			{
				var salesChannelCode = salesChannel.Code;
				if (string.IsNullOrEmpty(salesChannelCode))
				{
					SetValue(order, WhsDocketSchema.WD_WSH_SalesChannel, ZGuid.Empty);
				}
				else
				{
					var salesChannelPK = factory.LoadFromUniqueKey<WhsSalesChannel>(WhsSalesChannelSchema.WSH_Code, salesChannelCode)?.PK;
					if (salesChannelPK == null)
					{
						var errorMessage = Res.GetString("e860f4a4-3156-4eeb-8eb0-242f294cdd75",
							"Cannot Import Warehouse Order Job -- as the provided Sales Channel Code ({0}) does not match any known Sales Channels. Please check value or add the required one.", salesChannelCode);
						throw new DataObjectReadFailureException(errorMessage);
					}
					SetValue(order, WhsDocketSchema.WD_WSH_SalesChannel, salesChannelPK);
				}
			}
		}

		protected override void CalculateTotalsIfNeededCore(WhsOrder order)
		{
			var lines = new Lazy<IEnumerable<ILineWithProductAndQuantity>>(() => order.Lines.Where(l => l.SupplierPart != null).ToProductAndQuantities().ToArray());

			if (dataObject.Order != null && (dataObject.Order.TotalLineVolume == null || dataObject.Order.TotalLineVolume == 0))
			{
				var volumeMeasure = order.GetVolumeMeasure();
				var totalVolume = UnitOfMeasureConverter.GetTotalQuantityFromLines(order, volumeMeasure, lines.Value);
				if (totalVolume > 0)
				{
					SetValue(order, WhsDocketSchema.WD_TotalCubic, totalVolume);
				}
			}

			if (dataObject.Order != null && (dataObject.Order.TotalLineWeight == null || dataObject.Order.TotalLineWeight == 0))
			{
				var weightMeasure = order.GetWeightMeasure();
				var totalWeight = UnitOfMeasureConverter.GetTotalQuantityFromLines(order, weightMeasure, lines.Value);
				if (totalWeight > 0)
				{
					SetValue(order, WhsDocketSchema.WD_TotalWeight, totalWeight);
				}
			}
		}

		protected override IEnumerable<IWarehouseCustomsLineAllocationInfo> GetAllocationKeyDetailsFromCustomsLineDetails(IWarehouseCustomsLineDetails warehouseCustomsLineDetails)
			=> warehouseCustomsLineDetails.AllocationInfos;

		#region LinkParent

		protected override IEnumerable<DataContextType> ParentDataContextTypes
		{
			get
			{
				yield return DataContextType.ForwardingShipment;
				yield return DataContextType.CustomsDeclaration;
				yield return DataContextType.WarehouseCustomsEntry;
				yield return DataContextType.NctsHeader;
			}
		}

		#endregion

		#region PopulateRelatedEntities

		void PopulateRelatedEntities(WhsOrder order)
		{
			if (dataObject.LocalProcessing != null)
			{
				var dataObjectHelper = new WhsDataObjectReaderHelper(order.Warehouse);
				SetValueIfNotReadOnly(order, WhsDocketSchema.WD_RequiredDate, dataObjectHelper.ConvertToZDateTimeOffset(dataObject.LocalProcessing.DeliveryRequiredBy));
			}

			// fallbacks

			if ((dataObject.Order == null || dataObject.Order.ClientReference.GetValueOrDefault().IsEmpty) && !dataObject.BookingConfirmationReference.GetValueOrDefault().IsEmpty)
			{
				SetValue(order, WhsDocketSchema.WD_CustomerReference, dataObject.BookingConfirmationReference);
			}
			else
			{
				// for customs outwards entry #
				var outwardsNumber = CustomsHelper.IsDataSourceCustoms ? dataObject.GetWarehouseCustomsOutwardEntryNumber(logger.TopLevelDataContext) : null;
				if (outwardsNumber.HasValue)
				{
					SetValue(order, WhsDocketSchema.WD_CustomerReference, outwardsNumber);
				}
			}

			if (ObjectFactory.Get<IAccounting>().ShouldAddJobInvoicingRecordAtSavingOrEditingOfOperationsJob(order))
			{
				new JobHeader.Loader(order).TryLoadOrCreateWithMutex();
			}
		}

		protected override DataObjectReader<OrderLine, WhsOrderLine> GetNewLineReader(WhsOrder order, OrderLine orderLineDataObject, IEnumerable<WhsOrderLine> matchedLines)
		{
			return new WhsOrderLineDataObjectReader(orderLineDataObject, logger, factory, order, matchedLines, GetExtraOrderLineDetails(orderLineDataObject));
		}

		protected override bool ShouldDeleteUnmatchedDocketLines(WhsOrder order, IEnumerable<OrderLine> lines)
		{
			// --> For Customs Imports for Real Whs, we pick the order and hold it via adding the HoldTheWarehouseOrder Event,
			// thus the Docket status will be picking, however we need to clear existing lines on picked Orders for Customs.
			return CustomsHelper.IsDataSourceCustoms || (!order.IsAttachedToPickButNotFinalised && dataObject.Order?.OrderLineCollection?.Content != CollectionContent.Partial);
		}

		protected override DataObjectList<AdditionalReference> GetAdditionalReferencesFromImport()
		{
			var result = base.GetAdditionalReferencesFromImport();

			if (!dataObject.WayBillNumber.GetValueOrDefault().IsEmpty
				&& (result == null || !result.Any(o => o.Type.GetCodeAsUpperCase() == WarehouseAdditionalReferenceTypes.Codes.HouseBill)))
			{
				result = new DataObjectList<AdditionalReference>(result);
				result.Add(new AdditionalReference { Type = new EntryType { Code = WarehouseAdditionalReferenceTypes.Codes.HouseBill }, ReferenceNumber = dataObject.WayBillNumber });
			}

			return result;
		}

		#endregion

		#region PopulateAdditionalAddresses

		protected override IEnumerable<ZString> AddressTypesToIgnoreWhenAddingToDocketDocAddressCollectionCore
		{
			get
			{
				var result = base.AddressTypesToIgnoreWhenAddingToDocketDocAddressCollectionCore;

				if (ImportStrategy.ClientDocAddressType != DocAddressType.ConsignorDocumentaryAddress)
				{
					result = result.Concat(new ZString[] { nameof(DocAddressType.ConsignorDocumentaryAddress) });
				}

				return result;
			}
		}

		protected override bool PopulateAdditionalAddresses(OrganizationAddress organizationDataObject, WhsOrder order)
		{
			bool result = base.PopulateAdditionalAddresses(organizationDataObject, order);
			if (!result)
			{
				var addressType = organizationDataObject.AddressType.GetValueOrDefault();
				if (addressType == nameof(DocAddressType.SendingForwarderAddress))
				{
					var orgReader = new OrganisationDataObjectReader(organizationDataObject, logger, factory);
					var forwarderAddress = orgReader.GetMatched();
					if (forwarderAddress != null)
					{
						SetValue(order, WhsDocketSchema.WD_OH_Forwarder, forwarderAddress.OA_OH);
						result = true;
					}
				}
				else if (addressType == nameof(DocAddressType.ConsigneeAddress))
				{
					var orgReader = new OrganisationDataObjectReader(organizationDataObject, logger, factory)
					{
						PopulateIsResidential = PopulateJobDocAddressIsResidential
					};
					var consigneeAddress = orgReader.GetMatchedOrNew(order, OrganisationTypes.Consignee);
					if (consigneeAddress != null)
					{
						order.DocAddresses.Add(consigneeAddress);
						result = true;
					}
				}
				else if (IsAddressTypeNotPresentInAddressCollection(DocAddressType.ConsigneeAddress)
					&& addressType == nameof(DocAddressType.ConsigneeDocumentaryAddress))
				{
					var orgReader = new OrganisationDataObjectReader(organizationDataObject, logger, factory)
					{
						PopulateIsResidential = PopulateJobDocAddressIsResidential
					};
					var consigneeAddress = orgReader.GetMatchedOrNew(order, OrganisationTypes.Consignee, null, DocAddressType.ConsigneeAddress);
					if (consigneeAddress != null)
					{
						order.DocAddresses.Add(consigneeAddress);
						result = true;
					}
				}
				else if (IsDataSourceForwardingShipment
					&& IsAddressTypeNotPresentInAddressCollection(DocAddressType.DropOffAddress)
					&& addressType == nameof(DocAddressType.DepartureCFSAddress))
				{
					var orgReader = new OrganisationDataObjectReader(organizationDataObject, logger, factory)
					{
						PopulateIsResidential = PopulateJobDocAddressIsResidential
					};
					var dropOffAddress = orgReader.GetMatchedOrNew(order, OrganisationTypes.None, null, DocAddressType.DropOffAddress);
					if (dropOffAddress != null)
					{
						order.DocAddresses.Add(dropOffAddress);
						result = true;
					}
				}
				else if (IsDataSourceForwardingShipment
					&& IsAddressTypeNotPresentInAddressCollection(TransportCoConstants.AddressType)
					&& addressType == AddressTypes.PickupLocalCartage)
				{
					var orgReader = new OrganisationDataObjectReader(organizationDataObject, logger, factory)
					{
						PopulateIsResidential = PopulateJobDocAddressIsResidential
					};
					var transportCompanyDocumentaryAddress = orgReader.GetMatchedOrNew(order, OrganisationTypes.Carrier, null, TransportCoConstants.AddressType);
					if (transportCompanyDocumentaryAddress != null)
					{
						order.DocAddresses.Add(transportCompanyDocumentaryAddress);
						result = true;
					}
				}
			}

			return result;
		}

		protected override void AfterPopulateAddressesCore(WhsOrder order)
		{
			var consigneeAddress = order.LoadJobDocAddressQuickly(DocAddressType.ConsigneeAddress);
			var carrierAddress = order.LoadJobDocAddressQuickly(DocAddressType.TransportCompanyDocumentaryAddress);
			if ((consigneeAddress != null && consigneeAddress.HasChanges) || (carrierAddress != null && carrierAddress.HasChanges))
			{
				order.SetRateTransportZone();
			}
		}

		bool IsDataSourceForwardingShipment => dataObject.GetMatchingDataSource(DataContextType.ForwardingShipment) != null;

		#endregion

		#region PopulateBizOImportSuspender

		protected override void PopulateBizOImportSuspender(WhsOrder import, bool isImportingData)
		{
			base.PopulateBizOImportSuspender(import, isImportingData);
			((IBuyerSupplierRelationshipConsumer)import).IsSettingDefaultValues = isImportingData;
		}

		#endregion

		#region CanChangeWarehouse

		protected override bool CheckCanChangeWarehouse(WhsOrder docket) => !docket.WD_WL_CrossDockInfo.OriginalValue.IsValid && !docket.HasReservedStock;

		#endregion

		#region ValidateDataObjectFieldsUseWesternEuropeanOnly

		protected override void ValidateDataObjectFieldsUseWesternEuropeanOnlyCore()
		{
			base.ValidateDataObjectFieldsUseWesternEuropeanOnlyCore();

			if ((dataObject.Order == null || dataObject.Order.ClientReference.GetValueOrDefault().IsEmpty)
				&& !dataObject.BookingConfirmationReference.GetValueOrDefault().IsWesternEuropeanOrEmpty)
			{
				ThrowErrorForInvalidCharactersInField(nameof(dataObject.BookingConfirmationReference));
			}
		}

		#endregion

		#endregion

		#region ImportStrategy

		protected override WhsImportStrategy GetNewImportStrategyCore() => new OrderImportStrategy(this);

		protected override ImportFromCustomsStrategy GetNewCustomsImportStrategy() => new OrderImportFromCustomsStrategy(this);

		protected override string CannotUpdateLinesWarningMessage => Res.GetString("24fe0531-2147-4dd2-b63c-90ff8400ed81", "Cannot update {0} Lines on a Finalized or In Picking {0}.", DocketType);

		#region OrderImportStrategy

		class OrderImportStrategy : ImportFromOrderAndReceiveStrategy
		{
			public OrderImportStrategy(WhsOrderDataObjectReader reader)
				: base(reader)
			{
			}

			protected new WhsOrderDataObjectReader Reader
			{
				get { return (WhsOrderDataObjectReader)base.Reader; }
			}

			// this is not ideal but MC decided 'for now'..
			protected override WhsWarehouse GetRelatedWarehouseCore(WhsOrder order)
			{
				var result = base.GetRelatedWarehouseCore(order);
				if (result == null && order.WD_OH_Client.IsValid)
				{
					var clientParams = WhsClientParams.GetClientParams(order.Client);
					var firstClientParameterByWarehouse = clientParams.ClientParametersByWarehouse.Count > 0 ? clientParams.ClientParametersByWarehouse[0] : null;
					if (firstClientParameterByWarehouse != null)
					{
						result = firstClientParameterByWarehouse.Warehouse;
					}
					else
					{
						var query = new ZQuery();
						query.AddToFilter(WhsWarehouseSchema.WW_WarehouseType, new[] { WarehouseTypes.Codes.Product, WarehouseTypes.Codes.FreeTradeZone });
						query.AddToFilter(WhsWarehouseSchema.WW_IsActive, SQLComparisonOperator.Equal, true);
						query.OrderBy = WhsWarehouseSchema.WW_WarehouseCode.Name + OrderByClause.Descending;
						result = Reader.factory.LoadTop1<WhsWarehouse>(query);
					}
				}

				return result;
			}

			protected override bool CanUpdateDocketLinesCore(WhsOrder docket)
			{
				return base.CanUpdateDocketLinesCore(docket) && !docket.AreLinesUpdateDisabledAfterPick;
			}

			protected override ZString GetReasonForNotAbleToUpdateMatchedDocketCore(WhsOrder matchedDocket)
			{
				ZString result;

				if (matchedDocket.Pick?.IsReadyForPlanningOrPlanned ?? false)
				{
					result = Res.GetString("76ec1dd2-ba2c-4215-abd6-0aa910477dcb", "The Order's Pick is Ready For Planning or Planned.");
				}
				else
				{
					result = base.GetReasonForNotAbleToUpdateMatchedDocketCore(matchedDocket);
				}

				return result;
			}

			protected override void ValidateAfterPopulateLinesCore(WhsOrder docket)
			{
				base.ValidateAfterPopulateLinesCore(docket);

				var lines = docket.Lines.ToArray();
				var isOrderingAvailableInventory = lines.Any(l => l.WE_WHC_NKOrderedHeldCode.IsEmpty);
				var isOrderingHeldInventory = lines.Any(l => !l.WE_WHC_NKOrderedHeldCode.IsEmpty);

				if (isOrderingAvailableInventory && isOrderingHeldInventory)
				{
					var errorMessage = Res.GetString("7B3DD7F9-C6F1-4D9A-8473-AB414E1C4EF7", "Cannot order both held and available inventory.");
					throw new DataObjectReadFailureException(errorMessage);
				}
			}
		}

		#endregion

		#region OrderImportFromCustomsStrategy

		public class OrderImportFromCustomsStrategy : ImportFromCustomsStrategy
		{
			public OrderImportFromCustomsStrategy(WhsOrderDataObjectReader reader)
				: base(reader)
			{
			}

			protected new WhsOrderDataObjectReader Reader => (WhsOrderDataObjectReader)base.Reader;

			protected override string CustomsSubType => OrderType.Codes.Customs;

			// tested in WhsBondedChangeOfInventoryDataObjectReaderTest.TestPopulateBusinessObject_CreateFinalisedOrderAndReceive
			protected override OrganizationAddress ClientOrganizationAddressCore
			{
				get
				{
					return IsWarehouseBondedChangeOfOwnership
						? Reader.dataObject.GetWarehouseCustomsDetailsChangeOfOwnership(Reader.logger.TopLevelDataContext)?.OldOwner
						: base.ClientOrganizationAddressCore;
				}
			}

			protected override bool ShouldAmendmentCancelOutDocket(WhsOrder order) => !IsVirtualWarehouseOrderAmendment;

			#region TransportMode

			protected override UniversalCodeDescriptionPair TransportModeCore => null;

			#endregion

			#region BeforeClearingExistingLines

			protected override void BeforeReadLinesIntoCollectionForRealWarehouse(WhsOrder order)
			{
				base.BeforeReadLinesIntoCollectionForRealWarehouse(order);

				if (!Reader.IsNewBO)
				{
					var pick = order.Pick;
					if (pick != null && !pick.IsFinalised)
					{
						pick.CancelPick();
					}
				}
			}

			#endregion

			#region IsInwardsProcessingJobForDocket

			protected override bool IsChangeOfRegimeInwardProcessingJobForDocket()
			{
				return Reader.dataObject.GetWarehouseCustomsDetailsChangeOfRegime(Reader.logger.TopLevelDataContext)?.OutOfRegimeType == CustomsRegime.InwardProcessing;
			}

			#endregion

			#region ShouldPopulateBizO

			protected override bool ShouldPopulateBizOCore(UniversalShipment dataObject, WhsOrder whsOrder)
			{
				var result = base.ShouldPopulateBizOCore(dataObject, whsOrder);
				var orderInDatabaseRealWhsAndPickFinalised = result
						&& whsOrder.IsInDatabase
						&& (whsOrder.Pick?.IsFinalised ?? false);

				if (orderInDatabaseRealWhsAndPickFinalised
						&& !Reader.IsCheckingIfAmendmendIsValid) // this is to prevent infinite recursion, as we going to read bizO again to check if it is a valid amendment
				{
					var orderLinesWithoutOEN = whsOrder.Lines.Cast<WhsOrderLine>().Where(l => l.CustomsData.WB_EntryKey.IsEmpty).ToDictionary(l => l.PK);

					var newReader = new WhsOrderDataObjectReader(dataObject, new WrappingLogger(Reader.logger), new UniversalObjectFactory());
					var orderFromAnotherReader = newReader.ReadIntoBusinessObject();

					var contextContainsHoldCode = Reader.logger.TopLevelDataContext.ContainsHoldCode();
					if (ObjectFactory.Get<IWhsOrderCustomsAmendmentChecker>().CanDoAnAmendment(orderFromAnotherReader, orderLinesWithoutOEN.Keys, contextContainsHoldCode))
					{
						if (contextContainsHoldCode)
						{
							SetCustomsClearingOnLinesWhereOutwardEntryNumberChanged(orderLinesWithoutOEN.Values, orderFromAnotherReader.Lines.Cast<WhsOrderLine>().Where(l => !l.CustomsData.WB_EntryKey.IsEmpty));
							Reader.logger.Log(LogType.Information, "Amendments allowed - No immediate changes made, but relevant order lines set for Customs Clearing.");
							result = false;
						}
						else
						{
							PerformAndLogOutwardsEntryKeyChanges(orderLinesWithoutOEN, orderFromAnotherReader);
						}
					}
					else
					{
						ThrowCannotDoAmendmentError();
					}
				}

				return result;
			}

			void SetCustomsClearingOnLinesWhereOutwardEntryNumberChanged(IEnumerable<WhsOrderLine> orderLinesWithNoOutwardsNum, IEnumerable<WhsOrderLine> linesHasOutwardsKey)
			{
				var lineHasOutwardsKeyPks = linesHasOutwardsKey.Select(l => l.PK).ToHashSet();
				foreach (var line in orderLinesWithNoOutwardsNum.Where(ol => lineHasOutwardsKeyPks.Contains(ol.PK)))
				{
					line.CustomsClearingInProgress = true;
				}
			}

			void PerformAndLogOutwardsEntryKeyChanges(Dictionary<ZGuid, WhsOrderLine> orderLinesWithoutOEN, WhsOrder orderFromAnotherReader)
			{
				var amendmentsMade = false;
				var amendmentsCleared = false;
				foreach (var updatedOrderLine in orderFromAnotherReader.Lines.Where(l => orderLinesWithoutOEN.ContainsKey(l.PK)).ToArray())
				{
					var matchingOrderLine = orderLinesWithoutOEN[updatedOrderLine.PK];
					var matchingCustomsData = matchingOrderLine.CustomsData;

					var updatedCustomsData = updatedOrderLine.CustomsData;
					var updatedEntryKeyIsEmpty = updatedCustomsData.WB_EntryKey.IsEmpty
						//Imported Entry Keys with a value of null are replaced with this Constant in Customs.
						|| updatedCustomsData.WB_EntryKey == CustomsDataTransferConstants.EntryNumberPlaceHolder;

					if (!updatedEntryKeyIsEmpty)
					{
						amendmentsMade = true;
						matchingCustomsData.WB_EntryKey = updatedCustomsData.WB_EntryKey;
						matchingCustomsData.WB_EntryLineNo = updatedCustomsData.WB_EntryLineNo;
					}
					else if (!amendmentsCleared && updatedEntryKeyIsEmpty)
					{
						amendmentsCleared = true;
					}

					matchingOrderLine.CustomsClearingInProgress = false;
				}

				if (amendmentsMade)
				{
					Reader.logger.Log(LogType.Information, "Amendments allowed - Outwards Entry Number set on relevant order lines and Customs Clearing Status removed.");
				}

				if (amendmentsCleared)
				{
					Reader.logger.Log(LogType.Information, "Amendments cleared - Relevant order lines' Customs Clearing status removed.");
				}
			}

			static void ThrowCannotDoAmendmentError() =>
				throw new DataObjectReadFailureException("Cannot do amendment - Import changed data other than providing Outwards Entry Numbers or Import attempted to mark a line that was already in progress of being Customs Cleared.");

			#endregion

			protected override ZString FinalisedCannotUpdateMessage(ZString? customsJobNo)
			{
				return new ZString(Res.GetString("f20313c1-3dda-4e32-8341-e585db94e2a5",
						"Warehouse Order could not be amended for Customs Job {0} because it is already finalized, Customs cleared or had changes after finalization that are not allowed.", customsJobNo));
			}

			protected override ZString GetReasonForNotAbleToUpdateMatchedDocketCore(WhsOrder matchedDocket)
			{
				ZString result;

				if (matchedDocket.Pick?.IsReadyForPlanningOrPlanned ?? false)
				{
					result = Res.GetString("76ec1dd2-ba2c-4215-abd6-0aa910477dcb", "The Order's Pick is Ready For Planning or Planned.");
				}
				else if (Reader.IsCheckingIfAmendmendIsValid
						|| (matchedDocket.IsFinalised && (!matchedDocket.Warehouse?.WW_IsVirtualWarehouse ?? false) && IsNotCustomsCleared(matchedDocket) && WhsCustomsHelper.CanFinaliseWhsOrderWithoutCustomsClearance(matchedDocket)))
				{
					result = ZString.Empty; // Allow update
				}
				else
				{
					result = base.GetReasonForNotAbleToUpdateMatchedDocketCore(matchedDocket);
				}

				return result;
			}

			bool IsNotCustomsCleared(WhsOrder whsOrder) => whsOrder.Lines.Any(o => o.CustomsData.WB_EntryKey.IsEmpty);

			protected override bool CanUpdateDocketLinesCore(WhsOrder docket)
			{
				return Reader.IsCheckingIfAmendmendIsValid || base.CanUpdateDocketLinesCore(docket);
			}

			#region LoadDocketFromCustomsLinesDocketNumbers

			protected override WhsOrder LoadDocketFromCustomsLinesDocketNumbersCore()
			{
				var warehouseCustomsLineDetails = Reader.dataObject.GetWarehouseCustomsLineDetails(Reader.logger.TopLevelDataContext);
				var orderNumbers = warehouseCustomsLineDetails.Select(l => l.OrderNumber).Distinct().ToArray();
				if (orderNumbers.Length > 1)
				{
					throw new DataObjectReadFailureException(Res.GetString("fa783ca9-d9a2-4bf7-b3f6-19098a94fa50",
						"Cannot Import - Import contains order-line Order Numbers that reference multiple different Warehouse Orders."));
				}

				WhsOrder whsOrder = null;
				if (orderNumbers.Length > 0 && !string.IsNullOrEmpty(orderNumbers.Single()))
				{
					var query = new ZQuery();
					query.AddToFilter(WhsDocketSchema.WD_ExternalReference, orderNumbers.Single());
					query.AddToFilter(WhsDocketSchema.WD_DocketType, Reader.DocketTypeCode);
					query.AddToFilter(WhsDocketSchema.WD_OH_Client, Reader.ClientOrganisationPK);
					var whsOrders = Reader.factory.Load<WhsOrder>(query);

					if (whsOrders.Length > 1)
					{
						throw new DataObjectReadFailureException(Res.GetString("e612ce02-ef00-4be2-9afe-2819d1cc5676",
							"Cannot Import - The Order Number found on the Order Lines matches multiple Warehouse Orders."));
					}

					whsOrder = whsOrders.SingleOrDefault();

					if (whsOrder != null)
					{
						IsOrderLoadedFromCustomsLinesOrderNumber = true;
						IsVirtualWarehouseOrderAmendment = whsOrder.Warehouse.WW_IsVirtualWarehouse;
					}

					if (IsOrderLoadedFromCustomsLinesOrderNumber && whsOrder.IsCancelled)
					{
						throw new DataObjectReadFailureException(Res.GetString("15786140-41cc-49e8-8e57-344be5984936",
							"Cannot Import - The Order Number found on the Order Lines is Canceled."));
					}
				}

				return whsOrder;
			}

			protected bool IsOrderLoadedFromCustomsLinesOrderNumber;

			bool IsVirtualWarehouseOrderAmendment;

			protected override void SetExternalReferenceCore(WhsOrder docket)
			{
				if (!IsOrderLoadedFromCustomsLinesOrderNumber)
				{
					base.SetExternalReferenceCore(docket);
				}
			}

			#endregion

			#region AfterPopulate

			protected override void AfterPopulateWhenRealWarehouse(WhsOrder order)
			{
				base.AfterPopulateWhenRealWarehouse(order);
				PickOrderForRealWarehouse(order);
			}

			protected override void AfterPopulateWhenVirtualWarehouse(WhsOrder order, IEnumerable<WhsInventoryView> amendedInventory)
			{
				base.AfterPopulateWhenVirtualWarehouse(order);
				PickOrderForVirtualWarehouse(order, amendedInventory);
			}

			#region PickOrder

			void PickOrderForRealWarehouse(WhsOrder order)
			{
				if (!OrderIsConnectedToFinalisedPick(order))
				{
					PickOrder(order);
				}
			}

			bool OrderIsConnectedToFinalisedPick(WhsOrder order)
			{
				if (isOrdersPickFinalisedCache == null)
				{
					isOrdersPickFinalisedCache = order.Pick?.IsFinalised ?? false;
				}

				return isOrdersPickFinalisedCache.Value;
			}
			bool? isOrdersPickFinalisedCache;

			/// <summary>
			/// The virtual warehouse pick must also include any inventory that have had their quantities amended in this transaction (quantity changes are not yet reflected in the DB).
			/// </summary>
			void PickOrderForVirtualWarehouse(WhsOrder order, IEnumerable<WhsInventoryView> amendedInventory)
			{
				if (!OrderIsConnectedToFinalisedPick(order))
				{
					PickOrder(order, amendedInventory);
				}
			}

			void PickOrder(WhsOrder order, IEnumerable<WhsInventoryView> amendedInventory = null)
			{
				SetConsigneeToClient(order);
				SetRequiredDateToTodayIfEmpty(order);
				PickOrderForBond(order, amendedInventory);
				RejectIfInShortfall(order);
				SyncOrderLineBondedAttributeWithInventory(order);
			}

			#region SetConsigneeToClient

			void SetConsigneeToClient(WhsOrder order)
			{
				if (order.Consignee == null)
				{
					order.ConsigneePK = Reader.ClientOrganisationPK;
				}
			}

			#endregion

			#region PickOrderForBond

			void PickOrderForBond(WhsOrder order, IEnumerable<WhsInventoryView> amendedInventory)
			{
				var pick = order.Factory.New<WhsPick>();
				pick.PickOrdersFailed += OnPickOrdersFailed;
				pick.PickOrdersForBond(order, amendedInventory);
				pick.PickOrdersFailed -= OnPickOrdersFailed;
			}

			void OnPickOrdersFailed(object sender, WhsPick.DocketPickabilityEventArgs e)
			{
				Reader.ThrowImportFailureExceptionIfNotEmpty(new ZStringBuilder(e.Message));
			}

			#endregion

			protected override void CancelForPhysicalWarehouseCore(WhsOrder docket)
			{
				var pick = docket?.Pick;
				if (pick != null)
				{
					var isCancellableError = pick.GetIsCancellableErrorMessage();
					if (!isCancellableError.IsEmpty)
					{
						var message = new ZStringBuilder(Res.GetString("ce11b31f-4976-4f18-b07d-940631833458",
							"Warehouse Order could not be amended for Customs Job {0} because of the following error(s) when canceling original pick:\r\n{1}", CustomsJobNo, isCancellableError));

						Reader.ThrowImportFailureExceptionIfNotEmpty(message);
					}
					else if (!docket.Pick.CancelPick())
					{
						var message = new ZStringBuilder(Res.GetString("36c4e987-d817-4702-b069-f897fe093dfd",
							"Warehouse Order could not be amended for Customs Job {0} because the original pick could not be canceled.", CustomsJobNo));

						Reader.ThrowImportFailureExceptionIfNotEmpty(message);
					}
				}
			}

			#region SyncOrderLineBondedAttributeWithInventory

			void SyncOrderLineBondedAttributeWithInventory(WhsOrder order)
			{
				foreach (var orderLine in order.Lines)
				{
					var inventory = orderLine.PickLines[0].Inventory.InDocketLine; // if shortfall pass, it will have pickline and inventory.
					SetCustomsDataFromInventory(orderLine.CustomsData, inventory.CustomsData);
				}
			}

			void SetCustomsDataFromInventory(WhsBondedWarehouseAttribute orderLineCustomsData, WhsBondedWarehouseAttribute inventoryCustomsData)
			{
				Reader.SetValue(orderLineCustomsData, WhsBondedWarehouseAttributeSchema.WB_BondedWhsQty, inventoryCustomsData.WB_BondedWhsQty);
				Reader.SetValue(orderLineCustomsData, WhsBondedWarehouseAttributeSchema.WB_AddInfo, MergeAddInfos(orderLineCustomsData.WB_AddInfo, inventoryCustomsData.WB_AddInfo));
				Reader.SetValue(orderLineCustomsData, WhsBondedWarehouseAttributeSchema.WB_CustomsQty, inventoryCustomsData.WB_CustomsQty);
				Reader.SetValue(orderLineCustomsData, WhsBondedWarehouseAttributeSchema.WB_CustomsUnitOfQty, inventoryCustomsData.WB_CustomsUnitOfQty);
				Reader.SetValue(orderLineCustomsData, WhsBondedWarehouseAttributeSchema.WB_EntryDate, inventoryCustomsData.WB_EntryDate);
				Reader.SetValue(orderLineCustomsData, WhsBondedWarehouseAttributeSchema.WB_RN_NKCountryOfOrigin, inventoryCustomsData.WB_RN_NKCountryOfOrigin);
				Reader.SetValue(orderLineCustomsData, WhsBondedWarehouseAttributeSchema.WB_TILV, inventoryCustomsData.WB_TILV);
				Reader.SetValue(orderLineCustomsData, WhsBondedWarehouseAttributeSchema.WB_ValueForDuty, inventoryCustomsData.WB_ValueForDuty);
				Reader.SetValue(orderLineCustomsData, WhsBondedWarehouseAttributeSchema.WB_CustomsSecondQuantity, inventoryCustomsData.WB_CustomsSecondQuantity);
				Reader.SetValue(orderLineCustomsData, WhsBondedWarehouseAttributeSchema.WB_CustomsSecondUnitQty, inventoryCustomsData.WB_CustomsSecondUnitQty);
				Reader.SetValue(orderLineCustomsData, WhsBondedWarehouseAttributeSchema.WB_Tariff, inventoryCustomsData.WB_Tariff);
				Reader.SetValue(orderLineCustomsData, WhsBondedWarehouseAttributeSchema.WB_PrimaryPreference, inventoryCustomsData.WB_PrimaryPreference);
				Reader.SetValue(orderLineCustomsData, WhsBondedWarehouseAttributeSchema.WB_CustomsThirdQuantity, inventoryCustomsData.WB_CustomsThirdQuantity);
				Reader.SetValue(orderLineCustomsData, WhsBondedWarehouseAttributeSchema.WB_CustomsThirdUnitQty, inventoryCustomsData.WB_CustomsThirdUnitQty);
				Reader.SetValue(orderLineCustomsData, WhsBondedWarehouseAttributeSchema.WB_ZoneStatus, inventoryCustomsData.WB_ZoneStatus);
				Reader.SetValue(orderLineCustomsData, WhsBondedWarehouseAttributeSchema.WB_IsFromAnotherFTZWhs, inventoryCustomsData.WB_IsFromAnotherFTZWhs);
				Reader.SetValue(orderLineCustomsData, WhsBondedWarehouseAttributeSchema.WB_OA_ManufacturerAddress, inventoryCustomsData.WB_OA_ManufacturerAddress);
				Reader.SetValue(orderLineCustomsData, WhsBondedWarehouseAttributeSchema.WB_RX_NKTILVCurrency, inventoryCustomsData.WB_RX_NKTILVCurrency);
				Reader.SetValue(orderLineCustomsData, WhsBondedWarehouseAttributeSchema.WB_BondedWhsUnitOfQty, inventoryCustomsData.WB_BondedWhsUnitOfQty);
			}

			string MergeAddInfos(string targetAddInfo, string sourceAddInfo)
			{
				var targetAddInfoEntries = targetAddInfo.Split('*');
				var sourceAddInfoEntries = sourceAddInfo.Split('*');

				var targetAddInfoMap = targetAddInfoEntries.ToDictionary(s => s.Split('=')[0], s => s);
				var sourceAddInfoMap = sourceAddInfoEntries.ToDictionary(s => s.Split('=')[0], s => s);

				List<string> keysToUpdate = new List<string>();
				foreach (var key in targetAddInfoMap.Keys)
				{
					if (sourceAddInfoMap.ContainsKey(key))
					{
						keysToUpdate.Add(key);
					}
				}

				foreach (var key in keysToUpdate)
				{
					targetAddInfoMap[key] = sourceAddInfoMap[key];
				}

				foreach (var key in sourceAddInfoMap.Keys)
				{
					if (!targetAddInfoMap.ContainsKey(key))
					{
						targetAddInfoMap.Add(key, sourceAddInfoMap[key]);
					}
				}

				var sb = new ZStringBuilder();
				foreach (var entry in targetAddInfoMap.Values)
				{
					sb.AppendIfNotEmpty(entry);
				}

				return sb.ToStringWithDelimiterBetweenAppends("*");
			}

			#endregion

			#endregion

			#endregion

			#region AfterFinaliseDocket

			protected override void AfterFinaliseDocket(WhsOrder order)
			{
				base.AfterFinaliseDocket(order);
				order.Pick.FinalisePick();
				SendErrorReporterIfFailedToFinalizePick(order);
			}

			#endregion
		}

		#region ClientDescriptionForErrorMessage

		protected override ZString ClientDescriptionForErrorMessage => CustomsHelper?.IsWarehouseBondedChangeOfOwnership ?? false ? (ZString)Res.GetString("WhsOrderDataObjectReader|ChangeOfOwnership|OwnerDescription", "Old Owner") : base.ClientDescriptionForErrorMessage;

		#endregion

		#region DataObjectReadFailureExceptionCaught

		protected override void DataObjectReadFailureExceptionCaught(WhsOrder order)
		{
			if (order?.JobHeader != null && !order.IsInDatabase)
			{
				order.JobHeader.Dispose();
			}
		}

		#endregion

		#region CustomsHelper

		protected override CustomsDataSourceHelper<WhsOrder> GetNewCustomsDataSourceHelper()
		{
			return new CustomsDataSourceHelperForOrder(dataObject, logger.TopLevelDataContext);
		}

		#endregion

		#endregion

		#endregion
	}
}
