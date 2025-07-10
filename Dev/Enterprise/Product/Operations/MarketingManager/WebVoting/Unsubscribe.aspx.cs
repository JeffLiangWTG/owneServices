using System;
using CargoWise.EntityFramework;
using Enterprise.MarketingManager.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.WebVoting
{
	public partial class Unsubscribe : BasePage
	{
		protected override bool IsPersistDataSourceBetweenPostbacks => false;
		static string SuccessImagePath { get; } = "UnsubscribeImages/success.png";
		static string ErrorImagePath { get; } = "UnsubscribeImages/error.png";

		GlbCompanyCampaignUnsubscribe DataSourceGlbCompanyCampaignUnsubscribe => (GlbCompanyCampaignUnsubscribe)DataSource;

		protected void Page_Load(object sender, EventArgs e)
		{
			DataSourceGlbCompanyCampaignUnsubscribe.Process();
			ResultImage.Src = DataSourceGlbCompanyCampaignUnsubscribe.IsErrorResult
				? ErrorImagePath
				: SuccessImagePath;

			Resubscribe.InnerText = DataSourceGlbCompanyCampaignUnsubscribe.ResubscribeLabelMessage;
			Resubscribe.HRef = DataSourceGlbCompanyCampaignUnsubscribe.ResubscribeHref;
			Resubscribe.Visible = DataSourceGlbCompanyCampaignUnsubscribe.ResubscribeAvailable;
			DataSourceGlbCompanyCampaignUnsubscribe.SetContactVerified();
		}

		protected override BusinessObject GetNewDataSource()
		{
			var queryRequest = Request.QueryString;
			var campaignItemPk = SecureQueryStringHelper.GetZGuid(queryRequest, GlbCompanyCampaignItemSchema.Constants.PK);
			var unsubscribeType = SecureQueryStringHelper.GetValue(queryRequest, UnsubscribeUrlHelper.UnsubscribeTypeKey);
			var resubscribe = SecureQueryStringHelper.GetValue(queryRequest, UnsubscribeUrlHelper.ResubscribeKey);

			return new GlbCompanyCampaignUnsubscribe(Factory, campaignItemPk, unsubscribeType, resubscribe);
		}
	}
}
