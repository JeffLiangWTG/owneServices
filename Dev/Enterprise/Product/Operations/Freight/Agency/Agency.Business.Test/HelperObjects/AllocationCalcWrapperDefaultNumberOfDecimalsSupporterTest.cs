using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Freight.Agency.Business.Testing
{
	internal class AllocationCalcWrapperDefaultNumberOfDecimalsSupporterTest : DefaultNumberOfDecimalsSupporterForShippingTest
	{
		protected override void SetUp()
		{
			base.SetUp();
			wrapper = new AllocationCalcWrapper(Factory.New<AgencyShipment>());
		}

		public override BusinessObject BizObj
		{
			get
			{
				return wrapper;
			}
		}

		AllocationCalcWrapper wrapper;
		public override Dictionary<ZString, ZString> MeasurePropertiesAndUnits
		{
			get
			{
				if (measurePropertiesAndUnits == null)
				{
					measurePropertiesAndUnits = new Dictionary<ZString, ZString>();
					measurePropertiesAndUnits.Add(AllocationCalcWrapper.Schema.Allocated_Tonnes, Core.Constants.Weight.Tonnes);
					measurePropertiesAndUnits.Add(AllocationCalcWrapper.Schema.Available_Tonnes, Core.Constants.Weight.Tonnes);
					measurePropertiesAndUnits.Add(AllocationCalcWrapper.Schema.Required_Tonnes, Core.Constants.Weight.Tonnes);
					measurePropertiesAndUnits.Add(AllocationCalcWrapper.Schema.Allocated_Volume, Core.Constants.Volume.CubicMetres);
					measurePropertiesAndUnits.Add(AllocationCalcWrapper.Schema.Available_Volume, Core.Constants.Volume.CubicMetres);
					measurePropertiesAndUnits.Add(AllocationCalcWrapper.Schema.Required_Volume, Core.Constants.Volume.CubicMetres);
				}

				return measurePropertiesAndUnits;
			}
		}

		Dictionary<ZString, ZString> measurePropertiesAndUnits;
	}
}
