using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MarketingManager.GUI.Testing
{
	[TestedType(typeof(UniqueDaysActivityCountFilter))]
	public class UniqueDaysActivityCountFilterTest : ModuleTextFilterTest
	{
		public void TestDefaultProperty()
		{
			UniqueDaysActivityCountFilter filter = (UniqueDaysActivityCountFilter)GetNewBusinessObject();
			AssertEquals(1, filter.DefaultProperty);
		}

		public void TestComparisonOperatorChanged()
		{
			UniqueDaysActivityCountFilter filter = (UniqueDaysActivityCountFilter)GetNewBusinessObject();
			filter.ComparisonOperator = Enterprise.MarketingManager.GUI.CampaignContactNumberFilter.ComparisonConstants.GreaterThanOrEqualTo;
			filter.Property = 0;
			filter.Validation.ValidateAll();
			AssertHasErrors(filter.PropertyInfo);
			filter.ComparisonOperator = Enterprise.MarketingManager.GUI.CampaignContactNumberFilter.ComparisonConstants.Exact;
			AssertNoErrors(filter.PropertyInfo);
			AssertEquals(0, filter.Property);
			filter.ComparisonOperator = Enterprise.MarketingManager.GUI.CampaignContactNumberFilter.ComparisonConstants.LessThan;
			AssertNoErrors(filter.PropertyInfo);
			AssertEquals(1, filter.Property);
		}

		#region Implementation

		protected override FilterCategory ExpectedDefaultCategory
		{
			get { return FilterCategories.TextSearch; }
		}

		public override void TestIsExpensiveQuery()
		{
			AssertEquals(false, Filter.IsExpensiveQuery);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new UniqueDaysActivityCountFilter("Test", DummyBizoSchema.Z0_Decimal);
		}

		#endregion
	}
}
