using NUnit.Framework;

namespace Enterprise.Customs.US.ISF.Business.Testing
{
	sealed class ActionReasonCodeListTest : TestCase
	{
		public void TestICodeDescriptionPairListProviderMembers()
		{
			var list = new ActionReasonCodeList();
			AssertEquals(list, list.GetCodeDescriptionPairList());
		}
	}
}
