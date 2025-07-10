using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.PL.GUI.Testing;

sealed class ImportInvoiceDetailsUserControlTest : TestCaseWithFactory
{
	public void TestDataSourceType()
	{
		using (var control = new ImportInvoiceDetailsUserControl())
		{
			AssertEquals(typeof(JobComInvoiceHeader), control.DataSourceType);
		}
	}

	public void TestTranCircumstanceUserControl()
	{
		using (var control = new ImportInvoiceDetailsUserControl())
		{
			var tranCircumstancesUserControl = control.TranCircumstanceUserControl;
			CombineAssertions(() =>
			{
				AssertType<TranCircumstancesUserControl>("Type", tranCircumstancesUserControl);
				AssertEquals("BindingMember", ".", tranCircumstancesUserControl.GetBindingMember());
			});
		}
	}

	public void TestValuationMethodDropEdit()
	{
		using (var control = new ImportInvoiceDetailsUserControl())
		{
			var valuationMethodDropEdit = control.ValuationMethodDropEdit;
			CombineAssertions(() =>
			{
				AssertType<ZDropEdit>("Type", valuationMethodDropEdit);
				AssertEquals("BindingMember", nameof(JobComInvoiceHeader.ZG_ValuationMethod), valuationMethodDropEdit.GetBindingMember());
			});
		}
	}
}
