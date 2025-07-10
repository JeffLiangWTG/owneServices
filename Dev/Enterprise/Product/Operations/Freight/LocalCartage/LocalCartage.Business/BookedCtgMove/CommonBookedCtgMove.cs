using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.DistanceCalculation.Business;
using Enterprise.Freight.DistanceCalculation.Integration;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.LocalCartage.Business
{
	[UniversalCopyWithExtendedEntities]
	public class CommonBookedCtgMove : AutoJobBookedCtgMove, Integration.ICommonBookedCtgMove, IUNDGDataItemProvider, IDistanceCalculationConsumer, IHaveServices
	{
		public CommonBookedCtgMove(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : AutoJobBookedCtgMove.Schema
		{
			public const string ShowRequestedPickup = "ShowRequestedPickup";
			public const string ShowRequestedDelivery = "ShowRequestedDelivery";
			public const string IsPickupAddressBooking = "IsPickupAddressBooking";
			public const string IsDeliveryAddressBooking = "IsDeliveryAddressBooking";
			public const string PostcodeDistance = "PostcodeDistance";
		}

		public override void Delete()
		{
			if (!IsDeleted)
			{
				var cartage = Cartage;
				var container = Container;

				CartageLegs.DeleteAll();

				base.Delete();

				if (container != null && cartage != null && !cartage.HasParent && !container.IsDeleted)
				{
					container.Delete();
				}
			}
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EW_F3_NKPackType = FreightPacksDataRegistry.Instance.OuterPackUnit.Value;
			EW_DimUnit = Env.Registry.OuterPacklinesMeasurementDefaultUnit;
			EW_VolumeUQ = Env.Registry.FreightVolumeUnit;
			EW_WeightUQ = Env.Registry.FreightWeightUnit;
			EW_DistanceUnit = DistanceCalculationRegistry.Instance.DefaultDistanceUnit.Value;
		}

		protected override NoteTypeCollection NoteTypesCore
		{
			get
			{
				NoteTypeCollection notes = base.NoteTypesCore;
				notes.Add(PredefinedNoteTypes.Instance.AutoRatingAuditLog);
				return notes;
			}
		}

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new CommonBookedCtgMoveFetchStrategy(this);
		}

		[RelatedBusinessObject("Cartage")]
		[List("Lookups.Cartages")]
		public override ZGuid EW_JJ
		{
			get { return base.EW_JJ; }
			set
			{
				if (base.EW_JJ == value)
				{
					base.EW_JJ = value;
				}
				else
				{
					if (!base.EW_JJ.IsEmpty)
					{
						BehaviorStrategy.CartageBookedMoveLinkBroken(this);
					}

					using (SuspendSettingHasChanges())
					{
						base.EW_JJ = value;
					}

					if (!EW_JJ.IsEmpty)
					{
						BehaviorStrategy.CartageBookedMoveLinkCreated(this);
					}
				}
			}
		}

		protected bool EW_JJ_ReadOnly
		{
			get { return true; }
		}

		public override ZGuid EW_JC_Container
		{
			get { return base.EW_JC_Container; }
			set
			{
				if (!value.IsEmpty && base.EW_JC_Container != value && Container != null) // do not change container unless it's removed
				{
					throw new InvalidOperationException("Container should not be changed unless it's removed.");
				}
				base.EW_JC_Container = value;
			}
		}

		[List("BindToLists.DimensionUnits")]
		public override ZString EW_DimUnit
		{
			[DebuggerStepThrough()]
			get { return base.EW_DimUnit; }
			set
			{
				base.EW_DimUnit = value;
				SetVolume();
			}
		}

		[List("Lookups.DropModes")]
		public override ZString EW_DropMode
		{
			get { return base.EW_DropMode; }
			set { base.EW_DropMode = value; }
		}

		public override ZInt EW_BookedPackCount
		{
			[DebuggerStepThrough()]
			get { return base.EW_BookedPackCount; }
			set
			{
				base.EW_BookedPackCount = value;
				SetVolume();
				if (Cartage != null)
				{
					Cartage.MarkAsNeedingValidation();
				}
			}
		}

		[List("BindToLists.OuterPackTypes")]
		public override ZString EW_F3_NKPackType
		{
			[DebuggerStepThrough()]
			get { return base.EW_F3_NKPackType; }
			[DebuggerStepThrough()]
			set { base.EW_F3_NKPackType = value; }
		}

		[MeasureUnit(AutoJobBookedCtgMove.Schema.EW_WeightUQ, MeasureUnitType.Weight)]
		public override ZDecimal EW_BookedWeight
		{
			get => base.EW_BookedWeight;
			set => base.EW_BookedWeight = value;
		}

		[List("BindToLists.WeightUnits")]
		public override ZString EW_WeightUQ
		{
			[DebuggerStepThrough()]
			get { return base.EW_WeightUQ; }
			[DebuggerStepThrough()]
			set { base.EW_WeightUQ = value; }
		}

		[MeasureUnit(AutoJobBookedCtgMove.Schema.EW_VolumeUQ, MeasureUnitType.Volume)]
		public override ZDecimal EW_BookedVolume
		{
			get => base.EW_BookedVolume;
			set => base.EW_BookedVolume = value;
		}

		[List("BindToLists.VolumeUnits")]
		public override ZString EW_VolumeUQ
		{
			[DebuggerStepThrough()]
			get { return base.EW_VolumeUQ; }
			set
			{
				base.EW_VolumeUQ = value;
				SetVolume();
			}
		}

		[BusinessObjectTestExclude()]
		[List("Lookups.CartageAddressList")]
		public override ZGuid EW_E2PickupAddressID
		{
			[DebuggerStepThrough()]
			get { return base.EW_E2PickupAddressID; }
			set
			{
				if (base.EW_E2PickupAddressID == value)
				{
					base.EW_E2PickupAddressID = value;
				}
				else
				{
					ZGuid previousValue = base.EW_E2PickupAddressID;

					if (Cartage == null || Cartage.DocAddresses.Contains(value))
					{
						base.EW_E2PickupAddressID = value;
					}
					else
					{
						JobDocAddress docAddress = Cartage.AddJobDocAddress(value);
						base.EW_E2PickupAddressID = docAddress != null ? docAddress.PK : ZGuid.Empty;
					}

					BehaviorStrategy.PickupAddressChanged(this, previousValue);
					EW_E2PickupAddressIDInfo.RefreshBinding();
				}
			}
		}

		[BusinessObjectTestExclude()]
		[List("Lookups.CartageAddressList")]
		public override ZGuid EW_E2WaitPointAddressID
		{
			[DebuggerStepThrough()]
			get { return base.EW_E2WaitPointAddressID; }
			set
			{
				if (base.EW_E2WaitPointAddressID == value)
				{
					base.EW_E2WaitPointAddressID = value;
				}
				else
				{
					ZGuid previousValue = base.EW_E2WaitPointAddressID;

					if (Cartage == null || Cartage.DocAddresses.Contains(value))
					{
						base.EW_E2WaitPointAddressID = value;
					}
					else
					{
						JobDocAddress docAddress = Cartage.AddJobDocAddress(value);
						base.EW_E2WaitPointAddressID = docAddress != null ? docAddress.PK : ZGuid.Empty;
					}

					BehaviorStrategy.WaitPointAddressChanged(this, previousValue);
					EW_E2WaitPointAddressIDInfo.RefreshBinding();
				}
			}
		}

		[BusinessObjectTestExclude()]
		[List("Lookups.CartageAddressList")]
		public override ZGuid EW_E2DeliveryAddressID
		{
			[DebuggerStepThrough()]
			get { return base.EW_E2DeliveryAddressID; }
			set
			{
				if (base.EW_E2DeliveryAddressID == value)
				{
					base.EW_E2DeliveryAddressID = value;
				}
				else
				{
					ZGuid previousValue = base.EW_E2DeliveryAddressID;

					if (Cartage == null || Cartage.DocAddresses.Contains(value))
					{
						base.EW_E2DeliveryAddressID = value;
					}
					else
					{
						JobDocAddress docAddress = Cartage.AddJobDocAddress(value);
						base.EW_E2DeliveryAddressID = docAddress != null ? docAddress.PK : ZGuid.Empty;
					}

					BehaviorStrategy.DeliveryAddressChanged(this, previousValue);
					EW_E2DeliveryAddressIDInfo.RefreshBinding();
				}
			}
		}

		[MeasureUnit(AutoJobBookedCtgMove.Schema.EW_DimUnit, MeasureUnitType.Length)]
		public override ZDecimal EW_BookedHeight
		{
			[DebuggerStepThrough()]
			get { return base.EW_BookedHeight; }
			set
			{
				base.EW_BookedHeight = value;
				SetVolume();
			}
		}

		[MeasureUnit(AutoJobBookedCtgMove.Schema.EW_DimUnit, MeasureUnitType.Length)]
		public override ZDecimal EW_BookedLength
		{
			[DebuggerStepThrough()]
			get { return base.EW_BookedLength; }
			set
			{
				base.EW_BookedLength = value;
				SetVolume();
			}
		}

		[MeasureUnit(AutoJobBookedCtgMove.Schema.EW_DimUnit, MeasureUnitType.Length)]
		public override ZDecimal EW_BookedWidth
		{
			[DebuggerStepThrough()]
			get { return base.EW_BookedWidth; }
			set
			{
				base.EW_BookedWidth = value;
				SetVolume();
			}
		}

		void SetVolume()
		{
			if (EW_BookedLength > 0 && EW_BookedWidth > 0 && EW_BookedHeight > 0 && EW_BookedPackCount > 0)
			{
				EW_BookedVolume = CalculatedVolume;
			}
		}

		ZDecimal CalculatedVolume
		{
			get { return FreightUtilities.CalculateVolume(EW_BookedVolume, EW_BookedPackCount, EW_BookedLength, EW_BookedWidth, EW_BookedHeight, EW_DimUnit, EW_VolumeUQ, JobBookedCtgMoveSchema.EW_BookedVolume.Scale); }
		}

		[MeasureUnit(AutoJobBookedCtgMove.Schema.EW_DistanceUnit, MeasureUnitType.Length)]
		public override ZDecimal EW_Distance
		{
			get => base.EW_Distance;
			set => base.EW_Distance = value;
		}

		[List("BindToLists.DistanceUnits")]
		public override ZString EW_DistanceUnit
		{
			get { return base.EW_DistanceUnit; }
			set
			{
				base.EW_DistanceUnit = value;
				PostcodeDistanceInfo.RefreshBinding();
			}
		}

		public CommonCartage Cartage
		{
			get { return Factory.Load<CommonCartage>(EW_JJ); }
		}

		[ChildEditableTestExclude()]
		[ChildEditable()]
		public CommonContainer Container
		{
			get
			{
				CommonContainer container = Factory.Load<CommonContainer>(EW_JC_Container);
				if (container != null)
				{
					var cartage = Cartage;
					if (cartage != null && !IsRegisteredEditableChildObject(container))
					{
						RegisterEditableChildObject(container);
					}
				}
				return container;
			}
		}

		public CodeDescriptionPairList ContainerModes
		{
			get { return CommonContainerLookups.GetContainerModesList(Cartage.JJ_ShippingTransportMode); }
		}

		[ChildEditableTestExclude()]
		[ChildEditable()]
		[UniversalCopyCollectionEntity(JobContainerLegsSchema.Constants.TableName, JobContainerLegsSchema.Constants.JU_EW)]
		public CommonCartageLegCollection CartageLegs
		{
			get
			{
				if (cartageLegs == null)
				{
					cartageLegs = new CommonCartageLegCollection(this);
					var cartage = Cartage;
					if (cartage != null && cartage.IsRoot)
					{
						RegisterEditableChildObject(cartageLegs);
					}
				}
				return cartageLegs;
			}
		}
		CommonCartageLegCollection cartageLegs;

		public CommonCartageLeg FirstCartageLeg
		{
			get
			{
				CommonCartageLegCollection legs = CartageLegs;
				legs.ApplySort(JobContainerLegsSchema.JU_DisplayOrder.Name, ListSortDirection.Ascending);
				return (legs.Count > 0) ? CartageLegs[0] : null;
			}
		}

		public CommonCartageLeg LastCartageLeg
		{
			get
			{
				CommonCartageLegCollection legs = CartageLegs;
				legs.ApplySort(JobContainerLegsSchema.JU_DisplayOrder.Name, ListSortDirection.Ascending);
				return (legs.Count > 0) ? CartageLegs[legs.Count - 1] : null;
			}
		}

		public JobDocAddress PickupFromDocAddress
		{
			get { return Factory.Load<JobDocAddress>(EW_E2PickupAddressID); }
		}

		public JobDocAddress WaitPointDocAddress
		{
			get { return Factory.Load<JobDocAddress>(EW_E2WaitPointAddressID); }
		}

		public JobDocAddress DeliverToDocAddress
		{
			get { return Factory.Load<JobDocAddress>(EW_E2DeliveryAddressID); }
		}

		public void ClearDeletedDocAddress(ZGuid deletedDocAddressPK)
		{
			if (!IsDeleted)
			{
				if (EW_E2PickupAddressID == deletedDocAddressPK)
				{
					EW_E2PickupAddressID = ZGuid.Empty;
				}

				if (EW_E2WaitPointAddressID == deletedDocAddressPK)
				{
					EW_E2WaitPointAddressID = ZGuid.Empty;
				}

				if (EW_E2DeliveryAddressID == deletedDocAddressPK)
				{
					EW_E2DeliveryAddressID = ZGuid.Empty;
				}
			}
		}

		[ChildEditable(true)]
		public UNDGDataItemCollection UNDGs
		{
			get
			{
				if (fUNDGs == null)
				{
					fUNDGs = new UNDGDataItemCollection(this);
					RegisterEditableChildObject(fUNDGs);
				}
				return fUNDGs;
			}
		}
		UNDGDataItemCollection fUNDGs;

		bool IUNDGDataItemProvider.NeedFetchHintForLoad => true;

		public CartageBindToLists BindToLists
		{
			get { return bindToLists ?? (bindToLists = new CartageBindToLists(Factory)); }
		}
		CartageBindToLists bindToLists;

		public ZBool IsLoose
		{
			get { return EW_JC_Container.IsEmpty; }
		}

		public ZBool IsContainerised
		{
			get { return !EW_JC_Container.IsEmpty && Container != null; }
		}

		public ZString RequestedAddressCode
		{
			get
			{
				ZString result = "";
				if (RequestedAddressType != DocAddressType.None)
				{
					result = CommonCartageAddressHelper.GetOrgTypeFromCartageDocAddressType(RequestedAddressType);
				}
				return result;
			}
		}

		public DocAddressType RequestedAddressType
		{
			get { return CommonCartageAddressHelper.GetRelevantDocAddressType(new List<DocAddressType> { PickupDocAddressType, WaitPointDocAddressType, DeliverToDocAddressType }); }
		}

		public ZBool ShowRequestedPickup
		{
			get { return IsContainerised || (Cartage != null && Cartage.IsExportOrOrigin); }
		}

		public ZPropertyInfo ShowRequestedPickupInfo
		{
			get { return GetZPropertyInfo(Schema.ShowRequestedPickup); }
		}

		public ZBool ShowRequestedDelivery
		{
			get { return IsContainerised || (Cartage != null && Cartage.IsImportOrDestination); }
		}

		public ZPropertyInfo ShowRequestedDeliveryInfo
		{
			get { return GetZPropertyInfo(Schema.ShowRequestedDelivery); }
		}

		public ZBool IsPickupAddressBooking
		{
			get { return PickupDocAddressType == RequestedAddressType; }
		}

		public ZPropertyInfo IsPickupAddressBookingInfo
		{
			get { return GetZPropertyInfo(Schema.IsPickupAddressBooking); }
		}

		public ZBool IsDeliveryAddressBooking
		{
			get { return !IsPickupAddressBooking; }
		}

		public ZPropertyInfo IsDeliveryAddressBookingInfo
		{
			get { return GetZPropertyInfo(Schema.IsDeliveryAddressBooking); }
		}

		public bool IsTransportingGoods
		{
			get
			{
				foreach (CommonCartageLeg leg in CartageLegs)
				{
					if (!leg.JU_IsEmptyContainer)
					{
						return true;
					}
				}
				return false;
			}
		}

		[ResourceStringData("BookedCtgMove|PostcodeDistance", ShortCaption = "P/C Distance", Caption = "Postcode Distance", FullDescription = "The Calculated Distance between the Pickup Address Postcode and Delivery Address Postcode.")]
		public ZDecimal PostcodeDistance
		{
			get
			{
				ZDecimal result = new ZDecimal(RefLatLongPostcode.CalculateDistance(PickupFromDocAddress, WaitPointDocAddress));
				if (EW_DistanceUnit != Constants.Length.Kilometres && Constants.Length.ContainsCode(EW_DistanceUnit))
				{
					result = Constants.Length.Convert(result, Constants.Length.Kilometres, EW_DistanceUnit);
				}

				return result;
			}
		}

		public ZPropertyInfo PostcodeDistanceInfo
		{
			get { return GetZPropertyInfo(Schema.PostcodeDistance); }
		}

		public ZDecimal TotalContainerWeight
		{
			get { return IsTransportingGoods ? Container.JC_GrossWeight : Container.JC_TareWeight; }
		}

		public ZDecimal TotalWeight
		{
			get { return Container != null ? TotalContainerWeight : EW_BookedWeight; }
		}

		public ZDecimal TotalVolume
		{
			get { return EW_BookedVolume; }
		}

		public DocAddressType PickupDocAddressType
		{
			get
			{
				var pickupFromAddress = PickupFromDocAddress;
				return pickupFromAddress != null ? pickupFromAddress.DocAddressType : DocAddressType.None;
			}
		}

		public DocAddressType WaitPointDocAddressType
		{
			get
			{
				var waitPointDocAddress = WaitPointDocAddress;
				return waitPointDocAddress != null ? waitPointDocAddress.DocAddressType : DocAddressType.None;
			}
		}

		public DocAddressType DeliverToDocAddressType
		{
			get
			{
				var deliverToDocAddress = DeliverToDocAddress;
				return deliverToDocAddress != null ? deliverToDocAddress.DocAddressType : DocAddressType.None;
			}
		}

		public ZString Identifier
		{
			get
			{
				ZString result = "";

				if (IsContainerised)
				{
					result = Container.JC_ContainerNum;
				}
				else
				{
					result = EW_BookedPackCount + " " + EW_F3_NKPackType + "/" +
							EW_BookedWeight + " " + EW_WeightUQ + "/" +
							EW_BookedVolume + " " + EW_VolumeUQ;
				}

				return result;
			}
		}

		public void DefaultAddresses()
		{
			DefaultingAddresses = true;

			try
			{
				var cartage = Cartage;
				var cartageType = cartage.CartageType;
				if (cartageType != null)
				{
					var moveType = IsContainerised ? cartageType.ContainerizedBooking : cartageType.LooseBooking;
					if (moveType != null)
					{
						var picAddress = CommonCartageAddressHelper.GetCartagePickupAddress(cartage, moveType);
						var waitAddress = CommonCartageAddressHelper.GetCartageWaitPointAddress(cartage, moveType);
						var dlvAddress = CommonCartageAddressHelper.GetCartageDeliveryAddress(cartage, moveType);

						if (picAddress != null)
						{
							EW_E2PickupAddressID = picAddress.PK;
						}

						if (waitAddress != null)
						{
							EW_E2WaitPointAddressID = waitAddress.PK;
						}

						if (dlvAddress != null)
						{
							EW_E2DeliveryAddressID = dlvAddress.PK;
						}
					}
				}
			}
			finally
			{
				DefaultingAddresses = false;
			}
		}

		internal bool DefaultingAddresses { get; private set; }

		public void CreateDefaultLegs()
		{
			var cartage = Cartage;
			var cartageType = cartage.CartageType;
			if (cartageType != null)
			{
				var moveTypes = IsContainerised ? cartageType.ContainerizedCartageLegTypes : cartageType.LooseCartageLegTypes;

				foreach (var moveType in moveTypes)
				{
					var leg = CartageLegs.AddNew();
					using (leg.SuspendSettingHasChanges())
					{
						var picAddress = CommonCartageAddressHelper.GetCartagePickupAddress(cartage, moveType);
						var waitAddress = CommonCartageAddressHelper.GetCartageWaitPointAddress(cartage, moveType);
						var dlvAddress = CommonCartageAddressHelper.GetCartageDeliveryAddress(cartage, moveType);

						if (picAddress != null)
						{
							leg.JU_E2PickupAddressID = picAddress.PK;
						}

						if (waitAddress != null)
						{
							leg.JU_E2WaitPointAddressID = waitAddress.PK;
						}

						if (dlvAddress != null)
						{
							leg.JU_E2DeliveryAddressID = dlvAddress.PK;
						}
					}
				}
			}
			else
			{
				CartageLegs.AddNew();
			}
		}

		public new CommonBookedCtgMoveLookups Lookups
		{
			get { return lookups ?? (lookups = (CommonBookedCtgMoveLookups)GetNewLookups()); }
		}
		CommonBookedCtgMoveLookups lookups;

		protected override JobBookedCtgMoveLookups GetNewLookups()
		{
			return new CommonBookedCtgMoveLookups(this);
		}

		public new CommonBookedCtgMoveValidation Validation
		{
			get { return (CommonBookedCtgMoveValidation)GetNewValidation(); }
		}

		protected override JobBookedCtgMoveValidation GetNewValidation()
		{
			return new CommonBookedCtgMoveValidation(this);
		}

		CommonBookedCtgMoveBehaviorStrategy BehaviorStrategy
		{
			get { return CommonCartageBehaviorStrategyProvider.GetCartageProvider(Factory).BookedCtgMoveBehaviorStrategy; }
		}

		SecurityCheckpoint IDistanceCalculationConsumer.Checkpoint
		{
			get { return Env.Security.RoadDistanceCalculationServiceLocalTransport; }
		}

		ZDecimal IDistanceCalculationConsumer.Distance
		{
			get { return EW_Distance; }
			set { EW_Distance = value; }
		}

		ZString IDistanceCalculationConsumer.DistanceUnit
		{
			get { return EW_DistanceUnit; }
			set { EW_DistanceUnit = value; }
		}

		DistanceCalculationConfiguration IDistanceCalculationConsumer.DistanceCalculationConfig
		{
			get { return DistanceCalculationHelper.GetConfigurationFromJobDocAddress(null, Cartage.LocalClient); }
		}

		DistanceCalculationAddress IDistanceCalculationConsumer.OriginAddress
		{
			get { return DistanceCalculationHelper.GetAddressFromAddress(PickupFromDocAddress); }
		}

		DistanceCalculationAddress IDistanceCalculationConsumer.DestinationAddress
		{
			get { return DistanceCalculationHelper.GetAddressFromAddress(WaitPointDocAddress); }
		}

		public void SetCalculatedDistance(INotifications notifications)
		{
			new FreightDistanceCalculator(this, notifications).SetCalculatedDistance();
		}

		public void SetDropModeToCartageDropModeFallbackIfEmpty()
		{
			if (Cartage != null && !Cartage.IsImportingData)
			{
				ZString dropMode;

				if (!(dropMode = ParentDropMode).IsEmpty || !(dropMode = AddressDropMode).IsEmpty || !(dropMode = CartageTypeDropMode).IsEmpty)
				{
					EW_DropMode = dropMode;
				}
			}
		}

		ZString ParentDropMode
		{
			get
			{
				ZString dropMode = Cartage != null ? Cartage.JJ_DropMode : ZString.Empty;
				return !dropMode.IsEmpty && Lookups.DropModes.ContainsCode(dropMode) ? dropMode : ZString.Empty;
			}
		}

		ZString AddressDropMode
		{
			get
			{
				var relevantDocAddressType = RequestedAddressType;
				JobDocAddress relevantDocAddress = null;

				if (PickupDocAddressType == relevantDocAddressType)
				{
					relevantDocAddress = PickupFromDocAddress;
				}
				else if (WaitPointDocAddressType == relevantDocAddressType)
				{
					relevantDocAddress = WaitPointDocAddress;
				}
				else if (DeliverToDocAddressType == relevantDocAddressType)
				{
					relevantDocAddress = DeliverToDocAddress;
				}

				return GetAddressDropMode(relevantDocAddress);
			}
		}

		public ZString GetAddressDropMode(JobDocAddress relevantDocAddress)
		{
			ZString result = "";

			if (relevantDocAddress != null && relevantDocAddress.Address != null)
			{
				if (Cartage != null && Cartage.IsAir)
				{
					result = relevantDocAddress.Address.OA_AIREquipmentNeeded;
				}
				else if (IsLoose)
				{
					result = relevantDocAddress.Address.OA_LCLEquipmentNeeded;
				}
				else
				{
					result = relevantDocAddress.Address.OA_FCLEquipmentNeeded;
				}
			}

			return result;
		}

		ZString CartageTypeDropMode
		{
			get
			{
				ZString result = "";
				var cartage = Cartage;
				if (cartage != null)
				{
					var cartageType = cartage.CartageType;
					if (cartageType != null)
					{
						if (IsLoose && cartageType.LooseBooking != null)
						{
							result = cartageType.LooseBooking.E4_EquipmentGroup;
						}
						else if (IsContainerised && cartageType.ContainerizedBooking != null)
						{
							result = cartageType.ContainerizedBooking.E4_EquipmentGroup;
						}
					}
				}
				return result;
			}
		}

		ZString IHaveServices.ContainerMode
		{
			get { return IsContainerised ? (Container != null ? Container.JC_ContainerMode : ZString.Empty) : ZString.Empty; }
		}

		IHaveServices[] IHaveServices.DependentServiceParents
		{
			get { return Array.Empty<IHaveServices>(); }
		}

		BusinessObject IHaveServices.ServiceParent
		{
			get { return this; }
		}

		[ChildEditable()]
		public JobServiceDependentCollection Services
		{
			get
			{
				if (services == null)
				{
					services = new CartageJobServiceDependentCollection(this, Factory);
					services.Load();
					RegisterEditableChildObject(services);
				}
				return services;
			}
		}
		CartageJobServiceDependentCollection services;

		ZString IHaveServices.TableCode
		{
			get { return JobBookedCtgMoveSchema.Constants.Prefix; }
		}

		ZString IHaveServices.TransportMode
		{
			get { return (Cartage != null) ? Cartage.JJ_ShippingTransportMode : (ZString)Constants.TransportModes.Sea; }
		}

		bool IHaveServices.NeedsServiceEvents
		{
			get { return false; }
		}

		void IHaveServices.JobServiceDeleted(ZGuid servicePK) { }

		public FreightMode FreightMode
		{
			get
			{
				if (Container != null)
				{
					return FreightMode.FRO; //FCL
				}
				else if (Cartage.IsAir)
				{
					return FreightMode.LSE; //Air
				}
				else
				{
					return FreightMode.LRO; //LTL (Loose)
				}
			}
		}

		IBranch IHaveServices.ServiceBranch => Cartage?.Branch;
	}
}
