using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class DeclarationTypeListCodeDescriptionPairProviderTest : TestCase
	{
		public void TestICodeDescriptionPairListProviderMembers()
		{
			var factory = new BusinessObjectFactory();
			var list = new CodeDescriptionPairList();
			list.AddRange(TWRefCusCodeListLoader.GetImportDeclarationType(factory, ZDateTime.Today));
			list.AddRange(TWRefCusCodeListLoader.GetExportDeclarationType(factory, ZDateTime.Today));
			list.Sort();

			DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProvider provider = new OfficeOfReceiptListDescriptionPairProvider();
			AssertEquals(list, provider.GetCodeDescriptionPairList());
		}
	}
}
