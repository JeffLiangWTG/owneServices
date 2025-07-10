using System;
using System.Text.RegularExpressions;
using System.Web.UI.WebControls;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.MarketingManager.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.MarketingManager.WebVoting
{
	public partial class SubscribePreference : BasePage
	{
		protected override bool IsPersistDataSourceBetweenPostbacks => false;
		protected static string SuccessImagePath { get; } = "UnsubscribeImages/success.png";
		protected static string ErrorImagePath { get; } = "UnsubscribeImages/error.png";

		protected SubscriptionPreferenceBusinessObjectAdapter DataSourceGlbCompanyCampaignUnsubscribe => (SubscriptionPreferenceBusinessObjectAdapter)DataSource;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1044:FactoryGetDatabaseCountCollectionCountRule", Justification = "Baseline")]
		protected void Page_Load(object sender, EventArgs e)
		{
			if (DataSourceGlbCompanyCampaignUnsubscribe == null)
			{
				HidePage(Res.GetString("bd60de4a-d040-49ee-a500-680247610719", "This URL is invalid or your session has expired"));
				return;
			}

			if (DataSourceGlbCompanyCampaignUnsubscribe.CampaignItemPk.IsEmpty && DataSourceGlbCompanyCampaignUnsubscribe.Contact == null)
			{
				HidePage(Res.GetString("e97d4490-afa5-4ffb-a035-ad0ec419763d", "This page is unavailable for support login"));
				return;
			}

			ResultImage.Src = SuccessImagePath;
			ResultImage.Visible = false;
			var list = DataSourceGlbCompanyCampaignUnsubscribe.SubscriptionList;
			DataSourceGlbCompanyCampaignUnsubscribe.SetContactVerified();
			if (list?.Count == 0)
			{
				ShowResult();
			}
			else
			{
				SubmitButton.Text = Res.GetString("e7a2626f-159f-47e5-9738-e0ff100dd98d", "SUBMIT");
				SubscriptionPreferenceLabel.Text = DataSourceGlbCompanyCampaignUnsubscribe.SubscripitionPreferencePageTitle;
				string originalMsg = DataSourceGlbCompanyCampaignUnsubscribe.SubscripitionPreferencePageMessage;
				string pageDesc = Regex.Replace(originalMsg, @"\r\n?|\n", "<br />");

				if (DataSourceGlbCompanyCampaignUnsubscribe.CampaignItemPk.IsEmpty)
				{
					var parser = new ContactDocumentParser(Factory);
					SubscriptionPreferenceDescLabel.Text = parser.Parse(DataSourceGlbCompanyCampaignUnsubscribe.Contact, pageDesc);
				}
				else
				{
					CampaignDocumentParser parser = new CampaignDocumentParser(Factory);
					SubscriptionPreferenceDescLabel.Text = parser.Parse(CampaignDocumentParser.ParseType.PlainText, DataSourceGlbCompanyCampaignUnsubscribe.ContactCampaignItem, pageDesc);
				}
				SubscriptionResultContainer.Visible = false;
			}
		}

		protected override BusinessObject GetNewDataSource()
		{
			try
			{
				var queryRequest = Request.QueryString;
				var campaignItemPk = SecureQueryStringHelper.GetZGuid(queryRequest, GlbCompanyCampaignItemSchema.Constants.PK);
				if (!campaignItemPk.IsEmpty)
				{
					return new SubscriptionPreferenceBusinessObjectAdapter(Factory, campaignItemPk, "", "");
				}
				else
				{
					if (SecureQueryStringHelper.IsQueryNeverExpire(queryRequest))
					{
						return null;
					}

					var contackPK = SecureQueryStringHelper.GetZGuid(queryRequest, OrgContactSchema.Constants.PK);
					var publishedListCode = SecureQueryStringHelper.GetValue(queryRequest, UnsubscribeUrlHelper.PublishedListCodeKey);
					return new SubscriptionPreferenceBusinessObjectAdapter(Factory, contackPK, publishedListCode);
				}
			}
			catch (ExpiredQueryStringException)
			{
				return null;
			}
			catch (InvalidQueryStringException)
			{
				return null;
			}
		}

		protected override void InitializeCulture()
		{
			try
			{
				base.InitializeCulture();
			}
			catch (ExpiredQueryStringException)
			{
			}
		}

		protected void SubmitButton_Click(object sender, EventArgs e)
		{
			foreach (RepeaterItem item in this.SubscriptionPreferencesRepeater.Items)
			{
				if (item.ItemType == ListItemType.Item || item.ItemType == ListItemType.AlternatingItem)
				{
					var checkBox = (ZSelectionCheckBox)item.FindControl("cbSubscription");
					var label = (Label)item.FindControl("lblSubscription");
					foreach (SubscriptionProperties sub in DataSourceGlbCompanyCampaignUnsubscribe.SubscriptionList)
					{
						if (sub.PublishedDescription.Equals(label.Text))
						{
							sub.IsSubscribed = checkBox.Checked;
							DataSourceGlbCompanyCampaignUnsubscribe.Process(sub);
							break;
						}
					}
				}
			}
			ShowResult();
		}

		protected virtual void ShowResult()
		{
			ResultImage.Src = DataSourceGlbCompanyCampaignUnsubscribe.IsErrorResult
				? ErrorImagePath
				: SuccessImagePath;
			ResultImage.Visible = true;
			ResultMessageLabel.Text = DataSourceGlbCompanyCampaignUnsubscribe.ResultMessage;
			SubscriptionResultContainer.Visible = true;
			SubscriptionListContainer.Visible = false;
			SubmitButton.Visible = false;
		}

		protected void HidePage(string text)
		{
			SubscriptionResultContainer.Visible = false;
			SubscriptionPreferenceLabel.Text = text;
			SubscriptionPreferenceDescLabel.Visible = false;
			SubscriptionListContainer.Visible = true;
			SubscriptionPreferencesRepeater.Visible = false;
			SubmitButton.Visible = false;
		}

		public bool GetChechBoxStatus(object o)
		{
			CargoWise.Types.ZBool? isSubed = o as CargoWise.Types.ZBool?;
			if (isSubed == null)
			{
				return false;
			}
			else
			{
				return (bool)isSubed;
			}
		}
	}
}
