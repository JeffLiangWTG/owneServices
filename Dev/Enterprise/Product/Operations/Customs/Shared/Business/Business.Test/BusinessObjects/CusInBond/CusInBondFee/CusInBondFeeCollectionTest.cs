using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CusInBondFeeCollection<CusInBondFeeForTest>))]
	sealed class CusInBondFeeCollectionTest : ActiveBusinessObjectCollectionTestCase<CusInBondFeeCollection<CusInBondFeeForTest>>
	{
		protected override CusInBondFeeCollection<CusInBondFeeForTest> GetCollectionToTest() => new CusInBondFeeCollection<CusInBondFeeForTest>(goodItem);

		protected override void SetUp()
		{
			base.SetUp();
			var header = (CusInBondHeader)Factory.New<Integration.Customs.EU.NCTS.ICusInBondHeader>();
			header.BH_HeaderType = "D";
			var bill = (CusInBondBill)Factory.New<Integration.Customs.EU.NCTS.ICusInBondBill>();
			bill.B0_BH = header.PK;
			goodItem = (CusInBondCargoDesc)Factory.New<Integration.Customs.EU.NCTS.IDepartureCargoDesc>();
			goodItem.BY_ParentID = bill.PK;
			goodItem.BY_ParentTableCode = bill.TablePrefix;
		}
		CusInBondCargoDesc goodItem;
	}
}
