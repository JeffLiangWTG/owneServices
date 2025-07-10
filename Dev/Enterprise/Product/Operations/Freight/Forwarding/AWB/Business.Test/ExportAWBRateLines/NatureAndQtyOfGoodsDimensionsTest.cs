using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.AWB.Business.Testing
{
	[TestedType(typeof(NatureAndQtyOfGoodsDimensions))]
	public class NatureAndQtyOfGoodsDimensionsTest : NatureAndQtyOfGoodsTest
	{
		public override void TestText()
		{
			AssertSerialization();
			AssertDeserialization();
		}

		public void TestDimensions_DeserializedFromRateLine()
		{
			var rateline = Factory.New<ExportAWBRateLine>();
			rateline.ER_NatureAndQtyOfGoods = "DIMS 1x2x3 M x 4";

			var dimensions = new NatureAndQtyOfGoodsDimensions(rateline);

			AssertEquals(1, dimensions.Length);
			AssertEquals(2, dimensions.Width);
			AssertEquals(3, dimensions.Height);
			AssertEquals(4, dimensions.Count);
			AssertEquals(Core.Constants.Length.Metres, dimensions.Unit);
		}

		public override void TestTextSize()
		{
			NatureAndQtyOfGoodsDimensions dim = (NatureAndQtyOfGoodsDimensions)GetNewBusinessObject();
			dim.Length = 99999;
			dim.Width = 99999;
			dim.Height = 99999;
			dim.Count = 9999;

			int longestUnit = Core.Constants.Length.Codes.Max((unit) => unit.Length);
			dim.Unit = Core.Constants.Length.Codes
				.Where((unit) => unit.Length == longestUnit)
				.First();

			AssertEquals("prerequisite", false, dim.HasErrors);
			Assert("max dimension serialized", dim.Text.Length < 36);
		}

		void AssertSerialization()
		{
			NatureAndQtyOfGoodsDimensions dimensions = (NatureAndQtyOfGoodsDimensions)GetNewBusinessObject();
			AssertEquals("DIMS 0x0x0  x 0", dimensions.Text.ToString());

			dimensions.Length = 1;
			dimensions.Unit = Core.Constants.Volume.CubicFeet;
			AssertEquals("DIMS 1x0x0 CF x 0", dimensions.Text.ToString());

			dimensions.Width = 2;
			dimensions.Unit = Core.Constants.Volume.Litre;
			AssertEquals("DIMS 1x2x0 L x 0", dimensions.Text.ToString());

			dimensions.Height = 3;
			dimensions.Unit = Core.Constants.Volume.TeaChest;
			AssertEquals("DIMS 1x2x3 TE x 0", dimensions.Text.ToString());

			dimensions.Count = 4;
			dimensions.Unit = Core.Constants.Volume.CubicMetres;
			AssertEquals("DIMS 1x2x3 M3 x 4", dimensions.Text.ToString());
		}

		void AssertDeserialization()
		{
			var dimensions = (NatureAndQtyOfGoodsDimensions)GetNewBusinessObject();
			AssertEquals(0, dimensions.Length);
			AssertEquals(0, dimensions.Width);
			AssertEquals(0, dimensions.Height);
			AssertEquals(0, dimensions.Count);
			AssertEquals(ZString.Empty, dimensions.Unit);

			dimensions.Text = "AAAA";
			AssertEquals(0, dimensions.Length);
			AssertEquals(0, dimensions.Width);
			AssertEquals(0, dimensions.Height);
			AssertEquals(0, dimensions.Count);
			AssertEquals(ZString.Empty, dimensions.Unit);

			dimensions.Text = "DIMS 1x2x3  x 4";
			AssertEquals(0, dimensions.Length);
			AssertEquals(0, dimensions.Width);
			AssertEquals(0, dimensions.Height);
			AssertEquals(0, dimensions.Count);
			AssertEquals(ZString.Empty, dimensions.Unit);

			dimensions.Text = "DIMS 1.1x2x3 IN x 4";
			AssertEquals(0, dimensions.Length);
			AssertEquals(0, dimensions.Width);
			AssertEquals(0, dimensions.Height);
			AssertEquals(0, dimensions.Count);
			AssertEquals(ZString.Empty, dimensions.Unit);

			dimensions.Text = "DIMS 1x2.2x3 IN x 4";
			AssertEquals(0, dimensions.Length);
			AssertEquals(0, dimensions.Width);
			AssertEquals(0, dimensions.Height);
			AssertEquals(0, dimensions.Count);
			AssertEquals(ZString.Empty, dimensions.Unit);

			dimensions.Text = "DIMS 1x2x3.3 IN x 4";
			AssertEquals(0, dimensions.Length);
			AssertEquals(0, dimensions.Width);
			AssertEquals(0, dimensions.Height);
			AssertEquals(0, dimensions.Count);
			AssertEquals(ZString.Empty, dimensions.Unit);

			dimensions.Text = "DIMS 1x2x3 IN x 4.4";
			AssertEquals(0, dimensions.Length);
			AssertEquals(0, dimensions.Width);
			AssertEquals(0, dimensions.Height);
			AssertEquals(0, dimensions.Count);
			AssertEquals(ZString.Empty, dimensions.Unit);

			dimensions.Text = "DIMS 1x2x3 M x 4";
			AssertEquals(1, dimensions.Length);
			AssertEquals(2, dimensions.Width);
			AssertEquals(3, dimensions.Height);
			AssertEquals(4, dimensions.Count);
			AssertEquals(Core.Constants.Length.Metres, dimensions.Unit);
		}

		public void TestSupportsAllUnits()
		{
			NatureAndQtyOfGoodsDimensions dim = (NatureAndQtyOfGoodsDimensions)GetNewBusinessObject();
			dim.Length = 1;
			dim.Width = 2;
			dim.Height = 3;
			dim.Count = 4;

			foreach (string unit in Core.Constants.Length.Codes)
			{
				dim.Unit = unit;
				AssertEquals(unit, false, dim.HasErrors);
				AssertEquals(true, NatureAndQtyOfGoodsDimensions.IsValidDimension(dim.Text));
			}
		}

		public void TestIsValidDimension()
		{
			AssertEquals(false, NatureAndQtyOfGoodsDimensions.IsValidDimension("AAA"));
			AssertEquals(false, NatureAndQtyOfGoodsDimensions.IsValidDimension("DIMS"));
			AssertEquals(false, NatureAndQtyOfGoodsDimensions.IsValidDimension("DIMS 1x2x3"));
			AssertEquals(false, NatureAndQtyOfGoodsDimensions.IsValidDimension("DIMS 1x2x3 4"));
			AssertEquals(false, NatureAndQtyOfGoodsDimensions.IsValidDimension("DIMS 1x2x3 M"));
			AssertEquals(false, NatureAndQtyOfGoodsDimensions.IsValidDimension("DIMS 1.x2x3 M x 4"));
			AssertEquals(false, NatureAndQtyOfGoodsDimensions.IsValidDimension("DIMS 1.1x2x3 M x 4"));
			AssertEquals(false, NatureAndQtyOfGoodsDimensions.IsValidDimension("DIMS 1x2.2x3 M x 4"));
			AssertEquals(false, NatureAndQtyOfGoodsDimensions.IsValidDimension("DIMS 1x2x3.3 M x 4"));
			AssertEquals(false, NatureAndQtyOfGoodsDimensions.IsValidDimension("DIMS 1x2x3 M x 4.4"));

			AssertEquals(true, NatureAndQtyOfGoodsDimensions.IsValidDimension("DIMS 1x2x3 IN x 4"));

			AssertEquals(false, NatureAndQtyOfGoodsDimensions.IsValidDimension("DIMS 123456x1x1 IN x 1"));
			AssertEquals(false, NatureAndQtyOfGoodsDimensions.IsValidDimension("DIMS 1x123456x1 IN x 1"));
			AssertEquals(false, NatureAndQtyOfGoodsDimensions.IsValidDimension("DIMS 1x1x123456 IN x 1"));
			AssertEquals(false, NatureAndQtyOfGoodsDimensions.IsValidDimension("DIMS 1x1x1 IN x 12345"));
		}

		public void TestHumanReadableName()
		{
			AssertEquals("Nature and Quantity Of Goods Dimensions", GetNewBusinessObject().HumanReadableName);
		}

		public override Type ExpectedValidationType
		{
			get { return typeof(NatureAndQtyOfGoodsDimensionsValidation); }
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new NatureAndQtyOfGoodsDimensions(Factory.New<ExportAWBRateLine>());
		}
	}
}
