using System.Diagnostics.CodeAnalysis;

namespace Enterprise.Tracking.Business
{
	[SuppressMessage("Microsoft.Design", "CA1052:StaticHolderTypesShouldBeSealed")]
	public static class WebTracker
	{
		//WARNING: Modification of page names will affect grids column layouts
		#region Pages

		[SuppressMessage("Microsoft.Design", "CA1052:StaticHolderTypesShouldBeSealed")]
		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Identifier")]
		public static class Pages
		{
			public const string BookingDetails = "BookingDetails";
			public const string Bookings = "Bookings";
			public const string CartageDetails = "CartageDetails";
			public const string Cartages = "Cartages";
			public const string ContainerDetails = "ContainerDetails";
			public const string Containers = "Containers";
			public const string LinerAndAgencyContainerDetails = "LinerAndAgencyContainerDetails";
			public const string LinerAndAgencyContainers = "LinerAndAgencyContainers";
			public const string DeclarationDetails = "DeclarationDetails";
			public const string Declarations = "Declarations";
			public const string EditImporterSecurityFiling = "EditISF";
			public const string ImporterSecurityFiling = "ISFs";
			public const string ImporterSecurityFilingDetails = "ISFDetails";
			public const string OrderDetails = "OrderDetails";
			public const string Orders = "Orders";
			public const string Quotations = "Quotations";
			public const string FlightSchedules = "FlightSchedules";
			public const string RailSchedules = "RailSchedules";
			public const string RoadSchedules = "RoadSchedules";
			public const string SailingSchedules = "SailingSchedules";
			public const string ShipmentDetails = "ShipmentDetails";
			public const string Shipments = "Shipments";
			public const string CFSShipmentDetails = "CFSShipmentDetails";
			public const string CFSShipments = "CFSShipments";
			public const string BillOfLadingDetails = "BillOfLadingDetails";
			public const string BillsOfLading = "BillsOfLading";
			public const string LinerAndAgencyBookingDetails = "LinerAndAgencyBookingDetails";
			public const string LinerAndAgencyBookings = "LinerAndAgencyBookings";
			public const string WarehouseInventory = "WarehouseInventory";
			public const string WarehouseInventoryDetails = "WarehouseInventoryDetails";
			public const string OrgSupplierParts = "OrgSupplierParts";
			public const string ProductDetails = "ProductDetails";
			public const string ProductImage = "ProductImage";
			public const string WarehouseOrderDetails = "WarehouseOrderDetails";
			public const string WarehouseOrders = "WarehouseOrders";
			public const string WarehouseReceipts = "WarehouseReceipts";
			public const string WarehouseReceiveDetails = "WarehouseReceiveDetails";
			public const string EditBillOfLading = "EditBillOfLading";
			public const string EditBooking = "EditBooking";
			public const string EditLinerAndAgencyBooking = "EditLinerAndAgencyBooking";
			public const string ChangePassword = "ChangePassword";
			public const string ResetPassword = "ResetPassword";
			public const string SetPassword = "SetPassword";
			public const string ContainerBatchUpdate = "ContainerBatchUpdate";
			public const string ContainerSummary = "ContainerSummary";
			public const string Default = "Default";
			public const string EditContainer = "EditContainer";
			public const string EditLinerAndAgencyContainer = "EditLinerAndAgencyContainer";
			public const string EditOrder = "EditOrder";
			public const string EditWarehouseOrder = "EditWarehouseOrder";
			public const string EditWarehouseReceive = "EditWarehouseReceive";
			public const string ForgotPassword = "ForgotPassword";
			public const string Quotation = "Quotation";
			public const string SwitchCompany = "SwitchCompany";
			public const string TermsAndConditions = "TermsAndConditions";
			public const string Login = "Login";
			public const string LoginSuperseded = "LoginSuperseded";
			public const string SetMasterPassword = "SetMasterPassword";
			public const string ResetMasterPassword = "ResetMasterPassword";
			public const string LoginRedirection = "LoginRedirection";
			public const string Transactions = "Transactions";
			public const string WarehouseOrderLineAllocation = "WarehouseOrderLineAllocation";
			public const string eDocAttach = "eDocAttach";
			public const string MAWBs = "MAWBs";
			public const string MAWBDetails = "MAWBDetails";
			public const string HAWBs = "HAWBs";
			public const string HAWBDetails = "HAWBDetails";
			public const string HAWBList = "HAWBList";
			public const string EDIMessages = "EDIMessages";
			public const string ViewTermsAndConditions = "ViewTermsAndConditions";
		}

		#endregion

		//WARNING: Never change grids column enumerators order - it will cause incorrect grid layouts
		//WARNING: New columns should be added to the end of each enumerator
		#region Grid Columns

		[SuppressMessage("Microsoft.Design", "CA1052:StaticHolderTypesShouldBeSealed")]
		public static class Grids
		{
			#region PackLines

			public enum PackLines : int
			{
				Pieces = 0,
				PackType,
				Length,
				Width,
				Height,
				UD,
				Weight,
				WeightUQ,
				Volume,
				VolumeUQ,
				Description,
				MarksAndNumbers,
				LinePrice,
				Currency,
				TariffNum,
				Container,
				Products
			}

			#endregion

			#region TrackingBookings

			public enum TrackingBookings : int
			{
				BookingNumber = 0,
				Description,
				ShipperReference,
				Origin,
				Destination,
				Packs,
				Weight,
				WeightUnit,
				Volume,
				VolumeUnit,
				GoodsValue,
				Currency,
				Canceled,
				EstimatedPickup,
				PickupRequiredBy,
				EstimatedDelivery,
				DeliveryRequiredBy,
				DeliveryDate,
				ServiceLevel,
				OrderReferences,
				Vessel,
				Voyage,
				MAWB,
				Consignee,
				Consignor,
				DepotCutOff,
				CFSReference,
				AdditionalTerms,
				INCO,
				ChargesApply,
				ReleaseType,
				OnBoard,
				DeliveryAgent,
				PickupAgent
			}

			#endregion

			#region Milestones

			public enum Milestones : int
			{
				LastMilestoneDescription = 1000,
				LastMilestoneDate,
				NextMilestoneDescription,
				NextMilestoneDate,
				Description,
				Status,
				Date,
				ParentCode
			}

			#endregion

			#region DangerousGoods

			public enum DangerousGoods : int
			{
				Code = 0,
				UNNumber,
				Variant,
				Variation,
				ProperShippingName,
				USDOTName,
				EMS,
				IMOClass,
				StowageRequirements,
				Active,
				System,
				State,
				FlashPoint,
				PackingGroup,
				StowageCategory,
				TankProvisions,
				PackingProvisions,
				TreatAs,
				TechnicalName
			}

			#endregion

			#region TrackingContainers

			public enum TrackingContainers : int
			{
				ContainerNumber = 0,
				ShipmentNumber,
				Type,
				Mode,
				Packs,
				Departure,
				Arrival,
				Quarantine,
				Available,
				LastFreeDay,
				DetentionStarts,
				TimeSlot,
				LocalTransportReference,
				RequiredDelivery,
				ConfirmedDelivery,
				ActualDelivery,
				Deliver,
				EmptyReady,
				EmptyPickup,
				ActualDehire,
				Vessel,
				Voyage,
				DeliverySequence,
				ShipmentStatuses,
				StatusDescription,
				SealNumber,
				ContainersCount,
				TareWeight,
				Weight,
				DeliveryMode,
				EstimatedDelivery,
				EstimatedReturn,
				ActualReturn,
				Palletized,
				Chargeable,
				Items,
				Pallets,
				StorageBegins,
				NetWeight,
				GrossWeight,
				WQ,
				Commodity,
				IsShipperOwned,
				EmptyPickupFrom,
				EmptyReleased,
				WharfGateIn,
				Loaded,
				EmptyReturnTo,
				EmptyReturnBy,
				Unloaded,
				WharfGateOut,
				EmptyReturned,
				ContainerStatus,
				VerifiedDate,
				VerifiedMethod,
				VerifiedCompany,
				VerifiedContact,
				VerifiedEmail,
				VerifiedPhone
			}

			#endregion

			#region LinerAndAgencyContainers

			public enum LinerAndAgencyContainers : int
			{
				ContainerNumber = 0,
				ShipmentNumber,
				TypeDescription,
				Mode,
				Packs,
				RequiredDelivery,
				ActualDelivery,
				Deliver,
				EmptyReady,
				EmptyReturnRequired,
				ActualDehire,
				Vessel,
				Voyage,
				SealNumber,
				ContainersCount,
				TareWeight,
				NetWeight,
				GrossWeight,
				WQ,
				Commodity,
				IsShipperOwned,
				EmptyPickupFrom,
				EmptyReleased,
				WharfGateIn,
				Loaded,
				EmptyReturnTo,
				EmptyReturnBy,
				Unloaded,
				WharfGateOut,
				EmptyReturned,
				ContainerStatus,
				VerifiedDate,
				VerifiedMethod,
				VerifiedCompany,
				VerifiedContact,
				VerifiedEmail,
				VerifiedPhone
			}

			#endregion

			#region TrackingOrders

			public enum TrackingOrders : int
			{
				OrderNumber = 0,
				SplitNumber,
				TransportMode,
				Supplier,
				Buyer,
				ControllingCustomer,
				Status,
				OrderDate,
				Origin,
				Destination,
				CurrentVessel,
				CurrentVoyage,
				Packs,
				Volume,
				Weight,
				RequiredExWorks,
				RequiredInStore,
				ExFactory,
				OriginReceival,
				Departure,
				Arrival,
				ClearanceCommenced,
				ClearanceFinalized,
				Unpacked,
				LocalTransportAdvised,
				Delivered,
				HouseBill,
				MasterBill,
				Load,
				Discharge,
				PickupAddress,
				DeliveryAddress,
				ConsolNumber,
				BookingConfRef,
				ContainerNumber,
				InvoiceNumber,
				ProductNumber,
				ShipmentNumber,
				ConfirmedDate,
				FollowUpDate,
				SendingAgent,
				ReceivingAgent,
				ServiceLevel,
				ContainerMode,
				CreatedOn,
				CreatedTime,
				LastEditTime,
				MainVessel,
				MainVoyage,
				GoodsDescription,
				PlannedContainers,
				IncoTerm,
				AdditionalTerms
			}

			#endregion

			#region TrackingOrderLines

			public enum TrackingOrderLines : int
			{
				OrderLineNumber = 0,
				OrderNumber,
				LineNumber,
				LineSplitNumber,
				PartNumber,
				Quantity,
				UnitOfQuantity,
				ShipmentNumber,
				HouseBill,
				Supplier,
				ProductNumber,
				CustomAttribute1,
				Description,
				ContainerQuantity,
				Packs,
				PartAttribute1,
				PartAttribute2,
				PartAttribute3,
				InnerPacks,
				OuterPacks,
				QuantityOrdered,
				QuantityInvoiced,
				QuantityReceived,
				QuantityRemaining,
				ItemPrice,
				TotalPrice,
				LineStatus,
				RequiredDate,
				INCOTerm,
				AdditionalTerms,
				InnerPacksUQ,
				OuterPacksUQ,
				ConfirmNumber,
				ConfirmDate,
				RequiredExWorksDate,
				InvoiceNumber,
				ContainerNumber,
				SerialNumber
			}

			#endregion

			#region TrackingQuotations

			public enum TrackingQuotations : int
			{
				QuoteNumber = 0,
				Company,
				QuoteStatus,
				QuoteDate,
				ExpiryDate,
				TransportMode,
				Origin,
				Destination,
				Volume,
				VolumeUnit,
				Weight,
				WeightUnit
			}

			#endregion

			#region TrackingSchedules

			public enum TrackingSchedules : int
			{
				Reference = 0,
				Vessel,
				LoadPort,
				DischargePort,
				LCLCutOff,
				ETD,
				ETA,
				LCLAvailabilityDate,
				DocumentaryCutoff,
				Carrier,
				Chartered,
				LCLReceivalCommences,
				LCLStorageDate,
				ReservedMasterBill,
				DepartureBerth,
				ArrivalBerth,
				DepartureReference,
				ArrivalReference,
				ATD,
				ATA,
				Tranship,
				Type,
				FCLCutOff,
				FCLReceivalCommences,
				FCLAvailabilityDate,
				FCLStorageDate
			}

			#endregion

			#region TrackingShipments

			public enum TrackingShipments : int
			{
				ShipmentNumber = 0,
				HouseBill,
				Shipper,
				Consignee,
				Origin,
				ETD,
				Destination,
				ETA,
				CurrentLoadPort,
				CurrentDischargePort,
				CurrentVessel,
				CurrentVoyage,
				BookingReference,
				OwnerReference,
				Mode,
				Packs,
				Weight,
				Volume,
				GoodsValue,
				Currency,
				GoodsDescription,
				EstimatedPickup,
				PickupRequiredBy,
				EstimatedDelivery,
				DeliveryRequiredBy,
				DeliveryDate,
				ServiceLevel,
				Charges,
				ShipperFullAddress,
				ShipperAddress,
				ShipperCity,
				ShipperState,
				ShipperPostCode,
				ConsigneeFullAddress,
				ConsigneeAddress,
				ConsigneeCity,
				ConsigneeState,
				ConsigneePostCode,
				ReceivedDate,
				ReceivedBy,
				PiecesReceived,
				BookedOnline,
				ActualPickup,
				Containers,
				OrderReferences,
				MainLoadPort,
				MainDischargePort,
				MainVessel,
				MainVoyage,
				Type,
				Inspection,
				AdditionalTerms,
				INCO,
				LoadingMeters,
				ContainerMode,
				ChargesApply,
				ReleaseType,
				OnBoard,
				DeliveryAgent,
				PickupAgent,
				DeclarationCountry,
				StorageDate,
				TEUCount,
				JobNotes,
				FirstLegLoadETD,
				FirstLegLoadATD,
				LastLegDischargeETA,
				LastLegDischargeATA,
			}

			#endregion

			#region CFSShipments

			public enum CFSShipments : int
			{
				ShipmentNumber = 0,
				HouseBill,
				Shipper,
				Consignee,
				Origin,
				ETD,
				Destination,
				ETA,
				CurrentLoadPort,
				CurrentDischargePort,
				CurrentVessel,
				CurrentVoyage,
				BookingReference,
				Mode,
				Packs,
				Weight,
				Volume,
				GoodsDescription,
				EstimatedPickup,
				PickupRequiredBy,
				EstimatedDelivery,
				DeliveryRequiredBy,
				DeliveryDate,
				ServiceLevel,
				Charges,
				ShipperFullAddress,
				ShipperAddress,
				ShipperCity,
				ShipperState,
				ShipperPostCode,
				ConsigneeFullAddress,
				ConsigneeAddress,
				ConsigneeCity,
				ConsigneeState,
				ConsigneePostCode,
				ReceivedDate,
				ReceivedBy,
				PiecesReceived,
				BookedOnline,
				ActualPickup,
				MainLoadPort,
				MainDischargePort,
				MainVessel,
				MainVoyage,
				Type,
				AdditionalTerms,
				INCO,
				LoadingMeters,
				ChargesApply,
				ReleaseType,
				OnBoard,
				DeliveryAgent,
				PickupAgent,
				ClientRef,
				InterimReceipt,
				WhsReceipt,
				EntryNo,
				WhsLocation,
				MasterBill
			}

			#endregion

			#region TrackingDeclarations

			public enum TrackingDeclarations : int
			{
				JobNumber = 0,
				Branch,
				Type,
				Transport,
				DeclarationReference,
				Vessel,
				Voyage,
				DateOfArrival,
				Origin,
				FinalDestination,
				HouseBill,
				Supplier,
				Importer,
				ImporterCode,
				SupplierCode,
				AgentsReference,
				ContainerMode,
				Containers,
				DateOfFirstArrival,
				EFTMode,
				EntryAuthorisationDate,
				EntryStatus,
				EntrySubmittedDate,
				ExportDate,
				ExportGoodsType,
				GoodsDescription,
				MasterBill,
				SubType,
				OwnerRef,
				Arrival,
				FirstArrival,
				Loading,
				TotalPacks,
				PackType,
				EntryNumber,
				EarliestCustomsEntry,
				EntryStatusDescription,
				OrderReferences,
				Volume,
				VolumeUnit,
				Weight,
				WeightUnit,
				MessageStatus,
				CargoStatus,
				CargoStatusDescription,
				DateCreated,
				Broker,
				ReleaseDate,
				ReleaseStatusDesc,
				EntryPort,
				ENSStatusDescription,
				Paperless,
				StatementNumber,
				FilerCode,
				PaymentType,
				PaymentDueDate,
				StatementPaidDate,
				PaymentStatus,
				EntryType,
				PeriodicStatementMonth,
				TotalEnteredValue,
				TotalPayable,
				TotalInvoiced,
				TotalOutstanding,
				FDAStatus,
				FDAStatusDescription,
				AuditDate,
				EIStatusDesc,
				ITNumber,
				LiquidationDate,
				ReconIssue,
				ReconIssueDesc,
				UltimateConsigneeName,
				Top3Containers,
				Country,
				PGAStatus,
				PGAStatusDescription,
				CA_AcceptedDate,
				CA_AccountingDate,
				TEUCount,
				TSWStatus,
				CarrierSCAC
			}

			#endregion

			#region TrackingImporterSecurityFilings

			public enum TrackingImporterSecurityFilings : int
			{
				JobReference = 0,
				CustomsReference,
				Importer,
				HouseBill,
				Status,
				SellingParty,
				BuyingParty,
				MainShipToParty,
				CarrierSCAC,
				IDType,
				ImporterIDType,
				ImporterIdentification,
				DateOfBirth,
				IssueCountry,
				UnloadPort,
				DeliveryPort,
				BondHolder,
				SuretyCode,
				ConsigneeIDType,
				ConsigneeID,
				OceanBill,
				MasterBill,
				FirstUSRouteVessel,
				FirstUSRouteVoyage,
				FirstUSRouteLoadPort,
				FirstUSRouteDischargePort,
				FirstUSRouteETD,
				FirstUSRouteETA,
				FirstUSRouteATD,
				FirstUSRouteATA,
				ActionReasonCode,
				FirstAcceptedDate,
				LastAcceptedDate,
				CreatedTime
			}

			#endregion

			#region TrackingUSCForeignPorts

			public enum TrackingUSCForeignPorts : int
			{
				Code = 0,
				Name
			}

			#endregion

			#region TrackingUSCRegionDistrictPorts

			public enum TrackingUSCRegionDistrictPorts : int
			{
				Code = 0,
				Name
			}

			#endregion

			#region LinerAndAgencyBillOfLadings

			public enum LinerAndAgencyBillOfLadings : int
			{
				ShipmentNumber = 0,
				OceanBill,
				BookingParty,
				Shipper,
				Consignee,
				Status,
				Vessel,
				Voyage,
				Origin,
				Load,
				DischargePort,
				Destination,
				CargoDescription,
				Packs,
				Type,
				Weight,
				WeightUnit,
				Volume,
				VolumeUnit,
				CargoType,
				ETD,
				ETA,
				ConsignorContact,
				ShippersRef,
				InterimNumber,
				Booked,
				LCLReceivalCommences,
				LCLCutOff,
				FCLReceivalCommences,
				FCLCutOff,
				PaymentTerms
			}

			#endregion

			#region LinerAndAgencyBookings

			public enum LinerAndAgencyBookings : int
			{
				ShipmentNumber = 0,
				OceanBill,
				Consignor,
				Consignee,
				Status,
				Vessel,
				Voyage,
				Origin,
				Load,
				DischargePort,
				Destination,
				CargoDescription,
				Packs,
				Type,
				Weight,
				WeightUnit,
				Volume,
				VolumeUnit,
				CargoType,
				ETD,
				ETA,
				ConsignorContact,
				ShippersRef,
				Booked,
				LCLReceivalCommences,
				LCLCutOff,
				FCLReceivalCommences,
				FCLCutOff,
				JobStatus
			}

			#endregion

			#region TrackingCartages

			public enum TrackingCartages : int
			{
				JobID = 0,
				Type,
				FirstAddress,
				SecondAddress,
				ThirdAddress,
				FourthAddress,
				LocalClient,
				ReferenceNumber,
				QuoteNumber,
				WaybillNumber,
				Description,
				CompDate,
				Vessel,
				Voyage,
				FCLStorageDate,
				FCLAvailabilityDate,
				FCLReceivalCommences,
				FCLCutOff,
				DropMode,
				ContainerNumber,
				ServiceLevel,
				JobStatus,
				SailingATA,
				SailingATD,
				LCLStorageDate,
				LCLAvailabilityDate,
				LCLReceivalCommences,
				LCLCutOff,
				EstimatedTimeOfArrival,
				EstimatedTimeOfDeparture
			}

			#endregion

			#region TrackingCartageLegs

			public enum TrackingCartageLegs : int
			{
				Pickup = 0,
				Delivery,
				ContainerNumber,
				PlannedPickupTime,
				DeliveryPlanned,
				PickupTimeIn,
				PickupTimeOut,
				DeliverTimeIn,
				DeliverTimeOut,
				LegNotes
			}

			#endregion

			#region TrackingCartageLooseBooking

			public enum TrackingCartageLooseBooking : int
			{
				Order = 0,
				Packs,
				Type,
				Weight,
				WeightUnit,
				Volume,
				VolumeUnit,
				FirstParty,
				SecondParty,
				DropMode
			}

			#endregion

			#region OrgSupplierParts

			public enum OrgSupplierParts : int
			{
				ProductNumber = 0,
				Description
			}

			#endregion

			#region TrackingInventory

			public enum TrackingInventory : int
			{
				Warehouse = 0,
				Product,
				Description,
				HasProductImage,
				AvailableToPickQuantity,
				ReservedQuantity,
				CommittedQuantity,
				TotalQuantity,
				ClientQuantity,
				ProductWeight,
				TotalWeight,
				WeightUnit,
				ProductVolume,
				TotalVolume,
				VolumeUnit,
				TotalValue,
				Currency,
				LastCost,
				Inventories
			}

			#endregion

			#region TrackingInventoryDetails

			public enum TrackingInventoryDetails : int
			{
				ReceiptReference = 0,
				Product,
				ETA,
				Status,
				AvailableToPickQuantity,
				CommittedQuantity,
				ReservedQuantity,
				TotalQuantity,
				TotalValue,
				Currency,
				PartAttribute1,
				PartAttribute2,
				PartAttribute3,
				ExpiryDate,
				PackingDate,
				LastCost,
				SerialNumber
			}

			#endregion

			#region TrackingWarehouses

			public enum TrackingWarehouses : int
			{
				WarehouseName = 0,
				Code
			}

			#endregion

			#region TrackingWarehouseOrders

			public enum TrackingWarehouseOrders : int
			{
				OrderNumber = 0,
				Warehouse,
				Consignee,
				TransportCompany,
				TransportReference,
				DocketNumber,
				RequiredDate,
				Status,
				Units,
				Weight,
				Cubic,
				FinalisedDate,
				CustomerReference
			}

			#endregion

			#region TrackingWarehouseReceives

			public enum TrackingWarehouseReceives : int
			{
				ReceiveNumber = 0,
				Warehouse,
				DocketNumber,
				BookingDate,
				ETA,
				ArrivalDate,
				Status,
				TotalUnits,
				TotalPallets,
				FinalisedDate,
				CustomerReference
			}

			#endregion

			#region TrackingTransports

			public enum TrackingTransports : int
			{
				Leg = 0,
				Mode,
				Type,
				Parent,
				Bill,
				Vessel,
				Voyage,
				Load,
				Discharge,
				Departure,
				Arrival,
				Status,
				Carrier
			}

			#endregion

			#region DeliveryInformation

			public enum DeliveryInformation : int
			{
				Dispatched = 0,
				Driver,
				TransportCompanyName,
				VehicleRegistration,
				GatePassID
			}

			#endregion

			#region TrackingPackLines

			public enum TrackingPackLines : int
			{
				Pieces = 0,
				PackType,
				Length,
				Width,
				Height,
				UnitOfDimension,
				Weight,
				WeightUnit,
				Volume,
				VolumeUnit,
				Description,
				MarksAndNumbers,
				LinePrice,
				Currency,
				TariffNumber,
				ContainerNumber,
				MultipleProducts,
				ReferenceNumber,
				CustomAttribute1,
				CustomAttribute2,
				CustomAttribute3,
				CustomAttribute4,
				LoadingMeters
			}

			#endregion

			#region TrackingImporterSecurityFilingLines

			public enum TrackingImporterSecurityFilingLines : int
			{
				Origin = 0,
				Tariff,
				Manufacturer,
				Product
			}

			#endregion

			#region Documents

			public enum Documents : int
			{
				Date = 0,
				Description,
				Type,
				View
			}

			#endregion

			#region CustomsDisposition

			public enum CustomsDisposition : int
			{
				Code = 0,
				Narrative,
				Date,
				ReleaseDate,
				ReleaseOrigin,
				ReleaseOriginDescription
			}

			#endregion

			#region CustomsEntrySummaryStatus

			public enum CustomsEntrySummaryStatus : int
			{
				ErrorID = 0,
				NarrativeMessage,
				StatusDate
			}

			#endregion

			#region CustomsPGA

			public enum CustomsPGAStatus : int
			{
				AgencyCode = 0,
				StatusCode,
				StatusDescription,
				StatusDate,
				Notes,
			}

			public enum CustomsPGALine : int
			{
				PGA = 0,
				Date,
				Status,
				StatusMessage,
				DispositionCode,
				DispositionCodeDescription,
				DispositionBeginningCBPLine,
				DispositionBeginningPGALine,
				Range,
				DispositionEndPGALine,
				DispositionEndCBPLine
			}

			#endregion

			#region CustomsEntriesData

			public enum CustomsEntriesData : int
			{
				ReferenceNumber = 0,
				EntryNumber,
				MessageStatus,
				EntryAdvice
			}

			#endregion

			#region CustomsInvoice

			public enum CustomsInvoice : int
			{
				InvoiceNumber = 0,
				SupplierName,
				ImporterName,
				InvoiceTerms,
				TotalAmount,
				Currency,
				InvoiceDate,
				InvoiceLines
			}

			#endregion

			#region ARAPInvoicing

			public enum ARAPInvoicing : int
			{
				InvoiceNumber = 0,
				ConsignorName,
				ConsigneeName,
				LastRequested,
				Issuer,
				Type,
				Terms,
				InvoiceDate,
				DueDate,
				Currency,
				Amount,
				OutstandingAmount,
				PaidDate,
				ComplianceNumber
			}

			#endregion

			#region ARAPInvoicingLine

			public enum ARAPInvoicingLine : int
			{
				Description = 0,
				Currency,
				ExTaxAmount,
				TaxAmount,
				TotalAmount
			}

			#endregion

			#region ISFReference

			public enum ISFReference : int
			{
				Description = 0,
				BillNumber,
				BillStatus
			}

			#endregion

			#region ISFContainer

			public enum ISFContainer : int
			{
				DescriptionCode = 0,
				ContainerNumber,
				ISO
			}

			#endregion

			#region TrackingAddress

			public enum TrackingAddress : int
			{
				AddressType = 0,
				AddressOverride,
				CompanyName,
				AddressLine1,
				Country,
				State,
				City,
				Post,
				Contact,
				Phone,
				Fax,
				Email
			}

			#endregion

			#region ISFLine

			public enum ISFLine : int
			{
				Origin = 0,
				Tariff,
				Manufacturer,
				Product
			}

			#endregion

			#region PlannedVoyage

			public enum PlannedVoyage : int
			{
				VoyageType = 0,
				Vessel,
				Voyage,
				ETD,
				ETA
			}

			#endregion

			#region UnitConversion

			public enum UnitConversion : int
			{
				QuantityInParent = 0,
				Package,
				ParentPackage
			}

			#endregion

			#region ParamsByWarehouseAndClient

			public enum ParamsByWarehouseAndClient : int
			{
				Warehouse = 0,
				StockTakeCycle,
				ExpiryNotificationPeriod,
				ReplenMinimum,
				EconomicQuantity,
				UQ
			}

			#endregion

			#region WarehouseDocketReference

			public enum WarehouseDocketReference : int
			{
				ReferenceType = 0,
				Reference
			}

			#endregion

			#region WarehouseDocketLine

			public enum WarehouseDocketLine : int
			{
				Product = 0,
				Description,
				Packs,
				PackTypes,
				OrderedQuantity,
				UQ,
				Reserved,
				ReleaseDetails,
				Quantity,
				PartAttribute1,
				PartAttribute2,
				PartAttribute3
			}

			#endregion

			#region WarehouseReceiveLine

			public enum WarehouseReceiveLine : int
			{
				LineNo = 0,
				Product,
				Description,
				Packs,
				PackTypes,
				OrderedQuantity,
				UQ,
				Reserved,
				ReleaseDetails,
				Quantity,
				PartAttribute1,
				PartAttribute2,
				PartAttribute3,
				SerialNumber,
				ExpiryDate
			}

			#endregion

			#region MAWB

			public enum MAWB : int
			{
				MAWB = 0,
				Origin,
				DepartureAndRouting,
				Destination,
				DestinationName,
				IssueDate,
				IssuePlace,
				To1st,
				By1st,
				To2nd,
				By2nd,
				To3rd,
				By3rd,
				FirstCarrier,
				FirstFlight,
				FirstFlightDate,
				SecondCarrier,
				SecondFlight,
				SecondFlightDate,
				Currency,
				ChargeCode,
				WtVal,
				DeclaredValue,
				CustomsValue,
				ShipperName,
				ShipperAddress,
				ShipperAddress2,
				ShipperPlace,
				ShipperState,
				ShipperCountryCode,
				ShipperPostCode,
				ShipperContactDetail,
				ConsigneeName,
				ConsigneeAddress,
				ConsigneeAddress2,
				ConsigneePlace,
				ConsigneeState,
				ConsigneeCountryCode,
				ConsigneePostCode,
				ConsigneeContactDetail,
				AWBStatusDescription
			}

			#endregion

			#region HAWB

			public enum HAWB : int
			{
				HAWB = 0,
				Origin,
				Destination,
				Pieces,
				ActWeight,
				WeightUQ,
				SLAC,
				ShipperName,
				ShipperAddress,
				ShipperAddress2,
				ShipperPlace,
				ShipperState,
				ShipperCountryCode,
				ShipperPostCode,
				ShipperContactDetail,
				ConsigneeName,
				ConsigneeAddress,
				ConsigneeAddress2,
				ConsigneePlace,
				ConsigneeState,
				ConsigneeCountryCode,
				ConsigneePostCode,
				ConsigneeContactDetail,
				AWBStatusDescription
			}

			#endregion

			#region EDIMessage

			public enum EDIMessage : int
			{
				MessageNo = 0,
				MessageType,
				Status,
				DateTimeCreated,
				MessageText
			}

			#endregion

			#region TrackingAWBAccountingInfo

			public enum TrackingAWBAccountingInfo : int
			{
				Code = 0,
				Information
			}

			#endregion

			#region TrackingAWBSpecialHandling

			public enum TrackingAWBSpecialHandling : int
			{
				Code = 0,
				Description
			}

			#endregion

			#region TrackingAWBOtherCharges

			public enum TrackingAWBOtherCharges : int
			{
				ChargeCode = 0,
				Entitlement,
				Description,
				Amount
			}

			#endregion

			#region CustomsContainer

			public enum CustomsContainer : int
			{
				ContainerNumber = 0,
				SealNumber,
				BookingReference,
				Type,
				Mode,
				Weight,
				WeightUnits,
				ISO
			}

			#endregion

			#region Event

			public enum Event : int
			{
				Code = 0,
				Time,
				Description,
				EventDetails,
				Source,
				IsEstimate
			}

			#endregion

			#region ReferenceNumbers

			public enum ReferenceNumbers : int
			{
				Country = 0,
				NumberType,
				Number,
				TypeDescription,
				IssueDate,
				Information
			}

			#endregion

			public enum CusDecHouseBills : int
			{
				BillType = 0,
				BillNum,
				HBLIssueDate,
				ParentBill,
				ManifestQty,
				UQ,
				ConsignRefNo,
				BillIssuerSCAC,
				ISFBillStatus,
				ISFBillStatusDescription,
				ITNumber,
				IsSplit
			}
		}

		#endregion

		#region Upload

		[SuppressMessage("Microsoft.Design", "CA1053:StaticHolderTypesShouldNotHaveConstructors")]
		public class Upload
		{
			public enum UploadStep : int
			{
				ValidateFile = 1,
				ImportFile = 2,
				ShowResults = 3
			}
		}

		#endregion

		#region Visibility

		[SuppressMessage("Microsoft.Design", "CA1053:StaticHolderTypesShouldNotHaveConstructors")]
		public class Visibility
		{
			[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Visibility Status")]
			public const string False = "False";
			[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Visibility Status")]
			public const string True = "True";
		}

		#endregion
	}
}
