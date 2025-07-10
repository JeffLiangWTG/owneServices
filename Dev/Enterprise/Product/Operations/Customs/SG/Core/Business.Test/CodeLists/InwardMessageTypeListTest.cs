using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	public class InwardMessageTypeListTest : TestCase
	{
		public void TestInwardMessageTypeList()
		{
			InwardMessageTypeList inwardMessageTypeList = new InwardMessageTypeList();
			Assert(inwardMessageTypeList is InwardMessageTypeList);
			Assert(inwardMessageTypeList.ContainsCode("INP"));
			Assert(inwardMessageTypeList.ContainsCode("IPT"));
			AssertEquals(2, inwardMessageTypeList.Count);
		}
	}
}
