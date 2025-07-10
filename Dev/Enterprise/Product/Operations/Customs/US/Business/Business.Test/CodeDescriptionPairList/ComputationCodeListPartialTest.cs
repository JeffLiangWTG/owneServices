using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class ComputationCodeListTest : TestCase
	{
		public void TestIsQuantityRequired()
		{
			AssertEquals(false, ComputationCodeList.IsFirstQuantityRequired(ComputationCodeList.Codes.AdValorem));
			AssertEquals(false, ComputationCodeList.IsSecondQuantityRequired(ComputationCodeList.Codes.AdValorem));
			AssertEquals(false, ComputationCodeList.IsThirdQuantityRequired(ComputationCodeList.Codes.AdValorem));

			AssertEquals(false, ComputationCodeList.IsFirstQuantityRequired(ComputationCodeList.Codes.CompoundSpecificAdValorem));
			AssertEquals(false, ComputationCodeList.IsSecondQuantityRequired(ComputationCodeList.Codes.CompoundSpecificAdValorem));
			AssertEquals(true, ComputationCodeList.IsThirdQuantityRequired(ComputationCodeList.Codes.CompoundSpecificAdValorem));

			AssertEquals(true, ComputationCodeList.IsFirstQuantityRequired(ComputationCodeList.Codes.CompoundSpecificAndAdValoremFirstQuantity));
			AssertEquals(false, ComputationCodeList.IsSecondQuantityRequired(ComputationCodeList.Codes.CompoundSpecificAndAdValoremFirstQuantity));
			AssertEquals(false, ComputationCodeList.IsThirdQuantityRequired(ComputationCodeList.Codes.CompoundSpecificAndAdValoremFirstQuantity));

			AssertEquals(false, ComputationCodeList.IsFirstQuantityRequired(ComputationCodeList.Codes.CompoundSpecificAndAdValoremSecondQuantity));
			AssertEquals(true, ComputationCodeList.IsSecondQuantityRequired(ComputationCodeList.Codes.CompoundSpecificAndAdValoremSecondQuantity));
			AssertEquals(false, ComputationCodeList.IsThirdQuantityRequired(ComputationCodeList.Codes.CompoundSpecificAndAdValoremSecondQuantity));

			AssertEquals(false, ComputationCodeList.IsFirstQuantityRequired(ComputationCodeList.Codes.Free));
			AssertEquals(false, ComputationCodeList.IsSecondQuantityRequired(ComputationCodeList.Codes.Free));
			AssertEquals(false, ComputationCodeList.IsThirdQuantityRequired(ComputationCodeList.Codes.Free));

			AssertEquals(false, ComputationCodeList.IsFirstQuantityRequired(ComputationCodeList.Codes.FunctionalAdValorem));
			AssertEquals(false, ComputationCodeList.IsSecondQuantityRequired(ComputationCodeList.Codes.FunctionalAdValorem));
			AssertEquals(true, ComputationCodeList.IsThirdQuantityRequired(ComputationCodeList.Codes.FunctionalAdValorem));

			AssertEquals(true, ComputationCodeList.IsFirstQuantityRequired(ComputationCodeList.Codes.MultipleSpecific));
			AssertEquals(true, ComputationCodeList.IsSecondQuantityRequired(ComputationCodeList.Codes.MultipleSpecific));
			AssertEquals(false, ComputationCodeList.IsThirdQuantityRequired(ComputationCodeList.Codes.MultipleSpecific));

			AssertEquals(false, ComputationCodeList.IsFirstQuantityRequired(ComputationCodeList.Codes.SpecificCompound));
			AssertEquals(true, ComputationCodeList.IsSecondQuantityRequired(ComputationCodeList.Codes.SpecificCompound));
			AssertEquals(false, ComputationCodeList.IsThirdQuantityRequired(ComputationCodeList.Codes.SpecificCompound));

			AssertEquals(true, ComputationCodeList.IsFirstQuantityRequired(ComputationCodeList.Codes.SpecificPlusCompound));
			AssertEquals(true, ComputationCodeList.IsSecondQuantityRequired(ComputationCodeList.Codes.SpecificPlusCompound));
			AssertEquals(true, ComputationCodeList.IsThirdQuantityRequired(ComputationCodeList.Codes.SpecificPlusCompound));
		}
	}
}
