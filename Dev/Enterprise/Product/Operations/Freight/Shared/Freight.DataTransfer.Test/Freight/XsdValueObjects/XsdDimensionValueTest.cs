using CargoWise.EntityFramework.Testing;
using Enterprise.Core;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.ZArchitecture;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Freight.DataTransfer.Testing
{
	sealed class XsdDimensionValueTest : TestCaseWithDummy
	{
		public void TestMetricWeightConversion()
		{
			AssertWeightConversion(1500000m, Core.Constants.Weight.Grams, 1500m, Core.Constants.Weight.Kilograms);
			AssertWeightConversion(1500000m, Core.Constants.Weight.Kilograms, 1500m, Core.Constants.Weight.Tonnes);
			AssertWeightConversion(999999999000.000m, Core.Constants.Weight.Grams, 999999.999m, Core.Constants.Weight.Tonnes);
			AssertWeightConversion(999999999000000.000m, Core.Constants.Weight.Grams, 999999.999m, Core.Constants.Weight.Kilotonnes);
			AssertFailedWeightConversion(1000000000000000.000m, Core.Constants.Weight.Grams, 999999.999m, Core.Constants.Weight.Kilotonnes);
		}

		public void TestUSWeightConversion()
		{
			AssertWeightConversion(1600000m, Core.Constants.Weight.Ounces, 100000m, Core.Constants.Weight.Pounds);
			AssertWeightConversion(1600000m, Core.Constants.Weight.Pounds, 800m, Core.Constants.Weight.ShortTons);

			AssertWeightConversion(1200000m, Core.Constants.Weight.OuncesTroy, 100000m, Core.Constants.Weight.PoundsTroy);
			AssertWeightConversion(1750000m, Core.Constants.Weight.PoundsTroy, 720m, Core.Constants.Weight.ShortTons);
			AssertWeightConversion(1008000m, Core.Constants.Weight.ShortTons, 900000.253m, Core.Constants.Weight.LongTons);
		}

		public void TestMetricVolumeConversion()
		{
			AssertVolumeConversion(1500000m, Core.Constants.Volume.CubicDecimetres, 1500m, Core.Constants.Volume.CubicMetres);
			AssertVolumeConversion(1500000m, Core.Constants.Volume.CubicMetres, 1500m, Core.Constants.Volume.MegaLitre);
			AssertVolumeConversion(1500000000m, Core.Constants.Volume.CubicDecimetres, 1500m, Core.Constants.Volume.MegaLitre);
			AssertFailedVolumeConversion(1000000000000m, Core.Constants.Volume.CubicDecimetres, 999999.999m, Core.Constants.Volume.MegaLitre);
		}

		public void TestUSVolumeConversion()
		{
			AssertVolumeConversion(1728000m, Core.Constants.Volume.CubicInches, 1000m, Core.Constants.Volume.CubicFeet);
			AssertVolumeConversion(2700000m, Core.Constants.Volume.CubicFeet, 100000m, Core.Constants.Volume.CubicYards);
			AssertVolumeConversion(4665600000m, Core.Constants.Volume.CubicInches, 100000m, Core.Constants.Volume.CubicYards);
		}

		#region Implementation

		void AssertFailedWeightConversion(decimal startWeight, string startUnit, decimal maxWeight, string maxUnit)
		{
			string format = "{0:0.000} {1}";

			Xsd.DimensionValue dim = new Xsd.DimensionValue();
			dim.Value = startWeight;
			dim.DimensionType = startUnit;

			Dummy.Z0_Decimal = 0;
			Dummy.Z0_FK_Code = "";

			NotificationBuffer buffer = new NotificationBuffer();
			XsdDimensionValue.ImportWeight(Dummy.Z0_DecimalInfo, Dummy.Z0_FK_CodeInfo, dim, 9, 3, new ValueObjectImportContext(Factory, buffer), "error context");

			AssertEquals("expected notification",
				string.Format("Error: Value overflow error (error context; value = {0}{1}, max = {2}{1})\r\n", Constants.Weight.Convert(startWeight, startUnit, maxUnit), maxUnit, maxWeight),
				buffer.AsString);

			AssertEquals(
				"should not have changed the existing values",
				"0.000 ",
				string.Format(format, Dummy.Z0_Decimal, Dummy.Z0_FK_Code));
		}

		void AssertWeightConversion(decimal startWeight, string startUnit, decimal expectedWeight, string expectedUnit)
		{
			string format = "{0:0.000} {1}";

			Xsd.DimensionValue dim = new Xsd.DimensionValue();
			dim.Value = startWeight;
			dim.DimensionType = startUnit;

			Dummy.Z0_AnotherDecimal = 0;
			Dummy.Z0_FK_Code = "";

			NotificationBuffer buffer = new NotificationBuffer();
			XsdDimensionValue.ImportWeight(Dummy.Z0_AnotherDecimalInfo, Dummy.Z0_FK_CodeInfo, dim, 9, 3, new ValueObjectImportContext(Factory, buffer), "error context");

			AssertEquals("should not report any notifications", "", buffer.AsString);
			AssertEquals(
				"should have the desired result",
				string.Format(format, expectedWeight, expectedUnit),
				string.Format(format, Dummy.Z0_AnotherDecimal, Dummy.Z0_FK_Code));
		}

		void AssertFailedVolumeConversion(decimal startVolume, string startUnit, decimal maxVolume, string maxUnit)
		{
			string format = "{0:0.000} {1}";

			Xsd.DimensionValue dim = new Xsd.DimensionValue();
			dim.Value = startVolume;
			dim.DimensionType = startUnit;

			Dummy.Z0_Decimal = 0;
			Dummy.Z0_FK_Code = "";

			NotificationBuffer buffer = new NotificationBuffer();
			XsdDimensionValue.ImportVolume(Dummy.Z0_DecimalInfo, Dummy.Z0_FK_CodeInfo, dim, 9, 3, new ValueObjectImportContext(Factory, buffer), "error context");

			AssertEquals("expected notification",
				string.Format("Error: Value overflow error (error context; value = {0}{1}, max = {2}{1})\r\n", Constants.Volume.Convert(startVolume, startUnit, maxUnit), maxUnit, maxVolume),
				buffer.AsString);

			AssertEquals(
				"should not have changed the existing values",
				"0.000 ",
				string.Format(format, Dummy.Z0_Decimal, Dummy.Z0_FK_Code));
		}

		void AssertVolumeConversion(decimal startVolume, string startUnit, decimal expectedVolume, string expectedUnit)
		{
			string format = "{0:0.000} {1}";

			Xsd.DimensionValue dim = new Xsd.DimensionValue();
			dim.Value = startVolume;
			dim.DimensionType = startUnit;

			Dummy.Z0_Decimal = 0;
			Dummy.Z0_FK_Code = "";

			NotificationBuffer buffer = new NotificationBuffer();
			XsdDimensionValue.ImportVolume(Dummy.Z0_DecimalInfo, Dummy.Z0_FK_CodeInfo, dim, 9, 3, new ValueObjectImportContext(Factory, buffer), "error context");

			AssertEquals("should not report any notifications", "", buffer.AsString);
			AssertEquals(
				"should have the desired result",
				string.Format(format, expectedVolume, expectedUnit),
				string.Format(format, Dummy.Z0_Decimal, Dummy.Z0_FK_Code));
		}

		#endregion
	}
}
