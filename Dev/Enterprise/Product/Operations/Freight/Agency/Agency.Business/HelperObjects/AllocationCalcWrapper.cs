using System;
using System.ComponentModel;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Freight.Business;
using Enterprise.Registry.Business;

namespace Enterprise.Freight.Agency.Business
{
	[ProvideMetaDataProperty("DefaultNumberOfDecimals", MetaDataTypes.DecimalPlaces)]
	public sealed class AllocationCalcWrapper : NonPersistentBusinessObject, IObsoleteValidation, IDefaultNumberOfDecimalsSupporter
	{
		#region Schema

		public static class Schema
		{
			public const string AllocationMethod = "AllocationMethod";

			public const string Allocated_TEU = "Allocated_TEU";
			public const string Allocated_PowerPoints = "Allocated_PowerPoints";
			public const string Allocated_Tonnes = "Allocated_Tonnes";
			public const string Allocated_Volume = "Allocated_Volume";
			public const string Allocated_Area = "Allocated_Area";

			public const string Available_TEU = "Available_TEU";
			public const string Available_PowerPoints = "Available_PowerPoints";
			public const string Available_Tonnes = "Available_Tonnes";
			public const string Available_Volume = "Available_Volume";
			public const string Available_Area = "Available_Area";

			public const string Required_TEU = "Required_TEU";
			public const string Required_PowerPoints = "Required_PowerPoints";
			public const string Required_Tonnes = "Required_Tonnes";
			public const string Required_Volume = "Required_Volume";
			public const string Required_Area = "Required_Area";
		}

		#endregion

		public const int SaneOverallocationCutOffFactor = 4;

		public AllocationCalcWrapper(AgencyShipment shipment)
			: base(shipment.Factory)
		{
			this.Shipment = shipment;
			HookShipment(shipment);

			UpdateAllocatedValues();
			UpdateRequiredValues();
		}

		public AgencyShipment Shipment { get; private set; }

		#region Properties

		[ResourceStringData("NPBO:Enterprise.Freight.Agency.Business.AllocationCalcWrapper|AllocationMethod", Caption = "Allocation Method", FullDescription = "The allocation method in use for this sailing.")]
		public ZString AllocationMethod
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return allocationMethod; }
		}
		public ZPropertyInfo AllocationMethodInfo
		{
			get { return GetZPropertyInfo(Schema.AllocationMethod); }
		}

		[DecimalPlaces(2)]
		[ResourceStringData("NPBO:Enterprise.Freight.Agency.Business.AllocationCalcWrapper|Allocated_TEU", Caption = "Over Allocated TEUs", FullDescription = "The total number of allocated TEUs adjusted for overallocation.")]
		public ZDecimal Allocated_TEU
		{
			get { return GetAllocated(AllocationAspectTypes.TEU); }
		}
		public ZPropertyInfo Allocated_TEUInfo
		{
			get { return GetZPropertyInfo(Schema.Allocated_TEU); }
		}

		[DecimalPlaces(0)]
		[ResourceStringData("NPBO:Enterprise.Freight.Agency.Business.AllocationCalcWrapper|Allocated_PowerPoints", Caption = "Over Allocated Power Points", FullDescription = "The total number of allocated power points adjusted for overallocation.")]
		public ZDecimal Allocated_PowerPoints
		{
			get { return Math.Floor(GetAllocated(AllocationAspectTypes.PowerPoints)); }
		}
		public ZPropertyInfo Allocated_PowerPointsInfo
		{
			get { return GetZPropertyInfo(Schema.Allocated_PowerPoints); }
		}

		[ResourceStringData("NPBO:Enterprise.Freight.Agency.Business.AllocationCalcWrapper|Allocated_Tonnes", Caption = "Over Allocated Tonnes", FullDescription = "The total allocated weight in tonnes adjusted for overallocation.")]
		public ZDecimal Allocated_Tonnes
		{
			get { return this.GetRoundedValue(Allocated_TonnesInfo, GetAllocated(AllocationAspectTypes.Tonnes)); }
		}
		public ZPropertyInfo Allocated_TonnesInfo
		{
			get { return GetZPropertyInfo(Schema.Allocated_Tonnes); }
		}

		[ResourceStringData("NPBO:Enterprise.Freight.Agency.Business.AllocationCalcWrapper|Allocated_Volume", Caption = "Over Allocated Volume", FullDescription = "The total allocated volume in cubic meters adjusted for overallocation.\r\n\r\nContainerized shipments count towards the TEUs instead of volume.")]
		public ZDecimal Allocated_Volume
		{
			get { return this.GetRoundedValue(Allocated_VolumeInfo, GetAllocated(AllocationAspectTypes.Volume)); }
		}
		public ZPropertyInfo Allocated_VolumeInfo
		{
			get { return GetZPropertyInfo(Schema.Allocated_Volume); }
		}

		[DecimalPlaces(3)]
		[ResourceStringData("NPBO:Enterprise.Freight.Agency.Business.AllocationCalcWrapper|Allocated_Area", Caption = "Over Allocated Volume", FullDescription = "The total allocated floor space in square meters adjusted for overallocation.\r\n\r\nOnly break-bulk and roll-on-roll-off count towards area.")]
		public ZDecimal Allocated_Area
		{
			get { return GetAllocated(AllocationAspectTypes.Area); }
		}
		public ZPropertyInfo Allocated_AreaInfo
		{
			get { return GetZPropertyInfo(Schema.Allocated_Area); }
		}

		[DecimalPlaces(2)]
		[ResourceStringData("NPBO:Enterprise.Freight.Agency.Business.AllocationCalcWrapper|Available_TEU", Caption = "Available TEUs", FullDescription = "The total number of TEUs still available for use.")]
		public ZDecimal Available_TEU
		{
			get { return GetAvailable(AllocationAspectTypes.TEU); }
		}
		public ZPropertyInfo Available_TEUInfo
		{
			get { return GetZPropertyInfo(nameof(Available_TEU)); }
		}

		[DecimalPlaces(0)]
		[ResourceStringData("NPBO:Enterprise.Freight.Agency.Business.AllocationCalcWrapper|Available_PowerPoints", Caption = "Available Power Points", FullDescription = "The total number of power points still available for use.")]
		public ZDecimal Available_PowerPoints
		{
			get { return GetAvailable(AllocationAspectTypes.PowerPoints); }
		}
		public ZPropertyInfo Available_PowerPointsInfo
		{
			get { return GetZPropertyInfo(Schema.Available_PowerPoints); }
		}

		[ResourceStringData("NPBO:Enterprise.Freight.Agency.Business.AllocationCalcWrapper|Available_Tonnes", Caption = "Available Tonnes", FullDescription = "The total weight in tonnes still available for use.")]
		public ZDecimal Available_Tonnes
		{
			get { return this.GetRoundedValue(Available_TonnesInfo, GetAvailable(AllocationAspectTypes.Tonnes)); }
		}
		public ZPropertyInfo Available_TonnesInfo
		{
			get { return GetZPropertyInfo(Schema.Available_Tonnes); }
		}

		[ResourceStringData("NPBO:Enterprise.Freight.Agency.Business.AllocationCalcWrapper|Available_Volume", Caption = "Available Volume", FullDescription = "The total volume in cubic meters still available for use.\r\n\r\nContainerized shipments count towards the TEUs instead of volume.")]
		public ZDecimal Available_Volume
		{
			get { return this.GetRoundedValue(Available_VolumeInfo, GetAvailable(AllocationAspectTypes.Volume)); }
		}
		public ZPropertyInfo Available_VolumeInfo
		{
			get { return GetZPropertyInfo(Schema.Available_Volume); }
		}

		[DecimalPlaces(3)]
		[ResourceStringData("NPBO:Enterprise.Freight.Agency.Business.AllocationCalcWrapper|Available_Area", Caption = "Available Area", FullDescription = "The total floor space in square meters still available for use.\r\n\r\nOnly break-bulk and roll-on roll-off count towards area.")]
		public ZDecimal Available_Area
		{
			get { return GetAvailable(AllocationAspectTypes.Area); }
		}
		public ZPropertyInfo Available_AreaInfo
		{
			get { return GetZPropertyInfo(Schema.Available_Area); }
		}

		[DecimalPlaces(2)]
		[ResourceStringData("NPBO:Enterprise.Freight.Agency.Business.AllocationCalcWrapper|Required_TEU", Caption = "Required TEUs", FullDescription = "The number of available TEUs required to fit this shipment.")]
		public ZDecimal Required_TEU
		{
			get { return GetRequired(AllocationAspectTypes.TEU); }
		}
		public ZPropertyInfo Required_TEUInfo
		{
			get { return GetZPropertyInfo(Schema.Required_TEU); }
		}
		public void ValidateRequired_TEU()
		{
			Required_TEUInfo.ClearAllNotifications();
			ValidateAllocationAspect(Required_TEUInfo, AllocationAspectTypes.TEU, Res.GetString("c078fd72-a580-4c6c-832c-761cc0135e54", "TEU"));
		}

		[DecimalPlaces(0)]
		[ResourceStringData("NPBO:Enterprise.Freight.Agency.Business.AllocationCalcWrapper|Required_PowerPoints", Caption = "Required Power Points", FullDescription = "The number of available power points required to fit this shipment.")]
		public ZDecimal Required_PowerPoints
		{
			get { return GetRequired(AllocationAspectTypes.PowerPoints); }
		}
		public ZPropertyInfo Required_PowerPointsInfo
		{
			get { return GetZPropertyInfo(Schema.Required_PowerPoints); }
		}
		public void ValidateRequired_PowerPoints()
		{
			Required_PowerPointsInfo.ClearAllNotifications();
			ValidateAllocationAspect(Required_PowerPointsInfo, AllocationAspectTypes.PowerPoints, Res.GetString("45bf7af9-ff17-4614-8f90-48723f3db942", "Power Points"));
		}

		[ResourceStringData("NPBO:Enterprise.Freight.Agency.Business.AllocationCalcWrapper|Required_Tonnes", Caption = "Required Tonnes", FullDescription = "The available weight in tonnes required to fit this shipment.")]
		public ZDecimal Required_Tonnes
		{
			get { return this.GetRoundedValue(Required_TonnesInfo, GetRequired(AllocationAspectTypes.Tonnes)); }
		}
		public ZPropertyInfo Required_TonnesInfo
		{
			get { return GetZPropertyInfo(Schema.Required_Tonnes); }
		}
		public void ValidateRequired_Tonnes()
		{
			Required_TonnesInfo.ClearAllNotifications();
			ValidateAllocationAspect(Required_TonnesInfo, AllocationAspectTypes.Tonnes, Res.GetString("2b25384e-7ef8-4b6a-ade2-00ae1a64b7c5", "Tonnes"));
		}

		[ResourceStringData("NPBO:Enterprise.Freight.Agency.Business.AllocationCalcWrapper|Required_Volume", Caption = "Required Volume", FullDescription = "The available volume in cubic meters required to fit this shipment.\r\n\r\nContainerized shipments count towards the TEUs instead of volume.")]
		public ZDecimal Required_Volume
		{
			get { return this.GetRoundedValue(Required_VolumeInfo, GetRequired(AllocationAspectTypes.Volume)); }
		}
		public ZPropertyInfo Required_VolumeInfo
		{
			get { return GetZPropertyInfo(Schema.Required_Volume); }
		}
		public void ValidateRequired_Volume()
		{
			Required_VolumeInfo.ClearAllNotifications();
			ValidateAllocationAspect(Required_VolumeInfo, AllocationAspectTypes.Volume, Res.GetString("2d0ce516-0130-46e3-ab48-a2095c0f7ef9", "Volume"));
		}

		[DecimalPlaces(3)]
		[ResourceStringData("NPBO:Enterprise.Freight.Agency.Business.AllocationCalcWrapper|Required_Area", Caption = "Required Area", FullDescription = "The available floor space in square meters required to fit this shipment.\r\n\r\nOnly break-bulk and roll-on roll-off count towards area.")]
		public ZDecimal Required_Area
		{
			get { return GetRequired(AllocationAspectTypes.Area); }
		}
		public ZPropertyInfo Required_AreaInfo
		{
			get { return GetZPropertyInfo(Schema.Required_Area); }
		}
		public void ValidateRequired_Area()
		{
			Required_AreaInfo.ClearAllNotifications();
			ValidateAllocationAspect(Required_AreaInfo, AllocationAspectTypes.Area, Res.GetString("6f733fbd-a970-4422-9a3d-fa84f88b851f", "Area"));
		}

		#endregion

		#region Operations

		public void UpdateRequiredValues()
		{
			required = AllocationUsage.LoadFromShipment(Shipment);

			ValidateRequired();

			Required_TEUInfo.RefreshBinding();
			Required_PowerPointsInfo.RefreshBinding();
			Required_TonnesInfo.RefreshBinding();
			Required_VolumeInfo.RefreshBinding();
			Required_AreaInfo.RefreshBinding();
		}

		public void UpdateAllocatedValues()
		{
			allocated = null;
			used = null;
			allocationMethod = ZString.Empty;

			if (Shipment.Sailing != null)
			{
				allocationMethod = Shipment.Sailing.Origin.VoyageCountry.J0_AllocationMethod;
				AllocationUsageSet allocatedSet = Shipment.LoadAllocationUsageSet();

				if (allocatedSet != null)
				{
					allocated = allocatedSet.Allocation;
					used = allocatedSet.Used;
				}
			}

			AllocationMethodInfo.RefreshBinding();
			ValidateRequired_TEU();
			ValidateRequired_PowerPoints();
			ValidateRequired_Tonnes();
			ValidateRequired_Volume();
			ValidateRequired_Area();

			Allocated_TEUInfo.RefreshBinding();
			Allocated_PowerPointsInfo.RefreshBinding();
			Allocated_TonnesInfo.RefreshBinding();
			Allocated_VolumeInfo.RefreshBinding();
			Allocated_AreaInfo.RefreshBinding();

			Available_TEUInfo.RefreshBinding();
			Available_PowerPointsInfo.RefreshBinding();
			Available_TonnesInfo.RefreshBinding();
			Available_VolumeInfo.RefreshBinding();
			Available_AreaInfo.RefreshBinding();
		}

		#endregion

		#region Implementation

		protected override void RunPreSaveValidationCore()
		{
			UpdateAllocatedValues();
			UpdateRequiredValues();

			base.RunPreSaveValidationCore();
			ValidateRequired();
		}

		void ValidateRequired()
		{
			ValidateRequired_TEU();
			ValidateRequired_PowerPoints();
			ValidateRequired_Tonnes();
			ValidateRequired_Volume();
			ValidateRequired_Area();
		}

		ZDecimal GetAllocated(string code)
		{
			return allocated == null ? 0m : allocated.GetAspectOverallocation(code);
		}

		ZDecimal GetAvailable(string code)
		{
			if (allocated == null || used == null)
			{
				return ZDecimal.Zero;
			}
			else
			{
				return allocated.GetAspectOverallocation(code) - used.GetAspectValue(code);
			}
		}

		ZDecimal GetRequired(string code)
		{
			return required == null ? ZDecimal.Zero : required.GetAspectValue(code);
		}

		void ValidateAllocationAspect(ZPropertyInfo info, string code, string name)
		{
			if (Shipment.ShouldEnforceAllocations)
			{
				ZDecimal requiredValue = (ZDecimal)info.Value;
				ZDecimal totalRequiredValue = requiredValue + (used == null ? ZDecimal.Zero : used.GetAspectValue(code));
				ZDecimal allocatedValue = allocated == null ? 0m : allocated.GetAspect(code);
				ZDecimal availableValue = GetAvailable(code);

				if (requiredValue > 0)
				{
					if (allocatedValue == 0)
					{
						info.AddError(Res.GetString("ed899c61-811b-4184-8992-082adb187063", "The {0} allocation is not specified.", name));
					}
					else if (totalRequiredValue > allocatedValue * SaneOverallocationCutOffFactor)
					{
						ZInt proposed = (ZInt)Math.Ceiling((100m * totalRequiredValue / allocatedValue) - 100);
						info.AddError(Res.GetString("44595f98-3e89-48aa-83fe-0270723f71be", "This {0} allocation would require an over allocation percentage of {1}% which is really excessive. Please ensure the values entered for both the allocation and all the jobs are reasonable.", name, proposed));
					}
					else if (requiredValue > availableValue)
					{
						info.AddWarning(Res.GetString("57618472-ead5-4146-99a8-146bfdf4da85", "The {0} allocation is insufficient.", name));
					}
				}
			}
		}

		void HookShipment(AgencyShipment shipment)
		{
			shipment.JS_ActualVolumeInfo.ValueChanged += new EventHandler(UpdateRequiredValues);
			shipment.JS_UnitOfVolumeInfo.ValueChanged += new EventHandler(UpdateRequiredValues);
			shipment.JS_ActualWeightInfo.ValueChanged += new EventHandler(UpdateRequiredValues);
			shipment.JS_UnitOfWeightInfo.ValueChanged += new EventHandler(UpdateRequiredValues);
			shipment.JS_PackingModeInfo.ValueChanged += new EventHandler(ReHookContainers);
			shipment.JS_PackingModeInfo.ValueChanged += new EventHandler(UpdateRequiredValues);
			shipment.JS_JXInfo.ValueChanged += new EventHandler(UpdateAllocatedValues);

			HookContainers(shipment.ShippingContainers);
			HookPacklines(shipment.OuterPackLines);
		}

		void ReHookContainers(object sender, EventArgs e)
		{
			foreach (AgencyShipmentContainer container in Shipment.ShippingContainers)
			{
				UnHookContainer(container);
			}

			foreach (AgencyShipmentContainer container in Shipment.ShippingContainers)
			{
				HookContainer(container);
			}
		}

		void HookContainers(AgencyShipmentContainerDependentCollection containers)
		{
			containers.CountChanged += new CollectionCountChangedEventHandler(Container_CountChanged);

			foreach (AgencyShipmentContainer container in containers)
			{
				HookContainer(container);
			}
		}

		void HookContainer(AgencyShipmentContainer container)
		{
			if (Shipment.IsFCL)
			{
				container.JC_GrossWeightInfo.ValueChanged += UpdateRequiredValues;
				container.JC_VolumeCapacityInfo.ValueChanged += UpdateRequiredValues;
			}
			else
			{
				container.JC_ContainerCountInfo.ValueChanged += UpdateRequiredValues;
				container.JC_TotalUnitOfMeasureInfo.ValueChanged += UpdateRequiredValues;
				container.JC_TotalLengthInfo.ValueChanged += UpdateRequiredValues;
				container.JC_TotalWidthInfo.ValueChanged += UpdateRequiredValues;
			}
		}

		void UnHookContainer(AgencyShipmentContainer container)
		{
			container.JC_GrossWeightInfo.ValueChanged -= UpdateRequiredValues;
			container.JC_VolumeCapacityInfo.ValueChanged -= UpdateRequiredValues;

			container.JC_ContainerCountInfo.ValueChanged -= UpdateRequiredValues;
			container.JC_TotalUnitOfMeasureInfo.ValueChanged -= UpdateRequiredValues;
			container.JC_TotalLengthInfo.ValueChanged -= UpdateRequiredValues;
			container.JC_TotalWidthInfo.ValueChanged -= UpdateRequiredValues;
		}

		void HookPacklines(AgencyShipmentPackLineCollection packlines)
		{
			packlines.CountChanged += new CollectionCountChangedEventHandler(Packline_CountChanged);

			foreach (AgencyShipmentPackLine packline in packlines)
			{
				HookPackline(packline);
			}
		}

		void HookPackline(AgencyShipmentPackLine packline)
		{
			packline.JL_PackageCountInfo.ValueChanged += new EventHandler(UpdateRequiredValues);
			packline.JL_UnitOfDimensionInfo.ValueChanged += new EventHandler(UpdateRequiredValues);
			packline.JL_LengthInfo.ValueChanged += new EventHandler(UpdateRequiredValues);
			packline.JL_WidthInfo.ValueChanged += new EventHandler(UpdateRequiredValues);
		}

		void UnHookPackline(AgencyShipmentPackLine packline)
		{
			packline.JL_PackageCountInfo.ValueChanged -= new EventHandler(UpdateRequiredValues);
			packline.JL_UnitOfDimensionInfo.ValueChanged -= new EventHandler(UpdateRequiredValues);
			packline.JL_LengthInfo.ValueChanged -= new EventHandler(UpdateRequiredValues);
			packline.JL_WidthInfo.ValueChanged -= new EventHandler(UpdateRequiredValues);
		}

		void UpdateRequiredValues(object sender, EventArgs e)
		{
			UpdateRequiredValues();
		}

		void UpdateAllocatedValues(object sender, EventArgs e)
		{
			UpdateAllocatedValues();
		}

		void Container_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			if (e.ItemAdded)
			{
				HookContainer((AgencyShipmentContainer)e.BizObject);
			}
			else if (e.ItemRemoved)
			{
				UnHookContainer((AgencyShipmentContainer)e.BizObject);
			}

			UpdateRequiredValues();
		}

		void Packline_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			if (e.ItemAdded)
			{
				HookPackline((AgencyShipmentPackLine)e.BizObject);
			}
			else if (e.ItemRemoved)
			{
				UnHookPackline((AgencyShipmentPackLine)e.BizObject);
			}

			UpdateRequiredValues();
		}

		SlotAllocation allocated;
		AllocationUsage required;
		AllocationUsage used;
		ZString allocationMethod;

		#endregion

		#region IDefaultNumberOfDecimalsSupporter Members

		ZString IDefaultNumberOfDecimalsSupporter.TransportMode
		{
			get { return Core.Constants.TransportModes.Sea; }
		}

		public int GetDefaultNumberOfDecimals(PropertyDescriptor property)
		{
			return DefaultNumberOfDecimalsSupporterHelperForShipping.GetDefaultNumberOfDecimalsMetaDataProperty(this, property);
		}

		ZString IDefaultNumberOfDecimalsSupporter.GetUnitOfMeasure(PropertyDescriptor property)
		{
			var unitOfMeasure = ZString.Empty;
			var propertyName = DefaultNumberOfDecimals.GetBoundPropertyName(property.Name);

			switch (propertyName)
			{
				case Schema.Allocated_Tonnes:
				case Schema.Available_Tonnes:
				case Schema.Required_Tonnes:
					unitOfMeasure = Core.Constants.Weight.Tonnes;
					break;

				case Schema.Allocated_Volume:
				case Schema.Available_Volume:
				case Schema.Required_Volume:
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



