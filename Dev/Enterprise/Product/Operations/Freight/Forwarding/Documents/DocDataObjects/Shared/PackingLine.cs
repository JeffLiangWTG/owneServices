using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.Core;
using MaxLengthAttribute = Enterprise.DocumentVisualizer.DocDataObjects.MaxLengthAttribute;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	public sealed class PackingLine : DocDataObject, IPackingLine
	{
		public PackingLine(object identifier, BusinessObjectFactory factory)
			: base(identifier)
		{
			this.factory = factory;
		}

		readonly BusinessObjectFactory factory;

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
				}
			}
		}

		ZInt packingOrder;

		public ZPropertyInfo PackingOrderInfo => GetZPropertyInfo(nameof(PackingOrder));

		#endregion

		#region PackingLineID

		public ZString PackingLineID
		{
			get => packingLineID;
			set
			{
				if (SetNonPersistentPropertyValue(PackingLineIDInfo, ref packingLineID, value))
				{
				}
			}
		}

		ZString packingLineID;

		public ZPropertyInfo PackingLineIDInfo => GetZPropertyInfo(nameof(PackingLineID));

		#endregion

		#region ContainerNumber

		public ZString ContainerNumber
		{
			get => containerNumber;
			set
			{
				if (SetNonPersistentPropertyValue(ContainerNumberInfo, ref containerNumber, value))
				{
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
				}
			}
		}

		ZShort itemNumber;

		ZPropertyInfo ItemNumberInfo => GetZPropertyInfo(nameof(ItemNumber));

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

		[IgnoreChanges]
		public ZBool IsVisible_CargoItem
		{
			get => isVisible_CargoItem;
			set
			{
				if (SetNonPersistentPropertyValue(IsVisible_CargoItemInfo, ref isVisible_CargoItem, value))
				{
					Validate(IsVisible_CargoItemInfo);
				}
			}
		}

		ZBool isVisible_CargoItem;

		public ZPropertyInfo IsVisible_CargoItemInfo => GetZPropertyInfo(nameof(IsVisible_CargoItem));

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

		public ZString PackageNumberOriginalValue
		{
			get => packageNumberOriginalValue;
			set
			{
				if (SetNonPersistentPropertyValue(PackageNumberOriginalValueInfo, ref packageNumberOriginalValue, value))
				{
					Validate(PackageNumberOriginalValueInfo);
				}
			}
		}

		ZString packageNumberOriginalValue;

		public ZPropertyInfo PackageNumberOriginalValueInfo => GetZPropertyInfo(nameof(PackageNumberOriginalValue));

		[IgnoreChanges]
		public ZBool IsVisible_PackageNumber
		{
			get => isVisible_PackageNumber;
			set
			{
				if (SetNonPersistentPropertyValue(IsVisible_PackageNumberInfo, ref isVisible_PackageNumber, value))
				{
					Validate(IsVisible_PackageNumberInfo);
				}
			}
		}

		ZBool isVisible_PackageNumber;

		public ZPropertyInfo IsVisible_PackageNumberInfo => GetZPropertyInfo(nameof(IsVisible_PackageNumber));

		#endregion

		#region Height

		public Measurement Height
		{
			get => height;
			set => height = SetChild(height, value);
		}

		Measurement height;

		IMeasurement IPackingLine.Height => Height;

		#endregion

		#region Length

		public Measurement Length
		{
			get => length;
			set => length = SetChild(length, value);
		}

		Measurement length;

		IMeasurement IPackingLine.Length => Length;

		#endregion

		#region Width

		public Measurement Width
		{
			get => width;
			set => width = SetChild(width, value);
		}

		Measurement width;

		IMeasurement IPackingLine.Width => Width;

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

		#region AnyPackCountIsZeroInGroupedSubPackLines

		public bool AnyPackCountIsZeroInGroupedSubPackLines { get; set; }

		public ZString ShipmentIDWithAnyPackCountIsZeroInGroupedSubPackLines { get; set; }

		#endregion

		#region AnyPackWeightIsZeroInGroupedSubPackLines

		public bool AnyPackWeightIsZeroInGroupedSubPackLines { get; set; }

		public ZString ShipmentIDWithAnyPackWeightIsZeroInGroupedSubPackLines { get; set; }

		#endregion

		#region AnyPackVolumeIsZeroInGroupedSubPackLines

		public bool AnyPackVolumeIsZeroInGroupedSubPackLines { get; set; }

		public ZString ShipmentIDWithAnyPackVolumeIsZeroInGroupedSubPackLines { get; set; }

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

		#region EntryType

		public ICodeDescription EntryType
		{
			get => entryType;
			set => entryType = SetChild(entryType, value);
		}

		ICodeDescription entryType;

		public ZString EntryTypeStatus
		{
			get => entryTypeStatus;
			set => entryTypeStatus = value;
		}

		ZString entryTypeStatus;

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

		[IgnoreChanges]
		public ZBool IsVisible_Complete
		{
			get => isVisible_Complete;
			set
			{
				if (SetNonPersistentPropertyValue(IsVisible_CompleteInfo, ref isVisible_Complete, value))
				{
					Validate(IsVisible_CompleteInfo);
				}
			}
		}

		ZBool isVisible_Complete;

		public ZPropertyInfo IsVisible_CompleteInfo => GetZPropertyInfo(nameof(IsVisible_Complete));

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

		[IgnoreChanges]
		public ZBool IsVisible_Shortage
		{
			get => isVisible_Shortage;
			set
			{
				if (SetNonPersistentPropertyValue(IsVisible_ShortageInfo, ref isVisible_Shortage, value))
				{
					Validate(IsVisible_ShortageInfo);
				}
			}
		}

		ZBool isVisible_Shortage;

		public ZPropertyInfo IsVisible_ShortageInfo => GetZPropertyInfo(nameof(IsVisible_Shortage));

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

		#region DocumentNoLabel

		public ZString DocumentNoLabel
		{
			get => documentNoLabel;
			set
			{
				if (SetNonPersistentPropertyValue(DocumentNoLabelInfo, ref documentNoLabel, value))
				{
					Validate(DocumentNoLabelInfo);
				}
			}
		}

		public ZString documentNoLabel;

		public ZPropertyInfo DocumentNoLabelInfo => GetZPropertyInfo(nameof(DocumentNoLabel));

		#endregion

		#region LoadingMeters

		public ZDecimal LoadingMeters
		{
			get => loadingMeters;
			set
			{
				if (SetNonPersistentPropertyValue(LoadingMetersInfo, ref loadingMeters, value))
				{
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

		#region GroupITNNumber

		public ZString GroupITNNumber
		{
			get => groupITNNumber;
			set
			{
				var formattedValue = string.Join(", ", value.ToString().Split(new string[] { ",", " ", ";", "/", System.Environment.NewLine, "\t" }, StringSplitOptions.RemoveEmptyEntries).Distinct(StringComparer.OrdinalIgnoreCase)).ToUpperInvariant();

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
				if (SetNonPersistentPropertyValue(GroupPOFNumberInfo, ref groupPOFNumber, RemoveDuplicatedSections(value)))
				{
					Validate(GroupPOFNumberInfo);
				}
			}
		}

		ZString groupPOFNumber;

		public ZPropertyInfo GroupPOFNumberInfo => GetZPropertyInfo(nameof(GroupPOFNumber));

		ZString RemoveDuplicatedSections(ZString str)
		{
			return string.Join(", ", str.ToString().Split(new string[] { ", " }, StringSplitOptions.RemoveEmptyEntries).Where(x => !string.IsNullOrEmpty(x.Trim())).Distinct(StringComparer.OrdinalIgnoreCase));
		}

		#endregion

		#region GroupPOFCode

		public ZString GroupPOFCode
		{
			get => groupPOFCode;
			set
			{
				if (SetNonPersistentPropertyValue(GroupPOFCodeInfo, ref groupPOFCode, RemoveDuplicatedSections(value)))
				{
					Validate(GroupPOFCodeInfo);
				}
			}
		}

		ZString groupPOFCode;

		public ZPropertyInfo GroupPOFCodeInfo => GetZPropertyInfo(nameof(GroupPOFCode));

		#endregion

		#region GroupExportStatementField1Type

		public ZString GroupExportStatementField1Type
		{
			get => groupExportStatementField1Type;
			set
			{
				if (SetNonPersistentPropertyValue(GroupExportStatementField1TypeInfo, ref groupExportStatementField1Type, RemoveDuplicatedSections(value)))
				{
					Validate(GroupExportStatementField1TypeInfo);
				}
			}
		}

		ZString groupExportStatementField1Type;

		public ZPropertyInfo GroupExportStatementField1TypeInfo => GetZPropertyInfo(nameof(GroupExportStatementField1Type));

		#endregion

		#region GroupExportStatementField1Code

		public ZString GroupExportStatementField1Code
		{
			get => groupExportStatementField1Code;
			set
			{
				if (SetNonPersistentPropertyValue(GroupExportStatementField1CodeInfo, ref groupExportStatementField1Code, value))
				{
					Validate(GroupExportStatementField1CodeInfo);
				}
			}
		}

		ZString groupExportStatementField1Code;

		public ZPropertyInfo GroupExportStatementField1CodeInfo => GetZPropertyInfo(nameof(GroupExportStatementField1Code));

		#endregion

		#region GroupExportStatementField2Type

		public ZString GroupExportStatementField2Type
		{
			get => groupExportStatementField2Type;
			set
			{
				if (SetNonPersistentPropertyValue(GroupExportStatementField2TypeInfo, ref groupExportStatementField2Type, RemoveDuplicatedSections(value)))
				{
					Validate(GroupExportStatementField2TypeInfo);
				}
			}
		}

		ZString groupExportStatementField2Type;

		public ZPropertyInfo GroupExportStatementField2TypeInfo => GetZPropertyInfo(nameof(GroupExportStatementField2Type));

		#endregion

		#region GroupExportStatementField2Code

		public ZString GroupExportStatementField2Code
		{
			get => groupExportStatementField2Code;
			set
			{
				if (SetNonPersistentPropertyValue(GroupExportStatementField2CodeInfo, ref groupExportStatementField2Code, value))
				{
					Validate(GroupExportStatementField2CodeInfo);
				}
			}
		}

		ZString groupExportStatementField2Code;

		public ZPropertyInfo GroupExportStatementField2CodeInfo => GetZPropertyInfo(nameof(GroupExportStatementField2Code));

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

		#region GroupUCRNumber

		public ZString GroupUCRNumber
		{
			get => groupUCRNumber;
			set
			{
				var formattedValue = string.Join(", ", value.ToString().Split(new string[] { ",", " ", ";", "/", System.Environment.NewLine, "\t" }, StringSplitOptions.RemoveEmptyEntries).Distinct(StringComparer.OrdinalIgnoreCase)).ToUpperInvariant();

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

		#region HasInnerPackLines

		public ZBool HasInnerPackLines
		{
			get => hasInnerPackLines;
			set
			{
				if (SetNonPersistentPropertyValue(HasInnerPackLinesInfo, ref hasInnerPackLines, value))
				{
				}
			}
		}

		ZBool hasInnerPackLines;

		public ZPropertyInfo HasInnerPackLinesInfo => GetZPropertyInfo(nameof(HasInnerPackLines));

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

		#region CUSCodes

		[List(nameof(EuropeanUnionECICSCusCodeList))]
		public ZString CUSCode1
		{
			get => cusCode1;
			set
			{
				if (SetNonPersistentPropertyValue(CUSCode1Info, ref cusCode1, value))
				{
					Validate(CUSCode1Info);
				}
			}
		}

		ZString cusCode1;

		public ZPropertyInfo CUSCode1Info => GetZPropertyInfo(nameof(CUSCode1));

		[List(nameof(EuropeanUnionECICSCusCodeList))]
		public ZString CUSCode2
		{
			get => cusCode2;
			set
			{
				if (SetNonPersistentPropertyValue(CUSCode2Info, ref cusCode2, value))
				{
					Validate(CUSCode2Info);
				}
			}
		}

		ZString cusCode2;

		public ZPropertyInfo CUSCode2Info => GetZPropertyInfo(nameof(CUSCode2));

		[List(nameof(EuropeanUnionECICSCusCodeList))]
		public ZString CUSCode3
		{
			get => cusCode3;
			set
			{
				if (SetNonPersistentPropertyValue(CUSCode3Info, ref cusCode3, value))
				{
					Validate(CUSCode3Info);
				}
			}
		}

		ZString cusCode3;

		public ZPropertyInfo CUSCode3Info => GetZPropertyInfo(nameof(CUSCode3));

		[List(nameof(EuropeanUnionECICSCusCodeList))]
		public ZString CUSCode4
		{
			get => cusCode4;
			set
			{
				if (SetNonPersistentPropertyValue(CUSCode4Info, ref cusCode4, value))
				{
					Validate(CUSCode4Info);
				}
			}
		}

		ZString cusCode4;

		public ZPropertyInfo CUSCode4Info => GetZPropertyInfo(nameof(CUSCode4));

		[List(nameof(EuropeanUnionECICSCusCodeList))]
		public ZString CUSCode5
		{
			get => cusCode5;
			set
			{
				if (SetNonPersistentPropertyValue(CUSCode5Info, ref cusCode5, value))
				{
					Validate(CUSCode5Info);
				}
			}
		}

		ZString cusCode5;

		public ZPropertyInfo CUSCode5Info => GetZPropertyInfo(nameof(CUSCode5));

		[List(nameof(EuropeanUnionECICSCusCodeList))]
		public ZString CUSCode6
		{
			get => cusCode6;
			set
			{
				if (SetNonPersistentPropertyValue(CUSCode6Info, ref cusCode6, value))
				{
					Validate(CUSCode6Info);
				}
			}
		}

		ZString cusCode6;

		public ZPropertyInfo CUSCode6Info => GetZPropertyInfo(nameof(CUSCode6));

		[List(nameof(EuropeanUnionECICSCusCodeList))]
		public ZString CUSCode7
		{
			get => cusCode7;
			set
			{
				if (SetNonPersistentPropertyValue(CUSCode7Info, ref cusCode7, value))
				{
					Validate(CUSCode7Info);
				}
			}
		}

		ZString cusCode7;

		public ZPropertyInfo CUSCode7Info => GetZPropertyInfo(nameof(CUSCode7));

		[List(nameof(EuropeanUnionECICSCusCodeList))]
		public ZString CUSCode8
		{
			get => cusCode8;
			set
			{
				if (SetNonPersistentPropertyValue(CUSCode8Info, ref cusCode8, value))
				{
					Validate(CUSCode8Info);
				}
			}
		}

		ZString cusCode8;

		public ZPropertyInfo CUSCode8Info => GetZPropertyInfo(nameof(CUSCode8));

		[List(nameof(EuropeanUnionECICSCusCodeList))]
		public ZString CUSCode9
		{
			get => cusCode9;
			set
			{
				if (SetNonPersistentPropertyValue(CUSCode9Info, ref cusCode9, value))
				{
					Validate(CUSCode9Info);
				}
			}
		}

		ZString cusCode9;

		public ZPropertyInfo CUSCode9Info => GetZPropertyInfo(nameof(CUSCode9));

		public CustomsOfficeCodeCollection EuropeanUnionECICSCusCodeList
		{
			get
			{
				if (europeanUnionECICSList == null)
				{
					europeanUnionECICSList = factory != null
						? factory.GetCachedValue("Enterprise.Freight.Forwarding.Documents.DocDataObjects.PackingLine.EuropeanUnionECICSCusCodeList", () => CustomsOfficeCodeCollectionHelper.GetEuropeanUnionECICSList(factory))
						: CustomsOfficeCodeCollectionHelper.GetEuropeanUnionECICSList(new BusinessObjectFactory());
				}

				return europeanUnionECICSList;
			}
		}

		CustomsOfficeCodeCollection europeanUnionECICSList;

		#endregion

		#region  HBLPaymentType

		[List(nameof(HBLPaymentTypeList))]
		public ZString HBLPaymentType
		{
			get => hBLPaymentType;
			set
			{
				if (SetNonPersistentPropertyValue(HBLPaymentTypeInfo, ref hBLPaymentType, value))
				{
					Validate(HBLPaymentTypeInfo);
				}
			}
		}

		ZString hBLPaymentType;

		public ZPropertyInfo HBLPaymentTypeInfo => GetZPropertyInfo(nameof(HBLPaymentType));

		public CodeDescriptionPairList HBLPaymentTypeList
		{
			get
			{
				return factory.GetCachedValue("Enterprise.Freight.Forwarding.Documents.DocDataObjects.PackingLine.HBLPaymentTypeList", () => FreightCodePairLists.PackLineHouseBillPaymentTypeList());
			}
		}

		#endregion
	}
}
