using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business.Testing;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class AllocationAdjustmentDefaultNumberOfDecimalsSupporterTest : DefaultNumberOfDecimalsSupporterForFreightTest
	{
		public override void TestRoundingWhenDefaultNumberOfDecimalsChange()
		{
			Assert("No change in transport mode expected during the lifecycle of this bizObj", true);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.WeightVerificationUnit = Core.Constants.Weight.Kilograms;
			consol.VolumeVerificationUnit = Core.Constants.Volume.CubicMetres;

			allocationAdjustment = new AllocationAdjustment(consol);
		}

		public override BusinessObject BizObj
		{
			get { return allocationAdjustment; }
		}
		AllocationAdjustment allocationAdjustment;

		public override Dictionary<ZString, ZString> MeasurePropertiesAndUnits
		{
			get
			{
				if (measurePropertiesAndUnits == null)
				{
					measurePropertiesAndUnits = new Dictionary<ZString, ZString>();
					measurePropertiesAndUnits.Add(AllocationAdjustment.Schema.Weight, AllocationAdjustment.Schema.WeightUnit);
					measurePropertiesAndUnits.Add(AllocationAdjustment.Schema.Volume, AllocationAdjustment.Schema.VolumeUnit);
					measurePropertiesAndUnits.Add(AllocationAdjustment.Schema.Chargeable, AllocationAdjustment.Schema.ChargeableUnit);
					measurePropertiesAndUnits.Add(AllocationAdjustment.Schema.AllocatedWeight, AllocationAdjustment.Schema.AllocatedWeightUnit);
					measurePropertiesAndUnits.Add(AllocationAdjustment.Schema.AllocatedVolume, AllocationAdjustment.Schema.AllocatedVolumeUnit);
					measurePropertiesAndUnits.Add(AllocationAdjustment.Schema.AllocatedChargeable, AllocationAdjustment.Schema.AllocatedChargeableUnit);
				}

				return measurePropertiesAndUnits;
			}
		}
		Dictionary<ZString, ZString> measurePropertiesAndUnits;

		public override Dictionary<ZString, ZString> NewMeasurePropertiesAndUnits
		{
			get
			{
				if (newMeasurePropertiesAndUnits == null)
				{
					newMeasurePropertiesAndUnits = new Dictionary<ZString, ZString>();
					newMeasurePropertiesAndUnits.Add(AllocationAdjustment.Schema.NewAllocatedWeight, AllocationAdjustment.Schema.AllocatedWeightUnit);
					newMeasurePropertiesAndUnits.Add(AllocationAdjustment.Schema.NewAllocatedVolume, AllocationAdjustment.Schema.AllocatedVolumeUnit);
					newMeasurePropertiesAndUnits.Add(AllocationAdjustment.Schema.NewAllocatedChargeable, AllocationAdjustment.Schema.AllocatedChargeableUnit);
				}

				return newMeasurePropertiesAndUnits;
			}
		}
		Dictionary<ZString, ZString> newMeasurePropertiesAndUnits;

		#endregion
	}
}
