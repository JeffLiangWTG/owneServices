using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.MarketingManager.GUI.Testing
{
	public class GetDistinctDaysCountQueryHelperTest : TestCase
	{
		public void TestSubQueryWithNullableColumn()
		{
			var distinctDaysActivitySql = GetDistinctDaysCountQueryHelper.GetDistinctDaysActivitySQL(SQLComparisonOperator.Equal, 0, new[] { ZGuid.Empty });
			AssertContains("select GCC_G8_Recipient from dbo.GlbCompanyCampaignClick where GCC_G8_Recipient is not null", distinctDaysActivitySql);
		}
	}
}
