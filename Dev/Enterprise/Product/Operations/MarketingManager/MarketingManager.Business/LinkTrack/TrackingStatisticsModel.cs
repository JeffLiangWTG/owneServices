using System;
using System.Collections.Generic;
using CargoWise.Data;
using CargoWise.Types;

namespace Enterprise.MarketingManager.Business
{
	public static class TrackingStatisticsModel
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "SQL query string param const string")]
		public static void LoadClickCounts(List<CampaignClicksPerInterval> counts,
			ZGuid campaignPk,
			ZDateTime startDateInclusive,
			ZDateTime endDateExclusive,
			int intervals,
			List<CampaignClicksByUrl> clicksByUrl
			)
		{
			counts.Clear();
			clicksByUrl.Clear();

			const string sql =
				"select IntervalIndex, StartDateInclusive, EndDateExclusive, LinkPk, ClickCount, UniqueClickCount " +
				"from dbo.GetCampaignClickCountsPerInterval(@campaignPk, @intervals, @startDateInclusive, @endDateExclusive) " +
				"order by IntervalIndex, LinkPk; " +
				"select URL, UniqueClickCount " +
				"from dbo.GetCampaignUniqueClickCountsByUrl(@campaignPk, @startDateInclusive, @endDateExclusive)";

			using (var cmd = Db.Connection.Command(sql))
			{
				cmd.AddParameter("@campaignPk", System.Data.SqlDbType.UniqueIdentifier, campaignPk.ToGuid());
				cmd.AddParameter("@intervals", System.Data.SqlDbType.Int, intervals);
				cmd.AddParameter("@startDateInclusive", System.Data.SqlDbType.DateTime, !startDateInclusive.IsEmpty ? startDateInclusive.ToDateTime() : DBNull.Value);
				cmd.AddParameter("@endDateExclusive", System.Data.SqlDbType.DateTime, !endDateExclusive.IsEmpty ? endDateExclusive.ToDateTime() : DBNull.Value);

				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						counts.Add(new CampaignClicksPerInterval()
						{
							IntervalIndex = reader.GetInt32(0),
							StartDateInclusive = reader.GetDateTime(1),
							EndDateExclusive = reader.GetDateTime(2),
							LinkPk = !reader.IsDBNull(3) ? reader.GetGuid(3) : ZGuid.Empty,
							ClickCount = reader.GetInt32(4),
							UniqueClickCount = reader.GetInt32(5)
						});
					}

					if (reader.NextResult())
					{
						while (reader.Read())
						{
							clicksByUrl.Add(new CampaignClicksByUrl()
							{
								LinkURL = reader.GetString(0),
								UniqueClickCount = reader.GetInt32(1)
							});
						}
					}
				}
			}
		}
	}
}
