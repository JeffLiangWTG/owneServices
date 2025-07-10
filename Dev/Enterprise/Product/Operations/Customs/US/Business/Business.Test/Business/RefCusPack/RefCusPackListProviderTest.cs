using CargoWise.Types;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class RefCusPackListProviderTest : CargoWise.EntityFramework.Testing.TestCaseWithFactory
	{
		public void TestGetCustomsPackListUS()
		{
			var provider = new RefCusPackListProvider();
			var list = provider.GetCIPCustomsPackList(Factory, ZString.Empty);
			var expectedList = Factory.GetCachedValue<USCusUQList>();
			AssertContainsExactElementsInAnyOrder(expectedList, list);
		}

		public void TestGetCommercialPackListUS()
		{
			var provider = new RefCusPackListProvider();
			var list = provider.GetCommercialPackList(Factory, ZString.Empty);
			var expectedList = new MasterFiles.Business.RefPackTypeCollection(Factory).GetAsCodeDescriptionPairWithStandardUnits();
			AssertContainsExactElementsInAnyOrder(expectedList, list);
		}

		public void TestLoaderUS()
		{
			var provider = MasterFiles.Business.RefCusPackListProvider.Loader.GetRefCusPackListProvider(Factory, Core.Constants.CountryCodes.UnitedStates);
			AssertEquals("Enterprise.Customs.US.Business.RefCusPackListProvider", provider.GetType().ToString());
		}

		public void TestRP_CustomsPack_ListForUS()
		{
			var provider = MasterFiles.Business.RefCusPackListProvider.Loader.GetRefCusPackListProvider(Factory, Core.Constants.CountryCodes.UnitedStates);
			var list = provider.GetCIPCustomsPackList(Factory, ZString.Empty);
			var aesList = new AESUnitOfMeasureList();
			var abiList = new ABIUnitOfMeasureList();
			AssertEquals(true, list.Count >= abiList.Count);
			AssertEquals(true, list.Count >= aesList.Count);
			foreach (CodeDescriptionPair pair in list)
			{
				var existInAES = aesList.ContainsCode(pair.Code);
				var existInABI = abiList.ContainsCode(pair.Code);
				var extraDesc = "";
				var desc = "";
				if (!existInAES && !existInABI)
				{
					Fail(string.Format("Unknown code found '{0}'", pair.CodeAndDescription));
				}
				else if (existInABI && existInAES)
				{
					extraDesc = "Common";
					desc = abiList.GetDescriptionFromCode(pair.Code);
				}
				else if (existInABI)
				{
					extraDesc = "Import Only";
					desc = abiList.GetDescriptionFromCode(pair.Code);
				}
				else
				{
					extraDesc = "Export Only";
					desc = aesList.GetDescriptionFromCode(pair.Code);
				}
				AssertEquals(string.Format("{0} ({1})", desc, extraDesc), pair.Description);
			}
		}

		public void TestGetDeclarationPackTypeList()
		{
			var provider = new RefCusPackListProvider();
			var list = provider.GetDeclarationPackTypeList(Factory);

			CombineAssertions(() =>
			{
				AssertEquals("List Count", 140, list.Count);
				AssertEquals("Should include AE", true, list.ContainsCode("AE"));
				AssertEquals("Should include WB", true, list.ContainsCode("WB"));
			});
		}
	}
}
