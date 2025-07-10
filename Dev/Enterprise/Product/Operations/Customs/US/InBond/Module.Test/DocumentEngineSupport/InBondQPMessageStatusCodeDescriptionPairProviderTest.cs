using Enterprise.DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProviderTesting;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.InBond.Module.Testing
{
	sealed class InBondQPMessageStatusCodeDescriptionPairProviderTest : CodeDescriptionPairListProviderTest
	{
		public override void TestIsReturningCorrectCollection()
		{
			ReadOnlyCodeDescriptionPairList actualList = CreateCodeDescriptionPairListProvider().GetCodeDescriptionPairList();
			CodeDescriptionPairList expectedList = new CusInBondHeaderFilterStripBusinessObject().Lookups.InbondQPMessageStatusListForFilter;
			AssertEquals(expectedList.Count, actualList.Count);
			for (int i = 0; i < expectedList.Count; i++)
			{
				AssertEquals(expectedList[i].Code, actualList[i].Code);
				AssertEquals(expectedList[i].Description, actualList[i].Description);
			}
		}

		protected override DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProvider CreateCodeDescriptionPairListProvider() => new InBondQPMessageStatusCodeDescriptionPairProvider();
	}
}
