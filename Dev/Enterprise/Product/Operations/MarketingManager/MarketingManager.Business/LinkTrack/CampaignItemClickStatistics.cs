using System.Collections.Generic;
using CargoWise.Data;
using CargoWise.Types;

namespace Enterprise.MarketingManager.Business
{
	public static class CampaignItemClickStatistics
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		public static void LoadClickCounts(List<CampaignItemClicks> counts, ZGuid campaignItemPK, ZString reportBy)
		{
			counts.Clear();

			const string sql =
				"select FirstClick, LastClick, ClickCount, LinkContext, LinkUrl, IsImage " +
				"from dbo.GetCampaignItemClicks(@campaignItemPk, @reportBy)";

			using (var cmd = Db.Connection.Command(sql))
			{
				cmd.AddParameter("@campaignItemPk", System.Data.SqlDbType.UniqueIdentifier, campaignItemPK.ToGuid());
				cmd.AddParameter("@reportBy", System.Data.SqlDbType.VarChar, reportBy.ToString());

				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						counts.Add(new CampaignItemClicks()
						{
							FirstClickDateTime = reader.GetDateTime(0),
							LastClickDateTime = reader.GetDateTime(1),
							ClickCount = reader.GetInt32(2),
							LinkContext = !reader.IsDBNull(3) ? reader.GetString(3) : "",
							LinkUrl = !reader.IsDBNull(4) ? reader.GetString(4) : "",
							IsImage = reader.GetBoolean(5)
						});
					}
				}
			}
		}
	}
}
