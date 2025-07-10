using System.Windows.Forms;
using Enterprise.Integration.Rating;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MarketingManager.GUI.Testing
{
	[TestedType(typeof(GenerateQuoteSettingsForm))]
	class GenerateQuoteSettingsFormTest : ZFormBasherTest
	{
		#region Form Caption

		public void TestFormCaption()
		{
			using (var form = GetNewForm())
			{
				AssertEquals("New Quotation", form.FormCaption);
			}
		}

		#endregion

		#region Create Button

		public void TestCreateButton()
		{
			var quote1 = (IRelatableActivity)Factory.New<IQuote>();
			var quote2 = (IRelatableActivity)Factory.New<IQuote>();
			var quoteSelectionItemCollection = new QuoteSelectionItemCollection();
			var quoteSelectionItem1 = quoteSelectionItemCollection.AddNew(quote1);
			var quoteSelectionItem2 = quoteSelectionItemCollection.AddNew(quote2);

			var tradeDetail1 = Factory.New<OrgTradeDetail>();
			var tradeDetail2 = Factory.New<OrgTradeDetail>();
			var tradeDetailSelectionItemCollection = new TradeDetailSelectionItemCollection();
			var tradeDetailSelectionItem1 = tradeDetailSelectionItemCollection.AddNew(tradeDetail1);
			var tradeDetailSelectionItem2 = tradeDetailSelectionItemCollection.AddNew(tradeDetail2);

			var settings = new GenerateQuoteSettings(quoteSelectionItemCollection, tradeDetailSelectionItemCollection);
			using (var form = new GenerateQuoteSettingsForm(settings))
			{
				form.Show();

				settings.ShouldCreateAmendment = true;
				form.CreateButton.PerformClick();
				AssertEquals("There are errors - can't save.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Errors!", UnitTestUserNotification.Instance.LastMessage.Caption);
				AssertNotEquals(DialogResult.OK, form.DialogResult);

				settings.ShouldCreateAmendment = false;
				UnitTestUserNotification.Instance.ClearMessages();
				form.CreateButton.PerformClick();
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasNone);
				AssertEquals(DialogResult.OK, form.DialogResult);
			}
		}

		#endregion

		#region Overrides

		GenerateQuoteSettingsForm GetNewForm()
		{
			var quoteSelectionItems = new QuoteSelectionItemCollection();
			var tradeDetailSelectionitems = new TradeDetailSelectionItemCollection();
			var settings = new GenerateQuoteSettings(quoteSelectionItems, tradeDetailSelectionitems);
			return new GenerateQuoteSettingsForm(settings);
		}

		protected override Form GetFormToBashCore()
		{
			return GetNewForm();
		}

		#endregion
	}
}
