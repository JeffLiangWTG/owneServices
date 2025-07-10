using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Rating.Business.Testing;
using Enterprise.Rating.Rateable;
using Moq;

namespace Enterprise.Rating.Business.Test
{
	public class WarehouseComparerTest : TestCaseWithFactory
	{
		public void TestGetSimilarity()
		{
			var warehousePk1 = Guid.NewGuid();
			var warehousePk2 = Guid.NewGuid();

			var rate = Helper.NewClientRate(Helper.NewOrgHeader());
			var entry1 = rate.AddRateEntry(RatingConstants.RateCategory.WHS);
			entry1.TI_WW_Warehouse = warehousePk1;
			var line1 = entry1.AddRateLine("ODOC");

			var entry2 = rate.AddRateEntry(RatingConstants.RateCategory.WHS);
			entry2.TI_WW_Warehouse = warehousePk2;
			var line2 = entry2.AddRateLine("ODOC");

			// matches all warehouses
			var entry3 = rate.AddRateEntry(RatingConstants.RateCategory.WHS);
			var line3 = entry3.AddRateLine("ODOC");

			var criteria = new TestRatingCriteria();

			var measuresMock = new Mock<IDistinctPartRateDimensions>();
			var measures = measuresMock.Object;

			var partListMock = new Mock<IHasPartDimensions>();
			partListMock.Setup(x => x.HasWarehouse).Returns(true);
			var partList = partListMock.Object;

			var part1 = CreatePart(warehousePk1);

			var lineProvider = new FastLineProvider(criteria);
			var fastLine1 = lineProvider.GetOrCreate(line1);
			var fastLine2 = lineProvider.GetOrCreate(line2);
			var fastLine3 = lineProvider.GetOrCreate(line3);

			var comparer = new WarehouseComparer();
			AssertEquals("same warehouse on line and part", Similarity.Exact, comparer.GetSimilarity(fastLine1, measures, partList, part1, null));
			AssertEquals("different warehouse on line and part", Similarity.None, comparer.GetSimilarity(fastLine2, measures, partList, part1, null));
			AssertEquals("part has warehouse, entry does not", Similarity.Generic, comparer.GetSimilarity(fastLine3, measures, partList, part1, null));
		}

		static IRateablePart CreatePart(Guid? warehousePk)
		{
			var partMock = new Mock<IRateablePart>();
			partMock.Setup(x => x.WarehousePk).Returns(warehousePk);
			return partMock.Object;
		}

		protected TestHelper Helper => testHelper ?? (testHelper = new TestHelper(Factory));
		TestHelper testHelper;
	}
}
