using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	sealed class VGMMessagingContainer : DocDataObject, IContainer
	{
		public VGMMessagingContainer(Container container, IStmALogProvider logProvider)
			: base(container?.Identifier)
		{
			this.container = Argument.NotNull(container, nameof(container));
			this.logProvider = logProvider;
			SetChild(null, container);
		}

		readonly Container container;
		readonly IStmALogProvider logProvider;

		#region LogProvider

		public IStmALogParent LogParent => logProvider as IStmALogParent;

		#endregion

		#region Statement

		public ZString Statement
		{
			get => statement;
			set
			{
				if (SetNonPersistentPropertyValue(StatementInfo, ref statement, value))
				{
					Validate(StatementInfo);
				}
			}
		}

		ZString statement;

		public ZPropertyInfo StatementInfo => GetZPropertyInfo(nameof(Statement));

		#endregion

		#region IContainer Properties

		public ZString Number
		{
			get => container.Number;
			set => container.Number = value;
		}

		public ZPropertyInfo NumberInfo => container.NumberInfo;

		public IContainerType Type => container.Type;
		public ZInt ContainerCount
		{
			get => container.ContainerCount;
			set => container.ContainerCount = value;
		}
		public ZInt PackCount
		{
			get => container.PackCount;
			set => container.PackCount = value;
		}

		public ICodeDescription PackType => container.PackType;

		public ZBool IsEmpty
		{
			get => container.IsEmpty;
			set => container.IsEmpty = value;
		}
		public ZBool IsPartOf
		{
			get => container.IsPartOf;
			set => container.IsPartOf = value;
		}
		public ZBool IsShipperOwned
		{
			get => container.IsShipperOwned;
			set => container.IsShipperOwned = value;
		}
		public ZBool IsNonOperativeReefer
		{
			get => container.IsNonOperativeReefer;
			set => container.IsNonOperativeReefer = value;
		}
		public ZBool HasControlledAtmosphere
		{
			get => container.HasControlledAtmosphere;
			set => container.HasControlledAtmosphere = value;
		}
		public ZString TemperatureRecorderSerialNumber
		{
			get => container.TemperatureRecorderSerialNumber;
			set => container.TemperatureRecorderSerialNumber = value;
		}
		public ZBool Genset => container.HasControlledAtmosphere && container.Genset;
		public IMeasurement SetTemperature => container.SetTemperature;
		public IMeasurement Humidity => container.Humidity;
		public IMeasurement AirVentFlow => container.AirVentFlow;

		#region Seal

		public ZString Seal
		{
			get => container.Seal;
			set => container.Seal = value;
		}

		#endregion

		#region SealPartyType

		public ICodeDescription SealPartyType
		{
			get => container.SealPartyType;
			set => container.SealPartyType = value;
		}

		#endregion

		public ZString SecondSeal
		{
			get => container.SecondSeal;
			set => container.SecondSeal = value;
		}
		public ICodeDescription SecondSealPartyType => container.SecondSealPartyType;
		public ZString ThirdSeal
		{
			get => container.ThirdSeal;
			set => container.ThirdSeal = value;
		}
		public ICodeDescription ThirdSealPartyType => container.ThirdSealPartyType;
		public ZDateTime EmptyRequired
		{
			get => container.EmptyRequired;
			set => container.EmptyRequired = value;
		}
		public ZDateTime DepartureEstimatedPickup
		{
			get => container.DepartureEstimatedPickup;
			set => container.DepartureEstimatedPickup = value;
		}

		public ZDateTime VerifiedDate
		{
			get => container.VerifiedDate;
			set => container.VerifiedDate = value;
		}

		public Address VerifiedByAddress => container.VerifiedByAddress;
		public ICodeDescription VerifiedStatus => container.VerifiedStatus;
		public CodeDescription VerifiedMethod => container.VerifiedMethod;
		public ZBool IsDamaged => container.IsDamaged;
		public ZBool IsSealOk => container.IsSealOk;
		public ZDateTime LCLAvailable => container.LCLAvailable;
		public ZDateTime LCLStorageCommences => container.LCLStorageCommences;
		public ZDateTime LCLUnpack => container.LCLUnpack;
		public ZDateTime PackDate => container.PackDate;
		public ZDateTime ArrivalCartageAdvised => container.ArrivalCartageAdvised;
		public ZDateTime ArrivalCartageComplete => container.ArrivalCartageComplete;
		public ZDecimal ArrivalTruckWaitCost => container.ArrivalTruckWaitCost;
		public ZDateTime ArrivalTruckWaitTime => container.ArrivalTruckWaitTime;
		public ZString ArrivalCartageReference => container.ArrivalCartageReference;
		public ZDateTime ArrivalDeliveryRequiredBy => container.ArrivalDeliveryRequiredBy;
		public ZDateTime ArrivalEstimatedDelivery => container.ArrivalEstimatedDelivery;
		public ZBool ArrivalPickupByRail => container.ArrivalPickupByRail;
		public ZDateTime ArrivalSlotDateTime => container.ArrivalSlotDateTime;
		public ZString ArrivalSlotReference => container.ArrivalSlotReference;
		public ZDecimal ArrivalCarrierDetentionCost => container.ArrivalCarrierDetentionCost;
		public ZByte ArrivalCarrierDetentionDays => container.ArrivalCarrierDetentionDays;
		public ZString ContainerImportDORelease => container.ContainerImportDORelease;
		public ZDateTime ContainerParkEmptyPickupGateOut => container.ContainerParkEmptyPickupGateOut;
		public ZDateTime ContainerParkEmptyReturnGateIn => container.ContainerParkEmptyReturnGateIn;
		public ICodeDescription ContainerMode => container.ContainerMode;
		public ICodeDescription ContainerQuality => container.ContainerQuality;
		public ICodeDescription ContainerStatus => container.ContainerStatus;
		public ICodeDescription Commodity => container.Commodity;
		public ZString DeliveryMode => container.DeliveryMode;
		public ZShort DeliverySequence => container.DeliverySequence;
		public ZDateTime DepartureCartageAdvised => container.DepartureCartageAdvised;
		public ZDateTime DepartureCartageComplete => container.DepartureCartageComplete;
		public ZDecimal DepartureTruckWaitCost => container.DepartureTruckWaitCost;
		public ZDateTime DepartureTruckWaitTime => container.DepartureTruckWaitTime;
		public ZString DepartureCartageReference => container.DepartureCartageReference;
		public ZBool DepartureDeliveryByRail => container.DepartureDeliveryByRail;
		public ZDateTime DepartureSlotDateTime => container.DepartureSlotDateTime;
		public ZString DepartureSlotReference => container.DepartureSlotReference;
		public ZDateTime EmptyReturnedBy => container.EmptyReturnedBy;
		public ZDateTime EmptyReadyForReturn => container.EmptyReadyForReturn;
		public ZString EmptyReturnReference => container.EmptyReturnReference;
		public ZString ExportDepotCustomsReference => container.ExportDepotCustomsReference;
		public ZString ImportDepotCustomsReference => container.ImportDepotCustomsReference;
		public ZDateTime FCLAvailable => container.FCLAvailable;
		public ZBool FCLHeldInTransitStaging => container.FCLHeldInTransitStaging;
		public ZDateTime FCLOnBoardVessel => container.FCLOnBoardVessel;
		public ZDecimal ArrivalCTOStorageCost => container.ArrivalCTOStorageCost;
		public ZDateTime ArrivalCTOStorageStartDate => container.ArrivalCTOStorageStartDate;
		public ZByte ArrivalCTOStorageDays => container.ArrivalCTOStorageDays;
		public ZDateTime FCLUnloadFromVessel => container.FCLUnloadFromVessel;
		public ZDateTime FCLWharfGateIn => container.FCLWharfGateIn;
		public ZDateTime FCLWharfGateOut => container.FCLWharfGateOut;
		public IMeasurement GoodsValue => container.GoodsValue;
		public IAddress DepartureContainerYard => container.DepartureContainerYard;
		public ZString ReleaseNumber => container.ReleaseNumber;
		public ZString RefrigGeneratorID => container.RefrigGeneratorID;
		public IMeasurement GoodsWeight => container.GoodsWeight;
		public IMeasurement TareWeight => container.TareWeight;
		public IMeasurement Dunnage => container.Dunnage;
		public IMeasurement NetWeight => container.NetWeight;
		public IMeasurement Volume => container.Volume;
		public IMeasurement GrossWeight => container.GrossWeight;
		public IMeasurement WeightCapacity => container.WeightCapacity;
		public IMeasurement VolumeCapacity => container.VolumeCapacity;
		public IMeasurement TotalWidth => container.TotalWidth;
		public IMeasurement TotalHeight => container.TotalHeight;
		public IMeasurement TotalLength => container.TotalLength;
		public IMeasurement SetPointTemperature => container.SetPointTemperature;
		public IMeasurement OverhangFront => container.OverhangFront;
		public IMeasurement OverhangBack => container.OverhangBack;
		public IMeasurement OverhangLeft => container.OverhangLeft;
		public IMeasurement OverhangRight => container.OverhangRight;
		public IMeasurement OverhangHeight => container.OverhangHeight;
		public IReadOnlyCollection<IPackingLine> PackingLines => container.PackingLines;
		public IReadOnlyCollection<IReferenceNumber> Numbers => container.Numbers;

		public ZString CustomerLoadReference
		{
			get => container.CustomerLoadReference;
			set => container.CustomerLoadReference = value;
		}

		public IReadOnlyCollection<IAdditionalService> AdditionalServices => container.AdditionalServices;
		public IReadOnlyCollection<IMilestone> Milestones => container.Milestones;

		public VGMMessageStatus MessageStatus => messageStatus ?? (messageStatus = new VGMMessageStatus(logProvider));
		VGMMessageStatus messageStatus;

		#region ShiLianDan

		public ZString ShiLianDan
		{
			get => shiLianDan;
			set
			{
				if (SetNonPersistentPropertyValue(ShiLianDanInfo, ref shiLianDan, value))
				{
					Validate(ShiLianDanInfo);
				}
			}
		}

		ZString shiLianDan;

		public ZPropertyInfo ShiLianDanInfo => GetZPropertyInfo(nameof(ShiLianDan));

		#endregion

		#endregion

		#region Implementation

		ICodeDescription IContainer.VerifiedMethod => VerifiedMethod;
		IAddress IContainer.VerifiedByAddress => VerifiedByAddress;

		#endregion
	}
}
