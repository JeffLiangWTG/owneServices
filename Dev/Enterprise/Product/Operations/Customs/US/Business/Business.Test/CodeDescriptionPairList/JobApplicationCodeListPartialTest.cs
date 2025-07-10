using NUnit.Framework;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class BrokerJobApplicationCodeListTest : TestCase
	{
		public void TestGetCodeDescriptionPairList()
		{
			JobApplicationCodeList list = new JobApplicationCodeList();
			ReadOnlyCodeDescriptionPairList iList = ((DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProvider)list).GetCodeDescriptionPairList();

			AssertEquals(true, iList.ContainsCode(JobApplicationCodeList.Codes.ACE));
			AssertEquals(true, iList.ContainsCode(JobApplicationCodeList.Codes.ACS));
		}
	}
}
