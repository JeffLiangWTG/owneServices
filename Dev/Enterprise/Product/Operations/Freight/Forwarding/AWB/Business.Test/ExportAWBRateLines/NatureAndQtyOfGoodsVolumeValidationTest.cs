using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.AWB.Business.Testing
{
	[TestedType(typeof(NatureAndQtyOfGoodsVolumeValidation))]
	sealed class NatureAndQtyOfGoodsVolumeValidationTest : NatureAndQtyOfGoodsValidationTest
	{
		protected override void AssertText()
		{
			Assert("n/a", true);
		}

		public void TestValidateVolume()
		{
			ExportAWBRateLine line = Factory.New<ExportAWBRateLine>();
			line.ER_NatureAndQtyOfGoodsType = Core.Constants.AWB.NatureAndQtyOfGoodsTypes.Volume;

			NatureAndQtyOfGoodsVolume natureAndQtyOfGoods = new NatureAndQtyOfGoodsVolume(line);
			AssertNoErrors(natureAndQtyOfGoods.VolumeInfo);

			natureAndQtyOfGoods.Volume = -1m;
			AssertHasError(natureAndQtyOfGoods.VolumeInfo, "Please enter a 'Volume' within the range 0.01 to 99999.999.");

			natureAndQtyOfGoods.Volume = 0m;
			AssertHasError(natureAndQtyOfGoods.VolumeInfo, "Please enter a 'Volume' within the range 0.01 to 99999.999.");

			natureAndQtyOfGoods.Volume = 100000m;
			AssertHasError(natureAndQtyOfGoods.VolumeInfo, "Please enter a 'Volume' within the range 0.01 to 99999.999.");

			natureAndQtyOfGoods.Volume = 12345.678m;
			AssertNoError(natureAndQtyOfGoods.VolumeInfo, "Please enter a 'Volume' within the range 0.01 to 99999.999.");
		}

		public void TestValidateVolumeUnit()
		{
			ExportAWBRateLine line = Factory.New<ExportAWBRateLine>();
			line.ER_NatureAndQtyOfGoodsType = Core.Constants.AWB.NatureAndQtyOfGoodsTypes.Volume;

			NatureAndQtyOfGoodsVolume natureAndQtyOfGoods = new NatureAndQtyOfGoodsVolume(line);
			NatureAndQtyOfGoodsVolumeValidation validation = new NatureAndQtyOfGoodsVolumeValidation(natureAndQtyOfGoods);

			natureAndQtyOfGoods.Unit = ZString.Empty;
			validation.ValidateUnit();
			AssertHasError(natureAndQtyOfGoods.UnitInfo, "Please enter a value.");

			line.ER_NatureAndQtyOfGoodsType = Core.Constants.AWB.NatureAndQtyOfGoodsTypes.GoodsDescription;
			validation.ValidateUnit();
			AssertNoError(natureAndQtyOfGoods.UnitInfo, "Please enter a value.");

			line.ER_NatureAndQtyOfGoodsType = Core.Constants.AWB.NatureAndQtyOfGoodsTypes.Volume;
			natureAndQtyOfGoods.Unit = "AA";
			validation.ValidateUnit();
			AssertHasError(natureAndQtyOfGoods.UnitInfo, "Enter a valid selection.");

			line.ER_NatureAndQtyOfGoodsType = Core.Constants.AWB.NatureAndQtyOfGoodsTypes.GoodsDescription;
			validation.ValidateUnit();
			AssertNoError(natureAndQtyOfGoods.UnitInfo, "Enter a valid selection.");

			line.ER_NatureAndQtyOfGoodsType = Core.Constants.AWB.NatureAndQtyOfGoodsTypes.Volume;
			natureAndQtyOfGoods.Unit = Core.Constants.Volume.CubicMetres;
			AssertNoError(natureAndQtyOfGoods.UnitInfo, "Enter a valid selection.");
		}

		protected override NatureAndQtyOfGoodsValidation GetNewValidation(NatureAndQtyOfGoods parent)
		{
			return new NatureAndQtyOfGoodsVolumeValidation((NatureAndQtyOfGoodsVolume)parent);
		}

		protected override NatureAndQtyOfGoods GetNewParent()
		{
			return new NatureAndQtyOfGoodsVolume(Factory.New<ExportAWBRateLine>());
		}
	}
}
