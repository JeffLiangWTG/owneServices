using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	public sealed class Container : DocDataObject, IContainer
	{
		#region Ctor

		public Container(object identifier = default)
			: base(identifier)
		{
		}

		#endregion

		#region Number

		public ZString Number
		{
			get => number;
			set
			{
				if (SetNonPersistentPropertyValue(NumberInfo, ref number, value))
				{
					Validate(NumberInfo);
				}
			}
		}

		ZString number;

		public ZPropertyInfo NumberInfo => GetZPropertyInfo(nameof(Number));

		#endregion

		#region Type

		public ContainerType Type
		{
			get => type;
			set => type = SetChild(type, value);
		}

		ContainerType type;

		#endregion

		#region ContainerCount

		public ZInt ContainerCount
		{
			get => containerCount;
			set
			{
				if (SetNonPersistentPropertyValue(ContainerCountInfo, ref containerCount, value))
				{
					Validate(ContainerCountInfo);
				}
			}
		}

		ZInt containerCount;

		public ZPropertyInfo ContainerCountInfo => GetZPropertyInfo(nameof(ContainerCount));

		#endregion

		#region PalletCount

		public ZInt PalletCount
		{
			get => palletCount;
			set
			{
				if (SetNonPersistentPropertyValue(PalletCountInfo, ref palletCount, value))
				{
					Validate(PalletCountInfo);
				}
			}
		}

		ZInt palletCount;

		public ZPropertyInfo PalletCountInfo => GetZPropertyInfo(nameof(PalletCount));

		#endregion

		#region ItemCount

		public ZInt ItemCount
		{
			get => itemCount;
			set
			{
				if (SetNonPersistentPropertyValue(ItemCountInfo, ref itemCount, value))
				{
					Validate(ItemCountInfo);
				}
			}
		}

		ZInt itemCount;

		public ZPropertyInfo ItemCountInfo => GetZPropertyInfo(nameof(ItemCount));

		#endregion

		#region IsEmpty

		public ZBool IsEmpty
		{
			get => isEmpty;
			set
			{
				if (SetNonPersistentPropertyValue(IsEmptyInfo, ref isEmpty, value))
				{
					Validate(IsEmptyInfo);
				}
			}
		}

		ZBool isEmpty;

		public ZPropertyInfo IsEmptyInfo => GetZPropertyInfo(nameof(IsEmpty));

		#endregion

		#region IsPartOf

		public ZBool IsPartOf
		{
			get => isPartOf;
			set
			{
				if (SetNonPersistentPropertyValue(IsPartOfInfo, ref isPartOf, value))
				{
					Validate(IsPartOfInfo);
				}
			}
		}

		ZBool isPartOf;

		public ZPropertyInfo IsPartOfInfo => GetZPropertyInfo(nameof(IsPartOf));

		#endregion

		#region HasControlledAtmosphere

		public ZBool HasControlledAtmosphere
		{
			get => hasControlledAtmosphere;
			set
			{
				if (SetNonPersistentPropertyValue(HasControlledAtmosphereInfo, ref hasControlledAtmosphere, value))
				{
					Validate(HasControlledAtmosphereInfo);
				}
			}
		}

		ZBool hasControlledAtmosphere;

		public ZPropertyInfo HasControlledAtmosphereInfo => GetZPropertyInfo(nameof(HasControlledAtmosphere));

		#endregion

		#region TemperatureRecorderSerialNumber

		public ZString TemperatureRecorderSerialNumber
		{
			get => temperatureRecorderSerialNumber;
			set
			{
				if (SetNonPersistentPropertyValue(TemperatureRecorderSerialNumberInfo, ref temperatureRecorderSerialNumber, value))
				{
					Validate(TemperatureRecorderSerialNumberInfo);
				}
			}
		}

		ZString temperatureRecorderSerialNumber;

		public ZPropertyInfo TemperatureRecorderSerialNumberInfo => GetZPropertyInfo(nameof(TemperatureRecorderSerialNumber));

		#endregion

		#region Genset

		public ZBool Genset
		{
			get => genset;
			set
			{
				if (SetNonPersistentPropertyValue(GensetInfo, ref genset, value))
				{
					Validate(GensetInfo);
				}
			}
		}

		ZBool genset;

		public ZPropertyInfo GensetInfo => GetZPropertyInfo(nameof(Genset));

		#endregion

		#region SetTemperature

		public Measurement SetTemperature
		{
			get => setTemperature;
			set => setTemperature = SetChild(setTemperature, value);
		}

		Measurement setTemperature;

		#endregion

		#region Humidity

		public Measurement Humidity
		{
			get => humidity;
			set => humidity = SetChild(humidity, value);
		}

		Measurement humidity;

		#endregion

		#region AirVentFlow

		public Measurement AirVentFlow
		{
			get => airVentFlow;
			set => airVentFlow = SetChild(airVentFlow, value);
		}

		Measurement airVentFlow;

		#endregion

		#region Seal

		public ZString Seal
		{
			get => seal;
			set
			{
				if (SetNonPersistentPropertyValue(SealInfo, ref seal, value))
				{
					Validate(SealInfo);
				}
			}
		}

		ZString seal;

		public ZPropertyInfo SealInfo => GetZPropertyInfo(nameof(Seal));

		#endregion

		#region SealPartyType

		public ICodeDescription SealPartyType
		{
			get => sealPartyType;
			set => sealPartyType = SetChild(sealPartyType, value);
		}

		ICodeDescription sealPartyType;

		#endregion

		#region SecondSeal

		public ZString SecondSeal
		{
			get => secondSeal;
			set
			{
				if (SetNonPersistentPropertyValue(SecondSealInfo, ref secondSeal, value))
				{
					Validate(SecondSealInfo);
				}
			}
		}

		ZString secondSeal;

		public ZPropertyInfo SecondSealInfo => GetZPropertyInfo(nameof(SecondSeal));

		#endregion

		#region SecondSealPartyType

		public ICodeDescription SecondSealPartyType
		{
			get => secondSealPartyType;
			set => secondSealPartyType = SetChild(secondSealPartyType, value);
		}

		ICodeDescription secondSealPartyType;

		#endregion

		#region ThirdSeal

		public ZString ThirdSeal
		{
			get => thirdSeal;
			set
			{
				if (SetNonPersistentPropertyValue(ThirdSealInfo, ref thirdSeal, value))
				{
					Validate(ThirdSealInfo);
				}
			}
		}

		ZString thirdSeal;

		public ZPropertyInfo ThirdSealInfo => GetZPropertyInfo(nameof(ThirdSeal));

		#endregion

		#region ThirdSealPartyType

		public ICodeDescription ThirdSealPartyType
		{
			get => thirdSealPartyType;
			set => thirdSealPartyType = SetChild(thirdSealPartyType, value);
		}

		ICodeDescription thirdSealPartyType;

		#endregion

		#region EmptyRequired

		public ZDateTime EmptyRequired
		{
			get => emptyRequired;
			set
			{
				if (SetNonPersistentPropertyValue(EmptyRequiredInfo, ref emptyRequired, value))
				{
					Validate(EmptyRequiredInfo);
				}
			}
		}

		ZDateTime emptyRequired;

		public ZPropertyInfo EmptyRequiredInfo => GetZPropertyInfo(nameof(EmptyRequired));

		#endregion

		#region EmptyReadyForReturn

		public ZDateTime EmptyReadyForReturn
		{
			get => emptyReadyForReturn;
			set
			{
				if (SetNonPersistentPropertyValue(EmptyReadyForReturnInfo, ref emptyReadyForReturn, value))
				{
					Validate(EmptyReadyForReturnInfo);
				}
			}
		}

		ZDateTime emptyReadyForReturn;

		public ZPropertyInfo EmptyReadyForReturnInfo => GetZPropertyInfo(nameof(EmptyReadyForReturn));

		#endregion

		#region EmptyReturnedBy

		public ZDateTime EmptyReturnedBy
		{
			get => emptyReturnedBy;
			set
			{
				if (SetNonPersistentPropertyValue(EmptyReturnedByInfo, ref emptyReturnedBy, value))
				{
					Validate(EmptyReturnedByInfo);
				}
			}
		}

		ZDateTime emptyReturnedBy;

		public ZPropertyInfo EmptyReturnedByInfo => GetZPropertyInfo(nameof(EmptyReturnedBy));

		#endregion

		#region EmptyReturnReference

		public ZString EmptyReturnReference
		{
			get => emptyReturnReference;
			set
			{
				if (SetNonPersistentPropertyValue(EmptyReturnReferenceInfo, ref emptyReturnReference, value))
				{
					Validate(EmptyReturnReferenceInfo);
				}
			}
		}

		ZString emptyReturnReference;

		public ZPropertyInfo EmptyReturnReferenceInfo => GetZPropertyInfo(nameof(EmptyReturnReference));

		#endregion

		#region DepartureEstimatedPickup

		public ZDateTime DepartureEstimatedPickup
		{
			get => departureEstimatedPickup;
			set
			{
				if (SetNonPersistentPropertyValue(DepartureEstimatedPickupInfo, ref departureEstimatedPickup, value))
				{
					Validate(DepartureEstimatedPickupInfo);
				}
			}
		}

		ZDateTime departureEstimatedPickup;

		public ZPropertyInfo DepartureEstimatedPickupInfo => GetZPropertyInfo(nameof(DepartureEstimatedPickup));

		#endregion

		#region Measurements

		#region GoodsWeight

		public Measurement GoodsWeight
		{
			get => goodsWeight;
			set => goodsWeight = SetChild(goodsWeight, value);
		}

		Measurement goodsWeight;

		#endregion

		#region TareWeight

		public Measurement TareWeight
		{
			get => tareWeight;
			set => tareWeight = SetChild(tareWeight, value);
		}

		Measurement tareWeight;

		#endregion

		#region Dunnage

		public Measurement Dunnage
		{
			get => dunnage;
			set => dunnage = SetChild(dunnage, value);
		}

		Measurement dunnage;

		#endregion

		#region OverhangFront

		public IMeasurement OverhangFront
		{
			get => overhangFront;
			set => overhangFront = SetChild(overhangFront, value);
		}

		IMeasurement overhangFront;

		#endregion

		#region OverhangBack

		public IMeasurement OverhangBack
		{
			get => overhangBack;
			set => overhangBack = SetChild(overhangBack, value);
		}

		IMeasurement overhangBack;

		#endregion

		#region OverhangLeft

		public IMeasurement OverhangLeft
		{
			get => overhangLeft;
			set => overhangLeft = SetChild(overhangLeft, value);
		}

		IMeasurement overhangLeft;

		#endregion

		#region OverhangRight

		public IMeasurement OverhangRight
		{
			get => overhangRight;
			set => overhangRight = SetChild(overhangRight, value);
		}

		IMeasurement overhangRight;

		#endregion

		#region OverhangLength

		public IMeasurement OverhangLength
		{
			get => overhangLength;
			set => overhangLength = SetChild(overhangLength, value);
		}

		IMeasurement overhangLength;

		#endregion

		#region OverhangWidth

		public IMeasurement OverhangWidth
		{
			get => overhangWidth;
			set => overhangWidth = SetChild(overhangWidth, value);
		}

		IMeasurement overhangWidth;

		#endregion

		#region OverhangHeight

		public IMeasurement OverhangHeight
		{
			get => overhangHeight;
			set => overhangHeight = SetChild(overhangHeight, value);
		}

		IMeasurement overhangHeight;

		#endregion

		#endregion

		#region DepartureContainerYard

		public IAddress DepartureContainerYard
		{
			get => departureContainerYard;
			set => departureContainerYard = SetChild(departureContainerYard, value);
		}

		IAddress departureContainerYard;

		#endregion

		#region ContainerMode

		public ICodeDescription ContainerMode
		{
			get => containerMode;
			set => containerMode = SetChild(containerMode, value);
		}
		ICodeDescription containerMode;

		#endregion

		#region ContainerQuality

		public ICodeDescription ContainerQuality
		{
			get => containerQuality;
			set => containerQuality = SetChild(containerQuality, value);
		}

		ICodeDescription containerQuality;

		#endregion

		#region Commodity

		public ICodeDescription Commodity
		{
			get => commodity;
			set => commodity = SetChild(commodity, value);
		}

		ICodeDescription commodity;

		#endregion

		#region VGM

		#region VerifiedByAddress

		public Address VerifiedByAddress
		{
			get => verifiedByAddress;
			set => verifiedByAddress = SetChild(verifiedByAddress, value);
		}
		Address verifiedByAddress;

		#endregion

		#region GrossWeight

		public IMeasurement GrossWeight
		{
			get => grossWeight;
			set => grossWeight = SetChild(grossWeight, value);
		}

		IMeasurement grossWeight;

		#endregion

		#region NetWeight

		public IMeasurement NetWeight
		{
			get => netWeight;
			set => netWeight = SetChild(netWeight, value);
		}

		IMeasurement netWeight;

		#endregion

		#region Volume

		public IMeasurement Volume
		{
			get => volume;
			set => volume = SetChild(volume, value);
		}

		IMeasurement volume;

		#endregion

		#region GrossVolume

		public IMeasurement GrossVolume
		{
			get => grossVolume;
			set => grossVolume = SetChild(grossVolume, value);
		}

		IMeasurement grossVolume;

		#endregion

		#region VerifiedMethod

		public CodeDescription VerifiedMethod
		{
			get => verifiedMethod;
			set => verifiedMethod = SetChild(verifiedMethod, value);
		}
		CodeDescription verifiedMethod;

		#endregion

		#region VerifiedStatus

		public ICodeDescription VerifiedStatus
		{
			get => verifiedStatus;
			set => verifiedStatus = SetChild(verifiedStatus, value);
		}

		ICodeDescription verifiedStatus;

		#endregion

		#region VerifiedDate

		public ZDateTime VerifiedDate
		{
			get => verifiedDate;
			set
			{
				if (SetNonPersistentPropertyValue(VerifiedDateInfo, ref verifiedDate, value))
				{
					Validate(VerifiedDateInfo);
				}
			}
		}

		ZDateTime verifiedDate;

		public ZPropertyInfo VerifiedDateInfo => GetZPropertyInfo(nameof(VerifiedDate));

		#endregion

		#endregion

		#region IsShipperOwned

		public ZBool IsShipperOwned
		{
			get => isShipperOwned;
			set
			{
				if (SetNonPersistentPropertyValue(IsShipperOwnedInfo, ref isShipperOwned, value))
				{
					Validate(IsShipperOwnedInfo);
				}
			}
		}

		ZBool isShipperOwned;

		public ZPropertyInfo IsShipperOwnedInfo => GetZPropertyInfo(nameof(IsShipperOwned));

		#endregion

		#region IsNonOperativeReefer

		public ZBool IsNonOperativeReefer
		{
			get => isNonOperativeReefer;
			set
			{
				if (SetNonPersistentPropertyValue(IsNonOperativeReeferInfo, ref isNonOperativeReefer, value))
				{
					Validate(IsNonOperativeReeferInfo);
				}
			}
		}

		ZBool isNonOperativeReefer;

		public ZPropertyInfo IsNonOperativeReeferInfo => GetZPropertyInfo(nameof(IsNonOperativeReefer));

		#endregion

		#region IsChargeable

		public ZBool IsChargeable
		{
			get => isChargeable;
			set
			{
				if (SetNonPersistentPropertyValue(IsChargeableInfo, ref isChargeable, value))
				{
					Validate(IsChargeableInfo);
				}
			}
		}

		ZBool isChargeable;

		public ZPropertyInfo IsChargeableInfo => GetZPropertyInfo(nameof(IsChargeable));

		#endregion

		#region IsDamaged

		public ZBool IsDamaged
		{
			get => isDamaged;
			set
			{
				if (SetNonPersistentPropertyValue(IsDamagedInfo, ref isDamaged, value))
				{
					Validate(IsDamagedInfo);
				}
			}
		}

		ZBool isDamaged;

		public ZPropertyInfo IsDamagedInfo => GetZPropertyInfo(nameof(IsDamaged));

		#endregion

		#region IsSealOk

		public ZBool IsSealOk
		{
			get => isSealOk;
			set
			{
				if (SetNonPersistentPropertyValue(IsSealOkInfo, ref isSealOk, value))
				{
					Validate(IsSealOkInfo);
				}
			}
		}

		ZBool isSealOk;

		public ZPropertyInfo IsSealOkInfo => GetZPropertyInfo(nameof(IsSealOk));

		#endregion

		#region LCLAvailable

		public ZDateTime LCLAvailable
		{
			get => lclAvailable;
			set
			{
				if (SetNonPersistentPropertyValue(LCLAvailableInfo, ref lclAvailable, value))
				{
					Validate(LCLAvailableInfo);
				}
			}
		}

		ZDateTime lclAvailable;

		public ZPropertyInfo LCLAvailableInfo => GetZPropertyInfo(nameof(LCLAvailable));

		#endregion

		#region LCLStorageCommences

		public ZDateTime LCLStorageCommences
		{
			get => lclStorageCommences;
			set
			{
				if (SetNonPersistentPropertyValue(LCLStorageCommencesInfo, ref lclStorageCommences, value))
				{
					Validate(LCLStorageCommencesInfo);
				}
			}
		}

		ZDateTime lclStorageCommences;

		public ZPropertyInfo LCLStorageCommencesInfo => GetZPropertyInfo(nameof(LCLStorageCommences));

		#endregion

		#region LCLUnpack

		public ZDateTime LCLUnpack
		{
			get => lclUnpack;
			set
			{
				if (SetNonPersistentPropertyValue(LCLUnpackInfo, ref lclUnpack, value))
				{
					Validate(LCLUnpackInfo);
				}
			}
		}

		ZDateTime lclUnpack;

		public ZPropertyInfo LCLUnpackInfo => GetZPropertyInfo(nameof(LCLUnpack));

		#endregion

		#region PackDate

		public ZDateTime PackDate
		{
			get => packDate;
			set
			{
				if (SetNonPersistentPropertyValue(PackDateInfo, ref packDate, value))
				{
					Validate(PackDateInfo);
				}
			}
		}

		ZDateTime packDate;

		public ZPropertyInfo PackDateInfo => GetZPropertyInfo(nameof(PackDate));

		#endregion

		#region ArrivalCartageAdvised

		public ZDateTime ArrivalCartageAdvised
		{
			get => arrivalCartageAdvised;
			set
			{
				if (SetNonPersistentPropertyValue(ArrivalCartageAdvisedInfo, ref arrivalCartageAdvised, value))
				{
					Validate(ArrivalCartageAdvisedInfo);
				}
			}
		}

		ZDateTime arrivalCartageAdvised;

		public ZPropertyInfo ArrivalCartageAdvisedInfo => GetZPropertyInfo(nameof(ArrivalCartageAdvised));

		#endregion

		#region ArrivalCartageComplete

		public ZDateTime ArrivalCartageComplete
		{
			get => arrivalCartageComplete;
			set
			{
				if (SetNonPersistentPropertyValue(ArrivalCartageCompleteInfo, ref arrivalCartageComplete, value))
				{
					Validate(ArrivalCartageCompleteInfo);
				}
			}
		}

		ZDateTime arrivalCartageComplete;

		public ZPropertyInfo ArrivalCartageCompleteInfo => GetZPropertyInfo(nameof(ArrivalCartageComplete));

		#endregion

		#region ArrivalTruckWaitTime

		[ZDateTimeDurationValueExclude1900]
		public ZDateTime ArrivalTruckWaitTime
		{
			get => arrivalTruckWaitTime;
			set
			{
				if (SetNonPersistentPropertyValue(ArrivalTruckWaitTimeInfo, ref arrivalTruckWaitTime, value.ConvertToDurationBasedDate(ArrivalTruckWaitTimeInfo)))
				{
					Validate(ArrivalTruckWaitTimeInfo);
				}
			}
		}

		ZDateTime arrivalTruckWaitTime;

		public ZPropertyInfo ArrivalTruckWaitTimeInfo => GetZPropertyInfo(nameof(ArrivalTruckWaitTime));

		#endregion

		#region ArrivalTruckWaitCost

		public ZDecimal ArrivalTruckWaitCost
		{
			get => arrivalTruckWaitCost;
			set
			{
				if (SetNonPersistentPropertyValue(ArrivalTruckWaitCostInfo, ref arrivalTruckWaitCost, value))
				{
					Validate(ArrivalTruckWaitCostInfo);
				}
			}
		}

		ZDecimal arrivalTruckWaitCost;

		public ZPropertyInfo ArrivalTruckWaitCostInfo => GetZPropertyInfo(nameof(ArrivalTruckWaitCost));

		#endregion

		#region ArrivalCartageReference

		public ZString ArrivalCartageReference
		{
			get => arrivalCartageReference;
			set
			{
				if (SetNonPersistentPropertyValue(ArrivalCartageReferenceInfo, ref arrivalCartageReference, value))
				{
					Validate(ArrivalCartageReferenceInfo);
				}
			}
		}

		ZString arrivalCartageReference;

		public ZPropertyInfo ArrivalCartageReferenceInfo => GetZPropertyInfo(nameof(ArrivalCartageReference));

		#endregion

		#region ArrivalDeliveryRequiredBy

		public ZDateTime ArrivalDeliveryRequiredBy
		{
			get => arrivalDeliveryRequiredBy;
			set
			{
				if (SetNonPersistentPropertyValue(ArrivalDeliveryRequiredByInfo, ref arrivalDeliveryRequiredBy, value))
				{
					Validate(ArrivalDeliveryRequiredByInfo);
				}
			}
		}

		ZDateTime arrivalDeliveryRequiredBy;

		public ZPropertyInfo ArrivalDeliveryRequiredByInfo => GetZPropertyInfo(nameof(ArrivalDeliveryRequiredBy));

		#endregion

		#region ArrivalEstimatedDelivery

		public ZDateTime ArrivalEstimatedDelivery
		{
			get => arrivalEstimatedDelivery;
			set
			{
				if (SetNonPersistentPropertyValue(ArrivalEstimatedDeliveryInfo, ref arrivalEstimatedDelivery, value))
				{
					Validate(ArrivalEstimatedDeliveryInfo);
				}
			}
		}

		ZDateTime arrivalEstimatedDelivery;

		public ZPropertyInfo ArrivalEstimatedDeliveryInfo => GetZPropertyInfo(nameof(ArrivalEstimatedDelivery));

		#endregion

		#region ArrivalPickupByRail

		public ZBool ArrivalPickupByRail
		{
			get => arrivalPickupByRail;
			set
			{
				if (SetNonPersistentPropertyValue(ArrivalPickupByRailInfo, ref arrivalPickupByRail, value))
				{
					Validate(ArrivalPickupByRailInfo);
				}
			}
		}

		ZBool arrivalPickupByRail;

		public ZPropertyInfo ArrivalPickupByRailInfo => GetZPropertyInfo(nameof(ArrivalPickupByRail));

		#endregion

		#region ArrivalSlotDateTime

		public ZDateTime ArrivalSlotDateTime
		{
			get => arrivalSlotDateTime;
			set
			{
				if (SetNonPersistentPropertyValue(ArrivalSlotDateTimeInfo, ref arrivalSlotDateTime, value))
				{
					Validate(ArrivalSlotDateTimeInfo);
				}
			}
		}

		ZDateTime arrivalSlotDateTime;

		public ZPropertyInfo ArrivalSlotDateTimeInfo => GetZPropertyInfo(nameof(ArrivalSlotDateTime));

		#endregion

		#region ArrivalSlotReference

		public ZString ArrivalSlotReference
		{
			get => arrivalSlotReference;
			set
			{
				if (SetNonPersistentPropertyValue(ArrivalSlotReferenceInfo, ref arrivalSlotReference, value))
				{
					Validate(ArrivalSlotReferenceInfo);
				}
			}
		}

		ZString arrivalSlotReference;

		public ZPropertyInfo ArrivalSlotReferenceInfo => GetZPropertyInfo(nameof(ArrivalSlotReference));

		#endregion

		#region ArrivalCarrierDetentionCost

		public ZDecimal ArrivalCarrierDetentionCost
		{
			get => arrivalCarrierDetentionCost;
			set
			{
				if (SetNonPersistentPropertyValue(ArrivalCarrierDetentionCostInfo, ref arrivalCarrierDetentionCost, value))
				{
					Validate(ArrivalCarrierDetentionCostInfo);
				}
			}
		}

		ZDecimal arrivalCarrierDetentionCost;

		public ZPropertyInfo ArrivalCarrierDetentionCostInfo => GetZPropertyInfo(nameof(ArrivalCarrierDetentionCost));

		#endregion

		#region ArrivalCarrierDetentionDays

		public ZByte ArrivalCarrierDetentionDays
		{
			get => arrivalCarrierDetentionDays;
			set
			{
				if (SetNonPersistentPropertyValue(ArrivalCarrierDetentionDaysInfo, ref arrivalCarrierDetentionDays, value))
				{
					Validate(ArrivalCarrierDetentionDaysInfo);
				}
			}
		}

		ZByte arrivalCarrierDetentionDays;

		public ZPropertyInfo ArrivalCarrierDetentionDaysInfo => GetZPropertyInfo(nameof(ArrivalCarrierDetentionDays));

		#endregion

		#region ContainerImportDORelease

		public ZString ContainerImportDORelease
		{
			get => containerImportDORelease;
			set
			{
				if (SetNonPersistentPropertyValue(ContainerImportDOReleaseInfo, ref containerImportDORelease, value))
				{
					Validate(ContainerImportDOReleaseInfo);
				}
			}
		}

		ZString containerImportDORelease;

		public ZPropertyInfo ContainerImportDOReleaseInfo => GetZPropertyInfo(nameof(ContainerImportDORelease));

		#endregion

		#region ContainerParkEmptyPickupGateOut

		public ZDateTime ContainerParkEmptyPickupGateOut
		{
			get => containerParkEmptyPickupGateOut;
			set
			{
				if (SetNonPersistentPropertyValue(ContainerParkEmptyPickupGateOutInfo, ref containerParkEmptyPickupGateOut, value))
				{
					Validate(ContainerParkEmptyPickupGateOutInfo);
				}
			}
		}

		ZDateTime containerParkEmptyPickupGateOut;

		public ZPropertyInfo ContainerParkEmptyPickupGateOutInfo => GetZPropertyInfo(nameof(ContainerParkEmptyPickupGateOut));

		#endregion

		#region ContainerParkEmptyReturnGateIn

		public ZDateTime ContainerParkEmptyReturnGateIn
		{
			get => containerParkEmptyReturnGateIn;
			set
			{
				if (SetNonPersistentPropertyValue(ContainerParkEmptyReturnGateInInfo, ref containerParkEmptyReturnGateIn, value))
				{
					Validate(ContainerParkEmptyReturnGateInInfo);
				}
			}
		}

		ZDateTime containerParkEmptyReturnGateIn;

		public ZPropertyInfo ContainerParkEmptyReturnGateInInfo => GetZPropertyInfo(nameof(ContainerParkEmptyReturnGateIn));

		#endregion

		#region DeliveryMode

		public ZString DeliveryMode
		{
			get => deliveryMode;
			set
			{
				if (SetNonPersistentPropertyValue(DeliveryModeInfo, ref deliveryMode, value))
				{
					Validate(DeliveryModeInfo);
				}
			}
		}

		ZString deliveryMode;

		public ZPropertyInfo DeliveryModeInfo => GetZPropertyInfo(nameof(DeliveryMode));

		#endregion

		#region DeliverySequence

		public ZShort DeliverySequence
		{
			get => deliverySequence;
			set
			{
				if (SetNonPersistentPropertyValue(DeliverySequenceInfo, ref deliverySequence, value))
				{
					Validate(DeliverySequenceInfo);
				}
			}
		}

		ZShort deliverySequence;

		public ZPropertyInfo DeliverySequenceInfo => GetZPropertyInfo(nameof(DeliverySequence));

		#endregion

		#region DepartureCartageAdvised

		public ZDateTime DepartureCartageAdvised
		{
			get => departureCartageAdvised;
			set
			{
				if (SetNonPersistentPropertyValue(DepartureCartageAdvisedInfo, ref departureCartageAdvised, value))
				{
					Validate(DepartureCartageAdvisedInfo);
				}
			}
		}

		ZDateTime departureCartageAdvised;

		public ZPropertyInfo DepartureCartageAdvisedInfo => GetZPropertyInfo(nameof(DepartureCartageAdvised));

		#endregion

		#region DepartureCartageComplete

		public ZDateTime DepartureCartageComplete
		{
			get => departureCartageComplete;
			set
			{
				if (SetNonPersistentPropertyValue(DepartureCartageCompleteInfo, ref departureCartageComplete, value))
				{
					Validate(DepartureCartageCompleteInfo);
				}
			}
		}

		ZDateTime departureCartageComplete;

		public ZPropertyInfo DepartureCartageCompleteInfo => GetZPropertyInfo(nameof(DepartureCartageComplete));

		#endregion

		#region DepartureTruckWaitTime

		[ZDateTimeDurationValueExclude1900]
		public ZDateTime DepartureTruckWaitTime
		{
			get => departureTruckWaitTime;
			set
			{
				if (SetNonPersistentPropertyValue(DepartureTruckWaitTimeInfo, ref departureTruckWaitTime, value.ConvertToDurationBasedDate(DepartureTruckWaitTimeInfo)))
				{
					Validate(DepartureTruckWaitTimeInfo);
				}
			}
		}

		ZDateTime departureTruckWaitTime;

		public ZPropertyInfo DepartureTruckWaitTimeInfo => GetZPropertyInfo(nameof(DepartureTruckWaitTime));

		#endregion

		#region DepartureTruckWaitCost

		public ZDecimal DepartureTruckWaitCost
		{
			get => departureTruckWaitCost;
			set
			{
				if (SetNonPersistentPropertyValue(DepartureTruckWaitCostInfo, ref departureTruckWaitCost, value))
				{
					Validate(DepartureTruckWaitCostInfo);
				}
			}
		}

		ZDecimal departureTruckWaitCost;

		public ZPropertyInfo DepartureTruckWaitCostInfo => GetZPropertyInfo(nameof(DepartureTruckWaitCost));

		#endregion

		#region DepartureCartageReference

		public ZString DepartureCartageReference
		{
			get => departureCartageReference;
			set
			{
				if (SetNonPersistentPropertyValue(DepartureCartageReferenceInfo, ref departureCartageReference, value))
				{
					Validate(DepartureCartageReferenceInfo);
				}
			}
		}

		ZString departureCartageReference;

		public ZPropertyInfo DepartureCartageReferenceInfo => GetZPropertyInfo(nameof(DepartureCartageReference));

		#endregion

		#region DepartureDeliveryByRail

		public ZBool DepartureDeliveryByRail
		{
			get => departureDeliveryByRail;
			set
			{
				if (SetNonPersistentPropertyValue(DepartureDeliveryByRailInfo, ref departureDeliveryByRail, value))
				{
					Validate(DepartureDeliveryByRailInfo);
				}
			}
		}

		ZBool departureDeliveryByRail;

		public ZPropertyInfo DepartureDeliveryByRailInfo => GetZPropertyInfo(nameof(DepartureDeliveryByRail));

		#endregion

		#region DepartureSlotDateTime

		public ZDateTime DepartureSlotDateTime
		{
			get => departureSlotDateTime;
			set
			{
				if (SetNonPersistentPropertyValue(DepartureSlotDateTimeInfo, ref departureSlotDateTime, value))
				{
					Validate(DepartureSlotDateTimeInfo);
				}
			}
		}

		ZDateTime departureSlotDateTime;

		public ZPropertyInfo DepartureSlotDateTimeInfo => GetZPropertyInfo(nameof(DepartureSlotDateTime));

		#endregion

		#region DepartureSlotReference

		public ZString DepartureSlotReference
		{
			get => departureSlotReference;
			set
			{
				if (SetNonPersistentPropertyValue(DepartureSlotReferenceInfo, ref departureSlotReference, value))
				{
					Validate(DepartureSlotReferenceInfo);
				}
			}
		}

		ZString departureSlotReference;

		public ZPropertyInfo DepartureSlotReferenceInfo => GetZPropertyInfo(nameof(DepartureSlotReference));

		#endregion

		#region ExportDepotCustomsReference

		public ZString ExportDepotCustomsReference
		{
			get => exportDepotCustomsReference;
			set
			{
				if (SetNonPersistentPropertyValue(ExportDepotCustomsReferenceInfo, ref exportDepotCustomsReference, value))
				{
					Validate(ExportDepotCustomsReferenceInfo);
				}
			}
		}

		ZString exportDepotCustomsReference;

		public ZPropertyInfo ExportDepotCustomsReferenceInfo => GetZPropertyInfo(nameof(ExportDepotCustomsReference));

		#endregion

		#region ImportDepotCustomsReference

		public ZString ImportDepotCustomsReference
		{
			get => importDepotCustomsReference;
			set
			{
				if (SetNonPersistentPropertyValue(ImportDepotCustomsReferenceInfo, ref importDepotCustomsReference, value))
				{
					Validate(ImportDepotCustomsReferenceInfo);
				}
			}
		}

		ZString importDepotCustomsReference;

		public ZPropertyInfo ImportDepotCustomsReferenceInfo => GetZPropertyInfo(nameof(ImportDepotCustomsReference));

		#endregion

		#region FCLAvailable

		public ZDateTime FCLAvailable
		{
			get => fclAvailable;
			set
			{
				if (SetNonPersistentPropertyValue(FCLAvailableInfo, ref fclAvailable, value))
				{
					Validate(FCLAvailableInfo);
				}
			}
		}

		ZDateTime fclAvailable;

		public ZPropertyInfo FCLAvailableInfo => GetZPropertyInfo(nameof(FCLAvailable));

		#endregion

		#region FCLHeldInTransitStaging

		public ZBool FCLHeldInTransitStaging
		{
			get => fclHeldInTransitStaging;
			set
			{
				if (SetNonPersistentPropertyValue(FCLHeldInTransitStagingInfo, ref fclHeldInTransitStaging, value))
				{
					Validate(FCLHeldInTransitStagingInfo);
				}
			}
		}

		ZBool fclHeldInTransitStaging;

		public ZPropertyInfo FCLHeldInTransitStagingInfo => GetZPropertyInfo(nameof(FCLHeldInTransitStaging));

		#endregion

		#region FCLOnBoardVessel

		public ZDateTime FCLOnBoardVessel
		{
			get => fclOnBoardVessel;
			set
			{
				if (SetNonPersistentPropertyValue(FCLOnBoardVesselInfo, ref fclOnBoardVessel, value))
				{
					Validate(FCLOnBoardVesselInfo);
				}
			}
		}

		ZDateTime fclOnBoardVessel;

		public ZPropertyInfo FCLOnBoardVesselInfo => GetZPropertyInfo(nameof(FCLOnBoardVessel));

		#endregion

		#region ArrivalCTOStorageCost

		public ZDecimal ArrivalCTOStorageCost
		{
			get => arrivalCTOStorageCost;
			set
			{
				if (SetNonPersistentPropertyValue(ArrivalCTOStorageCostInfo, ref arrivalCTOStorageCost, value))
				{
					Validate(ArrivalCTOStorageCostInfo);
				}
			}
		}

		ZDecimal arrivalCTOStorageCost;

		public ZPropertyInfo ArrivalCTOStorageCostInfo => GetZPropertyInfo(nameof(ArrivalCTOStorageCost));

		#endregion

		#region ArrivalCTOStorageStartDate

		public ZDateTime ArrivalCTOStorageStartDate
		{
			get => arrivalCTOStorageStartDate;
			set
			{
				if (SetNonPersistentPropertyValue(ArrivalCTOStorageStartDateInfo, ref arrivalCTOStorageStartDate, value))
				{
					Validate(ArrivalCTOStorageStartDateInfo);
				}
			}
		}

		ZDateTime arrivalCTOStorageStartDate;

		public ZPropertyInfo ArrivalCTOStorageStartDateInfo => GetZPropertyInfo(nameof(ArrivalCTOStorageStartDate));

		#endregion

		#region ArrivalCTOStorageDays

		public ZByte ArrivalCTOStorageDays
		{
			get => arrivalCTOStorageDays;
			set
			{
				if (SetNonPersistentPropertyValue(ArrivalCTOStorageDaysInfo, ref arrivalCTOStorageDays, value))
				{
					Validate(ArrivalCTOStorageDaysInfo);
				}
			}
		}

		ZByte arrivalCTOStorageDays;

		public ZPropertyInfo ArrivalCTOStorageDaysInfo => GetZPropertyInfo(nameof(ArrivalCTOStorageDays));

		#endregion

		#region FCLUnloadFromVessel

		public ZDateTime FCLUnloadFromVessel
		{
			get => fclUnloadFromVessel;
			set
			{
				if (SetNonPersistentPropertyValue(FCLUnloadFromVesselInfo, ref fclUnloadFromVessel, value))
				{
					Validate(FCLUnloadFromVesselInfo);
				}
			}
		}

		ZDateTime fclUnloadFromVessel;

		public ZPropertyInfo FCLUnloadFromVesselInfo => GetZPropertyInfo(nameof(FCLUnloadFromVessel));

		#endregion

		#region FCLWharfGateIn

		public ZDateTime FCLWharfGateIn
		{
			get => fclWharfGateIn;
			set
			{
				if (SetNonPersistentPropertyValue(FCLWharfGateInInfo, ref fclWharfGateIn, value))
				{
					Validate(FCLWharfGateInInfo);
				}
			}
		}

		ZDateTime fclWharfGateIn;

		public ZPropertyInfo FCLWharfGateInInfo => GetZPropertyInfo(nameof(FCLWharfGateIn));

		#endregion

		#region FCLWharfGateOut

		public ZDateTime FCLWharfGateOut
		{
			get => fclWharfGateOut;
			set
			{
				if (SetNonPersistentPropertyValue(FCLWharfGateOutInfo, ref fclWharfGateOut, value))
				{
					Validate(FCLWharfGateOutInfo);
				}
			}
		}

		ZDateTime fclWharfGateOut;

		public ZPropertyInfo FCLWharfGateOutInfo => GetZPropertyInfo(nameof(FCLWharfGateOut));

		#endregion

		#region GoodsValue

		public IMeasurement GoodsValue
		{
			get => goodsValue;
			set => goodsValue = SetChild(goodsValue, value);
		}

		IMeasurement goodsValue;

		#endregion

		#region ContainerStatus

		public ICodeDescription ContainerStatus
		{
			get => containerStatus;
			set => containerStatus = SetChild(containerStatus, value);
		}

		ICodeDescription containerStatus;

		#endregion

		#region TransportReference

		public ZString TransportReference
		{
			get => transportReference;
			set
			{
				if (SetNonPersistentPropertyValue(TransportReferenceInfo, ref transportReference, value))
				{
					Validate(TransportReferenceInfo);
				}
			}
		}

		ZString transportReference;

		public ZPropertyInfo TransportReferenceInfo => GetZPropertyInfo(nameof(TransportReference));

		#endregion

		#region ReleaseNumber

		public ZString ReleaseNumber
		{
			get => releaseNumber;
			set
			{
				if (SetNonPersistentPropertyValue(ReleaseNumberInfo, ref releaseNumber, value))
				{
					Validate(ReleaseNumberInfo);
				}
			}
		}

		ZString releaseNumber;

		public ZPropertyInfo ReleaseNumberInfo => GetZPropertyInfo(nameof(ReleaseNumber));

		#endregion

		#region RefrigGeneratorID

		public ZString RefrigGeneratorID
		{
			get => refrigGeneratorID;
			set
			{
				if (SetNonPersistentPropertyValue(RefrigGeneratorIDInfo, ref refrigGeneratorID, value))
				{
					Validate(RefrigGeneratorIDInfo);
				}
			}
		}

		ZString refrigGeneratorID;

		public ZPropertyInfo RefrigGeneratorIDInfo => GetZPropertyInfo(nameof(RefrigGeneratorID));

		#endregion

		#region PackCount

		public ZInt PackCount
		{
			get => packCount;
			set
			{
				if (SetNonPersistentPropertyValue(PackCountInfo, ref packCount, value))
				{
					Validate(PackCountInfo);
				}
			}
		}

		ZInt packCount;

		public ZPropertyInfo PackCountInfo => GetZPropertyInfo(nameof(PackCount));

		#endregion

		#region PackageType

		public ICodeDescription PackType
		{
			get => packType;
			set => packType = SetChild(packType, value);
		}

		ICodeDescription packType;

		#endregion

		#region WeightCapacity

		public IMeasurement WeightCapacity
		{
			get => weightCapacity;
			set => weightCapacity = SetChild(weightCapacity, value);
		}

		IMeasurement weightCapacity;

		#endregion

		#region VolumeCapacity

		public IMeasurement VolumeCapacity
		{
			get => volumeCapacity;
			set => volumeCapacity = SetChild(volumeCapacity, value);
		}

		IMeasurement volumeCapacity;

		#endregion

		#region TotalWidth

		public IMeasurement TotalWidth
		{
			get => totalWidth;
			set => totalWidth = SetChild(totalWidth, value);
		}

		IMeasurement totalWidth;

		#endregion

		#region TotalHeight

		public IMeasurement TotalHeight
		{
			get => totalHeight;
			set => totalHeight = SetChild(totalHeight, value);
		}

		IMeasurement totalHeight;

		#endregion

		#region TotalLength

		public IMeasurement TotalLength
		{
			get => totalLength;
			set => totalLength = SetChild(totalLength, value);
		}

		IMeasurement totalLength;

		#endregion

		#region SetPointTemperature

		public IMeasurement SetPointTemperature
		{
			get => setPointTemperature;
			set => setPointTemperature = SetChild(setPointTemperature, value);
		}

		IMeasurement setPointTemperature;

		#endregion

		#region PackingLines

		public IReadOnlyCollection<PackingLine> PackingLines
		{
			get => packingLines;
			set => packingLines = SetChildCollection(packingLines, value);
		}

		IReadOnlyCollection<PackingLine> packingLines;

		#endregion

		#region Numbers

		public IReadOnlyCollection<IReferenceNumber> Numbers
		{
			get => numbers;
			set => numbers = SetChildCollection(numbers, value);
		}

		IReadOnlyCollection<IReferenceNumber> numbers;

		#endregion

		#region AdditionalServices

		public IReadOnlyCollection<IAdditionalService> AdditionalServices
		{
			get => additionalServices;
			set => additionalServices = SetChildCollection(additionalServices, value);
		}

		IReadOnlyCollection<IAdditionalService> additionalServices;

		#endregion

		#region Milestones

		public IReadOnlyCollection<IMilestone> Milestones
		{
			get => milestones;
			set => milestones = SetChildCollection(milestones, value);
		}

		IReadOnlyCollection<IMilestone> milestones;

		#endregion

		#region TransportBookingPickupDeliveryInfos

		public IReadOnlyCollection<TransportBookingPickupDeliveryInfo> TransportBookingPickupDeliveryInfos
		{
			get => transportBookingPickupDeliveryInfos;
			set => transportBookingPickupDeliveryInfos = SetChildCollection(transportBookingPickupDeliveryInfos, value);
		}

		IReadOnlyCollection<TransportBookingPickupDeliveryInfo> transportBookingPickupDeliveryInfos;

		#endregion

		#region Implementation

		IContainerType IContainer.Type => Type;

		IMeasurement IContainer.SetTemperature => SetTemperature;
		IMeasurement IContainer.Humidity => Humidity;
		IMeasurement IContainer.AirVentFlow => AirVentFlow;
		IMeasurement IContainer.GoodsWeight => GoodsWeight;
		IMeasurement IContainer.TareWeight => TareWeight;
		IMeasurement IContainer.Dunnage => Dunnage;
		ICodeDescription IContainer.VerifiedMethod => VerifiedMethod;
		IAddress IContainer.VerifiedByAddress => VerifiedByAddress;
		IReadOnlyCollection<IPackingLine> IContainer.PackingLines => PackingLines;

		#endregion

		#region IsSealNumberMandatory

		public ZBool IsSealNumberMandatory
		{
			get => isSealNumberMandatory;
			set
			{
				if (SetNonPersistentPropertyValue(IsSealNumberMandatoryInfo, ref isSealNumberMandatory, value))
				{
					Validate(IsSealNumberMandatoryInfo);
				}
			}
		}

		ZBool isSealNumberMandatory;

		public ZPropertyInfo IsSealNumberMandatoryInfo => GetZPropertyInfo(nameof(IsSealNumberMandatory));

		#endregion

		#region CustomerLoadReference

		public ZString CustomerLoadReference
		{
			get => customerLoadReference;
			set
			{
				if (SetNonPersistentPropertyValue(CustomerLoadReferenceInfo, ref customerLoadReference, value))
				{
					Validate(CustomerLoadReferenceInfo);
				}
			}
		}
		ZString customerLoadReference;

		public ZPropertyInfo CustomerLoadReferenceInfo => GetZPropertyInfo(nameof(CustomerLoadReference));

		#endregion

		#region EPANStatus

		public ZString EPANStatus
		{
			get => espnStatus;
			set
			{
				if (SetNonPersistentPropertyValue(EPANStatusInfo, ref espnStatus, value))
				{
					Validate(EPANStatusInfo);
				}
			}
		}
		ZString espnStatus;

		public ZPropertyInfo EPANStatusInfo => GetZPropertyInfo(nameof(EPANStatus));

		#endregion

		#region RefrigerationType

		public ZString RefrigerationType
		{
			get => refrigerationType;
			set
			{
				if (SetNonPersistentPropertyValue(RefrigerationTypeInfo, ref refrigerationType, value))
				{
					Validate(RefrigerationTypeInfo);
				}
			}
		}
		ZString refrigerationType;

		public ZPropertyInfo RefrigerationTypeInfo => GetZPropertyInfo(nameof(RefrigerationType));
		#endregion

		#region HSCode

		public ZString HSCode
		{
			get => hsCode;
			set
			{
				if (SetNonPersistentPropertyValue(HSCodeInfo, ref hsCode, value))
				{
					Validate(HSCodeInfo);
				}
			}
		}
		ZString hsCode;

		public ZPropertyInfo HSCodeInfo => GetZPropertyInfo(nameof(HSCode));

		#endregion
	}
}
