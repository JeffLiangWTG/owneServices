using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Rating.Business.Testing
{
	[TestedType(typeof(QuotationProcessTaskCollection))]
	public class QuotationProcessTaskCollectionTest : RatingHeaderProcessTaskCollectionTest<Quote, QuotationProcessTaskCollection>
	{
		public void TestAdditionalFilter()
		{
			var processTaskCollection = new QuotationProcessTaskCollectionForTest(RatingHeader);
			var expectedFilter = new ZQuery(ProcessTasksSchema.P9_ParentTableCode, RatingHeaderSchema.Constants.Prefix);
			AssertCollectionContains(expectedFilter, processTaskCollection.AdditionalFilter.GetCompositeParts());
		}

		#region Implementation

		protected override QuotationProcessTaskCollection GetCollectionToTestCore()
		{
			return new QuotationProcessTaskCollection(RatingHeader);
		}

		class QuotationProcessTaskCollectionForTest : QuotationProcessTaskCollection
		{
			public QuotationProcessTaskCollectionForTest(Quote quote)
				: base(quote)
			{
			}

			public new ZQuery AdditionalFilter
			{
				get { return base.AdditionalFilter; }
			}
		}

		#endregion
	}
}
