using System.Globalization;
using CargoWise.EntityFramework;
using Enterprise.Core;
using NUnit.Framework;
using DensityValues = Enterprise.Freight.Forwarding.Business.Density.DensityValues;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	[TestedType(typeof(ShipmentDensity))]
	sealed class ShipmentDensityTest : BaseDensityTest
	{
		public void TestDensityFactorIsCalculated_WhenVolumeChargeableFactorActualWeightChanged()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.JS_UnitOfVolume = Constants.Volume.CubicCentimeters;
			shipment.JS_UnitOfWeight = Constants.Weight.Kilograms;
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_ActualVolume = 6000m;
			shipment.JS_ActualWeight = 1m;
			var expectedDensityValues = new DensityValues(1m, "1:6", "1 to 1 cargo");
			AssertDensities("Density should be calculated in AIR", expectedDensityValues, shipment.Density);

			shipment.JS_ActualVolume = 3000m;
			expectedDensityValues = new DensityValues(0.5m, "1:3", "Dense ++");
			AssertDensities("Density should change when volume changed in AIR", expectedDensityValues, shipment.Density);

			shipment.JS_ActualWeight = 0.5m;
			expectedDensityValues = new DensityValues(1m, "1:6", "1 to 1 cargo");
			AssertDensities("Density should change when weight changed in AIR", expectedDensityValues, shipment.Density);

			shipment.JS_ActualVolume = 0m;
			expectedDensityValues = new DensityValues(0m, "1:1", "Dense ++++");
			AssertDensities("Density should be 0 when volume is 0 in AIR", expectedDensityValues, shipment.Density);

			shipment.JS_ActualWeight = 0m;
			expectedDensityValues = new DensityValues(Density.DensityFactorForZeroDivision, Density.NAValueForZeroDivision, Density.NAValueForZeroDivision);
			AssertDensities("Density should be N/A when weight is 0 in AIR", expectedDensityValues, shipment.Density);

			shipment.JS_ActualVolume = 332m;
			shipment.JS_ActualWeight = 1m;
			shipment.JS_UnitOfVolume = Constants.Volume.CubicInches;
			shipment.JS_UnitOfWeight = Constants.Weight.Pounds;
			expectedDensityValues = new DensityValues(2m, "1:12", "Volume +++++");
			AssertDensities("Density should change when units changed in AIR", expectedDensityValues, shipment.Density);

			shipment.JS_RL_NKDestination = "AUMEL";
			expectedDensityValues = new DensityValues(1.71134m, "1:10", "Volume +++");
			AssertDensities("Density should change when switching to domestic with destination change in AIR", expectedDensityValues, shipment.Density);

			shipment.JS_RL_NKOrigin = "USLAX";
			expectedDensityValues = new DensityValues(2m, "1:12", "Volume +++++");
			AssertDensities("Density should change when switching to international with origin change in AIR", expectedDensityValues, shipment.Density);

			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_UnitOfVolume = Constants.Volume.CubicMetres;
			shipment.JS_UnitOfWeight = Constants.Weight.Kilograms;
			shipment.JS_ActualVolume = 3m;
			shipment.JS_ActualWeight = 4000m;
			expectedDensityValues = new DensityValues(0.7500m, "1:5", "Dense");
			AssertDensities("Density should change when transport mode changed to SEA", expectedDensityValues, shipment.Density);

			shipment.JS_ActualWeight = 1875m;
			expectedDensityValues = new DensityValues(1.6m, "1:10", "Volume +++");
			AssertDensities("Density should change when weight changed in SEA", expectedDensityValues, shipment.Density);

			shipment.JS_ActualWeight = 0m;
			expectedDensityValues = new DensityValues(Density.DensityFactorForZeroDivision, Density.NAValueForZeroDivision, Density.NAValueForZeroDivision);
			AssertDensities("Density should be N/A when weight is 0 in SEA", expectedDensityValues, shipment.Density);

			shipment.JS_ActualVolume = 0m;
			expectedDensityValues = new DensityValues(Density.DensityFactorForZeroDivision, Density.NAValueForZeroDivision, Density.NAValueForZeroDivision);
			AssertDensities("Density should be N/A when volume is 0 in SEA", expectedDensityValues, shipment.Density);
		}

		void AssertDensities(string message, DensityValues expected, ShipmentDensity result)
		{
			CombineAssertions(message, () =>
			{
				AssertEquals("Density Factor",
					expected.DensityFactor.ToString("F4", CultureInfo.CurrentCulture),
					result.DensityFactor.ToString("F4", CultureInfo.CurrentCulture)
				);
				AssertEquals("Density Remark", expected.DensityRemark, result.DensityRemark);
				AssertEquals("Volume Ratio", expected.VolumeRatio, result.VolumeRatio);
			});
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new ShipmentDensity(Factory.New<ForwardingShipment>());
		}

		#endregion
	}
}
