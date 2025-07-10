using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(ConsolUnAllocatedPackLinesView))]
	sealed class ConsolUnAllocatedPackLinesViewBOCollectionTest : BusinessObjectCollectionViewTestCase<ConsolUnAllocatedPackLinesView>
	{
		public void TestShouldIncludeThisPackLine()
		{
			var consol = Factory.New<CommonConsol>();
			var shipment1 = consol.Shipments.AddNew();
			var packLine1 = shipment1.OuterPackLines.AddNew();
			var shipment2 = Factory.New<CommonShipment>();
			var packLine2 = shipment2.OuterPackLines.AddNew();

			var collection = new PackLineNonDependentCollection(Factory);
			collection.Add(packLine1);
			collection.Add(packLine2);

			var packLinesView = new ConsolUnAllocatedPackLinesView(consol, collection);
			packLinesView.Rebuild();
			AssertCollectionContains(packLine1, packLinesView);
			AssertCollectionNotContains(packLine2, packLinesView);
		}

		protected override ConsolUnAllocatedPackLinesView GetCollectionToTest()
		{
			PackLineNonDependentCollection collection = new PackLineNonDependentCollection(Factory);
			return new ConsolUnAllocatedPackLinesView(null, collection);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			PackLine result = Factory.New<PackLine>();
			result.JL_FreightMode = FreightConstants.OuterPackType;
			result.JL_PackageCount = 2;
			return result;
		}
	}
}
