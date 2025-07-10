using System;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWiseOne.ResourceStrings;
using Enterprise.MarketingManager.Business;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.MarketingManager.WebVoting
{
	public class BasePage : ZPage
	{
		protected override void AddLightBox(HtmlForm form) { }

		protected override void RenderLightBoxScript() { }

		protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);
			SetCacheability();
		}

		protected virtual void SetCacheability()
		{
			Response.Cache.SetCacheability(HttpCacheability.NoCache);
		}

		protected override string PageHeaderControlPath
		{
			get { return "Base/WebVotingBanner.ascx"; }
		}

		protected override void InitializeCulture()
		{
			using (Db.DisposableActionForDbConnection())
			{
				LanguageSet = false;
				if (!IsErrorPage())
				{
					try
					{
						var queryString = new SecureQueryString(Request.QueryString[VoteExamSurveyUrlHelper.VoteExamSurveyQueryStringKey]);
						string language = queryString[VoteExamSurveyUrlHelper.LanguageStringKey];
						if (!string.IsNullOrEmpty(language))
						{
							string sessionLanguage = Session["Language"] as string;
							if (sessionLanguage != language)
							{
								if (WebAppEnvironment.SetupLanguage(language))
								{
									ObjectFactory.Get<IResourceStrings>().CurrentLanguage = language;
									LanguageSet = true;
								}
							}
						}
					}
					catch (InvalidQueryStringException) { }
				}
				base.InitializeCulture();
			}
		}

		protected override void Localize(Control control)
		{
		}

#if DEBUG
		protected internal void InitializeCultureInternal() => InitializeCulture();
		protected internal HtmlGenericControl FooterInternal => Footer;
#endif
	}
}
