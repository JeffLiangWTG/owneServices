using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.NZ.Business.MAFeBACCa.CodedLists.Testing
{
	class GoodsTypeListTest : CodeDescriptionEnumListTestCase
	{
		public void TestGetCodeFromTariff()
		{
			AssertEquals("null", GoodsTypeList.Codes.Miscellaneous, GoodsTypeList.GetCodeFromTariff(null));
			AssertEquals("string.Empty", GoodsTypeList.Codes.Miscellaneous, GoodsTypeList.GetCodeFromTariff(string.Empty));
			AssertEquals("01", GoodsTypeList.Codes.Animals, GoodsTypeList.GetCodeFromTariff("01"));
			AssertEquals("02", GoodsTypeList.Codes.AnimalProducts, GoodsTypeList.GetCodeFromTariff("02"));
			AssertEquals("03", GoodsTypeList.Codes.AnimalProducts, GoodsTypeList.GetCodeFromTariff("03"));
			AssertEquals("04", GoodsTypeList.Codes.AnimalProducts, GoodsTypeList.GetCodeFromTariff("04"));
			AssertEquals("05", GoodsTypeList.Codes.AnimalProducts, GoodsTypeList.GetCodeFromTariff("05"));
			AssertEquals("06", GoodsTypeList.Codes.NurseryStock, GoodsTypeList.GetCodeFromTariff("06"));
			AssertEquals("07", GoodsTypeList.Codes.Produce, GoodsTypeList.GetCodeFromTariff("07"));
			AssertEquals("08", GoodsTypeList.Codes.Produce, GoodsTypeList.GetCodeFromTariff("08"));
			AssertEquals("09", GoodsTypeList.Codes.Produce, GoodsTypeList.GetCodeFromTariff("09"));
			AssertEquals("10", GoodsTypeList.Codes.SeedGrain, GoodsTypeList.GetCodeFromTariff("10"));
			AssertEquals("11", GoodsTypeList.Codes.PlantProducts, GoodsTypeList.GetCodeFromTariff("11"));
			AssertEquals("12", GoodsTypeList.Codes.PlantProducts, GoodsTypeList.GetCodeFromTariff("12"));
			AssertEquals("13", GoodsTypeList.Codes.PlantProducts, GoodsTypeList.GetCodeFromTariff("13"));
			AssertEquals("14", GoodsTypeList.Codes.PlantProducts, GoodsTypeList.GetCodeFromTariff("14"));
			AssertEquals("15", GoodsTypeList.Codes.Miscellaneous, GoodsTypeList.GetCodeFromTariff("15"));
			AssertEquals("16", GoodsTypeList.Codes.AnimalProducts, GoodsTypeList.GetCodeFromTariff("16"));
			AssertEquals("17", GoodsTypeList.Codes.Miscellaneous, GoodsTypeList.GetCodeFromTariff("17"));
			AssertEquals("18", GoodsTypeList.Codes.Miscellaneous, GoodsTypeList.GetCodeFromTariff("18"));
			AssertEquals("19", GoodsTypeList.Codes.Miscellaneous, GoodsTypeList.GetCodeFromTariff("19"));
			AssertEquals("20", GoodsTypeList.Codes.Miscellaneous, GoodsTypeList.GetCodeFromTariff("20"));

			AssertEquals("31", GoodsTypeList.Codes.Fertiliser, GoodsTypeList.GetCodeFromTariff("31"));
			AssertEquals("32", GoodsTypeList.Codes.Miscellaneous, GoodsTypeList.GetCodeFromTariff("32"));
			AssertEquals("87", GoodsTypeList.Codes.Vehicles, GoodsTypeList.GetCodeFromTariff("87"));
			AssertEquals("8707", GoodsTypeList.Codes.Vehicles, GoodsTypeList.GetCodeFromTariff("8707"));
			AssertEquals("8708", GoodsTypeList.Codes.CarParts, GoodsTypeList.GetCodeFromTariff("8708"));
			AssertEquals("8709", GoodsTypeList.Codes.Vehicles, GoodsTypeList.GetCodeFromTariff("8709"));

			AssertEquals("4010", GoodsTypeList.Codes.Miscellaneous, GoodsTypeList.GetCodeFromTariff("4010"));
			AssertEquals("4011", GoodsTypeList.Codes.Tyres, GoodsTypeList.GetCodeFromTariff("4011"));
			AssertEquals("4012", GoodsTypeList.Codes.Tyres, GoodsTypeList.GetCodeFromTariff("4012"));
			AssertEquals("4013", GoodsTypeList.Codes.Tyres, GoodsTypeList.GetCodeFromTariff("4013"));
			AssertEquals("4014", GoodsTypeList.Codes.Miscellaneous, GoodsTypeList.GetCodeFromTariff("4014"));
			AssertEquals("4077", GoodsTypeList.Codes.Miscellaneous, GoodsTypeList.GetCodeFromTariff("4077"));

			AssertEquals("43", GoodsTypeList.Codes.Miscellaneous, GoodsTypeList.GetCodeFromTariff("43"));
			AssertEquals("44", GoodsTypeList.Codes.Timber, GoodsTypeList.GetCodeFromTariff("44"));
			AssertEquals("45", GoodsTypeList.Codes.Miscellaneous, GoodsTypeList.GetCodeFromTariff("45"));
		}

		protected override CodeDescriptionPairList GetNewList()
		{
			return new GoodsTypeList();
		}
	}
}
