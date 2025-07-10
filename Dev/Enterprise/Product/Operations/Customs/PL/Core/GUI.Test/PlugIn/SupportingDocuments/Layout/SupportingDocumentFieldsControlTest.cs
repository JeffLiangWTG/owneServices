using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.GUI;
using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.Customs.PL.GUI.PlugIn;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.PL.GUI.Testing;

sealed class SupportingDocumentFieldsControlTest : TestCaseWithFactory
{
	public void TestControls()
	{
		CombineAssertions(() =>
		{
			var convertControl = FindAndAssertControl<ConvertToLocalCurrencyControl>("ValueConvertToLocalCurrencyControl");
			AssertEquals("ValueConvertToLocalCurrencyControl BindTo", nameof(SupportingDocument.CSI_Value), convertControl.BindToAmount);
			AssertEquals("ValueConvertToLocalCurrencyControl BindTo", nameof(SupportingDocument.CSI_RX_NKCurrency), convertControl.BindToUnit);

			var reference2Control = FindAndAssertControl<ZTextBox>("SupDocReference2TextBox");
			AssertEquals("SupDocReference2TextBox BindTo", nameof(SupportingDocument.CSI_ReferenceNumber2), reference2Control.BindTo);

			var descriptionCOntrol = FindAndAssertControl<ZTextBox>("SupDocDescriptionTextBox");
			AssertEquals("SupDocDescriptionTextBox BindTo", nameof(SupportingDocument.CSI_Description), descriptionCOntrol.BindTo);
		});
	}

	T FindAndAssertControl<T>(string controlName) where T : Control
	{
		T field = null;
		AssertNoExceptionThrown($"{controlName} should be found", () => { field = control.FindSingle<T>(controlName); });
		AssertNotNull($"{controlName} should not be null", field);
		return field;
	}

	protected override void SetUp()
	{
		base.SetUp();
		control = new SupportingDocumentFieldsControl();
	}
	SupportingDocumentFieldsControl control;

	protected override void TearDown()
	{
		base.TearDown();
		control?.Dispose();
	}
}
