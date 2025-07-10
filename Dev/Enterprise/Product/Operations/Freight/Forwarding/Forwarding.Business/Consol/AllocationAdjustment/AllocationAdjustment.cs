using System.ComponentModel;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Business
{
	[ProvideMetaDataProperty("DefaultNumberOfDecimals", MetaDataTypes.DecimalPlaces)]
	public class AllocationAdjustment : NonPersistentBusinessObject, IObsoleteValidation, IDefaultNumberOfDecimalsSupporter
	{
		#region Schema

		public abstract class Schema
		{
			public const string Weight = "Weight";
			public const string WeightUnit = "WeightUnit";
			public const string Volume = "Volume";
			public const string VolumeUnit = "VolumeUnit";
			public const string Chargeable = "Chargeable";
			public const string ChargeableUnit = "ChargeableUnit";

			public const string AllocatedWeight = "AllocatedWeight";
			public const string AllocatedWeightUnit = "AllocatedWeightUnit";
			public const string AllocatedVolume = "AllocatedVolume";
			public const string AllocatedVolumeUnit = "AllocatedVolumeUnit";
			public const string AllocatedChargeable = "AllocatedChargeable";
			public const string AllocatedChargeableUnit = "AllocatedChargeableUnit";

			public const string NewAllocatedWeight = "NewAllocatedWeight";
			public const string NewAllocatedVolume = "NewAllocatedVolume";
			public const string NewAllocatedChargeable = "NewAllocatedChargeable";
		}

		#endregion

		public AllocationAdjustment(ForwardingConsol consol)
		{
			Argument.NotNull(consol, "consol");
			Consol = consol;

			CalculateNewAllocationValues();
		}

		public readonly ForwardingConsol Consol;

		#region Properties

		public ZString UniqueConsignRef
		{
			get { return Consol.JK_UniqueConsignRef; }
		}

		#region Actual

		public ZDecimal Weight
		{
			get { return this.GetRoundedValue(WeightInfo, Consol.JK_TotalShipmentWeight); }
		}

		public ZPropertyInfo WeightInfo
		{
			get { return GetZPropertyInfo(Schema.Weight); }
		}

		public ZString WeightUnit
		{
			get { return Consol.JK_TotalShipmentWeightUnit; }
		}

		public ZDecimal Volume
		{
			get { return this.GetRoundedValue(VolumeInfo, Consol.JK_TotalShipmentVolume); }
		}

		public ZPropertyInfo VolumeInfo
		{
			get { return GetZPropertyInfo(Schema.Volume); }
		}

		public ZString VolumeUnit
		{
			get { return Consol.JK_TotalShipmentVolumeUnit; }
		}

		public ZDecimal Chargeable
		{
			get { return this.GetRoundedValue(ChargeableInfo, Consol.JK_TotalShipmentChargeable); }
		}

		public ZPropertyInfo ChargeableInfo
		{
			get { return GetZPropertyInfo(Schema.Chargeable); }
		}

		public ZString ChargeableUnit
		{
			get { return Consol.JK_Calc_TotalShipmentChargeableUnit; }
		}

		public ZInt ShipmentCount
		{
			get
			{
				return Consol.JK_AgentType == Constants.AgentType.AWBMaster ? Consol.ColoadConsols.Sum(c => c.Shipments.Count) : Consol.Shipments.Count;
			}
		}

		#endregion

		#region Allocated

		public ZDecimal AllocatedWeight
		{
			get { return Consol.JK_TotalShipmentActWeightCheck; }
			private set { Consol.JK_TotalShipmentActWeightCheck = this.GetRoundedValue(AllocatedWeightInfo, value); }
		}

		public ZPropertyInfo AllocatedWeightInfo
		{
			get { return GetZPropertyInfo(Schema.AllocatedWeight); }
		}

		public ZString AllocatedWeightUnit
		{
			get { return Consol.WeightVerificationUnit; }
		}

		public ZDecimal AllocatedVolume
		{
			get { return Consol.JK_TotalShipmentActVolumeCheck; }
			private set { Consol.JK_TotalShipmentActVolumeCheck = this.GetRoundedValue(AllocatedVolumeInfo, value); }
		}

		public ZPropertyInfo AllocatedVolumeInfo
		{
			get { return GetZPropertyInfo(Schema.AllocatedVolume); }
		}

		public ZString AllocatedVolumeUnit
		{
			get { return Consol.VolumeVerificationUnit; }
		}

		public ZDecimal AllocatedChargeable
		{
			get { return Consol.JK_TotalShipmentChargableCheck; }
			private set { Consol.JK_TotalShipmentChargableCheck = this.GetRoundedValue(AllocatedChargeableInfo, value); }
		}

		public ZPropertyInfo AllocatedChargeableInfo
		{
			get { return GetZPropertyInfo(Schema.AllocatedChargeable); }
		}

		public ZString AllocatedChargeableUnit
		{
			get { return Consol.JK_TotalShipmentChargeableUnit; }
		}

		public ZShort AllocatedShipmentCount
		{
			get { return Consol.JK_TotalShipmentCountCheck; }
			private set { Consol.JK_TotalShipmentCountCheck = value; }
		}

		#endregion

		#region New Allocated

		public ZDecimal NewAllocatedWeight { get; private set; }

		public ZPropertyInfo NewAllocatedWeightInfo
		{
			get { return this.GetZPropertyInfo(Schema.NewAllocatedWeight); }
		}

		public ZDecimal NewAllocatedVolume { get; private set; }

		public ZPropertyInfo NewAllocatedVolumeInfo
		{
			get { return this.GetZPropertyInfo(Schema.NewAllocatedVolume); }
		}

		public ZDecimal NewAllocatedChargeable { get; private set; }

		public ZPropertyInfo NewAllocatedChargeableInfo
		{
			get { return this.GetZPropertyInfo(Schema.NewAllocatedChargeable); }
		}

		public ZShort NewAllocatedShipmentCount { get; private set; }

		#endregion

		#endregion

		#region Calculate New Allocation

		void CalculateNewAllocationValues()
		{
			NewAllocatedWeight = AllocatedWeight;
			NewAllocatedVolume = AllocatedVolume;
			NewAllocatedChargeable = AllocatedChargeable;
			NewAllocatedShipmentCount = AllocatedShipmentCount;

			PreAllocationCheckCollection allocationChecks = ForwardingConfigurationRegistry.Instance.ConsolPreAllocationCheck.Value;

			if (allocationChecks.Weight.IsRestriction && Consol.Validation.WeightExceedsPreAllocationPercentage)
			{
				NewAllocatedWeight = GetNewAllocation(Weight, WeightUnit, AllocatedWeight, AllocatedWeightUnit, allocationChecks.Weight.Percentage);
			}

			if (allocationChecks.Volume.IsRestriction && Consol.Validation.VolumeExceedsPreAllocationPercentage)
			{
				NewAllocatedVolume = GetNewAllocation(Volume, VolumeUnit, AllocatedVolume, AllocatedVolumeUnit, allocationChecks.Volume.Percentage);
			}

			if (allocationChecks.Chargeable.IsRestriction && Consol.Validation.ChargeableExceedsPreAllocationPercentage)
			{
				NewAllocatedChargeable = GetNewAllocation(Chargeable, ChargeableUnit, AllocatedChargeable, AllocatedChargeableUnit, allocationChecks.Chargeable.Percentage);
			}

			if (allocationChecks.ShipmentCount.IsRestriction && Consol.Validation.ShipmentCountExceedsPreAllocationPercentage)
			{
				NewAllocatedShipmentCount = (ZShort)decimal.Ceiling(ShipmentCount * 100 / allocationChecks.ShipmentCount.Percentage);
			}

			TypeValidation.CheckValidDecimal(NewAllocatedWeightInfo, JobConsolSchema.JK_TotalShipmentActWeightCheck.Precision, JobConsolSchema.JK_TotalShipmentActWeightCheck.Scale);
			TypeValidation.CheckValidDecimal(NewAllocatedVolumeInfo, JobConsolSchema.JK_TotalShipmentActVolumeCheck.Precision, JobConsolSchema.JK_TotalShipmentActVolumeCheck.Scale);
			TypeValidation.CheckValidDecimal(NewAllocatedChargeableInfo, JobConsolSchema.JK_TotalShipmentChargableCheck.Precision, JobConsolSchema.JK_TotalShipmentChargableCheck.Scale);
			TypeValidation.CheckValidDecimal(NewAllocatedVolumeInfo, JobConsolSchema.JK_TotalShipmentActVolumeCheck.Precision, JobConsolSchema.JK_TotalShipmentActVolumeCheck.Scale);
		}

		ZDecimal GetNewAllocation(ZDecimal totalMeasure, ZString totalUnit, ZDecimal preAllocationMeasure, ZString preAllocationUnit, ZDecimal restrictionPercentage)
		{
			ZDecimal result = preAllocationMeasure;
			if (preAllocationMeasure != 0 && restrictionPercentage > 0)
			{
				if (totalUnit != preAllocationUnit)
				{
					if (Constants.Weight.ContainsCode(totalUnit) && Constants.Weight.ContainsCode(preAllocationUnit))
					{
						totalMeasure = Constants.Weight.Convert(totalMeasure, totalUnit, preAllocationUnit, false);
					}
					else if (Constants.Volume.ContainsCode(totalUnit) && Constants.Volume.ContainsCode(preAllocationUnit))
					{
						totalMeasure = Constants.Volume.Convert(totalMeasure, totalUnit, preAllocationUnit, false);
					}
				}

				result = totalMeasure * 100 / restrictionPercentage;
			}

			return result;
		}

		#endregion

		#region Adjustment

		public void Adjust(GlbStaff authorizer)
		{
			if (AllocatedWeight != NewAllocatedWeight)
			{
				AddAdjustmentLog(authorizer, (NoResString)"weight", AllocatedWeight, NewAllocatedWeight, true);  // Log identifier
				AllocatedWeight = NewAllocatedWeight;
			}

			if (AllocatedVolume != NewAllocatedVolume)
			{
				AddAdjustmentLog(authorizer, (NoResString)"volume", AllocatedVolume, NewAllocatedVolume, true);  // Log identifier
				AllocatedVolume = NewAllocatedVolume;
			}

			if (AllocatedChargeable != NewAllocatedChargeable)
			{
				AddAdjustmentLog(authorizer, (NoResString)"chargeable", AllocatedChargeable, NewAllocatedChargeable, true);  // Log identifier
				AllocatedChargeable = NewAllocatedChargeable;
			}

			if (AllocatedShipmentCount != NewAllocatedShipmentCount)
			{
				AddAdjustmentLog(authorizer, (NoResString)"no. of shipments", (ZDecimal)AllocatedShipmentCount, (ZDecimal)NewAllocatedShipmentCount, false); // Log identifier
				AllocatedShipmentCount = NewAllocatedShipmentCount;
			}
		}

		void AddAdjustmentLog(GlbStaff authorizer, ZString allocationMeasure, ZDecimal oldValue, ZDecimal newValue, bool showDecimals)
		{
			ZString reference = string.Format(CultureInfo.InvariantCulture, (NoResString)"Pre-Allocated {0} changed: {1} => {2}", allocationMeasure, oldValue.ToString(showDecimals ? 3 : 0), newValue.ToString(showDecimals ? 3 : 0));  // Log identifier
			StmALog adjustmentLog = Consol.Logs.AddNew(Events.Authorised, reference);

			if (authorizer != null)
			{
				adjustmentLog.SL_GS_NKUser = authorizer.GS_Code;
			}
		}

		#endregion

		#region IDefaultNumberOfDecimalsSupporter Members

		ZString IDefaultNumberOfDecimalsSupporter.TransportMode
		{
			get
			{
				IDefaultNumberOfDecimalsSupporter parentConsol = Consol;
				var result = (parentConsol != null) ? parentConsol.TransportMode : ZString.Empty;

				return result;
			}
		}

		public int GetDefaultNumberOfDecimals(PropertyDescriptor property)
		{
			return DefaultNumberOfDecimalsSupporterHelperForFreight.GetDefaultNumberOfDecimalsMetaDataProperty(this, property);
		}

		ZString IDefaultNumberOfDecimalsSupporter.GetUnitOfMeasure(PropertyDescriptor property)
		{
			var unitOfMeasure = ZString.Empty;
			var propertyName = DefaultNumberOfDecimals.GetBoundPropertyName(property.Name);

			switch (propertyName)
			{
				case Schema.Weight:
					unitOfMeasure = WeightUnit;
					break;

				case Schema.Volume:
					unitOfMeasure = VolumeUnit;
					break;

				case Schema.Chargeable:
					unitOfMeasure = ChargeableUnit;
					break;

				case Schema.AllocatedWeight:
				case Schema.NewAllocatedWeight:
					unitOfMeasure = AllocatedWeightUnit;
					break;

				case Schema.AllocatedVolume:
				case Schema.NewAllocatedVolume:
					unitOfMeasure = AllocatedVolumeUnit;
					break;

				case Schema.AllocatedChargeable:
				case Schema.NewAllocatedChargeable:
					unitOfMeasure = AllocatedChargeableUnit;
					break;

				default:
					break;
			}

			return unitOfMeasure;
		}

		ZDecimal IDefaultNumberOfDecimalsSupporter.GetRoundedValue(PropertyDescriptor property, ZDecimal value)
		{
			return DefaultNumberOfDecimalsSupporterHelperForFreight.GetRoundedValue(this, property, value);
		}

		void IDefaultNumberOfDecimalsSupporter.RoundMeasurePropertiesOnTransportModeChanged()
		{
			// no change in transport mode expected during the lifecycle of this bizObj
		}

		#endregion
	}
}
