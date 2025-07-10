using System;
using System.Net;
using System.Web;
using Enterprise.MasterFiles.Tracking;
using Enterprise.Registry.Business;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Tracking.Web
{
	public partial class TermsAndConditions : BasePage
	{
		protected override void OnLoad(EventArgs e)
		{
			string text = TermsAndConditionsRegistryItem.Value;

			if (!string.IsNullOrEmpty(text))
			{
				TermsAndConditionsText.Text = text.IndexOf("<br") == -1 ? text.Replace(System.Environment.NewLine, @"<br />") : text;
			}
			else
			{
				HttpContext.Current.Response.Redirect(RedirectUrl);
			}
		}

		protected void IAgreeButton_Click(object sender, EventArgs e)
		{
			ApplyUserAgreed();
			HttpContext.Current.Response.Redirect(RedirectUrl);
		}

		protected void IDisagreeButton_Click(object sender, EventArgs e)
		{
			if (IsBooking)
			{
				HttpContext.Current.Response.Redirect(AppInstance.DefaultPage);
			}
			else
			{
				HttpContext.Current.Response.Redirect(AppInstance.LogoutPage);
			}
		}

#if DEBUG
		internal
#endif
		void ApplyUserAgreed()
		{
			if (SiteUser == null || !SiteUser.IsLoggedIn)
			{
				return;
			}

			if (IsBooking)
			{
				Session[Global.BookingTermsAndConditionsIndexer] = true;
			}
			else
			{
				SiteUser.SetUserWebContractSignedAndSaveToDb();
			}
		}

		bool IsBooking
		{
			get { return RedirectUrl.ToLower().IndexOf(AppInstance.EditBookingPage.ToLower()) != -1; }
		}

		protected MultilingualStringRegistryItem TermsAndConditionsRegistryItem
		{
			get
			{
				MultilingualStringRegistryItem result;

				if (IsBooking)
				{
					result = WebDataRegistry.Instance.BookingTermsAndConditions;
				}
				else
				{
					result = WebDataRegistry.Instance.WebTrackerSiteTermsAndConditions;
				}
				return result;
			}
		}

		protected override string GetPageName()
		{
			return WebTracker.Pages.TermsAndConditions;
		}

		protected override string GetPageRelativePath()
		{
			return TrackingConstants.RelativePath.TermsAndConditionsPage;
		}

		protected virtual string RedirectUrl
		{
			get
			{
				var result = AppInstance.GetValidRedirectURL(HttpContext.Current.Request.Params[Global.TermsAndConditionsRedirectUrlTag]);
				if (result != null && WebUtility.UrlDecode(result).ToLower().IndexOf(AppInstance.TermsAndConditionsPage.ToLower()) == -1)
				{
					result = WebUtility.UrlDecode(result);
				}
				else
				{
					result = AppInstance.DefaultPage;
				}

				return result;
			}
		}
	}
}
