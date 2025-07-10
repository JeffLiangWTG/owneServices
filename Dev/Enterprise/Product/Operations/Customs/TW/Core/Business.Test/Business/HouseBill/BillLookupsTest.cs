using NUnit.Framework;
namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class BillLookupsTest : Customs.Business.Testing.CusDecHouseBillLookupsTest
	{
		[ExpectNoExceptions]
		public void TestHouseBill()
		{
			Bill parent = Factory.New<Bill>();
			NUnit.Framework.Assert.That(parent, NUnit.Framework.Is.EqualTo(parent.Lookups.HouseBill));
		}
	}
}
