using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using NUnit.Framework;
using DensityValues = Enterprise.Freight.Forwarding.Business.Density.DensityValues;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	[TestedType(typeof(ConsolDensity))]
	sealed class ConsolDensityTest : BaseDensityTest
	{
		public void TestDensityFactor()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_CorrectedConsolWeightUnit = Constants.Weight.Kilograms;
			consol.JK_CorrectedConsolVolumeUnit = Constants.Volume.CubicMetres;
			consol.JK_TransportMode = Constants.TransportModes.Air;

			var shipment1 = Factory.New<ForwardingShipment>();
			shipment1.JS_ActualWeight = 10m;
			shipment1.JS_ActualVolume = 7m;
			shipment1.JS_UnitOfWeight = Constants.Weight.Kilograms;
			shipment1.JS_UnitOfVolume = Constants.Volume.CubicMetres;

			var shipment2 = Factory.New<ForwardingShipment>();
			shipment2.JS_ActualWeight = 20m;
			shipment2.JS_ActualVolume = 8m;
			shipment2.JS_UnitOfWeight = Constants.Weight.Kilograms;
			shipment2.JS_UnitOfVolume = Constants.Volume.CubicMetres;

			consol.Shipments.Add(shipment1);
			consol.Shipments.Add(shipment2);

			AssertEquals(consol.JK_CorrectedConsolWeight, 30m);
			AssertEquals(consol.JK_Calc_ActualVolumeWeight, (ZDecimal)2500);

			var expectedDensityValues = new DensityValues(83.3333m, "1:12", "Volume +++++");
			AssertDensities("Test", expectedDensityValues, consol.Density);

			shipment1.JS_ActualWeight = 40m;
			shipment2.JS_ActualWeight = 40m;

			expectedDensityValues = new DensityValues(31.25m, "1:12", "Volume +++++");
			AssertDensities("Test", expectedDensityValues, consol.Density);

			shipment1.JS_ActualWeight = 2000m;
			shipment2.JS_ActualWeight = 2000m;

			expectedDensityValues = new DensityValues(0.625m, "1:4", "Dense +");
			AssertDensities("Test", expectedDensityValues, consol.Density);

			shipment1.JS_ActualWeight = 200m;
			shipment1.JS_ActualVolume = 1m;
			shipment2.JS_ActualWeight = 200m;
			shipment2.JS_ActualVolume = 1m;

			expectedDensityValues = new DensityValues(0.8333m, "1:5", "Dense");
			AssertDensities("Test", expectedDensityValues, consol.Density);
		}

		public void TestVolumeUtilisationPercentage()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TotalShipmentActVolumeCheck = 100m;
			consol.VolumeVerificationUnit = Constants.Volume.CubicMetres;

			var shipment1 = Factory.New<ForwardingShipment>();
			shipment1.JS_ActualVolume = 10m;
			shipment1.JS_UnitOfVolume = Constants.Volume.CubicMetres;

			var shipment2 = Factory.New<ForwardingShipment>();
			shipment2.JS_ActualVolume = 20m;
			shipment2.JS_UnitOfVolume = Constants.Volume.CubicMetres;

			consol.Shipments.Add(shipment1);
			consol.Shipments.Add(shipment2);

			AssertEquals("Volume Utilisation Percentage is 30%", consol.Density.VolumeUtilisationPercentage, 30m);
		}

		public void TestVolumeUtilisationPercentage_ZeroValues()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TotalShipmentActVolumeCheck = 0m;
			consol.VolumeVerificationUnit = Constants.Volume.CubicMetres;

			var shipment1 = Factory.New<ForwardingShipment>();
			shipment1.JS_ActualVolume = 10m;
			shipment1.JS_UnitOfVolume = Constants.Volume.CubicMetres;

			var shipment2 = Factory.New<ForwardingShipment>();
			shipment2.JS_ActualVolume = 20m;
			shipment2.JS_UnitOfVolume = Constants.Volume.CubicMetres;

			consol.Shipments.Add(shipment1);
			consol.Shipments.Add(shipment2);

			AssertEquals("Volume Utilisation Percentage cannot be calculated so is 0%", consol.Density.VolumeUtilisationPercentage, 0m);
		}

		public void TestVolumeUtilisationPercentage_UpdatesThroughSubscription()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TotalShipmentActVolumeCheck = 100m;
			consol.VolumeVerificationUnit = Constants.Volume.CubicMetres;

			var shipment1 = Factory.New<ForwardingShipment>();
			shipment1.JS_ActualVolume = 10m;
			shipment1.JS_UnitOfVolume = Constants.Volume.CubicMetres;

			var shipment2 = Factory.New<ForwardingShipment>();
			shipment2.JS_ActualVolume = 20m;
			shipment2.JS_UnitOfVolume = Constants.Volume.CubicMetres;

			consol.Shipments.Add(shipment1);
			consol.Shipments.Add(shipment2);

			Factory.Save();

			var hasChangesFlag = false;

			consol.Density.VolumeUtilisationPercentageInfo.ValueChanged += (sender, e) => hasChangesFlag = true;

			shipment1.JS_ActualVolume = 20m;
			AssertEquals(true, hasChangesFlag);
			AssertEquals("Volume Utilisation Percentage is 40%", 40m, consol.Density.VolumeUtilisationPercentage);

			hasChangesFlag = false;

			shipment2.JS_ActualVolume = 50m;
			AssertEquals(true, hasChangesFlag);
			AssertEquals("Volume Utilisation Percentage is 70%", 70m, consol.Density.VolumeUtilisationPercentage);

			hasChangesFlag = false;

			shipment1.JS_ActualVolume = 5m;
			AssertEquals(true, hasChangesFlag);
			AssertEquals("Volume Utilisation Percentage is 40%", 55m, consol.Density.VolumeUtilisationPercentage);

			hasChangesFlag = false;

			shipment2.JS_ActualVolume = 65m;
			AssertEquals(true, hasChangesFlag);
			AssertEquals("Volume Utilisation Percentage is 70%", 70m, consol.Density.VolumeUtilisationPercentage);
		}

		public void TestVolumeUtilisationPercentage_UnitConversion()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TotalShipmentActVolumeCheck = 0.1m;
			consol.VolumeVerificationUnit = Constants.Volume.CubicMetres;

			var shipment1 = Factory.New<ForwardingShipment>();
			shipment1.JS_ActualVolume = 10m;
			shipment1.JS_UnitOfVolume = Constants.Volume.Litre;

			var shipment2 = Factory.New<ForwardingShipment>();
			shipment2.JS_ActualVolume = 20m;
			shipment2.JS_UnitOfVolume = Constants.Volume.Litre;

			consol.Shipments.Add(shipment1);
			consol.Shipments.Add(shipment2);

			Factory.Save();

			AssertEquals("Volume Utilisation Percentage is 30%", consol.Density.VolumeUtilisationPercentage, 30m);
		}

		public void TestWeightUtilisationPercentage()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TotalShipmentActWeightCheck = 100m;
			consol.WeightVerificationUnit = Constants.Weight.Kilograms;

			var shipment1 = Factory.New<ForwardingShipment>();
			shipment1.JS_ActualWeight = 10m;
			shipment1.JS_UnitOfWeight = Constants.Weight.Kilograms;

			var shipment2 = Factory.New<ForwardingShipment>();
			shipment2.JS_ActualWeight = 20m;
			shipment2.JS_UnitOfWeight = Constants.Weight.Kilograms;

			consol.Shipments.Add(shipment1);
			consol.Shipments.Add(shipment2);

			AssertEquals("Weight Utilisation Percentage is 30%", consol.Density.WeightUtilisationPercentage, 30m);
		}

		public void TestWeightUtilisationPercentage_ZeroValues()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TotalShipmentActWeightCheck = 0m;
			consol.WeightVerificationUnit = Constants.Weight.Kilograms;

			var shipment1 = Factory.New<ForwardingShipment>();
			shipment1.JS_ActualWeight = 10m;
			shipment1.JS_UnitOfWeight = Constants.Weight.Kilograms;

			var shipment2 = Factory.New<ForwardingShipment>();
			shipment2.JS_ActualWeight = 20m;
			shipment2.JS_UnitOfWeight = Constants.Weight.Kilograms;

			consol.Shipments.Add(shipment1);
			consol.Shipments.Add(shipment2);

			AssertEquals("Weight Utilisation Percentage cannot be calculated so is 0%", 0m, consol.Density.WeightUtilisationPercentage);
		}

		public void TestWeightUtilisationPercentage_UpdatesThroughSubscription()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TotalShipmentActWeightCheck = 100m;
			consol.WeightVerificationUnit = Constants.Weight.Kilograms;

			var shipment1 = Factory.New<ForwardingShipment>();
			shipment1.JS_ActualWeight = 10m;
			shipment1.JS_UnitOfWeight = Constants.Weight.Kilograms;

			var shipment2 = Factory.New<ForwardingShipment>();
			shipment2.JS_ActualWeight = 20m;
			shipment2.JS_UnitOfWeight = Constants.Weight.Kilograms;

			consol.Shipments.Add(shipment1);
			consol.Shipments.Add(shipment2);

			Factory.Save();

			var hasChangesFlag = false;

			consol.Density.WeightUtilisationPercentageInfo.ValueChanged += (sender, e) => hasChangesFlag = true;

			shipment1.JS_ActualWeight = 20m;
			AssertEquals(true, hasChangesFlag);
			AssertEquals("Weight Utilisation Percentage is 40%", 40m, consol.Density.WeightUtilisationPercentage);

			hasChangesFlag = false;

			shipment2.JS_ActualWeight = 50m;
			AssertEquals(true, hasChangesFlag);
			AssertEquals("Weight Utilisation Percentage is 70%", 70m, consol.Density.WeightUtilisationPercentage);

			hasChangesFlag = false;

			shipment1.JS_ActualWeight = 5m;
			AssertEquals(true, hasChangesFlag);
			AssertEquals("Weight Utilisation Percentage is 40%", 55m, consol.Density.WeightUtilisationPercentage);

			hasChangesFlag = false;

			shipment2.JS_ActualWeight = 65m;
			AssertEquals(true, hasChangesFlag);
			AssertEquals("Weight Utilisation Percentage is 70%", 70m, consol.Density.WeightUtilisationPercentage);
		}

		public void TestWeightUtilisationPercentage_UnitConversion()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TotalShipmentActWeightCheck = 0.1m;
			consol.WeightVerificationUnit = Constants.Weight.Kilograms;

			var shipment1 = Factory.New<ForwardingShipment>();
			shipment1.JS_ActualWeight = 10m;
			shipment1.JS_UnitOfWeight = Constants.Weight.Grams;

			var shipment2 = Factory.New<ForwardingShipment>();
			shipment2.JS_ActualWeight = 20m;
			shipment2.JS_UnitOfWeight = Constants.Weight.Grams;

			consol.Shipments.Add(shipment1);
			consol.Shipments.Add(shipment2);

			AssertEquals("Weight Utilisation Percentage is 30%", consol.Density.WeightUtilisationPercentage, 30m);
		}

		public void TestExcessVolumeWeight_Weight()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_OverrideConsolChargeable = true;

			consol.JK_CorrectedConsolWeight = 30m;
			consol.JK_CorrectedConsolWeightUnit = Constants.Weight.Kilograms;

			consol.JK_CorrectedConsolVolume = 10m;
			consol.JK_CorrectedConsolVolumeUnit = Constants.Volume.Litre;

			AssertEquals("Excess Weight is correct value", consol.Density.ExcessVolumeWeight, 28.333333m);
		}

		public void TestExcessVolumeWeight_Volume()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_OverrideConsolChargeable = true;

			consol.JK_CorrectedConsolVolume = 30m;
			consol.JK_CorrectedConsolVolumeUnit = Constants.Volume.Litre;

			consol.JK_CorrectedConsolWeight = 10m;
			consol.JK_CorrectedConsolWeightUnit = Constants.Weight.Kilograms;

			AssertEquals("Excess Volume is correct value", consol.Density.ExcessVolumeWeight, 20m);
		}

		public void TestExcessVolumeWeight_UpdatesThroughSubscription()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;

			consol.JK_CorrectedConsolWeightUnit = Constants.Weight.Kilograms;

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ActualWeight = 30m;
			shipment.JS_UnitOfWeight = Constants.Weight.Kilograms;
			shipment.JS_ActualVolume = 10m;
			shipment.JS_UnitOfVolume = Constants.Volume.Litre;

			consol.Shipments.Add(shipment);

			AssertEquals("Excess Weight is correct value", consol.Density.ExcessVolumeWeight, 28.333333m);

			var hasChangedFlag = false;
			consol.Density.ExcessVolumeWeightInfo.ValueChanged += (sender, e) => hasChangedFlag = true;

			shipment.JS_ActualWeight = 40m;

			AssertEquals(true, hasChangedFlag);
			AssertEquals("Excess Weight has been updated", consol.Density.ExcessVolumeWeight, 38.333333m);
		}

		void AssertDensities(string message, DensityValues expected, ConsolDensity result)
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
			return new ConsolDensity(Factory.New<ForwardingConsol>());
		}

		#endregion
	}
}
