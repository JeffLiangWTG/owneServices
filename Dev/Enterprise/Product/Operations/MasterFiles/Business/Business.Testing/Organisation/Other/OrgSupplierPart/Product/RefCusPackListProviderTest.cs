using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class RefCusPackListProviderTest : TestCaseWithFactory
	{
		public void TestGetCustomsPackList()
		{
			var provider = new RefCusPackListProvider();
			var list = provider.GetCustomsPackList(Factory, ZString.Empty, ZString.Empty);
			var expectedList = new BaseCusUQList();
			AssertEquals("default list", 21, list.Count);
			AssertContainsExactElementsInAnyOrder("default list", expectedList, list);

			list = provider.GetCustomsPackList(Factory, "AMS", ZString.Empty);
			AssertEquals("AMS list", 139, list.Count);
			Assert("AMS list", list.ContainsCode("WRP"));

			list = provider.GetCustomsPackList(Factory, "AFR", ZString.Empty);
			AssertEquals("AFR list", 75, list.Count);
			Assert("AFR list", list.ContainsCode("ZZ"));

			list = provider.GetCustomsPackList(Factory, "GMB", ZString.Empty);
			AssertEquals("GMB list", 76, list.Count);
			Assert("GMB list", list.ContainsCode("BOX"));

			list = provider.GetCustomsPackList(Factory, "GMP", ZString.Empty);
			AssertEquals("GMP list", 76, list.Count);
			Assert("GMP list", list.ContainsCode("BOX"));
		}

		public void TestGetCommercialPackList()
		{
			var provider = new RefCusPackListProvider();
			var list = provider.GetCommercialPackList(Factory, ZString.Empty);

			AssertEquals(76, list.Count);
		}

		public void TestGetPackConversionTypeList()
		{
			AssertNull(new RefCusPackListProvider().GetPackConversionTypeList(Factory));
		}

		public void TestLoader()
		{
			var provider = RefCusPackListProvider.Loader.GetRefCusPackListProvider(Factory, ZString.Empty);
			AssertEquals(null, provider);
		}

		public void TestGetDeclarationPackTypeList()
		{
			AssertEquals(0, new RefCusPackListProvider().GetDeclarationPackTypeList(Factory).Count);
		}

		public void TestGetDeclarationPackTypeListProvider()
		{
			var factory = Factory;
			CombineAssertions(() =>
			{
				AssertEquals("PR", "Enterprise.Customs.US.Business.RefCusPackListProvider", RefCusPackListProvider.GetDeclarationPackTypeListProvider(factory, Core.Constants.CountryCodes.PuertoRico).GetType().FullName);
				AssertEquals("LI", "Enterprise.Customs.Business.RefCusPackListProvider", RefCusPackListProvider.GetDeclarationPackTypeListProvider(factory, Core.Constants.CountryCodes.Liechtenstein).GetType().FullName);
				AssertEquals("FI", "Enterprise.Customs.EU.Business.RefCusPackListProvider", RefCusPackListProvider.GetDeclarationPackTypeListProvider(factory, Core.Constants.CountryCodes.Finland).GetType().FullName);
				AssertEquals("GB", "Enterprise.Customs.EU.Business.RefCusPackListProvider", RefCusPackListProvider.GetDeclarationPackTypeListProvider(factory, Core.Constants.CountryCodes.UnitedKingdom).GetType().FullName);
				AssertEquals("DE", "Enterprise.Customs.EU.Business.RefCusPackListProvider", RefCusPackListProvider.GetDeclarationPackTypeListProvider(factory, Core.Constants.CountryCodes.Germany).GetType().FullName);
				AssertEquals("ES", "Enterprise.Customs.EU.Business.RefCusPackListProvider", RefCusPackListProvider.GetDeclarationPackTypeListProvider(factory, Core.Constants.CountryCodes.Spain).GetType().FullName);
				AssertEquals("IE", "Enterprise.Customs.EU.Business.RefCusPackListProvider", RefCusPackListProvider.GetDeclarationPackTypeListProvider(factory, Core.Constants.CountryCodes.Ireland).GetType().FullName);
				AssertEquals("CN", "Enterprise.Customs.CN.Business.RefCusPackListProvider", RefCusPackListProvider.GetDeclarationPackTypeListProvider(factory, Core.Constants.CountryCodes.China).GetType().FullName);
				AssertEquals("BR", "Enterprise.Customs.BR.Business.RefCusPackListProvider", RefCusPackListProvider.GetDeclarationPackTypeListProvider(factory, Core.Constants.CountryCodes.Brazil).GetType().FullName);
				AssertEquals("CA", "Enterprise.Customs.CA.Business.RefCusPackListProvider", RefCusPackListProvider.GetDeclarationPackTypeListProvider(factory, Core.Constants.CountryCodes.Canada).GetType().FullName);
				AssertEquals("TW", "Enterprise.Customs.TW.Business.RefCusPackListProvider", RefCusPackListProvider.GetDeclarationPackTypeListProvider(factory, Core.Constants.CountryCodes.Taiwan).GetType().FullName);
				AssertEquals("US", "Enterprise.Customs.US.Business.RefCusPackListProvider", RefCusPackListProvider.GetDeclarationPackTypeListProvider(factory, Core.Constants.CountryCodes.UnitedStates).GetType().FullName);
				AssertEquals("NZ", "Enterprise.Customs.NZ.Business.RefCusPackListProvider", RefCusPackListProvider.GetDeclarationPackTypeListProvider(factory, Core.Constants.CountryCodes.NewZealand).GetType().FullName);
				AssertEquals("AU", "Enterprise.Customs.AU.Declaration.Business.RefCusPackListProvider", RefCusPackListProvider.GetDeclarationPackTypeListProvider(factory, Core.Constants.CountryCodes.Australia).GetType().FullName);
			});
		}
	}
}
