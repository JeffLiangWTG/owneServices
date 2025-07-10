using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.Freight.DataTransfer;
using Enterprise.Freight.DataTransfer.Universal;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	class PackagesWrapper : IPackageParentDataObject
	{
		public PackagesWrapper(UniversalShipment shipment, DataObjectList<PackingLine> packingLines)
		{
			this.shipment = shipment;
			this.packingLines = packingLines;

			IPackageParentDataObject packageParentDataObject = this;
			packageParentDataObject.GoodsDescription = shipment.GoodsDescription;
			packageParentDataObject.OuterPacks = shipment.OuterPacks;
			packageParentDataObject.OuterPacksPackageType = shipment.OuterPacksPackageType;
			packageParentDataObject.TotalNoOfPacks = shipment.TotalNoOfPacks;
			packageParentDataObject.TotalNoOfPacksPackageType = shipment.TotalNoOfPacksPackageType;
			packageParentDataObject.TotalVolume = shipment.TotalVolume;
			packageParentDataObject.TotalVolumeUnit = shipment.TotalVolumeUnit;
			packageParentDataObject.TotalWeight = shipment.TotalWeight;
			packageParentDataObject.TotalWeightUnit = shipment.TotalWeightUnit;
			packageParentDataObject.TotalNoOfPieces = shipment.TotalNoOfPieces;
		}

		readonly UniversalShipment shipment;
		readonly DataObjectList<PackingLine> packingLines;

		public DataObjectList<Container> ContainerCollection => shipment.ContainerCollection;
		public DataObjectList<PackingLine> PackingLineCollection => packingLines;
		ZString? IPackageParentDataObject.GoodsDescription { get; set; }
		ZInt? IPackageParentDataObject.OuterPacks { get; set; }
		PackageType IPackageParentDataObject.OuterPacksPackageType { get; set; }
		ZInt? IPackageParentDataObject.TotalNoOfPacks { get; set; }
		PackageType IPackageParentDataObject.TotalNoOfPacksPackageType { get; set; }
		ZDecimal? IPackageParentDataObject.TotalVolume { get; set; }
		UnitOfVolume IPackageParentDataObject.TotalVolumeUnit { get; set; }
		ZDecimal? IPackageParentDataObject.TotalWeight { get; set; }
		UnitOfWeight IPackageParentDataObject.TotalWeightUnit { get; set; }
		ZInt? IPackageParentDataObject.TotalNoOfPieces { get; set; }
	}

	[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Constant strings")]
	public class ShipmentDataObjectReadingHelper : DataObjectReader<UniversalShipment>, ITopLevelDataObjectReader
	{
		#region Constants

		public const decimal InvalidDecimalValue = -1m;
		const string ImportDeliveryDueDateReason = "Changed by Data Import";
		const string ProcessedTransitDispatchesPropertyName = "Processed Transit Dispatches";
		const string ProcessedTransitDispatchesSeparator = ",";

		#endregion

		public ShipmentDataObjectReadingHelper(UniversalShipment shipmentDataObject, IXmlImportLogger logger, UniversalObjectFactory factory)
			: base(shipmentDataObject, logger, factory)
		{
		}

		internal IOrderLineLinkManager OrderLineLinkManager
		{
			get
			{
				return orderLineLinkManager ?? (orderLineLinkManager = new OrderLineLinkManager());
			}
		}
		IOrderLineLinkManager orderLineLinkManager;

		public IContainerLinkManager<ForwardingConsol> LinkManager
		{
			get;
			set;
		}

		public ShipmentReferences GenerateShipmentReference()
		{
			var reference = new ShipmentReferences();

			if (dataObject.TransportMode.GetCodeAsUpperCase() == Constants.TransportModes.Air)
			{
				reference.HAWBNumber = dataObject.WayBillNumber.GetValueOrDefault();
			}
			else
			{
				reference.HBOLNumber = dataObject.WayBillNumber.GetValueOrDefault();
			}

			reference.ShippersReference = dataObject.BookingConfirmationReference.GetValueOrDefault();

			var orderNumbers = new List<ZString>();

			if (dataObject.LocalProcessing != null
				&& dataObject.LocalProcessing.OrderNumberCollection != null)
			{
				foreach (var orderNumberDataObject in dataObject.LocalProcessing.OrderNumberCollection)
				{
					var orderNumber = orderNumberDataObject.OrderReference.GetValueOrDefault();
					if (!orderNumber.IsEmpty)
					{
						orderNumbers.Add(orderNumber);
					}
				}
			}

			reference.OrderNumbers = orderNumbers;
			reference.InterimReceipt = dataObject.InterimReceiptNumber.GetValueOrDefault();
			reference.CFSReference = dataObject.CFSReference.GetValueOrDefault();
			reference.PopulateAdditionalReferences(dataObject, onlyWhenCodesMappedToTarget: !(dataObject.IsNVOCC() && dataObject.IsLinkOnly() && dataObject.IsShippingInstructionMessage()));
			reference.OriginUNLOCO = dataObject.PortOfOrigin.GetUNLOCOAsUpperCase(factory.BOFactory);
			reference.DestinationUNLOCO = dataObject.PortOfDestination.GetUNLOCOAsUpperCase(factory.BOFactory);

			reference.IsVGM = dataObject.IsVGM();
			reference.MBOLNumber = dataObject.WayBillNumber.GetValueOrDefault();
			reference.WayBillTypeCode = dataObject.WayBillType?.Code ?? ZString.Empty;
			reference.SCAC = dataObject.GetSCAC(DocAddressType.ShippingLineAddress);

			reference.IsNVOCC = dataObject.IsNVOCC();
			reference.AgentsReference = dataObject.AgentsReference.GetValueOrDefault();
			reference.CoLoadBookingConfirmationReference = dataObject.CoLoadBookingConfirmationReference.GetValueOrDefault();
			reference.CoLoadMasterBillNumber = dataObject.CoLoadMasterBillNumber.GetValueOrDefault();
			reference.BookingPartyPK = GetBookingParty();
			reference.BookingPartyName = GetBookingPartyName();

			return reference;
		}

		ZGuid GetBookingParty()
		{
			var bookingPartyDataObject = dataObject.OrganizationAddressCollection?.FirstOrDefault(nameof(DocAddressType.BookingPartyDocumentaryAddress));
			if (bookingPartyDataObject != null)
			{
				var bookingPartyAddress = new OrganisationDataObjectReader(bookingPartyDataObject, logger, factory).GetMatched();
				if (bookingPartyAddress != null)
				{
					return bookingPartyAddress.OA_OH;
				}
			}

			return ZGuid.Empty;
		}

		ZString GetBookingPartyName()
		{
			var bookingPartyDataObject = dataObject.OrganizationAddressCollection?.FirstOrDefault(nameof(DocAddressType.BookingPartyDocumentaryAddress));
			return bookingPartyDataObject?.CompanyName.GetValueOrDefault().ToUpper() ?? ZString.Empty;
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Documentname Constant")]
		public void PopulateBusinessObject(ForwardingShipment shipmentBO, bool excludeBookingParty = false)
		{
			var isBookingRequest = dataObject?.DataContext?.DocumentaryOverride?.DocumentName.ToString() == "Booking Request";

			List<OrganizationAddress> organisationCollection = null;
			if (dataObject.OrganizationAddressCollection != null)
			{
				organisationCollection = new List<OrganizationAddress>(dataObject.OrganizationAddressCollection);
			}

			shipmentBO.IsSettingDefaultValues = true;
			shipmentBO.IsSettingDefaultValuesForHBLAWBChargesDisplay = !IsTWFunctionalityEnabled(shipmentBO);
			var wayBillNumber = shipmentBO.JS_HouseBill;

			SetValue(shipmentBO, JobShipmentSchema.JS_AdditionalTerms, dataObject.AdditionalTerms);
			if (isBookingRequest)
			{
				AddAdditionalBookingReferences(shipmentBO);
			}
			else
			{
				SetValue(shipmentBO, JobShipmentSchema.JS_BookingReference, dataObject.BookingConfirmationReference);
			}
			SetValue(shipmentBO, JobShipmentSchema.JS_CFSReference, dataObject.CFSReference);

			SetValue(shipmentBO, JobShipmentSchema.JS_InterimReceipt, dataObject.InterimReceiptNumber);
			SetValue(shipmentBO, JobShipmentSchema.JS_IsDirectBooking, dataObject.IsDirectBooking);

			SetValue(shipmentBO, JobShipmentSchema.JS_TransportMode, dataObject.TransportMode);
			SetValue(shipmentBO, JobShipmentSchema.JS_PackingMode, dataObject.ContainerMode);

			SetValue(shipmentBO, JobShipmentSchema.JS_HouseBill, !dataObject.WayBillNumber.HasValue ? wayBillNumber : dataObject.WayBillNumber);

			using (shipmentBO.DeferDefaultCFSForDeliveryDueDate())
			{
				SetConsignorConsigneeOrgAddresses(shipmentBO, organisationCollection);
			}

			SetValue(shipmentBO, JobShipmentSchema.JS_RL_NKOrigin, dataObject.PortOfOrigin);
			SetValue(shipmentBO, JobShipmentSchema.JS_RL_NKDestination, dataObject.PortOfDestination);

			var action = shipmentBO.BuyerSupplierLinksHelper?.SuspendPortRestoration();
			try
			{
				using (shipmentBO.DeferDefaultCFSForDeliveryDueDate())
				{
					SetConsignorConsigneePickupDeliveryAddresses(shipmentBO, organisationCollection);
					SetOrgAddresses(shipmentBO, organisationCollection, excludeBookingParty);
				}

				FillCollections(shipmentBO);

				SetValue(shipmentBO, JobShipmentSchema.JS_INCO, dataObject.ShipmentIncoTerm);
				SetValue(shipmentBO, JobShipmentSchema.JS_GoodsDescription, dataObject.GoodsDescription);
				SetValue(shipmentBO, JobShipmentSchema.JS_RS_NKServiceLevel, dataObject.ServiceLevel);
				SetValue(shipmentBO, JobShipmentSchema.JS_UnitFreightRate, dataObject.FreightRate);
				SetValue(shipmentBO, JobShipmentSchema.JS_RX_NKFrtRateCurrency, dataObject.FreightRateCurrency);
				SetValue(shipmentBO, JobShipmentSchema.JS_GoodsValue, dataObject.GoodsValue);
				SetValue(shipmentBO, JobShipmentSchema.JS_RX_NKGoodsValueCurr, dataObject.GoodsValueCurrency);
				SetValue(shipmentBO, JobShipmentSchema.JS_InsuranceValue, dataObject.InsuranceValue);
				SetValue(shipmentBO, JobShipmentSchema.JS_RX_NKInsuranceCurrency, dataObject.InsuranceValueCurrency);

				SetValue(shipmentBO, JobShipmentSchema.JS_F3_NKPackType, dataObject.OuterPacksPackageType);
				SetValue(shipmentBO, JobShipmentSchema.JS_OuterPacks, dataObject.OuterPacks);
				SetValue(shipmentBO, JobShipmentSchema.JS_PackingOrder, dataObject.PackingOrder);

				SetValue(shipmentBO, JobShipmentSchema.JS_ActualVolume, dataObject.TotalVolume);
				SetValue(shipmentBO, JobShipmentSchema.JS_UnitOfVolume, dataObject.TotalVolumeUnit);
				SetValue(shipmentBO, JobShipmentSchema.JS_ActualWeight, dataObject.TotalWeight);
				SetValue(shipmentBO, JobShipmentSchema.JS_UnitOfWeight, dataObject.TotalWeightUnit);
				SetValue(shipmentBO, JobShipmentSchema.JS_ActualChargeable, dataObject.ActualChargeable);

				SetValue(shipmentBO, JobShipmentSchema.JS_HouseBillOfLadingType, dataObject.HouseBillOfLadingType);
			}
			finally
			{
				action?.Dispose();
			}
		}

		public bool IsTWFunctionalityEnabled(ForwardingShipment shipmentBO)
		{
			return dataObject.PackingLineCollection != null
				&& dataObject.PackingLineCollection.Count > 0
				&& !shipmentBO.IsMasterShipmentRepresentingAllChildShipments
				&& (IsTWFunctionalityEnabled(shipmentBO, DataContextType.TransitReceive, out _) || IsTWFunctionalityEnabled(shipmentBO, DataContextType.TransitDispatch, out _));
		}

		public void ImportAviationSecurityInspectionTypeIfNeeded(ForwardingShipment shipmentBO)
		{
			if (dataObject.AviationSecurityInspectionType is null)
			{
				return;
			}
			if (string.IsNullOrEmpty(dataObject.AviationSecurityInspectionType.Code))
			{
				dataObject.AviationSecurityInspectionType.Code = ShipmentInspectionType.AviationSecurity_Unknown_Code;
			}
			new AviationSecurityInspectionTypeDataObjectReader(dataObject.AviationSecurityInspectionType, logger, factory, shipmentBO, shipmentBO.Logs, shipmentBO.IsInDatabase, (NoResString)"Shipment Inspection Type Changed by Data Import").ReadIntoBusinessObject(); // System event info
		}

		public void ImportAviationSecurityAdditionalInspectionTypeIfNeeded(ForwardingShipment shipmentBO)
		{
			if (dataObject.AviationSecurityAdditionalInspectionType != null)
			{
				new AviationSecurityAdditionalInspectionTypeDataObjectReader(dataObject.AviationSecurityAdditionalInspectionType, logger, factory, shipmentBO, shipmentBO.Logs, shipmentBO.IsInDatabase, (NoResString)"Shipment Additional Inspection Type Changed by Data Import").ReadIntoBusinessObject(); // System event info
			}
		}

		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		void FillCollections(ForwardingShipment shipmentBO)
		{
			if (dataObject.DateCollection != null)
			{
				foreach (var dateDataObject in dataObject.DateCollection)
				{
					switch (dateDataObject.Type)
					{
						case DateType.BookingConfirmed:
							if (!dateDataObject.IsEstimate.GetValueOrDefault())
							{
								SetValue(shipmentBO, JobShipmentSchema.JS_A_BKD, dateDataObject.Value);
							}
							break;
						case DateType.Received:
							if (!dateDataObject.IsEstimate.GetValueOrDefault())
							{
								SetValue(shipmentBO, JobShipmentSchema.JS_A_RCV, dateDataObject.Value);
							}
							break;
						case DateType.Departure:
							if (!dateDataObject.IsEstimate.HasValue || dateDataObject.IsEstimate.Value)
							{
								SetValue(shipmentBO, JobShipmentSchema.JS_E_DEP, dateDataObject.Value);
							}
							break;
						case DateType.Arrival:
							if (!dateDataObject.IsEstimate.HasValue || dateDataObject.IsEstimate.Value)
							{
								SetValue(shipmentBO, JobShipmentSchema.JS_E_ARV, dateDataObject.Value);
							}
							break;
						case DateType.DeliveryDueDate:
							shipmentBO.GetReasonForChangingDeliveryDueDateEventHandler += (sender, arg) => arg.Reason = ImportDeliveryDueDateReason;
							SetValue(shipmentBO, JobShipmentSchema.JS_DeliveryDueDate, dateDataObject.Value);
							break;
						case DateType.RevisedDeliveryDueDate:
							shipmentBO.GetReasonForChangingDeliveryDueDateEventHandler += (sender, arg) => arg.Reason = ImportDeliveryDueDateReason;
							SetValue(shipmentBO, JobShipmentSchema.JS_RevisedDeliveryDueDate, new ZDateTimeOffset(dateDataObject.Value ?? ZDateTime.Empty, DateTimeKind.Local, TimeSpan.Zero));
							break;
						default:
							SetDate(dateDataObject);
							break;
					}
				}
			}

			var isComplete = dataObject?.PackingLineCollection?.Content == CollectionContent.Complete;
			if (dataObject.PackingLineCollection != null && (dataObject.PackingLineCollection.Count > 0 || isComplete))
			{
				if (!shipmentBO.IsMasterShipmentRepresentingAllChildShipments)
				{
					using (shipmentBO.OuterPackLines.TemporarilyDisableAutomaticPackingIntoContainer())
					using (ContainerPackingMonitor.MonitorContainerTareWeightChanges(dataObject, shipmentBO))
					using (RequireTEUMonitorHelper.MonitorRequireTEUChanges(dataObject, shipmentBO))
					{
						if (IsTWFunctionalityEnabled(shipmentBO, DataContextType.TransitReceive, out var matchedReceiveAddress))
						{
							var processedTransitDispatches = GetProcessedTransitDispatches(shipmentBO);
							if (processedTransitDispatches.Contains(matchedReceiveAddress.Header.OH_Code))
							{
								throw new DataObjectReadFailureException("RCN xml has not been processed. This update should be done through the DCN.");
							}

							new ShipmentDataObjectSplitPackageHelper(dataObject, shipmentBO, matchedReceiveAddress, logger, factory, this).ProcessTransitReceive();
						}
						else if (IsTWFunctionalityEnabled(shipmentBO, DataContextType.TransitDispatch, out var matchedDispatchAddress))
						{
							new ShipmentDataObjectSplitPackageHelper(dataObject, shipmentBO, matchedDispatchAddress, logger, factory, this).ProcessTransitDispatch();
							var processedTransitDispatches = GetProcessedTransitDispatches(shipmentBO);
							if (!processedTransitDispatches.Contains(matchedDispatchAddress.Header.OH_Code))
							{
								processedTransitDispatches = processedTransitDispatches.Append(matchedDispatchAddress.Header.OH_Code).ToArray();
								shipmentBO.SetUserDefinedValue(ProcessedTransitDispatchesPropertyName, ZString.Join(ProcessedTransitDispatchesSeparator, processedTransitDispatches));
							}
						}
						else
						{
							ForwardingPackingLineCollectionReader forwardingPackingLineCollectionReader;
							var readingContext = new ForwardingPackingLineCollectionReadingContext
							{
								PackingLineDataObjectCollection = dataObject.PackingLineCollection,
								Logger = logger,
								Factory = factory,
								ShipmentBO = shipmentBO,
								ContainerLinkManager = LinkManager,
								OrderLineLinkManager = OrderLineLinkManager
							};

							forwardingPackingLineCollectionReader = new ForwardingPackingLineCollectionReader(readingContext);
							forwardingPackingLineCollectionReader.ReadIntoCollection();
							shipmentBO.UpdateInspectionTypeFromPackLines();
						}
					}
				}
				else
				{
					logger.Log(LogType.Warning, Enterprise.Freight.Forwarding.DataTransfer.Res.GetString("BCBE5979-E569-4E4B-8559-0CC469741AD3", "Can't create pack lines as the shipment is Master."));
				}
			}

			if (dataObject.EntryNumberCollection != null)
			{
				foreach (var entryNumberDataObject in dataObject.EntryNumberCollection)
				{
					var cusEntryNumCollection = shipmentBO.CusEntryNumbersForAllCountries;
					cusEntryNumCollection.Add(new EntryNumberDataObjectReader(entryNumberDataObject, logger, factory, cusEntryNumCollection, shipmentBO).ReadIntoBusinessObject());
				}
			}

			if (dataObject.AdditionalReferenceCollection != null)
			{
				var additionalReferenceCollectionReader = new ShipmentAdditionalReferenceCollectionReader<ForwardingShipment>(dataObject.AdditionalReferenceCollection, logger, factory, shipmentBO);
				additionalReferenceCollectionReader.ReadIntoCollection();
			}
		}

		static ZString[] GetProcessedTransitDispatches(ForwardingShipment shipmentBO)
		{
			var value = shipmentBO.GetUserDefinedValue<ZString>(ProcessedTransitDispatchesPropertyName);
			return value.IsEmpty ? Array.Empty<ZString>() : value.Split(ProcessedTransitDispatchesSeparator);
		}

		void AddAdditionalBookingReferences(ForwardingShipment shipmentBO)
		{
			var reference = dataObject.CoLoadBookingConfirmationReference;
			if (!reference.HasValue)
			{
				reference = dataObject.BookingConfirmationReference;
			}
			if (reference.HasValue)
			{
				var bookingConfirmationReference = shipmentBO.Numbers.GetFirstReferenceNumberByType(CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.BKG);
				if (bookingConfirmationReference == null)
				{
					bookingConfirmationReference = shipmentBO.Numbers.AddNew();
					bookingConfirmationReference.CE_RN_NKCountryCode = shipmentBO.CurrentCountryCode;
					bookingConfirmationReference.CE_EntryType = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.BKG;
				}
				bookingConfirmationReference.CE_EntryNum = reference.ToString();
			}
		}

		public bool IsTWFunctionalityEnabled(ForwardingShipment shipmentBO, DataContextType contextType, out OrgAddress matchedAddress)
		{
			matchedAddress = null;

			if (!FreightConfigurationRegistry.Instance.EnableTransitWarehouseIntegration.Value)
			{
				return false;
			}

			if (((!dataObject.DataContext?.DataSourceCollection?.Any(s => (s?.Type.GetValueOrDefault() ?? ZString.Empty) == contextType.ToString())) ?? true)
				|| dataObject.OrganizationAddressCollection == null
				|| dataObject.OrganizationAddressCollection?.FirstOrDefault(a => a.AddressType.GetValueOrDefault() == nameof(DocAddressType.LocalCartageCFS)) == null)
			{
				return false;
			}

			var warehouseAddressDO = dataObject.OrganizationAddressCollection.FirstOrDefault(a => a.AddressType.GetValueOrDefault() == nameof(DocAddressType.LocalCartageCFS));
			matchedAddress = new OrganisationDataObjectReader(warehouseAddressDO, new UniversalDataBuss.Integration.DummyLogger(), factory).GetMatched();

			if (matchedAddress?.Header == null)
			{
				throw new DataObjectReadFailureException("LocalCartageCFS could not be matched to any organisation in CW1");
			}

			var matchingAddressPK = matchedAddress.PK;
			if (matchingAddressPK == shipmentBO.ExportReceivingDepot?.PK ||
				matchingAddressPK == shipmentBO.ImportReleaseDepot?.PK ||
				shipmentBO.Consols.Cast<CommonConsol>().Any(consol => matchingAddressPK == consol.PackDepotAddress?.PK || matchingAddressPK == consol.UnpackDepotAddress?.PK))
			{
				return true;
			}

			throw new DataObjectReadFailureException("LocalCartageCFS address could not be matched to the shipment CFS address or the export consol CFS address");
		}

		#region Populate Orders

		public void PopulateOrders(ForwardingShipment shipmentBO)
		{
			PopulateAttachedOrders(shipmentBO);
			LinkWarehouseOrder(shipmentBO);
		}

		internal void PopulateAttachedOrders(ForwardingShipment shipmentBO)
		{
			var orderReadingHelper = new OrderDataObjectReadingHelper(dataObject, logger, factory, shipmentBO, OrderLineLinkManager);
			orderReadingHelper.PopulateOrders();
		}

		internal bool LinkWarehouseOrder(ForwardingShipment shipmentBO)
		{
			if (logger.TopLevelDataContext != null) // Should never be null, but many universal tests are incorrectly setup
			{
				var warehouseOrderLinker = ObjectFactory.New<IWarehouseDocketLinker>(DataContextType.WarehouseOrder, logger.TopLevelDataObject, factory);
				bool linkedNewOrder = warehouseOrderLinker.LinkDocket(shipmentBO, shipmentBO.GetUniversalDataContextManager(), logger);
				if (linkedNewOrder)
				{
					shipmentBO.GenericOrders.BuildCollection(); // rebuild the collection just in case it was already loaded.
					var limitNotification = new OrdersOnShipmentLimitHelper(shipmentBO).CreateNotification(shipmentBO.GenericOrders.Count);
					if (limitNotification != null && limitNotification.Type == NotificationType.Error)
					{
						throw new DataObjectValidationException(string.Format(CultureInfo.InvariantCulture, "{0}:\r\n{1}", shipmentBO.JS_UniqueConsignRef, limitNotification.Message));
					}

					return true;
				}
			}

			return false;
		}

		#endregion

		void SetConsignorConsigneeOrgAddresses(ForwardingShipment shipmentBO, List<OrganizationAddress> organisationCollection)
		{
			if (organisationCollection is null)
			{
				return;
			}
			var consignorDocumentaryAddress = organisationCollection.FirstOrDefault(nameof(DocAddressType.ConsignorDocumentaryAddress)) ?? organisationCollection.FirstOrDefault(nameof(DocAddressType.LocalCartageExporter));
			PopulateJobDocAddress(shipmentBO, organisationCollection, consignorDocumentaryAddress, OrganisationTypes.Consignor, DocAddressType.ConsignorDocumentaryAddress);

			var consigneeDocumentaryAddress = organisationCollection.FirstOrDefault(nameof(DocAddressType.ConsigneeDocumentaryAddress))
				?? organisationCollection.FirstOrDefault(nameof(DocAddressType.ConsigneeAddress))
				?? organisationCollection.FirstOrDefault(nameof(DocAddressType.LocalCartageImporter));
			PopulateJobDocAddress(shipmentBO, organisationCollection, consigneeDocumentaryAddress, OrganisationTypes.Consignee, DocAddressType.ConsigneeDocumentaryAddress);
		}

		void PopulateJobDocAddress(ForwardingShipment shipmentBO, List<OrganizationAddress> organisationCollection,
			OrganizationAddress orgAddress, OrganisationTypes orgType, DocAddressType docAddressType)
		{
			if (orgAddress is null)
			{
				return;
			}
			var jobDocAddress = new OrganisationDataObjectReader(orgAddress, logger, factory).GetMatchedOrNew(shipmentBO, orgType, null, docAddressType);
			if (jobDocAddress != null)
			{
				shipmentBO.DocAddresses.Add(jobDocAddress);
			}
			organisationCollection.Remove(orgAddress);
		}

		void SetConsignorConsigneePickupDeliveryAddresses(ForwardingShipment shipmentBO, List<OrganizationAddress> organisationCollection)
		{
			var consignorPickupDeliveryAddress = organisationCollection.FirstOrDefault(nameof(DocAddressType.ConsignorPickupDeliveryAddress));
			var consigneePickupDeliveryAddress = organisationCollection.FirstOrDefault(nameof(DocAddressType.ConsigneePickupDeliveryAddress));
			PopulateJobDocAddress(shipmentBO, organisationCollection, consignorPickupDeliveryAddress);
			PopulateJobDocAddress(shipmentBO, organisationCollection, consigneePickupDeliveryAddress);
		}

		void PopulateJobDocAddress(ForwardingShipment shipmentBO, List<OrganizationAddress> organisationCollection, OrganizationAddress orgAddress)
		{
			if (orgAddress is null)
			{
				return;
			}
			var jobDocAddress = new OrganisationDataObjectReader(orgAddress, logger, factory).GetMatchedOrNew(shipmentBO);
			if (jobDocAddress != null)
			{
				shipmentBO.DocAddresses.Add(jobDocAddress);
			}
			organisationCollection.Remove(orgAddress);
		}

		[SuppressMessage("Microsoft.Maintainability", "CA1502: Avoid excessive complexity")]
		void SetOrgAddresses(ForwardingShipment shipmentBO, List<OrganizationAddress> organisationCollection, bool excludeBookingParty)
		{
			if (organisationCollection is null)
			{
				return;
			}

			var localClientAddressDataObject = organisationCollection.FirstOrDefault(nameof(DocAddressType.LocalClient));
			if (localClientAddressDataObject != null)
			{
				if (!localClientAddressDataObject.RemoveAddressFromBizo(logger, shipmentBO.ShipmentJobHeader, JobHeaderSchema.JH_OA_LocalChargesAddr))
				{
					var localClientAddress = new OrganisationDataObjectReader(localClientAddressDataObject, logger, factory).GetMatched(true);
					if (localClientAddress != null)
					{
						GetJobHeaderForOrg(shipmentBO, (NoResString)"Local Client").JH_OA_LocalChargesAddr = localClientAddress.PK;  // This is only used in exception message
					}
				}
				organisationCollection.Remove(localClientAddressDataObject);
			}

			var overseasAgentAddressDataObject = organisationCollection.FirstOrDefault(nameof(DocAddressType.OverseasAgent));
			if (overseasAgentAddressDataObject != null)
			{
				if (!overseasAgentAddressDataObject.RemoveAddressFromBizo(logger, shipmentBO.ShipmentJobHeader, JobHeaderSchema.JH_OA_AgentCollectAddr))
				{
					var overseasAgentAddress = new OrganisationDataObjectReader(overseasAgentAddressDataObject, logger, factory).GetMatched();
					if (overseasAgentAddress != null)
					{
						GetJobHeaderForOrg(shipmentBO, (NoResString)"Overseas Agent").JH_OA_AgentCollectAddr = overseasAgentAddress.PK;    // This is only used in exception message
					}
				}
				organisationCollection.Remove(overseasAgentAddressDataObject);
			}

			var exportBrokerAddress = organisationCollection.FirstOrDefault(nameof(DocAddressType.ExportBroker));
			if (exportBrokerAddress != null)
			{
				SetValue(shipmentBO, null, JobShipmentSchema.JS_OH_ExportBroker, exportBrokerAddress);
				organisationCollection.Remove(exportBrokerAddress);
			}

			var importBrokerAddress = organisationCollection.FirstOrDefault(nameof(DocAddressType.ImportBroker));
			if (importBrokerAddress != null)
			{
				SetValue(shipmentBO, null, JobShipmentSchema.JS_OH_ImportBroker, importBrokerAddress);
				organisationCollection.Remove(importBrokerAddress);
			}

			var departureCFSAddress = organisationCollection.FirstOrDefault(nameof(DocAddressType.DepartureCFSAddress))
				?? (dataObject.GetMatchingDataSource(DataContextType.WarehouseOrder) != null
					? organisationCollection.FirstOrDefault(nameof(DocAddressType.DropOffAddress))
					: null);

			if (departureCFSAddress != null)
			{
				SetValue(shipmentBO, JobShipmentSchema.JS_OA_ExportReceivingDepot, null, departureCFSAddress);
				organisationCollection.Remove(departureCFSAddress);
			}

			var deliveryAgentAddress = organisationCollection.FirstOrDefault(nameof(DocAddressType.DeliveryAgent));
			if (deliveryAgentAddress != null)
			{
				SetValue(shipmentBO, null, JobShipmentSchema.JS_OH_DeliveryAgent, deliveryAgentAddress);
				organisationCollection.Remove(deliveryAgentAddress);
			}

			var shippingLineAddress = organisationCollection.FirstOrDefault(nameof(DocAddressType.ShippingLineAddress));
			if (shippingLineAddress != null)
			{
				SetValue(shipmentBO, JobShipmentSchema.JS_OA_BookedShippingLineAddress, null, shippingLineAddress);
				organisationCollection.Remove(shippingLineAddress);
			}

			var creditorAddress = organisationCollection.FirstOrDefault(nameof(DocAddressType.Creditor));
			if (creditorAddress != null)
			{
				SetValue(shipmentBO, null, JobShipmentSchema.JS_OH_Creditor, creditorAddress);
				organisationCollection.Remove(creditorAddress);
			}

			var localCartagePickupCompanyAddress = organisationCollection.FirstOrDefault(AddressTypes.PickupLocalCartage)
				?? (dataObject.GetMatchingDataSource(DataContextType.WarehouseOrder) != null
					? organisationCollection.FirstOrDefault(nameof(DocAddressType.TransportCompanyDocumentaryAddress))
					: null)
				?? organisationCollection.FirstOrDefault(nameof(DocAddressType.LocalCartagePickupFromAddress));

			if (localCartagePickupCompanyAddress != null)
			{
				SetValue(shipmentBO.DocsAndCartage, JobDocsAndCartageSchema.JP_OA_PickupCartageCoAddr, null, localCartagePickupCompanyAddress);
				organisationCollection.Remove(localCartagePickupCompanyAddress);
			}

			var controllingCustomerAddress = organisationCollection.FirstOrDefault(nameof(DocAddressType.ControllingCustomer));
			var legacyShipmentControllingPartyAddress = organisationCollection.FirstOrDefault(LegacyUniversalAddressTypes.LegacyShipmentControllingPartyAddressType);
			var controllingCustomerAddressToImport = controllingCustomerAddress ?? legacyShipmentControllingPartyAddress;

			if (controllingCustomerAddressToImport != null)
			{
				var jobDocAddress = new OrganisationDataObjectReader(controllingCustomerAddressToImport, logger, factory).GetMatchedOrNew(shipmentBO, OrganisationTypes.ControllingCustomer, null, DocAddressType.ControllingCustomer);

				if (jobDocAddress != null)
				{
					shipmentBO.DocAddresses.Add(jobDocAddress);
				}

				if (controllingCustomerAddress != null)
				{
					organisationCollection.Remove(controllingCustomerAddress);
				}

				if (legacyShipmentControllingPartyAddress != null)
				{
					organisationCollection.Remove(legacyShipmentControllingPartyAddress);
				}
			}

			if (SetAdditionalAddresses != null)
			{
				SetAdditionalAddresses(organisationCollection);
			}

			foreach (var orgAddressDataObject in organisationCollection)
			{
				if (excludeBookingParty && orgAddressDataObject.AddressType.GetValueOrDefault() == nameof(DocAddressType.BookingPartyDocumentaryAddress))
				{
					continue;
				}

				var jobDocAddress = new OrganisationDataObjectReader(orgAddressDataObject, logger, factory).GetMatchedOrNew(shipmentBO);
				if (jobDocAddress != null)
				{
					shipmentBO.DocAddresses.Add(jobDocAddress);
				}
			}
		}

		JobHeader GetJobHeaderForOrg(ForwardingShipment shipmentBO, string orgTypeDesc)
		{
			var errorMessage = shipmentBO.CreateShipmentJobHeaderWithMutex();
			var jobHeader = shipmentBO.ShipmentJobHeader
				?? throw new DataObjectReadFailureException(string.Format(CultureInfo.InvariantCulture, "Could not save {0} Organization - Failed to create Shipment JobHeader with mutex. Message : {1}", orgTypeDesc, errorMessage));

			if (!jobHeader.IsInDatabase && jobHeader.Department == null)
			{
				jobHeader.DisposeAndDeleteNew(); // Removes the Mutex for a newly added JobHeader
				throw new DataObjectReadFailureException(string.Format(CultureInfo.InvariantCulture, "Could not save {0} Organization - Must have an Origin, Destination and Transport Mode to be able to calculate a Department for a Job Costing record, and the {0} is saved on the Job Costing record.", orgTypeDesc));
			}

			return jobHeader;
		}

		internal void SetValue(BusinessObject shipmentBO, SchemaGuidColumn orgAddress, SchemaGuidColumn orgHeader, OrganizationAddress addressDataObject)
		{
			if (addressDataObject.RemoveAddressFromBizo(logger, shipmentBO, orgAddress, orgHeader))
			{
				return;
			}

			var addressBO = new OrganisationDataObjectReader(addressDataObject, logger, factory).GetMatched();
			if (addressBO != null)
			{
				if (orgAddress != null)
				{
					shipmentBO[orgAddress] = addressBO.PK;
				}

				if (orgHeader != null)
				{
					shipmentBO[orgHeader] = addressBO.OA_OH;
				}
			}
		}

		internal delegate void SetAdditionalAddressesDelegate(List<OrganizationAddress> organisationCollection);

		internal SetAdditionalAddressesDelegate SetAdditionalAddresses { get; set; }

		public Action<Date> SetDate { get; set; }

		#region ITopLevelDataObjectReader Members

		InvalidOperationException GetTopLevelDataReaderException() => new InvalidOperationException(
				"This method should never be called. ITopLevelDataObjectReader is only implemented so that the framework allows us to update a TopLevelDataObject (UniversalShipment).");

		BusinessObject ITopLevelDataObjectReader.GetExistingBusinessObject()
		{
			throw GetTopLevelDataReaderException();
		}

		public void ReadIntoBusinessObject(ref BusinessObject targetBO)
		{
			throw GetTopLevelDataReaderException();
		}

		public BusinessObject ReadIntoTopLevelBusinessObject()
		{
			throw GetTopLevelDataReaderException();
		}

		public IEnumerable<(string KeyValue, string KeySource)> ReadKeysForParallelism()
		{
			throw GetTopLevelDataReaderException();
		}

		#endregion
	}

	public class ChildShipmentsParent
	{
		ChildShipmentsParent(ForwardingShipment[] shipments, ForwardingConsol consol)
		{
			this.Shipments = shipments;
			this.Consol = consol;
		}

		internal ForwardingConsol Consol
		{
			get;
			private set;
		}

		internal readonly ForwardingShipment[] Shipments;

		public static ChildShipmentsParent ToChildShipmentsParent(ForwardingConsol consol)
		{
			return new ChildShipmentsParent(consol.TopLevelShipments.ToArray<ForwardingShipment>(), consol);
		}

		[SuppressMessage("Microsoft.Usage", "CA2225:OperatorOverloadsHaveNamedAlternates")]
		public static ChildShipmentsParent ToChildShipmentsParent(ForwardingShipment shipment)
		{
			return new ChildShipmentsParent(shipment.CoLoadShipments.ToArray<ForwardingShipment>(), null);
		}
	}
}

