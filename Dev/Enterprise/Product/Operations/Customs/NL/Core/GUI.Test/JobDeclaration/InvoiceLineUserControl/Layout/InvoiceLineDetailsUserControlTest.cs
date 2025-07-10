using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.NL.Business.Declaration;

namespace Enterprise.Customs.NL.GUI.Testing;

sealed class InvoiceLineDetailsUserControlTest : TestCaseWithFactory
{
	public void TestBindingSourceDataSourceType()
	{
		AssertEquals(typeof(JobComInvoiceLine), control.BindingSource.DataSourceType);
	}

	public void TestECCNCodesUserControl()
	{
		AssertType<ECCNCodesUserControl>(control.ECCNCodesUserControl);
	}

	InvoiceLineDetailsUserControl control;

	protected override void SetUp()
	{
		base.SetUp();
		control = new InvoiceLineDetailsUserControl();
	}

	protected override void TearDown()
	{
		base.TearDown();
		control.Dispose();
	}
}
