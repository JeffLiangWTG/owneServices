using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(ExchangeRateWrapperForm))]
	public class ExchangeRateWrapperFormTest : ZFormBasherTest
	{
		[RequiresSTA]
		public void TestFormVerb()
		{
			using (ExchangeRateWrapperForm form = new ExchangeRateWrapperForm(new BulkExchangeRateUpdater(Factory)))
			{
				AssertEquals("No Form Verb", "", form.FormVerb);
			}
		}

		protected override Form GetFormToBashCore()
		{
			return new ExchangeRateWrapperForm(new BulkExchangeRateUpdater(Factory));
		}

		[RequiresSTA]
		public void TestExchangeRateDecimalPlaces()
		{
			ZBool originalReciprocal = GlbCompany.CurrentCompany.GC_IsReciprocal;
			GlbCompany.CurrentCompany.GC_IsReciprocal = true;

			try
			{
				BulkExchangeRateUpdater updater = new BulkExchangeRateUpdater(Factory);
				using (ExchangeRateWrapperForm exRateForm = new ExchangeRateWrapperForm(updater))
				{
					exRateForm.Show();

					ZCalcEditColumnStyle buyRateColumn =
						(ZCalcEditColumnStyle)exRateForm.ExchangeRateWrapperGrid.Columns["BuyRate"].ColumnStyle;
					ZCalcEditColumnStyle sellRateColumn =
						(ZCalcEditColumnStyle)exRateForm.ExchangeRateWrapperGrid.Columns["SellRate"].ColumnStyle;

					AssertNotNull("There should be a column named BuyRate", buyRateColumn);
					AssertEquals("BuyRate column should have 6 decimal places", 6, buyRateColumn.Decimals);
					AssertNotNull("There should be a column named SellRate", sellRateColumn);
					AssertEquals("SellRate column should have 6 decimal places", 6, sellRateColumn.Decimals);
				}
			}
			finally
			{
				GlbCompany.CurrentCompany.GC_IsReciprocal = originalReciprocal;
			}
		}

		[RequiresSTA]
		public void TestNoExceptionWhenPopulateDatesWithInvalidValue()
		{
			var updater = new BulkExchangeRateUpdater(Factory);
			using (var exRateForm = new ExchangeRateWrapperForm(updater))
			{
				exRateForm.Show();

				var exRate = updater.ExchangeRateWrappers.AddNew();
				exRate.CurrencyNK = "AUD";

				exRate.BuyStartDate = ZDateTime.Invalid;
				AssertNoExceptionThrown(() => exRate.BuyExpiryDate = ZDateTime.Today);

				exRate.BuyStartDate = ZDateTime.Today;
				AssertNoExceptionThrown(() => exRate.BuyExpiryDate = ZDateTime.Invalid);

				exRate.SellStartDate = ZDateTime.Invalid;
				AssertNoExceptionThrown(() => exRate.SellExpiryDate = ZDateTime.Today);

				exRate.SellStartDate = ZDateTime.Today;
				AssertNoExceptionThrown(() => exRate.SellExpiryDate = ZDateTime.Invalid);
			}
		}
	}
}
