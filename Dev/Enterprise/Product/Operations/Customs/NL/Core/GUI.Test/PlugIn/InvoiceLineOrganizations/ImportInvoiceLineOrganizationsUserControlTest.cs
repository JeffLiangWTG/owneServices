using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.NL.GUI.Testing;

sealed class ImportInvoiceLineOrganizationsUserControlTest : TestCaseWithFactory
{
	public void TestConsigneeRemoved()
	{
		using (var control = new ImportInvoiceLineOrganizationsUserControl())
		{
			AssertEquals(false, control.Controls.Contains(control.ConsigneeAddressControl));
		}
	}

	public void TestConsignorRemoved()
	{
		using (var control = new ImportInvoiceLineOrganizationsUserControl())
		{
			AssertEquals(false, control.Controls.Contains(control.ConsignorAddressControl));
		}
	}
}
