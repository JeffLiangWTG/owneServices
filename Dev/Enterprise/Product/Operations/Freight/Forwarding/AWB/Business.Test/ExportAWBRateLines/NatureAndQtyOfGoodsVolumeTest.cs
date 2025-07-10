using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.AWB.Business.Testing
{
	[TestedType(typeof(NatureAndQtyOfGoodsVolume))]
	sealed class NatureAndQtyOfGoodsVolumeTest : NatureAndQtyOfGoodsTest
	{
		public override void TestText()
		{
			AssertSerialization();
			AssertDeserialization();
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

		void AssertSerialization()
		{
			NatureAndQtyOfGoodsVolume volume = (NatureAndQtyOfGoodsVolume)GetNewBusinessObject();
			AssertEquals("VOL 0.00", volume.Text.ToString());

			volume.Volume = 10;
			volume.Unit = Core.Constants.Volume.CubicFeet;
			AssertEquals("VOL 10.00 CF", volume.Text.ToString());

			volume.Volume = 65.2;
			volume.Unit = Core.Constants.Volume.Litre;
			AssertEquals("VOL 65.20 L", volume.Text.ToString());

			volume.Volume = 99.24;
			volume.Unit = Core.Constants.Volume.TeaChest;
			AssertEquals("VOL 99.24 TE", volume.Text.ToString());
		}

		void AssertDeserialization()
		{
			NatureAndQtyOfGoodsVolume volume = (NatureAndQtyOfGoodsVolume)GetNewBusinessObject();
			AssertEquals(0m, volume.Volume);
			AssertEquals(ZString.Empty, volume.Unit);

			volume.Text = "AAAA";
			AssertEquals(0m, volume.Volume);
			AssertEquals(ZString.Empty, volume.Unit);

			volume.Text = "VOL 23";
			AssertEquals(0m, volume.Volume);
			AssertEquals(ZString.Empty, volume.Unit);

			volume.Text = "VOL M3";
			AssertEquals(0m, volume.Volume);
			AssertEquals(ZString.Empty, volume.Unit);

			volume.Text = "VOL A M3";
			AssertEquals(0m, volume.Volume);
			AssertEquals(ZString.Empty, volume.Unit);

			volume.Text = "VOL .9 M3";
			AssertEquals(0m, volume.Volume);
			AssertEquals(ZString.Empty, volume.Unit);

			volume.Text = "VOL 60 M3";
			AssertEquals(60m, volume.Volume);
			AssertEquals(Core.Constants.Volume.CubicMetres, volume.Unit);

			volume.Text = "VOL 60.23 CF";
			AssertEquals(60.23m, volume.Volume);
			AssertEquals(Core.Constants.Volume.CubicFeet, volume.Unit);

			volume.Text = "VOL 60.230 CF";
			AssertEquals("Must support deserialization for values with 3 decimals", 60.23m, volume.Volume);
			AssertEquals(Core.Constants.Volume.CubicFeet, volume.Unit);

			volume.Text = "vol 10.670 m3";
			AssertEquals(10.670m, volume.Volume);
			AssertEquals("m3", volume.Unit);

			volume.Text = "vol 10.671 m3";
			AssertEquals(10.68m, volume.Volume);
			AssertEquals("m3", volume.Unit);

			volume.Text = "VOL 1234567890 L";
			AssertEquals(0m, volume.Volume);
			AssertEquals(ZString.Empty, volume.Unit);

			volume.Text = "VOL 123456789 L";
			AssertEquals(0m, volume.Volume);
			AssertEquals(ZString.Empty, volume.Unit);

			volume.Text = "VOL 12345678.9 L";
			AssertEquals(0m, volume.Volume);
			AssertEquals(ZString.Empty, volume.Unit);

			volume.Text = "VOL 1234567.8 L";
			AssertEquals(0m, volume.Volume);
			AssertEquals(ZString.Empty, volume.Unit);

			volume.Text = "VOL 123456.78 L";
			AssertEquals(123456.78m, volume.Volume);
			AssertEquals(Core.Constants.Volume.Litre, volume.Unit);

			volume.Text = "VOL 12345.670 L";
			AssertEquals(12345.67m, volume.Volume);
			AssertEquals(Core.Constants.Volume.Litre, volume.Unit);

			var previousCulture = Thread.CurrentThread.CurrentCulture;
			try
			{
				Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture("fr");

				volume.Text = "VOL 12345.670 L";
				AssertEquals(12345.67m, volume.Volume);
				AssertEquals(Core.Constants.Volume.Litre, volume.Unit);

				volume.Text = "VOL 1234.5678 L";
				AssertEquals(0m, volume.Volume);
				AssertEquals(ZString.Empty, volume.Unit);
			}
			finally
			{
				Thread.CurrentThread.CurrentCulture = previousCulture;
			}
		}

		public void TestIsValidVolume()
		{
			AssertEquals(false, NatureAndQtyOfGoodsVolume.IsValidVolume("AAA"));
			AssertEquals(false, NatureAndQtyOfGoodsVolume.IsValidVolume("VOL"));
			AssertEquals(false, NatureAndQtyOfGoodsVolume.IsValidVolume("VOL 10"));

			AssertEquals(true, NatureAndQtyOfGoodsVolume.IsValidVolume("VOL 10 M3"));
			AssertEquals(true, NatureAndQtyOfGoodsVolume.IsValidVolume("VOL 10.2 M3"));
			AssertEquals(true, NatureAndQtyOfGoodsVolume.IsValidVolume("VOL 10.23 M3"));
			AssertEquals(true, NatureAndQtyOfGoodsVolume.IsValidVolume("VOL 10.234 M3"));

			AssertEquals(false, NatureAndQtyOfGoodsVolume.IsValidVolume("VOL 10.2345 M3"));
			AssertEquals(false, NatureAndQtyOfGoodsVolume.IsValidVolume("VOL 123456789 M3"));
			AssertEquals(false, NatureAndQtyOfGoodsVolume.IsValidVolume("VOL 12345678.9 M3"));
			AssertEquals(false, NatureAndQtyOfGoodsVolume.IsValidVolume("VOL 1234567.8 M3"));

			AssertEquals(true, NatureAndQtyOfGoodsVolume.IsValidVolume("VOL 123456.78 M3"));
			AssertEquals(true, NatureAndQtyOfGoodsVolume.IsValidVolume("VOL 12345.678 M3"));

			AssertEquals(false, NatureAndQtyOfGoodsVolume.IsValidVolume("VOL 1234.5678 M3"));
		}

		public void TestSupportsAllUnits()
		{
			NatureAndQtyOfGoodsVolume volume = (NatureAndQtyOfGoodsVolume)GetNewBusinessObject();
			volume.Volume = 1.23;

			foreach (string unit in Core.Constants.Volume.Codes)
			{
				volume.Unit = unit;
				AssertEquals(unit, false, volume.HasErrors);
				AssertEquals(true, NatureAndQtyOfGoodsVolume.IsValidVolume(volume.Text));
			}
		}

		public void TestVolume_RoundedTo2Decimals()
		{
			NatureAndQtyOfGoodsVolume volume = (NatureAndQtyOfGoodsVolume)GetNewBusinessObject();
			volume.Volume = 1.23m;
			AssertEquals(1.23m, volume.Volume);

			volume.Volume = 1.231m;
			AssertEquals(1.24m, volume.Volume);

			volume.Volume = 1.234m;
			AssertEquals(1.24m, volume.Volume);

			volume.Volume = 1.239m;
			AssertEquals(1.24m, volume.Volume);

			volume.Volume = 1.30m;
			AssertEquals(1.30m, volume.Volume);
		}

		public void TestVolume_DecimalPlaces()
		{
			var decimalPlacesAttribute = typeof(NatureAndQtyOfGoodsVolume)
				.GetProperty(NatureAndQtyOfGoodsVolume.Schema.Volume)
				.GetCustomAttributes(typeof(DecimalPlacesAttribute), true)[0] as DecimalPlacesAttribute;

			AssertEquals("Volume should show 2 decimals", 2, decimalPlacesAttribute.DecimalPlaces);
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
