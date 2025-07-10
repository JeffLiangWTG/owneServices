using System;
using System.ComponentModel;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Agency.Business
{
	[ProvideMetaDataProperty("DefaultNumberOfDecimals", MetaDataTypes.DecimalPlaces)]
	public sealed class AllocationAdjustmentDetails : SecurityOverridenLogin, IDefaultNumberOfDecimalsSupporter
	{
		const string PercentLabelFormat = "  +{0}%";

		#region Schema

		public static class Schema
		{
			public const string Allocated_TEU = "Allocated_TEU";
			public const string Allocated_PowerPoints = "Allocated_PowerPoints";
			public const string Allocated_Tonnes = "Allocated_Tonnes";
			public const string Allocated_Volume = "Allocated_Volume";
			public const string Allocated_Area = "Allocated_Area";

			public const string TotalRequired_TEU = "TotalRequired_TEU";
			public const string TotalRequired_PowerPoints = "TotalRequired_PowerPoints";
			public const string TotalRequired_Tonnes = "TotalRequired_Tonnes";
			public const string TotalRequired_Volume = "TotalRequired_Volume";
			public const string TotalRequired_Area = "TotalRequired_Area";

			public const string OldOverAllocation_TEU = "OldOverAllocation_TEU";
			public const string OldOverAllocation_PowerPoints = "OldOverAllocation_PowerPoints";
			public const string OldOverAllocation_Tonnes = "OldOverAllocation_Tonnes";
			public const string OldOverAllocation_Volume = "OldOverAllocation_Volume";
			public const string OldOverAllocation_Area = "OldOverAllocation_Area";
			public const string OldOverAllocationPercent = "OldOverAllocationPercent";
			public const string OldPercentLabelText = "OldPercentLabelText";

			public const string NewOverAllocation_TEU = "NewOverAllocation_TEU";
			public const string NewOverAllocation_PowerPoints = "NewOverAllocation_PowerPoints";
			public const string NewOverAllocation_Tonnes = "NewOverAllocation_Tonnes";
			public const string NewOverAllocation_Volume = "NewOverAllocation_Volume";
			public const string NewOverAllocation_Area = "NewOverAllocation_Area";
			public const string NewOverAllocationPercent = "NewOverAllocationPercent";

			public const string NewPercentLabelText = "NewPercentLabelText";
		}

		#endregion

		public AllocationAdjustmentDetails(SlotAllocation allocation, AllocationUsage totalRequired)
			: base(allocation.Factory)
		{
			this.allocation = allocation;
			this.totalRequired = totalRequired;
			DefaultNewOverAllocationPercent();
		}

		#region Properties

		[DecimalPlaces(2)]
		[ResourceStringData("NPBO:Enterprise.Freight.Agency.Business.AllocationAdjustmentDetails|Allocated_TEU", ShortCaption = "TEUs", Caption = "Allocated TEUs", FullDescription = "The number of TEUs you have been allocated.")]
		public ZDecimal Allocated_TEU
		{
			get { return allocation.GetAspect(AllocationAspectTypes.TEU); }
		}
		public ZPropertyInfo Allocated_TEUInfo
		{
			get { return GetZPropertyInfo(Schema.Allocated_TEU); }
		}

		[DecimalPlaces(0)]
		[ResourceStringData("NPBO:Enterprise.Freight.Agency.Business.AllocationAdjustmentDetails|Allocated_PowerPoints", ShortCaption = "Power Points", Caption = "Allocated Power Points", FullDescription = "The number of power points you have been allocated.")]
		public ZDecimal Allocated_PowerPoints
		{
			get { return allocation.GetAspect(AllocationAspectTypes.PowerPoints); }
		}
		public ZPropertyInfo Allocated_PowerPointsInfo
		{
			get { return GetZPropertyInfo(Schema.Allocated_PowerPoints); }
		}

		[ResourceStringData("NPBO:Enterprise.Freight.Agency.Business.AllocationAdjustmentDetails|Allocated_Tonnes", ShortCaption = "Tonnes", Caption = "Allocated Tonnes", FullDescription = "The total weight you have been allocated.")]
		public ZDecimal Allocated_Tonnes
		{
			get { return this.GetRoundedValue(Allocated_TonnesInfo, allocation.GetAspect(AllocationAspectTypes.Tonnes)); }
		}
		public ZPropertyInfo Allocated_TonnesInfo
		{
			get { return GetZPropertyInfo(Schema.Allocated_Tonnes); }
		}

		[ResourceStringData("NPBO:Enterprise.Freight.Agency.Business.AllocationAdjustmentDetails|Allocated_Volume", ShortCaption = "Volume", Caption = "Allocated Volume", FullDescription = "The total volume of space you have been allocated in cubic meters.\r\n\r\nContainerized shipments count towards TEUs instead of volume.")]
		public ZDecimal Allocated_Volume
		{
			get { return this.GetRoundedValue(Allocated_VolumeInfo, allocation.GetAspect(AllocationAspectTypes.Volume)); }
		}
		public ZPropertyInfo Allocated_VolumeInfo
		{
			get { return GetZPropertyInfo(Schema.Allocated_Volume); }
		}

		[DecimalPlaces(3)]
		[ResourceStringData("NPBO:Enterprise.Freight.Agency.Business.AllocationAdjustmentDetails|Allocated_Area", ShortCaption = "Area", Caption = "Allocated Area", FullDescription = "The total floor space in square meters you have been allocated.\r\n\r\nOnly break-bulk and roll-on roll-off count towards area.")]
		public ZDecimal Allocated_Area
		{
			get { return allocation.GetAspect(AllocationAspectTypes.Area); }
		}
		public ZPropertyInfo Allocated_AreaInfo
		{
			get { return GetZPropertyInfo(Schema.Allocated_Area); }
		}

		[DecimalPlaces(2)]
		[ResourceStringData("NPBO:Enterprise.Freight.Agency.Business.AllocationAdjustmentDetails|TotalRequired_TEU", Caption = "Total Required TEUs", FullDescription = "The total number of TEUs from all shipments confirmed against this allocation.")]
		public ZDecimal TotalRequired_TEU
		{
			get { return totalRequired.TEU; }
		}
		public ZPropertyInfo TotalRequired_TEUInfo
		{
			get { return GetZPropertyInfo(Schema.TotalRequired_TEU); }
		}

		[ResourceStringData("NPBO:Enterprise.Freight.Agency.Business.AllocationAdjustmentDetails|TotalRequired_PowerPoints", Caption = "Total Required Power Points", FullDescription = "The total number of power points from all shipments confirmed against this allocation.")]
		public ZInt TotalRequired_PowerPoints
		{
			get { return totalRequired.PowerPoints; }
		}
		public ZPropertyInfo TotalRequired_PowerPointsInfo
		{
			get { return GetZPropertyInfo(Schema.TotalRequired_PowerPoints); }
		}

		[ResourceStringData("NPBO:Enterprise.Freight.Agency.Business.AllocationAdjustmentDetails|TotalRequired_Tonnes", Caption = "Total Required Tonnes", FullDescription = "The total weight in tonnes from all shipments confirmed against this allocation.")]
		public ZDecimal TotalRequired_Tonnes
		{
			get { return this.GetRoundedValue(TotalRequired_TonnesInfo, totalRequired.Tonnes); }
		}
		public ZPropertyInfo TotalRequired_TonnesInfo
		{
			get { return GetZPropertyInfo(Schema.TotalRequired_Tonnes); }
		}

		[ResourceStringData("NPBO:Enterprise.Freight.Agency.Business.AllocationAdjustmentDetails|TotalRequired_Volume", Caption = "Total Required Volume", FullDescription = "The total volume in cubic meters from all shipments confirmed against this allocation.\r\n\r\nContainerized shipments count towards TEUs instead of volume.")]
		public ZDecimal TotalRequired_Volume
		{
			get { return this.GetRoundedValue(TotalRequired_VolumeInfo, totalRequired.Volume); }
		}
		public ZPropertyInfo TotalRequired_VolumeInfo
		{
			get { return GetZPropertyInfo(Schema.TotalRequired_Volume); }
		}

		[DecimalPlaces(3)]
		public ZDecimal TotalRequired_Area
		{
			get { return totalRequired.Area; }
		}
		public ZPropertyInfo TotalRequired_AreaInfo
		{
			get { return GetZPropertyInfo(Schema.TotalRequired_Area); }
		}

		[DecimalPlaces(2)]
		[ResourceStringData("NPBO:Enterprise.Freight.Agency.Business.AllocationAdjustmentDetails|OldOverAllocation_TEU", Caption = "Over Allocation TEUs", FullDescription = "The current upper limit on TEUs for shipments booked against this allocation.")]
		public ZDecimal OldOverAllocation_TEU
		{
			get { return allocation.GetAspectOverallocation(AllocationAspectTypes.TEU); }
		}
		public ZPropertyInfo OldOverAllocation_TEUInfo
		{
			get { return GetZPropertyInfo(Schema.OldOverAllocation_TEU); }
		}

		[DecimalPlaces(0)]
		[ResourceStringData("NPBO:Enterprise.Freight.Agency.Business.AllocationAdjustmentDetails|OldOverAllocation_PowerPoints", Caption = "Over Allocation Power Points", FullDescription = "The current upper limit on power points for shipments booked against this allocation.")]
		public ZDecimal OldOverAllocation_PowerPoints
		{
			get { return Math.Floor(allocation.GetAspectOverallocation(AllocationAspectTypes.PowerPoints)); }
		}
		public ZPropertyInfo OldOverAllocation_PowerPointsInfo
		{
			get { return GetZPropertyInfo(Schema.OldOverAllocation_PowerPoints); }
		}

		[ResourceStringData("NPBO:Enterprise.Freight.Agency.Business.AllocationAdjustmentDetails|OldOverAllocation_Tonnes", Caption = "Over Allocation Tonnes", FullDescription = "The current upper limit on the weight in tonnes for shipments booked against this allocation.")]
		public ZDecimal OldOverAllocation_Tonnes
		{
			get { return this.GetRoundedValue(OldOverAllocation_TonnesInfo, allocation.GetAspectOverallocation(AllocationAspectTypes.Tonnes)); }
		}
		public ZPropertyInfo OldOverAllocation_TonnesInfo
		{
			get { return GetZPropertyInfo(Schema.OldOverAllocation_Tonnes); }
		}

		[ResourceStringData("NPBO:Enterprise.Freight.Agency.Business.AllocationAdjustmentDetails|OldOverAllocation_Volume", Caption = "Over Allocation Volume", FullDescription = "The current upper limit on the volume in cubic meters for shipments booked against this allocation.\r\n\r\nContainerized shipments count towards TEUs instead of volume.")]
		public ZDecimal OldOverAllocation_Volume
		{
			get { return this.GetRoundedValue(OldOverAllocation_VolumeInfo, allocation.GetAspectOverallocation(AllocationAspectTypes.Volume)); }
		}
		public ZPropertyInfo OldOverAllocation_VolumeInfo
		{
			get { return GetZPropertyInfo(Schema.OldOverAllocation_Volume); }
		}

		[DecimalPlaces(3)]
		[ResourceStringData("NPBO:Enterprise.Freight.Agency.Business.AllocationAdjustmentDetails|OldOverAllocation_Area", Caption = "Over Allocation Area", FullDescription = "The current upper limit on the floor space in square meters for shipments booked against this allocation.\r\n\r\nOnly break-bulk and roll-on roll-off count towards area.")]
		public ZDecimal OldOverAllocation_Area
		{
			get { return allocation.GetAspectOverallocation(AllocationAspectTypes.Area); }
		}
		public ZPropertyInfo OldOverAllocation_AreaInfo
		{
			get { return GetZPropertyInfo(Schema.OldOverAllocation_Area); }
		}

		[DecimalPlaces(0)]
		public ZDecimal OldOverAllocationPercent
		{
			get { return allocation.E0_OverAllocationPercent; }
		}
		public ZPropertyInfo OldOverAllocationPercentInfo
		{
			get { return GetZPropertyInfo(Schema.OldOverAllocationPercent); }
		}

		public ZString OldPercentLabelText
		{
			get { return ZString.Format(PercentLabelFormat, OldOverAllocationPercent); }
		}
		public ZPropertyInfo OldPercentLabelTextInfo
		{
			get { return GetZPropertyInfo(Schema.OldPercentLabelText); }
		}

		[DecimalPlaces(2)]
		[ResourceStringData("NPBO:Enterprise.Freight.Agency.Business.AllocationAdjustmentDetails|NewOverAllocation_TEU", Caption = "New Over Allocation TEUs", FullDescription = "What the new upper limit on TEUs for shipments booked against this allocation will be if the proposed over allocation percent is accepted.")]
		public ZDecimal NewOverAllocation_TEU
		{
			get { return CalculateNewOverAllocation(AllocationAspectTypes.TEU); }
		}
		public ZPropertyInfo NewOverAllocation_TEUInfo
		{
			get { return GetZPropertyInfo(Schema.NewOverAllocation_TEU); }
		}

		[DecimalPlaces(0)]
		[ResourceStringData("NPBO:Enterprise.Freight.Agency.Business.AllocationAdjustmentDetails|NewOverAllocation_PowerPoints", Caption = "New Over Allocation Power Points", FullDescription = "What the new upper limit on power points for shipments booked against this allocation will be if the proposed over allocation percent is accepted.")]
		public ZDecimal NewOverAllocation_PowerPoints
		{
			get { return decimal.Floor(CalculateNewOverAllocation(AllocationAspectTypes.PowerPoints)); }
		}
		public ZPropertyInfo NewOverAllocation_PowerPointsInfo
		{
			get { return GetZPropertyInfo(Schema.NewOverAllocation_PowerPoints); }
		}

		[ResourceStringData("NPBO:Enterprise.Freight.Agency.Business.AllocationAdjustmentDetails|NewOverAllocation_Tonnes", Caption = "Over Allocation Tonnes", FullDescription = "What the new upper limit on tonnage for shipments booked against this allocation will be if the proposed over allocation percent is accepted.")]
		public ZDecimal NewOverAllocation_Tonnes
		{
			get { return this.GetRoundedValue(NewOverAllocation_TonnesInfo, CalculateNewOverAllocation(AllocationAspectTypes.Tonnes)); }
		}
		public ZPropertyInfo NewOverAllocation_TonnesInfo
		{
			get { return GetZPropertyInfo(Schema.NewOverAllocation_Tonnes); }
		}

		[ResourceStringData("NPBO:Enterprise.Freight.Agency.Business.AllocationAdjustmentDetails|NewOverAllocation_Volume", Caption = "New Over Allocation Volume", FullDescription = "What the new upper limit on volume in cubic meters for shipments booked against this allocation will be if the proposed over allocation percent is accepted.\r\n\r\nContainerized shipments count towards TEUs instead of volume.")]
		public ZDecimal NewOverAllocation_Volume
		{
			get { return this.GetRoundedValue(NewOverAllocation_VolumeInfo, CalculateNewOverAllocation(AllocationAspectTypes.Volume)); }
		}
		public ZPropertyInfo NewOverAllocation_VolumeInfo
		{
			get { return GetZPropertyInfo(Schema.NewOverAllocation_Volume); }
		}

		[DecimalPlaces(3)]
		[ResourceStringData("NPBO:Enterprise.Freight.Agency.Business.AllocationAdjustmentDetails|NewOverAllocation_Area", Caption = "Over Allocation Area", FullDescription = "What the new upper limit on floor space in square meters for shipments booked against this allocation will be if the proposed over allocation percent is accepted.\r\n\r\nOnly break-bulk and roll-on roll-off count towards area.")]
		public ZDecimal NewOverAllocation_Area
		{
			get { return CalculateNewOverAllocation(AllocationAspectTypes.Area); }
		}
		public ZPropertyInfo NewOverAllocation_AreaInfo
		{
			get { return GetZPropertyInfo(Schema.NewOverAllocation_Area); }
		}

		[DecimalPlaces(0)]
		public ZDecimal NewOverAllocationPercent
		{
			get { return newOverAllocationPercent; }
		}
		public ZPropertyInfo NewOverAllocationPercentInfo
		{
			get { return GetZPropertyInfo(Schema.NewOverAllocationPercent); }
		}

		public ZString NewPercentLabelText
		{
			get { return ZString.Format(PercentLabelFormat, NewOverAllocationPercent); }
		}
		public ZPropertyInfo NewPercentLabelTextInfo
		{
			get { return GetZPropertyInfo(Schema.NewPercentLabelText); }
		}

		#endregion

		#region AcceptNewOverAllocation

		public void AcceptNewOverAllocation()
		{
			GlbStaff authorizer = Factory.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.GS_LoginName, Login));
			StmALog log = allocation.Parent.Voyage.Logs.AddNew(Events.Authorised, ZString.Format("Increased over allocation from +{0}% to +{1}% ({2})", OldOverAllocationPercent, NewOverAllocationPercent, allocation.HumanReadableName));

			if (authorizer != null)
			{
				log.SL_GS_NKUser = authorizer.GS_Code;
			}

			allocation.E0_UseDefaultOverAllocation = false;
			allocation.E0_OverAllocationPercent = NewOverAllocationPercent;
		}

		#endregion

		#region Implementation

		ZDecimal CalculateNewOverAllocation(string code)
		{
			return allocation.GetAspect(code) * (1 + NewOverAllocationPercent / 100m);
		}

		ZDecimal CalculateRequiredPercentage(string code, ZDecimal required)
		{
			ZDecimal aspect = allocation.GetAspect(code);
			return (aspect == 0) ? 0 : ((100m * required / aspect) - 100);
		}

		void DefaultNewOverAllocationPercent()
		{
			decimal[] requiredPercents = new decimal[]
			{
				CalculateRequiredPercentage(AllocationAspectTypes.TEU, totalRequired.TEU),
				CalculateRequiredPercentage(AllocationAspectTypes.Tonnes, totalRequired.Tonnes),
				CalculateRequiredPercentage(AllocationAspectTypes.Volume, totalRequired.Volume),
				CalculateRequiredPercentage(AllocationAspectTypes.PowerPoints, (ZDecimal)totalRequired.PowerPoints),
				CalculateRequiredPercentage(AllocationAspectTypes.Area, totalRequired.Area),
			};

			decimal result = 0m;

			foreach (decimal requirement in requiredPercents)
			{
				if (requirement > result)
				{
					result = requirement;
				}
			}

			newOverAllocationPercent = decimal.Ceiling(result);
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			if (Env.Security.SailingScheduleAllocationEdit.IsAllowed)
			{
				Login = GlbStaff.CurrentUser.GS_LoginName;
			}
		}

		ZDecimal newOverAllocationPercent;
		readonly SlotAllocation allocation;
		readonly AllocationUsage totalRequired;

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
				case Schema.TotalRequired_Tonnes:
				case Schema.OldOverAllocation_Tonnes:
				case Schema.NewOverAllocation_Tonnes:
					unitOfMeasure = Core.Constants.Weight.Tonnes;
					break;

				case Schema.Allocated_Volume:
				case Schema.TotalRequired_Volume:
				case Schema.OldOverAllocation_Volume:
				case Schema.NewOverAllocation_Volume:
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
