using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Freight.Agency.Business.Testing
{
	public abstract class AgencyAllocationItemDefaultNumberOfDecimalsSupporterTest : DefaultNumberOfDecimalsSupporterForShippingTest
	{
		public override BusinessObject BizObj
		{
			get
			{
				return null;
			}
		}

		public override Dictionary<ZString, ZString> MeasurePropertiesAndUnits
		{
			get
			{
				if (measurePropertiesAndUnits == null)
				{
					measurePropertiesAndUnits = new Dictionary<ZString, ZString>();
					measurePropertiesAndUnits.Add(AgencyAllocationItem<BusinessObject>.Schema.Tonnes, Core.Constants.Weight.Tonnes);
					measurePropertiesAndUnits.Add(AgencyAllocationItem<BusinessObject>.Schema.UsedTonnes, Core.Constants.Weight.Tonnes);
					measurePropertiesAndUnits.Add(AgencyAllocationItem<BusinessObject>.Schema.OverallocatedTonnes, Core.Constants.Weight.Tonnes);
					measurePropertiesAndUnits.Add(AgencyAllocationItem<BusinessObject>.Schema.Volume, Core.Constants.Volume.CubicMetres);
					measurePropertiesAndUnits.Add(AgencyAllocationItem<BusinessObject>.Schema.UsedVolume, Core.Constants.Volume.CubicMetres);
					measurePropertiesAndUnits.Add(AgencyAllocationItem<BusinessObject>.Schema.OverallocatedVolume, Core.Constants.Volume.CubicMetres);
				}

				return measurePropertiesAndUnits;
			}
		}

		Dictionary<ZString, ZString> measurePropertiesAndUnits;
	}
}
