using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Rating.Business.Testing;
using Enterprise.Rating.Rateable;
using Moq;

namespace Enterprise.Rating.Business.Test
{
	public class ProductComparerTest : TestCaseWithFactory
	{
		public void TestGetSimilarity()
		{
			var rate = Helper.NewClientRate(Helper.NewOrgHeader());
			var productPK = Helper.NewOrgSupplierPart(rate.Header).PK;
			var entry1 = rate.AddRateEntry("FCL", "SEA", "AUSYD", "USLAX", "STD", "");
			var rateLine1 = entry1.RateLines.AddNew();
			var rateLine2 = entry1.RateLines.AddNew();
			rateLine2.TL_OP_ProductNumber = productPK;

			var criteria = new TestRatingCriteria();
			var lineProvider = new FastLineProvider(criteria);
			var line1 = lineProvider.GetOrCreate(rateLine1);
			var line2 = lineProvider.GetOrCreate(rateLine2);

			var measuresMock = new Mock<IDistinctPartRateDimensions>();
			var measures = measuresMock.Object;

			var partListMock = new Mock<IHasPartDimensions>();
			partListMock.Setup(x => x.HasProduct).Returns(true);
			var partList = partListMock.Object;

			var partNoProduct = CreatePart(ZGuid.Empty);
			var partProduct = CreatePart(productPK);

			var comparer = new ProductComparer();

			CombineAssertions(() =>
			{
				AssertEquals("line with no product vs part with no product", Similarity.Exact, comparer.GetSimilarity(line1, measures, partList, partNoProduct));
				AssertEquals("line with no product vs part with product", Similarity.Generic, comparer.GetSimilarity(line1, measures, partList, partProduct));

				AssertEquals("line with product vs part with no product", Similarity.None, comparer.GetSimilarity(line2, measures, partList, partNoProduct));
				AssertEquals("line with product vs part with product", Similarity.Exact, comparer.GetSimilarity(line2, measures, partList, partProduct));
			});
		}

		static IRateablePart CreatePart(ZGuid productPk)
		{
			var partMock = new Mock<IRateablePart>();
			partMock.Setup(x => x.ProductPk).Returns(NullableHelper.ToNullable(productPk));
			return partMock.Object;
		}

		protected TestHelper Helper => testHelper ?? (testHelper = new TestHelper(Factory));
		TestHelper testHelper;
	}
}
