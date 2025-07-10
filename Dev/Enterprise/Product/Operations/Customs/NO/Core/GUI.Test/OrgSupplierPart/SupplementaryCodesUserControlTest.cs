using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.GUI;
using Enterprise.Customs.GUI.Testing;
using Enterprise.Customs.NO.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.NO.GUI.Testing;

[TestedType(typeof(SupplementaryCodesUserControl))]
sealed class SupplementaryCodesUserControlTest : TestCaseWithFactory
{
	public void TestControls() => CombineAssertions(() =>
	{
		using var userControl = new SupplementaryCodesUserControl();
		_ = userControl.AssertThisControl(x =>
			x.WithCaptionRenderingEnabled()
		);
		_ = userControl.AssertContainsControl<ZArchitecture.ZTextBox>("Supplement1TextBox",
			x => x
			.WithCaption("Excise codes")
			.WithBindTo("CI_Supplement1"));
		_ = userControl.AssertContainsControl<ZArchitecture.ZLabel>("SupplementLabel1",
			x => x
			.WithText("/"));
		_ = userControl.AssertContainsControl<ZArchitecture.ZTextBox>("Supplement2TextBox",
			x => x
			.WithBindTo("CI_Supplement2"));
		_ = userControl.AssertContainsControl<ZArchitecture.ZLabel>("SupplementLabel2",
			x => x
			.WithText("/"));
		_ = userControl.AssertContainsControl<ZArchitecture.ZTextBox>("CI_AdditionalSupplementsTextBox",
			x => x
			.WithBindTo("CI_AdditionalSupplements"));
		_ = userControl.AssertContainsControl<ZButton>("AdditionalSupplementaryCodesEditButton",
			x => x
			.WithCaption("More..."));
	});

	public void TestAdditionalSupplementaryCodesEditButton_Click()
	{
		var part = Factory.New<OrgSupplierPart>();
		var pivot = part.PivotsForBinding.AddNew();

		using var form = new ZForm(pivot);
		using var control = new SupplementaryCodesUserControl();
		form.Controls.Add(control);
		form.Show();

		ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
		var button = control.FindSingle<ZButton>("AdditionalSupplementaryCodesEditButton");
		button.PerformClick();
		AssertType<AdditionalSupplementaryCodesForm>(ZFormModaliser.LastFormShownDialogForTest);
	}
}
