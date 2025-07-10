using System.Reflection;
using CargoWise.EntityFramework;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Business.Testing;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.eTail.Business.Testing
{
	[TestedType(typeof(HVLVConsignmentDensity))]
	public class HVLVConsignmentDensityTest : BaseDensityTest
	{
		public void TestIsRefreshAllowed()
		{
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			var density = new HVLVConsignmentDensity(consignment);

			var refreshAllowedProperty = density.GetType().GetProperty("IsRefreshAllowed", BindingFlags.Instance | BindingFlags.NonPublic);
			AssertEquals("IsRefreshAllowed should be set to true", true, refreshAllowedProperty.GetValue(density));
		}

		public void TestTotalWeightAndVolumeIsCorrectlyConvertedToChargableUQ()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = TransportModes.Air;
			shipment.JS_UniqueConsignRef = "S0001016";

			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_WeightUQ = Weight.Grams;
			consignment.HVC_VolumeUQ = Volume.CubicDecimetres;
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;

			var item = consignment.Items.AddNew();
			item.HVI_ManifestedWeight = 1000;
			item.HVI_ActualWeight = 1000;
			item.HVI_ManifestedVolume = 1000;
			item.HVI_ActualVolume = 1000;

			var density = new HVLVConsignmentDensity(consignment);
			var densityTotalWeight = density.GetType().GetProperty("TotalWeight", BindingFlags.Instance | BindingFlags.NonPublic);
			var densityTotalVolume = density.GetType().GetProperty("TotalVolume", BindingFlags.Instance | BindingFlags.NonPublic);

			AssertEquals("Weight 1000g should be converted to 1kg when charged by weight", 1m, densityTotalWeight.GetValue(density));
			AssertEquals("Volume 1000dm3 should not be converted when charged by weight", 1000m, densityTotalVolume.GetValue(density));

			shipment.JS_TransportMode = TransportModes.Sea;
			AssertEquals("Weight 1000g should not be converted when charged by volume", 1000m, densityTotalWeight.GetValue(density));
			AssertEquals("Volume 1000dm3 should be converted to 1m3 when charged by volume", 1m, densityTotalVolume.GetValue(density));
		}

		public void TestDensityCalculation_ShouldIncludeUQConversion()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = TransportModes.Air;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "GBEDI";

			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment.HVC_WeightUQ = Weight.Grams;
			consignment.HVC_VolumeUQ = Volume.CubicMetres;

			var item = consignment.Items.AddNew();
			item.HVI_ManifestedWeight = 100;
			item.HVI_ActualWeight = 100;
			item.HVI_ManifestedVolume = 0.006;
			item.HVI_ActualVolume = 0.006;

			AssertEquals("Density should be 10 for 100g/0.006m3(Total Weight = 0.1kg, Volume Weight = 1kg)", 10m, consignment.DensityFactor);

			consignment.HVC_WeightUQ = Weight.Kilograms;
			AssertEquals("Density should be updated to 0.01 when changing weight UQ to kg (100kg/0.006m3)(Total Weight = 100kg, Volume Weight = 1kg)", 0.01m, consignment.DensityFactor);

			item.HVI_ActualWeight = 0.1;
			AssertEquals("Density should be updated to 10 when changing weight value to 0.1 (0.1kg/0.006m3)(Total Weight = 0.1kg, Volume Weight = 1kg)", 10m, consignment.DensityFactor);

			shipment.JS_TransportMode = TransportModes.Sea;
			item.HVI_ActualWeight = 1;
			AssertEquals("Density should be 6 for 1kg/0.006m3(Total Volume = 0.006m3, Volume Weight = 0.001m3)", 6m, consignment.DensityFactor);

			item.HVI_ActualVolume = 6;
			AssertEquals("Density should be updated to 6000 when changing volume value to 6 (1kg/6m3)(Total Volume = 6m3, Volume Weight = 0.001m3)", 6000m, consignment.DensityFactor);

			consignment.HVC_VolumeUQ = Volume.CubicDecimetres;
			AssertEquals("Density should be updated to 6 when changing volume UQ to dm3 (100kg/6dm3)(Total Volume = 0.006m3, Volume Weight = 0.001m3)", 6m, consignment.DensityFactor);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new HVLVConsignmentDensity(Factory.New<HVLVConsignment>());
		}
	}
}
