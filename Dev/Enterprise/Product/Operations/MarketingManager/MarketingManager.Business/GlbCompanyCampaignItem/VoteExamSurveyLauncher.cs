using System.Collections.Generic;
using System.Net;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.Recruiter;

namespace Enterprise.MarketingManager.Business
{
	public class VoteExamSurveyLauncher : NonPersistentBusinessObject, IObsoleteValidation
	{
		public VoteExamSurveyLauncher(BusinessObjectFactory factory, ZGuid campaignItemPK, ZGuid campaignPK, string countryCode, string examSettingsCode)
			: this(factory, campaignItemPK, campaignPK, countryCode, ZDate.Empty, examSettingsCode)
		{
		}

		public VoteExamSurveyLauncher(BusinessObjectFactory factory, ZGuid campaignItemPK, ZGuid campaignPK, string countryCode, ZDate testExpiry,
			string examSettingsCode, ZGuid recipientPK = default(ZGuid), string recipientTableCode = "")
			: base(factory)
		{
			this.campaignItemPK = campaignItemPK;
			this.campaignPK = campaignPK;
			this.countryCode = countryCode;
			this.testExpiry = testExpiry;
			examVersion = ObjectFactory.Get<IExamSettingCodeHelper>().GetExamVersion(examSettingsCode, campaignItemPK, factory);
		}

		readonly ZGuid campaignItemPK;
		readonly ZGuid campaignPK;
		readonly string countryCode;
		readonly ZDate testExpiry;
		readonly string examVersion;

		#region VoteExamSurveyDetails

		public IEnumerable<KeyValuePair<string, string>> VoteExamSurveyDetails
		{
			get
			{
				if (Campaign != null)
				{
					foreach (var detailLine in Campaign.GetVoteExamSurveyDetails(countryCode))
					{
						if (!string.IsNullOrEmpty(detailLine.Key) && string.IsNullOrEmpty(detailLine.Value))
						{
							continue;
						}

						yield return new KeyValuePair<string, string>(detailLine.Key, RenderTextForWeb(detailLine.Value));
					}
				}
			}
		}

		#endregion

		#region Properties

		public bool CanRedirect
		{
			get { return ErrorMessage.IsEmpty; }
		}

		public ZString ErrorMessage
		{
			get
			{
				ZString result = "";

				GlbCompanyCampaign campaign = Campaign;
				if (campaign == null)
				{
					result = Res.GetString("32a7a37b-00f4-4217-88d1-596b6a2d0137", "Either your session has expired or the campaign URL has been manually altered.");
				}
				else if (campaign?.HasEnded ?? false)
				{
					result = Res.GetString("8c54a7c8-061c-4151-95cb-e247334813eb", "This campaign has ended.");
				}
				else if (CampaignItem != null && CampaignItem.HasPreviousSessionEnded && !CampaignItem.CanResetVoteSurveyExam)
				{
					result = CampaignItem.HasPreviousSessionEndedMessage;
				}
				else if (!testExpiry.IsEmpty && testExpiry < ZDateTime.Today.Date)
				{
					result = Res.GetString("2A0A930E-6806-4ae4-937D-E306C591729A", "Your test link has expired.");
				}

				return result;
			}
		}

		public ZPropertyInfo ErrorMessageInfo
		{
			get { return GetZPropertyInfo(nameof(ErrorMessage)); }
		}

		public bool ShouldAutoLaunch
		{
			get { return CanRedirect && CampaignItem != null; }
		}

		GlbCompanyCampaignItem CampaignItem
		{
			get { return Factory.Load<GlbCompanyCampaignItem>(campaignItemPK); }
		}

		public GlbCompanyCampaign Campaign
		{
			get
			{
				GlbCompanyCampaignItem campaignItem = CampaignItem;

				var result = (campaignItem != null)
					? campaignItem.CompanyCampaign
					: Factory.Load<GlbCompanyCampaign>(campaignPK);

				if (result != null)
				{
					result.SetCurrentSettings(new GlbCompanyCampaign.CampaignSettings(examVersion));
				}

				return result;
			}
		}

		#endregion

		#region RenderTextForWeb

		public static string RenderTextForWeb(string text)
		{
			return !string.IsNullOrEmpty(text) ? WebUtility.HtmlEncode(text).Replace("\r\n", "<br/>").Replace("\t", "&nbsp;&nbsp;&nbsp;&nbsp;") : "";
		}
		#endregion
	}
}
