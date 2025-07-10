using System;
using System.Collections;
using System.Web.UI.HtmlControls;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.MarketingManager.WebVoting.Testing
{
	[HttpContextEnabledTest]
	public class BasePageTest : TestCaseWithFactory
	{
		public void TestPageHeaderControlPath()
		{
			AssertEquals("Base/WebVotingBanner.ascx", Page.PageHeaderControlPath);
		}

		public void TestLightBoxNotPresent()
		{
			using (var page = Page)
			{
				page.Controls.Add(new HtmlForm());

				page.OnInit_Exposed();
				Assert("LightBox should not exist", !page.IsLightBoxOnForm());
			}
		}

		#region Implementation

		BasePageForTest Page
		{
			get
			{
				if (fPage == null)
				{
					fPage = new BasePageForTest();
				}
				return fPage;
			}
		}

		BasePageForTest fPage;

		class BasePageForTest : BasePage
		{
			public new string PageHeaderControlPath
			{
				get { return base.PageHeaderControlPath; }
			}

			public bool IsLightBoxOnForm()
			{
				var typeList = new ArrayList();
				typeList.Add(typeof(ZLightBox));

				var lightBoxes = GetControlsRecursivelyFromControl(FormControl, typeList);
				return lightBoxes != null && lightBoxes.Count > 0;
			}

			public void OnInit_Exposed()
			{
				base.OnInit(EventArgs.Empty);
			}

			protected override Uri RequestUrl
			{
				get { return new Uri("http://www.test.com/Campaign/Test.aspx"); }
			}

			protected override BrowserType GetBrowserType()
			{
				return BrowserType.IE;
			}

			protected override void LoadPageHeader()
			{
			}

			protected override void SetCacheability()
			{
			}

			#endregion
		}
	}
}
