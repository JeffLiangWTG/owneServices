using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.PL.GUI.Testing;

class CPCUserControlTest : TestCaseWithFactory
{
	public void TestIExtendedControl()
	{
		using (var control = new CPCUserControl())
		{
			CombineAssertions(() =>
			{
				AssertEquals("Host", control, control.Host);
				AssertType<DefaultControlExtensionCollection>("Extensions", control.Extensions);
			});
		}
	}

	public void TestZLabelCaptionRenderer()
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		using (var form = new ZForm(invoiceLine))
		using (var control = new CPCUserControl())
		{
			form.CaptionRenderingEnabled = true;
			form.Controls.Add(control);
			form.Show();

			AssertNotNullOrEmpty(control.GetExtension<ZLabelCaptionRenderer>().Caption);
		}
	}

	public void TestResourceStringBindingMember()
	{
		using (var control = new CPCUserControl())
		{
			AssertEquals(nameof(JobComInvoiceLine.ProcedureCodeBase), control.ResourceStringBindingMember);
		}
	}

	public void TestBindingSourceDataSourceType()
	{
		using (var control = new InvoiceLineDetailsUserControl())
		{
			AssertEquals(typeof(JobComInvoiceLine), control.BindingSource.DataSourceType);
		}
	}

	public void TestControls()
	{
		using (var control = new CPCUserControl())
		{
			CombineAssertions(() =>
			{
				AssertNoExceptionThrown(() => control.FindSingle<ZDropEdit>("RequestedCustomsProcedureCodeDropEdit"));
				AssertNoExceptionThrown(() => control.FindSingle<ZDropEdit>("PreviousCustomsProcedureCodeDropEdit"));
			});
		}
	}
}
