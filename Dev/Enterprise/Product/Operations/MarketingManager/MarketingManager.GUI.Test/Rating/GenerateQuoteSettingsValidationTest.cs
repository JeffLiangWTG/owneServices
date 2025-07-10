using CargoWise.EntityFramework.Testing;
using Enterprise.Integration.Rating;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MarketingManager.GUI.Testing
{
	class GenerateQuoteSettingsValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckShouldCreateAmendment()
		{
			var quoteSelectionItems = new QuoteSelectionItemCollection();
			var item1 = quoteSelectionItems.AddNew((IRelatableActivity)Factory.New<IQuote>());
			var item2 = quoteSelectionItems.AddNew((IRelatableActivity)Factory.New<IQuote>());
			var item3 = quoteSelectionItems.AddNew((IRelatableActivity)Factory.New<IQuote>());

			var tradeDetailSelectionItems = new TradeDetailSelectionItemCollection();

			var settings = new GenerateQuoteSettings(quoteSelectionItems, tradeDetailSelectionItems);
			settings.ShouldCreateAmendment = true;
			AssertHasError(settings.ShouldCreateAmendmentInfo, "Please select a quotation to amend.");

			item2.Selected = true;

			AssertNoErrors(settings.ShouldCreateAmendmentInfo);
		}
	}
}
