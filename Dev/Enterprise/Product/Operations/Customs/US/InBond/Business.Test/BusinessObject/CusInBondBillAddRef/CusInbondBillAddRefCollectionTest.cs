using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.InBond.Business.Testing
{
	[TestedType(typeof(CusInbondBillAddRefCollection))]
	sealed class CusInbondBillAddRefCollectionTest : ActiveBusinessObjectCollectionTestCase<CusInbondBillAddRefCollection>
	{
		protected override CusInbondBillAddRefCollection GetCollectionToTest()
		{
			CusInBondHeader header = Factory.New<CusInBondHeader>();
			CusInBondBill bill = header.Bills.AddNew();
			return new CusInbondBillAddRefCollection(bill);
		}
	}
}
