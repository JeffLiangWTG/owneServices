using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.Business.Testing
{
	public class PackLinerDefaultNumberOfDecimalsSupporterTest : DefaultNumberOfDecimalsSupporterForFreightTest
	{
		public void TestRoundingWhenDefaultNumberOfDecimalsChange_Length()
		{
			FreightConfigurationRegistry.Instance.DefaultNumberOfDecimalPlaces.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new DefaultNumberOfDecimalsCollection(Module.Freight));

			var shipment = Factory.New<CommonShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;

			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_UnitOfDimension = Core.Constants.Length.Metres;

			packLine.JL_Length = 4.231m;
			packLine.JL_Height = 3.141m;
			packLine.JL_Width = 1.359m;

			packLine.JL_OutturnedLength = 3.266m;
			packLine.JL_OutturnedHeight = 4.235m;
			packLine.JL_OutturnedWidth = 1.364m;

			AssertEquals(4.231m, packLine.JL_Length);
			AssertEquals(3.141m, packLine.JL_Height);
			AssertEquals(1.359m, packLine.JL_Width);

			AssertEquals(3.266m, packLine.JL_OutturnedLength);
			AssertEquals(4.235m, packLine.JL_OutturnedHeight);
			AssertEquals(1.364m, packLine.JL_OutturnedWidth);

			var collection = new DefaultNumberOfDecimalsCollection(Module.Freight);

			var defaultNumberOfDecimals_SeaLength = collection.AddNew();
			defaultNumberOfDecimals_SeaLength.UnitOfMeasure = Constants.Length.Metres;
			defaultNumberOfDecimals_SeaLength.TransportMode = Constants.TransportModes.Sea;
			defaultNumberOfDecimals_SeaLength.NumberOfDecimals = 2;
			defaultNumberOfDecimals_SeaLength.RoundingMode = RoundingModes.Up;

			using (FreightConfigurationRegistry.Instance.DefaultNumberOfDecimalPlaces.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection))
			{
				shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;

				AssertEquals(4.24m, packLine.JL_Length);
				AssertEquals(3.15m, packLine.JL_Height);
				AssertEquals(1.36m, packLine.JL_Width);

				AssertEquals(3.27m, packLine.JL_OutturnedLength);
				AssertEquals(4.24m, packLine.JL_OutturnedHeight);
				AssertEquals(1.37m, packLine.JL_OutturnedWidth);
			}
		}

		public override void TestRoundingWhenDefaultNumberOfDecimalsChange()
		{
			FreightConfigurationRegistry.Instance.DefaultNumberOfDecimalPlaces.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new DefaultNumberOfDecimalsCollection(Module.Freight));

			var shipment = Factory.New<CommonShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;

			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_ActualVolumeUQ = Core.Constants.Volume.CubicMetres;
			packLine.JL_ActualWeightUQ = Core.Constants.Weight.Kilograms;

			packLine.JL_ActualWeight = 33.121m;
			packLine.JL_PackageCount = 1;
			packLine.JL_Length = 4.23m;
			packLine.JL_Height = 3.14m;
			packLine.JL_Width = 1.35m;
			packLine.JL_ActualVolume = 5.369m;
			packLine.JL_Outturn = 1;
			packLine.JL_OutturnedLength = 3.26m;
			packLine.JL_OutturnedHeight = 4.23m;
			packLine.JL_OutturnedWidth = 1.36m;
			packLine.JL_OutturnedVolume = 3.128m;
			packLine.JL_OutturnedWeight = 35.462m;

			AssertEquals(5.369m, packLine.JL_ActualVolume);
			AssertEquals(5.369m, packLine.JL_Calc_VolumeToDeliver);
			AssertEquals(17.931m, packLine.CalculatedVolume);
			AssertEquals(18.754m, packLine.CalculatedOutturnedVolume);

			AssertEquals(33.121m, packLine.JL_ActualWeight);
			AssertEquals(33.121m, packLine.JL_Calc_WeightToDeliver);

			AssertEquals(3.128m, packLine.JL_OutturnedVolume);
			AssertEquals(35.462m, packLine.JL_OutturnedWeight);

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

			using (FreightConfigurationRegistry.Instance.DefaultNumberOfDecimalPlaces.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection))
			{
				shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;

				AssertEquals(5.36m, packLine.JL_ActualVolume);
				AssertEquals(5.36m, packLine.JL_Calc_VolumeToDeliver);
				AssertEquals(17.93m, packLine.CalculatedVolume);
				AssertEquals(18.75m, packLine.CalculatedOutturnedVolume);

				AssertEquals(33.13m, packLine.JL_ActualWeight);
				AssertEquals(33.13m, packLine.JL_Calc_WeightToDeliver);

				AssertEquals(3.12m, packLine.JL_OutturnedVolume);
				AssertEquals(35.47m, packLine.JL_OutturnedWeight);
			}
		}

		public void TestRoundingWithDefaultNumberOfDecimals_ShouldOverrideDefault_CalculatedVolume()
		{
			var shipment = Factory.New<CommonShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;

			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_UnitOfDimension = Core.Constants.Length.Metres;

			packLine.JL_PackageCount = 5;
			packLine.JL_Length = 1.22m;
			packLine.JL_Height = 1.46m;
			packLine.JL_Width = 1.05m;

			// 5 x 1.22 x 1.46 x 1.05 = 9.3513
			AssertEquals("Precondition: Calculated Volume should be rounded using default bankers rounding", 9.351m, packLine.CalculatedVolume);

			var collection = new DefaultNumberOfDecimalsCollection(Module.Freight);

			var defaultNumberOfDecimals = collection.AddNew();
			defaultNumberOfDecimals.UnitOfMeasure = Constants.Volume.CubicMetres;
			defaultNumberOfDecimals.TransportMode = Constants.TransportModes.Air;
			defaultNumberOfDecimals.NumberOfDecimals = 3;
			defaultNumberOfDecimals.RoundingMode = RoundingModes.Up;

			using (FreightConfigurationRegistry.Instance.DefaultNumberOfDecimalPlaces.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection))
			{
				AssertEquals("Calculated Volume should be rounded up", 9.352m, packLine.CalculatedVolume);
			}
		}

		public void TestRoundingWithDefaultNumberOfDecimals_ShouldOverrideDefault_CalculatedOutturnedVolume()
		{
			var shipment = Factory.New<CommonShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;

			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_UnitOfDimension = Core.Constants.Length.Metres;

			packLine.JL_Outturn = 5;
			packLine.JL_OutturnedLength = 1.22m;
			packLine.JL_OutturnedHeight = 1.46m;
			packLine.JL_OutturnedWidth = 1.05m;

			// 5 x 1.22 x 1.46 x 1.05 = 9.3513
			AssertEquals("Precondition: Calculated Outturned Volume should be rounded using default bankers rounding", 9.351m, packLine.CalculatedOutturnedVolume);

			var collection = new DefaultNumberOfDecimalsCollection(Module.Freight);

			var defaultNumberOfDecimals = collection.AddNew();
			defaultNumberOfDecimals.UnitOfMeasure = Constants.Volume.CubicMetres;
			defaultNumberOfDecimals.TransportMode = Constants.TransportModes.Air;
			defaultNumberOfDecimals.NumberOfDecimals = 3;
			defaultNumberOfDecimals.RoundingMode = RoundingModes.Up;

			using (FreightConfigurationRegistry.Instance.DefaultNumberOfDecimalPlaces.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection))
			{
				AssertEquals("Calculated Volume should be rounded up", 9.352m, packLine.CalculatedOutturnedVolume);
			}
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			var shipment = Factory.New<CommonShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;

			packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_ActualVolumeUQ = Core.Constants.Volume.CubicMetres;
			packLine.JL_ActualWeightUQ = Core.Constants.Weight.Kilograms;
			packLine.JL_UnitOfDimension = Core.Constants.Length.Metres;
		}

		public override BusinessObject BizObj
		{
			get { return packLine; }
		}
		PackLine packLine;

		public override Dictionary<ZString, ZString> MeasurePropertiesAndUnits
		{
			get
			{
				if (measurePropertiesAndUnits == null)
				{
					measurePropertiesAndUnits = new Dictionary<ZString, ZString>();
					measurePropertiesAndUnits.Add(PackLine.Schema.JL_ActualVolume, PackLine.Schema.JL_ActualVolumeUQ);
					measurePropertiesAndUnits.Add(PackLine.Schema.JL_Calc_VolumeToDeliver, PackLine.Schema.JL_ActualVolumeUQ);
					measurePropertiesAndUnits.Add(PackLine.Schema.CalculatedVolume, PackLine.Schema.JL_ActualVolumeUQ);
					measurePropertiesAndUnits.Add(PackLine.Schema.CalculatedOutturnedVolume, PackLine.Schema.JL_ActualVolumeUQ);

					measurePropertiesAndUnits.Add(PackLine.Schema.JL_ActualWeight, PackLine.Schema.JL_ActualWeightUQ);
					measurePropertiesAndUnits.Add(PackLine.Schema.JL_Calc_WeightToDeliver, PackLine.Schema.JL_ActualWeightUQ);

					measurePropertiesAndUnits.Add(PackLine.Schema.JL_Length, PackLine.Schema.JL_UnitOfDimension);
					measurePropertiesAndUnits.Add(PackLine.Schema.JL_Width, PackLine.Schema.JL_UnitOfDimension);
					measurePropertiesAndUnits.Add(PackLine.Schema.JL_Height, PackLine.Schema.JL_UnitOfDimension);
					measurePropertiesAndUnits.Add(PackLine.Schema.JL_OutturnedLength, PackLine.Schema.JL_UnitOfDimension);
					measurePropertiesAndUnits.Add(PackLine.Schema.JL_OutturnedWidth, PackLine.Schema.JL_UnitOfDimension);
					measurePropertiesAndUnits.Add(PackLine.Schema.JL_OutturnedHeight, PackLine.Schema.JL_UnitOfDimension);
				}

				return measurePropertiesAndUnits;
			}
		}
		Dictionary<ZString, ZString> measurePropertiesAndUnits;

		#endregion
	}
}
