using NUnit.Framework;

namespace Enterprise.Customs.US.ISF.Business.Testing
{
	sealed class TransportModeCodesTest : TestCase
	{
		public void TestICodeDescriptionPairListProviderMembers()
		{
			var list = new TransportModeCodes();
			AssertEquals(list, list.GetCodeDescriptionPairList());
		}
	}
}
