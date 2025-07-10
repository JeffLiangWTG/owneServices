using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.NO.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NO.GUI.Testing
{
	sealed class InvoiceLayoutsUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceDataSourceType()
		{
			AssertEquals(typeof(JobComInvoiceHeader), control.BindingSource.DataSourceType);
		}

		public void TestExchangeRateControl()
		{
			var exchangeRateControl = control.ExchangeRatePlusFixedRateUserControl;
			CombineAssertions(() =>
			{
				AssertType<ExchangeRatePlusFixedRateUserControl>("Type", exchangeRateControl);
				AssertEquals("Visible", true, exchangeRateControl.Visible);
			});
		}

		public void TestInvoiceDateDateEdit()
		{
			var invoiceDateDateEdit = control.InvoiceDateDateEdit;
			CombineAssertions(() =>
			{
				AssertType<ZDateEdit>("Type", invoiceDateDateEdit);
				AssertEquals("BindTo", nameof(JobComInvoiceHeader.JZ_InvoiceDate), invoiceDateDateEdit.BindTo);
				AssertEquals("Visible", true, invoiceDateDateEdit.Visible);
			});
		}

		public void TestValuationMethodDropEdit()
		{
			var valuationMethodDropEdit = control.ValuationMethodDropEdit;
			CombineAssertions(() =>
			{
				AssertType<ZDropEdit>("Type", valuationMethodDropEdit);
				AssertEquals("BindTo", nameof(JobComInvoiceHeader.JZ_ValuationMethod), valuationMethodDropEdit.BindTo);
				AssertEquals("Visible", true, valuationMethodDropEdit.Visible);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new InvoiceLayoutsUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}
		InvoiceLayoutsUserControl control;
	}
}
