using CargoWise.EntityFramework;
using Enterprise.DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProviderTesting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using ICodeDescriptionPairListProvider = Enterprise.DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProvider;

namespace Enterprise.Customs.Module.Testing
{
	sealed class DeclarationServiceLevelCodeDescriptionPairProviderTest : CodeDescriptionPairListProviderTest
	{
		public override void TestIsReturningCorrectCollection()
		{
			var list = CreateCodeDescriptionPairListProvider().GetCodeDescriptionPairList();
			AssertListEqual(list, ServiceLevelList);
			foreach (var item in list)
			{
				Assert(item is CodeDescriptionPair);
			}
		}

		protected override ICodeDescriptionPairListProvider CreateCodeDescriptionPairListProvider()
		{
			return new DeclarationServiceLevelCodeDescriptionPairProvider();
		}

		CodeDescriptionPairList ServiceLevelList
		{
			get
			{
				if (fServiceLevelList == null)
				{
					fServiceLevelList = new CodeDescriptionPairList();
					fServiceLevelList.AddRange(new RefServiceLevelCollection(new BusinessObjectFactory()));
					fServiceLevelList.SortByDescription();
				}

				return fServiceLevelList;
			}
		}

		CodeDescriptionPairList fServiceLevelList;
	}
}
