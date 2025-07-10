using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.AWB.Business;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.AWB.Testing
{
	[TestedType(typeof(NatureAndQtyOfGoodsVolume))]
	sealed class NatureAndQtyOfGoodsVolumeTest : Forwarding.AWB.Business.Testing.NatureAndQtyOfGoodsTest
	{
		public override void TestText()
		{
			AssertSerializationForRoundedVolume();

			OverrideDecimalRegistryValueForTest();

			AssertSerialization_WithOverriddenDecimal();
			AssertDeserialization_WithOverriddenDecimal();
		}

		public override void TestTextSize()
		{
			NatureAndQtyOfGoodsVolume volume = (NatureAndQtyOfGoodsVolume)GetNewBusinessObject();
			volume.Volume = 99999.99;

			int longestUnit = Core.Constants.Volume.Codes.Max((unit) => unit.Length);
			volume.Unit = Core.Constants.Volume.Codes
				.Where((unit) => unit.Length == longestUnit)
				.First();

			AssertEquals("prerequisite", false, volume.HasErrors);
			Assert("max volume serialized", volume.Text.Length < 36);
		}

		public void AssertSerialization_WithOverriddenDecimal()
		{
			var volume = (NatureAndQtyOfGoodsVolume)GetNewBusinessObject();
			AssertEquals("VOL 0.00", volume.Text.ToString());

			volume.Volume = 11.0;
			volume.Unit = Constants.Volume.CubicMetres;
			AssertEquals("VOL 11.000 M3", volume.Text.ToString());
		}

		public void AssertDeserialization_WithOverriddenDecimal()
		{
			var volume = (NatureAndQtyOfGoodsVolume)GetNewBusinessObject();
			AssertEquals(0m, volume.Volume);
			AssertEquals(ZString.Empty, volume.Unit);

			volume.Text = "VOL 11.00 M3";
			AssertEquals(11.00m, volume.Volume);
			AssertEquals(Constants.Volume.CubicMetres, volume.Unit);
		}

		public void TestVolumeRoundedToUnit()
		{
			OverrideDecimalRegistryValueForTest();

			var volume = (NatureAndQtyOfGoodsVolume)GetNewBusinessObject();
			volume.Volume = 1.234m;
			AssertEquals(1.24m, volume.Volume);

			volume.Unit = "M3";
			volume.Volume = 1.234m;
			AssertEquals(1.234m, volume.Volume);
		}

		void OverrideDecimalRegistryValueForTest()
		{
			var collection = new DefaultNumberOfDecimalsCollection(Module.Freight);
			var defaultNumberOfDecimals_AWBWeight = collection.AddNew();
			defaultNumberOfDecimals_AWBWeight.TransportMode = Core.Constants.TransportModes.Air;
			defaultNumberOfDecimals_AWBWeight.UnitOfMeasure = Core.Constants.Volume.CubicMetres;
			defaultNumberOfDecimals_AWBWeight.NumberOfDecimals = 3;
			defaultNumberOfDecimals_AWBWeight.RoundingMode = RoundingModes.Up;
			FreightConfigurationRegistry.Instance.DefaultNumberOfDecimalPlaces.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
			FreightConfigurationRegistry.Instance.UseFreightNumberOfDecimalPlacesForAWBWeightAndVolume.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
		}

		public void AssertSerializationForRoundedVolume()
		{
			var collection = new DefaultNumberOfDecimalsCollection(Module.Freight);
			var defaultNumberOfDecimals_AWBWeight = collection.AddNew();
			defaultNumberOfDecimals_AWBWeight.TransportMode = Core.Constants.TransportModes.Air;
			defaultNumberOfDecimals_AWBWeight.UnitOfMeasure = Core.Constants.Volume.CubicMetres;
			defaultNumberOfDecimals_AWBWeight.NumberOfDecimals = 3;
			defaultNumberOfDecimals_AWBWeight.RoundingMode = RoundingModes.Up;

			using (FreightConfigurationRegistry.Instance.UseFreightNumberOfDecimalPlacesForAWBWeightAndVolume.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var volume = (NatureAndQtyOfGoodsVolume)GetNewBusinessObject();
				volume.Unit = Constants.Volume.CubicMetres;
				volume.Volume = 1.234m;
				AssertEquals(1.234m, volume.Volume);
			}

			using (FreightConfigurationRegistry.Instance.DefaultNumberOfDecimalPlaces.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection))
			using (FreightConfigurationRegistry.Instance.UseFreightNumberOfDecimalPlacesForAWBWeightAndVolume.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var volume = (NatureAndQtyOfGoodsVolume)GetNewBusinessObject();
				volume.Unit = Constants.Volume.CubicMetres;
				volume.Volume = 1.234m;
				AssertEquals(1.234m, volume.Volume);
			}

			using (FreightConfigurationRegistry.Instance.UseFreightNumberOfDecimalPlacesForAWBWeightAndVolume.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var volume = (NatureAndQtyOfGoodsVolume)GetNewBusinessObject();
				volume.Unit = Constants.Volume.CubicMetres;
				volume.Volume = 1.234m;
				AssertEquals(1.24m, volume.Volume);
			}

			using (FreightConfigurationRegistry.Instance.DefaultNumberOfDecimalPlaces.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection))
			using (FreightConfigurationRegistry.Instance.UseFreightNumberOfDecimalPlacesForAWBWeightAndVolume.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var volume = (NatureAndQtyOfGoodsVolume)GetNewBusinessObject();
				volume.Unit = Constants.Volume.CubicMetres;
				volume.Volume = 1.234m;
				AssertEquals(1.24m, volume.Volume);
			}
		}

		#region Implementation

		public override Type ExpectedValidationType
		{
			get { return typeof(NatureAndQtyOfGoodsVolumeValidation); }
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new NatureAndQtyOfGoodsVolume(Factory.New<ExportAWBRateLine>());
		}

		#endregion
	}
}
