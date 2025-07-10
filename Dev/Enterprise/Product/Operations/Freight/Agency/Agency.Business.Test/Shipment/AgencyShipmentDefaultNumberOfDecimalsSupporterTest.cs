using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.Agency.Business.Testing
{
	internal class AgencyShipmentDefaultNumberOfDecimalsSupporterTest : DefaultNumberOfDecimalsSupporterForShippingTest
	{
		protected override void SetUp()
		{
			base.SetUp();
			shipment = Factory.New<AgencyShipment>();
		}

		public override BusinessObject BizObj
		{
			get
			{
				return shipment;
			}
		}

		AgencyShipment shipment;
		public override Dictionary<ZString, ZString> MeasurePropertiesAndUnits
		{
			get
			{
				if (measurePropertiesAndUnits == null)
				{
					measurePropertiesAndUnits = new Dictionary<ZString, ZString>();
					measurePropertiesAndUnits.Add(AgencyShipment.Schema.TopLevelPacksTotalVolumeInShipmentVolumeUnit, AgencyShipment.Schema.JS_UnitOfVolume);
					measurePropertiesAndUnits.Add(AgencyShipment.Schema.TopLevelPacksTotalWeightInShipmentWeightUnit, AgencyShipment.Schema.JS_UnitOfWeight);
					measurePropertiesAndUnits.Add(AgencyShipment.Schema.JS_ActualWeight, AgencyShipment.Schema.JS_UnitOfWeight);
					measurePropertiesAndUnits.Add(AgencyShipment.Schema.JS_DocumentedWeight, AgencyShipment.Schema.JS_UnitOfWeight);
					measurePropertiesAndUnits.Add(AgencyShipment.Schema.JS_ManifestedWeight, AgencyShipment.Schema.JS_UnitOfWeight);
					measurePropertiesAndUnits.Add(AgencyShipment.Schema.JS_ActualWeightReadOnly, AgencyShipment.Schema.JS_UnitOfWeight);
					measurePropertiesAndUnits.Add(AgencyShipment.Schema.JS_Calc_ActualVolumeWeight, AgencyShipment.Schema.JS_Calc_ActualVolumeWeightUnit);
					measurePropertiesAndUnits.Add(AgencyShipment.Schema.JS_ActualVolume, AgencyShipment.Schema.JS_UnitOfVolume);
					measurePropertiesAndUnits.Add(AgencyShipment.Schema.JS_DocumentedVolume, AgencyShipment.Schema.JS_UnitOfVolume);
					measurePropertiesAndUnits.Add(AgencyShipment.Schema.JS_ManifestedVolume, AgencyShipment.Schema.JS_UnitOfVolume);
					measurePropertiesAndUnits.Add(AgencyShipment.Schema.JS_ActualVolumeReadOnly, AgencyShipment.Schema.JS_UnitOfVolume);
					measurePropertiesAndUnits.Add(AgencyShipment.Schema.JS_ActualChargeable, AgencyShipment.Schema.JS_ChargeableUnit);
					measurePropertiesAndUnits.Add(AgencyShipment.Schema.TotalInnerPackLineWeight, AgencyShipment.Schema.TotalPackLineWeightUnit);
					measurePropertiesAndUnits.Add(AgencyShipment.Schema.TotalOuterPacksWeight, AgencyShipment.Schema.TotalPackLineWeightUnit);
					measurePropertiesAndUnits.Add(AgencyShipment.Schema.TotalInnerPackLineVolume, AgencyShipment.Schema.TotalPackLineVolumeUnit);
					measurePropertiesAndUnits.Add(AgencyShipment.Schema.TotalOuterPacksVolume, AgencyShipment.Schema.TotalPackLineVolumeUnit);
					measurePropertiesAndUnits.Add(AgencyShipment.Schema.TotalOuterPacksWeight_Imperial, Constants.Weight.Pounds);
					measurePropertiesAndUnits.Add(AgencyShipment.Schema.JS_ActualWeight_Imperial, Constants.Weight.Pounds);
					measurePropertiesAndUnits.Add(AgencyShipment.Schema.TotalOuterPacksVolume_Imperial, Constants.Volume.CubicFeet);
					measurePropertiesAndUnits.Add(AgencyShipment.Schema.JS_ActualVolume_Imperial, Constants.Volume.CubicFeet);
					measurePropertiesAndUnits.Add(AgencyShipment.Schema.JS_Calc_DocumentedWeight_Converted, Env.Registry.FreightWeightUnit);
					measurePropertiesAndUnits.Add(AgencyShipment.Schema.JS_Calc_DocumentedVolume_Converted, Env.Registry.FreightVolumeUnit);
				}

				return measurePropertiesAndUnits;
			}
		}

		Dictionary<ZString, ZString> measurePropertiesAndUnits;
	}
}
