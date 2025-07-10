using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;

namespace Enterprise.Freight.Business.Testing
{
	sealed class PackLocationDefaultNumberOfDecimalsSupporterTest : DefaultNumberOfDecimalsSupporterForFreightTest
	{
		public override void TestRoundingWhenDefaultNumberOfDecimalsChange()
		{
			FreightConfigurationRegistry.Instance.DefaultNumberOfDecimalPlaces.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new DefaultNumberOfDecimalsCollection(Module.Freight));

			var shipment = Factory.New<CommonShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;

			var line = shipment.OuterPackLines.AddNew();
			var location = line.PackLocations.AddNew();
			location.JQ_WeightUW = Core.Constants.Weight.Kilograms;
			location.JQ_VolumeUV = Core.Constants.Volume.CubicMetres;

			location.JQ_Weight = 15.231m;
			location.JQ_Volume = 2.428m;

			AssertEquals(15.231m, location.JQ_Weight);
			AssertEquals(2.428m, location.JQ_Volume);

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

			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;

			AssertEquals(15.24m, location.JQ_Weight);
			AssertEquals(2.42m, location.JQ_Volume);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			var shipment = Factory.New<CommonShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;

			var line = shipment.OuterPackLines.AddNew();
			location = line.PackLocations.AddNew();
			location.JQ_WeightUW = Core.Constants.Weight.Kilograms;
			location.JQ_VolumeUV = Core.Constants.Volume.CubicMetres;
		}

		public override BusinessObject BizObj
		{
			get { return location; }
		}
		PackLocation location;

		public override Dictionary<ZString, ZString> MeasurePropertiesAndUnits
		{
			get
			{
				if (measurePropertiesAndUnits == null)
				{
					measurePropertiesAndUnits = new Dictionary<ZString, ZString>();

					measurePropertiesAndUnits.Add(PackLocation.Schema.JQ_Volume, PackLocation.Schema.JQ_VolumeUV);
					measurePropertiesAndUnits.Add(PackLocation.Schema.JQ_Weight, PackLocation.Schema.JQ_WeightUW);
				}

				return measurePropertiesAndUnits;
			}
		}
		Dictionary<ZString, ZString> measurePropertiesAndUnits;

		#endregion

	}
}
