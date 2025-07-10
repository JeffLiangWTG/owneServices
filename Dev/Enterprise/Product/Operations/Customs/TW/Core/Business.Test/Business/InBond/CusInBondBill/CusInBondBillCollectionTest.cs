using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(CusInBondBillCollection))]
	sealed class CusInBondBillCollectionTest : ActiveBusinessObjectCollectionTestCase<CusInBondBillCollection>
	{
		protected override CusInBondBillCollection GetCollectionToTest()
		{
			var cusInBondHeader = Factory.NewWithValidTestData<CusInBondHeader>();
			return new CusInBondBillCollection(cusInBondHeader);
		}
	}
}
