using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.PL.GUI.Testing;

class InvoiceLineLayoutUserControlTest : TestCaseWithFactory
{
	public void TestBindingSourceDataSourceType()
	{
		using (var control = new InvoiceLineDetailsUserControl())
		{
			AssertEquals(typeof(JobComInvoiceLine), control.BindingSource.DataSourceType);
		}
	}

	public void TestControls()
	{
		using (var control = new InvoiceLineDetailsUserControl())
		{
			AssertNoExceptionThrown(() => control.FindSingle<CPCUserControl>("CPCUserControl"));
		}
	}
}
