using CargoWise.Application;
using CargoWise.EntityFramework.Testing;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class EntryHeaderTypeListProviderTest : TestCaseWithFactory
	{
		public void TestGetEntryTypes()
		{
			var objectProvider = ObjectFactory.Get<Integration.Customs.IUSCusEntryHeaderTypeListProvider>();
			AssertEquals(typeof(EntryHeaderTypeListProvider), objectProvider.GetType());
			var expectedList = new[] { CusEntryHeaderMessageTypeList.Codes.EntrySummary, CusEntryHeaderMessageTypeList.Codes.Export };
			var actualList = ((CodeDescriptionPairList)objectProvider.GetEntryTypes()).GetAllCodes();
			AssertContainsExactElementsInAnyOrder(expectedList, actualList);
		}
	}
}
