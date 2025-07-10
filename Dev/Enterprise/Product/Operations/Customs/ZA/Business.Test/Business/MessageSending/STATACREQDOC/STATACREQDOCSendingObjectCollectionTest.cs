using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ZA.DataRegistry.Business;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.Testing
{
	[TestedType(typeof(STATACREQDOCSendingObjectCollection))]
	sealed class STATACREQDOCSendingObjectCollectionTest : NonPersistentBusinessObjectCollectionTestCase<STATACREQDOCSendingObjectCollection>
	{
		public void TestAllowNew()
		{
			Assert(!GetCollectionToTest().AllowNew);
		}

		protected override STATACREQDOCSendingObjectCollection GetCollectionToTest()
		{
			return new STATACREQDOCSendingObjectCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new STATACREQDOCSendingObject(new STATACREQDOCSendingObjectParent(Factory), mapping);
		}

		protected override void SetUp()
		{
			base.SetUp();
			mapping = new FinancialAccountNumberPortMap();
		}

		FinancialAccountNumberPortMap mapping;
	}
}
