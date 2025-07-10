using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.GUI;
using Enterprise.Customs.GUI.Testing;
using Enterprise.Customs.NO.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.NO.GUI.Testing;

[TestedType(typeof(AdditionalSupplementaryCodesUserControl))]
sealed class AdditionalSupplementaryCodesUserControlTest : TestCaseWithFactory
{
	public void TestIExtendedControl()
	{
		using var control = new AdditionalSupplementaryCodesUserControl();
		IExtendedControl extendedControl = control;
		AssertNotNull("Extended Control", extendedControl);
		CombineAssertions(() =>
		{
			AssertSame("Host", control, extendedControl.Host);
			AssertType<DefaultControlExtensionCollection>(extendedControl.Extensions);
		});
	}

	public void TestResourceStringBindingMember()
	{
		using var control = new AdditionalSupplementaryCodesUserControl();
		IResourceStringBindingMember resourceStringBindingMember = control;
		AssertNotNull("ResourceString Binding Member type", resourceStringBindingMember);
		AssertEquals("Binding Member Value", nameof(JobComInvoiceLine.JI_AdditionalSupplements), resourceStringBindingMember.ResourceStringBindingMember);
	}

	public void TestControls() => CombineAssertions(() =>
	{
		using var control = new AdditionalSupplementaryCodesUserControl();

		_ = control.AssertContainsControl<ZTextBox>("AdditionalSupplementsTextBox", x => x
			.WithBindTo(nameof(JobComInvoiceLine.JI_AdditionalSupplements))
			.WithCaption("Add. Exc. Codes")
			.WithFullDescription("Additional excise codes"));

		_ = control.AssertContainsControl<ZButton>("AdditionalSupplementaryCodesEditButton", x => x
			.WithCaption("More..")
			.WithFullDescription("Select excise codes"));
	});

	public void TestAdditionalSupplementaryCodesEditButton_Click()
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		using var form = new ZForm(invoiceLine);
		using var control = new AdditionalSupplementaryCodesUserControl();
		form.Controls.Add(control);
		form.Show();
		ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
		var button = control.FindSingle<ZButton>("AdditionalSupplementaryCodesEditButton");
		button.PerformClick();
		AssertType<AdditionalSupplementaryCodesForm>(ZFormModaliser.LastFormShownDialogForTest);
	}
}
