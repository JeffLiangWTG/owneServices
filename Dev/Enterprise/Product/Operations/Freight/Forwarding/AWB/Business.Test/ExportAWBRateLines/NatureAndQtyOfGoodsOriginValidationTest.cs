using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.AWB.Business.Testing
{
	[TestedType(typeof(NatureAndQtyOfGoodsOriginValidation))]
	sealed class NatureAndQtyOfGoodsOriginValidationTest : NatureAndQtyOfGoodsValidationTest
	{
		protected override void AssertText()
		{
			Assert("n/a", true);
		}

		public void TestValidateCountry()
		{
			ExportAWBRateLine line = Factory.New<ExportAWBRateLine>();
			line.ER_NatureAndQtyOfGoodsType = Core.Constants.AWB.NatureAndQtyOfGoodsTypes.CountryOfGoodsOrigin;

			NatureAndQtyOfGoodsOrigin natureAndQtyOfGoods = new NatureAndQtyOfGoodsOrigin(line);

			natureAndQtyOfGoods.Country = "A";
			AssertHasError(natureAndQtyOfGoods.CountryInfo, "Enter a valid selection.");

			natureAndQtyOfGoods.Country = "XX";
			AssertHasError(natureAndQtyOfGoods.CountryInfo, "Enter a valid selection.");

			natureAndQtyOfGoods.Country = ZString.Empty;
			AssertHasError(natureAndQtyOfGoods.CountryInfo, "Please enter a value.");

			natureAndQtyOfGoods.Country = Core.Constants.CountryCodes.Australia;
			AssertNoError(natureAndQtyOfGoods.CountryInfo, "Please enter a value.");
			AssertNoError(natureAndQtyOfGoods.CountryInfo, "Enter a valid selection.");
		}

		protected override NatureAndQtyOfGoodsValidation GetNewValidation(NatureAndQtyOfGoods parent)
		{
			return new NatureAndQtyOfGoodsOriginValidation((NatureAndQtyOfGoodsOrigin)parent);
		}

		protected override NatureAndQtyOfGoods GetNewParent()
		{
			return new NatureAndQtyOfGoodsOrigin(Factory.New<ExportAWBRateLine>());
		}
	}
}
