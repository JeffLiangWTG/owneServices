using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.AWB.Business.Testing
{
	[TestedType(typeof(NatureAndQtyOfGoodsSLACValidation))]
	sealed class NatureAndQtyOfGoodsSLACValidationTest : NatureAndQtyOfGoodsValidationTest
	{
		protected override void AssertText()
		{
			Assert("n/a", true);
		}

		public void TestValidateCount()
		{
			var line = Factory.New<ExportAWBRateLine>();
			line.ER_NatureAndQtyOfGoodsType = Core.Constants.AWB.NatureAndQtyOfGoodsTypes.ShippersLoadAndCount;

			var natureAndQtyOfGoods = new NatureAndQtyOfGoodsSLAC(line);
			AssertNoErrors(natureAndQtyOfGoods.CountInfo);

			natureAndQtyOfGoods.Count = -1;
			AssertHasError(natureAndQtyOfGoods.CountInfo, "Please enter a 'Count' within the range 1 to 99999.");

			natureAndQtyOfGoods.Count = 100000;
			AssertHasError(natureAndQtyOfGoods.CountInfo, "Please enter a 'Count' within the range 1 to 99999.");

			natureAndQtyOfGoods.Count = 99999;
			AssertNoError(natureAndQtyOfGoods.CountInfo, "Please enter a 'Count' within the range 1 to 99999.");

			natureAndQtyOfGoods.Count = 0;
			AssertHasError(natureAndQtyOfGoods.CountInfo, "Please enter a 'Count' within the range 1 to 99999.");

			natureAndQtyOfGoods.Count = 1;
			AssertNoError(natureAndQtyOfGoods.CountInfo, "Please enter a 'Count' within the range 1 to 99999.");

			natureAndQtyOfGoods.Count = 123;
			AssertNoError(natureAndQtyOfGoods.CountInfo, "Please enter a 'Count' within the range 1 to 99999.");
		}

		protected override NatureAndQtyOfGoodsValidation GetNewValidation(NatureAndQtyOfGoods parent)
		{
			return new NatureAndQtyOfGoodsSLACValidation((NatureAndQtyOfGoodsSLAC)parent);
		}

		protected override NatureAndQtyOfGoods GetNewParent()
		{
			return new NatureAndQtyOfGoodsSLAC(Factory.New<ExportAWBRateLine>());
		}
	}
}
