using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	sealed class BrokerYesNoListTest : TestCase
	{
		public void TestGetCodeDescriptionPairList()
		{
			YesNoList list = new YesNoList();
			ReadOnlyCodeDescriptionPairList iList = ((DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProvider)list).GetCodeDescriptionPairList();

			AssertEquals(true, iList.ContainsCode(YesNoList.Codes.No));
			AssertEquals(true, iList.ContainsCode(YesNoList.Codes.Yes));
			Assert(YesNoList.IsYes(YesNoList.Codes.Yes));
			Assert(YesNoList.IsYesOrNo(YesNoList.Codes.Yes));
			Assert(!YesNoList.IsNo(YesNoList.Codes.Yes));
			Assert(!YesNoList.IsYes(YesNoList.Codes.No));
			Assert(YesNoList.IsYesOrNo(YesNoList.Codes.No));
			Assert(YesNoList.IsNo(YesNoList.Codes.No));
		}
	}
}
