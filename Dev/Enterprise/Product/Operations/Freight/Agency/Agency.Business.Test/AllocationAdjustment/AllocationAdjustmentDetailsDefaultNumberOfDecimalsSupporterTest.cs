using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;

namespace Enterprise.Freight.Agency.Business.Testing
{
	internal class AllocationAdjustmentDetailsDefaultNumberOfDecimalsSupporterTest : DefaultNumberOfDecimalsSupporterForShippingTest
	{
		protected override void SetUp()
		{
			base.SetUp();
			var allocation = Factory.New<SlotAllocation>();
			var required = new AllocationUsage();
			details = new AllocationAdjustmentDetails(allocation, required);
		}

		public override BusinessObject BizObj
		{
			get
			{
				return details;
			}
		}

		AllocationAdjustmentDetails details;
		public override Dictionary<ZString, ZString> MeasurePropertiesAndUnits
		{
			get
			{
				if (measurePropertiesAndUnits == null)
				{
					measurePropertiesAndUnits = new Dictionary<ZString, ZString>();
					measurePropertiesAndUnits.Add(AllocationAdjustmentDetails.Schema.Allocated_Tonnes, Core.Constants.Weight.Tonnes);
					measurePropertiesAndUnits.Add(AllocationAdjustmentDetails.Schema.TotalRequired_Tonnes, Core.Constants.Weight.Tonnes);
					measurePropertiesAndUnits.Add(AllocationAdjustmentDetails.Schema.OldOverAllocation_Tonnes, Core.Constants.Weight.Tonnes);
					measurePropertiesAndUnits.Add(AllocationAdjustmentDetails.Schema.NewOverAllocation_Tonnes, Core.Constants.Weight.Tonnes);
					measurePropertiesAndUnits.Add(AllocationAdjustmentDetails.Schema.Allocated_Volume, Core.Constants.Volume.CubicMetres);
					measurePropertiesAndUnits.Add(AllocationAdjustmentDetails.Schema.TotalRequired_Volume, Core.Constants.Volume.CubicMetres);
					measurePropertiesAndUnits.Add(AllocationAdjustmentDetails.Schema.OldOverAllocation_Volume, Core.Constants.Volume.CubicMetres);
					measurePropertiesAndUnits.Add(AllocationAdjustmentDetails.Schema.NewOverAllocation_Volume, Core.Constants.Volume.CubicMetres);
				}

				return measurePropertiesAndUnits;
			}
		}

		Dictionary<ZString, ZString> measurePropertiesAndUnits;
	}
}
