using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.NO.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NO.GUI.Testing
{
	sealed class ExchangeRatePlusFixedRateUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceDataSourceType()
		{
			AssertEquals(typeof(JobComInvoiceHeader), control.BindingSource.DataSourceType);
		}

		public void TestInvoiceCurrExRateCalcEdit()
		{
			var invoiceCurrExRateCalcEdit = control.InvoiceCurrExRateCalcEdit;
			CombineAssertions(() =>
			{
				AssertType<ZCalcEdit>("Type", invoiceCurrExRateCalcEdit);
				AssertEquals("BindTo", nameof(JobComInvoiceHeader.JZ_InvoiceCurrExRate), invoiceCurrExRateCalcEdit.BindTo);
			});
		}

		public void TestFixedCurrencyCheckBox()
		{
			var fixedCurrencyCheckBox = control.FixedCurrencyCheckBox;
			CombineAssertions(() =>
			{
				AssertType<ZCheckBox>("Type", fixedCurrencyCheckBox);
				AssertEquals("BindTo", nameof(JobComInvoiceHeader.IsJZ_InvoiceCurrExRateUserEnterable), fixedCurrencyCheckBox.BindTo);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new ExchangeRatePlusFixedRateUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}
		ExchangeRatePlusFixedRateUserControl control;
	}
}
