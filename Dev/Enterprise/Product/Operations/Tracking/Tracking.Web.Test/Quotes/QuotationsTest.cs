using System;
using System.Collections.Generic;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Registry.Business;
using Enterprise.Tracking.Business;
using Enterprise.Tracking.Business.Testing;
using Enterprise.Tracking.Web.Quotes;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web.Testing
{
	[HttpContextEnabledTest]
	sealed class QuotationsTest : BasePageWithAuthorisationTest
	{
		[HttpContextEnabledTest]
		public void TestQuoteDownloadCommand()
		{
			var quote = Factory.NewWithValidTestData<Quote>();
			Factory.Save();

			var quotes = new QuoteCollection(Factory);
			quotes.Add(quote);

			var quotePage = new QuotationsForTest();
			var searchControl = (ZSearchControl)quotePage.SearchControl;
			var quotesGrid = searchControl.SearchResultsDataGrid;
			quotesGrid.Page = quotePage;
			quotesGrid.Bind(quotes);

			var item = quotesGrid.Items[0];
			quotePage.OnSearchResultsDataGridItemCommandForTesting(quotesGrid, item);

			Assert(quotePage.ZClientScript.IsStartupScriptRegistered(typeof(Quotations), "QuoteDownload"));
		}

		public void TestQuoteDownloadScript()
		{
			var quotePage = new QuotationsForTest();
			var expectedScript = @"
					<script type=""text/javascript"">
					var downloadControl = $('QuoteDownloadLink');

					if (downloadControl)
					{
						downloadControl.href = 'http://www.test.com/quote.aspx';
						downloadControl.click();
					}
					</script>";

			AssertEquals(expectedScript, quotePage.GetQuoteDownloadScriptForTesting("http://www.test.com/quote.aspx"));
		}

		[HttpContextEnabledTest]
		public void TestCopyQuote()
		{
			var helper = new TestHelper(Factory);
			var originalQuote = Factory.NewWithValidTestData<Quote>();
			originalQuote.TH_OneTimeQuote = true;
			Factory.Save();

			var quotesPage = new QuotationsForTest();
			quotesPage.SiteUser.Login(helper.TestOrg.OH_Code, helper.TestContact.Email, helper.TestContact.PasswordForTesting);
			var quotesGrid = ((ZSearchControl)quotesPage.SearchControl).SearchResultsDataGrid;
			quotesGrid.SetSelectedKeyForTesting(originalQuote.PK.ToString());
			quotesPage.OnCopyQuoteClickHandlerForTesting();

			var newQuotePK = GetRedirectReferencePK();
			var newQuote = quotesPage.Factory.Load<Quote>(newQuotePK);
			AssertEquals(quotesPage.SiteUser.ContactAndCompanyReference, newQuote.Logs.AutoCreatedLogDefaultSL_Reference);
		}

		protected override string GetExpectedPageName() => WebTracker.Pages.Quotations;

		protected override System.Web.UI.Control GetNewControl() => new QuotationsForTest();

		protected override BooleanRegistryItem UseWebModule => WebDataRegistry.Instance.UseWebForwardingQuotesModule;

		protected override WebSecurityRight SiteUserSecurityRight => WebSecurityRightsList.WebQuotes;

		protected override IReadOnlyCollection<ILicenceCheckpoint> ExpectedLicenceCheckPoints => new ILicenceCheckpoint[] { Environment.Env.Licence.WebTrackerBooking };

		public class QuotationsForTest : Quotations
		{
			protected override ZGlobal GetNewTestGlobal()
			{
				return new TestGlobal();
			}

			public QuotationsForTest()
			{
				SearchControlHolder = new HtmlGenericControl();
				SetupSearchControl();
			}

			public void OnCopyQuoteClickHandlerForTesting() => OnCopyQuoteClickHandler(this, EventArgs.Empty);

			public void OnSearchResultsDataGridItemCommandForTesting(object source, DataGridItem item) => OnSearchResultsDataGridItemCommand(source, new DataGridCommandEventArgs(item, source, new CommandEventArgs("ViewQuote", null)));

			public string GetQuoteDownloadScriptForTesting(string url) => GetQuoteDownloadScript(url);
		}
	}
}
