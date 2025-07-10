using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using CargoWise.Data;

namespace Enterprise.MarketingManager.Business
{
	public static class ContactsAndUniqueOrganisations
	{
		[SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		public static ContactsAndUniqueOrganisationsData LoadEmailsAndOrgsCount(GlbCompanyCampaign campaign)
		{
			var result = new ContactsAndUniqueOrganisationsData();

			const string sql = "select TrackingStatus, EmailsCount, OrganisationsCount " +
							"from dbo.GetCampaignContactsAndUniqueOrganisations(@campaignPk, @campaignCategory, @campaignType)";

			using (var cmd = Db.Connection.Command(sql))
			{
				cmd.AddParameter("@campaignPk", SqlDbType.UniqueIdentifier,
					campaign != null ? campaign.PK.ToGuid() : Guid.Empty);
				cmd.AddParameter("@campaignCategory", SqlDbType.VarChar,
					campaign != null ? campaign.G0_Category.ToString() : string.Empty);
				cmd.AddParameter("@campaignType", SqlDbType.VarChar,
					campaign != null ? campaign.G0_Type.ToString() : string.Empty);

				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						var chartData = new TrackingStatusChartData
						{
							Status = reader.GetString(0),
							EmailsCount = reader.GetInt32(1),
							ClientsCount = reader.GetInt32(2)
						};

						if (chartData.Status == TrackingStatusChartData.TotalStatus)
						{
							result.TotalData = chartData;
							continue;
						}

						if (chartData.Status == TrackingStatusChartData.UnsubscribeStatus)
						{
							chartData.MultilingualTrackingStatus = TrackingStatusChartData.UnsubscribeDescription;
							result.UnsubscribedData = chartData;
							continue;
						}

						chartData.MultilingualTrackingStatus =
							new TrackingStatusCodes().GetMultilingualDescriptionFromCode(chartData.Status);
						result.DeliveryData.Add(chartData);
					}
				}
			}

			return result;
		}

		public class ContactsAndUniqueOrganisationsData
		{
			public Collection<TrackingStatusChartData> DeliveryData { get; } =
				new Collection<TrackingStatusChartData>();

			public TrackingStatusChartData UnsubscribedData { get; set; }
			public TrackingStatusChartData TotalData { get; set; }
		}
	}
}
