using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DataObjects;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	sealed class CarrierMessageData : DocDataObject, IDataSourceProvider
	{
		public CarrierMessageData(ZString sourceType, ZString sourceID, ZString documentName)
		{
			SourceType = sourceType;
			SourceID = sourceID;
			DocumentName = documentName;
		}

		#region IDataSourceProvider members

		public ZString SourceID
		{
			get => sourceID;
			set
			{
				sourceID = value;
				SourceIDInfo.RefreshBinding();
			}
		}
		ZString sourceID;

		public ZPropertyInfo SourceIDInfo => GetZPropertyInfo(nameof(SourceID));

		public ZString SourceType { get; }

		public ZString DocumentName { get; }

		#endregion

		#region FreightForwarderReference

		public ZString FreightForwarderReference
		{
			get => freightForwarderReference;
			set
			{
				if (SetNonPersistentPropertyValue(FreightForwarderReferenceInfo, ref freightForwarderReference, value))
				{
					Validate(FreightForwarderReferenceInfo);
				}
			}
		}
		ZString freightForwarderReference;

		public ZPropertyInfo FreightForwarderReferenceInfo => GetZPropertyInfo(nameof(FreightForwarderReference));

		#endregion

		#region TransportMode

		public CodeDescription TransportMode
		{
			get => transportMode;
			set => transportMode = SetChild(TransportMode, value);
		}
		CodeDescription transportMode;

		#endregion

		#region IsCoload

		public ZBool IsCoload
		{
			get => isCoload;
			set
			{
				if (SetNonPersistentPropertyValue(IsColoadInfo, ref isCoload, value))
				{
					Validate(IsColoadInfo);
				}
			}
		}
		ZBool isCoload;

		public ZPropertyInfo IsColoadInfo => GetZPropertyInfo(nameof(IsCoload));

		#endregion

		#region IsGatewayCoload

		public ZBool IsGatewayCoload
		{
			get => isGatewayCoload;
			set
			{
				if (SetNonPersistentPropertyValue(IsGatewayColoadInfo, ref isGatewayCoload, value))
				{
					Validate(IsGatewayColoadInfo);
				}
			}
		}
		ZBool isGatewayCoload;

		public ZPropertyInfo IsGatewayColoadInfo => GetZPropertyInfo(nameof(IsGatewayCoload));

		#endregion

		#region IsDirect

		public ZBool IsDirect
		{
			get => isDirect;
			set
			{
				if (SetNonPersistentPropertyValue(IsDirectInfo, ref isDirect, value))
				{
					Validate(IsDirectInfo);
				}
			}
		}
		ZBool isDirect;

		public ZPropertyInfo IsDirectInfo => GetZPropertyInfo(nameof(IsDirect));

		#endregion

		#region IsHazardous

		public ZBool IsHazardous
		{
			get => isHazardous;
			set
			{
				if (SetNonPersistentPropertyValue(IsHazardousInfo, ref isHazardous, value))
				{
					Validate(IsHazardousInfo);
				}
			}
		}
		ZBool isHazardous;

		public ZPropertyInfo IsHazardousInfo => GetZPropertyInfo(nameof(IsHazardous));

		#endregion

		#region IsOutOfGauge

		public ZBool IsOutOfGauge
		{
			get => isOutOfGauge;
			set
			{
				if (SetNonPersistentPropertyValue(IsOutOfGaugeInfo, ref isOutOfGauge, value))
				{
					Validate(IsOutOfGaugeInfo);
				}
			}
		}
		ZBool isOutOfGauge;

		public ZPropertyInfo IsOutOfGaugeInfo => GetZPropertyInfo(nameof(IsOutOfGauge));

		#endregion

		#region HasOverhangDimensionContainers

		public ZBool HasOverhangDimensionContainers
		{
			get => hasOverhangDimensionContainers;
			set
			{
				if (SetNonPersistentPropertyValue(HasOverhangDimensionContainersInfo, ref hasOverhangDimensionContainers, value))
				{
					Validate(HasOverhangDimensionContainersInfo);
				}
			}
		}
		ZBool hasOverhangDimensionContainers;

		public ZPropertyInfo HasOverhangDimensionContainersInfo => GetZPropertyInfo(nameof(HasOverhangDimensionContainers));

		#endregion

		#region IncludeHBL

		public ZBool IncludeHbl
		{
			get => includeHbl;
			set
			{
				if (SetNonPersistentPropertyValue(IncludeHblInfo, ref includeHbl, value))
				{
					Validate(IncludeHblInfo);
				}
			}
		}
		ZBool includeHbl;

		public ZPropertyInfo IncludeHblInfo => GetZPropertyInfo(nameof(IncludeHbl));

		#endregion

		#region NumberOfOriginals

		public ZInt NumberOfOriginals
		{
			get => numberOfOriginals;
			set
			{
				if (SetNonPersistentPropertyValue(NumberOfOriginalsInfo, ref numberOfOriginals, value))
				{
					Validate(NumberOfOriginalsInfo);
				}
			}
		}
		ZInt numberOfOriginals;

		public ZPropertyInfo NumberOfOriginalsInfo => GetZPropertyInfo(nameof(NumberOfOriginals));

		#endregion

		#region NumberOfCopies

		public ZInt NumberOfCopies
		{
			get => numberOfCopies;
			set
			{
				if (SetNonPersistentPropertyValue(NumberOfCopiesInfo, ref numberOfCopies, value))
				{
					Validate(NumberOfCopiesInfo);
				}
			}
		}
		ZInt numberOfCopies;

		public ZPropertyInfo NumberOfCopiesInfo => GetZPropertyInfo(nameof(NumberOfCopies));

		#endregion

		#region EarliestDepartureDate

		public ZDateTime EarliestDepartureDate
		{
			get => earliestDepartureDate;
			set
			{
				if (SetNonPersistentPropertyValue(EarliestDepartureDateInfo, ref earliestDepartureDate, value))
				{
					Validate(EarliestDepartureDateInfo);
				}
			}
		}
		ZDateTime earliestDepartureDate;

		public ZPropertyInfo EarliestDepartureDateInfo => GetZPropertyInfo(nameof(EarliestDepartureDate));

		#endregion

		#region LatestDeliveryDate

		public ZDateTime LatestDeliveryDate
		{
			get => latestDeliveryDate;
			set
			{
				if (SetNonPersistentPropertyValue(LatestDeliveryDateInfo, ref latestDeliveryDate, value))
				{
					Validate(LatestDeliveryDateInfo);
				}
			}
		}
		ZDateTime latestDeliveryDate;

		public ZPropertyInfo LatestDeliveryDateInfo => GetZPropertyInfo(nameof(LatestDeliveryDate));

		#endregion

		#region DateOfIssue

		public ZDateTime DateOfIssue
		{
			get => dateOfIssue;
			set
			{
				if (SetNonPersistentPropertyValue(DateOfIssueInfo, ref dateOfIssue, value))
				{
					Validate(DateOfIssueInfo);
				}
			}
		}
		ZDateTime dateOfIssue;

		public ZPropertyInfo DateOfIssueInfo => GetZPropertyInfo(nameof(DateOfIssue));

		#endregion

		#region PortOfFirstArrivalDate

		public ZDateTime PortOfFirstArrivalDate
		{
			get => portOfFirstArrivalDate;
			set
			{
				if (SetNonPersistentPropertyValue(PortOfFirstArrivalDateInfo, ref portOfFirstArrivalDate, value))
				{
					Validate(PortOfFirstArrivalDateInfo);
				}
			}
		}
		ZDateTime portOfFirstArrivalDate;

		public ZPropertyInfo PortOfFirstArrivalDateInfo => GetZPropertyInfo(nameof(PortOfFirstArrivalDate));

		#endregion

		#region FirstForeignArrivalDate

		public ZDateTime FirstForeignArrivalDate
		{
			get => firstForeignArrivalDate;
			set
			{
				if (SetNonPersistentPropertyValue(FirstForeignArrivalDateInfo, ref firstForeignArrivalDate, value))
				{
					Validate(FirstForeignArrivalDateInfo);
				}
			}
		}
		ZDateTime firstForeignArrivalDate;

		public ZPropertyInfo FirstForeignArrivalDateInfo => GetZPropertyInfo(nameof(FirstForeignArrivalDate));

		#endregion

		#region LastForeignDepartureDate

		public ZDateTime LastForeignDepartureDate
		{
			get => lastForeignDepartureDate;
			set
			{
				if (SetNonPersistentPropertyValue(LastForeignDepartureDateInfo, ref lastForeignDepartureDate, value))
				{
					Validate(LastForeignDepartureDateInfo);
				}
			}
		}
		ZDateTime lastForeignDepartureDate;

		public ZPropertyInfo LastForeignDepartureDateInfo => GetZPropertyInfo(nameof(LastForeignDepartureDate));

		#endregion

		#region IsDoorPickup

		public ZBool IsDoorPickup
		{
			get => isDoorPickup;
			set
			{
				if (SetNonPersistentPropertyValue(IsDoorPickupInfo, ref isDoorPickup, value))
				{
					Validate(IsDoorPickupInfo);
				}
			}
		}
		ZBool isDoorPickup;

		public ZPropertyInfo IsDoorPickupInfo => GetZPropertyInfo(nameof(IsDoorPickup));

		#endregion

		#region IsDoorDelivery

		public ZBool IsDoorDelivery
		{
			get => isDoorDelivery;
			set
			{
				if (SetNonPersistentPropertyValue(IsDoorDeliveryInfo, ref isDoorDelivery, value))
				{
					Validate(IsDoorDeliveryInfo);
				}
			}
		}
		ZBool isDoorDelivery;

		public ZPropertyInfo IsDoorDeliveryInfo => GetZPropertyInfo(nameof(IsDoorDelivery));

		#endregion

		#region BookingReference

		public ZString BookingReference
		{
			get => bookingReference;
			set
			{
				if (SetNonPersistentPropertyValue(BookingReferenceInfo, ref bookingReference, value))
				{
					Validate(BookingReferenceInfo);
				}
			}
		}
		ZString bookingReference;

		public ZPropertyInfo BookingReferenceInfo => GetZPropertyInfo(nameof(BookingReference));

		#endregion

		#region CoLoadBookingReference

		public ZString CoLoadBookingReference
		{
			get => coLoadBookingReference;
			set
			{
				if (SetNonPersistentPropertyValue(CoLoadBookingReferenceInfo, ref coLoadBookingReference, value))
				{
					Validate(CoLoadBookingReferenceInfo);
				}
			}
		}
		ZString coLoadBookingReference;

		public ZPropertyInfo CoLoadBookingReferenceInfo => GetZPropertyInfo(nameof(CoLoadBookingReference));

		#endregion

		#region MasterBillNumber

		public ZString MasterBillNumber
		{
			get => masterBillNumber;
			set
			{
				if (SetNonPersistentPropertyValue(MasterBillNumberInfo, ref masterBillNumber, value))
				{
					Validate(MasterBillNumberInfo);
				}
			}
		}
		ZString masterBillNumber;

		public ZPropertyInfo MasterBillNumberInfo => GetZPropertyInfo(nameof(MasterBillNumber));

		#endregion

		#region CoLoadMasterBillNumber

		public ZString CoLoadMasterBillNumber
		{
			get => coLoadMasterBillNumber;
			set
			{
				if (SetNonPersistentPropertyValue(CoLoadMasterBillNumberInfo, ref coLoadMasterBillNumber, value))
				{
					Validate(CoLoadMasterBillNumberInfo);
				}
			}
		}
		ZString coLoadMasterBillNumber;

		public ZPropertyInfo CoLoadMasterBillNumberInfo => GetZPropertyInfo(nameof(CoLoadMasterBillNumber));

		#endregion

		#region ShipperReference

		public ZString ShipperReference
		{
			get => shipperReference;
			set
			{
				if (SetNonPersistentPropertyValue(ShipperReferenceInfo, ref shipperReference, value))
				{
					Validate(ShipperReferenceInfo);
				}
			}
		}
		ZString shipperReference;

		public ZPropertyInfo ShipperReferenceInfo => GetZPropertyInfo(nameof(ShipperReference));

		#endregion

		#region BillOfLadingNumber

		public ZString BillOfLadingNumber
		{
			get => billOfLadingNumber;
			set
			{
				if (SetNonPersistentPropertyValue(BillOfLadingNumberInfo, ref billOfLadingNumber, value))
				{
					Validate(BillOfLadingNumberInfo);
				}
			}
		}

		ZString billOfLadingNumber;

		public ZPropertyInfo BillOfLadingNumberInfo => GetZPropertyInfo(nameof(BillOfLadingNumber));

		#endregion

		#region USCanadaManifestSelfFilerID

		public ZString USCanadaManifestSelfFilerID
		{
			get => usCanadaManifestSelfFilerID;
			set
			{
				if (SetNonPersistentPropertyValue(USCanadaManifestSelfFilerIDInfo, ref usCanadaManifestSelfFilerID, value))
				{
					Validate(USCanadaManifestSelfFilerIDInfo);
				}
			}
		}

		ZString usCanadaManifestSelfFilerID;

		public ZPropertyInfo USCanadaManifestSelfFilerIDInfo => GetZPropertyInfo(nameof(USCanadaManifestSelfFilerID));

		#endregion

		#region IsUSOrUSTerritoryExport

		public ZBool IsUSOrUSTerritoryExport
		{
			get => isUSOrUSTerritoryExport;
			set
			{
				if (SetNonPersistentPropertyValue(IsUSOrUSTerritoryExportInfo, ref isUSOrUSTerritoryExport, value))
				{
					Validate(IsUSOrUSTerritoryExportInfo);
				}
			}
		}

		ZBool isUSOrUSTerritoryExport;

		public ZPropertyInfo IsUSOrUSTerritoryExportInfo => GetZPropertyInfo(nameof(IsUSOrUSTerritoryExport));

		#endregion

		#region IsUSOrCAImport

		public ZBool IsUSCanadaManifestSelfFilerIDSupported
		{
			get => isUSCanadaManifestSelfFilerIDSupported;
			set
			{
				if (SetNonPersistentPropertyValue(IsUSCanadaManifestSelfFilerIDSupportedInfo, ref isUSCanadaManifestSelfFilerIDSupported, value))
				{
					Validate(IsUSCanadaManifestSelfFilerIDSupportedInfo);
				}
			}
		}

		ZBool isUSCanadaManifestSelfFilerIDSupported;

		public ZPropertyInfo IsUSCanadaManifestSelfFilerIDSupportedInfo => GetZPropertyInfo(nameof(IsUSCanadaManifestSelfFilerIDSupported));

		#endregion

		#region IsBrazilExport

		public ZBool IsBrazilExport
		{
			get => isBrazilExport;
			set
			{
				if (SetNonPersistentPropertyValue(IsBrazilExportInfo, ref isBrazilExport, value))
				{
					Validate(IsBrazilExportInfo);
				}
			}
		}

		ZBool isBrazilExport;

		public ZPropertyInfo IsBrazilExportInfo => GetZPropertyInfo(nameof(IsBrazilExport));

		#endregion

		#region IsCanadaExport

		public ZBool IsCanadaExport
		{
			get => isCanadaExport;
			set
			{
				if (SetNonPersistentPropertyValue(IsCanadaExportInfo, ref isCanadaExport, value))
				{
					Validate(IsCanadaExportInfo);
				}
			}
		}

		ZBool isCanadaExport;

		public ZPropertyInfo IsCanadaExportInfo => GetZPropertyInfo(nameof(IsCanadaExport));

		#endregion

		#region IsToEgypt

		public ZBool IsToEgypt
		{
			get => isToEgypt;
			set
			{
				if (SetNonPersistentPropertyValue(IsToEgyptInfo, ref isToEgypt, value))
				{
					Validate(IsToEgyptInfo);
				}
			}
		}

		ZBool isToEgypt = false;

		public ZPropertyInfo IsToEgyptInfo => GetZPropertyInfo(nameof(IsToEgypt));

		#endregion

		#region IsToKenya

		public ZBool IsToKenya
		{
			get => isToKenya;
			set
			{
				if (SetNonPersistentPropertyValue(IsToKenyaInfo, ref isToKenya, value))
				{
					Validate(IsToKenyaInfo);
				}
			}
		}

		ZBool isToKenya = false;

		public ZPropertyInfo IsToKenyaInfo => GetZPropertyInfo(nameof(IsToKenya));

		#endregion

		#region IsTransitThroughBrazil

		public ZBool IsTransitThroughBrazil
		{
			get => isTransitThroughBrazil;
			set
			{
				if (SetNonPersistentPropertyValue(IsTransitThroughBrazilInfo, ref isTransitThroughBrazil, value))
				{
					Validate(IsTransitThroughBrazilInfo);
				}
			}
		}

		ZBool isTransitThroughBrazil;

		public ZPropertyInfo IsTransitThroughBrazilInfo => GetZPropertyInfo(nameof(IsTransitThroughBrazil));

		#endregion

		#region IsToSpecificAfricanCountry

		public ZBool IsToSpecificAfricanCountry
		{
			get => isToSpecificAfricanCountry;
			set
			{
				if (SetNonPersistentPropertyValue(IsToSpecificAfricanCountryInfo, ref isToSpecificAfricanCountry, value))
				{
					Validate(IsToSpecificAfricanCountryInfo);
				}
			}
		}

		ZBool isToSpecificAfricanCountry;

		public ZPropertyInfo IsToSpecificAfricanCountryInfo => GetZPropertyInfo(nameof(IsToSpecificAfricanCountry));

		#endregion

		#region BRWoodenPackageProcessType

		public CodeDescription BRWoodenPackageProcessType
		{
			get => brWoodenPackageProcessType;
			set => brWoodenPackageProcessType = SetChild(BRWoodenPackageProcessType, value);
		}
		CodeDescription brWoodenPackageProcessType;

		#endregion

		#region GoodsHandlingInstructions

		public ZString GoodsHandlingInstructions
		{
			get => goodsHandlingInstructions;
			set
			{
				if (SetNonPersistentPropertyValue(GoodsHandlingInstructionsInfo, ref goodsHandlingInstructions, value))
				{
					Validate(GoodsHandlingInstructionsInfo);
				}
			}
		}

		ZString goodsHandlingInstructions;

		public ZPropertyInfo GoodsHandlingInstructionsInfo => GetZPropertyInfo(nameof(GoodsHandlingInstructions));

		#endregion

		public CodeDescription ContainerMode
		{
			get => containerMode;
			set => containerMode = SetChild(ContainerMode, value);
		}
		CodeDescription containerMode;

		public ICodeDescription AgentType
		{
			get => agentType;
			set => agentType = SetChild(agentType, value);
		}

		ICodeDescription agentType;

		#region IsNonContainerized

		public ZBool IsNonContainerized
		{
			get => isNonContainerized;
			set
			{
				if (SetNonPersistentPropertyValue(IsNonContainerizedInfo, ref isNonContainerized, value))
				{
					Validate(IsNonContainerizedInfo);
				}
			}
		}
		ZBool isNonContainerized;

		public ZPropertyInfo IsNonContainerizedInfo => GetZPropertyInfo(nameof(IsNonContainerized));

		#endregion

		#region Est. Cargo Pickup Date Time

		public ZDateTime EstCargoPickupDateTime
		{
			get => estCargoPickupDateTime;
			set
			{
				if (SetNonPersistentPropertyValue(EstCargoPickupDateTimeInfo, ref estCargoPickupDateTime, value))
				{
					Validate(EstCargoPickupDateTimeInfo);
				}
			}
		}
		ZDateTime estCargoPickupDateTime;

		public ZPropertyInfo EstCargoPickupDateTimeInfo => GetZPropertyInfo(nameof(EstCargoPickupDateTime));

		#endregion

		#region IsRORO

		public ZBool IsRORO
		{
			get => isRORO;
			set
			{
				if (SetNonPersistentPropertyValue(IsROROInfo, ref isRORO, value))
				{
					Validate(IsROROInfo);
				}
			}
		}
		ZBool isRORO;

		public ZPropertyInfo IsROROInfo => GetZPropertyInfo(nameof(IsRORO));

		#endregion

		#region Optional Charges

		public IOptionalCharge OptionalChargeBasicFreight
		{
			get => optionalBasicFreightCharge;
			set => optionalBasicFreightCharge = SetChild(optionalBasicFreightCharge, value);
		}

		IOptionalCharge optionalBasicFreightCharge;

		public IOptionalCharge OptionalChargeDestinationHaulage
		{
			get => optionalChargeDestinationHaulage;
			set => optionalChargeDestinationHaulage = SetChild(optionalChargeDestinationHaulage, value);
		}

		IOptionalCharge optionalChargeDestinationHaulage;

		public IOptionalCharge OptionalChargeDestinationPort
		{
			get => optionalChargeDestinationPort;
			set => optionalChargeDestinationPort = SetChild(optionalChargeDestinationPort, value);
		}

		IOptionalCharge optionalChargeDestinationPort;

		public IOptionalCharge OptionalChargeOriginHaulage
		{
			get => optionalChargeOriginHaulage;
			set => optionalChargeOriginHaulage = SetChild(optionalChargeOriginHaulage, value);
		}

		IOptionalCharge optionalChargeOriginHaulage;

		public IOptionalCharge OptionalChargeOriginPort
		{
			get => optionalChargeOriginPort;
			set => optionalChargeOriginPort = SetChild(optionalChargeOriginPort, value);
		}

		IOptionalCharge optionalChargeOriginPort;

		#endregion

		#region IsFreightPrepaid

		public ZBool IsFreightPrepaid
		{
			get => isFreightPrepaid;
			set
			{
				if (SetNonPersistentPropertyValue(IsFreightPrepaidInfo, ref isFreightPrepaid, value))
				{
					isFreightCollect = false;

					IsFreightPrepaidInfo.RefreshBinding();
					IsFreightCollectInfo.RefreshBinding();
					Validate(IsFreightPrepaidInfo);
				}
			}
		}

		ZBool isFreightPrepaid;

		public ZPropertyInfo IsFreightPrepaidInfo => GetZPropertyInfo(nameof(IsFreightPrepaid));

		#endregion

		#region IsFreightCollect

		public ZBool IsFreightCollect
		{
			get => isFreightCollect;
			set
			{
				if (SetNonPersistentPropertyValue(IsFreightCollectInfo, ref isFreightCollect, value))
				{
					isFreightPrepaid = false;

					IsFreightPrepaidInfo.RefreshBinding();
					IsFreightCollectInfo.RefreshBinding();
					Validate(IsFreightCollectInfo);
				}
			}
		}

		ZBool isFreightCollect;

		public ZPropertyInfo IsFreightCollectInfo => GetZPropertyInfo(nameof(IsFreightCollect));

		#endregion

		#region IsFreightAsAgreed

		public ZBool IsFreightAsAgreed
		{
			get => isFreightAsAgreed;
			set
			{
				if (SetNonPersistentPropertyValue(IsFreightAsAgreedInfo, ref isFreightAsAgreed, value))
				{
					Validate(IsFreightAsAgreedInfo);
				}
			}
		}

		ZBool isFreightAsAgreed;

		public ZPropertyInfo IsFreightAsAgreedInfo => GetZPropertyInfo(nameof(IsFreightAsAgreed));

		#endregion

		#region IsReceivedForShipment

		public ZBool IsReceivedForShipment
		{
			get => isReceivedForShipment;
			set
			{
				if (SetNonPersistentPropertyValue(IsReceivedForShipmentInfo, ref isReceivedForShipment, value))
				{
					Validate(IsReceivedForShipmentInfo);
				}
			}
		}

		ZBool isReceivedForShipment;

		public ZPropertyInfo IsReceivedForShipmentInfo => GetZPropertyInfo(nameof(IsReceivedForShipment));

		#endregion

		#region IsLadenOnBoard

		public ZBool IsLadenOnBoard
		{
			get => isLadenOnBoard;
			set
			{
				if (SetNonPersistentPropertyValue(IsLadenOnBoardInfo, ref isLadenOnBoard, value))
				{
					Validate(IsLadenOnBoardInfo);
				}
			}
		}

		ZBool isLadenOnBoard;

		public ZPropertyInfo IsLadenOnBoardInfo => GetZPropertyInfo(nameof(IsLadenOnBoard));

		#endregion

		#region IsOnBoardRail

		public ZBool IsOnBoardRail
		{
			get => isOnBoardRail;
			set
			{
				if (SetNonPersistentPropertyValue(IsOnBoardRailInfo, ref isOnBoardRail, value))
				{
					Validate(IsOnBoardRailInfo);
				}
			}
		}

		ZBool isOnBoardRail;

		public ZPropertyInfo IsOnBoardRailInfo => GetZPropertyInfo(nameof(IsOnBoardRail));

		#endregion

		#region IsLadenOnBoardVessel

		public ZBool IsLadenOnBoardVessel
		{
			get => isLadenOnBoardVessel;
			set
			{
				if (SetNonPersistentPropertyValue(IsLadenOnBoardVesselInfo, ref isLadenOnBoardVessel, value))
				{
					Validate(IsLadenOnBoardVesselInfo);
				}
			}
		}

		ZBool isLadenOnBoardVessel;

		public ZPropertyInfo IsLadenOnBoardVesselInfo => GetZPropertyInfo(nameof(IsLadenOnBoardVessel));

		#endregion

		#region IsOnBoardVessel

		public ZBool IsOnBoardVessel
		{
			get => isOnBoardVessel;
			set
			{
				if (SetNonPersistentPropertyValue(IsOnBoardVesselInfo, ref isOnBoardVessel, value))
				{
					Validate(IsOnBoardVesselInfo);
				}
			}
		}

		ZBool isOnBoardVessel;

		public ZPropertyInfo IsOnBoardVesselInfo => GetZPropertyInfo(nameof(IsOnBoardVessel));

		#endregion

		#region IsLadenOnBoardNamedVessel

		public ZBool IsLadenOnBoardNamedVessel
		{
			get => isLadenOnBoardNamedVessel;
			set
			{
				if (SetNonPersistentPropertyValue(IsLadenOnBoardNamedVesselInfo, ref isLadenOnBoardNamedVessel, value))
				{
					Validate(IsLadenOnBoardNamedVesselInfo);
				}
			}
		}

		ZBool isLadenOnBoardNamedVessel;

		public ZPropertyInfo IsLadenOnBoardNamedVesselInfo => GetZPropertyInfo(nameof(IsLadenOnBoardNamedVessel));

		#endregion

		#region IsShipperLoadAndCount

		public ZBool IsShipperLoadAndCount
		{
			get => isShipperLoadAndCount;
			set
			{
				if (SetNonPersistentPropertyValue(IsShipperLoadAndCountInfo, ref isShipperLoadAndCount, value))
				{
					Validate(IsShipperLoadAndCountInfo);
				}
			}
		}

		ZBool isShipperLoadAndCount;

		public ZPropertyInfo IsShipperLoadAndCountInfo => GetZPropertyInfo(nameof(IsShipperLoadAndCount));

		#endregion

		#region IsShipperLoadStowageAndCount

		public ZBool IsShipperLoadStowageAndCount
		{
			get => isShipperLoadStowageAndCount;
			set
			{
				if (SetNonPersistentPropertyValue(IsShipperLoadStowageAndCountInfo, ref isShipperLoadStowageAndCount, value))
				{
					Validate(IsShipperLoadStowageAndCountInfo);
				}
			}
		}

		ZBool isShipperLoadStowageAndCount;

		public ZPropertyInfo IsShipperLoadStowageAndCountInfo => GetZPropertyInfo(nameof(IsShipperLoadStowageAndCount));

		#endregion

		#region IsNoShipperExportDeclarationRequired

		public ZBool IsNoShipperExportDeclarationRequired
		{
			get => isNoShipperExportDeclarationRequired;
			set
			{
				if (SetNonPersistentPropertyValue(IsNoShipperExportDeclarationRequiredInfo, ref isNoShipperExportDeclarationRequired, value))
				{
					Validate(IsNoShipperExportDeclarationRequiredInfo);
				}
			}
		}

		ZBool isNoShipperExportDeclarationRequired;

		public ZPropertyInfo IsNoShipperExportDeclarationRequiredInfo => GetZPropertyInfo(nameof(IsNoShipperExportDeclarationRequired));

		#endregion

		#region IsTaxIdModifiable

		public ZBool IsTaxIdModifiable
		{
			get => isTaxIdModifiable;
			set
			{
				if (SetNonPersistentPropertyValue(IsTaxIdModifiableInfo, ref isTaxIdModifiable, value))
				{
					Validate(IsTaxIdModifiableInfo);
				}
			}
		}

		ZBool isTaxIdModifiable;

		public ZPropertyInfo IsTaxIdModifiableInfo => GetZPropertyInfo(nameof(IsTaxIdModifiable));

		#endregion

		#region IssueFreightedBillOfLading

		public ZBool IssueFreightedBillOfLading
		{
			get => issueFreightedBillOfLading;
			set
			{
				if (SetNonPersistentPropertyValue(IssueFreightedBillOfLadingInfo, ref issueFreightedBillOfLading, value))
				{
					Validate(IssueFreightedBillOfLadingInfo);
				}
			}
		}

		ZBool issueFreightedBillOfLading;

		public ZPropertyInfo IssueFreightedBillOfLadingInfo => GetZPropertyInfo(nameof(IssueFreightedBillOfLading));

		#endregion

		#region OtherBillClauses

		public ZString OtherBillClauses
		{
			get => otherBillClauses;
			set
			{
				if (SetNonPersistentPropertyValue(OtherBillClausesInfo, ref otherBillClauses, value))
				{
					Validate(OtherBillClausesInfo);
				}
			}
		}

		ZString otherBillClauses;

		public ZPropertyInfo OtherBillClausesInfo => GetZPropertyInfo(nameof(OtherBillClauses));

		#endregion

		#region ForwardingInstructions

		public ZString ForwardingInstructions
		{
			get => forwardingInstructions;
			set
			{
				if (SetNonPersistentPropertyValue(ForwardingInstructionsInfo, ref forwardingInstructions, value))
				{
					Validate(ForwardingInstructionsInfo);
				}
			}
		}

		ZString forwardingInstructions;

		public ZPropertyInfo ForwardingInstructionsInfo => GetZPropertyInfo(nameof(ForwardingInstructions));

		#endregion

		#region PaymentMethod

		public CodeDescription PaymentMethod
		{
			get => paymentMethod;
			set => paymentMethod = SetChild(paymentMethod, value);
		}
		CodeDescription paymentMethod;

		#endregion

		#region SpecialInstructions

		public ZString SpecialInstructions
		{
			get => specialInstructions;
			set
			{
				if (SetNonPersistentPropertyValue(SpecialInstructionsInfo, ref specialInstructions, value))
				{
					Validate(SpecialInstructionsInfo);
				}
			}
		}

		ZString specialInstructions;

		public ZPropertyInfo SpecialInstructionsInfo => GetZPropertyInfo(nameof(SpecialInstructions));

		#endregion

		#region HasShipments

		public ZBool HasShipments
		{
			get => hasShipments;
			set
			{
				if (SetNonPersistentPropertyValue(HasShipmentsInfo, ref hasShipments, value))
				{
					Validate(HasShipmentsInfo);
				}
			}
		}
		ZBool hasShipments;

		public ZPropertyInfo HasShipmentsInfo => GetZPropertyInfo(nameof(HasShipments));

		#endregion

		#region ErrorPlaceHolder

		public ZString ErrorPlaceHolder
		{
			get => errorPlaceHolder;
			set
			{
				if (SetNonPersistentPropertyValue(ErrorPlaceHolderInfo, ref errorPlaceHolder, value))
				{
				}
			}
		}

		ZString errorPlaceHolder;

		public ZPropertyInfo ErrorPlaceHolderInfo => GetZPropertyInfo(nameof(ErrorPlaceHolder));

		#endregion

		public CodeDescription ReleaseType
		{
			get => releaseType;
			set => releaseType = SetChild(releaseType, value);
		}
		CodeDescription releaseType;

		public Money GoodsValue
		{
			get => goodsValue;
			set => goodsValue = SetChild(goodsValue, value);
		}
		Money goodsValue;

		public Transports Transports
		{
			get => transports;
			set => transports = SetChild(transports, value);
		}
		Transports transports;

		#region Ports

		public Unloco PortOfLoading
		{
			get => portOfLoading;
			set => portOfLoading = SetChild(portOfLoading, value);
		}
		Unloco portOfLoading;

		public Unloco PortOfDischarge
		{
			get => portOfDischarge;
			set => portOfDischarge = SetChild(portOfDischarge, value);
		}
		Unloco portOfDischarge;

		public Unloco Origin
		{
			get => origin;
			set => origin = SetChild(origin, value);
		}
		Unloco origin;

		public Unloco Destination
		{
			get => destination;
			set => destination = SetChild(destination, value);
		}
		Unloco destination;

		public Unloco PlaceOfReceipt
		{
			get => placeOfReceipt;
			set => placeOfReceipt = SetChild(placeOfReceipt, value);
		}
		Unloco placeOfReceipt;

		public Unloco PlaceOfDelivery
		{
			get => placeOfDelivery;
			set => placeOfDelivery = SetChild(placeOfDelivery, value);
		}
		Unloco placeOfDelivery;

		public Unloco PlaceOfIssue
		{
			get => placeOfIssue;
			set => placeOfIssue = SetChild(placeOfIssue, value);
		}
		Unloco placeOfIssue;

		public Unloco CarrierBookingOffice
		{
			get => carrierBookingOffice;
			set => carrierBookingOffice = SetChild(carrierBookingOffice, value);
		}
		Unloco carrierBookingOffice;

		public Unloco FreightPayableAt
		{
			get => freightPayableAt;
			set => freightPayableAt = SetChild(freightPayableAt, value);
		}

		Unloco freightPayableAt;

		public Unloco OperationalPort
		{
			get => operationalPort;
			set => operationalPort = SetChild(operationalPort, value);
		}

		Unloco operationalPort;

		#endregion

		#region Addresses

		public Address Shipper
		{
			get => shipper;
			set => shipper = SetChild(shipper, value);
		}
		Address shipper;

		public Address Carrier
		{
			get => carrier;
			set => carrier = SetChild(carrier, value);
		}
		Address carrier;

		public Address Creditor
		{
			get => creditor;
			set => creditor = SetChild(creditor, value);
		}
		Address creditor;

		public Address Consignee
		{
			get => consignee;
			set => consignee = SetChild(consignee, value);
		}
		Address consignee;

		public Address NotifyParty
		{
			get => notifyParty;
			set => notifyParty = SetChild(notifyParty, value);
		}
		Address notifyParty;

		public Address NotifyParty2
		{
			get => notifyParty2;
			set => notifyParty2 = SetChild(notifyParty2, value);
		}
		Address notifyParty2;

		public Address NotifyParty3
		{
			get => notifyParty3;
			set => notifyParty3 = SetChild(notifyParty3, value);
		}
		Address notifyParty3;

		public Address Forwarder
		{
			get => forwarder;
			set => forwarder = SetChild(forwarder, value);
		}
		Address forwarder;

		internal Address SendingForwarder
		{
			get => sendingForwarder;
			set => sendingForwarder = SetChild(sendingForwarder, value);
		}
		Address sendingForwarder;

		public Address Buyer
		{
			get => buyer;
			set => buyer = SetChild(buyer, value);
		}
		Address buyer;

		public Address FreightPayer
		{
			get => freightPayer;
			set => freightPayer = SetChild(freightPayer, value);
		}
		Address freightPayer;

		public Address PickupFrom
		{
			get => pickupFrom;
			set => pickupFrom = SetChild(pickupFrom, value);
		}
		Address pickupFrom;

		public Address DeliverTo
		{
			get => deliverTo;
			set => deliverTo = SetChild(deliverTo, value);
		}
		Address deliverTo;

		public Address CurrentUser
		{
			get => currentUser;
			set => currentUser = SetChild(currentUser, value);
		}
		Address currentUser;

		public Address CustomsBroker
		{
			get => customsBroker;
			set => customsBroker = SetChild(customsBroker, value);
		}
		Address customsBroker;

		public Address Recipient
		{
			get => recipient;
			set => recipient = SetChild(recipient, value);
		}
		Address recipient;

		public ZString RecipientType
		{
			get => recipientType;
			set
			{
				if (SetNonPersistentPropertyValue(RecipientTypeInfo, ref recipientType, value))
				{
					Validate(RecipientTypeInfo);
				}
			}
		}
		ZString recipientType;

		public ZPropertyInfo RecipientTypeInfo => GetZPropertyInfo(nameof(RecipientType));

		#endregion

		#region Collections

		#region ConsolDGRestrictionCollection

		public IReadOnlyCollection<DGRestriction> PreallocatedUNDGCollection
		{
			get => preallocatedUNDGCollection;
			set => preallocatedUNDGCollection = SetChild(preallocatedUNDGCollection, value);
		}
		IReadOnlyCollection<DGRestriction> preallocatedUNDGCollection;

		#endregion

		#region Containers

		public IReadOnlyCollection<Container> Containers
		{
			get => containers;
			set => containers = SetChildCollection(containers, value);
		}

		IReadOnlyCollection<Container> containers;

		public ZString UnitOfWeight
		{
			get => unitOfWeight;
			set
			{
				if (SetNonPersistentPropertyValue(UnitOfWeightInfo, ref unitOfWeight, value))
				{
					Validate(UnitOfWeightInfo);
				}
			}
		}
		ZString unitOfWeight;

		public ZPropertyInfo UnitOfWeightInfo => GetZPropertyInfo(nameof(UnitOfWeight));

		public ZString UnitOfVolume
		{
			get => unitOfVolume;
			set
			{
				if (SetNonPersistentPropertyValue(UnitOfVolumeInfo, ref unitOfVolume, value))
				{
					Validate(UnitOfVolumeInfo);
				}
			}
		}
		ZString unitOfVolume;

		public ZPropertyInfo UnitOfVolumeInfo => GetZPropertyInfo(nameof(UnitOfVolume));

		public ZString UnitOfDimensions
		{
			get => unitOfDimensions;
			set
			{
				if (SetNonPersistentPropertyValue(UnitOfDimensionsInfo, ref unitOfDimensions, value))
				{
					Validate(UnitOfDimensionsInfo);
				}
			}
		}
		ZString unitOfDimensions;

		public ZPropertyInfo UnitOfDimensionsInfo => GetZPropertyInfo(nameof(UnitOfDimensions));

		#endregion

		#region Shipments

		public IReadOnlyCollection<Shipment> Shipments
		{
			get => shipments;
			set => shipments = SetChildCollection(shipments, value);
		}

		IReadOnlyCollection<Shipment> shipments;

		#endregion

		#region Numbers

		public IReadOnlyCollection<IReferenceNumber> Numbers
		{
			get => numbers;
			set => numbers = SetChildCollection(numbers, value);
		}

		IReadOnlyCollection<IReferenceNumber> numbers;

		#endregion

		#region CarrierContractNumber

		public ZString CarrierContractNumbersFormatted
		{
			get => carrierContractNumbersFormatted;
			set
			{
				if (SetNonPersistentPropertyValue(CarrierContractNumbersFormattedInfo, ref carrierContractNumbersFormatted, value))
				{
					Validate(CarrierContractNumbersFormattedInfo);
				}
			}
		}

		ZString carrierContractNumbersFormatted;

		public ZPropertyInfo CarrierContractNumbersFormattedInfo => GetZPropertyInfo(nameof(CarrierContractNumbersFormatted));

		#endregion

		#region ContractNamedAccount

		public ZString ContractNamedAccount
		{
			get => contractNamedAccount;
			set
			{
				if (SetNonPersistentPropertyValue(ContractNamedAccountInfo, ref contractNamedAccount, value))
				{
					Validate(ContractNamedAccountInfo);
				}
			}
		}

		ZString contractNamedAccount;

		public ZPropertyInfo ContractNamedAccountInfo => GetZPropertyInfo(nameof(ContractNamedAccount));

		#endregion

		#endregion

		#region ShipperTaxInfo

		public IReadOnlyCollection<TaxInfo> ShipperTaxInfo
		{
			get => shipperTaxInfo;
			set => shipperTaxInfo = SetChildCollection(shipperTaxInfo, value);
		}

		IReadOnlyCollection<TaxInfo> shipperTaxInfo;

		public TaxInfo ShipperTaxInfo1 => ShipperTaxInfo?.Count > 0
			? ShipperTaxInfo.ElementAt(0)
			: null;

		public TaxInfo ShipperTaxInfo2 => ShipperTaxInfo?.Count > 1
			? ShipperTaxInfo.ElementAt(1)
			: null;

		public TaxInfo ShipperTaxInfo3 => ShipperTaxInfo?.Count > 2
			? ShipperTaxInfo.ElementAt(2)
			: null;

		#endregion

		#region ConsigneeTaxInfo

		public IReadOnlyCollection<TaxInfo> ConsigneeTaxInfo
		{
			get => consigneeTaxInfo;
			set => consigneeTaxInfo = SetChildCollection(consigneeTaxInfo, value);
		}

		IReadOnlyCollection<TaxInfo> consigneeTaxInfo;

		public IReadOnlyCollection<TaxInfo> ConsigneeTaxInfoOriginal
		{
			get => consigneeTaxInfoOriginal;
			set => consigneeTaxInfoOriginal = SetChildCollection(consigneeTaxInfoOriginal, value);
		}

		IReadOnlyCollection<TaxInfo> consigneeTaxInfoOriginal;

		public TaxInfo ConsigneeTaxInfo1 => ConsigneeTaxInfo?.Count > 0
			? ConsigneeTaxInfo.ElementAt(0)
			: null;

		public TaxInfo ConsigneeTaxInfo2 => ConsigneeTaxInfo?.Count > 1
			? ConsigneeTaxInfo.ElementAt(1)
			: null;

		public TaxInfo ConsigneeTaxInfo3 => ConsigneeTaxInfo?.Count > 2
			? ConsigneeTaxInfo.ElementAt(2)
			: null;

		#endregion

		#region NotifyPartyTaxInfo

		public IReadOnlyCollection<TaxInfo> NotifyPartyTaxInfo
		{
			get => notifyPartyTaxInfo;
			set => notifyPartyTaxInfo = SetChildCollection(notifyPartyTaxInfo, value);
		}

		IReadOnlyCollection<TaxInfo> notifyPartyTaxInfo;

		public IReadOnlyCollection<TaxInfo> NotifyPartyTaxInfoOriginal
		{
			get => notifyPartyTaxInfoOriginal;
			set => notifyPartyTaxInfoOriginal = SetChildCollection(notifyPartyTaxInfoOriginal, value);
		}

		IReadOnlyCollection<TaxInfo> notifyPartyTaxInfoOriginal;

		public TaxInfo NotifyPartyTaxInfo1 => NotifyPartyTaxInfo?.Count > 0
			? NotifyPartyTaxInfo.ElementAt(0)
			: null;

		public TaxInfo NotifyPartyTaxInfo2 => NotifyPartyTaxInfo?.Count > 1
			? NotifyPartyTaxInfo.ElementAt(1)
			: null;

		public TaxInfo NotifyPartyTaxInfo3 => NotifyPartyTaxInfo?.Count > 2
			? NotifyPartyTaxInfo.ElementAt(2)
			: null;

		#endregion

		#region Appearance Properties

		public bool IsNVO { get; set; }

		#endregion

		#region SuppressGoodsValue

		public ZBool SuppressGoodsValue
		{
			get => suppressGoodsValue;
			set
			{
				if (SetNonPersistentPropertyValue(SuppressGoodsValueInfo, ref suppressGoodsValue, value))
				{
					Validate(SuppressGoodsValueInfo);
				}
			}
		}

		ZBool suppressGoodsValue;

		public ZPropertyInfo SuppressGoodsValueInfo => GetZPropertyInfo(nameof(SuppressGoodsValue));

		#endregion

		#region AcidNumber
		public ZString AcidNumber
		{
			get => acidNumber;
			set
			{
				if (SetNonPersistentPropertyValue(AcidNumberInfo, ref acidNumber, value))
				{
					Validate(AcidNumberInfo);
				}
			}
		}
		ZString acidNumber;

		public ZPropertyInfo AcidNumberInfo => GetZPropertyInfo(nameof(AcidNumber));

		#endregion

		#region IsColoadLCL 

		public ZBool IsColoadLCL
		{
			get => isColoadLCL;
			set
			{
				if (SetNonPersistentPropertyValue(IsColoadLCLInfo, ref isColoadLCL, value))
				{
					Validate(IsColoadLCLInfo);
				}
			}
		}

		ZBool isColoadLCL;

		public ZPropertyInfo IsColoadLCLInfo => GetZPropertyInfo(nameof(IsColoadLCL));

		#endregion

		#region HIR Reference

		public RegistrationNumber HIRReference
		{
			get => hirReference;
			set => hirReference = SetChild(hirReference, value);
		}

		RegistrationNumber hirReference;

		#endregion

		#region EBL Provider

		public ICodeDescription EBLProvider
		{
			get => eblProvider;
			set => eblProvider = SetChild(eblProvider, value);
		}
		ICodeDescription eblProvider;

		#endregion

		#region ElectronicBillOfLadingProviderMandatory

		public ZBool ElectronicBillOfLadingProviderMandatory
		{
			get => electronicBillOfLadingProviderMandatory;
			set
			{
				if (SetNonPersistentPropertyValue(ElectronicBillOfLadingProviderMandatoryInfo, ref electronicBillOfLadingProviderMandatory, value))
				{
					Validate(ElectronicBillOfLadingProviderMandatoryInfo);
				}
			}
		}
		ZBool electronicBillOfLadingProviderMandatory;

		public ZPropertyInfo ElectronicBillOfLadingProviderMandatoryInfo => GetZPropertyInfo(nameof(ElectronicBillOfLadingProviderMandatory));

		#endregion

		#region PackageGrouping

		public CodeDescription PackageGrouping
		{
			get => packageGrouping;
			set => packageGrouping = SetChild(packageGrouping, value);
		}
		CodeDescription packageGrouping;

		#endregion

		#region RUCNumber

		public ZString RUCNumber
		{
			get => rucNumber;
			set
			{
				if (SetNonPersistentPropertyValue(RUCNumberInfo, ref rucNumber, value))
				{
					Validate(RUCNumberInfo);
				}
			}
		}
		ZString rucNumber;

		public ZPropertyInfo RUCNumberInfo => GetZPropertyInfo(nameof(RUCNumber));

		#endregion

		#region IsRequiredSendAttachment

		public ZBool IsRequiredSendAttachment
		{
			get => isRequiredSendAttachment;
			set
			{
				if (SetNonPersistentPropertyValue(IsRequiredSendAttachmentInfo, ref isRequiredSendAttachment, value))
				{
					Validate(IsRequiredSendAttachmentInfo);
				}
			}
		}
		ZBool isRequiredSendAttachment;

		public ZPropertyInfo IsRequiredSendAttachmentInfo => GetZPropertyInfo(nameof(IsRequiredSendAttachment));

		#endregion

		#region IsShowICS2

		public ZBool IsShowICS2
		{
			get => isShowICS2;
			set
			{
				if (SetNonPersistentPropertyValue(IsShowICS2Info, ref isShowICS2, value))
				{
					Validate(IsShowICS2Info);
				}
			}
		}

		ZBool isShowICS2;

		public ZPropertyInfo IsShowICS2Info => GetZPropertyInfo(nameof(IsShowICS2));

		#endregion

		#region ICS2DeclarantEORINumber

		public ZString ICS2DeclarantEORINumber
		{
			get => ics2DeclarantEORINumber;
			set
			{
				if (SetNonPersistentPropertyValue(ICS2DeclarantEORINumberInfo, ref ics2DeclarantEORINumber, value))
				{
					Validate(ICS2DeclarantEORINumberInfo);
				}
			}
		}
		ZString ics2DeclarantEORINumber;

		public ZPropertyInfo ICS2DeclarantEORINumberInfo => GetZPropertyInfo(nameof(ICS2DeclarantEORINumber));

		#endregion

		#region MustHaveContainerPackline

		public ZBool MustHaveContainerPackline
		{
			get => mustHaveContainerPackline;
			set
			{
				if (SetNonPersistentPropertyValue(MustHaveContainerPacklineInfo, ref mustHaveContainerPackline, value))
				{
					Validate(MustHaveContainerPacklineInfo);
				}
			}
		}
		ZBool mustHaveContainerPackline;

		public ZPropertyInfo MustHaveContainerPacklineInfo => GetZPropertyInfo(nameof(MustHaveContainerPackline));

		#endregion
	}
}
