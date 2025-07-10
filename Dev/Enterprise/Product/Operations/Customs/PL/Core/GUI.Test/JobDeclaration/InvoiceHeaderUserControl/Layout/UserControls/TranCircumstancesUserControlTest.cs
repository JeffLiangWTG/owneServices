using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.PL.GUI.Testing;

sealed class TranCircumstancesUserControlTest : TestCaseWithFactory
{
	public void TestDataSourceType()
	{
		using (var control = new TranCircumstancesUserControl())
		{
			AssertEquals(typeof(JobComInvoiceHeader), control.DataSourceType);
		}
	}

	public void TestTranCircumstances1DropEdit()
	{
		using (var control = new TranCircumstancesUserControl())
		{
			var tranCircumstances1DropEdit = control.TranCircumstances1DropEdit;
			CombineAssertions(() =>
			{
				AssertType<ZDropEdit>("Type", tranCircumstances1DropEdit);
				AssertEquals("BindingMember", nameof(JobComInvoiceHeader.TranCircumstanceCode1), tranCircumstances1DropEdit.GetBindingMember());
			});
		}
	}

	public void TestAdditionalTranCircumstancesTextBox()
	{
		using (var control = new TranCircumstancesUserControl())
		{
			var additionalTranCircumstancesTextBox = control.AdditionalTranCircumstancesTextBox;
			CombineAssertions(() =>
			{
				AssertType<ZTextBox>("Type", additionalTranCircumstancesTextBox);
				AssertEquals("BindingMember", nameof(JobComInvoiceHeader.AdditionalTranCircumstanceCodesAsString), additionalTranCircumstancesTextBox.GetBindingMember());
			});
		}
	}

	public void TestAdditionalTranCircumstancesEditButton()
	{
		using (var control = new TranCircumstancesUserControl())
		{
			var additionalTranCircumstancesEditButton = control.AdditionalTranCircumstancesEditButton;
			AssertType<ZButton>("Type", additionalTranCircumstancesEditButton);
			AssertEquals("Cpation", "More...", additionalTranCircumstancesEditButton.CaptionResourceString.Caption);
		}
	}

	public void TestResourceStyringBindingMember()
	{
		using (var control = new TranCircumstancesUserControl())
		{
			AssertEquals(nameof(JobComInvoiceHeader.TranCircumstanceCode1), control.ResourceStringBindingMember);
		}
	}

	public void TestAdditionalTranCircumstancesEditButton_Click()
	{
		var invoiceHeader = Factory.New<JobDeclaration>().Invoices.AddNew();

		using (var form = new ZForm(invoiceHeader))
		using (var control = new TranCircumstancesUserControl())
		{
			form.Controls.Add(control);
			form.Show();

			var additionalTranCircumstancesEditButton = control.AdditionalTranCircumstancesEditButton;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.No;
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			additionalTranCircumstancesEditButton.PerformClick();
			AssertType<AdditionalTranCircumstancesForm>(ZFormModaliser.LastFormShownDialogForTest);
		}
	}

	public void TestIExtendedControl()
	{
		using (var control = new TranCircumstancesUserControl())
		{
			CombineAssertions(() =>
			{
				AssertEquals("Host", control, control.Host);
				AssertType<DefaultControlExtensionCollection>("Extensions", control.Extensions);
			});
		}
	}
}
