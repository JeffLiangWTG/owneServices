using CargoWise.EntityFramework.Testing;
using Enterprise.eTail.Business;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Services.ServiceHost.Tests
{
	class ETailDensityPropertiesProviderControllerTest : TestCaseWithFactory
	{
		public void TestDensityProperties_HVLVConsignment()
		{
			var consignment = PrepareConsignment();

			var item1 = consignment.Items.AddNew();
			item1.HVI_ManifestedWeight = 500;
			item1.HVI_ManifestedVolume = 1;

			var item2 = consignment.Items.AddNew();
			item2.HVI_ManifestedWeight = 1000;
			item2.HVI_ManifestedVolume = 2;

			Factory.Save();

			CombineAssertions("Should return correct consignment density values", () =>
			{
				AssertEquals("Should return correct chargeable", "3.000 M3", controller.GetChargeable(consignment.TablePrefix, consignment.PK.ToGuid()));
				AssertEquals("Should return correct volume weight", "1.5 M3", controller.GetVolumeWeight(consignment.TablePrefix, consignment.PK.ToGuid()));
				AssertEquals("Should return correct density factor", "2.00", controller.GetDensityFactor(consignment.TablePrefix, consignment.PK.ToGuid()));
			});
		}

		public void TestDensityProperties_HVLVItem()
		{
			var consignment = PrepareConsignment();

			var item1 = consignment.Items.AddNew();
			item1.HVI_ManifestedWeight = 300;
			item1.HVI_ManifestedVolume = 1;

			Factory.Save();

			CombineAssertions("Should return correct item density values", () =>
			{
				AssertEquals("Should return correct chargeable", "1.000 M3", controller.GetChargeable(item1.TablePrefix, item1.PK.ToGuid()));
				AssertEquals("Should return correct volume weight", "0.3 M3", controller.GetVolumeWeight(item1.TablePrefix, item1.PK.ToGuid()));
				AssertEquals("Should return correct density factor", "3.33", controller.GetDensityFactor(item1.TablePrefix, item1.PK.ToGuid()));
			});
		}

		public void TestDensityProperties_InvalidInput()
		{
			CombineAssertions("Should return empty string with invalid input", () =>
			{
				AssertEquals("Invalid table prefix", string.Empty, controller.GetChargeable("", System.Guid.NewGuid()));
				AssertEquals("Invalid PK", string.Empty, controller.GetVolumeWeight("HVC", entityPK: System.Guid.Empty));
			});
		}

		HVLVConsignment PrepareConsignment()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.HighVolumeLowValue;

			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;

			consignment.HVC_WeightUQ = Core.Constants.Weight.Kilograms;
			consignment.HVC_VolumeUQ = Core.Constants.Volume.CubicMetres;

			return consignment;
		}

		protected override void SetUp()
		{
			base.SetUp();
			controller = new ETailDensityPropertiesProviderController();
		}

		ETailDensityPropertiesProviderController controller;
	}
}
