using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Warehouse.Environment.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Business.Testing
{
	[TestedType(typeof(SalesDashboardActivityCollection))]
	sealed class SalesDashboardActivityCollectionTest : BusinessObjectCollectionTestCase
	{
		#region Implementation

		public void TestConstructors()
		{
			var collection = new SalesDashboardActivityCollection(Factory);
			AssertNotNull(collection);

			var mockRefreshable = new Mock<IModuleGridCollectionRefreshable>();
			mockRefreshable.Setup(m => m.GetTableNamesToMonitor()).Returns(new[] { "Test Table" });

			var collectionRefreshable = new SalesDashboardActivityCollection(Factory, mockRefreshable.Object);
			AssertNotNull(collectionRefreshable);
		}

		protected sealed override BusinessObjectCollection GetCollectionToTest()
		{
			return new SalesDashboardActivityCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<OpportunitySalesDashboardActivity>();
		}

		#endregion
	}
}
