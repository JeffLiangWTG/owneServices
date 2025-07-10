using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TR.NCTS.Business.Testing
{
	[TestedType(typeof(SPTSBillCollection))]
	public class SPTSBillCollectionTest : ActiveBusinessObjectCollectionTestCase<SPTSBillCollection>
	{
		protected override SPTSBillCollection GetCollectionToTest()
		{
			var cusInBondHeader = Factory.NewWithValidTestData<SPTSHeader>();
			return new SPTSBillCollection(cusInBondHeader);
		}
	}
}
