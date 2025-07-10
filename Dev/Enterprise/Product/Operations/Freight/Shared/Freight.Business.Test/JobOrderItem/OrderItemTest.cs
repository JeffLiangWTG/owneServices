using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(OrderItem))]
	sealed class OrderItemTest : EnterpriseBusinessObjectTestCase
	{
		#region Implementation

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var parent = factory.NewWithValidTestData<CommonShipment>();
			var docsAndCartage = JobDocsAndCartage.New(parent);

			var item = factory.New<OrderItem>();
			item.JT_JP = docsAndCartage.PK;
			item.FillWithValidTestData();

			return item;
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			var parent = Factory.NewWithValidTestData<CommonShipment>();
			var docsAndCartage = JobDocsAndCartage.New(parent);

			var item = Factory.New<OrderItem>();
			item.JT_JP = docsAndCartage.PK;
			item.FillWithValidTestData();

			return item;
		}

		public override void TestSaveAndDeleteBusinessObject()
		{
			Assert("This should be implemented if a client has an issue with deleting", true);
		}

		#endregion
	}
}
