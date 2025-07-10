using CargoWise.EntityFramework.Testing;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class SecondarySpecProgIndicatorListTest : TestCaseWithFactory
	{
		public void TestGetCodeDescriptionPairList()
		{
			ICodeDescriptionPairListProvider listProvider = new CodeDescriptionPairListProvider(() => new SecondarySpecProgIndicatorList());

			AssertEquals("ICodeDescriptionPairListProvider returns the same list", listProvider.CodeDescriptionPairList, new SecondarySpecProgIndicatorList());
		}
	}
}
