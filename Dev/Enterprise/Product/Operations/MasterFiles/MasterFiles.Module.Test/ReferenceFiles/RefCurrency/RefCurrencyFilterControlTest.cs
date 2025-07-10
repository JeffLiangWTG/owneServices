using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	public class RefCurrencyFilterControlTest : TestCaseWithFactory
	{
		[RequiresSTA]
		public void TestNumOfDecimalsByCompany()
		{
			var originalReciprocal = GlbCompany.CurrentCompany.GC_IsReciprocal;

			try
			{
				GlbCompany.CurrentCompany.GC_IsReciprocal = false;

				AssertDecimalsForExchangeRateColumn(6);

				GlbCompany.CurrentCompany.GC_IsReciprocal = true;

				AssertDecimalsForExchangeRateColumn(6);
			}
			finally
			{
				GlbCompany.CurrentCompany.GC_IsReciprocal = originalReciprocal;
			}
		}

		void AssertDecimalsForExchangeRateColumn(int expectedDecimals)
		{
			var currencies = new RefCurrencyCollection(Factory);
			var filterBizO = new RefCurrencyFilterBusinessObject();

			using (var form = new ZForm())
			{
				using (var filterRC = new RefCurrencyFilterControl(currencies, filterBizO))
				{
					form.Controls.Add(filterRC);
					form.Show();
					Application.DoEvents();
					var sellExRateColumnInfo = (ZCalcEditColumnStyleInfo)filterRC.FilteredGrid.GetColumnStyle("CurrentSellRate");
					AssertEquals("Exchange rate should have correct decimal places", expectedDecimals, sellExRateColumnInfo.Decimals);

					var costExRateColumnInfo = (ZCalcEditColumnStyleInfo)filterRC.FilteredGrid.GetColumnStyle("CurrentBuyRate");
					AssertEquals("Exchange rate should have correct decimal places", expectedDecimals, costExRateColumnInfo.Decimals);

					var customsExRateColumnInfo = (ZCalcEditColumnStyleInfo)filterRC.FilteredGrid.GetColumnStyle("CurrentCustomsRate");
					AssertEquals("Exchange rate should have correct decimal places", expectedDecimals, customsExRateColumnInfo.Decimals);
				}
			}
		}
	}
}
