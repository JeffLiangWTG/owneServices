using System.ComponentModel;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Registry.Business;

namespace Enterprise.Freight.Agency.Business
{
	public abstract class AgencyAllocationItem : NonPersistentBusinessObject
	{
		protected AgencyAllocationItem(BusinessObjectFactory factory)
			: base(factory)
		{ }

		#region Schema

		public abstract class Schema
		{
			public const string TEU = "TEU";
			public const string PowerPoints = "PowerPoints";
			public const string Tonnes = "Tonnes";
			public const string Volume = "Volume";
			public const string Area = "Area";

			public const string UsedTEU = "UsedTEU";
			public const string UsedGP_TEU = "UsedGP_TEU";
			public const string UsedReefer_TEU = "UsedReefer_TEU";
			public const string UsedPowerPoints = "UsedPowerPoints";
			public const string UsedTonnes = "UsedTonnes";
			public const string UsedVolume = "UsedVolume";
			public const string UsedArea = "UsedArea";

			public const string OverallocatedTEU = "OverallocatedTEU";
			public const string OverallocatedPowerPoints = "OverallocatedPowerPoints";
			public const string OverallocatedTonnes = "OverallocatedTonnes";
			public const string OverallocatedVolume = "OverallocatedVolume";
			public const string OverallocatedArea = "OverallocatedArea";

			public const string OverallocationPercent = "OverallocationPercent";
			public const string OverrideOverallocationPercent = "OverrideOverallocationPercent";

			public const string TableName = "DodgySoftware";
		}

		#endregion
	}

	[ProvideMetaDataProperty("DefaultNumberOfDecimals", MetaDataTypes.DecimalPlaces)]
	public abstract class AgencyAllocationItem<T> : AgencyAllocationItem, IDefaultNumberOfDecimalsSupporter
		where T : BusinessObject
	{
		protected AgencyAllocationItem(T bizObj, ZGuid principalPK) : this(bizObj, bizObj.Factory, principalPK) { }
		protected AgencyAllocationItem(BusinessObjectFactory factory) : this(null, factory, ZGuid.Empty) { }

		AgencyAllocationItem(T bizObj, BusinessObjectFactory factory, ZGuid principalPK)
			: base(factory)
		{
			this.principalPK = principalPK;
			this.bizObj = bizObj;
		}

		#region Properties

		[ReadOnlyMember(nameof(AllocationFieldsReadonly))]
		[ResourceStringData("NPBO:Enterprise.Freight.Agency.Business.AllocationAdjustmentDetails|Tonnes", ShortCaption = "Tonnes", Caption = "Allocated Tonnes", FullDescription = "The total weight in tonnes you have been allocated.")]
		public ZDecimal Tonnes
		{
			get { return Allocation.GetAspect(AllocationAspectTypes.Tonnes); }
			set
			{
				var roundedValue = this.GetRoundedValue(TonnesInfo, value);

				Allocation.SetAspect(AllocationAspectTypes.Tonnes, roundedValue);
				TonnesInfo.RefreshBinding();
				OverallocatedTonnesInfo.RefreshBinding();
				if (!IsValidationSuspended)
				{
					ValidateTonnes();
				}
			}
		}
		public ZPropertyInfo TonnesInfo
		{
			get { return GetZPropertyInfo(Schema.Tonnes); }
		}
		public void ValidateTonnes()
		{
			TonnesInfo.ClearAllNotifications();
			TypeValidation.CheckValidDecimal(TonnesInfo, 6, 0);

			ZDecimal tonnes = Allocation.GetAspect(AllocationAspectTypes.Tonnes);

			if (tonnes < 0)
			{
				TonnesInfo.AddError(YouCantHaveANegativeAllocation);
			}
			else if (tonnes == 0 && (Allocation.GetAspect(AllocationAspectTypes.TEU) != 0 || Allocation.GetAspect(AllocationAspectTypes.Volume) != 0))
			{
				TonnesInfo.AddWarning(Res.GetString("d8adc72c-25bf-491e-b186-2b85d1da668d", "Enter your weight limit to prevent over booking."));
			}

			if (ShouldValidateUsage)
			{
				if (Usage.Tonnes > OverallocatedTonnes)
				{
					TonnesInfo.AddError(Res.GetString("f5bd2d46-5393-4a12-b6e6-a306d56927ca", "{0:0.000} tonnes have already been booked.", Usage.Tonnes));
				}
				else if (Usage.Tonnes > tonnes)
				{
					TonnesInfo.AddWarning(Res.GetString("f5bd2d46-5393-4a12-b6e6-a306d56927ca", "{0:0.000} tonnes have already been booked.", Usage.Tonnes));
				}
			}
		}

		[ReadOnlyMember(nameof(AllocationFieldsReadonly))]
		[ResourceStringData("NPBO:Enterprise.Freight.Agency.Business.AllocationAdjustmentDetails|Volume", ShortCaption = "Volume", Caption = "Allocated Volume", FullDescription = "The total volume of space you have been allocated in cubic meters.\r\n\r\nContainerized shipments count towards the TEUs instead of volume.")]
		public ZDecimal Volume
		{
			get { return Allocation.GetAspect(AllocationAspectTypes.Volume); }
			set
			{
				var roundedValue = this.GetRoundedValue(VolumeInfo, value);

				Allocation.SetAspect(AllocationAspectTypes.Volume, roundedValue);
				VolumeInfo.RefreshBinding();
				OverallocatedVolumeInfo.RefreshBinding();
				if (!IsValidationSuspended)
				{
					ValidateVolume();
					ValidateTonnes();
				}
			}
		}
		public ZPropertyInfo VolumeInfo
		{
			get { return GetZPropertyInfo(Schema.Volume); }
		}
		public void ValidateVolume()
		{
			VolumeInfo.ClearAllNotifications();
			TypeValidation.CheckValidDecimal(VolumeInfo, 6, 0);

			ZDecimal volume = Allocation.GetAspect(AllocationAspectTypes.Volume);

			if (volume < 0)
			{
				VolumeInfo.AddError(YouCantHaveANegativeAllocation);
			}

			if (ShouldValidateUsage)
			{
				if (Usage.Volume > OverallocatedVolume)
				{
					VolumeInfo.AddError(Res.GetString("993d254b-f4f2-49cb-a9ac-2b916a12c4bb", "{0:0.000} M3 have already been booked.", Usage.Volume));
				}
				else if (Usage.Volume > volume)
				{
					VolumeInfo.AddWarning(Res.GetString("993d254b-f4f2-49cb-a9ac-2b916a12c4bb", "{0:0.000} M3 have already been booked.", Usage.Volume));
				}
			}
		}

		[DecimalPlaces(2)]
		[ReadOnlyMember(nameof(AllocationFieldsReadonly))]
		[ResourceStringData("NPBO:Enterprise.Freight.Agency.Business.AllocationAdjustmentDetails|TEU", ShortCaption = "TEUs", Caption = "Allocated TEUs", FullDescription = "The total number of TEUs you have been allocated.")]
		public ZDecimal TEU
		{
			get { return Allocation.GetAspect(AllocationAspectTypes.TEU); }
			set
			{
				Allocation.SetAspect(AllocationAspectTypes.TEU, value);
				TEUInfo.RefreshBinding();
				if (!IsValidationSuspended)
				{
					ValidateTEU();
					ValidateTonnes();
					ValidatePowerPoints();
				}
			}
		}
		public ZPropertyInfo TEUInfo
		{
			get { return GetZPropertyInfo(Schema.TEU); }
		}
		public void ValidateTEU()
		{
			TEUInfo.ClearAllNotifications();
			TypeValidation.CheckValidDecimal(TEUInfo, 6, 0);

			ZDecimal teu = Allocation.GetAspect(AllocationAspectTypes.TEU);

			if (teu < 0)
			{
				TEUInfo.AddError(YouCantHaveANegativeAllocation);
			}
			else if (teu == 0 && Allocation.GetAspect(AllocationAspectTypes.PowerPoints) > 0)
			{
				TEUInfo.AddError(Res.GetString("13d233c6-1d50-478c-bc69-8f3ac790ca9e", "If you specify power points then you also need to specify TEU."));
			}

			if (ShouldValidateUsage)
			{
				if (Usage.TEU > OverallocatedTEU)
				{
					TEUInfo.AddError(Res.GetString("582c541f-168b-4a62-a0ee-cc1e6526a35e", "{0:0.00} TEU have already been booked.", Usage.TEU));
				}
				else if (Usage.TEU > teu)
				{
					TEUInfo.AddWarning(Res.GetString("582c541f-168b-4a62-a0ee-cc1e6526a35e", "{0:0.00} TEU have already been booked.", Usage.TEU));
				}
			}
		}

		[DecimalPlaces(0)]
		[ReadOnlyMember(nameof(AllocationFieldsReadonly))]
		[ResourceStringData("NPBO:Enterprise.Freight.Agency.Business.AllocationAdjustmentDetails|PowerPoints", ShortCaption = "Power", MediumCaption = "Power Points", Caption = "Allocated Power Points", FullDescription = "The total number of power points you have been allocated.")]
		public ZDecimal PowerPoints
		{
			get { return Allocation.GetAspect(AllocationAspectTypes.PowerPoints); }
			set
			{
				Allocation.SetAspect(AllocationAspectTypes.PowerPoints, value);
				PowerPointsInfo.RefreshBinding();
				OverallocatedPowerPointsInfo.RefreshBinding();
				if (!IsValidationSuspended)
				{
					ValidatePowerPoints();
					ValidateTEU();
				}
			}
		}
		public ZPropertyInfo PowerPointsInfo
		{
			get { return GetZPropertyInfo(Schema.PowerPoints); }
		}
		public void ValidatePowerPoints()
		{
			PowerPointsInfo.ClearAllNotifications();
			TypeValidation.CheckValidDecimal(PowerPointsInfo, 6, 0);

			ZDecimal powerPoints = Allocation.GetAspect(AllocationAspectTypes.PowerPoints);

			if (powerPoints < 0)
			{
				PowerPointsInfo.AddError(YouCantHaveANegativeAllocation);
			}
			else if (powerPoints > 0 && Allocation.GetAspect(AllocationAspectTypes.TEU) == 0)
			{
				PowerPointsInfo.AddError(Res.GetString("13d233c6-1d50-478c-bc69-8f3ac790ca9e", "If you specify power points then you also need to specify TEU."));
			}

			if (ShouldValidateUsage)
			{
				if (Usage.PowerPoints > OverallocatedPowerPoints)
				{
					PowerPointsInfo.AddError(Res.GetString("89cad970-d99f-4557-b6d8-137453860c55", "{0:0} power points have already been booked.", Usage.PowerPoints));
				}
				else if (Usage.PowerPoints > powerPoints)
				{
					PowerPointsInfo.AddWarning(Res.GetString("89cad970-d99f-4557-b6d8-137453860c55", "{0:0} power points have already been booked.", Usage.PowerPoints));
				}
			}
		}

		[DecimalPlaces(3)]
		[ReadOnlyMember(nameof(AllocationFieldsReadonly))]
		[ResourceStringData("NPBO:Enterprise.Freight.Agency.Business.AllocationAdjustmentDetails|Area", ShortCaption = "Area", Caption = "Allocated Area", FullDescription = "The total floor space in square meters you have been allocated.\r\n\r\nOnly break-bulk and roll-on roll-off count towards area.")]
		public ZDecimal Area
		{
			get { return Allocation.GetAspect(AllocationAspectTypes.Area); }
			set
			{
				Allocation.SetAspect(AllocationAspectTypes.Area, value);
				AreaInfo.RefreshBinding();
				OverallocatedAreaInfo.RefreshBinding();

				if (!IsValidationSuspended)
				{
					ValidateArea();
				}
			}
		}
		public ZPropertyInfo AreaInfo
		{
			get { return GetZPropertyInfo(Schema.Area); }
		}
		public void ValidateArea()
		{
			AreaInfo.ClearAllNotifications();
			TypeValidation.CheckValidDecimal(AreaInfo, 6, 0);

			ZDecimal area = Allocation.GetAspect(AllocationAspectTypes.Area);

			if (area < 0)
			{
				AreaInfo.AddError(YouCantHaveANegativeAllocation);
			}

			if (ShouldValidateUsage)
			{
				if (Usage.Area > OverallocatedArea)
				{
					AreaInfo.AddError(Res.GetString("7d9f8703-7ea1-4927-bfcd-41f858722e7d", "{0:0.000} M2 have already been booked.", Usage.Area));
				}
				else if (Usage.Area > area)
				{
					AreaInfo.AddWarning(Res.GetString("7d9f8703-7ea1-4927-bfcd-41f858722e7d", "{0:0.000} M2 have already been booked.", Usage.Area));
				}
			}
		}

		public ZBool IsUsageEmpty
		{
			get { return Usage.IsEmpty; }
		}

		[ResourceStringData("NPBO:Enterprise.Freight.Agency.Business.AllocationAdjustmentDetails|UsedTonnes", Caption = "Used Tonnes", FullDescription = "How much of the allocated tonnage has been booked.")]
		public ZDecimal UsedTonnes
		{
			get { return this.GetRoundedValue(UsedTonnesInfo, Usage.Tonnes); }
		}
		public ZPropertyInfo UsedTonnesInfo
		{
			get { return GetZPropertyInfo(Schema.UsedTonnes); }
		}

		[ResourceStringData("NPBO:Enterprise.Freight.Agency.Business.AllocationAdjustmentDetails|UsedVolume", Caption = "Used Volume", FullDescription = "How much of the allocated volume has been booked.\r\n\r\nContainerized shipments count towards the TEUs instead of volume.")]
		public ZDecimal UsedVolume
		{
			get { return this.GetRoundedValue(UsedVolumeInfo, Usage.Volume); }
		}
		public ZPropertyInfo UsedVolumeInfo
		{
			get { return GetZPropertyInfo(Schema.UsedVolume); }
		}

		[DecimalPlaces(2)]
		[ResourceStringData("NPBO:Enterprise.Freight.Agency.Business.AllocationAdjustmentDetails|UsedTEU", Caption = "Used TEU", FullDescription = "How many of the allocated TEUs have been used.")]
		public ZDecimal UsedTEU
		{
			get { return Usage.TEU; }
		}
		public ZPropertyInfo UsedTEUInfo
		{
			get { return GetZPropertyInfo(Schema.UsedTEU); }
		}

		[DecimalPlaces(2)]
		[ResourceStringData("NPBO:Enterprise.Freight.Agency.Business.AllocationAdjustmentDetails|UsedGP_TEU", ShortCaption = "Used GP TEUs", Caption = "Used General Purpose TEUs", FullDescription = "How many of the allocated TEUs have been booked by non-reefer containers.")]
		public ZDecimal UsedGP_TEU
		{
			get { return Usage.GP_TEU; }
		}
		public ZPropertyInfo UsedGP_TEUInfo
		{
			get { return GetZPropertyInfo(Schema.UsedGP_TEU); }
		}

		[DecimalPlaces(2)]
		[ResourceStringData("NPBO:Enterprise.Freight.Agency.Business.AllocationAdjustmentDetails|UsedReefer_TEU", ShortCaption = "Used Reefer TEUs", Caption = "Used Refrigerated TEUs", FullDescription = "How many of the allocated TEUs have been booked by reefer containers.")]
		public ZDecimal UsedReefer_TEU
		{
			get { return Usage.Reefer_TEU; }
		}
		public ZPropertyInfo UsedReefer_TEUInfo
		{
			get { return GetZPropertyInfo(Schema.UsedReefer_TEU); }
		}

		[ResourceStringData("NPBO:Enterprise.Freight.Agency.Business.AllocationAdjustmentDetails|UsedPowerPoints", ShortCaption = "Used Power", Caption = "Used Power Points", FullDescription = "How many of the allocated power points have been booked.")]
		public ZInt UsedPowerPoints
		{
			get { return Usage.PowerPoints; }
		}
		public ZPropertyInfo UsedPowerPointsInfo
		{
			get { return GetZPropertyInfo(Schema.UsedPowerPoints); }
		}

		[DecimalPlaces(3)]
		[ResourceStringData("NPBO:Enterprise.Freight.Agency.Business.AllocationAdjustmentDetails|UsedArea", Caption = "Used Area", FullDescription = "How much of the allocated floor space has been booked.\r\n\r\nOnly break-bulk and roll-on roll-off count towards area.")]
		public ZDecimal UsedArea
		{
			get { return Usage.Area; }
		}
		public ZPropertyInfo UsedAreaInfo
		{
			get { return GetZPropertyInfo(Schema.UsedArea); }
		}

		[DecimalPlaces(2)]
		[ResourceStringData("NPBO:Enterprise.Freight.Agency.Business.AllocationAdjustmentDetails|OverallocatedTEU", ShortCaption = "Over. TEUs", Caption = "Over Allocation TEUs", FullDescription = "The upper limit of the TEUs that may be booked.")]
		public ZDecimal OverallocatedTEU
		{
			get { return Allocation.GetAspectOverallocation(AllocationAspectTypes.TEU); }
		}
		public ZPropertyInfo OverallocatedTEUInfo
		{
			get { return GetZPropertyInfo(Schema.OverallocatedTEU); }
		}

		[DecimalPlaces(0)]
		[ResourceStringData("NPBO:Enterprise.Freight.Agency.Business.AllocationAdjustmentDetails|OverallocatedPowerPoints", ShortCaption = "Over. Power", MediumCaption = "Over. Power Points", Caption = "Over Allocation Power Points", FullDescription = "The upper limit on the power points in tonnes that may be booked.")]
		public ZDecimal OverallocatedPowerPoints
		{
			get { return Allocation.GetAspectOverallocation(AllocationAspectTypes.PowerPoints); }
		}
		public ZPropertyInfo OverallocatedPowerPointsInfo
		{
			get { return GetZPropertyInfo(Schema.OverallocatedPowerPoints); }
		}

		[ResourceStringData("NPBO:Enterprise.Freight.Agency.Business.AllocationAdjustmentDetails|OverallocatedVolume", ShortCaption = "Over. Volume", Caption = "Over Allocation Volume", FullDescription = "The upper limit on the volume in cubic meters that may be booked.\r\n\r\nContainerized shipments count towards the TEUs instead of volume.")]
		public ZDecimal OverallocatedVolume
		{
			get { return this.GetRoundedValue(OverallocatedVolumeInfo, Allocation.GetAspectOverallocation(AllocationAspectTypes.Volume)); }
		}
		public ZPropertyInfo OverallocatedVolumeInfo
		{
			get { return GetZPropertyInfo(Schema.OverallocatedVolume); }
		}

		[ResourceStringData("NPBO:Enterprise.Freight.Agency.Business.AllocationAdjustmentDetails|OverallocatedTonnes", ShortCaption = "Over. Tonnes", Caption = "Over Allocation Tonnes", FullDescription = "The upper limit on the weight in tonnes that may be booked.")]
		public ZDecimal OverallocatedTonnes
		{
			get { return this.GetRoundedValue(OverallocatedTonnesInfo, Allocation.GetAspectOverallocation(AllocationAspectTypes.Tonnes)); }
		}
		public ZPropertyInfo OverallocatedTonnesInfo
		{
			get { return GetZPropertyInfo(Schema.OverallocatedTonnes); }
		}

		[DecimalPlaces(3)]
		[ResourceStringData("NPBO:Enterprise.Freight.Agency.Business.AllocationAdjustmentDetails|OverallocatedArea", ShortCaption = "Over. Area", Caption = "Over Allocation Area", FullDescription = "The upper limit on the floor space in square meters that may be booked.\r\n\r\nOnly break-bulk and roll-on roll-off count towards area.")]
		public ZDecimal OverallocatedArea
		{
			get { return Allocation.GetAspectOverallocation(AllocationAspectTypes.Area); }
		}
		public ZPropertyInfo OverallocatedAreaInfo
		{
			get { return GetZPropertyInfo(Schema.OverallocatedArea); }
		}

		[DecimalPlaces(0)]
		[ResourceStringData("NPBO:Enterprise.Freight.Agency.Business.AllocationAdjustmentDetails|OverallocationPercent", ShortCaption = "Over Alloc. %", Caption = "Over Allocation Percent", FullDescription = "The percentage by which the allocation may be exceeded.\r\nA value of 0% indicates that bookings may only be taken up to the allocated values.\r\nA value of 10% indicates that bookings may exceed the allocated values by up to 10%.")]
		public ZDecimal OverallocationPercent
		{
			get { return Allocation.E0_OverAllocationPercent; }
			set
			{
				Allocation.E0_OverAllocationPercent = value;
				OverallocationPercentInfo.RefreshBinding();
				RefreshOverallocationBindings();

				if (!IsValidationSuspended)
				{
					ValidateOverallocationPercent();
				}
			}
		}
		public ZPropertyInfo OverallocationPercentInfo
		{
			get { return GetZPropertyInfo(Schema.OverallocationPercent); }
		}
		public void ValidateOverallocationPercent()
		{
			OverallocationPercentInfo.ClearAllNotifications();

			if (OverallocationPercent >= 100m)
			{
				OverallocationPercentInfo.AddWarning(Res.GetString("3ab5049e-7186-42b0-85be-0e5fd4b0c026", "Values greater than 100% will allow users to take bookings of more than twice your allocation."));
			}
		}
		protected bool OverallocationPercent_ReadOnly
		{
			get { return AllocationFieldsReadonly || !OverrideOverallocationPercent; }
		}

		[ReadOnlyMember(nameof(AllocationFieldsReadonly))]
		[ResourceStringData("NPBO:Enterprise.Freight.Agency.Business.AllocationAdjustmentDetails|OverrideOverallocationPercent", ShortCaption = "Override", Caption = "Override Over Allocation Percent", FullDescription = "The \"Override Over Allocation Percent\" flag determines if the over allocation percent should come from the registry or is set separately for this allocation.\r\n\r\nIf checked, you can specify the over allocation percent separately for this allocation.\r\nIf unchecked, the over allocation percent will come from the registry.")]
		public ZBool OverrideOverallocationPercent
		{
			get { return !Allocation.E0_UseDefaultOverAllocation; }
			set
			{
				Allocation.E0_UseDefaultOverAllocation = !value;
				OverrideOverallocationPercentInfo.RefreshBinding();
				OverallocationPercentInfo.RefreshBinding();
				RefreshOverallocationBindings();
			}
		}
		public ZPropertyInfo OverrideOverallocationPercentInfo
		{
			get { return GetZPropertyInfo(Schema.OverrideOverallocationPercent); }
		}

		public ZGuid PrincipalPK
		{
			get { return principalPK; }
		}
		readonly ZGuid principalPK;

		[ResourceStringData("NPBO:Enterprise.Freight.Agency.Business.AllocationAdjustmentDetails|LoadedCargoWeight", ShortCaption = "Cargo Weight", Caption = "Actual Loaded Cargo Weight", FullDescription = "The total weight of the goods loaded on the vessel as reported by the wharf.")]
		public ZInt LoadedCargoWeight
		{
			get { return Allocation.E0_LoadedCargoWeight; }
			set
			{
				Allocation.E0_LoadedCargoWeight = value;
				Allocation.E0_LoadedCargoWeightInfo.RefreshBinding();
			}
		}
		public ZPropertyInfo LoadedCargoWeightInfo
		{
			get { return Allocation == null ? null : Allocation.E0_LoadedCargoWeightInfo; }
		}

		#endregion

		#region Implementation

		static string YouCantHaveANegativeAllocation
		{
			get { return Res.GetString("fe82ea15-4f6c-4454-973b-3ca354bb0fac", "You can't have a negative allocation."); }
		}

		protected bool AllocationFieldsReadonly
		{
			get { return !Env.Security.SailingScheduleAllocationEdit.IsAllowed; }
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();

			ValidateTEU();
			ValidatePowerPoints();
			ValidateTonnes();
			ValidateVolume();
			ValidateArea();

			ValidateOverallocationPercent();
		}

		protected SlotAllocation Allocation
		{
			get
			{
				if (allocation == null || allocation.IsDeleted)
				{
					allocation = SlotAllocations == null ? null : SlotAllocations.GetAllocation(principalPK);
					this.RefreshBinding();
				}
				return allocation;
			}
		}
		SlotAllocation allocation;

		AllocationUsage Usage
		{
			get { return GetUsage(); }
		}

		void RefreshOverallocationBindings()
		{
			OverallocatedTEUInfo.RefreshBinding();
			OverallocatedPowerPointsInfo.RefreshBinding();
			OverallocatedTonnesInfo.RefreshBinding();
			OverallocatedVolumeInfo.RefreshBinding();
			OverallocatedAreaInfo.RefreshBinding();
		}

		public void RefreshUsageBindings()
		{
			UsedTonnesInfo.RefreshBinding();
			UsedVolumeInfo.RefreshBinding();
			UsedTEUInfo.RefreshBinding();
			UsedGP_TEUInfo.RefreshBinding();
			UsedReefer_TEUInfo.RefreshBinding();
			UsedPowerPointsInfo.RefreshBinding();
			UsedAreaInfo.RefreshBinding();

			RefreshUsageBindingsCore();
		}

		protected virtual void RefreshUsageBindingsCore()
		{
		}

		public override void Delete()
		{
			if (SlotAllocations != null)
			{
				SlotAllocations.RemoveAllocation(PrincipalPK);
			}
			base.Delete();
		}

		public T BizObj
		{
			get { return (bizObj == null || bizObj.IsDeleted) ? null : bizObj; }
		}

		readonly T bizObj;

		protected abstract SlotAllocationDependentCollection SlotAllocations { get; }
		protected abstract bool ShouldValidateUsage { get; }
		protected abstract AllocationUsage GetUsage();

		#endregion

		#region IDefaultNumberOfDecimalsSupporter Members

		ZString IDefaultNumberOfDecimalsSupporter.TransportMode
		{
			get { return Core.Constants.TransportModes.Sea; }
		}

		public int GetDefaultNumberOfDecimals(PropertyDescriptor property)
		{
			return GetDefaultNumberOfDecimalsCore(property);
		}

		protected virtual int GetDefaultNumberOfDecimalsCore(PropertyDescriptor property)
		{
			return 0;
		}

		ZString IDefaultNumberOfDecimalsSupporter.GetUnitOfMeasure(PropertyDescriptor property)
		{
			var unitOfMeasure = ZString.Empty;
			var propertyName = DefaultNumberOfDecimals.GetBoundPropertyName(property.Name);

			switch (propertyName)
			{
				case Schema.Tonnes:
				case Schema.UsedTonnes:
				case Schema.OverallocatedTonnes:
					unitOfMeasure = Core.Constants.Weight.Tonnes;
					break;

				case Schema.Volume:
				case Schema.UsedVolume:
				case Schema.OverallocatedVolume:
					unitOfMeasure = Core.Constants.Volume.CubicMetres;
					break;

				default:
					break;
			}

			return unitOfMeasure;
		}

		ZDecimal IDefaultNumberOfDecimalsSupporter.GetRoundedValue(PropertyDescriptor property, ZDecimal value)
		{
			return DefaultNumberOfDecimalsSupporterHelperForShipping.GetRoundedValue(this, property, value);
		}

		void IDefaultNumberOfDecimalsSupporter.RoundMeasurePropertiesOnTransportModeChanged()
		{
			// not required as transport mode is always SEA
		}

		#endregion
	}
}
