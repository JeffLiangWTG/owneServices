using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using UniversalCodeDescriptionPair = Enterprise.UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal
{
	public abstract class WhsOrderAndReceiveDataObjectReader<TDocket, TDocketLine> : WhsDocketDataObjectReader<TDocket, TDocketLine>, IChangeOfInventoryGetExistingBusinessObject
		where TDocket : WhsDocket
		where TDocketLine : WhsDocketLine
	{
		protected WhsOrderAndReceiveDataObjectReader(UniversalShipment whsDocketDataObject, IXmlImportLogger logger, UniversalObjectFactory factory)
			: base(whsDocketDataObject, logger, factory)
		{
		}

		#region Matching Job

		protected override TDocket GetExistingBusinessObjectUsingModuleSpecificBusinessRules()
		{
			var docket = base.GetExistingBusinessObjectUsingModuleSpecificBusinessRules()
				?? ImportStrategy.LoadDocketFromCustomsLinesDocketNumbers;

			return ImportStrategy.SetMostRecentDocketForCustomsIfValid_VirtualFinalisedOrRealCancelled(docket) ? null : docket;
		}

		WhsDocket IChangeOfInventoryGetExistingBusinessObject.GetExistingBusinessObject()
		{
			return base.GetExistingBusinessObjectUsingModuleSpecificBusinessRules();
		}

		#endregion

		#region Matching References (fallbacks)

		protected override IMatchingBusinessEntityFinder<TDocket> GetCombinedReferenceMatcher()
		{
			IMatchingBusinessEntityFinder<TDocket> result = null;

			var order = dataObject.Order;
			if (order != null)
			{
				var whsDocketReferences = new WhsOrderAndReceiveReferences();
				whsDocketReferences.ClientReference = order.ClientReference.GetValueOrDefault(); // whs customer ref
				whsDocketReferences.TransportReference = order.TransportReference.GetValueOrDefault();
				whsDocketReferences.References = GetAdditionalReferencesForMatching();
				whsDocketReferences.ClientOrganization = ClientOrganisationPK;
				whsDocketReferences.ExternalReference = order.OrderNumber.GetValueOrDefault();
				whsDocketReferences.ExternalReferenceSplit = order.OrderNumberSplit.GetValueOrDefault();

				result = GetMatcher(whsDocketReferences);
			}

			return result;
		}

		protected abstract WhsOrderAndReceiveLastResortMatcher<TDocket> GetMatcher(WhsOrderAndReceiveReferences whsDocketReferences);

		#region GetAdditionalReferencesForMatching

		protected List<KeyValuePair<ZString, ZString>> GetAdditionalReferencesForMatching()
		{
			var references = new List<KeyValuePair<ZString, ZString>>();

			if (dataObject.AdditionalReferenceCollection != null)
			{
				foreach (var referenceDataObject in dataObject.AdditionalReferenceCollection)
				{
					var referenceType = referenceDataObject.Type.GetCodeAsUpperCase();
					var referenceNumber = referenceDataObject.ReferenceNumber.GetValueOrDefault();
					if (!referenceType.IsEmpty && !referenceNumber.IsEmpty)
					{
						references.Add(new KeyValuePair<ZString, ZString>(referenceType, referenceNumber));
					}
				}
			}

			return references;
		}

		#endregion

		#endregion

		// Create / Update

		#region Create / Update Job

		protected override void PopulateBusinessObjectCore(TDocket docket)
		{
			base.PopulateBusinessObjectCore(docket);

			LinkParents(docket);

			var orderDataObject = dataObject.Order;
			if (orderDataObject != null)
			{
				SetValue(docket, WhsDocketSchema.WD_CustomerReference, orderDataObject.ClientReference);
				SetValue(docket, WhsDocketSchema.WD_DropMode, orderDataObject.DropMode);

				SetValue(docket, WhsDocketSchema.WD_TotalCubic, orderDataObject.TotalLineVolume);
				SetValue(docket, WhsDocketSchema.WD_TotalWeight, orderDataObject.TotalLineWeight);
				SetValue(docket, WhsDocketSchema.WD_TotalUnits, orderDataObject.TotalUnits);
				SetValue(docket, WhsDocketSchema.WD_TransportReference, orderDataObject.TransportReference);
			}

			ImportStrategy.SetExternalReference(docket);
			SetValue(docket, WhsDocketSchema.WD_ExternalReferenceSplit, ImportStrategy.ExternalReferenceSplit);
			SetValue(docket, WhsDocketSchema.WD_PL_NKCarrierServiceLevel, dataObject.CarrierServiceLevel);
			SetValue(docket, WhsDocketSchema.WD_RS_NKServiceLevel, dataObject.ServiceLevel);
			SetValue(docket, WhsDocketSchema.WD_TotalOrderValue, dataObject.GoodsValue);
			SetValue(docket, WhsDocketSchema.WD_RX_NKTotalOrderCurrency, dataObject.GoodsValueCurrency);

			SetValue(docket, WhsDocketSchema.WD_IsInwardsProcessingJob, ImportStrategy.IsInwardProcessingJob);

			PopulateRelatedEntities(docket);
		}

		#region GetReasonForNotAbleToUpdateFromDataSourceOrTargetBO

		protected override ZString GetReasonForNotAbleToUpdateFromDataSourceOrTargetBO(TDocket docket)
		{
			var result = base.GetReasonForNotAbleToUpdateFromDataSourceOrTargetBO(docket);
			var orderDataObject = dataObject.Order;

			if (orderDataObject != null && result.IsEmpty)
			{
				var status = orderDataObject.Status.GetCodeAsUpperCase();

				if (status == DocketStatus.Codes.Cancelled)
				{
					if (IsNewBO)
					{
						result = Res.GetString("9ebe21d1-b508-4d77-a077-a3dc6a0e2ca2", "Cannot cancel the warehouse job as there is no existing job number '{0}'.", orderDataObject.OrderNumber);
					}
					else if (docket != null && !docket.IsCustomsTransaction && !WhsDocket.CanCancel(docket.WD_DocketStatus))
					{
						result = WhsDocket.CantCancelReasonMsg;
					}
				}
			}

			return result;
		}

		#endregion

		#region LinkDocketWithParent

		void LinkParents(TDocket docket)
		{
			var linker = new WarehouseDocketLinker(docket, logger.TopLevelDataObject, factory);
			foreach (var parentDataContextType in ParentDataContextTypes)
			{
				var dataSource = dataObject.DataContext.GetMatchingDataSource(parentDataContextType);
				linker.LinkDocket(dataSource, logger);
			}
		}

		protected abstract IEnumerable<DataContextType> ParentDataContextTypes { get; }

		#endregion

		#region PopulateRelatedEntities

		void PopulateRelatedEntities(TDocket docket)
		{
			if (dataObject.LocalProcessing != null && dataObject.LocalProcessing.AdditionalServiceCollection != null)
			{
				foreach (var serviceDataObject in dataObject.LocalProcessing.AdditionalServiceCollection)
				{
					docket.Services.Add(new AdditionalServiceDataObjectReader(serviceDataObject, logger, factory, docket).ReadIntoBusinessObject());
				}
			}

			if (dataObject.ContainerCollection != null)
			{
				if (dataObject.ContainerCollection.Content == CollectionContent.Complete)
				{
					var validContainerIDs = GetValidContainerIDs()?.Select(c => c?.ToUpper()).ToHashSet();
					var existingContainers = new List<WhsDocketContainer>(new TypedEnumerable<WhsDocketContainer>(docket.Containers));
					foreach (var containerDataObject in dataObject.ContainerCollection)
					{
						if (validContainerIDs == null || validContainerIDs.Contains(containerDataObject.ContainerNumber?.ToUpper()))
						{
							var container = new WhsDocketContainerDataObjectReader(containerDataObject, logger, factory, docket).ReadIntoBusinessObject();
							docket.Containers.Add(container);
							existingContainers.Remove(container);
						}
					}
					existingContainers.DeleteAll();
				}
				else if (dataObject.ContainerCollection.Content == null || dataObject.ContainerCollection.Content == CollectionContent.Partial)
				{
					foreach (var containerDataObject in dataObject.ContainerCollection)
					{
						docket.Containers.Add(new WhsDocketContainerDataObjectReader(containerDataObject, logger, factory, docket).ReadIntoBusinessObject());
					}
				}
			}
		}

		protected virtual IEnumerable<ZString?> GetValidContainerIDs() => null;

		protected override IEnumerable<ZString> AddressTypesToIgnoreWhenAddingToDocketDocAddressCollectionCore
		{
			get { return new ZString[] { nameof(DocAddressType.CustomsWarehouseAddress) }; }
		}

		#endregion

		#region PopulateLines

		protected override IEnumerable<OrderLine> GetDocketLinesCollectionCore(OrgHeader client)
		{
			var result = base.GetDocketLinesCollectionCore(client);
			return result ?? GetDocketLinesFromCommercialInvoiceLines(client);
		}

		IEnumerable<OrderLine> GetDocketLinesFromCommercialInvoiceLines(OrgHeader client)
		{
			IEnumerable<OrderLine> result = null;

			var commercialInvoiceLinesMap = CustomsHelper.IsDataSourceCustoms ? dataObject.GetWarehouseCustomsLineDetails(logger.TopLevelDataContext).ToArray() : null;
			if (commercialInvoiceLinesMap != null && commercialInvoiceLinesMap.Length > 0)
			{
				var linesToImport = commercialInvoiceLinesMap.Where(ld => ImportStrategy.ShouldImportLine(ld));
				result = linesToImport.SelectMany(l => GetDocketLinesFromInvoiceLine(l, client));
			}

			return result;
		}

		protected IEnumerable<OrderLine> GetDocketLinesFromInvoiceLine(IWarehouseCustomsLineDetails warehouseCustomsLineDetails, OrgHeader client)
		{
			var packDetails = GetPackDetailsFromCustomsLineDetails(warehouseCustomsLineDetails);
			if (packDetails != null && packDetails.Any())
			{
				foreach (var packDetail in packDetails)
				{
					yield return GetDocketLineFromInvoiceLine(warehouseCustomsLineDetails, client, packDetail, null);
				}
			}
			else
			{
				var allocationInfos = GetAllocationKeyDetailsFromCustomsLineDetails(warehouseCustomsLineDetails);
				if (allocationInfos != null && allocationInfos.Any())
				{
					if (warehouseCustomsLineDetails.InvoiceLine.BondedWarehouseQuantity != null)
					{
						var errorMessage = Res.GetString("f6551a5b-e11f-4ded-a46b-aec690a557b2",
							"Cannot Import Customs Job {0} as Order Line Bonded Warehouse Quantity and Allocation Key Infos were both provided.", CustomsHelper.CustomsJobNo);

						throw new DataObjectReadFailureException(errorMessage);
					}

					foreach (var allocationInfo in allocationInfos)
					{
						yield return GetDocketLineFromInvoiceLine(warehouseCustomsLineDetails, client, null, allocationInfo);
					}
				}
				else
				{
					yield return GetDocketLineFromInvoiceLine(warehouseCustomsLineDetails, client, null, null);
				}
			}
		}

		protected virtual IEnumerable<IWarehouseCustomsLinePackDetails> GetPackDetailsFromCustomsLineDetails(IWarehouseCustomsLineDetails warehouseCustomsLineDetails)
		{
			return null;
		}

		protected virtual IEnumerable<IWarehouseCustomsLineAllocationInfo> GetAllocationKeyDetailsFromCustomsLineDetails(IWarehouseCustomsLineDetails warehouseCustomsLineDetails)
		{
			return null;
		}

		OrderLine GetDocketLineFromInvoiceLine(IWarehouseCustomsLineDetails warehouseCustomsLineDetails, OrgHeader client, IWarehouseCustomsLinePackDetails packDetail, IWarehouseCustomsLineAllocationInfo allocationInfo)
		{
			var invoiceLine = warehouseCustomsLineDetails.InvoiceLine;
			var orderLine = new OrderLine();
			orderLine.Commodity = invoiceLine.Commodity;
			orderLine.CustomsData = new CustomsEntryInfo
			{
				AdditionalInformation = warehouseCustomsLineDetails.AddInfos,
				CountryOfOrigin = warehouseCustomsLineDetails.CountryOfOrigin,
				CustomsQuantity = warehouseCustomsLineDetails.CustomsQuantity,
				CustomsQuantityUnit = warehouseCustomsLineDetails.CustomsQuantityUnit,
				EntryLineNumber = warehouseCustomsLineDetails.EntryLineNumber,
				EntryKey = warehouseCustomsLineDetails.EntryNumber,
				InwardsEntryKey = warehouseCustomsLineDetails.PreviousEntryNumber,
				InwardsEntryLineNumber = warehouseCustomsLineDetails.PreviousEntryLineNumber,
				TILV = warehouseCustomsLineDetails.TILV,
				ValueForDuty = warehouseCustomsLineDetails.ValueForDuty,
				DeclarationReference = CustomsHelper.IsDataSourceCustoms ? CustomsHelper.CustomsJobNo : ZString.Empty,
				CustomsSecondQuantity = warehouseCustomsLineDetails.CustomsSecondQuantity,
				CustomsSecondUnitQty = warehouseCustomsLineDetails.CustomsSecondQuantityUnit,
				Tariff = warehouseCustomsLineDetails.Tariff,
				PrimaryPreference = warehouseCustomsLineDetails.PrimaryPreference,
				CustomsThirdQuantity = warehouseCustomsLineDetails.CustomsThirdQuantity,
				CustomsThirdUnitQty = warehouseCustomsLineDetails.CustomsThirdQuantityUnit,
				ManufacturerAddress = warehouseCustomsLineDetails.ManufacturerAddress,
				CustomsDeadline = warehouseCustomsLineDetails.CustomsDeadline,
				InwardStyle = warehouseCustomsLineDetails.Style,
				InwardProcedure = warehouseCustomsLineDetails.Procedure,
				DataImportMatchingKey = warehouseCustomsLineDetails.DataImportMatchingKey
			};

			var uSWarehouseCustomsLineDetails = warehouseCustomsLineDetails as IUSWarehouseCustomsLineDetails;
			if (uSWarehouseCustomsLineDetails != null)
			{
				var zoneStatusCode = uSWarehouseCustomsLineDetails.ZoneStatus ?? ZString.Empty;
				orderLine.CustomsData.ZoneStatus = (!zoneStatusCode.IsEmpty) ? new UniversalCodeDescriptionPair { Code = zoneStatusCode, Description = zoneStatusCode } : null;
				orderLine.CustomsData.IsFromOtherFTZWarehouse = uSWarehouseCustomsLineDetails.FromOtherFTZ;
				var outwardTypeCode = GetOutwardTypeCode(uSWarehouseCustomsLineDetails.OutwardType);
				orderLine.CustomsData.OutwardType = (!outwardTypeCode.IsEmpty) ? new UniversalCodeDescriptionPair { Code = outwardTypeCode, Description = outwardTypeCode } : null;
			}

			orderLine.ExtendedLinePrice = invoiceLine.LinePrice;

			if (warehouseCustomsLineDetails.OrderLineNo.HasValue)
			{
				orderLine.LineNumber = warehouseCustomsLineDetails.OrderLineNo;
			}

			orderLine.OrderedQty = allocationInfo?.Quantity ?? invoiceLine.BondedWarehouseQuantity;

			var bondedWarehouseQuantityUnit = invoiceLine.BondedWarehouseQuantityUnit;
			if (bondedWarehouseQuantityUnit != null)
			{
				orderLine.PackageQtyUnit = new PackageType { Code = bondedWarehouseQuantityUnit.Code, Description = bondedWarehouseQuantityUnit.Description };
			}

			var customisedFields = invoiceLine.CustomizedFieldCollection;
			if (customisedFields != null)
			{
				var attributeManager = GetProductOwner(client).PartAttributeManager;
				orderLine.PartAttribute1 = customisedFields.Find(c => c.Key.GetValueOrDefault().EqualsIgnoringCase(attributeManager.PartAttributeName1.GetUnresolvedString()))?.Value;
				orderLine.PartAttribute2 = customisedFields.Find(c => c.Key.GetValueOrDefault().EqualsIgnoringCase(attributeManager.PartAttributeName2.GetUnresolvedString()))?.Value;
				orderLine.PartAttribute3 = customisedFields.Find(c => c.Key.GetValueOrDefault().EqualsIgnoringCase(attributeManager.PartAttributeName3.GetUnresolvedString()))?.Value;
				orderLine.SerialNumber = customisedFields.Find(c => c.Key.GetValueOrDefault().EqualsIgnoringCase(attributeManager.SerialNumberName.GetUnresolvedString()))?.Value;
			}

			orderLine.Product = new Product { Code = invoiceLine.PartNo };

			PopulateDocketLineDataObjectFromInvoiceLineAndPackDetails(orderLine, warehouseCustomsLineDetails, packDetail);
			PopulateExtraDetails(warehouseCustomsLineDetails, orderLine, allocationInfo);

			return orderLine;
		}

		static ZString GetOutwardTypeCode(OutwardType? outwardType)
		{
			switch (outwardType)
			{
				case OutwardType.Consumption:
					return "CNN";
				case OutwardType.Exports:
					return "EXS";
				case OutwardType.ToOtherFTZ:
					return "TOF";
				default:
					return ZString.Empty;
			}
		}

		void PopulateExtraDetails(IWarehouseCustomsLineDetails lineDetails, OrderLine orderLine, IWarehouseCustomsLineAllocationInfo allocationInfo)
		{
			var extraDetails = new ExtraOrderLineDetails(
				lineDetails.SupplierAddress,
				lineDetails.ExtraClassificationDetails,
				GetExtraCustomsDetails(lineDetails.AdditionalAddInfos),
				lineDetails.NewOwnerProductCode,
				lineDetails.NewOwnerPartAttribute1,
				lineDetails.NewOwnerPartAttribute2,
				lineDetails.NewOwnerPartAttribute3,
				lineDetails.NewOwnerSerialNumber,
				allocationInfo?.AllocationKey);
			GetOrAddExtraOrderLineDetails(orderLine, () => extraDetails);
		}

		IEnumerable<ExtraOrderLineDetails.CustomsDetail> GetExtraCustomsDetails(IEnumerable<IWarehouseCustomsLineAddInfo> additionalAddInfos)
			=> additionalAddInfos?.Select(x => new ExtraOrderLineDetails.CustomsDetail(x.Type, x.AddInfoData)).ToArray();

		protected virtual void PopulateDocketLineDataObjectFromInvoiceLineAndPackDetails(OrderLine docketLineDataObject, IWarehouseCustomsLineDetails warehouseCustomsLineDetails, IWarehouseCustomsLinePackDetails packDetail)
		{
		}

		protected virtual OrgHeader GetProductOwner(OrgHeader client)
		{
			return client;
		}

		#region ExtraOrderLineDetails

		protected ExtraOrderLineDetails GetExtraOrderLineDetails(OrderLine orderLine)
			=> ExtraOrderLineDetailsByOrderLine.TryGetValue(orderLine, out var result) ? result : null;

		protected ExtraOrderLineDetails GetOrAddExtraOrderLineDetails(OrderLine orderLine, Func<ExtraOrderLineDetails> getNew)
			=> ExtraOrderLineDetailsByOrderLine.GetOrAdd(orderLine, getNew);

		Dictionary<OrderLine, ExtraOrderLineDetails> ExtraOrderLineDetailsByOrderLine
			=> extraOrderLineDetailsByOrderLine ?? (extraOrderLineDetailsByOrderLine = new Dictionary<OrderLine, ExtraOrderLineDetails>());

		Dictionary<OrderLine, ExtraOrderLineDetails> extraOrderLineDetailsByOrderLine;

		#endregion

		#endregion

		#region PopulateAdditionalAddresses

		protected override bool PopulateAdditionalAddresses(OrganizationAddress organizationDataObject, TDocket docket)
		{
			bool result = base.PopulateAdditionalAddresses(organizationDataObject, docket);
			if (!result && organizationDataObject.AddressType.GetValueOrDefault() == nameof(DocAddressType.LocalClient))
			{
				var localClientAddress = new OrganisationDataObjectReader(organizationDataObject, logger, factory).GetMatched();
				if (localClientAddress != null)
				{
					var loader = new JobHeader.Loader(docket);
					var jobHeader = loader.TryLoadOrCreateWithMutex();

					if (jobHeader != null)
					{
						SetValue(jobHeader, JobHeaderSchema.JH_OA_LocalChargesAddr, localClientAddress.PK);
						result = true;
					}
					else
					{
						throw new DataObjectReadFailureException(Res.GetString("219af4ab-e766-4f2e-b746-844416679a81", "Unable to set Local Client due to the following error:\r\n{0}", loader.GetJobCreationError().Message));
					}
				}
			}

			return result;
		}

		#endregion

		#region ValidateDataObjectFieldsUseWesternEuropeanOnly

		protected override void ValidateDataObjectFieldsUseWesternEuropeanOnlyCore()
		{
			base.ValidateDataObjectFieldsUseWesternEuropeanOnlyCore();

			if (dataObject.Order != null)
			{
				if (!dataObject.Order.ClientReference.GetValueOrDefault().IsWesternEuropeanOrEmpty)
				{
					ThrowErrorForInvalidCharactersInField(nameof(dataObject.Order.ClientReference));
				}

				if (!dataObject.Order.TransportReference.GetValueOrDefault().IsWesternEuropeanOrEmpty)
				{
					ThrowErrorForInvalidCharactersInField(nameof(dataObject.Order.TransportReference));
				}
			}
		}

		#endregion

		#endregion

		#region ImportStrategy

		protected sealed override WhsImportStrategy GetNewImportStrategy()
		{
			return CustomsHelper.IsDataSourceCustoms ? GetNewCustomsImportStrategy() : GetNewImportStrategyCore();
		}

		protected virtual WhsImportStrategy GetNewImportStrategyCore()
		{
			return base.GetNewImportStrategy();
		}

		protected abstract ImportFromCustomsStrategy GetNewCustomsImportStrategy();

		#region ImportFromCustomsStrategy

		public abstract class ImportFromCustomsStrategy : WhsImportStrategy
		{
			protected ImportFromCustomsStrategy(WhsOrderAndReceiveDataObjectReader<TDocket, TDocketLine> reader)
				: base(reader)
			{
			}

			protected new WhsOrderAndReceiveDataObjectReader<TDocket, TDocketLine> Reader
			{
				get { return (WhsOrderAndReceiveDataObjectReader<TDocket, TDocketLine>)base.Reader; }
			}

			#region ShouldImportLine

			protected override bool ShouldImportLineCore(IWarehouseCustomsLineDetails warehouseCustomsLineDetails)
			{
				return warehouseCustomsLineDetails.InvoiceLine.BondedWarehouseQuantity > 0
					|| warehouseCustomsLineDetails.AllocationInfos.Any(); // qty 0 means the goods are not for bond (Customs rule)
			}

			#endregion

			#region SetMostRecentDocketForCustomsIfValid_VirtualFinalisedOrRealCancelled

			protected override bool SetMostRecentDocketForCustomsIfValid_VirtualFinalisedOrRealCancelledCore(TDocket docket)
			{
				var result = false;

				if (docket != null)
				{
					var isVirtualWarehouse = docket.Warehouse.WW_IsVirtualWarehouse;
					var isDocketCancelled = docket.IsCancelled;

					var isVirtualWarehouseOrChangeOfOwnership = isVirtualWarehouse || IsStockRemainingInCurrentLocationForChangeOfInventory;
					if (IsWarehouseBondedChangeOfInventory && !isVirtualWarehouseOrChangeOfOwnership)
					{
						if (docket.IsFinalised)
						{
							throw new DataObjectReadFailureException(Res.GetString("1f85e9a0-9e96-47d2-989e-d43df7d4ea73", "Cannot amend warehouse job for stock movement if original warehouse job is finalized in Physical Warehouse."));
						}
						else if (isDocketCancelled)
						{
							throw new DataObjectReadFailureException(Res.GetString("9b7ff414-316b-4bcb-b6fe-c87e77f32984", "Cannot amend warehouse job for stock movement if original warehouse job is canceled in Physical Warehouse."));
						}
					}

					// We want the Import to create a new Docket if the previous one was cancelled
					// or the previous Docket is going to be cancelled.
					result = (isVirtualWarehouse || IsStockRemainingInCurrentLocationForChangeOfInventory) && ShouldAmendmentCancelOutDocket(docket)
						|| isDocketCancelled;

					if (result)
					{
						PreviousDocket = docket;
					}
				}

				return result;
			}

			/// <summary>
			/// The docket we are amending.
			/// </summary>
			TDocket PreviousDocket;

			#endregion

			#region DocketSubType

			protected sealed override UniversalCodeDescriptionPair DocketSubTypeCore
			{
				get { return new UniversalCodeDescriptionPair { Code = CustomsSubType }; }
			}

			protected abstract string CustomsSubType { get; }

			#endregion

			#region ExternalReference

			protected override void SetExternalReferenceCore(TDocket docket)
			{
				base.SetExternalReferenceCore(docket);
				Reader.SetValue(docket, WhsDocketSchema.WD_CustomsParentReference, MatchingReference);
			}

			protected override ZString? ExternalReference => Reader.CustomsHelper.CustomsJobNo;

			#endregion

			#region MatchingReference

			protected override ZString? MatchingReferenceCore
			{
				get { return Reader.CustomsHelper.CustomsParentReferenceToMatchForDocket; }
			}

			#endregion

			#region ExternalReferenceSplit

			protected override ZByte? ExternalReferenceSplitCore
			{
				get
				{
					ZByte? referenceSplit = null;
					if (PreviousDocket != null && (ShouldAmendmentCancelOutDocket(PreviousDocket) || PreviousDocket.IsCancelled))
					{
						if (PreviousDocket.WD_ExternalReferenceSplit < byte.MaxValue)
						{
							referenceSplit = (ZByte)(PreviousDocket.WD_ExternalReferenceSplit + 1);
						}
						else
						{
							Reader.ThrowImportFailureExceptionIfNotEmpty(new ZStringBuilder(Res.GetString("82d9f610-7b75-480a-8be9-9b820ae92ed5", "Unable to generate a new {0} as the previous {0} has reached the maximum allowed splits.", Reader.DocketType)));
						}
					}
					else
					{
						referenceSplit = base.ExternalReferenceSplitCore;
					}

					return referenceSplit;
				}
			}

			protected abstract bool ShouldAmendmentCancelOutDocket(TDocket docket);

			#endregion

			#region ClientDocAddressTypeCore

			protected override DocAddressType ClientDocAddressTypeCore
			{
				get { return DocAddressType.ImporterDocumentaryAddress; }
			}

			#endregion

			#region PopulateRelatedWarehouse

			protected override WhsWarehouse GetRelatedWarehouseCore(TDocket docket)
			{
				WhsWarehouse result = null;

				var warehouseAddress = GetRelatedWarehouseAddressCore();
				if (warehouseAddress != null)
				{
					var orgAddress = new OrganisationDataObjectReader(warehouseAddress, Reader.logger, Reader.factory).GetMatched();
					if (orgAddress != null && !orgAddress.Header.IsSystemDefinedOrganisation)
					{
						result = GetRelatedWarehouseFromAddress(orgAddress);
					}
					else
					{
						// reject entire job if warehouse address is unmatched or bad
						var errorMessage = new ZStringBuilder();
						warehouseAddress.AddAddressErrorMessage(Res.GetString("dc6ee254-d102-4756-b8fe-c52b0f883e4f", "Warehouse"), errorMessage);
						Reader.ThrowImportFailureExceptionIfNotEmpty(errorMessage);
					}
				}

				return result;
			}

			protected virtual OrganizationAddress GetRelatedWarehouseAddressCore()
			{
				return Reader.dataObject.OrganizationAddressCollection.FirstOrDefault(nameof(DocAddressType.CustomsWarehouseAddress));
			}

			WhsWarehouse GetRelatedWarehouseFromAddress(OrgAddress orgAddress)
			{
				return orgAddress.IsInDatabase ? LoadWarehouseWithAddress(orgAddress) : null;
			}

			WhsWarehouse LoadWarehouseWithAddress(OrgAddress orgAddress)
			{
				return LoadWarehouseByAddress(orgAddress);
			}

			#endregion

			#region IsInwardProcessingJobCore

			protected override bool IsInwardProcessingJobCore
				=> IsWarehouseBondedChangeOfRegime
					? IsChangeOfRegimeInwardProcessingJobForDocket()
					: Reader.dataObject.GetWarehouseRegimeType() == CustomsRegime.InwardProcessing;

			protected abstract bool IsChangeOfRegimeInwardProcessingJobForDocket();

			#endregion

			#region BeforeReadIntoCollectionCore

			protected sealed override void BeforeReadIntoCollectionCore(TDocket docket)
			{
				base.BeforeReadIntoCollectionCore(docket);

				if (!docket.Warehouse.WW_IsVirtualWarehouse)
				{
					// this needs to be set before clearing lines
					if (!IsWarehouseBondedChangeOfInventory && !CanFinaliseWithoutCustomsClearance(docket))
					{
						docket.Logs.AddNew(Events.HoldTheWarehouseOrder); // new Customs Orders/Receipts should be HELD until confirmed by Customs for finalisation
					}
					BeforeReadLinesIntoCollectionForRealWarehouse(docket);
				}
			}

			bool CanFinaliseWithoutCustomsClearance(WhsDocket docket)
			{
				var order = docket as WhsOrder;
				return order != null && WhsCustomsHelper.CanFinaliseWhsOrderWithoutCustomsClearance(order);
			}

			protected virtual void BeforeReadLinesIntoCollectionForRealWarehouse(TDocket docket)
			{
			}

			#endregion

			#region BeforePopulate

			protected override void BeforePopulateCore(TDocket docket)
			{
				base.BeforePopulateCore(docket);

				if (BranchFromUniversalShipment == null)
				{
					var errorMessage = Reader.dataObject.Branch == null
						? Res.GetString("3fe0b8ef-2301-49b6-8e09-dc310caf0baa", "Cannot Import Customs Job {0} as no Branch was provided.", CustomsJobNo)
						: Res.GetString("bd804c1c-70a3-4d1b-8483-142c5e0e5a40", "Cannot Import Customs Job {0}. Unable to match Branch {1}.", CustomsJobNo, Reader.dataObject.Branch.ToStringContents());

					throw new DataObjectReadFailureException(errorMessage);
				}
			}

			protected GlbBranch BranchFromUniversalShipment
			{
				get
				{
					if (branch == null)
					{
						var branchCode = Reader.dataObject.Branch.GetCodeAsUpperCase();
						if (!branchCode.IsEmpty)
						{
							branch = Reader.factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.GB_Code, branchCode));
						}
					}

					return branch;
				}
			}

			GlbBranch branch;

			#endregion

			#region AfterPopulate

			protected override void AfterPopulateCore(TDocket docket)
			{
				base.AfterPopulateCore(docket);
				// if you add something here it will not be executed for amendments in virtual warehouse. See overriden method to find out why.

				var isVirtualWarehouse = docket.Warehouse.WW_IsVirtualWarehouse;
				if (isVirtualWarehouse)
				{
					var amendedInventory = CancelOutPreviousDocket();
					AfterPopulateWhenVirtualWarehouse(docket, amendedInventory);
					FinaliseDocket(docket);
				}
				else
				{
					var isFinaliseAllowed = true;
					if (IsWarehouseBondedChangeOfOwnership && IsStockRemainingInCurrentLocationForChangeOfInventory)
					{
						CancelOutPreviousDocket();
					}
					else if (IsWarehouseBondedChangeOfInventory)
					{
						CancelForPhysicalWarehouse(PreviousDocket);

						isFinaliseAllowed = false;
						docket.Logs.AddNew(Events.HoldTheWarehouseOrder);
					}

					AfterPopulateWhenRealWarehouse(docket);

					if (IsWarehouseBondedChangeOfInventory && isFinaliseAllowed)
					{
						FinaliseDocket(docket); // pick finalised after order
					}
				}
			}

			protected virtual void AfterPopulateWhenRealWarehouse(TDocket docket)
			{
			}

			protected virtual void AfterPopulateWhenVirtualWarehouse(TDocket docket, IEnumerable<WhsInventoryView> amendedInventory = null)
			{
			}

			protected ZString? CustomsJobNo => Reader.CustomsHelper.CustomsJobNo;

			protected override string GetNameForFinaliseDocket(TDocket docket) => Res.GetString("c375c05e-5900-4f9a-8cbc-0a6c1de5f37f", "Customs Job {0}", CustomsJobNo);

			#endregion

			#region PopulateBizOSuspenderCore

			protected override IDisposable PopulateBizOSuspenderCore(TDocket docket)
			{
				return IsWarehouseBondedChangeOfInventory
					? new DisposableList(new[] { docket.MarkAsImportingForChangeOfInventory(), PreviousDocket?.MarkAsImportingForChangeOfInventory() })
					: base.PopulateBizOSuspenderCore(docket);
			}

			#endregion

			#region CancelOutPreviousDocket

			/// <summary>
			/// Cancels out the previous order or receipt. Used when inventory has not physically moved/changed locations.
			/// Only to be used for Virtual Warehouse or Change of Ownership.
			/// </summary>
			IEnumerable<WhsInventoryView> CancelOutPreviousDocket()
			{
				IEnumerable<WhsInventoryView> amendedInventory = null;

				// in a virtual warehouse the docket cannot be cancelled then later reinstated (that same docket)
				if (PreviousDocket != null && !PreviousDocket.IsCancelled)
				{
					amendedInventory = Reader.CustomsHelper.CancelOutDocketAndThrowImportFailureExceptionIfCancellingOutFailed(PreviousDocket);
				}

				return amendedInventory;
			}

			#endregion

			#region CancelOutPick

			/// <summary>
			/// Perform any cancellation logic specific to Physical Warehouses. This will be used for Physical Warehouses during Change of Regime or Change of Warehouse.
			/// </summary>
			void CancelForPhysicalWarehouse(TDocket docket) => CancelForPhysicalWarehouseCore(docket);

			protected virtual void CancelForPhysicalWarehouseCore(TDocket docket)
			{
			}

			#endregion

			#region RequireClientForMatch

			protected override bool RequireClientForMatchCore
			{
				get { return false; }
			}

			#endregion

			#region AddMatchingReferenceFilter

			protected override void AddMatchingReferenceFilterCore(ZString matchingReferenceNumber, ZQuery query)
			{
				// do not call base
				query.AddToFilter(WhsDocketSchema.WD_CustomsParentReference, matchingReferenceNumber);
			}

			#endregion

			#region AddAdditionalFilter

			protected override void AddAdditionalFilterCore(UniversalShipment dataObject, ZQuery query)
			{
				// do not call base
				query.OrderBy = WhsDocketSchema.Constants.WD_ExternalReferenceSplit + OrderByClause.Descending;
			}

			#endregion

			#region GetReasonForNotAbleToUpdateMatchedDocket

			protected abstract ZString FinalisedCannotUpdateMessage(ZString? customsJobNo);

			protected override ZString GetReasonForNotAbleToUpdateMatchedDocketCore(TDocket matchedDocket)
			{
				ZString result;

				bool isVirtualWarehouseOrChangeOfInventory = matchedDocket.Warehouse.WW_IsVirtualWarehouse || IsWarehouseBondedChangeOfInventory;
				if (matchedDocket.IsFinalised && !isVirtualWarehouseOrChangeOfInventory)
				{
					result = FinalisedCannotUpdateMessage(CustomsJobNo);
				}
				else if (isVirtualWarehouseOrChangeOfInventory && !ShouldAmendmentCancelOutDocket(matchedDocket) && matchedDocket.IsCancelled)
				{
					result = ZString.Empty; // Allow update
				}
				else
				{
					result = base.GetReasonForNotAbleToUpdateMatchedDocketCore(matchedDocket);
				}

				return result;
			}

			#endregion

			#region OnWarehouseMatched

			protected override void OnWarehouseMatchedCore(WhsWarehouse warehouse)
			{
				base.OnWarehouseMatchedCore(warehouse);

				var branchCountry = BranchFromUniversalShipment.Company.GC_RN_NKCountryCode;
				var warehouseCountry = warehouse.RelatedCompanyBranch.Company.GC_RN_NKCountryCode;

				if (!warehouseCountry.EqualsIgnoringCase(branchCountry))
				{
					var errorMessage = Res.GetString("a20ccca6-c137-460a-ba19-3a36294b4e46",
						"Cannot Import Customs Job {0} as its Branch Country ({1}) does not match the Warehouse Country ({2}).", CustomsJobNo, branchCountry, warehouseCountry);
					throw new DataObjectReadFailureException(errorMessage);
				}
			}

			#endregion

			#region IsWarehouseBondedChangeOfOwnership

			protected bool IsWarehouseBondedChangeOfOwnership => Reader.CustomsHelper.IsWarehouseBondedChangeOfOwnership;

			#endregion

			#region IsWarehouseBondedChangeOfRegime

			protected bool IsWarehouseBondedChangeOfRegime => Reader.CustomsHelper.IsWarehouseBondedChangeOfRegime;

			#endregion

			#region IsWarehouseBondedChangeOfInventory

			protected bool IsWarehouseBondedChangeOfInventory => Reader.CustomsHelper.IsWarehouseBondedChangeOfInventory;

			#endregion

			#region IsWarehouseBondedChangeOfWarehouse

			protected override bool IsWarehouseBondedChangeOfWarehouseCore
			{
				get
				{
					var isChangeOfWarehouse = false;

					if (IsWarehouseBondedChangeOfOwnership)
					{
						isChangeOfWarehouse = NewWarehouseAddressForChangeOfOwnership != null;
					}
					else if (IsWarehouseBondedChangeOfRegime)
					{
						isChangeOfWarehouse = NewWarehouseAddressForChangeOfRegime != null;
					}

					return isChangeOfWarehouse;
				}
			}

			protected OrganizationAddress NewWarehouseAddressForChangeOfOwnership => Reader.dataObject.GetWarehouseCustomsDetailsChangeOfOwnership(Reader.logger.TopLevelDataContext)?.NewWarehouse;

			protected OrganizationAddress NewWarehouseAddressForChangeOfRegime => Reader.dataObject.GetWarehouseCustomsDetailsChangeOfRegime(Reader.logger.TopLevelDataContext)?.NewWarehouse;

			#endregion

			#region IsStockRemainingInCurrentLocationForChangeOfInventory

			protected bool IsStockRemainingInCurrentLocationForChangeOfInventory
			{
				get
				{
					var changeOfRegimeDetails = Reader.dataObject.GetWarehouseCustomsDetailsChangeOfRegime(Reader.logger.TopLevelDataContext);
					return IsWarehouseBondedChangeOfInventory
						&& !IsWarehouseBondedChangeOfWarehouse
						&& (!IsWarehouseBondedChangeOfRegime || changeOfRegimeDetails?.IntoRegimeType == changeOfRegimeDetails?.OutOfRegimeType);
				}
			}

			#endregion
		}

		#region IsCheckingIfWhsOrderAmendmendIsValid

		protected bool IsCheckingIfAmendmendIsValid => logger is WrappingLogger;

		protected class WrappingLogger : UniversalDataBuss.Integration.DummyLogger
		{
			public WrappingLogger(IXmlImportLogger wrappedLogger)
			{
				TopLevelDataObject = wrappedLogger.TopLevelDataObject;
			}
		}

		#endregion

		#endregion

		#region CustomsHelper

		protected CustomsDataSourceHelper<TDocket> CustomsHelper
		{
			get { return customsHelper ?? (customsHelper = GetNewCustomsDataSourceHelper()); }
		}

		protected abstract CustomsDataSourceHelper<TDocket> GetNewCustomsDataSourceHelper();

		CustomsDataSourceHelper<TDocket> customsHelper;

		#endregion

		#region ImportFromOrderAndReceiveStartegy

		public abstract class ImportFromOrderAndReceiveStrategy : WhsImportStrategy
		{
			protected ImportFromOrderAndReceiveStrategy(WhsOrderAndReceiveDataObjectReader<TDocket, TDocketLine> reader)
				: base(reader)
			{
			}

			protected new WhsOrderAndReceiveDataObjectReader<TDocket, TDocketLine> Reader
			{
				get { return (WhsOrderAndReceiveDataObjectReader<TDocket, TDocketLine>)base.Reader; }
			}

			protected IXmlImportLogger Logger
			{
				get { return Reader.logger; }
			}

			#region BeforePopulate

			protected override void BeforePopulateCore(TDocket docket)
			{
				base.BeforePopulateCore(docket);
				var dataObjectStatus = Reader.dataObject.Order?.Status.GetCodeAsUpperCase() ?? ZString.Empty;

				if (docket != null && !docket.IsCustomsTransaction && dataObjectStatus == DocketStatus.Codes.Cancelled)
				{
					var cancelError = docket.CanCancel();
					if (string.IsNullOrEmpty(cancelError))
					{
						docket.WD_DocketStatus = DocketStatus.Codes.Cancelled;
						var errorMessage = Res.GetString("2988feff-e2ee-4a2f-abf7-0a21040515e6", "The warehouse {0} - {1} has been canceled and no other updates were made.", docket.GetType().Name, docket.WD_DocketID);
						Logger.Log(LogType.Information, errorMessage);
					}
					else
					{
						var errorMessage = Res.GetString("4afd837e-56e5-4539-ba12-35eecc5b4224", "The warehouse {0} - {1} failed set to canceled because {2}", docket.GetType().Name, docket.WD_DocketID, cancelError);
						throw new DataObjectReadFailureException(errorMessage);
					}
				}
			}

			#endregion

			#region ShouldPopulateBizOCore

			protected override bool ShouldPopulateBizOCore(UniversalShipment dataObject, TDocket docket)
			{
				return !docket.IsCancelled && base.ShouldPopulateBizOCore(dataObject, docket);
			}

			#endregion

			#region AfterPopulateCore

			protected override void AfterPopulateCore(TDocket docket)
			{
				if (!docket.IsCancelled)
				{
					base.AfterPopulateCore(docket);
				}
			}

			#endregion
		}

		#endregion

		#endregion
	}
}
