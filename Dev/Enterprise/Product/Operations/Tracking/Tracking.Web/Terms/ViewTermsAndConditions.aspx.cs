using System;
using Enterprise.MasterFiles.Tracking;
using Enterprise.Registry.Business;
using Enterprise.Tracking.Business;

namespace Enterprise.Tracking.Web
{
	public partial class ViewTermsAndConditions : BasePage
	{
		protected override void OnLoad(EventArgs e)
		{
			string text = WebDataRegistry.Instance.WebTrackerSiteTermsAndConditions.Value;

			if (!string.IsNullOrEmpty(text))
			{
				TermsAndConditionsText.EnableHtmlEncoding = false;
				TermsAndConditionsText.Text = text.IndexOf("<br") == -1 ? text.Replace(System.Environment.NewLine, @"<br />") : text;
			}
		}

		protected override string GetPageName()
		{
			return WebTracker.Pages.ViewTermsAndConditions;
		}

		protected override string GetPageRelativePath()
		{
			return TrackingConstants.RelativePath.ViewTermsAndConditionsPage;
		}
	}
}
