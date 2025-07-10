using NUnit.Framework;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class CRLReleaseStatusListTest : TestCase
	{
		public void TestGetCodeDescriptionPairList()
		{
			CRLReleaseStatusList list = new CRLReleaseStatusList();
			ReadOnlyCodeDescriptionPairList iList = ((DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProvider)list).GetCodeDescriptionPairList();

			AssertEquals(true, iList.ContainsCode(CRLReleaseStatusList.Codes.CAN));
			AssertEquals(true, iList.ContainsCode(CRLReleaseStatusList.Codes.DEL));
			AssertEquals(true, iList.ContainsCode(CRLReleaseStatusList.Codes.EXM));
			AssertEquals(true, iList.ContainsCode(CRLReleaseStatusList.Codes.HLD));
			AssertEquals(true, iList.ContainsCode(CRLReleaseStatusList.Codes.NRL));
			AssertEquals(true, iList.ContainsCode(CRLReleaseStatusList.Codes.REL));
		}
	}
}
