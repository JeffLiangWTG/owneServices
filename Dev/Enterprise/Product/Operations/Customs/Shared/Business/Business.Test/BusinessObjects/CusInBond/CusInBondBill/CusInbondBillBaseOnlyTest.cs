using CargoWise.Application;
using CargoWise.Application.Testing;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.Business.Testing
{
	sealed class CusInbondBillBaseOnlyTest : TestCaseWithFactory
	{
		public void TestHeader()
		{
			var header = Factory.New<CusInBondHeaderForTesting>();
			header.BH_ApplicationCode = "TST";
			var types = new KeyObjectHandleDictionaryObject
			{
				{ "TST", new TestObjectHandle(header) }
			};

			using (ObjectFactory.Substitute("CusInBondHeaderApplicationCodeTypes", types))
			{
				var bill = Factory.New<CusInBondBillForTesting>();
				bill.B0_BH = header.PK;
				AssertEquals(header, bill.Header);
				bill.B0_BH = ZGuid.Empty;
				AssertNull(bill.Header);
			}
		}
	}
}
