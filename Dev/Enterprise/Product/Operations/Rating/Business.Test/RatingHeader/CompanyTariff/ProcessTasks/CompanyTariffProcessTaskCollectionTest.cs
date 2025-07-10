using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Rating.Business.Testing
{
	[TestedType(typeof(CompanyTariffProcessTaskCollection))]
	public class CompanyTariffProcessTaskCollectionTest : RatingHeaderProcessTaskCollectionTest<CompanyTariff, CompanyTariffProcessTaskCollection>
	{
		public void TestAdditionalFilter()
		{
			var processTaskCollection = new CompanyTariffProcessTaskCollectionForTest(RatingHeader);
			var expectedFilter = new ZQuery(ProcessTasksSchema.P9_ParentTableCode, RatingHeaderSchema.Constants.Prefix);
			AssertCollectionContains(expectedFilter, processTaskCollection.AdditionalFilter.GetCompositeParts());
		}

		#region Implementation

		protected override CompanyTariffProcessTaskCollection GetCollectionToTestCore()
		{
			return new CompanyTariffProcessTaskCollection(RatingHeader);
		}

		class CompanyTariffProcessTaskCollectionForTest : CompanyTariffProcessTaskCollection
		{
			public CompanyTariffProcessTaskCollectionForTest(CompanyTariff companyTariff)
				: base(companyTariff)
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
