using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class OfficeOfReceiptListDescriptionPairProviderTest : TestCase
	{
		public void TestICodeDescriptionPairListProviderMembers()
		{
			var factory = new BusinessObjectFactory();
			var list = TWRefCusCodeListTypes.GetCustomsOfficeList(factory);
			DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProvider provider = new OfficeOfReceiptListDescriptionPairProvider();
			AssertEquals(list, provider.GetCodeDescriptionPairList());
		}
	}
}
