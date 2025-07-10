using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Registry.Business;

namespace Enterprise.Freight.Business.Testing
{
	public class CommonConsolDefaultNumberOfDecimalsSupporterTest : DefaultNumberOfDecimalsSupporterForFreightTest
	{
		public override void TestRoundingWhenDefaultNumberOfDecimalsChange()
		{
			FreightConfigurationRegistry.Instance.DefaultNumberOfDecimalPlaces.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new DefaultNumberOfDecimalsCollection(Module.Freight));

			consol.JK_ConsolChargeable = 156.158m;
			consol.JK_CorrectedConsolWeight = 162.264m;
			consol.JK_CorrectedConsolVolume = 5.181m;

			var shipment = consol.Shipments.AddNew();
			shipment.JS_UnitOfWeight = Constants.Weight.Kilograms;
			shipment.JS_UnitOfVolume = Constants.Volume.CubicMetres;

			shipment.JS_ActualWeight = 123.522m;
			shipment.JS_DocumentedWeight = 150.526m;
			shipment.JS_ManifestedWeight = 235.123m;

			shipment.JS_ActualVolume = 5.549m;
			shipment.JS_DocumentedVolume = 4.258m;
			shipment.JS_ManifestedVolume = 3.126m;

			AssertEquals(123.522m, consol.JK_TotalShipmentWeight);
			AssertEquals(150.526m, consol.JK_TotalDocumentedWeight);
			AssertEquals(235.123m, consol.JK_TotalManifestedWeight);

			AssertEquals(5.549m, consol.JK_TotalShipmentVolume);
			AssertEquals(4.258m, consol.JK_TotalDocumentedVolume);
			AssertEquals(3.126m, consol.JK_TotalManifestedVolume);

			AssertEquals("JK_TotalShipmentChargeable is made up of shipment weight for AIR transport mode.", 924.833m, consol.JK_TotalShipmentChargeable);

			AssertEquals("JK_ConsolChargeable is made up of shipment weight for AIR transport mode.", 863.500m, consol.JK_ConsolChargeable);
			AssertEquals("JK_TotalDocumentedChargeable is made up of shipment weight for AIR transport mode.", 709.667m, consol.JK_TotalDocumentedChargeable);
			AssertEquals("JK_TotalManifestedChargeable is made up of shipment weight for AIR transport mode.", 521.000m, consol.JK_TotalManifestedChargeable);

			AssertEquals(5.181m, consol.JK_CorrectedConsolVolume);
			AssertEquals(162.264m, consol.JK_CorrectedConsolWeight);

			var collection = new DefaultNumberOfDecimalsCollection(Module.Freight);
			var defaultNumberOfDecimals_SeaWeight = collection.AddNew();
			defaultNumberOfDecimals_SeaWeight.UnitOfMeasure = Constants.Weight.Kilograms;
			defaultNumberOfDecimals_SeaWeight.TransportMode = Constants.TransportModes.Sea;
			defaultNumberOfDecimals_SeaWeight.NumberOfDecimals = 2;
			defaultNumberOfDecimals_SeaWeight.RoundingMode = RoundingModes.Up;
			var defaultNumberOfDecimals_SeaVolume = collection.AddNew();
			defaultNumberOfDecimals_SeaVolume.UnitOfMeasure = Constants.Volume.CubicMetres;
			defaultNumberOfDecimals_SeaVolume.TransportMode = Constants.TransportModes.Sea;
			defaultNumberOfDecimals_SeaVolume.NumberOfDecimals = 2;
			defaultNumberOfDecimals_SeaVolume.RoundingMode = RoundingModes.Down;

			FreightConfigurationRegistry.Instance.DefaultNumberOfDecimalPlaces.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			consol.JK_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_TransportMode = Constants.TransportModes.Sea;

			AssertEquals(123.53m, consol.JK_TotalShipmentWeight);
			AssertEquals(150.53m, consol.JK_TotalDocumentedWeight);
			AssertEquals(235.13m, consol.JK_TotalManifestedWeight);

			AssertEquals(5.54m, consol.JK_TotalShipmentVolume);
			AssertEquals(4.25m, consol.JK_TotalDocumentedVolume);
			AssertEquals(3.12m, consol.JK_TotalManifestedVolume);

			AssertEquals("JK_TotalShipmentChargeable is made up of shipment volume for SEA transport mode.", 5.54m, consol.JK_TotalShipmentChargeable);

			AssertEquals("JK_ConsolChargeable is made up of shipment volume for SEA transport mode.", 5.18m, consol.JK_ConsolChargeable);
			AssertEquals("JK_TotalDocumentedChargeable is made up of shipment volume for SEA transport mode.", 4.25m, consol.JK_TotalDocumentedChargeable);
			AssertEquals("JK_TotalManifestedChargeable is made up of shipment volume for SEA transport mode.", 3.12m, consol.JK_TotalManifestedChargeable);

			AssertEquals(5.18m, consol.JK_CorrectedConsolVolume);
			AssertEquals(162.27m, consol.JK_CorrectedConsolWeight);
		}

		protected override void SetUp()
		{
			base.SetUp();

			consol = Factory.New<CommonConsol>();
			consol.JK_OverrideConsolChargeable = true;
			consol.JK_TransportMode = Constants.TransportModes.Air;

			consol.JK_CorrectedConsolVolumeUnit = Constants.Volume.CubicMetres;
			consol.JK_CorrectedConsolWeightUnit = Constants.Weight.Kilograms;
		}

		public override BusinessObject BizObj
		{
			get { return consol; }
		}
		CommonConsol consol;

		public override Dictionary<ZString, ZString> MeasurePropertiesAndUnits
		{
			get
			{
				if (measurePropertiesAndUnits == null)
				{
					measurePropertiesAndUnits = new Dictionary<ZString, ZString>();
					measurePropertiesAndUnits.Add(CommonConsol.Schema.JK_TotalShipmentWeight, CommonConsol.Schema.JK_TotalShipmentWeightUnit);
					measurePropertiesAndUnits.Add(CommonConsol.Schema.JK_TotalDocumentedWeight, CommonConsol.Schema.JK_TotalShipmentWeightUnit);
					measurePropertiesAndUnits.Add(CommonConsol.Schema.JK_TotalManifestedWeight, CommonConsol.Schema.JK_TotalShipmentWeightUnit);

					measurePropertiesAndUnits.Add(CommonConsol.Schema.JK_TotalShipmentVolume, CommonConsol.Schema.JK_TotalShipmentVolumeUnit);
					measurePropertiesAndUnits.Add(CommonConsol.Schema.JK_TotalDocumentedVolume, CommonConsol.Schema.JK_TotalShipmentVolumeUnit);
					measurePropertiesAndUnits.Add(CommonConsol.Schema.JK_TotalManifestedVolume, CommonConsol.Schema.JK_TotalShipmentVolumeUnit);
					measurePropertiesAndUnits.Add(CommonConsol.Schema.JK_TotalShipmentChargeable, CommonConsol.Schema.JK_Calc_TotalShipmentChargeableUnit);

					measurePropertiesAndUnits.Add(CommonConsol.Schema.JK_ConsolChargeable, CommonConsol.Schema.JK_ConsolChargeableUnit);
					measurePropertiesAndUnits.Add(CommonConsol.Schema.JK_TotalDocumentedChargeable, CommonConsol.Schema.JK_ConsolChargeableUnit);
					measurePropertiesAndUnits.Add(CommonConsol.Schema.JK_TotalManifestedChargeable, CommonConsol.Schema.JK_ConsolChargeableUnit);

					measurePropertiesAndUnits.Add(CommonConsol.Schema.JK_CorrectedConsolVolume, CommonConsol.Schema.JK_CorrectedConsolVolumeUnit);
					measurePropertiesAndUnits.Add(CommonConsol.Schema.JK_CorrectedConsolWeight, CommonConsol.Schema.JK_CorrectedConsolWeightUnit);
					measurePropertiesAndUnits.Add(CommonConsol.Schema.JK_Calc_FreeSpace, CommonConsol.Schema.JK_Calc_TotalShipmentChargeableUnit);
					measurePropertiesAndUnits.Add(CommonConsol.Schema.JK_Calc_ActualVolumeWeight, CommonConsol.Schema.JK_Calc_ActualVolumeWeightUnit);
				}

				return measurePropertiesAndUnits;
			}
		}
		Dictionary<ZString, ZString> measurePropertiesAndUnits;
	}
}
