using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MarketingManager.GUI
{
	public static class GetDistinctDaysCountQueryHelper
	{
		public static string GetDistinctDaysActivitySQL(SQLComparisonOperator comparisonOperator, ZInt value, ZGuid[] campaignPKs)
		{
			string pks = string.Join(", ", campaignPKs.Select(pk => "'" + pk + "'"));
			if (value.Equals(0))
			{
				const string zeroCountQuery =
@"{0} NOT IN
(
select GCC_G8_Recipient from dbo.GlbCompanyCampaignClick where GCC_G8_Recipient is not null
)
and G8_G0 in ({1})";
				return string.Format(CultureInfo.InvariantCulture, zeroCountQuery, GlbCompanyCampaignItem.Schema.PK, pks);
			}
			else
			{
				const string countQuery =
@"{0} in
(
SELECT GCC_G8_Recipient
FROM
              dbo.GlbCompanyCampaignClick
              JOIN dbo.GlbCompanyCampaignItem on GCC_G8_Recipient = G8_PK
			  CROSS APPLY (SELECT LocalDateTime FROM GetDateInLocalTime(GCC_PK, {2}, G8_RecipientTableCode, GCC_G8_Recipient)) AS LocalDateTimeTable
WHERE G8_G0 in ({1})
GROUP BY GCC_G8_Recipient
HAVING COUNT(DISTINCT LocalDateTime) {3} {4}
) ";

				return string.Format(CultureInfo.InvariantCulture,
					countQuery,
					GlbCompanyCampaignItem.Schema.PK,
					pks,
					GlbBranch.CurrentBranch.HomePort.StandardZoneUTCOffset,
					comparisonOperator.ComparisonText(value),
					value);
			}
		}
	}
}
