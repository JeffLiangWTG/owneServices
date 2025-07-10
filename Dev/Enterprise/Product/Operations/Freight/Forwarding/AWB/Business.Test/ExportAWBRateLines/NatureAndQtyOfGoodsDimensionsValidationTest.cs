using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.AWB.Business.Testing
{
	[TestedType(typeof(NatureAndQtyOfGoodsDimensionsValidation))]
	sealed class NatureAndQtyOfGoodsDimensionsValidationTest : NatureAndQtyOfGoodsValidationTest
	{
		protected override void AssertText()
		{
			Assert("n/a", true);
		}

		public void TestValidateLength()
		{
			var line = Factory.New<ExportAWBRateLine>();
			line.ER_NatureAndQtyOfGoodsType = Core.Constants.AWB.NatureAndQtyOfGoodsTypes.Dimensions;

			var natureAndQtyOfGoods = new NatureAndQtyOfGoodsDimensions(line);
			AssertNoErrors(natureAndQtyOfGoods.LengthInfo);

			var message = "AWB Dimension 'Length' must be within the range 1 to 99999.";

			natureAndQtyOfGoods.Length = 100000;
			AssertHasError(natureAndQtyOfGoods.LengthInfo, message);

			natureAndQtyOfGoods.Length = 0;
			AssertHasError(natureAndQtyOfGoods.LengthInfo, message);

			natureAndQtyOfGoods.Length = 99999;
			AssertNoError(natureAndQtyOfGoods.LengthInfo, message);

			natureAndQtyOfGoods.Length = 1;
			AssertNoError(natureAndQtyOfGoods.LengthInfo, message);

			natureAndQtyOfGoods.Length = 123;
			AssertNoError(natureAndQtyOfGoods.LengthInfo, message);
		}

		public void TestValidateWidth()
		{
			var line = Factory.New<ExportAWBRateLine>();
			line.ER_NatureAndQtyOfGoodsType = Core.Constants.AWB.NatureAndQtyOfGoodsTypes.Dimensions;

			var natureAndQtyOfGoods = new NatureAndQtyOfGoodsDimensions(line);
			AssertNoErrors(natureAndQtyOfGoods.WidthInfo);

			var message = "AWB Dimension 'Width' must be within the range 1 to 99999.";

			natureAndQtyOfGoods.Width = 100000;
			AssertHasError(natureAndQtyOfGoods.WidthInfo, message);

			natureAndQtyOfGoods.Width = 0;
			AssertHasError(natureAndQtyOfGoods.WidthInfo, message);

			natureAndQtyOfGoods.Width = 99999;
			AssertNoError(natureAndQtyOfGoods.WidthInfo, message);

			natureAndQtyOfGoods.Width = 1;
			AssertNoError(natureAndQtyOfGoods.WidthInfo, message);

			natureAndQtyOfGoods.Width = 123;
			AssertNoError(natureAndQtyOfGoods.WidthInfo, message);
		}

		public void TestValidateHeight()
		{
			var line = Factory.New<ExportAWBRateLine>();
			line.ER_NatureAndQtyOfGoodsType = Core.Constants.AWB.NatureAndQtyOfGoodsTypes.Dimensions;

			var natureAndQtyOfGoods = new NatureAndQtyOfGoodsDimensions(line);
			AssertNoErrors(natureAndQtyOfGoods.HeightInfo);

			var message = "AWB Dimension 'Height' must be within the range 1 to 99999.";

			natureAndQtyOfGoods.Height = 100000;
			AssertHasError(natureAndQtyOfGoods.HeightInfo, message);

			natureAndQtyOfGoods.Height = 0;
			AssertHasError(natureAndQtyOfGoods.HeightInfo, message);

			natureAndQtyOfGoods.Height = 99999;
			AssertNoError(natureAndQtyOfGoods.HeightInfo, message);

			natureAndQtyOfGoods.Height = 1;
			AssertNoError(natureAndQtyOfGoods.HeightInfo, message);

			natureAndQtyOfGoods.Height = 123;
			AssertNoError(natureAndQtyOfGoods.HeightInfo, message);
		}

		public void TestValidateDimensionUnit()
		{
			ExportAWBRateLine line = Factory.New<ExportAWBRateLine>();
			line.ER_NatureAndQtyOfGoodsType = Core.Constants.AWB.NatureAndQtyOfGoodsTypes.Dimensions;

			NatureAndQtyOfGoodsDimensions natureAndQtyOfGoods = new NatureAndQtyOfGoodsDimensions(line);
			NatureAndQtyOfGoodsDimensionsValidation validation = new NatureAndQtyOfGoodsDimensionsValidation(natureAndQtyOfGoods);

			natureAndQtyOfGoods.Unit = ZString.Empty;
			validation.ValidateUnit();
			AssertHasError(natureAndQtyOfGoods.UnitInfo, "Please enter a value.");

			line.ER_NatureAndQtyOfGoodsType = Core.Constants.AWB.NatureAndQtyOfGoodsTypes.GoodsDescription;
			validation.ValidateUnit();
			AssertNoError(natureAndQtyOfGoods.UnitInfo, "Please enter a value.");

			line.ER_NatureAndQtyOfGoodsType = Core.Constants.AWB.NatureAndQtyOfGoodsTypes.Dimensions;
			natureAndQtyOfGoods.Unit = "AA";
			validation.ValidateUnit();
			AssertHasError(natureAndQtyOfGoods.UnitInfo, "Enter a valid selection.");

			line.ER_NatureAndQtyOfGoodsType = Core.Constants.AWB.NatureAndQtyOfGoodsTypes.GoodsDescription;
			validation.ValidateUnit();
			AssertNoError(natureAndQtyOfGoods.UnitInfo, "Enter a valid selection.");

			line.ER_NatureAndQtyOfGoodsType = Core.Constants.AWB.NatureAndQtyOfGoodsTypes.Dimensions;
			natureAndQtyOfGoods.Unit = Core.Constants.Length.Feet;
			AssertNoError(natureAndQtyOfGoods.UnitInfo, "Enter a valid selection.");
		}

		public void TestValidateCount()
		{
			var line = Factory.New<ExportAWBRateLine>();
			line.ER_NatureAndQtyOfGoodsType = Core.Constants.AWB.NatureAndQtyOfGoodsTypes.Dimensions;

			var natureAndQtyOfGoods = new NatureAndQtyOfGoodsDimensions(line);
			AssertNoErrors(natureAndQtyOfGoods.CountInfo);

			var message = "AWB Dimension 'Package Count' must be within the range 1 to 9999.";

			natureAndQtyOfGoods.Count = 10000;
			AssertHasError(natureAndQtyOfGoods.CountInfo, message);

			natureAndQtyOfGoods.Count = 0;
			AssertHasError(natureAndQtyOfGoods.CountInfo, message);

			natureAndQtyOfGoods.Count = 9999;
			AssertNoError(natureAndQtyOfGoods.CountInfo, message);

			natureAndQtyOfGoods.Count = 1;
			AssertNoError(natureAndQtyOfGoods.CountInfo, message);

			natureAndQtyOfGoods.Count = 123;
			AssertNoError(natureAndQtyOfGoods.CountInfo, message);
		}

		protected override NatureAndQtyOfGoodsValidation GetNewValidation(NatureAndQtyOfGoods parent)
		{
			return new NatureAndQtyOfGoodsDimensionsValidation((NatureAndQtyOfGoodsDimensions)parent);
		}

		protected override NatureAndQtyOfGoods GetNewParent()
		{
			return new NatureAndQtyOfGoodsDimensions(Factory.New<ExportAWBRateLine>());
		}
	}
}
