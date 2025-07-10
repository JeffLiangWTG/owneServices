using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	public class DutiableUQListTest : TestCase
	{
		public void TestDutiableUQList()
		{
			DutiableUQList dutiableUQList = new DutiableUQList();
			AssertEquals(5, dutiableUQList.Count);
			Assert(dutiableUQList.ContainsCode(UnitOfQuantityCodeList.Codes.KGM));
			Assert(dutiableUQList.ContainsCode(UnitOfQuantityCodeList.Codes.LTR));
			Assert(dutiableUQList.ContainsCode(UnitOfQuantityCodeList.Codes.NMB));
			Assert(dutiableUQList.ContainsCode(UnitOfQuantityCodeList.Codes.STK));
			Assert(dutiableUQList.ContainsCode(UnitOfQuantityCodeList.Codes.DAL));
		}
	}
}
