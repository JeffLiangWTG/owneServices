
using System.Web;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MarketingManager.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.WebVoting
{
	public static class ClientSpecificRequestHandlerHelper
	{
		public static WebThemeCustomObject FindTheme(HttpContext context)
		{
			var companyCode = GetCompanyCodeFromUrlReferrer(context);
			var themeUrl = WebDataRegistry.Instance.WebCampaignCustomThemeUrl.Value.Find(context.Request.Url.AbsoluteUri, companyCode);
			var themeName = themeUrl?.ThemeName ?? ZString.Empty;
			var theme = WebDataRegistry.Instance.WebCampaignCustomTheme.Value.Find(themeName);
			return theme;
		}

		static string GetCompanyCodeFromUrlReferrer(HttpContext context)
		{
			var result = string.Empty;

			if (context.Request.UrlReferrer != null)
			{
				var query = context.Request?.UrlReferrer?.Query;
				if (query != null)
				{
					var queryString = HttpUtility.ParseQueryString(query)[VoteExamSurveyUrlHelper.VoteExamSurveyQueryStringKey];
					if (!queryString.IsNullOrEmpty())
					{
						result = GetCompanyCodeFromQueryString(queryString);
					}
				}
			}

			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults")]
		public static string GetCompanyCodeFromQueryString(string queryString)
		{
			var result = string.Empty;
			SecureQueryString secureQueryString;
			try
			{
				secureQueryString = new SecureQueryString(queryString);
			}
			catch (QueryStringException)
			{
				return string.Empty;
			}
			var campaignPkText = secureQueryString[GlbCompanyCampaignSchema.Constants.PK];
			ZGuid campaignPk = ZGuid.Empty;
			ZGuid.TryParse(campaignPkText, out campaignPk);

			var campaignItemPkText = secureQueryString[GlbCompanyCampaignItemSchema.Constants.PK];
			ZGuid campaignItemPk = ZGuid.Empty;
			ZGuid.TryParse(campaignItemPkText, out campaignItemPk);

			var factory = new BusinessObjectFactory();
			if (!campaignPk.IsEmpty)
			{
				var campaign = factory.Load<GlbCompanyCampaign>(campaignPk);
				result = campaign?.Company?.GC_Code ?? string.Empty;
			}
			else if (!campaignItemPk.IsEmpty)
			{
				var campaignItem = factory.Load<GlbCompanyCampaignItem>(campaignItemPk);
				result = campaignItem?.CompanyCampaign?.Company?.GC_Code ?? string.Empty;
			}
			return result;
		}
	}
}
