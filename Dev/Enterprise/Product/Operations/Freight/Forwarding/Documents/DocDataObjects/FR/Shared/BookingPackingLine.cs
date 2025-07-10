using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.FR
{
	sealed class BookingPackingLine : DocDataObject, IPackingLine
	{
		public BookingPackingLine(object identifier)
			: base(identifier)
		{
		}

		#region PackingLineID

		public ZString PackingLineID
		{
			get => packingLineID;
			set
			{
				if (SetNonPersistentPropertyValue(PackingLineIDInfo, ref packingLineID, value))
				{
					Validate(PackingLineIDInfo);
				}
			}
		}

		ZString packingLineID;

		public ZPropertyInfo PackingLineIDInfo => GetZPropertyInfo(nameof(PackingLineID));

		#endregion

		#region EntryType

		public ICodeDescription EntryType
		{
			get => entryType;
			set => entryType = SetChild(entryType, value);
		}

		ICodeDescription entryType;

		#endregion

		#region Complete

		public ZBool Complete
		{
			get => complete;
			set
			{
				if (SetNonPersistentPropertyValue(CompleteInfo, ref complete, value))
				{
					Validate(CompleteInfo);
				}
			}
		}

		ZBool complete;

		public ZPropertyInfo CompleteInfo => GetZPropertyInfo(nameof(Complete));

		#endregion

		#region Shortage

		public ZBool Shortage
		{
			get => shortage;
			set
			{
				if (SetNonPersistentPropertyValue(ShortageInfo, ref shortage, value))
				{
					Validate(ShortageInfo);
				}
			}
		}

		ZBool shortage;

		public ZPropertyInfo ShortageInfo => GetZPropertyInfo(nameof(Shortage));

		#endregion

		#region Commodity

		public ICodeDescription Commodity
		{
			get => commodity;
			set => commodity = SetChild(commodity, value);
		}

		ICodeDescription commodity;

		#endregion

		#region PackingOrder

		public ZInt PackingOrder
		{
			get => packingOrder;
			set
			{
				if (SetNonPersistentPropertyValue(PackingOrderInfo, ref packingOrder, value))
				{
					Validate(PackingOrderInfo);
				}
			}
		}

		ZInt packingOrder;

		public ZPropertyInfo PackingOrderInfo => GetZPropertyInfo(nameof(PackingOrder));

		#endregion

		#region ContainerNumber

		public ZString ContainerNumber
		{
			get => containerNumber;
			set
			{
				if (SetNonPersistentPropertyValue(ContainerNumberInfo, ref containerNumber, value))
				{
					Validate(ContainerNumberInfo);
				}
			}
		}

		ZString containerNumber;

		public ZPropertyInfo ContainerNumberInfo => GetZPropertyInfo(nameof(ContainerNumber));

		#endregion

		#region ItemNumber

		public ZShort ItemNumber
		{
			get => itemNumber;
			set
			{
				if (SetNonPersistentPropertyValue(ItemNumberInfo, ref itemNumber, value))
				{
					Validate(ItemNumberInfo);
				}
			}
		}

		ZShort itemNumber;

		ZPropertyInfo ItemNumberInfo => GetZPropertyInfo(nameof(ItemNumber));

		#endregion

		#region Height

		public IMeasurement Height
		{
			get => height;
			set => height = SetChild(height, value);
		}

		IMeasurement height;

		#endregion

		#region Length

		public IMeasurement Length
		{
			get => length;
			set => length = SetChild(length, value);
		}

		IMeasurement length;

		#endregion

		#region Width

		public IMeasurement Width
		{
			get => width;
			set => width = SetChild(width, value);
		}

		IMeasurement width;

		#endregion

		#region Origin

		public ICountry Origin
		{
			get => origin;
			set => origin = SetChild(origin, value);
		}

		ICountry origin;

		#endregion

		#region Outturn

		public ZInt Outturn
		{
			get => outturn;
			set
			{
				if (SetNonPersistentPropertyValue(OutturnInfo, ref outturn, value))
				{
					Validate(OutturnInfo);
				}
			}
		}

		ZInt outturn;

		public ZPropertyInfo OutturnInfo => GetZPropertyInfo(nameof(Outturn));

		#endregion

		#region Damaged

		public ZInt Damaged
		{
			get => damaged;
			set
			{
				if (SetNonPersistentPropertyValue(DamagedInfo, ref damaged, value))
				{
					Validate(DamagedInfo);
				}
			}
		}

		ZInt damaged;

		public ZPropertyInfo DamagedInfo => GetZPropertyInfo(nameof(Damaged));

		#endregion

		#region Pillaged

		public ZInt Pillaged
		{
			get => pillaged;
			set
			{
				if (SetNonPersistentPropertyValue(PillagedInfo, ref pillaged, value))
				{
					Validate(PillagedInfo);
				}
			}
		}

		ZInt pillaged;

		public ZPropertyInfo PillagedInfo => GetZPropertyInfo(nameof(Pillaged));

		#endregion

		#region OutturnComment

		public ZString OutturnComment
		{
			get => outturnComment;
			set
			{
				if (SetNonPersistentPropertyValue(OutturnCommentInfo, ref outturnComment, value))
				{
					Validate(OutturnCommentInfo);
				}
			}
		}

		ZString outturnComment;

		public ZPropertyInfo OutturnCommentInfo => GetZPropertyInfo(nameof(OutturnComment));

		#endregion

		#region OutturnHeight

		public IMeasurement OutturnHeight
		{
			get => outturnHeight;
			set => outturnHeight = SetChild(outturnHeight, value);
		}

		IMeasurement outturnHeight;

		#endregion

		#region OutturnLength

		public IMeasurement OutturnLength
		{
			get => outturnLength;
			set => outturnLength = SetChild(outturnLength, value);
		}

		IMeasurement outturnLength;

		#endregion

		#region OutturnVolume

		public IMeasurement OutturnVolume
		{
			get => outturnVolume;
			set => outturnVolume = SetChild(outturnVolume, value);
		}

		IMeasurement outturnVolume;

		#endregion

		#region OutturnWeight

		public IMeasurement OutturnWeight
		{
			get => outturnWeight;
			set => outturnWeight = SetChild(outturnWeight, value);
		}

		IMeasurement outturnWeight;

		#endregion

		#region OutturnWidth

		public IMeasurement OutturnWidth
		{
			get => outturnWidth;
			set => outturnWidth = SetChild(outturnWidth, value);
		}

		IMeasurement outturnWidth;

		#endregion

		#region Quantity

		public ZInt Quantity
		{
			get => quantity;
			set
			{
				if (SetNonPersistentPropertyValue(QuantityInfo, ref quantity, value))
				{
					Validate(QuantityInfo);
				}
			}
		}

		ZInt quantity;

		public ZPropertyInfo QuantityInfo => GetZPropertyInfo(nameof(Quantity));

		#endregion

		#region GoodsDescription

		[MaxLength(NotificationTypes.MessageError, 31981)]
		public ZString GoodsDescription
		{
			get => goodsDescription;
			set
			{
				if (SetNonPersistentPropertyValue(GoodsDescriptionInfo, ref goodsDescription, value))
				{
					Validate(GoodsDescriptionInfo);
				}
			}
		}

		ZString goodsDescription;

		public ZPropertyInfo GoodsDescriptionInfo => GetZPropertyInfo(nameof(GoodsDescription));

		#endregion

		#region ShortGoodsDescription

		public ZString ShortGoodsDescription
		{
			get => shortGoodsDescription;
			set
			{
				if (SetNonPersistentPropertyValue(ShortGoodsDescriptionInfo, ref shortGoodsDescription, value))
				{
					Validate(ShortGoodsDescriptionInfo);
				}
			}
		}

		ZString shortGoodsDescription;

		public ZPropertyInfo ShortGoodsDescriptionInfo => GetZPropertyInfo(nameof(ShortGoodsDescription));

		#endregion

		#region DetailedGoodsDescription

		public ZString DetailedGoodsDescription
		{
			get => detailedGoodsDescription;
			set
			{
				if (SetNonPersistentPropertyValue(DetailedGoodsDescriptionInfo, ref detailedGoodsDescription, value))
				{
					Validate(DetailedGoodsDescriptionInfo);
				}
			}
		}

		ZString detailedGoodsDescription;

		public ZPropertyInfo DetailedGoodsDescriptionInfo => GetZPropertyInfo(nameof(DetailedGoodsDescription));

		#endregion

		#region MarksAndNumbers

		[MaxLength(NotificationTypes.MessageError, 31981)]
		public ZString MarksAndNumbers
		{
			get => marksAndNumbers;
			set
			{
				if (SetNonPersistentPropertyValue(MarksAndNumbersInfo, ref marksAndNumbers, value))
				{
					Validate(MarksAndNumbersInfo);
				}
			}
		}

		ZString marksAndNumbers;

		public ZPropertyInfo MarksAndNumbersInfo => GetZPropertyInfo(nameof(MarksAndNumbers));

		#endregion

		#region HarmonizedCode

		public IHarmonizedCode HarmonizedCode
		{
			get => harmonizedCode;
			set => harmonizedCode = SetChild(harmonizedCode, value);
		}

		IHarmonizedCode harmonizedCode;

		#region ExportHarmonizedCodes

		public IHarmonizedCode ExportHarmonizedCode
		{
			get => exportHarmonizedCode;
			set => exportHarmonizedCode = SetChild(exportHarmonizedCode, value);
		}

		IHarmonizedCode exportHarmonizedCode;

		#endregion

		#region ImportHarmonizedCode

		public IHarmonizedCode ImportHarmonizedCode
		{
			get => importHarmonizedCode;
			set => importHarmonizedCode = SetChild(importHarmonizedCode, value);
		}

		IHarmonizedCode importHarmonizedCode;

		#endregion

		public IReadOnlyCollection<HarmonizedCode> HarmonizedCodes
		{
			get => harmonizedCodes;
			set => harmonizedCodes = SetChildCollection(harmonizedCodes, value);
		}

		IReadOnlyCollection<HarmonizedCode> harmonizedCodes;

		IReadOnlyCollection<IHarmonizedCode> IPackingLine.HarmonizedCodes => HarmonizedCodes;

		#endregion

		#region ReferenceNumber

		public ZString ReferenceNumber
		{
			get => referenceNumber;
			set
			{
				if (SetNonPersistentPropertyValue(ReferenceNumberInfo, ref referenceNumber, value))
				{
					Validate(ReferenceNumberInfo);
				}
			}
		}

		ZString referenceNumber;

		public ZPropertyInfo ReferenceNumberInfo => GetZPropertyInfo(nameof(ReferenceNumber));

		#endregion

		#region ShipmentEntryNumbers

		public ZString ShipmentEntryNumbers
		{
			get => shipmentEntryNumbers;
			set
			{
				if (SetNonPersistentPropertyValue(ShipmentEntryNumbersInfo, ref shipmentEntryNumbers, value))
				{
					Validate(ShipmentEntryNumbersInfo);
				}
			}
		}

		ZString shipmentEntryNumbers;

		public ZPropertyInfo ShipmentEntryNumbersInfo => GetZPropertyInfo(nameof(ShipmentEntryNumbers));

		#endregion

		#region ImportReferenceNumber

		public ZString ImportReferenceNumber
		{
			get => importReferenceNumber;
			set
			{
				if (SetNonPersistentPropertyValue(ImportReferenceNumberInfo, ref importReferenceNumber, value))
				{
					Validate(ImportReferenceNumberInfo);
				}
			}
		}

		ZString importReferenceNumber;

		public ZPropertyInfo ImportReferenceNumberInfo => GetZPropertyInfo(nameof(ImportReferenceNumber));

		#endregion

		#region ExportReferenceNumber

		public ZString ExportReferenceNumber
		{
			get => exportReferenceNumber;
			set
			{
				if (SetNonPersistentPropertyValue(ExportReferenceNumberInfo, ref exportReferenceNumber, value))
				{
					Validate(ExportReferenceNumberInfo);
				}
			}
		}

		ZString exportReferenceNumber;

		public ZPropertyInfo ExportReferenceNumberInfo => GetZPropertyInfo(nameof(ExportReferenceNumber));

		#endregion

		#region LoadingMeters

		public ZDecimal LoadingMeters
		{
			get => loadingMeters;
			set
			{
				if (SetNonPersistentPropertyValue(LoadingMetersInfo, ref loadingMeters, value))
				{
					Validate(LoadingMetersInfo);
				}
			}
		}

		ZDecimal loadingMeters;

		public ZPropertyInfo LoadingMetersInfo => GetZPropertyInfo(nameof(LoadingMeters));

		#endregion

		#region EndItemNumber

		public ZShort EndItemNumber
		{
			get => endItemNumber;
			set
			{
				if (SetNonPersistentPropertyValue(EndItemNumberInfo, ref endItemNumber, value))
				{
					Validate(EndItemNumberInfo);
				}
			}
		}

		ZShort endItemNumber;

		public ZPropertyInfo EndItemNumberInfo => GetZPropertyInfo(nameof(EndItemNumber));

		#endregion

		#region PackageType

		public ICodeDescription PackageType
		{
			get => packageType;
			set => packageType = SetChild(packageType, value);
		}

		ICodeDescription packageType;

		#endregion

		#region Weight

		public Measurement Weight
		{
			get => weight;
			set => weight = SetChild(weight, value);
		}

		Measurement weight;

		IMeasurement IPackingLine.Weight => Weight;

		#endregion

		#region Volume

		public Measurement Volume
		{
			get => volume;
			set => volume = SetChild(volume, value);
		}

		Measurement volume;

		IMeasurement IPackingLine.Volume => Volume;

		#endregion

		#region VehicleColor

		public ZString VehicleColor
		{
			get => vehicleColor;
			set
			{
				if (SetNonPersistentPropertyValue(VehicleColorInfo, ref vehicleColor, value))
				{
					Validate(VehicleColorInfo);
				}
			}
		}

		ZString vehicleColor;

		public ZPropertyInfo VehicleColorInfo => GetZPropertyInfo(nameof(VehicleColor));

		#endregion

		#region VehicleMake

		public ZString VehicleMake
		{
			get => vehicleMake;
			set
			{
				if (SetNonPersistentPropertyValue(VehicleMakeInfo, ref vehicleMake, value))
				{
					Validate(VehicleMakeInfo);
				}
			}
		}

		ZString vehicleMake;

		public ZPropertyInfo VehicleMakeInfo => GetZPropertyInfo(nameof(VehicleMake));

		#endregion

		#region VehicleModel

		public ZString VehicleModel
		{
			get => vehicleModel;
			set
			{
				if (SetNonPersistentPropertyValue(VehicleModelInfo, ref vehicleModel, value))
				{
					Validate(VehicleModelInfo);
				}
			}
		}

		ZString vehicleModel;

		public ZPropertyInfo VehicleModelInfo => GetZPropertyInfo(nameof(VehicleModel));

		#endregion

		#region VehicleNumberOfDoors

		public ZInt VehicleNumberOfDoors
		{
			get => vehicleNumberOfDoors;
			set
			{
				if (SetNonPersistentPropertyValue(VehicleNumberOfDoorsInfo, ref vehicleNumberOfDoors, value))
				{
					Validate(VehicleNumberOfDoorsInfo);
				}
			}
		}

		ZInt vehicleNumberOfDoors;

		public ZPropertyInfo VehicleNumberOfDoorsInfo => GetZPropertyInfo(nameof(VehicleNumberOfDoors));

		#endregion

		#region VehicleTransmission

		public ICodeDescription VehicleTransmission
		{
			get => vehicleTransmission;
			set => vehicleTransmission = SetChild(vehicleTransmission, value);
		}

		ICodeDescription vehicleTransmission;

		#endregion

		#region VehicleYear

		public ZInt VehicleYear
		{
			get => vehicleYear;
			set
			{
				if (SetNonPersistentPropertyValue(VehicleYearInfo, ref vehicleYear, value))
				{
					Validate(VehicleYearInfo);
				}
			}
		}

		ZInt vehicleYear;

		public ZPropertyInfo VehicleYearInfo => GetZPropertyInfo(nameof(VehicleYear));

		#endregion

		#region VIN

		public ZString VIN
		{
			get => vin;
			set
			{
				if (SetNonPersistentPropertyValue(VINInfo, ref vin, value))
				{
					Validate(VINInfo);
				}
			}
		}

		ZString vin;

		public ZPropertyInfo VINInfo => GetZPropertyInfo(nameof(VIN));

		#endregion

		#region ShipmentID

		public ZString ShipmentID
		{
			get => shipmentID;
			set
			{
				if (SetNonPersistentPropertyValue(ShipmentIDInfo, ref shipmentID, value))
				{
					Validate(ShipmentIDInfo);
				}
			}
		}

		ZString shipmentID;

		public ZPropertyInfo ShipmentIDInfo => GetZPropertyInfo(nameof(ShipmentID));

		#endregion

		#region ShippersRef

		public ZString ShippersRef
		{
			get => shippersRef;
			set
			{
				if (SetNonPersistentPropertyValue(ShippersRefInfo, ref shippersRef, value))
				{
					Validate(ShippersRefInfo);
				}
			}
		}

		ZString shippersRef;

		public ZPropertyInfo ShippersRefInfo => GetZPropertyInfo(nameof(ShippersRef));

		#endregion

		#region GroupITNNumber

		public ZString GroupITNNumber
		{
			get => groupITNNumber;
			set
			{
				var formattedValue = string.Join(", ", value.ToUpperInvariant().ToString().Split(new string[] { ",", " ", ";", "/", System.Environment.NewLine, "\t" }, StringSplitOptions.RemoveEmptyEntries).Distinct());

				if (SetNonPersistentPropertyValue(GroupITNNumberInfo, ref groupITNNumber, formattedValue))
				{
					Validate(GroupITNNumberInfo);
				}
			}
		}

		ZString groupITNNumber;

		public ZPropertyInfo GroupITNNumberInfo => GetZPropertyInfo(nameof(GroupITNNumber));

		#endregion

		#region GroupPOFNumber

		public ZString GroupPOFNumber
		{
			get => groupPOFNumber;
			set
			{
				if (SetNonPersistentPropertyValue(GroupPOFNumberInfo, ref groupPOFNumber, value))
				{
					Validate(GroupPOFNumberInfo);
				}
			}
		}

		ZString groupPOFNumber;

		public ZPropertyInfo GroupPOFNumberInfo => GetZPropertyInfo(nameof(GroupPOFNumber));

		#endregion

		#region GroupDUENumber

		public ZString GroupDUENumber
		{
			get => groupDUENumber;
			set
			{
				var formattedValue = string.Join(", ", value.ToString().Split(new string[] { ",", " " }, StringSplitOptions.RemoveEmptyEntries).Distinct(StringComparer.OrdinalIgnoreCase));

				if (SetNonPersistentPropertyValue(GroupDUENumberInfo, ref groupDUENumber, formattedValue))
				{
					Validate(GroupDUENumberInfo);
				}
			}
		}

		ZString groupDUENumber;

		public ZPropertyInfo GroupDUENumberInfo => GetZPropertyInfo(nameof(GroupDUENumber));

		#endregion

		#region GroupUCRNumber

		public ZString GroupUCRNumber
		{
			get => groupUCRNumber;
			set
			{
				var formattedValue = string.Join(", ", value.ToUpperInvariant().ToString().Split(new string[] { ",", " ", ";", "/", System.Environment.NewLine, "\t" }, StringSplitOptions.RemoveEmptyEntries).Distinct());

				if (SetNonPersistentPropertyValue(GroupUCRNumberInfo, ref groupUCRNumber, formattedValue))
				{
					Validate(GroupUCRNumberInfo);
				}
			}
		}

		ZString groupUCRNumber;

		public ZPropertyInfo GroupUCRNumberInfo => GetZPropertyInfo(nameof(GroupUCRNumber));

		#endregion

		#region GroupCTKNumber

		public ZString GroupCTKNumber
		{
			get => groupCTKNumber;
			set
			{
				var formattedValue = string.Join(", ", value.ToString().Split(new string[] { ",", " " }, StringSplitOptions.RemoveEmptyEntries).Distinct(StringComparer.OrdinalIgnoreCase));

				if (SetNonPersistentPropertyValue(GroupCTKNumberInfo, ref groupCTKNumber, formattedValue))
				{
					Validate(GroupCTKNumberInfo);
				}
			}
		}

		ZString groupCTKNumber;

		public ZPropertyInfo GroupCTKNumberInfo => GetZPropertyInfo(nameof(GroupCTKNumber));

		#endregion

		#region GroupCTNNumber

		public ZString GroupCTNNumber
		{
			get => groupCTNNumber;
			set
			{
				var formattedValue = string.Join(", ", value.ToString().Split(new string[] { ",", " " }, StringSplitOptions.RemoveEmptyEntries).Distinct(StringComparer.OrdinalIgnoreCase));

				if (SetNonPersistentPropertyValue(GroupCTNNumberInfo, ref groupCTNNumber, formattedValue))
				{
					Validate(GroupCTNNumberInfo);
				}
			}
		}

		ZString groupCTNNumber;

		public ZPropertyInfo GroupCTNNumberInfo => GetZPropertyInfo(nameof(GroupCTNNumber));

		#endregion

		#region GroupKeyByConsignment

		public ZString GroupKeyByConsignment
		{
			get => groupKeyByConsignment;
			set
			{
				if (SetNonPersistentPropertyValue(GroupKeyByConsignmentInfo, ref groupKeyByConsignment, value))
				{
				}
			}
		}

		ZString groupKeyByConsignment;

		public ZPropertyInfo GroupKeyByConsignmentInfo => GetZPropertyInfo(nameof(GroupKeyByConsignment));

		#endregion

		#region DangerousGoods

		public IReadOnlyCollection<DangerousGood> DangerousGoods
		{
			get => dangerousGoods;
			set => dangerousGoods = SetChildCollection(dangerousGoods, value);
		}

		IReadOnlyCollection<DangerousGood> dangerousGoods;

		IReadOnlyCollection<IDangerousGood> IPackingLine.DangerousGoods => DangerousGoods;

		#endregion

		#region PackingLines

		public IReadOnlyCollection<PackingLine> PackingLines
		{
			get => packingLines;
			set => packingLines = SetChildCollection(packingLines, value);
		}

		IReadOnlyCollection<PackingLine> packingLines;

		IReadOnlyCollection<IPackingLine> IPackingLine.PackingLines => PackingLines;

		#endregion

		#region TemperatureMinimum

		public IMeasurement TemperatureMinimum
		{
			get => temperatureMinimum;
			set => temperatureMinimum = SetChild(temperatureMinimum, value);
		}

		IMeasurement temperatureMinimum;

		#endregion

		#region TemperatureMaximum

		public IMeasurement TemperatureMaximum
		{
			get => temperatureMaximum;
			set => temperatureMaximum = SetChild(temperatureMaximum, value);
		}

		IMeasurement temperatureMaximum;

		#endregion

		#region RequiresTemperatureControl

		public ZBool RequiresTemperatureControl
		{
			get => requiresTemperatureControl;
			set
			{
				if (SetNonPersistentPropertyValue(RequiresTemperatureControlInfo, ref requiresTemperatureControl, value))
				{
					Validate(RequiresTemperatureControlInfo);
				}
			}
		}

		ZBool requiresTemperatureControl;

		public ZPropertyInfo RequiresTemperatureControlInfo => GetZPropertyInfo(nameof(RequiresTemperatureControl));

		#endregion

		#region LastKnownTransitWarehouseStatus

		public ZString LastKnownTransitWarehouseStatus
		{
			get => lastKnownTransitWarehouseStatus;
			set
			{
				if (SetNonPersistentPropertyValue(LastKnownTransitWarehouseStatusInfo, ref lastKnownTransitWarehouseStatus, value))
				{
					Validate(LastKnownTransitWarehouseStatusInfo);
				}
			}
		}

		ZString lastKnownTransitWarehouseStatus;

		public ZPropertyInfo LastKnownTransitWarehouseStatusInfo => GetZPropertyInfo(nameof(LastKnownTransitWarehouseStatus));

		#endregion

		#region CargoItem

		public ZString CargoItem
		{
			get => cargoItem;
			set
			{
				if (SetNonPersistentPropertyValue(CargoItemInfo, ref cargoItem, value))
				{
					Validate(CargoItemInfo);
				}
			}
		}

		ZString cargoItem;

		public ZPropertyInfo CargoItemInfo => GetZPropertyInfo(nameof(CargoItem));

		#endregion

		#region PackageNumber

		public ZString PackageNumber
		{
			get => packageNumber;
			set
			{
				if (SetNonPersistentPropertyValue(PackageNumberInfo, ref packageNumber, value))
				{
					Validate(PackageNumberInfo);
				}
			}
		}

		ZString packageNumber;

		public ZPropertyInfo PackageNumberInfo => GetZPropertyInfo(nameof(PackageNumber));

		#endregion
	}
}
