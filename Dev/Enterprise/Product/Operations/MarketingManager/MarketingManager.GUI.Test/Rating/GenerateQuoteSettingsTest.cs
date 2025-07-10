using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MarketingManager.GUI.Testing
{
	[TestedType(typeof(GenerateQuoteSettings))]
	class GenerateQuoteSettingsTest : NonPersistentBusinessObjectTestCase
	{
		#region Default Values

		public void TestDefaultValues()
		{
			var settings = GetNewSettings();
			AssertEquals(true, settings.ShouldCreateNew);
			AssertEquals(false, settings.ShouldCreateAmendment);
		}

		#endregion

		#region Properties

		public void TestShouldCreateAmendmentForBinding()
		{
			var quoteSelectionItems = new QuoteSelectionItemCollection();
			var tradeDetailSelectionItems = new TradeDetailSelectionItemCollection();
			var settings = new GenerateQuoteSettings(quoteSelectionItems, tradeDetailSelectionItems);

			settings.ShouldCreateAmendmentForBinding = true;
			AssertEquals(false, settings.ShouldCreateAmendmentForBindingInfo.HasErrors());

			settings.Validation.ValidateShouldCreateAmendment();
			AssertEquals(true, settings.ShouldCreateAmendmentForBindingInfo.HasErrors());

			settings.ShouldCreateAmendmentForBinding = false;
			AssertEquals(false, settings.ShouldCreateAmendmentForBindingInfo.HasErrors());
		}

		#endregion

		#region QuoteSelectionItems

		public void TestQuoteSelectionItems_ReadOnlyIfNotAmendment()
		{
			var quoteSelectionItems = new QuoteSelectionItemCollection();
			var tradeDetailSelectionItems = new TradeDetailSelectionItemCollection();
			var settings = new GenerateQuoteSettings(quoteSelectionItems, tradeDetailSelectionItems);

			AssertEquals("Precondition", false, settings.ShouldCreateAmendment);
			AssertEquals(true, settings.QuoteSelectionItems.ReadOnly);

			settings.ShouldCreateAmendment = true;
			AssertEquals(false, settings.QuoteSelectionItems.ReadOnly);
		}

		#endregion

		#region Overrides

		GenerateQuoteSettings GetNewSettings()
		{
			var quoteSelectionItems = new QuoteSelectionItemCollection();
			var tradeDetailSelectionitems = new TradeDetailSelectionItemCollection();
			return new GenerateQuoteSettings(quoteSelectionItems, tradeDetailSelectionitems);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return GetNewSettings();
		}

		#endregion
	}
}
