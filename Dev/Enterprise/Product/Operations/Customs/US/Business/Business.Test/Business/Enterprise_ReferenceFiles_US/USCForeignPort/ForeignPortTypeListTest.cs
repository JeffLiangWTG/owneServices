using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	class ForeignPortTypeListTest : TestCase
	{
		public void TestGetCodeFromType()
		{
			AssertEquals(ForeignPortTypeList.Codes.AES, ForeignPortTypeList.GetCodeFromType(USCForeignPortWrapper.Type.AES));
			AssertEquals(ForeignPortTypeList.Codes.Common, ForeignPortTypeList.GetCodeFromType(USCForeignPortWrapper.Type.Common));
			AssertEquals(ForeignPortTypeList.Codes.InBond, ForeignPortTypeList.GetCodeFromType(USCForeignPortWrapper.Type.InBond));
		}
	}
}
