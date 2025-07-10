using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Registry.Business;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.Business.Testing
{
	sealed class CommonShipmentDefaultNumberOfDecimalsSupporterTest : DefaultNumberOfDecimalsSupporterForFreightTest
	{
		public override void TestRoundingWhenDefaultNumberOfDecimalsChange()
		{
			FreightConfigurationRegistry.Instance.DefaultNumberOfDecimalPlaces.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new DefaultNumberOfDecimalsCollection(Enterprise.Registry.Business.Module.Freight));

			shipment.JS_ActualWeight = 12.251m;
			shipment.JS_ActualVolume = 3.128m;
			shipment.JS_ActualChargeable = 15.265m;

			var innerPackLine = shipment.InnerPackLines.AddNew();
			innerPackLine.JL_ActualWeight = 10.362m;
			innerPackLine.JL_ActualVolume = 1.526m;
			var outerPackLine = shipment.OuterPackLines.AddNew();
			outerPackLine.JL_ActualWeight = 1.231m;
			outerPackLine.JL_ActualVolume = 1.648m;

			AssertEquals(12.251m, shipment.JS_ActualWeight);
			AssertEquals(12.251m, shipment.JS_DocumentedWeight);
			AssertEquals(12.251m, shipment.JS_ManifestedWeight);

			AssertEquals(3.128m, shipment.JS_ActualVolume);
			AssertEquals(3.128m, shipment.JS_DocumentedVolume);
			AssertEquals(3.128m, shipment.JS_ManifestedVolume);

			AssertEquals(15.265m, shipment.JS_ActualChargeable);
			AssertEquals(15.265m, shipment.JS_DocumentedChargeable);
			AssertEquals(15.265m, shipment.JS_ManifestedChargeable);

			AssertEquals(10.362m, shipment.TotalInnerPackLineWeight);
			AssertEquals(1.526m, shipment.TotalInnerPackLineVolume);
			AssertEquals("Sum of OuterPackLines JL_ActualWeight and JS_ActualWeight", 13.482m, shipment.TotalOuterPacksWeight);
			AssertEquals("Sum of OuterPackLines JL_ActualVolume and JS_ActualVolume", 4.776m, shipment.TotalOuterPacksVolume);

			AssertEquals(29.723m, shipment.TotalOuterPacksWeight_Imperial);
			AssertEquals(168.663m, shipment.TotalOuterPacksVolume_Imperial);
			AssertEquals(27.009m, shipment.JS_ActualWeight_Imperial);
			AssertEquals(110.464m, shipment.JS_ActualVolume_Imperial);

			AssertEquals(12.251m, shipment.JS_Calc_DocumentedWeight_Converted);
			AssertEquals(3.128m, shipment.JS_Calc_DocumentedVolume_Converted);

			var collection = new DefaultNumberOfDecimalsCollection(Enterprise.Registry.Business.Module.Freight);
			var defaultNumberOfDecimals_SeaWeight = collection.AddNew();
			defaultNumberOfDecimals_SeaWeight.UnitOfMeasure = Core.Constants.Weight.Kilograms;
			defaultNumberOfDecimals_SeaWeight.TransportMode = Core.Constants.TransportModes.Sea;
			defaultNumberOfDecimals_SeaWeight.NumberOfDecimals = 2;
			defaultNumberOfDecimals_SeaWeight.RoundingMode = RoundingModes.Up;
			var defaultNumberOfDecimals_SeaVolume = collection.AddNew();
			defaultNumberOfDecimals_SeaVolume.UnitOfMeasure = Core.Constants.Volume.CubicMetres;
			defaultNumberOfDecimals_SeaVolume.TransportMode = Core.Constants.TransportModes.Sea;
			defaultNumberOfDecimals_SeaVolume.NumberOfDecimals = 2;
			defaultNumberOfDecimals_SeaVolume.RoundingMode = RoundingModes.Down;

			FreightConfigurationRegistry.Instance.DefaultNumberOfDecimalPlaces.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;

			AssertEquals(12.26m, shipment.JS_ActualWeight);
			AssertEquals(12.26m, shipment.JS_DocumentedWeight);
			AssertEquals(12.26m, shipment.JS_ManifestedWeight);

			AssertEquals(3.12m, shipment.JS_ActualVolume);
			AssertEquals(3.12m, shipment.JS_DocumentedVolume);
			AssertEquals(3.12m, shipment.JS_ManifestedVolume);

			AssertEquals("Chargeable is reset due to change of transport mode.", 3.12m, shipment.JS_ActualChargeable);
			AssertEquals("Chargeable is reset due to change of transport mode.", 3.12m, shipment.JS_DocumentedChargeable);
			AssertEquals("Chargeable is reset due to change of transport mode.", 3.12m, shipment.JS_ManifestedChargeable);

			AssertEquals(10.37m, shipment.TotalInnerPackLineWeight);
			AssertEquals(1.52m, shipment.TotalInnerPackLineVolume);
			AssertEquals("Sum of OuterPackLines JL_ActualWeight and JS_ActualWeight", 13.50m, shipment.TotalOuterPacksWeight);
			AssertEquals("Sum of OuterPackLines JL_ActualVolume and JS_ActualVolume", 4.76m, shipment.TotalOuterPacksVolume);

			AssertEquals("No imperial units are set in registry. Default 3 decimals are used.", 29.762m, shipment.TotalOuterPacksWeight_Imperial);
			AssertEquals("No imperial units are set in registry. Default 3 decimals are used.", 168.098m, shipment.TotalOuterPacksVolume_Imperial);
			AssertEquals("No imperial units are set in registry. Default 3 decimals are used.", 27.029m, shipment.JS_ActualWeight_Imperial);
			AssertEquals("No imperial units are set in registry. Default 3 decimals are used.", 110.182m, shipment.JS_ActualVolume_Imperial);

			AssertEquals(12.26m, shipment.JS_Calc_DocumentedWeight_Converted);
			AssertEquals(3.12m, shipment.JS_Calc_DocumentedVolume_Converted);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			shipment = Factory.New<CommonShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_UnitOfWeight = Core.Constants.Weight.Kilograms;
			shipment.JS_UnitOfVolume = Core.Constants.Volume.CubicMetres;
		}

		public override BusinessObject BizObj
		{
			get { return shipment; }
		}
		CommonShipment shipment;

		public override Dictionary<ZString, ZString> MeasurePropertiesAndUnits
		{
			get
			{
				if (measurePropertiesAndUnits == null)
				{
					measurePropertiesAndUnits = new Dictionary<ZString, ZString>();
					measurePropertiesAndUnits.Add(CommonShipment.Schema.JS_ActualWeight, CommonShipment.Schema.JS_UnitOfWeight);
					measurePropertiesAndUnits.Add(CommonShipment.Schema.JS_DocumentedWeight, CommonShipment.Schema.JS_UnitOfWeight);
					measurePropertiesAndUnits.Add(CommonShipment.Schema.JS_ManifestedWeight, CommonShipment.Schema.JS_UnitOfWeight);
					measurePropertiesAndUnits.Add(CommonShipment.Schema.JS_ActualWeightReadOnly, CommonShipment.Schema.JS_UnitOfWeight);
					measurePropertiesAndUnits.Add(CommonShipment.Schema.JS_Calc_ActualVolumeWeight, CommonShipment.Schema.JS_Calc_ActualVolumeWeightUnit);

					measurePropertiesAndUnits.Add(CommonShipment.Schema.JS_ActualVolume, CommonShipment.Schema.JS_UnitOfVolume);
					measurePropertiesAndUnits.Add(CommonShipment.Schema.JS_DocumentedVolume, CommonShipment.Schema.JS_UnitOfVolume);
					measurePropertiesAndUnits.Add(CommonShipment.Schema.JS_ManifestedVolume, CommonShipment.Schema.JS_UnitOfVolume);
					measurePropertiesAndUnits.Add(CommonShipment.Schema.JS_ActualVolumeReadOnly, CommonShipment.Schema.JS_UnitOfVolume);

					measurePropertiesAndUnits.Add(CommonShipment.Schema.JS_ActualChargeable, CommonShipment.Schema.JS_ChargeableUnit);

					measurePropertiesAndUnits.Add(CommonShipment.Schema.TotalInnerPackLineWeight, CommonShipment.Schema.TotalPackLineWeightUnit);
					measurePropertiesAndUnits.Add(CommonShipment.Schema.TotalOuterPacksWeight, CommonShipment.Schema.TotalPackLineWeightUnit);

					measurePropertiesAndUnits.Add(CommonShipment.Schema.TotalInnerPackLineVolume, CommonShipment.Schema.TotalPackLineVolumeUnit);
					measurePropertiesAndUnits.Add(CommonShipment.Schema.TotalOuterPacksVolume, CommonShipment.Schema.TotalPackLineVolumeUnit);

					measurePropertiesAndUnits.Add(CommonShipment.Schema.TotalOuterPacksWeight_Imperial, Constants.Weight.Pounds);
					measurePropertiesAndUnits.Add(CommonShipment.Schema.JS_ActualWeight_Imperial, Constants.Weight.Pounds);

					measurePropertiesAndUnits.Add(CommonShipment.Schema.TotalOuterPacksVolume_Imperial, Constants.Volume.CubicFeet);
					measurePropertiesAndUnits.Add(CommonShipment.Schema.JS_ActualVolume_Imperial, Constants.Volume.CubicFeet);

					measurePropertiesAndUnits.Add(CommonShipment.Schema.JS_Calc_DocumentedWeight_Converted, Env.Registry.FreightWeightUnit);
					measurePropertiesAndUnits.Add(CommonShipment.Schema.JS_Calc_DocumentedVolume_Converted, Env.Registry.FreightVolumeUnit);
				}

				return measurePropertiesAndUnits;
			}
		}
		Dictionary<ZString, ZString> measurePropertiesAndUnits;

		#endregion
	}
}
