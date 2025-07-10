using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.Business.Testing
{
	sealed class CusInBondEventBaseOnlyTest : TestCaseWithFactory
	{
		public void TestTypeDecider()
		{
			AssertType<CusInBondEventTypeDecider>(CusInBondEvent.TypeDecider);
		}

		public void TestICusGoodsLocationTypeSupporter()
		{
			var cusInBondEventForTest = Factory.New<CusInBondEventForTest>();
			AssertEquals(typeof(CusGoodsLocation), (cusInBondEventForTest as ICusGoodsLocationTypeSupporter).GoodsLocationType);
		}
	}

	sealed class CusInBondEventForTest : CusInBondEvent
	{
		public CusInBondEventForTest(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
	}
}
