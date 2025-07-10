using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.Registry.Business;

namespace Enterprise.Freight.CFS.Business.Testing
{
	public class CFSLoadListConsolDefaultNumberOfDecimalsSupporterTest : CommonConsolDefaultNumberOfDecimalsSupporterTest
	{
		public override void TestRoundingWhenDefaultNumberOfDecimalsChange()
		{
			FreightConfigurationRegistry.Instance.DefaultNumberOfDecimalPlaces.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new DefaultNumberOfDecimalsCollection(Module.Freight));

			var loadList = Factory.New<CFSLoadListConsol>();
			loadList.JK_OverrideConsolChargeable = true;
			loadList.JK_TransportMode = Core.Constants.TransportModes.Air;
			loadList.JK_TotalShipmentActOtherUnit = Core.Constants.Volume.CubicMetres;
			loadList.JK_TotalShipmentChargeableUnit = Core.Constants.Weight.Kilograms;

			loadList.JK_ConsolChargeable = 156.158m;
			loadList.JK_CorrectedConsolWeight = 162.264m;
			loadList.JK_CorrectedConsolVolume = 5.181m;

			loadList.JK_TotalShipmentActVolumeCheck = 5.126m;
			loadList.JK_TotalShipmentActWeightCheck = 126.136m;
			loadList.JK_TotalShipmentChargableCheck = 135.624m;

			var shipment = loadList.Shipments.AddNew();
			shipment.JS_UnitOfWeight = Core.Constants.Weight.Kilograms;
			shipment.JS_UnitOfVolume = Core.Constants.Volume.CubicMetres;

			shipment.JS_ActualWeight = 123.522m;
			shipment.JS_DocumentedWeight = 150.526m;
			shipment.JS_ManifestedWeight = 235.123m;

			shipment.JS_ActualVolume = 5.549m;
			shipment.JS_DocumentedVolume = 4.258m;
			shipment.JS_ManifestedVolume = 3.126m;

			AssertEquals(123.522m, loadList.JK_TotalShipmentWeight);
			AssertEquals(150.526m, loadList.JK_TotalDocumentedWeight);
			AssertEquals(235.123m, loadList.JK_TotalManifestedWeight);

			AssertEquals(5.549m, loadList.JK_TotalShipmentVolume);
			AssertEquals(4.258m, loadList.JK_TotalDocumentedVolume);
			AssertEquals(3.126m, loadList.JK_TotalManifestedVolume);

			AssertEquals("JK_TotalShipmentChargeable is made up of shipment weight for AIR transport mode.", 924.833m, loadList.JK_TotalShipmentChargeable);

			AssertEquals("JK_ConsolChargeable is made up of shipment weight for AIR transport mode.", 863.500m, loadList.JK_ConsolChargeable);
			AssertEquals("JK_TotalDocumentedChargeable is made up of shipment weight for AIR transport mode.", 709.667m, loadList.JK_TotalDocumentedChargeable);
			AssertEquals("JK_TotalManifestedChargeable is made up of shipment weight for AIR transport mode.", 521.000m, loadList.JK_TotalManifestedChargeable);

			AssertEquals(5.181m, loadList.JK_CorrectedConsolVolume);
			AssertEquals(162.264m, loadList.JK_CorrectedConsolWeight);

			AssertEquals(5.126m, loadList.JK_TotalShipmentActVolumeCheck);
			AssertEquals(126.136m, loadList.JK_TotalShipmentActWeightCheck);
			AssertEquals(135.624m, loadList.JK_TotalShipmentChargableCheck);

			var collection = new DefaultNumberOfDecimalsCollection(Module.Freight);
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

			loadList.JK_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			loadList.JK_TotalShipmentChargeableUnit = Core.Constants.Volume.CubicMetres;
			loadList.JK_TotalShipmentActOtherUnit = Core.Constants.Weight.Kilograms;

			AssertEquals(123.53m, loadList.JK_TotalShipmentWeight);
			AssertEquals(150.53m, loadList.JK_TotalDocumentedWeight);
			AssertEquals(235.13m, loadList.JK_TotalManifestedWeight);

			AssertEquals(5.54m, loadList.JK_TotalShipmentVolume);
			AssertEquals(4.25m, loadList.JK_TotalDocumentedVolume);
			AssertEquals(3.12m, loadList.JK_TotalManifestedVolume);

			AssertEquals("JK_TotalShipmentChargeable is made up of shipment volume for SEA transport mode.", 5.54m, loadList.JK_TotalShipmentChargeable);

			AssertEquals("JK_ConsolChargeable is made up of shipment volume for SEA transport mode.", 5.18m, loadList.JK_ConsolChargeable);
			AssertEquals("JK_TotalDocumentedChargeable is made up of shipment volume for SEA transport mode.", 4.25m, loadList.JK_TotalDocumentedChargeable);
			AssertEquals("JK_TotalManifestedChargeable is made up of shipment volume for SEA transport mode.", 3.12m, loadList.JK_TotalManifestedChargeable);

			AssertEquals(5.18m, loadList.JK_CorrectedConsolVolume);
			AssertEquals(162.27m, loadList.JK_CorrectedConsolWeight);

			AssertEquals(5.13m, loadList.JK_TotalShipmentActVolumeCheck);
			AssertEquals(126.13m, loadList.JK_TotalShipmentActWeightCheck);
			AssertEquals(135.63m, loadList.JK_TotalShipmentChargableCheck);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			consol = Factory.New<CFSLoadListConsol>();
			consol.JK_OverrideConsolChargeable = true;
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;

			consol.JK_TotalShipmentActOtherUnit = Core.Constants.Volume.CubicMetres;
			consol.JK_TotalShipmentChargeableUnit = Core.Constants.Weight.Kilograms;
		}

		public override BusinessObject BizObj
		{
			get { return consol; }
		}
		CFSLoadListConsol consol;

		public override Dictionary<ZString, ZString> MeasurePropertiesAndUnits
		{
			get
			{
				if (measurePropertiesAndUnits == null)
				{
					measurePropertiesAndUnits = base.MeasurePropertiesAndUnits;
					measurePropertiesAndUnits.Add(CFSLoadListConsol.Schema.JK_TotalShipmentActVolumeCheck, CFSLoadListConsol.Schema.JK_TotalShipmentActOtherUnit);
					measurePropertiesAndUnits.Add(CFSLoadListConsol.Schema.JK_TotalShipmentActWeightCheck, CFSLoadListConsol.Schema.JK_TotalShipmentChargeableUnit);
					measurePropertiesAndUnits.Add(CFSLoadListConsol.Schema.JK_TotalShipmentChargableCheck, CFSLoadListConsol.Schema.JK_TotalShipmentChargeableUnit);
				}

				return measurePropertiesAndUnits;
			}
		}
		Dictionary<ZString, ZString> measurePropertiesAndUnits;

		#endregion

	}
}
